import { Component, inject, OnInit, signal } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { GridComponent } from '@ui/grid/grid.component';
import { MenuButtonItem } from '@ui/button/menu-button/menu-button.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { map, Observable, take, tap } from 'rxjs';
import { PriceOptimizationService } from '../../services/price-optimization.service';
import {
  Feature,
  SoeEntityState,
  TermGroup,
  TermGroup_PurchaseCartStatus,
} from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import {
  ChangeCartStateModel,
  PurchaseCartDTO,
} from '../../models/price-optimization.model';
import { CrudActionTypeEnum } from '@shared/enums';
import { Perform } from '@shared/util/perform.class';
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

export enum FunctionType {
  CloseCart = 1,
  RemoveCart = 2,
}

@Component({
  selector: 'soe-price-optimization-grid',
  templateUrl: './price-optimization-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class PriceOptimizationGridComponent
  extends GridBaseDirective<PurchaseCartDTO, PriceOptimizationService>
  implements OnInit
{
  service = inject(PriceOptimizationService);
  coreService = inject(CoreService);
  progressService = inject(ProgressService);
  messageboxService = inject(MessageboxService);

  performSaveData = new Perform<BackendResponse>(this.progressService);

  cartStatus: ISmallGenericType[] = [];
  selectedCartStatusIds: number[] = [1];
  menuList: MenuButtonItem[] = [];
  allItemsSelectionId = 3;
  showButtons = signal(false);

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Billing_Price_Optimization,
      'Billing.Purchase.PriceOptimization',
      { lookups: this.loadCartStatus() }
    );

    this.buildFunctionList();
  }

  private loadCartStatus(): Observable<ISmallGenericType[]> {
    return this.performLoadData.load$(
      this.coreService
        .getTermGroupContent(TermGroup.PurchaseCartStatus, false, true, true)
        .pipe(
          tap(x => {
            this.cartStatus = x;
          })
        )
    );
  }

  buildFunctionList() {
    this.menuList = [];
    this.menuList.push(
      {
        id: FunctionType.CloseCart,
        label: this.translate.instant('core.close'),
      },
      {
        id: FunctionType.RemoveCart,
        label: this.translate.instant('core.delete'),
      }
    );
  }

  performAction(selected: MenuButtonItem): void {
    switch (selected.id) {
      case FunctionType.CloseCart:
        this.close();
        break;
      case FunctionType.RemoveCart:
        this.remove();
        break;
    }
  }

  override selectionChanged(rows: PurchaseCartDTO[]): void {
    this.showButtons.set(this.grid.getSelectedCount() > 0);
    this.selectedRows.set(rows);
  }

  private close() {
    const ids: number[] = [];
    this.grid.getSelectedRows().forEach(row => {
      if (row.status === TermGroup_PurchaseCartStatus.Open) {
        row.status = TermGroup_PurchaseCartStatus.Closed;
        row.isModified = true;
        ids.push(row.purchaseCartId);
      }

      const model = new ChangeCartStateModel();
      model.ids = ids;
      model.stateTo = TermGroup_PurchaseCartStatus.Closed;

      this.performSaveData.crud(
        CrudActionTypeEnum.Save,
        this.service.changeStatus(model).pipe(
          tap(res => {
            if (res.success) {
              this.refreshGrid();
            }
          })
        ),
        undefined,
        undefined,
        {
          showToastOnComplete: true,
        }
      );
    });
  }

  private remove() {
    const model: PurchaseCartDTO[] = [];
    this.grid.getSelectedRows().forEach(row => {
      row.state = SoeEntityState.Deleted;
      row.isModified = true;

      model.push(row);
    });

    const mb = this.messageboxService.question(
      'core.info',
      this.translate
        .instant('billing.purchase.priceoptimization.bulkdeletewarning')
        .replace('{0}', this.grid.getSelectedCount().toString())
    );
    mb.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
      if (response?.result) {
        this.performSaveData.crud(
          CrudActionTypeEnum.Save,
          this.service.deletePurchaseCarts(model).pipe(
            tap(res => {
              if (res.success) {
                this.refreshGrid();
              }
            })
          ),
          undefined,
          undefined,
          {
            showToastOnComplete: true,
          }
        );
      }
    });
  }

  filterChange(event: any = null) {
    this.allItemsSelectionId = event?.allItemsSelectionId ?? 0;
    this.selectedCartStatusIds = event?.selectedCartStatusIds ?? [];
    this.loadData().subscribe();
  }

  override loadData(
    id?: number,
    additionalProps?: {
      allItemsSelectionId?: number;
      selectedCartStatusIds?: number[];
    }
  ): Observable<PurchaseCartDTO[]> {
    return this.performLoadData.load$(
      this.service
        .getGrid(id, {
          allItemsSelectionId: this.allItemsSelectionId,
          selectedCartStatusIds: this.selectedCartStatusIds,
        })
        .pipe(
          map(data => {
            data.forEach(row => {
              row.statusName =
                this.cartStatus.find(c => c.id == row.status)?.name || '';
            });

            this.grid.setData(data);
            this.grid.refreshCells();
            return data;
          })
        )
    );
  }

  override onGridReadyToDefine(grid: GridComponent<PurchaseCartDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'billing.purchase.priceoptimization.sequencenumber',
        'billing.purchase.priceoptimization.internalnote',
        'common.createdby',
        'billing.purchase.priceoptimization.createddate',
        'billing.purchase.priceoptimization.lastmodifiedby',
        'billing.purchase.priceoptimization.lastmodifieddate',
        'common.name',
        'common.status',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.enableRowSelection();

        this.grid.addColumnText(
          'seqNr',
          terms['billing.purchase.priceoptimization.sequencenumber'],
          {
            flex: 1,
          }
        );
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 1,
        });
        this.grid.addColumnText('statusName', terms['common.status'], {
          flex: 1,
        });
        this.grid.addColumnText(
          'description',
          terms['billing.purchase.priceoptimization.internalnote'],
          {
            flex: 1,
          }
        );
        this.grid.addColumnText('createdBy', terms['common.createdby'], {
          flex: 1,
        });

        this.grid.addColumnDate(
          'created',
          terms['billing.purchase.priceoptimization.createddate'],
          {
            flex: 1,
          }
        );
        this.grid.addColumnText(
          'modifiedBy',
          terms['billing.purchase.priceoptimization.lastmodifiedby'],
          {
            flex: 1,
          }
        );
        this.grid.addColumnDate(
          'modified',
          terms['billing.purchase.priceoptimization.lastmodifieddate'],
          {
            flex: 1,
          }
        );

        this.grid.addColumnIconEdit({ onClick: r => this.edit(r) });
      });

    super.finalizeInitGrid();
  }
}
