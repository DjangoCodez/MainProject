import { Component, OnInit, inject, signal } from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  Feature,
  PurchaseCustomerInvoiceViewType,
} from '@shared/models/generated-interfaces/Enumerations';
import { IPurchaseDeliveryRowDTO } from '@shared/models/generated-interfaces/PurchaseDeliveryDTOs ';
import { IPurchaseSmallDTO } from '@shared/models/generated-interfaces/PurchaseDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressOptions } from '@shared/services/progress';
import { Perform } from '@shared/util/perform.class';
import { SupplierService } from '@src/app/features/economy/services/supplier.service';
import { MenuButtonItem } from '@ui/button/menu-button/menu-button.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { BehaviorSubject, Observable, of, tap } from 'rxjs';
import { PurchaseService } from '../../../purchase/services/purchase.service';
import { PurchaseDeliveryForm } from '../../models/purchase-delivery-form.model';
import {
  PurchaseDeliveryRowDTO,
  PurchaseDeliverySaveDTO,
} from '../../models/purchase-delivery.model';
import { PurchaseDeliveryService } from '../../services/purchase-delivery.service';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import { ResponseUtil } from '@shared/util/response-util';

export enum FunctionType {
  Save = 1,
  SaveAndClose = 2,
}

export enum Tab {
  delivery = 1,
  waitingDelivery = 2,
}

