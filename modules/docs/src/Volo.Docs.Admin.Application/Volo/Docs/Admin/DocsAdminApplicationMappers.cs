using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;
using Volo.Docs.Admin.Documents;
using Volo.Docs.Admin.Projects;
using Volo.Docs.Documents;
using Volo.Docs.Projects;

namespace Volo.Docs.Admin;

[Mapper]
public partial class ProjectPdfFileToProjectPdfFileDtoMapper : MapperBase<ProjectPdfFile, ProjectPdfFileDto>
{
    public override partial ProjectPdfFileDto Map(ProjectPdfFile source);

    public override partial void Map(ProjectPdfFile source, ProjectPdfFileDto destination);
}

[Mapper]
public partial class DocumentInfoToDocumentInfoDtoMapper : MapperBase<DocumentInfo, DocumentInfoDto>
{
    public override partial DocumentInfoDto Map(DocumentInfo source);

    public override partial void Map(DocumentInfo source, DocumentInfoDto destination);
}

[Mapper]
public partial class ProjectWithoutDetailsToProjectWithoutDetailsDtoMapper : MapperBase<ProjectWithoutDetails, ProjectWithoutDetailsDto>
{
    public override partial ProjectWithoutDetailsDto Map(ProjectWithoutDetails source);

    public override partial void Map(ProjectWithoutDetails source, ProjectWithoutDetailsDto destination);
}

[Mapper]
public partial class DocumentWithoutContentToDocumentDtoMapper : MapperBase<DocumentWithoutContent, DocumentDto>
{
    public override partial DocumentDto Map(DocumentWithoutContent source);

    public override partial void Map(DocumentWithoutContent source, DocumentDto destination);
}

[Mapper]
public partial class DocumentToDocumentDtoMapper : MapperBase<Document, DocumentDto>
{
    [MapperIgnoreTarget(nameof(DocumentDto.ProjectName))]
    public override partial DocumentDto Map(Document source);

    [MapperIgnoreTarget(nameof(DocumentDto.ProjectName))]
    public override partial void Map(Document source, DocumentDto destination);
}

[Mapper]
public partial class ProjectToProjectDtoMapper : MapperBase<Project, ProjectDto>
{
    public override partial ProjectDto Map(Project source);

    public override partial void Map(Project source, ProjectDto destination);
}