import { RoutesService } from '@abp/ng.core';
import { inject, provideAppInitializer } from '@angular/core';
import { eCmsKitPublicPolicyNames } from '../enums/policy-names';
import { eCmsKitPublicRouteNames } from '../enums/route-names';

export const CMS_KIT_PUBLIC_ROUTE_PROVIDERS = [
  provideAppInitializer(() => {
    configureRoutes();
  }),
];

export function configureRoutes() {
  const routesService = inject(RoutesService);
  routesService.add([
    {
      path: '/cms-kit/pages/:slug',
      name: eCmsKitPublicRouteNames.Pages,
      requiredPolicy: eCmsKitPublicPolicyNames.Pages,
    },
    {
      path: '/cms-kit/blogs',
      name: eCmsKitPublicRouteNames.Blogs,
      requiredPolicy: eCmsKitPublicPolicyNames.Blogs,
    },
    {
      path: '/cms-kit/blogs/:blogSlug/:blogPostSlug',
      name: eCmsKitPublicRouteNames.BlogPosts,
      requiredPolicy: eCmsKitPublicPolicyNames.Blogs,
    },
  ]);
}
