import { inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser, DOCUMENT } from '@angular/common';

export class DomStrategy {
  private readonly isBrowser: boolean;

  constructor(
    private targetFactory: () => HTMLElement,
    public position: InsertPosition = 'beforeend',
  ) {
    const platformId = inject(PLATFORM_ID);
    this.isBrowser = isPlatformBrowser(platformId);
  }

  insertElement<T extends HTMLElement>(element: T) {
    if (this.isBrowser) {
      const target = this.targetFactory();
      target.insertAdjacentElement(this.position, element);
    }
  }
}

export const DOM_STRATEGY = {
  AfterElement: (el: HTMLElement) => new DomStrategy(() => el, 'afterend'),
  BeforeElement: (el: HTMLElement) => new DomStrategy(() => el, 'beforebegin'),
  AppendToBody: () => new DomStrategy(() => inject(DOCUMENT).body, 'beforeend'),
  AppendToHead: () => new DomStrategy(() => inject(DOCUMENT).head, 'beforeend'),
  PrependToHead: () => new DomStrategy(() => inject(DOCUMENT).head, 'afterbegin'),
};
