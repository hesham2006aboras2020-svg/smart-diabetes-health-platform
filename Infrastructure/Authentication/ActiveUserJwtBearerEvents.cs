using System.Security.Claims;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Authentication;

public sealed class ActiveUserJwtBearerEvents(DiabetesDbContext dbContext) : JwtBearerEvents
{
    public override async Task TokenValidated(TokenValidatedContext context)
    {
        var userIdValue = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            context.Fail("The token does not identify a valid user.");
            return;
        }

        var isActive = await dbContext.Users
            .AsNoTracking()
            .AnyAsync(
                user => user.Id == userId && user.AccountStatus == AccountStatus.Active,
                context.HttpContext.RequestAborted);

        if (!isActive)
            context.Fail("The account is not active.");
    }
}
