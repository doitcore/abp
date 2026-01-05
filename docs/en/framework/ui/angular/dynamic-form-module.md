```json
//[doc-seo]
{
    "Description": "Learn how to use the ABP Dynamic Form Module to create dynamic, configurable forms with validation, conditional logic, and custom components in Angular applications."
}
```

# Dynamic Form Module

The ABP Dynamic Form Module is a powerful component that allows you to create dynamic, configurable forms without writing extensive HTML templates. It provides a declarative way to define form fields with validation, conditional logic, grid layout, and custom components.

## Installation

The Dynamic Form Module is part of the `@abp/ng.components` package. If you haven't installed it yet, install it via npm:

```bash
npm install @abp/ng.components
```

## Usage

Import the `DynamicFormComponent` in your component:

```ts
import { DynamicFormComponent } from '@abp/ng.components/dynamic-form';

@Component({
  selector: 'app-my-component',
  imports: [DynamicFormComponent],
  templateUrl: './my-component.component.html',
})
export class MyComponent {}
```

## Basic Example

Here's a simple example of how to use the dynamic form:

```ts
import { Component } from '@angular/core';
import { DynamicFormComponent } from '@abp/ng.components/dynamic-form';
import { FormFieldConfig } from '@abp/ng.components/dynamic-form';

@Component({
  selector: 'app-user-form',
  imports: [DynamicFormComponent],
  template: `
    <abp-dynamic-form
      [fields]="formFields"
      [submitButtonText]="'Submit'"
      [showCancelButton]="true"
      (onSubmit)="handleSubmit($event)"
      (formCancel)="handleCancel()">
    </abp-dynamic-form>
  `,
})
export class UserFormComponent {
  formFields: FormFieldConfig[] = [
    {
      key: 'firstName',
      type: 'text',
      label: 'First Name',
      placeholder: 'Enter your first name',
      required: true,
      order: 1,
    },
    {
      key: 'lastName',
      type: 'text',
      label: 'Last Name',
      placeholder: 'Enter your last name',
      required: true,
      order: 2,
    },
    {
      key: 'email',
      type: 'email',
      label: 'Email',
      placeholder: 'Enter your email',
      required: true,
      order: 3,
    },
  ];

  handleSubmit(formValue: any) {
    console.log('Form submitted:', formValue);
    // Handle form submission
  }

  handleCancel() {
    console.log('Form cancelled');
    // Handle form cancellation
  }
}
```

## Component Inputs

The `DynamicFormComponent` accepts the following inputs:

| Input | Type | Default | Description |
|-------|------|---------|-------------|
| `fields` | `FormFieldConfig[]` | `[]` | Array of field configurations |
| `values` | `Record<string, any>` | `undefined` | Initial values for the form |
| `submitButtonText` | `string` | `'Submit'` | Text for the submit button |
| `submitInProgress` | `boolean` | `false` | Whether form submission is in progress |
| `showCancelButton` | `boolean` | `false` | Whether to show the cancel button |

## Component Outputs

| Output | Type | Description |
|--------|------|-------------|
| `onSubmit` | `EventEmitter<any>` | Emitted when the form is submitted with valid data |
| `formCancel` | `EventEmitter<void>` | Emitted when the cancel button is clicked |

## FormFieldConfig Properties

The `FormFieldConfig` interface defines the structure of each field in the form:

```ts
interface FormFieldConfig {
  key: string;                    // Unique identifier for the field
  type: FieldType;                // Type of the field
  label: string;                  // Label text for the field
  value?: any;                    // Initial value
  placeholder?: string;           // Placeholder text
  required?: boolean;             // Whether the field is required
  disabled?: boolean;             // Whether the field is disabled
  options?: OptionProps;          // Options for select fields
  validators?: ValidatorConfig[]; // Array of validator configurations
  conditionalLogic?: ConditionalRule[]; // Array of conditional rules
  order?: number;                 // Display order (ascending)
  gridSize?: number;              // Bootstrap grid size (1-12)
  component?: Type<ControlValueAccessor>; // Custom component
}
```

### Field Types

The following field types are supported:

- `text` - Text input
- `email` - Email input
- `number` - Number input
- `select` - Dropdown select
- `checkbox` - Checkbox
- `date` - Date picker
- `textarea` - Textarea

## Validators

You can add validators to your form fields using the `validators` property:

```ts
const formFields: FormFieldConfig[] = [
  {
    key: 'username',
    type: 'text',
    label: 'Username',
    validators: [
      {
        type: 'required',
        message: 'Username is required',
      },
      {
        type: 'minLength',
        value: 3,
        message: 'Username must be at least 3 characters',
      },
      {
        type: 'maxLength',
        value: 20,
        message: 'Username must not exceed 20 characters',
      },
    ],
  },
  {
    key: 'age',
    type: 'number',
    label: 'Age',
    validators: [
      {
        type: 'min',
        value: 18,
        message: 'You must be at least 18 years old',
      },
      {
        type: 'max',
        value: 100,
        message: 'Age must not exceed 100',
      },
    ],
  },
];
```

