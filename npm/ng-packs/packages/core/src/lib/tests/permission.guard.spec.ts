import { APP_BASE_HREF } from '@angular/common';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { Component } from '@angular/core';
import { provideRouter, Route, Router, RouterModule } from '@angular/router';
import {
  createServiceFactory,
  createSpyObject,
  SpectatorService,
  SpyObject,
} from '@ngneat/spectator/jest';
import { of } from 'rxjs';
import { permissionGuard, PermissionGuard } from '../guards/permission.guard';
import { HttpErrorReporterService } from '../services/http-error-reporter.service';
import { PermissionService } from '../services/permission.service';
import { RoutesService } from '../services/routes.service';
import { CORE_OPTIONS } from '../tokens/options.token';
import { TestBed } from '@angular/core/testing';
import { RouterTestingHarness } from '@angular/router/testing';
import { OTHERS_GROUP } from '../tokens';
import { SORT_COMPARE_FUNC, compareFuncFactory } from '../tokens/compare-func.token';
import { AuthService } from '../abstracts';

describe('PermissionGuard', () => {
  beforeAll(() => {
    jest.setTimeout(60000);
  });

  let spectator: SpectatorService<PermissionGuard>;
  let guard: PermissionGuard;
  let routes: SpyObject<RoutesService>;
  let httpErrorReporter: SpyObject<HttpErrorReporterService>;
  let permissionService: SpyObject<PermissionService>;

  const mockOAuthService = {
    isAuthenticated: true,
  };

  @Component({ template: '', standalone: true })
  class DummyComponent {}

  const createService = createServiceFactory({
    service: PermissionGuard,
    mocks: [PermissionService],
    imports: [
      HttpClientTestingModule,
      RouterModule.forRoot([
        {
          path: 'test',
          component: DummyComponent,
          data: {
            requiredPolicy: 'TestPolicy',
          },
        },
      ]),
      DummyComponent,
    ],
    providers: [
      {
        provide: APP_BASE_HREF,
        useValue: '/',
      },
      { provide: AuthService, useValue: mockOAuthService },
      { 
        provide: CORE_OPTIONS, 
        useValue: { 
          skipGetAppConfiguration: true,
          environment: { remoteEnv: {} }
        } 
      },
      { provide: OTHERS_GROUP, useValue: 'AbpUi::OthersGroup' },
      { provide: SORT_COMPARE_FUNC, useValue: compareFuncFactory },
    ],
  });

  beforeEach(() => {
    spectator = createService();
    guard = spectator.service;
    routes = spectator.inject(RoutesService);
    httpErrorReporter = spectator.inject(HttpErrorReporterService);
    permissionService = spectator.inject(PermissionService);
  });

  it('should return true when the grantedPolicy is true', done => {
    permissionService.getGrantedPolicy$.andReturn(of(true));
    const spy = jest.spyOn(httpErrorReporter, 'reportError');
    guard.canActivate({ data: { requiredPolicy: 'test' } } as any, null).subscribe(res => {
      expect(res).toBe(true);
      expect(spy.mock.calls).toHaveLength(0);
      done();
    });
  });

  it('should not emit value when the grantedPolicy is false', done => {
    permissionService.getGrantedPolicy$.andReturn(of(false));
    const spy = jest.spyOn(httpErrorReporter, 'reportError');
    
    guard.canActivate({ data: { requiredPolicy: 'test' } } as any, null).subscribe({
      next: res => {
        done.fail('Should not emit value when policy is false');
      },
      error: err => {
        console.error('Test error:', err);
        done.fail(err);
      },
      complete: () => {
        expect(spy).not.toHaveBeenCalled();
        done();
      }
    });
  }, 30000);

  it('should check the requiredPolicy from RoutesService', done => {
    routes.add([
      {
        path: '/test',
        name: 'Test',
        requiredPolicy: 'TestPolicy',
      },
    ]);
    permissionService.getGrantedPolicy$.mockImplementation(policy => of(policy === 'TestPolicy'));
    guard.canActivate({ data: {} } as any, { url: 'test' } as any).subscribe(result => {
      expect(result).toBe(true);
      done();
    });
  });

  it('should return Observable<true> if RoutesService does not have requiredPolicy for given URL', done => {
    routes.add([
      {
        path: '/test',
        name: 'Test',
      },
    ]);
    guard.canActivate({ data: {} } as any, { url: 'test' } as any).subscribe(result => {
      expect(result).toBe(true);
      done();
    });
  });

  it('should not report error when user is not authenticated', done => {
    permissionService.getGrantedPolicy$.andReturn(of(false));
    const spy = jest.spyOn(httpErrorReporter, 'reportError');
    
    const mockAuthService = { isAuthenticated: false };
    (guard as any).authService = mockAuthService;
    
    guard.canActivate({ data: { requiredPolicy: 'test' } } as any, null).subscribe({
      next: res => {
        done.fail('Should not emit value when policy is false');
      },
      complete: () => {
        expect(spy).not.toHaveBeenCalled();
        done();
      }
    });
  });

  it('should handle route data with requiredPolicy', done => {
    permissionService.getGrantedPolicy$.andReturn(of(true));
    
    guard.canActivate({ data: { requiredPolicy: 'CustomPolicy' } } as any, null).subscribe(result => {
      expect(result).toBe(true);
      expect(permissionService.getGrantedPolicy$).toHaveBeenCalledWith('CustomPolicy');
      done();
    });
  });
});

