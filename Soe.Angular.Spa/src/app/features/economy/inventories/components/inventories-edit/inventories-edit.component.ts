import {
  Component,
  computed,
  inject,
  OnDestroy,
  OnInit,
  signal,
} from '@angular/core';
import { InventoryWriteOffTemplatesDTO } from '@features/economy/inventory-write-off-templates/models/inventory-write-off-templates.model';
import { AccountingSettingsRowDTO } from '@shared/components/accounting-settings/accounting-settings/accounting-settings.models';
import { CategoryItem } from '@shared/components/categories/categories.model';
import { FilesHelper } from '@shared/components/files-helper/files-helper.component';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import { ValidationHandler } from '@shared/handlers';
import { TermCollection } from '@shared/localization/term-types';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { ICustomerInvoiceGridDTO } from '@shared/models/generated-interfaces/CustomerInvoiceDTOs';
import {
  Feature,
  SoeDataStorageRecordType,
  SoeEntityType,
  SoeOriginStatusClassification,
  SoeOriginStatusClassificationGroup,
  SoeOriginType,
  TermGroup,
  TermGroup_ChangeStatusGridAllItemsSelection,
  TermGroup_InventoryStatus,
  TermGroup_InventoryWriteOffMethodPeriodType,
  TermGroup_InventoryWriteOffMethodType,
} from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import {
  IAccountingSettingsRowDTO,
  IInventoryWriteOffMethodGridDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ISupplierInvoiceGridDTO } from '@shared/models/generated-interfaces/SupplierInvoiceDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { BrowserUtil } from '@shared/util/browser-util';
import { DateUtil } from '@shared/util/date-util';
import { NumberUtil } from '@shared/util/number-util';
import { InventoryAdjustFunctions } from '@shared/util/Enumerations';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { MenuButtonItem } from '@ui/button/menu-button/menu-button.component';
import { ToolbarEditConfig } from '@ui/toolbar/models/toolbar';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, Subject, take, tap, takeUntil } from 'rxjs';
import { InventoryWriteOffMethodsService } from '../../../inventory-write-off-methods/services/inventory-write-off-methods.service';
import { InventoryWriteOffTemplatesService } from '../../../inventory-write-off-templates/services/inventory-write-off-templates.service';
import { VoucherSeriesTypeService } from '../../../services/voucher-series-type.service';
import { InventoriesForm } from '../../models/inventories-form.model';
import { InventoriesUploadForm } from '../../models/inventories-upload-form.model';
import {
  CustomerInvoicesGridModel,
  InventoriesAdjustmentDialogData,
  InventoryDTO,
  InventoryUploadDTO,
  SaveInventoryModel,
} from '../../models/inventories.model';
import { InventoriesService } from '../../services/inventories.service';
import { InventoriesAdjustmentDialogComponent } from '../inventories-adjustment-dialog/inventories-adjustment-dialog.component';

