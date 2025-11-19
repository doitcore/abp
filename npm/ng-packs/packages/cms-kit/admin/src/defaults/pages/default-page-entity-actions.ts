import { Router } from '@angular/router';
import { ListService } from '@abp/ng.core';
import { EntityAction } from '@abp/ng.components/extensible';
import { ConfirmationService, Confirmation, ToasterService } from '@abp/ng.theme.shared';
import { PageAdminService, PageDto } from '@abp/ng.cms-kit/proxy';

export const DEFAULT_PAGE_ENTITY_ACTIONS = EntityAction.createMany<PageDto>([
  {
    text: 'AbpUi::Edit',
    action: data => {
      const router = data.getInjected(Router);
      router.navigate(['/cms/pages/update', data.record.id]);
    },
    permission: 'CmsKit.Pages.Update',
  },
  {
    text: 'AbpUi::Delete',
    action: data => {
      const pageService = data.getInjected(PageAdminService);
      const confirmationService = data.getInjected(ConfirmationService);
      const list = data.getInjected(ListService);

      confirmationService
        .warn('CmsKit::PageDeletionConfirmationMessage', 'AbpUi::AreYouSure', {
          yesText: 'AbpUi::Yes',
          cancelText: 'AbpUi::Cancel',
        })
        .subscribe((status: Confirmation.Status) => {
          if (status === Confirmation.Status.confirm) {
            pageService.delete(data.record.id!).subscribe(() => {
              list.get();
            });
          }
        });
    },
    permission: 'CmsKit.Pages.Delete',
  },
  {
    text: 'CmsKit::SetAsHomePage',
    action: data => {
      const pageService = data.getInjected(PageAdminService);
      const list = data.getInjected(ListService);
      const toasterService = data.getInjected(ToasterService);

      pageService.setAsHomePage(data.record.id!).subscribe(() => {
        list.get();
        if (data.record.isHomePage) {
          toasterService.warn('CmsKit::RemovedSettingAsHomePage');
        } else {
          toasterService.success('CmsKit::CompletedSettingAsHomePage');
        }
      });
    },
    permission: 'CmsKit.Pages.SetAsHomePage',
  },
]);
