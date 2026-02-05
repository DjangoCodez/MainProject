import {
  Component,
  EventEmitter,
  inject,
  Input,
  OnDestroy,
  OnInit,
  Output,
} from '@angular/core';
import { BudgetRowProjectChangeLogDTO } from '@features/billing/project-budget/models/project-budget.model';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { debounce } from 'lodash';
import { BehaviorSubject, take } from 'rxjs';

@Component({
  selector: 'soe-change-log-modal-grid',
  templateUrl: './change-log-modal-grid.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: true,
  imports: [GridComponent],
})
export class ChangeLogModalGridComponent
  extends GridBaseDirective<BudgetRowProjectChangeLogDTO>
  implements OnInit, OnDestroy
{
  @Input() rows!: BehaviorSubject<BudgetRowProjectChangeLogDTO[]>;
  @Input() isReadOnly: boolean = false;
  @Output() rowsChanged = new EventEmitter<void>();

  flowHandler = inject(FlowHandlerService);
  private isDestroyed = false;

  ngOnInit(): void {
    this.flowHandler.execute({
      skipInitialLoad: true,
      setupGrid: this.setupGrid.bind(this),
    });
  }

  ngOnDestroy(): void {
    this.grid.onRowDoubleClicked = () => {};
    this.isDestroyed = true;
  }

  setupGrid(grid: GridComponent<BudgetRowProjectChangeLogDTO>) {
    if (this.isDestroyed) return;

    super.setupGrid(grid, 'common.dialogs.searchcustomer');
    this.translate
      .get([
        'core.comment',
        'common.type',
        'common.quantity',
        'common.amount',
        'common.from',
        'common.to',
        'core.time.hours',
        'billing.projects.budget.diff',
        'billing.projects.budget.costsubtype',
        'billing.projects.budget.categorization',
        'common.description',
        'billing.projects.budget.changecomment',
        'common.dashboard.performanceanalyzer.xaxis',
        'common.date',
        'common.time',
        'common.user',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        if (!this.isReadOnly) this.grid.enableRowSelection(undefined, false);

        this.grid.api.updateGridOptions({
          onCellValueChanged: this.onCellValueChanged.bind(this),
        });
        this.grid.context.suppressGridMenu = true;

        if (this.isReadOnly) {
          const dateHeaderCol = this.grid.addColumnHeader(
            'dateHeader',
            terms['common.dashboard.performanceanalyzer.xaxis'],
            {}
          );
          this.grid.addColumnDate('created', '', {
            headerColumnDef: dateHeaderCol,
            flex: 1,
          });
          this.grid.addColumnTime('created', '', {
            headerColumnDef: dateHeaderCol,
            flex: 1,
          });
          this.grid.addColumnText('createdBy', terms['common.user'], {
            flex: 1,
            headerColumnDef: dateHeaderCol,
            editable: false,
          });
        } else {
          const generalCat = this.grid.addColumnHeader(
            'generalCat',
            terms['billing.projects.budget.categorization'],
            { enableHiding: false }
          );
          this.grid.addColumnText('typeName', terms['common.type'], {
            flex: 1,
            headerColumnDef: generalCat,
            editable: false,
          });
          this.grid.addColumnText(
            'timeCodeName',
            terms['billing.projects.budget.costsubtype'],
            { flex: 1, headerColumnDef: generalCat, editable: false }
          );
          this.grid.addColumnText('description', terms['common.description'], {
            flex: 1,
            headerColumnDef: generalCat,
            editable: false,
          });
        }

        const amountCat = this.grid.addColumnHeader(
          'amountCat',
          terms['common.amount'],
          { enableHiding: false }
        );
        this.grid.addColumnNumber('fromTotalAmount', terms['common.from'], {
          decimals: 2,
          editable: false,
          flex: 1,
          headerColumnDef: amountCat,
          clearZero: true,
        });
        this.grid.addColumnNumber('toTotalAmount', terms['common.to'], {
          decimals: 2,
          editable: false,
          flex: 1,
          headerColumnDef: amountCat,
          clearZero: true,
        });
        this.grid.addColumnNumber(
          'totalAmountDiff',
          terms['billing.projects.budget.diff'],
          {
            decimals: 2,
            editable: false,
            flex: 1,
            headerColumnDef: amountCat,
            clearZero: true,
          }
        );

        const quantityCat = this.grid.addColumnHeader(
          'quantityCat',
          terms['core.time.hours'],
          { enableHiding: false }
        );
        this.grid.addColumnNumber('fromTotalQuantity', terms['common.from'], {
          decimals: 2,
          editable: false,
          flex: 1,
          headerColumnDef: quantityCat,
          clearZero: true,
        });
        this.grid.addColumnNumber('toTotalQuantity', terms['common.to'], {
          decimals: 2,
          editable: false,
          flex: 1,
          headerColumnDef: quantityCat,
          clearZero: true,
        });
        this.grid.addColumnNumber(
          'totalQuantityDiff',
          terms['billing.projects.budget.diff'],
          {
            decimals: 2,
            editable: false,
            flex: 1,
            headerColumnDef: quantityCat,
            clearZero: true,
          }
        );

        this.grid.addColumnText(
          'comment',
          this.isReadOnly
            ? terms['core.comment']
            : terms['billing.projects.budget.changecomment'],
          { flex: 1, editable: !this.isReadOnly }
        );

        this.grid.setNbrOfRowsToShow(15, 15);
        super.finalizeInitGrid();
      });
  }

  onCellValueChanged(event: any) {
    if (event.newValue !== event.oldValue) this.rowsChanged.emit();
  }
}
