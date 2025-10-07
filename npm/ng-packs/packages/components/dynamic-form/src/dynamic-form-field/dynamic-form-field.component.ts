import { ChangeDetectionStrategy, Component, InjectionToken } from '@angular/core';
import { NgTemplateOutlet } from '@angular/common';

export const ABP_DYNAMIC_FORM_FIELD = new InjectionToken<DynamicFormFieldComponent>('AbpDynamicFormField');

@Component({
  selector: 'abp-dynamic-form-field',
  templateUrl: './dynamic-form-field.component.html',
  providers: [{ provide: ABP_DYNAMIC_FORM_FIELD, useExisting: DynamicFormFieldComponent }],
  host: { 'class': 'abp-dynamic-form-field' },
  exportAs: 'abpDynamicFormField',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    NgTemplateOutlet
  ]
})

export class DynamicFormFieldComponent {
  field = input<FormFieldConfig>();
  @Input() isVisible: boolean = true;

  ngOnInit() {
    const control = this.form.get(this.field.key);
    if (control) {
      control.valueChanges.subscribe(value => {
        this.fieldChange.emit({ fieldKey: this.field.key, value });
      });
    }
  }

  isFieldInvalid(): boolean {
    const control = this.form.get(this.field.key);
    return !!(control && control.invalid && (control.dirty || control.touched));
  }

  getErrorMessage(): string {
    const control = this.form.get(this.field.key);
    if (!control || !control.errors) return '';

    const validators = this.field.validators || [];

    for (const validator of validators) {
      if (control.errors[validator.type]) {
        return validator.message;
      }
    }

    // Fallback error messages
    if (control.errors['required']) return `${this.field.label} is required`;
    if (control.errors['email']) return 'Please enter a valid email address';
    if (control.errors['minlength']) return `Minimum length is ${control.errors['minlength'].requiredLength}`;
    if (control.errors['maxlength']) return `Maximum length is ${control.errors['maxlength'].requiredLength}`;

    return 'Invalid input';
  }
}
