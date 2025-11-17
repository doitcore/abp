import { Routes } from '@angular/router';
import { Provider } from '@angular/core';
import {
  RouterOutletComponent,
  authGuard,
  permissionGuard,
  ReplaceableRouteContainerComponent,
} from '@abp/ng.core';
import { eCmsKitAdminComponents } from './enums';
import {
  CommentListComponent,
  CommentApproveComponent,
  CommentDetailsComponent,
} from './components';
import {
  CMS_KIT_ADMIN_ENTITY_ACTION_CONTRIBUTORS,
  CMS_KIT_ADMIN_ENTITY_PROP_CONTRIBUTORS,
} from './tokens';
import { cmsKitAdminExtensionsResolver } from './resolvers';
import { CmsKitAdminConfigOptions } from './models';

export function createRoutes(config: CmsKitAdminConfigOptions = {}): Routes {
  return [
    {
      path: '',
      component: RouterOutletComponent,
      providers: provideCmsKitAdminContributors(config),
      canActivate: [authGuard, permissionGuard],
      children: [
        {
          path: 'comments',
          component: ReplaceableRouteContainerComponent,
          resolve: {
            extensions: cmsKitAdminExtensionsResolver,
          },
          data: {
            requiredPolicy: 'CmsKit.Comments',
            replaceableComponent: {
              key: eCmsKitAdminComponents.CommentList,
              defaultComponent: CommentListComponent,
            },
          },
          title: 'CmsKit::Comments',
        },
        {
          path: 'comments/approve',
          component: ReplaceableRouteContainerComponent,
          resolve: {
            extensions: cmsKitAdminExtensionsResolver,
          },
          data: {
            requiredPolicy: 'CmsKit.Comments',
            replaceableComponent: {
              key: eCmsKitAdminComponents.CommentApprove,
              defaultComponent: CommentApproveComponent,
            },
          },
          title: 'CmsKit::Comments',
        },
        {
          path: 'comments/:id',
          component: ReplaceableRouteContainerComponent,
          resolve: {
            extensions: cmsKitAdminExtensionsResolver,
          },
          data: {
            requiredPolicy: 'CmsKit.Comments',
            replaceableComponent: {
              key: eCmsKitAdminComponents.CommentDetails,
              defaultComponent: CommentDetailsComponent,
            },
          },
          title: 'CmsKit::Comments',
        },
      ],
    },
  ];
}

function provideCmsKitAdminContributors(options: CmsKitAdminConfigOptions = {}): Provider[] {
  return [
    {
      provide: CMS_KIT_ADMIN_ENTITY_ACTION_CONTRIBUTORS,
      useValue: options.entityActionContributors,
    },
    {
      provide: CMS_KIT_ADMIN_ENTITY_PROP_CONTRIBUTORS,
      useValue: options.entityPropContributors,
    },
  ];
}
