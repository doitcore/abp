import { eLayoutType, RoutesService } from '@abp/ng.core';
import { eThemeSharedRouteNames } from '@abp/ng.theme.shared';
import { inject, provideAppInitializer } from '@angular/core';
import { eCmsKitAdminPolicyNames } from '../enums/policy-names';
import { eCmsKitAdminRouteNames } from '../enums/route-names';

export const CMS_KIT_ADMIN_ROUTE_PROVIDERS = [
  provideAppInitializer(() => {
    configureRoutes();
  }),
];

export function configureRoutes() {
  const routesService = inject(RoutesService);
  routesService.add([
    {
      path: '/cms-kit/comments',
      name: eCmsKitAdminRouteNames.Comments,
      parentName: eThemeSharedRouteNames.Administration,
      order: 100,
      layout: eLayoutType.application,
      iconClass: 'fa fa-comments',
      requiredPolicy: eCmsKitAdminPolicyNames.Comments,
    },
    {
      path: '/cms-kit/tags',
      name: eCmsKitAdminRouteNames.Tags,
      parentName: eThemeSharedRouteNames.Administration,
      order: 101,
      layout: eLayoutType.application,
      iconClass: 'fa fa-tags',
      requiredPolicy: eCmsKitAdminPolicyNames.Tags,
    },
    {
      path: '/cms-kit/pages',
      name: eCmsKitAdminRouteNames.Pages,
      parentName: eThemeSharedRouteNames.Administration,
      order: 102,
      layout: eLayoutType.application,
      iconClass: 'fa fa-file',
      requiredPolicy: eCmsKitAdminPolicyNames.Pages,
    },
    {
      path: '/cms-kit/blogs',
      name: eCmsKitAdminRouteNames.Blogs,
      parentName: eThemeSharedRouteNames.Administration,
      order: 103,
      layout: eLayoutType.application,
      iconClass: 'fa fa-blog',
      requiredPolicy: eCmsKitAdminPolicyNames.Blogs,
    },
    {
      path: '/cms-kit/blog-posts',
      name: eCmsKitAdminRouteNames.BlogPosts,
      parentName: eThemeSharedRouteNames.Administration,
      order: 104,
      layout: eLayoutType.application,
      iconClass: 'fa fa-file-alt',
      requiredPolicy: eCmsKitAdminPolicyNames.BlogPosts,
    },
    {
      path: '/cms-kit/menus',
      name: eCmsKitAdminRouteNames.Menus,
      parentName: eThemeSharedRouteNames.Administration,
      order: 105,
      layout: eLayoutType.application,
      iconClass: 'fa fa-bars',
      requiredPolicy: eCmsKitAdminPolicyNames.Menus,
    },
    {
      path: '/cms-kit/global-resources',
      name: eCmsKitAdminRouteNames.GlobalResources,
      parentName: eThemeSharedRouteNames.Administration,
      order: 106,
      layout: eLayoutType.application,
      iconClass: 'fa fa-globe',
      requiredPolicy: eCmsKitAdminPolicyNames.GlobalResources,
    },
  ]);
}
