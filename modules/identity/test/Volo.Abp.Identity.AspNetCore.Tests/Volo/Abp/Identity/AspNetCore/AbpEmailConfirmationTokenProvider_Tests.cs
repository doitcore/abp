using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Shouldly;
using Volo.Abp.Uow;
using Xunit;

namespace Volo.Abp.Identity.AspNetCore;

public class AbpEmailConfirmationTokenProvider_Tests : AbpIdentityAspNetCoreTestBase
{
    protected IIdentityUserRepository UserRepository { get; }
    protected IdentityUserManager UserManager { get; }
    protected IdentityTestData TestData { get; }
    protected IUnitOfWorkManager UnitOfWorkManager { get; }

    public AbpEmailConfirmationTokenProvider_Tests()
    {
        UserRepository = GetRequiredService<IIdentityUserRepository>();
        UserManager = GetRequiredService<IdentityUserManager>();
        TestData = GetRequiredService<IdentityTestData>();
        UnitOfWorkManager = GetRequiredService<IUnitOfWorkManager>();
    }

    [Fact]
    public void AbpEmailConfirmationTokenProvider_Should_Be_Registered()
    {
        var identityOptions = GetRequiredService<IOptions<IdentityOptions>>().Value;

        identityOptions.Tokens.ProviderMap.ShouldContainKey(AbpEmailConfirmationTokenProvider.ProviderName);
        identityOptions.Tokens.ProviderMap[AbpEmailConfirmationTokenProvider.ProviderName].ProviderType
            .ShouldBe(typeof(AbpEmailConfirmationTokenProvider));
    }

    [Fact]
    public void EmailConfirmationTokenProvider_Should_Be_Configured_As_Abp()
    {
        var identityOptions = GetRequiredService<IOptions<IdentityOptions>>().Value;

        identityOptions.Tokens.EmailConfirmationTokenProvider.ShouldBe(AbpEmailConfirmationTokenProvider.ProviderName);
    }

    [Fact]
    public async Task Generate_And_Verify_Email_Confirmation_Token_Should_Succeed()
    {
        using (var uow = UnitOfWorkManager.Begin())
        {
            var john = await UserRepository.GetAsync(TestData.UserJohnId);

            var token = await UserManager.GenerateEmailConfirmationTokenAsync(john);
            token.ShouldNotBeNullOrEmpty();

            var isValid = await UserManager.VerifyUserTokenAsync(
                john,
                UserManager.Options.Tokens.EmailConfirmationTokenProvider,
                UserManager<IdentityUser>.ConfirmEmailTokenPurpose,
                token);

            isValid.ShouldBeTrue();

            await uow.CompleteAsync();
        }
    }

    [Fact]
    public async Task Invalid_Token_Should_Fail_Verification()
    {
        using (var uow = UnitOfWorkManager.Begin())
        {
            var john = await UserRepository.GetAsync(TestData.UserJohnId);

            await UserManager.GenerateEmailConfirmationTokenAsync(john);

            var isValid = await UserManager.VerifyUserTokenAsync(
                john,
                UserManager.Options.Tokens.EmailConfirmationTokenProvider,
                UserManager<IdentityUser>.ConfirmEmailTokenPurpose,
                "invalid-token-value");

            isValid.ShouldBeFalse();

            await uow.CompleteAsync();
        }
    }

    [Fact]
    public async Task Second_Token_Should_Invalidate_First_Token()
    {
        using (var uow = UnitOfWorkManager.Begin())
        {
            var john = await UserRepository.GetAsync(TestData.UserJohnId);

            var firstToken = await UserManager.GenerateEmailConfirmationTokenAsync(john);

            john = await UserRepository.GetAsync(TestData.UserJohnId);
            var secondToken = await UserManager.GenerateEmailConfirmationTokenAsync(john);

            john = await UserRepository.GetAsync(TestData.UserJohnId);

            var firstTokenValid = await UserManager.VerifyUserTokenAsync(
                john,
                UserManager.Options.Tokens.EmailConfirmationTokenProvider,
                UserManager<IdentityUser>.ConfirmEmailTokenPurpose,
                firstToken);

            var secondTokenValid = await UserManager.VerifyUserTokenAsync(
                john,
                UserManager.Options.Tokens.EmailConfirmationTokenProvider,
                UserManager<IdentityUser>.ConfirmEmailTokenPurpose,
                secondToken);

            firstTokenValid.ShouldBeFalse();
            secondTokenValid.ShouldBeTrue();

            await uow.CompleteAsync();
        }
    }

    [Fact]
    public async Task Token_Should_Become_Invalid_After_Email_Confirmation_With_Explicit_Revocation()
    {
        using (var uow = UnitOfWorkManager.Begin())
        {
            var john = await UserRepository.GetAsync(TestData.UserJohnId);

            var token = await UserManager.GenerateEmailConfirmationTokenAsync(john);

            var result = await UserManager.ConfirmEmailAsync(john, token);
            result.Succeeded.ShouldBeTrue();

            // ConfirmEmailAsync does NOT update SecurityStamp, so the hash is not
            // automatically invalidated. Callers that require single-use semantics must
            // explicitly revoke the stored token hash after a successful confirmation.
            john = await UserRepository.GetAsync(TestData.UserJohnId);
            var removeResult = await UserManager.RemoveEmailConfirmationTokenAsync(john);
            removeResult.Succeeded.ShouldBeTrue();

            john = await UserRepository.GetAsync(TestData.UserJohnId);
            var isValid = await UserManager.VerifyUserTokenAsync(
                john,
                UserManager.Options.Tokens.EmailConfirmationTokenProvider,
                UserManager<IdentityUser>.ConfirmEmailTokenPurpose,
                token);

            isValid.ShouldBeFalse();

            await uow.CompleteAsync();
        }
    }

    [Fact]
    public async Task Token_Hash_Should_Persist_Across_UnitOfWork_Boundaries()
    {
        string token;

        // UoW 1: generate the token; UpdateAsync inside GenerateAsync must persist the hash.
        using (var uow = UnitOfWorkManager.Begin(requiresNew: true))
        {
            var john = await UserRepository.GetAsync(TestData.UserJohnId);
            token = await UserManager.GenerateEmailConfirmationTokenAsync(john);
            await uow.CompleteAsync();
        }

        // UoW 2: validate using a fresh DbContext to confirm the hash was written to the DB.
        using (var uow = UnitOfWorkManager.Begin(requiresNew: true))
        {
            var john = await UserRepository.GetAsync(TestData.UserJohnId);
            var isValid = await UserManager.VerifyUserTokenAsync(
                john,
                UserManager.Options.Tokens.EmailConfirmationTokenProvider,
                UserManager<IdentityUser>.ConfirmEmailTokenPurpose,
                token);

            isValid.ShouldBeTrue();
            await uow.CompleteAsync();
        }
    }
}