@Component({
  selector: 'soe-purchase-delivery-edit',
  templateUrl: './purchase-delivery-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class PurchaseDeliveryEditComponent
  extends EditBaseDirective<
    PurchaseDeliverySaveDTO,
    PurchaseDeliveryService,
    PurchaseDeliveryForm
  >
  implements OnInit
{
  menuList: MenuButtonItem[] = [];
  supplierOptions = signal<SmallGenericType[]>([]);
  filteredPurchase = signal<IPurchaseSmallDTO[]>([]);
  purchaseData = new BehaviorSubject<PurchaseDeliveryRowDTO[]>([]);

  purchaseId!: number;
  supplierId: number = 0;
  customerInvoiceId!: number;
  customerInvoiceRowId!: number;
  purchaseDeliveryId!: number;
  id!: number;
  isHideFetchButton = signal(false);
  isFetchDisable = signal(true);
  isSaveDisable = signal(true);

  service = inject(PurchaseDeliveryService);
  supplierService = inject(SupplierService);
  purchaseService = inject(PurchaseService);

  performLoadSupplier = new Perform<SmallGenericType[]>(this.progressService);
  performLoadPurchaseOrders = new Perform<IPurchaseSmallDTO[]>(
    this.progressService
  );
  performPurchaseData = new Perform<IPurchaseDeliveryRowDTO[]>(
    this.progressService
  );
  loadPurchase: IPurchaseSmallDTO[] = [];

  get viewType(): PurchaseCustomerInvoiceViewType {
    if (this.purchaseId && this.purchaseId > 0)
      return PurchaseCustomerInvoiceViewType.FromPurchase;
    else if (this.customerInvoiceId && this.customerInvoiceId > 0)
      return PurchaseCustomerInvoiceViewType.FromCustomerInvoice;
    else if (this.customerInvoiceRowId && this.customerInvoiceRowId > 0)
      return PurchaseCustomerInvoiceViewType.FromCustomerInvoiceRow;
    else if (this.purchaseDeliveryId && this.purchaseDeliveryId > 0)
      return PurchaseCustomerInvoiceViewType.FromPurchaseDelivery;
    else return PurchaseCustomerInvoiceViewType.Unknown;
  }

  ngOnInit() {
    super.ngOnInit();

    this.form?.deliveryNr.disable();
    this.form?.originDescription.disable();

    this.startFlow(Feature.Billing_Purchase_Delivery_Edit, {
      lookups: [
        this.loadSuppliers(),
        this.loadPurchaseOrders(),
        this.buildFunctionList(),
      ],
    });

    this.id = this.form?.value[this.idFieldName];
    if (this.form && this.form?.value[this.idFieldName] == 0)
      this.form.isNew = true;
    if (!this.form?.isNew) this.isHideFetchButton.set(true);
    else if (this.form?.isNew) this.loadData().subscribe(); //waiting delivery (+ icon clicked)
  }

  override createEditToolbar(): void {
    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('refresh', {
          iconName: signal('refresh'),
          tooltip: signal('common.customer.invoices.reloadorder'),
          onAction: () => this.clickRefresh(),
        }),
      ],
    });
  }

  override loadData(): Observable<void> {
    //waiting delivery
    if (this.form?.value.purchaseId && this.form?.value.purchaseId != 0) {
      this.recordConfig.hideRecordNavigator = true;
      this.purchaseId = this.form?.value.purchaseId;

      this.isSaveDisable.set(false);
      return this.loadDataFromAwaitingDelivery();
    }
    //delivery
    else {
      this.form?.deliveryDate.disable();
      this.form?.supplierId.disable();

      return this.loadDataFromDelivery();
    }
  }

  loadDataFromDelivery() {
    return this.performLoadData.load$(
      this.service.get(this.form?.getIdControl()?.value).pipe(
        tap(value => {
          const supplierDict = [];
          if (value) {
            this.loadPurchaseRows(
              value.purchaseDeliveryId,
              value.supplierId ? value.supplierId : 0,
              Tab.delivery
            ).subscribe();
            this.form?.reset(value);

            this.purchaseDeliveryId = value.purchaseDeliveryId;
            this.purchaseId = value.purchaseId;
            supplierDict.push({
              id: value.supplierId ? value.supplierId : 0,
              name: value.supplierName ? value.supplierName : '',
            });
            this.supplierOptions.set(supplierDict);
          }
        })
      )
    );
  }

  loadDataFromAwaitingDelivery() {
    return this.performLoadData.load$(
      this.purchaseService.get(this.purchaseId).pipe(
        //load purchase rows
        tap(value => {
          this.form?.reset(value);
          this.supplierId = value.supplierId || 0;

          //load purchase rows
          this.loadPurchaseRows(
            this.purchaseId,
            this.form?.value.supplierId,
            Tab.waitingDelivery
          ).subscribe();
          this.form?.reset(value);

          this.isHideFetchButton.set(true);
          this.form?.purchaseId.disable();
          this.form?.supplierId.disable();

          this.form?.patchValue({
            deliveryDate: new Date(),
            copyQty: true,
          });
        })
      )
    );
  }

  loadPurchaseRows(purchaseDeliveryId: number, supplierId: number, tab: Tab) {
    if (tab == Tab.waitingDelivery) {
      return this.performPurchaseData.load$(
        this.service.getRowsFromPurchase(purchaseDeliveryId, supplierId).pipe(
          tap(value => {
            this.purchaseData.next(value);
          })
        )
      );
    } else if (tab == Tab.delivery) {
      return this.performPurchaseData.load$(
        this.service.getDeliveryRows(purchaseDeliveryId).pipe(
          tap(value => {
            this.purchaseData.next(value);
          })
        )
      );
    } else return of();
  }

  loadSuppliers(): Observable<SmallGenericType[]> {
    return this.performLoadSupplier.load$(
      this.supplierService.getSupplierDict(true, true, false).pipe(
        tap(value => {
          this.supplierOptions.set(value);
          this.form?.patchValue(value);
        })
      )
    );
  }

  loadPurchaseOrders(): Observable<IPurchaseSmallDTO[]> {
    return this.performLoadPurchaseOrders.load$(
      this.purchaseService.getOpenPurchasesForSelect(true).pipe(
        tap(value => {
          this.loadPurchase = value;
          this.filteredPurchase.set(value);
          this.form?.patchValue(value);
        })
      )
    );
  }

  changeSupplier(supplierId: number | undefined) {
    const filteredPurchase = this.loadPurchase?.filter(
      p => p.supplierId == supplierId
    );
    this.filteredPurchase.set([]);
    if (filteredPurchase) this.filteredPurchase.set(filteredPurchase);
  }

  changePurchase(event: IPurchaseSmallDTO) {
    this.form?.patchValue({
      originDescription: '',
    });
    this.isFetchDisable.set(false);
    this.getOriginDescription();
    const filteredPurchase = this.loadPurchase?.filter(
      p => p.purchaseId == event.purchaseId
    );
    if (filteredPurchase) {
      this.form?.patchValue({ supplierId: filteredPurchase[0].supplierId });
      this.changeSupplier(filteredPurchase[0].supplierId);
    }
    this.form?.markAsPristine();
  }

  getOriginDescription() {
    const description =
      this.loadPurchase?.find(d => d.purchaseId == this.form?.value.purchaseId)
        ?.originDescription ?? '';

    this.form?.patchValue({
      originDescription: description,
    });
  }

  fetchPurchaseRows() {
    return this.performPurchaseData.load(
      this.service.getRowsFromPurchase(this.purchaseId, this.supplierId).pipe(
        tap(values => {
          const finalValues: PurchaseDeliveryRowDTO[] = [];
          values.forEach(obj => {
            if (values) {
              const value = { ...obj, deliveryDate: new Date() };
              if (value) this.isSaveDisable.set(false);
              finalValues.push(value);
              this.form?.markAsDirty();
            }
          });
          this.purchaseData.next(finalValues);
        })
      )
    );
  }

  buildFunctionList() {
    this.menuList.push({
      id: FunctionType.Save,
      label: this.translate.instant('core.save') + ' (Ctrl+S)',
    });
    this.menuList.push({
      id: FunctionType.SaveAndClose,
      label: this.translate.instant('core.saveandclose') + ' (Ctrl+Enter)',
    });

    return of(undefined);
  }

  peformAction(selected: MenuButtonItem): void {
    switch (selected.id) {
      case FunctionType.Save:
        this.performSave();
        break;
      case FunctionType.SaveAndClose:
        this.performSaveAndClose();
        break;
    }
  }

  performSaveAndClose() {
    this.performSave();
    this.additionalSaveProps = { closeTabOnSave: true };
  }

  performSave(options?: ProgressOptions): void {
    if (!this.form || this.form.invalid) return;

    const model = new PurchaseDeliverySaveDTO();
    model.supplierId = this.supplierId;
    model.deliveryDate = this.form?.value.deliveryDate;
    model.purchaseDeliveryId = this.form?.value.purchaseDeliveryId || 0;

    this.purchaseData.value.forEach(purchaseRow => {
      model.rows.push({
        purchaseDeliveryRowId: purchaseRow.purchaseDeliveryRowId,
        deliveredQuantity: purchaseRow.deliveredQuantity,
        deliveryDate: purchaseRow.deliveryDate || new Date(),
        purchasePrice: purchaseRow.purchasePrice || 0,
        purchasePriceCurrency: purchaseRow.purchasePriceCurrency || 0,
        purchaseRowId: purchaseRow.purchaseRowId,
        isModified: purchaseRow.isModified,
        setRowAsDelivered: purchaseRow.remainingQuantity <= 0,
        purchaseNr: purchaseRow.purchaseNr,
      });
    });

    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service.save(model).pipe(tap(this.updateFormValueAndEmitChange)),
      undefined,
      undefined,
      options
    );
  }

  updateFormValueAndEmitChange = (backendResponse: BackendResponse) => {
    const entityId = ResponseUtil.getEntityId(backendResponse);
    const numberValue = ResponseUtil.getNumberValue(backendResponse);
    if (entityId && entityId > 0) {
      this.form!.isNew = false;
      this.isHideFetchButton.set(true);

      this.form?.patchValue({
        purchaseDeliveryId: entityId,
        deliveryNr: numberValue,
      });
      this.form?.markAsPristine();
      this.form?.markAsUntouched();
      this.form?.supplierId.disable();

      this.id = this.form?.value[this.idFieldName];

      this.actionTakenSignal().set({
        rowItemId: numberValue,
        ref: this.ref(),
        type: CrudActionTypeEnum.Save,
        form: this.form,
        additionalProps: this.additionalSaveProps,
        updateGrid: () => {
          return this.service.getGrid!(this.form?.value[this.idFieldName]);
        },
      });

      this.loadData().subscribe();
      this.isSaveDisable.set(true);
    }
  };

  clickRefresh() {
    if (this.form?.getIdControl()?.value > 0) {
      this.loadData().subscribe();
    }
  }

  changeDate(event?: Date) {
    if (this.performPurchaseData.data) {
      const finalValues: PurchaseDeliveryRowDTO[] = [];
      this.purchaseData.value.forEach(obj => {
        const value = { ...obj, deliveryDate: event };
        finalValues.push(value);
      });
      this.purchaseData.next(finalValues);
    }
  }

  checkFinalDelivery(event: boolean) {
    const finalValues: PurchaseDeliveryRowDTO[] = [];
    this.purchaseData.value.forEach(obj => {
      const value = { ...obj, isLocked: event };
      finalValues.push(value);
    });
    this.purchaseData.next(finalValues);
  }

  openSupplier() {
    //TODO: Open economy > supplier edit screen
  }
}
