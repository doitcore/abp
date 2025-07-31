import {
  NavigationCancel,
  NavigationEnd,
  NavigationError,
  NavigationStart,
  ResolveEnd,
  ResolveStart,
  Router,
  RouterEvent,
} from '@angular/router';
import { createServiceFactory, SpectatorService } from '@ngneat/spectator/jest';
import { Subject } from 'rxjs';
import { take } from 'rxjs/operators';
import { NavigationEventKey, RouterEvents } from '../services/router-events.service';

class MockRouterEvent extends RouterEvent { constructor(id: number) { super(id, ''); (this as any).id = id; } }
class MockNavigationStart extends NavigationStart { constructor(id: number) { super(id, '', 'imperative'); (this as any).id = id; } }
class MockResolveStart extends ResolveStart { constructor(id: number) { super(id, '', '', null); (this as any).id = id; } }
class MockNavigationError extends NavigationError { constructor(id: number) { super(id, '', '', null); (this as any).id = id; } }
class MockNavigationEnd extends NavigationEnd { constructor(id: number) { super(id, '', ''); (this as any).id = id; } }
class MockResolveEnd extends ResolveEnd { constructor(id: number) { super(id, '', '', null); (this as any).id = id; } }
class MockNavigationCancel extends NavigationCancel { constructor(id: number) { super(id, '', ''); (this as any).id = id; } }

describe('RouterEvents', () => {
  let spectator: SpectatorService<RouterEvents>;
  let service: RouterEvents;
  const events = new Subject<RouterEvent>();
  const emitRouterEvents = () => {
    events.next(new MockRouterEvent(0));
    events.next(new MockNavigationStart(1));
    events.next(new MockResolveStart(2));
    events.next(new MockRouterEvent(3));
    events.next(new MockNavigationError(4));
    events.next(new MockNavigationEnd(5));
    events.next(new MockResolveEnd(6));
    events.next(new MockNavigationCancel(7));
  };

  const createService = createServiceFactory({
    service: RouterEvents,
    providers: [
      {
        provide: Router,
        useValue: { events },
      },
    ],
  });

  beforeEach(() => {
    spectator = createService();
    service = spectator.service;
  });

  describe('getNavigationEvents', () => {
    test.each`
      filtered               | expected
      ${['Start', 'Cancel']} | ${[1, 7]}
      ${['Error', 'Cancel']} | ${[4, 7]}
      ${['Start', 'End']}    | ${[1, 5]}
      ${['Error', 'End']}    | ${[4, 5]}
    `(
      'should return a stream of given navigation events',
      ({ filtered, expected }: NavigationEventTest) => {
        const stream = service.getNavigationEvents(...filtered);
        const collected: number[] = [];

        stream.pipe(take(2)).subscribe(event => collected.push((event as any).id));

        emitRouterEvents();

        expect(collected).toEqual(expected);
      },
    );
  });

  describe('getAnyNavigationEvent', () => {
    it('should return a stream of any navigation event', () => {
      const stream = service.getAllNavigationEvents();
      const collected: number[] = [];

      stream.pipe(take(4)).subscribe(event => collected.push((event as any).id));

      emitRouterEvents();

      expect(collected).toEqual([1, 4, 5, 7]);
    });
  });

  describe('getEvents', () => {
    it('should return a stream of given router events', () => {
      const stream = service.getEvents(ResolveEnd, ResolveStart);
      const collected: number[] = [];

      stream.pipe(take(2)).subscribe(event => collected.push((event as any).id));

      emitRouterEvents();

      expect(collected).toEqual([2, 6]);
    });
  });

  describe('getAnyEvent', () => {
    it('should return a stream of any router event', () => {
      const stream = service.getAllEvents();
      const collected: number[] = [];

      stream.pipe(take(8)).subscribe((event: any) => collected.push((event as any).id));

      emitRouterEvents();

      expect(collected).toEqual([0, 1, 2, 3, 4, 5, 6, 7]);
    });
  });
});

interface NavigationEventTest {
  filtered: [NavigationEventKey, ...NavigationEventKey[]];
  expected: number[];
}
