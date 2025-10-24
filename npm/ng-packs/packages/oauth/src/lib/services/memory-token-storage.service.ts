import { inject, Injectable } from '@angular/core';
import { OAuthStorage } from 'angular-oauth2-oidc';
import { AbpLocalStorageService } from '@abp/ng.core';

@Injectable({
  providedIn: 'root',
})
export class MemoryTokenStorageService implements OAuthStorage {
  private keysShouldStoreInMemory = ['access_token', 'id_token', 'expires_at', 'id_token_claims_obj', 'id_token_expires_at', 'id_token_stored_at', 'access_token_stored_at', 'abpOAuthClientId', 'granted_scopes'];
  private data = new Map<string, string>();
  private localStorageService = inject(AbpLocalStorageService);

  getItem(key: string): string {
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
      this.data.set(key, data);
      return;
    }
    this.localStorageService.setItem(key, data);
  }

  clear() {
    this.data.clear();
  }
}
