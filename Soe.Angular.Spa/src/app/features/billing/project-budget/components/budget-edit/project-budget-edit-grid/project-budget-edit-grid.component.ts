import {
  Component,
  EventEmitter,
  Input,
  OnChanges,
  OnInit,
  Output,
  Signal,
  SimpleChanges,
  computed,
  effect,
  inject,
  input,
  signal,
} from '@angular/core';
import { MaterialCodesService } from '@features/billing/material-codes/services/material-codes.service';
import { BudgetRowProjectDTO } from '@features/billing/project-budget/models/project-budget.model';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  CompanySettingType,
  Feature,
  ProjectCentralBudgetRowType,
  SoeTimeCodeType,
  TermGroup_ProjectBudgetPeriodType,
} from '@shared/models/generated-interfaces/Enumerations';
import { ITimeCodeDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { SharedModule } from '@shared/shared.module';
import { DateUtil } from '@shared/util/date-util';
import { SettingsUtil } from '@shared/util/settings-util';
import { AG_NODE, GridComponent } from '@ui/grid/grid.component';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { CellValueChangedEvent, ColDef } from 'ag-grid-community';
import { BehaviorSubject, Observable, map, of, take, tap, timeout } from 'rxjs';
import { ProjectBudgetRowService } from './project-budget-row.service';
import { NOOP_TREE_KEY_MANAGER_FACTORY } from '@angular/cdk/a11y';

export type ProjectBudgetRowActions = null | ResetRows | ResetDateRange;
type ResetRows = {
  type: 'RESET_ROWS';
  rows: BudgetRowProjectDTO[];
  emit?: boolean;
};
type ResetDateRange = {
  type: 'RESET_DATE_RANGE';
  data: DateRangeData;
};

export type DateRangeData = {
  periodType: TermGroup_ProjectBudgetPeriodType;
  fromDate: Date;
  toDate: Date;
};

@Component({
  selector: 'soe-project-budget-edit-grid',
  templateUrl: './project-budget-edit-grid.component.html',
  standalone: true,
  providers: [FlowHandlerService, ToolbarService],
  imports: [SharedModule, GridWrapperComponent],
})
export class ProjectBudgetEditGridComponent
  extends GridBaseDirective<BudgetRowProjectDTO>
  implements OnInit, OnChanges
{
  private readonly materialCodesService = inject(MaterialCodesService);
  private readonly budgetRowService = inject(ProjectBudgetRowService);
  private readonly coreService = inject(CoreService);

  @Input({ required: true }) rowAction!: Signal<ProjectBudgetRowActions>;
  @Output() rowsChanged = new EventEmitter<BudgetRowProjectDTO[]>();
  @Output() dateRangeChanged = new EventEmitter<Date[]>();

  isGridDisabled = input(false);
  dateRangeData = signal<null | DateRangeData>(null);
  dateRange = computed(() => {
    const data = this.dateRangeData();
    if (!data) return [];
    return this.budgetRowService.calculatePeriodRange(
      data.periodType,
      data.fromDate,
      data.toDate
    );
  });
  noPeriodsAreUsed = computed(() => {
    return this.dateRange().length === 0;
  });

  showTotalRows = signal(true);

  overheadCostPerHour = false;
  timeCodes: ITimeCodeDTO[] = [];
  budgetRowTypes: SmallGenericType[] = [];
  rows = new BehaviorSubject<BudgetRowProjectDTO[]>([]);

  loading = true;
  runResetRows = false;

  constructor() {
    super();
    effect(() => {
      const action = this.rowAction();
      if (!action) return;
      if (action.type === 'RESET_ROWS') {
        if (this.loading && (!action.rows || action.rows.length === 0))
          this.runResetRows = true;
        else this.resetRows(action.rows, action.emit ?? true);
      }
      if (action.type === 'RESET_DATE_RANGE') {
        console.log('range action', action.data);
        if (!action.data || action.data === this.dateRangeData()) return;

        this.dateRangeData.set(action.data);
        this.alterPeriodColumns();
      }
    });
  }

  override ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(
      Feature.Economy_Accounting_Budget_Edit,
      'Economy.Accounting.Budget.Rows',
      {
        skipInitialLoad: true,
        useLegacyToolbar: true,
        lookups: [
          this.loadTimeCodes(),
          this.loadBudgetTypes(),
          this.loadCompanySettings(),
        ],
      }
    );
  }

  override onFinished(): void {
    if (this.runResetRows) this.resetRows([], true);
    this.loading = false;
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.nofPeriod && this.grid) {
      this.alterPeriodColumns();
    }
  }

  onGridReadyToDefine(grid: GridComponent<BudgetRowProjectDTO>): void {
    super.onGridReadyToDefine(grid);
    this.grid.api.updateGridOptions({
      onCellValueChanged: this.onCellValueChanged.bind(this),
      isExternalFilterPresent: () => true,
      doesExternalFilterPass: node =>
        this.showTotalRows() || !node.data.isTotalsRow,
    });
    this.grid.setNbrOfRowsToShow(16);
    this.grid.updateGridHeightBasedOnNbrOfRows();

    this.translate
      .get([
        'core.delete',
        'core.comment',
        'common.type',
        'common.quantity',
        'common.amount',
        'billing.projects.list.budgethours',
        'billing.projects.list.budgethourstotal',
        'billing.projects.budget.totalquantity',
        'billing.projects.budget.totalamount',
        'billing.projects.budget.costsubtype',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.terms = terms;

        this.grid.setRowClassCallback(params => {
          if (params.data?.isDeleted) return 'strike-through';
          return '';
        });

        this.grid.addColumnModified('isModified');
        this.grid.addColumnIcon('', '', {
          iconName: 'plus',
          pinned: 'left',
          tooltip: terms['common.newrow'],
          onClick: row => {
            this.addNewRow(row);
          },
          showIcon: row => {
            return !this.isGridDisabled() && row && !row.isTotalsRow;
          },
        });

        this.grid.addColumnSelect(
          'type',
          terms['common.type'],
          this.budgetRowTypes,
          undefined,
          {
            pinned: 'left',
            minWidth: 100,
            flex: 2,
            editable: false,
          }
        );

        this.grid.addColumnAutocomplete<ITimeCodeDTO>(
          'timeCodeId',
          terms['billing.projects.budget.costsubtype'],
          {
            source: value => this.getTimeCodesForRow(value),
            minWidth: 100,
            pinned: 'left',
            optionIdField: 'timeCodeId',
            optionDisplayNameField: 'name',
            editable: params => {
              return (
                !this.isGridDisabled() &&
                this.isSubTypeColumnEditable(params.data!)
              );
            },
          }
        );

        this.grid.addColumnText('comment', this.terms['core.comment'], {
          enableHiding: true,
          sortable: false,
          suppressFilter: true,
          suppressFloatingFilter: true,
          flex: 1,
          minWidth: 100,
          suppressSizeToFit: true,
          editable: params => {
            return !this.isGridDisabled();
          },
        });

        this.grid.addColumnNumber(
          'totalAmount',
          terms['billing.projects.budget.totalamount'],
          {
            enableHiding: true,
            decimals: 2,
            editable: () => {
              return !this.isGridDisabled(); //&& this.noPeriodsAreUsed();
            },
            sortable: false,
            minWidth: 85,
            flex: 1,
            suppressSizeToFit: true,
            aggFuncOnGrouping: 'sumBudgetAmounts',
            suppressFilter: true,
            suppressFloatingFilter: true,
          }
        );

        this.grid.addColumnNumber(
          'totalQuantity',
          terms['billing.projects.list.budgethourstotal'],
          {
            enableHiding: true,
            flex: 1,
            minWidth: 65,
            editable: row => {
              return (
                !this.isGridDisabled() &&
                //this.noPeriodsAreUsed() &&
                row.data?.type === ProjectCentralBudgetRowType.CostPersonell
              );
            },
            sortable: false,
            aggFuncOnGrouping: 'sumBudgetQuantities',
            suppressFilter: true,
            suppressFloatingFilter: true,
          }
        );

        this.addPeriodColumnsToGrid();

        this.grid.addColumnIconDelete({
          tooltip: terms['core.delete'],
          suppressFilter: true,
          suppressFloatingFilter: true,
          editable: false,
          showIcon: data => {
            return !this.isGridDisabled() && this.canBeDeleted(data);
          },
          onClick: row => {
            this.deleteRow(row);
          },
        });

        this.grid.useGrouping({
          stickyGrandTotalRow: 'bottom',
          includeFooter: false,
          includeTotalFooter: false,
          groupSelectsFiltered: false,
          keepColumnsAfterGroup: false,
          selectChildren: false,
          hideGroupPanel: true,
        });

        this.grid.addGroupAggFunction('sumBudgetAmounts', values => {
          return this.sumAmounts(values);
        });

        this.grid.addGroupAggFunction('sumBudgetQuantities', values => {
          return this.sumQuantities(values);
        });

        this.grid.context.suppressFiltering = true;
        super.finalizeInitGrid();
      });
  }

  private sumAmounts(params: any) {
    if (!params || !params.colDef || !params.colDef.field) return 0;

    let sum = 0;
    if (this.isAmountColumn(params.colDef.field)) {
      const parts = params.colDef.field.split('_');
      if (parts.length === 2) {
        const num = Number(parts[1]);

        this.rows.value.forEach(row => {
          if (num && row.periods && num < row.periods.length) {
            if (
              row.type === ProjectCentralBudgetRowType.CostPersonell ||
              row.type === ProjectCentralBudgetRowType.CostMaterial ||
              row.type === ProjectCentralBudgetRowType.CostExpense ||
              row.type === ProjectCentralBudgetRowType.CostTotal ||
              row.type === ProjectCentralBudgetRowType.OverheadCost
            )
              sum += row.periods[num].amount * -1;
            else if (
              row.type === ProjectCentralBudgetRowType.OverheadCostPerHour
            ) {
              let periodHours = 0;
              this.rows.value
                .filter(
                  r => r.type === ProjectCentralBudgetRowType.CostPersonell
                )
                .forEach(r => {
                  periodHours += r.periods[num].quantity;
                });
              sum += row.periods[num].amount * periodHours * -1;
            } else sum += row.periods[num].amount;
          }
        });
      }
    } else {
      this.rows.value.forEach(row => {
        if (
          row.type === ProjectCentralBudgetRowType.CostPersonell ||
          row.type === ProjectCentralBudgetRowType.CostMaterial ||
          row.type === ProjectCentralBudgetRowType.CostExpense ||
          row.type === ProjectCentralBudgetRowType.CostTotal ||
          row.type === ProjectCentralBudgetRowType.OverheadCost
        )
          sum += row.totalAmount * -1;
        else if (row.type === ProjectCentralBudgetRowType.OverheadCostPerHour) {
          let totalHours = 0;
          this.rows.value
            .filter(r => r.type === ProjectCentralBudgetRowType.CostPersonell)
            .forEach(r => {
              totalHours += r.totalQuantity;
            });
          sum += row.totalAmount * totalHours * -1;
        } else sum += row.totalAmount * -1;
      });
    }
    return sum;
  }

  private sumQuantities(params: any) {
    if (!params || !params.colDef || !params.colDef.field) return 0;

    let sum = 0;
    if (this.isQuantityColumn(params.colDef.field)) {
      const parts = params.colDef.field.split('_');
      if (parts.length === 2) {
        const num = Number(parts[1]);
        this.rows.value.forEach(row => {
          if (num && row.periods && num < row.periods.length)
            sum += row.periods[num].quantity;
        });
      }
    } else {
      this.rows.value.forEach(row => {
        sum += row.totalQuantity;
      });
    }
    return sum;
  }

  private addPeriodColumnsToGrid() {
    if (!this.grid || !this.dateRangeData()) return [];
    return this.dateRange().map((d, i) => this.createColumnForType(i, d));
  }

  private createColumnForType(index: number, date: Date): ColDef {
    const header = this.grid.addColumnHeader(
      this.getPeriodHeaderColkey(index),
      this.getColumnTitleForPeriod(date)
    );
    this.grid.addColumnNumber(
      this.getAmountColKey(index),
      this.terms['common.amount'],
      {
        enableHiding: true,
        editable: () => {
          return !this.isGridDisabled();
        },
        decimals: 2,
        sortable: false,
        suppressFilter: true,
        suppressFloatingFilter: true,
        suppressSizeToFit: true,
        headerColumnDef: header,
        aggFuncOnGrouping: 'sumBudgetAmounts',
        minWidth: 85,
        flex: 1,
        clearZero: true,
        valueGetter: ({ data }) => {
          return data?.periods[index]?.amount ?? 0;
        },
        valueSetter: ({ data, newValue }) => {
          data.periods[index].amount = newValue;
          return true;
        },
      }
    );
    this.grid.addColumnNumber(
      this.getQuantityColKey(index),
      this.terms['billing.projects.list.budgethours'],
      {
        enableHiding: true,
        editable: ({ data }) => {
          return !this.isGridDisabled() && this.isQuantityColumnEditable(data!);
        },
        decimals: 2,
        sortable: false,
        suppressFilter: true,
        suppressFloatingFilter: true,
        suppressSizeToFit: true,
        headerColumnDef: header,
        aggFuncOnGrouping: 'sumBudgetQuantities',
        minWidth: 65,
        flex: 1,
        clearZero: true,
        valueGetter: ({ data }) => {
          return data?.periods[index]?.quantity ?? 0;
        },
        valueSetter: ({ data, newValue }) => {
          data.periods[index].quantity = newValue;
          return true;
        },
      }
    );
    return header;
  }

  // Actions
  private resetRows(rows: BudgetRowProjectDTO[], emit = true) {
    const updated =
      rows.length === 0
        ? this.budgetRowService.setupDefaultRows(rows, this.overheadCostPerHour)
        : rows;
    // Reset the rows in the grid
    if (this.grid) {
      this.alterPeriodColumns(updated);
    }
    this.rows.next(this.budgetRowService.toSorted(updated));
    if (emit) this.rowsChanged.emit(updated);
  }

  private alterPeriodColumns(rows: BudgetRowProjectDTO[] = []) {
    /*console.log('alterPeriodColumns', rows, !this.grid, !this.dateRangeData());
    if (!this.grid || !this.dateRangeData()) return;
    // We reset the columns to remove any existing period columns
    const colDefs = this.grid.columns.filter(
      col => !this.isHeaderColumn(col.field)
    );

    // Then we regenerate the period columns based on the current number of periods
    const newDefs = this.addPeriodColumnsToGrid();
    this.grid.api.setGridOption('columnDefs', [...colDefs, ...newDefs]);

    // regenerate rows
    let clonedRows: BudgetRowProjectDTO[] = [];
    const data = this.getRows();
    data.forEach(row => {
      const clonedRow = this.budgetRowService.cloneRow(row);
      clonedRow.periods = this.budgetRowService.getRowPeriodsFromRange();
      clonedRows.push(clonedRow);
    });
    console.log('handle cloned rows', clonedRows, data);
    // Map periods to new range
    clonedRows.forEach(row => {
      let prevRow = data.find(r => r.budgetRowNr === row.budgetRowNr);

      if (prevRow) {
        let date = this.dateRangeData()?.fromDate!;
        // Dates or type might have changed but the number of periods are the same
        if (prevRow.periods.length === row.periods.length) {
          for (let i = 0; i < row.periods.length; i++) {
            row.periods[i].amount = prevRow.periods[i].amount;
            row.periods[i].quantity = prevRow.periods[i].quantity;

            row.periods[i].startDate = date;
            if (
              this.dateRangeData()?.periodType ===
              TermGroup_ProjectBudgetPeriodType.Monthly
            ) {
              date = date.addMonths(1);
            } else if (
              this.dateRangeData()?.periodType ===
              TermGroup_ProjectBudgetPeriodType.Quarterly
            ) {
              date = date.addMonths(3);
            }
          }
        } else {
          if (
            this.dateRangeData()?.periodType ===
            TermGroup_ProjectBudgetPeriodType.Monthly
          ) {
            if (prevRow.periods.length < row.periods.length) {
              // Extended
              for (let i = 0; i < row.periods.length; i++) {}
            } else {
              // Shortened
            }
            date = date.addMonths(1);
          } else if (
            this.dateRangeData()?.periodType ===
            TermGroup_ProjectBudgetPeriodType.Quarterly
          ) {
            date = date.addMonths(3);
          }
        }
      }
    });

    /*const changed = this.budgetRowService.changePeriodRange(
      clonedRows,
      this.dateRange()
    );
    this.rows.next(clonedRows);*/
  }

  private addNewRow(row: AG_NODE<BudgetRowProjectDTO>) {
    const rows = this.grid.getAllRows() as AG_NODE<BudgetRowProjectDTO>[];
    this.budgetRowService.addRow(rows, row);
    this.resetRows(rows, false);
    //this.rows.next(rows); //this.grid.setData(rows);
    setTimeout(() => {
      this.grid.api.startEditingCell({
        rowIndex: this.grid.getRowIndex(row) + 1,
        colKey: 'timeCodeId',
      });
    }, 100);
  }

  private getRows() {
    return this.grid.getAllRows() as AG_NODE<BudgetRowProjectDTO>[];
  }
  private getvVisibleRows() {
    return this.grid
      .getAllRows()
      .filter(r => !r.isDeleted) as AG_NODE<BudgetRowProjectDTO>[];
  }
  private getRowsForParent(): BudgetRowProjectDTO[] {
    return this.getRows().filter(row => !row.isTotalsRow);
  }

  private deleteRow(row: AG_NODE<BudgetRowProjectDTO>): void {
    if (
      this.getRows().filter(r => r.type === row.type && !r.isDeleted).length ===
      1
    ) {
      row.totalAmount = 0;
      row.totalQuantity = 0;
      row.comment = '';
      row.timeCodeId = 0;
      row.isModified = true;
      row.periods.forEach(p => {
        p.amount = 0;
        p.quantity = 0;
      });
      this.grid.api.redrawRows();
    } else {
      row.isDeleted = true;
      row.isModified = true;
      this.grid.api.applyTransaction({ remove: [row] });
      this.grid.api.redrawRows();
    }
  }

  onCellValueChanged(event: CellValueChangedEvent<BudgetRowProjectDTO>) {
    const colKey = event.colDef.field;
    const data = event.data;
    if (data && colKey) {
      if (this.isPeriodColumn(colKey)) {
        if (data.type !== ProjectCentralBudgetRowType.OverheadCostPerHour) {
          const diff = event.newValue - event.oldValue;
          if (this.isAmountColumn(colKey)) {
            data.totalAmount += diff;
          }
          if (this.isQuantityColumn(colKey)) {
            data.totalQuantity += diff;
          }
        } else {
          const totalSum = data.periods.reduce((sum, p) => sum + p.amount, 0);
          data.totalAmount = totalSum / data.periods.length;
        }
      } else if (this.isTotalAmountColumn(colKey)) {
        this.totalAmountChanged(event);
      } else if (this.isTotalQuantityColumn(colKey)) {
        this.totalQuantityChanged(event);
      }

      if (event.oldValue != event.newValue) {
        data.isModified = true;
      }

      event.node.setData(data);
      event.api.refreshCells();

      event.api.refreshClientSideRowModel('aggregate');
    }

    this.rowsChanged.emit(this.getRowsForParent());
  }

  // Load data
  private loadTimeCodes(): Observable<ITimeCodeDTO[]> {
    return this.materialCodesService
      .getTimeCodes(SoeTimeCodeType.WorkAndMaterial, true, false, false)
      .pipe(
        tap(result => {
          this.timeCodes = result;
        })
      );
  }

  override loadCompanySettings() {
    return this.coreService
      .getCompanySettings([
        CompanySettingType.ProjectOverheadCostAsAmountPerHour,
      ])
      .pipe(
        tap(setting => {
          this.overheadCostPerHour = SettingsUtil.getBoolCompanySetting(
            setting,
            CompanySettingType.ProjectOverheadCostAsAmountPerHour
          );
        })
      );
  }

  private loadBudgetTypes(): Observable<SmallGenericType[]> {
    const translationKeys = [
      'billing.projects.list.budgethours',
      'billing.projects.list.budgethourstotal',
      'billing.projects.list.budgetincomepersonel',
      'billing.projects.list.budgetincomematerial',
      'billing.projects.list.budgetincometotal',
      'billing.projects.list.budgetcostpersonal',
      'billing.projects.list.budgetcostmaterial',
      'billing.projects.list.budgetcost',
      'billing.projects.list.budgetoverheadperhour',
      'billing.projects.list.budgetoverhead',
      'billing.projects.list.budgetcost',
    ];

    return this.translate.get(translationKeys).pipe(
      map(terms => {
        this.budgetRowTypes = [
          {
            id: ProjectCentralBudgetRowType.IncomePersonellTotal,
            name: terms['billing.projects.list.budgetincomepersonel'],
          },
          {
            id: ProjectCentralBudgetRowType.IncomeMaterialTotal,
            name: terms['billing.projects.list.budgetincomematerial'],
          },
          {
            id: ProjectCentralBudgetRowType.CostPersonell,
            name: terms['billing.projects.list.budgetcostpersonal'],
          },
          {
            id: ProjectCentralBudgetRowType.CostMaterial,
            name: terms['billing.projects.list.budgetcostmaterial'],
          },
          {
            id: ProjectCentralBudgetRowType.OverheadCostPerHour,
            name: terms['billing.projects.list.budgetoverheadperhour'],
          },
          {
            id: ProjectCentralBudgetRowType.OverheadCost,
            name: terms['billing.projects.list.budgetoverhead'],
          },
          {
            id: ProjectCentralBudgetRowType.CostExpense,
            name: terms['billing.projects.list.budgetcost'],
          },
        ];
        return this.budgetRowTypes;
      })
    );
  }

  // Actions
  private totalAmountChanged(
    event: CellValueChangedEvent<BudgetRowProjectDTO>
  ) {
    const data = event.data;
    event.api.stopEditing();

    if (data.type === ProjectCentralBudgetRowType.OverheadCostPerHour) {
      data.periods.forEach(r => {
        r.amount = event.newValue;
      });
    } else {
      const periodAmount = Math.floor(event.newValue / data.periods.length);
      let handled = 0;
      for (let i = 0; i < data.periods.length; i++) {
        if (data.periods.length - 1 === i)
          data.periods[i].amount = event.newValue - handled;
        else data.periods[i].amount = periodAmount;
        handled += periodAmount;
      }
    }

    setTimeout(() => {
      // Fix focus issue where editing starts in the cell before new data is set.
      const cell = this.grid.api.getFocusedCell();
      if (!cell) return;
      this.grid.api.startEditingCell({
        rowIndex: cell.rowIndex,
        colKey: cell.column,
      });
    }, 100);
  }

  private totalQuantityChanged(
    event: CellValueChangedEvent<BudgetRowProjectDTO>
  ) {
    const data = event.data;

    const periodQuantity = Math.floor(event.newValue / data.periods.length);
    let handled = 0;
    for (let i = 0; i < data.periods.length; i++) {
      if (data.periods.length - 1 === i)
        data.periods[i].quantity = event.newValue - handled;
      else data.periods[i].quantity = periodQuantity;
      handled += periodQuantity;
    }
  }

  // Utility methods
  private getTimeCodesForRow(row: any): ITimeCodeDTO[] {
    if (!row || !row.type) return [];
    const timeCodes = this.timeCodes.filter(tc => {
      if (
        row.type === ProjectCentralBudgetRowType.CostMaterial &&
        tc.type === SoeTimeCodeType.Material
      ) {
        return true;
      }
      if (
        row.type === ProjectCentralBudgetRowType.CostPersonell &&
        tc.type === SoeTimeCodeType.Work
      ) {
        return true;
      }
      return false;
    });
    console.log('getTimeCodesForRow', row, timeCodes);
    return timeCodes;
  }
  private getColumnTitleForPeriod(date: Date) {
    const { periodType } = this.dateRangeData()!;
    const year = date.getFullYear();
    if (periodType === TermGroup_ProjectBudgetPeriodType.Monthly)
      return `${year}-${date.getMonth() + 1}`;

    if (periodType === TermGroup_ProjectBudgetPeriodType.Quarterly)
      return `${year}-Q${DateUtil.getQuarter(date)}`;
    return `${year}`;
  }

  private getPeriodHeaderColkey(period: number): string {
    return `header_${period}`;
  }
  private getAmountColKey(period: number): string {
    return `amount_${period}`;
  }
  private getQuantityColKey(period: number): string {
    return `quantity_${period}`;
  }
  // Predicates
  canBeDeleted(row: BudgetRowProjectDTO): boolean {
    return row && !row.isTotalsRow && !this.isGridDisabled();
  }

  isSubTypeColumnEditable(row: BudgetRowProjectDTO): boolean {
    return (
      row?.type === ProjectCentralBudgetRowType.CostPersonell ||
      row?.type === ProjectCentralBudgetRowType.CostMaterial
    );
  }

  isQuantityColumnEditable(row: BudgetRowProjectDTO) {
    return row?.type === ProjectCentralBudgetRowType.CostPersonell;
  }
  isRowVisibleInGrid(row: BudgetRowProjectDTO): boolean {
    // Only show rows that are not totals rows
    if (row.isDeleted) return false;
    if (!this.showTotalRows() && row.isTotalsRow) return false;

    return true;
  }
  isHeaderColumn(colKey?: string): boolean {
    if (colKey) {
      return (
        this.isHeaderColumnForPeriod(colKey) || this.isPeriodColumn(colKey)
      );
    }
    return false;
  }
  private isPeriodColumn(colKey: string): boolean {
    return this.isQuantityColumn(colKey) || this.isAmountColumn(colKey);
  }
  private isQuantityColumn(colKey: string): boolean {
    return colKey.startsWith('quantity_');
  }
  private isAmountColumn(colKey: string): boolean {
    return colKey.startsWith('amount_');
  }
  private isTotalAmountColumn(colKey: string): boolean {
    return colKey.toLowerCase() === 'totalamount';
  }
  private isTotalQuantityColumn(colKey: string): boolean {
    return colKey.toLowerCase() === 'totalquantity';
  }
  private isHeaderColumnForPeriod(colKey: string): boolean {
    return colKey.startsWith('header_');
  }
}
