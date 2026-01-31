import { AfterViewInit, Directive, ElementRef, inject, input, numberAttribute } from '@angular/core';

@Directive({
  selector: '[autofocus]',
})
export class AutofocusDirective implements AfterViewInit {
  private elRef = inject(ElementRef);

  readonly delay = input(0, { alias: 'autofocus', transform: numberAttribute });

  ngAfterViewInit(): void {
    setTimeout(() => this.elRef.nativeElement.focus(), this.delay());
  }
}
