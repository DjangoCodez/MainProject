import {
  SoeFormGroup,
  SoeTextFormControl,
  SoeSelectFormControl,
  SoeTimeRangeFormControl,
} from '@shared/extensions/soe-formgroup.extension';
import { ValidationHandler } from '@shared/handlers/validation.handler';
import { IScheduleCycleRuleTypeDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface IScheduleCycleRuleTypeForm {
  validationHandler: ValidationHandler;
  element: IScheduleCycleRuleTypeDTO | undefined;
}

export class ScheduleCycleRuleTypeForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IScheduleCycleRuleTypeForm) {
    const defaultTime = '00:00';
    super(validationHandler, {
      scheduleCycleRuleTypeId: new SoeTextFormControl(
        element?.scheduleCycleRuleTypeId || 0,
        { isIdField: true }
      ),
      name: new SoeTextFormControl(
        element?.name || '',
        {
          isNameField: true,
          required: true,
          maxLength: 100,
          minLength: 1,
        },
        'common.name'
      ),
      accountId: new SoeSelectFormControl(
        element?.accountId || undefined,
        {},
        'common.user.attestrole.accounthierarchy'
      ),
      dayOfWeekIds: new SoeSelectFormControl(
        element?.dayOfWeekIds || [],
        {},
        'time.schedule.schedulecycleruletype.weekday'
      ),
      timeRange: new SoeTimeRangeFormControl(
        [element?.startTime ?? defaultTime, element?.stopTime ?? defaultTime],
        { validateRange: true },
        'time.schedule.schedulecycleruletype.timerange'
      ),
    });
  }

  get scheduleCycleRuleTypeId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.scheduleCycleRuleTypeId;
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }

  get accountId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.accountId;
  }

  get dayOfWeekIds(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.dayOfWeekIds;
  }

  get timeRange(): SoeTimeRangeFormControl {
    return <SoeTimeRangeFormControl>this.controls.timeRange;
  }

  customPatchValue(value: IScheduleCycleRuleTypeDTO): void {
    this.patchValue({
      scheduleCycleRuleTypeId: value.scheduleCycleRuleTypeId,
      name: value.name,
      accountId: value.accountId,
      dayOfWeekIds: value.dayOfWeekIds,
      timeRange: [value.startTime, value.stopTime],
    });
  }

  getRawValueForDTO(): Partial<IScheduleCycleRuleTypeDTO> {
    const raw = this.getRawValue();
    const timeRangeValue = this.timeRange.value as [any, any] | undefined;

    return {
      ...raw,
      startTime: timeRangeValue?.[0],
      stopTime: timeRangeValue?.[1],
    };
  }
}
