import {
  SoeFormGroup,
  SoeNumberFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { SysMultiCompanyConnectionRequest } from './clients.model';
import { ValidationHandler } from '@shared/handlers/validation.handler';

interface IAddClientForm {
  validationHandler: ValidationHandler;
  element: SysMultiCompanyConnectionRequest;
}

export class AddClientForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IAddClientForm) {
    super(validationHandler, {
      connectionRequestId: new SoeNumberFormControl(
        element?.sysMultiCompanyConnectionRequestId,
        { isIdField: true }
      ),
      code: new SoeTextFormControl(element?.code, {
        disabled: true,
      }),
      expiresAtUTC: new SoeTextFormControl(element?.expiresAtUTC),
    });
  }

  public get connectionRequestId(): number {
    return this.get('connectionRequestId')?.value;
  }

  public get code(): string {
    return this.get('code')?.value;
  }

  public get expiresAtUTC(): string {
    return this.get('expiresAtUTC')?.value;
  }
}
