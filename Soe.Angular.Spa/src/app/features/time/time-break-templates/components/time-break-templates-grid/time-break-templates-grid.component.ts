import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { ITimeBreakTemplateGridDTONew } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { DateUtil } from '@shared/util/date-util';
import { DayOfWeek } from '@shared/util/Enumerations';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, Subscription } from 'rxjs';
import { map, take, tap } from 'rxjs/operators';
import { TimeCodeBreakGroupService } from '../../../time-code-break-group/services/time-code-break-group.service';
import { TimeBreakTemplatesService } from '../../services/time-break-templates.service';

@Component({
  selector: 'soe-time-break-templates-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class TimeBreakTemplatesGridComponent
  extends GridBaseDirective<
    ITimeBreakTemplateGridDTONew,
    TimeBreakTemplatesService
  >
  implements OnInit, OnDestroy
{
  service = inject(TimeBreakTemplatesService);
  timeCodeBreakGroupService = inject(TimeCodeBreakGroupService);

  breakGroups: SmallGenericType[] = [];
  private refreshSubscription: Subscription | undefined;

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(
      Feature.Time_Schedule_TimeBreakTemplate,
      'Time.Schedule.TimeBreakTemplates',
      { lookups: [this.loadBreakGroups()] }
    );

    this.refreshSubscription = this.service.refreshGrid$.subscribe(() => {
      this.refreshGrid$().subscribe(data => {
        this.service.emitGridData(data);
      });
    });
  }

  ngOnDestroy(): void {
    this.refreshSubscription?.unsubscribe();
  }

  private loadBreakGroups(): Observable<SmallGenericType[]> {
    return this.timeCodeBreakGroupService.getGrid().pipe(
      map(groups =>
        groups.map(g => new SmallGenericType(g.timeCodeBreakGroupId, g.name))
      ),
      tap(groups => {
        this.breakGroups = groups;
      })
    );
  }

  override onGridReadyToDefine(
    grid: GridComponent<ITimeBreakTemplateGridDTONew>
  ) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.rownr',
        'common.shifttype',
        'common.weekday',
        'time.schedule.timebreaktemplate.lengthincludingbreak',
        'time.schedule.timebreaktemplate.shiftstartfromtime',
        'time.schedule.timebreaktemplate.longbreaks',
        'time.schedule.timebreaktemplate.majornbrofbreaks',
        'time.schedule.timebreaktemplate.majorbreaktype',
        'time.schedule.timebreaktemplate.nbrofbreaks',
        'time.schedule.timebreaktemplate.shortbreaks',
        'time.schedule.timebreaktemplate.breaktype',
        'time.schedule.timebreaktemplate.mintimeafterstart',
        'time.schedule.timebreaktemplate.mintimebeforeend',
        'time.schedule.timebreaktemplate.mintimebetweenbreaks',
        'common.startdate',
        'common.stopdate',
        'core.edit',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnNumber('rowNr', terms['common.rownr'], {
          flex: 5,
          sortable: true,
          sort: 'asc',
        });

        this.grid.addColumnText('shiftTypeNames', terms['common.shifttype'], {
          flex: 15,
        });

        this.grid.addColumnText('dayOfWeekNames', terms['common.weekday'], {
          flex: 15,
          valueGetter: params =>
            params.data?.dayOfWeekNames
              ? this.translateWeekdayNames(params.data.dayOfWeekNames)
              : '',
        });

        const shiftLengthCol = this.grid.addColumnTimeSpan(
          'shiftLength',
          terms['time.schedule.timebreaktemplate.lengthincludingbreak'],
          { flex: 10, returnable: true, padHours: true }
        );
        shiftLengthCol.wrapHeaderText = true;
        shiftLengthCol.autoHeaderHeight = true;
        this.grid.columns.push(shiftLengthCol);

        const shiftStartCol = this.grid.addColumnTimeSpan(
          'shiftStartFromTimeMinutes',
          terms['time.schedule.timebreaktemplate.shiftstartfromtime'],
          { flex: 10, returnable: true, padHours: true }
        );
        shiftStartCol.wrapHeaderText = true;
        shiftStartCol.autoHeaderHeight = true;
        this.grid.columns.push(shiftStartCol);

        const longBreaksHeader = this.grid.addColumnHeader(
          '',
          terms['time.schedule.timebreaktemplate.longbreaks']
        );
        longBreaksHeader.suppressColumnsToolPanel = true;
        longBreaksHeader.suppressFiltersToolPanel = true;

        const majorNbrOfBreaksCol = this.grid.addColumnNumber(
          'majorNbrOfBreaks',
          terms['time.schedule.timebreaktemplate.majornbrofbreaks'],
          { flex: 8, returnable: true }
        );
        majorNbrOfBreaksCol.wrapHeaderText = true;
        majorNbrOfBreaksCol.autoHeaderHeight = true;
        majorNbrOfBreaksCol.suppressColumnsToolPanel = true;
        majorNbrOfBreaksCol.suppressFiltersToolPanel = true;
        this.grid.addChild(longBreaksHeader, majorNbrOfBreaksCol);

        const majorTimeCodeBreakGroupCol = this.grid.addColumnText(
          'majorTimeCodeBreakGroupName',
          terms['time.schedule.timebreaktemplate.majorbreaktype'],
          { flex: 10, returnable: true }
        );
        majorTimeCodeBreakGroupCol.wrapHeaderText = true;
        majorTimeCodeBreakGroupCol.autoHeaderHeight = true;
        majorTimeCodeBreakGroupCol.filter = 'agSetColumnFilter';
        majorTimeCodeBreakGroupCol.filterParams = {
          values: this.breakGroups.map(g => g.name),
        };
        majorTimeCodeBreakGroupCol.suppressColumnsToolPanel = true;
        majorTimeCodeBreakGroupCol.suppressFiltersToolPanel = true;
        this.grid.addChild(longBreaksHeader, majorTimeCodeBreakGroupCol);

        const majorMinTimeAfterStartCol = this.grid.addColumnNumber(
          'majorMinTimeAfterStart',
          terms['time.schedule.timebreaktemplate.mintimeafterstart'],
          { flex: 8, returnable: true }
        );
        majorMinTimeAfterStartCol.wrapHeaderText = true;
        majorMinTimeAfterStartCol.autoHeaderHeight = true;
        majorMinTimeAfterStartCol.suppressColumnsToolPanel = true;
        majorMinTimeAfterStartCol.suppressFiltersToolPanel = true;
        this.grid.addChild(longBreaksHeader, majorMinTimeAfterStartCol);

        const majorMinTimeBeforeEndCol = this.grid.addColumnNumber(
          'majorMinTimeBeforeEnd',
          terms['time.schedule.timebreaktemplate.mintimebeforeend'],
          {
            flex: 8,
            returnable: true,
          }
        );
        majorMinTimeBeforeEndCol.wrapHeaderText = true;
        majorMinTimeBeforeEndCol.autoHeaderHeight = true;
        majorMinTimeBeforeEndCol.suppressColumnsToolPanel = true;
        majorMinTimeBeforeEndCol.suppressFiltersToolPanel = true;
        this.grid.addChild(longBreaksHeader, majorMinTimeBeforeEndCol);

        const shortBreaksHeader = this.grid.addColumnHeader(
          '',
          terms['time.schedule.timebreaktemplate.shortbreaks']
        );
        shortBreaksHeader.suppressColumnsToolPanel = true;
        shortBreaksHeader.suppressFiltersToolPanel = true;

        const minorNbrOfBreaksCol = this.grid.addColumnNumber(
          'minorNbrOfBreaks',
          terms['time.schedule.timebreaktemplate.nbrofbreaks'],
          { flex: 8, returnable: true }
        );
        minorNbrOfBreaksCol.wrapHeaderText = true;
        minorNbrOfBreaksCol.autoHeaderHeight = true;
        minorNbrOfBreaksCol.suppressColumnsToolPanel = true;
        minorNbrOfBreaksCol.suppressFiltersToolPanel = true;
        this.grid.addChild(shortBreaksHeader, minorNbrOfBreaksCol);

        const minorTimeCodeBreakGroupCol = this.grid.addColumnText(
          'minorTimeCodeBreakGroupName',
          terms['time.schedule.timebreaktemplate.breaktype'],
          { flex: 10, returnable: true }
        );
        minorTimeCodeBreakGroupCol.wrapHeaderText = true;
        minorTimeCodeBreakGroupCol.autoHeaderHeight = true;
        minorTimeCodeBreakGroupCol.filter = 'agSetColumnFilter';
        minorTimeCodeBreakGroupCol.filterParams = {
          values: this.breakGroups.map(g => g.name),
        };
        minorTimeCodeBreakGroupCol.suppressColumnsToolPanel = true;
        minorTimeCodeBreakGroupCol.suppressFiltersToolPanel = true;
        this.grid.addChild(shortBreaksHeader, minorTimeCodeBreakGroupCol);

        const minorMinTimeAfterStartCol = this.grid.addColumnNumber(
          'minorMinTimeAfterStart',
          terms['time.schedule.timebreaktemplate.mintimeafterstart'],
          { flex: 8, returnable: true }
        );
        minorMinTimeAfterStartCol.wrapHeaderText = true;
        minorMinTimeAfterStartCol.autoHeaderHeight = true;
        minorMinTimeAfterStartCol.suppressColumnsToolPanel = true;
        minorMinTimeAfterStartCol.suppressFiltersToolPanel = true;
        this.grid.addChild(shortBreaksHeader, minorMinTimeAfterStartCol);

        const minorMinTimeBeforeEndCol = this.grid.addColumnNumber(
          'minorMinTimeBeforeEnd',
          terms['time.schedule.timebreaktemplate.mintimebeforeend'],
          {
            flex: 8,
            returnable: true,
          }
        );
        minorMinTimeBeforeEndCol.wrapHeaderText = true;
        minorMinTimeBeforeEndCol.autoHeaderHeight = true;
        minorMinTimeBeforeEndCol.suppressColumnsToolPanel = true;
        minorMinTimeBeforeEndCol.suppressFiltersToolPanel = true;
        this.grid.addChild(shortBreaksHeader, minorMinTimeBeforeEndCol);

        const minTimeBetweenBreaksCol = this.grid.addColumnNumber(
          'minTimeBetweenBreaks',
          terms['time.schedule.timebreaktemplate.mintimebetweenbreaks'],
          { flex: 10, returnable: true }
        );
        minTimeBetweenBreaksCol.wrapHeaderText = true;
        minTimeBetweenBreaksCol.autoHeaderHeight = true;
        this.grid.columns.push(minTimeBetweenBreaksCol);

        this.grid.addColumnDate('startDate', terms['common.startdate'], {
          flex: 10,
        });

        this.grid.addColumnDate('stopDate', terms['common.stopdate'], {
          flex: 10,
        });

        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            this.edit(row);
          },
        });

        super.finalizeInitGrid();
      });
  }

  override onAfterLoadData(data?: ITimeBreakTemplateGridDTONew[]): void {
    if (data) {
      data.forEach((item, index) => {
        item.rowNr = index + 1;
        (item as any).name = item.rowNr.toString();
      });
    }
  }

  private translateWeekdayNames(englishNames: string): string {
    return englishNames
      .split(',')
      .map(name => {
        const trimmed = name.trim();
        const capitalized =
          trimmed.charAt(0).toUpperCase() + trimmed.slice(1).toLowerCase();
        const dayOfWeek = DayOfWeek[capitalized as keyof typeof DayOfWeek];
        if (dayOfWeek !== undefined) {
          return DateUtil.getDayOfWeekName(dayOfWeek, true);
        }
        return trimmed;
      })
      .join(', ');
  }
}
