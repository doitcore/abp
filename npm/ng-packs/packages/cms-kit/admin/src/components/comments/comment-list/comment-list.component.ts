import { Component, OnInit, inject, LOCALE_ID } from '@angular/core';
import { CommonModule, formatDate } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { NgbDatepickerModule } from '@ng-bootstrap/ng-bootstrap';
import { ListService, PagedResultDto, ConfigStateService, LocalizationPipe } from '@abp/ng.core';
import { ExtensibleModule, EXTENSIONS_IDENTIFIER } from '@abp/ng.components/extensible';
import { PageModule } from '@abp/ng.components/page';
import { ButtonComponent, FormInputComponent } from '@abp/ng.theme.shared';
import {
  CommentAdminService,
  CommentGetListInput,
  CommentWithAuthorDto,
  CommentApproveState,
  commentApproveStateOptions,
} from '@abp/ng.cms-kit/proxy';
import { eCmsKitAdminComponents } from '../../../enums';
import { CMS_KIT_COMMENTS_REQUIRE_APPROVEMENT } from '../constants';

@Component({
  selector: 'abp-comment-list',
  templateUrl: './comment-list.component.html',
  providers: [
    ListService,
    {
      provide: EXTENSIONS_IDENTIFIER,
      useValue: eCmsKitAdminComponents.CommentList,
    },
  ],
  imports: [
    ExtensibleModule,
    PageModule,
    ReactiveFormsModule,
    NgbDatepickerModule,
    CommonModule,
    LocalizationPipe,
    FormInputComponent,
    ButtonComponent,
  ],
})
export class CommentListComponent implements OnInit {
  data: PagedResultDto<CommentWithAuthorDto> = { items: [], totalCount: 0 };

  public readonly list = inject(ListService<CommentGetListInput>);
  private commentService = inject(CommentAdminService);
  private fb = inject(FormBuilder);
  private configState = inject(ConfigStateService);
  private locale = inject(LOCALE_ID);

  filterForm!: FormGroup;
  commentApproveStateOptions = commentApproveStateOptions;
  requireApprovement = false;

  ngOnInit() {
    this.requireApprovement =
      this.configState.getSetting(CMS_KIT_COMMENTS_REQUIRE_APPROVEMENT) === 'true';
    this.createFilterForm();
    this.hookToQuery();
  }

  private createFilterForm() {
    this.filterForm = this.fb.group({
      creationStartDate: [null],
      creationEndDate: [null],
      author: [''],
      entityType: [''],
      commentApproveState: [CommentApproveState.All],
    });
  }

  onFilter() {
    const formValue = this.filterForm.value;
    const filters: Partial<CommentGetListInput> = {
      author: formValue.author || undefined,
      entityType: formValue.entityType || undefined,
      commentApproveState: formValue.commentApproveState,
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
        let filters: Partial<CommentGetListInput> = {};
        if (this.list.filter) {
          try {
            filters = JSON.parse(this.list.filter);
          } catch {
            // Ignore parse errors, use empty filters
          }
        }
        const input: CommentGetListInput = {
          ...query,
          ...filters,
        };
        return this.commentService.getList(input);
      })
      .subscribe(res => (this.data = res));
  }
}
