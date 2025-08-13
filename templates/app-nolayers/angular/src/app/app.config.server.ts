import { mergeApplicationConfig, ApplicationConfig } from '@angular/core';
import {provideServerRendering, withRoutes} from '@angular/ssr';
import { appConfig } from './app.config';
import {provideAbpOAuth} from "@abp/ng.oauth";
import {serverRoutes} from "./app.routes.server";

const serverConfig: ApplicationConfig = {
  providers: [
    provideAbpOAuth(),
    provideServerRendering(withRoutes(serverRoutes))
  ]
};

export const config = mergeApplicationConfig(appConfig, serverConfig);
