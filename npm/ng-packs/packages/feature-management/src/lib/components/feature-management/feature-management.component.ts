import { ConfigStateService, LocalizationModule, TrackByService } from '@abp/ng.core';
import {
  FeatureDto,
  FeatureGroupDto,
  FeaturesService,
  UpdateFeatureDto,
} from '@abp/ng.feature-management/proxy';
import {
  Confirmation,
  ConfirmationService,
  LocaleDirection,
  ThemeSharedModule,
  ToasterService,
} from '@abp/ng.theme.shared';
import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { CommonModule, NgTemplateOutlet } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbNavModule } from '@ng-bootstrap/ng-bootstrap';
import { finalize } from 'rxjs/operators';
import { FreeTextInputDirective } from '../../directives';
import { FeatureManagement } from '../../models/feature-management';

enum ValueTypes {
  ToggleStringValueType = 'ToggleStringValueType',
  FreeTextStringValueType = 'FreeTextStringValueType',
  SelectionStringValueType = 'SelectionStringValueType',
}

@Component({
  selector: 'abp-feature-management',
  templateUrl: './feature-management.component.html',
  exportAs: 'abpFeatureManagement',
  imports: [
    CommonModule,
    ThemeSharedModule,
    LocalizationModule,
    FormsModule,
    NgbNavModule,
    FreeTextInputDirective,
    NgTemplateOutlet,
  ],
})
export class FeatureManagementComponent
  implements
    FeatureManagement.FeatureManagementComponentInputs,
    FeatureManagement.FeatureManagementComponentOutputs
{
  protected readonly track = inject(TrackByService);
  protected readonly toasterService = inject(ToasterService);
  protected readonly service = inject(FeaturesService);
  protected readonly configState = inject(ConfigStateService);
  protected readonly confirmationService = inject(ConfirmationService);

  @Input()
  providerKey: string;

  @Input()
  providerName: string;

  @Input({ required: false })
  providerTitle: string;

  selectedGroupDisplayName: string;

  groups: Pick<FeatureGroupDto, 'name' | 'displayName'>[] = [];

  features: {
    [group: string]: Array<FeatureDto & { style?: { [key: string]: number }; initialValue: any }>;
  };

  valueTypes = ValueTypes;

  protected _visible;

  @Input()
  get visible(): boolean {
    return this._visible;
  }

  set visible(value: boolean) {
    if (this._visible === value) {
      return;
    }

    this._visible = value;
    this.visibleChange.emit(value);

    if (value) {
      this.openModal();
      return;
    }
  }

  @Output() readonly visibleChange = new EventEmitter<boolean>();

  modalBusy = false;

  openModal() {
    if (!this.providerName) {
      throw new Error('providerName is required.');
    }

    this.getFeatures();
  }

  getFeatures() {
    this.service.get(this.providerName, this.providerKey).subscribe(res => {
      if (!res.groups?.length) return;
      this.groups = res.groups.map(({ name, displayName }) => ({ name, displayName }));
      this.selectedGroupDisplayName = this.groups[0].displayName;
      this.features = res.groups.reduce(
        (acc, val) => ({
          ...acc,
          [val.name]: mapFeatures(val.features, document.body.dir as LocaleDirection),
        }),
        {},
      );
    });
  }

  save() {
    if (this.modalBusy) return;

    const changedFeatures = [] as UpdateFeatureDto[];

    Object.keys(this.features).forEach(key => {
      this.features[key].forEach(feature => {
        if (feature.value !== feature.initialValue)
          changedFeatures.push({ name: feature.name, value: `${feature.value}` });
      });
    });

    if (!changedFeatures.length) {
      this.visible = false;
      return;
    }

    this.modalBusy = true;
    this.service
      .update(this.providerName, this.providerKey, { features: changedFeatures })
      .pipe(finalize(() => (this.modalBusy = false)))
      .subscribe(() => {
        this.visible = false;

        this.toasterService.success('AbpUi::SavedSuccessfully');
        if (!this.providerKey) {
          // to refresh host's features
          this.configState.refreshAppState().subscribe();
        }
      });
  }

  resetToDefault() {
    this.confirmationService
      .warn('AbpFeatureManagement::AreYouSureToResetToDefault', 'AbpFeatureManagement::AreYouSure')
      .subscribe((status: Confirmation.Status) => {
        if (status === Confirmation.Status.confirm) {
          this.service.delete(this.providerName, this.providerKey).subscribe(() => {
            this.toasterService.success('AbpFeatureManagement::ResetedToDefault');
            this.visible = false;

            if (!this.providerKey) {
              // to refresh host's features
              this.configState.refreshAppState().subscribe();
            }
          });
        }
      });
  }

  onCheckboxClick(val: boolean, feature: FeatureDto) {
    if (val) {
      this.checkToggleAncestors(feature);
    } else {
      this.uncheckToggleDescendants(feature);
    }
  }

  private uncheckToggleDescendants(feature: FeatureDto) {
    this.findAllDescendantsOfByType(feature, ValueTypes.ToggleStringValueType).forEach(node =>
      this.setFeatureValue(node, false),
    );
  }

  private checkToggleAncestors(feature: FeatureDto) {
    this.findAllAncestorsOfByType(feature, ValueTypes.ToggleStringValueType).forEach(node =>
      this.setFeatureValue(node, true),
    );
  }

  private findAllAncestorsOfByType(feature: FeatureDto, type: ValueTypes) {
    let parent = this.findParentByType(feature, type);
    const ancestors = [];
    while (parent) {
      ancestors.push(parent);
      parent = this.findParentByType(parent, type);
    }
    return ancestors;
  }

  private findAllDescendantsOfByType(feature: FeatureDto, type: ValueTypes) {
    const descendants = [];
    const queue = [feature];

    while (queue.length) {
      const node = queue.pop();
      const newDescendants = this.findChildrenByType(node, type);
      descendants.push(...newDescendants);
      queue.push(...newDescendants);
    }

    return descendants;
  }

  private findParentByType(feature: FeatureDto, type: ValueTypes) {
    return this.getCurrentGroup().find(
      f => f.valueType.name === type && f.name === feature.parentName,
    );
  }

  private findChildrenByType(feature: FeatureDto, type: ValueTypes) {
    return this.getCurrentGroup().filter(
      f => f.valueType.name === type && f.parentName === feature.name,
    );
  }

  private getCurrentGroup() {
    return this.features[this.selectedGroupDisplayName] ?? [];
  }

  private setFeatureValue(feature: FeatureDto, val: boolean) {
    feature.value = val as any;
  }
}

function mapFeatures(features: FeatureDto[], dir: LocaleDirection) {
  const margin = `margin-${dir === 'rtl' ? 'right' : 'left'}.px`;

  return features.map(feature => {
    const value =
      feature.valueType?.name === ValueTypes.ToggleStringValueType
        ? (feature.value || '').toLowerCase() === 'true'
        : feature.value;

    return {
      ...feature,
      value,
      initialValue: value,
      style: { [margin]: feature.depth * 20 },
    };
  });
}
