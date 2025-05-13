using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;
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
        return await ProjectPdfGenerator.GenerateAsync(project, input.Version, input.LanguageCode);
    }
}