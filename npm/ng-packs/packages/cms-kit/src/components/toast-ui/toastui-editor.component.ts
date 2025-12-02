import { isPlatformBrowser } from '@angular/common';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import {
  Component,
  AfterViewInit,
  ViewChild,
  ElementRef,
  forwardRef,
  inject,
  PLATFORM_ID,
} from '@angular/core';
import Editor from '@toast-ui/editor';

import { AbpLocalStorageService } from '@abp/ng.core';

@Component({
  selector: 'abp-toastui-editor',
  template: `<div #editorContainer></div>`,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => ToastuiEditorComponent),
      multi: true,
    },
  ],
})
export class ToastuiEditorComponent implements AfterViewInit, ControlValueAccessor {
  @ViewChild('editorContainer', { static: true })
  editorContainer!: ElementRef<HTMLDivElement>;

  private editor!: Editor;
  private value = '';

  private platformId = inject(PLATFORM_ID);
  private localStorageService = inject(AbpLocalStorageService);

  private onChange: (value: string) => void = () => {};
  private onTouched: () => void = () => {};

  writeValue(value: string): void {
    this.value = value || '';
    if (this.editor) {
      this.editor.setMarkdown(this.value);
    }
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  ngAfterViewInit(): void {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    this.initializeEditor();
    // TODO: Revise this approach on lepton side :: Service injection through DI
    // this.setupThemeListener();
  }

  private getTheme(): string {
    return this.localStorageService.getItem('LPX_THEME') || 'light';
  }

  private initializeEditor(): void {
    const theme = this.getTheme();
    this.editor = new Editor({
      el: this.editorContainer.nativeElement,
      previewStyle: 'tab',
      height: '500px',
      autofocus: true,
      theme: theme,
      initialValue: this.value,
    });

    this.editor.addHook('change', () => {
      const value = this.editor.getMarkdown();
      this.onChange(value);
    });
  }
}