### Available Validator Types

| Type | Description | Requires Value |
|------|-------------|----------------|
| `required` | Field is required | No |
| `email` | Must be a valid email | No |
| `minLength` | Minimum string length | Yes |
| `maxLength` | Maximum string length | Yes |
| `min` | Minimum numeric value | Yes |
| `max` | Maximum numeric value | Yes |
| `pattern` | Regular expression pattern | Yes |
| `requiredTrue` | Must be true (for checkboxes) | No |

## Select Fields with Options

You can create dropdown select fields with static or dynamic options:

### Static Options

```ts
const formFields: FormFieldConfig[] = [
  {
    key: 'country',
    type: 'select',
    label: 'Country',
    options: {
      defaultValues: [
        { key: 'us', value: 'United States' },
        { key: 'uk', value: 'United Kingdom' },
        { key: 'ca', value: 'Canada' },
      ],
      valueProp: 'key',
      labelProp: 'value',
    },
  },
];
```

### Dynamic Options from API

```ts
const formFields: FormFieldConfig[] = [
  {
    key: 'department',
    type: 'select',
    label: 'Department',
    options: {
      url: '/api/departments',
      apiName: 'MyApi',
      valueProp: 'id',
      labelProp: 'name',
    },
  },
];
```

### OptionProps Interface

```ts
interface OptionProps<T = any> {
  defaultValues?: T[];           // Static array of options
  url?: string;                  // API endpoint URL
  disabled?: (option: T) => boolean; // Function to disable specific options
  labelProp?: string;            // Property name for label
  valueProp?: string;            // Property name for value
  apiName?: string;              // API name for RestService
}
```

## Conditional Logic

The Dynamic Form Module supports conditional logic to show/hide or enable/disable fields based on other field values:

```ts
const formFields: FormFieldConfig[] = [
  {
    key: 'hasLicense',
    type: 'checkbox',
    label: 'Do you have a driver\'s license?',
    order: 1,
  },
  {
    key: 'licenseNumber',
    type: 'text',
    label: 'License Number',
    placeholder: 'Enter your license number',
    order: 2,
    conditionalLogic: [
      {
        dependsOn: 'hasLicense',
        condition: 'equals',
        value: true,
        action: 'show',
      },
    ],
  },
  {
    key: 'age',
    type: 'number',
    label: 'Age',
    order: 3,
  },
  {
    key: 'parentConsent',
    type: 'checkbox',
    label: 'Parent Consent Required',
    order: 4,
    conditionalLogic: [
      {
        dependsOn: 'age',
        condition: 'lessThan',
        value: 18,
        action: 'show',
      },
    ],
  },
];
```

### Conditional Rule Interface

```ts
interface ConditionalRule {
  dependsOn: string;    // Key of the field to watch
  condition: string;    // Condition type
  value: any;          // Value to compare against
  action: string;      // Action to perform
}
```

### Available Conditions

- `equals` - Field value equals the specified value
- `notEquals` - Field value does not equal the specified value
- `contains` - Field value contains the specified value (for strings/arrays)
- `greaterThan` - Field value is greater than the specified value (for numbers)
- `lessThan` - Field value is less than the specified value (for numbers)

### Available Actions

- `show` - Show the field when condition is met
- `hide` - Hide the field when condition is met
- `enable` - Enable the field when condition is met
- `disable` - Disable the field when condition is met

## Grid Layout

You can use the `gridSize` property to control the Bootstrap grid layout:

```ts
const formFields: FormFieldConfig[] = [
  {
    key: 'firstName',
    type: 'text',
    label: 'First Name',
    gridSize: 6, // Half width
    order: 1,
  },
  {
    key: 'lastName',
    type: 'text',
    label: 'Last Name',
    gridSize: 6, // Half width
    order: 2,
  },
  {
    key: 'address',
    type: 'textarea',
    label: 'Address',
    gridSize: 12, // Full width
    order: 3,
  },
];
```

The `gridSize` property uses Bootstrap's 12-column grid system. If not specified, it defaults to 12 (full width).

## Custom Components

You can use custom components for specific fields by providing a component that implements `ControlValueAccessor`:

