import {
  Component,
  ViewChild,
  ViewContainerRef,
  OnChanges,
  SimpleChanges,
  ChangeDetectionStrategy,
  forwardRef,
  Type,
  Injector,
  effect,
  DestroyRef,
  inject,
  input,
  ChangeDetectorRef,
} from '@angular/core';
import {
  ControlValueAccessor, NG_VALUE_ACCESSOR, FormControl, ReactiveFormsModule
} from '@angular/forms';
import { CommonModule } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

type controlValueAccessorLike = Partial<ControlValueAccessor> & { setDisabledState?(d: boolean): void };
type acceptsFormControl = { formControl?: FormControl };

@Component({
  selector: 'abp-dynamic-form-field-host',
  imports: [CommonModule, ReactiveFormsModule],
  template: `<ng-template #vcRef></ng-template>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [{
    provide: NG_VALUE_ACCESSOR,
    useExisting: forwardRef(() => DynamicFieldHostComponent),
    multi: true
  }]
})
export class DynamicFieldHostComponent implements ControlValueAccessor, OnChanges {
  component = input<Type<any>>();
  inputs = input<Record<string, any>>({});

  @ViewChild('vcRef', { read: ViewContainerRef, static: true }) viewContainerRef!: ViewContainerRef;
  private componentRef?: any;

  private lastValue: any;
  private disabled = false;

  // if child has not implemented ControlValueAccessor. Create form control
  private innerControl = new FormControl<any>(null);
  readonly destroyRef = inject(DestroyRef);

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['component']) {
      this.createChild();
    } else if (this.componentRef && changes['inputs']) {
      this.applyInputs();
    }
  }

  private createChild() {
    this.viewContainerRef.clear();
    if (!this.component()) return;

    this.componentRef = this.viewContainerRef.createComponent(this.component());
    this.applyInputs();

    const instance: any = this.componentRef.instance as controlValueAccessorLike & acceptsFormControl;

    if (this.isCVA(instance)) {
      // Child CVA ise wrapper -> child delege
      instance.registerOnChange?.((v: any) => this.onChange(v));
      instance.registerOnTouched?.(() => this.onTouched());
      if (this.disabled && instance.setDisabledState) {
        instance.setDisabledState(true);
      }
      // İlk değeri ilet
      if (this.lastValue !== undefined) {
        instance.writeValue?.(this.lastValue);
      }
    } else {
      // Child CVA değilse, formControl input’u varsa köprüle
      if ('formControl' in instance) {
        instance.formControl = this.innerControl;
        // son değeri uygula
        if (this.lastValue !== undefined) {
          this.innerControl.setValue(this.lastValue, { emitEvent: false });
        }
        this.innerControl.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(v => this.onChange(v));
        this.innerControl.disabled ? null : (this.disabled && this.innerControl.disable({ emitEvent: false }));
      } else {
        // Son çare: valueChange EventEmitter’ı varsa bağla (konvansiyonel)
        if ('valueChange' in instance && instance['valueChange']?.subscribe) {
          instance['valueChange'].pipe(takeUntilDestroyed(this.destroyRef)).subscribe((v: any) => this.onChange(v));
        }
      }
    }
  }

  private applyInputs() {
    if (!this.componentRef) return;
    const inst = this.componentRef.instance;
    for (const [k, v] of Object.entries(this.inputs ?? {})) {
      inst[k] = v;
    }
    this.componentRef.changeDetectorRef?.markForCheck?.();
  }

  private isCVA(obj: any): obj is controlValueAccessorLike {
    return obj && typeof obj.writeValue === 'function' && typeof obj.registerOnChange === 'function';
  }

  /* --------- CVA (wrapper) --------- */

  writeValue(obj: any): void {
    this.lastValue = obj;
    if (!this.componentRef) return;

    const inst: any = this.componentRef.instance as controlValueAccessorLike & acceptsFormControl;

    if (this.isCVA(inst)) {
      inst.writeValue?.(obj);
    } else if ('formControl' in inst && inst.formControl instanceof FormControl) {
      inst.formControl.setValue(obj, { emitEvent: false });
    }
  }

  private onChange: (v: any) => void = () => {};
  private onTouched: () => void = () => {};

  registerOnChange(fn: any): void { this.onChange = fn; }
  registerOnTouched(fn: any): void { this.onTouched = fn; }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
    if (!this.componentRef) return;

    const inst = this.componentRef.instance as controlValueAccessorLike & acceptsFormControl;

    if (this.isCVA(inst) && inst.setDisabledState) {
      inst.setDisabledState(isDisabled);
    } else if ('formControl' in inst && inst.formControl instanceof FormControl) {
      isDisabled ? inst.formControl.disable({ emitEvent: false }) : inst.formControl.enable({ emitEvent: false });
    }
  }
}
