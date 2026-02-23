using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Volo.Abp.Identity.AspNetCore;

/// <summary>
/// Provides extension methods on <see cref="IdentityUserManager"/> for invalidating
/// single-active tokens managed by <see cref="AbpSingleActiveTokenProvider"/>.
/// These helpers live in the AspNetCore layer because they depend on
/// <see cref="AbpSingleActiveTokenProvider.TokenHashSuffix"/>.
/// </summary>
public static class IdentityUserManagerSingleActiveTokenExtensions
{
    /// <summary>
    /// Removes the stored password-reset token hash for <paramref name="user"/>,
    /// immediately invalidating any previously issued password-reset token.
    /// </summary>
    public static Task<IdentityResult> RemovePasswordResetTokenAsync(this IdentityUserManager manager, IdentityUser user)
    {
        var name = UserManager<IdentityUser>.ResetPasswordTokenPurpose + AbpSingleActiveTokenProvider.TokenHashSuffix;
        return manager.RemoveAuthenticationTokenAsync(user, manager.Options.Tokens.PasswordResetTokenProvider, name);
    }

    /// <summary>
    /// Removes the stored email-confirmation token hash for <paramref name="user"/>,
    /// immediately invalidating any previously issued email-confirmation token.
    /// </summary>
    public static Task<IdentityResult> RemoveEmailConfirmationTokenAsync(this IdentityUserManager manager, IdentityUser user)
    {
        var name = UserManager<IdentityUser>.ConfirmEmailTokenPurpose + AbpSingleActiveTokenProvider.TokenHashSuffix;
        return manager.RemoveAuthenticationTokenAsync(user, manager.Options.Tokens.EmailConfirmationTokenProvider, name);
    }

    /// <summary>
    /// Removes the stored change-email token hash for <paramref name="user"/>,
    /// immediately invalidating any previously issued change-email token for <paramref name="newEmail"/>.
    /// </summary>
    public static Task<IdentityResult> RemoveChangeEmailTokenAsync(this IdentityUserManager manager, IdentityUser user, string newEmail)
    {
        var name = UserManager<IdentityUser>.GetChangeEmailTokenPurpose(newEmail) + AbpSingleActiveTokenProvider.TokenHashSuffix;
        return manager.RemoveAuthenticationTokenAsync(user, manager.Options.Tokens.ChangeEmailTokenProvider, name);
    }
}
