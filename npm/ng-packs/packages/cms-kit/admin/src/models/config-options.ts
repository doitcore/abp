import { eCmsKitAdminComponents } from '../enums';
import {
  EntityActionContributorCallback,
  EntityPropContributorCallback,
  ToolbarActionContributorCallback,
} from '@abp/ng.components/extensible';
import { CommentWithAuthorDto, TagDto } from '@abp/ng.cms-kit/proxy';

export type CmsKitAdminEntityActionContributors = Partial<{
  [eCmsKitAdminComponents.CommentList]: EntityActionContributorCallback<CommentWithAuthorDto>[];
  [eCmsKitAdminComponents.CommentApprove]: EntityActionContributorCallback<CommentWithAuthorDto>[];
  [eCmsKitAdminComponents.CommentDetails]: EntityActionContributorCallback<CommentWithAuthorDto>[];
  [eCmsKitAdminComponents.TagList]: EntityActionContributorCallback<TagDto>[];
}>;

export type CmsKitAdminEntityPropContributors = Partial<{
  [eCmsKitAdminComponents.CommentList]: EntityPropContributorCallback<CommentWithAuthorDto>[];
  [eCmsKitAdminComponents.CommentApprove]: EntityPropContributorCallback<CommentWithAuthorDto>[];
  [eCmsKitAdminComponents.CommentDetails]: EntityPropContributorCallback<CommentWithAuthorDto>[];
  [eCmsKitAdminComponents.TagList]: EntityPropContributorCallback<TagDto>[];
}>;

export type CmsKitAdminToolbarActionContributors = Partial<{
  [eCmsKitAdminComponents.TagList]: ToolbarActionContributorCallback<TagDto[]>[];
}>;

export type CmsKitAdminCreateFormPropContributors = Partial<{}>;

export type CmsKitAdminEditFormPropContributors = Partial<{}>;

export interface CmsKitAdminConfigOptions {
  entityActionContributors?: CmsKitAdminEntityActionContributors;
  entityPropContributors?: CmsKitAdminEntityPropContributors;
  toolbarActionContributors?: CmsKitAdminToolbarActionContributors;
  createFormPropContributors?: CmsKitAdminCreateFormPropContributors;
  editFormPropContributors?: CmsKitAdminEditFormPropContributors;
}
