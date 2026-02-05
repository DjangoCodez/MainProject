import {
  AfterViewInit,
  ChangeDetectorRef,
  Component,
  ElementRef,
  OnInit,
  ViewChild,
  inject,
  signal,
} from '@angular/core';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { StockBalanceService } from '../../services/stock-balance.service';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import {
  IAutocompleteIStockProductDTO,
  StockProductDTO,
  StockTransactionDTO,
} from '../../models/stock-balance.model';
import {
  CompanySettingType,
  Feature,
  TermGroup,
  TermGroup_StockTransactionType,
} from '@shared/models/generated-interfaces/Enumerations';
import { Observable, of, tap } from 'rxjs';
import {
  IProductUnitConvertDTO,
  IStockProductDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { IProductSmallDTO } from '@shared/models/generated-interfaces/ProductDTOs';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { SettingsUtil } from '@shared/util/settings-util';
import { StockBalanceForm } from '../../models/stock-balance-form.model';
import { BillingService } from '../../../services/services/billing.service';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { ValidationHandler } from '@shared/handlers';
import { Validators } from '@angular/forms';
import { ProgressOptions } from '@shared/services/progress';
import { CrudActionTypeEnum } from '@shared/enums';
import { focusOnElement } from '@shared/util/focus-util';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import { ResponseUtil } from '@shared/util/response-util';

@Component({
  selector: 'soe-stock-balance-edit',
  templateUrl: './stock-balance-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class StockBalanceEditComponent
  extends EditBaseDirective<
    StockProductDTO,
    StockBalanceService,
    StockBalanceForm
  >
  implements OnInit, AfterViewInit
{
  @ViewChild('invoiceProductId')
  public invoiceProductId!: ElementRef;
  @ViewChild('actionType')
  public actionType!: ElementRef;

  readonly service = inject(StockBalanceService);
  readonly billingService = inject(BillingService);
  readonly validationHandler = inject(ValidationHandler);
  readonly coreService = inject(CoreService);
  readonly cdr = inject(ChangeDetectorRef);

  products: SmallGenericType[] = [];
  targetStocks = signal<SmallGenericType[]>([]);
  allStocksForProduct: IStockProductDTO[] = [];
  stockTransactions: StockTransactionDTO[] = [];
  actionTypes: ISmallGenericType[] = [];
  productUnitConverts: ISmallGenericType[] = [];
  productUnitConvertDtos: IProductUnitConvertDTO[] = [];
  productStocksData: IAutocompleteIStockProductDTO[] = [];
  productStocksDict: SmallGenericType[] = [];

  useProductUnitConvert = signal(true);
  showVoucherColumn?: boolean = undefined;
  hasPriceChangePermission = signal(true);

  isReadOnly = false;
  productUnitLabel = '';
  private _avgPrice = 0.0;

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(Feature.Billing_Stock_Saldo, {
      lookups: [
        this.loadProducts(),
        this.loadActionTypes(),
        this.loadCompanySettings(),
      ],
      additionalModifyPermissions: [Feature.Billing_Stock_Change_AvgPrice],
    });

    this.form?.transaction?.actionType.valueChanges.subscribe(x => {
      this.toggleValidationsAndDisability(Number(x));

      if (
        Number(x) === TermGroup_StockTransactionType.StockTransfer &&
        this.form?.invoiceProductId.value
      ) {
        this.loadTargetProductStocks();
      }
      this.cdr.detectChanges();
    });

    this.toggleValidationsAndDisability(TermGroup_StockTransactionType.Add);
  }

  override onFinished(): void {
    this.loadStockTransaction();

    this.hasPriceChangePermission.set(
      this.flowHandler.hasModifyAccess(Feature.Billing_Stock_Change_AvgPrice)
    );
  }

  ngAfterViewInit(): void {
    this.setDisabled();
    if (this.form?.isNew) {
      this.focusOnProduct();
    } else {
      this.focusOnActionType();
    }
  }

  updateAvaragePriceInForm() {
    if (!this.form?.isNew) {
      const data: StockTransactionDTO = this.form?.getRawValue().transaction;

      if (
        data &&
        data.actionType == TermGroup_StockTransactionType.AveragePriceChange
      ) {
        this.form?.patchValue({ avgPrice: data.price });
      }
    }
  }

  override updateFormValueAndEmitChange = (
    backendResponse: BackendResponse
  ) => {
    if (backendResponse.success) {
      this.updateAvaragePriceInForm();
      this.form?.initForm();
      this.loadStockTransaction();

      if (this.isNew()) this.stockTransactions = [];
    }
  };

  //#region Data Loading

  loadActionTypes(): Observable<SmallGenericType[]> {
    return this.performLoadData.load$(
      this.coreService
        .getTermGroupContent(TermGroup.StockTransactionType, false, false)
        .pipe(
          tap(data => {
            this.actionTypes = data.filter(
              x =>
                x.id < TermGroup_StockTransactionType.Correction ||
                x.id > TermGroup_StockTransactionType.Reserve
            );
            if (!this.hasPriceChangePermission()) {
              this.actionTypes = this.actionTypes.filter(
                x => x.id != TermGroup_StockTransactionType.AveragePriceChange
              );
            }
          })
        )
    );
  }

  loadCompanySettings(): Observable<void> {
    const settingTypes: number[] = [
      CompanySettingType.BillingUseProductUnitConvert,
      CompanySettingType.AccountingCreateVouchersForStockTransactions,
    ];
    return this.performLoadData.load$(
      this.coreService.getCompanySettings(settingTypes).pipe(
        tap(x => {
          this.useProductUnitConvert.set(
            SettingsUtil.getBoolCompanySetting(
              x,
              CompanySettingType.BillingUseProductUnitConvert,
              false
            )
          );

          this.showVoucherColumn = SettingsUtil.getBoolCompanySetting(
            x,
            CompanySettingType.AccountingCreateVouchersForStockTransactions,
            false
          );
        })
      )
    );
  }

  loadProductUnitConverts(
    invoiceProductId: number,
    addEmptyRow: boolean
  ): void {
    this.billingService
      .GetProductUnitConverts(invoiceProductId, addEmptyRow)
      .pipe(
        tap(x => {
          this.productUnitConvertDtos = x;
          this.productUnitConverts = [];
          x.forEach(obj => {
            const name =
              obj.productUnitConvertId > 0
                ? obj.productUnitName + ' (' + obj.convertFactor + ')'
                : '';
            this.productUnitConverts.push(
              new SmallGenericType(obj.productUnitConvertId, name)
            );
          });

          this.toggleQuantityDisability(this.form?.value.actionType);
          this.toggleProductUnitDisability(this.form?.value.actionType);
        })
      );
  }

  loadStockTransaction(): void {
    if (
      this.form?.controls.stockProductId.value &&
      this.form?.controls.stockProductId.value > 0 &&
      !this.form?.isNew
    ) {
      return this.performLoadData.load(
        this.service
          .getStockProductTransactions(this.form?.controls.stockProductId.value)
          .pipe(
            tap(x => {
              this.calculateTotal(x);
              this.stockTransactions = x;
            })
          )
      );
    }
  }

  calculateTotal(rowData: StockTransactionDTO[]) {
    rowData.forEach(row => {
      row.total = row.quantity * row.price;
    });
  }

  loadData(): Observable<void> {
    if (
      !this.isNew() &&
      this.form?.value.stockProductId &&
      this.form?.value.stockProductId > 0
    ) {
      return this.performLoadData.load$(
        this.service.get(this.form?.value.stockProductId).pipe(
          tap(value => {
            this.products = [
              <SmallGenericType>{
                id: value.invoiceProductId,
                name: value.productNumber + ' ' + value.productName,
              },
            ];

            this.productStocksData = [
              <IAutocompleteIStockProductDTO>{
                stockProductId: value.stockProductId,
                stockName: value.stockName,
              },
            ];
            this.updateWarehouseLocationDict(this.productStocksData);
            /*
            this.productStocksData.next([
              <IAutocompleteIStockProductDTO>{
                stockProductId: value.stockProductId,
                stockName: value.stockName,
              },
            ]);
            */
            this.form?.customPatch(<StockProductDTO>value);

            this._avgPrice = this.form?.avgPrice.value;
            if (value.invoiceProductId) {
              this.loadProductUnitConverts(value.invoiceProductId, true);
            }
          })
        )
      );
    } else {
      this.toggleQuantityDisability(TermGroup_StockTransactionType.Add);
      return of(undefined);
    }
  }

  loadProducts(): Observable<IProductSmallDTO[] | boolean> {
    if (this.isNew()) {
      return this.performLoadData.load$(
        this.service.getStockProductProducts().pipe(
          tap(products => {
            this.products = products.map(
              x => <SmallGenericType>{ id: x.productId, name: x.numberName }
            );
          })
        )
      );
    }

    return of(true);
  }

  private isNew() {
    return this.form?.isNew;
  }

  private loadStockProductsForInvoiceProduct(
    productId: number,
    updateForm: boolean
  ): void {
    this.service.getStockProductsByProductId(productId).subscribe(x => {
      const selectedLocation = this.form?.stockProductId.value;
      const _productStocksData = this.productStocksData;
      const selectedStockId =
        _productStocksData.find(x => x.stockProductId == selectedLocation)
          ?.stockId || 0;

      this.productStocksData = <IAutocompleteIStockProductDTO[]>x;
      this.updateWarehouseLocationDict(this.productStocksData);
      this.loadTargetProductStocks();

      const stockProductId =
        this.productStocksData.find(x => x.stockId == selectedStockId)
          ?.stockProductId || 0;
      this.form?.patchValue({ stockProductId: stockProductId });

      if (updateForm) {
        const stocksRow = this.productStocksData.find(
          location => location.stockProductId == stockProductId
        );
        this.updateForm(stocksRow);
      }
    });
  }

  updateWarehouseLocationDict(locations: IAutocompleteIStockProductDTO[]) {
    const warehouseLocationFrom: SmallGenericType[] = [];
    locations.forEach(data => {
      warehouseLocationFrom.push(<SmallGenericType>{
        id: data.stockProductId,
        name: data.stockName,
      });
    });
    this.productStocksDict = warehouseLocationFrom;
  }

  addStockTransaction() {
    const data = this.form?.getRawValue().transaction;

    if (this.productStocksData.length > 0 && data.stockProductId) {
      const stockData = this.productStocksData.find(
        x => x.stockProductId === data.stockProductId
      );

      data.isModified = true;
      data.stockName = stockData?.stockName;
      data.productId = stockData?.invoiceProductId;
      data.productNr = stockData?.productNumber + ' ' + stockData?.productName;
      data.childStockTransaction = this.targetStocks().find(
        to => to.id === data.targetStockId
      )?.name;
      data.actionTypeName = this.actionTypes.find(
        x => x.id == data.actionType
      )?.name;
    }

    this.stockTransactions = [...this.stockTransactions, data];

    this.calculateTotal(this.stockTransactions);

    this.form?.clearDataAdding();

    this.focusOnProduct();
  }

  focusOnProduct() {
    focusOnElement((<any>this.invoiceProductId).inputER.nativeElement, 150);
  }

  focusOnActionType() {
    focusOnElement((<any>this.actionType).inputER.nativeElement, 150);
  }

  //#endregion

  //#region Events

  productSelectionChanged($event: SmallGenericType) {
    this.targetStocks.set([]);
    this.allStocksForProduct = [];
    this.loadStockProductsForInvoiceProduct($event.id, true);
    this.loadProductUnitConverts($event.id, true);
    this.updateAvaragePriceInForm();
  }

  productStockChanged($event: IAutocompleteIStockProductDTO) {
    this.setTargetProductStocks(false);

    const selectedLocationDetails = this.productStocksData.find(
      e => e.stockProductId == $event.id
    );
    this.updateForm(selectedLocationDetails!);
  }

  onProductUnitConvertChange(value: number) {
    this.productUnitLabel = '';
    this.cdr.detectChanges();
    const obj = this.productUnitConvertDtos.find(
      x => x.productUnitConvertId == value
    );
    if (obj && value > 0) {
      this.productUnitLabel = obj.productUnitName;
    } else {
      this.productUnitLabel = '';
    }
  }

  override performSave(options?: ProgressOptions): void {
    if (!this.form || !this.service) return;

    if (this.isNew()) {
      this.performAction.crud(
        CrudActionTypeEnum.Save,
        this.service
          .saveTransactions(this.stockTransactions)
          .pipe(tap(this.updateFormValueAndEmitChange)),
        undefined,
        undefined,
        options
      );
    } else {
      if (this.form.invalid) return;

      this.performAction.crud(
        CrudActionTypeEnum.Save,
        this.service.save(this.form?.getRawValue()).pipe(
          tap((result: BackendResponse) => {
            const decimalValue = ResponseUtil.getDecimalValue(result);
            if (result.success && decimalValue) {
              this.form?.patchValue({ avgPrice: decimalValue });
              //we have a new average price
            }
            this.updateFormValueAndEmitChange(result);
          })
        ),
        undefined,
        undefined,
        options
      );
    }
  }

  //#endregion

  //#region Helper Methods

  private toggleQuantityDisability(value: TermGroup_StockTransactionType) {
    this.form?.transaction?.quantity.clearValidators();
    if (value === TermGroup_StockTransactionType.AveragePriceChange) {
      this.form?.transaction?.quantity.disable();
    } else {
      this.form?.transaction?.quantity.enable();
      this.form?.transaction?.quantity.addValidators(Validators.required);
    }
    this.form?.transaction?.quantity.updateValueAndValidity();
  }

  private toggleProductUnitDisability(
    actionType: TermGroup_StockTransactionType
  ) {
    if (
      (actionType && actionType !== TermGroup_StockTransactionType.Add) ||
      this.productUnitConverts.length < 2
    )
      this.form?.transaction?.productUnitConvertId.disable();
    else this.form?.transaction?.productUnitConvertId.enable();
  }

  private toggleAvgPriceValidations(
    actionType: TermGroup_StockTransactionType
  ) {
    this.form?.transaction?.removePriceValidations();
    if (
      actionType === TermGroup_StockTransactionType.AveragePriceChange ||
      actionType === TermGroup_StockTransactionType.Add ||
      actionType === TermGroup_StockTransactionType.Loss ||
      actionType === TermGroup_StockTransactionType.Take
    ) {
      this.form?.transaction?.addPriceValidations();
    }
  }

  private toggleTargetStockValidation(
    actionType: TermGroup_StockTransactionType
  ) {
    this.form?.transaction?.targetStockId.clearValidators();
    if (actionType === TermGroup_StockTransactionType.StockTransfer) {
      this.form?.transaction?.targetStockId.addValidators(Validators.required);
    }
    this.form?.transaction?.targetStockId.updateValueAndValidity();
  }

  private toggleAvgPriceEditable(actionType: TermGroup_StockTransactionType) {
    if (actionType === TermGroup_StockTransactionType.StockTransfer) {
      this.form?.transaction?.price.disable();
    } else {
      this.form?.transaction?.price.enable();
    }
  }

  private setDisabled() {
    if (!this.form?.isNew) {
      this.form?.invoiceProductId.disable();
      this.form?.stockProductId.disable();
      this.form?.stockName.disable();
    }
  }
  /*
  private setProductStocks() {
    if (this.productStocksData.length === 1) {
      this.form?.patchStockProductId(this.productStocksData[0].stockProductId);
    }
  }
*/
  private loadTargetProductStocks() {
    if (this.allStocksForProduct.length > 0) {
      this.setTargetProductStocks(false);
    } else {
      this.service
        .getStockProductsByProductId(this.form?.invoiceProductId.value)
        .subscribe(x => {
          this.allStocksForProduct = x;
          this.setTargetProductStocks(true);
        });
    }
  }

  private setTargetProductStocks(showWarning: boolean) {
    this.targetStocks.set(
      this.allStocksForProduct
        .filter(x => x.stockProductId !== this.form?.stockProductId.value)
        .map(
          m =>
            <SmallGenericType>{
              id: m.stockId,
              name: m.stockName,
            }
        ) ?? []
    );
    if (showWarning) {
      if (this.targetStocks().length === 0) {
        this.messageboxService.error(
          'core.warning',
          'billing.stock.stocksaldo.missingtarget'
        );
      }
    }
  }

  private updateForm(stockProduct?: IAutocompleteIStockProductDTO) {
    this.form?.patchTransaction(stockProduct);
  }

  private toggleValidationsAndDisability(
    transactionType: TermGroup_StockTransactionType
  ) {
    this.toggleAvgPriceValidations(transactionType);
    this.toggleQuantityDisability(transactionType);
    this.toggleProductUnitDisability(transactionType);
    this.toggleTargetStockValidation(transactionType);
    this.toggleAvgPriceEditable(transactionType);
  }
  //#endregion
}
