import { BlogDto } from '@abp/ng.cms-kit/proxy';
import { EntityAction } from '@abp/ng.components/extensible';
import { BlogAdminService } from '@abp/ng.cms-kit/proxy';
import { ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { ListService } from '@abp/ng.core';
import { BlogListComponent } from '../../components/blogs/blog-list/blog-list.component';

export const DEFAULT_BLOG_ENTITY_ACTIONS = EntityAction.createMany<BlogDto>([
  {
    text: 'CmsKit::Features',
    action: data => {
      const component = data.getInjected(BlogListComponent);
      component.openFeatures(data.record.id!);
    },
    permission: 'CmsKit.Blogs.Features',
  },
  {
    text: 'AbpUi::Edit',
    action: data => {
      const component = data.getInjected(BlogListComponent);
      component.edit(data.record.id!);
    },
    permission: 'CmsKit.Blogs.Update',
  },
  {
    text: 'AbpUi::Delete',
    action: data => {
      const blogService = data.getInjected(BlogAdminService);
      const confirmationService = data.getInjected(ConfirmationService);
      const list = data.getInjected(ListService);

      confirmationService
        .warn('CmsKit::BlogDeletionConfirmationMessage', 'AbpUi::AreYouSure', {
          yesText: 'AbpUi::Yes',
          cancelText: 'AbpUi::Cancel',
          messageLocalizationParams: [data.record.name || ''],
        })
        .subscribe((status: Confirmation.Status) => {
          if (status === Confirmation.Status.confirm) {
            blogService.delete(data.record.id!).subscribe(() => {
              list.get();
            });
          }
        });
    },
    permission: 'CmsKit.Blogs.Delete',
  },
]);
