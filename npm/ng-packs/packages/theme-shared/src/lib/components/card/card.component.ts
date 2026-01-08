import { Component, Input } from '@angular/core';
import { NgClass } from '@angular/common';

@Component({
  selector: 'abp-card',
  template: ` <div class="card" [ngClass]="cardClass" [style]="cardStyle">
    <ng-content></ng-content>
  </div>`,
  imports: [NgClass],
})
export class CardComponent {
  @Input() cardClass: string;

  @Input() cardStyle: string;
}
