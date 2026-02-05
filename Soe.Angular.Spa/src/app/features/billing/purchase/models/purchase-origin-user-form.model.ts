import {
  SoeCheckboxFormControl,
  SoeFormGroup,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { OriginUserSmallDTO } from './purchase.model';

interface IOriginUserForm {
  validationHandler: ValidationHandler;
  element: OriginUserSmallDTO | undefined;
}
export class OriginUserForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IOriginUserForm) {
    super(validationHandler, {
      originUserId: new SoeTextFormControl(element?.originUserId || 0),
      userId: new SoeTextFormControl(element?.userId || 0),
      name: new SoeTextFormControl(element?.name || ''),
      main: new SoeCheckboxFormControl(element?.main || false),
      isReady: new SoeCheckboxFormControl(element?.isReady || false),
    });
  }

  get originUserId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.originUserId;
  }
  get userId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.userId;
  }
  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }
  get main(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.main;
  }
  get isReady(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.isReady;
  }
}
