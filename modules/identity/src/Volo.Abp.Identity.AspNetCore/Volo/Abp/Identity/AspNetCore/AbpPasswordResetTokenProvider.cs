using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.Identity;

namespace Volo.Abp.Identity.AspNetCore;

/// <summary>
/// Password reset token provider that enforces only the most recently issued
/// token to be valid, with a configurable expiration period.
/// </summary>
public class AbpPasswordResetTokenProvider : AbpSingleActiveTokenProvider
{
    public const string ProviderName = "AbpPasswordReset";

    public AbpPasswordResetTokenProvider(
        IDataProtectionProvider dataProtectionProvider,
        IOptions<AbpPasswordResetTokenProviderOptions> options,
        ILogger<DataProtectorTokenProvider<IdentityUser>> logger,
        IIdentityUserRepository userRepository)
        : base(dataProtectionProvider, options, logger, userRepository)
    {
    }
}
