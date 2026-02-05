import {
  Component,
  OnDestroy,
  OnInit,
  computed,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { CategoryItem } from '@shared/components/categories/categories.model';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { TermCollection } from '@shared/localization/term-types';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  ActionResultSave,
  CompTermsRecordType,
  CompanySettingType,
  ExternalProductType,
  Feature,
  ProductAccountType,
  SoeEntityState,
  SoeEntityType,
  SoeTimeCodeType,
  TermGroup,
  TermGroup_Country,
  TermGroup_InvoiceProductCalculationType,
  TermGroup_InvoiceProductVatType,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IAccountDimSmallDTO,
  IAccountingSettingsRowDTO,
  ICompTermDTO,
  IProductGroupDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressOptions } from '@shared/services/progress';
import { BrowserUtil } from '@shared/util/browser-util';
import { SettingsUtil } from '@shared/util/settings-util';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import {
  Observable,
  Subject,
  distinctUntilChanged,
  of,
  takeUntil,
  tap,
  mergeMap,
  map,
} from 'rxjs';
import { InvoiceProductForm } from '../../models/invoice-product-form.model';
import { InvoiceProductDTO } from '../../models/invoice-product.model';
import {
  PriorityAccountRow,
  ProductUnitConvertDTO,
} from '../../models/product.model';
import { ProductService } from '../../services/product.service';
import { AccountingPriorityComponent } from './accounting-priority/accounting-priority.component';
import { ProductUnitConvertGridComponent } from './product-unit-convert-grid/product-unit-convert-grid.component';
import { ProductsPriceListsComponent } from './products-price-lists/products-price-lists.component';
import { StockDTO } from '../../models/stock.model';
import { ProductStocksComponent } from './product-stocks/product-stocks.component';
import { ExtraFieldsService } from '@shared/features/extra-fields/services/extra-fields.service';
import { LanguageTranslationsComponent } from '@shared/features/language-translations/language-translations.component';
import { IPriceListTypeGridDTO } from '@shared/models/generated-interfaces/PriceListTypeDTOs';
import { CrudActionTypeEnum } from '@shared/enums';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { CopyProductDialogData } from '../../models/copy-product-dialog-data.models';
import { CopyProductDialogComponent } from './copy-product-dialog/copy-product-dialog.component';
import { LanguageTranslationsService } from '@shared/features/language-translations/services/language-translations.service';
import {
  IExtraFieldGridDTO,
  IExtraFieldRecordDTO,
} from '@shared/models/generated-interfaces/ExtraFieldDTO';
import { PriceListDTO } from '@features/billing/models/pricelist.model';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import { ResponseUtil } from '@shared/util/response-util';

