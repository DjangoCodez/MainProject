import { ValidationHandler } from '@shared/handlers';
import {
  SoeFormGroup,
  SoeNumberFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import {
  CompanyGroupMappingHeadDTO,
  CompanyGroupMappingRowDTO,
} from './company-group-mappings.model';
import {
  AbstractControl,
  AsyncValidatorFn,
  FormArray,
  ValidationErrors,
} from '@angular/forms';
import { ICompanyGroupMappingRowDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CompanyGroupMappingRowsForm } from './company-group-mappings-rows-form.model';

interface ICompanyGroupMappingHeadForm {
  validationHandler: ValidationHandler;
  element?: CompanyGroupMappingHeadDTO;
}

export class CompanyGroupMappingHeadForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;

  constructor({ validationHandler, element }: ICompanyGroupMappingHeadForm) {
    super(validationHandler, {
      companyGroupMappingHeadId: new SoeTextFormControl(
        element?.companyGroupMappingHeadId || 0,
        {
          isIdField: true,
        }
      ),
      number: new SoeNumberFormControl(
        element?.number || '',
        {
          required: true,
          zeroNotAllowed: true,
          maxValue: 2147483647,
        },
        'common.number'
      ),
      name: new SoeTextFormControl(
        element?.name || '',
        { required: true, isNameField: true },
        'common.name'
      ),
      description: new SoeTextFormControl(element?.description || '', {}),
      rows: new FormArray<CompanyGroupMappingRowsForm>([]),
    });

    this.thisValidationHandler = validationHandler;
    this.patchAccounts(element?.rows ?? []);
  }

  get companyGroupMappingHeadId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.companyGroupMappingHeadId;
  }

  get number(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.number;
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }

  get description(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.description;
  }

  get rows(): FormArray<CompanyGroupMappingRowsForm> {
    return <FormArray>this.controls.rows;
  }

  customPatchValue(element: CompanyGroupMappingHeadDTO) {
    (this.controls.rows as FormArray).clear();

    this.patchValue(element);
    this.patchAccounts(element.rows);

    this.markAsPristine();
    this.markAsUntouched();
  }

  patchAccounts(accounts: ICompanyGroupMappingRowDTO[]) {
    for (const invoiceProductRow of accounts) {
      const accountRows = new CompanyGroupMappingRowsForm({
        validationHandler: this.thisValidationHandler,
        element: invoiceProductRow,
      });
      this.rows.push(accountRows, { emitEvent: false });
    }
    this.rows.updateValueAndValidity();
  }

  onDoCopy(): void {
    const formArray = this.rows;
    if (formArray) {
      const elementArray: CompanyGroupMappingRowDTO[] = [];
      for (const row of formArray.controls) {
        elementArray.push(
          new CompanyGroupMappingRowDTO(
            row.value.companyGroupMappingRowId,
            row.value.companyGroupMappingHeadId,
            row.value.childAccountFrom,
            row.value.childAccountTo,
            row.value.groupCompanyAccount,
            row.value.childAccountToName,
            row.value.childAccountFromName,
            row.value.groupCompanyAccountName
          )
        );
      }
      (this.controls.rows as FormArray).clear();
      this.patchAccounts(elementArray);
    }
  }
}

export function addNumberValidator(errorTerm: string): AsyncValidatorFn {
  return (control: AbstractControl): Promise<ValidationErrors | null> => {
    return new Promise<ValidationErrors | null>(resolve => {
      resolve({ custom: { value: errorTerm } });
    });
  };
}
