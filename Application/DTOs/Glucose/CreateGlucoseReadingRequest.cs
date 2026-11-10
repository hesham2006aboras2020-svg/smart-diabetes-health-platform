using Domain.Enums;

namespace Application.DTOs.Glucose;

public sealed record CreateGlucoseReadingRequest(
    decimal GlucoseValue,
    GlucoseUnit Unit,
    DateTime MeasurementDateTime,
    MeasurementContext MeasurementContext,
    string? Notes);