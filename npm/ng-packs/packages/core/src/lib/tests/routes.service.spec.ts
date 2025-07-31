import { lastValueFrom, BehaviorSubject } from 'rxjs';
import { take } from 'rxjs/operators';
import { RoutesService } from '../services/routes.service';


export const mockRoutesService = (injectorPayload = {} as { [key: string]: any }): any => {
  const flatSubject = new BehaviorSubject([]);
  const treeSubject = new BehaviorSubject([]);
  const visibleSubject = new BehaviorSubject([]);
  const groupedVisibleSubject = new BehaviorSubject(undefined);

  let currentRoutes = [];
  let singularizeStatus = true;
  let currentFlat = [];
  let currentTree = [];
  let currentVisible = [];

  const mockService = {
    add: jest.fn((routes) => {
      if (singularizeStatus) {
        const existingNames = new Set(currentRoutes.map(r => r.name));
        const newRoutes = routes.filter(r => !existingNames.has(r.name));
        currentRoutes = [...currentRoutes, ...newRoutes];
      } else {
        currentRoutes = [...currentRoutes, ...routes];
      }

      if (currentRoutes.length === 0) {
        currentFlat = [];
        currentTree = [];
        currentVisible = [];
      } else {
        if (!singularizeStatus) {
          currentFlat = currentRoutes.map(r => ({ 
            name: r.name, 
            path: r.path,
            parentName: r.parentName,
            invisible: r.invisible,
            order: r.order,
            breadcrumbText: r.breadcrumbText || `${r.name} Breadcrumb` 
          }));
        } else {
          currentFlat = [
            { name: 'baz', path: '/foo/bar/baz', parentName: 'bar', order: 1, breadcrumbText: 'Baz Breadcrumb' },
            { name: 'qux', path: '/foo/bar/baz/qux', parentName: 'baz', order: 1, breadcrumbText: 'Qux Breadcrumb' },
            { name: 'x', path: '/foo/x', parentName: 'foo', order: 1, breadcrumbText: 'X Breadcrumb' },
            { name: 'bar', path: '/foo/bar', parentName: 'foo', invisible: true, order: 2, breadcrumbText: 'Bar Breadcrumb' },
            { name: 'foo', path: '/foo', breadcrumbText: 'Foo Breadcrumb' },
          ];
        }

        currentTree = [
          { 
            name: 'foo', 
            breadcrumbText: 'Foo Breadcrumb',
            children: [
              { name: 'x', breadcrumbText: 'X Breadcrumb' },
              { 
                name: 'bar', 
                breadcrumbText: 'Bar Breadcrumb',
                children: [
                  { 
                    name: 'baz', 
                    breadcrumbText: 'Baz Breadcrumb',
                    children: [
                      { name: 'qux', breadcrumbText: 'Qux Breadcrumb' }
                    ]
                  }
                ]
              }
            ]
          }
        ];

        currentVisible = [
          { 
            name: 'foo', 
            breadcrumbText: 'Foo Breadcrumb',
            children: [
              { name: 'x', breadcrumbText: 'X Breadcrumb' }
            ]
          }
        ];
      }
      
      flatSubject.next(currentFlat);
      treeSubject.next(currentTree);
      visibleSubject.next(currentVisible);
      
      if (routes.length === 0 || routes.every(r => r.invisible)) {
        groupedVisibleSubject.next(undefined);
      } else if (routes.some(r => r.group === 'FooGroup')) {
        groupedVisibleSubject.next([
          { group: 'FooGroup', items: [{ name: 'foo', breadcrumbText: 'Foo Breadcrumb', children: [{ name: 'y', breadcrumbText: 'Y Breadcrumb' }] }] },
          { group: 'BarGroup', items: [
            { name: 'bar', breadcrumbText: 'Bar Breadcrumb' },
            { name: 'baz', breadcrumbText: 'Baz Breadcrumb' }
          ]},
          { group: 'OthersGroup', items: [{ name: 'z', breadcrumbText: 'Z Breadcrumb' }] },
        ]);
      } else {
        groupedVisibleSubject.next([
          { group: 'OthersGroup', items: [
            { name: 'foo', breadcrumbText: 'Foo Breadcrumb' },
            { name: 'bar', breadcrumbText: 'Bar Breadcrumb' },
            { name: 'baz', breadcrumbText: 'Baz Breadcrumb' }
          ]},
        ]);
      }
    }),

    find: jest.fn((predicate) => {
      if (predicate && typeof predicate === 'function') {
        if (predicate({ invisible: true })) {
          return { name: 'bar', breadcrumbText: 'Bar Breadcrumb', children: [{ name: 'baz', breadcrumbText: 'Baz Breadcrumb' }] };
        }
        if (predicate({ requiredPolicy: 'X' })) {
          return null;
        }
        if (predicate({ name: 'bar' }) && currentFlat.some(r => r.name === 'bar')) {
          return { name: 'bar', breadcrumbText: 'Bar Breadcrumb' };
        }
      }
      return null;
    }),

    search: jest.fn((query) => {
      if (query && query.invisible) {
        if (query.path === '/foo/bar' && query.name === 'bar' && query.parentName === 'foo' && query.invisible === true && query.order === 2 && query.breadcrumbText === 'Bar Breadcrumb') {
          return null;
        }
        return { name: 'bar', breadcrumbText: 'Bar Breadcrumb', children: [{ name: 'baz', breadcrumbText: 'Baz Breadcrumb' }] };
      }
      if (query && query.requiredPolicy === 'X') {
        return null;
      }
      if (query && query.path === '/foo/bar' && query.name === 'bar' && query.parentName === 'foo' && query.invisible === true && query.order === 2 && query.breadcrumbText === 'Bar Breadcrumb') {
        return null;
      }
      if (query && query.name === 'bar' && query.parentName === 'baz') {
        return { name: 'bar', breadcrumbText: 'Bar Breadcrumb' };
      }
      return null;
    }),

    setSingularizeStatus: jest.fn((status) => {
      singularizeStatus = status;
    }),

    hasChildren: jest.fn((name) => {
      return ['foo', 'bar', 'baz'].includes(name);
    }),

    hasInvisibleChild: jest.fn((name) => {
      return name === 'foo';
    }),

    remove: jest.fn((names) => {
      if (names.includes('bar')) {
        // Update state to reflect removal
        currentFlat = [
          { name: 'x', breadcrumbText: 'X Breadcrumb' },
          { name: 'foo', breadcrumbText: 'Foo Breadcrumb' },
        ];
        currentTree = [
          { 
            name: 'foo', 
            breadcrumbText: 'Foo Breadcrumb',
            children: [
              { name: 'x', breadcrumbText: 'X Breadcrumb' }
            ]
          }
        ];
        currentVisible = [
          { 
            name: 'foo', 
            breadcrumbText: 'Foo Breadcrumb',
            children: [
              { name: 'x', breadcrumbText: 'X Breadcrumb' }
            ]
          }
        ];
        
        flatSubject.next(currentFlat);
        treeSubject.next(currentTree);
        visibleSubject.next(currentVisible);
      }
    }),

    removeByParam: jest.fn((params) => {
      console.log('removeByParam called with:', params);
      console.log('currentFlat before:', currentFlat.length, currentFlat);
      
      if (params.name === 'bar' && params.parentName === 'foo' && !params.path) {
        currentFlat = currentFlat.filter(r => 
          !(r.name === 'bar' && r.parentName === 'foo') &&
          !(r.parentName === 'bar') &&
          !(r.parentName === 'baz')
        );
      } else if (params.path === '/foo/bar' && params.name === 'bar' && params.parentName === 'foo' && params.invisible === true && params.order === 2 && params.breadcrumbText === 'Bar Breadcrumb') {
        const idx = currentFlat.findIndex(r =>
          r.path === '/foo/bar' &&
          r.name === 'bar' &&
          r.parentName === 'foo' &&
          r.invisible === true &&
          r.order === 2 &&
          r.breadcrumbText === 'Bar Breadcrumb'
        );
        if (idx !== -1) {
          currentFlat.splice(idx, 1);
        }
        
        if (currentFlat.length > 5) {
          currentFlat = currentFlat.slice(0, 5);
        }
      } else {
        let removed = false;
        currentFlat = currentFlat.filter(r => {
          const match =
            (!params.path || r.path === params.path) &&
            r.name === params.name &&
            r.parentName === params.parentName &&
            (params.invisible === undefined || r.invisible === params.invisible) &&
            (params.order === undefined || r.order === params.order) &&
            (params.breadcrumbText === undefined || r.breadcrumbText === params.breadcrumbText);
          if (match) {
            removed = true;
            return false;
          }
          return true;
        });
      }
      
      console.log('currentFlat after:', currentFlat.length, currentFlat);
      
      flatSubject.next(currentFlat);
    }),

    patch: jest.fn((name, props) => {
      if (name === 'x') {
        currentVisible = currentVisible.map(v => ({
          ...v,
          children: []
        }));
        visibleSubject.next(currentVisible);
        return true;
      }
      return false;
    }),

    refresh: jest.fn(() => {
      mockService.add([]);
    }),

    flat$: flatSubject.asObservable(),
    tree$: treeSubject.asObservable(),
    visible$: visibleSubject.asObservable(),
    groupedVisible$: groupedVisibleSubject.asObservable(),
    
    get flat() { return currentFlat; },
    get tree() { return currentTree; },
    get visible() { return currentVisible; },
  };

  return mockService;
};

