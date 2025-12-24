import { Component, input, inject, output, OnInit, OnDestroy, ChangeDetectionStrategy } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LocalizationPipe } from '@abp/ng.core';
import { PermissionsService, SearchProviderKeyInfo } from '@abp/ng.permission-management/proxy';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';
import { ResourcePermissionStateService } from '../../../services/resource-permission-state.service';

@Component({
    selector: 'abp-provider-key-search',
    templateUrl: './provider-key-search.component.html',
    imports: [FormsModule, LocalizationPipe],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProviderKeySearchComponent implements OnInit, OnDestroy {
    readonly state = inject(ResourcePermissionStateService);
    private readonly service = inject(PermissionsService);

    readonly resourceName = input.required<string>();

    readonly keySelected = output<SearchProviderKeyInfo>();

    private readonly searchSubject = new Subject<string>();
    private readonly destroy$ = new Subject<void>();

    ngOnInit() {
        this.searchSubject.pipe(
            debounceTime(300),
            distinctUntilChanged(),
            takeUntil(this.destroy$)
        ).subscribe(filter => {
            this.loadProviderKeys(filter);
        });
    }

    ngOnDestroy() {
        this.destroy$.next();
        this.destroy$.complete();
    }

    onSearchInput(filter: string) {
        this.state.searchFilter.set(filter);
        this.state.showDropdown.set(true);
        this.searchSubject.next(filter);
    }

    onSearchFocus() {
        this.state.showDropdown.set(true);
        this.loadProviderKeys(this.state.searchFilter() || '');
    }

    onSearchBlur(event: FocusEvent) {
        const relatedTarget = event.relatedTarget as HTMLElement;
        if (!relatedTarget?.closest('.list-group')) {
            this.state.showDropdown.set(false);
        }
    }

    selectProviderKey(key: SearchProviderKeyInfo) {
        this.state.selectProviderKey(key);
        this.keySelected.emit(key);
    }

    private loadProviderKeys(filter: string) {
        const providerName = this.state.selectedProviderName();
        if (!providerName) return;

        this.service.searchResourceProviderKey(
            this.resourceName(),
            providerName,
            filter,
            1
        ).subscribe(res => {
            this.state.searchResults.set(res.keys || []);
        });
    }
}
