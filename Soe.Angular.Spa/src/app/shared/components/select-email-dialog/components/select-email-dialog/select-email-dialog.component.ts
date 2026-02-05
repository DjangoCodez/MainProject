import { Component, inject, signal } from '@angular/core';
import {
  SelectEmailDialogCloseData,
  SelectEmailDialogData,
  SelectEmailDialogFormDTO,
  SelectEmailRecipientsDTO,
} from '../../models/select-email-dialog.model';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Observable, of, tap } from 'rxjs';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { Perform } from '@shared/util/perform.class';
import { SettingsUtil } from '@shared/util/settings-util';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import {
  EmailTemplateType,
  SettingMainType,
  TermGroup,
  UserSettingType,
} from '@shared/models/generated-interfaces/Enumerations';
import { IEmailTemplateDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SaveUserCompanySettingModel } from '@shared/components/select-project-dialog/models/select-project-dialog.model';
import { SelectEmailDialogForm } from '../../models/select-email-dialog-form.model';
import { ValidationHandler } from '@shared/handlers';

@Component({
    selector: 'soe-select-email-dialog',
    templateUrl: './select-email-dialog.component.html',
    styleUrls: ['./select-email-dialog.component.scss'],
    providers: [FlowHandlerService],
  standalone: false,
})
export class SelectEmailDialogComponent extends DialogComponent<SelectEmailDialogData> {
  coreService = inject(CoreService);
  progressService = inject(ProgressService);
  validationHandler = inject(ValidationHandler);
  handler = inject(FlowHandlerService);

  performLanguageLoad = new Perform<ISmallGenericType[]>(this.progressService);
  performUserSettingLoad = new Perform<any>(this.progressService);
  performEmailTemplatesByTypeLoad = new Perform<IEmailTemplateDTO[]>(
    this.progressService
  );
  performEmailTemplatesLoad = new Perform<any>(this.progressService);

  performActionSaveUserSetting = new Perform<SaveUserCompanySettingModel>(
    this.progressService
  );

  languages: ISmallGenericType[] = [];

  emailTemplates: any[] = [];
  defaultEmail = 0;
  defaultEmailTemplateId!: number;
  defaultReportTemplateId!: number;
  showReportSelection = signal(false);
  grid = signal(false);
  showAttachments = signal(false);
  showChecklists = signal(false);
  showDefaulTemailtemplateMissingSelection = signal(true);
  type!: number;
  types!: any;
  reports!: ISmallGenericType[];
  disableSend = signal(true);

  form: SelectEmailDialogForm = new SelectEmailDialogForm({
    validationHandler: this.validationHandler,
    element: new SelectEmailDialogFormDTO(),
  });

  constructor() {
    super();
    this.setDialogParam();

    this.handler.execute({
      lookups: [
        this.loadLanguages(),
        this.loadUserSettings(),
        this.loadTemplates(),
      ],
      onFinished: this.finished.bind(this),
    });
  }

  finished() {
    this.setDisableControllers();
    this.setControlVisibility();
    if (this.data.isSendEmailDocuments) {
      this.form?.patchValue({ emailAddresses: this.data.defaultEmail || '' });
    }
  }

  setDisableControllers() {
    let isDisabled = this.showReportSelection()
      ? !this.form.selectedReportId.value || !this.form.selectedTemplateId.value
      : !this.form.selectedTemplateId.value;

    if (
      this.data.attachments &&
      this.data.attachments?.length > 0 &&
      this.data?.isSendEmailDocuments
    )
      isDisabled = false;

    this.disableSend.set(isDisabled);
  }

  setControlVisibility() {
    if (
      this.form.attachments.value &&
      this.form.attachments.value.length > 0 &&
      !this.grid()
    ) {
      this.showAttachments.set(true);
    }

    if (
      this.form.checkLists.value &&
      this.form.checkLists.value.length > 0 &&
      !this.grid()
    ) {
      this.showChecklists.set(true);
    }
  }

  changeReport() {
    this.setDisableControllers();
  }

  changeEmailTemplate() {
    this.setDisableControllers();
  }

  loadLanguages(): Observable<ISmallGenericType[]> {
    return this.performLanguageLoad.load$(
      this.coreService
        .getTermGroupContent(TermGroup.Language, false, false)
        .pipe(
          tap(data => {
            this.languages = data;
          })
        )
    );
  }

  loadUserSettings(): Observable<any> {
    const settingTypes: number[] = [UserSettingType.BillingMergePdfs];

    return this.performUserSettingLoad.load$(
      this.coreService.getUserSettings(settingTypes).pipe(
        tap(data => {
          this.form.patchValue({
            mergePdfs: SettingsUtil.getBoolUserSetting(
              data,
              UserSettingType.BillingMergePdfs,
              false
            ),
          });
        })
      )
    );
  }

