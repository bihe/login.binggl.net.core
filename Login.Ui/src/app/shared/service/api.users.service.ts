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
import { UserInfo } from '../models/user.info.model';

@Injectable()
export class ApiUserService extends ApiBaseService {
  private readonly APP_INFO_URL: string = '/api/v1/users';

  constructor (private http: Http) {
    super();
  }

  getUserInfo(): Observable<UserInfo> {
    return this.http.get(this.APP_INFO_URL, this.getRequestOptions())
      .timeoutWith(this.RequestTimeOutDefault, Observable.throw(new Error('Timeout exceeded!')))
      .map(res => {
        return this.extractData<UserInfo>(res);
      })
      .catch(this.handleError);
  }

  saveUserInfo(payload: UserInfo): Observable<UserInfo> {
    return this.http.post(this.APP_INFO_URL, JSON.stringify(payload), this.getRequestOptions())
      .timeoutWith(this.RequestTimeOutDefault, Observable.throw(new Error('Timeout exceeded!')))
      .map(res => {
        return this.extractData<UserInfo>(res);
      })
      .catch(this.handleError);
  }
}
