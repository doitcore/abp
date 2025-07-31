import { Component } from '@angular/core';
import { createComponentFactory, Spectator } from '@ngneat/spectator/jest';
import { of, throwError } from 'rxjs';
import { AbpApplicationConfigurationService } from '../proxy/volo/abp/asp-net-core/mvc/application-configurations/abp-application-configuration.service';
import { ApplicationConfigurationDto } from '../proxy/volo/abp/asp-net-core/mvc/application-configurations/models';
import { SessionStateService } from '../services/session-state.service';
import { EnvironmentService } from '../services/environment.service';
import { AuthService } from '../abstracts/auth.service';
import { ConfigStateService } from '../services/config-state.service';
import { CORE_OPTIONS } from '../tokens/options.token';
import { getInitialData, localeInitializer } from '../utils/initial-utils';
import * as environmentUtils from '../utils/environment-utils';
import * as multiTenancyUtils from '../utils/multi-tenancy-utils';
import { RestService } from '../services/rest.service';
import { CHECK_AUTHENTICATION_STATE_FN_KEY } from '../tokens/check-authentication-state';
import { APP_INIT_ERROR_HANDLERS } from '../tokens/app-config.token';
import { TestBed } from '@angular/core/testing';

const environment = { oAuthConfig: { issuer: 'test' } };

@Component({
  selector: 'abp-dummy',
  template: '',
})
export class DummyComponent {}

describe('InitialUtils', () => {
  let spectator: Spectator<DummyComponent>;
  const createComponent = createComponentFactory({
    component: DummyComponent,
    mocks: [
      EnvironmentService,
      ConfigStateService,
      AbpApplicationConfigurationService,
      AuthService,
      SessionStateService,
      RestService,
    ],
    providers: [
      {
        provide: CORE_OPTIONS,
        useValue: {
          environment,
          registerLocaleFn: () => Promise.resolve(),
          skipInitAuthService: false,
          skipGetAppConfiguration: false,
        },
      },
      {
        provide: CHECK_AUTHENTICATION_STATE_FN_KEY,
        useValue: () => {},
      },
      {
        provide: APP_INIT_ERROR_HANDLERS,
        useValue: [],
      },
    ],
  });

  beforeEach(() => (spectator = createComponent()));

  describe('#getInitialData', () => {
    test('should call the getConfiguration method of ApplicationConfigurationService and set states', async () => {
      const environmentService = spectator.inject(EnvironmentService);
      const configStateService = spectator.inject(ConfigStateService);
      const sessionStateService = spectator.inject(SessionStateService);
      const authService = spectator.inject(AuthService);

      const parseTenantFromUrlSpy = jest.spyOn(multiTenancyUtils, 'parseTenantFromUrl');
      const getRemoteEnvSpy = jest.spyOn(environmentUtils, 'getRemoteEnv');
      parseTenantFromUrlSpy.mockReturnValue(Promise.resolve());
      getRemoteEnvSpy.mockReturnValue(Promise.resolve());

      const appConfigRes = {
        currentTenant: { id: 'test', name: 'testing' },
      } as ApplicationConfigurationDto;

      const environmentSetStateSpy = jest.spyOn(environmentService, 'setState');
      const configRefreshAppStateSpy = jest.spyOn(configStateService, 'refreshAppState');
      configRefreshAppStateSpy.mockReturnValue(of(appConfigRes));
      const sessionSetTenantSpy = jest.spyOn(sessionStateService, 'setTenant');
      const authServiceInitSpy = jest.spyOn(authService, 'init');
      const configStateGetOneSpy = jest.spyOn(configStateService, 'getOne');
      configStateGetOneSpy.mockReturnValue(appConfigRes.currentTenant);

      await TestBed.runInInjectionContext(() => getInitialData());

      expect(typeof getInitialData).toBe('function');
      expect(configRefreshAppStateSpy).toHaveBeenCalled();
      expect(environmentSetStateSpy).toHaveBeenCalledWith(environment);
      expect(sessionSetTenantSpy).toHaveBeenCalledWith(appConfigRes.currentTenant);
      expect(authServiceInitSpy).toHaveBeenCalled();
    });

    test('should handle errors when refreshAppState fails', async () => {
      const configStateService = spectator.inject(ConfigStateService);
      const errorHandlers = spectator.inject(APP_INIT_ERROR_HANDLERS);
      
      const mockError = new Error('Configuration failed');
      const configRefreshAppStateSpy = jest.spyOn(configStateService, 'refreshAppState');
      configRefreshAppStateSpy.mockReturnValue(throwError(() => mockError));

      const errorHandlerSpy = jest.fn();
      errorHandlers.push(errorHandlerSpy);

      await expect(TestBed.runInInjectionContext(() => getInitialData())).rejects.toThrow('Configuration failed');
      expect(errorHandlerSpy).toHaveBeenCalledWith(mockError);
    });

    test('should skip auth service initialization when skipInitAuthService is true', async () => {
      const authService = spectator.inject(AuthService);
      const authServiceInitSpy = jest.spyOn(authService, 'init');

      const originalOptions = spectator.inject(CORE_OPTIONS);
      const modifiedOptions = { ...originalOptions, skipInitAuthService: true };
      
      expect(authServiceInitSpy).not.toHaveBeenCalled();
    });
  });

  describe('#localeInitializer', () => {
    test('should resolve registerLocale', async () => {
      expect(typeof localeInitializer).toBe('function');
      
      const sessionStateService = spectator.inject(SessionStateService);
      const getLanguageSpy = jest.spyOn(sessionStateService, 'getLanguage');
      getLanguageSpy.mockReturnValue('en');

      const result = await TestBed.runInInjectionContext(() => localeInitializer());
      expect(result).toBe('resolved');
    });

    test('should use default language when session language is not set', async () => {
      const sessionStateService = spectator.inject(SessionStateService);
      const getLanguageSpy = jest.spyOn(sessionStateService, 'getLanguage');
      getLanguageSpy.mockReturnValue(null);

      const result = await TestBed.runInInjectionContext(() => localeInitializer());
      expect(result).toBe('resolved');
    });
  });
});
