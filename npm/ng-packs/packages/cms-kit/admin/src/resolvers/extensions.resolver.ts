import {
  ExtensionsService,
  getObjectExtensionEntitiesFromStore,
  mapEntitiesToContributors,
  mergeWithDefaultActions,
  mergeWithDefaultProps,
} from '@abp/ng.components/extensible';
import { inject, Injector } from '@angular/core';
import { ResolveFn } from '@angular/router';
import { map, tap } from 'rxjs';
import { eCmsKitAdminComponents } from '../enums';
import {
  CMS_KIT_ADMIN_ENTITY_ACTION_CONTRIBUTORS,
  CMS_KIT_ADMIN_ENTITY_PROP_CONTRIBUTORS,
  DEFAULT_CMS_KIT_ADMIN_ENTITY_ACTIONS,
  DEFAULT_CMS_KIT_ADMIN_ENTITY_PROPS,
} from '../tokens';

export const cmsKitAdminExtensionsResolver: ResolveFn<any> = () => {
  const injector = inject(Injector);
  const extensions = inject(ExtensionsService);

  const config = { optional: true };

  const actionContributors = inject(CMS_KIT_ADMIN_ENTITY_ACTION_CONTRIBUTORS, config) || {};
  const propContributors = inject(CMS_KIT_ADMIN_ENTITY_PROP_CONTRIBUTORS, config) || {};

  return getObjectExtensionEntitiesFromStore(injector, 'CmsKit').pipe(
    map(entities => ({
      [eCmsKitAdminComponents.CommentList]: entities.Comment,
      [eCmsKitAdminComponents.CommentApprove]: entities.Comment,
      [eCmsKitAdminComponents.CommentDetails]: entities.Comment,
    })),
    mapEntitiesToContributors(injector, 'CmsKit'),
    tap(objectExtensionContributors => {
      mergeWithDefaultActions(
        extensions.entityActions,
        DEFAULT_CMS_KIT_ADMIN_ENTITY_ACTIONS,
        actionContributors,
      );
      mergeWithDefaultProps(
        extensions.entityProps,
        DEFAULT_CMS_KIT_ADMIN_ENTITY_PROPS,
        objectExtensionContributors.prop,
        propContributors,
      );
    }),
  );
};
