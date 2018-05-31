import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';
import { NgModule } from '@angular/core';

import { sharedConfig } from './app.module.shared';
import { AppRoutingModule } from './app.module.routes';

@NgModule({
  bootstrap: sharedConfig.bootstrap,
  declarations: sharedConfig.declarations,
  imports: [
      BrowserModule,
      BrowserAnimationsModule,
      FormsModule,
      HttpModule,
      AppRoutingModule,
      ...sharedConfig.imports
  ],
  providers: [
      ...sharedConfig.providers
  ],
  entryComponents: sharedConfig.entryComponents
})
export class AppModule { }
