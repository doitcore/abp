import { Injectable, inject } from '@angular/core';
import {
  UrlTree,
  ActivatedRouteSnapshot,
  RouterStateSnapshot,
  CanActivateFn,
} from '@angular/router';

import { Observable, timer, filter, take, map, firstValueFrom, timeout, catchError, of } from 'rxjs';
import { OAuthService } from 'angular-oauth2-oidc';

import { AuthService, IAbpGuard, EnvironmentService } from '@abp/ng.core';

/**
 * @deprecated Use `abpOAuthGuard` *function* instead.
 */
@Injectable({
  providedIn: 'root',
})
export class AbpOAuthGuard implements IAbpGuard {
  protected readonly oAuthService = inject(OAuthService);
  protected readonly authService = inject(AuthService);

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot,
  ): Observable<boolean> | boolean | UrlTree {
    const hasValidAccessToken = this.oAuthService.hasValidAccessToken();
    if (hasValidAccessToken) {
      return true;
    }

    const params = { returnUrl: state.url };
    this.authService.navigateToLogin(params);
    return false;
  }
}

export const abpOAuthGuard: CanActivateFn = (
  route: ActivatedRouteSnapshot,
  state: RouterStateSnapshot,
) => {
  const oAuthService = inject(OAuthService);
  const authService = inject(AuthService);

  const hasValidAccessToken = oAuthService.hasValidAccessToken();

  if (hasValidAccessToken) {
    return true;
  }

  const params = { returnUrl: state.url };
  authService.navigateToLogin(params);
  return false;
};

export const asyncAbpOAuthGuard: CanActivateFn = (
  route: ActivatedRouteSnapshot,
  state: RouterStateSnapshot,
) => {
  const oAuthService = inject(OAuthService);
  const authService = inject(AuthService);
  const environmentService = inject(EnvironmentService);

  const { oAuthConfig } = environmentService.getEnvironment();

  if (oAuthConfig?.responseType === 'code') {
    return firstValueFrom(
      timer(0, 100).pipe(
        map(() => oAuthService.hasValidAccessToken()),
        filter(Boolean),
        take(1),
        timeout(3000),
        catchError(() => {
          authService.navigateToLogin({ returnUrl: state.url });
          return of(false);
        })
      )
    );
  }

  if (oAuthService.hasValidAccessToken()) {
    return true;
  }

  authService.navigateToLogin({ returnUrl: state.url });
  return false;
};
