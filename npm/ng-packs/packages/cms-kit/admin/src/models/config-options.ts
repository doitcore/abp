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
  BlogPostListDto,
  CreatePageInputDto,
  UpdatePageInputDto,
  CreateBlogDto,
  CreateBlogPostDto,
  UpdateBlogDto,
  UpdateBlogPostDto,
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
  [eCmsKitAdminComponents.BlogPosts]: EntityActionContributorCallback<BlogPostListDto>[];
}>;

export type CmsKitAdminEntityPropContributors = Partial<{
  [eCmsKitAdminComponents.CommentList]: EntityPropContributorCallback<CommentWithAuthorDto>[];
  [eCmsKitAdminComponents.CommentApprove]: EntityPropContributorCallback<CommentWithAuthorDto>[];
  [eCmsKitAdminComponents.CommentDetails]: EntityPropContributorCallback<CommentWithAuthorDto>[];
  [eCmsKitAdminComponents.Tags]: EntityPropContributorCallback<TagDto>[];
  [eCmsKitAdminComponents.Pages]: EntityPropContributorCallback<PageDto>[];
  [eCmsKitAdminComponents.Blogs]: EntityPropContributorCallback<BlogDto>[];
  [eCmsKitAdminComponents.BlogPosts]: EntityPropContributorCallback<BlogPostListDto>[];
}>;

export type CmsKitAdminToolbarActionContributors = Partial<{
  [eCmsKitAdminComponents.Tags]: ToolbarActionContributorCallback<TagDto[]>[];
  [eCmsKitAdminComponents.Pages]: ToolbarActionContributorCallback<PageDto[]>[];
  [eCmsKitAdminComponents.Blogs]: ToolbarActionContributorCallback<BlogDto[]>[];
  [eCmsKitAdminComponents.BlogPosts]: ToolbarActionContributorCallback<BlogPostListDto[]>[];
}>;

export type CmsKitAdminCreateFormPropContributors = Partial<{
  [eCmsKitAdminComponents.Tags]: CreateFormPropContributorCallback<TagCreateDto>[];
  [eCmsKitAdminComponents.PageForm]: CreateFormPropContributorCallback<CreatePageInputDto>[];
  [eCmsKitAdminComponents.Blogs]: CreateFormPropContributorCallback<CreateBlogDto>[];
  [eCmsKitAdminComponents.BlogPostForm]: CreateFormPropContributorCallback<CreateBlogPostDto>[];
}>;

export type CmsKitAdminEditFormPropContributors = Partial<{
  [eCmsKitAdminComponents.Tags]: EditFormPropContributorCallback<TagUpdateDto>[];
  [eCmsKitAdminComponents.PageForm]: EditFormPropContributorCallback<UpdatePageInputDto>[];
  [eCmsKitAdminComponents.Blogs]: EditFormPropContributorCallback<UpdateBlogDto>[];
  [eCmsKitAdminComponents.BlogPostForm]: EditFormPropContributorCallback<UpdateBlogPostDto>[];
}>;

export interface CmsKitAdminConfigOptions {
  entityActionContributors?: CmsKitAdminEntityActionContributors;
  entityPropContributors?: CmsKitAdminEntityPropContributors;
  toolbarActionContributors?: CmsKitAdminToolbarActionContributors;
  createFormPropContributors?: CmsKitAdminCreateFormPropContributors;
  editFormPropContributors?: CmsKitAdminEditFormPropContributors;
}
