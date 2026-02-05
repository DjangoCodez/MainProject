import {
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { StaffingNeedsLocationDTO } from '../../models/staffing-needs.model';

interface IStaffingNeedsLocationsForm {
  validationHandler: ValidationHandler;
  element: StaffingNeedsLocationDTO | undefined;
}
export class StaffingNeedsLocationsForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IStaffingNeedsLocationsForm) {
    super(validationHandler, {
      staffingNeedsLocationId: new SoeTextFormControl(
        element?.staffingNeedsLocationId || 0,
        { isIdField: true }
      ),
      name: new SoeTextFormControl(
        element?.name || '',
        { isNameField: true, required: true, maxLength: 100, minLength: 1 },
        'common.name'
      ),
      description: new SoeTextFormControl(element?.description || '', {
        maxLength: 50,
      }),
      externalCode: new SoeTextFormControl(element?.externalCode || '', {
        maxLength: 512,
      }),
      staffingNeedsLocationGroupId: new SoeSelectFormControl(
        element?.staffingNeedsLocationGroupId,
        { required: true },
        'common.group'
      ),
    });
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }

  get description(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.description;
  }

  get externalCode(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.externalCode;
  }

  get staffingNeedsLocationGroupId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.staffingNeedsLocationGroupId;
  }
}
