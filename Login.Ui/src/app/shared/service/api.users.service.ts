import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { catchError, timeout } from 'rxjs/operators';
import { UserInfo } from '../models/user.info.model';
import { BaseDataService } from './api.base.service';


@Injectable()
export class ApiUserService extends BaseDataService {
  private readonly APP_INFO_URL: string = '/api/v1/users';

  constructor (private http: HttpClient) {
    super();
  }

  getUserInfo(): Observable<UserInfo> {
    return this.http.get<UserInfo>(this.APP_INFO_URL, this.RequestOptions)
      .pipe(
        timeout(this.RequestTimeOutDefault),
        catchError(this.handleError)
      );
  }

  saveUserInfo(payload: UserInfo): Observable<UserInfo> {
    return this.http.post<UserInfo>(this.APP_INFO_URL, payload, this.RequestOptions)
      .pipe(
        timeout(this.RequestTimeOutDefault),
        catchError(this.handleError)
      );
  }
}
