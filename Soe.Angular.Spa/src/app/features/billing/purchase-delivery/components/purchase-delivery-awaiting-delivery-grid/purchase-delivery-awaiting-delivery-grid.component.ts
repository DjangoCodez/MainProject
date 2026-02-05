import { Component, OnInit, inject } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import {
  Feature,
  SoeOriginStatus,
  TermGroup_ChangeStatusGridAllItemsSelection,
} from '@shared/models/generated-interfaces/Enumerations';
import { IPurchaseGridDTO } from '@shared/models/generated-interfaces/PurchaseDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, of, take, tap } from 'rxjs';
import { PurchaseService } from '../../../purchase/services/purchase.service';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { PurchaseEditComponent } from '../../../purchase/components/purchase-edit/purchase-edit.component';
import { PurchaseDeliveryEditComponent } from '../purchase-delivery-edit/purchase-delivery-edit.component';
import { PurchaseForm } from '../../../purchase/models/purchase-form.model';
import { PurchaseDeliveryForm } from '../../models/purchase-delivery-form.model';

@Component({
  selector: 'soe-purchase-delivery-awaiting-delivery-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class PurchaseDeliveryAwaitingDeliveryGridComponent
  extends GridBaseDirective<IPurchaseGridDTO, PurchaseService>
  implements OnInit
{
  progressService = inject(ProgressService);
  service = inject(PurchaseService);
  performGridLoad = new Perform<IPurchaseGridDTO[]>(this.progressService);
  performStatusLoad = new Perform<ISmallGenericType[]>(this.progressService);
  selectedPurchaseStatus: number[] = [];

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Billing_Purchase_Delivery_List,
      'billing.purchase.delivery.awaitingdelivery',
      {
        lookups: this.loadPurchaseStatus(),
      }
    );
  }

  override onGridReadyToDefine(grid: GridComponent<IPurchaseGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'billing.purchase.purchasenr',
        'billing.purchase.supplierno',
        'billing.purchase.suppliername',
        'billing.purchase.purchasedate',
        'core.edit',
        'billing.purchase.origindescription',
        'billing.purchase.delivery.new_delivery',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText(
          'purchaseNr',
          terms['billing.purchase.purchasenr'],
          {
            flex: 1,
            enableHiding: false,
          }
        );
        this.grid.addColumnText(
          'supplierNr',
          terms['billing.purchase.supplierno'],
          {
            flex: 1,
            enableHiding: false,
          }
        );
        this.grid.addColumnText(
          'supplierName',
          terms['billing.purchase.suppliername'],
          {
            flex: 1,
            enableHiding: false,
          }
        );
        this.grid.addColumnDate(
          'purchaseDate',
          terms['billing.purchase.purchasedate'],
          {
            flex: 1,
            enableHiding: false,
          }
        );
        this.grid.addColumnText(
          'origindescription',
          terms['billing.purchase.origindescription'],
          {
            flex: 1,
            enableHiding: false,
          }
        );
        this.grid.addColumnIcon(null, '', {
          width: 25,
          maxWidth: 25,
          minWidth: 25,
          pinned: 'right',
          iconName: 'plus',
          tooltip: terms['billing.purchase.delivery.new_delivery'],
          suppressFilter: true,
          onClick: row => {
            this.edit(
              {
                ...row,
                purchaseNr: row.purchaseNr,
                purchaseId: row.purchaseId,
              },
              {
                filteredRows: [],
                editComponent: PurchaseDeliveryEditComponent,
                editTabLabel: 'billing.purchase.delivery.new_delivery',
                FormClass: PurchaseDeliveryForm,
                gridIndex: 1,
              }
            );
          },
        });
        this.grid.addColumnIconEdit({
          iconName: 'pencil',
          iconClass: 'pencil',
          tooltip: terms['core.edit'],
          onClick: row => {
            this.edit(
              {
                ...row,
                purchaseNr: row.purchaseNr,
                purchaseId: row.purchaseId,
              },
              {
                filteredRows: [],
                editComponent: PurchaseEditComponent,
                editTabLabel: 'billing.purchase.list.purchase',
                FormClass: PurchaseForm,
              }
            );
          },
        });

        super.finalizeInitGrid();
      });
  }

  private loadPurchaseStatus() {
    return this.performStatusLoad.load$(
      this.service.getPurchaseStatus().pipe(
        tap(data => {
          data.forEach(s => {
            if (
              !(
                s.id == SoeOriginStatus.PurchaseDeliveryCompleted ||
                s.id == SoeOriginStatus.Origin
              )
            ) {
              this.selectedPurchaseStatus.push(s.id);
            }
          });
          this.refreshGrid();
        })
      )
    );
  }

  override loadData(id?: number | undefined): Observable<IPurchaseGridDTO[]> {
    if (!this.selectedPurchaseStatus.length) return of([]);

    return this.performGridLoad.load$(
      this.service.getGrid(undefined, {
        allItemsSelection: TermGroup_ChangeStatusGridAllItemsSelection.All,
        selectedPurchaseStatusIds: this.selectedPurchaseStatus,
      })
    );
  }
}
