import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { VERSION } from '@angular/core';
import { ApiVersionService } from './api.version.service';
import { VersionInfo } from './version.info.model';
import { MessageUtils } from '../../shared/utils/message.utils';
import { MatSnackBar } from '@angular/material';

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
