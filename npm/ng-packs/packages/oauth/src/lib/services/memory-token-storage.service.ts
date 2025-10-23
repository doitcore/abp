import { Injectable } from '@angular/core';
import { OAuthStorage } from 'angular-oauth2-oidc';

@Injectable({
  providedIn: 'root',
})
export class MemoryTokenStorageService implements OAuthStorage {
  private keysShouldStoreLocalstorage = ['PKCE_verifier', 'abpOAuthClientId', 'abp_session', 'refresh_token', 'granted_scopes', 'id_token_claims_obj'];
  private data = new Map<string, string>();

  getItem(key: string): string {
    console.log('getItem -->>', key);
    if (key !== 'access_token') {
      return localStorage.getItem(key);
    }
    return this.data.get(key) || null;
  }

  removeItem(key: string): void {
    if (key !== 'access_token') {
      localStorage.removeItem(key);
      return;
    }
    this.data.delete(key);
  }

  setItem(key: string, data: string): void {
    if (key !== 'access_token') {
      console.log('setItem -->', key, data);
      localStorage.setItem(key, data);
      return;
    }
    console.log('setItem -->', key, data);
    this.data.set(key, data);
  }

  clear() {
    this.data.clear();
  }
}
