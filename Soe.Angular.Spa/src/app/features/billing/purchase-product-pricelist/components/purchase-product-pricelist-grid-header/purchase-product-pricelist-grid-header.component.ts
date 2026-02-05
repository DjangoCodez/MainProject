import { Component, EventEmitter, Output, inject } from '@angular/core';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { Perform } from '@shared/util/perform.class';
import { ValidationHandler } from '@shared/handlers';
import { ProgressService } from '@shared/services/progress/progress.service';
import { SupplierProductPriceListGridHeaderForm } from '../../models/purchase-product-pricelist-grid-header-form.model';
import { SupplierService } from '../../../../economy/services/supplier.service';
import { tap } from 'rxjs';
import { SupplierProductPriceListGridHeaderDTO } from '../../models/purchase-product-pricelist.model';

@Component({
    selector: 'soe-purchase-product-pricelist-grid-header',
    templateUrl: './purchase-product-pricelist-grid-header.component.html',
    standalone: false
})
export class PurchaseProductPricelistGridHeaderComponent {
  @Output() onSearchClick = new EventEmitter<number>();

  validationHandler = inject(ValidationHandler);
  supplierService = inject(SupplierService);
  suppliersDict: SmallGenericType[] = [];
  performLoadSuppliers = new Perform<SmallGenericType[]>(this.progressService);

  formSearch: SupplierProductPriceListGridHeaderForm =
    new SupplierProductPriceListGridHeaderForm({
      validationHandler: this.validationHandler,
      element: new SupplierProductPriceListGridHeaderDTO(),
    });

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
    const searchDto = this.formSearch
      .value as SupplierProductPriceListGridHeaderDTO;
    this.onSearchClick.emit(searchDto.supplierId);
  }
}
