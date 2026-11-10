using Domain.Enums;

namespace Application.DTOs.DoctorDashboard;

public sealed record DoctorPatientReadingResponse(
    Guid Id,
    decimal GlucoseValue,
    GlucoseUnit Unit,
    DateTime MeasurementDateTime,
    MeasurementContext MeasurementContext,
    string? Notes);