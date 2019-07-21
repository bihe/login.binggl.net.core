import { Component, OnInit, VERSION } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { ApiVersionService } from '../../shared/service/api.version.service';
import { MessageUtils } from '../../shared/utils/message.utils';
import { VersionInfo } from './version.info.model';

@Component({
  selector: 'app-footer',
  templateUrl: './footer.component.html',
  styleUrls: ['./footer.component.css']
})
export class FooterComponent implements OnInit {

  appData: VersionInfo;
  year: number = new Date().getFullYear();

  constructor(
    private versionService: ApiVersionService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.versionService.getApplicationInfo()
      .subscribe(
        data => {
          this.appData = data;
          this.appData.uiRuntime = 'angular=' + VERSION.full;
        },
        error => {
          console.log('Error: ' + error);
          new MessageUtils().showError(this.snackBar, error);
        }
      );
  }
}
