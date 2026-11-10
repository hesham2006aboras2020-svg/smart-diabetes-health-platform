using Application.DTOs.Patient;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class PatientProfileService(
    UserManager<User> userManager,
    DiabetesDbContext db) : IPatientProfileService
{
    public async Task<PatientProfileResponse> GetAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var patient = await db.Patients
            .AsNoTracking()
            .Include(x => x.User)
            .SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken)
            ?? throw new KeyNotFoundException("ملف المريض غير موجود.");

        return Map(patient);
    }

    public async Task<PatientProfileResponse> UpdateAsync(
        Guid userId,
        UpdatePatientProfileRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
            throw new ArgumentException("الاسم الكامل مطلوب.");

        var fullName = request.FullName.Trim();
        if (fullName.Length < 2 || fullName.Length > 100)
            throw new ArgumentException("الاسم الكامل يجب أن يكون بين حرفين و100 حرف.");

        if (request.DateOfBirth.HasValue && request.DateOfBirth.Value > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ArgumentException("تاريخ الميلاد لا يمكن أن يكون في المستقبل.");

        var gender = string.IsNullOrWhiteSpace(request.Gender)
            ? null
            : request.Gender.Trim();

        if (gender is not null && gender.Length > 50)
            throw new ArgumentException("الجنس يجب ألا يتجاوز 50 حرفًا.");

        var patient = await db.Patients
            .Include(x => x.User)
            .SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken)
            ?? throw new KeyNotFoundException("ملف المريض غير موجود.");

        if (patient.User.AccountStatus != AccountStatus.Active)
            throw new UnauthorizedAccessException("حساب المريض غير نشط.");

        patient.User.FullName = fullName;
        patient.User.UpdatedAt = DateTime.UtcNow;
        patient.DateOfBirth = request.DateOfBirth;
        patient.Gender = gender;

        await db.SaveChangesAsync(cancellationToken);

        return Map(patient);
    }

    private static PatientProfileResponse Map(Patient patient)
    {
        var email = patient.User.Email
            ?? throw new InvalidOperationException("بريد المستخدم غير موجود.");

        return new PatientProfileResponse(
            patient.Id,
            patient.User.FullName,
            email,
            patient.DateOfBirth,
            patient.Gender,
            patient.User.AccountStatus.ToString(),
            patient.CreatedAt);
    }
}