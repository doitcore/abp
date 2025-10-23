import { inject, Injectable } from '@angular/core';
import { OAuthStorage } from 'angular-oauth2-oidc';
import { AbpLocalStorageService } from '@abp/ng.core';

@Injectable({
  providedIn: 'root',
})
export class MemoryTokenStorageService implements OAuthStorage {
  private keysShouldStoreLocalstorage = ['PKCE_verifier', 'abpOAuthClientId', 'abp_session', 'refresh_token', 'granted_scopes', 'id_token_claims_obj'];
  private keysShouldStoreInMemory = ['access_token'];
  private data = new Map<string, string>();
  private localStorageService = inject(AbpLocalStorageService);

  getItem(key: string): string {
    console.log('getItem -->>', key);
    if (this.keysShouldStoreInMemory.includes(key)) {
      return this.data.get(key) || null;
    }
    return this.localStorageService.getItem(key);
  }

  removeItem(key: string): void {
    if (this.keysShouldStoreInMemory.includes(key)) {
      this.data.delete(key);
      return;
    }
    this.localStorageService.removeItem(key);
  }

  setItem(key: string, data: string): void {
    if (this.keysShouldStoreInMemory.includes(key)) {
      console.log('setItem -->', key, data);
      this.data.set(key, data);
      return;
    }
    console.log('setItem -->', key, data);
    this.localStorageService.setItem(key, data);
  }

  clear() {
    this.data.clear();
  }
}
