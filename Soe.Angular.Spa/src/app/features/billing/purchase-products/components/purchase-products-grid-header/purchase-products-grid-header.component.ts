import { Component, EventEmitter, Output, inject } from '@angular/core';
import { SupplierProductGridHeaderForm } from '../../models/purchase-products-search-form.model';
import { SupplierProductGridHeaderDTO } from '../../models/purchase-product.model';
import { ValidationHandler } from '@shared/handlers';
import { SupplierService } from '../../../../economy/services/supplier.service';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { tap } from 'rxjs';

@Component({
    selector: 'soe-purchase-products-grid-header',
    templateUrl: './purchase-products-grid-header.component.html',
    standalone: false
})
export class PurchaseProductsGridHeaderComponent {
  @Output() onSearchClick = new EventEmitter<SupplierProductGridHeaderDTO>();

  validationHandler = inject(ValidationHandler);
  supplierService = inject(SupplierService);
  suppliersDict: SmallGenericType[] = [];
  performLoadSuppliers = new Perform<SmallGenericType[]>(this.progressService);

  formSearch: SupplierProductGridHeaderForm = new SupplierProductGridHeaderForm(
    {
      validationHandler: this.validationHandler,
      element: new SupplierProductGridHeaderDTO(),
    }
  );
  constructor(private progressService: ProgressService) {
    this.loadSuppliers();
  }

  loadSuppliers() {
    this.performLoadSuppliers.load(
      this.supplierService.getSupplierDict(true, false, true).pipe(
        tap(data => {
          this.suppliersDict = data;
        })
      )
    );
  }

  search(): void {
    const searchDto = this.formSearch.value as SupplierProductGridHeaderDTO;
    this.onSearchClick.emit({
      supplierIds: searchDto.supplierIds,
      supplierProduct: searchDto.supplierProduct,
      supplierProductName: searchDto.supplierProductName,
      product: searchDto.product,
      productName: searchDto.productName,
      invoiceProductId: searchDto.invoiceProductId,
    });
  }
}
