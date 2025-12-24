import { Component, input, inject, ChangeDetectionStrategy } from '@angular/core';
import { LocalizationPipe } from '@abp/ng.core';
import { ResourcePermissionStateService } from '../../../services/resource-permission-state.service';

interface PermissionItem {
    name?: string | null;
    displayName?: string | null;
}

@Component({
    selector: 'abp-permission-checkbox-list',
    template: `
        <div class="mb-3">
            @if (showTitle()) {
                <h5>{{ title() | abpLocalization }}</h5>
            }
            <div class="form-check form-switch mb-2">
                <input
                    class="form-check-input"
                    type="checkbox"
                    [id]="'grantAll-' + idPrefix()"
                    [checked]="state.allPermissionsSelected()"
                    (change)="state.toggleAllPermissions(!state.allPermissionsSelected())"
                />
                <label class="form-check-label" [for]="'grantAll-' + idPrefix()">
                    {{ 'AbpPermissionManagement::GrantAllResourcePermissions' | abpLocalization }}
                </label>
            </div>
            <div class="border rounded p-3" style="max-height: 300px; overflow-y: auto;">
                @for (perm of permissions(); track perm.name) {
                    <div class="form-check">
                        <input
                            class="form-check-input"
                            type="checkbox"
                            [id]="'perm-' + idPrefix() + '-' + perm.name"
                            [checked]="state.isPermissionSelected(perm.name || '')"
                            (change)="state.togglePermission(perm.name || '')"
                        />
                        <label class="form-check-label" [for]="'perm-' + idPrefix() + '-' + perm.name">
                            {{ perm.displayName }}
                        </label>
                    </div>
                }
            </div>
        </div>
    `,
    imports: [LocalizationPipe],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PermissionCheckboxListComponent {
    readonly state = inject(ResourcePermissionStateService);

    readonly permissions = input.required<PermissionItem[]>();
    readonly idPrefix = input<string>('default');
    readonly title = input<string>('AbpPermissionManagement::ResourcePermissionPermissions');
    readonly showTitle = input<boolean>(true);
}
