import { Component, HostBinding, Input } from '@angular/core';
import { NgClass, NgStyle } from '@angular/common';

@Component({
  selector: 'abp-card-body',
  template: ` <div [ngClass]="cardBodyClass" [ngStyle]="cardBodyStyle">
    <ng-content></ng-content>
  </div>`,
  imports: [NgClass, NgStyle],
})
export class CardBodyComponent {
  @HostBinding('class') componentClass = 'card-body';
  @Input() cardBodyClass: string;
  @Input() cardBodyStyle: string;
}
