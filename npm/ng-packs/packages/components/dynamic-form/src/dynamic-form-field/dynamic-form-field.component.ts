import {
  ChangeDetectionStrategy,
  Component,
  forwardRef,
  InjectionToken,
  input,
} from '@angular/core';
import { NgTemplateOutlet } from '@angular/common';
import { FormFieldConfig } from '../dynamic-form.models';
import { ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';

export const ABP_DYNAMIC_FORM_FIELD = new InjectionToken<DynamicFormFieldComponent>('AbpDynamicFormField');

const DYNAMIC_FORM_FIELD_CONTROL_VALUE_ACCESSOR = {
  provide: NG_VALUE_ACCESSOR,
  useExisting: forwardRef(() => DynamicFormFieldComponent),
  multi: true,
};

@Component({
  selector: 'abp-dynamic-form-field',
  templateUrl: './dynamic-form-field.component.html',
  providers: [{ provide: ABP_DYNAMIC_FORM_FIELD, useExisting: DynamicFormFieldComponent }, DYNAMIC_FORM_FIELD_CONTROL_VALUE_ACCESSOR],
  host: { 'class': 'abp-dynamic-form-field' },
  exportAs: 'abpDynamicFormField',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    FormsModule,
    NgTemplateOutlet
  ]
})

export class DynamicFormFieldComponent implements ControlValueAccessor {
  field = input.required<FormFieldConfig>();
  isVisible = input<boolean>(true);
  disabled = false;
  value: any;

  writeValue(value: any[]): void {
    //
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  private onChange: (value: any) => void = () => {};
  private onTouched: () => void = () => {};
}
