import {Injectable} from '@angular/core';
import {HttpRequest, HttpHandler, HttpEvent, HttpInterceptor, HttpResponse} from '@angular/common/http';
import {Observable} from 'rxjs';
import {map} from 'rxjs/operators';
import {DateTime} from 'luxon';

@Injectable()
export class ApiDateTimeInterceptorInterceptor implements HttpInterceptor {

  private isoDateFormat = /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d+)?(?:Z|[+\-]\d{2}:\d{2})$/;

  public intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    return next.handle(request).pipe(map((val: HttpEvent<any>) => {
      if (val instanceof HttpResponse) {
        const body = val.body;
        this.convert(body);
      }
      return val;
    }));
  }

  private convert(body: any) {
    if (body === null || body === undefined || typeof body !== 'object')
      return body;

    for (const key of Object.keys(body)) {
      const value = body[key];
      if (this.isIsoDateString(value)) {
        body[key] = DateTime.fromISO(value);
      } else if (typeof value === 'object') {
        this.convert(value);
      }
    }
  }

  private isIsoDateString(value: any): boolean {
    if (value === null || value === undefined || typeof value !== 'string')
      return false;
    return this.isoDateFormat.test(value);
  }
}