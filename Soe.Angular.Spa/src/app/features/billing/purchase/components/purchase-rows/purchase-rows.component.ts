import {
  Component,
  EventEmitter,
  inject,
  Input,
  OnInit,
  Output,
  signal,
} from '@angular/core';
import { PurchaseForm } from '@features/billing/purchase/models/purchase-form.model';
import { StockWarehouseService } from '@features/billing/stock-warehouse/services/stock-warehouse.service';
import { ChangeIntrastatCodeComponent } from '@shared/components/billing/change-intrastat-code/components/change-intrastat-code/change-intrastat-code.component';
import {
  ChangeIntrastatCodeDialogData,
  IntrastatTransactionDTO,
} from '@shared/components/billing/change-intrastat-code/models/change-intrastat-code.model';
import { SelectCustomerInvoiceDialogComponent } from '@shared/components/select-customer-invoice-dialog/component/select-customer-invoice-dialog/select-customer-invoice-dialog.component';
import {
  SearchCustomerInvoiceDTO,
  SelectInvoiceDialogDTO,
} from '@shared/components/select-customer-invoice-dialog/model/customer-invoice-search.model';
import { TextBlockDialogData } from '@shared/components/text-block-dialog/models/text-block-dialog.model';
import { TextBlockDialogComponent } from '@shared/components/text-block-dialog/text-block-dialog.component';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { ValidationHandler } from '@shared/handlers';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  CompanySettingType,
  Feature,
  PurchaseRowType,
  SimpleTextEditorDialogMode,
  SoeEntityState,
  SoeEntityType,
  SoeInvoiceRowDiscountType,
  SoeOriginStatus,
  SoeOriginType,
  TermGroup_CurrencyType,
  TermGroup_InvoiceProductVatType,
  TermGroup_Languages,
  TextBlockType,
} from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { IProductSmallDTO } from '@shared/models/generated-interfaces/ProductDTOs';
import { IPurchaseRowDTO } from '@shared/models/generated-interfaces/PurchaseDTOs';
import { IProductUnitSmallDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ISupplierProductSmallDTO } from '@shared/models/generated-interfaces/SupplierProductDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { CurrencyService } from '@shared/services/currency.service';
import { DateUtil } from '@shared/util/date-util';
import { NumberUtil } from '@shared/util/number-util';
import { Perform } from '@shared/util/perform.class';
import { SettingsUtil } from '@shared/util/settings-util';
import { ProductUnitService } from '@src/app/features/billing/product-units/services/product-unit.service';
import { ProductService } from '@src/app/features/billing/products/services/product.service';
import { PurchaseProductsService } from '@src/app/features/billing/purchase-products/services/purchase-products.service';
import { BillingService } from '@src/app/features/billing/services/services/billing.service';
import { AggregationType } from '@ui/grid/interfaces';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { GridComponent } from '@ui/grid/grid.component';
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox';
import { MenuButtonItem } from '@ui/button/menu-button/menu-button.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import {
  CellClassParams,
  CellEditingStartedEvent,
  CellValueChangedEvent,
} from 'ag-grid-community';
import { BehaviorSubject, forkJoin, Observable, of, take, tap } from 'rxjs';
import {
  SupplierProductDTO,
  SupplierProductSmallDTO,
} from 'src/app/features/billing/purchase-products/models/purchase-product.model';
import { PurchaseRowsForm } from '../../models/purchase-rows-form.model';
import {
  ProductRowsProductDTO,
  PurchaseRowDTO,
  PurchaseRowSummeryFormDTO,
} from '../../models/purchase-rows.model';

export enum FunctionType {
  SetAcknowledgedDeliveryDate = 1,
  Intrastat = 2,
}

export enum AddRowFunctionType {
  Add = 1,
  AddText = 2,
  DeleteRow = 3,
}

