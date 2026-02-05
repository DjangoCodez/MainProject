import {
  Component,
  EventEmitter,
  Input,
  OnInit,
  Output,
} from '@angular/core';
import { ProductCleanupDTO } from '@features/billing/products/models/products-cleanup-dialog.model';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { BehaviorSubject, take } from 'rxjs';

@Component({
  selector: 'soe-products-cleanup-dialog-grid',
  standalone: false,
  templateUrl: './products-cleanup-dialog-grid.html',
  providers: [FlowHandlerService, ToolbarService],
})
export class ProductsCleanupDialogGrid
  extends GridBaseDirective<ProductCleanupDTO>
  implements OnInit
{
  @Input() rows!: BehaviorSubject<ProductCleanupDTO[]>;
  @Output() changeSelection = new EventEmitter<ProductCleanupDTO[]>();

  ngOnInit(): void {
    this.startFlow(Feature.Billing_Product_Products, 'cleanupDialogGrid', {
      skipInitialLoad: true,
    });
  }

  override onGridReadyToDefine(grid: GridComponent<ProductCleanupDTO>): void {
    super.onGridReadyToDefine(grid);
    this.translate
      .get([
        'common.number',
        'common.name',
        'common.active',
        'billing.products.external',
        'billing.products.cleanup.lastuseddate',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.setRowSelection('multiRow');
        this.grid.addColumnText('productNumber', terms['common.number'], {
          flex: 1,
        });
        this.grid.addColumnText('productName', terms['common.name'], {
          flex: 2,
        });
        this.grid.addColumnText(
          'externalStatus',
          terms['billing.products.external'],
          { flex: 1 }
        );
        this.grid.addColumnDate(
          'lastUsedDate',
          terms['billing.products.cleanup.lastuseddate'],
          { flex: 1 }
        );

        this.grid.finalizeInitGrid();
      });
  }

  selectionChanged(selectedRows: ProductCleanupDTO[]): void {
    this.changeSelection.emit(selectedRows);
  }
}
