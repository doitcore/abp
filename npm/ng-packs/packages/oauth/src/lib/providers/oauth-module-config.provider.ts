import {
  AuthService,
  AuthGuard,
  authGuard,
  ApiInterceptor,
  PIPE_TO_LOGIN_FN_KEY,
  CHECK_AUTHENTICATION_STATE_FN_KEY,
  AuthErrorFilterService,
} from '@abp/ng.core';
import { Provider, makeEnvironmentProviders, inject, provideAppInitializer } from '@angular/core';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { OAuthModule, OAuthStorage } from 'angular-oauth2-oidc';
import { AbpOAuthGuard, abpOAuthGuard } from '../guards';
import { OAuthConfigurationHandler } from '../handlers';
import { OAuthApiInterceptor } from '../interceptors';
import { AbpOAuthService, BrowserTokenStorageService, OAuthErrorFilterService } from '../services';
import { pipeToLogin, checkAccessToken, oAuthStorageFactory } from '../utils';
import { NavigateToManageProfileProvider } from './navigate-to-manage-profile.provider';
import { ServerTokenStorageService } from '../services/server-token-storage.service';

export function provideAbpOAuth({ ssr = false }: { ssr?: boolean }) {
  const providers = [
    {
      provide: AuthService,
      useClass: AbpOAuthService,
    },
    {
      provide: AuthGuard,
      useClass: AbpOAuthGuard,
    },
    {
      provide: authGuard,
      useValue: abpOAuthGuard,
    },
    {
      provide: ApiInterceptor,
      useClass: OAuthApiInterceptor,
    },
    {
      provide: PIPE_TO_LOGIN_FN_KEY,
      useValue: pipeToLogin,
    },
    {
      provide: CHECK_AUTHENTICATION_STATE_FN_KEY,
      useValue: checkAccessToken,
    },
    {
      provide: HTTP_INTERCEPTORS,
      useExisting: ApiInterceptor,
      multi: true,
    },
    NavigateToManageProfileProvider,
    provideAppInitializer(() => {
      inject(OAuthConfigurationHandler);
    }),
    OAuthModule.forRoot().providers as Provider[],
    {
      provide: OAuthStorage,
      useFactory: oAuthStorageFactory,
    },
    { provide: AuthErrorFilterService, useExisting: OAuthErrorFilterService },
  ];
  console.log('ssr --->>>>', ssr);
  if (ssr) {
    providers.push(ServerTokenStorageService);
  } else {
    providers.push(BrowserTokenStorageService);
  }

  return makeEnvironmentProviders(providers);
}
