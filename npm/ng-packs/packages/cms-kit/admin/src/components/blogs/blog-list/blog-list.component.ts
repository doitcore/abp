import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ListService, PagedResultDto, LocalizationPipe } from '@abp/ng.core';
import { ExtensibleTableComponent, EXTENSIONS_IDENTIFIER } from '@abp/ng.components/extensible';
import { PageComponent } from '@abp/ng.components/page';
import { BlogAdminService, BlogGetListInput, BlogDto } from '@abp/ng.cms-kit/proxy';
import { eCmsKitAdminComponents } from '../../../enums';
import { BlogModalComponent, BlogModalVisibleChange } from '../blog-modal/blog-modal.component';
import {
  BlogFeaturesModalComponent,
  BlogFeaturesModalVisibleChange,
} from '../blog-features-modal/blog-features-modal.component';

@Component({
  selector: 'abp-blog-list',
  templateUrl: './blog-list.component.html',
  providers: [
    ListService,
    {
      provide: EXTENSIONS_IDENTIFIER,
      useValue: eCmsKitAdminComponents.Blogs,
    },
  ],
  imports: [
    ExtensibleTableComponent,
    PageComponent,
    LocalizationPipe,
    FormsModule,
    CommonModule,
    BlogModalComponent,
    BlogFeaturesModalComponent,
  ],
})
export class BlogListComponent implements OnInit {
  data: PagedResultDto<BlogDto> = { items: [], totalCount: 0 };

  public readonly list = inject(ListService<BlogGetListInput>);
  private blogService = inject(BlogAdminService);

  filter = '';
  isModalVisible = false;
  selected?: BlogDto;
  isFeaturesModalVisible = false;
  selectedBlogId?: string;

  ngOnInit() {
    this.hookToQuery();
  }

  onSearch() {
    this.list.filter = this.filter;
    this.list.get();
  }

  add() {
    this.selected = {} as BlogDto;
    this.isModalVisible = true;
  }

  edit(id: string) {
    this.blogService.get(id).subscribe(blog => {
      this.selected = blog;
      this.isModalVisible = true;
    });
  }

  openFeatures(id: string) {
    this.selectedBlogId = id;
    this.isFeaturesModalVisible = true;
  }

  private hookToQuery() {
    this.list
      .hookToQuery(query => {
        let filters: Partial<BlogGetListInput> = {};
        if (this.list.filter) {
          filters.filter = this.list.filter;
        }
        const input: BlogGetListInput = {
          ...query,
          ...filters,
        };
        return this.blogService.getList(input);
      })
      .subscribe(res => {
        this.data = res;
      });
  }

  onVisibleModalChange(visibilityChange: BlogModalVisibleChange) {
    if (visibilityChange.visible) {
      return;
    }
    if (visibilityChange.refresh) {
      this.list.get();
    }
    this.selected = null;
    this.isModalVisible = false;
  }

  onFeaturesModalChange(visibilityChange: BlogFeaturesModalVisibleChange) {
    if (visibilityChange.visible) {
      return;
    }
    if (visibilityChange.refresh) {
      this.list.get();
    }
    this.selectedBlogId = null;
    this.isFeaturesModalVisible = false;
  }
}
