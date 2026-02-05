import { Component, inject, OnInit, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Observable } from 'rxjs';
import { map, tap, switchMap } from 'rxjs/operators';
import { of } from 'rxjs';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { ITimeBreakTemplateDTONew } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { TimeCodeBreakGroupService } from '../../../time-code-break-group/services/time-code-break-group.service';
import { TimeBreakTemplatesForm } from '../../models/time-break-templates-form.model';
import { TimeBreakTemplatesService } from '../../services/time-break-templates.service';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import { NavigatorRecordConfig } from '@ui/record-navigator/record-navigator.component';
import { clearAndSetFormArray } from '@shared/util/form-util';
import { DateUtil } from '@shared/util/date-util';
import { IActionResult } from '@shared/models/generated-interfaces/ActionResult';
import { CrudActionTypeEnum } from '@shared/enums/action.enum';

@Component({
  selector: 'soe-time-break-templates-edit',
  standalone: false,
  templateUrl: './time-break-templates-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
})
export class TimeBreakTemplatesEditComponent
  extends EditBaseDirective<
    ITimeBreakTemplateDTONew,
    TimeBreakTemplatesService,
    TimeBreakTemplatesForm
  >
  implements OnInit
{
  service = inject(TimeBreakTemplatesService);
  timeCodeBreakGroupService = inject(TimeCodeBreakGroupService);
  private destroyRef = inject(DestroyRef);

  breakGroups: SmallGenericType[] = [];

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(Feature.Time_Schedule_TimeBreakTemplate, {
      lookups: [this.loadBreakGroups()],
    });

  this.service.gridData$
      .pipe(
        tap(data => {
          const id = this.form?.getIdControl()?.value;
          if (data && this.form && id) {
            const records = data.map((item, index) => {
              const rowNr = index + 1;
              return new SmallGenericType(
                item.timeBreakTemplateId,
                rowNr.toString()
              );
            });
            this.form.records = records;

            const currentRecord = records.find(r => r.id === id);
            if (currentRecord) {
              this.form.patchValue({ name: currentRecord.name });
            }

            const hiddenConfig = new NavigatorRecordConfig();
            Object.assign(hiddenConfig, this.recordConfig);
            hiddenConfig.hideRecordNavigator = true;
            this.recordConfig = hiddenConfig;

            setTimeout(() => {
              const showConfig = new NavigatorRecordConfig();
              Object.assign(showConfig, this.recordConfig);
              showConfig.hideRecordNavigator = false;
              this.recordConfig = showConfig;
            });
          }
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe();
  }

  override newRecord(): Observable<void> {
    const defaultStartTime = DateUtil.defaultDateTime();
    defaultStartTime.setHours(8, 0, 0, 0);

    this.form?.patchValue({
      shiftLength: '08:00',
      shiftStartFromTime: defaultStartTime,
      majorTimeCodeBreakGroupId: 0,
      minorTimeCodeBreakGroupId: 0,
      majorNbrOfBreaks: 1,
    });

    setTimeout(() => {
      this.form?.markAsDirty();
    });

    return of(undefined);
  }

  override loadData(): Observable<void> {
    return this.performLoadData.load$(
      this.service.get(this.form?.getIdControl()?.value).pipe(
        switchMap((value: ITimeBreakTemplateDTONew) => {
          if (this.form) {
            const {
              shiftTypeIds,
              dayTypeIds,
              dayOfWeeks,
              shiftStartFromTime,
              shiftLength,
              ...rest
            } = value;

            let shiftLengthStr = shiftLength;
            if (typeof shiftLength === 'number') {
              shiftLengthStr = DateUtil.minutesToTimeSpan(
                shiftLength,
                false,
                false,
                true
              ) as any;
            }

            let shiftStartFromTimeDate: Date | number = shiftStartFromTime;
            if (typeof shiftStartFromTime === 'number') {
              const hours = Math.floor(shiftStartFromTime / 60);
              const minutes = shiftStartFromTime % 60;
              shiftStartFromTimeDate = DateUtil.defaultDateTime();
              shiftStartFromTimeDate.setHours(hours, minutes, 0, 0);
            }

            this.form.reset({
              ...rest,
              shiftLength: shiftLengthStr,
              shiftStartFromTime: shiftStartFromTimeDate,
            });

            const shiftTypes = shiftTypeIds?.map(id => ({ id })) || [];
            clearAndSetFormArray(shiftTypes, this.form.shiftTypeIds);

            const dayTypes = dayTypeIds?.map(id => ({ id })) || [];
            clearAndSetFormArray(dayTypes, this.form.dayTypeIds);

            const weekDays = dayOfWeeks?.map(id => ({ id })) || [];
            clearAndSetFormArray(weekDays, this.form.dayOfWeeks);
          }

          return this.service.getGrid().pipe(
            tap(data => {
              if (data && this.form) {
                const records = data.map((item, index) => {
                  const rowNr = index + 1;
                  return new SmallGenericType(
                    item.timeBreakTemplateId,
                    rowNr.toString()
                  );
                });
                this.form.records = records;

                const currentRecord = records.find(
                  r => r.id === value.timeBreakTemplateId
                );
                if (currentRecord) {
                  this.form.patchValue({ name: currentRecord.name });
                }
              }
            }),
            map(() => void 0)
          );
        })
      ),
      { showDialogDelay: 1000 }
    );
  }

  override onSaveCompleted(backendResponse: BackendResponse): void {
    super.onSaveCompleted(backendResponse);
    this.service.triggerRefresh();
  }

  override emitActionDeleted(response: BackendResponse): void {
    super.emitActionDeleted(response);
    this.service.triggerRefresh();
  }

  override performSave(options?: any, skipLoadData = false): void {
    if (!this.form || this.form.invalid || !this.service) return;

    const formValue = this.form.getRawValue();

    let shiftLengthMinutes = formValue.shiftLength;
    if (typeof formValue.shiftLength === 'string') {
      shiftLengthMinutes = DateUtil.timeSpanToMinutes(formValue.shiftLength);
    }

    let shiftStartFromTimeMinutes = null;
    if (formValue.shiftStartFromTime) {
      if (formValue.shiftStartFromTime instanceof Date) {
        const date = formValue.shiftStartFromTime as Date;
        shiftStartFromTimeMinutes = date.getHours() * 60 + date.getMinutes();
      } else if (typeof formValue.shiftStartFromTime === 'string') {
        shiftStartFromTimeMinutes = DateUtil.timeSpanToMinutes(
          formValue.shiftStartFromTime
        );
      }
    }

    const transformedValue: ITimeBreakTemplateDTONew = {
      ...formValue,
      shiftLength: shiftLengthMinutes,
      shiftStartFromTime: shiftStartFromTimeMinutes as any,
      shiftTypeIds:
        formValue.shiftTypeIds
          ?.map((item: any) => item.id)
          .filter((id: number) => id !== 0) || [],
      dayTypeIds:
        formValue.dayTypeIds
          ?.map((item: any) => item.id)
          .filter((id: number) => id !== 0) || [],
      dayOfWeeks:
        formValue.dayOfWeeks
          ?.map((item: any) => item.id)
          .filter((id: number) => id !== -1) || [],
    };

    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service.validate(transformedValue).pipe(
        switchMap((validationResult: IActionResult) => {
          if (!validationResult.success) {
            return of({
              success: false,
              errorMessage:
                validationResult.errorMessage || 'Validation failed',
            } as BackendResponse);
          }
          return this.service.save(transformedValue);
        }),
        tap((res: BackendResponse) => {
          if (!res.success) return;
          setTimeout(() => {
            this.updateFormValueAndEmitChange(res, skipLoadData);
            this.triggerCloseDialog(res);
          });
        })
      ),
      options?.callback,
      options?.errorCallback,
      options
    );
  }

  private loadBreakGroups(): Observable<SmallGenericType[]> {
    return this.timeCodeBreakGroupService.getGrid().pipe(
      map(groups =>
        groups.map(g => new SmallGenericType(g.timeCodeBreakGroupId, g.name))
      ),
      tap(groups => {
        this.breakGroups = [
          new SmallGenericType(0, this.translate.instant('core.notselected')),
          ...groups,
        ];
      })
    );
  }
}
