import { makeEnvironmentProviders } from '@angular/core';
import { CMS_KIT_ADMIN_ROUTE_PROVIDERS } from './route.provider';

export function provideCmsKitAdminConfig() {
  return makeEnvironmentProviders([CMS_KIT_ADMIN_ROUTE_PROVIDERS]);
}
