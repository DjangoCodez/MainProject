import { Component, input, Input, OnInit } from '@angular/core';
import { PlanningPeriodsForm } from '@features/time/planning-periods/models/planning-periods-form.model';
import { TimePeriodForm } from '@features/time/planning-periods/models/pp-timeperiods-form.model';
import { EmbeddedGridBaseDirective } from '@shared/directives/grid-base/embedded-grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { ITimePeriodDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { startWith, take } from 'rxjs';

@Component({
  selector: 'soe-pp-timeperiods-grid',
  standalone: false,
  templateUrl:
    '../../../../../../shared/ui-components/grid/grid-wrapper/embedded-grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
})
export class PpTimeperiodsGridComponent
  extends EmbeddedGridBaseDirective<
    ITimePeriodDTO,
    PlanningPeriodsForm,
    TimePeriodForm
  >
  implements OnInit
{
  @Input({ required: true }) form!: PlanningPeriodsForm;

  toolbarNoBorder = input(true);
  toolbarNoMargin = input(true);
  toolbarNoTopBottomPadding = input(true);
  height = input(66);

  override ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Preferences_TimeSettings_PlanningPeriod,
      'time.planningperiods.timeperiods.grid'
    );

    this.form.timePeriods.valueChanges
      .pipe(startWith(this.form.timePeriods.getRawValue()))
      .subscribe(v => {
        this.initRows(v);
      });
  }

  override onGridReadyToDefine(grid: GridComponent<ITimePeriodDTO>): void {
    super.onGridReadyToDefine(grid);

    // setup embedded grid options for enabling built in functionality in base class
    //this.embeddedGridOptions.showValidationErrors = true;
    this.embeddedGridOptions.newRowStartEditField = 'name';
    this.embeddedGridOptions.formRows = this.form.timePeriods;
    this.embeddedGridOptions.rowType = {} as ITimePeriodDTO;
    this.embeddedGridOptions.rowFormType = new TimePeriodForm({
      validationHandler: this.form.formValidationHandler,
      element: undefined,
    });

    this.translate
      .get([
        'common.name',
        'common.from',
        'common.to',
        'common.permission',
        'core.delete',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 33,
          editable: true,
        });
        this.grid.addColumnDate('startDate', terms['common.from'], {
          flex: 33,
          editable: true,
        });
        this.grid.addColumnDate('stopDate', terms['common.to'], {
          flex: 33,
          editable: true,
        });
        this.grid.addColumnIconDelete({
          tooltip: terms['core.delete'],
          onClick: row => {
            this.deleteRow(row);
          },
        });

        this.grid.setNbrOfRowsToShow(1);
        super.finalizeInitGrid({ hidden: true });
      });
  }

  override onCellEditingStopped(event: any) {
    // Keep base behavior (marks form dirty)
    super.onCellEditingStopped(event);

    // Optionally trigger array validity if you have cross-row validators
    // this.form.timePeriods.updateValueAndValidity({ emitEvent: false });
  }

  private initRows(rows: ITimePeriodDTO[]) {
    this.rowData.next(rows);
  }

  override addRow(): void {
    const row: any = {
      timePeriodId: 0,
      timePeriodHeadId: this.form?.value.timePeriodHeadId,
      startDate: this.getLatestDate()?.addDays(1) ?? null,
      stopDate: this.getLatestDate()?.addDays(1) ?? null,
      rowNr: (this.getMaxRowNumber() ?? 0) + 1,
    };
    super.addRow(row, this.form.timePeriods, TimePeriodForm);
  }

  override deleteRow(row: any) {
    super.deleteRow(row, this.form.timePeriods);
  }

  // HELPER METHODS
  private getLatestDate() {
    return this.form.timePeriods.value.filter((tp: ITimePeriodDTO) => {
      return tp.stopDate;
    }).length > 0 // Only calculate latest date if has any stopdates
      ? new Date(
          Math.max(
            ...this.form.timePeriods.value.map((tp: ITimePeriodDTO) => {
              {
                return tp.stopDate;
              }
            })
          )
        )
      : null;
  }

  private getMaxRowNumber() {
    return Math.max(
      0,
      ...this.form.timePeriods.value.map((tp: ITimePeriodDTO) => {
        return tp.rowNr;
      })
    );
  }
}
