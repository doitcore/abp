import { Inject, Injectable, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

@Injectable({
  providedIn: 'root',
})
export class AbpLocalStorageService implements Storage {
  private isBrowser: boolean;

  constructor(@Inject(PLATFORM_ID) private platformId: unknown) {
    this.isBrowser = isPlatformBrowser(this.platformId);
  }
  [name: string]: any;
  get length(): number {
    return this.isBrowser ? localStorage.length : 0;
  }

  clear(): void {
    if (this.isBrowser) {
      localStorage.clear();
    }
  }
  getItem(key: string): string | null {
    if (!this.isBrowser) {
      return null;
    }
    return localStorage.getItem(key);
  }
  key(index: number): string | null {
    if (!this.isBrowser) {
      return null;
    }
    return localStorage.key(index);
  }
  removeItem(key: string): void {
    if (this.isBrowser) {
      localStorage.removeItem(key);
    }
  }
  setItem(key: string, value: string): void {
    if (this.isBrowser) {
      localStorage.setItem(key, value);
    }
  }
}
