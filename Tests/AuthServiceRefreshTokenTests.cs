using System.Security.Cryptography;
using System.Text;
using Application.DTOs.Auth;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Authentication;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Xunit;

namespace Tests;

public sealed class AuthServiceRefreshTokenTests
{
    [Fact]
    public async Task RefreshAsync_RotatesTokenAndRevokesPreviousToken()
    {
        await using var db = CreateDb();
        var user = CreateUser();
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var oldToken = CreateRefreshToken(user, rawToken, DateTime.UtcNow.AddDays(1));

        db.Users.Add(user);
        db.RefreshTokens.Add(oldToken);
        await db.SaveChangesAsync();

        var service = CreateService(db);

        var response = await service.RefreshAsync(
            new RefreshTokenRequest(rawToken),
            CancellationToken.None);

        var revoked = await db.RefreshTokens.SingleAsync(x => x.Id == oldToken.Id);
        var replacement = await db.RefreshTokens.SingleAsync(x => x.Token == HashRefreshToken(response.RefreshToken));

        Assert.True(revoked.RevokedAt.HasValue);
        Assert.NotEqual(oldToken.Token, response.RefreshToken);
        Assert.Equal(user.Id, replacement.UserId);
        Assert.Equal(HashRefreshToken(rawToken), revoked.Token);
    }

    [Fact]
    public async Task RefreshAsync_RejectsRevokedToken()
    {
        await using var db = CreateDb();
        var user = CreateUser();
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var oldToken = CreateRefreshToken(user, rawToken, DateTime.UtcNow.AddDays(1));
        oldToken.RevokedAt = DateTime.UtcNow.AddMinutes(-1);

        db.Users.Add(user);
        db.RefreshTokens.Add(oldToken);
        await db.SaveChangesAsync();

        var service = CreateService(db);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.RefreshAsync(
                new RefreshTokenRequest(rawToken),
                CancellationToken.None));
    }

    [Fact]
    public async Task RefreshAsync_RejectsExpiredToken()
    {
        await using var db = CreateDb();
        var user = CreateUser();
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var oldToken = CreateRefreshToken(user, rawToken, DateTime.UtcNow.AddMinutes(-1));

        db.Users.Add(user);
        db.RefreshTokens.Add(oldToken);
        await db.SaveChangesAsync();

        var service = CreateService(db);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.RefreshAsync(
                new RefreshTokenRequest(rawToken),
                CancellationToken.None));
    }

    [Fact]
    public async Task RefreshAsync_RejectsTokenForDisabledUser()
    {
        await using var db = CreateDb();
        var user = CreateUser();
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var refreshToken = CreateRefreshToken(user, rawToken, DateTime.UtcNow.AddDays(1));

        db.Users.Add(user);
        db.RefreshTokens.Add(refreshToken);
        await db.SaveChangesAsync();

        user.AccountStatus = AccountStatus.Suspended;
        await db.SaveChangesAsync();

        var service = CreateService(db);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.RefreshAsync(
                new RefreshTokenRequest(rawToken),
                CancellationToken.None));
    }

    private static DiabetesDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<DiabetesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new DiabetesDbContext(options);
    }

    private static AuthService CreateService(DiabetesDbContext db)
    {
        var options = Options.Create(new JwtOptions
        {
            Issuer = "test-issuer",
            Audience = "test-audience",
            Key = "test-key-that-is-long-enough-for-hmac-sha256",
            AccessTokenMinutes = 30
        });

        return new AuthService(null!, db, options);
    }

    private static User CreateUser() => new()
    {
        Id = Guid.NewGuid(),
        UserName = "patient@example.com",
        Email = "patient@example.com",
        FullName = "Test Patient",
        Role = Role.Patient,
        AccountStatus = AccountStatus.Active,
        CreatedAt = DateTime.UtcNow
    };

    private static RefreshToken CreateRefreshToken(
        User user,
        string rawToken,
        DateTime expiresAt) => new()
    {
        Id = Guid.NewGuid(),
        UserId = user.Id,
        User = user,
        Token = HashRefreshToken(rawToken),
        ExpiresAt = expiresAt,
        CreatedAt = DateTime.UtcNow
    };

    private static string HashRefreshToken(string token)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hash);
    }
}
