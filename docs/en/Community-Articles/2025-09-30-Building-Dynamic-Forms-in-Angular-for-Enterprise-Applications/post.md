# Building Dynamic Forms in Angular for Enterprise Applications

## Introduction

Dynamic forms are useful for enterprise applications where form structures need to be flexible, configurable, and generated at runtime based on business requirements. This approach allows developers to create forms from configuration objects rather than hardcoding them, enabling greater flexibility and maintainability.

## Benefits

1. **Flexibility**: Forms can be easily modified without changing the code.
2. **Reusability**: Form components can be shared across components.
3. **Maintainability**: Changes to form structures can be managed through configuration files or databases.
4. **Scalability**: New form fields and types can be added without significant code changes.
4. **User Experience**: Dynamic forms can adapt to user roles and permissions, providing a tailored experience.

## Architecture

### 1. Form Configuration Model

We define a model to represent the form configuration. This model includes field types, labels, validation rules, and other metadata.

```typescript
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
    gridSize?: number; // For layout purposes, e.g., Bootstrap grid size (1-12)
}

export interface ValidatorConfig {
    type: 'required' | 'email' | 'minLength' | 'maxLength' | 'pattern' | 'custom';
    value?: any;
    message: string;
}

// Conditional logic to show/hide or enable/disable fields based on other field values
export interface ConditionalRule {
    dependsOn: string;
    condition: 'equals' | 'notEquals' | 'contains' | 'greaterThan' | 'lessThan';
    value: any;
    action: 'show' | 'hide' | 'enable' | 'disable';
}

```
### 2. Dynamic Form Service

A service to handle form creation and validation processes.

```typescript
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

```

### 3. Dynamic Form Component

```typescript
@Component({
    selector: 'app-dynamic-form',
    template: `
    <form [formGroup]="dynamicForm" (ngSubmit)="onSubmit()" class="dynamic-form">
      @for (field of sortedFields; track field.key) {
        <div class="row">
          <div [ngClass]="'col-md-' + (field.gridSize || 12)">
            <app-dynamic-form-field
              [field]="field"
              [form]="dynamicForm"
              [isVisible]="isFieldVisible(field)"
              (fieldChange)="onFieldChange($event)">
            </app-dynamic-form-field>
          </div>
        </div>
      }
      <div class="form-actions">
        <button
          type="button"
          class="btn btn-secondary"
          (click)="onCancel()">
          Cancel
        </button>
        <button
          type="submit"
          class="btn btn-primary"
          [disabled]="!dynamicForm.valid || isSubmitting">
          {{ submitButtonText() }}
        </button>
      </div>
    </form>
  `,
    styles: [`
    .dynamic-form {
      display: flex;
      gap: 0.5rem;
      flex-direction: column;
    }
    .form-actions {
      display: flex;
      justify-content: flex-end;
      gap: 0.5rem;
    }
  `],
    imports: [ReactiveFormsModule, CommonModule, DynamicFormFieldComponent],
})
export class DynamicFormComponent implements OnInit {
    fields = input<FormFieldConfig[]>([]);
    submitButtonText = input<string>('Submit');
    formSubmit = output<any>();
    formCancel = output<void>();
    private dynamicFormService = inject(DynamicFormService);

    dynamicForm!: FormGroup;
    isSubmitting = false;
    fieldVisibility: { [key: string]: boolean } = {};

    ngOnInit() {
        this.dynamicForm = this.dynamicFormService.createFormGroup(this.fields());
        this.initializeFieldVisibility();
        this.setupConditionalLogic();
    }

    get sortedFields(): FormFieldConfig[] {
        return this.fields().sort((a, b) => (a.order || 0) - (b.order || 0));
    }

    onSubmit() {
        if (this.dynamicForm.valid) {
            this.isSubmitting = true;
            this.formSubmit.emit(this.dynamicForm.value);
        } else {
            this.markAllFieldsAsTouched();
        }
    }

    onCancel() {
        this.formCancel.emit();
    }

    onFieldChange(event: { fieldKey: string; value: any }) {
        this.evaluateConditionalLogic(event.fieldKey);
    }

    isFieldVisible(field: FormFieldConfig): boolean {
        return this.fieldVisibility[field.key] !== false;
    }

    private initializeFieldVisibility() {
        this.fields().forEach(field => {
            this.fieldVisibility[field.key] = !field.conditionalLogic?.length;
        });
    }

    private setupConditionalLogic() {
        this.fields().forEach(field => {
            if (field.conditionalLogic) {
                field.conditionalLogic.forEach(rule => {
                    const dependentControl = this.dynamicForm.get(rule.dependsOn);
                    if (dependentControl) {
                        dependentControl.valueChanges.subscribe(() => {
                            this.evaluateConditionalLogic(field.key);
                        });
                    }
                });
            }
        });
    }

    private evaluateConditionalLogic(fieldKey: string) {
        const field = this.fields().find(f => f.key === fieldKey);
        if (!field?.conditionalLogic) return;

        field.conditionalLogic.forEach(rule => {
            const dependentValue = this.dynamicForm.get(rule.dependsOn)?.value;
            const conditionMet = this.evaluateCondition(dependentValue, rule.condition, rule.value);

            this.applyConditionalAction(fieldKey, rule.action, conditionMet);
        });
    }

    private evaluateCondition(fieldValue: any, condition: string, ruleValue: any): boolean {
        switch (condition) {
            case 'equals':
                return fieldValue === ruleValue;
            case 'notEquals':
                return fieldValue !== ruleValue;
            case 'contains':
                return fieldValue && fieldValue.includes && fieldValue.includes(ruleValue);
            case 'greaterThan':
                return Number(fieldValue) > Number(ruleValue);
            case 'lessThan':
                return Number(fieldValue) < Number(ruleValue);
            default:
                return false;
        }
    }

    private applyConditionalAction(fieldKey: string, action: string, shouldApply: boolean) {
        const control = this.dynamicForm.get(fieldKey);

        switch (action) {
            case 'show':
                this.fieldVisibility[fieldKey] = shouldApply;
                break;
            case 'hide':
                this.fieldVisibility[fieldKey] = !shouldApply;
                break;
            case 'enable':
                if (control) {
                    shouldApply ? control.enable() : control.disable();
                }
                break;
            case 'disable':
                if (control) {
                    shouldApply ? control.disable() : control.enable();
                }
                break;
        }
    }

    private markAllFieldsAsTouched() {
        Object.keys(this.dynamicForm.controls).forEach(key => {
            this.dynamicForm.get(key)?.markAsTouched();
        });
    }
}
```