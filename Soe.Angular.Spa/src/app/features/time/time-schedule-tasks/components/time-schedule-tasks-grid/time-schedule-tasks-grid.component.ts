import { Component, inject, OnInit, signal } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { ShiftTypeService } from '@shared/features/shift-type/services/shift-type.service';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  CompanySettingType,
  Feature,
} from '@shared/models/generated-interfaces/Enumerations';
import { ITimeScheduleTaskGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { SettingsUtil } from '@shared/util/settings-util';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, take, tap } from 'rxjs';
import { TimeScheduleTasksService } from '../../services/time-schedule-tasks.service';

@Component({
  selector: 'soe-time-schedule-tasks-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class TimeScheduleTasksGridComponent
  extends GridBaseDirective<ITimeScheduleTaskGridDTO, TimeScheduleTasksService>
  implements OnInit
{
  service = inject(TimeScheduleTasksService);
  shiftTypeService = inject(ShiftTypeService);
  private readonly coreService = inject(CoreService);
  progressService = inject(ProgressService);
  useAccountsHierarchy = signal(false);

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Schedule_StaffingNeeds_Tasks,
      'Time.Schedule.StaffingNeeds.Tasks',
      { lookups: [this.loadShiftTypes(), this.loadTaskTypes()] }
    );
  }

  override loadCompanySettings() {
    return this.coreService
      .getCompanySettings([CompanySettingType.UseAccountHierarchy])
      .pipe(
        tap((x: any) => {
          this.useAccountsHierarchy.set(
            SettingsUtil.getBoolCompanySetting(
              x,
              CompanySettingType.UseAccountHierarchy
            )
          );
        })
      );
  }

  private loadShiftTypes(): Observable<SmallGenericType[]> {
    return this.shiftTypeService.getShiftTypesDict(true).pipe(
      tap(shiftTypes => {
        // Rename empty to 'Not specified'
        const empty = shiftTypes.find(s => s.id === 0);
        if (empty) empty.name = this.translate.instant('core.notspecified');
      })
    );
  }

  private loadTaskTypes(): Observable<SmallGenericType[]> {
    return this.service.getTaskTypesDict(false);
  }

  override onGridReadyToDefine(grid: GridComponent<ITimeScheduleTaskGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.active',
        'common.dailyrecurrencepattern',
        'common.dailyrecurrencepattern.rangetype',
        'common.dailyrecurrencepattern.startdate',
        'common.description',
        'common.name',
        'common.user.attestrole.accounthierarchy',
        'core.edit',
        'time.schedule.shifttype.shifttype',
        'time.schedule.timescheduletask.length',
        'time.schedule.timescheduletask.starttime',
        'time.schedule.timescheduletask.stoptime',
        'time.schedule.timescheduletasktype.allowoverlapping',
        'time.schedule.timescheduletasktype.dontassignbreakleftovers',
        'time.schedule.timescheduletasktype.isstaffingneedsfrequency',
        'time.schedule.timescheduletasktype.nbrofpersons',
        'time.schedule.timescheduletasktype.onlyoneemployee',
        'time.schedule.timescheduletasktype.type',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnActive('state', terms['common.active'], {
          idField: 'timeScheduleTaskId',
        });
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 20,
        });
        this.grid.addColumnText('description', terms['common.description'], {
          flex: 20,
          enableHiding: true,
        });
        this.grid.addColumnTime(
          'startTime',
          terms['time.schedule.timescheduletask.starttime'],
          {
            flex: 10,
            enableHiding: true,
          }
        );
        this.grid.addColumnTime(
          'stopTime',
          terms['time.schedule.timescheduletask.stoptime'],
          {
            flex: 10,
            enableHiding: true,
          }
        );
        this.grid.addColumnTimeSpan(
          'length',
          terms['time.schedule.timescheduletask.length'],
          {
            flex: 10,
            enableHiding: true,
          }
        );
        this.grid.addColumnSelect(
          'shiftTypeId',
          terms['time.schedule.shifttype.shifttype'],
          this.shiftTypeService.performShiftTypes.data || [],
          undefined,
          {
            flex: 20,
            enableHiding: true,
          }
        );
        if ((this.service.performTaskTypes.data?.length || 0) > 0) {
          this.grid.addColumnSelect(
            'typeId',
            terms['time.schedule.timescheduletasktype.type'],
            this.service.performTaskTypes.data || [],
            undefined,
            {
              flex: 20,
              enableHiding: true,
            }
          );
        }
        if (this.useAccountsHierarchy()) {
          this.grid.addColumnText(
            'accountName',
            terms['common.user.attestrole.accounthierarchy'],
            {
              flex: 20,
              enableHiding: true,
            }
          );
        }
        this.grid.addColumnText(
          'recurrenceStartsOnDescription',
          terms['common.dailyrecurrencepattern.startdate'],
          {
            flex: 10,
            enableHiding: true,
          }
        );
        this.grid.addColumnText(
          'recurrenceEndsOnDescription',
          terms['common.dailyrecurrencepattern.rangetype'],
          {
            flex: 10,
            enableHiding: true,
          }
        );
        this.grid.addColumnText(
          'recurrencePatternDescription',
          terms['common.dailyrecurrencepattern'],
          {
            flex: 10,
            enableHiding: true,
          }
        );
        this.grid.addColumnNumber(
          'nbrOfPersons',
          terms['time.schedule.timescheduletasktype.nbrofpersons'],
          {
            flex: 10,
            enableHiding: true,
          }
        );
        this.grid.addColumnBool(
          'onlyOneEmployee',
          terms['time.schedule.timescheduletasktype.onlyoneemployee'],
          {
            flex: 10,
            enableHiding: true,
          }
        );
        this.grid.addColumnBool(
          'dontAssignBreakLeftovers',
          terms['time.schedule.timescheduletasktype.dontassignbreakleftovers'],
          {
            flex: 10,
            enableHiding: true,
          }
        );
        this.grid.addColumnBool(
          'allowOverlapping',
          terms['time.schedule.timescheduletasktype.allowoverlapping'],
          {
            flex: 10,
            enableHiding: true,
          }
        );
        this.grid.addColumnBool(
          'isStaffingNeedsFrequency',
          terms['time.schedule.timescheduletasktype.isstaffingneedsfrequency'],
          {
            flex: 10,
            enableHiding: true,
          }
        );
        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            this.edit(row);
          },
        });
        super.finalizeInitGrid();
      });
  }
}
