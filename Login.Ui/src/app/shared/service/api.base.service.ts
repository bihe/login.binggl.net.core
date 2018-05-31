import { Response, RequestOptions, Headers } from '@angular/http';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/observable/throw';

export class ApiBaseService {

  protected get RequestTimeOutDefault(): number { return 1000 * 60 * 1; }
  protected get RequestTimeOutLongRunning(): number { return 1000 * 60 * 10; }

  protected handleError (error: Response | any) {
    let errorRaised: any;
    if (error instanceof Response) {
      try {
        errorRaised = error.json();
        errorRaised.Status = error.status;
      } catch (exception) {
        errorRaised = error.toString();
      }
    } else {
      errorRaised = error.message ? error.message : error.toString();
    }
    return Observable.throw(errorRaised);
  }

  protected getRequestHeaders() {
    const headers = new Headers({
      'Content-Type': 'application/json',
      'Accept': 'application/json'
    });
    return { headers: headers };
  }

  protected getRequestOptions(): RequestOptions {
    const options = new RequestOptions(this.getRequestHeaders());
    return options;
  }

  protected extractRaw(res: Response) {
    if (res.status < 200 || res.status >= 300) {
      Observable.throw(new Error('Bad response status: ' + res.status));
    }
    const body = res.text().length > 0 ? res.text() : '';
    return body;
  }

  protected extractData<T>(res: Response): T {
    if (res.status < 200 || res.status >= 300) {
      Observable.throw(new Error('Bad response status: ' + res.status));
    }
    const data: T = res.text().length > 0 ? <T>res.json() as T : null;
    return data;
  }
}
