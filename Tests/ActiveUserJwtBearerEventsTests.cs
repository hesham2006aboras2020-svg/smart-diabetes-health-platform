using System.Security.Claims;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Authentication;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Tests;

public sealed class ActiveUserJwtBearerEventsTests
{
    [Fact]
    public async Task TokenValidated_AllowsActiveUser()
    {
        await using var db = CreateDb();
        var user = CreateUser(Role.Patient, AccountStatus.Active);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var context = CreateContext(user.Id, user.Role);
        await new ActiveUserJwtBearerEvents(db).TokenValidated(context);

        Assert.Null(context.Result);
    }

    [Theory]
    [InlineData(Role.Patient)]
    [InlineData(Role.Doctor)]
    [InlineData(Role.Admin)]
    public async Task TokenValidated_RejectsPreviouslyIssuedTokenForDisabledUser(Role role)
    {
        await using var db = CreateDb();
        var user = CreateUser(role, AccountStatus.Active);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var previouslyIssuedTokenPrincipal = CreateContext(user.Id, role);
        user.AccountStatus = AccountStatus.Suspended;
        await db.SaveChangesAsync();

        await new ActiveUserJwtBearerEvents(db).TokenValidated(previouslyIssuedTokenPrincipal);

        Assert.NotNull(previouslyIssuedTokenPrincipal.Result?.Failure);
    }

    [Fact]
    public async Task TokenValidated_AllowsUserAfterReactivation()
    {
        await using var db = CreateDb();
        var user = CreateUser(Role.Patient, AccountStatus.Suspended);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        user.AccountStatus = AccountStatus.Active;
        await db.SaveChangesAsync();

        var context = CreateContext(user.Id, user.Role);
        await new ActiveUserJwtBearerEvents(db).TokenValidated(context);

        Assert.Null(context.Result);
    }

    private static DiabetesDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<DiabetesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static TokenValidatedContext CreateContext(Guid userId, Role role)
    {
        var scheme = new AuthenticationScheme(
            JwtBearerDefaults.AuthenticationScheme,
            JwtBearerDefaults.AuthenticationScheme,
            typeof(JwtBearerHandler));
        var context = new TokenValidatedContext(
            new DefaultHttpContext(),
            scheme,
            new JwtBearerOptions());

        context.Principal = new ClaimsPrincipal(
            new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Role, role.ToString())
                ],
                JwtBearerDefaults.AuthenticationScheme));

        return context;
    }

    private static User CreateUser(Role role, AccountStatus status) => new()
    {
        Id = Guid.NewGuid(),
        UserName = $"{role}-{Guid.NewGuid()}@example.test",
        Email = $"{role}-{Guid.NewGuid()}@example.test",
        FullName = "Test User",
        Role = role,
        AccountStatus = status,
        CreatedAt = DateTime.UtcNow
    };
}
