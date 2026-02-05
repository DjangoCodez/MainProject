import { DatePipe } from '@angular/common';
import {
  Component,
  OnDestroy,
  OnInit,
  ViewChild,
  inject,
  signal,
} from '@angular/core';
import { CommonCustomerService } from '@features/billing/shared/services/common-customer.service';
import { SupplierHelper } from '@features/billing/shared/services/supplier-helper.service';
import { StockBalanceService } from '@features/billing/stock-balance/services/stock-balance.service';
import { StockDTO } from '@features/billing/stock-warehouse/models/stock-warehouse.model';
import { SuppliersEditComponent } from '@features/economy/suppliers/components/suppliers-edit/suppliers-edit.component';
import { SupplierHeadForm } from '@features/economy/suppliers/models/supplier-head-form.model';
import { TranslateService } from '@ngx-translate/core';
import {
  EditDeliveryAddressComponent,
  IEditDeliveryAddressDialogData,
} from '@shared/components/billing/edit-delivery-address/edit-delivery-address.component';
import { SelectUsersDialogComponent } from '@shared/components/billing/select-users-dialog/components/select-users-dialog/select-users-dialog.component';
import {
  SelectUsersDialogData,
  UserSmallDTO,
} from '@shared/components/billing/select-users-dialog/models/select-users-dialog.model';
import { SelectCustomerInvoiceDialogComponent } from '@shared/components/select-customer-invoice-dialog/component/select-customer-invoice-dialog/select-customer-invoice-dialog.component';
import {
  SearchCustomerInvoiceDTO,
  SelectInvoiceDialogDTO,
} from '@shared/components/select-customer-invoice-dialog/model/customer-invoice-search.model';
import { SelectEmailDialogComponent } from '@shared/components/select-email-dialog/components/select-email-dialog/select-email-dialog.component';
import {
  SelectEmailDialogCloseData,
  SelectEmailDialogData,
} from '@shared/components/select-email-dialog/models/select-email-dialog.model';
import { SelectProjectDialogComponent } from '@shared/components/select-project-dialog/components/select-project-dialog/select-project-dialog.component';
import { SelectProjectDialogData } from '@shared/components/select-project-dialog/models/select-project-dialog.model';
import { SelectReportDialogComponent } from '@shared/components/select-report-dialog/components/select-report-dialog/select-report-dialog.component';
import {
  GetPurchasePrintUrlModel,
  SelectReportDialogCloseData,
  SelectReportDialogData,
} from '@shared/components/select-report-dialog/models/select-report-dialog.model';
import { SelectSupplierModalComponent } from '@shared/components/select-supplier-modal/select-supplier-modal.component';
import { TraceRowPageName } from '@shared/components/trace-rows/models/trace-rows.model';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { IContactAddressDTO } from '@shared/models/generated-interfaces/ContactDTO';
import {
  CompanySettingType,
  EmailTemplateType,
  Feature,
  PurchaseCustomerInvoiceViewType,
  PurchaseRowType,
  SoeEntityState,
  SoeOriginStatus,
  SoeOriginType,
  SoeReportTemplateType,
  TermGroup,
  TermGroup_InvoiceVatType,
  TermGroup_SysContactAddressRowType,
  TermGroup_SysContactAddressType,
  UserSettingType,
} from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { IProjectSearchResultDTO } from '@shared/models/generated-interfaces/ProjectDTOs';
import { IPurchaseRowDTO } from '@shared/models/generated-interfaces/PurchaseDTOs';
import {
  IPaymentConditionDTO,
  IStockProductDTO,
  IUserSmallDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { CurrencyService } from '@shared/services/currency.service';
import { ProgressOptions } from '@shared/services/progress';
import { ReportService } from '@shared/services/report.service';
import { BrowserUtil } from '@shared/util/browser-util';
import { DateUtil } from '@shared/util/date-util';
import { SettingsUtil } from '@shared/util/settings-util';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { StringUtil } from '@shared/util/string-util';
import { SupplierService } from '@src/app/features/economy/services/supplier.service';
import { DialogData } from '@ui/dialog/models/dialog';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox';
import { MenuButtonItem } from '@ui/button/menu-button/menu-button.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToasterService } from '@ui/toaster/services/toaster.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { cloneDeep } from 'lodash';
import { BehaviorSubject, Observable, Subscription, take, tap } from 'rxjs';
import { DeliveryConditionDTO } from '../../../delivery-condition/models/delivery-condition.model';
import { DeliveryConditionService } from '../../../delivery-condition/services/delivery-condition.service';
import { DeliveryTypesService } from '../../../delivery-types/services/delivery-types.service';
import { PurchaseDeliveryRowDTO } from '../../../purchase-delivery/models/purchase-delivery.model';
import { PurchaseDeliveryService } from '../../../purchase-delivery/services/purchase-delivery.service';
import { BillingService } from '../../../services/services/billing.service';
import { PurchaseForm } from '../../models/purchase-form.model';
import { PurchaseRowDTO } from '../../models/purchase-rows.model';
import {
  OriginUserSmallDTO,
  PurchaseDTO,
  PurchaseDateDialogData,
  PurchaseDeliveryAddressesDialogData,
  PurchaseEditPrintFunctions,
  PurchaseEditSaveFunctions,
  ReturnSetPurchaseDateDialog,
  SavePurchaseModel,
  SavePurchaseStatus,
  SendPurchaseEmail,
  StatusFunctionDTO,
} from '../../models/purchase.model';
import { PurchaseService } from '../../services/purchase.service';
import { PurchaseDeliveryAddressesDialogComponent } from '../purchase-delivery-addresses-dialog/purchase-delivery-addresses-dialog.component';
import { PurchaseRowsComponent } from '../purchase-rows/purchase-rows.component';
import { PurchaseSetPurchaseDateDialogComponent } from '../purchase-set-purchase-date-dialog/purchase-set-purchase-date-dialog.component';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import { ResponseUtil } from '@shared/util/response-util';

@Component({
  selector: 'soe-purchase-edit',
  templateUrl: './purchase-edit.component.html',
  styleUrls: ['./purchase-edit.component.scss'],
  providers: [FlowHandlerService, ToolbarService, CurrencyService],
  standalone: false,
})
export class PurchaseEditComponent
  extends EditBaseDirective<PurchaseDTO, PurchaseService, PurchaseForm>
  implements OnInit, OnDestroy
{
  @ViewChild(PurchaseRowsComponent) purchaseRowsComponent: any;

  currencyService = inject(CurrencyService);

  service = inject(PurchaseService);
  coreService = inject(CoreService);
  deliveryTypesService = inject(DeliveryTypesService);
  deliveryConditionService = inject(DeliveryConditionService);
  dialogServiceV2 = inject(DialogService);
  translationService = inject(TranslateService);
  supplierService = inject(SupplierService);
  billingService = inject(BillingService);
  commonCustomerService = inject(CommonCustomerService);
  stockBalanceService = inject(StockBalanceService);
  messageBoxService = inject(MessageboxService);
  supplierHelper = inject(SupplierHelper);
  purchaseDeliveryService = inject(PurchaseDeliveryService);
  reportService = inject(ReportService);
  toasterService = inject(ToasterService);

  traceRowsRendered = signal(false);
  customerInvoiceRowsRendered = signal(false);
  deliveryRowsRendered = signal(false);

  disabledParticipantButton = signal(true);
  disabledReferenceYourButton = signal(true);
  disabledSetPurchaseDateButton = signal(true);
  disabledGetDeliveryAddressesButton = signal(true);
  disabledEditDeliveryAddressButton = signal(true);
  visibleGetDeliveryAddressButton = signal(false);
  disableAllowedStatusFunctionsButton = signal(true);
  disabledEditEmailButton = signal(true);
  disableShowProjectCentralLinkButton = signal(false);
  disablePurchaseRows = signal(true);
  visibleAllowedStatusFunctionsButton = signal(false);
  disablePrintMenu = signal(false);
  isOriginState = signal(false);

  //Permissions
  useCurrency = signal(false);
  tracingPermission = signal(false);
  deliveryPermission = signal(false);
  orderRowsPermission = signal(false);
  readOnlyPermission = signal(false);
  modifyPermission = signal(false);

  purchaseRowsRendered = signal(false);

  //settings
  defaultVatType: TermGroup_InvoiceVatType =
    TermGroup_InvoiceVatType.Merchandise;
  defaultDeliveryTypeId!: number;
  defaultDeliveryConditionId!: number;
  defaultReportId!: number;
  defaultEmailTemplatePurchase!: number;
  billingDefaultStockPlace!: number;
  billingProductDefaultStock!: number;

  //originUsers: IOriginUserSmallDTO[] = [];
  selectedUsers: IUserSmallDTO[] = [];
  paymentConditions: IPaymentConditionDTO[] = [];
  //currencies: ICompCurrencySmallDTO[] = [];
  deliveryAddresses: IContactAddressDTO[] = [];
  statusTypes: ISmallGenericType[] = [];
  ourReferencesId: ISmallGenericType[] = [];
  deliveryTypes: ISmallGenericType[] = [];
  stocks: StockDTO[] = [];
  deliveryConditions: DeliveryConditionDTO[] = [];

  menuSaveList: MenuButtonItem[] = [];
  menuPrintList: MenuButtonItem[] = [];

  getPurchasePrintUrlSubscription!: Subscription;
  sendPurchaseAsEmailSubscription!: Subscription;
  selectEmailDialogSubscription!: Subscription;

  headExpanderLabel = signal('');
  purchaseRows = new BehaviorSubject<PurchaseRowDTO[]>([]);
  deletedPurchaseRows: PurchaseRowDTO[] = [];
  purchaseDeliveryRows = new BehaviorSubject<PurchaseDeliveryRowDTO[]>([]);
  originalPurchase: PurchaseDTO = new PurchaseDTO();
  originalPurchaseRows: IPurchaseRowDTO[] = [];
  statusFunctions: StatusFunctionDTO[] = [];

  totalAmountExVatCurrency = signal(0);
  useCentRounding: boolean | undefined;
  originStatus = 0;
  purchaseRowsLength = 0;
  invoiceData: SearchCustomerInvoiceDTO | undefined;
  accordionLabel = signal('');
  selectedProjectNumber = '';
  messageboxService = inject(MessageboxService);
  secondaryAccordionTitlePurchaseRows = signal('');
  terms: any = [];

  currencyRateSecondaryLabel = signal('');
  showCurrencyRateSecondaryLabel = signal(false);
  pageName = TraceRowPageName.Purchase;
  currentStatusOption: any;
  allowedStatusFunctions: any = [];
  oldStatus: any;

  get viewType(): PurchaseCustomerInvoiceViewType {
    if (this.form?.value.purchaseId && this.form.value.purchaseId > 0)
      return PurchaseCustomerInvoiceViewType.FromPurchase;
    else return PurchaseCustomerInvoiceViewType.Unknown;
  }

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(Feature.Billing_Purchase_Purchase_Edit, {
      additionalModifyPermissions: [
        Feature.Billing_Purchase_Purchase_Edit,
        Feature.Economy_Preferences_Currency,
        Feature.Billing_Purchase_Purchase_Edit_TraceRows,
        Feature.Billing_Purchase_Delivery_List,
        Feature.Billing_Order_Orders_Edit_ProductRows,
      ],
      additionalReadPermissions: [
        Feature.Billing_Purchase_Purchase_Edit,
        Feature.Billing_Invoice_Invoices_Edit,
      ],
      lookups: [
        this.loadOurReferences(true),
        this.supplierHelper.loadSuppliers(true),
        this.loadDeliveryTypes(true),
        this.loadDeliveryConditions(true),
        this.loadDeliveryAddresses(SoeConfigUtil.actorCompanyId, true),
        this.supplierHelper.loadPaymentConditions(true),
        this.loadStatusTypes(),
        this.loadStockLocation(),
      ],
    });

    this.purchaseRows.asObservable().subscribe(p => {
      this.purchaseRowsLength = p.length;
      this.disableAllowedStatusFunctionsButton.set(
        this.purchaseRows.value.length <= 0
      );
    });
  }

  override onFinished(): void {
    this.setupFunctions();
    this.disableControls();
    this.setChangeHandlers();
    this.setLabels();
    this.setAllowedStatusFunctions();
    this.onCopy();
    this.onNew();
  }

  private doCopy() {
    let i = 0;
    const newPurchaseRows: PurchaseRowDTO[] = cloneDeep(
      this.purchaseRows.getValue()
    );
    newPurchaseRows.forEach(r => {
      i++;
      r.purchaseRowId = 0;
      r.tempRowId = i;
    });
    this.form?.customPurchaseRowsPatchValue(newPurchaseRows);
    super.copy();
  }

  onCopy() {
    if (this.form?.isCopy) {
      this.currencyService.setCurrencyId(this.form?.value.currencyId);
      this.originalPurchase = SoeConfigUtil.cloneDTO(this.form?.value);
      this.form?.patchValue({
        originStatus: SoeOriginStatus.Origin,
        purchaseNr: '',
        statusName: this.translate.instant('core.new'),
      });
      this.setStatusName(true);
      this.totalAmountExVatCurrency.set(
        this.form?.value.totalAmountExVatCurrency
      );
      if (this.form?.value?.purchaseRows) {
        this.purchaseRows.next(this.form.value.purchaseRows);
      }
      this.changeSupplier(true);
      //this.purchaseAmountHelper.fromPurchase(this.form?.value);
      this.originStatus = this.form?.value.originStatus;
      this.setLabels();
      this.setControlVisibility();
      this.originalPurchaseRows = SoeConfigUtil.cloneDTOs(
        this.form.value.purchaseRows
      );
      this.setHeadExpanderLabel();
      this.disableAllowedStatusFunctionsButton.set(true);
      this.purchaseRowsLength = this.form.value.purchaseRows.length;
      this.loadPurchaseRowsLabel();
    }
  }

  setAllowedStatusFunctions() {
    if (this.oldStatus === this.form?.value.originStatus) {
      return this.allowedStatusFunctions;
    } else {
      this.oldStatus = this.form?.value.originStatus;
    }

    let current = 0;
    if (this.form?.value.originStatus) {
      current = Number(this.form?.value.originStatus);
    }
    const currentFunc = this.statusFunctions.find(s => s.id === current);
    const idx = this.statusFunctions.findIndex(t => t.id === current);
    this.allowedStatusFunctions = [];

    this.allowedStatusFunctions.push(currentFunc);
    this.currentStatusOption = currentFunc;
    for (let i = 0; i < this.statusFunctions.length; i++) {
      if (i === idx - 1 || i === idx + 1)
        this.allowedStatusFunctions.push(this.statusFunctions[i]);
    }
    return this.allowedStatusFunctions;
  }

  override onPermissionsLoaded() {
    super.onPermissionsLoaded();
    this.readOnlyPermission.set(
      this.flowHandler.hasReadAccess(Feature.Billing_Purchase_Purchase_Edit)
    );
    this.modifyPermission.set(
      this.flowHandler.hasModifyAccess(Feature.Billing_Purchase_Purchase_Edit)
    );
    this.useCurrency.set(
      this.flowHandler.hasModifyAccess(Feature.Economy_Preferences_Currency)
    );
    this.tracingPermission.set(
      this.flowHandler.hasModifyAccess(
        Feature.Billing_Purchase_Purchase_Edit_TraceRows
      )
    );
    this.deliveryPermission.set(
      this.flowHandler.hasModifyAccess(Feature.Billing_Purchase_Delivery_List)
    );
    this.orderRowsPermission.set(
      this.flowHandler.hasModifyAccess(
        Feature.Billing_Order_Orders_Edit_ProductRows
      )
    );
  }

  override createEditToolbar(): void {
    super.createEditToolbar();

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButtonReload({
          disabled: signal(this.form?.isNew ?? true),
          onAction: () => this.reloadPurchaseOrder(),
        }),
      ],
    });
  }

  reloadPurchaseOrder() {
    this.loadData().subscribe(() => {
      if (this.deliveryRowsRendered()) {
        this.loadDeliveryRows().subscribe();
      }
    });
  }

  setChangeHandlers() {
    this.form?.currencyId.valueChanges.subscribe((value: number) => {
      this.currencyService.setCurrencyId(parseInt(value.toString()));
      this.currencyChanged();
    });

    this.form?.referenceOurId.valueChanges.subscribe((value: number) => {
      if (value) {
        const ourRef = this.ourReferencesId.find(f => f.id == value);
        if (ourRef) {
          this.form?.patchValue({ referenceOur: ourRef.name });
        }
      }
    });

    this.form?.referenceYour.valueChanges.subscribe(value => {
      this.disabledReferenceYourButton.set(value ? false : true);
    });

    this.form?.wantedDeliveryDate.valueChanges.subscribe(value => {
      if (this.purchaseRowsRendered()) {
        const purchaseRows = this.purchaseRows.getValue();
        purchaseRows.forEach(r => {
          r.wantedDeliveryDate = this.form?.value.wantedDeliveryDate;
          r.isModified = true;
        });
        this.purchaseRows.next(purchaseRows);
        this.purchaseRowsComponent.refreshRows();
        ////this.$scope.$broadcast('refreshRows');
      } else {
        this.openPurchaseRowExpander();
      }

      this.form?.markAsDirty();
    });
  }

  disableControls() {
    this.form?.purchaseNr.disable();
    this.form?.statusName.disable();
    this.form?.projectNr.disable();
    this.form?.orderNr.disable();
    this.form?.supplierCustomerNr.disable();
    this.form?.confirmedDeliveryDate.disable();
    this.form?.currencyRate.disable();
    this.form?.stockId.disable();

    let status: SoeOriginStatus = SoeOriginStatus.Origin;
    if (this.form?.originStatus) {
      status = this.form?.originStatus.value;
    }
    if (
      !(
        !this.form?.value.purchaseId ||
        this.form?.dirty ||
        status == SoeOriginStatus.Origin
      )
    ) {
      this.disablePrintMenu.set(false);
    }

    this.isOriginState.set(status == SoeOriginStatus.Origin);

    this.disableFieldsBasedOnStatus(status);
  }

  private disableFieldsBasedOnStatus(status: SoeOriginStatus) {
    const allowEditOrigin = [SoeOriginStatus.Origin];
    const allowEditDone = [
      SoeOriginStatus.Origin,
      SoeOriginStatus.PurchaseDone,
    ];
    const allowEditSent = [
      SoeOriginStatus.Origin,
      SoeOriginStatus.PurchaseDone,
      SoeOriginStatus.PurchaseSent,
    ];
    const allowEditAccepted = [
      SoeOriginStatus.Origin,
      SoeOriginStatus.PurchaseDone,
      SoeOriginStatus.PurchaseSent,
      SoeOriginStatus.PurchaseAccepted,
    ];
    const allowEditPartlyDelivered = [
      SoeOriginStatus.Origin,
      SoeOriginStatus.PurchaseDone,
      SoeOriginStatus.PurchaseSent,
      SoeOriginStatus.PurchaseAccepted,
      SoeOriginStatus.PurchasePartlyDelivered,
    ];
    const allowEditPartlyDeliveredNonOrigin = [
      SoeOriginStatus.PurchaseDone,
      SoeOriginStatus.PurchaseSent,
      SoeOriginStatus.PurchaseAccepted,
      SoeOriginStatus.PurchasePartlyDelivered,
    ];

    // Disable fields based on status
    if (!allowEditOrigin.includes(status)) {
      this.form?.supplierId.disable();
      this.form?.currencyId.disable();
    } else {
      this.form?.supplierId.enable();
      this.form?.currencyId.enable();
    }

    if (!allowEditDone.includes(status)) {
      this.form?.purchaseDate.disable();
      this.form?.deliveryAddressId.disable();
      this.form?.stockId.disable();
    } else {
      this.form?.purchaseDate.enable();
      this.form?.deliveryAddressId.enable();
      this.form?.stockId.enable();
    }

    if (!allowEditSent.includes(status)) {
      this.form?.contactEComId.disable();
      this.form?.wantedDeliveryDate.disable();
      this.form?.deliveryTypeId.disable();
      this.form?.deliveryConditionId.disable();
      this.form?.paymentConditionId.disable();
      this.disabledEditEmailButton.set(true);
    } else {
      this.form?.contactEComId.enable();
      this.form?.wantedDeliveryDate.enable();
      this.form?.deliveryTypeId.enable();
      this.form?.deliveryConditionId.enable();
      this.form?.paymentConditionId.enable();
      this.disabledEditEmailButton.set(false);
    }

    if (!allowEditAccepted.includes(status)) {
      this.form?.referenceOur.disable();
      this.form?.referenceYour.disable();
      this.form?.purchaseLabel.disable();
      this.form?.origindescription.disable();
      this.disabledEditDeliveryAddressButton.set(true);
      this.disabledGetDeliveryAddressesButton.set(true);
      this.disableAllowedStatusFunctionsButton.set(true);
      this.visibleAllowedStatusFunctionsButton.set(false);
    } else {
      this.form?.referenceOur.enable();
      this.form?.referenceYour.enable();
      this.form?.purchaseLabel.enable();
      this.form?.origindescription.enable();
      this.disabledEditDeliveryAddressButton.set(false);
      this.disabledGetDeliveryAddressesButton.set(false);
      this.disableAllowedStatusFunctionsButton.set(false);
      this.visibleAllowedStatusFunctionsButton.set(true);
    }

    if (!allowEditPartlyDelivered.includes(status)) {
      this.disabledParticipantButton.set(true);
    } else {
      this.disabledParticipantButton.set(false);
    }

    if (!allowEditPartlyDeliveredNonOrigin.includes(status)) {
      this.disabledSetPurchaseDateButton.set(true);
    } else {
      this.disabledSetPurchaseDateButton.set(false);
    }
  }

  setLabels() {
    this.setCurrencyRateSecondaryLabel();
  }

  setHeadExpanderLabel() {
    setTimeout(() => {
      if (this.form?.isNew && !this.form?.isCopy) {
        this.headExpanderLabel.set(
          this.translate.instant('billing.productrows.functions.newpurchase')
        );
      } else {
        const purchaseNr: string = this.form?.getRawValue().purchaseNr;
        let supplier = ' ';
        if (this.supplierHelper.supplier) {
          supplier = this.supplierHelper.supplier.name;
        }
        let statusName = ' ';
        if (this.form?.getRawValue().statusName) {
          statusName = this.form?.getRawValue()?.statusName;
        }
        let projectNr = ' ';
        if (this.form?.getRawValue().projectNr) {
          projectNr = this.form?.getRawValue()?.projectNr;
        } else {
          projectNr = this.translate.instant('billing.order.noproject');
        }

        const label: string = '{0} {1} | {2}: {3} | {4}: {5} | {6}: {7}'.format(
          this.translate.instant('billing.purchase.list.purchase'),
          purchaseNr,
          this.translate.instant('billing.purchase.supplier'),
          supplier,
          this.translate.instant('billing.order.status'),
          statusName,
          this.translate.instant('billing.project.project'),
          projectNr
        );
        this.headExpanderLabel.set(label);
      }
    }, 1000);
  }

  setCurrencyRateSecondaryLabel() {
    this.currencyRateSecondaryLabel.set('');
    this.showCurrencyRateSecondaryLabel.set(false);
    if (this.form && this.form.getRawValue().currencyDate) {
      const pipe = new DatePipe(SoeConfigUtil.language);
      const currencyRateShortDate = pipe.transform(
        this.form?.getRawValue()?.currencyDate,
        'shortDate'
      );
      if (currencyRateShortDate) {
        this.currencyRateSecondaryLabel.set(currencyRateShortDate);
        this.showCurrencyRateSecondaryLabel.set(true);
      }
    }
  }

  supplierOnChange(value: ISmallGenericType) {
    this.changeSupplier(false);
  }

  changeSupplier(fromLoad = true) {
    const supplierId = this.form?.getRawValue().supplierId;
    this.supplierHelper.loadSupplier(supplierId).subscribe(data => {
      if (this.supplierHelper.supplier) {
        if (supplierId !== this.supplierHelper.supplier.actorSupplierId) {
          this.form?.patchValue({
            supplierId: this.supplierHelper.supplier.actorSupplierId,
          });
          this.form?.markAsDirty();
          this.form?.updateValueAndValidity();
          const mb = this.messageboxService.warning(
            this.translate.instant('core.warning'),
            this.translate.instant('billing.purchase.changesupplier')
          );
          mb.afterClosed().subscribe(
            (response: IMessageboxComponentResponse) => {
              if (response.result) {
                this.supplierChanged(fromLoad);
              }
            }
          );
        }
        this.supplierChanged(fromLoad);
      }

      this.setHeadExpanderLabel();
    });
  }

  supplierChanged(fromLoad: boolean) {
    if (this.supplierHelper.supplier) {
      this.disablePurchaseRows.set(false);
      if (fromLoad) {
        //manual email
        if (!this.form?.contactEComId.value) {
          this.form?.patchValue({ contactEComId: 0 });
          if (this.form?.supplierEmail.value) {
            this.supplierHelper.supplierEmails[0].name =
              this.form?.supplierEmail.value;
          }
        }
      } else {
        this.form?.patchValue({
          paymentConditionId: this.supplierHelper.supplier.paymentConditionId,
          deliveryConditionId: this.supplierHelper.supplier.deliveryConditionId,
          deliveryTypeId: this.supplierHelper.supplier.deliveryTypeId,
          contactEComId: this.supplierHelper.supplier.contactEcomId,
          supplierCustomerNr: this.supplierHelper.supplier.ourCustomerNr,
        });
        if (this.supplierHelper.supplier.vatType) {
          this.form?.patchValue({
            vatType: this.supplierHelper.supplier.vatType,
          });
        }

        if (this.supplierHelper.supplier.currencyId) {
          this.form?.patchValue({
            currencyId: this.supplierHelper.supplier.currencyId,
          });
        }
        this.updatePurchaseRowsWithBaseData();
      }

      //manual email
      if (!this.form?.getRawValue().contactEComId) {
        this.form?.patchValue({
          contactEComId: 0,
        });
        if (this.form?.getRawValue().supplierEmail) {
          this.supplierHelper.supplierEmails[0].name =
            this.form?.getRawValue().supplierEmail;
        }
      }
    }
  }
  openSupplier() {
    this.openEditInNewTab({
      id: this.form?.getRawValue().supplierId,
      additionalProps: {
        editComponent: SuppliersEditComponent,
        FormClass: SupplierHeadForm,
        editTabLabel: 'economy.supplier.supplier.supplier',
      },
    });
  }

  searchSupplier() {
    const dialogOpts = <Partial<DialogData>>{
      size: 'lg',
      title: this.terms['common.dialogs.searchsupplier'],
    };
    this.dialogServiceV2
      .open(SelectSupplierModalComponent, dialogOpts)
      .afterClosed()
      .pipe(
        tap(x => {
          if (x) {
            this.form?.patchValue({ supplierId: x.actorSupplierId });
          }
        })
      )
      .subscribe();
  }

  openSelectProject() {
    const dialogData = new SelectProjectDialogData();
    dialogData.title = 'billing.projects.list.searchprojects';
    dialogData.size = 'xl';
    dialogData.customerId = this.form?.getRawValue()?.supplierId;
    dialogData.projectsWithoutCustomer = false;
    dialogData.showFindHidden = false;
    dialogData.loadHidden = false;
    dialogData.useDelete = false;
    dialogData.currentProjectNr = undefined;
    dialogData.currentProjectId = undefined;
    dialogData.showAllProjects = true;
    const projectSelectDialog = this.dialogServiceV2.open(
      SelectProjectDialogComponent,
      dialogData
    );

    projectSelectDialog
      .afterClosed()
      .subscribe((project: IProjectSearchResultDTO) => {
        if (project) {
          this.form?.patchValue({
            projectNr: project.number,
            projectId: project.projectId,
          });
          this.form?.markAsDirty();
        }
      });
  }

  clickProjectCentralLink() {
    const url =
      '/soe/billing/project/central/?project=' +
      this.form?.getRawValue()?.projectId;
    BrowserUtil.openInNewTab(window, url);
  }

  openCustomerInvoice() {
    const dialogData = new SelectInvoiceDialogDTO();
    dialogData.title = this.translate.instant('core.search');
    dialogData.size = 'lg';
    dialogData.originType = SoeOriginType.Order;
    if (!this.invoiceData) {
      this.invoiceData = new SearchCustomerInvoiceDTO();
    }
    const formData = this.form?.getAllValues({ includeDisabled: true });
    this.invoiceData.projectNr = formData.projectNr;
    this.invoiceData.projectName = formData.projectName;
    this.invoiceData.number = formData.orderNr;
    if (formData.projectId && formData.projectId > 0) {
      this.invoiceData.projectId = formData.projectId;
    } else {
      this.invoiceData.projectId = undefined;
    }
    this.invoiceData.isNew = this.form?.isNew;
    if (this.invoiceData) dialogData.invoiceValue = this.invoiceData;

    this.dialogServiceV2
      .open(SelectCustomerInvoiceDialogComponent, dialogData)
      .afterClosed()
      .subscribe(result => {
        if (
          result &&
          (result.deliveryAddressId > 0 ||
            !StringUtil.isEmpty(result.invoiceHeadText))
        ) {
          const mb = this.messageboxService.question(
            this.translate.instant('core.verifyquestion'),
            this.translate.instant('billing.purchase.takeorderaddress')
          );
          mb.afterClosed().subscribe(
            (response: IMessageboxComponentResponse) => {
              if (response.result) {
                //get address
                if (result.deliveryAddressId > 0) {
                  this.commonCustomerService
                    .getContactAddresses(
                      result.customerId,
                      TermGroup_SysContactAddressType.Delivery,
                      false,
                      true,
                      false
                    )
                    .pipe(
                      tap(data => {
                        const deliveryAddress = data.find(
                          f => f.contactAddressId === result.deliveryAddressId
                        );
                        if (deliveryAddress) {
                          this.deliveryAddresses[0].address =
                            this.formatDeliveryAddress(
                              deliveryAddress.contactAddressRows,
                              false
                            );

                          this.form?.patchValue({
                            deliveryAddressId: 0,
                            deliveryAddress: this.deliveryAddresses[0].address,
                          });
                        }
                      })
                    )
                    .subscribe();
                } else if (!StringUtil.isEmpty(result.invoiceHeadText)) {
                  this.deliveryAddresses[0].address = result.invoiceHeadText;
                  this.form?.patchValue({
                    deliveryAddressId: 0,
                    deliveryAddress: this.deliveryAddresses[0].address,
                  });
                }
              }
            }
          );
        }
        if (result) {
          this.form?.patchValue({
            orderId: result ? result.customerInvoiceId : 0,
            orderNr: result ? result.number : '',
            projectNr: result ? result.projectNr : '',
            projectId: result ? result.projectId : 0,
          });
          this.form?.markAsDirty();
          this.form?.markAsTouched();
          this.invoiceData = result;
        }
      });
  }

  formatDeliveryAddress(
    addressRows: any[],
    isFinInvoiceCustomer: boolean
  ): string {
    let strAddress = '';
    let tmpName = '';
    let tmpStreetAddress = '';
    let tmpPostalCode = '';
    let tmpPostalAddress = '';
    let tmpCountry = '';

    addressRows.forEach(row => {
      switch (row.sysContactAddressRowTypeId) {
        case TermGroup_SysContactAddressRowType.Name:
          tmpName += row.text;
          break;
        case TermGroup_SysContactAddressRowType.StreetAddress:
          tmpStreetAddress += row.text;
          break;
        case TermGroup_SysContactAddressRowType.Address:
          tmpStreetAddress += row.text;
          break;
        case TermGroup_SysContactAddressRowType.PostalCode:
          tmpPostalCode += row.text;
          break;
        case TermGroup_SysContactAddressRowType.PostalAddress:
          tmpPostalAddress += row.text;
          break;
        case TermGroup_SysContactAddressRowType.Country:
          tmpCountry += row.text;
          break;
      }
    });

    strAddress = tmpName;

    if (strAddress == '' || strAddress == ' ') strAddress = tmpStreetAddress;
    else strAddress += '\r' + tmpStreetAddress;

    strAddress += '\r' + tmpPostalCode;

    if (isFinInvoiceCustomer)
      //4 lines needed for finvoice
      strAddress += '\r' + tmpPostalAddress;
    else strAddress += ' ' + tmpPostalAddress;

    if (tmpCountry !== '') {
      strAddress += '\r' + tmpCountry;
    }

    return strAddress;
  }

  openSelectUser() {
    const dialogData = new SelectUsersDialogData();
    dialogData.title = 'common.customer.customer.selectusers';
    dialogData.size = 'lg';
    dialogData.showMain = true;
    dialogData.showParticipant = true;
    this.selectedUsers = [];

    this.form?.value.originUsers.forEach((user: any) => {
      const u = new UserSmallDTO();
      u.name = user.name;
      u.main = user.main;
      u.userId = user.userId;
      this.selectedUsers.push(u);
    });
    dialogData.selectedUsers = this.selectedUsers;

    const userSelectDialog = this.dialogServiceV2.open(
      SelectUsersDialogComponent,
      dialogData
    );

    userSelectDialog.afterClosed().subscribe((res: IUserSmallDTO[]) => {
      const originUsers: OriginUserSmallDTO[] = [];
      if (res) {
        res.forEach(user => {
          if (user) {
            const o = new OriginUserSmallDTO();
            o.userId = user.userId;
            o.main = user.main;
            o.name = user.name;
            originUsers.push(o);
          }
        });
        this.form?.customOriginUsersPatchValue(originUsers);
        this.form?.markAsDirty();
      }
    });
  }

  showYourReferenceInfo() {
    //function not implemented in purchase page
  }

  editEmail() {
    const emailDialog = this.messageboxService.show(
      this.translate.instant('common.contactaddresses.ecommenu.email'),
      ' ',
      {
        showInputText: true,
        inputTextLabel: 'common.contactaddresses.ecommenu.email',
        inputTextValue: this.form?.getRawValue()?.supplierEmail,
        buttons: 'okCancel',
      }
    );

    emailDialog.afterClosed().subscribe((email: any) => {
      if (email.result && email.textValue) {
        if (email.textValue !== this.form?.getRawValue()?.supplierEmail) {
          this.form?.patchValue({
            contactEComId: 0,
            supplierEmail: email.textValue,
          });
          this.supplierHelper.supplierEmails[0].name = email.textValue;
          this.form?.markAsDirty();
        }
      }
    });
  }

  openSetPurchaseDate(id: SoeOriginStatus) {
    if (id !== this.form?.getRawValue()?.originStatus) {
      this.saveChangedStatus(id, false);
    }
    if (id === SoeOriginStatus.PurchaseAccepted) {
      const cb = this.setPurchaseDateCb(
        id,
        this.form?.getRawValue()?.originStatus
      );
      if (this.purchaseRowsRendered()) {
        cb();
      } else {
        this.openPurchaseRowExpander(cb);
      }
    } else {
      this.form?.patchValue({
        originStatus: id,
      });
      this.setStatusName(true);
    }
  }

  openPurchaseRowExpander(cb?: () => void, openConfirmedDialog: any = null) {
    if (!this.purchaseRowsRendered()) {
      this.purchaseRowsRendered.set(true);
      this.loadPurchaseRows(openConfirmedDialog);
      if (cb) cb();

      if (openConfirmedDialog) this.openConfirmDialog();
    }
  }

  updatePurchaseRowsWithBaseData() {
    this.purchaseRowsComponent?.updateFromPurchase({
      supplierId: this.form?.value.supplierId,
      purchaseDate: this.form?.value.purchaseDate,
    });
    ////this.$scope.$broadcast('updateFromPurchase', {
    // //   supplierId: this.form?.value.supplierId,
    // //   purchaseDate: this.form?.value.purchaseDate,
    // // });
  }

  setPurchaseDateCb(newId: any, oldId: any) {
    return () => {
      this.openSetPurchaseDateModal(newId, oldId);
    };
  }

  openSetPurchaseDateModal(
    id: SoeOriginStatus,
    oldId: SoeOriginStatus,
    setConfirmed = false
  ) {
    const dialogData = new PurchaseDateDialogData();
    dialogData.title = this.translate.instant(
      'billing.purchaserows.accdeliverydate'
    );
    dialogData.size = 'lg';
    dialogData.purchaseRows = this.purchaseRows;
    dialogData.newStatus = this.form?.getRawValue()?.originStatus;
    dialogData.confirmedDeliveryDate =
      this.form?.getRawValue()?.confirmedDeliveryDate;
    dialogData.purchaseDate = this.form?.getRawValue()?.purchaseDate;
    dialogData.useConfirmed = setConfirmed;
    this.dialogServiceV2
      .open(PurchaseSetPurchaseDateDialogComponent, dialogData)
      .afterClosed()
      .subscribe((result: ReturnSetPurchaseDateDialog) => {
        if (result) {
          if (
            (setConfirmed || id === SoeOriginStatus.PurchaseAccepted) &&
            result.selectedDate
          ) {
            this.form?.patchValue({
              confirmedDeliveryDate: result.selectedDate,
            });
          }
          this.form?.patchValue({
            originStatus: id,
          });
          this.purchaseRows.next(result.purchaseRowsChanges);
        } else {
          this.form?.patchValue({
            originStatus: oldId,
          });
        }
        this.setStatusName(true);
      });
  }

  saveChangedStatus(id: SoeOriginStatus, showDialog: boolean = true) {
    const model = new SavePurchaseStatus(this.form?.value.purchaseId, id);
    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service.savePurchaseStatus(model),
      (successResponse: BackendResponse) =>
        this.afterSaveChangedStatusSuccess(successResponse, showDialog),
      (errorResponse: BackendResponse) =>
        this.afterSaveChangedStatusError(errorResponse),
      {
        showDialog: true,
        showToastOnComplete: showDialog,
      }
    );
  }

  afterSaveChangedStatusSuccess(
    response: BackendResponse,
    showDialog: boolean
  ) {
    this.progressService.saveComplete(<ProgressOptions>{
      showDialogOnComplete: showDialog,
      showToastOnComplete: false,
    });

    this.updateFormValueAndEmitChange(response, true);
  }

  afterSaveChangedStatusError(response: BackendResponse) {
    this.progressService.saveComplete(<ProgressOptions>{
      showDialogOnError: true,
      showToastOnError: false,
      message: ResponseUtil.getErrorMessage(response),
    });
  }
  statusChanged(id: SoeOriginStatus) {
    if (id !== this.form?.value.originStatus) {
      //this.dirtyHandler.setDirty();
      this.saveChangedStatus(id);
    }

    if (id === SoeOriginStatus.PurchaseAccepted) {
      const cb = this.setPurchaseDateCb(id, this.form?.value.originStatus);
      if (this.purchaseRowsRendered()) {
        cb();
      } else {
        this.openPurchaseRowExpander(cb);
      }
    } else {
      this.form?.patchValue({ originStatus: id });
      this.setStatusName(true);
    }
  }

  setStatusName(statusChanged: boolean) {
    if (statusChanged) {
      const item =
        this.statusTypes?.find(s => s.id === this.form?.value.originStatus) ||
        this.statusTypes.find(s => s.id === SoeOriginStatus.Origin);
      this.form?.patchValue({
        statusName: item?.name || '',
      });
    }
    this.currentStatusOption = this.statusFunctions.find(
      s => s.id === this.form?.value.originStatus
    );
  }

  private setupStatusFunctions() {
    this.statusFunctions.push(
      new StatusFunctionDTO(
        SoeOriginStatus.Origin,
        this.statusTypes.find(s => s.id === SoeOriginStatus.Origin)?.name ?? '',
        'file-alt'
      )
    );
    this.statusFunctions.push(
      new StatusFunctionDTO(
        SoeOriginStatus.PurchaseDone,
        this.statusTypes.find(s => s.id === SoeOriginStatus.PurchaseDone)
          ?.name ?? '',
        'file-check'
      )
    );
    this.statusFunctions.push(
      new StatusFunctionDTO(
        SoeOriginStatus.PurchaseSent,
        this.statusTypes.find(s => s.id === SoeOriginStatus.PurchaseSent)
          ?.name ?? '',
        'envelope'
      )
    );
    this.statusFunctions.push(
      new StatusFunctionDTO(
        SoeOriginStatus.PurchaseAccepted,
        this.statusTypes.find(s => s.id === SoeOriginStatus.PurchaseAccepted)
          ?.name ?? '',
        'comment-check'
      )
    );
  }

  setStatusValues(row: PurchaseRowDTO) {
    if (
      row.status < 73 &&
      ((row.accDeliveryDate && row.accDeliveryDate < DateUtil.getToday()) ||
        (this.form?.value.confirmedDeliveryDate &&
          this.form?.value.confirmedDeliveryDate < DateUtil.getToday()))
    ) {
      row.statusName = this.translate.instant('billing.purchase.late');
      row.statusIcon = '#da1e28';
    } else {
      switch (row.status) {
        case 70:
          row.statusName = this.translate.instant(
            'common.customer.invoices.ready'
          );
          row.statusIcon = '#b6b6b6';
          break;
        case 71:
          row.statusName = this.translate.instant('common.sent');
          row.statusIcon = '#b6b6b6';
          break;
        case 72:
          row.statusName = this.translate.instant('billing.purchase.confirmed');
          row.statusIcon = '#ff832b';
          break;
        case 73:
          row.statusName = this.translate.instant(
            'billing.purchase.partlydeliverd'
          );
          row.statusIcon = '#0565c9';
          break;
        case 74:
          row.statusName = this.translate.instant(
            'billing.purchase.finaldelivered'
          );
          row.statusIcon = '#24a148';
          break;
        default:
          row.statusName = this.translate.instant('billing.purchase.origin');
          row.statusIcon = '#b6b6b6';
          break;
      }
    }
  }

  editDeliveryAddress() {
    let tmpDeliveryAddress: string = this.form?.deliveryAddress.value;

    if (
      this.form?.deliveryAddressId.value &&
      this.form?.deliveryAddressId.value != 0
    ) {
      tmpDeliveryAddress = this.formatDeliveryAddress(
        this.deliveryAddresses.filter(
          i => i.contactAddressId == this.form?.deliveryAddressId.value
        )[0].contactAddressRows,
        false
      ); //this.customer.isFinvoiceCustomer);
    }
    this.dialogServiceV2
      .open(EditDeliveryAddressComponent, {
        title: 'billing.order.deliveryaddress',
        addressString: tmpDeliveryAddress,
        size: 'sm',
      } as IEditDeliveryAddressDialogData)
      .afterClosed()
      .subscribe(value => {
        if (tmpDeliveryAddress !== value) {
          this.deliveryAddresses[0].address = value;
          this.form?.patchValue({
            deliveryAddressId: 0,
            deliveryAddress: value,
          });
          this.form?.markAsDirty();
        }
      });
  }

  getDeliveryAddresses() {
    this.dialogServiceV2
      .open(
        PurchaseDeliveryAddressesDialogComponent,
        new PurchaseDeliveryAddressesDialogData(
          'billing.purchase.deliveryaddressfromcustomer',
          this.form?.value.orderId
        )
      )
      .afterClosed()
      .subscribe(value => {
        this.form?.patchValue({
          deliveryAddressId: 0,
          deliveryAddress: value,
        });
        this.deliveryAddresses[0].address = value;
        this.form?.markAsDirty();
      });
  }

  toggleTracingOpened(isOpen: boolean) {
    this.traceRowsRendered.set(isOpen);
  }
  toggleDeliveryRowsOpened(isOpen: boolean) {
    this.deliveryRowsRendered.set(isOpen);
  }

  toggleCustomerInvoiceRowsOpened(isOpen: boolean) {
    this.customerInvoiceRowsRendered.set(isOpen);
  }

  private getDefaultUser() {
    const originUsers = [];
    if (SoeConfigUtil.userId > 0) {
      const user = new OriginUserSmallDTO();
      user.name = SoeConfigUtil.loginName;
      user.userId = SoeConfigUtil.userId;
      user.main = true;
      originUsers.push(user);
    }
    return originUsers;
  }

  private onNew() {
    if (this.form?.isNew && !this.form?.isCopy) {
      if (!this.disablePurchaseRows()) this.disablePurchaseRows.set(true);
      this.originalPurchase = new PurchaseDTO();
      this.form?.customOriginUsersPatchValue(this.getDefaultUser());
      this.setStatusName(true);
      this.form?.patchValue({
        vatType: this.defaultVatType,
        originStatus: SoeOriginStatus.Origin,
        deliveryTypeId: this.defaultDeliveryTypeId,
        deliveryConditionId: this.defaultDeliveryConditionId,
        currencyId: this.currencyService.baseCurrency?.currencyId,
        currencyDate: DateUtil.getToday(),
        currencyRate: 1,
        purchaseDate: DateUtil.getToday(),
        vatAmountCurrency: 0,
        totalAmountCurrency: 0,
        statusName: this.translate.instant('core.new'),
      });
      this.supplierHelper.supplier = undefined;
      this.supplierHelper.supplierReferences = [];
      this.supplierHelper.supplierEmails = [];
      this.currencyService.setCurrencyDate(DateUtil.getToday());

      this.setHeadExpanderLabel();
      this.loadPurchaseRowsLabel();
      this.disableAllowedStatusFunctionsButton.set(true);
      this.setInitialStockLocation();
      this.setStockLocation();
    }
  }
  setInitialStockLocation() {
    if (this.billingDefaultStockPlace) {
      this.form?.patchValue({ stockId: this.billingDefaultStockPlace });
    } else if (this.billingProductDefaultStock) {
      this.form?.patchValue({ stockId: this.billingProductDefaultStock });
    }
  }
  setStockLocation(canSetPurchaseRowStock = false, setDeliveryAddress = true) {
    if (this.form?.value.stockId) {
      this.setStockNameAndDeliveryAddress(
        this.form?.value.stockId,
        setDeliveryAddress
      );
      if (canSetPurchaseRowStock) this.setStockLocationOnPurchaseRows();
    }
  }

  setStockLocationOnPurchaseRows() {
    if (this.form?.value.stockId && this.purchaseRows.getValue().length > 0) {
      this.performLoadData.load(
        this.stockBalanceService
          .getStockProductsByStockId(this.form?.value.stockId)
          .pipe(
            tap((data: IStockProductDTO[]) => {
              let notFound = false;
              const rows = this.purchaseRows.getValue();
              rows.forEach(f => {
                if (f.type != PurchaseRowType.TextRow) {
                  if (f.productId) {
                    const product = data.find(
                      ip => ip.invoiceProductId == f.productId
                    );
                    if (!product) {
                      notFound = true;
                    }
                  }
                }
              });

              if (notFound) {
                const mb = this.messageboxService.question(
                  this.translate.instant('core.warning'),
                  this.translate.instant(
                    'billing.purchase.product.notinselectedstock'
                  )
                );
                mb.afterClosed().subscribe(
                  (response: IMessageboxComponentResponse) => {
                    rows.forEach(f => {
                      if (f.productId) {
                        const product = data.find(
                          ip => ip.invoiceProductId == f.productId
                        );
                        if (product) {
                          this.setStockAndStockForProduct(f);
                        } else {
                          if (!response?.result) {
                            f.stockId = 0;
                            f.stockCode = '';
                            const stocksForProductArray: SmallGenericType[] =
                              [];
                            f.stocksForProduct.forEach(sp => {
                              if (sp.id != this.form?.value.stockId) {
                                stocksForProductArray.push(sp);
                              }
                            });
                            f.stocksForProduct = stocksForProductArray;
                            f.isModified = true;
                            this.form?.markAsDirty();
                          }
                        }
                      }
                    });
                    this.purchaseRows.next(rows);
                  }
                );
              } else {
                rows.forEach(f => {
                  this.setStockAndStockForProduct(f);
                });
                this.purchaseRows.next(rows);
              }
            })
          )
      );
    }
  }
  setStockAndStockForProduct(f: PurchaseRowDTO) {
    if (f.type !== PurchaseRowType.TextRow && f.productId) {
      f.stockId = this.form?.value.stockId;
      f.stockCode = this.form?.value.stockCode;
      const stocksForProduct = f.stocksForProduct.find(
        sp => sp.id == this.form?.value.stockId
      );
      if (!stocksForProduct) {
        f.stocksForProduct.push(
          new SmallGenericType(
            this.form?.value.stockId,
            this.form?.value.stockCode
          )
        );
      }
      f.isModified = true;
      this.form?.markAsDirty();
    }
  }

  stockValueChanged(value: number) {
    const stock = this.stocks.find(x => x.stockId == value);
    if (stock) {
      this.form?.patchValue({
        stockCode: stock.code,
      });
      this.setStockLocation(true, true);
    }
  }

  setStockNameAndDeliveryAddress(id: number, setDeliveryAddress: boolean) {
    const stock = this.stocks.find(x => x.stockId === id);
    if (stock && stock.name) {
      this.form?.patchValue({ stockCode: stock.code });
      if (setDeliveryAddress && !this.form?.value.orderId) {
        this.form?.patchValue({ deliveryAddressId: stock.deliveryAddressId });
      }
    }
  }

  override loadUserSettings(): Observable<void> {
    return this.performLoadData.load$(
      this.coreService
        .getUserAndCompanySettings(
          [UserSettingType.BillingDefaultStockPlace],
          false
        )
        .pipe(
          tap(x => {
            this.billingDefaultStockPlace = SettingsUtil.getIntUserSetting(
              x,
              UserSettingType.BillingDefaultStockPlace
            );
          })
        )
    );
  }

  override loadData(): Observable<void> {
    return this.performLoadData.load$(
      this.service.get(this.form?.getIdControl()?.value).pipe(
        tap(value => {
          this.form?.reset(value);
          this.changeSupplier();
          const date = value.currencyDate ?? value.purchaseDate ?? new Date();
          this.currencyService.setCurrencyIdAndDate(
            value.currencyId ?? 0,
            date
          );

          if (this.form?.value.referenceOur) {
            const selectedReferenceOur = this.ourReferencesId.find(
              f => f.name == this.form?.getRawValue().referenceOur
            );
            if (selectedReferenceOur) {
              this.form?.patchValue({
                referenceOurId: selectedReferenceOur.id,
              });
            }
          }

          //manual delivery Address
          if (value.deliveryAddressId == 0) {
            if (value.deliveryAddress != null && value.deliveryAddress != '') {
              this.deliveryAddresses[0].address = value.deliveryAddress;
            }
          }
          this.originalPurchase = SoeConfigUtil.cloneDTO(value);
          this.form?.customOriginUsersPatchValue(value.originUsers);

          this.originStatus = value.originStatus;

          this.setLabels();
          this.setControlVisibility();
          /*
          this.setStockLocation(false, false);
          */
          this.loadPurchaseRows(false);
        })
      )
    );
  }

  setControlVisibility() {
    if (this.form?.getRawValue().orderId) {
      this.visibleGetDeliveryAddressButton.set(true);
    } else {
      this.visibleGetDeliveryAddressButton.set(false);
    }
  }

  loadPurchaseRows(loadFromCopy = false): void {
    if (this.form?.value.purchaseId) {
      this.performLoadData.load(
        this.service
          .getPurchaseOrderRows(this.form?.value[this.idFieldName])
          .pipe(
            tap(value => {
              let totalAmountExVatCurrency = 0;

              value.forEach((element, i) => {
                element.stocksForProduct = [];
                element.sumAmountCurrency =
                  element.quantity * element.purchasePriceCurrency;
                totalAmountExVatCurrency =
                  totalAmountExVatCurrency +
                  element.quantity * element.purchasePriceCurrency;

                if (loadFromCopy) {
                  element.purchaseRowId = 0;
                  element.tempRowId = i + 1;
                }
                this.setStatusValues(element);
                if (element.stockId) {
                  element.stocksForProduct.push(
                    new SmallGenericType(element.stockId, element.stockCode)
                  );
                } else {
                  element.stockId = 0;
                  element.stocksForProduct.push(new SmallGenericType(0, ' '));
                }
              });
              this.totalAmountExVatCurrency.set(totalAmountExVatCurrency);
              value.sort((a, b) => a.rowNr - b.rowNr);
              this.originalPurchaseRows = SoeConfigUtil.cloneDTOs(value);
              this.purchaseRows.next(value);
              this.purchaseRowsLength = value.length;
              this.loadPurchaseRowsLabel();
            })
          )
      );
    } else if (!this.form?.isCopy) {
      this.purchaseRows.next([]);
    }
  }

  loadDeliveryRows() {
    return this.performLoadData.load$(
      this.purchaseDeliveryService
        .getPurchaseDeliveryRowsByPurchaseId(this.form?.value.purchaseId)
        .pipe(
          tap(data => {
            this.purchaseDeliveryRows.next(data);
          })
        )
    );
  }

  openConfirmDialog() {
    this.openSetPurchaseDateModal(
      this.form?.value.originStatus,
      this.form?.value.originStatus,
      true
    );
  }

  override loadCompanySettings(): Observable<void> {
    const settingTypes = [
      CompanySettingType.CustomerInvoiceDefaultVatType,
      CompanySettingType.BillingDefaultDeliveryType,
      CompanySettingType.BillingDefaultDeliveryCondition,
      CompanySettingType.BillingDefaultPurchaseOrderReportTemplate,
      CompanySettingType.BillingUseCentRounding,
      CompanySettingType.BillingDefaultEmailTemplatePurchase,
      CompanySettingType.BillingDefaultStock,
    ];
    return this.performLoadData.load$(
      this.coreService.getCompanySettings(settingTypes).pipe(
        tap(x => {
          this.defaultVatType = SettingsUtil.getIntCompanySetting(
            x,
            CompanySettingType.CustomerInvoiceDefaultVatType,
            this.defaultVatType
          );
          this.defaultDeliveryTypeId = SettingsUtil.getIntCompanySetting(
            x,
            CompanySettingType.BillingDefaultDeliveryType
          );
          this.defaultDeliveryConditionId = SettingsUtil.getIntCompanySetting(
            x,
            CompanySettingType.BillingDefaultDeliveryCondition
          );
          this.defaultReportId = SettingsUtil.getIntCompanySetting(
            x,
            CompanySettingType.BillingDefaultPurchaseOrderReportTemplate
          );
          this.useCentRounding = SettingsUtil.getBoolCompanySetting(
            x,
            CompanySettingType.BillingUseCentRounding
          );
          this.defaultEmailTemplatePurchase = SettingsUtil.getIntCompanySetting(
            x,
            CompanySettingType.BillingDefaultEmailTemplatePurchase
          );
          this.billingProductDefaultStock = SettingsUtil.getIntCompanySetting(
            x,
            CompanySettingType.BillingDefaultStock
          );
        })
      )
    );
  }

  loadPurchaseRowsLabel(total: number = this.totalAmountExVatCurrency()) {
    const _total = total.toLocaleString(undefined, {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    });
    this.secondaryAccordionTitlePurchaseRows.set(
      '({0}) | {1}: {2}'.format(
        this.purchaseRowsLength.toString(),
        this.translate.instant('billing.productrows.totalamount'),
        _total
      )
    );
  }

  getProject() {
    return {
      projectId: this.form?.getRawValue().projectId,
      name: this.form?.getRawValue().projectNr,
    };
  }

  loadOurReferences(useCache: boolean = false): Observable<any> {
    return this.performLoadData.load$(
      this.coreService.getUsersDict(true, false, true, false, useCache).pipe(
        tap(data => {
          this.ourReferencesId = data;
        })
      )
    );
  }
  loadDeliveryTypes(useCache: boolean = false): Observable<any> {
    return this.performLoadData.load$(
      this.deliveryTypesService.getDeliveryTypesDict(true, useCache).pipe(
        tap(data => {
          this.deliveryTypes = data;
        })
      )
    );
  }
  loadDeliveryConditions(useCache: boolean = false): Observable<any> {
    return this.performLoadData.load$(
      this.deliveryConditionService
        .getDeliveryConditionsDict(true, useCache)
        .pipe(
          tap(data => {
            this.deliveryConditions = data;
          })
        )
    );
  }

  loadDeliveryAddresses(
    customerId: number,
    useCache: boolean = false
  ): Observable<any[]> {
    return this.performLoadData.load$(
      this.coreService
        .getContactAddresses(
          customerId,
          TermGroup_SysContactAddressType.Delivery,
          true,
          true,
          false,
          useCache
        )
        .pipe(
          tap(data => {
            this.deliveryAddresses = data;
          })
        )
    );
  }

  loadStatusTypes(): Observable<ISmallGenericType[]> {
    return this.performLoadData.load$(
      this.coreService
        .getTermGroupContent(TermGroup.OriginStatus, false, false, true)
        .pipe(
          tap(data => {
            this.statusTypes = data.filter(
              t =>
                t.id === SoeOriginStatus.PurchaseAccepted ||
                t.id === SoeOriginStatus.PurchaseSent ||
                t.id === SoeOriginStatus.PurchaseDone ||
                t.id === SoeOriginStatus.Origin ||
                t.id === SoeOriginStatus.None
            );
            this.setupStatusFunctions();
          })
        )
    );
  }
  loadStockLocation(): Observable<any> {
    return this.performLoadData.load$(
      this.service.getStocks(true).pipe(
        tap(data => {
          const list: StockDTO[] = [];
          const obj = new StockDTO();
          obj.stockId = 0;
          obj.name = '';

          list.push(obj);
          data.forEach(d => {
            list.push(d);
          });
          this.stocks = list;
        })
      )
    );
  }

  peformPrintAction(selected: MenuButtonItem): void {
    switch (selected.id) {
      case PurchaseEditPrintFunctions.Print:
        this.printPurchase(this.defaultReportId);
        break;
      case PurchaseEditPrintFunctions.EMail:
        this.openSelectEmailDialog();
        break;
      case PurchaseEditPrintFunctions.ReportDialog:
        this.openSelectReport();
        break;
    }
  }

  executeStatusFunction(selected: MenuButtonItem): void {
    if (selected.id) {
      const option: SoeOriginStatus = selected.id;
      this.statusChanged(option);
    }
  }

  openSelectReport() {
    const dialogData = new SelectReportDialogData();
    dialogData.title = 'common.selectreport';
    dialogData.size = 'lg';
    dialogData.reportTypes = [SoeReportTemplateType.PurchaseOrder];
    dialogData.showCopy = false;
    dialogData.showEmail = true;
    dialogData.copyValue = false;
    dialogData.reports = [];
    dialogData.defaultReportId = 0;
    dialogData.langId = this.supplierHelper.sysLanguageId;
    dialogData.showReminder = false;
    dialogData.showLangSelection = true;
    dialogData.showSavePrintout = false;
    dialogData.savePrintout = false;
    const selectReportDialog = this.dialogServiceV2.open(
      SelectReportDialogComponent,
      dialogData
    );

    selectReportDialog
      .afterClosed()
      .subscribe((result: SelectReportDialogCloseData) => {
        if (result && result.reportId) {
          if (result.email) {
            this.openSelectEmailDialog(result.reportId);
          } else {
            this.printPurchase(result.reportId, result.languageId);
          }

          const model = new GetPurchasePrintUrlModel(
            [],
            [],
            result.reportId,
            result.languageId
          );

          this.reportService.getPurchasePrintUrl(model).pipe(
            tap(url => {
              BrowserUtil.openInSameTab(window, url);
            })
          );
        }
      });
  }

  openSelectEmailDialog(reportId = 0) {
    const dialogData = new SelectEmailDialogData();
    dialogData.title = 'common.checkdistribution';
    dialogData.size = 'lg';
    dialogData.defaultEmail = this.form?.value.contactEComId
      ? this.form?.value.contactEComId
      : 0;
    dialogData.defaultEmailTemplateId = this.defaultEmailTemplatePurchase;
    dialogData.recipients = this.supplierHelper.supplierEmails;
    dialogData.attachments = [];
    dialogData.attachmentsSelected = false;
    dialogData.checklists = [];
    dialogData.types = this.terms;
    dialogData.grid = false;
    dialogData.type = EmailTemplateType.PurchaseOrder;
    dialogData.showReportSelection = false;
    dialogData.reports = [];
    dialogData.defaultReportTemplateId = undefined;
    dialogData.langId = undefined;
    const selectEmailDialog = this.dialogServiceV2.open(
      SelectEmailDialogComponent,
      dialogData
    );
    this.selectEmailDialogSubscription = selectEmailDialog
      .afterClosed()
      .subscribe((result: SelectEmailDialogCloseData) => {
        const purchaseIds: number[] = [];
        const recipients: number[] = [];
        let singleRecipient = '';
        result.recipients.forEach(rec => {
          if (rec.id > 0) recipients.push(rec.id);
          else singleRecipient = rec.name;
        });
        const model = new SendPurchaseEmail(
          purchaseIds,
          result.emailTemplateId,
          result.languageId
        );
        model.purchaseId = this.form?.value.purchaseId;
        model.recipients = recipients;
        model.singleRecipient = singleRecipient;
        model.reportId = reportId;
        this.sendPurchaseAsEmailSubscription = this.service
          .sendPurchaseAsEmail(model)
          .pipe(
            tap((res: any) => {
              if (res.success) {
                this.form?.patchValue({
                  originStatus: SoeOriginStatus.PurchaseSent,
                });
                this.setStatusName(true);

                this.toasterService.success(
                  this.translate.instant('common.sent'),
                  ''
                );
              }
            })
          )
          .subscribe();
      });
  }

  printPurchase(reportId = 0, languageId = 0) {
    const model = new GetPurchasePrintUrlModel(
      [this.form?.value.purchaseId],
      [],
      reportId,
      languageId
    );

    this.performLoadData
      .load$(this.reportService.getPurchasePrintUrl(model))
      .subscribe(url => {
        BrowserUtil.openInSameTab(window, url);
      });
  }

  peformSaveAction(selected: MenuButtonItem): void {
    switch (selected.id) {
      case PurchaseEditSaveFunctions.Save:
        this.performSave();
        break;
      case PurchaseEditSaveFunctions.SaveAndClose:
        this.performSaveAndClose();
        break;
    }
  }
  performSaveAndClose() {
    this.additionalSaveProps = { closeTabOnSave: true };
    this.performSave();
  }

  override performSave(options?: ProgressOptions): void {
    if (!this.form || this.form.invalid || !this.service) return;

    let modifiedFields = null;
    const purchaseObj = this.form.getRawValue();
    if (purchaseObj.stockId == 0) {
      purchaseObj.stockId = undefined;
    }
    if (this.form.isNew) {
      modifiedFields = SoeConfigUtil.toDTO(
        purchaseObj,
        PurchaseDTO.getPropertiesToSkipOnSave(),
        true
      );
      modifiedFields.copyDeliveryAddress = false;
    } else {
      modifiedFields = SoeConfigUtil.diffDTO(
        this.originalPurchase,
        purchaseObj,
        PurchaseDTO.getPropertiesToSkipOnSave(),
        true
      );
      modifiedFields['purchaseid'] = this.form.purchaseId.value
        ? this.form.purchaseId.value
        : 0;
    }
    if (this.form.deliveryAddressId.value > 0) {
      this.form.patchValue({ deliveryAddress: '' });
      modifiedFields['deliveryaddress'] = null;
    }

    if (this.form.contactEComId.value > 0) {
      this.form.patchValue({ supplierEmail: '' });
      modifiedFields['supplieremail'] = null;
    } else {
      this.form.patchValue({ contactEComId: 0 });
      modifiedFields['contactecomid'] = null;
    }

    const rows = this.purchaseRows.getValue();
    const newRows = rows.filter(
      r =>
        !r.purchaseRowId &&
        (r.type === PurchaseRowType.TextRow ||
          r.productId ||
          r.supplierProductId)
    );
    const modifiedRows: any[] = [];

    rows
      .filter(r => r.purchaseRowId && r.isModified)
      .forEach(row => {
        let origRow: PurchaseRowDTO = new PurchaseRowDTO();
        origRow = SoeConfigUtil.cloneDTO(
          this.originalPurchaseRows.find(
            r => r.purchaseRowId && r.purchaseRowId === row.purchaseRowId
          )
        );
        if (origRow) {
          const rowDiffs = SoeConfigUtil.diffDTO(
            origRow,
            row,
            PurchaseRowDTO.getPropertiesToSkipOnSave(),
            true
          );
          if (row.quantity !== origRow.quantity)
            rowDiffs['quantity'] = row.quantity;
          if (row.purchasePriceCurrency !== origRow.purchasePriceCurrency)
            rowDiffs['purchasepricecurrency'] = row.purchasePriceCurrency;
          rowDiffs['purchaserowid'] = origRow.purchaseRowId;
          rowDiffs['rownr'] = row.rowNr;
          rowDiffs['state'] = row.state;
          rowDiffs['accDeliveryDate'] = row.accDeliveryDate;
          modifiedRows.push(rowDiffs);
        } else {
          newRows.push(row);
        }
      });
    if (this.deletedPurchaseRows.length > 0) {
      this.deletedPurchaseRows.forEach(row => {
        let origDelRow: PurchaseRowDTO = new PurchaseRowDTO();
        origDelRow = SoeConfigUtil.cloneDTO(
          this.deletedPurchaseRows.find(
            r => r.purchaseRowId && r.purchaseRowId === row.purchaseRowId
          )
        );
        if (origDelRow) {
          const rowDelDiffs = SoeConfigUtil.diffDTO(
            origDelRow,
            row,
            PurchaseRowDTO.getPropertiesToSkipOnSave(),
            true
          );
          if (row.quantity !== origDelRow.quantity)
            rowDelDiffs['quantity'] = row.quantity;
          if (row.purchasePriceCurrency !== origDelRow.purchasePriceCurrency)
            rowDelDiffs['purchasepricecurrency'] = row.purchasePriceCurrency;
          rowDelDiffs['purchaserowid'] = origDelRow.purchaseRowId;
          rowDelDiffs['rownr'] = origDelRow.rowNr;
          rowDelDiffs['state'] = origDelRow.state;
          rowDelDiffs['accDeliveryDate'] = origDelRow.accDeliveryDate;
          modifiedRows.push(rowDelDiffs);
        }
      });
    }
    const model = new SavePurchaseModel();
    model.modifiedRows = modifiedRows;
    model.newRows = newRows;
    model.originUsers = this.form?.value.originUsers;
    model.modifiedFields = modifiedFields;
    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service
        .savePurchase(model)
        .pipe(tap(this.updateFormValueAndEmitChange)),
      undefined,
      undefined,
      options
    );
  }

  override onSaveCompleted(backendResponse: BackendResponse): void {
    if (ResponseUtil.getEntityId(backendResponse)) {
      this.form!.isNew = false;
      this.deletedPurchaseRows = [];
    }
  }

  purchaseRowDeleted(row: PurchaseRowDTO) {
    let foundId = false;
    this.deletedPurchaseRows.forEach(f => {
      if (row.parentRowId === f.purchaseRowId) {
        foundId = true;
      }
    });
    if (!foundId) {
      row.isModified = true;
      row.state = SoeEntityState.Deleted;
      this.deletedPurchaseRows.push(row);
    }
  }

  setupFunctions() {
    this.translate
      .get([
        'billing.purchase.rows',
        'core.save',
        'core.saveandclose',
        'common.report.report.print',
        'common.email',
        'common.report.report.reports',
        'billing.purchase.list.purchase',
        'billing.purchase.rows',
        'billing.productrows.functions.newpurchase',
        'billing.purchase.list.purchase',
        'billing.purchase.supplier',
        'billing.order.status',
        'billing.project.project',
        'billing.order.noproject',
        'billing.productrows.totalamount',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        // Functions
        this.menuSaveList = [];
        this.menuPrintList = [];
        this.menuSaveList.push({
          id: PurchaseEditSaveFunctions.Save,
          label: terms['core.save'] + ' (Ctrl+S)',
          icon: 'save',
        });
        this.menuSaveList.push({
          id: PurchaseEditSaveFunctions.SaveAndClose,
          label: terms['core.saveandclose'] + ' (Ctrl+Enter)',
          icon: 'save',
        });

        this.menuPrintList.push({
          id: PurchaseEditPrintFunctions.Print,
          label: terms['common.report.report.print'],
          icon: 'print',
        });
        this.menuPrintList.push({
          id: PurchaseEditPrintFunctions.EMail,
          label: terms['common.email'],
          icon: 'envelope',
        });
        this.menuPrintList.push({
          id: PurchaseEditPrintFunctions.ReportDialog,
          label: terms['common.report.report.reports'],
          icon: 'print',
        });
      });
  }

  currencyChanged() {
    if (this.form?.value) {
      this.currencyService.toForm(this.form);
      this.setCurrencyRateSecondaryLabel();
      // Recalculate rows
      if (this.purchaseRowsComponent)
        this.purchaseRowsComponent.recalculateRows();
    }
  }

  ngOnDestroy() {
    this.getPurchasePrintUrlSubscription?.unsubscribe();
    this.sendPurchaseAsEmailSubscription?.unsubscribe();
    this.selectEmailDialogSubscription?.unsubscribe();
  }
}
