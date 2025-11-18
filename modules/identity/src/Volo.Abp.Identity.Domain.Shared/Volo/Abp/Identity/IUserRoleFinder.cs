using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Volo.Abp.Identity;

public interface IUserRoleFinder
{
    [Obsolete("Use GetRoleNamesAsync instead.")]
    Task<string[]> GetRolesAsync(Guid userId);

    Task<string[]> GetRoleNamesAsync(Guid userId);

    Task<List<UserFinderResult>> SearchUserAsync(string filter);

    Task<List<RoleFinderResult>> SearchRoleAsync(string filter);

    Task<List<UserFinderResult>> SearchUserByIdsAsync(Guid[] ids);

    Task<List<RoleFinderResult>> SearchRoleByIdsAsync(Guid[] ids);
}
