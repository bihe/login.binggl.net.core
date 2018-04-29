import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { MatProgressSpinnerModule, MatTooltipModule, MatSnackBarModule } from '@angular/material';
// import { MatChipsModule } from '@angular/material/chips';
// import { MatCardModule } from '@angular/material/card';
// import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatButtonModule } from '@angular/material/button';

import { AppComponent } from './components/app/app.component';
import { HomeComponent } from './components/home/home.component';
import { FooterComponent } from './components/footer/footer.component';
import { HeaderComponent } from './components/header/header.component';
import { ApiVersionService } from './components/footer/api.version.service';
import { ApplicationState } from './shared/service/application.state';
import { ApiUserService } from './shared/service/api.users.service';
import { EditComponent } from './components/edit/edit.component';

@NgModule({
  imports: [ MatProgressSpinnerModule, MatTooltipModule, MatSnackBarModule, MatButtonModule ],
  exports: [ MatProgressSpinnerModule, MatTooltipModule, MatSnackBarModule, MatButtonModule ],
})
export class AppMaterialModule { }

export const sharedConfig: NgModule = {
    bootstrap: [ AppComponent ],
    declarations: [
        AppComponent,
        HomeComponent,
        EditComponent,
        FooterComponent,
        HeaderComponent
    ],
    imports: [
        AppMaterialModule
    ],
    providers: [ ApplicationState, ApiVersionService, ApiUserService ],
    entryComponents: []
};
