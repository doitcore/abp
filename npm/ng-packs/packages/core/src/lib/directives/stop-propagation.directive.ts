import { Directive, ElementRef, EventEmitter, OnInit, Output, inject } from '@angular/core';
import { fromEvent } from 'rxjs';
import { SubscriptionService } from '../services/subscription.service';

@Directive({
  selector: '[click.stop]',
  providers: [SubscriptionService],
})
export class StopPropagationDirective implements OnInit {
  private el = inject(ElementRef);
  private subscription = inject(SubscriptionService);

  @Output('click.stop') readonly stopPropEvent = new EventEmitter<MouseEvent>();

  ngOnInit(): void {
    this.subscription.addOne(fromEvent<MouseEvent>(this.el.nativeElement, 'click'), event => {
      event.stopPropagation();
      this.stopPropEvent.emit(event);
    });
  }
}
