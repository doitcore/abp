import { ApplicationInfo, EnvironmentService } from '@abp/ng.core';
import { RouterLink } from '@angular/router';
import { Component, inject } from '@angular/core';

@Component({
  selector: 'abp-logo',
  template: `
    <a class="navbar-brand" routerLink="/">
      @if (appInfo.logoUrl) {
        <img [src]="appInfo.logoUrl" [alt]="appInfo.name" width="100%" height="auto" />
      } @else {
        {{ appInfo.name }}
      }
    </a>
  `,
  imports: [RouterLink],
})
export class LogoComponent {
  private environment = inject(EnvironmentService);

  get appInfo(): ApplicationInfo {
    return this.environment.getEnvironment().application;
  }
}
