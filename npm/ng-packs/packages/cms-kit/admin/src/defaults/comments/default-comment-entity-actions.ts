import { CommentWithAuthorDto } from '@abp/ng.cms-kit/proxy';
import { EntityAction } from '@abp/ng.components/extensible';
import { Router } from '@angular/router';
import { CommentAdminService } from '@abp/ng.cms-kit/proxy';
import { ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { ConfigStateService, ListService } from '@abp/ng.core';

export const DEFAULT_COMMENT_ENTITY_ACTIONS = EntityAction.createMany<CommentWithAuthorDto>([
  {
    text: 'CmsKit::Details',
    action: data => {
      const router = data.getInjected(Router);
      router.navigate(['/cms/comments', data.record.id]);
    },
  },
  {
    text: 'CmsKit::Delete',
    action: data => {
      const commentService = data.getInjected(CommentAdminService);
      const confirmation = data.getInjected(ConfirmationService);
      const list = data.getInjected(ListService);

      confirmation
        .warn('CmsKit::CommentDeletionConfirmationMessage', 'AbpUi::AreYouSure', {
          yesText: 'AbpUi::Yes',
          cancelText: 'AbpUi::Cancel',
        })
        .subscribe(status => {
          if (status === Confirmation.Status.confirm) {
            commentService.delete(data.record.id!).subscribe(() => {
              list.get();
            });
          }
        });
    },
    permission: 'CmsKit.Comments.Delete',
  },
  {
    // text: data => {
    //   return data.record.isApproved ? 'CmsKit::Disapproved' : 'CmsKit::Approve';
    // },
    // TODO: Add a resolver for the text
    text: 'CmsKit::Approve',
    action: data => {
      const commentService = data.getInjected(CommentAdminService);
      const list = data.getInjected(ListService);
      const newApprovalStatus = !data.record.isApproved;

      commentService
        .updateApprovalStatus(data.record.id!, { isApproved: newApprovalStatus })
        .subscribe(() => {
          list.get();
        });
    },
    visible: data => {
      const configState = data.getInjected(ConfigStateService);
      const requireApprovement =
        configState.getSetting('CmsKit.Comments.RequireApprovement') === 'true';
      return requireApprovement;
    },
  },
]);
