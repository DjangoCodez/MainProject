import { Component, OnInit, inject } from '@angular/core';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { tap } from 'rxjs/operators';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { Observable } from 'rxjs';
import { HolidaysService } from '../../services/holidays.service';
import { TimeService } from '../../../services/time.service';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { HolidayForm } from '../../models/holidays-form.model';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { IHolidayDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { DateUtil } from '@shared/util/date-util';
import { ProgressOptions } from '@shared/services/progress';
import { CrudActionTypeEnum } from '@shared/enums';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import { ResponseUtil } from '@shared/util/response-util';

@Component({
  selector: 'soe-holidays-edit',
  templateUrl: './holidays-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class HolidaysEditComponent
  extends EditBaseDirective<IHolidayDTO, HolidaysService, HolidayForm>
  implements OnInit
{
  readonly service = inject(HolidaysService);
  private readonly timeService = inject(TimeService);

  holidayTypes: SmallGenericType[] = [];
  dayTypes: SmallGenericType[] = [];

  private originalDateString!: string;

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(Feature.Time_Preferences_ScheduleSettings_Holidays_Edit, {
      lookups: [this.loadDayTypes(), this.loadHolidayTypes()],
    });

    // Set date to 1900-01-01 if HolidayType
    this.form?.sysHolidayTypeId.valueChanges.subscribe((value: number) => {
      if (value && value !== 0)
        this.form?.date.reset(DateUtil.defaultDateTime());
      else if (!value || value === 0)
        this.form?.date.reset(DateUtil.getToday());
    });
  }

  override onFinished(): void {
    const date = this.form?.value.date;
    if (date) {
      // format as yyyyMMdd
      const d = new Date(date);
      const yyyy = d.getFullYear().toString();
      const mm = String(d.getMonth() + 1).padStart(2, '0');
      const dd = String(d.getDate()).padStart(2, '0');
      this.originalDateString = `${yyyy}${mm}${dd}`;
    } else {
      this.originalDateString = DateUtil.defaultDateTime().toDateString();
    }
  }

  override performSave(
    options?: ProgressOptions,
    skipLoadData?: boolean
  ): void {
    if (!options) {
      options = {};
    }
    options.callback = (val: BackendResponse) => {
      this.messageboxService
        .question(
          'time.schedule.halfday.modal.updateschedule',
          'time.schedule.halfday.modal.updatescheduletext'
        )
        .afterClosed()
        .subscribe(({ result }) => {
          if (result) {
            if (isNew) {
              this.performAction.crud(
                CrudActionTypeEnum.Work,
                this.service.onAddHoliday(
                  ResponseUtil.getEntityId(val),
                  this.form?.value.dayTypeId
                )
              );
            } else {
              this.performAction.crud(
                CrudActionTypeEnum.Work,
                this.service.onUpdateHoliday(
                  ResponseUtil.getEntityId(val),
                  this.form?.value.dayTypeId,
                  this.originalDateString
                )
              );
            }
          }
        });
    };

    const isNew = !this.form?.getIdControl()?.value;
    super.performSave(options, skipLoadData);
  }

  override performDelete(options?: ProgressOptions): void {
    if (!options) {
      options = {};
    }
    options.callback = (val: BackendResponse) => {
      this.messageboxService
        .question(
          'time.schedule.halfday.modal.updateschedule',
          'time.schedule.halfday.modal.updatescheduletext'
        )
        .afterClosed()
        .subscribe(({ result }) => {
          if (result) {
            this.performAction.crud(
              CrudActionTypeEnum.Work,
              this.service.onDeleteHoliday(
                this.form?.getIdControl()?.value,
                this.form?.value.dayTypeId
              )
            );
          }
          this.emitActionDeleted(val);
        });
    };
    super.performDelete(options);
  }

  //LOAD DATA

  loadDayTypes(): Observable<SmallGenericType[]> {
    return this.performLoadData.load$(
      this.timeService
        .getDayTypes(false)
        .pipe(tap(dtypes => (this.dayTypes = dtypes)))
    );
  }

  loadHolidayTypes() {
    return this.timeService.getHolidayTypesDict().pipe(
      tap(x => {
        this.holidayTypes = x;

        if (this.form?.value[this.idFieldName] >= 0) {
          const empty = new SmallGenericType(0, '');
          this.holidayTypes.splice(0, 0, empty);
        }
      })
    );
  }
}
