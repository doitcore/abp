import {
  Component, Input, ViewChild, ViewContainerRef, OnChanges, SimpleChanges,
  ChangeDetectionStrategy, forwardRef, Type, Injector, effect, DestroyRef, inject
} from '@angular/core';
import {
  ControlValueAccessor, NG_VALUE_ACCESSOR, FormControl, ReactiveFormsModule
} from '@angular/forms';
import { CommonModule } from '@angular/common';

type MaybeCVA = Partial<ControlValueAccessor> & { setDisabledState?(d: boolean): void };
type WithFormControlInput = { formControl?: FormControl };

@Component({
  selector: 'abp-dynamic-form-field-host',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `<ng-template #vc></ng-template>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [{
    provide: NG_VALUE_ACCESSOR,
    useExisting: forwardRef(() => DynamicFieldHostComponent),
    multi: true
  }]
})
export class DynamicFieldHostComponent implements ControlValueAccessor, OnChanges {
  @Input({ required: true }) component!: Type<any>;
  @Input() inputs: Record<string, any> = {}; // field, visible, vs.

  @ViewChild('vc', { read: ViewContainerRef, static: true }) vc!: ViewContainerRef;

  private compRef?: any;
  private onChange: (v: any) => void = () => {};
  private onTouched: () => void = () => {};
  private lastValue: any;
  private disabled = false;

  // Eğer child CVA değilse kullanmak üzere bir iç kontrol (opsiyonel)
  private innerControl = new FormControl<any>(null);

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['component']) {
      this.createChild();
    } else if (this.compRef && changes['inputs']) {
      this.applyInputs();
    }
  }

  private createChild() {
    this.vc.clear();
    if (!this.component) return;

    this.compRef = this.vc.createComponent(this.component);

    // inputları ata
    this.applyInputs();

    // ÇOCUK TIPINI KONTROL ET: CVA mı?
    const instance: any = this.compRef.instance as MaybeCVA & WithFormControlInput;

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
        this.innerControl.valueChanges.subscribe(v => this.onChange(v));
        this.innerControl.disabled ? null : (this.disabled && this.innerControl.disable({ emitEvent: false }));
      } else {
        // Son çare: valueChange EventEmitter’ı varsa bağla (konvansiyonel)
        if ('valueChange' in instance && instance['valueChange']?.subscribe) {
          instance['valueChange'].subscribe((v: any) => this.onChange(v));
        }
      }
    }
  }

  private applyInputs() {
    if (!this.compRef) return;
    const inst = this.compRef.instance;
    for (const [k, v] of Object.entries(this.inputs ?? {})) {
      inst[k] = v;
    }
    // change detection tetiklenebilir:
    this.compRef.changeDetectorRef?.markForCheck?.();
  }

  private isCVA(obj: any): obj is MaybeCVA {
    return obj && typeof obj.writeValue === 'function' && typeof obj.registerOnChange === 'function';
  }

  /* --------- CVA (wrapper) --------- */

  writeValue(obj: any): void {
    this.lastValue = obj;
    if (!this.compRef) return;

    const inst: any = this.compRef.instance as MaybeCVA & WithFormControlInput;

    if (this.isCVA(inst)) {
      inst.writeValue?.(obj);
    } else if ('formControl' in inst && inst.formControl instanceof FormControl) {
      inst.formControl.setValue(obj, { emitEvent: false });
    }
  }

  registerOnChange(fn: any): void { this.onChange = fn; }
  registerOnTouched(fn: any): void { this.onTouched = fn; }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
    if (!this.compRef) return;

    const inst = this.compRef.instance as MaybeCVA & WithFormControlInput;

    if (this.isCVA(inst) && inst.setDisabledState) {
      inst.setDisabledState(isDisabled);
    } else if ('formControl' in inst && inst.formControl instanceof FormControl) {
      isDisabled ? inst.formControl.disable({ emitEvent: false }) : inst.formControl.enable({ emitEvent: false });
    }
  }
}
