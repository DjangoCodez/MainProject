import { Component, inject, OnInit } from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import { TermCollection } from '@shared/localization/term-types';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { ITimeHalfdayEditDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressOptions } from '@shared/services/progress';
import { clearAndSetFormArray } from '@shared/util/form-util';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { BehaviorSubject, Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import {
  createDayTypeValidator,
  HalfdayForm,
} from '../../models/halfday-form.model';
import { HalfdaysService } from '../../services/halfdays.service';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import { ResponseUtil } from '@shared/util/response-util';

@Component({
  selector: 'soe-halfdays-edit',
  templateUrl: './halfdays-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class HalfdaysEditComponent
  extends EditBaseDirective<ITimeHalfdayEditDTO, HalfdaysService, HalfdayForm>
  implements OnInit
{
  service = inject(HalfdaysService);
  messageboxService = inject(MessageboxService);

  halfDayTypes: SmallGenericType[] = [];
  breaks = new BehaviorSubject<SmallGenericType[] | undefined>([]);
  companyDayTypes: SmallGenericType[] = [];

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(Feature.Time_Preferences_ScheduleSettings_Halfdays_Edit, {
      lookups: [
        this.loadHalfdayTypes(),
        this.loadCompanyDayTypes(),
        this.loadBreaks(),
      ],
    });
  }

  override loadData(): Observable<void> {
    return this.performLoadData.load$(
      this.service.get(this.form?.getIdControl()?.value).pipe(
        tap((value: ITimeHalfdayEditDTO) => {
          this.form?.customPatchValue(value);
        })
      )
    );
  }

  override loadTerms(): Observable<TermCollection> {
    return super
      .loadTerms(['time.schedule.halfday.duplicatenameordaytype'])
      .pipe(
        tap(() => {
          this.addFormValidators();
        })
      );
  }

  private addFormValidators() {
    const gridData = this.getFilteredGridData();
    this.form?.addValidators([
      createDayTypeValidator(
        this.terms['time.schedule.halfday.duplicatenameordaytype'],
        gridData
      ),
    ]);
  }

  private getFilteredGridData() {
    const dayTypes = this.companyDayTypes;
    return this.service.performHalfdayGrid.data
      ?.filter(
        (row: any) => row.timeHalfdayId !== this.form?.value.timeHalfdayId
      )
      .map((row: any) => {
        const match = dayTypes.find(
          dayType => dayType.name === row.dayTypeName
        );
        return {
          dayTypeName: row.dayTypeName,
          name: row.name,
          dayTypeId: match?.id,
        };
      });
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
                this.service.onAddHalfDay(ResponseUtil.getEntityId(val))
              );
            } else {
              this.performAction.crud(
                CrudActionTypeEnum.Work,
                this.service.onUpdateHalfDay(
                  ResponseUtil.getEntityId(val),
                  this.form?.value.dayTypeId
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
              this.service.onDeleteHalfDay(this.form?.getIdControl()?.value)
            );
          }
          this.emitActionDeleted(val);
        });
    };
    super.performDelete(options);
  }

  loadHalfdayTypes(): Observable<void> {
    return this.performLoadData.load$(
      this.service
        .getHalfdayTypesDict(false)
        .pipe(tap(x => (this.halfDayTypes = x)))
    );
  }

  loadCompanyDayTypes(): Observable<void> {
    return this.performLoadData.load$(
      this.service.getDayTypesByCompanyDict(false).pipe(
        tap(x => {
          this.companyDayTypes = x;
          this.addFormValidators();
        })
      )
    );
  }

  loadBreaks(): Observable<void> {
    return this.performLoadData.load$(
      this.service.getTimeCodeBreaks(false).pipe(tap(x => this.breaks.next(x)))
    );
  }

  breaksChanged(selectedBreakIds: number[]) {
    clearAndSetFormArray(selectedBreakIds, this.form!.timeCodeBreakIds, true);
  }
}
