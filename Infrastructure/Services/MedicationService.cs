using Application.DTOs.Medication;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class MedicationService(DiabetesDbContext dbContext) : IMedicationService
{
    public async Task<MedicationResponse> CreateMedicationAsync(
        Guid userId,
        CreateMedicationRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateText(request.Name, 200, "اسم الدواء مطلوب ويجب ألا يتجاوز 200 حرف.");
        ValidateOptionalText(request.Dosage, 100, "الجرعة يجب ألا تتجاوز 100 حرف.");
        ValidateOptionalText(request.Instructions, 500, "التعليمات يجب ألا تتجاوز 500 حرف.");

        var patient = await dbContext.Patients
            .SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        if (patient is null)
            throw new UnauthorizedAccessException("حساب المريض غير موجود.");

        var medication = new Medication
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            Name = request.Name.Trim(),
            Dosage = string.IsNullOrWhiteSpace(request.Dosage) ? null : request.Dosage.Trim(),
            Instructions = string.IsNullOrWhiteSpace(request.Instructions) ? null : request.Instructions.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Medications.Add(medication);
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapMedication(medication);
    }

    public async Task<IReadOnlyList<MedicationResponse>> GetMedicationsAsync(
        Guid userId,
        bool activeOnly,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Medications
            .AsNoTracking()
            .Where(x => x.Patient.UserId == userId);

        if (activeOnly)
            query = query.Where(x => x.IsActive);

        var medications = await query
            .Include(x => x.Schedules)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return medications.Select(MapMedication).ToList();
    }

    public async Task SetMedicationStatusAsync(
        Guid userId,
        Guid medicationId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var medication = await GetOwnedMedicationAsync(userId, medicationId, cancellationToken);

        medication.IsActive = isActive;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<MedicationScheduleResponse> AddScheduleAsync(
        Guid userId,
        Guid medicationId,
        CreateMedicationScheduleRequest request,
        CancellationToken cancellationToken = default)
    {
        var medication = await GetOwnedMedicationAsync(userId, medicationId, cancellationToken);

        ValidateText(request.Frequency, 50, "تكرار الدواء مطلوب ويجب ألا يتجاوز 50 حرفًا.");

        if (request.EndDate.HasValue && request.EndDate.Value < request.StartDate)
            throw new ArgumentException("تاريخ نهاية الجدول يجب ألا يسبق تاريخ البداية.");

        var schedule = new MedicationSchedule
        {
            Id = Guid.NewGuid(),
            MedicationId = medication.Id,
            Time = request.Time,
            Frequency = request.Frequency.Trim(),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = true
        };

        dbContext.MedicationSchedules.Add(schedule);
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapSchedule(schedule);
    }

    public async Task<MedicationLogResponse> AddLogAsync(
        Guid userId,
        Guid medicationId,
        CreateMedicationLogRequest request,
        CancellationToken cancellationToken = default)
    {
        await GetOwnedMedicationAsync(userId, medicationId, cancellationToken);

        if (request.ScheduledTime > DateTime.UtcNow.AddMinutes(5))
            throw new ArgumentException("وقت الجرعة المجدول لا يمكن أن يكون في المستقبل.");

        if (request.TakenAt.HasValue && request.TakenAt.Value > DateTime.UtcNow.AddMinutes(5))
            throw new ArgumentException("وقت تناول الجرعة لا يمكن أن يكون في المستقبل.");

        ValidateOptionalText(request.Notes, 500, "ملاحظات الجرعة يجب ألا تتجاوز 500 حرف.");

        if (!Enum.IsDefined(request.Status))
            throw new ArgumentException("حالة الجرعة غير مدعومة.");

        if (request.Status == Domain.Enums.MedicationLogStatus.Taken && !request.TakenAt.HasValue)
            throw new ArgumentException("يجب تسجيل وقت تناول الجرعة عند اختيار حالة تم تناولها.");

        var log = new MedicationLog
        {
            Id = Guid.NewGuid(),
            MedicationId = medicationId,
            ScheduledTime = request.ScheduledTime,
            TakenAt = request.TakenAt,
            Status = request.Status,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim()
        };

        dbContext.MedicationLogs.Add(log);
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapLog(log);
    }

    public async Task<IReadOnlyList<MedicationLogResponse>> GetLogsAsync(
        Guid userId,
        Guid medicationId,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default)
    {
        await GetOwnedMedicationAsync(userId, medicationId, cancellationToken);

        if (from.HasValue && to.HasValue && from > to)
            throw new ArgumentException("تاريخ البداية يجب أن يسبق تاريخ النهاية.");

        var query = dbContext.MedicationLogs
            .AsNoTracking()
            .Where(x => x.MedicationId == medicationId);

        if (from.HasValue)
            query = query.Where(x => x.ScheduledTime >= from.Value);

        if (to.HasValue)
            query = query.Where(x => x.ScheduledTime <= to.Value);

        var logs = await query
            .OrderByDescending(x => x.ScheduledTime)
            .Take(500)
            .ToListAsync(cancellationToken);

        return logs.Select(MapLog).ToList();
    }

    private async Task<Medication> GetOwnedMedicationAsync(
        Guid userId,
        Guid medicationId,
        CancellationToken cancellationToken)
    {
        var medication = await dbContext.Medications
            .SingleOrDefaultAsync(
                x => x.Id == medicationId && x.Patient.UserId == userId,
                cancellationToken);

        return medication ?? throw new KeyNotFoundException("الدواء غير موجود في حسابك.");
    }

    private static void ValidateText(string value, int maxLength, string message)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Trim().Length > maxLength)
            throw new ArgumentException(message);
    }

    private static void ValidateOptionalText(string? value, int maxLength, string message)
    {
        if (value?.Length > maxLength)
            throw new ArgumentException(message);
    }

    private static MedicationResponse MapMedication(Medication medication) =>
        new(
            medication.Id,
            medication.Name,
            medication.Dosage,
            medication.Instructions,
            medication.IsActive,
            medication.Schedules
                .OrderBy(x => x.Time)
                .Select(MapSchedule)
                .ToList());

    private static MedicationScheduleResponse MapSchedule(MedicationSchedule schedule) =>
        new(
            schedule.Id,
            schedule.Time,
            schedule.Frequency,
            schedule.StartDate,
            schedule.EndDate,
            schedule.IsActive);

    private static MedicationLogResponse MapLog(MedicationLog log) =>
        new(
            log.Id,
            log.ScheduledTime,
            log.TakenAt,
            log.Status,
            log.Notes);
}
