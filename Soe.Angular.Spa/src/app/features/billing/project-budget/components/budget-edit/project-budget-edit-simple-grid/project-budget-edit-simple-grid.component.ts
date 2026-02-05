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
import {
  BudgetRowProjectChangeLogDTO,
  BudgetRowProjectDTO,
} from '@features/billing/project-budget/models/project-budget.model';
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
import {
  CellClassParams,
  CellValueChangedEvent,
  ColDef,
} from 'ag-grid-community';
import { BehaviorSubject, Observable, map, of, take, tap, timeout } from 'rxjs';
import { ProjectBudgetRowService } from './project-budget-row-simple.service';
import { ColumnUtil, SoeColGroupDef, TimeColumnOptions } from '@ui/grid/util';
import { LogicalFileSystem } from '@angular/compiler-cli';
import { LogicalProjectStrategy } from 'node_modules/@angular/compiler-cli/src/ngtsc/imports';
import { ProjectBudgetService } from '@features/billing/project-budget/services/project-budget.service';
import { el } from 'date-fns/locale';
import { ChangeLogDialogData } from '../change-log-modal/change-log-modal.model';
import { ChangeLogModal } from '../change-log-modal/change-log-modal.component';
import { DialogService } from '@ui/dialog/services/dialog.service';

export type ProjectBudgetRowActions = null | ResetRows;
type ResetRows = {
  type: 'RESET_ROWS';
  rows: BudgetRowProjectDTO[];
  emit?: boolean;
};

export type DateRangeData = {
  periodType: TermGroup_ProjectBudgetPeriodType;
  fromDate: Date;
  toDate: Date;
};

