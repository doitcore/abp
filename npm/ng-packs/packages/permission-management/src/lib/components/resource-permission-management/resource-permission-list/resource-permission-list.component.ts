import { Component, inject, output, ChangeDetectionStrategy } from '@angular/core';
import { ListService, LocalizationPipe } from '@abp/ng.core';
import { ExtensibleTableComponent, EXTENSIONS_IDENTIFIER } from '@abp/ng.components/extensible';
import { ResourcePermissionGrantInfoDto } from '@abp/ng.permission-management/proxy';
import { ResourcePermissionStateService } from '../../../services/resource-permission-state.service';
import { ePermissionManagementComponents } from '../../../enums/components';
import { configureResourcePermissionExtensions } from '../../../services/extensions.service';

@Component({
    selector: 'abp-resource-permission-list',
    template: `
        <div class="d-grid gap-2 mb-3 d-md-flex justify-content-md-end">
            <button class="btn btn-sm btn-primary" type="button" (click)="addClicked.emit()">
                <i class="fa fa-plus me-1"></i>
                {{ 'AbpPermissionManagement::AddResourcePermission' | abpLocalization }}
            </button>
        </div>

        <ng-template #actionsTemplate let-row>
            <div class="btn-group btn-group-sm" role="group">
                <button
                    class="btn btn-outline-primary"
                    type="button"
                    (click)="editClicked.emit(row)"
                    [title]="'AbpUi::Edit' | abpLocalization"
                >
                    <i class="fa fa-edit"></i>
                </button>
                <button
                    class="btn btn-outline-danger"
                    type="button"
                    (click)="deleteClicked.emit(row)"
                    [title]="'AbpUi::Delete' | abpLocalization"
                >
                    <i class="fa fa-trash"></i>
                </button>
            </div>
        </ng-template>

        @if (state.resourcePermissions().length > 0) {
            <abp-extensible-table
                [data]="state.resourcePermissions()"
                [recordsTotal]="state.totalCount()"
                [list]="list"
                [actionsTemplate]="actionsTemplate"
                actionsText="AbpUi::Actions"
            ></abp-extensible-table>
        } @else {
            <div class="alert alert-info">
                {{ 'AbpPermissionManagement::NoPermissionsAssigned' | abpLocalization }}
            </div>
        }
    `,
    providers: [
        ListService,
        {
            provide: EXTENSIONS_IDENTIFIER,
            useValue: ePermissionManagementComponents.ResourcePermissions,
        },
    ],
    imports: [LocalizationPipe, ExtensibleTableComponent],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResourcePermissionListComponent {
    readonly state = inject(ResourcePermissionStateService);
    readonly list = inject(ListService);

    readonly addClicked = output<void>();
    readonly editClicked = output<ResourcePermissionGrantInfoDto>();
    readonly deleteClicked = output<ResourcePermissionGrantInfoDto>();

    constructor() {
        configureResourcePermissionExtensions();
    }
}
