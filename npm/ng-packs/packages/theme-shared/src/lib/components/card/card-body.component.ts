import { Component, HostBinding, Input } from '@angular/core';
import { NgClass } from '@angular/common';

@Component({
  selector: 'abp-card-body',
  template: ` <div [ngClass]="cardBodyClass" [style]="cardBodyStyle">
    <ng-content></ng-content>
  </div>`,
  imports: [NgClass],
})
export class CardBodyComponent {
  @HostBinding('class') componentClass = 'card-body';
  @Input() cardBodyClass: string;
  @Input() cardBodyStyle: string;
}
