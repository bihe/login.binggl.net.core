import { Injectable } from '@angular/core';
import { Http } from '@angular/http';
import { Observable } from 'rxjs';
import { catchError, map, timeout } from 'rxjs/operators';
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
      .pipe(
        timeout(this.RequestTimeOutDefault),
        map(res => {
          return this.extractData<UserInfo>(res);
        }),
        catchError(this.handleError)
      );

  }

  saveUserInfo(payload: UserInfo): Observable<UserInfo> {
    return this.http.post(this.APP_INFO_URL, JSON.stringify(payload), this.getRequestOptions())
      .pipe(
        timeout(this.RequestTimeOutDefault),
        map(res => {
          return this.extractData<UserInfo>(res);
        }),
        catchError(this.handleError)
      );
  }
}
