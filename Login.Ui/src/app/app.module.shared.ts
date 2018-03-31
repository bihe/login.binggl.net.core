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
import { FooterComponent } from './components/footer/footer.component';
import { HeaderComponent } from './components/header/header.component';
import { NavigationComponent } from './components/navigation/navigation.component';
import { ApiVersionService } from './components/footer/api.version.service';

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
        HomeComponent,
        FooterComponent,
        HeaderComponent,
        NavigationComponent
    ],
    imports: [
        AppMaterialModule
    ],
    providers: [ ApiVersionService ],
    entryComponents: []
};
