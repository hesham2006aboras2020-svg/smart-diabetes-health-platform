using Infrastructure.Authentication;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<DiabetesDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddScoped<Application.Interfaces.IAuthService, AuthService>();
        services.AddScoped<Application.Interfaces.IAdminService, AdminService>();
        services.AddScoped<Application.Interfaces.IGlucoseService, GlucoseService>();
        services.AddScoped<Application.Interfaces.IMedicationService, MedicationService>();
        services.AddScoped<Application.Interfaces.IDoctorPatientService, DoctorPatientService>();
        services.AddScoped<Application.Interfaces.IDoctorNoteService, DoctorNoteService>();
        services.AddScoped<Application.Interfaces.IHealthAnalyticsService, HealthAnalyticsService>();
        services.AddScoped<Application.Interfaces.IAlertService, AlertService>();
        services.AddScoped<Application.Interfaces.IDoctorDashboardService, DoctorDashboardService>();
        services.AddScoped<Application.Interfaces.IHealthReportService, HealthReportService>();
        services.AddScoped<AdminSeeder>();

        services.AddIdentityCore<Domain.Entities.User>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = false;
        })
        .AddRoles<IdentityRole<Guid>>()
        .AddEntityFrameworkStores<DiabetesDbContext>()
        .AddDefaultTokenProviders();

        return services;
    }
}