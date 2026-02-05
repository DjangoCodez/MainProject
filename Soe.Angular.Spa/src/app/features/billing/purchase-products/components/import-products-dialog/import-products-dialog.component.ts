import { Component, OnInit, inject } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';
import { ValidationHandler } from '@shared/handlers';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { DialogData } from '@ui/dialog/models/dialog';
import { SupplierService } from '@src/app/features/economy/services/supplier.service';
import {
  ImportSupplierProductDialogDTO,
  ImportSupplierProductDialogForm,
} from './import-products-dialog.model';
import { ImportDynamicComponent } from '@shared/components/billing/import-dynamic/import-dynamic/import-dynamic.component';
import {
  ImportDyanmicDialogData,
  ImportDynamicDTO,
} from '@shared/components/billing/import-dynamic/import-dynamic/import-dynamic.model';
import { ImportDynamicService } from '@shared/components/billing/import-dynamic/import-dynamic/import-dynamic.service';
import { PurchaseProductPricelistService } from '../../../purchase-product-pricelist/services/purchase-product-pricelist.service';
import {
  IImportDynamicResultDTO,
  IImportOptionsDTO,
  ISupplierProductImportRawDTO,
} from '@shared/models/generated-interfaces/ImportDynamicDTO';
import { ISupplierProductImportDTO } from '@shared/models/generated-interfaces/SupplierProductDTOs';
import { Observable } from 'rxjs';
import { DialogService } from '@ui/dialog/services/dialog.service';

@Component({
  selector: 'soe-import-products-dialog',
  templateUrl: './import-products-dialog.component.html',
  styleUrls: ['./import-products-dialog.component.scss'],
  standalone: false,
})
export class ImportProductsDialogComponent
  extends DialogComponent<DialogData>
  implements OnInit
{
  performSupplierLoad = new Perform<SmallGenericType[]>(this.progressService);
  supplierService = inject(SupplierService);
  importDynamicService = inject(ImportDynamicService);
  dialogRef = inject(MatDialogRef);
  validationHandler = inject(ValidationHandler);
  priceListService = inject(PurchaseProductPricelistService);
  private readonly dialogService = inject(DialogService);
  form: ImportSupplierProductDialogForm = new ImportSupplierProductDialogForm({
    validationHandler: this.validationHandler,
    element: new ImportSupplierProductDialogDTO(),
  });

  constructor(private progressService: ProgressService) {
    super();
  }

  ngOnInit(): void {
    this.loadSuppliers();
  }

  private loadSuppliers(): void {
    this.performSupplierLoad.load(
      this.supplierService.getSupplierDict(true, true, false)
    );
  }

  override closeDialog(): void {
    this.dialogRef.close(false);
  }

  protected triggerOk(): void {
    this.openImportDynamic(this.form.supplierId.value);
  }

  private openImportDynamic(id: number | undefined): void {
    const importCallBack = (
      data: ISupplierProductImportRawDTO[],
      options: IImportOptionsDTO
    ): Observable<IImportDynamicResultDTO> => {
      const model: ISupplierProductImportDTO = {
        importToPriceList: false,
        importPrices: true,
        supplierId: id,
        priceListId: undefined,
        rows: data,
        options: options,
      };

      return this.priceListService.performPricelistImport(model);
    };

    const isMultipleSuppliers = id === null || id?.valueOf() == 0;
    this.importDynamicService
      .getSupplierPricelistImport(false, true, isMultipleSuppliers)
      .subscribe(config => {
        this.dialogService
          .open(ImportDynamicComponent, <ImportDyanmicDialogData>{
            size: 'xl',
            title: 'billing.stock.stocks.importfromfile',
            disableClose: true,
            importDTO: config as ImportDynamicDTO,
            callback: importCallBack,
          })
          .afterClosed()
          .subscribe((): void => this.closeDialog());
      });
  }
}
