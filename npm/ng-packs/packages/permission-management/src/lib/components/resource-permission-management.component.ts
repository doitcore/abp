import { LocalizationPipe } from '@abp/ng.core';
import {
    ButtonComponent,
    ModalCloseDirective,
    ModalComponent,
    ToasterService,
} from '@abp/ng.theme.shared';
import {
    GetResourcePermissionListResultDto,
    GetResourceProviderListResultDto,
    GetResourcePermissionDefinitionListResultDto,
    GetResourcePermissionWithProviderListResultDto,
    PermissionsService,
    ResourcePermissionGrantInfoDto,
    ResourceProviderDto,
    SearchProviderKeyInfo,
    ResourcePermissionDefinitionDto,
    ResourcePermissionWithProdiverGrantInfoDto,
} from '@abp/ng.permission-management/proxy';
import {
    ChangeDetectionStrategy,
    Component,
    EventEmitter,
    inject,
    Input,
    Output,
    signal,
    computed,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { finalize, switchMap, of, debounceTime, Subject, distinctUntilChanged } from 'rxjs';

type ViewMode = 'list' | 'add' | 'edit';

@Component({
    selector: 'abp-resource-permission-management',
    templateUrl: './resource-permission-management.component.html',
    exportAs: 'abpResourcePermissionManagement',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FormsModule,
        ModalComponent,
        LocalizationPipe,
        ButtonComponent,
        ModalCloseDirective,
    ],
})
export class ResourcePermissionManagementComponent {
    protected readonly service = inject(PermissionsService);
    protected readonly toasterService = inject(ToasterService);

    @Input() resourceName!: string;
    @Input() resourceKey!: string;
    @Input() resourceDisplayName?: string;

    protected _visible = false;

    @Input()
    get visible(): boolean {
        return this._visible;
    }

    set visible(value: boolean) {
        if (value === this._visible) return;

        if (value) {
            this.openModal();
        } else {
            this.resetState();
        }
        this._visible = value;
        this.visibleChange.emit(value);
    }

    @Output() readonly visibleChange = new EventEmitter<boolean>();

    // State signals
    viewMode = signal<ViewMode>('list');
    modalBusy = signal(false);
    hasResourcePermission = signal(false);
    hasProviderKeyLookupService = signal(false);

    // Data
    resourcePermissions = signal<ResourcePermissionGrantInfoDto[]>([]);
    providers = signal<ResourceProviderDto[]>([]);
    permissionDefinitions = signal<ResourcePermissionDefinitionDto[]>([]);
    searchResults = signal<SearchProviderKeyInfo[]>([]);
    permissionsWithProvider = signal<ResourcePermissionWithProdiverGrantInfoDto[]>([]);

    // Form state for add/edit
    selectedProviderName = signal<string>('');
    selectedProviderKey = signal<string>('');
    searchFilter = signal('');
    selectedPermissions = signal<string[]>([]);

    // Edit mode state
    editProviderName = signal<string>('');
    editProviderKey = signal<string>('');

    // Search subject for debounce
    private searchSubject = new Subject<string>();

    constructor() {
        this.searchSubject.pipe(
            debounceTime(300),
            distinctUntilChanged()
        ).subscribe(filter => {
            this.performSearch(filter);
        });
    }

    openModal() {
        this.modalBusy.set(true);

        // Load resource permissions and providers
        this.service.getResource(this.resourceName, this.resourceKey).pipe(
            switchMap(permRes => {
                this.resourcePermissions.set(permRes.permissions || []);
                return this.service.getResourceProviderKeyLookupServices(this.resourceName);
            }),
            switchMap(providerRes => {
                this.providers.set(providerRes.providers || []);
                this.hasProviderKeyLookupService.set((providerRes.providers?.length || 0) > 0);
                if (providerRes.providers?.length) {
                    this.selectedProviderName.set(providerRes.providers[0].name || '');
                }
                return this.service.getResourceDefinitions(this.resourceName);
            }),
            finalize(() => this.modalBusy.set(false))
        ).subscribe({
            next: defRes => {
                this.permissionDefinitions.set(defRes.permissions || []);
                this.hasResourcePermission.set((defRes.permissions?.length || 0) > 0);
            },
            error: () => {
                this.toasterService.error('AbpPermissionManagement::ErrorLoadingPermissions');
            }
        });
    }

    resetState() {
        this.viewMode.set('list');
        this.resourcePermissions.set([]);
        this.selectedProviderName.set('');
        this.selectedProviderKey.set('');
        this.searchFilter.set('');
        this.selectedPermissions.set([]);
        this.searchResults.set([]);
    }

    // View mode navigation
    goToAddMode() {
        this.viewMode.set('add');
        this.selectedPermissions.set([]);
        this.selectedProviderKey.set('');
        this.searchResults.set([]);
    }

