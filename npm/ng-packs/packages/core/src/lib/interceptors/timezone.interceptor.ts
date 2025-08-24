import { inject, Injectable } from '@angular/core';
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { TimezoneService } from '../services';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class TimezoneInterceptor implements HttpInterceptor {
  protected readonly timezoneService = inject(TimezoneService);

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (!this.timezoneService.isUtcClockEnabled) {
      return next.handle(req);
    }
    const timezone = this.timezoneService.timezone;
    if (timezone) {
      req = req.clone({
        setHeaders: {
          __timezone: timezone,
        },
      });
    }
    return next.handle(req);
  }
}
