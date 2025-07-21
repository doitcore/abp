import { AbpVisibleDirective, NavItem, NavItemsService } from '@abp/ng.theme.shared';
import { Component, TrackByFunction, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PermissionDirective, SSRService, ToInjectorPipe } from '@abp/ng.core';

@Component({
  selector: 'abp-nav-items',
  templateUrl: 'nav-items.component.html',
  imports: [CommonModule, AbpVisibleDirective, PermissionDirective, ToInjectorPipe],
})
export class NavItemsComponent {
  protected readonly ssrService = inject(SSRService);
  readonly navItems = inject(NavItemsService);

  trackByFn: TrackByFunction<NavItem> = (_, element) => element.id;
}
