import { Inject, Injectable } from '@angular/core';
import { OAuthStorage } from 'angular-oauth2-oidc';

@Injectable({
  providedIn: null,
})
export class ServerTokenStorageService implements OAuthStorage {
  private cookies: Map<string, string> = new Map();
  constructor(@Inject('cookies') c: any) {
    const cookieItems = c.split(';');
    for (const item of cookieItems) {
      const [key, value] = item.split('=');
      if (key && value) {
        this.cookies.set(key.trim(), value.trim());
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