    goToEditMode(grant: ResourcePermissionGrantInfoDto) {
        this.editProviderName.set(grant.providerName || '');
        this.editProviderKey.set(grant.providerKey || '');
        this.modalBusy.set(true);

        this.service.getResourceByProvider(
            this.resourceName,
            this.resourceKey,
            grant.providerName || '',
            grant.providerKey || ''
        ).pipe(
            finalize(() => this.modalBusy.set(false))
        ).subscribe({
            next: res => {
                this.permissionsWithProvider.set(res.permissions || []);
                this.selectedPermissions.set(
                    (res.permissions || []).filter(p => p.isGranted).map(p => p.name || '')
                );
                this.viewMode.set('edit');
            }
        });
    }

    goToListMode() {
        this.viewMode.set('list');
        this.selectedPermissions.set([]);
    }

    // Provider selection
    onProviderChange(providerName: string) {
        this.selectedProviderName.set(providerName);
        this.selectedProviderKey.set('');
        this.searchResults.set([]);
        this.searchFilter.set('');
    }

    // Search
    onSearchInput(filter: string) {
        this.searchFilter.set(filter);
        this.searchSubject.next(filter);
    }

    private performSearch(filter: string) {
        if (!filter || !this.selectedProviderName()) return;

        this.service.searchResourceProviderKey(
            this.resourceName,
            this.selectedProviderName(),
            filter,
            1
        ).subscribe(res => {
            this.searchResults.set(res.keys || []);
        });
    }

    selectProviderKey(key: SearchProviderKeyInfo) {
        this.selectedProviderKey.set(key.providerKey || '');
        this.searchFilter.set(key.providerDisplayName || key.providerKey || '');
        this.searchResults.set([]);
    }

    // Permission toggle
    togglePermission(permissionName: string) {
        const current = this.selectedPermissions();
        if (current.includes(permissionName)) {
            this.selectedPermissions.set(current.filter(p => p !== permissionName));
        } else {
            this.selectedPermissions.set([...current, permissionName]);
        }
    }

    toggleAllPermissions(selectAll: boolean) {
        if (this.viewMode() === 'add') {
            this.selectedPermissions.set(
                selectAll
                    ? this.permissionDefinitions().map(p => p.name || '')
                    : []
            );
        } else {
            this.selectedPermissions.set(
                selectAll
                    ? this.permissionsWithProvider().map(p => p.name || '')
                    : []
            );
        }
    }

    isPermissionSelected(permissionName: string): boolean {
        return this.selectedPermissions().includes(permissionName);
    }

    allPermissionsSelected = computed(() => {
        const definitions = this.viewMode() === 'add'
            ? this.permissionDefinitions()
            : this.permissionsWithProvider();
        return definitions.length > 0 &&
            definitions.every(p => this.selectedPermissions().includes(p.name || ''));
    });

    // Save operations
    saveAddPermission() {
        if (!this.selectedProviderKey() || this.selectedPermissions().length === 0) {
            this.toasterService.warn('AbpPermissionManagement::PleaseSelectProviderAndPermissions');
            return;
        }

        this.modalBusy.set(true);
        this.service.updateResource(
            this.resourceName,
            this.resourceKey,
            {
                providerName: this.selectedProviderName(),
                providerKey: this.selectedProviderKey(),
                permissions: this.selectedPermissions()
            }
        ).pipe(
            switchMap(() => this.service.getResource(this.resourceName, this.resourceKey)),
            finalize(() => this.modalBusy.set(false))
        ).subscribe({
            next: res => {
                this.resourcePermissions.set(res.permissions || []);
                this.toasterService.success('AbpUi::SavedSuccessfully');
                this.goToListMode();
            }
        });
    }

    saveEditPermission() {
        this.modalBusy.set(true);
        this.service.updateResource(
            this.resourceName,
            this.resourceKey,
            {
                providerName: this.editProviderName(),
                providerKey: this.editProviderKey(),
                permissions: this.selectedPermissions()
            }
        ).pipe(
            switchMap(() => this.service.getResource(this.resourceName, this.resourceKey)),
            finalize(() => this.modalBusy.set(false))
        ).subscribe({
            next: res => {
                this.resourcePermissions.set(res.permissions || []);
                this.toasterService.success('AbpUi::SavedSuccessfully');
                this.goToListMode();
            }
        });
    }

    deletePermission(grant: ResourcePermissionGrantInfoDto) {
        this.modalBusy.set(true);
        this.service.deleteResource(
            this.resourceName,
            this.resourceKey,
            grant.providerName || '',
            grant.providerKey || ''
        ).pipe(
            switchMap(() => this.service.getResource(this.resourceName, this.resourceKey)),
            finalize(() => this.modalBusy.set(false))
        ).subscribe({
            next: res => {
                this.resourcePermissions.set(res.permissions || []);
                this.toasterService.success('AbpUi::SuccessfullyDeleted');
            }
        });
    }
}
