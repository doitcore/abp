import { GetPermissionListResultDto } from '@abp/ng.permission-management/proxy';
import { InputSignal, OutputEmitterRef } from '@angular/core';

export namespace PermissionManagement {
  export interface State {
    permissionRes: GetPermissionListResultDto;
  }

  export interface PermissionManagementComponentInputs {
    visible: boolean;
    readonly providerName: InputSignal<string>;
    readonly providerKey: InputSignal<string>;
    readonly hideBadges: InputSignal<boolean>;
  }

  export interface PermissionManagementComponentOutputs {
    readonly visibleChange: OutputEmitterRef<boolean>;
  }
}

