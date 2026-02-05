import { Component, inject, OnInit } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import {
  ITimeScheduleEventDTO,
  ITimeScheduleEventMessageGroupDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { clearAndSetFormArray } from '@shared/util/form-util';
import { MessageGroupService } from '../../services/message-group.service';
import { TimeScheduleEventsService } from '../../services/time-schedule-events.service';
import { TimeScheduleEventForm } from '../../models/time-schedule-event-form.model';
import { ProgressOptions } from '@shared/services/progress/progress-options.class';
import { CrudActionTypeEnum } from '@shared/enums/action.enum';

@Component({
  selector: 'soe-time-schedule-events-edit',
  templateUrl: './time-schedule-events-edit.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class TimeScheduleEventsEditComponent
  extends EditBaseDirective<
    ITimeScheduleEventDTO,
    TimeScheduleEventsService,
    TimeScheduleEventForm
  >
  implements OnInit
{
  service = inject(TimeScheduleEventsService);
  messageGroupService = inject(MessageGroupService);

  recipientGroups = new BehaviorSubject<ISmallGenericType[] | undefined>([]);

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(Feature.Time_Schedule_SchedulePlanning_SalesCalender, {
      lookups: [this.loadRecipientGroups()],
    });
  }

  override loadData(): Observable<void> {
    return this.performLoadData.load$(
      this.service.get(this.form?.getIdControl()?.value).pipe(
        tap((value: ITimeScheduleEventDTO) => {
          this.form?.customPatchValue(value);
        })
      )
    );
  }

  loadRecipientGroups(): Observable<void> {
    return this.performLoadData.load$(
      this.messageGroupService
        .getDict()
        .pipe(tap(groups => this.recipientGroups.next(groups)))
    );
  }

  onRecipientGroupsChanged(selectedIds: number[]): void {
    clearAndSetFormArray(selectedIds, this.form!.messageGroupIds, true);
  }

  override copy(): void {
    if (!this.form) return;

    const element = this.transformFormToDTO();
    element.timeScheduleEventId = 0;

    const newForm = new TimeScheduleEventForm({
      validationHandler: this.form.formValidationHandler,
      element,
    });

    this.copyActionTakenSignal()?.set({
      ref: this.ref(),
      form: newForm,
      filteredRows: this.form.records,
    });
  }

  override performSave(
    options?: ProgressOptions,
    skipLoadData?: boolean
  ): void {
    if (!this.form || this.form.invalid) return;

    const dto = this.transformFormToDTO();

    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service.save(dto).pipe(
        tap(res => {
          this.updateFormValueAndEmitChange(res, skipLoadData);
          if (res.success) this.triggerCloseDialog(res);
        })
      ),
      options?.callback,
      options?.errorCallback,
      options
    );
  }

  private transformFormToDTO(): ITimeScheduleEventDTO {
    const formValue = this.form!.getRawValue();
    return {
      timeScheduleEventId: formValue.timeScheduleEventId,
      actorCompanyId: 0,
      date: formValue.date,
      name: formValue.name,
      description: formValue.description,
      created: undefined,
      createdBy: '',
      modified: undefined,
      modifiedBy: '',
      state: 0,
      timeScheduleEventMessageGroups: (formValue.messageGroupIds || []).map(
        (messageGroupId: number) =>
          ({
            timeScheduleEventMessageGroupId: 0,
            timeScheduleEventId: 0,
            messageGroupId: messageGroupId,
          }) as ITimeScheduleEventMessageGroupDTO
      ),
    };
  }
}
