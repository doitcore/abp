import { Inject, Injectable } from '@angular/core';
import { OAuthStorage } from 'angular-oauth2-oidc';

@Injectable({
  providedIn: null,
})
export class ServerTokenStorageService implements OAuthStorage {
  private cookies: Map<string, string> = new Map();
  constructor(@Inject('cookies') c: string | undefined) {
    if (c) {
      const cookieItems = c.split(';');
      for (const item of cookieItems) {
        const index = item.indexOf('=');
        if (index > -1) {
          const key = item.slice(0, index).trim();
          const value = item.slice(index + 1).trim();
          this.cookies.set(key, value);
        }
      }
    }
  }

  getItem(key: string): string {
    if (this.cookies) {
      return this.cookies.get(key);
    }
    return '';
  }

  removeItem(key: string): void {}

  setItem(key: string, data: string): void {}
}
