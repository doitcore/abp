import { Component, HostBinding, Input } from '@angular/core';
import { NgClass, NgStyle } from '@angular/common';

@Component({
  selector: 'abp-card-footer',
  template: `
    <div [ngStyle]="cardFooterStyle" [ngClass]="cardFooterClass">
      <ng-content></ng-content>
    </div>
  `,
  styles: [],
  imports: [NgClass, NgStyle],
})
export class CardFooterComponent {
  @HostBinding('class') componentClass = 'card-footer';
  @Input() cardFooterStyle: string;
  @Input() cardFooterClass: string;
}
