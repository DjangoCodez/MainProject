import { SoeFormGroup, SoeSelectFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { IEmployeeGroupAttestTransitionDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface IEgAttestTransitionsForm {
  validationHandler: ValidationHandler;
  element: IEmployeeGroupAttestTransitionDTO | undefined;
}
export class EgAttestTransitionsForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IEgAttestTransitionsForm) {
    super(validationHandler, {
      attestTransitionId: new SoeSelectFormControl(
        element?.attestTransitionId || 0,
        {}
      ),
      entity: new SoeSelectFormControl(element?.entity || 0, {}),
    });
  }

  customPatchValue(element: IEmployeeGroupAttestTransitionDTO) {
    this.patchValue(element);
  }
}
