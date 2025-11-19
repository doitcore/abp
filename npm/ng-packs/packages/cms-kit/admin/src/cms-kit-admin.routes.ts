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
  TagListComponent,
  PageListComponent,
  PageFormComponent,
} from './components';
import {
  CMS_KIT_ADMIN_ENTITY_ACTION_CONTRIBUTORS,
  CMS_KIT_ADMIN_ENTITY_PROP_CONTRIBUTORS,
  CMS_KIT_ADMIN_TOOLBAR_ACTION_CONTRIBUTORS,
  CMS_KIT_ADMIN_CREATE_FORM_PROP_CONTRIBUTORS,
  CMS_KIT_ADMIN_EDIT_FORM_PROP_CONTRIBUTORS,
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
        {
          path: 'tags',
          component: ReplaceableRouteContainerComponent,
          resolve: {
            extensions: cmsKitAdminExtensionsResolver,
          },
          data: {
            requiredPolicy: 'CmsKit.Tags',
            replaceableComponent: {
              key: eCmsKitAdminComponents.TagList,
              defaultComponent: TagListComponent,
            },
          },
          title: 'CmsKit::Tags',
        },
        {
          path: 'pages',
          component: ReplaceableRouteContainerComponent,
          resolve: {
            extensions: cmsKitAdminExtensionsResolver,
          },
          data: {
            requiredPolicy: 'CmsKit.Pages',
            replaceableComponent: {
              key: eCmsKitAdminComponents.PageList,
              defaultComponent: PageListComponent,
            },
          },
          title: 'CmsKit::Pages',
        },
        {
          path: 'pages/create',
          component: ReplaceableRouteContainerComponent,
          resolve: {
            extensions: cmsKitAdminExtensionsResolver,
          },
          data: {
            requiredPolicy: 'CmsKit.Pages.Create',
            replaceableComponent: {
              key: eCmsKitAdminComponents.PageCreate,
              defaultComponent: PageFormComponent,
            },
          },
          title: 'CmsKit::Pages',
        },
        {
          path: 'pages/update/:id',
          component: ReplaceableRouteContainerComponent,
          resolve: {
            extensions: cmsKitAdminExtensionsResolver,
          },
          data: {
            requiredPolicy: 'CmsKit.Pages.Update',
            replaceableComponent: {
              key: eCmsKitAdminComponents.PageEdit,
              defaultComponent: PageFormComponent,
            },
          },
          title: 'CmsKit::Pages',
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
    {
      provide: CMS_KIT_ADMIN_TOOLBAR_ACTION_CONTRIBUTORS,
      useValue: options.toolbarActionContributors,
    },
    {
      provide: CMS_KIT_ADMIN_CREATE_FORM_PROP_CONTRIBUTORS,
      useValue: options.createFormPropContributors,
    },
    {
      provide: CMS_KIT_ADMIN_EDIT_FORM_PROP_CONTRIBUTORS,
      useValue: options.editFormPropContributors,
    },
  ];
}
