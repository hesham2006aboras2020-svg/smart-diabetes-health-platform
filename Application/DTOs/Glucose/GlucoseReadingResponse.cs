using Domain.Enums;

namespace Application.DTOs.Glucose;

public sealed record GlucoseReadingResponse(
    Guid Id,
    decimal GlucoseValue,
    GlucoseUnit Unit,
    DateTime MeasurementDateTime,
    MeasurementContext MeasurementContext,
    string? Notes,
    DateTime CreatedAt,
    string Status);