import { CommentWithAuthorDto, TagDto, PageDto } from '@abp/ng.cms-kit/proxy';
import {
  EntityActionContributorCallback,
  EntityPropContributorCallback,
  ToolbarActionContributorCallback,
  CreateFormPropContributorCallback,
  EditFormPropContributorCallback,
} from '@abp/ng.components/extensible';
import { InjectionToken } from '@angular/core';
import { DEFAULT_COMMENT_ENTITY_ACTIONS } from '../defaults/comments/default-comment-entity-actions';
import { DEFAULT_COMMENT_ENTITY_PROPS } from '../defaults/comments/default-comment-entity-props';
import {
  DEFAULT_TAG_ENTITY_ACTIONS,
  DEFAULT_TAG_ENTITY_PROPS,
  DEFAULT_TAG_TOOLBAR_ACTIONS,
  DEFAULT_TAG_CREATE_FORM_PROPS,
  DEFAULT_TAG_EDIT_FORM_PROPS,
} from '../defaults/tags';
import {
  DEFAULT_PAGE_ENTITY_ACTIONS,
  DEFAULT_PAGE_ENTITY_PROPS,
  DEFAULT_PAGE_TOOLBAR_ACTIONS,
  DEFAULT_PAGE_CREATE_FORM_PROPS,
  DEFAULT_PAGE_EDIT_FORM_PROPS,
} from '../defaults/pages';
import { eCmsKitAdminComponents } from '../enums';

export const DEFAULT_CMS_KIT_ADMIN_ENTITY_ACTIONS = {
  [eCmsKitAdminComponents.CommentList]: DEFAULT_COMMENT_ENTITY_ACTIONS,
  [eCmsKitAdminComponents.CommentApprove]: DEFAULT_COMMENT_ENTITY_ACTIONS,
  [eCmsKitAdminComponents.CommentDetails]: DEFAULT_COMMENT_ENTITY_ACTIONS,
  [eCmsKitAdminComponents.TagList]: DEFAULT_TAG_ENTITY_ACTIONS,
  [eCmsKitAdminComponents.PageList]: DEFAULT_PAGE_ENTITY_ACTIONS,
};

export const DEFAULT_CMS_KIT_ADMIN_ENTITY_PROPS = {
  [eCmsKitAdminComponents.CommentList]: DEFAULT_COMMENT_ENTITY_PROPS,
  [eCmsKitAdminComponents.CommentApprove]: DEFAULT_COMMENT_ENTITY_PROPS,
  [eCmsKitAdminComponents.CommentDetails]: DEFAULT_COMMENT_ENTITY_PROPS,
  [eCmsKitAdminComponents.TagList]: DEFAULT_TAG_ENTITY_PROPS,
  [eCmsKitAdminComponents.PageList]: DEFAULT_PAGE_ENTITY_PROPS,
};

export const DEFAULT_CMS_KIT_ADMIN_TOOLBAR_ACTIONS = {
  [eCmsKitAdminComponents.TagList]: DEFAULT_TAG_TOOLBAR_ACTIONS,
  [eCmsKitAdminComponents.PageList]: DEFAULT_PAGE_TOOLBAR_ACTIONS,
};

export const DEFAULT_CMS_KIT_ADMIN_CREATE_FORM_PROPS = {
  [eCmsKitAdminComponents.TagList]: DEFAULT_TAG_CREATE_FORM_PROPS,
  [eCmsKitAdminComponents.PageList]: DEFAULT_PAGE_CREATE_FORM_PROPS,
  [eCmsKitAdminComponents.PageCreate]: DEFAULT_PAGE_CREATE_FORM_PROPS,
};

export const DEFAULT_CMS_KIT_ADMIN_EDIT_FORM_PROPS = {
  [eCmsKitAdminComponents.TagList]: DEFAULT_TAG_EDIT_FORM_PROPS,
  [eCmsKitAdminComponents.PageList]: DEFAULT_PAGE_EDIT_FORM_PROPS,
  [eCmsKitAdminComponents.PageEdit]: DEFAULT_PAGE_EDIT_FORM_PROPS,
};

export const CMS_KIT_ADMIN_ENTITY_ACTION_CONTRIBUTORS =
  new InjectionToken<EntityActionContributors>('CMS_KIT_ADMIN_ENTITY_ACTION_CONTRIBUTORS');

export const CMS_KIT_ADMIN_ENTITY_PROP_CONTRIBUTORS = new InjectionToken<EntityPropContributors>(
  'CMS_KIT_ADMIN_ENTITY_PROP_CONTRIBUTORS',
);

export const CMS_KIT_ADMIN_TOOLBAR_ACTION_CONTRIBUTORS =
  new InjectionToken<ToolbarActionContributors>('CMS_KIT_ADMIN_TOOLBAR_ACTION_CONTRIBUTORS');

export const CMS_KIT_ADMIN_CREATE_FORM_PROP_CONTRIBUTORS =
  new InjectionToken<CreateFormPropContributors>('CMS_KIT_ADMIN_CREATE_FORM_PROP_CONTRIBUTORS');

export const CMS_KIT_ADMIN_EDIT_FORM_PROP_CONTRIBUTORS =
  new InjectionToken<EditFormPropContributors>('CMS_KIT_ADMIN_EDIT_FORM_PROP_CONTRIBUTORS');

// Fix for TS4023 -> https://github.com/microsoft/TypeScript/issues/9944#issuecomment-254693497
type EntityActionContributors = Partial<{
  [eCmsKitAdminComponents.CommentList]: EntityActionContributorCallback<CommentWithAuthorDto>[];
  [eCmsKitAdminComponents.CommentApprove]: EntityActionContributorCallback<CommentWithAuthorDto>[];
  [eCmsKitAdminComponents.CommentDetails]: EntityActionContributorCallback<CommentWithAuthorDto>[];
  [eCmsKitAdminComponents.TagList]: EntityActionContributorCallback<TagDto>[];
  [eCmsKitAdminComponents.PageList]: EntityActionContributorCallback<PageDto>[];
}>;

type EntityPropContributors = Partial<{
  [eCmsKitAdminComponents.CommentList]: EntityPropContributorCallback<CommentWithAuthorDto>[];
  [eCmsKitAdminComponents.CommentApprove]: EntityPropContributorCallback<CommentWithAuthorDto>[];
  [eCmsKitAdminComponents.CommentDetails]: EntityPropContributorCallback<CommentWithAuthorDto>[];
  [eCmsKitAdminComponents.TagList]: EntityPropContributorCallback<TagDto>[];
  [eCmsKitAdminComponents.PageList]: EntityPropContributorCallback<PageDto>[];
}>;

type ToolbarActionContributors = Partial<{
  [eCmsKitAdminComponents.TagList]: ToolbarActionContributorCallback<TagDto[]>[];
  [eCmsKitAdminComponents.PageList]: ToolbarActionContributorCallback<PageDto[]>[];
}>;

type CreateFormPropContributors = Partial<{
  [eCmsKitAdminComponents.TagList]: CreateFormPropContributorCallback<TagDto>[];
  [eCmsKitAdminComponents.PageList]: CreateFormPropContributorCallback<PageDto>[];
}>;

type EditFormPropContributors = Partial<{
  [eCmsKitAdminComponents.TagList]: EditFormPropContributorCallback<TagDto>[];
  [eCmsKitAdminComponents.PageList]: EditFormPropContributorCallback<PageDto>[];
}>;
