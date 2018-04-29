import { Component, OnInit } from '@angular/core';
import { DomSanitizer, SafeStyle } from '@angular/platform-browser';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material';
import { ApplicationState } from '../../shared/service/application.state';
import { MessageUtils } from '../../shared/utils/message.utils';
import { UserInfo } from '../../shared/models/user.info.model';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css']
})
export class HeaderComponent implements OnInit {

  menuVisible = false;
  showProgress = false;
  userInfo: UserInfo;

  constructor(
    private state: ApplicationState,
    private snackBar: MatSnackBar,
    private sanitizer: DomSanitizer,
    private router: Router) {
  }

  ngOnInit() {
    this.state.getProgress()
      .subscribe(
        data => {
          this.showProgress = data;
        },
        error => {
          new MessageUtils().showError(this.snackBar, error);
        }
      );

    this.state.getUserInfo()
      .subscribe(
        data => {
          this.userInfo = data;
        },
        error => {
          new MessageUtils().showError(this.snackBar, error);
        }
      );
  }

  toggleMenu(visible: boolean) {
    this.menuVisible = visible;
  }

  menuTransform() {
    if (this.menuVisible) {
      return this.sanitizer.bypassSecurityTrustStyle('translateX(0)');
    } else {
      return this.sanitizer.bypassSecurityTrustStyle('translateX(-110%)');
    }
  }
}
