using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace Volo.Abp.Identity.AspNetCore;

/// <summary>
/// Base class for ABP token providers that enforce a "single active token" policy:
/// generating a new token automatically invalidates all previously issued tokens.
/// Token validity is enforced by SecurityStamp verification (via the base class) and
/// by the stored hash, which is overwritten each time a new token is generated.
/// </summary>
public abstract class AbpSingleActiveTokenProvider : DataProtectorTokenProvider<IdentityUser>
{
    public const string TokenHashSuffix = "_TokenHash";

    protected IIdentityUserRepository UserRepository { get; }

    protected AbpSingleActiveTokenProvider(
        IDataProtectionProvider dataProtectionProvider,
        IOptions<DataProtectionTokenProviderOptions> options,
        ILogger<DataProtectorTokenProvider<IdentityUser>> logger,
        IIdentityUserRepository userRepository)
        : base(dataProtectionProvider, options, logger)
    {
        UserRepository = userRepository;
    }

    public override async Task<string> GenerateAsync(string purpose, UserManager<IdentityUser> manager, IdentityUser user)
    {
        var token = await base.GenerateAsync(purpose, manager, user);

        await UserRepository.EnsureCollectionLoadedAsync(user, u => u.Tokens);
        var tokenHash = ComputeSha256Hash(token);
        user.SetToken(Options.Name, purpose + TokenHashSuffix, tokenHash);

        await manager.UpdateAsync(user);

        return token;
    }

    public override async Task<bool> ValidateAsync(string purpose, string token, UserManager<IdentityUser> manager, IdentityUser user)
    {
        if (!await base.ValidateAsync(purpose, token, manager, user))
        {
            return false;
        }

        await UserRepository.EnsureCollectionLoadedAsync(user, u => u.Tokens);

        var storedHash = user.FindToken(Options.Name, purpose + TokenHashSuffix)?.Value;
        if (storedHash == null)
        {
            return false;
        }

        var inputHash = ComputeSha256Hash(token);
        return string.Equals(storedHash, inputHash, StringComparison.Ordinal);
    }

    protected virtual string ComputeSha256Hash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }
}
