using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Application;
using Volo.Abp.Mapperly;
using Volo.Abp.Caching;
using Volo.Abp.Modularity;
using Volo.Blogging.Comments;
using Volo.Blogging.Posts;

namespace Volo.Blogging.Admin
{
    [DependsOn(
        typeof(BloggingDomainModule),
        typeof(BloggingAdminApplicationContractsModule),
        typeof(AbpCachingModule),
        typeof(AbpMapperlyModule),
        typeof(AbpDddApplicationModule)
        )]
    public class BloggingAdminApplicationModule : AbpModule
    {
    }
}
