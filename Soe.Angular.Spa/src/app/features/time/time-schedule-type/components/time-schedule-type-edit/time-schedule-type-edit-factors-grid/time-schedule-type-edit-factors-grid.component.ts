import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import {
  Component,
  input,
  Input,
  OnDestroy,
  OnInit,
  signal,
} from '@angular/core';
import { ITimeScheduleTypeFactorDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { BehaviorSubject, Subject, take, takeUntil } from 'rxjs';
import { TimeScheduleTypeForm } from '@features/time/time-schedule-type/models/time-schedule-type-form.model';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { DateUtil } from '@shared/util/date-util';
import { CellValueChangedEvent } from 'ag-grid-community';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';

@Component({
  selector: 'soe-time-schedule-type-edit-factors-grid',
  templateUrl: './time-schedule-type-edit-factors-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class TimeScheduleTypeEditFactorsGridComponent
  extends GridBaseDirective<ITimeScheduleTypeFactorDTO>
  implements OnInit, OnDestroy
{
  form = input.required<TimeScheduleTypeForm>();

  private _destroy$ = new Subject<void>();

  factors = new BehaviorSubject<ITimeScheduleTypeFactorDTO[]>([]);

  override ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Preferences_TimeSettings_TimeScheduleType_Edit,
      'Time.Schedule.ScheduleTypes.Factors.Rows',
      { skipInitialLoad: true }
    );

    // Update grid data when form changes
    this.form()
      ?.factors.valueChanges.pipe(takeUntil(this._destroy$))
      .subscribe(factors => {
        this.setGridData(factors, false);
      });

    this.form()
      ?.valueChanges.pipe(takeUntil(this._destroy$))
      .subscribe(frm => {
        // Set grid data when form is loaded on copy
        if (frm.isCopy) {
          this.form().controls.isCopy.patchValue(false);
          this.setGridData(frm.factors, false);
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
          onAction: () => this.addFactor(),
        }),
      ],
    });
  }

  override onGridReadyToDefine(
    grid: GridComponent<ITimeScheduleTypeFactorDTO>
  ): void {
    super.onGridReadyToDefine(grid);

    this.grid.api.updateGridOptions({
      onCellValueChanged: this.onCellValueChanged.bind(this),
    });

    this.translate
      .get([
        'time.employee.employee.factor',
        'time.schedule.scheduletype.factorfromtime',
        'time.schedule.scheduletype.factortotime',
        'common.length',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnNumber(
          'factor',
          terms['time.employee.employee.factor'],
          {
            editable: true,
            flex: 1,
            decimals: 2,
            maxDecimals: 2,
            suppressFilter: true,
            suppressFloatingFilter: true,
          }
        );
        this.grid.addColumnTime(
          'fromTime',
          terms['time.schedule.scheduletype.factorfromtime'],
          {
            editable: true,
            flex: 1,
            suppressFilter: true,
            suppressFloatingFilter: true,
          }
        );
        this.grid.addColumnTime(
          'toTime',
          terms['time.schedule.scheduletype.factortotime'],
          {
            editable: true,
            flex: 1,
            suppressFilter: true,
            suppressFloatingFilter: true,
          }
        );
        this.grid.addColumnTimeSpan('length', terms['common.length'], {
          editable: false,
          flex: 1,
          suppressFilter: true,
          suppressFloatingFilter: true,
        });
        this.grid.addColumnIconDelete({
          tooltip: terms['core.delete'],
          onClick: row => {
            this.deleteFactor(row);
          },
          suppressFilter: true,
          suppressFloatingFilter: true,
        });

        this.grid.setNbrOfRowsToShow(1, 7); // Adjusts height based on number of rows
        this.grid.context.suppressFiltering = true;
        super.finalizeInitGrid({ hidden: true });
        this.setGridData(this.form().factors.value, false);
      });
  }

  private addFactor(): void {
    const factors = this.factors.value;
    const factor: Partial<ITimeScheduleTypeFactorDTO> = {
      factor: 0,
      fromTime: DateUtil.defaultDateTime(),
      toTime: DateUtil.defaultDateTime(),
    };
    factors.push(factor as ITimeScheduleTypeFactorDTO);
    this.setGridData(factors);
    setTimeout(() => {
      this.grid.startEditing(factors.length - 1, 'factor');
    });
  }

  private deleteFactor(factor: ITimeScheduleTypeFactorDTO): void {
    const factors = this.factors.value;
    factors.splice(factors.indexOf(factor), 1);
    this.setGridData(factors);
  }

  private setGridData(
    factors: ITimeScheduleTypeFactorDTO[],
    patchForm = true
  ): void {
    if (this.grid) this.factors.next(factors);
    if (patchForm) {
      const currentData = [...factors];
      this.form().patchFactors(currentData);
      this.setDirty();
    }
  }

  private onCellValueChanged(event: CellValueChangedEvent) {
    if (!event.colDef.field || event.newValue === event.oldValue) return;

    const factorsForm = this.form().factors.at(event.rowIndex ?? 0);

    // Update form when values change in the grid
    switch (event.colDef.field) {
      case 'fromTime':
        factorsForm.controls.fromTime.patchValue(event.newValue);
        factorsForm.setLength();
        break;
      case 'toTime':
        factorsForm.controls.toTime.patchValue(event.newValue);
        factorsForm.setLength();
        break;

      default:
        // All fields that are not specificly handled above
        factorsForm.controls[event.colDef.field].patchValue(event.newValue);
        break;
    }

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
