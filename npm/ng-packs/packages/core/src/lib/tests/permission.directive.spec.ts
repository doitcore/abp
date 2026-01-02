import { createDirectiveFactory, SpectatorDirective } from '@ngneat/spectator/vitest';
import { Subject } from 'rxjs';
import { PermissionDirective } from '../directives/permission.directive';
import { PermissionService } from '../services/permission.service';
import { ChangeDetectorRef } from '@angular/core';
import { QUEUE_MANAGER } from '../tokens/queue.token';

describe('PermissionDirective', () => {
  let spectator: SpectatorDirective<PermissionDirective>;
  let directive: PermissionDirective;
  const grantedPolicy$ = new Subject<boolean>();
  const createDirective = createDirectiveFactory({
    directive: PermissionDirective,
    providers: [
      { provide: PermissionService, useValue: { getGrantedPolicy$: () => grantedPolicy$ } },
      { provide: QUEUE_MANAGER, useValue: { add: vi.fn() } },
      { provide: ChangeDetectorRef, useValue: { detectChanges: vi.fn() } },
    ],
  });

  beforeEach(() => {
    spectator = createDirective('<div [abpPermission]="permission" [abpPermissionRunChangeDetection]="runCD"></div>', {
      hostProps: { permission: 'test', runCD: false },
    });
    directive = spectator.directive;
  });

  it('should create directive', () => {
    expect(directive).toBeTruthy();
  });

  it('should handle permission input', () => {
    spectator.setHostInput({ permission: 'new-permission' });
    spectator.detectChanges();
    expect(directive).toBeTruthy();
  });

  it('should handle runChangeDetection input', () => {
    spectator.setHostInput({ runCD: true });
    spectator.detectChanges();
    expect(directive).toBeTruthy();
  });
});
