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
        var phoneNumber = PhoneNumberNormalizer.Normalize(configuration["AdminSeed:PhoneNumber"]);
        var password = configuration["AdminSeed:Password"];
        var fullName = configuration["AdminSeed:FullName"]?.Trim();

        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(phoneNumber) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(fullName))
            return;

        if (!PhoneNumberNormalizer.IsValid(phoneNumber))
            throw new InvalidOperationException("Configured admin phone number is invalid.");

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

            if (string.IsNullOrWhiteSpace(existing.PhoneNumber))
            {
                existing.PhoneNumber = phoneNumber;
                var updateResult = await userManager.UpdateAsync(existing);
                if (!updateResult.Succeeded)
                    throw new InvalidOperationException(
                        string.Join("; ", updateResult.Errors.Select(x => x.Description)));
            }

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
            PhoneNumber = phoneNumber,
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
