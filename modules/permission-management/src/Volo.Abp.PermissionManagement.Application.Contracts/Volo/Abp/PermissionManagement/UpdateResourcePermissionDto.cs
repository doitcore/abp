namespace Volo.Abp.PermissionManagement;

public class UpdateResourcePermissionDto
{
    public string ProviderName { get; set; }

    public string ProviderKey { get; set; }

    public string Name { get; set; }

    public bool IsGranted { get; set; }
}
