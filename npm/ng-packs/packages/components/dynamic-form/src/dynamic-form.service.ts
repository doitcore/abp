import {Injectable} from '@angular/core';
import {FormControl, FormGroup, ValidatorFn, Validators} from '@angular/forms';
import {FormFieldConfig, ValidatorConfig} from './dynamic-form.models';

@Injectable({
  providedIn: 'root'
})

export class DynamicFormService {

  createFormGroup(fields: FormFieldConfig[]): FormGroup {
    const group: any = {};

    fields.forEach(field => {
      const validators = this.buildValidators(field.validators || []);
      const initialValue = this.getInitialValue(field);

      group[field.key] = new FormControl({
        value: initialValue,
        disabled: field.disabled || false
      }, validators);
    });

    return new FormGroup(group);
  }

  private buildValidators(validatorConfigs: ValidatorConfig[]): ValidatorFn[] {
    return validatorConfigs.map(config => {
      switch (config.type) {
        case 'required':
          return Validators.required;
        case 'email':
          return Validators.email;
        case 'minLength':
          return Validators.minLength(config.value);
        case 'maxLength':
          return Validators.maxLength(config.value);
        case 'pattern':
          return Validators.pattern(config.value);
        default:
          return Validators.nullValidator;
      }
    });
  }

  private getInitialValue(field: FormFieldConfig): any {
    if (field.value) {
      return field.value;
    }
    switch (field.type) {
      case 'checkbox':
        return false;
      case 'number':
        return 0;
      default:
        return '';
    }
  }
}
