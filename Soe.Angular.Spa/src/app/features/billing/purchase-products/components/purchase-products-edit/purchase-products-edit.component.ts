import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  inject,
} from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { PurchaseProductsService } from '../../services/purchase-products.service';
import {
  ProductTypeheadDTO,
  SupplierProductDTO,
  SupplierProductPriceDTO,
} from '../../models/purchase-product.model';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { SupplierService } from '@src/app/features/economy/services/supplier.service';
import { ProductUnitService } from '../../../product-units/services/product-unit.service';
import { ProductService } from '../../../products/services/product.service';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { IProductUnitSmallDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { PurchaseProductForm } from '../../models/purchase-product-form.model';
import { ISupplierProductPriceDTO } from '@shared/models/generated-interfaces/SupplierProductDTOs';
import { BillingService } from '../../../services/services/billing.service';
import { orderBy } from 'lodash';
import { CrudActionTypeEnum } from '@shared/enums';
import { ProgressOptions } from '@shared/services/progress';
import { ProductUnitSmallDTO } from '../../../product-units/models/product-units.model';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'soe-purchase-products-edit',
  templateUrl: './purchase-products-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class PurchaseProductsEditComponent
  extends EditBaseDirective<
    SupplierProductDTO,
    PurchaseProductsService,
    PurchaseProductForm
  >
  implements OnInit
{
  private readonly supplierService = inject(SupplierService);
  private readonly productService = inject(ProductService);
  private readonly productUnitService = inject(ProductUnitService);
  private readonly billingService = inject(BillingService);
  service = inject(PurchaseProductsService);
  suppliers: SmallGenericType[] = [];
  products: ProductTypeheadDTO[] = [];
  units: IProductUnitSmallDTO[] = [];

  purchaseData = new BehaviorSubject<SupplierProductPriceDTO[]>([]);
  purchaseSaveData: ISupplierProductPriceDTO[] = [];

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(Feature.Billing_Purchase_Products, {
      additionalReadPermissions: [
        Feature.Billing_Product_Products_Edit,
        Feature.Economy_Supplier_Suppliers_Edit,
      ],
      additionalModifyPermissions: [
        Feature.Billing_Product_Products_Edit,
        Feature.Economy_Supplier_Suppliers_Edit,
      ],
      lookups: [this.loadSuppliers(), this.loadProducts(), this.loadUnits()],
      skipDefaultToolbar: true,
    });

    this.form?.productId.valueChanges.subscribe(value => {
      const product = this.products.find(x => x.productId == value);
      if (product) {
        this.itemChanged(product);
      }
    });
  }

  override loadData(): Observable<void> {
    return this.performLoadData.load$(
      this.service.get(this.form?.getIdControl()?.value).pipe(
        tap(value => {
          this.form?.reset(value);
          this.loadProductDetails(value);

          if ((this.form?.getIdControl()?.value as number) > 0)
            this.form?.supplierId.disable();
          this.loadPriceRows();
        })
      )
    );
  }

  override onFinished(): void {
    super.onFinished();
    const parentProductInfo = this.productService.parentProductContext;
    if (
      this.form?.getIdControl()?.value === 0 &&
      !this.form?.isCopy &&
      parentProductInfo
    ) {
      this.form.patchValue({
        productId: parentProductInfo.productId,
        itemName: parentProductInfo.name,
        itemUnit: this.filterSelectedUnit(parentProductInfo.productUnitId),
      });
      this.productService.parentProductContext = undefined;
      this.form.markAsDirty();
    }
  }

  loadProductDetails(values: SupplierProductDTO) {
    const selectedProduct = this.products.filter(
      r => r.productId == values.productId
    );

    if (selectedProduct.length > 0) {
      this.form?.patchSelectedItemValues(
        selectedProduct[0]?.name,
        this.filterSelectedUnit(selectedProduct[0].productUnitId)
      );
    }
  }

  filterSelectedUnit(selectedUnitId?: number) {
    if (selectedUnitId)
      return this.units.filter(u => u.productUnitId == selectedUnitId)[0].name;
    else return '';
  }

  loadPriceRows() {
    this.performLoadData
      .load$(
        this.billingService.getSupplierProductPricesGrid(
          this.form?.supplierProductId.value
        )
      )
      .subscribe(value => {
        this.purchaseData.next(
          orderBy(value, ['currencyCode', 'quantity', 'startDate'])
        );
      });
  }

  loadSuppliers(): Observable<SmallGenericType[]> {
    return this.performLoadData.load$(
      this.supplierService.getSupplierDict(true, false, false).pipe(
        tap(x => {
          this.suppliers = x;
        })
      )
    );
  }

  loadUnits(): Observable<ProductUnitSmallDTO[]> {
    return this.performLoadData.load$(
      this.productUnitService.getGrid(undefined, { useCache: true }).pipe(
        tap(u => {
          this.units = u;
        })
      )
    );
  }

  loadProducts(): Observable<ProductTypeheadDTO[]> {
    return this.performLoadData.load$(
      this.productService.getProductForSelect().pipe(
        tap(p => {
          this.products = p;
        })
      )
    );
  }

  protected itemChanged(value: ProductTypeheadDTO) {
    this.form?.patchSelectedItemValues(
      value.name,
      this.filterSelectedUnit(value.productUnitId)
    );
  }

  // isDirty(value: any) {
  //   if (value) this.form?.markAsDirty();
  // }

  updateModifiedRows(value: ISupplierProductPriceDTO[]) {
    this.purchaseSaveData = value;
  }

  performSave(options?: ProgressOptions): void {
    const model = <SupplierProductDTO>this.form?.getRawValue();
    model.priceRows = this.purchaseSaveData.map(
      priceRow => priceRow as SupplierProductPriceDTO
    );

    this.additionalSaveProps = {
      ...this.additionalSaveProps,
      productId: model.productId,
    };

    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service.save(model).pipe(tap(this.updateFormValueAndEmitChange)),
      undefined,
      undefined,
      options
    );
  }
}
