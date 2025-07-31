import { HttpClient } from '@angular/common/http';
import { Component, inject as inject_1 } from '@angular/core';
import { ActivatedRoute, RouterModule, RouterOutlet } from '@angular/router';
import { of, BehaviorSubject } from 'rxjs';
import { createRoutingFactory, SpectatorRouting } from '@ngneat/spectator/jest';
import { DynamicLayoutComponent, RouterOutletComponent } from '../components';
import { eLayoutType } from '../enums/common';
import { ABP } from '../models';
import { AbpApplicationConfigurationService } from '../proxy/volo/abp/asp-net-core/mvc/application-configurations/abp-application-configuration.service';
import { ReplaceableComponentsService, RoutesService, RouterEvents, EnvironmentService, LocalizationService } from '../services';
import { DYNAMIC_LAYOUTS_TOKEN } from '../tokens/dynamic-layout.token';

const mockRoutesService = () => ({
  add: jest.fn(),
  find: jest.fn((predicate) => {

    if (predicate && typeof predicate === 'function') {
      if (predicate({ path: '/parentWithLayout/childWithoutLayout' })) {
        return { layout: eLayoutType.application };
      }
      if (predicate({ path: '/parentWithLayout/childWithLayout' })) {
        return { layout: eLayoutType.account };
      }
      if (predicate({ path: '/withData' })) {
        return { layout: eLayoutType.empty };
      }
      if (predicate({ path: '/withoutLayout' })) {
        return { layout: null };
      }
    }
    return null;
  }),
  search: jest.fn((query) => {

    if (query && query.invisible) {
      return { layout: eLayoutType.account };
    }
    return null;
  }),
  flat$: of([]),
  tree$: of([]),
  visible$: of([]),
});

@Component({
  selector: 'abp-layout-application',
  template: '<router-outlet></router-outlet>',
  standalone: true,
  imports: [RouterOutlet],
})
class DummyApplicationLayoutComponent {}

@Component({
  selector: 'abp-layout-account',
  template: '<router-outlet></router-outlet>',
  standalone: true,
  imports: [RouterOutlet],
})
class DummyAccountLayoutComponent {}

@Component({
  selector: 'abp-layout-empty',
  template: '<router-outlet></router-outlet>',
  standalone: true,
  imports: [RouterOutlet],
})
class DummyEmptyLayoutComponent {}

const LAYOUTS = [
  DummyApplicationLayoutComponent,
  DummyAccountLayoutComponent,
  DummyEmptyLayoutComponent,
];

@Component({
  selector: 'abp-dummy',
  template: '{{route.snapshot.data?.name}} works!',
  standalone: true,
})
class DummyComponent {
  route = inject_1(ActivatedRoute);
}

const routes: ABP.Route[] = [
  {
    path: '',
    name: 'Root',
  },
  {
    path: '/parentWithLayout',
    name: 'ParentWithLayout',
    parentName: 'Root',
    layout: eLayoutType.application,
  },
  {
    path: '/parentWithLayout/childWithoutLayout',
    name: 'ChildWithoutLayout',
    parentName: 'ParentWithLayout',
  },
  {
    path: '/parentWithLayout/childWithLayout',
    name: 'ChildWithLayout',
    parentName: 'ParentWithLayout',
    layout: eLayoutType.account,
  },
  {
    path: '/withData',
    name: 'WithData',
    layout: eLayoutType.application,
  },
];