  loadTemplates(): Observable<IEmailTemplateDTO[]> {
    if (this.data.hideTemplate) {
      return of([]);
    }
    this.emailTemplates = [];
    if (this.type !== null && this.type !== undefined) {
      return this.performEmailTemplatesByTypeLoad.load$(
        this.coreService.getEmailTemplatesByType(this.type).pipe(
          tap(data => {
            data.forEach(y => {
              if (this.types) {
              switch (y.type) {
                case EmailTemplateType.Invoice:
                  y.typename = this.types['billing.invoices.invoice'];
                  break;
                case EmailTemplateType.Reminder:
                    y.typename =
                      this.types['common.customer.invoices.reminder'];
                  break;
                case EmailTemplateType.PurchaseOrder:
                  y.typename = this.types['billing.purchase.list.purchase'];
                    break;
                  default:
                    y.typename = '';
                    break;
              }
              }
            });
            this.emailTemplates = data;

            // Set default

            if (!this.form.selectedTemplateId.value)
              this.form.patchValue({
                selectedTemplateId: this.emailTemplates[0].emailTemplateId,
              });
          })
        )
      );
    } else {
      return this.performEmailTemplatesLoad.load$(
        this.coreService.getEmailTemplates().pipe(
          tap(data => {
            data.forEach(y => {
              if (this.types) {
              switch (y.type) {
                case EmailTemplateType.Invoice:
                  y.typename = this.types['billing.invoices.invoice'];
                  break;
                case EmailTemplateType.Reminder:
                    y.typename =
                      this.types['common.customer.invoices.reminder'];
                  break;
                case EmailTemplateType.PurchaseOrder:
                  y.typename = this.types['billing.purchase.list.purchase'];
                    break;
                  default:
                    y.typename = '';
                    break;
                }
              }
            });
            this.emailTemplates = data;

            // Set default
            if (!this.form.selectedTemplateId.value)
              this.form.patchValue({
                selectedTemplateId: this.emailTemplates[0].emailTemplateId,
              });
          })
        )
      );
    }
  }

  saveUserSetting(): Observable<any> {
    const model = new SaveUserCompanySettingModel(
      SettingMainType.User,
      UserSettingType.BillingMergePdfs,
      this.form.mergePdfs.value
    );
    return of(this.coreService.saveBoolSetting(model));
  }

  cancel() {
    this.close(false);
  }

  send() {
    this.close(true);
  }

  close(send: boolean) {
    if (send) {
      let emailDialogCloseData = new SelectEmailDialogCloseData(
          this.form.selectedTemplateId.value,
          this.form.selectedReportId.value,
          this.form.selectedLanguageId.value,
          this.form.mergePdfs.value,
          // (this.form.value.recipients as FormArray).controls.filter(
          //   f => f.value.isSelected
          // // )
          this.form.selectedRecipients.value,
          // this.form.recipients.value,
          // (this.form.value.attachments as FormArray).controls.filter(
          //   f => f.value.isSelected
          // )
          this.form.attachments.value,
          // (this.form.value.checklists as FormArray).controls.filter(
          //   f => f.value.isSelected
          // )
          this.form.checkLists.value
      );
      if (this.form.selectedTemplateId.value) {
        this.saveUserSetting();
      }
      if (this.data.isSendEmailDocuments) {
        emailDialogCloseData.emailAddresses = this.form.emailAddresses.value;
      }
      this.dialogRef.close(emailDialogCloseData);
    } else {
      this.dialogRef.close(false);
    }
  }

  setDialogParam() {
    if (this.data) {
      this.form.patchValue({
        selectedLanguageId: this.data.langId
          ? this.data.langId
          : SoeConfigUtil.sysCountryId,
      });
      this.defaultEmail = this.data.defaultEmail ? this.data.defaultEmail : 0;

      if (this.data.defaultEmailTemplateId || this.data.isSendEmailDocuments) {
        this.showDefaulTemailtemplateMissingSelection.set(false);
      if (this.data.defaultEmailTemplateId) {
        this.defaultEmailTemplateId = this.data.defaultEmailTemplateId;
        }
      }
      if (this.data.defaultReportTemplateId) {
        this.defaultReportTemplateId = this.data.defaultReportTemplateId;
      }
      if (this.data.type) {
        this.type = this.data.type;
      }
      if (this.data.types) {
        this.types = this.data.types;
      }
      if (this.data.showReportSelection) {
        this.showReportSelection.set(this.data.showReportSelection);
      }
      if (this.data.recipients) {
        const recipientDtos: SelectEmailRecipientsDTO[] = [];
        this.data.recipients.forEach(f => {
          recipientDtos.push(
            new SelectEmailRecipientsDTO(
              f.id,
              f.name,
              f.id === this.defaultEmail
            )
          );
        });
        this.form.customRecipientsPatchValue(recipientDtos);
      }
      if (this.data.attachments) {
        this.form.customAttachmentsPatchValue(this.data.attachments);
      }
      if (this.data.checklists) {
        this.form.customCheckListsPatchValue(this.data.checklists);
      }
      if (this.data.reports) {
        this.reports = this.data.reports;
        if (this.reports.length > 0) {
          this.form.patchValue({
            selectedReportId: this.reports[0].id,
          });
        }
      }
      if (this.data.grid) {
        this.grid.set(this.data.grid);
      }
    }
  }
}
