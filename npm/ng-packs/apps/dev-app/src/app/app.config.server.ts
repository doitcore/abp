import { mergeApplicationConfig, ApplicationConfig } from '@angular/core';
import { appConfig } from './app.config';
import { provideAbpOAuth } from '@abp/ng.oauth';
import { withRoutes, provideServerRendering } from '@angular/ssr';
import { appServerRoutes } from './app.routes.server';

const serverConfig: ApplicationConfig = {
  providers: [provideAbpOAuth({ ssr: true }), provideServerRendering(withRoutes(appServerRoutes))],
};

export const config = mergeApplicationConfig(appConfig, serverConfig);
