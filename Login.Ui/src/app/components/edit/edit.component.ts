import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ApplicationState } from '../../shared/service/application.state';
import { MessageUtils } from '../../shared/utils/message.utils';
import { MatSnackBar } from '@angular/material';
import { ApiUserService } from '../../shared/service/api.users.service';

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

          this.jsonPayload = JSON.stringify(data, null, 4);
        },
        error => {
          console.log('Error: ' + error);
          new MessageUtils().showError(this.snackBar, error);
        }
      );
  }

  public save() {
    console.log('Save the JSON payload!');
  }

}
