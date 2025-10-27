import { DOCUMENT, inject, Injectable } from '@angular/core';
import { OAuthStorage } from 'angular-oauth2-oidc';
import { AbpLocalStorageService } from '@abp/ng.core';

@Injectable({
  providedIn: 'root',
})
export class MemoryTokenStorageService implements OAuthStorage {
  private keysShouldStoreInMemory = [
    'access_token', 'id_token', 'expires_at',
    'id_token_claims_obj', 'id_token_expires_at',
    'id_token_stored_at', 'access_token_stored_at',
    'abpOAuthClientId', 'granted_scopes'
  ];

  private worker?: any;
  private port?: MessagePort;
  private cache = new Map<string, string>();
  private localStorageService = inject(AbpLocalStorageService);
  private _document = inject(DOCUMENT);
  private useSharedWorker = false;

  constructor() {
    this.initializeStorage();
  }

  private initializeStorage(): void {
    // @ts-ignore
    if (typeof SharedWorker !== 'undefined') {
      try {
        // @ts-ignore
        this.worker = new SharedWorker(
          new URL('../workers/token-storage.worker.ts', import.meta.url),
          { name: 'oauth-token-storage' }
        );

        this.port = this.worker.port;
        this.port.start();
        this.useSharedWorker = true;

        this.port.onmessage = (event) => {
          const { action, key, value } = event.data;

          switch (action) {
            case 'set':
              this.checkAuthStateChanges(key);
              this.cache.set(key, value);
              break;
            case 'remove':
              this.cache.delete(key);
              this.refreshDocument();
              break;
            case 'clear':
              this.cache.clear();
              this.refreshDocument();
              break;
          }
        };

        console.log('✅ Using SharedWorker for cross-tab token storage');
      } catch (error) {
        console.warn('⚠️ SharedWorker failed:', error);
        this.useSharedWorker = false;
      }
    } else {
      console.warn('⚠️ SharedWorker not supported');
      this.useSharedWorker = false;
    }
  }

  getItem(key: string): string | null {
    if (!this.keysShouldStoreInMemory.includes(key)) {
      return this.localStorageService.getItem(key);
    }
    return this.cache.get(key) || null;
  }

  setItem(key: string, value: string): void {
    if (!this.keysShouldStoreInMemory.includes(key)) {
      this.localStorageService.setItem(key, value);
      return;
    }

    if (this.useSharedWorker && this.port) {
      this.cache.set(key, value);
      this.port.postMessage({ action: 'set', key, value });
    } else {
      this.cache.set(key, value);
    }
  }

  removeItem(key: string): void {
    if (!this.keysShouldStoreInMemory.includes(key)) {
      this.localStorageService.removeItem(key);
      return;
    }

    if (this.useSharedWorker && this.port) {
      // this.cache.delete(key);
      this.port.postMessage({ action: 'remove', key });
    } else {
      this.cache.delete(key);
    }
  }

  clear(): void {
    if (this.useSharedWorker && this.port) {
      this.port.postMessage({ action: 'clear' });
    }
    this.cache.clear();
  }

  private checkAuthStateChanges = (key: string) => {
    if (key === 'access_token' && !this.cache.get('access_token')) {
      this.refreshDocument();
    }
  }

  private refreshDocument(): void {
    this._document.defaultView?.location.reload();
  }

}
