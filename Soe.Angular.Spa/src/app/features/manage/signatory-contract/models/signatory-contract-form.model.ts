import { ValidationHandler } from '@shared/handlers';
import { SignatoryContractDTO } from './signatory-contract-edit-dto.model';
import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { SignatoryContractAuthenticationMethodType } from '@shared/models/generated-interfaces/Enumerations';
import {
  AbstractControl,
  FormArray,
  ValidationErrors,
  ValidatorFn,
} from '@angular/forms';
import { SubSignatoryContractForm } from './sub-signatory-contract-form.model';

interface ISignatoryContractForm {
  validationHandler: ValidationHandler;
  element?: SignatoryContractDTO;
}

export class SignatoryContractForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ISignatoryContractForm) {
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
          zeroNotAllowed: true,
        },
        'manage.registry.signatorycontract.recipientuser'
      ),
      signedByUserName: new SoeTextFormControl(
        element?.signedByUserName ?? '',
        {
          disabled: true,
        }
      ),
      requiredAuthenticationMethodType: new SoeSelectFormControl(
        element?.requiredAuthenticationMethodType ??
          SignatoryContractAuthenticationMethodType.PasswordSMSCode,
        {
          required: true,
        },
        'manage.registry.signatorycontract.authenticationmethod'
      ),
      permissionTypes: new SoeSelectFormControl(
        element?.permissionTypes ?? [],
        {
          required: true,
        },
        'manage.registry.signatorycontract.permissions'
      ),
      subContracts: new FormArray<SubSignatoryContractForm>([]),
      revokedAt: new SoeDateFormControl(element?.revokedAt ?? undefined, {
        disabled: true,
      }),
      revokedBy: new SoeTextFormControl(element?.revokedBy ?? '', {
        disabled: true,
      }),
      revokedReason: new SoeTextFormControl(element?.revokedReason ?? '', {
        disabled: true,
      }),
    });

    this.customSubContractsPatchValues(
      <SignatoryContractDTO[]>element?.subContracts ?? []
    );
  }

  get signatoryContractId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.signatoryContractId;
  }

  get recipientUserId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.recipientUserId;
  }

  get signedByUserName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.signedByUserName;
  }

  get requiredAuthenticationMethodType(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.requiredAuthenticationMethodType;
  }

  get permissionTypes(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.permissionTypes;
  }

  get subContracts(): FormArray<SubSignatoryContractForm> {
    return <FormArray<SubSignatoryContractForm>>this.controls.subContracts;
  }

  get revokedAt(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.revokedAt;
  }

  get revokedBy(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.revokedBy;
  }

  get revokedReason(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.revokedReason;
  }

  public customSubContractsPatchValues(bSubContracts: SignatoryContractDTO[]) {
    this.subContracts.clear({ emitEvent: false });
    if (bSubContracts) {
      for (const child of bSubContracts) {
        const row = new SubSignatoryContractForm({
          validationHandler: this.formValidationHandler,
          element: child,
        });

        this.subContracts.push(row, { emitEvent: false });
      }
    }
    this.subContracts.updateValueAndValidity();
  }
}

export function subContractPermissionValidator(errorTerm: string): ValidatorFn {
  return (_form: AbstractControl): ValidationErrors | null => {

    const subContracts = _form.get('subContracts') as FormArray;
    const permissionTypes = _form.get(
      'permissionTypes'
    ) as SoeSelectFormControl;
    if (!subContracts.length) {
      return null;
    }

    const invalidSubContracts = subContracts.controls.some(subContract => {

      const subContractForm = subContract as SubSignatoryContractForm;

      const invalid = permissionTypes.value.length === 0 ||
        subContractForm.permissionTypes.value.some(
          (scp: number) => permissionTypes.value.indexOf(scp) === -1
        );

      return invalid;
    });

    if (invalidSubContracts) {

      const error: ValidationErrors = {};
      error[errorTerm] = true;

      return error;
    }

    return null;
  };
}
