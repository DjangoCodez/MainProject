import {
  SoeFormGroup,
  SoeNumberFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { FormArray } from '@angular/forms';
import { ValidationHandler } from '@shared/handlers';
import { IAccountingSettingsRowDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { AccountingSettingsRowDTO } from './accounting-settings.models';
import { IAccountingSettingDTO } from '@shared/models/generated-interfaces/AccountingSettingDTO';

interface IAccountingSettingsForm {
  validationHandler: ValidationHandler;
  element: IAccountingSettingsRowDTO | undefined;
}
export class AccountingSettingsFormArray extends FormArray<AccountingSettingsForm> {
  constructor(private validationHandler: ValidationHandler) {
    super([]);
  }

  public reset() {
    this.rawPatch([]);
  }

  public rawPatch(rows: IAccountingSettingsRowDTO[] | undefined) {
    this.clear({ emitEvent: false });
    if (rows && rows.length > 0) {
      rows.forEach(r => {
        this.push(
          new AccountingSettingsForm({
            validationHandler: this.validationHandler,
            element: r,
          }),
          { emitEvent: false }
        );
      });
      this.updateValueAndValidity();
    }
  }

  public patch(rows: IAccountingSettingDTO[]) {
    const convertedData = rows.map(
      AccountingSettingsRowDTO.fromAccountingSettings
    );
    this.rawPatch(convertedData);
  }
}

export class AccountingSettingsForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;

  constructor({ validationHandler, element }: IAccountingSettingsForm) {
    super(validationHandler, {
      type: new SoeNumberFormControl(element?.type || 0),
      accountDim1Nr: new SoeNumberFormControl(element?.accountDim1Nr || 0),
      account1Id: new SoeNumberFormControl(element?.account1Id || 0),
      account1Nr: new SoeTextFormControl(element?.account1Nr || ''),
      account1Name: new SoeTextFormControl(element?.account1Name || ''),
      accountDim2Nr: new SoeNumberFormControl(element?.accountDim2Nr || 0),
      account2Id: new SoeNumberFormControl(element?.account2Id || 0),
      account2Nr: new SoeTextFormControl(element?.account2Nr || ''),
      account2Name: new SoeTextFormControl(element?.account2Name || ''),
      accountDim3Nr: new SoeNumberFormControl(element?.accountDim3Nr || 0),
      account3Id: new SoeNumberFormControl(element?.account3Id || 0),
      account3Nr: new SoeTextFormControl(element?.account3Nr || ''),
      account3Name: new SoeTextFormControl(element?.account3Name || ''),
      accountDim4Nr: new SoeNumberFormControl(element?.accountDim4Nr || 0),
      account4Id: new SoeNumberFormControl(element?.account4Id || 0),
      account4Nr: new SoeTextFormControl(element?.account4Nr || ''),
      account4Name: new SoeTextFormControl(element?.account4Name || ''),
      accountDim5Nr: new SoeNumberFormControl(element?.accountDim5Nr || 0),
      account5Id: new SoeNumberFormControl(element?.account5Id || 0),
      account5Nr: new SoeTextFormControl(element?.account5Nr || ''),
      account5Name: new SoeTextFormControl(element?.account5Name || ''),
      accountDim6Nr: new SoeNumberFormControl(element?.accountDim6Nr || 0),
      account6Id: new SoeNumberFormControl(element?.account6Id || 0),
      account6Nr: new SoeTextFormControl(element?.account6Nr || ''),
      account6Name: new SoeTextFormControl(element?.account6Name || ''),
      percent: new SoeNumberFormControl(element?.percent || ''),
    });

    this.thisValidationHandler = validationHandler;
  }
}
