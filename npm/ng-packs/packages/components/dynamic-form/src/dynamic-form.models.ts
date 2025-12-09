import { Type } from '@angular/core';
import { ControlValueAccessor } from '@angular/forms';

export interface FormFieldConfig<T = any> {
  key: string;
  value?: any;
  type: 'text' | 'email' | 'number' | 'select' | 'checkbox' | 'date' | 'textarea';
  label: string;
  placeholder?: string;
  required?: boolean;
  disabled?: boolean;
  options?: OptionProps<T>;
  validators?: ValidatorConfig[];
  conditionalLogic?: ConditionalRule[];
  order?: number;
  gridSize?: number;
  component?: Type<ControlValueAccessor>;
}

export interface ValidatorConfig {
  type: 'required' | 'email' | 'minLength' | 'maxLength' | 'pattern' | 'custom' | 'min' | 'max' | 'requiredTrue';
  value?: any;
  message: string;
}

export interface ConditionalRule {
  dependsOn: string;
  condition: 'equals' | 'notEquals' | 'contains' | 'greaterThan' | 'lessThan';
  value: any;
  action: 'show' | 'hide' | 'enable' | 'disable';
}

export enum ConditionalAction {
  SHOW = 'show',
  HIDE = 'hide',
  ENABLE = 'enable',
  DISABLE = 'disable'
}

export interface OptionProps<T = any> {
  defaultValues?: T[];
  url?: string;
  disabled?: (option: T) => boolean;
  labelProp?: string;
  valueProp?: string;
  apiName?: string;
}