@Component({
  selector: 'soe-products-edit',
  templateUrl: './products-edit.component.html',
  styleUrls: ['./products-edit.component.scss'],
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ProductsEditComponent
  extends EditBaseDirective<
    InvoiceProductDTO,
    ProductService,
    InvoiceProductForm
  >
  implements OnInit, OnDestroy
{
  readonly service = inject(ProductService);
  private readonly coreService = inject(CoreService);
  private readonly extraFieldService = inject(ExtraFieldsService);
  private readonly dialogService = inject(DialogService);
  private readonly translationsService = inject(LanguageTranslationsService);

  //#region Properties
  protected showCalculatedCost = signal(false);
  protected showGuaranteePercentage = signal(false);
  protected commodityCodeTooltip = signal<string>('');
  protected productPriceLists: PriceListDTO[] = [];
  protected productAccountPriorityRows: PriorityAccountRow[] = [];
  protected productUnitConverts: ProductUnitConvertDTO[] = [];
  protected stocksForProduct: StockDTO[] = [];
  private extraFieldRecords: IExtraFieldRecordDTO[] = [];
  protected compTermRecordType = CompTermsRecordType.ProductName;
  protected compTermRows: ICompTermDTO[] = [];
  private isNotExternal = signal<boolean>(true);
  private hasUnitConversionsToSave = false;

  entityType = SoeEntityType.InvoiceProduct;

  private _destroy$ = new Subject<void>();
  //#endregion

  //#region Lookup Data
  protected productAccountSettingTypes: SmallGenericType[] = [];
  protected stockAccountSettingTypes: SmallGenericType[] = [];
  protected productBaseAccounts: SmallGenericType[] = [];
  protected stockBaseAccounts: SmallGenericType[] = [];
  protected vatTypes: SmallGenericType[] = [];
  protected productUnits: SmallGenericType[] = [];
  protected productGroups: IProductGroupDTO[] = [];
  protected calculationTypes: SmallGenericType[] = [];
  protected grossMarginCalculationTypes: SmallGenericType[] = [];
  protected materialCodes: SmallGenericType[] = [];
  protected vatCodes: SmallGenericType[] = [];
  protected householdDeductionTypes: SmallGenericType[] = [];
  protected accountingPrios: SmallGenericType[] = [];
  protected stocks: SmallGenericType[] = [];
  protected commodityCodes: SmallGenericType[] = [];
  protected countries: SmallGenericType[] = [];
  private extraFields = signal<IExtraFieldGridDTO[]>([]);
  private allPriceLists = signal<IPriceListTypeGridDTO[]>([]);
  //#endregion

  //#region company settings
  private defaultVatType!: TermGroup_InvoiceProductVatType;
  private defaultProductUnitId!: number;
  private defaultTimeCodeId!: number;
  private defaultVatCodeId!: number;
  private defaultHouseholdDeductionType!: number;
  protected defaultStockId!: number;
  protected useProductUnitConvert = signal(false);
  private copyProductPrices = false;
  private copyProductAccounts = false;
  private copyProductStock = false;
  public showStockAccordion = false;
  //#endregion

  //#region Permissions
  protected modifyCategoryPermission = signal(false);
  protected showPurchasePrice = signal(false);
  protected stockHandling = signal(false);
  protected useStockPermission = signal(false);
  protected hasExtraFieldPermission = signal(false);
  protected hasCommodityCodesPermission = signal(false);
  protected hasPurchaseProductsPermission = signal(false);
  protected showFixedPrice = signal(false);
  protected hideStocks = computed((): boolean => {
    return !(this.useStockPermission() || this.stockHandling());
  });
  protected showExtraFields = computed(() => {
    return this.hasExtraFieldPermission() && this.extraFields().length > 0;
  });
  //#endregion

  //#region Rendering Expanders
  private accountSettingsExpanderRendered: boolean = false;
  private stockExpanderRendered: boolean = false;
  protected supplierProductExpanderRendered = signal<boolean>(false);
  protected extraFieldsExpanderRendered = signal<boolean>(false);
  protected translationsExpanderRendered = signal<boolean>(false);
  //#endregion

  //#region Child GRIDS
  private productPriceGrid = viewChild(ProductsPriceListsComponent);
  private accountingPriorityGrid = viewChild(AccountingPriorityComponent);
  private unitConvertGrid = viewChild(ProductUnitConvertGridComponent);
  private stocksForProductGrid = viewChild(ProductStocksComponent);
  private compTermGrid = viewChild(LanguageTranslationsComponent);
  //#endregion

  ngOnInit(): void {
    this.startFlow(Feature.Billing_Product_Products_Edit, {
      additionalModifyPermissions: [
        Feature.Common_Categories_Product_Edit,
        Feature.Billing_Product_Products_FixedPrice,
        Feature.Billing_Product_Products_ShowPurchasePrice,
        Feature.Billing_Stock,
        Feature.Billing_Order_Orders_Edit_ProductRows_Stock,
        Feature.Billing_Product_Products_ExtraFields,
        Feature.Economy_Intrastat,
        Feature.Billing_Purchase_Products,
      ],
      lookups: [
        this.loadVatTypes(),
        this.loadVatCodes(),
        this.loadProductUnits(),
        this.loadMaterialCodes(),
        this.loadProductGroups(),
        this.loadHouseholdDeductionTypes(),
        this.loadCalculationTypes(),
        this.loadCommodityCodes(),
        this.loadCountries(),
        this.loadAccountingPriority(),
        this.loadExtraFields(),
        this.loadPriceLists(),
        this.loadProductUnitConverts(),
        this.loadGrossMarginCalculationTypes(),
      ],
    });

    this.bindFormValueChanges();
  }

  override onPermissionsLoaded(): void {
    super.onPermissionsLoaded();
    this.copyDisabled.set(
      !this.flowHandler.modifyPermission() || !this.form?.getIdControl()?.value
    );
    this.modifyCategoryPermission.set(
      this.flowHandler.hasModifyAccess(Feature.Common_Categories_Product_Edit)
    );
    this.showFixedPrice.set(
      this.flowHandler.hasModifyAccess(
        Feature.Billing_Product_Products_FixedPrice
      )
    );
    this.showPurchasePrice.set(
      this.flowHandler.hasModifyAccess(
        Feature.Billing_Product_Products_ShowPurchasePrice
      )
    );
    this.stockHandling.set(
      this.flowHandler.hasModifyAccess(Feature.Billing_Stock)
    );
    this.useStockPermission.set(
      this.flowHandler.hasModifyAccess(
        Feature.Billing_Order_Orders_Edit_ProductRows_Stock
      )
    );
    this.hasExtraFieldPermission.set(
      this.flowHandler.hasModifyAccess(
        Feature.Billing_Product_Products_ExtraFields
      )
    );
    this.hasCommodityCodesPermission.set(
      this.flowHandler.hasModifyAccess(Feature.Economy_Intrastat)
    );
    this.hasPurchaseProductsPermission.set(
      this.flowHandler.hasModifyAccess(Feature.Billing_Purchase_Products)
    );
  }

  override onFinished(): void {
    if (this.form?.isNew && !this.form?.isCopy) {
      this.form?.patchValue({
        vatType: this.defaultVatType,
        timeCodeId: this.defaultTimeCodeId,
        productUnitId: this.defaultProductUnitId,
        vatCodeId: this.defaultVatCodeId,
        householdDeductionType: this.defaultHouseholdDeductionType,
        accountingPrio: '1=0,2=0,3=0,4=0,5=0,6=0',
      });

      this.loadAccountPriorityRows();
    }

    if (this.form?.isCopy) {
      this.form?.number.reset();
      if (this.form?.copyPrice.value) {
        this.productPriceLists = this.form?.priceLists.value as PriceListDTO[];
      }

      if (!this.form?.copyAccounts.value) {
        this.form?.customPatchAccountSettings([], true);
        this.form?.customPatchAccountSettings([], false);
      }

      if (this.form?.copyStock.value) {
        this.stocksForProduct = this.form?.stocks.value as StockDTO[];
        this.stocksForProduct.forEach(s => {
          s.stockProductId = 0;
          s.saldo = 0;
          s.avgPrice = 0;
          s.stockShelfId = 0;
          s.stockShelfName = '';
        });
      }

      this.extraFieldRecords = this.form?.extraFields
        .value as IExtraFieldRecordDTO[];
    }

    this.setProductListPriceData();

    this.form?.sysWholesellerName.disable();
    this.form?.externalPrice.disable();
    this.form?.priceListName.disable();
  }

  private bindFormValueChanges(): void {
    this.form?.vatType.valueChanges
      .pipe(distinctUntilChanged(), takeUntil(this._destroy$))
      .subscribe(vatType => {
        this.showCalculatedCost.set(
          vatType === TermGroup_InvoiceProductVatType.Service
        );
      });
    this.form?.calculationType.valueChanges
      .pipe(distinctUntilChanged(), takeUntil(this._destroy$))
      .subscribe(calcType => {
        this.showGuaranteePercentage.set(
          calcType === TermGroup_InvoiceProductCalculationType.Lift
        );
      });
    this.form?.weight.valueChanges
      .pipe(distinctUntilChanged(), takeUntil(this._destroy$))
      .subscribe(weight => {
        if (weight && weight < 0) {
          this.messageboxService.error(
            'core.error',
            this.terms['billing.products.products.productweightinvalidmessage']
          );
        }
      });

    this.form?.isExternal.valueChanges
      .pipe(distinctUntilChanged(), takeUntil(this._destroy$))
      .subscribe(isExternal => {
        this.isNotExternal.set(!isExternal);
        if (isExternal) this.form?.number.disable();
        else this.form?.number.enable();

        if (!this.form?.isNew && isExternal) this.form?.purchasePrice.disable();
        else this.form?.purchasePrice.enable();
      });
  }

  override createEditToolbar(): void {
    super.createEditToolbar({
      copyOption: {
        onAction: () => this.copyProduct(),
      },
    });

    const showExternalProduct =
      this.form?.productId.value > 0 &&
      (SoeConfigUtil.sysCountryId === TermGroup_Country.FI ||
        this.form?.sysProductType.value === ExternalProductType.Plumbing);

    if (showExternalProduct) {
      this.toolbarService.createToolbarButton('openProduct', {
        iconName: signal('arrow-up-right-from-square'),
        caption: signal('common.searchinvoiceproduct.showexternalproductinfo'),
        tooltip: signal('common.searchinvoiceproduct.showexternalproductinfo'),
        hidden: this.isNotExternal,
        onAction: this.openProductInfo.bind(this),
      });
    }

    this.toolbarService.createToolbarButton('convertUserProduct', {
      iconName: signal('sync'),
      caption: signal('billing.products.products.converttouserproduct'),
      tooltip: signal(
        'billing.products.products.converttouserproductbuttontooltip'
      ),
      hidden: this.isNotExternal,
      onAction: this.convertToUserProduct.bind(this),
    });
  }

  //#region Lookups
  override loadTerms(): Observable<TermCollection> {
    return super
      .loadTerms([
        'billing.products.products.productnotdeletedmessage',
        'billing.products.products.producthasinvoicerowsmessage',
        'billing.products.products.producthasinterestrowsmessage',
        'billing.products.products.producthasreminderrowsmessage',
        'billing.products.products.failedsavemessage',
        'billing.products.products.productexistsmessage',
        'billing.products.products.productnotsavedmessage',
        'billing.products.products.productinusemessage',
        'billing.products.products.productpricelistnotsavedmessage',
        'billing.products.products.productcategoriesnotsavedmessage',
        'billing.products.products.productaccountsnotsavedmessage',
        'billing.products.products.translationsnotsavedmessage',
        'billing.products.product.fromstock',
        'billing.products.product.tostock',
        'billing.products.product.amounttomove',
        'billing.products.product.convertedtouserproduct',
        'billing.products.products.productweightinvalidmessage',
        'core.error',
        'core.aggrid.totals.filtered',
        'core.aggrid.totals.total',
        'core.aggrid.totals.selected',
        'billing.products.product',
        'billing.products.products.accountingsettingtype.receivables',
        'billing.products.products.accountingsettingtype.sales',
        'billing.products.products.accountingsettingtype.vat',
        'billing.products.products.accountingsettingtype.salesnovat',
        'billing.products.products.accountingsettingtype.salescontractor',
        'billing.products.products.stockaccountsettingtype.stockin',
        'billing.products.products.stockaccountsettingtype.stockinchange',
        'billing.products.products.stockaccountsettingtype.stockout',
        'billing.products.products.stockaccountsettingtype.stockoutchange',
        'billing.products.products.stockaccountsettingtype.stockinv',
        'billing.products.products.stockaccountsettingtype.stockinvchange',
        'billing.products.products.stockaccountsettingtype.stockloss',
        'billing.products.products.stockaccountsettingtype.stocklosschange',
        'billing.products.product.isstockproduct',
        'billing.products.product.stocks',
        'common.customer.customer.rot.socialsecnr',
        'common.customer.customer.rot.name',
        'common.customer.customer.rot.property',
        'common.customer.customer.rot.apartmentnr',
        'common.customer.customer.rot.cooperativeorgnr',
        'core.edit',
        'core.delete',
        'common.type',
        'common.productnr',
        'common.name',
        'common.customer.customer.marginalincome',
        'common.customer.customer.marginalincomeratio',
      ])
      .pipe(
        tap(() => {
          this.loadSettingTypes();
        })
      );
  }

  override loadCompanySettings(): Observable<void> {
    const settingTypes: CompanySettingType[] = [];

    settingTypes.push(CompanySettingType.BillingDefaultInvoiceProductVatType);
    settingTypes.push(CompanySettingType.BillingDefaultInvoiceProductUnit);
    settingTypes.push(CompanySettingType.BillingStandardMaterialCode);
    settingTypes.push(CompanySettingType.BillingDefaultVatCode);
    settingTypes.push(CompanySettingType.BillingDefaultHouseholdDeductionType);
    settingTypes.push(CompanySettingType.BillingDefaultStock);

    settingTypes.push(CompanySettingType.AccountCustomerClaim);
    settingTypes.push(CompanySettingType.AccountCustomerSalesVat);
    settingTypes.push(CompanySettingType.AccountCommonVatPayable1);
    settingTypes.push(CompanySettingType.AccountCustomerSalesNoVat);
    settingTypes.push(CompanySettingType.AccountCommonReverseVatSales);

    settingTypes.push(CompanySettingType.AccountStockIn);
    settingTypes.push(CompanySettingType.AccountStockInChange);
    settingTypes.push(CompanySettingType.AccountStockOut);
    settingTypes.push(CompanySettingType.AccountStockOutChange);
    settingTypes.push(CompanySettingType.AccountStockInventory);
    settingTypes.push(CompanySettingType.AccountStockInventoryChange);
    settingTypes.push(CompanySettingType.AccountStockLoss);
    settingTypes.push(CompanySettingType.AccountStockLossChange);
    settingTypes.push(CompanySettingType.BillingUseProductUnitConvert);

    settingTypes.push(CompanySettingType.BillingCopyProductPrices);
    settingTypes.push(CompanySettingType.BillingCopyProductAccounts);
    settingTypes.push(CompanySettingType.BillingCopyProductStock);
    settingTypes.push(
      CompanySettingType.AccountingCreateVouchersForStockTransactions
    );

    return this.performLoadData.load$(
      this.coreService.getCompanySettings(settingTypes).pipe(
        tap((setting: any): void => {
          this.defaultVatType = SettingsUtil.getIntCompanySetting(
            setting,
            CompanySettingType.BillingDefaultInvoiceProductVatType,
            this.defaultVatType
          );
          this.defaultProductUnitId = SettingsUtil.getIntCompanySetting(
            setting,
            CompanySettingType.BillingDefaultInvoiceProductUnit,
            this.defaultProductUnitId
          );
          this.defaultTimeCodeId = SettingsUtil.getIntCompanySetting(
            setting,
            CompanySettingType.BillingStandardMaterialCode,
            this.defaultTimeCodeId
          );
          this.defaultVatCodeId = SettingsUtil.getIntCompanySetting(
            setting,
            CompanySettingType.BillingDefaultVatCode,
            this.defaultVatCodeId
          );
          this.defaultHouseholdDeductionType =
            SettingsUtil.getIntCompanySetting(
              setting,
              CompanySettingType.BillingDefaultHouseholdDeductionType,
              this.defaultHouseholdDeductionType
            );
          this.defaultStockId = SettingsUtil.getIntCompanySetting(
            setting,
            CompanySettingType.BillingDefaultStock,
            this.defaultStockId
          );
          this.useProductUnitConvert.set(
            SettingsUtil.getBoolCompanySetting(
              setting,
              CompanySettingType.BillingUseProductUnitConvert,
              this.useProductUnitConvert()
            )
          );
          this.showStockAccordion = SettingsUtil.getBoolCompanySetting(
            setting,
            CompanySettingType.AccountingCreateVouchersForStockTransactions,
            false
          );

          // Base accounts for product
          this.productBaseAccounts = [];
          this.productBaseAccounts.push(
            new SmallGenericType(
              ProductAccountType.Purchase,
              SettingsUtil.getIntCompanySetting(
                setting,
                CompanySettingType.AccountCustomerClaim
              ).toString()
            )
          );
          this.productBaseAccounts.push(
            new SmallGenericType(
              ProductAccountType.Sales,
              SettingsUtil.getIntCompanySetting(
                setting,
                CompanySettingType.AccountCustomerSalesVat
              ).toString()
            )
          );
          this.productBaseAccounts.push(
            new SmallGenericType(
              ProductAccountType.VAT,
              SettingsUtil.getIntCompanySetting(
                setting,
                CompanySettingType.AccountCommonVatPayable1
              ).toString()
            )
          );
          this.productBaseAccounts.push(
            new SmallGenericType(
              ProductAccountType.SalesNoVat,
              SettingsUtil.getIntCompanySetting(
                setting,
                CompanySettingType.AccountCustomerSalesNoVat
              ).toString()
            )
          );
          this.productBaseAccounts.push(
            new SmallGenericType(
              ProductAccountType.SalesContractor,
              SettingsUtil.getIntCompanySetting(
                setting,
                CompanySettingType.AccountCommonReverseVatSales
              ).toString()
            )
          );

          // Base accounts for stock
          this.stockBaseAccounts = [];
          this.stockBaseAccounts.push(
            new SmallGenericType(
              ProductAccountType.StockIn,
              SettingsUtil.getIntCompanySetting(
                setting,
                CompanySettingType.AccountStockIn
              ).toString()
            )
          );
          this.stockBaseAccounts.push(
            new SmallGenericType(
              ProductAccountType.StockInChange,
              SettingsUtil.getIntCompanySetting(
                setting,
                CompanySettingType.AccountStockInChange
              ).toString()
            )
          );
          this.stockBaseAccounts.push(
            new SmallGenericType(
              ProductAccountType.StockOut,
              SettingsUtil.getIntCompanySetting(
                setting,
                CompanySettingType.AccountStockOut
              ).toString()
            )
          );
          this.stockBaseAccounts.push(
            new SmallGenericType(
              ProductAccountType.StockOutChange,
              SettingsUtil.getIntCompanySetting(
                setting,
                CompanySettingType.AccountStockOutChange
              ).toString()
            )
          );
          this.stockBaseAccounts.push(
            new SmallGenericType(
              ProductAccountType.StockInv,
              SettingsUtil.getIntCompanySetting(
                setting,
                CompanySettingType.AccountStockInventory
              ).toString()
            )
          );
          this.stockBaseAccounts.push(
            new SmallGenericType(
              ProductAccountType.StockInvChange,
              SettingsUtil.getIntCompanySetting(
                setting,
                CompanySettingType.AccountStockInventoryChange
              ).toString()
            )
          );
          this.stockBaseAccounts.push(
            new SmallGenericType(
              ProductAccountType.StockLoss,
              SettingsUtil.getIntCompanySetting(
                setting,
                CompanySettingType.AccountStockLoss
              ).toString()
            )
          );
          this.stockBaseAccounts.push(
            new SmallGenericType(
              ProductAccountType.StockLossChange,
              SettingsUtil.getIntCompanySetting(
                setting,
                CompanySettingType.AccountStockLossChange
              ).toString()
            )
          );

          // Settings for copying
          this.copyProductPrices = SettingsUtil.getBoolCompanySetting(
            setting,
            CompanySettingType.BillingCopyProductPrices,
            this.copyProductPrices
          );
          this.copyProductAccounts = SettingsUtil.getBoolCompanySetting(
            setting,
            CompanySettingType.BillingCopyProductAccounts,
            this.copyProductAccounts
          );
          this.copyProductStock = SettingsUtil.getBoolCompanySetting(
            setting,
            CompanySettingType.BillingCopyProductStock,
            this.copyProductStock
          );
        })
      )
    );
  }

  private loadSettingTypes(): void {
    this.productAccountSettingTypes = [];
    this.productAccountSettingTypes.push(
      new SmallGenericType(
        ProductAccountType.Purchase,
        this.terms[
          'billing.products.products.accountingsettingtype.receivables'
        ]
      )
    );
    this.productAccountSettingTypes.push(
      new SmallGenericType(
        ProductAccountType.Sales,
        this.terms['billing.products.products.accountingsettingtype.sales']
      )
    );
    this.productAccountSettingTypes.push(
      new SmallGenericType(
        ProductAccountType.VAT,
        this.terms['billing.products.products.accountingsettingtype.vat']
      )
    );
    this.productAccountSettingTypes.push(
      new SmallGenericType(
        ProductAccountType.SalesNoVat,
        this.terms['billing.products.products.accountingsettingtype.salesnovat']
      )
    );
    this.productAccountSettingTypes.push(
      new SmallGenericType(
        ProductAccountType.SalesContractor,
        this.terms[
          'billing.products.products.accountingsettingtype.salescontractor'
        ]
      )
    );

    this.stockAccountSettingTypes = [];
    this.stockAccountSettingTypes.push(
      new SmallGenericType(
        ProductAccountType.StockIn,
        this.terms['billing.products.products.stockaccountsettingtype.stockin']
      )
    );
    this.stockAccountSettingTypes.push(
      new SmallGenericType(
        ProductAccountType.StockInChange,
        this.terms[
          'billing.products.products.stockaccountsettingtype.stockinchange'
        ]
      )
    );
    this.stockAccountSettingTypes.push(
      new SmallGenericType(
        ProductAccountType.StockOut,
        this.terms['billing.products.products.stockaccountsettingtype.stockout']
      )
    );
    this.stockAccountSettingTypes.push(
      new SmallGenericType(
        ProductAccountType.StockOutChange,
        this.terms[
          'billing.products.products.stockaccountsettingtype.stockoutchange'
        ]
      )
    );
    this.stockAccountSettingTypes.push(
      new SmallGenericType(
        ProductAccountType.StockInv,
        this.terms['billing.products.products.stockaccountsettingtype.stockinv']
      )
    );
    this.stockAccountSettingTypes.push(
      new SmallGenericType(
        ProductAccountType.StockInvChange,
        this.terms[
          'billing.products.products.stockaccountsettingtype.stockinvchange'
        ]
      )
    );
    this.stockAccountSettingTypes.push(
      new SmallGenericType(
        ProductAccountType.StockLoss,
        this.terms[
          'billing.products.products.stockaccountsettingtype.stockloss'
        ]
      )
    );
    this.stockAccountSettingTypes.push(
      new SmallGenericType(
        ProductAccountType.StockLossChange,
        this.terms[
          'billing.products.products.stockaccountsettingtype.stocklosschange'
        ]
      )
    );
  }

  private loadProductUnitConverts(): Observable<void> {
    if (this.form?.productId.value > 0) {
      this.performLoadData.load(
        this.service
          .getProductUnitConverts(this.form?.productId.value, false)
          .pipe(
            tap(rows => {
              this.productUnitConverts = rows;
            })
          )
      );
    }

    return of(void 0);
  }

  private loadVatTypes(): Observable<void> {
    return this.performLoadData.load$(
      this.coreService
        .getTermGroupContent(TermGroup.InvoiceProductVatType, true, false)
        .pipe(tap(vTypes => (this.vatTypes = vTypes)))
    );
  }

  private loadVatCodes(): Observable<void> {
    return this.performLoadData.load$(
      this.service.getVatCodes().pipe(
        tap(vatCodes => {
          this.vatCodes = vatCodes.map(
            x => new SmallGenericType(x.vatCodeId, x.name)
          );
          this.vatCodes.splice(0, 0, new SmallGenericType(0, ''));
        })
      )
    );
  }

  private loadProductUnits(): Observable<void> {
    return this.performLoadData.load$(
      this.service
        .getProductUnitsDict()
        .pipe(tap(pUnits => (this.productUnits = pUnits)))
    );
  }

  private loadMaterialCodes(): Observable<void> {
    return this.performLoadData.load$(
      this.service.getMaterialCodes(SoeTimeCodeType.Material, true, false).pipe(
        tap(mCodes => {
          this.materialCodes = mCodes.map(
            x => new SmallGenericType(x.timeCodeId, x.name)
          );
          this.materialCodes.splice(0, 0, new SmallGenericType(0, ''));
        })
      )
    );
  }

  private loadProductGroups(): Observable<void> {
    return this.performLoadData.load$(
      this.service
        .getProductGroups()
        .pipe(tap(pGroups => (this.productGroups = pGroups)))
    );
  }

  private loadHouseholdDeductionTypes(): Observable<void> {
    return this.performLoadData.load$(
      this.service
        .getHouseholdDeductionTypes(true)
        .pipe(tap(hdTypes => (this.householdDeductionTypes = hdTypes)))
    );
  }

  private loadCalculationTypes(): Observable<void> {
    return this.performLoadData.load$(
      this.coreService
        .getTermGroupContent(
          TermGroup.InvoiceProductCalculationType,
          true,
          false
        )
        .pipe(tap(cTypes => (this.calculationTypes = cTypes)))
    );
  }

  private loadGrossMarginCalculationTypes(): Observable<void> {
    return this.performLoadData.load$(
      this.coreService
        .getTermGroupContent(TermGroup.GrossMarginCalculationType, true, false)
        .pipe(tap(cTypes => (this.grossMarginCalculationTypes = cTypes)))
    );
  }

  private loadCommodityCodes(): Observable<void> {
    return this.performLoadData.load$(
      this.service
        .getCustomerCommodityCodesDict(true)
        .pipe(tap(commodityCodes => (this.commodityCodes = commodityCodes)))
    );
  }

  private loadCountries(): Observable<void> {
    return this.performLoadData.load$(
      this.coreService
        .getCountries(true, false)
        .pipe(tap(c => (this.countries = c)))
    );
  }

  private loadAccountingPriority(): Observable<SmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(TermGroup.InvoiceProductAccountingPrio, true, false)
      .pipe(
        tap(x => {
          this.accountingPrios = x;
        })
      );
  }

  private loadAccountPriorityRows(): void {
    const productAccountingPriorities = String(this.form?.accountingPrio.value)
      .split(',')
      .map(x => {
        const values = x.split('=');
        return {
          dimNr: Number(values[0]),
          prioNr: Number(values[1]),
        };
      });

    this.productAccountPriorityRows = [];

    this.coreService
      .getAccountDimsSmall(
        false,
        false,
        false,
        false,
        false,
        true,
        true,
        false,
        false
      )
      .subscribe((dims: IAccountDimSmallDTO[]): void => {
        const priorityRows: PriorityAccountRow[] = [];
        dims.forEach(dim => {
          const productPrio = productAccountingPriorities.find(
            z => z.dimNr === dim.accountDimNr
          );

          if (productPrio) {
            const prio = this.accountingPrios.filter(
              a => a.id == productPrio.prioNr
            )[0];
            priorityRows.push(
              new PriorityAccountRow(
                dim.accountDimNr,
                dim.name,
                productPrio.prioNr,
                prio ? prio.name : ''
              )
            );
          }
        });
        this.productAccountPriorityRows = priorityRows;
      });
  }

  private loadStocksForProduct(): void {
    if (this.form?.productId.value <= 0) return;

    this.performLoadData.load(
      this.service
        .getStocksByProduct(this.form?.productId.value)
        .pipe(tap(s => (this.stocksForProduct = s)))
    );
  }

  private loadExtraFields(): Observable<IExtraFieldGridDTO[]> {
    return this.extraFieldService
      .getGrid(undefined, {
        entity: SoeEntityType.InvoiceProduct,
        loadRecords: false,
        connectedEntity: 0,
        connectedRecordId: 0,
        useCache: false,
      })
      .pipe(tap(ef => this.extraFields.set(ef)));
  }

  private loadPriceLists(): Observable<IPriceListTypeGridDTO[]> {
    if (this.form?.isExternal.value) {
      return this.service.getPriceListTypesGrid().pipe(
        tap(priceLists => {
          priceLists.forEach(p => {
            p.name = `${p.name} (${p.currency})`;
          });
          this.allPriceLists.set(priceLists);
        })
      );
    }

    return of([]);
  }

  private loadTranslations(): void {
    this.performLoadData.load(
      this.translationsService
        .getTranslations(
          this.compTermRecordType,
          this.form?.productId.value,
          true
        )
        .pipe(
          tap(compTerms => {
            this.compTermRows = compTerms;
            this.form?.patchCompTerms(compTerms);
          })
        )
    );
  }
  //#endregion

  override loadData(): Observable<void> {
    return this.performLoadData.load$(
      this.service.get(this.form?.getIdControl()?.value).pipe(
        tap((product: InvoiceProductDTO): void => {
          this.form?.customPatchValue(product);
          this.productPriceLists = product.priceLists;
          this.isNotExternal.set(!this.form?.isExternal.value);
        })
      )
    );
  }

  private setProductListPriceData(): void {
    if (this.productPriceLists.length > 0) {
      const priceList = this.allPriceLists().find(
        p => p.priceListTypeId === this.productPriceLists[0].priceListTypeId
      );
      this.form?.patchValue({
        externalPrice: this.productPriceLists[0].price,
        priceListName: priceList?.name ?? '',
      });
    }
  }

  //#region Events
  protected commodityCodeChanged(item: SmallGenericType): void {
    this.commodityCodeTooltip.set(item.name);
  }

  protected categoriesChanged(categories: CategoryItem[]): void {
    this.form?.customCategoryIdsPatchValue(categories.map(c => c.categoryId));
  }

  protected accountSettingPanelOpened(isOpened: boolean): void {
    if (!this.accountSettingsExpanderRendered) {
      this.accountSettingsExpanderRendered = isOpened;
      this.loadAccountPriorityRows();
    }
  }

  protected accountSettingsChanged(rows: IAccountingSettingsRowDTO[]): void {
    this.form?.customPatchAccountSettings(rows, true);
  }

  protected stockAccountSettingsChanged(
    rows: IAccountingSettingsRowDTO[]
  ): void {
    this.form?.customPatchAccountSettings(rows, false);
  }

  protected supplierProductsExpanderOpened(isOpened: boolean): void {
    if (!this.supplierProductExpanderRendered()) {
      this.supplierProductExpanderRendered.set(isOpened);
    }
  }

  protected stockPanelOpened(isOpened: boolean): void {
    if (!this.stockExpanderRendered) {
      this.stockExpanderRendered = isOpened;
      this.loadStocksForProduct();
    }
  }

  protected extraFieldsChanged(items: IExtraFieldRecordDTO[]): void {
    this.extraFieldRecords = items;
    this.form?.markAsDirty();
  }

  protected extraFieldsPanelOpened(isOpened: boolean): void {
    if (!this.extraFieldsExpanderRendered() && isOpened) {
      this.extraFieldsExpanderRendered.set(true);
    }
  }

  protected translationsExpanderOpened(isOpened: boolean): void {
    if (!this.translationsExpanderRendered() && isOpened) {
      this.translationsExpanderRendered.set(true);
      this.loadTranslations();
    }
  }
  //#endregion

  private convertToUserProduct(): void {
    if (this.form?.productId.value > 0 && this.form?.isExternal.value) {
      this.form?.isExternal.setValue(false);
      this.form?.markAsDirty();
      this.messageboxService.information(
        'core.ok',
        'billing.products.product.convertedtouserproduct'
      );
    }
  }

  private openProductInfo(): void {
    this.performAction.load(
      this.service.getProductExternalUrls([this.form?.productId.value]).pipe(
        tap(urls => {
          urls.forEach(url => {
            BrowserUtil.openInNewTab(window, url);
          });
        })
      )
    );
  }

  private copyProduct(): void {
    const dialogOpts = <Partial<CopyProductDialogData>>{
      title: 'billing.product.copyproduct',
      size: 'md',
      disableClose: true,
    };

    this.dialogService
      .open(CopyProductDialogComponent, dialogOpts)
      .afterClosed()
      .subscribe(res => {
        if (res && (<CopyProductDialogData>res).copyProductSetting) {
          const copySetting = (<CopyProductDialogData>res).copyProductSetting;
          this.form?.patchCopyItems(
            this.productPriceLists,
            this.stocksForProduct,
            this.extraFieldRecords,
            copySetting?.copyPrice ?? false,
            copySetting?.copyAccounts ?? false,
            copySetting?.copyStock ?? false
          );
          this.copy();
        }
      });
  }

  protected triggerProductSave(options?: ProgressOptions): void {
    this.applyAllSubGridChanges();

    //categories
    const categoryRecords = this.form?.categoryIds.value.map(id => {
      return { categoryId: id, default: false };
    });

    //accounting priorities
    let prioString: string = '';
    this.productAccountPriorityRows.forEach(row => {
      prioString += row.dimNr + '=' + row.prioNr + ',';
    });
    this.form?.accountingPrio.setValue(prioString);

    //accounting settings
    this.form?.mergeAccountingSettings();

    // Check vat code
    if (this.form?.vatCodeId.value === 0) this.form?.vatCodeId.setValue(null);

    // Check material code
    if (this.form?.timeCodeId.value === 0) this.form?.timeCodeId.setValue(null);

    //translations
    this.compTermRows = this.compTermRows.filter(
      c => c.state !== SoeEntityState.Deleted
    );
    this.form?.patchCompTerms(this.compTermRows);

    this.additionalSaveData = {
      priceLists: this.productPriceLists.filter(r => r.isModified),
      categoryRecords: categoryRecords,
      stocks: this.stocksForProduct,
      translations: this.compTermRows,
      extraFields: this.extraFieldRecords,
    };

    if (!this.form || this.form.invalid || !this.service) return;

    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service
        .save(
          this.form?.getRawValue() as InvoiceProductDTO,
          this.additionalSaveData
        )
        .pipe(
          mergeMap(result => {
            if (result.success) {
              const modifiedUnitConverts = this.productUnitConverts.filter(
                x => x.isModified
              );
              this.hasUnitConversionsToSave = modifiedUnitConverts.length > 0;
              if (modifiedUnitConverts.length > 0) {
                return this.service
                  .saveProductUnitConvert(modifiedUnitConverts)
                  .pipe(
                    map(r => {
                      ResponseUtil.setBooleanValue2(result, r.success);
                      ResponseUtil.setErrorMessage(
                        result,
                        ResponseUtil.getErrorMessage(r)
                      );
                      ResponseUtil.setErrorNumber(
                        result,
                        ActionResultSave.ProductUnitConversionNotSaved
                      );
                      return result;
                    })
                  );
              }
            }

            return of(result);
          })
        )
        .pipe(
          tap(res => {
            this.form?.markAsPristine();
            this.form?.markAsUntouched();

            this.productSaveCompleted(res);
          })
        ),
      undefined,
      undefined
    );
  }

  productSaveCompleted = (backendResponse: BackendResponse): void => {
    if (ResponseUtil.getEntityId(backendResponse)) {
      const booleanValue2 = ResponseUtil.getBooleanValue2(backendResponse);
      if (booleanValue2 || !this.hasUnitConversionsToSave) {
        this.updateFormValueAndEmitChange(backendResponse);
      } else if (!booleanValue2) {
        //When product successfully saved and product unit conversions saving failed.
        if (!booleanValue2) {
          this.progressService.saveError(<ProgressOptions>{
            showDialogOnError: true,
            showToastOnError: false,
            title: 'core.error',
            message: ResponseUtil.getErrorMessage(backendResponse),
          });
        }
      }
      this.loadSubGrids();
    } else {
      const errors = [
        {
          errorNumber: ActionResultSave.ProductExists,
          message: 'billing.products.products.productexistsmessage',
        },
        {
          errorNumber: ActionResultSave.ProductNotSaved,
          message: 'billing.products.products.productnotsavedmessage',
        },
        {
          errorNumber: ActionResultSave.ProductInUse,
          message: 'billing.products.products.productinusemessage',
        },
        {
          errorNumber: ActionResultSave.ProductPriceListNotSaved,
          message: 'billing.products.products.productpricelistnotsavedmessage',
        },
        {
          errorNumber: ActionResultSave.ProductCategoriesNotSaved,
          message: 'billing.products.products.productcategoriesnotsavedmessage',
        },
        {
          errorNumber: ActionResultSave.ProductAccountsNotSaved,
          message: 'billing.products.products.productaccountsnotsavedmessage',
        },
        {
          errorNumber: ActionResultSave.TranslationsSaveFailed,
          message: 'billing.products.products.translationsnotsavedmessage',
        },
        {
          errorNumber: ActionResultSave.ProductWeightInvalid,
          message: 'billing.products.products.productweightinvalidmessage',
        },
      ];
      let message =
        errors.find(
          e => e.errorNumber === ResponseUtil.getErrorNumber(backendResponse)
        ) ?? 'billing.products.products.failedsavemessage';

      const errorMsg = ResponseUtil.getErrorMessage(backendResponse);
      if (errorMsg && errorMsg.length > 0) {
        message = `${message}\n${errorMsg}`;
      }
      this.progressService.saveError(<ProgressOptions>{
        showDialogOnError: true,
        message: message,
      });
    }
  };

  private loadSubGrids(): void {
    if (this.extraFieldsExpanderRendered()) this.loadExtraFields().subscribe();

    if (this.stockExpanderRendered) this.loadStocksForProduct();

    if (this.translationsExpanderRendered()) this.loadTranslations();
  }

  private applyAllSubGridChanges(): void {
    this.productPriceGrid()?.grid?.applyChanges();
    this.accountingPriorityGrid()?.grid?.applyChanges();
    this.unitConvertGrid()?.grid?.applyChanges();
    this.stocksForProductGrid()?.grid.applyChanges();
    this.compTermGrid()?.grid?.applyChanges();
  }

  ngOnDestroy(): void {
    this._destroy$.next();
    this._destroy$.complete();
  }
}
