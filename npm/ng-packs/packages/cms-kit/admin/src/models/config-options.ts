import { eCmsKitAdminComponents } from '../enums';
import {
  EntityActionContributorCallback,
  EntityPropContributorCallback,
  ToolbarActionContributorCallback,
  CreateFormPropContributorCallback,
  EditFormPropContributorCallback,
} from '@abp/ng.components/extensible';
import {
  CommentWithAuthorDto,
  TagDto,
  PageDto,
  BlogDto,
  CreatePageInputDto,
  UpdatePageInputDto,
  CreateBlogDto,
  UpdateBlogDto,
  TagCreateDto,
  TagUpdateDto,
} from '@abp/ng.cms-kit/proxy';

export type CmsKitAdminEntityActionContributors = Partial<{
  [eCmsKitAdminComponents.CommentList]: EntityActionContributorCallback<CommentWithAuthorDto>[];
  [eCmsKitAdminComponents.CommentApprove]: EntityActionContributorCallback<CommentWithAuthorDto>[];
  [eCmsKitAdminComponents.CommentDetails]: EntityActionContributorCallback<CommentWithAuthorDto>[];
  [eCmsKitAdminComponents.Tags]: EntityActionContributorCallback<TagDto>[];
  [eCmsKitAdminComponents.Pages]: EntityActionContributorCallback<PageDto>[];
  [eCmsKitAdminComponents.Blogs]: EntityActionContributorCallback<BlogDto>[];
}>;

export type CmsKitAdminEntityPropContributors = Partial<{
  [eCmsKitAdminComponents.CommentList]: EntityPropContributorCallback<CommentWithAuthorDto>[];
  [eCmsKitAdminComponents.CommentApprove]: EntityPropContributorCallback<CommentWithAuthorDto>[];
  [eCmsKitAdminComponents.CommentDetails]: EntityPropContributorCallback<CommentWithAuthorDto>[];
  [eCmsKitAdminComponents.Tags]: EntityPropContributorCallback<TagDto>[];
  [eCmsKitAdminComponents.Pages]: EntityPropContributorCallback<PageDto>[];
  [eCmsKitAdminComponents.Blogs]: EntityPropContributorCallback<BlogDto>[];
}>;

export type CmsKitAdminToolbarActionContributors = Partial<{
  [eCmsKitAdminComponents.Tags]: ToolbarActionContributorCallback<TagDto[]>[];
  [eCmsKitAdminComponents.Pages]: ToolbarActionContributorCallback<PageDto[]>[];
  [eCmsKitAdminComponents.Blogs]: ToolbarActionContributorCallback<BlogDto[]>[];
}>;

export type CmsKitAdminCreateFormPropContributors = Partial<{
  [eCmsKitAdminComponents.Tags]: CreateFormPropContributorCallback<TagCreateDto>[];
  [eCmsKitAdminComponents.PageForm]: CreateFormPropContributorCallback<CreatePageInputDto>[];
  [eCmsKitAdminComponents.Blogs]: CreateFormPropContributorCallback<CreateBlogDto>[];
}>;

export type CmsKitAdminEditFormPropContributors = Partial<{
  [eCmsKitAdminComponents.Tags]: EditFormPropContributorCallback<TagUpdateDto>[];
  [eCmsKitAdminComponents.PageForm]: EditFormPropContributorCallback<UpdatePageInputDto>[];
  [eCmsKitAdminComponents.Blogs]: EditFormPropContributorCallback<UpdateBlogDto>[];
}>;

export interface CmsKitAdminConfigOptions {
  entityActionContributors?: CmsKitAdminEntityActionContributors;
  entityPropContributors?: CmsKitAdminEntityPropContributors;
  toolbarActionContributors?: CmsKitAdminToolbarActionContributors;
  createFormPropContributors?: CmsKitAdminCreateFormPropContributors;
  editFormPropContributors?: CmsKitAdminEditFormPropContributors;
}
