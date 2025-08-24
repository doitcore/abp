using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Volo.Abp.Caching;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;
using Volo.CmsKit.Features;
using Volo.CmsKit.Pages;
using Volo.CmsKit.Public.Pages;
using Volo.CmsKit.Public.Web.Pages.Public;

namespace Volo.CmsKit.Public.Web.Pages;

public class CmsKitPageRouteValueTransformer : CmsKitDynamicRouteValueTransformerBase
{
    protected IFeatureChecker FeatureChecker { get; }
    protected IPagePublicAppService PagePublicAppService { get; }
    protected IDistributedCache<PageCacheItem> PageCache { get; }

    public CmsKitPageRouteValueTransformer(
        ICurrentTenant currentTenant,
        ITenantConfigurationProvider tenantConfigurationProvider,
        IFeatureChecker featureChecker,
        IPagePublicAppService pagePublicAppService,
        IDistributedCache<PageCacheItem> pageCache)
        : base(currentTenant, tenantConfigurationProvider)
    {
        FeatureChecker = featureChecker;
        PagePublicAppService = pagePublicAppService;
        PageCache = pageCache;
    }

    protected async override ValueTask<RouteValueDictionary> DoTransformAsync(HttpContext httpContext, RouteValueDictionary values)
    {
        if (values.TryGetValue("slug", out var slugParameter) && slugParameter is not null)
        {
            if (!await FeatureChecker.IsEnabledAsync(CmsKitFeatures.PageEnable))
            {
                return values;
            }

            var slug = slugParameter.ToString().TrimStart('/');

            var exist = await PageCache.GetAsync(PageCacheItem.GetKey(slug)) != null;
            if (!exist)
            {
                exist = await PagePublicAppService.DoesSlugExistAsync(slug);
            }

            if (exist)
            {
                values["page"] = "/Public/CmsKit/Pages/Index";
            }
        }

        return values;
    }
}
