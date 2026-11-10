using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.DTOs.Admin;

public sealed record UpdateUserStatusRequest(
    [property: Required] AccountStatus Status,
    string? Reason);