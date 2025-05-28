import { NavItem, NavItemsService } from '@abp/ng.theme.shared';
import { Component, TrackByFunction } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PermissionDirective, ToInjectorPipe } from '@abp/ng.core';
import { map } from 'rxjs/operators';

//TODO: Refactor AbpVisibleDirective to ssr compatible

@Component({
  selector: 'abp-nav-items',
  templateUrl: 'nav-items.component.html',
  imports: [CommonModule, PermissionDirective, ToInjectorPipe],
})
export class NavItemsComponent {
  trackByFn: TrackByFunction<NavItem> = (_, element) => element.id;
  visibleItems$ = this.navItems.items$.pipe(
    map(
      items => items.filter(item => !item.visible || item.visible(item)), // sadece görünmesi gerekenleri seç
    ),
  );
  constructor(public readonly navItems: NavItemsService) {}
}
