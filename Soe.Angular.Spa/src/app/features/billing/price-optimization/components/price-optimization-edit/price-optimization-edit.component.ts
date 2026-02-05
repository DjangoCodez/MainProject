import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { PriceOptimizationService } from '../../services/price-optimization.service';
import {
  PurchaseCartDTO,
  PurchaseCartRowDTO,
} from '../../models/price-optimization.model';
import {
  Feature,
  SoeOriginType,
  TermGroup,
  TermGroup_PurchaseCartStatus,
} from '@shared/models/generated-interfaces/Enumerations';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { MenuButtonItem } from '@ui/button/menu-button/menu-button.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { PriceOptimizationForm } from '../../models/price-optimization-form.model';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { BehaviorSubject, Observable, of, tap } from 'rxjs';
import { CrudActionTypeEnum } from '@shared/enums';
import { ProgressOptions } from '@shared/services/progress';
import { SelectCustomerInvoiceDialogComponent } from '@shared/components/select-customer-invoice-dialog/component/select-customer-invoice-dialog/select-customer-invoice-dialog.component';
import { SelectInvoiceDialogDTO } from '@shared/components/select-customer-invoice-dialog/model/customer-invoice-search.model';
import { ITransferInvoiceDTO } from '@shared/models/generated-interfaces/PurchaseCartDTOs';
import { ISysWholsesellerPriceSearchDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { TraceRowPageName } from '@shared/components/trace-rows/models/trace-rows.model';
import { ToolbarButtonAction } from '@ui/toolbar/toolbar-button/toolbar-button.component';

export enum FunctionType {
  TransferToOrder = 1,
  TransferToOffer,
}

@Component({
  selector: 'soe-price-optimization-edit',
  templateUrl: './price-optimization-edit.component.html',
  standalone: false,
  providers: [FlowHandlerService, ToolbarService],
})
export class PriceOptimizationEditComponent
  extends EditBaseDirective<
    PurchaseCartDTO,
    PriceOptimizationService,
    PriceOptimizationForm
  >
  implements OnInit
{
  service = inject(PriceOptimizationService);
  coreService = inject(CoreService);
  dialogService = inject(DialogService);

  cartStatuses: ISmallGenericType[] = [];
  menuButtonList: MenuButtonItem[] = [];
  purchaseRows: BehaviorSubject<PurchaseCartRowDTO[]> = new BehaviorSubject<
    PurchaseCartRowDTO[]
  >([]);
  isNotOpen = signal(false);
  orderPermission = false;
  offerPermission = false;
  wholesalers: BehaviorSubject<any[]> = new BehaviorSubject<any[]>([]);
  pageName = TraceRowPageName.PriceOptimization;

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(Feature.Billing_Price_Optimization, {
      additionalModifyPermissions: [
        Feature.Billing_Offer,
        Feature.Billing_Order,
      ],
      lookups: [this.loadStatuses(), this.loadWholesalers()],
    });
  }

  override loadData(): Observable<void> {
    const shippingCartId = this.form?.getIdControl()?.value;
    return this.performLoadData.load$(
      this.service.get(shippingCartId).pipe(
        tap(value => {
          this.showHideFields(value);

          this.loadItemsGrid(shippingCartId).subscribe(() => {
            this.getPrices();
            this.form?.patchValue(value);
            this.updateStatusName();
          });
        })
      ),
      { showDialogDelay: 500 }
    );
  }

  override newRecord(): Observable<void> {
    let clearValues = () => {};

    this.form?.markAsDirty(); //Not auto setting dirty when new record

    if (this.form?.isCopy) {
      clearValues = () => {
        this.form?.patchValue({
          purchaseCartId: 0,
          seqNr: 0,
          statusValue: TermGroup_PurchaseCartStatus.Open,
          status: TermGroup_PurchaseCartStatus.Open,
        });

        this.form?.onDoCopy();
        this.purchaseRows.next(this.form?.getRawValue().purchaseCartRows);
        this.getPrices();
      };
    }

    setTimeout(() => {
      this.form?.patchValue({});
    });

    return of(clearValues());
  }

  override onFinished(): void {
    this.buildFunctionList();
    this.form?.seqNr.disable();
  }

  override onPermissionsLoaded(): void {
    super.onPermissionsLoaded();

    this.offerPermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Offer
    );
    this.orderPermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Order
    );
  }

  showHideFields(value: PurchaseCartDTO = this.form?.getRawValue()) {
    this.isNotOpen.set(this.isOpenChange(value));

    if (this.isNotOpen()) {
      this.form?.name.disable();
      this.form?.description.disable();
    } else {
      this.form?.name.enable();
      this.form?.description.enable();
    }
  }

  isOpenChange(value: PurchaseCartDTO): boolean {
    return value.status !== TermGroup_PurchaseCartStatus.Open;
  }

  loadItemsGrid(shoppingCartId: number) {
    return this.performLoadData.load$(
      this.service.getPurchaseCartRow(shoppingCartId).pipe(
        tap(value => {
          if (value) {
            value.forEach(row => {
              row.selectedPrice = row.purchasePrice ? row.purchasePrice : 0;
            });

            this.purchaseRows.next(value);
            this.form?.customRowPatchValue(value);
          }
        })
      )
    );
  }

  protected updatePrices(isUpdate: boolean): void {
    if (isUpdate) this.getPrices();
  }

  protected getPrices(): void {
    if (this.purchaseRows.value.length > 0) {
      const ProductIds = this.purchaseRows.value.map(row => {
        return row.sysProductId;
      });

      if (ProductIds.length > 0) {
        return this.performLoadData.load(
          this.service.getPurchaseCartRowPrices(ProductIds).pipe(
            tap((priceResults: ISysWholsesellerPriceSearchDTO[]) => {
              const updatedRows = this.purchaseRows.value.map(row => {
                const pricesForEachWholesaler = priceResults.filter(
                  (price: ISysWholsesellerPriceSearchDTO) =>
                    price.sysProductId === row.sysProductId
                );

                this.wholesalers.subscribe(wholesalers => {
                  wholesalers.forEach((wholesaler, index) => {
                    const priceMatch = pricesForEachWholesaler.find(
                      (p: ISysWholsesellerPriceSearchDTO) =>
                        p.sysWholesellerId === wholesaler.id
                    );

                    if (priceMatch) {
                      const priceField = `wholesalerPrice${index + 1}`;
                      (row as any)[priceField] = priceMatch.gnp;
                    }
                  });
                });

                return row;
              });
              this.purchaseRows.next(updatedRows);
            })
          )
        );
      }
    }
  }

  buildFunctionList() {
    this.menuButtonList = [];

    this.menuButtonList.push({
      id: FunctionType.TransferToOrder,
      label: this.translate.instant(
        'billing.purchase.priceoptimization.transfertoorder'
      ),
      disabled: signal(this.orderPermission) && this.isNotOpen,
    });
    this.menuButtonList.push({
      id: FunctionType.TransferToOffer,
      label: this.translate.instant(
        'billing.purchase.priceoptimization.transfertooffer'
      ),
      disabled: signal(this.offerPermission) && this.isNotOpen,
    });
  }

  override createEditToolbar(): void {
    super.createEditToolbar();

    if (!this.form?.getIdControl()?.value) return;

    const lockIcon = computed(() => (this.isNotOpen() ? 'lock-open' : 'lock'));
    const lockTooltip = computed(() =>
      this.isNotOpen()
        ? this.translate.instant(
            'billing.purchase.priceoptimization.openpriceoptimization'
          )
        : this.translate.instant(
            'billing.purchase.priceoptimization.closepriceoptimization'
          )
    );

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton(
          'billing.purchase.priceoptimization.closepriceoptimization',
          {
            iconName: lockIcon,
            tooltip: lockTooltip,
            onAction: event => this.openCloseCart(event),
          }
        ),
      ],
    });
  }

  performMenuButtonAction(functionType: MenuButtonItem) {
    this.openCustomerInvoice(functionType.id);
  }

  openCustomerInvoice(originType: number | undefined) {
    const dialogData = new SelectInvoiceDialogDTO();
    dialogData.title =
      originType === FunctionType.TransferToOrder
        ? this.translate.instant('billing.invoice.searchorder')
        : this.translate.instant('billing.invoice.searchoffer');
    dialogData.size = 'lg';
    dialogData.originType =
      originType === FunctionType.TransferToOrder
        ? SoeOriginType.Order
        : SoeOriginType.Offer;

    this.dialogService
      .open(SelectCustomerInvoiceDialogComponent, dialogData)
      .afterClosed()
      .subscribe((result: any) => {
        if (result) {
          const model: ITransferInvoiceDTO = {
            invoiceId: result.customerInvoiceId,
            purchaseCartId: this.form?.getIdControl()?.value,
          };
          this.performAction.crud(
            CrudActionTypeEnum.Save,
            this.service.transferPurchaseCartRowsToOrder(model).pipe(
              tap(res => {
                if (res.success) {
                  this.triggerCloseDialog(res);

                  this.form?.patchValue({
                    statusValue: TermGroup_PurchaseCartStatus.Transferred,
                    status: TermGroup_PurchaseCartStatus.Transferred,
                  });
                  this.saveStatusChange();
                  this.updateStatusName();
                }
              })
            )
          );
        }
      });
  }

  private openCloseCart(event: ToolbarButtonAction) {
    const currentStatus = this.form?.getRawValue().status;

    if (
      currentStatus == TermGroup_PurchaseCartStatus.Closed ||
      currentStatus == TermGroup_PurchaseCartStatus.Transferred
    ) {
      this.form?.patchValue({
        statusValue: TermGroup_PurchaseCartStatus.Open,
        status: TermGroup_PurchaseCartStatus.Open,
      });
    } else if (currentStatus == TermGroup_PurchaseCartStatus.Open) {
      this.form?.patchValue({
        statusValue: TermGroup_PurchaseCartStatus.Closed,
        status: TermGroup_PurchaseCartStatus.Closed,
      });
    }
    this.saveStatusChange();
  }

  private saveStatusChange(options?: ProgressOptions) {
    this.performSave(options, false);
  }

  override performSave(options?: ProgressOptions, skipLoadData = false): void {
    const model = <PurchaseCartDTO>this.form?.getRawValue();
    model.purchaseCartRows = this.purchaseRows.value;
    model.selectedWholesellerIds = this.form?.selectedWholesellerIds.value;

    if (!this.form || this.form.invalid || !this.service) return;

    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service.save(model).pipe(
        tap(res => {
          this.updateFormValueAndEmitChange(res, skipLoadData),
            undefined,
            undefined,
            {
              showToastOnComplete: true,
            };
        })
      ),
      options?.callback,
      options?.errorCallback,
      options
    );
  }

  loadStatuses(): Observable<ISmallGenericType[]> {
    return this.performLoadData.load$(
      this.coreService
        .getTermGroupContent(TermGroup.PurchaseCartStatus, false, true, true)
        .pipe(
          tap(data => {
            this.cartStatuses = data;
            this.updateStatusName();
          })
        )
    );
  }

  updateStatusName() {
    this.form?.patchValue({
      statusName:
        this.cartStatuses.find(
          status => status.id === this.form?.get('status')?.value
        )?.name || '',
    });
  }

  loadWholesalers(): Observable<any> {
    return this.service.getWholesalers(false).pipe(
      tap(wholesalers => {
        this.wholesalers.next(wholesalers);

        if (!this.form?.isCopy) {
          this.form?.selectedWholesellerIds.patchValue(
            wholesalers.map(w => w.id)
          );
        }
      })
    );
  }
}
