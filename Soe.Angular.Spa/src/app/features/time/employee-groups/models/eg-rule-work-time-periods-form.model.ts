import {
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { IEmployeeGroupRuleWorkTimePeriodDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface IEmployeeGroupRuleWorkTimePeriodsForm {
  validationHandler: ValidationHandler;
  element: IEmployeeGroupRuleWorkTimePeriodDTO | undefined;
}
export class EmployeeGroupRuleWorkTimePeriodsForm extends SoeFormGroup {
  constructor({
    validationHandler,
    element,
  }: IEmployeeGroupRuleWorkTimePeriodsForm) {
    super(validationHandler, {
      timePeriodId: new SoeSelectFormControl(element?.timePeriodId || 0, {}),
      ruleWorkTime: new SoeTextFormControl(element?.ruleWorkTime ?? 0, {}),

      // Extensions
      timePeriodHeadId: new SoeSelectFormControl(0, {}),
    });
  }

  customPatchValue(element: IEmployeeGroupRuleWorkTimePeriodDTO) {
    this.patchValue(element);
  }
}
