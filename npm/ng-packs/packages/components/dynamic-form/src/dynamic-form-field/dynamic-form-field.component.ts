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
import { FormFieldConfig } from '../dynamic-form.models';
import {
  ControlValueAccessor,
  FormBuilder,
  FormControl,
  FormControlName,
  FormGroupDirective,
  FormsModule,
  NG_VALUE_ACCESSOR,
  NgControl,
  FormGroup,
  ReactiveFormsModule,
} from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NgTemplateOutlet } from '@angular/common';
import { LocalizationPipe } from '@abp/ng.core';

export const ABP_DYNAMIC_FORM_FIELD = new InjectionToken<DynamicFormFieldComponent>('AbpDynamicFormField');

const DYNAMIC_FORM_FIELD_CONTROL_VALUE_ACCESSOR = {
  provide: NG_VALUE_ACCESSOR,
  useExisting: forwardRef(() => DynamicFormFieldComponent),
  multi: true,
};

@Component({
  selector: 'abp-dynamic-form-field',
  templateUrl: './dynamic-form-field.component.html',
  styleUrls: ['./dynamic-form-field.component.scss'],
  providers: [
    { provide: ABP_DYNAMIC_FORM_FIELD, useExisting: DynamicFormFieldComponent },
    DYNAMIC_FORM_FIELD_CONTROL_VALUE_ACCESSOR
  ],
  host: { class: 'abp-dynamic-form-field' },
  exportAs: 'abpDynamicFormField',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule, NgTemplateOutlet, LocalizationPipe, ReactiveFormsModule],
})
export class DynamicFormFieldComponent implements OnInit, ControlValueAccessor {
  field = input.required<FormFieldConfig>();
  visible = input<boolean>(true);
  control!: FormControl;
  fieldFormGroup: FormGroup;
  readonly changeDetectorRef = inject(ChangeDetectorRef);
  readonly destroyRef = inject(DestroyRef);
  private injector = inject(Injector);
  private formBuilder = inject(FormBuilder);

  constructor() {
    this.fieldFormGroup = this.formBuilder.group({
      value: [{ value: '' }]
    });
  }

  ngOnInit() {
    const ngControl = this.injector.get(NgControl, null);
    if (ngControl) {
      this.control = this.injector.get(FormGroupDirective).getControl(ngControl as FormControlName);
    }
    this.value.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(value => {
      this.onChange(value);
    });
  }

  writeValue(value: any[]): void {
    this.value.setValue(value || '');
    this.changeDetectorRef.markForCheck();
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    if (isDisabled) {
      this.value.disable();
    } else {
      this.value.enable();
    }
    this.changeDetectorRef.markForCheck();
  }

  get isInvalid(): boolean {
    if (this.control) {
      return (this.control.invalid && (this.control.dirty || this.control.touched));
    }
    return false;
  }

  get errors(): string[] {
    if (this.control && this.control.errors) {
      const errorKeys = Object.keys(this.control.errors);
      return errorKeys.map(key => {
        const validator = this.field().validators.find(v => v.type.toLowerCase() === key.toLowerCase());
        console.log(this.field().validators, key);
        if (validator && validator.message) {
          return validator.message;
        }
        // Fallback error messages
        if (key === 'required') return `${this.field().label} is required`;
        if (key === 'email') return 'Please enter a valid email address';
        if (key === 'minlength') return `Minimum length is ${this.control.errors[key].requiredLength}`;
        if (key === 'maxlength') return `Maximum length is ${this.control.errors[key].requiredLength}`;
        return `${this.field().label} is invalid due to ${key} validation.`;
      });
    }
  }
  get value() {
    return this.fieldFormGroup.get('value');
  }
  private onChange: (value: any) => void = () => {};
  private onTouched: () => void = () => {};
}