@Component({
  selector: 'soe-project-budget-edit-simple-grid',
  templateUrl: './project-budget-edit-simple-grid.component.html',
  standalone: true,
  providers: [FlowHandlerService, ToolbarService],
  imports: [SharedModule, GridWrapperComponent],
})
export class ProjectBudgetEditSimpleGridComponent
  extends GridBaseDirective<BudgetRowProjectDTO>
  implements OnInit, OnChanges
{
  private projectBudgetService = inject(ProjectBudgetService);
  private readonly materialCodesService = inject(MaterialCodesService);
  private readonly budgetRowService = inject(ProjectBudgetRowService);
  private readonly coreService = inject(CoreService);
  private readonly dialogService = inject(DialogService);

  @Input({ required: true }) rowAction!: Signal<ProjectBudgetRowActions>;
  @Output() rowsChanged = new EventEmitter<BudgetRowProjectDTO[]>();
  @Output() dateRangeChanged = new EventEmitter<Date[]>();

  isGridDisabled = input(false);
  isProjectBudgetForecast = input(false);
  showTotalRows = signal(true);
  compBudgetName = input('');
  resultUpdatedText = input('');

  overheadCostPerHour = false;
  timeCodes: ITimeCodeDTO[] = [];
  budgetRowTypes: SmallGenericType[] = [];
  rows = new BehaviorSubject<BudgetRowProjectDTO[]>([]);

  loading = true;
  runResetRows = false;

  budgetHeader: SoeColGroupDef | undefined;

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
    if (changes.compBudgetName && this.budgetHeader) {
      this.budgetHeader.headerName = changes.compBudgetName.currentValue || ' ';
      setTimeout(() => {
        this.grid.resetColumns();
      }, 100);
    }
  }

  onGridReadyToDefine(grid: GridComponent<BudgetRowProjectDTO>): void {
    console.log(
      'ProjectBudgetEditSimpleGridComponent.onGridReadyToDefine',
      this.isProjectBudgetForecast()
    );
    super.onGridReadyToDefine(grid);
    this.grid.api.updateGridOptions({
      onCellValueChanged: this.onCellValueChanged.bind(this),
      isExternalFilterPresent: () => true,
      doesExternalFilterPass: node =>
        this.showTotalRows() || !node.data.isTotalsRow,
    });
    this.grid.setNbrOfRowsToShow(16);
    this.grid.updateGridHeightBasedOnNbrOfRows();
    this.grid.context.suppressFiltering = true;

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
        'billing.project.central.outcome',
        'billing.project.central.budget',
        'billing.projects.budget.forecast',
        'billing.projects.budget.categorization',
        'billing.projects.budget.diffpercent',
        'common.description',
        'billing.projects.budget.diffforecastresult',
        'common.date',
        'common.dashboard.performanceanalyzer.xaxis',
        'common.time',
        'billing.projects.budget.amountfrom',
        'billing.projects.budget.amountto',
        'billing.projects.budget.hoursfrom',
        'billing.projects.budget.hoursto',
        'billing.projects.budget.diff',
        'common.user',
        'billing.projects.budget.log',
        'billing.projects.budget.complevel',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.terms = terms;

        // Styling
        this.grid.setRowClassCallback(params => {
          if (params.data?.isDeleted) return 'strike-through';
          return '';
        });

        const resultCat = this.grid.addColumnHeader(
          'resultCat',
          terms['billing.projects.budget.categorization'],
          { enableHiding: false }
        );

        const forecastHeader = this.grid.addColumnHeader(
          'budgetHeader',
          this.isProjectBudgetForecast()
            ? terms['billing.projects.budget.forecast']
            : terms['billing.project.central.budget'],
          { enableHiding: false }
        );

        // Columns
        this.grid.addColumnModified('isModified', {
          pinned: 'left',
          headerColumnDef: resultCat,
        });
        this.grid.addColumnIcon('', '', {
          iconName: 'plus',
          pinned: 'left',
          tooltip: terms['common.newrow'],
          headerColumnDef: resultCat,
          enableHiding: false,
          onClick: row => {
            this.addNewRow(row);
          },
          showIcon: row => {
            return (
              !this.isGridDisabled() &&
              row &&
              !row.isTotalsRow &&
              row.type !== ProjectCentralBudgetRowType.OverheadCostPerHour
            );
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
            headerColumnDef: resultCat,
            enableHiding: false,
            cellClassRules: {
              'disabled-grid-cell-background-color-default': (
                params: CellClassParams
              ) => {
                return params.data?.isDefault === true;
              },
              'disabled-grid-cell-background-color-nondefault': (
                params: CellClassParams
              ) => {
                return params.data?.isDefault === false;
              },
              'disabled-grid-cell-background-color': (
                params: CellClassParams
              ) => true,
            },
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
            headerColumnDef: resultCat,
            enableHiding: false,
            editable: params => {
              return (
                !this.isGridDisabled() &&
                this.isSubTypeColumnEditable(params.data!)
              );
            },
            cellClassRules: {
              'disabled-grid-cell-background-color': (
                params: CellClassParams
              ) => {
                return (
                  this.isGridDisabled() ||
                  !this.isSubTypeColumnEditable(params.data!)
                );
              },
            },
          }
        );

        this.grid.addColumnText('comment', this.terms['common.description'], {
          pinned: 'left',
          enableHiding: false,
          sortable: false,
          suppressFilter: true,
          suppressFloatingFilter: true,
          flex: 1,
          minWidth: 100,
          suppressSizeToFit: true,
          headerColumnDef: resultCat,
          editable: params => {
            return !this.isGridDisabled();
          },
          cellClassRules: {
            'disabled-grid-cell-background-color': (
              params: CellClassParams
            ) => {
              return this.isGridDisabled();
            },
          },
        });

        this.grid.addColumnNumber(
          'totalAmount',
          terms['billing.projects.budget.totalamount'],
          {
            enableHiding: false,
            decimals: 2,
            editable: () => {
              return !this.isGridDisabled();
            },
            sortable: false,
            minWidth: 85,
            flex: 1,
            suppressSizeToFit: true,
            suppressFilter: true,
            suppressFloatingFilter: true,
            headerColumnDef: forecastHeader,
            clearZero: true,
            cellClassRules: {
              'disabled-grid-cell-background-color': (
                params: CellClassParams
              ) => {
                return this.isGridDisabled();
              },
            },
          }
        );

        this.grid.addColumnNumber(
          'totalQuantity',
          terms['billing.projects.list.budgethourstotal'],
          {
            enableHiding: false,
            flex: 1,
            minWidth: 65,
            editable: row => {
              return (
                !this.isGridDisabled() &&
                row.data?.type === ProjectCentralBudgetRowType.CostPersonell
              );
            },
            sortable: false,
            suppressFilter: true,
            suppressFloatingFilter: true,
            headerColumnDef: forecastHeader,
            clearZero: true,
            cellClassRules: {
              'disabled-grid-cell-background-color': (
                params: CellClassParams
              ) => {
                return (
                  this.isGridDisabled() ||
                  params.data?.type !==
                    ProjectCentralBudgetRowType.CostPersonell
                );
              },
            },
          }
        );

        if (this.isProjectBudgetForecast()) {
          const resultHeader = this.grid.addColumnHeader(
            'resultHeader',
            this.resultUpdatedText()
              ? terms['billing.project.central.outcome'] +
                  ' (' +
                  this.resultUpdatedText() +
                  ')'
              : terms['billing.project.central.outcome'],
            { enableHiding: false }
          );

          this.grid.addColumnNumber(
            'totalAmountResult',
            terms['billing.projects.budget.totalamount'],
            {
              enableHiding: false,
              decimals: 2,
              editable: false,
              sortable: false,
              minWidth: 85,
              flex: 1,
              suppressSizeToFit: true,
              suppressFilter: true,
              suppressFloatingFilter: true,
              headerColumnDef: resultHeader,
              clearZero: true,
              cellClassRules: {
                'disabled-grid-cell-background-color': (
                  params: CellClassParams
                ) => {
                  return true;
                },
              },
            }
          );

          this.grid.addColumnTimeSpan(
            'totalQuantityResult',
            terms['billing.projects.list.budgethourstotal'],
            {
              enableHiding: false,
              flex: 1,
              minWidth: 65,
              editable: false,
              sortable: false,
              suppressFilter: true,
              suppressFloatingFilter: true,
              headerColumnDef: resultHeader,
              clearZero: true,
              cellClassRules: {
                'disabled-grid-cell-background-color': (
                  params: CellClassParams
                ) => {
                  return true;
                },
              },
            }
          );

          this.grid.addColumnNumber(
            'totalDiffResult',
            terms['billing.projects.budget.complevel'],
            {
              enableHiding: false,
              flex: 1,
              minWidth: 65,
              editable: false,
              sortable: false,
              suppressFilter: true,
              suppressFloatingFilter: true,
              headerColumnDef: resultHeader,
              clearZero: true,
              tooltip: terms['billing.projects.budget.diffforecastresult'],
              cellClassRules: {
                'error-background-color': (params: CellClassParams) => {
                  if (
                    params.data.type ===
                      ProjectCentralBudgetRowType.IncomeMaterialTotal ||
                    params.data.type ===
                      ProjectCentralBudgetRowType.IncomePersonellTotal
                  ) {
                    return params.value > 0 && params.value < 100;
                  } else if (
                    params.data.type ===
                      ProjectCentralBudgetRowType.CostMaterial ||
                    params.data.type ===
                      ProjectCentralBudgetRowType.CostPersonell ||
                    params.data.type ===
                      ProjectCentralBudgetRowType.OverheadCost ||
                    params.data.type === ProjectCentralBudgetRowType.CostExpense
                  ) {
                    return params.value > 100;
                  } else {
                    return false;
                  }
                },
                'ok-background-color': (params: CellClassParams) => {
                  if (
                    params.data.type ===
                      ProjectCentralBudgetRowType.IncomeMaterialTotal ||
                    params.data.type ===
                      ProjectCentralBudgetRowType.IncomePersonellTotal
                  ) {
                    return params.value > 100;
                  } else if (
                    params.data.type ===
                      ProjectCentralBudgetRowType.CostMaterial ||
                    params.data.type ===
                      ProjectCentralBudgetRowType.CostPersonell ||
                    params.data.type ===
                      ProjectCentralBudgetRowType.OverheadCost ||
                    params.data.type === ProjectCentralBudgetRowType.CostExpense
                  ) {
                    return params.value > 0 && params.value < 100;
                  } else {
                    return false;
                  }
                },
              },
            }
          );

          this.budgetHeader = this.grid.addColumnHeader(
            'budgetHeader',
            this.compBudgetName() ?? terms['billing.project.central.budget'],
            { enableHiding: false }
          );

          this.grid.addColumnNumber(
            'totalAmountCompBudget',
            terms['billing.projects.budget.totalamount'],
            {
              enableHiding: false,
              decimals: 2,
              editable: false,
              sortable: false,
              minWidth: 85,
              flex: 1,
              suppressSizeToFit: true,
              suppressFilter: true,
              suppressFloatingFilter: true,
              headerColumnDef: this.budgetHeader,
              clearZero: true,
              cellClassRules: {
                'disabled-grid-cell-background-color': (
                  params: CellClassParams
                ) => {
                  return true;
                },
              },
            }
          );

          this.grid.addColumnTimeSpan(
            'totalQuantityCompBudget',
            terms['billing.projects.list.budgethourstotal'],
            {
              enableHiding: false,
              flex: 1,
              minWidth: 65,
              editable: false,
              sortable: false,
              suppressFilter: true,
              suppressFloatingFilter: true,
              headerColumnDef: this.budgetHeader,
              clearZero: true,
              cellClassRules: {
                'disabled-grid-cell-background-color': (
                  params: CellClassParams
                ) => {
                  return true;
                },
              },
            }
          );
        }

        this.grid.addColumnIcon('', '', {
          iconName: 'clock-rotate-left',
          pinned: 'right',
          tooltip: terms['common.newrow'],
          enableHiding: false,
          onClick: row => {
            this.showRowLog(row);
          },
          showIcon: row => {
            return row && row.hasLogPosts && row.budgetRowId > 0;
          },
        });

        this.grid.addColumnIconDelete({
          tooltip: terms['core.delete'],
          suppressFilter: true,
          suppressFloatingFilter: true,
          editable: false,
          enableHiding: false,
          showIcon: data => {
            return !this.isGridDisabled() && this.canBeDeleted(data);
          },
          onClick: row => {
            this.deleteRow(row);
          },
        });

        super.finalizeInitGrid();
      });
  }

  private showRowLog(row: BudgetRowProjectDTO) {
    this.projectBudgetService
      .getChangeLog(row.budgetRowId)
      .subscribe(detailRows => {
        if (detailRows.length > 0) {
          const modalData = new ChangeLogDialogData();
          modalData.items = detailRows;
          modalData.title = this.translate.instant(
            'billing.projects.budget.log'
          );
          modalData.size = 'xl';
          modalData.isReadOnly = true;

          this.dialogService.open(ChangeLogModal, modalData);
        }
      });
  }

  // Actions
  private resetRows(rows: BudgetRowProjectDTO[], emit = true) {
    const updated =
      rows.length === 0
        ? this.budgetRowService.setupDefaultRows(rows, this.overheadCostPerHour)
        : rows;

    const sortedRows = this.budgetRowService.toSorted(updated);
    this.rows.next(sortedRows);
    if (emit) this.rowsChanged.emit(updated);
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
  private getVisibleRows() {
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
      if (event.oldValue != event.newValue) {
        if (
          (this.isTotalAmountColumn(colKey) ||
            this.isTotalQuantityColumn(colKey)) &&
          event.newValue === undefined
        ) {
          (data as any)[colKey as keyof BudgetRowProjectDTO] = event.oldValue;
          return;
        }
        data.isModified = true;
        if (data.budgetRowId > 0) {
          let logItem =
            data.changeLogItems.length > 0 ? data.changeLogItems[0] : null;
          if (!logItem && data.budgetRowId > 0) {
            logItem = new BudgetRowProjectChangeLogDTO();

            if (this.isTotalAmountColumn(colKey)) {
              logItem.fromTotalAmount = event.oldValue;
            } else if (this.isTotalQuantityColumn(colKey)) {
              logItem.fromTotalQuantity = event.oldValue;
            }

            const type = this.budgetRowTypes.find(t => t.id === data.type);
            if (type) logItem.typeName = type.name;

            logItem.budgetRowId = data.budgetRowId;

            data.changeLogItems.unshift(logItem);
          }

          if (logItem) {
            logItem.description = data.comment;

            const timeCode = this.timeCodes.find(
              t => t.timeCodeId === data.timeCodeId
            );
            if (timeCode) logItem.timeCodeName = timeCode.name;

            if (this.isTotalAmountColumn(colKey)) {
              if (logItem.fromTotalAmount === undefined)
                logItem.fromTotalAmount = event.oldValue;

              logItem.toTotalAmount = event.newValue;
              logItem.totalAmountDiff =
                logItem.toTotalAmount - logItem.fromTotalAmount;
            } else if (this.isTotalQuantityColumn(colKey)) {
              if (logItem.fromTotalQuantity === undefined)
                logItem.fromTotalQuantity = event.oldValue;

              logItem.toTotalQuantity = event.newValue;
              logItem.totalQuantityDiff =
                logItem.toTotalQuantity - logItem.fromTotalQuantity;
            }
          }
        }
        this.rowsChanged.emit(this.getVisibleRows());
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
    return timeCodes;
  }

  // Predicates
  canBeDeleted(row: BudgetRowProjectDTO): boolean {
    return row && !row.isTotalsRow && !row.isLocked && !this.isGridDisabled();
  }

  isSubTypeColumnEditable(row: BudgetRowProjectDTO): boolean {
    return (
      (row?.type === ProjectCentralBudgetRowType.CostPersonell ||
        row?.type === ProjectCentralBudgetRowType.CostMaterial) &&
      row?.isLocked !== true
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

  private isTotalAmountColumn(colKey: string): boolean {
    return colKey.toLowerCase() === 'totalamount';
  }

  private isTotalQuantityColumn(colKey: string): boolean {
    return colKey.toLowerCase() === 'totalquantity';
  }
}
