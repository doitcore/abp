import { Component, OnInit, inject, Injector, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup, FormControl } from '@angular/forms';
import { NgxValidateCoreModule } from '@ngx-validate/core';
import { LocalizationPipe } from '@abp/ng.core';
import {
  ExtensibleFormComponent,
  FormPropData,
  generateFormFromProps,
} from '@abp/ng.components/extensible';
import {
  ModalComponent,
  ModalCloseDirective,
  ButtonComponent,
  ToasterService,
} from '@abp/ng.theme.shared';
import {
  MenuItemAdminService,
  MenuItemDto,
  MenuItemWithDetailsDto,
  MenuItemCreateInput,
  MenuItemUpdateInput,
  PageLookupDto,
} from '@abp/ng.cms-kit/proxy';
import { NgbNavModule } from '@ng-bootstrap/ng-bootstrap';
import { forkJoin } from 'rxjs';

export interface MenuItemModalVisibleChange {
  visible: boolean;
  refresh: boolean;
}

@Component({
  selector: 'abp-menu-item-modal',
  templateUrl: './menu-item-modal.component.html',
  imports: [
    ExtensibleFormComponent,
    LocalizationPipe,
    ReactiveFormsModule,
    CommonModule,
    NgxValidateCoreModule,
    ModalComponent,
    ModalCloseDirective,
    ButtonComponent,
    NgbNavModule,
  ],
})
export class MenuItemModalComponent implements OnInit {
  private menuItemService = inject(MenuItemAdminService);
  private injector = inject(Injector);
  private toasterService = inject(ToasterService);

  selected = input<MenuItemWithDetailsDto | MenuItemDto>();
  parentId = input<string | null>();
  visible = input<boolean>(true);
  visibleChange = output<MenuItemModalVisibleChange>();

  form: FormGroup;
  activeTab: 'url' | 'page' = 'url';
  pages: PageLookupDto[] = [];
  selectedPage: PageLookupDto | null = null;

  ngOnInit() {
    const selectedItem = this.selected();

    if (selectedItem?.id) {
      // Load menu item and pages in parallel
      forkJoin({
        menuItem: this.menuItemService.get(selectedItem.id),
        pages: this.menuItemService.getPageLookup({
          maxResultCount: 1000,
        }),
      }).subscribe(({ menuItem, pages }) => {
        this.pages = pages.items || [];
        this.buildForm(menuItem);
        if (menuItem.pageId) {
          this.activeTab = 'page';
          this.selectedPage = this.pages.find(p => p.id === menuItem.pageId) || null;
        }
      });
    } else {
      // Load pages for create mode
      this.loadPages();
      this.buildForm();
    }
  }

  private loadPages() {
    this.menuItemService
      .getPageLookup({
        maxResultCount: 1000,
      })
      .subscribe(result => {
        this.pages = result.items || [];
      });
  }

  private buildForm(menuItem?: MenuItemWithDetailsDto | MenuItemDto) {
    const data = new FormPropData(this.injector, menuItem || {});
    const baseForm = generateFormFromProps(data);

    const parentId = this.parentId() || menuItem?.parentId || null;

    this.form = new FormGroup({
      ...baseForm.controls,
      url: new FormControl(menuItem?.url || ''),
      pageId: new FormControl(menuItem?.pageId || null),
    });

    if (parentId === null && !menuItem?.id) {
      this.menuItemService.getAvailableMenuOrder().subscribe(order => {
        this.form.patchValue({ order });
      });
    } else if (parentId && !menuItem?.id) {
      this.menuItemService.getAvailableMenuOrder(parentId).subscribe(order => {
        this.form.patchValue({ order });
      });
    }
  }

  onVisibleChange(visible: boolean, refresh = false) {
    this.visibleChange.emit({ visible, refresh });
  }

  save() {
    if (!this.form.valid) {
      return;
    }

    const formValue = this.form.value;
    const selectedItem = this.selected();

    // If page is selected, clear URL; if URL is used, clear pageId
    if (this.activeTab === 'page' && formValue.pageId) {
      formValue.url = '';
    } else if (this.activeTab === 'url') {
      formValue.pageId = null;
    }

    let observable$;

    if (selectedItem?.id) {
      const updateInput: MenuItemUpdateInput = {
        ...formValue,
        concurrencyStamp: (selectedItem as MenuItemWithDetailsDto).concurrencyStamp,
      };
      observable$ = this.menuItemService.update(selectedItem.id, updateInput);
    } else {
      const createInput: MenuItemCreateInput = {
        ...formValue,
      };
      observable$ = this.menuItemService.create(createInput);
    }

    observable$.subscribe({
      next: () => {
        this.onVisibleChange(false, true);
        this.toasterService.success('AbpUi::SavedSuccessfully');
      },
    });
  }
}
