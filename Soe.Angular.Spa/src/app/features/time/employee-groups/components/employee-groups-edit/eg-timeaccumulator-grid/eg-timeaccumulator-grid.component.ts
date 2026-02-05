import { Component, inject, input, Input, OnInit, signal } from '@angular/core';
import { EgTimeAccumulatorsRulesForm } from '@features/time/employee-groups/models/eg-time-accumulators-rules-form.model';
import { EmployeeGroupsForm } from '@features/time/employee-groups/models/employee-groups-form.model';
import { EmployeeGroupsService } from '@features/time/employee-groups/services/employee-groups.service';
import { EmbeddedGridBaseDirective } from '@shared/directives/grid-base/embedded-grid-base.directive';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  Feature,
  SoeEntityState,
  TermGroup,
  TermGroup_AccumulatorTimePeriodType,
} from '@shared/models/generated-interfaces/Enumerations';
import { ITimeAccumulatorEmployeeGroupRuleDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarEmbeddedGridConfig } from '@ui/toolbar/models/toolbar';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take, tap } from 'rxjs';
@Component({
  selector: 'soe-eg-time-accumulators-grid',
  standalone: false,
  templateUrl:
    '../../../../../../shared/ui-components/grid/grid-wrapper/embedded-grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
})
export class EgTimeAccumulatorGridComponent
  extends EmbeddedGridBaseDirective<
    ITimeAccumulatorEmployeeGroupRuleDTO,
    EmployeeGroupsForm
  >
  implements OnInit
{
  @Input({ required: true }) form!: EmployeeGroupsForm;

  noMargin = input(true);
  height = input(60);

  timeAccumulatorEmployeeGroupRules: SmallGenericType[] = [];
  periods: SmallGenericType[] = [];
  scheduledJobHeads: SmallGenericType[] = [];

  employeeGroupsService = inject(EmployeeGroupsService);
  coreService = inject(CoreService);

  override ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Employee_Groups_Edit,
      'time.employee.employeegroups.timeaccumulators.grid',
      {
        lookups: [
          this.loadTimeAccumulators(),
          this.loadPeriods(),
          this.loadScheduledJobHeads(),
        ],
      }
    );
    this.form.timeAccumulatorEmployeeGroupRules.valueChanges.subscribe(v =>
      this.initRows(v)
    );
  }

  override createGridToolbar(
    config?: Partial<ToolbarEmbeddedGridConfig>
  ): void {
    super.createGridToolbar();
    this.toolbarService.createItemGroup({
      alignLeft: true,
      items: [
        this.toolbarService.createToolbarLabel('label', {
          labelKey: signal('time.time.timeaccumulator.employeegrouprules'),
        }),
      ],
    });
  }

  override onGridReadyToDefine(
    grid: GridComponent<ITimeAccumulatorEmployeeGroupRuleDTO>
  ): void {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'time.time.timeaccumulators.timeaccumulator',
        'common.type',
        'time.time.timeaccumulator.employeegrouprule.minminuteswarning',
        'time.time.timeaccumulator.employeegrouprule.minminutes',
        'time.time.timeaccumulator.employeegrouprule.maxminuteswarning',
        'time.time.timeaccumulator.employeegrouprule.maxminutes',
        'time.time.timeaccumulator.employeegrouprule.showonpayrollslip',
        'time.time.timeaccumulator.employeegrouprule.scheduledjob',
        'time.time.timeaccumulator.employeegrouprule.type',
        'core.permission',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnSelect(
          'timeAccumulatorId',
          terms['time.time.timeaccumulators.timeaccumulator'],
          this.timeAccumulatorEmployeeGroupRules || [],
          undefined,
          {
            dropDownIdLabel: 'id',
            dropDownValueLabel: 'name',
            flex: 12,
            editable: true,
            suppressFilter: true,
            suppressFloatingFilter: true,
          }
        );
        this.grid.addColumnSelect(
          'type',
          terms['time.time.timeaccumulator.employeegrouprule.type'],
          this.periods || [],
          undefined,
          {
            dropDownIdLabel: 'id',
            dropDownValueLabel: 'name',
            flex: 12,
            editable: true,
            suppressFilter: true,
            suppressFloatingFilter: true,
          }
        );
        this.grid.addColumnTimeSpan(
          'minMinutesWarning',
          terms[
            'time.time.timeaccumulator.employeegrouprule.minminuteswarning'
          ],
          {
            flex: 12,
            editable: true,
            suppressFilter: true,
            suppressFloatingFilter: true,
            disableTimeFormatting: true,
          }
        );
        this.grid.addColumnTimeSpan(
          'minMinutes',
          terms['time.time.timeaccumulator.employeegrouprule.minminutes'],
          {
            flex: 12,
            editable: true,
            suppressFilter: true,
            suppressFloatingFilter: true,
            disableTimeFormatting: true,
          }
        );
        this.grid.addColumnTimeSpan(
          'maxMinutesWarning',
          terms[
            'time.time.timeaccumulator.employeegrouprule.maxminuteswarning'
          ],
          {
            flex: 12,
            editable: true,
            suppressFilter: true,
            suppressFloatingFilter: true,
            disableTimeFormatting: true,
          }
        );
        this.grid.addColumnTimeSpan(
          'maxMinutes',
          terms['time.time.timeaccumulator.employeegrouprule.maxminutes'],
          {
            flex: 12,
            editable: true,
            suppressFilter: true,
            suppressFloatingFilter: true,
            disableTimeFormatting: true,
          }
        );
        this.grid.addColumnBool(
          'showOnPayrollSlip',
          terms[
            'time.time.timeaccumulator.employeegrouprule.showonpayrollslip'
          ],
          {
            flex: 12,
            editable: true,
            suppressFilter: true,
            suppressFloatingFilter: true,
          }
        );
        this.grid.addColumnSelect(
          'scheduledJobHeadId',
          terms['time.time.timeaccumulator.employeegrouprule.scheduledjob'],
          this.scheduledJobHeads || [],
          undefined,
          {
            dropDownIdLabel: 'id',
            dropDownValueLabel: 'name',
            flex: 12,
            editable: true,
            suppressFilter: true,
            suppressFloatingFilter: true,
          }
        );
        this.grid.addColumnIconDelete({
          tooltip: terms['core.delete'],
          onClick: row => {
            this.deleteRow(row);
          },
          suppressFilter: true,
          suppressFloatingFilter: true,
        });

        this.grid.setNbrOfRowsToShow(1, 10);
        this.grid.context.suppressFiltering = true;
        super.finalizeInitGrid({ hidden: true });
      });
  }

  private initRows(rows: ITimeAccumulatorEmployeeGroupRuleDTO[]) {
    this.rowData.next(rows);
  }

  override addRow(): void {
    const row: ITimeAccumulatorEmployeeGroupRuleDTO = {
      employeeGroupId: this.form?.value.employeeGroupId,
      type: TermGroup_AccumulatorTimePeriodType.Unknown,
      showOnPayrollSlip: false,
      timeAccumulatorEmployeeGroupRuleId: 0,
      timeAccumulatorId: 0,
      state: SoeEntityState.Active,
    };
    super.addRow(
      row,
      this.form.timeAccumulatorEmployeeGroupRules,
      EgTimeAccumulatorsRulesForm
    );
  }

  override deleteRow(row: ITimeAccumulatorEmployeeGroupRuleDTO) {
    super.deleteRow(row, this.form.timeAccumulatorEmployeeGroupRules);
  }

  private loadTimeAccumulators() {
    return this.employeeGroupsService
      .getTimeAccumulatorsDict(true, false, true)
      .pipe(tap(x => (this.timeAccumulatorEmployeeGroupRules = x)));
  }

  private loadPeriods() {
    return this.coreService
      .getTermGroupContent(TermGroup.AccumulatorTimePeriodType, true, false)
      .pipe(tap(x => (this.periods = x)));
  }

  private loadScheduledJobHeads() {
    return this.coreService
      .getScheduledJobHeadsDict(true, false)
      .pipe(tap(x => (this.scheduledJobHeads = x)));
  }
}
