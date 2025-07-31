import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { LocalizationService } from '../services/localization.service';
import { ConfigStateService } from '../services/config-state.service';
import { SessionStateService } from '../services/session-state.service';
import { CORE_OPTIONS } from '../tokens/options.token';
import { ABP } from '../models/common';

describe('LocalizationService', () => {
  let service: LocalizationService;
  let sessionState: SessionStateService;
  let configState: ConfigStateService;

  const mockLocalizationData = {
    defaultResourceName: 'MyProjectName',
    values: {
      MyProjectName: {
        'Welcome': 'Welcome',
        'Hello {0}': 'Hello {0}',
      },
      AbpIdentity: {
        'Identity': 'Identity',
        'User': 'User',
      },
    },
  };

  const mockLocalizationsMap = new Map<string, Record<string, string>>();
  mockLocalizationsMap.set('AbpIdentity', { 'Identity': 'Identity', 'User': 'User' });
  mockLocalizationsMap.set('MyProjectName', { 'Welcome': 'Welcome', 'Hello {0}': 'Hello {0}' });

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        LocalizationService,
        {
          provide: CORE_OPTIONS,
          useValue: { 
            registerLocaleFn: () => Promise.resolve(), 
            cultureNameLocaleFileMap: {} 
          },
        },
        {
          provide: ConfigStateService,
          useValue: {
            refreshAppState: jest.fn(),
            getDeep: jest.fn().mockReturnValue({
              localization: {
                currentCulture: { cultureName: 'tr' },
                defaultResourceName: 'MyProjectName',
                values: mockLocalizationData.values,
              },
            }),
            getDeep$: jest.fn().mockReturnValue(of({
              localization: {
                currentCulture: { cultureName: 'tr' },
                defaultResourceName: 'MyProjectName',
                values: mockLocalizationData.values,
              },
            })),
            getOne: jest.fn().mockReturnValue(mockLocalizationData),
            getOne$: jest.fn().mockReturnValue(of(mockLocalizationData)),
            getAll: jest.fn().mockReturnValue({
              localization: mockLocalizationData,
            }),
            getAll$: jest.fn().mockReturnValue(of({
              localization: mockLocalizationData,
            })),
            refreshLocalization: jest.fn().mockReturnValue(of({})),
          },
        },
        {
          provide: SessionStateService,
          useValue: {
            setLanguage: jest.fn(),
            setTenant: jest.fn(),
            setInitialLanguage: jest.fn(),
            getLanguage: jest.fn().mockReturnValue('tr'),
            getTenant: jest.fn().mockReturnValue(null),
            getInitialLanguage: jest.fn().mockReturnValue('tr'),
            onLanguageChange$: jest.fn().mockReturnValue(of('tr')),
            getLanguage$: jest.fn().mockReturnValue(of('tr')),
          },
        },
      ],
    });
    service = TestBed.inject(LocalizationService);
    sessionState = TestBed.inject(SessionStateService);
    configState = TestBed.inject(ConfigStateService);

    (service as any).localizations$.next(mockLocalizationsMap);
  });

  describe('#currentLang', () => {
    it('should return current language', () => {
      expect(service.currentLang).toBe('tr');
    });

    it('should return observable of current language', (done) => {
      service.currentLang$.subscribe(lang => {
        expect(lang).toBe('tr');
        done();
      });
    });
  });

  describe('#languageChange$', () => {
    it('should emit language changes', (done) => {
      service.languageChange$.subscribe(lang => {
        expect(lang).toBe('tr');
        done();
      });
      
      (service as any)._languageChange$.next('tr');
    });
  });

  describe('#get', () => {
    it('should return observable localization for valid key', (done) => {
      service.get('AbpIdentity::Identity').subscribe(result => {
        expect(result).toBe('Identity');
        done();
      });
    });

    it('should return key when localization not found', (done) => {
      service.get('AbpIdentity::NonExistent').subscribe(result => {
        expect(result).toBe('NonExistent');
        done();
      });
    });

    it('should handle interpolation', (done) => {
      service.get('MyProjectName::Hello {0}', 'John').subscribe(result => {
        expect(result).toBe('Hello John');
        done();
      });
    });
  });

  describe('#instant', () => {
    it('should return localization for valid key', () => {
      const result = service.instant('AbpIdentity::Identity');
      expect(result).toBe('Identity');
    });

    it('should return key when localization not found', () => {
      const result = service.instant('AbpIdentity::NonExistent');
      expect(result).toBe('NonExistent');
    });

    it('should handle interpolation', () => {
      const result = service.instant('MyProjectName::Hello {0}', 'John');
      expect(result).toBe('Hello John');
    });
  });

  describe('#localize', () => {
    it('should return observable localization for valid resource and key', (done) => {
      service.localize('AbpIdentity', 'Identity', 'Default').subscribe(result => {
        expect(result).toBe('Identity');
        done();
      });
    });

    it('should return default value when key not found', (done) => {
      service.localize('AbpIdentity', 'NonExistent', 'Default').subscribe(result => {
        expect(result).toBe('Default');
        done();
      });
    });

    it('should return default value when resource not found', (done) => {
      service.localize('NonExistent', 'Identity', 'Default').subscribe(result => {
        expect(result).toBe('Default');
        done();
      });
    });
  });

  describe('#localizeSync', () => {
    it('should return localization for valid resource and key', () => {
      const result = service.localizeSync('AbpIdentity', 'Identity', 'Default');
      expect(result).toBe('Identity');
    });

    it('should return default value when key not found', () => {
      const result = service.localizeSync('AbpIdentity', 'NonExistent', 'Default');
      expect(result).toBe('Default');
    });

    it('should return default value when resource not found', () => {
      const result = service.localizeSync('NonExistent', 'Identity', 'Default');
      expect(result).toBe('Default');
    });
  });

  describe('#localizeWithFallback', () => {
    it('should return observable localization from first available resource', (done) => {
      service.localizeWithFallback(['AbpIdentity', 'MyProjectName'], ['Identity'], 'Default').subscribe(result => {
        expect(result).toBe('Identity');
        done();
      });
    });

    it('should return default value when no resource has the key', (done) => {
      service.localizeWithFallback(['AbpIdentity', 'MyProjectName'], ['NonExistent'], 'Default').subscribe(result => {
        expect(result).toBe('Default');
        done();
      });
    });
  });

  describe('#localizeWithFallbackSync', () => {
    it('should return localization from first available resource', () => {
      const result = service.localizeWithFallbackSync(['AbpIdentity', 'MyProjectName'], ['Identity'], 'Default');
      expect(result).toBe('Identity');
    });

    it('should return default value when no resource has the key', () => {
      const result = service.localizeWithFallbackSync(['AbpIdentity', 'MyProjectName'], ['NonExistent'], 'Default');
      expect(result).toBe('Default');
    });
  });

  describe('#getResource', () => {
    it('should return resource for valid resource name', () => {
      const resource = service.getResource('AbpIdentity');
      expect(resource).toBeDefined();
      expect(resource?.['Identity']).toBe('Identity');
    });

    it('should return undefined for non-existent resource', () => {
      const resource = service.getResource('NonExistent');
      expect(resource).toBeUndefined();
    });
  });

  describe('#getResource$', () => {
    it('should return observable resource for valid resource name', (done) => {
      service.getResource$('AbpIdentity').subscribe(resource => {
        expect(resource).toBeDefined();
        expect(resource?.['Identity']).toBe('Identity');
        done();
      });
    });

    it('should return observable undefined for non-existent resource', (done) => {
      service.getResource$('NonExistent').subscribe(resource => {
        expect(resource).toBeUndefined();
        done();
      });
    });
  });

  describe('#addLocalization', () => {
    it('should add localization data', () => {
      const localizations: ABP.Localization[] = [
        {
          culture: 'en',
          resources: [
            {
              resourceName: 'TestResource',
              texts: {
                'TestKey': 'TestValue',
              },
            },
          ],
        },
      ];

      service.addLocalization(localizations);
      
      expect(() => service.addLocalization(localizations)).not.toThrow();
    });
  });

  describe('#registerLocale', () => {
    it('should register locale successfully', async () => {
      const result = await service.registerLocale('en');
      expect(result).toBeUndefined();
    });
  });
});
