import { Component, OnInit, inject, LOCALE_ID } from '@angular/core';
import { CommonModule, formatDate } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { NgbDatepickerModule } from '@ng-bootstrap/ng-bootstrap';
import { ListService, LocalizationPipe, PagedResultDto } from '@abp/ng.core';
import { ExtensibleTableComponent, EXTENSIONS_IDENTIFIER } from '@abp/ng.components/extensible';
import { PageComponent } from '@abp/ng.components/page';
import { ButtonComponent, FormInputComponent } from '@abp/ng.theme.shared';
import {
  CommentAdminService,
  CommentGetListInput,
  CommentWithAuthorDto,
  CommentApproveState,
} from '@abp/ng.cms-kit/proxy';
import { eCmsKitAdminComponents } from '../../../enums';

@Component({
  selector: 'abp-comment-approve',
  templateUrl: './comment-approve.component.html',
  providers: [
    ListService,
    {
      provide: EXTENSIONS_IDENTIFIER,
      useValue: eCmsKitAdminComponents.CommentApprove,
    },
  ],
  imports: [
    ReactiveFormsModule,
    NgbDatepickerModule,
    CommonModule,
    PageComponent,
    ButtonComponent,
    FormInputComponent,
    LocalizationPipe,
    ExtensibleTableComponent,
  ],
})
export class CommentApproveComponent implements OnInit {
  data: PagedResultDto<CommentWithAuthorDto> = { items: [], totalCount: 0 };

  public readonly list = inject(ListService<CommentGetListInput>);
  private commentService = inject(CommentAdminService);
  private fb = inject(FormBuilder);
  private locale = inject(LOCALE_ID);

  filterForm!: FormGroup;

  ngOnInit() {
    this.createFilterForm();
    this.hookToQuery();
  }

  private createFilterForm() {
    this.filterForm = this.fb.group({
      creationStartDate: [null],
      creationEndDate: [null],
      author: [''],
      entityType: [''],
    });
  }

  onFilter() {
    const formValue = this.filterForm.value;
    const filters: Partial<CommentGetListInput> = {
      author: formValue.author || undefined,
      entityType: formValue.entityType || undefined,
      commentApproveState: CommentApproveState.Waiting,
    };

    if (formValue.creationStartDate) {
      filters.creationStartDate = this.formatDateForApi(formValue.creationStartDate);
    }

    if (formValue.creationEndDate) {
      filters.creationEndDate = this.formatDateForApi(formValue.creationEndDate);
    }

    this.list.filter = JSON.stringify(filters);
    this.list.get();
  }

  private formatDateForApi(date: any): string {
    if (!date) {
      return '';
    }

    if (typeof date === 'string') {
      return date;
    }

    if (date.year && date.month && date.day) {
      const jsDate = new Date(date.year, date.month - 1, date.day);
      return formatDate(jsDate, 'yyyy-MM-dd', this.locale);
    }

    return '';
  }

  private hookToQuery() {
    this.list
      .hookToQuery(query => {
        let filters: Partial<CommentGetListInput> = {
          commentApproveState: CommentApproveState.Waiting,
        };
        if (this.list.filter) {
          try {
            filters = { ...filters, ...JSON.parse(this.list.filter) };
          } catch {
            // Ignore parse errors, use default filters
          }
        }
        const input: CommentGetListInput = {
          ...query,
          ...filters,
        };
        return this.commentService.getList(input);
      })
      .subscribe(res => {
        this.data = res;
      });
  }
}
