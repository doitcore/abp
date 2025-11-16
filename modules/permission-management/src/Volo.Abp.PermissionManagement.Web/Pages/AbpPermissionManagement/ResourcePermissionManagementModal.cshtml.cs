using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace Volo.Abp.PermissionManagement.Web.Pages.AbpPermissionManagement;

public class ResourcePermissionManagementModal : AbpPageModel
{
    [Required]
    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public string ResourceName { get; set; }

    [Required]
    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public string ResourceKey { get; set; }

    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public string ResourceDisplayName { get; set; }

    public GetResourcePermissionListResultDto ResourcePermissions { get; set; }

    protected IPermissionAppService PermissionAppService { get; }

    public ResourcePermissionManagementModal(IPermissionAppService permissionAppService)
    {
        ObjectMapperContext = typeof(AbpPermissionManagementWebModule);

        PermissionAppService = permissionAppService;
    }

    public virtual Task<IActionResult> OnGetAsync()
    {
        ValidateModel();
        return Task.FromResult<IActionResult>(Page());
    }

    public virtual async Task<IActionResult> OnPostAsync()
    {
        ValidateModel();


        return NoContent();
    }

    public class ResourcePermissionViewModel
    {
        public string ProviderName { get; set; }

        public string ProviderKey { get; set; }

        public List<string> Permissions { get; set; }
    }
}
