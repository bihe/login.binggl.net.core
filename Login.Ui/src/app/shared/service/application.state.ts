import { ReplaySubject } from 'rxjs/ReplaySubject';
import { UserInfo } from '../../components/home/user.info.model';

export class ApplicationState {
    private progress: ReplaySubject<boolean> = new ReplaySubject();
    private userInfo: ReplaySubject<UserInfo> = new ReplaySubject();

    public setUserInfo(data: UserInfo) {
        this.userInfo.next(data);
    }

    public getUserInfo(): ReplaySubject<UserInfo> {
        return this.userInfo;
    }

    public setProgress(data: boolean) {
        this.progress.next(data);
    }

    public getProgress(): ReplaySubject<boolean> {
        return this.progress;
    }
}