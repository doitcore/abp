import { NgbModalOptions } from '@ng-bootstrap/ng-bootstrap';
import {
  ButtonComponent,
  collapse,
  ModalCloseDirective,
  ModalComponent,
  ToasterService,
} from '@abp/ng.theme.shared';
import { Component, inject, makeStateKey, OnInit, TransferState } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import {
  ReactiveFormsModule,
  UntypedFormBuilder,
  UntypedFormGroup,
  Validators,
} from '@angular/forms';
import { finalize } from 'rxjs/operators';
import { SettingManagementPolicyNames } from '../../enums/policy-names';
import { EmailSettingsService } from '@abp/ng.setting-management/proxy';
import { EmailSettingsDto } from '../../proxy/models';
import {
  ConfigStateService,
  LocalizationPipe,
  LocalizationService,
  PermissionDirective,
  SSRService,
} from '@abp/ng.core';
import { NgxValidateCoreModule } from '@ngx-validate/core';
import { CommonModule } from '@angular/common';

const { required, email } = Validators;

@Component({
  selector: 'abp-email-setting-group',
  templateUrl: 'email-setting-group.component.html',
  animations: [collapse],
  imports: [
    ReactiveFormsModule,
    CommonModule,
    LocalizationPipe,
    ButtonComponent,
    ModalComponent,
    ModalCloseDirective,
    NgxValidateCoreModule,
    PermissionDirective,
  ],
})
export class EmailSettingGroupComponent implements OnInit {
  protected readonly localizationService = inject(LocalizationService);
  protected readonly configStateSevice = inject(ConfigStateService);
  protected readonly ssrService = inject(SSRService);
  protected readonly currentUserEmail = toSignal(
    this.configStateSevice.getDeep$(['currentUser', 'email']),
  );

  form!: UntypedFormGroup;
  emailTestForm: UntypedFormGroup;
  saving = false;
  emailingPolicy = SettingManagementPolicyNames.Emailing;
  isEmailTestModalOpen = false;
  modalSize: NgbModalOptions = { size: 'lg' };
  EMAIL_SETTINGS_KEY = makeStateKey<any>('emailSettings');

  constructor(
    private emailSettingsService: EmailSettingsService,
    private fb: UntypedFormBuilder,
    private toasterService: ToasterService,
    private transferState: TransferState,
  ) {}

  ngOnInit() {
    this.getData();
  }

  private getData() {
    if (this.transferState.hasKey(this.EMAIL_SETTINGS_KEY)) {
      const emailSettings = this.transferState.get<EmailSettingsDto>(this.EMAIL_SETTINGS_KEY, null);
      this.buildForm(emailSettings);
      this.transferState.remove(this.EMAIL_SETTINGS_KEY);
    } else {
      this.emailSettingsService.get().subscribe(res => {
        this.buildForm(res);
        if (this.ssrService.isServer) {
          this.transferState.set(this.EMAIL_SETTINGS_KEY, res);
        }
      });
    }
  }

  private buildForm(emailSettings: EmailSettingsDto) {
    this.form = this.fb.group({
      defaultFromDisplayName: [emailSettings.defaultFromDisplayName, [Validators.required]],
      defaultFromAddress: [emailSettings.defaultFromAddress, [Validators.required]],
      smtpHost: [emailSettings.smtpHost],
      smtpPort: [emailSettings.smtpPort, [Validators.required]],
      smtpEnableSsl: [emailSettings.smtpEnableSsl],
      smtpUseDefaultCredentials: [emailSettings.smtpUseDefaultCredentials],
      smtpDomain: [emailSettings.smtpDomain],
      smtpUserName: [emailSettings.smtpUserName],
      smtpPassword: [emailSettings.smtpPassword],
    });
  }

  submit() {
    if (this.saving || this.form.invalid) return;

    this.saving = true;
    this.emailSettingsService
      .update(this.form.value)
      .pipe(finalize(() => (this.saving = false)))
      .subscribe(() => {
        this.toasterService.success('AbpSettingManagement::SavedSuccessfully');
        this.getData();
      });
  }

  openSendEmailModal() {
    this.buildEmailTestForm();
    this.isEmailTestModalOpen = true;
  }

  buildEmailTestForm() {
    const { defaultFromAddress } = this.form.value || {};
    const defaultSubject = this.localizationService.instant(
      'AbpSettingManagement::TestEmailSubject',
      ...[Math.floor(Math.random() * 9999).toString()],
    );
    const defaultBody = this.localizationService.instant('AbpSettingManagement::TestEmailBody');

    this.emailTestForm = this.fb.group({
      senderEmailAddress: [defaultFromAddress || '', [required, email]],
      targetEmailAddress: [this.currentUserEmail(), [required, email]],
      subject: [defaultSubject, [required]],
      body: [defaultBody],
    });
  }

  emailTestFormSubmit() {
    if (this.emailTestForm.invalid) {
      return;
    }

    this.emailSettingsService.sendTestEmail(this.emailTestForm.value).subscribe(res => {
      this.toasterService.success('AbpSettingManagement::SentSuccessfully');
      this.isEmailTestModalOpen = false;
    });
  }
}
