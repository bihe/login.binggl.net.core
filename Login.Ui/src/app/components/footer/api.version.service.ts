import { Injectable } from '@angular/core';
import { Http } from '@angular/http';
import { Observable } from 'rxjs';
import { catchError, map, timeout } from 'rxjs/operators';
import { ApiBaseService } from '../../shared/service/api.base.service';
import { VersionInfo } from './version.info.model';

@Injectable()
export class ApiVersionService extends ApiBaseService {
  private readonly APP_INFO_URL: string = '/api/v1/appinfo';

  constructor (private http: Http) {
    super();
  }

  getApplicationInfo(): Observable<VersionInfo> {
    return this.http.get(this.APP_INFO_URL, this.getRequestOptions())
      .pipe(
        timeout(this.RequestTimeOutDefault),
        map(res => {
          return this.extractData<VersionInfo>(res);
        }),
        catchError(this.handleError)
      );
  }
}
