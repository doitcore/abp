import { TagDto } from '@abp/ng.cms-kit/proxy';
import { EntityAction } from '@abp/ng.components/extensible';
import { TagAdminService } from '@abp/ng.cms-kit/proxy';
import { ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { ListService } from '@abp/ng.core';
import { TagListComponent } from '../../components/tags/tag-list/tag-list.component';

export const DEFAULT_TAG_ENTITY_ACTIONS = EntityAction.createMany<TagDto>([
  {
    text: 'AbpUi::Edit',
    action: data => {
      const component = data.getInjected(TagListComponent);
      component.edit(data.record.id!);
    },
    permission: 'CmsKit.Tags.Update',
  },
  {
    text: 'AbpUi::Delete',
    action: data => {
      const tagService = data.getInjected(TagAdminService);
      const confirmationService = data.getInjected(ConfirmationService);
      const list = data.getInjected(ListService);

      confirmationService
        .warn('CmsKit::TagDeletionConfirmationMessage', 'AbpUi::AreYouSure', {
          yesText: 'AbpUi::Yes',
          cancelText: 'AbpUi::Cancel',
          messageLocalizationParams: [data.record.name || ''],
        })
        .subscribe((status: Confirmation.Status) => {
          if (status === Confirmation.Status.confirm) {
            tagService.delete(data.record.id!).subscribe(() => {
              list.get();
            });
          }
        });
    },
    permission: 'CmsKit.Tags.Delete',
  },
]);
