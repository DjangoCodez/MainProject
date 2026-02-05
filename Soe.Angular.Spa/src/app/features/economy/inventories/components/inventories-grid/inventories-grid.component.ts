import { Component, inject, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { ValidationHandler } from '@shared/handlers';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  Feature,
  TermGroup,
  TermGroup_InventoryStatus,
} from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { IInventoryGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { AggregationType } from '@ui/grid/interfaces';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, of, take, tap } from 'rxjs';
import { InventoryWriteOffMethodsService } from '../../../inventory-write-off-methods/services/inventory-write-off-methods.service';
import { InventoriesFilterForm } from '../../models/inventories-filter-form.model';
import { InventoriesForm } from '../../models/inventories-form.model';
import {
  InventoryDTO,
  InventoryFilterDTO,
} from '../../models/inventories.model';
import { InventoriesService } from '../../services/inventories.service';
import { TwoValueCellRenderer } from '@ui/grid/cell-renderers/two-value-cell-renderer/two-value-cell-renderer.component';

@Component({
  selector: 'soe-inventories-grid',
  templateUrl: './inventories-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class InventoriesGridComponent
  extends GridBaseDirective<IInventoryGridDTO, InventoriesService>
  implements OnInit
{
  validationHandler = inject(ValidationHandler);

  form: InventoriesForm = new InventoriesForm({
    validationHandler: this.validationHandler,
    element: new InventoryDTO(),
  });

  service = inject(InventoriesService);
  coreService = inject(CoreService);
  writeOffMethodsService = inject(InventoryWriteOffMethodsService);
  progressService = inject(ProgressService);

  performLoad = new Perform<IInventoryGridDTO[]>(this.progressService);

  private inventoryStatuses: ISmallGenericType[] = [];
  selectedInventoryStatuses = [
    TermGroup_InventoryStatus.Draft,
    TermGroup_InventoryStatus.Active,
    TermGroup_InventoryStatus.Discarded,
    TermGroup_InventoryStatus.Sold,
    TermGroup_InventoryStatus.Inactive,
    TermGroup_InventoryStatus.WrittenOff,
  ].join(',');
  selectedInventoryStatusesId: number[] = [];
  statusName: SmallGenericType[] = [];
  writeOffMethods: Array<ISmallGenericType> = [];

  formFilter: InventoriesFilterForm = new InventoriesFilterForm({
    validationHandler: this.validationHandler,
    element: new InventoryFilterDTO(),
  });

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Economy_Inventory_Inventories,
      'Economy.Inventory.Inventories',
      {
        lookups: [
          this.loadInventoryWriteOffMethods(),
          this.loadInventoryStatuses(),
        ],
        skipInitialLoad: true,
      }
    );
  }

  override onGridReadyToDefine(grid: GridComponent<IInventoryGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.name',
        'common.description',
        'core.edit',
        'economy.inventory.inventories.status',
        'economy.inventory.inventories.inventorynr',
        'economy.inventory.inventories.accountnr',
        'economy.inventory.inventories.accountname',
        'economy.inventory.inventories.writeoffamount',
        'economy.inventory.inventories.writeoffremainingamount',
        'economy.inventory.inventories.purchasedate',
        'economy.inventory.inventories.purchaseamount',
        'economy.inventory.inventories.writeoffsum',
        'economy.inventory.inventories.accwriteoffamount',
        'economy.inventory.inventories.endamount',
        'economy.inventory.inventorywriteoffmethods.inventorywriteoffmethod',
        'economy.inventory.inventories.categories',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText(
          'inventoryNr',
          terms['economy.inventory.inventories.inventorynr'],
          { flex: 1, enableHiding: false }
        );
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 1,
          enableHiding: false,
        });
        this.grid.addColumnSelect(
          'status',
          terms['economy.inventory.inventories.status'],
          this.inventoryStatuses,
          null,
          { flex: 1, enableHiding: false }
        );
        this.grid.addColumnText(
          'inventoryAccountNumberName',
          terms['economy.inventory.inventories.accountnr'],
          {
            flex: 1,
            enableHiding: false,
            cellRenderer: TwoValueCellRenderer,
            cellRendererParams: {
              primaryValueKey: 'inventoryAccountNr',
              secondaryValueKey: 'inventoryAccountName',
            },
          }
        );
        this.grid.addColumnNumber(
          'writeOffAmount',
          terms['economy.inventory.inventories.writeoffamount'],
          { flex: 1, decimals: 2, enableHiding: false }
        );
        this.grid.addColumnNumber(
          'writeOffRemainingAmount',
          terms['economy.inventory.inventories.writeoffremainingamount'],
          { flex: 1, decimals: 2, enableHiding: false }
        );
        this.grid.addColumnDate(
          'purchaseDate',
          terms['economy.inventory.inventories.purchasedate'],
          { flex: 1, enableHiding: true }
        );
        this.grid.addColumnNumber(
          'purchaseAmount',
          terms['economy.inventory.inventories.purchaseamount'],
          { flex: 1, decimals: 2, enableHiding: false }
        );
        this.grid.addColumnNumber(
          'writeOffSum',
          terms['economy.inventory.inventories.writeoffsum'],
          { flex: 1, decimals: 2, enableHiding: false }
        );
        this.grid.addColumnNumber(
          'accWriteOffAmount',
          terms['economy.inventory.inventories.accwriteoffamount'],
          { flex: 1, decimals: 2, enableHiding: false }
        );
        this.grid.addColumnNumber(
          'endAmount',
          terms['economy.inventory.inventories.endamount'],
          {
            flex: 1,
            decimals: 2,
            aggFuncOnGrouping: 'sum',
            enableHiding: false,
          }
        );
        this.grid.addColumnText('description', terms['common.description'], {
          flex: 1,
          enableHiding: true,
        });
        this.grid.addColumnText(
          'inventoryWriteOffMethod',
          terms[
            'economy.inventory.inventorywriteoffmethods.inventorywriteoffmethod'
          ],
          { flex: 1, enableHiding: true }
        );
        this.grid.addColumnText(
          'categories',
          terms['economy.inventory.inventories.categories'],
          { flex: 1, enableHiding: true }
        );

        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            this.edit(row);
          },
        });

        this.grid.addAggregationsRow({
          writeOffAmount: AggregationType.Sum,
          writeOffRemainingAmount: AggregationType.Sum,
          purchaseAmount: AggregationType.Sum,
          writeOffSum: AggregationType.Sum,
          accWriteOffAmount: AggregationType.Sum,
          endAmount: AggregationType.Sum,
        });

        super.finalizeInitGrid();
      });
  }

  override loadData(): Observable<IInventoryGridDTO[]> {
    if (this.selectedInventoryStatuses) {
      return this.performLoad.load$(
        this.service.getGrid(undefined, {
          setting: this.selectedInventoryStatuses,
        })
      );
    } else {
      return of([]);
    }
  }

  private loadInventoryWriteOffMethods() {
    return this.writeOffMethodsService.getDict(true).pipe(
      tap(data => {
        this.writeOffMethods = data;
      })
    );
  }

  private loadInventoryStatuses() {
    return this.coreService
      .getTermGroupContent(
        TermGroup.InventoryStatus, // term group
        false, // add empty row
        true, // skip unkown
        true, // sort by id
        true // use cache
      )
      .pipe(tap(data => (this.inventoryStatuses = data)));
  }

  filterOnChange(filterDTO: InventoryFilterDTO): void {
    this.selectedInventoryStatuses = filterDTO.selectedStatusIds.join(',');
    this.selectedInventoryStatusesId = filterDTO.selectedStatusIds;

    if (filterDTO.selectedStatusIds.length > 0) {
      this.refreshGrid();
    }
  }
}
