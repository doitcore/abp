import { Validators } from '@angular/forms';
import { TagUpdateDto } from '@abp/ng.cms-kit/proxy';
import { FormProp, ePropType } from '@abp/ng.components/extensible';

export const DEFAULT_TAG_EDIT_FORM_PROPS = FormProp.createMany<TagUpdateDto>([
  {
    type: ePropType.String,
    name: 'name',
    displayName: 'CmsKit::Name',
    id: 'name',
    validators: () => [Validators.required],
  },
]);
