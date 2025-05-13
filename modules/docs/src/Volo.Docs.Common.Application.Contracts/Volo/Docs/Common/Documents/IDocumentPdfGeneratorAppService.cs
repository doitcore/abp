using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace Volo.Docs.Common.Documents;

public interface IDocumentPdfGeneratorAppService : IApplicationService
{
    Task<IRemoteStreamContent> GeneratePdfAsync(DocumentPdfGeneratorInput input);
}