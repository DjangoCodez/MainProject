import {
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { IStaffingNeedsRuleRowDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface IStaffingNeedsRulesRowForm {
  validationHandler: ValidationHandler;
  element: IStaffingNeedsRuleRowDTO | undefined;
}
export class StaffingNeedsRulesRowForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IStaffingNeedsRulesRowForm) {
    super(validationHandler, {
      staffingNeedsRuleRowId: new SoeNumberFormControl(
        element?.staffingNeedsRuleRowId || 0,
        {
          isIdField: true,
        }
      ),
      //   staffingNeedsRuleId: new SoeNumberFormControl(
      //     element?.staffingNeedsRuleId || 0
      //   ),
      sort: new SoeNumberFormControl(element?.sort || 0),
      dayId: new SoeSelectFormControl(element?.dayId || 0),
      dayTypeId: new SoeSelectFormControl(element?.dayTypeId || null),
      weekday: new SoeNumberFormControl(element?.weekday || 0),
      value: new SoeNumberFormControl(element?.value || 0),
      dayName: new SoeTextFormControl(element?.dayName || 0),
    });

    this.onCopy = this.onDoCopy.bind(this);
  }

  onDoCopy() {
    // CLEAR relation ids
  }

  get sort(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.sort;
  }

  get dayId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.dayId;
  }

  get dayTypeId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.dayTypeId;
  }

  get weekday(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.weekday;
  }

  //   get value(): SoeNumberFormControl {
  //     return <SoeNumberFormControl>this.controls.value;
  //   }

  get dayName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.dayName;
  }
}
