import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ApiUserService } from './api.users.service';
import { UserInfo } from './user.info.model';
import { ApplicationState } from '../../shared/service/application.state';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {

  userInfo: UserInfo;

  constructor(private userService: ApiUserService, private state: ApplicationState
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
          // new MessageUtils().showError(this.snackBar, error);
        }
      );
  }

}
