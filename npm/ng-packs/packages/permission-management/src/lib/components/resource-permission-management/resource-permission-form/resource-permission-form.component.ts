import { Component, input, inject, output, ChangeDetectionStrategy } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LocalizationPipe } from '@abp/ng.core';
import { ResourcePermissionStateService } from '../../../services/resource-permission-state.service';
import { ProviderKeySearchComponent } from '../provider-key-search/provider-key-search.component';
import { PermissionCheckboxListComponent } from '../permission-checkbox-list/permission-checkbox-list.component';

export type FormMode = 'add' | 'edit';

@Component({
    selector: 'abp-resource-permission-form',
    template: `
        @if (mode() === 'add') {
            <div class="mb-3">
                <label class="form-label fw-bold">
                    {{ 'AbpPermissionManagement::SelectProvider' | abpLocalization }}
                </label>
                <div class="mb-2">
                    @for (provider of state.providers(); track provider.name; let i = $index) {
                        <div class="form-check form-check-inline">
                            <input
                                class="form-check-input"
                                type="radio"
                                [id]="'provider-' + i"
                                [value]="provider.name"
                                [checked]="state.selectedProviderName() === provider.name"
                                (change)="state.onProviderChange(provider.name || '')"
                            />
                            <label class="form-check-label" [for]="'provider-' + i">
                                {{ provider.displayName }}
                            </label>
                        </div>
                    }
                </div>

                <abp-provider-key-search [resourceName]="resourceName()" />
            </div>

            <abp-permission-checkbox-list
                [permissions]="state.permissionDefinitions()"
                idPrefix="add"
            />
        } @else {
            <div class="mb-3" id="permissionList">
                <h4>{{ 'AbpPermissionManagement::Permissions' | abpLocalization }}</h4>
                <abp-permission-checkbox-list
                    [permissions]="state.permissionsWithProvider()"
                    idPrefix="edit"
                    [showTitle]="false"
                />
            </div>
        }
    `,
    imports: [
        FormsModule,
        LocalizationPipe,
        ProviderKeySearchComponent,
        PermissionCheckboxListComponent,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResourcePermissionFormComponent {
    readonly state = inject(ResourcePermissionStateService);

    readonly mode = input.required<FormMode>();
    readonly resourceName = input.required<string>();

    readonly save = output<void>();
    readonly cancel = output<void>();
}
