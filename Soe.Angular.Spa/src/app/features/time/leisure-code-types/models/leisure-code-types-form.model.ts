import {
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { ITimeLeisureCodeDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface ILeisureCodeTypesForm {
  validationHandler: ValidationHandler;
  element: ITimeLeisureCodeDTO | undefined;
}
export class LeisureCodeTypesForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ILeisureCodeTypesForm) {
    super(validationHandler, {
      timeLeisureCodeId: new SoeNumberFormControl(
        element?.timeLeisureCodeId || 0,
        {
          isIdField: true,
        }
      ),
      type: new SoeSelectFormControl(
        element?.type || '',
        { required: true, zeroNotAllowed: false },
        'common.type'
      ),
      code: new SoeTextFormControl(
        element?.code || '',
        { required: true, maxLength: 10, minLength: 1 },
        'common.code'
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
    });
  }

  get timeLeisureCodeId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.timeLeisureCodeId;
  }

  get type(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.type;
  }

  get code(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.code;
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }

  get description(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.description;
  }
}
