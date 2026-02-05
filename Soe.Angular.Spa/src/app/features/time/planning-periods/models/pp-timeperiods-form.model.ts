import {
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { ITimePeriodDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface ITimePeriodForm {
  validationHandler: ValidationHandler;
  element: ITimePeriodDTO | undefined;
}
export class TimePeriodForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler | undefined;
  constructor({ validationHandler, element }: ITimePeriodForm) {
    super(validationHandler, {
      timePeriodId: new SoeNumberFormControl(element?.timePeriodId || 0, {
        isIdField: true,
      }),
      name: new SoeTextFormControl(
        element?.name || '',
        { required: true },
        'common.name'
      ),
      startDate: new SoeDateFormControl(
        element?.startDate,
        { required: true },
        'common.from'
      ),
      stopDate: new SoeDateFormControl(
        element?.stopDate,
        { required: true },
        'common.to'
      ),
      rowNr: new SoeNumberFormControl(element?.rowNr, { required: false }, ''),
    });

    this.thisValidationHandler = validationHandler;
  }
  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }
  get startDate(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.startDate;
  }
  get stopDate(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.stopDate;
  }
  get rowNr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.rowNr;
  }
}
