using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Authentication;

public sealed class AdminSeeder(
    UserManager<User> userManager,
    RoleManager<IdentityRole<Guid>> roleManager,
    IConfiguration configuration)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var username = configuration["AdminSeed:Username"]?.Trim();
        var email = configuration["AdminSeed:Email"]?.Trim().ToLowerInvariant();
        var password = configuration["AdminSeed:Password"];
        var fullName = configuration["AdminSeed:FullName"]?.Trim();

        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(fullName))
            return;

        if (!await roleManager.RoleExistsAsync(Role.Admin.ToString()))
        {
            var roleResult = await roleManager.CreateAsync(
                new IdentityRole<Guid>(Role.Admin.ToString()));

            if (!roleResult.Succeeded)
                throw new InvalidOperationException(
                    string.Join("; ", roleResult.Errors.Select(x => x.Description)));
        }

        var existing = await userManager.FindByNameAsync(username);
        if (existing is not null)
        {
            if (existing.Role != Role.Admin)
                throw new InvalidOperationException("Configured admin username belongs to a non-admin account.");

            if (!await userManager.IsInRoleAsync(existing, Role.Admin.ToString()))
            {
                var addRoleResult = await userManager.AddToRoleAsync(existing, Role.Admin.ToString());
                if (!addRoleResult.Succeeded)
                    throw new InvalidOperationException(
                        string.Join("; ", addRoleResult.Errors.Select(x => x.Description)));
            }

            return;
        }

        var admin = new User
        {
            Id = Guid.NewGuid(),
            UserName = username,
            Email = email,
            EmailConfirmed = true,
            FullName = fullName,
            Role = Role.Admin,
            AccountStatus = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await userManager.CreateAsync(admin, password);
        if (!createResult.Succeeded)
            throw new InvalidOperationException(
                string.Join("; ", createResult.Errors.Select(x => x.Description)));

        var roleAssignment = await userManager.AddToRoleAsync(admin, Role.Admin.ToString());
        if (!roleAssignment.Succeeded)
            throw new InvalidOperationException(
                string.Join("; ", roleAssignment.Errors.Select(x => x.Description)));
    }
}