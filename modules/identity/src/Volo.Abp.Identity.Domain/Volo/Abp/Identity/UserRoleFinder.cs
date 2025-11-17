using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace Volo.Abp.Identity;

public class UserRoleFinder : IUserRoleFinder, ITransientDependency
{
    protected IIdentityUserRepository IdentityUserRepository { get; }
    protected IIdentityRoleRepository IdentityRoleRepository  { get; }

    public UserRoleFinder(IIdentityUserRepository identityUserRepository, IIdentityRoleRepository identityRoleRepository)
    {
        IdentityUserRepository = identityUserRepository;
        IdentityRoleRepository = identityRoleRepository;
    }

    [Obsolete("Use GetRoleNamesAsync instead.")]
    public virtual async Task<string[]> GetRolesAsync(Guid userId)
    {
        return (await IdentityUserRepository.GetRoleNamesAsync(userId)).ToArray();
    }

    public virtual async Task<string[]> GetRoleNamesAsync(Guid userId)
    {
        return (await IdentityUserRepository.GetRoleNamesAsync(userId)).ToArray();
    }

    public virtual async Task<List<UserFinderResult>> SearchUserAsync(string filter)
    {
        using (IdentityUserRepository.DisableTracking())
        {
            var users = await IdentityUserRepository.GetListAsync(filter: filter);
            return users.Select(user => new UserFinderResult
            {
                Id = user.Id,
                UserName = user.UserName
            }).ToList();
        }
    }

    public virtual async Task<List<RoleFinderResult>> SearchRoleAsync(string filter)
    {
        using (IdentityUserRepository.DisableTracking())
        {
            var roles = await IdentityRoleRepository.GetListAsync(filter: filter);
            return roles.Select(user => new RoleFinderResult
            {
                Id = user.Id,
                RoleName = user.Name
            }).ToList();
        }
    }
}
