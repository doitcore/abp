using System.Collections.Generic;
using System.Security.Claims;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Security.Claims;

namespace Volo.Abp.Identity;

[Dependency(ReplaceServices = true)]
public class FakeCurrentPrincipalAccessor : ThreadCurrentPrincipalAccessor
{
    private readonly IdentityTestData _testData;

    public FakeCurrentPrincipalAccessor(IdentityTestData testData)
    {
        _testData = testData;
    }

    protected override ClaimsPrincipal GetClaimsPrincipal()
    {
        return GetPrincipal();
    }

    private ClaimsPrincipal _principal;

    private ClaimsPrincipal GetPrincipal()
    {
        if (_principal == null)
        {
            lock (this)
            {
                if (_principal == null)
                {
                    _principal = new ClaimsPrincipal(
                        new ClaimsIdentity(
                            new List<Claim>
                            {
                                new Claim(AbpClaimTypes.UserId, _testData.UserAdminId.ToString()),
                                new Claim(AbpClaimTypes.UserName, "administrator"),
                                new Claim(AbpClaimTypes.Email, "administrator@abp.io")
                            }
                        )
                    );
                }
            }
        }

        return _principal;
    }
}
