import { eCmsKitAdminComponents } from '../enums';
import {
  EntityActionContributorCallback,
  EntityPropContributorCallback,
  ToolbarActionContributorCallback,
  CreateFormPropContributorCallback,
  EditFormPropContributorCallback,
} from '@abp/ng.components/extensible';
import { CommentWithAuthorDto, TagDto, PageDto } from '@abp/ng.cms-kit/proxy';

export type CmsKitAdminEntityActionContributors = Partial<{
  [eCmsKitAdminComponents.CommentList]: EntityActionContributorCallback<CommentWithAuthorDto>[];
  [eCmsKitAdminComponents.CommentApprove]: EntityActionContributorCallback<CommentWithAuthorDto>[];
  [eCmsKitAdminComponents.CommentDetails]: EntityActionContributorCallback<CommentWithAuthorDto>[];
  [eCmsKitAdminComponents.TagList]: EntityActionContributorCallback<TagDto>[];
  [eCmsKitAdminComponents.PageList]: EntityActionContributorCallback<PageDto>[];
}>;

export type CmsKitAdminEntityPropContributors = Partial<{
  [eCmsKitAdminComponents.CommentList]: EntityPropContributorCallback<CommentWithAuthorDto>[];
  [eCmsKitAdminComponents.CommentApprove]: EntityPropContributorCallback<CommentWithAuthorDto>[];
  [eCmsKitAdminComponents.CommentDetails]: EntityPropContributorCallback<CommentWithAuthorDto>[];
  [eCmsKitAdminComponents.TagList]: EntityPropContributorCallback<TagDto>[];
  [eCmsKitAdminComponents.PageList]: EntityPropContributorCallback<PageDto>[];
}>;

export type CmsKitAdminToolbarActionContributors = Partial<{
  [eCmsKitAdminComponents.TagList]: ToolbarActionContributorCallback<TagDto[]>[];
  [eCmsKitAdminComponents.PageList]: ToolbarActionContributorCallback<PageDto[]>[];
}>;

export type CmsKitAdminCreateFormPropContributors = Partial<{
  [eCmsKitAdminComponents.TagList]: CreateFormPropContributorCallback<TagDto>[];
  [eCmsKitAdminComponents.PageList]: CreateFormPropContributorCallback<PageDto>[];
  [eCmsKitAdminComponents.PageCreate]: CreateFormPropContributorCallback<PageDto>[];
}>;

export type CmsKitAdminEditFormPropContributors = Partial<{
  [eCmsKitAdminComponents.TagList]: EditFormPropContributorCallback<TagDto>[];
  [eCmsKitAdminComponents.PageList]: EditFormPropContributorCallback<PageDto>[];
  [eCmsKitAdminComponents.PageEdit]: EditFormPropContributorCallback<PageDto>[];
}>;

export interface CmsKitAdminConfigOptions {
  entityActionContributors?: CmsKitAdminEntityActionContributors;
  entityPropContributors?: CmsKitAdminEntityPropContributors;
  toolbarActionContributors?: CmsKitAdminToolbarActionContributors;
  createFormPropContributors?: CmsKitAdminCreateFormPropContributors;
  editFormPropContributors?: CmsKitAdminEditFormPropContributors;
}
