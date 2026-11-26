using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.DTOs.Auth;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Authentication;

public sealed class AuthService(
    UserManager<User> userManager,
    DiabetesDbContext db,
    IOptions<JwtOptions> jwtOptions) : IAuthService
{
    private readonly JwtOptions _jwt = jwtOptions.Value;

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<Role>(request.Role, true, out var role) || role == Role.Admin)
            throw new InvalidOperationException("Only Patient and Doctor registration is allowed.");

        if (role == Role.Doctor && string.IsNullOrWhiteSpace(request.LicenseNumber))
            throw new InvalidOperationException("Doctor license number is required.");

        var phoneNumber = NormalizePhoneNumber(request.PhoneNumber);
        if (!IsValidPhoneNumber(phoneNumber))
            throw new InvalidOperationException("A valid phone number is required.");

        if (await db.Users.AnyAsync(x => x.PhoneNumber == phoneNumber, cancellationToken))
            throw new InvalidOperationException("This phone number is already registered.");

        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(x => x.Email == email, cancellationToken))
            throw new InvalidOperationException("This email is already registered.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            PhoneNumber = phoneNumber,
            FullName = request.FullName.Trim(),
            Role = role,
            AccountStatus = role == Role.Doctor ? AccountStatus.Pending : AccountStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));

        if (role == Role.Patient)
        {
            db.Patients.Add(new Patient { Id = Guid.NewGuid(), UserId = user.Id, CreatedAt = DateTime.UtcNow });
        }
        else
        {
            var doctor = new Doctor
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                LicenseNumber = request.LicenseNumber?.Trim() ?? throw new InvalidOperationException("Doctor license number is required."),
                Specialization = request.Specialization?.Trim(),
                ClinicName = request.ClinicName?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            db.Doctors.Add(doctor);
            db.DoctorApprovalRequests.Add(new DoctorApprovalRequest
            {
                Id = Guid.NewGuid(),
                DoctorId = doctor.Id,
                Status = ApprovalStatus.Pending,
                SubmittedAt = DateTime.UtcNow
            });
        }

        await db.SaveChangesAsync(cancellationToken);

        if (role == Role.Doctor)
        {
            return new RegisterResponse(
                "تم إنشاء حساب الطبيب بنجاح، وهو بانتظار موافقة الإدارة قبل تسجيل الدخول.",
                role.ToString(),
                user.AccountStatus.ToString(),
                true,
                null);
        }

        return new RegisterResponse(
            "تم إنشاء الحساب بنجاح ويمكنك تسجيل الدخول الآن.",
            role.ToString(),
            user.AccountStatus.ToString(),
            false,
            await BuildResponseAsync(user));
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var identifier = request.Username.Trim();
        var normalizedPhone = NormalizePhoneNumber(identifier);

        User? user = null;
        if (IsValidPhoneNumber(normalizedPhone))
        {
            user = await db.Users.SingleOrDefaultAsync(
                x => x.PhoneNumber == normalizedPhone,
                cancellationToken);
        }

        user ??= await userManager.FindByNameAsync(identifier)
            ?? await userManager.FindByEmailAsync(identifier);

        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
            throw new UnauthorizedAccessException("Invalid phone number or password.");

        if (user.AccountStatus != AccountStatus.Active)
            throw new UnauthorizedAccessException("This account is not active.");

        return await BuildResponseAsync(user);
    }

    public async Task<AuthResponse> RefreshAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            throw new ArgumentException("Refresh token is required.");

        var refreshToken = request.RefreshToken.Trim();
        var tokenHash = HashRefreshToken(refreshToken);
        var storedToken = await db.RefreshTokens
            .Include(x => x.User)
            .SingleOrDefaultAsync(x => x.Token == tokenHash, cancellationToken);

        if (storedToken is null || storedToken.RevokedAt.HasValue || storedToken.ExpiresAt <= DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token is invalid or expired.");

        if (storedToken.User.AccountStatus != AccountStatus.Active)
            throw new UnauthorizedAccessException("This account is not active.");

        storedToken.RevokedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return await BuildResponseAsync(storedToken.User);
    }

    private async Task<AuthResponse> BuildResponseAsync(User user)
    {
        var email = user.Email ?? throw new InvalidOperationException("User email is required to issue a token.");
        var expires = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenMinutes);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _jwt.Issuer,
            _jwt.Audience,
            claims,
            expires: expires,
            signingCredentials: credentials);

        var refresh = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = HashRefreshToken(refresh),
            ExpiresAt = DateTime.UtcNow.AddDays(14),
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        return new AuthResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            refresh,
            expires,
            user.Role.ToString(),
            user.AccountStatus.ToString());
    }

    private static string NormalizePhoneNumber(string value)
    {
        var trimmed = value.Trim();
        var builder = new StringBuilder(trimmed.Length);

        foreach (var character in trimmed)
        {
            if (char.IsDigit(character))
                builder.Append(character);
            else if (character == '+' && builder.Length == 0)
                builder.Append(character);
        }

        return builder.ToString();
    }

    private static bool IsValidPhoneNumber(string value) =>
        System.Text.RegularExpressions.Regex.IsMatch(value, @"^\+?[1-9]\d{7,14}$");

    private static string HashRefreshToken(string token)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hash);
    }
}
