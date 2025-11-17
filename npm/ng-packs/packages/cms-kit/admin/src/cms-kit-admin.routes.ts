import { Routes } from '@angular/router';
import { Provider } from '@angular/core';
import {
  RouterOutletComponent,
  authGuard,
  permissionGuard,
  ReplaceableRouteContainerComponent,
} from '@abp/ng.core';
import { eCmsKitAdminComponents } from './enums';

export interface CmsKitAdminConfigOptions {
  // Extension point contributors
}

export function createRoutes(config: CmsKitAdminConfigOptions = {}): Routes {
  return [
    {
      path: '',
      component: RouterOutletComponent,
      providers: provideCmsKitAdminContributors(config),
      canActivate: [authGuard, permissionGuard],
      children: [
        // Routes will be added here
      ],
    },
  ];
}

function provideCmsKitAdminContributors(options: CmsKitAdminConfigOptions = {}): Provider[] {
  return [
    // Contributors will be added here
  ];
}
