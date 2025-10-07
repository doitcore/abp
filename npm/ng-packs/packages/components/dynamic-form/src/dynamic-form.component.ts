import { ChangeDetectionStrategy, Component, input, output, inject } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { DynamicFormService } from './dynamic-form.service';
import { FormFieldConfig } from './dynamic-form.models';
import { DynamicFormFieldComponent } from './dynamic-form-field';

@Component({
  selector: 'abp-dynamic-form',
  template: ``,
  changeDetection: ChangeDetectionStrategy.OnPush
})

export class DynamicFormComponent {
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
