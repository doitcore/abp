import { bootstrapApplication } from '@angular/platform-browser';
import { createAppConfig } from './app/app.config';
import { AppComponent } from './app/app.component';

bootstrapApplication(AppComponent, createAppConfig(false)).catch(err => console.error(err));
