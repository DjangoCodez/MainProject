import {
  Component,
  DestroyRef,
  ElementRef,
  inject,
  OnInit,
  signal,
  ViewChild,
  WritableSignal,
} from '@angular/core';
import { ShortcutService } from '@core/services/shortcut.service';
import { AccountDistributionService } from '@features/economy/account-distribution/services/account-distribution.service';
import { AccountingRowsComponent } from '@shared/components/accounting-rows/accounting-rows/accounting-rows.component';
import { AccountingRowDTO } from '@shared/components/accounting-rows/models/accounting-rows-model';
import { FilesHelper } from '@shared/components/files-helper/files-helper.component';
import { SaveUserCompanySettingModel } from '@shared/components/select-project-dialog/models/select-project-dialog.model';
import { TraceRowPageName } from '@shared/components/trace-rows/models/trace-rows.model';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import { ValidationHandler } from '@shared/handlers';
import {
  ActionResultSave,
  CompanySettingType,
  Feature,
  SettingMainType,
  SoeDataStorageRecordType,
  SoeEntityType,
  TermGroup_AccountStatus,
  TermGroup_CurrencyType,
  UserSettingType,
} from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import {
  IAccountPeriodDTO,
  IAccountYearDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { IVoucherRowHistoryViewDTO } from '@shared/models/generated-interfaces/VoucherRowHistoryDTOs';
import { IVoucherSeriesDTO } from '@shared/models/generated-interfaces/VoucherSeriesDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { MessagingService } from '@shared/services/messaging.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { CurrencyService } from '@shared/services/currency.service';
import { ProgressOptions } from '@shared/services/progress';
import { BrowserUtil } from '@shared/util/browser-util';
import { Constants } from '@shared/util/client-constants';
import { DateUtil } from '@shared/util/date-util';
import { Guid } from '@shared/util/string-util';
import { Perform } from '@shared/util/perform.class';
import { SettingsUtil } from '@shared/util/settings-util';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { focusOnElement } from '@shared/util/focus-util';
import {
  AccountingRowsContainers,
  VoucherEditSaveFunctions,
} from '@shared/util/Enumerations';
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarItemGroupConfig } from '@ui/toolbar/models/toolbar';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { BehaviorSubject, forkJoin, mergeMap, Observable, of, tap } from 'rxjs';
import {
  addDebitCreditBalanceValidator,
  KeepVoucherOpenAfterSaveForm,
  VoucherForm,
} from '../../models/voucher-form.model';
import {
  CalculateAccountBalanceForAccountsFromVoucherModel,
  EditVoucherNrModel,
  FileUploadDTO,
  SaveVoucherModel,
  VoucherHeadDTO,
  VoucherRowDTO,
} from '../../models/voucher.model';
import { VoucherService } from '../../services/voucher.service';
import { ActivatedRoute } from '@angular/router';
import { RequestReportService } from '@shared/services/request-report.service';
import { VoucherParamsService } from '../../services/voucher-params.service';
import { PersistedAccountingYearService } from '@features/economy/services/accounting-year.service';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import { ResponseUtil } from '@shared/util/response-util';

