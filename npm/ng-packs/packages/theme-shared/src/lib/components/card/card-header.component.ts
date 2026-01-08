import { Component, HostBinding, Input } from '@angular/core';
import { NgClass } from '@angular/common';

@Component({
  selector: 'abp-card-header',
  template: `
    <div [ngClass]="cardHeaderClass" [style]="cardHeaderStyle">
      <ng-content></ng-content>
    </div>
  `,
  styles: [],
  imports: [NgClass],
})
export class CardHeaderComponent {
  @HostBinding('class') componentClass = 'card-header';
  @Input() cardHeaderClass: string;
  @Input() cardHeaderStyle: string;
}
