import {
  SoeFormGroup,
  SoeNumberFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { IAnnualLeaveGroupDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface IAnnualLeaveGroupsForm {
  validationHandler: ValidationHandler;
  element: IAnnualLeaveGroupDTO | undefined;
}
export class AnnualLeaveGroupsForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IAnnualLeaveGroupsForm) {
    super(validationHandler, {
      annualLeaveGroupId: new SoeTextFormControl(
        element?.annualLeaveGroupId || 0,
        {
          isIdField: true,
        }
      ),
      name: new SoeTextFormControl(
        element?.name || '',
        { isNameField: true, required: true, maxLength: 80, minLength: 1 },
        'common.name'
      ),
      description: new SoeTextFormControl(
        element?.description || '',
        { maxLength: 50 },
        'common.description'
      ),
      type: new SoeNumberFormControl(
        element?.type || 0,
        {
          required: true,
          zeroNotAllowed: true,
        },
        'common.type'
      ),
      qualifyingDays: new SoeNumberFormControl(
        element?.qualifyingDays || 0,
        {
          required: true,
        },
        'time.employee.annualleavegroups.qualifyingdays'
      ),
      qualifyingMonths: new SoeNumberFormControl(
        element?.qualifyingMonths || 0,
        {
          required: true,
        },
        'time.employee.annualleavegroups.qualifyingdays'
      ),
      gapDays: new SoeNumberFormControl(
        element?.gapDays || 0,
        {
          required: true,
        },
        'time.employee.annualleavegroups.gapdays'
      ),
      ruleRestTimeMinimum: new SoeNumberFormControl(
        element?.ruleRestTimeMinimum || 1440,
        {
          required: true,
          disabled: true,
        },
        'time.employee.annualleavegroups.ruleresttimeminimum'
      ),
      timeDeviationCauseId: new SoeTextFormControl(
        element?.timeDeviationCauseId || ''
      ),
    });
  }

  get annualLeaveGroupId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.annualLeaveGroupId;
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }

  get description(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.description;
  }

  get type(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.type;
  }

  get qualifyingDays(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.qualifyingDays;
  }

  get qualifyingMonths(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.qualifyingMonths;
  }

  get gapDays(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.gapDays;
  }

  get ruleRestTimeMinimum(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.ruleRestTimeMinimum;
  }

  get timeDeviationCauseId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.timeDeviationCauseId;
  }
}
