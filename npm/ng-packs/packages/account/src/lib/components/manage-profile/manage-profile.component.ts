import { ProfileService } from '@abp/ng.account.core/proxy';
import { fadeIn, LoadingDirective } from '@abp/ng.theme.shared';
import { transition, trigger, useAnimation } from '@angular/animations';
import { Component, inject, makeStateKey, OnInit, TransferState } from '@angular/core';
import { eAccountComponents } from '../../enums/components';
import { ManageProfileStateService } from '../../services/manage-profile.state.service';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { LocalizationPipe, ReplaceableTemplateDirective, SSRService } from '@abp/ng.core';
import { PersonalSettingsComponent } from '../personal-settings/personal-settings.component';
import { ChangePasswordComponent } from '../change-password/change-password.component';

@Component({
  selector: 'abp-manage-profile',
  templateUrl: './manage-profile.component.html',
  animations: [trigger('fadeIn', [transition(':enter', useAnimation(fadeIn))])],
  styles: [
    //TODO: move static styles
    `
      .min-h-400 {
        min-height: 400px;
      }
    `,
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    PersonalSettingsComponent,
    ChangePasswordComponent,
    LocalizationPipe,
    ReplaceableTemplateDirective,
    LoadingDirective,
  ],
})
export class ManageProfileComponent implements OnInit {
  protected readonly ssrService = inject(SSRService);
  selectedTab = 0;
  changePasswordKey = eAccountComponents.ChangePassword;
  personalSettingsKey = eAccountComponents.PersonalSettings;
  profile$ = this.manageProfileState.getProfile$();
  hideChangePasswordTab?: boolean;
  PROFILE_SETTINGS_KEY = makeStateKey<any>('profileSettings');

  constructor(
    private transferState: TransferState,
    protected profileService: ProfileService,
    protected manageProfileState: ManageProfileStateService,
  ) {}

  ngOnInit() {
    if (this.transferState.hasKey(this.PROFILE_SETTINGS_KEY)) {
      const profileSettings = this.transferState.get(this.PROFILE_SETTINGS_KEY, null);
      this.manageProfileState.setProfile(profileSettings);
      if (profileSettings.isExternal) {
        this.hideChangePasswordTab = true;
        this.selectedTab = 1;
      }
      this.transferState.remove(this.PROFILE_SETTINGS_KEY);
    } else {
      this.profileService.get().subscribe(profile => {
        this.manageProfileState.setProfile(profile);
        if (profile.isExternal) {
          this.hideChangePasswordTab = true;
          this.selectedTab = 1;
        }
        if (this.ssrService.isServer) {
          this.transferState.set(this.PROFILE_SETTINGS_KEY, profile);
        }
      });
    }
  }
}