@Component({
  selector: 'soe-purchase-rows',
  templateUrl: './purchase-rows.component.html',
  styleUrls: ['./purchase-rows.component.scss'],
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class PurchaseRowsComponent
  extends GridBaseDirective<PurchaseRowDTO>
  implements OnInit
{
  @Input() rows!: BehaviorSubject<PurchaseRowDTO[]>;
  @Input() parentForm!: PurchaseForm;
  @Input() totalAmountExVatCurrency!: number;
  @Input() originStatus!: number;
  @Input() stockId!: number | undefined;
  @Input() stockCode!: number | undefined;
  @Input() purchaseId: number | undefined;
  @Input() useCentRounding: boolean | undefined;
  @Input() sysCountryId: number | undefined;
  @Input() intrastatCodeId: number | undefined;
  @Input() currencyService!: CurrencyService;

  @Output() getTotal = new EventEmitter<number>();
  @Output() rowDeleted = new EventEmitter<PurchaseRowDTO>();

  productService = inject(ProductService);
  productUnitService = inject(ProductUnitService);
  purchaseProductsService = inject(PurchaseProductsService);
  progressService = inject(ProgressService);
  validationHandler = inject(ValidationHandler);
  dialogServiceV2 = inject(DialogService);
  coreService = inject(CoreService);
  stockService = inject(StockWarehouseService);
  private billingService = inject(BillingService);

  performProductUnits = new Perform<ISmallGenericType[]>(this.progressService);

  products: IProductSmallDTO[] = [];
  performSupplierProduct = new Perform<SupplierProductSmallDTO[]>(
    this.progressService
  );
  performLoad = new Perform<any>(this.progressService);
  form: PurchaseRowsForm = new PurchaseRowsForm({
    validationHandler: this.validationHandler,
    element: new PurchaseRowSummeryFormDTO(),
  });
  centRounding: string = '';
  invoiceData: SearchCustomerInvoiceDTO | undefined;
  purchaseUnit: IProductUnitSmallDTO[] = [];
  private discountTypeDict: SmallGenericType[] = [];
  featureMenuList: MenuButtonItem[] = [];
  addNewMenuList: MenuButtonItem[] = [];
  isDisabledFeatureButton = signal(true);
  disableRowDelete = signal(true);
  supplierProducts: ISupplierProductSmallDTO[] = [];
  productList: ProductRowsProductDTO[] = [];

  // Company settings
  private defaultStockId = 0;
  private defaultProductUnitId = 0;
  private intrastatOriginType = 0;

  //permissions
  private useStock = signal(false);
  private intrastatPermission = signal(false);
  showAllRowsValue = signal(false);

  //private visibleRows: PurchaseRowDTO[] = [];
  private supplierProductList: SupplierProductDTO[] = [];
  private tempRowIdCounter = 0;

  private readonly = false;

  // acknowledgeDeliveryDate
  messageboxService = inject(MessageboxService);

  override ngOnInit(): void {
    super.ngOnInit();

    this.form?.totalAmountExVatCurrency.disable();
    this.form?.centRounding.disable();
    this.form?.baseCurrencyCode.disable();
    this.loadDiscountTypeDict();

    this.startFlow(
      Feature.Billing_Purchase_Purchase_Edit,
      'billing.purchase.rows',
      {
        additionalModifyPermissions: [
          Feature.Billing_Stock,
          Feature.Billing_Product_Products_ShowSalesPrice,
          Feature.Economy_Intrastat,
        ],
        skipInitialLoad: true,
        lookups: [
          this.loadProducts(),
          this.loadProductUnits(),
          this.loadSupplierProductId(),
        ],
        useLegacyToolbar: true,
      }
    );
    this.buildFunctionList();
  }

  override onPermissionsLoaded() {
    super.onPermissionsLoaded();
    this.useStock.set(this.flowHandler.hasModifyAccess(Feature.Billing_Stock));
    this.intrastatPermission.set(
      this.flowHandler.hasModifyAccess(Feature.Economy_Intrastat)
    );
  }

  override onGridReadyToDefine(grid: GridComponent<PurchaseRowDTO>) {
    this.exportFilenameKey.set('billing.purchase.rows');
    super.onGridReadyToDefine(grid);
    this.grid.setNbrOfRowsToShow(8, 8);
    this.grid.context.suppressDoubleClickToEdit = true;
    this.grid.api.updateGridOptions({
      onCellValueChanged: this.onCellValueChanged.bind(this),
      onCellEditingStarted: this.onCellEditingStarted.bind(this),
    });

    this.translate
      .get([
        'core.deleterow',
        'common.newrow',
        'common.rownr',
        'common.status',
        'common.new',
        'billing.order.ordernr',
        'billing.productrows.stockcode',
        'billing.productrows.addtextrow',
        'billing.purchaserows.quantity',
        'billing.purchaserows.wanteddeliverydate',
        'billing.purchaserows.accdeliverydate',
        'billing.purchaserows.deliverydate',
        'billing.purchaserows.productnr',
        'billing.purchaserows.text',
        'billing.purchaserows.discount',
        'billing.purchaserows.discounttype',
        'billing.purchaserows.deliveredquantity',
        'billing.purchaserows.purchaseprice',
        'billing.purchaserows.vatrate',
        'billing.purchaserows.vatamount',
        'billing.purchaserows.sumamount',
        'billing.purchaserows.purchaseunit',
        'billing.purchaserows.supplieritemno',
        'billing.purchaserows.acknowledgeDeliveryDate',
        'billing.productrows.functions.changeintrastatcode',
        'billing.purchaserows.discounttype.percent',
        'billing.purchaserows.discounttype.amount',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.enableRowSelection();
        this.grid.addColumnModified('isModified');
        this.grid.addColumnNumber('rowNr', terms['common.rownr'], {
          flex: 1,
          enableHiding: false,
          editable: false,
        });

        this.grid.addColumnSingleValue(); // add this where it should start to span over the columns

        this.grid.addColumnAutocomplete<SupplierProductSmallDTO>(
          'supplierProductId',
          terms['billing.purchaserows.supplieritemno'],
          {
            flex: 1,
            editable: row => this.isColEditable(row.data),
            source: () => this.supplierProducts,
            optionIdField: 'supplierProductId',
            optionNameField: 'numberName',
            optionDisplayNameField: 'supplierProductNr',
            cellClassRules: {
              'error-background-color': (params: CellClassParams) =>
                this.supplierProductNrCellRules(params),
            },
          }
        );
        this.grid.addColumnText('text', terms['billing.purchaserows.text'], {
          flex: 1,
          enableHiding: false,
          editable: row => this.isColEditable(row.data),
          //colSpan: (params: any) => this.isCurrentRowColspan(params),
        });
        this.grid.addColumnAutocomplete<IProductSmallDTO>(
          'productId',
          terms['billing.purchaserows.productnr'],
          {
            flex: 1,
            editable: row => this.isColEditable(row.data),
            source: _ => this.products ?? [],
            optionIdField: 'productId',
            optionNameField: 'number',
            optionDisplayNameField: 'productNr',
            cellClassRules: {
              'error-background-color': (params: CellClassParams) =>
                this.productNrCellRules(params),
            },
          }
        );
        this.grid.addColumnNumber(
          'quantity',
          terms['billing.purchaserows.quantity'],
          {
            flex: 1,
            enableHiding: false,
            editable: true,
          }
        );
        this.grid.addColumnSelect(
          'purchaseUnitId',
          terms['billing.purchaserows.purchaseunit'],
          this.purchaseUnit,
          () => {},
          {
            flex: 1,
            dropDownIdLabel: 'productUnitId',
            dropDownValueLabel: 'code',
            enableHiding: false,
            editable: row => this.isColEditable(row.data),
          }
        );
        this.grid.addColumnNumber(
          'purchasePriceCurrency',
          terms['billing.purchaserows.purchaseprice'],
          {
            flex: 1,
            decimals: 2,
            editable: row => this.isColEditable(row.data),
            cellClassRules: {
              'error-background-color': (params: CellClassParams) =>
                this.priceCellRules(params),
            },
          }
        );
        this.grid.addColumnDate(
          'wantedDeliveryDate',
          terms['billing.purchaserows.wanteddeliverydate'],
          {
            flex: 1,
            enableHiding: false,
            editable: row => this.isColEditable(row.data),
          }
        );
        if (this.useStock()) {
          this.grid.addColumnSelect(
            'stockId',
            terms['billing.productrows.stockcode'],
            [],
            undefined,
            {
              dropDownIdLabel: 'id',
              dropDownValueLabel: 'name',
              dynamicSelectOptions: (row: any) =>
                row?.data?.stocksForProduct || [],
              flex: 1,
              enableHiding: true,
              editable: (row: { data: PurchaseRowDTO | undefined }) =>
                this.isColEditable(row.data),
            }
          );
        }

        this.grid.addColumnText('orderNr', terms['billing.order.ordernr'], {
          flex: 1,
          tooltipField: 'attestStateName',
          buttonConfiguration: {
            iconPrefix: 'fal',
            iconName: 'search',
            tooltip: terms['common.customer.invoices.emailsent'],
            onClick: row => {
              this.openOrderSearch(row);
            },
          },
        });
        this.grid.addColumnDate(
          'accDeliveryDate',
          terms['billing.purchaserows.accdeliverydate'],
          {
            editable: row => this.isAccDateEditable(row.data),
            flex: 1,
            enableHiding: false,
          }
        );
        this.grid.addColumnNumber(
          'deliveredQuantity',
          terms['billing.purchaserows.deliveredquantity'],
          {
            flex: 1,
          }
        );
        this.grid.addColumnDate(
          'deliveryDate',
          terms['billing.purchaserows.deliverydate'],
          {
            flex: 1,
            enableHiding: false,
          }
        );
        this.grid.addColumnNumber(
          'discountValue',
          terms['billing.purchaserows.discount'],
          {
            flex: 1,
            enableHiding: true,
            decimals: 2,
            maxDecimals: 4,
            editable: row => this.isColEditable(row.data),
          }
        );
        this.grid.addColumnSelect(
          'discountType',
          terms['billing.purchaserows.discounttype'],
          this.discountTypeDict,
          this.gridDiscountTypeChanged.bind(this),
          {
            dropDownIdLabel: 'id',
            dropDownValueLabel: 'name',
            editable: true,
            enableHiding: true,
            flex: 1,
          }
        );
        this.grid.addColumnNumber(
          'sumAmountCurrency',
          terms['billing.purchaserows.sumamount'],
          {
            flex: 1,
            decimals: 2,
          }
        );

        this.grid.addColumnShape('statusIcon', '', {
          flex: 1,
          alignCenter: true,
          enableHiding: true,
          shape: 'circle',
          colorField: 'statusIcon',
          tooltipField: 'statusName',
        });

        this.grid.addColumnIconEdit({
          flex: 1,
          tooltip: terms['core.edit'],
          showIcon: row => row.type == PurchaseRowType.TextRow,
          onClick: row => this.edit(row),
        });
        this.grid.addColumnIconDelete({
          flex: 1,
          tooltip: terms['core.delete'],
          onClick: row => this.deleteRow(row),
        });

        this.grid.setSingelValueConfiguration([
          {
            field: 'text',
            predicate: (data: PurchaseRowDTO) =>
              data.type === PurchaseRowType.TextRow && this.readonly,
            editable: false,
          },
          {
            field: 'text',
            predicate: (data: PurchaseRowDTO) =>
              data.type === PurchaseRowType.TextRow && !this.readonly,
            editable: true,
          },
        ]);

        this.grid.addAggregationsRow({
          quantity: AggregationType.Sum,
          deliveredQuantity: AggregationType.Sum,
          sumAmountCurrency: AggregationType.Sum,
        });

        super.finalizeInitGrid();
      });
    this.form?.patchValue({
      totalAmountExVatCurrency: this.totalAmountExVatCurrency.toFixed(2),
    });
  }

  override onFinished(): void {
    this.purchaseRowsUpdated(false, true);
  }

  onCellEditingStarted(event: CellEditingStartedEvent) {
    if (event.colDef.field === 'stockId') {
      this.setStocksForProduct(event.data);
    }
  }

  showAllRowsValueChanged(event: boolean) {
    this.showAllRowsValue.set(event);
    this.showAllRows(event);
  }

  private showAllRows(status: boolean) {
    if (status) {
      if (this.rows.getValue().length > 8) {
        this.grid.setNbrOfRowsToShow(8);
      } else {
        this.grid.setNbrOfRowsToShow(this.rows.getValue().length);
      }
    } else {
      this.grid.setNbrOfRowsToShow(8, 8);
    }
    this.grid.updateGridHeightBasedOnNbrOfRows();
  }
  gridDiscountTypeChanged(row: any) {
    const rowData = row.data;
    if (!rowData) return;
    const obj = this.discountTypeDict.find((d: any) => {
      return d.id == row.data.discountType;
    });
    if (!obj) return;
    rowData.id = obj.id || rowData.discountType;
  }

  isCurrentRowColspan(params: any) {
    return params.data.type === PurchaseRowType.TextRow
      ? this.useStock()
        ? 15
        : 14
      : 1;
  }

  override loadCompanySettings() {
    const settingTypes: number[] = [
      CompanySettingType.BillingDefaultInvoiceProductUnit,
      CompanySettingType.BillingDefaultStock,
      CompanySettingType.BillingDefaultVatCode,
      CompanySettingType.BillingUseCentRounding,
      CompanySettingType.IntrastatImportOriginType,
    ];
    return this.coreService.getCompanySettings(settingTypes).pipe(
      tap(setting => {
        this.intrastatOriginType = SettingsUtil.getIntCompanySetting(
          setting,
          CompanySettingType.IntrastatImportOriginType
        );
        this.defaultStockId = SettingsUtil.getIntCompanySetting(
          setting,
          CompanySettingType.BillingDefaultStock
        );
        this.defaultProductUnitId = SettingsUtil.getIntCompanySetting(
          setting,
          CompanySettingType.BillingDefaultInvoiceProductUnit
        );
      })
    );
  }

  buildFunctionList() {
    this.featureMenuList.push({
      id: FunctionType.SetAcknowledgedDeliveryDate,
      label:
        ' + ' +
        this.translate.instant('billing.purchaserows.acknowledgeDeliveryDate'),
      disabled: this.isDisabledFeatureButton,
    });
    if (
      this.intrastatPermission() &&
      this.intrastatOriginType === SoeOriginType.Purchase
    ) {
      this.featureMenuList.push({
        id: FunctionType.Intrastat,
        label: this.translate.instant(
          'billing.productrows.functions.changeintrastatcode'
        ),
      });
    }

    this.addNewMenuList.push(
      {
        id: AddRowFunctionType.Add,
        label: this.translate.instant('common.newrow'),
        icon: 'plus',
        disabled: signal(false),
      },
      {
        id: AddRowFunctionType.AddText,
        label: this.translate.instant('billing.productrows.addtextrow'),
        icon: 'text',
        disabled: signal(false),
      },
      {
        type: 'divider',
      },
      {
        id: AddRowFunctionType.DeleteRow,
        label: this.translate.instant('core.deleterow'),
        icon: 'times',
        disabled: this.disableRowDelete,
      }
    );
  }

  loadDiscountTypeDict() {
    this.discountTypeDict.push({
      id: SoeInvoiceRowDiscountType.Percent,
      name: this.translate.instant('billing.purchaserows.discounttype.percent'),
    });
    this.discountTypeDict.push({
      id: SoeInvoiceRowDiscountType.Amount,
      name: this.translate.instant('billing.purchaserows.discounttype.amount'),
    });
    return this.discountTypeDict;
  }

  refreshRows() {
    if (this.rows.getValue()) {
      this.grid.refreshCells();
    }
  }

  recalculateRows() {
    if (this.rows) {
      //this.calculateTotals();
      this.calculateRowSums(true);
    }
  }

  calculateTotals() {
    let cent = 0;
    this.centRounding = '';
    this.totalAmountExVatCurrency = 0;
    this.rows.getValue().forEach(r => {
      this.totalAmountExVatCurrency += r.sumAmountCurrency;
    });
    if (this.useCentRounding) {
      cent =
        Math.abs(this.totalAmountExVatCurrency) -
        Math.floor(Math.abs(this.totalAmountExVatCurrency));
      if (cent !== 0) {
        cent =
          Number(this.totalAmountExVatCurrency.toFixed(0)) -
          this.totalAmountExVatCurrency;
        this.totalAmountExVatCurrency = Number(
          this.totalAmountExVatCurrency.toFixed(0)
        );
        this.form.patchValue({
          centRounding: cent.toFixed(2),
        });
        this.centRounding = cent.toFixed(2);
      }
      if (this.currencyService?.transactionCurrencyRate) {
        this.form.patchValue({
          baseCurrencyCode:
            this.totalAmountExVatCurrency *
            this.currencyService?.transactionCurrencyRate,
        });
      }

      this.form?.patchValue({
        totalAmountExVatCurrency: this.totalAmountExVatCurrency.toFixed(2),
      });
    }
  }

  updateFromPurchase(purchaseSupplier: any) {
    this.supplierChanged(true);
  }

  peformActionAddRow(selected: MenuButtonItem): void {
    switch (selected.id) {
      case AddRowFunctionType.Add:
        this.addRow(PurchaseRowType.PurchaseRow);
        break;
      case AddRowFunctionType.AddText:
        this.addRow(PurchaseRowType.TextRow);
        break;
      case AddRowFunctionType.DeleteRow:
        this.deleteRows();
    }
  }

  peformAction(selected: MenuButtonItem): void {
    switch (selected.id) {
      case FunctionType.SetAcknowledgedDeliveryDate:
        this.openAcknowledgeDeliveryDate();
        break;
      case FunctionType.Intrastat:
        this.openIntrastat();
        break;
    }
  }

  addRow(type: PurchaseRowType) {
    let rowNr = 1;
    let selectedIndex = this.grid?.api.getFocusedCell()?.rowIndex ?? -1;

    if (
      this.rows.value.length > 0 &&
      this.rows.value[this.rows.value.length - 1]
    ) {
      if (selectedIndex === -1) {
        // If no row is selected, add it to the end as fallback
        selectedIndex = this.rows.value.length - 1;
      }
      rowNr = this.rows.value[this.rows.value.length - 1].rowNr + 1;
    }

    const row: PurchaseRowDTO = {
      tempRowId: 0,
      purchaseRowId: 0,
      purchaseId: 0,
      purchaseNr: '',
      productName: '',
      productNr: '',
      stockCode: '',
      rowNr: rowNr,
      purchaseUnitId: 0,
      quantity: 0,
      text: '',
      purchasePrice: 0,
      purchasePriceCurrency: 0,
      discountType: 0,
      discountAmount: 0,
      discountAmountCurrency: 0,
      discountPercent: 0,
      vatAmount: 0,
      vatAmountCurrency: 0,
      vatRate: 0,
      vatCodeName: '',
      vatCodeCode: '',
      sumAmount: 0,
      sumAmountCurrency: 0,
      orderNr:
        this.parentForm.getAllValues({ includeDisabled: true }).orderNr ?? '',
      status: 0,
      statusName: '',
      isLocked: false,
      state: SoeEntityState.Active,
      type: type,
      supplierProductNr: '',
      customerInvoiceRowIds: [],
      modifiedBy: '',
      isModified: true,
      statusIcon: '',
      purchaseProductUnitCode: '',
      discountTypeText: '',
      discountValue: 0,
      stocksForProduct: [],
      stockId: this.parentForm.value.stockId,
    };
    if (this.parentForm.value.stockId && this.parentForm.value.stockId > 0) {
      row.stocksForProduct.push({
        id: this.parentForm.value.stockId,
        name: this.parentForm.value.stockCode,
      });
    }

    const currentRows = [...this.rows.getValue()];
    // Insert the new row just below the selected row
    if (selectedIndex !== -1) {
      // Insert the new row at the correct position
      currentRows.splice(selectedIndex + 1, 0, row);
    } else {
      // If no row is selected, append it at the end
      currentRows.push(row);
    }

    currentRows.forEach((r, index) => {
      r.rowNr = index + 1;
    });

    this.rows.next(currentRows);

    this.grid.api.refreshCells({ force: true });
    if (
      selectedIndex === 0 &&
      rowNr === 1 &&
      type === PurchaseRowType.PurchaseRow
    ) {
      this.focusFirstCell();
    } else {
      this.focusCell(selectedIndex + 1, 'supplierProductId');
    }

    this.grid.options.context.newRow = true;
    this.parentForm.markAsDirty();
    this.purchaseRowsUpdated(false, true);
    this.showAllRows(this.showAllRowsValue());
  }

  private focusFirstCell(): void {
    setTimeout((): void => {
      const lastRowIdx = this.grid?.api.getLastDisplayedRowIndex();
      this.grid?.api.setFocusedCell(lastRowIdx, 'supplierProductId');
      this.grid?.api.startEditingCell({
        rowIndex: lastRowIdx,
        colKey: 'supplierProductId',
      });
    }, 100);
  }

  private focusCell(index: number, colKey: string): void {
    setTimeout((): void => {
      this.grid?.api.setFocusedCell(index, colKey);
      this.grid?.api.startEditingCell({
        rowIndex: index,
        colKey: colKey,
      });
    }, 100);
  }

  priceCellRules(params: any) {
    return (
      (params.data.supplierProductId || params.data.productId) &&
      (!params.data.purchasePriceCurrency ||
        params.data.purchasePriceCurrency === 0)
    );
  }

  productNrCellRules(params: any) {
    return params.data.supplierProductId && !params.data.productId;
  }

  supplierProductNrCellRules(params: any) {
    return !params.data.supplierProductId && params.data.productId;
  }

  override createLegacyGridToolbar(): void {
    if (!this.flowHandler.modifyPermission()) return;

    const sortToolbar = this.toolbarUtils.createLegacySortGroup(
      () => {
        this.sortFirst();
        this.parentForm?.markAsDirty();
      },
      () => {
        this.sortUp();
        this.parentForm?.markAsDirty();
      },
      () => {
        this.sortDown();
        this.parentForm?.markAsDirty();
      },
      () => {
        this.sortLast();
        this.parentForm?.markAsDirty();
      }
    );
    sortToolbar.alignmentRight = false;
  }

  public setRowAsModified(row: PurchaseRowDTO, notify = true) {
    if (row) {
      row.isModified = true;
      if (notify) this.setParentAsModified();
      this.grid.refreshCells();
    }
  }
  private setParentAsModified() {
    this.parentForm.markAsDirty();
  }

  // Sorting
  private sortFirst() {
    const handledRows: number[] = [];
    const rows: PurchaseRowDTO[] = this.grid
      .getSelectedRows()
      .sort(r => r.rowNr);
    const currentRow = this.grid.getCurrentRow();
    if (rows.length === 0 && currentRow) rows.push(currentRow);

    rows.forEach(row => {
      if (!handledRows.find(id => id === row.tempRowId)) {
        if (handledRows.length === 0) {
          this.rows.value
            .filter(r => r.rowNr > 0 && r.rowNr <= row.rowNr)
            .forEach(r => {
              this.setRowAsModified(r, false);
            });
        }
        // Move row to the top
        row.rowNr = -(rows.length - handledRows.length);

        handledRows.push(row.tempRowId);
      }
    });

    this.afterSortMultiple(rows);
  }

  private sortUp() {
    // Get current row
    const handledRows: number[] = [];
    const rows: PurchaseRowDTO[] = this.grid
      .getSelectedRows()
      .sort(r => r.rowNr);
    const currentRow = this.grid.getCurrentRow();
    if (rows.length === 0 && currentRow) rows.push(currentRow);

    if (rows.length > 0) {
      this.multiplyRowNr();

      // Move current row before previous row
      rows.forEach(row => {
        const filteredList = this.rows.value
          .filter(r => r.rowNr < row.rowNr)
          .sort((a, b) => a.rowNr - b.rowNr);

        const prevRow = filteredList[filteredList.length - 1];
        if (prevRow) {
          row.rowNr = prevRow.rowNr - (rows.length - handledRows.length) - 10;
          handledRows.push(row.tempRowId);
          this.setRowAsModified(prevRow);
        }
      });

      this.afterSortMultiple(rows);
    }
  }

  private sortDown() {
    // Get current row

    const handledRows: number[] = [];
    const currentRow = this.grid.getCurrentRow();
    const rows: PurchaseRowDTO[] = this.grid
      .getSelectedRows()
      .filter(r => r.rowNr < this.rows.value.length)
      .sort(r => r.rowNr);
    if (rows.length === 0 && currentRow) rows.push(currentRow);

    if (rows.length > 0) {
      this.multiplyRowNr();
      rows.forEach(row => {
        // Get next row
        const nextRow = this.rows.value
          .filter(
            r =>
              r.rowNr > row.rowNr &&
              !rows.find(sr => sr.tempRowId === r.tempRowId)
          )
          .sort(s => s.rowNr)[0];

        if (nextRow) {
          if (!handledRows.find(id => id === row.tempRowId)) {
            // Move current row after next row
            row.rowNr = nextRow.rowNr + (rows.length + handledRows.length) + 10;
            handledRows.push(row.tempRowId);
          }
          this.setRowAsModified(nextRow);
        }
      });

      this.afterSortMultiple(rows);
    }
  }

  private sortLast() {
    const handledRows: number[] = [];
    const rows: PurchaseRowDTO[] = this.grid
      .getSelectedRows()
      .filter(r => r.rowNr <= this.rows.value.length)
      .sort(r => r.rowNr);
    const currentRow = this.grid.getCurrentRow();
    if (rows.length === 0 && currentRow) rows.push(currentRow);

    rows.forEach(row => {
      if (!handledRows.find(id => id === row.tempRowId)) {
        if (handledRows.length === 0) {
          this.rows.value
            .filter(r => r.rowNr >= row.rowNr)
            .map(r => {
              this.setRowAsModified(r, false);
            });
        }

        // Move row to the bottom
        row.rowNr =
          NumberUtil.max(this.rows.value, 'rowNr') + 2 + handledRows.length;

        handledRows.push(row.tempRowId);
      }
    });

    this.afterSortMultiple(rows);
  }

  private afterSortMultiple(rows: PurchaseRowDTO[]) {
    rows.forEach(row => {
      this.setRowAsModified(row, false);
    });

    this.reNumberRows();
    this.setParentAsModified();
  }

  private reNumberRows() {
    let i = 1;
    this.rows.value
      .filter(
        r =>
          r.type === PurchaseRowType.PurchaseRow ||
          r.type === PurchaseRowType.TextRow
      )
      .sort((a, b) => a.rowNr - b.rowNr)
      .map(r => {
        const oldRowNr = r.rowNr;
        r.rowNr = i++;
        if (oldRowNr && oldRowNr !== r.rowNr) {
          r.isModified = true;
        }
      });
    this.resetRows();
  }

  private resetRows() {
    const selectedRowIds = this.grid.getSelectedIds('tempRowId');

    this.rows.value.sort((a, b) => a.rowNr - b.rowNr);

    const updatedRows = this.rows.getValue();
    this.rows.next(updatedRows);

    if (selectedRowIds && selectedRowIds.length > 0) {
      this.rows.value.map(r => {
        if (selectedRowIds.find(f => f === r.tempRowId)) {
          const idx = this.grid?.api
            .getRenderedNodes()
            .findIndex(s => (<PurchaseRowDTO>s.data).tempRowId === r.tempRowId);
          this.grid?.api.getRenderedNodes()[idx].setSelected(true);
        }
      });
    }
  }

  onCellValueChanged(event: CellValueChangedEvent) {
    switch (event.colDef.field) {
      case 'productId':
        this.productChanged(event.data);
        break;

      case 'quantity':
        this.quantityChanged(event.data);
        break;
      case 'purchasePriceCurrency':
        this.purchasePriceChanged(event.data);
        break;
      case 'supplierProductId':
        this.supplierProductChanged(event.data);
        this.focusCell(event.rowIndex ? event.rowIndex : 0, 'quantity');

        break;
      case 'discountValue':
      case 'discountType':
        this.discountChanged(event.data);
        break;
      default:
        this.parentForm.markAsDirty();
        break;
    }

    event.data.isModified = true;
    this.refreshRows();
    this.getTotal.emit(this.form?.value.totalAmountExVatCurrency);
    this.parentForm?.markAsDirty();
  }

  private quantityChanged(row: PurchaseRowDTO) {
    this.getSupplierPurchasePrice(row).subscribe(() => {
      this.calculateRowSum(row);
      this.refreshRows();
    });
  }

  private purchasePriceChanged(row: PurchaseRowDTO) {
    this.calculateRowSum(row);
  }

  private discountChanged(row: PurchaseRowDTO) {
    this.calculateRowSum(row);
  }

  public calculateRowSums(refreshRows = false, ignoreSetModified = true) {
    const length = this.rows.getValue().length;
    this.rows.getValue().forEach((x, index) => {
      this.calculateRowSum(x, length == index + 1, ignoreSetModified);
    });
  }

  public calculateRowSum(
    row: PurchaseRowDTO,
    calcTotals = true,
    ignoreSetModified = false
  ) {
    this.calculateDiscount(row);
    row.sumAmountCurrency =
      row.quantity * row.purchasePriceCurrency - row.discountAmountCurrency;
    const am = this.currencyService.getCurrencyAmount(
      row.sumAmountCurrency,
      TermGroup_CurrencyType.TransactionCurrency,
      TermGroup_CurrencyType.BaseCurrency
    );

    if (am) {
      row.sumAmount = am;

      if (!ignoreSetModified) row.isModified = true;

      if (calcTotals) {
        this.calculateTotals();
      }
    }
    // workaround to get totals to update for calculated values
    this.grid.resetRows();
  }

  private calculateDiscount(row: PurchaseRowDTO) {
    row.discountAmountCurrency = 0;
    row.discountPercent = 0;

    if (row.purchasePriceCurrency !== 0) {
      const amountSum: number = row.purchasePriceCurrency * row.quantity;

      if (!row.discountValue) row.discountValue = 0;

      if (row.discountType === SoeInvoiceRowDiscountType.Amount) {
        row.discountAmountCurrency = row.discountValue;
        row.discountPercent =
          amountSum !== 0 ? (row.discountValue / amountSum) * 100 : 0;
      } else if (row.discountType === SoeInvoiceRowDiscountType.Percent) {
        row.discountPercent = row.discountValue;
        row.discountAmountCurrency = (amountSum * row.discountPercent) / 100;
      }
    }
  }

  selectionChanged(selectedRows: PurchaseRowDTO[]): void {
    this.isDisabledFeatureButton.set(
      selectedRows.length <= 0 || this.originStatus === SoeOriginStatus.Origin
    );

    this.disableRowDelete.set(selectedRows.length === 0);
  }

  recalculateTotalAmountExVatCurrency(beforeValue: number, afterValue: number) {
    const diff = afterValue - beforeValue;
    this.form?.patchValue({
      totalAmountExVatCurrency:
        this.form?.value.totalAmountExVatCurrency + diff,
    });
    this.getTotal.emit(this.form?.value.totalAmountExVatCurrency);
  }

  private deleteRow(row: PurchaseRowDTO) {
    const mb = this.messageboxService.warning(
      this.translate.instant('core.warning'),
      this.translate.instant('core.deleterowwarning')
    );

    mb.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
      if (response.result) {
        this.confirmedDeleteRow(row);
        this.purchaseRowsUpdated(false, true);
        this.showAllRows(this.showAllRowsValue());
      }
    });
  }

  private confirmedDeleteRow(row: PurchaseRowDTO) {
    if (row.purchaseRowId && row.purchaseRowId > 0) {
      //saved row & child rows deleted
      this.rowDeleted.emit(row);
      const childRows = this.rows
        .getValue()
        .filter(x => x.parentRowId === row.purchaseRowId);
      for (const childRow of childRows) {
        this.grid.deleteRow(childRow);
        this.rowDeleted.emit(childRow);
      }
      this.grid.deleteRow(row);
    } else {
      this.grid.deleteRow(row);

      // const childRows = this.rows
      //   .getValue()
      //   .filter(x => x.parentRowId === row.tempRowId);
      // for (const childRow of childRows) {
      //   this.grid.deleteRow(childRow);
      // }
    }
    this.rows.next(this.grid.agGrid.rowData ?? []);
    this.parentForm?.markAsDirty();
  }

  private deleteRows() {
    const mb = this.messageboxService.warning(
      this.translate.instant('core.warning'),
      this.translate.instant('core.deleterowwarning')
    );

    mb.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
      if (response.result) {
        const selectedRows = this.grid.getSelectedRows();
        if (selectedRows && selectedRows.length > 0) {
          selectedRows.forEach((row: PurchaseRowDTO) => {
            this.confirmedDeleteRow(row);
          });

          this.purchaseRowsUpdated(false, true);
          this.showAllRows(this.showAllRowsValue());
        }
      }
    });
  }

  loadProducts() {
    return this.performLoad.load$(
      this.productService.getProductsSmall().pipe(
        tap(data => {
          this.products = data;
        })
      )
    );
  }

  loadSupplierProductId() {
    return this.performSupplierProduct.load$(
      this.purchaseProductsService
        .getSupplierProductsSmall(
          this.parentForm.getAllValues({ includeDisabled: true }).supplierId
            ? this.parentForm.getAllValues({ includeDisabled: true }).supplierId
            : 0
        )
        .pipe(
          tap(data => {
            this.supplierProducts = data;
          })
        )
    );
  }

  loadProductUnits() {
    return this.productUnitService.getGrid(undefined, { useCache: false }).pipe(
      tap(x => {
        this.purchaseUnit = x;
      })
    );
  }

  edit(row: PurchaseRowDTO) {
    const dialogData = new TextBlockDialogData();
    dialogData.title = '';
    dialogData.size = 'lg';
    dialogData.text = row.text;
    dialogData.langId = TermGroup_Languages.Swedish;
    dialogData.mode = SimpleTextEditorDialogMode.EditInvoiceRowText;
    dialogData.type = TextBlockType.TextBlockEntity;
    dialogData.entity = SoeEntityType.CustomerInvoice;
    dialogData.editPermission = this.readonly == false;
    this.dialogServiceV2
      .open(TextBlockDialogComponent, dialogData)
      .afterClosed()
      .subscribe(value => {
        if (value) {
          row.text = value;
          row.isModified = true;
        }
        this.refreshRows();
        this.parentForm?.markAsDirty();
      });
  }

  openOrderSearch(row: IPurchaseRowDTO) {
    if (row.orderNr == undefined) row.orderNr = '';
    const dialogData = new SelectInvoiceDialogDTO();
    dialogData.title = this.translate.instant('core.search');
    dialogData.size = 'lg';
    dialogData.originType = SoeOriginType.Order;
    if (!this.invoiceData) {
      this.invoiceData = new SearchCustomerInvoiceDTO();
    }

    this.invoiceData.number = row.orderNr;
    this.invoiceData.projectId = undefined;
    this.invoiceData.isNew = false;
    this.invoiceData.originType = SoeOriginType.Order;
    if (this.invoiceData) dialogData.invoiceValue = this.invoiceData;
    this.dialogServiceV2
      .open(SelectCustomerInvoiceDialogComponent, dialogData)
      .afterClosed()
      .subscribe(value => {
        if (value) {
          row.orderNr = value.number;
          this.parentForm?.markAsDirty();
          // if (row.text) row.text = value.number;
        }
        this.refreshRows();
      });
  }
  openAcknowledgeDeliveryDate() {
    const mb = this.messageboxService.show(
      this.translate.instant('common.choosedate'),
      ' ',
      {
        showInputDate: true,
        inputDateLabel: 'common.date',
        inputDateValue: new Date(),
        buttons: 'okCancel',
      }
    );

    mb.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
      if (response.dateValue) {
        const selectedRows = this.grid.getSelectedRows();
        if (selectedRows && selectedRows.length > 0) {
          selectedRows.forEach((row: PurchaseRowDTO) => {
            row.accDeliveryDate = response.dateValue;
            if (row.purchaseRowId) {
              this.parentForm?.markAsDirty();
            }
          });

          this.purchaseRowsUpdated(false);
        }
      }
    });
  }

  private purchaseRowsUpdated(updateTextProperties: boolean, renumber = false) {
    if (this.rows) {
      const productsToLoad: number[] = [];
      this.rows.getValue().map(r => {
        if (r.productId && !productsToLoad.includes(r.productId))
          productsToLoad.push(r.productId);

        if (r.purchaseRowId) r.tempRowId = r.purchaseRowId;
        else if (!r.tempRowId) {
          r.tempRowId = this.tempRowIdCounter;
          this.tempRowIdCounter += 1;
        }

        if (!r.discountValue) {
          r.discountValue = r.discountPercent
            ? r.discountPercent
            : r.discountAmountCurrency;
        }
        if (!r.stocksForProduct) {
          r.stocksForProduct = [];
        }
      });

      if (updateTextProperties) {
        this.setDiscountTypeTexts();
      }

      this.calculateTotals();

      if (renumber) this.reNumberRows();
    }
  }

  private setDiscountTypeTexts() {
    this.rows.value.forEach(
      row => (row.discountTypeText = this.getDiscountTypeText(row.discountType))
    );
    this.parentForm?.markAsDirty();
  }

  private getDiscountTypeText(type: number): string {
    const dt = this.discountTypeDict.find(x => x.id === type);
    return dt ? dt.name : '';
  }

  openIntrastat() {
    const selectedRows = this.grid
      .getSelectedRows()
      .filter(r => r.type === PurchaseRowType.PurchaseRow && !r.isModified);
    let productIds: number[] = [];
    productIds = selectedRows
      .filter(p => p.productId !== undefined)
      .map(p => p.productId!);

    this.productService.getProductRowsProducts(productIds).pipe(
      tap(x => {
        const tempRows: IntrastatTransactionDTO[] = [];
        selectedRows.forEach(r => {
          // Get product to check vattype
          let isService = false;
          const product = x.find(p => p.productId === r.productId);
          if (product) {
            if (product.vatType === TermGroup_InvoiceProductVatType.Service)
              isService = true;
          }

          if (!isService) {
            const dto = new IntrastatTransactionDTO();
            dto.rowNr = r.rowNr;
            dto.customerInvoiceRowId = r.purchaseRowId;
            dto.intrastatTransactionId = r.intrastatTransactionId
              ? r.intrastatTransactionId
              : 0;
            dto.intrastatCodeId = r.intrastatCodeId ? r.intrastatCodeId : 0;
            dto.sysCountryId = r.sysCountryId;
            dto.originId = this.parentForm.getAllValues({
              includeDisabled: true,
            })?.supplierId
              ? this.parentForm.getAllValues({ includeDisabled: true })
                  .supplierId
              : 0;
            dto.productName = r.productName;
            dto.productNr = r.productNr;
            dto.productUnitId = r.purchaseUnitId;
            dto.productUnitCode = r.purchaseProductUnitCode;
            dto.quantity = r.quantity;
            dto.state = SoeEntityState.Active;

            if (!dto.intrastatCodeId && this.intrastatCodeId)
              dto.intrastatCodeId = this.intrastatCodeId;

            if (!dto.sysCountryId && this.sysCountryId)
              dto.sysCountryId = this.sysCountryId;

            tempRows.push(dto);
          }
        });
      })
    );

    const dialogData = new ChangeIntrastatCodeDialogData();
    dialogData.size = 'lg';
    this.dialogServiceV2
      .open(ChangeIntrastatCodeComponent, dialogData)
      .afterClosed()
      .subscribe(res => {});
  }

  private isAccDateEditable(row: PurchaseRowDTO | undefined): boolean {
    return !row?.isLocked && this.originStatus !== SoeOriginStatus.Origin;
  }

  private isColEditable(row: PurchaseRowDTO | undefined): boolean {
    return !row?.isLocked;
  }

  private supplierChanged(updateRows?: boolean) {
    if (updateRows) {
      this.rows.value.forEach(r => {
        this.productChanged(r);
      });
    }

    this.loadSupplierProductId().subscribe();
  }

  private productChanged(
    row: PurchaseRowDTO,
    supplierProductChange: boolean = false
  ) {
    row.supplierProductNr = '';
    row.supplierProductId = 0;
    row.purchasePriceCurrency = 0;

    if (!row.productId) {
      this.setProductValues(row, undefined, supplierProductChange);
      return;
    }

    const productSmall: IProductSmallDTO | undefined = this.findProduct(row);
    forkJoin([
      this.findSupplierProduct(
        productSmall?.productId,
        this.parentForm.getAllValues({ includeDisabled: true }).supplierId
      ),
      this.findFullProduct(
        productSmall?.productId ? productSmall?.productId : 0
      ),
    ])
      .pipe(
        tap(([supplierProduct, fullProduct]) => {
          if (supplierProduct) {
            row.supplierProductNr = supplierProduct.supplierProductNr;
            row.supplierProductId = supplierProduct.supplierProductId;
            row.text = supplierProduct.supplierProductName;
            row.sysCountryId = supplierProduct.sysCountryId;
            this.getSupplierPurchasePrice(row).subscribe();
          } else {
            row.text = productSmall ? productSmall.name : '';
          }
          this.setProductValues(row, fullProduct, supplierProductChange);
          this.setStocksForProduct(row, true);
        })
      )
      .subscribe();
  }

  private setStocksForProduct(row: PurchaseRowDTO, productChanged = false) {
    if (!this.useStock()) {
      row.stocksForProduct = [];
      return;
    }

    if (!row.stocksForProduct || productChanged) {
      row.stocksForProduct = [];
    }

    // get stocks for product
    if (row.productId && (productChanged || row.stocksForProduct.length < 2)) {
      //add empty row
      const arr: SmallGenericType[] = [];

      arr.push(new SmallGenericType(0, ' '));

      row.stocksForProduct.forEach(s => {
        if (s.id !== 0) {
          arr.push(new SmallGenericType(s.id, s.name));
        }
      });

      this.performLoad
        .load$(
          this.stockService.getStocksByProduct(row.productId).pipe(
            tap(stockList => {
              stockList.forEach(stock => {
                if (stock.stockId !== 0) {
                  arr.push(
                    new SmallGenericType(
                      stock.stockId,
                      stock.code + ' ' + stock.saldo
                    )
                  );
                }
              });
              row.stocksForProduct = arr;
              //set default stock
              if (productChanged) {
                const defaultStock = stockList.find(
                  s => s.stockId === this.defaultStockId
                );
                if (defaultStock) {
                  row.stockId = defaultStock.stockId;
                  row.stockCode = defaultStock.code;
                } else {
                  row.stockId = 0;
                  row.stockCode = '';
                }
              }
              this.refreshRows();
            })
          )
        )
        .subscribe();
    }
  }

  private findFullProduct(
    productId: number
  ): Observable<ProductRowsProductDTO | undefined> {
    const product = this.productList.find(p => p.productId === productId);
    if (product || !productId) {
      return of(product);
    } else {
      return this.productService.getProductForProductRows(productId).pipe(
        tap(x => {
          if (x) this.productList.push(x);
        })
      );
    }
  }

  private getSupplierPurchasePrice(row: PurchaseRowDTO): Observable<unknown> {
    if (row.supplierProductId) {
      return this.performLoad.load$(
        this.billingService
          .getSupplierProductPrice(
            row.supplierProductId,
            DateUtil.format(
              this.parentForm.purchaseDate.value,
              `yyyyMMdd'T'HHmmss`
            ),
            row.quantity ? row.quantity : 1,
            this.currencyService.getCurrencyId()
          )
          .pipe(
            tap(priceDTO => {
              row.purchasePriceCurrency = priceDTO ? priceDTO.price : 0;
            })
          )
      );
    }
    return of(false);
  }

  private setProductValues(
    row: PurchaseRowDTO,
    product?: ProductRowsProductDTO,
    supplierProductChange: boolean = false
  ) {
    if (row.type === PurchaseRowType.TextRow) return;

    // Set Product values
    if (!supplierProductChange) {
      row.productId = product ? product.productId : 0;
      row.productNr = product ? product.number : '';
      row.productName = product ? product.name : '';
    }
    // if (!row.purchaseUnitId) {
    //   row.purchaseUnitId =
    //     product && product.productUnitId
    //       ? product.productUnitId
    //       : this.defaultProductUnitId;
    // }

    // if (!row.sysCountryId)
    //   row.sysCountryId = product ? product.sysCountryId : undefined;

    // if (!row.intrastatCodeId)
    //   row.intrastatCodeId = product ? product.intrastatCodeId : undefined;
    // Product has changed, set text from product
    //if (product) {
    // Set ProductUnit values
    row.purchaseUnitId = product?.productUnitId ?? this.defaultProductUnitId;
    row.purchaseProductUnitCode =
      product && product.productUnitId
        ? product.productUnitCode
        : this.getProductUnitCode(this.defaultProductUnitId);
    row.sysCountryId = product ? product.sysCountryId : undefined;
    row.intrastatCodeId = product ? product.intrastatCodeId : undefined;
    //}
    if (!row.purchaseProductUnitCode && row.purchaseUnitId) {
      row.purchaseProductUnitCode = this.getProductUnitCode(row.purchaseUnitId);
    }

    if (!row.supplierProductId && product) {
      this.setPurchasePricefromProduct(row, product);
    }

    if (
      product?.showDescrAsTextRowOnPurchase &&
      product.description &&
      !row.purchaseRowId
    ) {
      let textRow: PurchaseRowDTO | undefined = this.rows.value.find(
        r =>
          r.parentRowId === row.tempRowId && r.type === PurchaseRowType.TextRow
      );
      if (!textRow) {
        // Add new TextRow
        this.addRow(PurchaseRowType.TextRow);
        textRow = this.rows.getValue()[this.rows.getValue().length - 1];

        if (textRow) {
          this.multiplyRowNr();
          textRow.parentRowId = row.tempRowId;
          textRow.rowNr = row.rowNr + 1;
          textRow.text = product.description;
          textRow.isModified = true;
          this.reNumberRows();
          this.refreshRows();
        }
      }
    }
  }

  private getProductUnitCode(productUnitId: number): string {
    const unit = this.purchaseUnit.find(x => x.productUnitId === productUnitId);
    return unit ? unit.name : '';
  }

  private setPurchasePricefromProduct(
    row: PurchaseRowDTO,
    product: ProductRowsProductDTO
  ) {
    if (product.purchasePrice && row.purchasePrice !== product.purchasePrice) {
      row.purchasePrice = product.purchasePrice;
      row.purchasePriceCurrency = this.currencyService.getCurrencyAmount(
        row.purchasePrice,
        TermGroup_CurrencyType.BaseCurrency,
        TermGroup_CurrencyType.TransactionCurrency
      );
      this.refreshRows();
    }
  }

  private multiplyRowNr() {
    const visibleRows = this.rows.value
      .filter(r => r.state === SoeEntityState.Active)
      .sort((a, b) => a.rowNr - b.rowNr);

    visibleRows.forEach(x => {
      x.rowNr *= 100;
    });
  }

  private findProduct(row: PurchaseRowDTO): IProductSmallDTO | undefined {
    return row.productId
      ? this.products?.find(p => p.productId === row.productId)
      : undefined;
  }

  private supplierProductChanged(row: PurchaseRowDTO) {
    if (!row.supplierProductId) {
      this.clearProductInfo(row);
      return;
    }
    row.text =
      this.supplierProducts.find(
        s => s.supplierProductId == row.supplierProductId
      )?.name ?? '';

    const supplierProductSmall = this.findSupplierProductSmall(row);
    if (supplierProductSmall) {
      this.findSupplierProductById(supplierProductSmall.supplierProductId)
        .pipe(
          tap(sp => {
            if (sp) {
              const text = row.text;
              this.findFullProduct(sp.productId)
                .pipe(
                  tap(p => {
                    row.supplierProductId = sp.supplierProductId;
                    if (p) {
                      row.productId = p.productId;
                      row.productNr = p.number;
                      row.productName = p.name;
                      this.productChanged(row, true);
                    } else {
                      this.clearProductInfo(row);
                      this.getSupplierPurchasePrice(row).subscribe(sub => {
                        this.refreshRows();
                      });
                    }
                  })
                )
                .subscribe(() => {
                  row.text =
                    text === row.text ? sp.supplierProductName : row.text;
                });
            }
          })
        )
        .subscribe();
    } else {
      this.refreshRows();
    }
  }

  private findSupplierProductSmall(
    row: PurchaseRowDTO
  ): ISupplierProductSmallDTO | null | undefined {
    return row.supplierProductId
      ? this.supplierProducts.find(
          p => p.supplierProductId === row.supplierProductId
        )
      : null;
  }

  private findSupplierProductById(
    supplierProductId: number
  ): Observable<SupplierProductDTO | undefined> {
    const product = this.supplierProductList.find(
      p => p.supplierProductId === supplierProductId
    );
    if (product || !supplierProductId) {
      return of(product);
    } else {
      return this.purchaseProductsService.get(supplierProductId).pipe(
        tap(supplierProduct => {
          if (supplierProduct) {
            this.supplierProductList.push(supplierProduct);
          }
        })
      );
      // .subscribe(supplierProduct => {
      //   return of(supplierProduct);
      // });
    }
  }

  private findSupplierProduct(
    productId?: number,
    supplierId?: number
  ): Observable<SupplierProductDTO | undefined> {
    if (productId && supplierId) {
      const product = this.supplierProductList.find(
        p => p.productId === productId && p.supplierId == supplierId
      );
      if (product || !productId) {
        return of(product);
      } else {
        this.purchaseProductsService
          .getSupplierProductByInvoiceProduct(productId, supplierId)
          .pipe(
            tap(supplierProduct => {
              if (supplierProduct) {
                this.supplierProductList.push(supplierProduct);
              }
              return supplierProduct;
            })
          )
          .subscribe();
      }
    }
    return of(undefined);
  }

  public clearProductInfo(row: PurchaseRowDTO) {
    row.productNr = '';
    row.productId = 0;
    row.purchasePriceCurrency = 0;
    this.setProductValues(row, undefined);
  }
}
