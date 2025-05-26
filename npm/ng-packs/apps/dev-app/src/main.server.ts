import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app.component';
import { createAppConfig } from './app/app.config';

const bootstrap = () => bootstrapApplication(AppComponent, createAppConfig(true));

export default bootstrap;
