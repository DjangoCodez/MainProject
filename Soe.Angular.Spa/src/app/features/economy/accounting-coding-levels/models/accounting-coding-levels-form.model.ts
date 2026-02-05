import { ValidationHandler } from '@shared/handlers';
import { AccountDimDTO } from './accounting-coding-levels.model';
import {
  SoeCheckboxFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { SoeEntityState } from '@shared/models/generated-interfaces/Enumerations';
import { distinctUntilChanged } from 'rxjs';
import { ValidationErrors, ValidatorFn } from '@angular/forms';

interface IAccountDimForm {
  validationHandler: ValidationHandler;
  element?: AccountDimDTO;
}

export class AccountDimForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IAccountDimForm) {
    super(validationHandler, {
      accountDimId: new SoeTextFormControl(element?.accountDimId || 0, {
        isIdField: true,
      }),
      actorCompanyId: new SoeTextFormControl(
        element?.actorCompanyId || SoeConfigUtil.actorCompanyId
      ),
      accountDimNr: new SoeNumberFormControl(
        element?.accountDimNr || undefined,
        { required: true, zeroNotAllowed: true },
        'common.number'
      ),
      shortName: new SoeTextFormControl(
        element?.shortName || '',
        {
          isNameField: true,
          required: true,
          maxLength: 10,
        },
        'common.shortname'
      ),
      name: new SoeTextFormControl(
        element?.name || '',
        { required: true, maxLength: 100 },
        'common.name'
      ),
      sysAccountStdTypeParentId: new SoeSelectFormControl(
        element?.sysAccountStdTypeParentId || undefined
      ),
      minChar: new SoeSelectFormControl(element?.minChar || undefined),
      maxChar: new SoeSelectFormControl(element?.minChar || undefined),
      sysSieDimNr: new SoeNumberFormControl(element?.sysSieDimNr || undefined),
      sysSieDimNrSelect: new SoeSelectFormControl(
        element?.sysSieDimNr || undefined
      ),
      linkedToProject: new SoeCheckboxFormControl(
        element?.linkedToProject || false
      ),
      linkedToShiftType: new SoeCheckboxFormControl(
        element?.linkedToShiftType || false
      ),
      onlyAllowAccountsWithParent: new SoeCheckboxFormControl(
        element?.onlyAllowAccountsWithParent || false
      ),
      useVatDeduction: new SoeCheckboxFormControl(
        element?.useVatDeduction || false
      ),
      mandatoryInOrder: new SoeCheckboxFormControl(
        element?.mandatoryInOrder || false
      ),
      mandatoryInCustomerInvoice: new SoeCheckboxFormControl(
        element?.mandatoryInCustomerInvoice || false
      ),
      useInSchedulePlanning: new SoeCheckboxFormControl(
        element?.useInSchedulePlanning || false
      ),
      excludeinAccountingExport: new SoeCheckboxFormControl(
        element?.excludeinAccountingExport || false
      ),
      excludeinSalaryReport: new SoeCheckboxFormControl(
        element?.excludeinSalaryReport || false
      ),
      parentAccountDimId: new SoeSelectFormControl(
        element?.parentAccountDimId || undefined
      ),
      state: new SoeTextFormControl(element?.state || SoeEntityState.Active),
      isActive: new SoeCheckboxFormControl(element?.isActive || true),
    });

    this.sysSieDimNr.valueChanges
      .pipe(distinctUntilChanged())
      .subscribe((value: number): void => {
        if (this.sysSieDimNrSelect.value !== value)
          this.sysSieDimNrSelect.patchValue(value);
      });
    this.sysSieDimNrSelect.valueChanges
      .pipe(distinctUntilChanged())
      .subscribe((value: number): void => {
        if (value) this.sysSieDimNr.patchValue(value);
      });
  }

  get accountDimId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.accountDimId;
  }

  get actorCompanyId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.actorCompanyId;
  }

  get accountDimNr(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.accountDimNr;
  }

  get shortName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.shortName;
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }

  get sysAccountStdTypeParentId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.sysAccountStdTypeParentId;
  }

  get minChar(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.minChar;
  }

  get maxChar(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.maxChar;
  }

  get sysSieDimNr(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.sysSieDimNr;
  }

  get sysSieDimNrSelect(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.sysSieDimNrSelect;
  }

  get linkedToProject(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.linkedToProject;
  }

  get linkedToShiftType(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.linkedToShiftType;
  }

  get useVatDeduction(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.useVatDeduction;
  }

  get mandatoryInOrder(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.mandatoryInOrder;
  }

  get mandatoryInCustomerInvoice(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.mandatoryInCustomerInvoice;
  }

  get useInSchedulePlanning(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.useInSchedulePlanning;
  }

  get excludeinAccountingExport(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.excludeinAccountingExport;
  }

  get excludeinSalaryReport(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.excludeinSalaryReport;
  }

  get parentAccountDimId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.parentAccountDimId;
  }

  get state(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.state;
  }

  get isActive(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.isActive;
  }

  customPatch(dim: AccountDimDTO): void {
    this.reset(dim);
    this.sysSieDimNrSelect.reset(dim.sysSieDimNr);
  }
}

export function projectShiftSelectionValidator(errorTerm: string): ValidatorFn {
  return (form): ValidationErrors | null => {
    const linkedToProject = form.get('linkedToProject')?.value;
    const linkedToShiftType = form.get('linkedToShiftType')?.value;
    if (linkedToProject === true && linkedToShiftType === true) {
      const error: ValidationErrors = {};
      error[errorTerm] = true;
      return error;
    }
    return null;
  };
}
