using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;
using Volo.Abp.Data;
using Volo.Docs.Projects;
using Volo.Docs.Projects.Pdf;

namespace Volo.Docs.Common.Documents;

[Authorize(DocsCommonPermissions.Projects.PdfGeneration)]
public class DocumentPdfGeneratorAppService : ApplicationService, IDocumentPdfGeneratorAppService
{
    protected IProjectPdfGenerator ProjectPdfGenerator { get; }
    protected IProjectRepository ProjectRepository { get; }
    
    public DocumentPdfGeneratorAppService(
        IProjectPdfGenerator projectPdfGenerator,
        IProjectRepository projectRepository)
    {
        ProjectPdfGenerator = projectPdfGenerator;
        ProjectRepository = projectRepository;
    }

    public virtual async Task<IRemoteStreamContent> GeneratePdfAsync(DocumentPdfGeneratorInput input)
    {
        var project = await ProjectRepository.GetAsync(input.ProjectId, includeDetails: true);
        
        // https://github.com/abpframework/abp/blob/e96f601641ab8a4bb7d704d3b9df2c00517d96f6/modules/docs/src/Volo.Docs.Application/Volo/Docs/Documents/DocumentAppService.cs#L73
        var inputVersionStringBuilder = new StringBuilder();
        input.Version = inputVersionStringBuilder.Append(GetProjectVersionPrefixIfExist(project)).Append(input.Version).ToString();
        return await ProjectPdfGenerator.GenerateAsync(project, input.Version, input.LanguageCode);
    }

    private string GetProjectVersionPrefixIfExist(Project project)
    {
        if (GetGithubVersionProviderSource(project) != GithubVersionProviderSource.Branches)
        {
            return string.Empty;
        }

        return project.GetProperty<string>("VersionBranchPrefix");
    }
    
    private GithubVersionProviderSource GetGithubVersionProviderSource(Project project)
    {
        return project.HasProperty("GithubVersionProviderSource")
            ? project.GetProperty<GithubVersionProviderSource>("GithubVersionProviderSource")
            : GithubVersionProviderSource.Releases;
    }
}