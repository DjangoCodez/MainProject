import { ValidationHandler } from '@shared/handlers';
import { ExternalCompanySearchFilter } from './external-company-search-dialog-data.model';
import { SoeFormGroup, SoeTextFormControl } from '@shared/extensions';

interface IExternalCompanySearchForm {
  validationHandler: ValidationHandler;
  element?: ExternalCompanySearchFilter;
}

export class ExternalCompanySearchForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IExternalCompanySearchForm) {
    super(validationHandler, {
      registrationNr: new SoeTextFormControl(element?.registrationNr ?? ''),
      name: new SoeTextFormControl(element?.name ?? ''),
    });
  }

  get registrationNr(): SoeTextFormControl {
    return this.get('registrationNr') as SoeTextFormControl;
  }

  get name(): SoeTextFormControl {
    return this.get('name') as SoeTextFormControl;
  }
}