@Component({
  selector: 'soe-voucher-edit',
  templateUrl: './voucher-edit.component.html',
  styleUrl: './voucher-edit.component.scss',
  providers: [FlowHandlerService, ToolbarService, CurrencyService],
  standalone: false,
})
export class VoucherEditComponent
  extends EditBaseDirective<VoucherHeadDTO, VoucherService, VoucherForm>
  implements OnInit
{
  @ViewChild(AccountingRowsComponent)
  accountingRowsComponent!: AccountingRowsComponent;
  @ViewChild('saveButton') saveButton!: ElementRef;
  @ViewChild('selectedDate') selectedDate!: ElementRef;

  messagingService = inject(MessagingService);
  service = inject(VoucherService);
  messageboxService = inject(MessageboxService);
  coreService = inject(CoreService);
  private readonly requestReportService = inject(RequestReportService);
  progress = inject(ProgressService);
  currencyService = inject(CurrencyService);
  shortcutService = inject(ShortcutService);
  accountDistributionService = inject(AccountDistributionService);
  validationHandler = inject(ValidationHandler);
  urlService = inject(VoucherParamsService);
  ayService = inject(PersistedAccountingYearService);
  performLoadData = new Perform<any>(this.progressService);
  filesHelper!: FilesHelper;
  formKeepVoucherOpenAfterSave: KeepVoucherOpenAfterSaveForm =
    new KeepVoucherOpenAfterSaveForm({
      validationHandler: this.validationHandler,
      element: false,
    });
  accountYear!: IAccountYearDTO;
  pageName = TraceRowPageName.Voucher;
  addRowWithSetAccountDimFocus = signal(false);

  historyGridRows = new BehaviorSubject<IVoucherRowHistoryViewDTO[]>([]);
  accountingRows: WritableSignal<AccountingRowDTO[]> = signal(
    [] as AccountingRowDTO[]
  );
  additionalToolbarItemGroups: ToolbarItemGroupConfig[] = [];

  revertVatVoucherId = 0;
  sequenceNumber = 0;
  historyLoaded: WritableSignal<boolean> = signal(false);
  traceRowsRendered: WritableSignal<boolean> = signal(false);
  showEditVoucherNrButton: WritableSignal<boolean> = signal(false);
  showDeleteButton: WritableSignal<boolean> = signal(false);
  isLocked: WritableSignal<boolean> = signal(false);
  isDateLocked: WritableSignal<boolean> = signal(false);
  showInfoMessage: WritableSignal<boolean> = signal(false);
  infoMessage: WritableSignal<string> = signal('');
  isSaving: WritableSignal<boolean> = signal(false);
  accountYearIsOpen: WritableSignal<boolean> = signal(false);

  //Company Settings
  allowUnbalancedVoucher: WritableSignal<boolean> = signal(false);
  allowEditVoucher: WritableSignal<boolean> = signal(false);
  allowEditVoucherDate: WritableSignal<boolean> = signal(false);
  showEnterpriseCurrency: WritableSignal<boolean> = signal(false);
  paymentFromTaxAgencyAccountId: WritableSignal<number> = signal(0);
  defaultVoucherSeriesId: WritableSignal<number> = signal(0);
  defaultVoucherSeriesTypeId: WritableSignal<number> = signal(0);

  // User Settings
  keepNewVoucherAfterSave: WritableSignal<boolean> = signal(false);

  // Permissions
  modifyAccountPeriodPermission: WritableSignal<boolean> = signal(false);
  reportPermission: WritableSignal<boolean> = signal(false);
  modifyPermission: WritableSignal<boolean> = signal(false);

  // Household Properties
  createHouseholdVoucher: WritableSignal<boolean> = signal(false);
  householdDate: Date | undefined;
  householdAmount: number | undefined;
  householdRowIds: number[] | undefined;
  householdInvoiceNbrs: string[] | undefined;
  householdVoucherSeriesId: WritableSignal<number> = signal(0);
  householdProductAccountId: number | undefined;
  productId: number | undefined;

  showNavigationButtons: WritableSignal<boolean> = signal(false);
  voucherIds: number[] = [];
  voucherSeries: IVoucherSeriesDTO[] = [];
  templates: ISmallGenericType[] = [];
  templateVoucherSeriesId = 0;
  accountPeriod?: IAccountPeriodDTO;

  saveFunctions: any = [];

  private voucherSeriesManuallyChangedId = 0;

  private toolbarCopyDisabled = signal(false);
  private toolbarDoInvertDisabled = signal(false);
  private toolbarPrintDisabled = signal(false);

  constructor(
    private elementRef: ElementRef,
    private destroyRef: DestroyRef,
    private route: ActivatedRoute
  ) {
    super();
    this.filesHelper = new FilesHelper(
      true,
      SoeEntityType.Voucher,
      SoeDataStorageRecordType.VoucherFileAttachment,
      Feature.Economy_Accounting_Vouchers_Edit,
      this.performLoadData
    );
    this.setShortcuts(elementRef, destroyRef);
    this.setupMessageListners();
    this.setTemplateStatus();
  }

  private setShortcuts(elementRef: ElementRef, destroyRef: DestroyRef) {
    this.shortcutService.bindShortcut(
      this.elementRef,
      this.destroyRef,
      ['Control', 's'],
      e => this.save()
    );

    this.shortcutService.bindShortcut(
      this.elementRef,
      this.destroyRef,
      ['Control', 'p'],
      e => this.save(true)
    );
  }

  private setupMessageListners() {
    this.messagingService
      .onEvent(Constants.EVENT_SELECT_ACCOUNTDISTRIBUTION_NAME)
      .subscribe(() => {
        const voucherSerie = this.getVoucherSeries(
          this.form?.voucherSeriesId.value
        );
        if (voucherSerie)
          this.accountDistributionService.changeAccountDistributionHeadName(
            this.form?.voucherNr.value +
              ', ' +
              voucherSerie.voucherSeriesTypeName +
              ', ' +
              DateUtil.localeDateFormat(this.form?.date.value)
          );
      });
  }

  ngOnInit() {
    super.ngOnInit();
    this.startFlow(Feature.Economy_Accounting_Vouchers_Edit, {
      additionalReadPermissions: [
        Feature.Economy_Distribution_Reports_Selection,
        Feature.Economy_Distribution_Reports_Selection_Download,
      ],
      additionalModifyPermissions: [
        Feature.Economy_Accounting_AccountPeriods,
        Feature.Economy_Accounting_Vouchers_Edit,
      ],
    });
    this.getRouteParams();
  }

  getRouteParams() {
    this.route.paramMap.subscribe(params => {
      if (params.get('createHousehold')) {
        const productIdParam = params.get('productId');
        const dateParam = params.get('date');
        const amountParam = params.get('amount');
        const idsParam = params.get('ids');
        const nbrsParam = params.get('nbrs');

        this.createHouseholdVoucher.set(true);
        this.productId = productIdParam ? Number(productIdParam) : undefined;
        if (dateParam) {
          this.householdDate = dateParam ? new Date(dateParam) : undefined;
          this.form?.patchValue({ date: new Date(dateParam) });
        }
        this.householdAmount = amountParam ? Number(amountParam) : undefined;
        this.householdRowIds = idsParam
          ? idsParam
              .split(',')
              .map(id => Number(id))
              .filter(n => !isNaN(n))
          : [];
        this.householdInvoiceNbrs = nbrsParam
          ? nbrsParam
              .split(',')
              .map(tag => tag.trim())
              .filter(tag => tag.length > 0)
          : [];
      } else if (this.householdRowIds && this.householdRowIds.length > 0) {
        this.voucherIds = this.householdRowIds;
      } else {
        this.showNavigationButtons.set(false);
      }
    });
  }

  setControlStatus() {
    this.setTemplateStatus();
    this.setIsLocked();
    this.setIsDateLocked();
    this.setControlVisibility();
    this.disableControls();
  }

  setTemplateStatus() {
    this.service.setIsTemplateSubject(this.urlService.isTemplate());
    this.form?.patchValue({ template: this.urlService.isTemplate() });
  }
  setIsLocked() {
    // Existing vouchers can only be edited if company setting says so.
    // Vouchers in a closed or locked period cannot be edited.
    // Templates can always be edited.
    this.isLocked.set(!this.form?.isNew && !this.allowEditVoucher());

    if (this.form) {
      if (
        this.form.voucherStatus.value == TermGroup_AccountStatus.Closed ||
        this.form.voucherStatus.value == TermGroup_AccountStatus.Locked
      ) {
        this.isLocked.set(true);
      }

      if (this.form.template.value) {
        this.isLocked.set(false);
      }
    }
  }

  setIsDateLocked() {
    // Existing vouchers can only be edited if company setting says so.
    // Vouchers in a closed or locked period cannot be edited.
    // Templates can always be edited.

    this.isDateLocked.set(!this.form?.isNew && !this.allowEditVoucherDate());

    if (this.form) {
      if (
        this.form.voucherStatus.value == TermGroup_AccountStatus.Closed ||
        this.form.voucherStatus.value == TermGroup_AccountStatus.Locked
      )
        this.isDateLocked.set(true);

      if (this.form.template.value) this.isDateLocked.set(false);
    }
  }

  setButtonOptions() {
    this.saveFunctions.push({
      id: VoucherEditSaveFunctions.Save,
      label: this.translate.instant('core.save') + ' (Ctrl+S)',
      icon: 'save',
    });
    if (!this.urlService.isTemplate()) {
      this.saveFunctions.push({
        id: VoucherEditSaveFunctions.SaveAndPrint,
        label:
          this.translate.instant('economy.accounting.voucher.saveandprint') +
          ' (Ctrl+P)',
        icon: 'print',
      });
      this.saveFunctions.push({
        id: VoucherEditSaveFunctions.SaveAsTemplate,
        label: this.translate.instant(
          'economy.accounting.voucher.saveastemplate'
        ),
        icon: 'save',
      });
    }
  }
  setControlVisibility() {
    this.showEditVoucherNrButton.set(SoeConfigUtil.isSupportSuperAdmin);
    this.setDeleteControlVisibility();
  }

  setDeleteControlVisibility() {
    this.showDeleteButton.set(
      this.modifyPermission() &&
        !this.form?.isNew &&
        (this.form?.template.value || SoeConfigUtil.isSupportSuperAdmin)
    );
  }

  disableControls(): void {
    this.form?.voucherNr.disable();
    this.form?.sourceTypeName.disable();
    if (!this.form?.isNew || this.urlService.isTemplate() || this.isLocked()) {
      this.form?.voucherSeriesId.disable();
    }
  }

  clearAccountingRows() {
    this.accountingRows.set([]);
  }
  setAccountingRows(rows: AccountingRowDTO[]) {
    this.accountingRows.set(rows);
  }

  getFormIsNew() {
    return this.form?.isNew ?? true;
  }

  setToolbarDisability() {
    const disabled = this.getFormIsNew();
    this.toolbarCopyDisabled.set(disabled);
    this.toolbarDoInvertDisabled.set(disabled);
    this.toolbarPrintDisabled.set(disabled);
  }

  //#region Help Methods

  setVoucherDateOnAccountingRows() {
    this.accountingRows.update(rows => {
      return rows.map(row => {
        row.date = this.form?.data.value ?? new Date();
        if (!row.parentRowId) {
          this.messagingService.publish(Constants.EVENT_VOUCHER_DATE_CHANGED, {
            data: row,
            container: AccountingRowsContainers.Voucher,
          });
        }
        return row;
      });
    });
  }

  private onCopy() {
    if (this.form?.isCopy) {
      if (this.form?.additionalPropsOnCopy) {
        const updatedAccountingRows =
          this.form?.additionalPropsOnCopy.accountingRows;
        AccountingRowDTO.clearRowIds(updatedAccountingRows, true);
        this.setAccountingRows(updatedAccountingRows);

        // Clear the IDs of all rows
        this.accountingRows.update(rows => {
          AccountingRowDTO.clearRowIds(rows, true);
          return rows;
        });
        // Clear the IDs of all rows
        AccountingRowDTO.clearRowIds(updatedAccountingRows, true);
        this.form?.customAccountingRowsPatchValue(updatedAccountingRows);
        this.form?.markAsDirty();
      }

      this.loadAccountYearByDate(this.form.date.value)
        .pipe(
          tap(year => {
            if (!year) {
              this,
                this.showError(
                  'economy.accounting.voucher.missingaccountyearfordate'
                );
              return;
            }
            this.accountYear = year;
            this.accountYearIsOpen.set(
              year.status === TermGroup_AccountStatus.Open
            );

            if (!this.accountYearIsOpen() && this.form?.isNew) {
              this.showError('economy.accounting.voucher.accountyearclosed');
              return;
            }

            this.loadDataLookups().subscribe(() => {
              this.new(true, true);
            });
          })
        )
        .subscribe();
    }
  }

  private onNewRecord(addDefaultAccountingRow = false) {
    if (!this.createHouseholdVoucher()) {
      this.ayService
        .getSelectedAccountYear()
        .pipe(
          tap((year: IAccountYearDTO) => {
            if (!year) {
              this.showError(
                'economy.accounting.voucher.missingaccountyearfordate'
              );
              return;
            }
            this.accountYear = year;
            this.accountYearIsOpen.set(
              year.status === TermGroup_AccountStatus.Open
            );

            if (!this.accountYearIsOpen() && this.form?.isNew) {
              this.showError('economy.accounting.voucher.accountyearclosed');
              return;
            }

            this.loadDataLookups().subscribe(() => {
              this.new(true, true);
              if (addDefaultAccountingRow && this.accountingRowsComponent) {
                setTimeout(() => {
                  this.accountingRowsComponent.addRowSetFocusedCell();
                }, 100);
              }
            });
          })
        )
        .subscribe();
    }
  }

  private new(clearRows: boolean, keepVoucherSeriesId = false) {
    let tempVoucherSeriesId = 0;

    if (keepVoucherSeriesId)
      tempVoucherSeriesId = this.form?.voucherSeriesId.value;

    this.form?.patchValue({ status: TermGroup_AccountStatus.Open });
    if (!this.form?.isCopy) {
      if (clearRows) {
        this.setAccountingRows([]);
        this.form?.customAccountingRowsPatchValue([]);
      }
    }

    if (!this.keepNewVoucherAfterSave()) {
      this.form?.patchValue({ voucherHeadId: 0, templateId: undefined });
    }

    if (!this.createHouseholdVoucher()) {
      if (keepVoucherSeriesId) {
        this.setVoucherSeries(tempVoucherSeriesId);
      } else {
        this.setDefaultVoucherSerieId();
      }
    }

    this.filesHelper.reset();
    if ((<any>this.selectedDate)?.inputER?.nativeElement)
      focusOnElement((<any>this.selectedDate).inputER?.nativeElement, 150);
  }

  setSeqNbr() {
    // Get next sequence number for selected voucher series
    if (this.form?.isNew) {
      const series = this.getVoucherSeries(this.form?.voucherSeriesId.value);

      this.form?.patchValue({
        voucherNr: (series?.voucherNrLatest ?? 0) + 1,
      });
    }
  }

  setDefaultVoucherSerieId() {
    const voucherSeridesId = this.urlService.isTemplate()
      ? this.templateVoucherSeriesId
      : this.defaultVoucherSeriesId();
    this.setVoucherSeries(voucherSeridesId);
  }

  setVoucherSeries(voucherSeriesId: number) {
    if (this.voucherSeries && this.voucherSeries.length > 0) {
      const series = this.getVoucherSeries(voucherSeriesId);
      if (series) {
        this.form?.patchValue(
          {
            voucherSeriesId: series.voucherSeriesId,
          },
          { emitEvent: false }
        );
        if (
          this.voucherSeriesManuallyChangedId !== 0 ||
          (series.voucherSeriesId !== this.defaultVoucherSeriesId &&
            series.voucherSeriesId !== this.templateVoucherSeriesId)
        )
          this.voucherSeriesManuallyChangedId = series.voucherSeriesId;
        this.setSeqNbr();
        if (!this.form?.date.value)
          this.form?.patchValue({
            date: new Date(),
          });
      }
    }
  }

  getVoucherSeries(voucherSeriesId: number): any {
    return this.voucherSeries.find(f => f.voucherSeriesId === voucherSeriesId);
  }

  getVoucherSeriesByType(voucherSeriesTypeId: number): any {
    return this.voucherSeries.find(
      x => x.voucherSeriesTypeId === voucherSeriesTypeId
    );
  }

  setDefaultValuesFromVoucherSeries(): void {
    if (this.defaultVoucherSeriesTypeId()) {
      const defaultType = this.getVoucherSeriesByType(
        this.defaultVoucherSeriesTypeId()
      );

      if (defaultType) {
        this.defaultVoucherSeriesId.set(defaultType.voucherSeriesId);
      }
    }
  }

  setVoucherSeriesByType(voucherSeriesTypeId: number) {
    const voucherSerie = this.getVoucherSeriesByType(voucherSeriesTypeId);
    if (voucherSerie) {
      this.setVoucherSeries(voucherSerie.voucherSeriesId);
    }
  }

  private patchFormValues(voucher: any): void {
    this.form?.reset(voucher);
    this.form?.customPatchValue(voucher);
    const accountingRows = VoucherRowDTO.toAccountingRowDTOs(voucher.rows);
    this.form?.customAccountingRowsPatchValue(accountingRows);
    this.form?.customAccountIdsPatchValue(voucher.accountIds);
    this.setControlStatus();
    this.setAccountingRows(accountingRows);
  }

  private showError(translationKey: string): void {
    this.messageboxService.error(
      this.translate.instant('core.error'),
      this.translate.instant(translationKey)
    );
  }

  private showWarning(translationKey: string): void {
    this.messageboxService.error(
      this.translate.instant('core.warning'),
      this.translate.instant(translationKey)
    );
  }

  private handleNewCopy(addDefaultAccountingRow = false) {
    this.setToolbarDisability();
    if (this.form?.isNew) {
      if (!this.form?.isCopy) {
        this.form?.reset();
        this.form?.updateValueAndValidity();
        this.addRowWithSetAccountDimFocus.set(true);
        //new request

        this.onNewRecord(addDefaultAccountingRow);
      } else {
        //copy request
        this.onCopy();
      }
    }
  }
  private resetAccountingRows() {
    const rows: AccountingRowDTO[] = [];
    this.form?.accountingRows.getRawValue()?.forEach((row: any) => {
      row.isModified = false;
      rows.push(row);
    });
    this.form?.customAccountingRowsPatchValue(rows);
    this.accountingRows.set(rows);
  }

  newHouseholdVoucher() {
    this.form?.patchValue({
      voucherSeriesId: this.householdVoucherSeriesId,
    });
    if (this.householdDate) this.form?.patchValue({ date: this.householdDate });

    this.setVoucherSeries(this.form?.voucherSeriesId.value);

    let text = this.translate.instant(
      'economy.accounting.voucher.householdvouchertext'
    );

    if (this.householdInvoiceNbrs && this.householdInvoiceNbrs.length > 0) {
      text +=
        '. ' +
        this.translate.instant('common.customer.invoices.invoicenr') +
        ': ';
      let first = true;
      this.householdInvoiceNbrs.map(x => {
        if (first) {
          text += x;
          first = false;
        } else {
          text += ', ' + x;
        }
      });
    }
    this.form?.patchValue({
      text: text,
    });
    this.createHouseholdVoucher.set(false);
    this.accountingRowsComponent.createDefaultAccountingRow(
      this.householdProductAccountId,
      this.householdAmount,
      false
    );
    this.accountingRowsComponent.createDefaultAccountingRow(
      this.paymentFromTaxAgencyAccountId(),
      this.householdAmount,
      true
    );

    this.form?.markAsDirty();
  }

  //#endregion

  //#region UI events

  accountingRowsReady(guid: Guid) {
    if (this.createHouseholdVoucher()) {
      this.newHouseholdVoucher();
    }
  }
  hasDebitCreditBalanceError(hasError: boolean) {
    this.form?.clearValidators();
    if (hasError) {
      this.form?.addValidators(
        addDebitCreditBalanceValidator(
          this.translate.instant('economy.accounting.voucher.unbalancedrows')
        )
      );
    }
    this.form?.updateValueAndValidity();
  }
  openVoucher(voucherHeadId: number) {
    this.openEditInNewTabSignal()?.set({
      id: voucherHeadId,
      additionalProps: {
        editComponent: VoucherEditComponent,
        FormClass: VoucherForm,
        editTabLabel: 'economy.accounting.voucher.voucher',
        isNew: false,
      },
    });
  }

  accountingRowsChanged(accountingRows: AccountingRowDTO[]) {
    this.form?.customAccountingRowsPatchValue(accountingRows);
  }

  voucherSeriesChanged(voucherSeriesId: number) {
    this.setVoucherSeries(voucherSeriesId);
  }

  voucherDateOnChange(date: any) {
    this.loadAccountYearByDate(this.form?.date.value)
      .pipe(
        tap((year: IAccountYearDTO) => {
          if (!year) {
            this.showError(
              'economy.accounting.voucher.missingaccountyearfordate'
            );
            return;
          }
          this.accountYear = year;
          this.accountYearIsOpen.set(
            year.status === TermGroup_AccountStatus.Open
          );

          if (!this.accountYearIsOpen() && this.form?.isNew) {
            this.showError('economy.accounting.voucher.accountyearclosed');
            return;
          }
          this.loadDataLookups().subscribe(() => {
            this.setVoucherDateOnAccountingRows();
          });
        })
      )
      .subscribe();
  }

  private printVoucher(voucherHeadId: number): void {
    this.setPrintButtonDisabled(true);
    this.performLoadData.load(
      this.requestReportService.printVoucher(voucherHeadId).pipe(
        tap(() => {
          this.setPrintButtonDisabled(false);
        })
      )
    );
  }

  private setPrintButtonDisabled(disabled: boolean): void {
    this.toolbarPrintDisabled.set(disabled);
  }

  doInvert(voucherSeriesTypeId?: number) {
    if (this.form) this.form.isNew = true;
    this.setControlStatus();
    this.setToolbarDisability();
    const hasVatVoucher = this.form?.vatVoucher.value;
    if (hasVatVoucher) {
      this.revertVatVoucherId = this.form?.voucherHeadId.value;
    }
    // Remember head information
    const seriesId = this.form?.voucherSeriesId.value;
    const date = this.form?.date.value;
    const text = this.form?.text.value;

    // Clear all fields but the rows

    this.setNewRefOnTab(Guid.newGuid(), true);
    this.form?.reset();
    this.form?.updateValueAndValidity();

    this.form?.patchValue({ voucherHeadId: 0 });
    if (hasVatVoucher) {
      this.form?.patchValue({ vatVoucher: false });
    }

    if (voucherSeriesTypeId) this.setVoucherSeriesByType(voucherSeriesTypeId);
    else this.setVoucherSeries(seriesId);
    this.form?.patchValue({ date: date });
    this.form?.patchValue({ text: text });
    this.filesHelper.reset();

    const updatedAccountingRows = [...this.accountingRows()];
    // Clear the IDs of all rows
    AccountingRowDTO.clearRowIds(updatedAccountingRows, true);
    // Swap debit and credit
    AccountingRowDTO.invertAmounts(updatedAccountingRows);
    this.accountingRows.set(updatedAccountingRows);

    this.form?.markAsDirty();

    this.updateAccountingRowsGrid();
  }

  doCopy() {
    const additionalProps = {
      accountingRows: this.accountingRows(),
    };
    super.copy(additionalProps);
  }

  templatesOnChange(value: any) {
    if (this.form?.templateId.value) {
      this.clearAccountingRows();
      this.loadTemplate().subscribe((template: VoucherHeadDTO) => {
        this.form?.patchValue(
          {
            vatVoucher: template.vatVoucher,
          },
          { emitEvent: false }
        );
        if (!this.form?.note.value) {
          this.form?.patchValue(
            {
              note: template.note,
            },
            { emitEvent: false }
          );
        }

        if (!this.form?.text.value) {
          this.form?.patchValue(
            {
              text: template.text,
            },
            { emitEvent: false }
          );
        }

        this.setAccountingRows(
          VoucherRowDTO.toAccountingRowDTOs(template.rows)
        );
        // Clear the IDs of all rows
        this.accountingRows.update(rows => {
          AccountingRowDTO.clearRowIds(rows, true);
          return rows;
        });

        this.form?.markAsDirty();
        this.updateAccountingRowsGrid(true);
      });
    }
  }

  loadFileList(opened: boolean) {
    if (opened) this.filesHelper.loadFiles(true, true).subscribe();
  }

  isVoucherTracingOpened(opened: boolean) {
    if (opened) {
      if (this.form?.voucherHeadId.value > 0 && !this.traceRowsRendered()) {
        this.traceRowsRendered.set(true);
      }
    }
  }

  isVoucherHistoryOpened(opened: boolean) {
    if (opened) {
      this.loadVoucherHistory();
    }
  }

  askEditVoucherNr() {
    const mb = this.messageboxService.show(
      this.translate.instant('core.warning'),
      '',
      {
        showInputText: true,
        inputTextLabel: this.translate.instant(
          'economy.accounting.voucher.editvouchernrwarning'
        ),
        buttons: 'okCancel',
      }
    );

    mb.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
      if (response.textValue && Number(response.textValue)) {
        this.editVoucherNr(Number(response.textValue));
      }
    });
  }

  editVoucherNr(newVoucherNr: number) {
    this.service
      .editVoucherNrOnlySuperSupport(
        new EditVoucherNrModel(this.form?.voucherHeadId.value, newVoucherNr)
      )
      .pipe(
        tap(result => {
          if (result.success) {
          } else {
          }
        })
      )
      .subscribe();
  }

  public delete(): Observable<BackendResponse> {
    if (this.form?.template.value) {
      return this.service.deleteVoucher(this.form?.voucherHeadId.value);
    } else {
      return this.service.deleteVouchersOnlySuperSupport([
        this.form?.voucherHeadId.value,
      ]);
    }
  }

  //#endregion

  //#region Overridings

  override createEditToolbar(): void {
    super.createEditToolbar({
      copyOption: {
        onAction: () => {
          if (!this.getFormIsNew()) this.doCopy();
        },
      },
    });

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('doInvert', {
          iconName: signal('arrow-right-arrow-left'),
          caption: signal('economy.accounting.voucher.invert'),
          tooltip: signal('economy.accounting.voucher.invert'),
          disabled: this.toolbarDoInvertDisabled,
          onAction: () => {
            if (!this.getFormIsNew()) this.doInvert();
          },
        }),
      ],
    });
    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('print', {
          iconName: signal('print'),
          tooltip: signal('economy.accounting.voucher.printaccountingorder'),
          disabled: this.toolbarPrintDisabled,
          onAction: () => {
            if (!this.getFormIsNew()) {
              this.printVoucher(this.form?.voucherHeadId.value);
            }
          },
        }),
      ],
    });
  }

  override performDelete(options?: ProgressOptions): void {
    if (!this.form || this.form.invalid || !this.service) return;
    this.additionalDeleteProps = { skipUpdateGrid: false };
    this.performAction.crud(
      CrudActionTypeEnum.Delete,
      this.delete(),
      (res: BackendResponse) => this.emitActionDeleted(res),
      undefined,
      options
    );
  }

  override onPermissionsLoaded() {
    super.onPermissionsLoaded();
    this.modifyPermission.set(
      this.flowHandler.hasModifyAccess(Feature.Economy_Accounting_Vouchers_Edit)
    );
    this.modifyAccountPeriodPermission.set(
      this.flowHandler.hasModifyAccess(
        Feature.Economy_Accounting_AccountPeriods
      )
    );
    this.reportPermission.set(
      this.flowHandler.hasReadAccess(
        Feature.Economy_Distribution_Reports_Selection
      ) &&
        this.flowHandler.hasReadAccess(
          Feature.Economy_Distribution_Reports_Selection_Download
        )
    );
  }

  override onFinished(): void {
    this.setControlStatus();
    this.setButtonOptions();
    this.handleNewCopy();
  }

  override loadUserSettings(): Observable<any> {
    const settingTypes: number[] = [UserSettingType.KeepNewVoucherAfterSave];

    return this.performLoadData.load$(
      this.coreService.getUserSettings(settingTypes).pipe(
        tap(data => {
          const val = SettingsUtil.getBoolUserSetting(
            data,
            UserSettingType.KeepNewVoucherAfterSave,
            false
          );
          this.keepNewVoucherAfterSave.set(val);
          this.formKeepVoucherOpenAfterSave?.patchValue(
            {
              keepVoucherOpenAfterSave: val,
            },
            { emitEvent: false }
          );
        })
      )
    );
  }

  override loadCompanySettings() {
    const settingTypes: CompanySettingType[] = [];

    settingTypes.push(CompanySettingType.AccountingAllowUnbalancedVoucher);
    settingTypes.push(CompanySettingType.AccountingAllowEditVoucher);
    settingTypes.push(CompanySettingType.AccountingAllowEditVoucherDate);
    settingTypes.push(CompanySettingType.AccountingShowEnterpriseCurrency);
    settingTypes.push(CompanySettingType.AccountCustomerPaymentFromTaxAgency);

    return of(
      this.performLoadData.load(
        this.coreService.getCompanySettings(settingTypes).pipe(
          tap(x => {
            this.allowUnbalancedVoucher.set(
              SettingsUtil.getBoolCompanySetting(
                x,
                CompanySettingType.AccountingAllowUnbalancedVoucher
              )
            );
            this.allowEditVoucher.set(
              SettingsUtil.getBoolCompanySetting(
                x,
                CompanySettingType.AccountingAllowEditVoucher
              )
            );
            this.allowEditVoucherDate.set(
              SettingsUtil.getBoolCompanySetting(
                x,
                CompanySettingType.AccountingAllowEditVoucherDate
              )
            );
            this.showEnterpriseCurrency.set(
              SettingsUtil.getBoolCompanySetting(
                x,
                CompanySettingType.AccountingShowEnterpriseCurrency
              )
            );
            this.paymentFromTaxAgencyAccountId.set(
              SettingsUtil.getIntCompanySetting(
                x,
                CompanySettingType.AccountCustomerPaymentFromTaxAgency
              )
            );
          })
        )
      )
    );
  }

  override loadData(): Observable<void> {
    return this.performLoadData.load$(
      this.service.get(this.form?.getIdControl()?.value).pipe(
        mergeMap((voucher: any) => {
          this.patchFormValues(voucher);
          this.filesHelper.recordId.set(this.form?.getIdControl()?.value);

          return this.loadAccountYearByDate(this.form?.date.value).pipe(
            tap((year: IAccountYearDTO) => {
              if (!year) {
                this.showError(
                  'economy.accounting.voucher.missingaccountyearfordate'
                );
                return;
              }
              this.accountYear = year;
              this.accountYearIsOpen.set(
                year.status === TermGroup_AccountStatus.Open
              );

              if (!this.accountYearIsOpen() && this.form?.isNew) {
                this.showError('economy.accounting.voucher.accountyearclosed');
                return;
              }
              this.loadDataLookups().subscribe();
            })
          );
        })
      )
    );
  }

  //#endregion

  //#region Data Loding Functions

  loadAccountYearByAccountYearId(accountYearId = 0): Observable<any> {
    return this.service.getAccountYear(accountYearId, false, false);
  }

  loadAccountYearByDate(voucherDate: any): Observable<any> {
    const dateString = voucherDate ? voucherDate.toDateTimeString() : '';
    return this.service.getAccountYearByDate(dateString, false);
  }

  loadDataLookups() {
    return this.performLoadData.load$(
      forkJoin([
        this.loadTemplates(this.accountYear.accountYearId),
        this.loadVoucherSeries(this.accountYear.accountYearId),
        this.loadDefaultVoucherSeriesId(this.accountYear.accountYearId),
        this.loadHousholdVoucherSeries(this.accountYear.accountYearId),
      ]).pipe(
        tap(() => {
          this.setDefaultValuesFromVoucherSeries();
          if (this.form?.value && this.form?.voucherSeriesTypeId.value) {
            this.setVoucherSeriesByType(this.form?.voucherSeriesTypeId.value);
          } else {
            this.setDefaultVoucherSerieId();
          }
          this.loadAccountPeriod(this.accountYear.accountYearId, false);
        })
      )
    );
  }
  private loadHousholdVoucherSeries(accountYearId: number): Observable<number> {
    if (accountYearId === 0 || !this.createHouseholdVoucher()) {
      this.householdVoucherSeriesId.set(0);
      return of(0);
    }
    return this.service
      .getDefaultVoucherSeriesId(
        accountYearId,
        CompanySettingType.CustomerPaymentVoucherSeriesType
      )
      .pipe(
        tap(x => {
          this.householdVoucherSeriesId.set(x);
        })
      );
  }

  private loadAccountPeriod(accountYearId: number, forceRefresh = false) {
    if (!this.form?.date.value || accountYearId === 0 || !accountYearId) {
      this.accountPeriod = undefined;
      return;
    }

    this.service
      .getAccountPeriod(
        accountYearId,
        DateUtil.toDateString(this.form?.date.value),
        false,
        forceRefresh
      )
      .pipe(
        tap(x => {
          this.accountPeriod = x;

          //Validate period is open n
          if (
            this.accountPeriod?.status !== TermGroup_AccountStatus.Open &&
            this.form?.isNew
          ) {
            if (this.modifyAccountPeriodPermission()) {
              const mb = this.messageboxService.show(
                this.translate.instant('core.warning'),
                '',
                {
                  showInputText: true,
                  inputTextLabel: this.translate.instant(
                    'economy.accounting.voucher.periodnotopenmodify'
                  ),
                  buttons: 'okCancel',
                }
              );

              mb.afterClosed().subscribe(
                (response: IMessageboxComponentResponse) => {
                  if (response.result) {
                    this.openAccountingPeriod();
                  }
                }
              );
            } else {
              this.showWarning('economy.accounting.voucher.periodnotopen');
            }
          }
        })
      )
      .subscribe();
  }

  loadTemplate() {
    return this.performLoadData.load$(
      this.service.getVoucher(
        this.form?.templateId.value,
        false,
        true,
        true,
        false
      )
    );
  }

  loadTemplates(accountYearId: number): Observable<void> {
    return this.performLoadData.load$(
      this.service.getVoucherTemplatesDict(accountYearId).pipe(
        tap(x => {
          this.templates = x;
        })
      )
    );
  }
  loadVoucherHistory() {
    if (this.form?.voucherHeadId.value > 0 && !this.historyLoaded()) {
      this.performLoadData.load(
        this.service.getVoucherRowHistory(this.form?.voucherHeadId.value).pipe(
          tap(data => {
            this.historyGridRows.next(data);
            this.historyLoaded.set(true);
          })
        )
      );
    }
  }

  loadVoucherSeries(accountYearId: number): Observable<void> {
    //Never use cache since latest or start number might have been updated else where
    return this.performLoadData.load$(
      this.service.getVoucherSeriesByYear(accountYearId, true).pipe(
        tap(result => {
          this.voucherSeries = result;
          const templateVoucherSerie = this.voucherSeries.find(
            s => s.voucherSeriesTypeIsTemplate
          );

          if (templateVoucherSerie)
            this.templateVoucherSeriesId = templateVoucherSerie.voucherSeriesId;

          if (!this.urlService.isTemplate()) {
            this.voucherSeries = this.voucherSeries.filter(
              x => !x.voucherSeriesTypeIsTemplate
            );
          }
        })
      )
    );
  }

  loadDefaultVoucherSeriesId(accountYearId: number): Observable<number> {
    return this.service
      .getDefaultVoucherSeriesId(
        accountYearId,
        CompanySettingType.AccountingVoucherSeriesTypeManual
      )
      .pipe(
        tap(voucherSerieId => {
          this.defaultVoucherSeriesId.set(voucherSerieId);
        })
      );
  }

  //#endregion

  //#region Action Perform

  private updateAccountingRowsGrid(setRowItemAccountsOnAllRows = false) {
    this.accountingRowsComponent.rowsAdded(setRowItemAccountsOnAllRows);
  }

  executeSaveFunction(option: any) {
    switch (option.id) {
      case VoucherEditSaveFunctions.Save:
        this.save();
        break;
      case VoucherEditSaveFunctions.SaveAndPrint:
        this.save(true);
        break;
      case VoucherEditSaveFunctions.SaveAsTemplate:
        this.save(false, true);
        break;
    }
  }

  public save(print = false, saveAsTemplate = false) {
    if (this.isSaving()) return;
    this.accountingRowsComponent?.applyGridChanges();
    this.showInfoMessage.set(false);
    this.infoMessage.set('');
    this.additionalToolbarItemGroups = [];

    this.isSaving.set(true);

    if (this.form?.isNew) {
      this.form?.patchValue({ status: TermGroup_AccountStatus.Open });
      this.form?.patchValue({
        accountPeriodId: this.accountPeriod
          ? this.accountPeriod.accountPeriodId
          : 0,
      });
    }

    const files: FileUploadDTO[] = [];

    let updateTab = false;
    if (saveAsTemplate || this.urlService.isTemplate()) {
      if (this.filesHelper.files.length > 0) {
        this.showError('economy.accounting.voucher.templateswithattachments');
        this.isSaving.set(false);
        return;
      }
      if (!this.templateVoucherSeriesId) {
        this.showError(
          'economy.accounting.voucher.missingtemplatevoucherserie'
        );
        this.isSaving.set(false);
        return;
      }
      //saving a existing voucher as a template?
      if (!this.form?.template.value && this.form?.voucherHeadId.value) {
        this.form?.patchValue({ voucherHeadId: 0 });
        updateTab = true;
      }
      this.form?.patchValue({ template: true });
      this.form?.patchValue({ voucherSeriesId: this.templateVoucherSeriesId });
    }

    // Clear empty amounts
    const accountingRows: AccountingRowDTO[] =
      this.form?.accountingRows.value || [];
    accountingRows.forEach((row: AccountingRowDTO) => {
      const r = new AccountingRowDTO(row);
      Object.assign(r, row);
      if (!r.debitAmount) {
        r.setDebitAmount(TermGroup_CurrencyType.BaseCurrency, 0);
      }
      if (!r.debitAmountEntCurrency)
        r.setDebitAmount(TermGroup_CurrencyType.EnterpriseCurrency, 0);
      if (!r.debitAmountLedgerCurrency)
        r.setDebitAmount(TermGroup_CurrencyType.LedgerCurrency, 0);
      if (!r.debitAmountCurrency)
        r.setDebitAmount(TermGroup_CurrencyType.TransactionCurrency, 0);
      if (!r.creditAmount)
        r.setCreditAmount(TermGroup_CurrencyType.BaseCurrency, 0);
      if (!r.creditAmountEntCurrency)
        r.setCreditAmount(TermGroup_CurrencyType.EnterpriseCurrency, 0);
      if (!r.creditAmountLedgerCurrency)
        r.setCreditAmount(TermGroup_CurrencyType.LedgerCurrency, 0);
      if (!r.creditAmountCurrency)
        r.setCreditAmount(TermGroup_CurrencyType.TransactionCurrency, 0);
    });
    const savedVoucherNr = this.form?.voucherNr.value;
    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service
        .saveVoucher(
          new SaveVoucherModel(
            this.form?.getAllValues({ includeDisabled: true }),
            accountingRows,
            this.householdRowIds ? this.householdRowIds : [],
            files,
            this.revertVatVoucherId
          )
        )
        .pipe(
          tap(res => {
            if (res.success) {
              this.showInfoMessage.set(true);
              ResponseUtil.setBooleanValue(res, print);
              ResponseUtil.setStringValue(res, savedVoucherNr);
              // res.booleanValue = print;
              // res.stringValue = savedVoucherNr;
              this.additionalSaveProps = {
                keepNewFormOnAfterSave: !this.keepNewVoucherAfterSave(),
              };
              this.resetAccountingRows();
              this.updateFormValueAndEmitChange(res, true);
              this.triggerCloseDialog(res);
            }
          })
        )
    );
  }

  override onSaveCompleted(backendResponse: BackendResponse): void {
    if (backendResponse.success) {
      this.afterSaveSuccess(
        backendResponse,
        ResponseUtil.getBooleanValue(backendResponse), //backendResponse.booleanValue,
        ResponseUtil.getStringValue(backendResponse) //backendResponse.stringValue
      );

      if (!this.keepNewVoucherAfterSave()) {
        if (this.form) this.form.isNew = true;
        this.setControlStatus();
        this.setNewRefOnTab(Guid.newGuid(), true);
        this.handleNewCopy(true);
      } else {
        if (this.form) this.form.isNew = false;
        this.form?.markAsUntouched();
      }
      this.setDeleteControlVisibility();
    } else {
      this.afterSaveError(backendResponse);
    }
  }

  private afterSaveSuccess = (
    result: BackendResponse,
    print: boolean,
    savedVoucherNr: string
  ): void => {
    if (result.success) {
      const newVoucherHeadId = ResponseUtil.getEntityId(result);

      if (newVoucherHeadId && newVoucherHeadId > 0)
        this.form?.patchValue({ voucherHeadId: newVoucherHeadId });

      this.sequenceNumber = Number(ResponseUtil.getNumberValue(result)); //result.integerValue2
      const accountYearId = this.accountYear.accountYearId;

      const model = new CalculateAccountBalanceForAccountsFromVoucherModel(
        accountYearId
      );
      this.service
        .calculateAccountBalanceForAccountsFromVoucher(model)
        .subscribe();

      // If a new template was saved, update the Template list
      if (this.form?.template.value)
        this.loadTemplates(accountYearId).subscribe();

      if (print) {
        this.printVoucher(newVoucherHeadId);
      }

      this.filesHelper.recordId.set(this.form?.getIdControl()?.value);

      if (this.filesHelper.filesLoaded()) {
        //this.documentExpanderIsOpen = false;
        if (!this.form?.isNew) this.filesHelper.loadFiles(true);
      }
      const message = ResponseUtil.getMessageValue(result);
      if (message && message.length > 0) {
        this.progress.saveComplete(<ProgressOptions>{
          showDialogOnComplete: true,
          showToastOnComplete: false,
          title: 'core.info',
          message: message,
        });
      }
      //add info bar and tool bar
      this.craeteAdditionalToolbarItemGroup(newVoucherHeadId);

      if (this.form?.isNew) {
        this.setShowInfoMessage(
          this.translate
            .instant('economy.accounting.voucher.vouchercreated')
            .format(savedVoucherNr)
        );
        this.loadVoucherSeries(accountYearId).subscribe();
      } else {
        this.setShowInfoMessage(
          this.translate
            .instant('economy.accounting.voucher.voucherupdated')
            .format(this.form?.voucherNr.value.toString())
        );
        if (this.historyLoaded()) {
          this.loadVoucherHistory();
        }
      }
    } else {
      this.progress.saveComplete(<ProgressOptions>{
        showDialogOnError: true,
        showToastOnError: false,
        message: ResponseUtil.getErrorMessage(result),
      });
    }
    this.isSaving.set(false);
  };

  private afterSaveError = (result: BackendResponse): void => {
    this.progress.saveComplete(<ProgressOptions>{
      showDialogOnError: true,
      showToastOnError: false,
      message: ResponseUtil.getErrorMessage(result),
    });
    this.isSaving.set(false);
  };

  private setShowInfoMessage(text: string) {
    this.infoMessage.set(text);
    this.showInfoMessage.set(!this.form?.template.value);
  }

  private craeteAdditionalToolbarItemGroup(newVoucherHeadId: number) {
    this.additionalToolbarItemGroups = [];

    this.additionalToolbarItemGroups = [
      {
        alignLeft: true,
        items: [
          this.toolbarService.createToolbarButton('print', {
            iconName: signal('print'),
            tooltip: signal('economy.accounting.voucher.askprint1'),
            disabled: this.toolbarPrintDisabled,
            onAction: () => this.printVoucher(newVoucherHeadId),
          }),
        ],
      },
    ];
  }

  private openAccountingPeriod() {
    return this.performLoadData.load(
      this.service
        .updateAccountPeriodStatus(
          this.accountPeriod?.accountPeriodId
            ? this.accountPeriod?.accountPeriodId
            : 0,
          TermGroup_AccountStatus.Open
        )
        .pipe(
          tap(result => {
            if (result.success)
              this.loadAccountPeriod(this.accountYear.accountYearId, true);
            else {
              if (
                result.errorNumber === ActionResultSave.AccountPeriodNotOpen
              ) {
                this.showWarning(
                  'economy.accounting.voucher.openperioderrornext'
                );
              } else
                this.showWarning('economy.accounting.voucher.openperioderror');
            }
          })
        )
    );
  }

  saveKeepVoucherSetting(keepNewVoucherAfterSave: boolean) {
    const model = new SaveUserCompanySettingModel(
      SettingMainType.User,
      UserSettingType.KeepNewVoucherAfterSave,
      keepNewVoucherAfterSave
    );
    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.coreService.saveBoolSetting(model).pipe(
        tap(result => {
          if (result.success) {
            this.keepNewVoucherAfterSave.set(keepNewVoucherAfterSave);
          }
        })
      )
    );
  }

  onEditCompletedForBalancedAccount() {
    if ((<any>this.saveButton)?.inputER?.nativeElement)
      focusOnElement((<any>this.saveButton).inputER.nativeElement, 150);
  }

  //#endregion
}
