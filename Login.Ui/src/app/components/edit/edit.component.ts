import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { Router } from '@angular/router';
import { UserInfo } from '../../shared/models/user.info.model';
import { ApiUserService } from '../../shared/service/api.users.service';
import { ApplicationState } from '../../shared/service/application.state';
import { MessageUtils } from '../../shared/utils/message.utils';

@Component({
  selector: 'app-edit',
  templateUrl: './edit.component.html',
  styleUrls: ['./edit.component.css']
})
export class EditComponent implements OnInit {

  public jsonPayload: string;
  public isEditable = false;

  constructor(private userService: ApiUserService,
    private state: ApplicationState,
    private snackBar: MatSnackBar,
    private router: Router,
  ) {}

  ngOnInit() {

    this.userService.getUserInfo()
      .subscribe(
        data => {
          this.isEditable = data.editable;
          if (!data.editable) {
            new MessageUtils().showError(this.snackBar, 'No permission to edit data!');
            this.router.navigateByUrl('/home');
            return;
          }

          this.jsonPayload = this.jsonify(data);
        },
        error => {
          console.log('Error: ' + error);
          new MessageUtils().showError(this.snackBar, error);
        }
      );
  }

  public save() {
    console.log('Save the JSON payload!');

    this.state.setProgress(true);

    let user: UserInfo;
    user = JSON.parse(this.jsonPayload);
    this.userService.saveUserInfo(user)
      .subscribe(
        data => {
          // stupid hack, to enforce a redirect to re-challenge authentication
          // the better approach would be to handle 302 or 403 and act accordingly
          if (data.userName === '__RELOGIN__') {
            console.log('No result received from backend - re-trigger authentication process!');
            window.location.href = '/relogin';
            return;
          }

          console.log('saved!');
          this.jsonPayload = this.jsonify(data);
          this.state.setProgress(false);
        },
        error => {
          this.state.setProgress(false);
          console.log('Error: ' + error);
          new MessageUtils().showError(this.snackBar, error);
        }
      );
  }

  private jsonify(data: UserInfo) {
    return JSON.stringify(data, null, 4);
  }
}
