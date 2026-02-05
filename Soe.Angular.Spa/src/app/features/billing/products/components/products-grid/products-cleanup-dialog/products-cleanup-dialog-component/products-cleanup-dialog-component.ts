import { Component, inject, OnInit } from '@angular/core';
import { ProductCleanupDialogForm } from '@features/billing/products/models/products-cleanup-dialog-form.model';
import {
  ProductCleanupDTO,
  ProductsCleanupDialogData,
} from '@features/billing/products/models/products-cleanup-dialog.model';
import { ProductService } from '@features/billing/products/services/product.service';
import { CrudActionTypeEnum } from '@shared/enums';
import { ValidationHandler } from '@shared/handlers';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import { ProgressService } from '@shared/services/progress';
import { DateUtil } from '@shared/util/date-util';
import { Perform } from '@shared/util/perform.class';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { BehaviorSubject, tap } from 'rxjs';

@Component({
  selector: 'soe-products-cleanup-dialog-component',
  standalone: false,
  templateUrl: './products-cleanup-dialog-component.html',
})
export class ProductsCleanupDialogComponent
  extends DialogComponent<ProductsCleanupDialogData>
  implements OnInit
{
  private readonly service = inject(ProductService);
  private readonly messageBox = inject(MessageboxService);
  private readonly validationHandler = inject(ValidationHandler);
  progressService = inject(ProgressService);
  performAction = new Perform<BackendResponse>(this.progressService);
  performLoadData = new Perform<ProductCleanupDTO[]>(this.progressService);

  productsRowData = new BehaviorSubject<ProductCleanupDTO[]>([]);
  selectedRowData: ProductCleanupDTO[] = [];

  form: ProductCleanupDialogForm = new ProductCleanupDialogForm({
    validationHandler: this.validationHandler,
    element: { lastUsedDate: new Date() } as ProductCleanupDTO,
  });

  ngOnInit(): void {}

  doSearch(): void {
    const isoDate = DateUtil.format(
      this.form.lastUsedDate.value,
      `yyyyMMdd'T'HHmmss`
    );
    this.performLoadData.load(
      this.service.getProductsForCleanup(isoDate).pipe(
        tap(data => {
          this.productsRowData.next(data);
        })
      )
    );
  }

  onSelectionChanged(selectedRows: ProductCleanupDTO[]): void {
    this.selectedRowData = [];
    this.selectedRowData = selectedRows;
  }

  onDeactivate(): void {
    if (this.selectedRowData.length === 0) return;
    const ids = this.selectedRowData.map(row => row.productId);
    this.messageBox
      .question('core.warning', 'core.confirminactivate')
      .afterClosed()
      .subscribe(res => {
        if (res.result) {
          this.performAction.crud(
            CrudActionTypeEnum.Save,
            this.service.inactivateProducts(ids),
            () => this.doSearch()
          );
        }
      });
  }

  onDelete(): void {
    if (this.selectedRowData.length === 0) return;
    const ids = this.selectedRowData.map(row => row.productId);
    this.messageBox
      .question('core.warning', 'core.confirmdelete')
      .afterClosed()
      .subscribe(res => {
        if (res.result) {
          this.performAction.crud(
            CrudActionTypeEnum.Delete,
            this.service.deleteProducts(ids),
            res => {
              if (res.success) this.doSearch();
            }
          );
        }
      });
  }
}
