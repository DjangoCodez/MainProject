import { ValidationHandler } from '@shared/handlers';
import { IShiftTypeHierarchyAccountDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeFormGroup, SoeNumberFormControl } from '@shared/extensions';
import { ValidationErrors, ValidatorFn } from '@angular/forms';

interface IHierarchyAccountsForm {
  validationHandler: ValidationHandler;
  element: IShiftTypeHierarchyAccountDTO | undefined;
}

export class HierarchyAccountsForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IHierarchyAccountsForm) {
    super(validationHandler, {
      shiftTypeHierarchyAccountId: new SoeNumberFormControl(
        element?.shiftTypeHierarchyAccountId || 0,
        {}
      ),
      accountId: new SoeNumberFormControl(element?.accountId || 0, {}),
      accountPermissionType: new SoeNumberFormControl(
        element?.accountPermissionType || 0,
        {}
      ),
      accountDimId: new SoeNumberFormControl(0),
    });
  }

  get shiftTypeHierarchyAccountId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.shiftTypeHierarchyAccountId;
  }

  get accountId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.accountId;
  }

  get accountPermissionType(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.accountPermissionType;
  }

  get accountDimId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.accountDimId;
  }

  validateRow(): string[] {
    const warnings: string[] = [];

    if (this.accountId.value === 0) {
      warnings.push('time.schedule.shifttype.missinghierarchyaccount');
    }

    return warnings;
  }
}

export function createAccountIdValidator(errormessage: string): ValidatorFn {
  return (form): ValidationErrors | null => {
    let noAccount = false;
    const hierarchyAccounts: IShiftTypeHierarchyAccountDTO[] =
      form.get('hierarchyAccounts')?.value || [];
    hierarchyAccounts.forEach(account => {
      if (account.accountId === 0) {
        noAccount = true;
      }
    });
    return noAccount ? { [errormessage]: true } : null;
  };
}
