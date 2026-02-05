import {
  SoeFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { IGenericType } from '@shared/models/generated-interfaces/GenericType';
import {
  IAccountDTO,
  IAccountMappingDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface IAccountMappingForm {
  validationHandler: ValidationHandler;
  element: IAccountMappingDTO;
}

export class AccountMappingForm extends SoeFormGroup {
  translateValidationHandler: ValidationHandler;
  accounts: IAccountDTO[];
  mandatoryLevels: IGenericType[];
  accountDimName: string;

  constructor({ validationHandler, element }: IAccountMappingForm) {
    super(validationHandler, {
      accountId: new SoeNumberFormControl(element?.accountId || 0, {
        isIdField: true,
      }),
      accountDimId: new SoeNumberFormControl(element?.accountDimId || 0),
      defaultAccountId: new SoeSelectFormControl(
        element?.defaultAccountId || 0
      ),
      mandatoryLevel: new SoeSelectFormControl(element?.mandatoryLevel || 0),
      accountDimName: new SoeTextFormControl(element?.accountDimName || '', {
        isNameField: true,
      }),
      accounts: new SoeFormControl(element?.accounts) || [],
      mandatoryLevels: new SoeFormControl(element?.mandatoryLevels) || [],
    });
    this.translateValidationHandler = validationHandler;
    this.accounts = element?.accounts || [];
    this.mandatoryLevels = element?.mandatoryLevels || [];
    this.accountDimName = element?.accountDimName || '';
  }

  get accountId(): SoeNumberFormControl {
    return this.controls.accountId as SoeNumberFormControl;
  }

  get accountDimId(): SoeNumberFormControl {
    return this.controls.accountDimId as SoeNumberFormControl;
  }

  get defaultAccountId(): SoeSelectFormControl {
    return this.controls.defaultAccountId as SoeSelectFormControl;
  }

  get mandatoryLevel(): SoeSelectFormControl {
    return this.controls.mandatoryLevel as SoeSelectFormControl;
  }
}
