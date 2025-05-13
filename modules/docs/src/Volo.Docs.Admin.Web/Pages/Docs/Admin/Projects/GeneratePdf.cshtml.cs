using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Docs.Admin.Projects;
using Volo.Docs.Common.Documents;
using Volo.Docs.Common.Projects;
using Volo.Docs.Projects;

namespace Volo.Docs.Admin.Pages.Docs.Admin.Projects;

public class GeneratePdfModal : DocsAdminPageModel
{
    protected IDocumentPdfGeneratorAppService DocumentPdfGeneratorAppService { get; }
    protected IProjectAppService ProjectAppService { get; }
    protected IProjectAdminAppService ProjectAdminAppService { get; }

    public GeneratePdfViewModel ViewModel { get; set; }

    public GeneratePdfModal(
        IDocumentPdfGeneratorAppService documentPdfGeneratorAppService,
        IProjectAppService projectAppService, 
        IProjectAdminAppService projectAdminAppService)
    {
        DocumentPdfGeneratorAppService = documentPdfGeneratorAppService;
        ProjectAppService = projectAppService;
        ProjectAdminAppService = projectAdminAppService;
    }

    public virtual async Task<IActionResult> OnGetAsync(Guid id)
    {
        var project = await ProjectAdminAppService.GetAsync(id);
        var versions = await ProjectAppService.GetVersionsAsync(project.ShortName);
        if(versions.Items.Count == 0)
        {
            versions.Items =
            [
                new VersionInfoDto { Name = ProjectConsts.Latest, DisplayName = ProjectConsts.Latest }
            ];
        }
        var languages = await ProjectAppService.GetLanguageListAsync(project.ShortName, versions.Items.FirstOrDefault()?.Name);
        ViewModel = new GeneratePdfViewModel
        {
            ProjectId = id,
            ShortName = project.ShortName,
            Versions = versions.Items.Select(x => new SelectListItem(x.DisplayName, x.Name)).ToList(),
            Languages = languages.Languages.Select(x => new SelectListItem(x.DisplayName, x.Code)).ToList()
        };

        return Page();
    }

    public class GeneratePdfViewModel
    {
        public Guid ProjectId { get; set; }
        public string ShortName { get; set; }
        public List<SelectListItem>  Versions { get; set; }
        public List<SelectListItem>  Languages { get; set; }
    }
}