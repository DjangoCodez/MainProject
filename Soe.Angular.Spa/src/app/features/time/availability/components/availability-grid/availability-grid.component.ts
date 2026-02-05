import { Component, OnInit, inject, signal } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { IEmployeeRequestGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { AvailabilityService } from '../../services/availability.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { GridComponent } from '@ui/grid/grid.component';
import {
  Feature,
  TermGroup_EmployeeRequestType,
} from '@shared/models/generated-interfaces/Enumerations';
import { Observable, take } from 'rxjs';
import { DateRangeValue } from '@ui/forms/datepicker/daterangepicker/daterangepicker.component';
import { ToolbarDaterangepickerAction } from '@ui/toolbar/toolbar-daterangepicker/toolbar-daterangepicker.component';
import { DateUtil } from '@shared/util/date-util';

@Component({
  selector: 'soe-availability-grid',
  templateUrl: './availability-grid.component.html',
  standalone: false,
  providers: [FlowHandlerService, ToolbarService],
})
export class AvailabilityGridComponent
  extends GridBaseDirective<IEmployeeRequestGridDTO, AvailabilityService>
  implements OnInit
{
  service = inject(AvailabilityService);

  private defaultDateFrom = () => DateUtil.getDateFirstInWeek(DateUtil.getToday());
  private defaultDateTo = () => DateUtil.getDateLastInWeek(DateUtil.getToday()).addDays(7);

  dateFrom = signal<Date>(this.defaultDateFrom());
  dateTo = signal<Date>(this.defaultDateTo());
  private toolbarDaterangeInitialDates = signal<DateRangeValue>([
    this.dateFrom(),
    this.dateTo(),
  ]);

  ngOnInit() {
    super.ngOnInit();
    this.setupToolbar();

    this.startFlow(
      Feature.Time_Schedule_Availability,
      'Time.Schedule.Availability'
    );
  }

  private setupToolbar(): void {
    this.toolbarService.createItemGroup({
      alignLeft: true,
      items: [
        this.toolbarService.createToolbarDaterangepicker('dateRange', {
          initialDates: this.toolbarDaterangeInitialDates,
          showArrows: signal(false),
          separatorDash: signal(true),
          onValueChanged: event => this.onDateRangeChanged((event as ToolbarDaterangepickerAction)?.value),
        }),
        this.toolbarService.createToolbarButton('filter', {
          iconName: signal('filter'),
          tooltip: signal('core.filter'),
          onAction: () => this.onFilterClick(),
        }),
      ],
    });
  }

  private onDateRangeChanged(value: DateRangeValue | undefined): void {
    if (value && value[0] && value[1]) {
      this.dateFrom.set(value[0]);
      this.dateTo.set(value[1]);
    }
  }

  private onFilterClick(): void {
    if (this.grid) {
      this.refreshGrid();
    }
  }

  protected override getDefaultClearFiltersOption() {
    return {
      onAction: () => {
        this.dateFrom.set(this.defaultDateFrom());
        this.dateTo.set(this.defaultDateTo());
        this.toolbarDaterangeInitialDates.set([this.dateFrom(), this.dateTo()]);
        this.grid?.clearFilters();
        this.refreshGrid();
      },
    };
  }

  override onGridReadyToDefine(grid: GridComponent<IEmployeeRequestGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.type',
        'common.name',
        'common.from',
        'common.to',
        'common.created',
        'common.modified',
        'time.schedule.planning.available',
        'time.schedule.planning.unavailable',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('type', terms['common.type'], {
          flex: 15,
          valueGetter: ({ data }) =>
            data?.type === TermGroup_EmployeeRequestType.InterestRequest
              ? terms['time.schedule.planning.available']
              : terms['time.schedule.planning.unavailable'],
        });
        this.grid.addColumnText('employeeName', terms['common.name'], {
          flex: 25,
        });
        this.grid.addColumnDateTime('start', terms['common.from'], {
          flex: 15,
          dateFormat: 'yyyy-MM-dd HH:mm',
        });
        this.grid.addColumnDateTime('stop', terms['common.to'], {
          flex: 15,
          dateFormat: 'yyyy-MM-dd HH:mm',
        });
        this.grid.addColumnDateTime('created', terms['common.created'], {
          flex: 15,
          dateFormat: 'yyyy-MM-dd HH:mm',
        });
        this.grid.addColumnDateTime('modified', terms['common.modified'], {
          flex: 15,
          dateFormat: 'yyyy-MM-dd HH:mm',
        });

        this.grid.options = {
          ...this.grid.options,
          rowSelection: undefined,
        };

        super.finalizeInitGrid();
      });
  }

  override loadData(
    id?: number,
    additionalProps?: any
  ): Observable<IEmployeeRequestGridDTO[]> {
    return this.performLoadData.load$(
      this.service.getGrid(id, { fromDate: this.dateFrom(), toDate: this.dateTo() }),
      { showDialogDelay: 1000 }
    );
  }
}