describe('DynamicLayoutComponent', () => {
  const createComponent = createRoutingFactory({
    component: DynamicLayoutComponent,
    stubsEnabled: false,
    declarations: [],
    mocks: [AbpApplicationConfigurationService, HttpClient],
    providers: [
      {
        provide: RoutesService,
        useValue: mockRoutesService(),
      },
      {
        provide: RouterEvents,
        useValue: {
          getNavigationEvents: jest.fn().mockReturnValue(of({})),
        },
      },
      {
        provide: EnvironmentService,
        useValue: {
          getEnvironment: jest.fn().mockReturnValue({
            oAuthConfig: { responseType: 'code' },
          }),
        },
      },
      {
        provide: LocalizationService,
        useValue: {
          languageChange$: new BehaviorSubject('en'),
        },
      },
      {
        provide: DYNAMIC_LAYOUTS_TOKEN,
        useValue: new Map([
          [eLayoutType.application, 'Theme.ApplicationLayoutComponent'],
          [eLayoutType.account, 'Theme.AccountLayoutComponent'],
          [eLayoutType.empty, 'Theme.EmptyLayoutComponent'],
        ]),
      },
      {
        provide: ReplaceableComponentsService,
        useValue: {
          add: jest.fn(),
          get: jest.fn((key) => {
            if (key === 'Theme.ApplicationLayoutComponent') {
              return { component: DummyApplicationLayoutComponent };
            }
            if (key === 'Theme.AccountLayoutComponent') {
              return { component: DummyAccountLayoutComponent };
            }
            if (key === 'Theme.EmptyLayoutComponent') {
              return { component: DummyEmptyLayoutComponent };
            }
            return null;
          }),
        },
      },
    ],
    imports: [RouterModule, DummyComponent, DynamicLayoutComponent, ...LAYOUTS],
    routes: [
      { path: '', component: RouterOutletComponent },
      {
        path: 'parentWithLayout',
        component: DynamicLayoutComponent,
        children: [
          {
            path: 'childWithoutLayout',
            component: DummyComponent,
            data: { name: 'childWithoutLayout' },
          },
          {
            path: 'childWithLayout',
            component: DummyComponent,
            data: { name: 'childWithLayout' },
          },
        ],
      },
      {
        path: 'withData',
        component: DynamicLayoutComponent,
        children: [
          {
            path: '',
            component: DummyComponent,
            data: { name: 'withData' },
          },
        ],
        data: { layout: eLayoutType.empty },
      },
      {
        path: 'withoutLayout',
        component: DynamicLayoutComponent,
        children: [
          {
            path: '',
            component: DummyComponent,
            data: { name: 'withoutLayout' },
          },
        ],
        data: { layout: null },
      },
    ],
  });

  let spectator: SpectatorRouting<DynamicLayoutComponent>;
  let replaceableComponents: ReplaceableComponentsService;

  beforeEach(async () => {
    spectator = createComponent();
    replaceableComponents = spectator.inject(ReplaceableComponentsService);
    const routesService = spectator.inject(RoutesService);
    routesService.add(routes);

    replaceableComponents.add({
      key: 'Theme.ApplicationLayoutComponent',
      component: DummyApplicationLayoutComponent,
    });
    replaceableComponents.add({
      key: 'Theme.AccountLayoutComponent',
      component: DummyAccountLayoutComponent,
    });
    replaceableComponents.add({
      key: 'Theme.EmptyLayoutComponent',
      component: DummyEmptyLayoutComponent,
    });
  });

  it('should handle application layout from parent abp route and display it', async () => {
    spectator.router.navigateByUrl('/parentWithLayout/childWithoutLayout');
    await spectator.fixture.whenStable();
    await new Promise(resolve => setTimeout(resolve, 100));
    spectator.detectComponentChanges();
    expect(spectator.query('abp-dynamic-layout')).toBeTruthy();
    expect(spectator.query('abp-layout-application')).toBeTruthy();
  });

  it('should handle account layout from own property and display it', async () => {
    spectator.router.navigateByUrl('/parentWithLayout/childWithLayout');
    await spectator.fixture.whenStable();
    await new Promise(resolve => setTimeout(resolve, 100));
    spectator.detectComponentChanges();
    expect(spectator.query('abp-layout-account')).toBeTruthy();
  });

  it('should handle empty layout from route data and display it', async () => {
    spectator.router.navigateByUrl('/withData');
    await spectator.fixture.whenStable();
    await new Promise(resolve => setTimeout(resolve, 100));
    spectator.detectComponentChanges();
    expect(spectator.query('abp-layout-empty')).toBeTruthy();
  });

  it('should display empty layout when layout is null', async () => {
    spectator.router.navigateByUrl('/withoutLayout');
    await spectator.fixture.whenStable();
    await new Promise(resolve => setTimeout(resolve, 100));
    spectator.detectComponentChanges();
    expect(spectator.query('abp-layout-empty')).toBeTruthy();
  });

  it('should handle layout not found scenario', async () => {
    spectator.router.navigateByUrl('/withoutLayout');
    await spectator.fixture.whenStable();
    await new Promise(resolve => setTimeout(resolve, 100));
    spectator.detectComponentChanges();

    expect(spectator.query('abp-dynamic-layout')).toBeTruthy();
  });
});
