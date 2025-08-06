import {
  ABP,
  CORE_OPTIONS,
  LocalizationPipe,
  RouterOutletComponent,
  RoutesService,
} from '@abp/ng.core';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { createRoutingFactory, SpectatorRouting } from '@ngneat/spectator/jest';
// eslint-disable-next-line @nx/enforce-module-boundaries
import { mockRoutesService } from '../../../../core/src/lib/tests/routes.service.spec';
import { BreadcrumbComponent, BreadcrumbItemsComponent } from '../components';

const mockRoutes: ABP.Route[] = [
  { name: 'Identity', path: '/identity' },
  { name: 'Users', path: '/identity/users', parentName: 'Identity' },
];

describe('BreadcrumbComponent', () => {
  let spectator: SpectatorRouting<RouterOutletComponent>;
  let routes: RoutesService;

  const createRouting = createRoutingFactory({
    component: RouterOutletComponent,
    stubsEnabled: false,
    detectChanges: false,
    mocks: [HttpClient],
    providers: [
      { provide: CORE_OPTIONS, useValue: {} },
      {
        provide: RoutesService,
        useFactory: () => mockRoutesService(),
      },
    ],
    declarations: [],
    imports: [
      RouterModule,
      HttpClientModule,
      LocalizationPipe,
      BreadcrumbComponent,
      BreadcrumbItemsComponent,
    ],
    routes: [
      {
        path: '',
        children: [
          {
            path: 'identity',
            children: [
              {
                path: 'users',
                component: BreadcrumbComponent,
              },
            ],
          },
        ],
      },
    ],
  });

  beforeEach(() => {
    spectator = createRouting();
    routes = spectator.inject(RoutesService);
  });

  it('should create component', async () => {
    routes.add(mockRoutes);
    await spectator.router.navigateByUrl('/identity/users');
    spectator.detectChanges();
    expect(spectator.component).toBeTruthy();
  });

  it('should handle empty routes', async () => {
    routes.add([]);
    await spectator.router.navigateByUrl('/identity/users');
    spectator.detectChanges();
    expect(spectator.component).toBeTruthy();
  });
});
