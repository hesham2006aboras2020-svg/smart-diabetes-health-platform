using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Authentication;

public sealed class NumericPasswordValidator<TUser> : IPasswordValidator<TUser>
    where TUser : class
{
    public Task<IdentityResult> ValidateAsync(
        UserManager<TUser> manager,
        TUser user,
        string? password)
    {
        if (string.IsNullOrEmpty(password) ||
            password.Length is < 6 or > 15 ||
            password.Any(c => c is < '0' or > '9'))
        {
            return Task.FromResult(IdentityResult.Failed(new IdentityError
            {
                Code = "NumericPassword",
                Description = "Password must contain only digits and be 6 to 15 digits long."
            }));
        }

        return Task.FromResult(IdentityResult.Success);
    }
}
