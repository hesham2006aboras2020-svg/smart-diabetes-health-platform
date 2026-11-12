using Application.DTOs.Medication;

namespace Application.Interfaces;

public interface IMedicationService
{
    Task<MedicationResponse> CreateMedicationAsync(
        Guid userId,
        CreateMedicationRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MedicationResponse>> GetMedicationsAsync(
        Guid userId,
        bool activeOnly,
        CancellationToken cancellationToken = default);

    Task SetMedicationStatusAsync(
        Guid userId,
        Guid medicationId,
        bool isActive,
        CancellationToken cancellationToken = default);

    Task<MedicationScheduleResponse> AddScheduleAsync(
        Guid userId,
        Guid medicationId,
        CreateMedicationScheduleRequest request,
        CancellationToken cancellationToken = default);

    Task<MedicationLogResponse> AddLogAsync(
        Guid userId,
        Guid medicationId,
        CreateMedicationLogRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MedicationLogResponse>> GetLogsAsync(
        Guid userId,
        Guid medicationId,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default);