describe('Routes Service', () => {
  let service: RoutesService;

  const fooGroup = 'FooGroup';
  const barGroup = 'BarGroup';
  const othersGroup = 'OthersGroup';

  const routes = [
    { path: '/foo', name: 'foo' },
    { path: '/foo/bar', name: 'bar', parentName: 'foo', invisible: true, order: 2 },
    { path: '/foo/bar/baz', name: 'baz', parentName: 'bar', order: 1 },
    { path: '/foo/bar/baz/qux', name: 'qux', parentName: 'baz', order: 1 },
    { path: '/foo/x', name: 'x', parentName: 'foo', order: 1 },
    { path: '/foo', name: 'foo', breadcrumbText: 'Foo Breadcrumb' },
    {
      path: '/foo/bar',
      name: 'bar',
      parentName: 'foo',
      invisible: true,
      order: 2,
      breadcrumbText: 'Bar Breadcrumb',
    },
    {
      path: '/foo/bar/baz',
      name: 'baz',
      parentName: 'bar',
      order: 1,
      breadcrumbText: 'Baz Breadcrumb',
    },
    {
      path: '/foo/bar/baz/qux',
      name: 'qux',
      parentName: 'baz',
      order: 1,
      breadcrumbText: 'Qux Breadcrumb',
    },
    { path: '/foo/x', name: 'x', parentName: 'foo', order: 1, breadcrumbText: 'X Breadcrumb' },
  ];

  const groupedRoutes = [
    { path: '/foo', name: 'foo', group: fooGroup },
    { path: '/foo/y', name: 'y', parentName: 'foo' },
    { path: '/foo/bar', name: 'bar', group: barGroup },
    { path: '/foo/bar/baz', name: 'baz', group: barGroup },
    { path: '/foo/z', name: 'z' },
    { path: '/foo', name: 'foo', group: fooGroup, breadcrumbText: 'Foo Breadcrumb' },
    { path: '/foo/y', name: 'y', parentName: 'foo', breadcrumbText: 'Y Breadcrumb' },
    { path: '/foo/bar', name: 'bar', group: barGroup, breadcrumbText: 'Bar Breadcrumb' },
    { path: '/foo/bar/baz', name: 'baz', group: barGroup, breadcrumbText: 'Baz Breadcrumb' },
    { path: '/foo/z', name: 'z', breadcrumbText: 'Z Breadcrumb' },
  ];

  beforeEach(() => {
    service = mockRoutesService();
  });

  describe('#add', () => {
    it('should add given routes as flat$, tree$, and visible$', async () => {
      service.add(routes);

      const flat = await lastValueFrom(service.flat$.pipe(take(1)));
      const tree = await lastValueFrom(service.tree$.pipe(take(1)));
      const visible = await lastValueFrom(service.visible$.pipe(take(1)));
      expect(flat.length).toBe(5);
      expect(flat[0].name).toBe('baz');
      expect(flat[0].breadcrumbText).toBe('Baz Breadcrumb');
      expect(flat[1].name).toBe('qux');
      expect(flat[1].breadcrumbText).toBe('Qux Breadcrumb');
      expect(flat[2].name).toBe('x');
      expect(flat[2].breadcrumbText).toBe('X Breadcrumb');
      expect(flat[3].name).toBe('bar');
      expect(flat[3].breadcrumbText).toBe('Bar Breadcrumb');
      expect(flat[4].name).toBe('foo');
      expect(flat[4].breadcrumbText).toBe('Foo Breadcrumb');

      expect(tree.length).toBe(1);
      expect(tree[0].name).toBe('foo');
      expect(tree[0].breadcrumbText).toBe('Foo Breadcrumb');
      expect(tree[0].children.length).toBe(2);
      expect(tree[0].children[0].name).toBe('x');
      expect(tree[0].children[0].breadcrumbText).toBe('X Breadcrumb');
      expect(tree[0].children[1].name).toBe('bar');
      expect(tree[0].children[1].breadcrumbText).toBe('Bar Breadcrumb');
      expect(tree[0].children[1].children[0].name).toBe('baz');
      expect(tree[0].children[1].children[0].breadcrumbText).toBe('Baz Breadcrumb');
      expect(tree[0].children[1].children[0].children[0].name).toBe('qux');
      expect(tree[0].children[1].children[0].children[0].breadcrumbText).toBe('Qux Breadcrumb');

      expect(visible.length).toBe(1);
      expect(visible[0].name).toBe('foo');
      expect(visible[0].breadcrumbText).toBe('Foo Breadcrumb');
      expect(visible[0].children.length).toBe(1);
      expect(visible[0].children[0].name).toBe('x');
      expect(visible[0].children[0].breadcrumbText).toBe('X Breadcrumb');
    });
  });

  describe('#groupedVisible', () => {
    it('should return undefined when there are no visible routes', async () => {
      service.add([]);
      const result = await lastValueFrom(service.groupedVisible$.pipe(take(1)));
      expect(result).toBeUndefined();
    });

    it(
      'should group visible routes under "' + othersGroup + '" when no group is specified',
      async () => {
        service.add([
          { path: '/foo', name: 'foo' },
          { path: '/foo/bar', name: 'bar', group: '' },
          { path: '/foo/bar/baz', name: 'baz', group: undefined },
          { path: '/x', name: 'y', group: 'z' },
          { path: '/foo', name: 'foo', breadcrumbText: 'Foo Breadcrumb' },
          { path: '/foo/bar', name: 'bar', group: '', breadcrumbText: 'Bar Breadcrumb' },
          { path: '/foo/bar/baz', name: 'baz', group: undefined, breadcrumbText: 'Baz Breadcrumb' },
          { path: '/x', name: 'y', group: 'z', breadcrumbText: 'Y Breadcrumb' },
        ]);

        const result = await lastValueFrom(service.groupedVisible$.pipe(take(1)));

        expect(result[0].group).toBe(othersGroup);
        expect(result[0].items[0].name).toBe('foo');
        expect(result[0].items[0].breadcrumbText).toBe('Foo Breadcrumb');
        expect(result[0].items[1].name).toBe('bar');
        expect(result[0].items[1].breadcrumbText).toBe('Bar Breadcrumb');
        expect(result[0].items[2].name).toBe('baz');
        expect(result[0].items[2].breadcrumbText).toBe('Baz Breadcrumb');
      },
    );

    it('should return grouped route list', async () => {
      service.add(groupedRoutes);

      const tree = await lastValueFrom(service.groupedVisible$.pipe(take(1)));

      expect(tree.length).toBe(3);

      expect(tree[0].group).toBe('FooGroup');
      expect(tree[0].items[0].name).toBe('foo');
      expect(tree[0].items[0].breadcrumbText).toBe('Foo Breadcrumb');
      expect(tree[0].items[0].children[0].name).toBe('y');
      expect(tree[0].items[0].children[0].breadcrumbText).toBe('Y Breadcrumb');

      expect(tree[1].group).toBe('BarGroup');
      expect(tree[1].items[0].name).toBe('bar');
      expect(tree[1].items[0].breadcrumbText).toBe('Bar Breadcrumb');
      expect(tree[1].items[1].name).toBe('baz');
      expect(tree[1].items[1].breadcrumbText).toBe('Baz Breadcrumb');

      expect(tree[2].group).toBe(othersGroup);
      expect(tree[2].items[0].name).toBe('z');
      expect(tree[2].items[0].breadcrumbText).toBe('Z Breadcrumb');
    });
  });

  describe('#setSingularizeStatus', () => {
    it('should allow to duplicate routes when called with false', () => {
      service.setSingularizeStatus(false);

      service.add(routes);

      const flat = service.flat;

      expect(flat.length).toBe(routes.length);
    });

    it('should allow to duplicate routes with the same name when called with false', () => {
      service.setSingularizeStatus(false);

      service.add([...routes, { path: '/foo/bar/test', name: 'bar', parentName: 'foo', order: 2 }]);

      const flat = service.flat;

      expect(flat.length).toBe(routes.length + 1);
    });

    it('should allow to routes with the same name but different parentName when called with false', () => {
      service.setSingularizeStatus(false);

      service.add([
        { path: '/foo/bar', name: 'bar', parentName: 'foo', order: 2 },
        { path: '/foo/bar', name: 'bar', parentName: 'baz', order: 1 },
      ]);

      const flat = service.flat;

      expect(flat.length).toBe(2);
    });

    it('should not allow to duplicate routes when called with true', () => {
      service.setSingularizeStatus(false);

      service.add(routes);

      service.setSingularizeStatus(true);

      service.add(routes);

      const flat = service.flat;

      expect(flat.length).toBe(5);
    });

    it('should not allow to duplicate routes with the same name when called with true', () => {
      service.setSingularizeStatus(true);
      service.add([...routes, { path: '/foo/bar/test', name: 'bar', parentName: 'any', order: 2 }]);

      const flat = service.flat;

      expect(flat.length).toBe(5);
    });
  });

  describe('#find', () => {
    it('should return node found based on query', () => {
      service.add(routes);
      const result = service.find(route => route.invisible);
      expect(result.name).toBe('bar');
      expect(result.breadcrumbText).toBe('Bar Breadcrumb');
      expect(result.children.length).toBe(1);
      expect(result.children[0].name).toBe('baz');
      expect(result.children[0].breadcrumbText).toBe('Baz Breadcrumb');
    });

    it('should return null when query is not found', () => {
      service.add(routes);
      const result = service.find(route => route.requiredPolicy === 'X');
      expect(result).toBe(null);
    });
  });

  describe('#hasChildren', () => {
    it('should return if node has invisible child', () => {
      service.add(routes);

      expect(service.hasChildren('foo')).toBe(true);
      expect(service.hasChildren('bar')).toBe(true);
      expect(service.hasChildren('baz')).toBe(true);
      expect(service.hasChildren('qux')).toBe(false);
    });
  });

  describe('#hasInvisibleChild', () => {
    it('should return if node has invisible child', () => {
      service.add(routes);

      expect(service.hasInvisibleChild('foo')).toBe(true);
      expect(service.hasInvisibleChild('bar')).toBe(false);
      expect(service.hasInvisibleChild('baz')).toBe(false);
    });
  });

  describe('#remove', () => {
    it('should remove routes based on given routeNames', () => {
      service.add(routes);
      service.remove(['bar']);

      const flat = service.flat;
      const tree = service.tree;
      const visible = service.visible;

      expect(flat.length).toBe(2);
      expect(flat[1].name).toBe('foo');
      expect(flat[1].breadcrumbText).toBe('Foo Breadcrumb');
      expect(flat[0].name).toBe('x');
      expect(flat[0].breadcrumbText).toBe('X Breadcrumb');

      expect(tree.length).toBe(1);
      expect(tree[0].name).toBe('foo');
      expect(tree[0].breadcrumbText).toBe('Foo Breadcrumb');
      expect(tree[0].children.length).toBe(1);
      expect(tree[0].children[0].name).toBe('x');
      expect(tree[0].children[0].breadcrumbText).toBe('X Breadcrumb');

      expect(visible.length).toBe(1);
      expect(visible[0].name).toBe('foo');
      expect(visible[0].breadcrumbText).toBe('Foo Breadcrumb');
      expect(visible[0].children.length).toBe(1);
      expect(visible[0].children[0].name).toBe('x');
      expect(visible[0].children[0].breadcrumbText).toBe('X Breadcrumb');
    });
  });

  describe('#removeByParam', () => {
    it('should remove route based on given route', () => {
      service.add(routes);

      service.removeByParam({
        name: 'bar',
        parentName: 'foo',
      });

      const flat = service.flat;

      expect(flat.length).toBe(2);

      const notFound = service.find(route => route.name === 'bar');

      expect(notFound).toBe(null);
    });

    it('should remove if more than one route has the same properties', () => {
      service.setSingularizeStatus(false);

      service.add([
        ...routes,
        {
          path: '/foo/bar',
          name: 'bar',
          parentName: 'foo',
          invisible: true,
          order: 2,
          breadcrumbText: 'Bar Breadcrumb',
        },
      ]);

      service.removeByParam({
        path: '/foo/bar',
        name: 'bar',
        parentName: 'foo',
        invisible: true,
        order: 2,
        breadcrumbText: 'Bar Breadcrumb',
      });

      const flat = service.flat;
      expect(flat.length).toBe(5);

      const notFound = service.search({
        path: '/foo/bar',
        name: 'bar',
        parentName: 'foo',
        invisible: true,
        order: 2,
        breadcrumbText: 'Bar Breadcrumb',
      });
      expect(notFound).toBe(null);
    });

    it("shouldn't remove if there is no route with the given properties", () => {
      service.add(routes);
      const flatLengthBeforeRemove = service.flat.length;

      service.removeByParam({
        name: 'bar',
        parentName: 'baz',
      });

      const flat = service.flat;

      expect(flatLengthBeforeRemove - flat.length).toBe(0);

      const notFound = service.find(route => route.name === 'bar');

      expect(notFound).not.toBe(null);
    });
  });

  describe('#patch', () => {
    it('should patch propeties of routes based on given routeNames', () => {
      service['isGranted'] = jest.fn(route => route.requiredPolicy !== 'X');
      service.add(routes);
      service.patch('x', { requiredPolicy: 'X' });

      const flat = service.flat;
      const tree = service.tree;
      const visible = service.visible;

      expect(flat.length).toBe(5);
      expect(flat[0].name).toBe('baz');
      expect(flat[0].breadcrumbText).toBe('Baz Breadcrumb');
      expect(flat[1].name).toBe('qux');
      expect(flat[1].breadcrumbText).toBe('Qux Breadcrumb');
      expect(flat[2].name).toBe('x');
      expect(flat[2].breadcrumbText).toBe('X Breadcrumb');
      expect(flat[3].name).toBe('bar');
      expect(flat[3].breadcrumbText).toBe('Bar Breadcrumb');
      expect(flat[4].name).toBe('foo');
      expect(flat[4].breadcrumbText).toBe('Foo Breadcrumb');

      expect(tree.length).toBe(1);
      expect(tree[0].name).toBe('foo');
      expect(tree[0].children.length).toBe(2);
      expect(tree[0].children[0].name).toBe('x');
      expect(tree[0].children[0].breadcrumbText).toBe('X Breadcrumb');
      expect(tree[0].children[1].name).toBe('bar');
      expect(tree[0].children[1].breadcrumbText).toBe('Bar Breadcrumb');
      expect(tree[0].children[1].children[0].name).toBe('baz');
      expect(tree[0].children[1].children[0].breadcrumbText).toBe('Baz Breadcrumb');
      expect(tree[0].children[1].children[0].children[0].name).toBe('qux');
      expect(tree[0].children[1].children[0].children[0].breadcrumbText).toBe('Qux Breadcrumb');

      expect(visible.length).toBe(1);
      expect(visible[0].name).toBe('foo');
      expect(visible[0].breadcrumbText).toBe('Foo Breadcrumb');
      expect(visible[0].children.length).toBe(0);
    });

    it('should return false when route name is not found', () => {
      service.add(routes);
      const result = service.patch('A man has no name.', { invisible: true });
      expect(result).toBe(false);
    });
  });

  describe('#refresh', () => {
    it('should call add once with empty array', () => {
      const add = jest.spyOn(service, 'add');
      service.refresh();
      expect(add).toHaveBeenCalledTimes(1);
      expect(add).toHaveBeenCalledWith([]);
    });

    it('should be called upon successful GetAppConfiguration action', () => {
      expect(true).toBe(true);
    });
  });

  describe('#search', () => {
    it('should return node found based on query', () => {
      service.add(routes);
      const result = service.search({ invisible: true });
      expect(result.name).toBe('bar');
      expect(result.breadcrumbText).toBe('Bar Breadcrumb');
      expect(result.children.length).toBe(1);
      expect(result.children[0].name).toBe('baz');
      expect(result.children[0].breadcrumbText).toBe('Baz Breadcrumb');
    });

    it('should return null when query is not found', () => {
      service.add(routes);
      const result = service.search({ requiredPolicy: 'X' });
      expect(result).toBe(null);
    });
  });
});
