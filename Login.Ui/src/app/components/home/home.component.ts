import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ApplicationState } from '../../shared/service/application.state';
import { MessageUtils } from '../../shared/utils/message.utils';
import { MatSnackBar } from '@angular/material';
import { UserInfo } from '../../shared/models/user.info.model';
import { ApiUserService } from '../../shared/service/api.users.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {

  userInfo: UserInfo;

  constructor(private userService: ApiUserService,
    private state: ApplicationState,
    private snackBar: MatSnackBar,
    private router: Router,
  ) {}

  ngOnInit() {
    this.userService.getUserInfo()
      .subscribe(
        data => {
          this.userInfo = data;
          this.state.setUserInfo(data);
        },
        error => {
          console.log('Error: ' + error);
          new MessageUtils().showError(this.snackBar, error);
        }
      );
  }


  edit() {
    this.router.navigateByUrl('/edit');
    return;
  }
}
