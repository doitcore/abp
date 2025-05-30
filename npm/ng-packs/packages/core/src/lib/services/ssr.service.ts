import { Inject, Injectable, PLATFORM_ID } from '@angular/core';
import { isPlatformServer } from '@angular/common';

@Injectable({
  providedIn: 'root',
})
export class SSRService {
  constructor(@Inject(PLATFORM_ID) private platformId: unknown) {}

  isSsr() {
    return isPlatformServer(this.platformId);
  }
}
