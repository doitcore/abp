import { Component, OnInit, inject, LOCALE_ID } from '@angular/core';
import { CommonModule, DatePipe, formatDate } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NgbDatepickerModule } from '@ng-bootstrap/ng-bootstrap';
import { ListService, LocalizationPipe, PagedResultDto, ConfigStateService } from '@abp/ng.core';
import { PageComponent } from '@abp/ng.components/page';
import { ExtensibleTableComponent, EXTENSIONS_IDENTIFIER } from '@abp/ng.components/extensible';
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
  selector: 'abp-comment-details',
  templateUrl: './comment-details.component.html',
  providers: [
    ListService,
    {
      provide: EXTENSIONS_IDENTIFIER,
      useValue: eCmsKitAdminComponents.CommentDetails,
    },
  ],
  imports: [
    ExtensibleTableComponent,
    PageComponent,
    LocalizationPipe,
    ReactiveFormsModule,
    NgbDatepickerModule,
    CommonModule,
    DatePipe,
    FormInputComponent,
    ButtonComponent,
  ],
})
export class CommentDetailsComponent implements OnInit {
  comment: CommentWithAuthorDto | null = null;
  data: PagedResultDto<CommentWithAuthorDto> = { items: [], totalCount: 0 };

  public readonly list = inject(ListService<CommentGetListInput>);
  private commentService = inject(CommentAdminService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private fb = inject(FormBuilder);
  private configState = inject(ConfigStateService);
  private locale = inject(LOCALE_ID);

  filterForm!: FormGroup;
  commentApproveStateOptions = commentApproveStateOptions;
  requireApprovement = false;
  commentId!: string;

  ngOnInit() {
    this.requireApprovement =
      this.configState.getSetting(CMS_KIT_COMMENTS_REQUIRE_APPROVEMENT) === 'true';
    this.route.params.subscribe(params => {
      const id = params['id'];
      if (id) {
        this.commentId = id;
        this.loadComment(id);
        this.createFilterForm();
        this.hookToQuery();
      }
    });
  }

  private createFilterForm() {
    this.filterForm = this.fb.group({
      creationStartDate: [null],
      creationEndDate: [null],
      author: [''],
      commentApproveState: [CommentApproveState.All],
    });
  }

  private loadComment(id: string) {
    this.commentService.get(id).subscribe(comment => {
      this.comment = comment;
    });
  }

  onFilter() {
    const formValue = this.filterForm.value;
    const filters: Partial<CommentGetListInput> = {
      author: formValue.author || undefined,
      commentApproveState: formValue.commentApproveState,
      repliedCommentId: this.commentId,
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
          repliedCommentId: this.commentId,
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

  navigateToReply(id: string) {
    this.router.navigate(['/cms/comments', id]);
  }
}
