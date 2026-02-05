import {
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { IStaffingNeedsLocationGroupGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface IStaffingNeedsLocationGroupsForm {
  validationHandler: ValidationHandler;
  element: IStaffingNeedsLocationGroupGridDTO | undefined;
}
export class StaffingNeedsLocationGroupsForm extends SoeFormGroup {
  constructor({
    validationHandler,
    element,
  }: IStaffingNeedsLocationGroupsForm) {
    super(validationHandler, {
      staffingNeedsLocationGroupId: new SoeTextFormControl(
        element?.staffingNeedsLocationGroupId || 0,
        { isIdField: true }
      ),
      name: new SoeTextFormControl(
        element?.name || '',
        { isNameField: true, required: true, maxLength: 80, minLength: 1 },
        'common.name'
      ),
      description: new SoeTextFormControl(element?.description || '', {
        maxLength: 50,
      }),
      accountId: new SoeSelectFormControl(element?.accountId || undefined),
      timeScheduleTaskId: new SoeSelectFormControl(
        element?.timeScheduleTaskId,
        { required: true },
        'time.schedule.staffingneedslocationgroup.timescheduletask'
      ),
    });
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }

  get description(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.description;
  }

  get accountId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.accountId;
  }

  get timeScheduleTaskId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.timeScheduleTaskId;
  }
}
