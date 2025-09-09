import { AbpVisibleDirective, NavItem, NavItemsService } from '@abp/ng.theme.shared';
import { Component, TrackByFunction, inject } from '@angular/core';
import { NgComponentOutlet, AsyncPipe } from '@angular/common';
import { PermissionDirective, ToInjectorPipe } from '@abp/ng.core';

@Component({
  selector: 'abp-nav-items',
  templateUrl: 'nav-items.component.html',
  imports: [NgComponentOutlet, AsyncPipe, AbpVisibleDirective, PermissionDirective, ToInjectorPipe],
})
export class NavItemsComponent {
  readonly navItems = inject(NavItemsService);

  trackByFn: TrackByFunction<NavItem> = (_, element) => element.id;
}
