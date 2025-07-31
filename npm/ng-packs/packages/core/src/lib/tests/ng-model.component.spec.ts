import { Component, Input, OnInit } from '@angular/core';
import { FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';
import { createHostFactory, SpectatorHost } from '@ngneat/spectator/jest';
import { timer } from 'rxjs';
import { AbstractNgModelComponent } from '../abstracts';

@Component({
  selector: 'abp-test',
  template: '',
  standalone: true,
  imports: [AbstractNgModelComponent],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: TestComponent,
      multi: true,
    },
  ],
})
export class TestComponent extends AbstractNgModelComponent implements OnInit {
  @Input() override: boolean;
  @Input() testValueFn: (value: any, previousValue?: any) => any;
  @Input() testValueLimitFn: (value: any, previousValue?: any) => any;

  ngOnInit() {
    if (this.testValueFn) {
      this.valueFn = this.testValueFn;
    }
    if (this.testValueLimitFn) {
      this.valueLimitFn = this.testValueLimitFn;
    }
    
    setTimeout(() => {
      if (this.override) {
        this.value = 'test';
      }
    }, 0);
  }
}

describe('AbstractNgModelComponent', () => {
  let spectator: SpectatorHost<TestComponent, { val: any; override: boolean; testValueFn: any; testValueLimitFn: any }>;

  const createHost = createHostFactory({
    component: TestComponent,
    imports: [FormsModule],
  });

  beforeEach(() => {
    spectator = createHost('<abp-test [(ngModel)]="val" [override]="override" [testValueFn]="testValueFn" [testValueLimitFn]="testValueLimitFn"></abp-test>', {
      hostProps: {
        val: '1',
        override: false,
        testValueFn: undefined,
        testValueLimitFn: undefined,
      },
    });
  });

  test('should pass the value with ngModel', done => {
    timer(0).subscribe(() => {
      expect(spectator.component.value).toBe('1');
      done();
    });
  });

  test('should set the value with ngModel', done => {
    spectator.setHostInput({ val: '2', override: true });

    timer(0).subscribe(() => {
      expect(spectator.hostComponent.val).toBe('test');
      done();
    });
  });

  test('should handle valueFn input', () => {
    const valueFn = jest.fn((value: any) => value + '_transformed');
    spectator.component.valueFn = valueFn;
    spectator.component.value = 'original';
    
    expect(valueFn).toHaveBeenCalledWith('original', '1');
    expect(spectator.component.value).toBe('original_transformed');
  });

  test('should handle valueLimitFn input', () => {
    const valueLimitFn = jest.fn((value: any) => value === 'blocked' ? false : value);
    spectator.component.valueLimitFn = valueLimitFn;
    spectator.component.value = 'allowed';
    
    expect(valueLimitFn).toHaveBeenCalledWith('allowed', '1');
    expect(spectator.component.value).toBe('1');
  });

  test('should block value when valueLimitFn returns false', () => {
    const valueLimitFn = jest.fn((value: any) => value === 'blocked' ? false : value);
    spectator.component.valueLimitFn = valueLimitFn;
    const originalValue = spectator.component.value;
    spectator.component.value = 'blocked';
    
    expect(valueLimitFn).toHaveBeenCalledWith('blocked', originalValue);
    expect(spectator.component.value).toBe('blocked');
  });

  test('should handle disabled state', () => {
    spectator.component.setDisabledState(true);
    expect(spectator.component.disabled).toBe(true);
  });

  test('should handle readonly state', () => {
    spectator.component.readonly = true;
    const originalValue = spectator.component.value;
    spectator.component.value = 'new_value';

    expect(spectator.component.value).toBe(originalValue);
  });

  test('should register onChange callback', () => {
    const onChangeSpy = jest.fn();
    spectator.component.registerOnChange(onChangeSpy);
    
    spectator.component.value = 'new_value';
    expect(onChangeSpy).toHaveBeenCalledWith('new_value');
  });

  test('should register onTouched callback', () => {
    const onTouchedSpy = jest.fn();
    spectator.component.registerOnTouched(onTouchedSpy);
    
    expect(spectator.component.onTouched).toBe(onTouchedSpy);
  });

  test('should notify value change', () => {
    const onChangeSpy = jest.fn();
    spectator.component.registerOnChange(onChangeSpy);
    
    spectator.component.notifyValueChange();
    expect(onChangeSpy).toHaveBeenCalledWith(spectator.component.value);
  });

  test('should write value correctly', () => {
    const valueLimitFn = jest.fn((value: any) => value);
    spectator.component.valueLimitFn = valueLimitFn;
    
    spectator.component.writeValue('new_written_value');
    expect(valueLimitFn).toHaveBeenCalledWith('new_written_value', '1');
    expect(spectator.component.value).toBe('new_written_value');
  });

  test('should handle default value', () => {
    (spectator.component as any)._value = undefined;
    expect(spectator.component.value).toBe(undefined);
    expect(spectator.component.defaultValue).toBe(undefined);
  });
});
