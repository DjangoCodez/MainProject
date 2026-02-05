import { SoeFormGroup, SoeTextFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { HouseholdTaxDeductionApplicantDTO } from './household-tax-deduction-Applicant.model';

interface IHouseholdTaxDeductionApplicantForm {
  validationHandler: ValidationHandler;
  element: HouseholdTaxDeductionApplicantDTO | undefined;
}

export class HouseholdTaxDeductionApplicantForm extends SoeFormGroup {
  constructor({
    validationHandler,
    element,
  }: IHouseholdTaxDeductionApplicantForm) {
    super(validationHandler, {
      property: new SoeTextFormControl(element?.property || '', {
        maxLength: 50,
      }),
      socialSecNr: new SoeTextFormControl(
        element?.socialSecNr || '',
        {
          required: true,
        },
        'common.customer.customer.rot.socialsecnr'
      ),

      name: new SoeTextFormControl(
        element?.name || '',
        {
          required: true,
        },
        'common.customer.customer.rot.name'
      ),

      apartmentNr: new SoeTextFormControl(element?.apartmentNr || ''),

      cooperativeOrgNr: new SoeTextFormControl(element?.cooperativeOrgNr || ''),

      comment: new SoeTextFormControl(element?.comment || ''),
    });
  }

  get property(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.property;
  }

  get socialSecNr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.socialSecNr;
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }

  get apartmentNr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.apartmentNr;
  }

  get cooperativeOrgNr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.cooperativeOrgNr;
  }

  get comment(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.comment;
  }
}
