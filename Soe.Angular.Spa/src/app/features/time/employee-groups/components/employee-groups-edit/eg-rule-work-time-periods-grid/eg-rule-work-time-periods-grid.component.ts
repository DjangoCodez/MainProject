import { Component, inject, input, Input, OnInit } from '@angular/core';
import { AccountProvisionBaseService } from '@features/time/account-provision-base/services/account-provision-base.service';
import { EmployeeGroupRuleWorkTimePeriodsForm } from '@features/time/employee-groups/models/eg-rule-work-time-periods-form.model';
import { EmployeeGroupsForm } from '@features/time/employee-groups/models/employee-groups-form.model';
import { PlanningPeriodsService } from '@features/time/planning-periods/services/planning-periods.service';
import { EmbeddedGridBaseDirective } from '@shared/directives/grid-base/embedded-grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import {
  IEmployeeGroupRuleWorkTimePeriodDTO,
  ITimePeriodHeadDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { Perform } from '@shared/util/perform.class';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, take, tap } from 'rxjs';

interface IEmployeeGroupRuleWorkTimePeriodExtendedDTO
  extends IEmployeeGroupRuleWorkTimePeriodDTO {
  timePeriodHeadId?: number;
}
@Component({
  selector: 'soe-eg-rule-work-time-periods-grid',
  standalone: false,
  templateUrl:
    '../../../../../../shared/ui-components/grid/grid-wrapper/embedded-grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
})
export class EgRuleWorkTimePeriodsGridComponent
  extends EmbeddedGridBaseDirective<
    IEmployeeGroupRuleWorkTimePeriodDTO,
    EmployeeGroupsForm
  >
  implements OnInit
{
  @Input({ required: true }) form!: EmployeeGroupsForm;

  noMargin = input(true);
  height = input(60);

  accountProvisionBaseService = inject(AccountProvisionBaseService);

  planningPeriodService = inject(PlanningPeriodsService);

  timePeriodHeads: SmallGenericType[] = [];
  timePeriods: SmallGenericType[] = [];

  performTimePeriods = new Perform<ITimePeriodHeadDTO[]>(this.progressService);

  headToTimePeriodMap = new Map<number, SmallGenericType[]>();
  timePeriodToHeadMap = new Map<number, number>();

  override ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Employee_Groups_Edit,
      'time.employee.employeegroups.ruleworktimeperiods.grid',
      {
        lookups: [this.loadTimePeriodHeadsIncludingPeriodsForType()],
      }
    );
    this.form.ruleWorkTimePeriods.valueChanges.subscribe(v => this.initRows(v));
  }

  override onGridReadyToDefine(
    grid: GridComponent<IEmployeeGroupRuleWorkTimePeriodDTO>
  ): void {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'time.time.planningperiod.planningperiod',
        'time.time.timeperiod.timeperiod',
        'time.employee.employeegroup.ruleworktime',
        'core.permission',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnSelect(
          'timePeriodHeadId' as keyof IEmployeeGroupRuleWorkTimePeriodDTO,
          terms['time.time.planningperiod.planningperiod'],
          this.timePeriodHeads || [],
          undefined,
          {
            dropDownIdLabel: 'id',
            dropDownValueLabel: 'name',
            flex: 33,
            editable: true,
          }
        );
        this.grid.addColumnSelect(
          'timePeriodId',
          terms['time.time.timeperiod.timeperiod'],
          this.timePeriods || [],
          undefined,
          {
            dropDownIdLabel: 'id',
            dropDownValueLabel: 'name',
            flex: 33,
            editable: true,
            suppressFilter: true,
            dynamicSelectOptions: (row: any) => {
              return this.getPeriodSelectOptions(row);
            },
          }
        );
        this.grid.addColumnTimeSpan(
          'ruleWorkTime',
          terms['time.employee.employeegroup.ruleworktime'],
          {
            flex: 33,
            editable: true,
            disableTimeFormatting: true,
          }
        );
        this.grid.addColumnIconDelete({
          tooltip: terms['core.delete'],
          onClick: row => {
            this.deleteRow(row);
          },
        });

        this.grid.setNbrOfRowsToShow(1, 10);
        super.finalizeInitGrid({ hidden: true });
      });
  }

  private initRows(rows: IEmployeeGroupRuleWorkTimePeriodDTO[]) {
    // Add HeadId as it isn't in the DTO
    const extendedRows = rows.map(row => {
      const extendedRow = row as IEmployeeGroupRuleWorkTimePeriodExtendedDTO;
      if (row.timePeriodId && this.timePeriodToHeadMap.has(row.timePeriodId)) {
        extendedRow.timePeriodHeadId = this.timePeriodToHeadMap.get(
          row.timePeriodId
        );
      }
      return extendedRow;
    });

    this.rowData.next(extendedRows);
  }

  override addRow(): void {
    const row: IEmployeeGroupRuleWorkTimePeriodDTO = {
      employeeGroupRuleWorkTimePeriodId: 0,
      employeeGroupId: this.form?.value.employeeGroupId,
      timePeriodId: 0,
      ruleWorkTime: 0,
    };
    super.addRow(
      row,
      this.form.ruleWorkTimePeriods,
      EmployeeGroupRuleWorkTimePeriodsForm
    );
  }

  override deleteRow(row: IEmployeeGroupRuleWorkTimePeriodDTO) {
    super.deleteRow(row, this.form.ruleWorkTimePeriods);
  }

  loadTimePeriodHeadsIncludingPeriodsForType(): Observable<
    ITimePeriodHeadDTO[]
  > {
    return this.planningPeriodService
      .getTimePeriodHeadsIncludingPeriodsForType()
      .pipe(
        tap(x => {
          // Set all heads in list
          x.forEach(head => {
            this.timePeriodHeads.unshift({
              id: head.timePeriodHeadId,
              name: head.name,
            });
            const timePeriodsTemp: SmallGenericType[] = []; // Store timePeriods per head temporary to set in map
            head.timePeriods.forEach(timePeriod => {
              // All timePeriods in one list
              this.timePeriods.unshift({
                id: timePeriod.timePeriodId,
                name: timePeriod.name,
              });
              // Set timeperiods per head temporarily
              timePeriodsTemp.unshift({
                id: timePeriod.timePeriodId,
                name: timePeriod.name,
              });

              // Map period to head
              this.timePeriodToHeadMap.set(
                timePeriod.timePeriodId,
                head.timePeriodHeadId
              );
            });
            // Map head to timeperiod
            this.headToTimePeriodMap.set(
              head.timePeriodHeadId,
              timePeriodsTemp
            );
            this.initRows(this.form?.ruleWorkTimePeriods.value); // Makes sure rows are initiated with the mapping
          });
        })
      );
  }

  getPeriodSelectOptions(row: any) {
    const timePeriodHeadId = row?.data?.timePeriodHeadId || 0;
    return this.headToTimePeriodMap.get(timePeriodHeadId) || [];
  }
}
