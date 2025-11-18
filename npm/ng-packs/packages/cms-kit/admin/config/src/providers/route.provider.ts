import { eLayoutType, RoutesService } from '@abp/ng.core';
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
      path: '/cms/comments',
      name: eCmsKitAdminRouteNames.Comments,
      parentName: eCmsKitAdminRouteNames.Cms,
      order: 100,
      layout: eLayoutType.application,
      iconClass: 'fa fa-comments',
      requiredPolicy: eCmsKitAdminPolicyNames.Comments,
    },
    {
      path: '/cms/tags',
      name: eCmsKitAdminRouteNames.Tags,
      parentName: eCmsKitAdminRouteNames.Cms,
      order: 101,
      layout: eLayoutType.application,
      iconClass: 'fa fa-tags',
      requiredPolicy: eCmsKitAdminPolicyNames.Tags,
    },
    {
      path: '/cms/pages',
      name: eCmsKitAdminRouteNames.Pages,
      parentName: eCmsKitAdminRouteNames.Cms,
      order: 102,
      layout: eLayoutType.application,
      iconClass: 'fa fa-file',
      requiredPolicy: eCmsKitAdminPolicyNames.Pages,
    },
    {
      path: '/cms/blogs',
      name: eCmsKitAdminRouteNames.Blogs,
      parentName: eCmsKitAdminRouteNames.Cms,
      order: 103,
      layout: eLayoutType.application,
      iconClass: 'fa fa-blog',
      requiredPolicy: eCmsKitAdminPolicyNames.Blogs,
    },
    {
      path: '/cms/blog-posts',
      name: eCmsKitAdminRouteNames.BlogPosts,
      parentName: eCmsKitAdminRouteNames.Cms,
      order: 104,
      layout: eLayoutType.application,
      iconClass: 'fa fa-file-alt',
      requiredPolicy: eCmsKitAdminPolicyNames.BlogPosts,
    },
    {
      path: '/cms/menus',
      name: eCmsKitAdminRouteNames.Menus,
      parentName: eCmsKitAdminRouteNames.Cms,
      order: 105,
      layout: eLayoutType.application,
      iconClass: 'fa fa-bars',
      requiredPolicy: eCmsKitAdminPolicyNames.Menus,
    },
    {
      path: '/cms/global-resources',
      name: eCmsKitAdminRouteNames.GlobalResources,
      parentName: eCmsKitAdminRouteNames.Cms,
      order: 106,
      layout: eLayoutType.application,
      iconClass: 'fa fa-globe',
      requiredPolicy: eCmsKitAdminPolicyNames.GlobalResources,
    },
  ]);
}
