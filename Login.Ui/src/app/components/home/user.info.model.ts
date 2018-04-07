import { SiteInfo } from './site.info.model';

export class UserInfo {
  public thisSite: string;
  public editable: boolean;
  public id: string;
  public email: string;
  public displayName: string;
  public userName: string;
  public sitePermissions: SiteInfo[];
}
