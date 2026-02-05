import { Component, Input, OnInit, inject, input, signal } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import {
  IAccountDTO,
  IAccountDimSmallDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { StringKeyOfNumberProperty } from '@shared/types';
import { AggregationType } from '@ui/grid/interfaces';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import {
  CellPosition,
  CellValueChangedEvent,
  ColDef,
  TabToNextCellParams,
} from 'ag-grid-community';
import { orderBy } from 'lodash';
import { BehaviorSubject, Observable, merge, of, take, tap } from 'rxjs';
import { DistributionCodeHeadDTO } from 'src/app/features/economy/distribution-codes/models/distribution-codes.model';
import { BudgetForm } from '../../../models/budget-form.model';
import { BudgetRowFlattenedDTO } from '../../../models/budget.model';

@Component({
  selector: 'soe-budget-edit-grid',
  templateUrl: './budget-edit-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class BudgetEditGridComponent
  extends GridBaseDirective<BudgetRowFlattenedDTO>
  implements OnInit
{
  @Input({ required: true }) form!: BudgetForm;
  @Input() rows = new BehaviorSubject<BudgetRowFlattenedDTO[]>([]);

  @Input() nofPeriod: number = 12;
  @Input() distCodeDict!: DistributionCodeHeadDTO[];

  isGridDisabled = input(false);

  private readonly coreService = inject(CoreService);

  accountDims!: IAccountDimSmallDTO[];
  readonly maxBudgetGridPeriodCols: number = 18;

  private allColumns: ColDef[] = [];

  constructor() {
    super();
  }

  override ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Economy_Accounting_Budget_Edit,
      'Economy.Accounting.Budget.Rows',
      {
        skipInitialLoad: true,
        useLegacyToolbar: true,
        lookups: [this.loadAccounts(true)],
      }
    );

    merge(
      this.form.dim2Id.valueChanges,
      this.form.dim3Id.valueChanges,
      this.form.noOfPeriods.valueChanges
    ).subscribe(() => {
      this.resetColumns();
    });
  }

  override createLegacyGridToolbar(): void {
    super.createLegacyGridToolbar({
      hideReload: true,
      hideClearFilters: true,
    });

    this.toolbarUtils.createLegacyGroup({
      buttons: [
        this.toolbarUtils.createLegacyButton({
          icon: 'plus',
          label: 'common.newrow',
          title: 'common.newrow',
          onClick: () => this.addNewRow(),
          disabled: this.isGridDisabled,
          hidden: signal(false),
        }),
      ],
    });
  }

  onGridReadyToDefine(grid: GridComponent<BudgetRowFlattenedDTO>): void {
    super.onGridReadyToDefine(grid);
    this.grid.api.updateGridOptions({
      onCellValueChanged: this.onCellValueChanged.bind(this),
      tabToNextCell: this.onTabToNextCell.bind(this),
    });

    this.translate
      .get([
        'core.delete',
        'common.sum',
        'economy.accounting.distributioncode.distributioncode',
        'economy.accounting.budget.getresultperiod',
        'common.accountingsettings.account',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.terms = terms;
        this.accountDims
          .filter(dim => dim.accountDimNr <= 3)
          .forEach(dim => {
            const dimId = 'dim' + dim.accountDimNr + 'Id';

            if (dim.accountDimNr == 1) {
              this.grid.addColumnAutocomplete<IAccountDTO>(
                dimId as StringKeyOfNumberProperty<BudgetRowFlattenedDTO>,
                dim.name,
                {
                  editable: () => {
                    return !this.isGridDisabled();
                  },
                  enableHiding: true,
                  source: () => orderBy(dim.accounts, a => a.accountNr),
                  scrollable: true,
                  optionIdField: 'accountId',
                  optionNameField: 'numberName',
                  optionDisplayNameField: 'dim1Name',
                  limit: 7,
                  suppressFilter: true,
                  suppressFloatingFilter: true,
                  sortable: false,
                  updater: () => {
                    this.form?.markAsDirty();
                    this.form?.markAsTouched();
                  },
                  flex: 1,
                }
              );
            }

            if (dim.accountDimNr == 2) {
              this.grid.addColumnAutocomplete<IAccountDTO>(
                dimId as StringKeyOfNumberProperty<BudgetRowFlattenedDTO>,
                dim.name,
                {
                  editable: () => {
                    return !this.isGridDisabled();
                  },
                  enableHiding: true,
                  source: () => orderBy(dim.accounts, a => a.accountNr),
                  scrollable: true,
                  optionIdField: 'accountId',
                  optionNameField: 'numberName',
                  optionDisplayNameField: 'dim1Name',
                  limit: 7,
                  suppressFilter: true,
                  suppressFloatingFilter: true,
                  sortable: false,
                  updater: () => {
                    this.form?.markAsDirty();
                    this.form?.markAsTouched();
                  },
                  flex: 1,
                }
              );
            }

            if (dim.accountDimNr == 3) {
              this.grid.addColumnAutocomplete<IAccountDTO>(
                dimId as StringKeyOfNumberProperty<BudgetRowFlattenedDTO>,
                dim.name,
                {
                  editable: () => {
                    return !this.isGridDisabled();
                  },
                  enableHiding: true,
                  source: () => orderBy(dim.accounts, a => a.accountNr),
                  scrollable: true,
                  optionIdField: 'accountId',
                  optionNameField: 'numberName',
                  optionDisplayNameField: 'dim1Name',
                  limit: 7,
                  suppressFilter: true,
                  suppressFloatingFilter: true,
                  sortable: false,
                  updater: () => {
                    this.form?.markAsDirty();
                    this.form?.markAsTouched();
                  },
                  flex: 1,
                }
              );
            }
          });

        this.grid.addColumnAutocomplete<DistributionCodeHeadDTO>(
          'distributionCodeHeadId',
          terms['economy.accounting.distributioncode.distributioncode'],
          {
            flex: 1,
            editable: () => {
              return !this.isGridDisabled();
            },
            source: () => this.distCodeDict,
            optionIdField: 'distributionCodeHeadId',
            optionNameField: 'name',
            suppressFilter: true,
            suppressFloatingFilter: true,
          }
        );
        this.grid.addColumnNumber('totalAmount', terms['common.sum'], {
          enableHiding: true,
          decimals: 2,
          editable: () => {
            return !this.isGridDisabled();
          },
          sortable: false,
          flex: 1,
          suppressFilter: true,
          suppressFloatingFilter: true,
        });

        this.addPeriodColumnsToGrid();

        this.grid.addColumnIconDelete({
          tooltip: terms['core.delete'],
          suppressFilter: true,
          suppressFloatingFilter: true,
          editable: false,
          showIcon: () => {
            return !this.isGridDisabled();
          },
          onClick: row => {
            this.deleteRow(row);
          },
        });

        this.grid.addAggregationsRow({
          totalAmount: AggregationType.Sum,
          amount1: AggregationType.Sum,
          amount2: AggregationType.Sum,
          amount3: AggregationType.Sum,
          amount4: AggregationType.Sum,
          amount5: AggregationType.Sum,
          amount6: AggregationType.Sum,
          amount7: AggregationType.Sum,
          amount8: AggregationType.Sum,
          amount9: AggregationType.Sum,
          amount10: AggregationType.Sum,
          amount11: AggregationType.Sum,
          amount12: AggregationType.Sum,
          amount13: AggregationType.Sum,
          amount14: AggregationType.Sum,
          amount15: AggregationType.Sum,
          amount16: AggregationType.Sum,
          amount17: AggregationType.Sum,
          amount18: AggregationType.Sum,
        });

        this.grid.context.suppressFiltering = true;
        super.finalizeInitGrid();

        this.allColumns = this.grid.columns;
      });
  }

  private addPeriodColumnsToGrid() {
    if (!this.grid) return;

    if (this.nofPeriod > this.maxBudgetGridPeriodCols) {
      this.nofPeriod = this.maxBudgetGridPeriodCols;
    }

    for (let i = 1; i <= this.maxBudgetGridPeriodCols; i++) {
      this.grid.addColumnNumber('amount' + i.toString(), i.toString(), {
        enableHiding: true,
        editable: () => {
          return !this.isGridDisabled();
        },
        decimals: 2,
        flex: 1,
        sortable: false,
        aggFuncOnGrouping: 'sum',
        suppressFilter: true,
        suppressFloatingFilter: true,
      });
    }
  }

  private resetColumns(): void {
    if (this.grid && this.allColumns.length) {
      let columns: ColDef[] = this.allColumns;

      if (!this.form.useDim2.value) {
        columns = columns.filter(c => c.field !== 'dim2Id');
      }

      if (!this.form.useDim3.value) {
        columns = columns.filter(c => c.field !== 'dim3Id');
      }

      let periods = 12;

      if (!isNaN(Number(this.form.noOfPeriods.value))) {
        periods = Number(this.form.noOfPeriods.value);
      }

      columns = columns.filter(c => {
        let addColumn = true;

        if (c.field!.includes('amount')) {
          const fieldNumberStr = c.field!.replace('amount', '');
          const fieldNumber = Number(fieldNumberStr);
          if (!isNaN(fieldNumber)) {
            if (fieldNumber > periods) {
              addColumn = false;
            }
          }
        }

        return addColumn;
      });

      this.grid.columns = columns;
      this.grid.resetColumns();
      this.grid.resetAggregationGridColumns();
    }
  }

  private loadAccounts(useCache: boolean): Observable<IAccountDimSmallDTO[]> {
    return this.coreService
      .getAccountDimsSmall(
        false,
        false,
        true,
        false,
        true,
        false,
        false,
        true,
        useCache
      )
      .pipe(tap(x => (this.accountDims = x)));
  }

  private addNewRow() {
    const newRow = this.form?.addBudgetRow();
    this.rows.next(this.form?.rows.value);
    this.grid.addRow(newRow);

    setTimeout(() => {
      this.grid.api.startEditingCell({
        rowIndex: this.grid.api.getDisplayedRowCount() - 1,
        colKey: 'dim1Id',
      });
    }, 10);
  }

  private deleteRow(row: BudgetRowFlattenedDTO): void {
    this.form?.deleteBudgetRow(row);

    const _rows = this.form?.rows.value.filter(x => !x.isDeleted);
    this.rows.next(_rows);
  }

  onCellValueChanged(event: CellValueChangedEvent) {
    if (
      event.colDef.field === 'totalAmount' ||
      event.colDef.field === 'distributionCodeHeadId'
    ) {
      if (event.data.distributionCodeHeadId) {
        const objDist = this.distCodeDict.filter(
          d => d.distributionCodeHeadId === event.data.distributionCodeHeadId
        )[0];

        this.form?.resetAmounts(event.data);
        for (let i = 1; i < objDist.noOfPeriods + 1; i++) {
          const periodA = objDist.periods[i - 1];
          if (periodA) {
            event.data['amount' + i] =
              (event.data.totalAmount * periodA.percent) / 100;
          }
        }
      } else {
        this.form?.resetAmounts(event.data);
        const periodValue = event.data.totalAmount / this.nofPeriod;
        for (let i = 1; i < this.nofPeriod + 1; i++) {
          event.data['amount' + i] = periodValue;
        }
      }
    }

    event.data.totalAmount = this.form?.setDirtyOnbudgetRowChange(event.data);

    event.api.refreshCells();
    event.node.setData(event.data);
  }

  private onTabToNextCell(event: TabToNextCellParams): CellPosition | boolean {
    if (
      !event.backwards &&
      event.nextCellPosition?.column.isPinned() &&
      event.previousCellPosition.rowIndex ===
        event.api.getLastDisplayedRowIndex()
    ) {
      this.addNewRow();
    } else if (event.nextCellPosition) {
      // let method onCellValueChanged run before editing starts
      const columnId = event.nextCellPosition.column.getId();
      const rowIndex = event.nextCellPosition.rowIndex;
      setTimeout(() => {
        this.grid.startEditing(rowIndex, columnId);
      }, 0);
    }

    return false;
  }
}
