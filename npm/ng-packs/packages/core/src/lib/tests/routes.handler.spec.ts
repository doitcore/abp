import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { RoutesHandler } from '../handlers/routes.handler';
import { RoutesService } from '../services/routes.service';

describe('Routes Handler', () => {
  let handler: RoutesHandler;
  let mockRoutesService: RoutesService;
  let mockRouter: Router;

  beforeEach(() => {
    mockRoutesService = { add: jest.fn() } as unknown as RoutesService;
    mockRouter = { config: [] } as unknown as Router;

    TestBed.configureTestingModule({
      providers: [
        RoutesHandler,
        { provide: RoutesService, useValue: mockRoutesService },
        { provide: Router, useValue: mockRouter },
      ],
    });
    handler = TestBed.inject(RoutesHandler);
  });

  describe('#addRoutes', () => {
    it('should add routes from router config', () => {
      const config = [
        { path: 'x' },
        { path: 'y', data: {} },
        { path: '', data: { routes: { name: 'Foo' } } },
        { path: 'bar', data: { routes: { name: 'Bar' } } },
        { data: { routes: [{ path: '/baz', name: 'Baz' }] } },
      ];
      const foo = [{ path: '/', name: 'Foo' }];
      const bar = [{ path: '/bar', name: 'Bar' }];
      const baz = [{ path: '/baz', name: 'Baz' }];

      const routes: any[] = [];
      const add = jest.fn((items: any[]) => {
        routes.push(...items);
        return items;
      });
      mockRoutesService.add = add;
      mockRouter.config = config;

      handler.addRoutes();

      expect(add).toHaveBeenCalledTimes(3);
      expect(routes).toEqual([
        { name: 'Foo', parentName: undefined, path: '/' },
        { name: 'Bar', parentName: undefined, path: '/bar' },
        { name: 'Baz', path: '/baz' },
      ]);
    });

    it('should not add routes when there is no router', () => {
      const routes: any[] = [];
      const add = jest.fn((items: any[]) => {
        routes.push(...items);
        return items;
      });
      mockRoutesService.add = add;
      mockRouter.config = null;

      handler.addRoutes();

      expect(add).not.toHaveBeenCalled();
    });
  });
});
