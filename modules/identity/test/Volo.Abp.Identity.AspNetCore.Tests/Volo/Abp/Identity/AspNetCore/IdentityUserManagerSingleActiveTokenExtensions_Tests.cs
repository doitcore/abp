using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Shouldly;
using Volo.Abp.Identity;
using Volo.Abp.Uow;
using Xunit;

namespace Volo.Abp.Identity.AspNetCore;

public class IdentityUserManagerSingleActiveTokenExtensions_Tests : AbpIdentityAspNetCoreTestBase
{
    private const string NewEmail = "newemail@example.com";

    protected IIdentityUserRepository UserRepository { get; }
    protected IdentityUserManager UserManager { get; }
    protected IdentityTestData TestData { get; }
    protected IUnitOfWorkManager UnitOfWorkManager { get; }

    public IdentityUserManagerSingleActiveTokenExtensions_Tests()
    {
        UserRepository = GetRequiredService<IIdentityUserRepository>();
        UserManager = GetRequiredService<IdentityUserManager>();
        TestData = GetRequiredService<IdentityTestData>();
        UnitOfWorkManager = GetRequiredService<IUnitOfWorkManager>();
    }

    [Fact]
    public async Task Should_Remove_PasswordReset_TokenHash()
    {
        using (var uow = UnitOfWorkManager.Begin())
        {
            var user = await UserRepository.GetAsync(TestData.UserJohnId);
            var providerName = UserManager.Options.Tokens.PasswordResetTokenProvider;
            var tokenKey = UserManager<IdentityUser>.ResetPasswordTokenPurpose + AbpSingleActiveTokenProvider.TokenHashSuffix;

            await UserManager.SetAuthenticationTokenAsync(user, providerName, tokenKey, "hash-value");
            (await UserManager.GetAuthenticationTokenAsync(user, providerName, tokenKey)).ShouldNotBeNull();

            var result = await UserManager.RemovePasswordResetTokenAsync(user);
            result.Succeeded.ShouldBeTrue();

            (await UserManager.GetAuthenticationTokenAsync(user, providerName, tokenKey)).ShouldBeNull();

            await uow.CompleteAsync();
        }
    }

    [Fact]
    public async Task Should_Remove_EmailConfirmation_TokenHash()
    {
        using (var uow = UnitOfWorkManager.Begin())
        {
            var user = await UserRepository.GetAsync(TestData.UserJohnId);
            var providerName = UserManager.Options.Tokens.EmailConfirmationTokenProvider;
            var tokenKey = UserManager<IdentityUser>.ConfirmEmailTokenPurpose + AbpSingleActiveTokenProvider.TokenHashSuffix;

            await UserManager.SetAuthenticationTokenAsync(user, providerName, tokenKey, "hash-value");
            (await UserManager.GetAuthenticationTokenAsync(user, providerName, tokenKey)).ShouldNotBeNull();

            var result = await UserManager.RemoveEmailConfirmationTokenAsync(user);
            result.Succeeded.ShouldBeTrue();

            (await UserManager.GetAuthenticationTokenAsync(user, providerName, tokenKey)).ShouldBeNull();

            await uow.CompleteAsync();
        }
    }

    [Fact]
    public async Task Should_Remove_ChangeEmail_TokenHash()
    {
        using (var uow = UnitOfWorkManager.Begin())
        {
            var user = await UserRepository.GetAsync(TestData.UserJohnId);
            var providerName = UserManager.Options.Tokens.ChangeEmailTokenProvider;
            var tokenKey = UserManager<IdentityUser>.GetChangeEmailTokenPurpose(NewEmail) + AbpSingleActiveTokenProvider.TokenHashSuffix;

            await UserManager.SetAuthenticationTokenAsync(user, providerName, tokenKey, "hash-value");
            (await UserManager.GetAuthenticationTokenAsync(user, providerName, tokenKey)).ShouldNotBeNull();

            var result = await UserManager.RemoveChangeEmailTokenAsync(user, NewEmail);
            result.Succeeded.ShouldBeTrue();

            (await UserManager.GetAuthenticationTokenAsync(user, providerName, tokenKey)).ShouldBeNull();

            await uow.CompleteAsync();
        }
    }
}
