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

        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = request.Email.Trim().ToLowerInvariant(),
            Email = request.Email.Trim().ToLowerInvariant(),
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
                LicenseNumber = request.LicenseNumber.Trim(),
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
        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
            throw new UnauthorizedAccessException("Invalid email or password.");

        if (user.AccountStatus != AccountStatus.Active)
            throw new UnauthorizedAccessException("This account is not active.");

        return await BuildResponseAsync(user);
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
            Token = refresh,
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
}
