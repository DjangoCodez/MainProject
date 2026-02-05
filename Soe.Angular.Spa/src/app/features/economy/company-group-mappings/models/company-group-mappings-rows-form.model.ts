import { ValidationHandler } from '@shared/handlers';
import {
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ICompanyGroupMappingRowDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface ICompanyGroupMappingRowsForm {
  validationHandler: ValidationHandler;
  element?: ICompanyGroupMappingRowDTO;
}

export class CompanyGroupMappingRowsForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;

  constructor({ validationHandler, element }: ICompanyGroupMappingRowsForm) {
    super(validationHandler, {
      companyGroupMappingRowId: new SoeTextFormControl(
        element?.companyGroupMappingRowId || 0,
        {
          isIdField: true,
        }
      ),
      childAccountFrom: new SoeSelectFormControl(
        element?.childAccountFrom || 0
      ),
      childAccountTo: new SoeSelectFormControl(element?.childAccountTo || 0),
      groupCompanyAccount: new SoeSelectFormControl(
        element?.groupCompanyAccount || 0,
        {}
      ),
    });

    this.thisValidationHandler = validationHandler;
  }

  get companyGroupMappingRowId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.companyGroupMappingRowId;
  }

  get childAccountFrom(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.childAccountFrom;
  }

  get childAccountTo(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.childAccountTo;
  }

  get groupCompanyAccount(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.groupCompanyAccount;
  }
}
