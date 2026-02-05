import { Component, inject, signal } from '@angular/core';
import { FormControl } from '@angular/forms';
import { TranslateService } from '@ngx-translate/core';
import { FileUploadDialogComponent } from '@shared/components/file-upload-dialog/file-upload-dialog.component';
import { IProductUnitConvertDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { AttachedFile } from '@ui/forms/file-upload/file-upload.component'
import { ColumnUtil } from '@ui/grid/util/column-util'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { GridComponent } from '@ui/grid/grid.component';
import { BehaviorSubject, take } from 'rxjs';
import { ProductUnitConversionDialogData } from './models/products-unit-conversion-dialog.models';
import { ProductsUnitConversionService } from './services/products-unit-conversion-dialog.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { CrudActionTypeEnum } from '@shared/enums';
import { ColDef } from 'ag-grid-enterprise';
import { fileUploadDialogData } from '@shared/components/file-upload-dialog/models/file-upload-dialog.model';
import { DialogService } from '@ui/dialog/services/dialog.service';

@Component({
  selector: 'soe-products-unit-conversion',
  templateUrl: './products-unit-conversion-dialog.component.html',
  styleUrls: ['./products-unit-conversion-dialog.component.scss'],
  standalone: false,
})
export class ProductsUnitConversionComponent extends DialogComponent<ProductUnitConversionDialogData> {
  private readonly translate = inject(TranslateService);
  private readonly productUnitService = inject(ProductsUnitConversionService);
  private readonly progressService = inject(ProgressService);
  private readonly perform = new Perform(this.progressService);
  private readonly dialogService = inject(DialogService);
  protected importFileName = new FormControl('');
  protected gridRef?: GridComponent<IProductUnitConvertDTO>;
  protected gridColumns: ColDef[] = [];
  protected gridRows = new BehaviorSubject<IProductUnitConvertDTO[]>([]);
  protected rowSelected = signal(false);

  constructor() {
    super();
    this.importFileName.disable();
    this.setGridColumns();
  }

  private setGridColumns(): void {
    this.translate
      .get([
        'billing.productrows.productnr',
        'common.name',
        'billing.product.productunit.unitfrom',
        'billing.product.productunit.unitto',
        'billing.product.productunit.convertfactor',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.gridColumns = [
          ColumnUtil.createColumnText(
            'productNr',
            terms['billing.productrows.productnr'],
            {
              enableHiding: false,
              suppressFilter: true,
              flex: 1,
              suppressSizeToFit: true,
              minWidth: 100,
            }
          ),
          ColumnUtil.createColumnText('productName', terms['common.name'], {
            enableHiding: false,
            suppressFilter: true,
            flex: 1,
            suppressSizeToFit: true,
            minWidth: 100,
          }),
          ColumnUtil.createColumnText(
            'baseProductUnitName',
            terms['billing.product.productunit.unitfrom'],
            {
              enableHiding: false,
              suppressFilter: true,
              flex: 1,
              suppressSizeToFit: true,
              minWidth: 100,
            }
          ),
          ColumnUtil.createColumnText(
            'productUnitName',
            terms['billing.product.productunit.unitto'],
            {
              enableHiding: false,
              suppressFilter: true,
              flex: 1,
              suppressSizeToFit: true,
              minWidth: 100,
            }
          ),
          ColumnUtil.createColumnNumber(
            'convertFactor',
            terms['billing.product.productunit.convertfactor'],
            {
              enableHiding: false,
              suppressFilter: true,
              flex: 1,
              suppressSizeToFit: true,
              minWidth: 100,
              decimals: 2,
            }
          ),
        ];
      });
  }

  protected openFileImportDialog(): void {
    this.dialogService
      .open(
        FileUploadDialogComponent,
        fileUploadDialogData({ multipleFiles: false, asBinary: false })
      )
      .afterClosed()
      .subscribe((result: unknown) => {
        if (result !== undefined && result !== false) {
          const uploadFile = <AttachedFile>result;
          this.importFileName.setValue(uploadFile.name ?? '');

          this.perform
            .load$(
              this.productUnitService.parseUnitConversionFile(
                this.data.productIds,
                uploadFile.content
              )
            )
            .subscribe((result): void => {
              this.gridRows.next(result as IProductUnitConvertDTO[]);
            });
        }
      });
  }

  protected importRows(): void {
    const rows = <IProductUnitConvertDTO[]>this.gridRef?.getSelectedRows();
    if (rows?.length > 0) {
      this.perform.crud(
        CrudActionTypeEnum.Work,
        this.productUnitService.saveProductUnitConvert(rows),
        result => {
          if (result.success) this.dialogRef.close(true);
        }
      );
    }
  }

  protected rowSelectionChanged(rows: IProductUnitConvertDTO[]): void {
    this.rowSelected.set(rows.length > 0);
  }
}
