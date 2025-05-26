import { Inject, Injectable } from '@angular/core';
import { OAuthStorage } from 'angular-oauth2-oidc';

@Injectable({
  providedIn: 'root',
})
export class ServerTokenStorageService implements OAuthStorage {
  private cookies: Map<string, string>;
  constructor(@Inject('cookies') c: any) {
    const cookies = JSON.parse(c);
    this.cookies = new Map<string, string>();

    for (const cookie of cookies) {
      this.cookies.set(cookie.key, cookie.value);
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