@Component({
  selector: 'soe-inventories-edit',
  templateUrl: './inventories-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class InventoriesEditComponent
  extends EditBaseDirective<InventoryDTO, InventoriesService, InventoriesForm>
  implements OnInit, OnDestroy
{
  private readonly noPropagation = {
    onlySelf: true,
    emitEvent: false,
  };

  reload$ = new Subject<void>();

  adjustFunctions: MenuButtonItem[] = [];
  disposeFunctions: MenuButtonItem[] = [];

  dialogServiceV2 = inject(DialogService);
  coreService = inject(CoreService);
  service = inject(InventoriesService);
  writeOffTemplatesService = inject(InventoryWriteOffTemplatesService);
  voucherService = inject(VoucherSeriesTypeService);
  writeOffMethodsService = inject(InventoryWriteOffMethodsService);
  validationHandler = inject(ValidationHandler);
  dialogService = inject(DialogService);

  uploadForm: InventoriesUploadForm = new InventoriesUploadForm({
    validationHandler: this.validationHandler,
    element: new InventoryUploadDTO(),
  });

  inventoryWriteOffTemplates: InventoryWriteOffTemplatesDTO[] = [];
  voucherSeries: ISmallGenericType[] = [];
  inventoryWriteOffMethods: IInventoryWriteOffMethodGridDTO[] = [];
  inventories: ISmallGenericType[] = [];
  periodTypes: ISmallGenericType[] = [];
  accountSettingTypes: SmallGenericType[] = [];
  baseAccounts: SmallGenericType[] = [];
  supplierInvoices: ISupplierInvoiceGridDTO[] = [];
  customerInvoices: ICustomerInvoiceGridDTO[] = [];
  accountingSettings: AccountingSettingsRowDTO[] = [];
  inventoryWriteOffTemplate!: InventoryWriteOffTemplatesDTO;
  inventoryStatuses: ISmallGenericType[] = [];

  private _destroy$ = new Subject<void>();

  filesHelper!: FilesHelper;
  isFileDisplayAccordionOpen: boolean = false;
  isTrackChangesAccordionOpen = false;

  isDraft = signal(false);
  isWriteOffsStarted = signal(false);
  isWrittenOff = signal(false);
  isValueZero = signal(false);
  isDisposed = signal(false);
  isActive = computed(
    () => !this.isDraft() && !this.isWrittenOff() && !this.isDisposed()
  );
  isDraftOrActive = computed(() => this.isDraft() || this.isActive());
  hideWriteOffOption = computed(() => !this.isValueZero() || !this.isActive());

  get inventoryId() {
    return this.form?.getIdControl()?.value;
  }

  constructor() {
    super();
    this.filesHelper = new FilesHelper(
      true,
      SoeEntityType.Inventory,
      SoeDataStorageRecordType.InventoryFileAttachment,
      Feature.Economy_Inventory_Inventories_Edit,
      this.performLoadData
    );
  }

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(Feature.Economy_Inventory_Inventories_Edit, {
      lookups: [
        this.loadSupplierInvoices(),
        this.loadCompanySettings(),
        this.loadBaseAccounts(),
        this.loadPeriodTypes(),
        this.loadVoucherSeriesTypes(),
        this.loadWriteOffTemplates(),
        this.loadWriteOffMethods(),
        this.loadCustomerInvoices(),
        this.loadInventoriesDict(),
        this.loadInventoryStatus(),
      ],
    });

    if (this.form?.isCopy || this.form?.isNew) this.callNextInventoryNr();

    this.form?.statusName.valueChanges
      .pipe(takeUntil(this._destroy$))
      .subscribe(() => {
        const status = this.inventoryStatuses.find(
          f => f.name === this.form?.statusName.value
        )?.id as TermGroup_InventoryStatus;
        this.updateFormControls(status);
      });
  }

  buildFunctionList() {
    //Adjust
    this.adjustFunctions.push({
      id: InventoryAdjustFunctions.OverWriteOff,
      label: this.translate.instant(
        'economy.inventory.inventories.overwriteoff'
      ),
    });
    this.adjustFunctions.push({
      id: InventoryAdjustFunctions.UnderWriteOff,
      label: this.translate.instant(
        'economy.inventory.inventories.underwriteoff'
      ),
    });
    this.adjustFunctions.push({
      id: InventoryAdjustFunctions.WriteDown,
      label: this.translate.instant('economy.inventory.inventories.writedown'),
    });
    this.adjustFunctions.push({
      id: InventoryAdjustFunctions.WriteUp,
      label: this.translate.instant('economy.inventory.inventories.writeup'),
    });

    //Dispose
    this.disposeFunctions.push({
      id: InventoryAdjustFunctions.Sold,
      label: this.translate.instant('economy.inventory.inventories.sold'),
    });
    this.disposeFunctions.push({
      id: InventoryAdjustFunctions.Discarded,
      label: this.translate.instant('economy.inventory.inventories.discarded'),
    });
    this.disposeFunctions.push({
      id: InventoryAdjustFunctions.WrittenOff,
      label: this.translate.instant(
        'economy.inventory.inventories.setaswrittenoff'
      ),
      hidden: this.hideWriteOffOption,
    });
  }

  executeAdjustFunction(option: MenuButtonItem) {
    this.openInventoryAdjustmentDialog(option.id || 0).subscribe(res => {
      if (!res.success) return;

      this.loadWriteOffTemplate(this.form?.writeOffTemplateId.value);
    });
  }

  executeDisposeFunction(option: MenuButtonItem) {
    if (option.id === InventoryAdjustFunctions.WrittenOff) {
      this.form?.patchValue({
        inventoryStatus: TermGroup_InventoryStatus.WrittenOff,
      });
      this.performSave();
    } else {
      this.openInventoryAdjustmentDialog(option.id || 0).subscribe(res => {
        if (!res.success) return;

        this.reloadData();
      });

      this.loadWriteOffTemplate(this.form?.writeOffTemplateId.value);
    }
  }

  override performSave(): void {
    if (!this.form || this.form.invalid || !this.service) return;

    const model = new SaveInventoryModel();
    model.inventory = this.form.getRawValue();
    model.categoryRecords = this.form?.categoryIds.value.map(id => {
      return {
        categoryId: id,
        default: false,
      };
    });
    model.inventory.accountingSettings = this.form?.accountingSettings.value;
    model.inventory.status = this.form.inventoryStatus.value;

    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service.save(model).pipe(tap(this.updateFormValueAndEmitChange))
    );
    this.updateFormControls(this.form.inventoryStatus.value);
  }

  override onFinished(): void {
    this.loadSettingsTypes();
    this.buildFunctionList();
    this.clearValuesWhenCopied();
    this.updateFormControls(this.form?.inventoryStatus.value);
  }

  private clearValuesWhenCopied() {
    if (this.form?.isCopy) {
      this.form?.patchValue({
        writeOffSum: 0.0,
        endAmount: 0.0,
        writeOffPeriods: 0,
        inventoryStatus: TermGroup_InventoryStatus.Draft,
      });
    }
  }

  public purchaseAmountChanged(value: number) {
    this.form?.purchaseAmount.setValue(value, this.noPropagation);
    if (this.form?.inventoryStatus.value !== TermGroup_InventoryStatus.Draft)
      return;

    const purchaseAmount = value;
    const writeOffSum = this.form?.writeOffSum.value ?? 0;
    const endAmount = this.form?.endAmount.value ?? 0;
    const writeOffRemainingAmount = purchaseAmount - writeOffSum - endAmount;
    this.form?.patchValue({
      writeOffAmount: purchaseAmount - endAmount,
      writeOffRemainingAmount: writeOffRemainingAmount,
    });

    if (writeOffRemainingAmount == 0) this.isValueZero.set(true);
  }

  public previouslyDepreciatedAmountChanged(value: number) {
    this.form?.writeOffSum.setValue(value, this.noPropagation);
    if (this.form?.inventoryStatus.value !== TermGroup_InventoryStatus.Draft)
      return;

    const writeOffSum = value;
    const purchaseAmount = this.form?.purchaseAmount.value ?? 0;
    const endAmount = this.form?.endAmount.value ?? 0;
    const writeOffRemainingAmount = purchaseAmount - writeOffSum - endAmount;
    this.form?.patchValue({
      writeOffRemainingAmount: writeOffRemainingAmount,
      accWriteOffAmount: writeOffSum,
    });

    if (writeOffRemainingAmount == 0) this.isValueZero.set(true);
  }

  public endAmountChanged(value: number) {
    this.form?.endAmount.setValue(value, this.noPropagation);
    if (this.form?.inventoryStatus.value !== TermGroup_InventoryStatus.Draft)
      return;

    const endAmount = value;
    const purchaseAmount = this.form?.purchaseAmount.value ?? 0;
    const writeOffSum = this.form?.writeOffSum.value ?? 0;
    const writeOffRemainingAmount = purchaseAmount - writeOffSum - endAmount;
    this.form?.patchValue({
      writeOffAmount: purchaseAmount - endAmount,
      writeOffRemainingAmount: writeOffRemainingAmount,
    });

    if (writeOffRemainingAmount == 0) this.isValueZero.set(true);
  }

  override loadTerms(): Observable<TermCollection> {
    return super.loadTerms([
      'economy.inventory.inventories.years',
      'economy.accounting.voucher.voucher',
      'economy.inventory.inventories.supplierinvoice',
      'economy.inventory.inventories.customerinvoice',
      'common.name',
      'core.edit',
      'core.yes',
      'core.no',
      'economy.inventory.inventoryaccountsettingtype.inventory',
      'economy.inventory.inventoryaccountsettingtype.accwriteoff',
      'economy.inventory.inventoryaccountsettingtype.writeoff',
      'economy.inventory.inventoryaccountsettingtype.accoverwriteoff',
      'economy.inventory.inventoryaccountsettingtype.overwriteoff',
      'economy.inventory.inventoryaccountsettingtype.accwritedown',
      'economy.inventory.inventoryaccountsettingtype.writedown',
      'economy.inventory.inventoryaccountsettingtype.accwriteup',
      'economy.inventory.inventoryaccountsettingtype.writeup',
      'economy.inventory.inventories.overwriteoff',
      'economy.inventory.inventories.underwriteoff',
      'economy.inventory.inventories.writedown',
      'economy.inventory.inventories.writeup',
      'economy.inventory.inventories.sold',
      'economy.inventory.inventories.discarded',
      'economy.inventory.inventories.setaswrittenoff',
    ]);
  }

  private setPeriodValuePercent() {
    this.form?.info.disable();

    let percent = 0,
      nbrOfYears = 0;
    let info = '';

    const periodName = this.periodTypes.find(
      e => e.id == this.form?.periodType.value
    )?.name;

    const period = this.inventoryWriteOffMethods.find(
      x =>
        x.inventoryWriteOffMethodId ===
        this.form!.inventoryWriteOffMethodId.value
    )!;

    const isIteration =
      period.type === TermGroup_InventoryWriteOffMethodType.Immediate ||
      period.type ===
        TermGroup_InventoryWriteOffMethodType.AccordingToTheBooks_ComplementaryRule;

    if (isIteration) {
      if (this.form?.periodValue.value > 0) {
        percent = 100 / this.form?.periodValue.value;
      }

      if (
        this.form?.periodType.value ==
        TermGroup_InventoryWriteOffMethodPeriodType.Period
      ) {
        nbrOfYears = this.form?.periodValue.value / 12;
      }
    } else {
      percent = period.yearPercent;

      if (
        this.form!.periodType.value ==
        TermGroup_InventoryWriteOffMethodPeriodType.Period
      ) {
        percent = percent / 12;
      }
    }

    info = percent.toFixed(2) + '% per ' + periodName?.toLowerCase();
    if (nbrOfYears > 0) {
      info +=
        ', ' +
        nbrOfYears.toFixed(2) +
        ' ' +
        this.terms['economy.inventory.inventories.years'];
    }
    this.form?.patchValue({ info: info });
  }

  override createEditToolbar(config?: Partial<ToolbarEditConfig>): void {
    super.createEditToolbar(config);

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButtonReload({
          onAction: () => this.reloadData(),
        }),
      ],
    });
  }

  reloadData() {
    if (this.form?.isNew || this.form?.isCopy) {
      this.reloadNewData();
    } else {
      this.loadData().subscribe();
      this.reload$.next();
    }
  }

  reloadNewData() {
    this.form?.patchValue({
      inventoryNr: '',
      name: '',
      parentName: 0,
      purchaseAmount: 0,
      writeOffSum: 0,
      endAmount: 0,
      writeOffTemplateId: 0,
      description: '',
      voucherSeriesTypeId: 0,
      inventoryWriteOffMethodId: 0,
      periodType: 0,
      periodValue: 0,
      purchaseDate: undefined,
      writeOffDate: undefined,
    });
    this.form?.patchCategories([]);
    this.form?.accountingSettings.reset();
    this.callNextInventoryNr();
  }

  override loadData(): Observable<void> {
    this.filesHelper.recordId.set(this.form?.getIdControl()?.value);

    return this.performLoadData.load$(
      this.service.get(this.form?.getIdControl()?.value).pipe(
        tap(value => {
          this.resetSignals();
          if (value.writeOffRemainingAmount <= 0) this.isValueZero.set(true);

          value.accWriteOffAmount = (
            value.writeOffAmount - value.writeOffRemainingAmount
          ).round(2);
          this.form?.customPatchValue(value);
          this.setPeriodValuePercent();

          if (value.accWriteOffAmount !== value.writeOffSum)
            this.setAsWriteOffsStarted();

          this.form?.patchValue({
            showPreliminary:
              this.form?.inventoryStatus.value ==
              TermGroup_InventoryStatus.Draft,
          });
        })
      )
    );
  }

  private resetSignals() {
    if (this.isDraft()) this.isDraft.set(false);
    if (this.isWriteOffsStarted()) this.isWriteOffsStarted.set(false);
    if (this.isWrittenOff()) this.isWrittenOff.set(false);
    if (this.isValueZero()) this.isValueZero.set(false);
    if (this.isDisposed()) this.isDisposed.set(false);
  }

  purchaseDateChange(purchaseDate: Date | undefined) {
    if (purchaseDate && (this.form?.isNew || this.form?.isCopy)) {
      this.form?.patchValue({
        writeOffDate: DateUtil.getDateFirstInMonth(purchaseDate),
      });
    }
  }

  writeOffTemplateChanged() {
    const templateId = this.form?.value.writeOffTemplateId;
    const writeOffTemplate = this.inventoryWriteOffTemplates.find(
      f => f.inventoryWriteOffTemplateId == templateId
    );
    if (!writeOffTemplate) return;

    this.form?.accountingSettings.patch(writeOffTemplate.accountingSettings);

    this.form?.patchValue({
      voucherSeriesTypeId: writeOffTemplate?.voucherSeriesTypeId,
      inventoryWriteOffMethodId: writeOffTemplate?.inventoryWriteOffMethodId,
    });
    this.writeOffMethodIdChanged();
  }

  writeOffMethodIdChanged() {
    const period = this.inventoryWriteOffMethods.find(
      x =>
        x.inventoryWriteOffMethodId ===
        this.form?.inventoryWriteOffMethodId.value
    );
    if (period) {
      this.form?.patchValue({
        periodType: period.periodType,
        periodValue: period.periodValue,
      });
    }

    this.setPeriodValuePercent();
  }

  fileListOpened(opened: boolean) {
    this.isFileDisplayAccordionOpen = opened;
    this.loadFileList();
  }

  loadFileList() {
    if (this.isFileDisplayAccordionOpen) {
      this.performLoadData
        .load$(this.filesHelper.loadFiles(true, true))
        .subscribe();
    }
  }

  callNextInventoryNr() {
    this.performLoadData.load(
      this.service.getNextInventoryNr().pipe(
        tap(x => {
          this.form?.patchValue({ inventoryNr: x });
        })
      )
    );
  }

  loadWriteOffTemplates() {
    return this.writeOffTemplatesService.getAll().pipe(
      tap(data => {
        this.inventoryWriteOffTemplates = data;
      })
    );
  }

  loadPeriodTypes() {
    return this.coreService
      .getTermGroupContent(
        TermGroup.InventoryWriteOffMethodPeriodType,
        false,
        false
      )
      .pipe(
        tap(data => {
          this.periodTypes = data;
        })
      );
  }

  loadVoucherSeriesTypes() {
    return this.voucherService.getVoucherSeriesTypesByCompany(false, true).pipe(
      tap(data => {
        this.voucherSeries = data;
      })
    );
  }

  loadWriteOffMethods() {
    return this.writeOffMethodsService.getGrid().pipe(
      tap(data => {
        this.inventoryWriteOffMethods = data;
      })
    );
  }

  loadSettingsTypes() {
    this.accountSettingTypes = this.service.getSettingsTypes(this.terms);
  }

  private loadBaseAccounts(): Observable<SmallGenericType[]> {
    return this.service.getBaseAccounts().pipe(
      tap(result => {
        this.baseAccounts = result;
      })
    );
  }

  openInventoryAdjustmentDialog(adjustmentType: InventoryAdjustFunctions) {
    const dialogData = new InventoriesAdjustmentDialogData();
    dialogData.title = 'economy.inventory.inventories.createadjustment';
    dialogData.size = 'xl';
    dialogData.hideFooter = true;
    dialogData.inventoryId = this.form?.inventoryId.value;
    dialogData.purchaseDate = this.form?.purchaseDate.value;
    dialogData.purchaseAmount = this.form?.purchaseAmount.value;
    dialogData.accWriteOffAmount = NumberUtil.round(
      this.form?.writeOffAmount.value -
        this.form?.writeOffRemainingAmount.value,
      2
    );

    dialogData.adjustmentType = adjustmentType;
    dialogData.accountingSettings = <any>this.form?.accountingSettings.value;
    dialogData.inventoryBaseAccounts = this.baseAccounts;
    dialogData.noteText =
      this.form?.inventoryNr.value + ' ' + this.form?.name.value;

    return this.dialogServiceV2
      .open(InventoriesAdjustmentDialogComponent, dialogData)
      .afterClosed()
      .pipe(take(1));
  }

  private loadWriteOffTemplate(id: number) {
    if (id) {
      this.inventoryWriteOffTemplate =
        this.inventoryWriteOffTemplates.find(
          f => f.inventoryWriteOffTemplateId == id
        ) ?? new InventoryWriteOffTemplatesDTO();

      this.setPeriodValuePercent();
    }
  }

  accountSettingsChanged(rows: IAccountingSettingsRowDTO[]) {
    this.form?.accountingSettings.rawPatch(rows);
  }

  loadSupplierInvoices(): Observable<ISupplierInvoiceGridDTO[]> {
    return this.performLoadData.load$(
      this.coreService
        .getInvoices(
          true,
          true,
          false,
          TermGroup_ChangeStatusGridAllItemsSelection.Twelve_Months,
          false,
          0
        )
        .pipe(
          tap(data => {
            const supplierInvoices: ISupplierInvoiceGridDTO[] = [];
            data.map(x => {
              if (x.seqNr && x.supplierInvoiceId) {
                x.supplierName =
                  x.seqNr + ' - ' + x.invoiceNr + ' - ' + x.supplierName;
                supplierInvoices.push(x);
              }
            });
            this.supplierInvoices = supplierInvoices;
          })
        )
    );
  }

  loadCustomerInvoices(): Observable<ICustomerInvoiceGridDTO[]> {
    const model = new CustomerInvoicesGridModel(
      SoeOriginStatusClassification.CustomerInvoicesAll,
      SoeOriginType.CustomerInvoice,
      true,
      true,
      false,
      true,
      TermGroup_ChangeStatusGridAllItemsSelection.Twelve_Months,
      false,
      []
    );

    return this.performLoadData.load$(
      this.coreService.getCustomerInvoices(model).pipe(
        tap(data => {
          const customerInvoices: ICustomerInvoiceGridDTO[] = [];
          data.map(x => {
            if (x.seqNr && x.actorCustomerName) {
              x.actorCustomerName =
                x.seqNr + ' - ' + x.invoiceNr + ' - ' + x.actorCustomerName;
              customerInvoices.push(x);
            }
          });
          this.customerInvoices = customerInvoices;
        })
      )
    );
  }

  loadInventoriesDict(): Observable<SmallGenericType[]> {
    return this.service.getInventoriesDict().pipe(
      tap(inventories => {
        this.inventories = inventories;
      })
    );
  }

  loadInventoryStatus(): Observable<SmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(TermGroup.InventoryStatus, false, false)
      .pipe(
        tap(data => {
          this.inventoryStatuses = data;
        })
      );
  }

  categoriesChanged(categories: CategoryItem[]) {
    this.form?.patchCategories(categories.map(c => c.categoryId));
  }

  showPreliminaryChanged(value: boolean): void {
    if (value)
      this.form?.patchValue({
        inventoryStatus: TermGroup_InventoryStatus.Draft,
      });
    else
      this.form?.patchValue({
        inventoryStatus: TermGroup_InventoryStatus.Active,
      });
  }

  openCustomerInvoice(): void {
    const invoiceId = this.form?.customerInvoiceId.value;
    const invoiceNr = this.form?.inventoryNr.value;
    BrowserUtil.openInNewTab(
      window,
      `/soe/economy/customer/invoice/status/default.aspx?classificationgroup=${SoeOriginStatusClassificationGroup.HandleCustomerInvoices}&invoiceId=${invoiceId}&invoiceNr=${invoiceNr}`
    );
  }

  openSupplierInvoice(): void {
    const supplierInvoiceId = this.form?.supplierInvoiceId.value;
    BrowserUtil.openInNewTab(
      window,
      `/soe/economy/supplier/invoice/status/default.aspx?classificationgroup=${SoeOriginStatusClassificationGroup.HandleSupplierInvoices}&invoiceId=${supplierInvoiceId}&invoiceNr=${0}`
    );
  }

  updateFormControls(inventoryStatus: TermGroup_InventoryStatus) {
    if (this.form?.isNew == true) {
      this.setAsDraft();
      return;
    }
    switch (inventoryStatus) {
      case TermGroup_InventoryStatus.Draft:
        this.setAsDraft();
        break;
      case TermGroup_InventoryStatus.Active:
        this.setAsActive();
        break;
      case TermGroup_InventoryStatus.WrittenOff:
        this.setAsWrittenOff();
        break;
      case TermGroup_InventoryStatus.Sold:
      case TermGroup_InventoryStatus.Discarded:
        this.setAsDisposed();
        break;
    }
  }

  private enableControls() {
    this.form?.inventoryNr.enable();
    this.form?.purchaseDate.enable();
    this.form?.writeOffDate.enable();
    this.form?.writeOffPeriods.enable();
    this.form?.purchaseAmount.enable();
    this.form?.writeOffSum.enable();
    this.form?.endAmount.enable();
    this.enableDepreciationControls();
  }

  private disableInventoriesControls() {
    this.form?.inventoryNr.disable();
    this.form?.purchaseDate.disable();
    this.form?.writeOffDate.disable();
    this.form?.writeOffPeriods.disable();
    this.form?.purchaseAmount.disable();
    this.form?.writeOffSum.disable();
    this.form?.endAmount.disable();
  }

  private disableDepreciationControls() {
    this.form?.voucherSeriesTypeId.disable();
    this.form?.writeOffTemplateId.disable();
    this.form?.inventoryWriteOffMethodId.disable();
    this.form?.showPreliminary.disable();
  }

  private enableDepreciationControls() {
    this.form?.voucherSeriesTypeId.enable();
    this.form?.writeOffTemplateId.enable();
    this.form?.inventoryWriteOffMethodId.enable();
    this.form?.showPreliminary.enable();
  }

  private setAsDraft() {
    this.isDraft.set(true);
    this.enableControls();
  }

  private setAsActive() {
    this.isDraft.set(false);
    this.disableInventoriesControls();

    if (this.isWriteOffsStarted()) this.disableDepreciationControls();
    else this.enableDepreciationControls();
  }

  private setAsWriteOffsStarted() {
    this.isWriteOffsStarted.set(true);
    this.disableDepreciationControls();
    this.form?.showPreliminary.disable();
  }

  private setAsWrittenOff() {
    this.isWrittenOff.set(true);
    this.disableInventoriesControls();
    this.disableDepreciationControls();
    this.form?.showPreliminary.disable();
  }

  private setAsDisposed() {
    this.isDisposed.set(true);
    this.disableInventoriesControls();
    this.disableDepreciationControls();
    this.form?.showPreliminary.disable();
  }

  ngOnDestroy(): void {
    this._destroy$.next();
    this._destroy$.complete();
  }
}
