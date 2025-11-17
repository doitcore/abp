using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity.Integration;

namespace Volo.Abp.Identity;

[Dependency(TryRegister = true)]
public class HttpClientUserRoleFinder : IUserRoleFinder, ITransientDependency
{
    protected IIdentityUserAppService _userAppService { get; }
    protected IIdentityUserIntegrationService _userIntegrationService { get; }

    public HttpClientUserRoleFinder(IIdentityUserAppService userAppService, IIdentityUserIntegrationService userIntegrationService)
    {
        _userAppService = userAppService;
        _userIntegrationService = userIntegrationService;
    }

    [Obsolete("Use GetRoleNamesAsync instead.")]
    public virtual async Task<string[]> GetRolesAsync(Guid userId)
    {
        var output = await _userAppService.GetRolesAsync(userId);
        return output.Items.Select(r => r.Name).ToArray();
    }

    public virtual async Task<string[]> GetRoleNamesAsync(Guid userId)
    {
        return await _userIntegrationService.GetRoleNamesAsync(userId);
    }

    public virtual async Task<List<UserFinderResult>> SearchUserAsync(string filter)
    {
        var users = await _userIntegrationService.SearchAsync(new UserLookupSearchInputDto()
        {
            Filter = filter
        });
        return users.Items.Select(u => new UserFinderResult
        {
            Id = u.Id,
            UserName = u.UserName
        }).ToList();
    }

    public virtual async Task<List<RoleFinderResult>> SearchRoleAsync(string filter)
    {
        var roles = await _userIntegrationService.SearchRoleAsync(new RoleLookupSearchInputDto()
        {
            Filter = filter
        });

        return roles.Items.Select(r => new RoleFinderResult
        {
            Id = r.Id,
            RoleName = r.Name
        }).ToList();
    }
}
