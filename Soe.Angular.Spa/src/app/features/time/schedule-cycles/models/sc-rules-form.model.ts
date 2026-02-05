import {
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { IScheduleCycleRuleDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface IScheduleCycleRuleFormParams {
  validationHandler: ValidationHandler;
  element: IScheduleCycleRuleDTO | undefined;
}

export class ScheduleCycleRuleForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IScheduleCycleRuleFormParams) {
    super(validationHandler, {
      scheduleCycleRuleId: new SoeNumberFormControl(
        element?.scheduleCycleRuleId || 0,
        { isIdField: true }
      ),
      scheduleCycleId: new SoeNumberFormControl(
        element?.scheduleCycleId || 0,
        {}
      ),
      scheduleCycleRuleTypeId: new SoeSelectFormControl(
        element?.scheduleCycleRuleTypeId || 0,
        {},
        'time.schedule.schedulecycle.rule'
      ),
      minOccurrences: new SoeNumberFormControl(
        element?.minOccurrences || 0,
        {},
        'time.schedule.schedulecycle.minoccurrences'
      ),
      maxOccurrences: new SoeNumberFormControl(
        element?.maxOccurrences || 0,
        {},
        'time.schedule.schedulecycle.maxoccurrences'
      ),
    });
  }

  get scheduleCycleRuleId() {
    return <SoeNumberFormControl>this.controls.scheduleCycleRuleId;
  }

  get scheduleCycleId() {
    return <SoeNumberFormControl>this.controls.scheduleCycleId;
  }

  get scheduleCycleRuleTypeId() {
    return <SoeSelectFormControl>this.controls.scheduleCycleRuleTypeId;
  }

  get minOccurrences() {
    return <SoeNumberFormControl>this.controls.minOccurrences;
  }

  get maxOccurrences() {
    return <SoeNumberFormControl>this.controls.maxOccurrences;
  }

  customPatchValue(element: IScheduleCycleRuleDTO) {
    this.patchValue({
      scheduleCycleRuleId: element.scheduleCycleRuleId,
      scheduleCycleId: element.scheduleCycleId,
      scheduleCycleRuleTypeId: element.scheduleCycleRuleTypeId,
      minOccurrences: element.minOccurrences,
      maxOccurrences: element.maxOccurrences,
    });
  }
}
