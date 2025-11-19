import { Router } from '@angular/router';
import { ListService } from '@abp/ng.core';
import { EntityAction } from '@abp/ng.components/extensible';
import { ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { BlogPostAdminService, BlogPostListDto } from '@abp/ng.cms-kit/proxy';

export const DEFAULT_BLOG_POST_ENTITY_ACTIONS = EntityAction.createMany<BlogPostListDto>([
  {
    text: 'AbpUi::Edit',
    action: data => {
      const router = data.getInjected(Router);
      router.navigate(['/cms/blog-posts/update', data.record.id]);
    },
    permission: 'CmsKit.BlogPosts.Update',
  },
  {
    text: 'AbpUi::Delete',
    action: data => {
      const blogPostService = data.getInjected(BlogPostAdminService);
      const confirmationService = data.getInjected(ConfirmationService);
      const list = data.getInjected(ListService);

      confirmationService
        .warn('CmsKit::BlogPostDeletionConfirmationMessage', 'AbpUi::AreYouSure', {
          yesText: 'AbpUi::Yes',
          cancelText: 'AbpUi::Cancel',
        })
        .subscribe((status: Confirmation.Status) => {
          if (status === Confirmation.Status.confirm) {
            blogPostService.delete(data.record.id!).subscribe(() => {
              list.get();
            });
          }
        });
    },
    permission: 'CmsKit.BlogPosts.Delete',
  },
]);
