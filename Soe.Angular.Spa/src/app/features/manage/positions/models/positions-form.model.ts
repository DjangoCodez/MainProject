import {
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { ISysPositionDTO } from '@shared/models/generated-interfaces/SOESysModelDTOs';

interface IPositionsForm {
  validationHandler: ValidationHandler;
  element: ISysPositionDTO | undefined;
}
export class PositionsForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IPositionsForm) {
    super(validationHandler, {
      sysPositionId: new SoeTextFormControl(element?.sysPositionId || 0, {
        isIdField: true,
      }),
      code: new SoeTextFormControl(
        element?.code || '',
        { required: true, maxLength: 50, minLength: 1 },
        'manage.registry.sysposition.ssyk'
      ),
      name: new SoeTextFormControl(
        element?.name || '',
        { isNameField: true, required: true, maxLength: 100 },
        'common.name'
      ),
      description: new SoeTextFormControl(
        element?.description || '',
        { maxLength: 512 },
        'common.description'
      ),
      sysCountryId: new SoeSelectFormControl(
        element?.sysCountryId || undefined,
        { required: true },
        'manage.registry.sysposition.country'
      ),
      sysLanguageId: new SoeSelectFormControl(
        element?.sysLanguageId || undefined,
        { required: true },
        'manage.registry.sysposition.language'
      ),
    });
  }
}
