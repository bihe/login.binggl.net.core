import { Injectable } from '@angular/core';
import { Http, Response } from '@angular/http';

import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/catch';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/timeoutWith';
import 'rxjs/add/operator/distinctUntilChanged';
import 'rxjs/add/operator/debounceTime';
import 'rxjs/add/observable/throw';

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
      .timeoutWith(this.RequestTimeOutDefault, Observable.throw(new Error('Timeout exceeded!')))
      .map(res => {
        return this.extractData<VersionInfo>(res);
      })
      .catch(this.handleError);
  }
}
