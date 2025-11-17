import { eCmsKitAdminComponents } from '../enums';
import {
  EntityActionContributorCallback,
  EntityPropContributorCallback,
} from '@abp/ng.components/extensible';
import { CommentWithAuthorDto } from '@abp/ng.cms-kit/proxy';

export type CmsKitAdminEntityActionContributors = Partial<{
  [eCmsKitAdminComponents.CommentList]: EntityActionContributorCallback<CommentWithAuthorDto>[];
  [eCmsKitAdminComponents.CommentApprove]: EntityActionContributorCallback<CommentWithAuthorDto>[];
  [eCmsKitAdminComponents.CommentDetails]: EntityActionContributorCallback<CommentWithAuthorDto>[];
}>;

export type CmsKitAdminEntityPropContributors = Partial<{
  [eCmsKitAdminComponents.CommentList]: EntityPropContributorCallback<CommentWithAuthorDto>[];
  [eCmsKitAdminComponents.CommentApprove]: EntityPropContributorCallback<CommentWithAuthorDto>[];
  [eCmsKitAdminComponents.CommentDetails]: EntityPropContributorCallback<CommentWithAuthorDto>[];
}>;

export type CmsKitAdminCreateFormPropContributors = Partial<{}>;

export type CmsKitAdminEditFormPropContributors = Partial<{}>;

export interface CmsKitAdminConfigOptions {
  entityActionContributors?: CmsKitAdminEntityActionContributors;
  entityPropContributors?: CmsKitAdminEntityPropContributors;
  createFormPropContributors?: CmsKitAdminCreateFormPropContributors;
  editFormPropContributors?: CmsKitAdminEditFormPropContributors;
}
