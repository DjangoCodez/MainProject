import {
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { IProjectUserDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface IProjectPersonForm {
  validationHandler: ValidationHandler;
  element: IProjectUserDTO | undefined;
}

export class ProjectPersonForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IProjectPersonForm) {
    super(validationHandler, {
      projectUserId: new SoeNumberFormControl(element?.projectUserId || 0),
      type: new SoeSelectFormControl(element?.type || 0, { required: true }),
      userId: new SoeSelectFormControl(element?.userId || 0, {
        required: true,
        zeroNotAllowed: true,
      }),
      timeCodeId: new SoeSelectFormControl(element?.timeCodeId || 0),
      dateFrom: new SoeDateFormControl(element?.dateFrom || undefined),
      dateTo: new SoeDateFormControl(element?.dateTo || undefined),
      employeeCalculatedCost: new SoeTextFormControl(
        element?.employeeCalculatedCost || '',
        { disabled: true }
      ),
    });
  }

  get projectUserId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.projectUserId;
  }

  get type(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.type;
  }

  get userId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.userId;
  }

  get timeCodeId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.timeCodeId;
  }

  get dateFrom(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.dateFrom;
  }

  get dateTo(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.dateTo;
  }

  get employeeCalculatedCost(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.employeeCalculatedCost;
  }
}
