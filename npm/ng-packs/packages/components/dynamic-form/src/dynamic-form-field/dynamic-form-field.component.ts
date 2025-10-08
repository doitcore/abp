import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  DestroyRef,
  forwardRef,
  inject,
  InjectionToken, Injector,
  input,
  OnInit,
} from '@angular/core';
import { NgTemplateOutlet } from '@angular/common';
import { FormFieldConfig } from '../dynamic-form.models';
import {
  AbstractControl,
  ControlValueAccessor,
  FormControl,
  FormControlName,
  FormGroupDirective,
  FormsModule,
  NG_VALIDATORS,
  NG_VALUE_ACCESSOR,
  NgControl,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

export const ABP_DYNAMIC_FORM_FIELD = new InjectionToken<DynamicFormFieldComponent>('AbpDynamicFormField');

const DYNAMIC_FORM_FIELD_CONTROL_VALUE_ACCESSOR = {
  provide: NG_VALUE_ACCESSOR,
  useExisting: forwardRef(() => DynamicFormFieldComponent),
  multi: true,
};

@Component({
  selector: 'abp-dynamic-form-field',
  templateUrl: './dynamic-form-field.component.html',
  providers: [
    { provide: ABP_DYNAMIC_FORM_FIELD, useExisting: DynamicFormFieldComponent },
    DYNAMIC_FORM_FIELD_CONTROL_VALUE_ACCESSOR
  ],
  host: { class: 'abp-dynamic-form-field' },
  exportAs: 'abpDynamicFormField',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule],
})
export class DynamicFormFieldComponent implements OnInit, ControlValueAccessor {
  field = input.required<FormFieldConfig>();
  visible = input<boolean>(true);
  disabled = false;
  value: any;
  isFieldInvalid: boolean = false;
  control!: FormControl;
  readonly changeDetectorRef = inject(ChangeDetectorRef);
  readonly destroyRef = inject(DestroyRef);
  private injector = inject(Injector);

  ngOnInit() {
    const ngControl = this.injector.get(NgControl, null);
    if (ngControl) {
      this.control = this.injector.get(FormGroupDirective).getControl(ngControl as FormControlName);
    }
  }

  onValueChange(value: any) {
    this.onChange(value);
    this.changeDetectorRef.markForCheck();
  }

  writeValue(value: any[]): void {
    this.value = value;
    this.changeDetectorRef.markForCheck();
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
    this.changeDetectorRef.markForCheck();
  }

  get isValid(): boolean {
    if (this.control) {
      return this.control.invalid && (this.control.dirty || this.control.touched);
    }
    return true;
  }

  private onChange: (value: any) => void = () => {};
  private onTouched: () => void = () => {};
}