```ts
// custom-rating.component.ts
import { Component, forwardRef } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 'app-custom-rating',
  template: `
    <div class="rating">
      @for (star of [1,2,3,4,5]; track star) {
        <span
          class="star"
          [class.filled]="star <= value"
          (click)="setValue(star)">
          ★
        </span>
      }
    </div>
  `,
  styles: [`
    .star { cursor: pointer; font-size: 24px; color: #ccc; }
    .star.filled { color: #ffc107; }
  `],
  providers: [{
    provide: NG_VALUE_ACCESSOR,
    useExisting: forwardRef(() => CustomRatingComponent),
    multi: true
  }]
})
export class CustomRatingComponent implements ControlValueAccessor {
  value = 0;
  onChange: any = () => {};
  onTouched: any = () => {};

  setValue(rating: number) {
    this.value = rating;
    this.onChange(rating);
    this.onTouched();
  }

  writeValue(value: any): void {
    this.value = value || 0;
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }
}
```

Then use it in your form configuration:

```ts
import { CustomRatingComponent } from './custom-rating.component';

const formFields: FormFieldConfig[] = [
  {
    key: 'rating',
    type: 'text', // Type is ignored when using custom component
    label: 'Rating',
    component: CustomRatingComponent,
    value: 3,
  },
];
```

## Setting Initial Values

You can set initial values for the form fields in two ways:

### 1. Using the `value` property in FormFieldConfig

```ts
const formFields: FormFieldConfig[] = [
  {
    key: 'firstName',
    type: 'text',
    label: 'First Name',
    value: 'John',
  },
];
```

### 2. Using the `values` input

```ts
@Component({
  template: `
    <abp-dynamic-form
      [fields]="formFields"
      [values]="initialValues"
      (onSubmit)="handleSubmit($event)">
    </abp-dynamic-form>
  `,
})
export class MyComponent {
  formFields: FormFieldConfig[] = [
    {
      key: 'firstName',
      type: 'text',
      label: 'First Name',
    },
    {
      key: 'lastName',
      type: 'text',
      label: 'Last Name',
    },
  ];

  initialValues = {
    firstName: 'John',
    lastName: 'Doe',
  };

  handleSubmit(formValue: any) {
    console.log(formValue);
  }
}
```

## Programmatic Form Control

You can access the form instance using the `exportAs` property and template reference variable:

```ts
@Component({
  template: `
    <abp-dynamic-form
      #myForm="abpDynamicForm"
      [fields]="formFields"
      (onSubmit)="handleSubmit($event)">
    </abp-dynamic-form>

    <button (click)="myForm.resetForm()">Reset Form</button>
  `,
})
export class MyComponent {
  formFields: FormFieldConfig[] = [
    // ... field configurations
  ];

  handleSubmit(formValue: any) {
    console.log(formValue);
  }
}
```

### Available Methods

- `resetForm()` - Resets the form to its initial state
- `submit()` - Programmatically submit the form

## Custom Action Buttons

You can customize the action buttons by projecting your own content:

```ts
@Component({
  template: `
    <abp-dynamic-form
      [fields]="formFields"
      (onSubmit)="handleSubmit($event)">

      <div actions class="form-actions">
        <button type="button" class="btn btn-secondary" (click)="handleCancel()">
          Cancel
        </button>
        <button type="submit" class="btn btn-success">
          Save Changes
        </button>
        <button type="button" class="btn btn-info" (click)="handleDraft()">
          Save as Draft
        </button>
      </div>
    </abp-dynamic-form>
  `,
})
export class MyComponent {
  formFields: FormFieldConfig[] = [
    // ... field configurations
  ];

  handleSubmit(formValue: any) {
    console.log('Form submitted:', formValue);
  }

  handleCancel() {
    console.log('Cancelled');
  }

  handleDraft() {
    console.log('Saved as draft');
  }
}
```

## Complete Example

Here's a complete example demonstrating various features:

```ts
import { Component } from '@angular/core';
import { DynamicFormComponent } from '@abp/ng.components/dynamic-form';
import { FormFieldConfig } from '@abp/ng.components/dynamic-form';

@Component({
  selector: 'app-employee-form',
  imports: [DynamicFormComponent],
  template: `
    <div class="container">
      <h2>Employee Registration</h2>
      <abp-dynamic-form
        #employeeForm="abpDynamicForm"
        [fields]="formFields"
        [submitButtonText]="'Register Employee'"
        [showCancelButton]="true"
        [submitInProgress]="isSubmitting"
        (onSubmit)="handleSubmit($event)"
        (formCancel)="handleCancel()">
      </abp-dynamic-form>
    </div>
  `,
})
export class EmployeeFormComponent {
  isSubmitting = false;

  formFields: FormFieldConfig[] = [
    // Personal Information
    {
      key: 'firstName',
      type: 'text',
      label: 'First Name',
      placeholder: 'Enter first name',
      required: true,
      gridSize: 6,
      order: 1,
      validators: [
        { type: 'required', message: 'First name is required' },
        { type: 'minLength', value: 2, message: 'First name must be at least 2 characters' },
      ],
    },
    {
      key: 'lastName',
      type: 'text',
      label: 'Last Name',
      placeholder: 'Enter last name',
      required: true,
      gridSize: 6,
      order: 2,
      validators: [
        { type: 'required', message: 'Last name is required' },
        { type: 'minLength', value: 2, message: 'Last name must be at least 2 characters' },
      ],
    },
    {
      key: 'email',
      type: 'email',
      label: 'Email',
      placeholder: 'Enter email address',
      required: true,
      gridSize: 6,
      order: 3,
      validators: [
        { type: 'required', message: 'Email is required' },
        { type: 'email', message: 'Please enter a valid email address' },
      ],
    },
    {
      key: 'phoneNumber',
      type: 'text',
      label: 'Phone Number',
      placeholder: 'Enter phone number',
      gridSize: 6,
      order: 4,
    },

    // Employment Details
    {
      key: 'department',
      type: 'select',
      label: 'Department',
      required: true,
      gridSize: 6,
      order: 5,
      options: {
        defaultValues: [
          { id: 1, name: 'Engineering' },
          { id: 2, name: 'Marketing' },
          { id: 3, name: 'Sales' },
          { id: 4, name: 'Human Resources' },
        ],
        valueProp: 'id',
        labelProp: 'name',
      },
      validators: [
        { type: 'required', message: 'Department is required' },
      ],
    },
    {
      key: 'position',
      type: 'text',
      label: 'Position',
      placeholder: 'Enter position',
      required: true,
      gridSize: 6,
      order: 6,
      validators: [
        { type: 'required', message: 'Position is required' },
      ],
    },
    {
      key: 'startDate',
      type: 'date',
      label: 'Start Date',
      required: true,
      gridSize: 6,
      order: 7,
      validators: [
        { type: 'required', message: 'Start date is required' },
      ],
    },

    // Conditional Fields
    {
      key: 'isManager',
      type: 'checkbox',
      label: 'Is this person a manager?',
      gridSize: 12,
      order: 8,
    },
    {
      key: 'teamSize',
      type: 'number',
      label: 'Team Size',
      placeholder: 'Number of team members',
      gridSize: 6,
      order: 9,
      conditionalLogic: [
        {
          dependsOn: 'isManager',
          condition: 'equals',
          value: true,
          action: 'show',
        },
      ],
      validators: [
        { type: 'min', value: 1, message: 'Team size must be at least 1' },
      ],
    },
    {
      key: 'managementExperience',
      type: 'textarea',
      label: 'Management Experience',
      placeholder: 'Describe your management experience',
      gridSize: 12,
      order: 10,
      conditionalLogic: [
        {
          dependsOn: 'isManager',
          condition: 'equals',
          value: true,
          action: 'show',
        },
      ],
    },

    // Additional Information
    {
      key: 'notes',
      type: 'textarea',
      label: 'Additional Notes',
      placeholder: 'Any additional information',
      gridSize: 12,
      order: 11,
    },
  ];

  handleSubmit(formValue: any) {
    this.isSubmitting = true;

    console.log('Employee Data:', formValue);

    // Simulate API call
    setTimeout(() => {
      this.isSubmitting = false;
      alert('Employee registered successfully!');
    }, 2000);
  }

  handleCancel() {
    if (confirm('Are you sure you want to cancel?')) {
      // Navigate back or reset form
      console.log('Form cancelled');
    }
  }
}
```

## API Reference

### DynamicFormComponent

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `dynamicForm` | `FormGroup` | The underlying Angular FormGroup instance |
| `fieldVisibility` | `{ [key: string]: boolean }` | Object tracking field visibility state |

#### Methods

| Method | Parameters | Returns | Description |
|--------|-----------|---------|-------------|
| `submit()` | - | `void` | Submits the form if valid |
| `onCancel()` | - | `void` | Emits the formCancel event |
| `resetForm()` | - | `void` | Resets the form to initial values |
| `isFieldVisible(field)` | `FormFieldConfig` | `boolean` | Checks if a field is currently visible |

### DynamicFormService

The `DynamicFormService` provides utility methods for form management:

#### Methods

| Method | Parameters | Returns | Description |
|--------|-----------|---------|-------------|
| `createFormGroup(fields)` | `FormFieldConfig[]` | `FormGroup` | Creates a FormGroup from field configurations |
| `getInitialValues(fields)` | `FormFieldConfig[]` | `any` | Extracts initial values from field configurations |
| `getOptions(url, apiName?)` | `string, string?` | `Observable<any[]>` | Fetches options from an API endpoint |

## See Also

- [Form Validation](./form-validation.md)
- [Form Input Component](./form-input-component.md)
- [Dynamic Form Extensions](./dynamic-form-extensions.md)