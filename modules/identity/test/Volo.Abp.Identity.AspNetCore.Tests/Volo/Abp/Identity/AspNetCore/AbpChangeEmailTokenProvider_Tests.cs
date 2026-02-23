using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Shouldly;
using Volo.Abp.Uow;
using Xunit;

namespace Volo.Abp.Identity.AspNetCore;

public class AbpChangeEmailTokenProvider_Tests : AbpIdentityAspNetCoreTestBase
{
    private const string NewEmail = "newemail@example.com";

    protected IIdentityUserRepository UserRepository { get; }
    protected IdentityUserManager UserManager { get; }
    protected IdentityTestData TestData { get; }
    protected IUnitOfWorkManager UnitOfWorkManager { get; }

    public AbpChangeEmailTokenProvider_Tests()
    {
        UserRepository = GetRequiredService<IIdentityUserRepository>();
        UserManager = GetRequiredService<IdentityUserManager>();
        TestData = GetRequiredService<IdentityTestData>();
        UnitOfWorkManager = GetRequiredService<IUnitOfWorkManager>();
    }

    [Fact]
    public void AbpChangeEmailTokenProvider_Should_Be_Registered()
    {
        var identityOptions = GetRequiredService<IOptions<IdentityOptions>>().Value;

        identityOptions.Tokens.ProviderMap.ShouldContainKey(AbpChangeEmailTokenProvider.ProviderName);
        identityOptions.Tokens.ProviderMap[AbpChangeEmailTokenProvider.ProviderName].ProviderType
            .ShouldBe(typeof(AbpChangeEmailTokenProvider));
    }

    [Fact]
    public void ChangeEmailTokenProvider_Should_Be_Configured_As_Abp()
    {
        var identityOptions = GetRequiredService<IOptions<IdentityOptions>>().Value;

        identityOptions.Tokens.ChangeEmailTokenProvider.ShouldBe(AbpChangeEmailTokenProvider.ProviderName);
    }

    [Fact]
    public async Task Generate_And_Verify_Change_Email_Token_Should_Succeed()
    {
        using (var uow = UnitOfWorkManager.Begin())
        {
            var john = await UserRepository.GetAsync(TestData.UserJohnId);

            var token = await UserManager.GenerateChangeEmailTokenAsync(john, NewEmail);
            token.ShouldNotBeNullOrEmpty();

            var isValid = await UserManager.VerifyUserTokenAsync(
                john,
                UserManager.Options.Tokens.ChangeEmailTokenProvider,
                UserManager<IdentityUser>.GetChangeEmailTokenPurpose(NewEmail),
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

            await UserManager.GenerateChangeEmailTokenAsync(john, NewEmail);

            var isValid = await UserManager.VerifyUserTokenAsync(
                john,
                UserManager.Options.Tokens.ChangeEmailTokenProvider,
                UserManager<IdentityUser>.GetChangeEmailTokenPurpose(NewEmail),
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

            var firstToken = await UserManager.GenerateChangeEmailTokenAsync(john, NewEmail);

            john = await UserRepository.GetAsync(TestData.UserJohnId);
            var secondToken = await UserManager.GenerateChangeEmailTokenAsync(john, NewEmail);

            john = await UserRepository.GetAsync(TestData.UserJohnId);

            var firstTokenValid = await UserManager.VerifyUserTokenAsync(
                john,
                UserManager.Options.Tokens.ChangeEmailTokenProvider,
                UserManager<IdentityUser>.GetChangeEmailTokenPurpose(NewEmail),
                firstToken);

            var secondTokenValid = await UserManager.VerifyUserTokenAsync(
                john,
                UserManager.Options.Tokens.ChangeEmailTokenProvider,
                UserManager<IdentityUser>.GetChangeEmailTokenPurpose(NewEmail),
                secondToken);

            firstTokenValid.ShouldBeFalse();
            secondTokenValid.ShouldBeTrue();

            await uow.CompleteAsync();
        }
    }

    [Fact]
    public async Task Token_Should_Become_Invalid_After_Email_Change()
    {
        using (var uow = UnitOfWorkManager.Begin())
        {
            var john = await UserRepository.GetAsync(TestData.UserJohnId);

            var token = await UserManager.GenerateChangeEmailTokenAsync(john, NewEmail);

            john = await UserRepository.GetAsync(TestData.UserJohnId);
            var result = await UserManager.ChangeEmailAsync(john, NewEmail, token);
            result.Succeeded.ShouldBeTrue();

            // SecurityStamp has changed after the email change, so the old token must be invalid.
            john = await UserRepository.GetAsync(TestData.UserJohnId);
            var isValid = await UserManager.VerifyUserTokenAsync(
                john,
                UserManager.Options.Tokens.ChangeEmailTokenProvider,
                UserManager<IdentityUser>.GetChangeEmailTokenPurpose(NewEmail),
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
            token = await UserManager.GenerateChangeEmailTokenAsync(john, NewEmail);
            await uow.CompleteAsync();
        }

        // UoW 2: validate using a fresh DbContext to confirm the hash was written to the DB.
        using (var uow = UnitOfWorkManager.Begin(requiresNew: true))
        {
            var john = await UserRepository.GetAsync(TestData.UserJohnId);
            var isValid = await UserManager.VerifyUserTokenAsync(
                john,
                UserManager.Options.Tokens.ChangeEmailTokenProvider,
                UserManager<IdentityUser>.GetChangeEmailTokenPurpose(NewEmail),
                token);

            isValid.ShouldBeTrue();
            await uow.CompleteAsync();
        }
    }
}
