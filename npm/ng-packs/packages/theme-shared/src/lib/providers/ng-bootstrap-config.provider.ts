import { inject, provideAppInitializer } from '@angular/core';
import { NgbDatepickerI18n, NgbInputDatepickerConfig, NgbTypeaheadConfig } from '@ng-bootstrap/ng-bootstrap';
import { DatepickerI18nAdapter } from '../adapters';

export const NG_BOOTSTRAP_CONFIG_PROVIDERS = [
  {
    provide: NgbDatepickerI18n,
    useClass: DatepickerI18nAdapter,
  },
  provideAppInitializer(() => {
    configureNgBootstrap();
  }),
];

export function configureNgBootstrap() {
  const datepicker: NgbInputDatepickerConfig = inject(NgbInputDatepickerConfig);
  const typeahead: NgbTypeaheadConfig = inject(NgbTypeaheadConfig);
  datepicker.container = 'body';
  typeahead.container = 'body';
}
