using Application.DTOs.Auth;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Authentication;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Tests;

public sealed class AuthServicePhoneAuthTests
{
    [Fact]
    public async Task RegisterAsync_CreatesPatientUsingNormalizedPhone()
    {
        await using var db = CreateDb();
        using var provider = CreateProvider(db);
        using var scope = provider.CreateScope();
        var service = CreateService(db, scope);

        var response = await service.RegisterAsync(
            CreatePatientRequest("+967 (777) 123-456"),
            CancellationToken.None);

        var user = await db.Users.SingleAsync();
        Assert.Equal("967777123456", user.PhoneNumber);
        Assert.Equal(user.PhoneNumber, user.UserName);
        Assert.EndsWith("@phone.invalid", user.Email);
        Assert.Equal(Role.Patient, user.Role);
        Assert.Equal(AccountStatus.Active, user.AccountStatus);
        Assert.Single(await db.Patients.ToListAsync());
        Assert.False(response.RequiresApproval);
        Assert.NotNull(response.Authentication);
    }

    [Fact]
    public async Task RegisterAsync_RejectsDuplicatePhoneWithDifferentFormatting()
    {
        await using var db = CreateDb();
        using var provider = CreateProvider(db);
        using var scope = provider.CreateScope();
        var service = CreateService(db, scope);

        await service.RegisterAsync(
            CreatePatientRequest("+٩٦٧ (٧٧٧) ١٢٣-٤٥٦"),
            CancellationToken.None);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RegisterAsync(
                CreatePatientRequest("00967 777 123 456"),
                CancellationToken.None));

        Assert.Contains("phone number is already registered", exception.Message);
        Assert.Single(await db.Users.ToListAsync());
    }

    [Fact]
    public async Task LoginAsync_AcceptsPhoneNumber()
    {
        await using var db = CreateDb();
        using var provider = CreateProvider(db);
        using var scope = provider.CreateScope();
        var service = CreateService(db, scope);
        const string password = "123456";

        await service.RegisterAsync(
            CreatePatientRequest("+967 (777) 123-456", password),
            CancellationToken.None);

        var response = await service.LoginAsync(
            new LoginRequest("00967 777 123 456", password),
            CancellationToken.None);

        Assert.False(string.IsNullOrWhiteSpace(response.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(response.RefreshToken));
        Assert.Equal("Patient", response.Role);
    }

    [Fact]
    public async Task LoginAsync_DoesNotAcceptEmailAsIdentifier()
    {
        await using var db = CreateDb();
        using var provider = CreateProvider(db);
        using var scope = provider.CreateScope();
        var service = CreateService(db, scope);

        await service.RegisterAsync(
            CreatePatientRequest("+967 (777) 123-456"),
            CancellationToken.None);
        var internalEmail = (await db.Users.SingleAsync()).Email!;

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.LoginAsync(
                new LoginRequest(internalEmail, "123456"),
                CancellationToken.None));
    }

    [Fact]
    public async Task RegisterAsync_KeepsDoctorPendingApprovalWorkflow()
    {
        await using var db = CreateDb();
        using var provider = CreateProvider(db);
        using var scope = provider.CreateScope();
        var service = CreateService(db, scope);
        var request = new RegisterRequest(
            "Test Doctor",
            "+967 777 123 456",
            "123456",
            Role.Doctor.ToString(),
            "LIC-123",
            "Endocrinology",
            "Test Clinic");

        var response = await service.RegisterAsync(request, CancellationToken.None);

        var user = await db.Users.SingleAsync();
        var doctor = await db.Doctors.SingleAsync();
        var approval = await db.DoctorApprovalRequests.SingleAsync();
        Assert.Equal(Role.Doctor, user.Role);
        Assert.Equal(AccountStatus.Pending, user.AccountStatus);
        Assert.Equal(user.Id, doctor.UserId);
        Assert.Equal(ApprovalStatus.Pending, approval.Status);
        Assert.Equal(doctor.Id, approval.DoctorId);
        Assert.True(response.RequiresApproval);
        Assert.Null(response.Authentication);
    }

    private static DiabetesDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<DiabetesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static ServiceProvider CreateProvider(DiabetesDbContext db)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped(_ => db);
        services.AddIdentityCore<User>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredUniqueChars = 1;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<DiabetesDbContext>()
            .AddPasswordValidator<NumericPasswordValidator<User>>();

        return services.BuildServiceProvider();
    }

    private static AuthService CreateService(DiabetesDbContext db, IServiceScope scope) =>
        new(
            scope.ServiceProvider.GetRequiredService<UserManager<User>>(),
            db,
            Options.Create(new JwtOptions
            {
                Issuer = "test-issuer",
                Audience = "test-audience",
                Key = "test-key-that-is-long-enough-for-hmac-sha256",
                AccessTokenMinutes = 30
            }));

    private static RegisterRequest CreatePatientRequest(
        string phoneNumber,
        string password = "123456") =>
        new(
            "Test Patient",
            phoneNumber,
            password,
            Role.Patient.ToString(),
            null,
            null,
            null);
}