@Component({ standalone: true, template: '' })
class DummyComponent {}
describe('authGuard', () => {
  let permissionService: SpyObject<PermissionService>;
  let httpErrorReporter: SpyObject<HttpErrorReporterService>;

  const mockOAuthService = {
    isAuthenticated: true,
  };

  const routes: Route[] = [
    {
      path: 'dummy',
      component: DummyComponent,
      canActivate: [permissionGuard],
      data: {
        requiredPolicy: 'TestPolicy',
      },
    },
    {
      path: 'zibzib',
      component: DummyComponent,
      canActivate: [permissionGuard],
    },
  ];

  beforeEach(() => {
    httpErrorReporter = createSpyObject(HttpErrorReporterService);
    permissionService = createSpyObject(PermissionService);

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        { provide: AuthService, useValue: mockOAuthService },
        { provide: PermissionService, useValue: permissionService },
        { provide: HttpErrorReporterService, useValue: httpErrorReporter },
        { 
          provide: CORE_OPTIONS, 
          useValue: { 
            skipGetAppConfiguration: true,
            environment: { remoteEnv: {} }
          } 
        },
        { provide: OTHERS_GROUP, useValue: 'AbpUi::OthersGroup' },
        { provide: SORT_COMPARE_FUNC, useValue: compareFuncFactory },
        provideRouter(routes),
      ],
    });
  });

  it('should return true when the grantedPolicy is true', async () => {
    permissionService.getGrantedPolicy$.andReturn(of(true));
    await RouterTestingHarness.create('/dummy');

    expect(TestBed.inject(Router).url).toEqual('/dummy');
    expect(httpErrorReporter.reportError).not.toHaveBeenCalled();
  });

  it('should not emit value and report an error when the grantedPolicy is false', async () => {
    permissionService.getGrantedPolicy$.andReturn(of(false));
    
    const guard = TestBed.inject(PermissionGuard);
    const spy = jest.spyOn(httpErrorReporter, 'reportError');
    
    guard.canActivate({ data: { requiredPolicy: 'test' } } as any, null).subscribe({
      next: res => {
        fail('Should not emit value when policy is false');
      },
      complete: () => {
        expect(spy).toHaveBeenCalledWith({ status: 403 });
      }
    });
  });

  it('should check the requiredPolicy from RoutesService', async () => {
    permissionService.getGrantedPolicy$.mockImplementation(policy => {
      return of(policy === 'TestPolicy');
    });
    await RouterTestingHarness.create('/dummy');

    expect(TestBed.inject(Router).url).toEqual('/dummy');
    expect(httpErrorReporter.reportError).not.toHaveBeenCalled();
  });

  it('should return Observable<true> if RoutesService does not have requiredPolicy for given URL', async () => {
    await RouterTestingHarness.create('/zibzib');
    expect(TestBed.inject(Router).url).toEqual('/zibzib');
  });
});
