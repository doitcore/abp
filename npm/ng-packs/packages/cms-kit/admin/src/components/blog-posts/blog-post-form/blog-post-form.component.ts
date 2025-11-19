import { Component, OnInit, inject, Injector, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup, FormControl } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { NgxValidateCoreModule } from '@ngx-validate/core';
import { LocalizationPipe } from '@abp/ng.core';
import {
  ExtensibleFormComponent,
  FormPropData,
  generateFormFromProps,
  EXTENSIONS_IDENTIFIER,
} from '@abp/ng.components/extensible';
import { PageComponent } from '@abp/ng.components/page';
import { ButtonComponent } from '@abp/ng.theme.shared';
import { ToastuiEditorComponent, prepareSlugFromControl } from '@abp/ng.cms-kit';
import { BlogPostAdminService, BlogPostDto } from '@abp/ng.cms-kit/proxy';
import { eCmsKitAdminComponents } from '../../../enums';
import { BlogPostFormService } from '../../../services';

@Component({
  selector: 'abp-blog-post-form',
  templateUrl: './blog-post-form.component.html',
  providers: [
    {
      provide: EXTENSIONS_IDENTIFIER,
      useValue: eCmsKitAdminComponents.BlogPostForm,
    },
  ],
  imports: [
    ButtonComponent,
    ExtensibleFormComponent,
    PageComponent,
    ToastuiEditorComponent,
    LocalizationPipe,
    ReactiveFormsModule,
    CommonModule,
    NgxValidateCoreModule,
  ],
})
export class BlogPostFormComponent implements OnInit {
  private blogPostService = inject(BlogPostAdminService);
  private injector = inject(Injector);
  private blogPostFormService = inject(BlogPostFormService);
  private route = inject(ActivatedRoute);
  private destroyRef = inject(DestroyRef);

  form: FormGroup;
  blogPost: BlogPostDto | null = null;
  blogPostId: string | null = null;
  isEditMode = false;

  ngOnInit() {
    const id = this.route.snapshot.params['id'];
    if (id) {
      this.isEditMode = true;
      this.blogPostId = id;
      this.loadBlogPost(id);
    } else {
      this.isEditMode = false;
      this.buildForm();
    }
  }

  private loadBlogPost(id: string) {
    this.blogPostService.get(id).subscribe(blogPost => {
      this.blogPost = blogPost;
      this.buildForm();
    });
  }

  private buildForm() {
    const data = new FormPropData(this.injector, this.blogPost || {});
    const baseForm = generateFormFromProps(data);
    this.form = new FormGroup({
      ...baseForm.controls,
      content: new FormControl(this.blogPost?.content || ''),
    });
    prepareSlugFromControl(this.form, 'title', 'slug', this.destroyRef);
  }

  private executeSaveOperation(operation: 'save' | 'draft' | 'publish' | 'sendToReview') {
    if (this.isEditMode) {
      if (!this.blogPost || !this.blogPostId) {
        return;
      }
      this.blogPostFormService.update(this.blogPostId, this.form, this.blogPost).subscribe();
      return;
    }

    switch (operation) {
      case 'save':
        this.blogPostFormService.createAsDraft(this.form).subscribe();
        break;
      case 'draft':
        this.blogPostFormService.createAsDraft(this.form).subscribe();
        break;
      case 'publish':
        this.blogPostFormService.createAndPublish(this.form).subscribe();
        break;
      case 'sendToReview':
        this.blogPostFormService.createAndSendToReview(this.form).subscribe();
        break;
    }
  }

  saveAsDraft() {
    this.executeSaveOperation('draft');
  }

  publish() {
    this.executeSaveOperation('publish');
  }

  sendToReview() {
    this.executeSaveOperation('sendToReview');
  }
}
