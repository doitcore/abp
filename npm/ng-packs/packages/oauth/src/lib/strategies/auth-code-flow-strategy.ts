import { Inject, PLATFORM_ID } from '@angular/core';
import { noop } from '@abp/ng.core';
import { Params } from '@angular/router';
import { filter, from, of, take, tap } from 'rxjs';
import { AuthFlowStrategy } from './auth-flow-strategy';
import { isTokenExpired } from '../utils';
import { isPlatformBrowser } from '@angular/common';

export class AuthCodeFlowStrategy extends AuthFlowStrategy {
  readonly isInternalAuth = false;
  @Inject(PLATFORM_ID) platformId: string;

  async init() {
    this.checkRememberMeOption();
    this.listenToTokenReceived();

    return super
      .init()
      .then(() => this.oAuthService.tryLogin().catch(noop))
      .then(() => this.oAuthService.setupAutomaticSilentRefresh());
  }

  private checkRememberMeOption() {
    const accessToken = this.oAuthService.getAccessToken();
    const isTokenExpire = isTokenExpired(this.oAuthService.getAccessTokenExpiration());
    let rememberMe = this.rememberMeService.get();

    if (accessToken && !rememberMe) {
      const rememberMeValue = this.rememberMeService.getFromToken(accessToken);

      this.rememberMeService.set(!!rememberMeValue);
    }

    rememberMe = this.rememberMeService.get();
    if (accessToken && isTokenExpire && !rememberMe) {
      this.rememberMeService.remove();
      this.oAuthService.logOut();
    }
  }

  private getCultureParams(queryParams?: Params) {
    const lang = this.sessionState.getLanguage();
    const culture = { culture: lang, 'ui-culture': lang };
    return { ...(lang && culture), ...queryParams };
  }

  protected setUICulture() {
    if (isPlatformBrowser(this.platformId)) {
      const urlParams = new URLSearchParams(window.location.search);
      this.configState.uiCultureFromAuthCodeFlow = urlParams.get('ui-culture');
    }
  }

  protected replaceURLParams() {
    if (isPlatformBrowser(this.platformId)) {
      const location = this.windowService.window.location;
      const history = this.windowService.window.history;

      const query = location.search
        .replace(/([?&])iss=[^&]*&?/, '$1')
        .replace(/([?&])culture=[^&]*&?/, '$1')
        .replace(/([?&])ui-culture=[^&]*&?/, '$1')
        .replace(/[?&]+$/, '');

      const href = location.origin + location.pathname + query + location.hash;

      history.replaceState(null, '', href);
    }
  }

  protected listenToTokenReceived() {
    if (isPlatformBrowser(this.platformId)) {
      this.oAuthService.events
        .pipe(
          filter(event => event.type === 'token_received'),
          tap(() => {
            this.setUICulture();
            this.replaceURLParams();
          }),
          take(1),
        )
        .subscribe();
    }
  }

  navigateToLogin(queryParams?: Params) {
    if (isPlatformBrowser(this.platformId)) {
      let additionalState = '';
      if (queryParams?.returnUrl) {
        additionalState = queryParams.returnUrl;
      }

      const cultureParams = this.getCultureParams(queryParams);
      this.oAuthService.initCodeFlow(additionalState, cultureParams);
    }
  }

  checkIfInternalAuth(queryParams?: Params) {
    if (isPlatformBrowser(this.platformId)) {
      this.oAuthService.initCodeFlow('', this.getCultureParams(queryParams));
      return false;
    }
  }

  logout(queryParams?: Params) {
    this.rememberMeService.remove();
    if (queryParams?.noRedirectToLogoutUrl) {
      this.router.navigate(['/']);
      return from(this.oAuthService.revokeTokenAndLogout(true));
    }
    return from(this.oAuthService.revokeTokenAndLogout(this.getCultureParams(queryParams)));
  }

  login(queryParams?: Params) {
    if (isPlatformBrowser(this.platformId)) {
      this.oAuthService.initCodeFlow('', this.getCultureParams(queryParams));
      return of(null);
    }
  }
}
