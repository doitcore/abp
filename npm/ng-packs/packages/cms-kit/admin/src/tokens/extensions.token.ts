import { CommentWithAuthorDto } from '@abp/ng.cms-kit/proxy';
import {
  EntityActionContributorCallback,
  EntityPropContributorCallback,
} from '@abp/ng.components/extensible';
import { InjectionToken } from '@angular/core';
import { DEFAULT_COMMENT_ENTITY_ACTIONS } from '../defaults/comments/default-comment-entity-actions';
import { DEFAULT_COMMENT_ENTITY_PROPS } from '../defaults/comments/default-comment-entity-props';
import { eCmsKitAdminComponents } from '../enums';

export const DEFAULT_CMS_KIT_ADMIN_ENTITY_ACTIONS = {
  [eCmsKitAdminComponents.CommentList]: DEFAULT_COMMENT_ENTITY_ACTIONS,
  [eCmsKitAdminComponents.CommentApprove]: DEFAULT_COMMENT_ENTITY_ACTIONS,
  [eCmsKitAdminComponents.CommentDetails]: DEFAULT_COMMENT_ENTITY_ACTIONS,
};

export const DEFAULT_CMS_KIT_ADMIN_ENTITY_PROPS = {
  [eCmsKitAdminComponents.CommentList]: DEFAULT_COMMENT_ENTITY_PROPS,
  [eCmsKitAdminComponents.CommentApprove]: DEFAULT_COMMENT_ENTITY_PROPS,
  [eCmsKitAdminComponents.CommentDetails]: DEFAULT_COMMENT_ENTITY_PROPS,
};

export const CMS_KIT_ADMIN_ENTITY_ACTION_CONTRIBUTORS =
  new InjectionToken<EntityActionContributors>('CMS_KIT_ADMIN_ENTITY_ACTION_CONTRIBUTORS');

export const CMS_KIT_ADMIN_ENTITY_PROP_CONTRIBUTORS = new InjectionToken<EntityPropContributors>(
  'CMS_KIT_ADMIN_ENTITY_PROP_CONTRIBUTORS',
);

// Fix for TS4023 -> https://github.com/microsoft/TypeScript/issues/9944#issuecomment-254693497
type EntityActionContributors = Partial<{
  [eCmsKitAdminComponents.CommentList]: EntityActionContributorCallback<CommentWithAuthorDto>[];
  [eCmsKitAdminComponents.CommentApprove]: EntityActionContributorCallback<CommentWithAuthorDto>[];
  [eCmsKitAdminComponents.CommentDetails]: EntityActionContributorCallback<CommentWithAuthorDto>[];
}>;

type EntityPropContributors = Partial<{
  [eCmsKitAdminComponents.CommentList]: EntityPropContributorCallback<CommentWithAuthorDto>[];
  [eCmsKitAdminComponents.CommentApprove]: EntityPropContributorCallback<CommentWithAuthorDto>[];
  [eCmsKitAdminComponents.CommentDetails]: EntityPropContributorCallback<CommentWithAuthorDto>[];
}>;
