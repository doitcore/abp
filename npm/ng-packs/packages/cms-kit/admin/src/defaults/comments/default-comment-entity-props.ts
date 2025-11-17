import { CommentWithAuthorDto } from '@abp/ng.cms-kit/proxy';
import { EntityProp, ePropType } from '@abp/ng.components/extensible';
import { ConfigStateService } from '@abp/ng.core';
import { of } from 'rxjs';

export const DEFAULT_COMMENT_ENTITY_PROPS = EntityProp.createMany<CommentWithAuthorDto>([
  {
    type: ePropType.String,
    name: 'author.userName',
    displayName: 'CmsKit::Username',
    sortable: false,
    columnWidth: 150,
  },
  {
    type: ePropType.String,
    name: 'entityType',
    displayName: 'CmsKit::EntityType',
    sortable: false,
    columnWidth: 200,
  },
  {
    type: ePropType.String,
    name: 'url',
    displayName: 'CmsKit::URL',
    sortable: false,
    valueResolver: data => {
      const url = data.record.url;
      if (url) {
        return of(
          `<a href="${url}#comment-${data.record.id}" target="_blank"><i class="fa fa-location-arrow"></i></a>`,
        );
      }
      return of('');
    },
  },
  {
    type: ePropType.String,
    name: 'text',
    displayName: 'CmsKit::Text',
    sortable: false,
    valueResolver: data => {
      const text = data.record.text || '';
      const maxChars = 64;
      if (text.length > maxChars) {
        return of(text.substring(0, maxChars) + '...');
      }
      return of(text);
    },
  },
  {
    type: ePropType.Boolean,
    name: 'isApproved',
    displayName: 'CmsKit::ApproveState',
    sortable: false,
    columnWidth: 100,
    columnVisible: getInjected => {
      const configState = getInjected(ConfigStateService);
      return configState.getSetting('CmsKit.Comments.RequireApprovement') === 'true';
    },
    valueResolver: data => {
      const isApproved = data.record.isApproved;
      if (isApproved === null || isApproved === undefined) {
        return of('<i class="fa-solid fa-hourglass-half text-muted"></i>');
      } else if (isApproved === true) {
        return of('<i class="fa-solid fa-check text-success"></i>');
      } else {
        return of('<i class="fa-solid fa-x text-danger"></i>');
      }
    },
  },
  {
    type: ePropType.Date,
    name: 'creationTime',
    displayName: 'AbpIdentity::CreationTime',
    sortable: true,
    columnWidth: 200,
  },
]);
