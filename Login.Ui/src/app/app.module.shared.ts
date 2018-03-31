import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
// import { MatInputModule, MatSnackBarModule, MatDialogModule, MatNativeDateModule, MatProgressSpinnerModule,
//     MatTooltipModule } from '@angular/material';
// import { MatChipsModule } from '@angular/material/chips';
// import { MatCardModule } from '@angular/material/card';
// import { MatSlideToggleModule } from '@angular/material/slide-toggle';
// import { MatButtonModule } from '@angular/material/button';

import { AppComponent } from './components/app/app.component';
import { HomeComponent } from './components/home/home.component';

@NgModule({
  // imports: [ MatInputModule, MatSnackBarModule, MatDialogModule, MatNativeDateModule, MatProgressSpinnerModule,
  //   MatTooltipModule, MatChipsModule, MatCardModule, MatSlideToggleModule, MatButtonModule ],
  // exports: [ MatInputModule, MatSnackBarModule, MatDialogModule, MatNativeDateModule, MatProgressSpinnerModule,
  //   MatTooltipModule, MatChipsModule, MatCardModule, MatSlideToggleModule, MatButtonModule ],
})
export class AppMaterialModule { }

export const sharedConfig: NgModule = {
    bootstrap: [ AppComponent ],
    declarations: [
        AppComponent,
        HomeComponent
    ],
    imports: [
        AppMaterialModule
    ],
    providers: [],
    entryComponents: []
};
