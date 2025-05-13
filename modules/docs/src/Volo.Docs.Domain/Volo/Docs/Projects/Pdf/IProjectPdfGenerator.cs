using System.Threading.Tasks;
using Volo.Abp.Content;

namespace Volo.Docs.Projects.Pdf;

public interface IProjectPdfGenerator
{
    Task<IRemoteStreamContent> GenerateAsync(Project project, string version, string languageCode);
}