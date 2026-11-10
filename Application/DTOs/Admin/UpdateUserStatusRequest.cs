using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Admin;

public sealed record UpdateUserStatusRequest(
    [property: Required] AccountStatus Status,
    string? Reason);