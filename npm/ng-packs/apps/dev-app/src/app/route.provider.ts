import { eLayoutType, RoutesService } from '@abp/ng.core';
import { inject, provideAppInitializer } from '@angular/core';

export const APP_ROUTE_PROVIDER = [
  provideAppInitializer(() => {
    configureRoutes();
  }),
];

function configureRoutes() {
  const routesService = inject(RoutesService);
  routesService.add([
    {
      path: '/',
      name: '::Menu:Home',
      iconClass: 'fas fa-home',
      order: 1,
      layout: eLayoutType.application,
    },
    {
      path: '/identity/users',
      name: '::Users:Server',
      iconClass: 'fas fa-home',
      order: 2,
      layout: eLayoutType.application,
      requiredPolicy: 'AbpIdentity.Users'
    },
    {
      path: '/identity/roles',
      name: '::Roles:Server',
      iconClass: 'fas fa-home',
      order: 3,
      layout: eLayoutType.application,
      requiredPolicy: 'AbpIdentity.Roles'
    },
  ]);
}
