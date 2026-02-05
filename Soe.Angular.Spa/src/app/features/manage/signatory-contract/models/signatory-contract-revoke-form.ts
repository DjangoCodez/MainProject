import { ValidationHandler } from '@shared/handlers';
import { SignatoryContractRevokeDTO } from './signatory-contract-revoke-dto';
import {
  SoeFormGroup,
  SoeNumberFormControl,
  SoeTextFormControl
} from '@shared/extensions';

interface ISignatoryContractRevokeForm {
  validationHandler: ValidationHandler;
  element?: SignatoryContractRevokeDTO;
}

export class SignatoryContractRevokeForm extends SoeFormGroup  {

  constructor({ validationHandler, element }: ISignatoryContractRevokeForm) {
    super(validationHandler, {
      signatoryContractId: new SoeNumberFormControl(
        element?.signatoryContractId ?? 0,
        {
          isIdField: true,
        }
      ),
      revokedReason: new SoeTextFormControl(
        element?.revokedReason ?? '',
        {
          required: true,
        },
        'manage.registry.signatorycontract.revokereason'
      ),
    });
  }

    get signatoryContractId(): SoeNumberFormControl {
        return <SoeNumberFormControl>this.controls.signatoryContractId;
    }

    get revokedReason(): SoeTextFormControl {
        return <SoeTextFormControl>this.controls.revokedReason;
    }
}
