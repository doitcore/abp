import { inject, Injectable, makeStateKey, PLATFORM_ID, TransferState } from '@angular/core';
import {
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest,
  HttpResponse,
} from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { isPlatformBrowser, isPlatformServer } from '@angular/common';
import { tap } from 'rxjs/operators';

@Injectable()
export class TransferStateInterceptor implements HttpInterceptor {
  private transferState = inject(TransferState);
  private platformId = inject(PLATFORM_ID);

  constructor() {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (req.method !== 'GET') {
      return next.handle(req);
    }

    const stateKey = makeStateKey<HttpResponse<any>>(req.urlWithParams);

    if (isPlatformBrowser(this.platformId)) {
      const storedResponse = this.transferState.get<HttpResponse<any>>(stateKey, null);
      if (storedResponse) {
        this.transferState.remove(stateKey);
        return of(new HttpResponse<any>({ body: storedResponse, status: 200 }));
      }
    }

    return next.handle(req).pipe(
      tap(event => {
        if (isPlatformServer(this.platformId) && event instanceof HttpResponse) {
          this.transferState.set(stateKey, event.body);
          console.log(`Interceptor: ${req.urlWithParams} verisi TransferState'e kaydedildi.`);
        }
      }),
    );
  }
}
