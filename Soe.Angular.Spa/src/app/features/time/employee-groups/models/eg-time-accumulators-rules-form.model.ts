import {
  SoeCheckboxFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { ITimeAccumulatorEmployeeGroupRuleDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface IEgTimeAccumulatorsRulesForm {
  validationHandler: ValidationHandler;
  element: ITimeAccumulatorEmployeeGroupRuleDTO | undefined;
}
export class EgTimeAccumulatorsRulesForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IEgTimeAccumulatorsRulesForm) {
    super(validationHandler, {
      timeAccumulatorId: new SoeSelectFormControl(
        element?.timeAccumulatorId || 0,
        {}
      ),
      type: new SoeNumberFormControl(element?.type || 0, {}),
      minMinutes: new SoeNumberFormControl(element?.minMinutes || null, {}),
      minTimeCodeId: new SoeNumberFormControl(
        element?.minTimeCodeId || null,
        {}
      ),
      maxMinutes: new SoeNumberFormControl(element?.maxMinutes || null, {}),
      maxTimeCodeId: new SoeNumberFormControl(
        element?.maxTimeCodeId || null,
        {}
      ),
      minMinutesWarning: new SoeNumberFormControl(
        element?.minMinutesWarning || null,
        {}
      ),
      maxMinutesWarning: new SoeNumberFormControl(
        element?.maxMinutesWarning || null,
        {}
      ),
      scheduledJobHeadId: new SoeNumberFormControl(
        element?.scheduledJobHeadId || null,
        {}
      ),
      showOnPayrollSlip: new SoeCheckboxFormControl(
        element?.showOnPayrollSlip || 0,
        {}
      ),
    });
  }

  customPatchValue(element: ITimeAccumulatorEmployeeGroupRuleDTO) {
    this.patchValue(element);
  }
}
