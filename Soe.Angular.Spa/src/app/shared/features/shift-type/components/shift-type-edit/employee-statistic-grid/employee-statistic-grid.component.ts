import {
  Component,
  inject,
  input,
  OnDestroy,
  OnInit,
  signal,
} from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { ShiftTypeForm } from '@shared/features/shift-type/models/shift-type-form.model';
import {
  Feature,
  TermGroup,
  TermGroup_EmployeeStatisticsType,
} from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { IShiftTypeEmployeeStatisticsTargetDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { CellValueChangedEvent } from 'ag-grid-community';
import { BehaviorSubject, Subject, take, takeUntil, tap } from 'rxjs';

@Component({
  selector: 'soe-employee-statistic-grid',
  templateUrl: './employee-statistic-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class EmployeeStatisticGridComponent
  extends GridBaseDirective<IShiftTypeEmployeeStatisticsTargetDTO>
  implements OnInit, OnDestroy
{
  form = input.required<ShiftTypeForm>();
  rows = new BehaviorSubject<IShiftTypeEmployeeStatisticsTargetDTO[]>([]);

  private _destroy$ = new Subject<void>();

  coreService = inject(CoreService);

  private statisticTargets: ISmallGenericType[] = [];

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Preferences_ScheduleSettings_ShiftType_Edit,
      'time.schedule.shifttype.shifttype.employeestatistic',
      {
        skipInitialLoad: true,
        lookups: this.loadStatisticTargets(),
      }
    );

    this.form()
      ?.employeeStatisticsTargets?.valueChanges.pipe(takeUntil(this._destroy$))
      .subscribe(rows => {
        this.setGridData(rows, false);
      });
    this.form()
      ?.valueChanges.pipe(takeUntil(this._destroy$))
      .subscribe(frm => {
        if (frm.isCopy) {
          this.form().controls.isCopy.patchValue(false);
          this.setGridData(frm.employeeStatisticsTargets, false);
        }
      });
  }

  override createGridToolbar(): void {
    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('addrow', {
          iconName: signal('plus'),
          caption: signal('common.newrow'),
          tooltip: signal('common.newrow'),
          onAction: () => this.addRow(),
        }),
      ],
    });
  }

  loadStatisticTargets() {
    return this.coreService
      .getTermGroupContent(TermGroup.EmployeeStatisticsType, false, false)
      .pipe(
        tap(x => {
          this.statisticTargets = [];
          x.forEach(target => {
            if (
              // Following targets are not used
              target.id != TermGroup_EmployeeStatisticsType.ArrivalAndGoHome &&
              target.id != TermGroup_EmployeeStatisticsType.AnsweredCalls &&
              target.id != TermGroup_EmployeeStatisticsType.CallDuration &&
              target.id != TermGroup_EmployeeStatisticsType.ConnectedTime &&
              target.id != TermGroup_EmployeeStatisticsType.NotAnsweredCalls
            ) {
              this.statisticTargets.push(target);
            }
          });
        })
      );
  }

  addRow() {
    const rows = this.rows.value;
    const row = {
      employeeStatisticsTypeName: '',
      targetValue: 0,
      fromDate: new Date(),
    };
    rows.push(row as IShiftTypeEmployeeStatisticsTargetDTO);
    this.setGridData(rows);
    setTimeout(() => {
      this.grid.startEditing(rows.length - 1, 'employeeStatisticsType');
    });
  }

  private setGridData(
    rows: IShiftTypeEmployeeStatisticsTargetDTO[],
    patchForm = true
  ): void {
    if (this.grid) this.rows.next(rows);
    if (patchForm) this.patchForm();
  }

  onGridReadyToDefine(
    grid: GridComponent<IShiftTypeEmployeeStatisticsTargetDTO>
  ): void {
    super.onGridReadyToDefine(grid);

    this.grid.api.updateGridOptions({
      onCellValueChanged: this.onCellValueChanged.bind(this),
    });

    this.translate
      .get([
        'common.type',
        'time.schedule.shifttype.targetvalue',
        'common.categories.datefrom',
        'core.delete',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnSelect(
          'employeeStatisticsType',
          terms['common.type'],
          this.statisticTargets,
          undefined,
          {
            dropDownIdLabel: 'id',
            dropDownValueLabel: 'name',
            flex: 33,
            editable: true,
          }
        );
        this.grid.addColumnNumber(
          'targetValue',
          terms['time.schedule.shifttype.targetvalue'],
          { editable: true, flex: 33 }
        );
        this.grid.addColumnDate(
          'fromDate',
          terms['common.categories.datefrom'],
          { flex: 33, editable: true }
        );
        this.grid.addColumnIconDelete({
          tooltip: terms['core.delete'],
          onClick: row => {
            this.deleteRow(row as IShiftTypeEmployeeStatisticsTargetDTO);
          },
        });

        this.grid.context.suppressGridMenu = true;
        super.finalizeInitGrid({ hidden: true });
        this.setGridData(this.form().employeeStatisticsTargets.value, false);
      });
  }

  private onCellValueChanged(event: CellValueChangedEvent) {
    if (!event.colDef.field || event.newValue === event.oldValue) return;

    const employeeStatisticForm = this.form().employeeStatisticsTargets.at(
      event.rowIndex ?? 0
    );

    // Update form when values change in the grid
    switch (event.colDef.field) {
      default:
        employeeStatisticForm.controls[event.colDef.field].patchValue(
          event.newValue
        );
        break;
    }
    this.setDirty();
  }

  private deleteRow(row: IShiftTypeEmployeeStatisticsTargetDTO): void {
    const rows = this.rows.value;
    rows.splice(rows.indexOf(row), 1);
    this.setGridData(rows);
  }

  private patchForm(): void {
    this.form().customStatisticsIdsPatchValue(this.rows.value);
    this.setDirty();
  }

  private setDirty() {
    this.form()?.markAsDirty();
    this.form()?.markAsTouched();
  }

  ngOnDestroy(): void {
    this._destroy$.next();
    this._destroy$.complete();
  }
}
