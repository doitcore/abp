import { Component, HostBinding, Input } from '@angular/core';
import { NgClass, NgStyle } from '@angular/common';

@Component({
  selector: 'abp-card-header',
  template: `
    <div [ngClass]="cardHeaderClass" [ngStyle]="cardHeaderStyle">
      <ng-content></ng-content>
    </div>
  `,
  styles: [],
  imports: [NgClass, NgStyle],
})
export class CardHeaderComponent {
  @HostBinding('class') componentClass = 'card-header';
  @Input() cardHeaderClass: string;
  @Input() cardHeaderStyle: string;
}
