import { ValidationHandler } from '@shared/handlers';
import { SignatoryContractDTO } from './signatory-contract-edit-dto.model';
import {
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl
} from '@shared/extensions';

interface ISubSignatoryContractForm {
  validationHandler: ValidationHandler;
  element?: SignatoryContractDTO;
}

export class SubSignatoryContractForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ISubSignatoryContractForm) {
    super(validationHandler, {
      signatoryContractId: new SoeNumberFormControl(
        element?.signatoryContractId ?? 0,
        {
          isIdField: true,
        }
      ),
      recipientUserId: new SoeSelectFormControl(
        element?.recipientUserId ?? 0,
        {
          required: true,
          zeroNotAllowed: true
        },
        'manage.registry.signatorycontract.recipientuser'
      ),
      permissionTypes: new SoeSelectFormControl(
        element?.permissionTypes ?? [],
        {
          required: true,
        },
        'manage.registry.signatorycontract.permissions'
      ),
    });
  }

  get signatoryContractId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.signatoryContractId;
  }

  get recipientUserId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.recipientUserId;
  }

  get permissionTypes(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.permissionTypes;
  }
}
