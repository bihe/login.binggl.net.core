import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { catchError, timeout } from 'rxjs/operators';
import { VersionInfo } from '../../components/footer/version.info.model';
import { BaseDataService } from './api.base.service';

@Injectable()
export class ApiVersionService extends BaseDataService {
  private readonly APP_INFO_URL: string = '/api/v1/appinfo';

  constructor (private http: HttpClient) {
    super();
  }

  getApplicationInfo(): Observable<VersionInfo> {
    return this.http.get<VersionInfo>(this.APP_INFO_URL, this.RequestOptions)
      .pipe(
        timeout(this.RequestTimeOutDefault),
        catchError(this.handleError)
      );
  }
}
