using System.Collections.Generic;

namespace Volo.Abp.PermissionManagement;

public class GetResourcePermissionListResultDto
{
    public string EntityDisplayName { get; set; }

    public List<ResourcePermissionGrantInfoDto> Permissions { get; set; }
}
