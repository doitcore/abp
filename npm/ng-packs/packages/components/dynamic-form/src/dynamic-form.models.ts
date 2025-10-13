
export interface FormFieldConfig {
  key: string;
  value?: any;
  type: 'text' | 'email' | 'number' | 'select' | 'checkbox' | 'date' | 'textarea';
  label: string;
  placeholder?: string;
  required?: boolean;
  disabled?: boolean;
  options?: { key: string; value: any }[];
  validators?: ValidatorConfig[];
  conditionalLogic?: ConditionalRule[];
  order?: number;
  gridSize?: number;
  component?: any;
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
