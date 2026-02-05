import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { FileUploadDialogComponent } from '@shared/components/file-upload-dialog/file-upload-dialog.component';
import { fileUploadDialogData } from '@shared/components/file-upload-dialog/models/file-upload-dialog.model';
import { TraceRowPageName } from '@shared/components/trace-rows/models/trace-rows.model';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import { TermCollection } from '@shared/localization/term-types';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  CompanySettingType,
  Feature,
  ImportPaymentIOState,
  ImportPaymentType,
  SoeInvoiceMatchingType,
  SoeOriginType,
  TermGroup,
  TermGroup_SysPaymentMethod,
  TermGroup_SysPaymentType,
} from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import {
  IMatchCodeDTO,
  IPaymentMethodDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { SettingsUtil } from '@shared/util/settings-util';
import { AttachedFile } from '@ui/forms/file-upload/file-upload.component';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox';
import { MenuButtonItem } from '@ui/button/menu-button/menu-button.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { MatchCodeService } from '../../../match-codes/services/match-codes.service';
import { ImportPaymentsForm } from '../../models/import-payments-form.model';
import {
  CrudResponse,
  PaymentImportDTO,
  PaymentImportIODTO,
  PaymentImportRowsDto,
} from '../../models/import-payments.model';
import { ImportPaymentsService } from '../../services/import-payments.service';
import { ImportPaymentSharedHandler } from '../../services/import-payments.shared.handler';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import { ResponseUtil } from '@shared/util/response-util';

@Component({
  selector: 'soe-import-payments-edit',
  templateUrl: './import-payments-edit.component.html',
  styleUrls: ['./import-payments-edit.component.scss'],
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ImportPaymentsEditComponent
  extends EditBaseDirective<
    PaymentImportDTO,
    ImportPaymentsService,
    ImportPaymentsForm
  >
  implements OnInit
{
  service = inject(ImportPaymentsService);
  matchSettingsService = inject(MatchCodeService);
  dialogService = inject(DialogService);
  progressService = inject(ProgressService);
  translationService = inject(TranslateService);
  coreService = inject(CoreService);
  importPaymentSharedHandler = inject(ImportPaymentSharedHandler);

  rows = new BehaviorSubject<PaymentImportIODTO[]>([]);

  uploadFile!: AttachedFile;
  protected importPaymentTypes: SmallGenericType[] = []; 
  paymentMethodsDict: SmallGenericType[] = [];
  paymentTypes: SmallGenericType[] = [];
  paymentMethods: IPaymentMethodDTO[] = [];
  matchCodes: IMatchCodeDTO[] = [];
  importTotalPaidAmount = 0;
  importDateLabel = signal('');
  defaultCreditAccountId = 0;
  defaultDebitAccountId = 0;
  defaultVoucherSeriesTypeId = 0;
  manualCustomerPaymentTransferToVoucher = false;
  defaultPaymentConditionId = 0;
  defaultPaymentMethodId = 0;
  voucherListReportId = 0;
  supplierInvoiceAskPrintVoucherOnTransfer = false;
  useExternalInvoiceNr = false;
  traceRowsRendered = signal(false);
  visibleTraceRows = signal(true);
  isUploadFileButtonVisible = signal(false);
  disableUpload = signal(false);
  showDeleteButton = signal(false);
  showSaveButton = signal(false);
  customerPaymentEditPermission = signal(false);
  errorInProccessing = false;
  showPaymentLabel = computed(
    () => this.importPaymentType() === ImportPaymentType.CustomerPayment
  );
  isPaymentDetailOpen = signal(true);
  menuList: MenuButtonItem[] = [];
  paymentImportRows!: PaymentImportRowsDto;

  textKeySingle = signal('economy.import.payment');
  pageName = TraceRowPageName.CustomerInvoice;

  protected readonly importPaymentType = signal<ImportPaymentType>(
    ImportPaymentType.CustomerPayment
  );

  ngOnInit() {
    super.ngOnInit();
    this.setPaymentType();
    this.startFlow(Feature.Economy_Import_Payments, {
      additionalModifyPermissions: [
        Feature.Economy_Customer_Payment_Payments_Edit,
      ],
      additionalReadPermissions: [
        Feature.Economy_Customer_Payment_Payments_Edit,
      ],
      lookups: [
        this.loadImportPaymentTypes(),
        this.loadPaymentMethods(),
        this.loadPaymentTypes(),
        this.loadMatchCodes(),
      ],
    });

    this.setSubscribers();
  }

  private setPaymentType(): void {

    if (this.form!.isNew) {
      const importType = this.addOptionId() as ImportPaymentType;
      this.form!.importType.setValue(importType);
    }

    this.importPaymentType.set(this.form!.importType.value);
  }

  override onPermissionsLoaded() {
    super.onPermissionsLoaded();
    this.customerPaymentEditPermission.set(
      this.flowHandler.hasReadAccess(
        Feature.Economy_Customer_Payment_Payments_Edit
      ) ||
        this.flowHandler.hasModifyAccess(
          Feature.Economy_Customer_Payment_Payments_Edit
        )
    );
  }

  override onFinished(): void {
    this.setDisabled();
    this.setValidatorTermKey();
    this.setLabelHeaders();
    this.new();
    this.progressService.loadComplete();
  }

  new() {
    if (this.form?.isNew && !this.form?.isCopy) {
      this.form?.importType.setValue(this.importPaymentType());
      this.paymentImportRows = new PaymentImportRowsDto();

      this.isUploadFileButtonVisible.set(true);
      this.isPaymentDetailOpen.set(false);
      if (!this.form?.isCopy) {
        this.setTraceRowVisibility(false);
      }
      if (
        this.paymentMethodsDict.find(x => x.id === this.defaultPaymentMethodId)
      ) {
        this.form?.patchValue({ type: this.defaultPaymentMethodId });
      }
    }
  }
  setTraceRowVisibility(status: boolean) {
    this.visibleTraceRows.set(status);
  }
  setSubscribers() {
    this.form?.importedIoInvoices.valueChanges.subscribe(
      (data: PaymentImportIODTO[]) => {
        if (data) {
          this.rows.next(data);
        }
      }
    );
  }

  setLabelHeaders() {
    this.textKeySingle.set(
      this.importPaymentType() === ImportPaymentType.CustomerPayment
        ? this.textKeySingle() + '.customer'
        : this.importPaymentType() === ImportPaymentType.SupplierPayment
          ? this.textKeySingle() + '.supplier'
          : ''
    );
    if (this.importPaymentType() === ImportPaymentType.SupplierPayment) {
      this.pageName = TraceRowPageName.SupplierInvoice;
    }
  }
  openTraceRowsExpander() {
    this.traceRowsRendered.set(!this.traceRowsRendered());
  }

  performUpdateAction(res: BackendResponse): void {
    if (res?.success) {
      this.loadData().subscribe();
      ResponseUtil.setEntityId(res, this.form?.getIdControl()?.value);
      this.updateFormValueAndEmitChange(res);
    }
  }

  override loadTerms(): Observable<TermCollection> {
    return super.loadTerms([
      'economy.import.payment.debit',
      'economy.import.payment.credit',
      'economy.import.payment.fullypaid',
      'economy.import.payment.matched',
      'economy.import.payment.paid',
      'economy.import.payment.partly_paid',
      'economy.import.payment.rest',
      'economy.import.payment.unknown',
      'economy.import.payment.error',
      'core.manual',
      'economy.import.payment.deleted',
      'economy.import.payment.manualstatus',
      'economy.import.payment.paiddate',
      'economy.import.payments.importdate',
    ]);
  }

  loadMatchCodes(): Observable<IMatchCodeDTO[]> {
    return this.service
      .getMatchCodes(SoeInvoiceMatchingType.CustomerInvoiceMatching, true)
      .pipe(
        tap(data => {
          this.matchCodes = data;
        })
      );
  }

  loadPaymentMethods(): Observable<IPaymentMethodDTO[]> {
    const originType =
      this.importPaymentType() == ImportPaymentType.CustomerPayment
        ? SoeOriginType.CustomerPayment
        : SoeOriginType.SupplierPayment;
    return this.service.getPaymentMethodsForImport(originType).pipe(
      tap(data => {
        this.paymentMethodsDict = [];
        this.paymentMethods = data;
        data.forEach(pm => {
          if (pm) {
            this.paymentMethodsDict.push(
              new SmallGenericType(pm.paymentMethodId, pm.name)
            );
          }
        });
      })
    );
  }

  private loadPaymentTypes(): Observable<ISmallGenericType[]> {
    return this.service.getSysPaymentTypeDict().pipe(
      tap(x => {
        //Only supported is bg and pg and sepa
        this.paymentTypes = [];
        x = x.filter(
          y =>
            y.id === TermGroup_SysPaymentType.BG ||
            y.id === TermGroup_SysPaymentType.PG ||
            y.id === TermGroup_SysPaymentType.SEPA
        );
        x.forEach((y: ISmallGenericType) => {
          this.paymentTypes.push({ id: y.id, name: y.name });
        });

        this.form?.patchValue({
          sysPaymentTypeId:
            this.paymentTypes.length > 0 ? this.paymentTypes[0].id : 0,
        });
      })
    );
  }

  loadImportedIoInvoices() {
    this.performLoadData.load(
      this.service
        .getImportedIoInvoices(this.form?.batchId.value, this.importPaymentType())
        .pipe(
          tap(values => {
            values.forEach(i => {
              if (i) {
                this.importPaymentSharedHandler.setStatusTexts(
                  i,
                  this.terms,
                  this.matchCodes
                );
                this.importTotalPaidAmount += i.paidAmount ?? 0;
              }
            });
            this.form?.customPaymentImportIOPatchValue(values);
            this.form?.markAsPristine();
            this.form?.markAsUntouched();
            this.setButtonVisibility();
            this.setDisabled();
          })
        )
    );
  }

  override loadData(): Observable<void> {
    return this.performLoadData.load$(
      this.service.get(this.form?.getIdControl()?.value).pipe(
        tap(value => {
          this.form?.reset(value);
          if (
            this.form?.getRawValue().paymentImportId &&
            this.form?.getRawValue().batchId
          ) {
            this.loadImportedIoInvoices();
          }
        })
      )
    );
  }

  setValidatorTermKey() {
    this.importDateLabel.set(
      this.importPaymentType() === ImportPaymentType.CustomerPayment
        ? this.terms['economy.import.payment.paiddate']
        : this.terms['economy.import.payments.importdate']
    );

    this.form?.importDate.setValidatorTermKey(this.importDateLabel());
  }

  setDisabled() {
    this.form?.batchId.disable();
    this.form!.importType.disable();

    if (this.form?.getRawValue().paymentImportId > 0) {
      if (
        this.form?.getRawValue().type &&
        this.paymentMethodsDict.find(
          f => f.id === this.form?.getRawValue().type
        )
      ) {
        this.form?.type.disable();
      } else {
        this.form?.type.enable();
      }
      this.form?.importDate.disable();
      this.form?.totalAmount.disable();
    }
  }

  setButtonVisibility() {
    this.showSaveButton.set(
      this.flowHandler.modifyPermission() &&
        this.importPaymentType() === ImportPaymentType.CustomerPayment &&
        this.form?.getRawValue().paymentImportId > 0
    );

    this.showDeleteButton.set(
      this.form?.getRawValue().paymentImportId > 0 &&
        this.form?.getRawValue().importedIoInvoices &&
        this.form?.getRawValue().importedIoInvoices.length > 0 &&
        !this.form
          ?.getRawValue()
          .importedIoInvoices.find(
            (p: PaymentImportIODTO) => p.state === ImportPaymentIOState.Closed
          )
    );
  }
  onPaymentMethodChanged() {
    this.setDisabled();
  }

  openFileUpload() {
    const fileDialog = this.dialogService.open(
      FileUploadDialogComponent,
      fileUploadDialogData({
        multipleFiles: false,
        asBinary: false,
      })
    );

    fileDialog.afterClosed().subscribe((res: AttachedFile) => {
      if (!res) return;
      this.uploadFile = res;
      this.form?.patchValue({ filename: this.uploadFile?.name });
      if (this.importPaymentType() == ImportPaymentType.CustomerPayment)
        this.startCustomerImportFile();
      else this.startSupplierImportFile();
    });
  }

  private startCustomerImportFile() {
    if (
      this.errorInProccessing &&
      this.form?.paymentImportId.value &&
      this.form?.batchId.value
    ) {
      this.paymentImportRows.base64String = this.uploadFile?.content || '';
      this.paymentImportRows.fileName = this.uploadFile?.name || '';

      const res = new CrudResponse();
      res.integerValue = this.form?.paymentImportId.value;
      this.savePaymentImportRows(this.paymentImportRows, res);
    } else {
      this.service
        .save(this.form?.getAllValues({ includeDisabled: true }))
        .pipe(
          tap(result => {
            if (result.success) {
              const value = ResponseUtil.getValueObject(result);
              const value2 = ResponseUtil.getValue2Object(result);
              this.isUploadFileButtonVisible.set(false);
              if (value && value2) {
                this.form?.patchValue({
                  paymentImportId: value2,
                  batchId: value,
                });
              }
              const paymentMethod = this.paymentMethods.find(
                x => x.paymentMethodId === this.form?.getRawValue().type
              );
              this.paymentImportRows.paymentIOType =
                TermGroup_SysPaymentMethod.BGMax;
              if (paymentMethod != null) {
                this.paymentImportRows.paymentIOType =
                  paymentMethod.sysPaymentMethodId;
              }

              this.paymentImportRows.paymentMethodId =
                this.form?.getRawValue().type;
              this.paymentImportRows.base64String =
                this.uploadFile?.content || '';
              this.paymentImportRows.fileName = this.uploadFile?.name || '';
              this.paymentImportRows.batchId = (<unknown>value) as number;
              this.paymentImportRows.paymentImportId =
                this.form?.getRawValue().paymentImportId;
              this.paymentImportRows.importType =
                ImportPaymentType.CustomerPayment;
              this.savePaymentImportRows(this.paymentImportRows, result);
            } else {
              this.isUploadFileButtonVisible.set(true);
              this.disableUpload.set(false);
              const errorMsg = ResponseUtil.getErrorMessage(result);
              if (errorMsg && errorMsg.length > 0)
                this.showErrorMessage(errorMsg);
            }
          })
        )
        .subscribe();
    }
  }

  delete() {
    const mb = this.messageboxService.warning(
      'core.delete',
      'core.deletewarning'
    );
    mb.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
      if (response?.result) {
        this.service
          .deletePaymentImportIOInvoices(
            this.form?.getRawValue().batchId,
            this.importPaymentType()
          )
          .pipe(
            tap(result => {
              if (result.success) {
                const mbs = this.showSuccessMessage(
                  this.translate.instant('core.deleted')
                );
                mbs.afterClosed().subscribe(() => {
                  this.loadData().subscribe();
                });
              } else {
                this.showErrorMessage(result.errorMessage);
              }
            })
          )
          .subscribe();
      }
    });
  }

  private startSupplierImportFile() {
    if (
      this.errorInProccessing &&
      this.form?.paymentImportId.value &&
      this.form?.batchId.value
    ) {
      this.paymentImportRows.base64String = this.uploadFile?.content || '';
      this.paymentImportRows.fileName = this.uploadFile?.name || '';

      const res = new CrudResponse();
      res.integerValue = this.form?.paymentImportId.value;
      this.savePaymentImportRows(this.paymentImportRows, res);
    } else {
      this.service
        .save(this.form?.getAllValues({ includeDisabled: true }))
        .pipe(
          tap(result => {
            if (result.success) {
              const value = ResponseUtil.getValueObject(result);
              const value2 = ResponseUtil.getValue2Object(result);
              if (value && value2) {
                this.isUploadFileButtonVisible.set(false);
                this.form?.patchValue({
                  paymentImportId: value2,
                  batchId: value,
                });
              }

              const paymentMethod = this.paymentMethods.find(
                x => x.paymentMethodId === this.form?.getRawValue().type
              );
              if (paymentMethod != null) {
                this.paymentImportRows.paymentIOType =
                  paymentMethod.sysPaymentMethodId;
              }

              this.paymentImportRows.paymentMethodId =
                this.form?.getRawValue().type;
              this.paymentImportRows.base64String =
                this.uploadFile?.content || '';
              this.paymentImportRows.fileName = this.uploadFile?.name || '';
              this.paymentImportRows.batchId = (<unknown>value) as number;
              this.paymentImportRows.paymentImportId =
                this.form?.getRawValue().paymentImportId;
              this.paymentImportRows.importType =
                ImportPaymentType.SupplierPayment;
              this.savePaymentImportRows(this.paymentImportRows, result);
            } else {
              this.isUploadFileButtonVisible.set(true);
              this.disableUpload.set(false);
              const errorMsg = ResponseUtil.getErrorMessage(result);
              if (errorMsg && errorMsg.length > 0) {
                this.showErrorMessage(errorMsg);
              }
            }
          })
        )
        .subscribe();
    }
  }

  public save() {
    if (!this.form || this.form.invalid || !this.service) return;
    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service.save(this.form?.getRawValue()).pipe(
        tap(res => {
          if (res.success) {
            const modifiedRows: PaymentImportIODTO[] =
              this.rows.getValue().filter(x => x.isModified) || [];
            if (modifiedRows.length > 0) {
              modifiedRows.forEach(r => {
                r.batchNr = this.form?.batchId.value;
              });
              this.savePaymentImportIOs(modifiedRows).subscribe(res => {
                if (res.success) {
                  res.integerValue = this.form?.getIdControl()?.value;
                  this.updateFormValueAndEmitChange(res);
                }
              });
            } else {
              this.updateFormValueAndEmitChange(res);
            }
          }
        })
      )
    );
  }

  private showSuccessMessage(
    message: string = this.translate.instant('core.saved')
  ) {
    return this.messageboxService.success(
      this.translate.instant('common.status'),
      message
    );
  }

  private showErrorMessage(
    errorMessage: string = this.translate.instant('core.error')
  ) {
    this.messageboxService.error(
      this.translate.instant('core.error'),
      errorMessage
    );
  }

  private savePaymentImportIOs(
    paymentImportRows: PaymentImportIODTO[]
  ): Observable<any> {
    return this.service.savePaymentImportIOs(paymentImportRows);
  }

  private savePaymentImportRows(
    paymentImportRows: PaymentImportRowsDto,
    res: any
  ) {
    this.service
      .savePaymentImportRow(paymentImportRows)
      .pipe(
        tap(result => {
          if (result.success) {
            this.updateFormValueAndEmitChange(res);
            this.setTraceRowVisibility(true);
            this.disableUpload.set(true);
            this.errorInProccessing = false;
          } else {
            this.errorInProccessing = true;
            this.isUploadFileButtonVisible.set(true);
            this.disableUpload.set(false);
            this.form?.patchValue({
              paymentImportId: 0,
              batchId: 0,
            });
            this.showErrorMessage(result.errorMessage);
          }

          if (result.keys && result.keys.length > 1) {
            let pos = 0;
            result.keys.forEach((key: number) => {
              if (this.form?.paymentImportId.value !== key) {
                this.openEditInNewTab({
                  id: key,
                  additionalProps: result.strings[pos],
                });
              }
              pos++;
            });
          }
        })
      )
      .subscribe();
  }
  override loadCompanySettings() {
    const settingTypes: CompanySettingType[] = [];

    settingTypes.push(CompanySettingType.AccountingVoucherImportVoucherSerie);
    settingTypes.push(CompanySettingType.CustomerPaymentDefaultPaymentMethod);
    settingTypes.push(CompanySettingType.SupplierPaymentDefaultPaymentMethod);
    settingTypes.push(
      CompanySettingType.CustomerPaymentManualTransferToVoucher
    );
    settingTypes.push(CompanySettingType.AccountingDefaultVoucherList);
    settingTypes.push(
      CompanySettingType.SupplierPaymentAskPrintVoucherOnTransfer
    );
    settingTypes.push(CompanySettingType.CustomerPaymentVoucherSeriesType);
    settingTypes.push(CompanySettingType.AccountCustomerSalesVat);
    settingTypes.push(CompanySettingType.AccountCustomerClaim);
    settingTypes.push(CompanySettingType.BillingUseExternalInvoiceNr);

    settingTypes.push(CompanySettingType.AccountCommonCheck);

    return this.coreService.getCompanySettings(settingTypes).pipe(
      tap(x => {
        this.defaultCreditAccountId = SettingsUtil.getIntCompanySetting(
          x,
          CompanySettingType.AccountCustomerClaim
        );
        this.defaultDebitAccountId = SettingsUtil.getIntCompanySetting(
          x,
          CompanySettingType.AccountCommonCheck
        );
        this.defaultVoucherSeriesTypeId = SettingsUtil.getIntCompanySetting(
          x,
          CompanySettingType.CustomerPaymentVoucherSeriesType
        );
        this.manualCustomerPaymentTransferToVoucher =
          SettingsUtil.getBoolCompanySetting(
            x,
            CompanySettingType.CustomerPaymentManualTransferToVoucher
          );
        this.defaultPaymentConditionId = SettingsUtil.getIntCompanySetting(
          x,
          CompanySettingType.CustomerPaymentDefaultPaymentCondition
        );

        if (this.importPaymentType() === ImportPaymentType.SupplierPayment)
          this.defaultPaymentMethodId = SettingsUtil.getIntCompanySetting(
            x,
            CompanySettingType.SupplierPaymentDefaultPaymentMethod
          );
        else if (this.importPaymentType() === ImportPaymentType.CustomerPayment)
          this.defaultPaymentMethodId = SettingsUtil.getIntCompanySetting(
            x,
            CompanySettingType.CustomerPaymentDefaultPaymentMethod
          );

        this.voucherListReportId = SettingsUtil.getIntCompanySetting(
          x,
          CompanySettingType.AccountingDefaultVoucherList
        );
        this.supplierInvoiceAskPrintVoucherOnTransfer =
          SettingsUtil.getBoolCompanySetting(
            x,
            CompanySettingType.SupplierPaymentAskPrintVoucherOnTransfer
          );
        this.useExternalInvoiceNr = SettingsUtil.getBoolCompanySetting(
          x,
          CompanySettingType.BillingUseExternalInvoiceNr
        );
      })
    );
  }

  private loadImportPaymentTypes(): Observable<SmallGenericType[]>  {
    return this.coreService
      .getTermGroupContent(TermGroup.ImportPaymentType, false, false)
      .pipe(
        tap(res => {
          this.importPaymentTypes = res;
        })
      );
  }
}
