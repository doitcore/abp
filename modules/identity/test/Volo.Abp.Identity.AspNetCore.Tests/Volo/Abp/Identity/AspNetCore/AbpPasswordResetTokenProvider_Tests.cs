using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Shouldly;
using Volo.Abp.Uow;
using Xunit;

namespace Volo.Abp.Identity.AspNetCore;

public class AbpPasswordResetTokenProvider_Tests : AbpIdentityAspNetCoreTestBase
{
    protected IIdentityUserRepository UserRepository { get; }
    protected IdentityUserManager UserManager { get; }
    protected IdentityTestData TestData { get; }
    protected IUnitOfWorkManager UnitOfWorkManager { get; }

    public AbpPasswordResetTokenProvider_Tests()
    {
        UserRepository = GetRequiredService<IIdentityUserRepository>();
        UserManager = GetRequiredService<IdentityUserManager>();
        TestData = GetRequiredService<IdentityTestData>();
        UnitOfWorkManager = GetRequiredService<IUnitOfWorkManager>();
    }

    [Fact]
    public void AbpPasswordResetTokenProvider_Should_Be_Registered()
    {
        var identityOptions = GetRequiredService<IOptions<IdentityOptions>>().Value;

        identityOptions.Tokens.ProviderMap.ShouldContainKey(AbpPasswordResetTokenProvider.ProviderName);
        identityOptions.Tokens.ProviderMap[AbpPasswordResetTokenProvider.ProviderName].ProviderType
            .ShouldBe(typeof(AbpPasswordResetTokenProvider));
    }

    [Fact]
    public void PasswordResetTokenProvider_Should_Be_Configured_As_Abp()
    {
        var identityOptions = GetRequiredService<IOptions<IdentityOptions>>().Value;

        identityOptions.Tokens.PasswordResetTokenProvider.ShouldBe(AbpPasswordResetTokenProvider.ProviderName);
    }

    [Fact]
    public async Task Generate_And_Verify_Password_Reset_Token_Should_Succeed()
    {
        using (var uow = UnitOfWorkManager.Begin())
        {
            var john = await UserRepository.GetAsync(TestData.UserJohnId);

            var token = await UserManager.GeneratePasswordResetTokenAsync(john);
            token.ShouldNotBeNullOrEmpty();

            var isValid = await UserManager.VerifyUserTokenAsync(
                john,
                UserManager.Options.Tokens.PasswordResetTokenProvider,
                UserManager<IdentityUser>.ResetPasswordTokenPurpose,
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

            await UserManager.GeneratePasswordResetTokenAsync(john);

            var isValid = await UserManager.VerifyUserTokenAsync(
                john,
                UserManager.Options.Tokens.PasswordResetTokenProvider,
                UserManager<IdentityUser>.ResetPasswordTokenPurpose,
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

            var firstToken = await UserManager.GeneratePasswordResetTokenAsync(john);

            john = await UserRepository.GetAsync(TestData.UserJohnId);
            var secondToken = await UserManager.GeneratePasswordResetTokenAsync(john);

            john = await UserRepository.GetAsync(TestData.UserJohnId);

            var firstTokenValid = await UserManager.VerifyUserTokenAsync(
                john,
                UserManager.Options.Tokens.PasswordResetTokenProvider,
                UserManager<IdentityUser>.ResetPasswordTokenPurpose,
                firstToken);

            var secondTokenValid = await UserManager.VerifyUserTokenAsync(
                john,
                UserManager.Options.Tokens.PasswordResetTokenProvider,
                UserManager<IdentityUser>.ResetPasswordTokenPurpose,
                secondToken);

            firstTokenValid.ShouldBeFalse();
            secondTokenValid.ShouldBeTrue();

            await uow.CompleteAsync();
        }
    }

    [Fact]
    public async Task Token_Should_Become_Invalid_After_Password_Reset()
    {
        using (var uow = UnitOfWorkManager.Begin())
        {
            var john = await UserRepository.GetAsync(TestData.UserJohnId);

            var token = await UserManager.GeneratePasswordResetTokenAsync(john);

            john = await UserRepository.GetAsync(TestData.UserJohnId);
            var result = await UserManager.ResetPasswordAsync(john, token, "1q2w3E*NewP@ss!");
            result.Succeeded.ShouldBeTrue();

            // SecurityStamp has changed after reset, so the old token must be invalid.
            john = await UserRepository.GetAsync(TestData.UserJohnId);
            var isValid = await UserManager.VerifyUserTokenAsync(
                john,
                UserManager.Options.Tokens.PasswordResetTokenProvider,
                UserManager<IdentityUser>.ResetPasswordTokenPurpose,
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
            token = await UserManager.GeneratePasswordResetTokenAsync(john);
            await uow.CompleteAsync();
        }

        // UoW 2: validate using a fresh DbContext to confirm the hash was written to the DB.
        using (var uow = UnitOfWorkManager.Begin(requiresNew: true))
        {
            var john = await UserRepository.GetAsync(TestData.UserJohnId);
            var isValid = await UserManager.VerifyUserTokenAsync(
                john,
                UserManager.Options.Tokens.PasswordResetTokenProvider,
                UserManager<IdentityUser>.ResetPasswordTokenPurpose,
                token);

            isValid.ShouldBeTrue();
            await uow.CompleteAsync();
        }
    }
}
