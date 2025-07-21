import { Directive, Output, EventEmitter, ElementRef, AfterViewInit, inject } from '@angular/core';

@Directive({
  selector: '[abpInit]',
})
export class InitDirective implements AfterViewInit {
  private elRef = inject(ElementRef);

  @Output('abpInit') readonly init = new EventEmitter<ElementRef<any>>();

  ngAfterViewInit() {
    this.init.emit(this.elRef);
  }
}
