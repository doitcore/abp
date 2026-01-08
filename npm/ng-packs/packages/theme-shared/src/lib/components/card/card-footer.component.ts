import { Component, HostBinding, Input } from '@angular/core';
import { NgClass } from '@angular/common';

@Component({
  selector: 'abp-card-footer',
  template: `
    <div [style]="cardFooterStyle" [ngClass]="cardFooterClass">
      <ng-content></ng-content>
    </div>
  `,
  styles: [],
  imports: [NgClass],
})
export class CardFooterComponent {
  @HostBinding('class') componentClass = 'card-footer';
  @Input() cardFooterStyle: string;
  @Input() cardFooterClass: string;
}
