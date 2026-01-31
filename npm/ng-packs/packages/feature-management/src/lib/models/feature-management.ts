import { InputSignal, OutputEmitterRef } from '@angular/core';

export namespace FeatureManagement {
  export interface FeatureManagementComponentInputs {
    visible: boolean;
    readonly providerName: InputSignal<string | undefined>;
    readonly providerKey: InputSignal<string | undefined>;
  }

  export interface FeatureManagementComponentOutputs {
    readonly visibleChange: OutputEmitterRef<boolean>;
  }
}

