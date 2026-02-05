import { FormGroup } from '@angular/forms';
import {
  SoeFormGroup,
  SoeTextFormControl,
  SoeSelectFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers/validation.handler';
import { IServiceUserDTO } from '@shared/models/generated-interfaces/ServiceUserDTO';

interface IAddServiceUserForm {
  validationHandler: ValidationHandler;
  element?: IServiceUserDTO;
  isEditMode?: boolean;
}

export class AddServiceUserForm extends SoeFormGroup {
  constructor({
    validationHandler,
    element,
    isEditMode = false,
  }: IAddServiceUserForm) {
    super(validationHandler, {
      userId: new SoeTextFormControl(element?.userId || 0, {
        isIdField: true,
      }),
      userName: new SoeTextFormControl(
        element?.userName || '',
        {
          required: true,
          isNameField: true,
        },
        'common.username'
      ),
      roleId: new SoeSelectFormControl(
        element?.roleId || 0,
        {
          required: true,
        },
        'common.role'
      ),
      attestRoleIds: new SoeSelectFormControl(element?.attestRoleIds || [], {
        disabled: isEditMode,
      }),
      connectionCode: new SoeTextFormControl(
        element?.connectionCode || '',
        {
          required: true,
          disabled: isEditMode,
        },
        'manage.serviceuser.connectioncode'
      ),
      serviceProvider: new FormGroup({
        licenseNumber: new SoeTextFormControl(
          element?.serviceProvider?.licenseNumber || '',
          { disabled: true }
        ),
        licenseName: new SoeTextFormControl(
          element?.serviceProvider?.licenseName || '',
          { disabled: true }
        ),
        companyName: new SoeTextFormControl(
          element?.serviceProvider?.companyName || '',
          { disabled: true }
        ),
      }),
    });
  }

  public get userId(): number {
    return this.get('userId')?.value;
  }

  public get code(): string {
    return this.get('code')?.value;
  }

  public get userName(): string {
    return this.get('userName')?.value;
  }

  public get roleId(): number {
    return this.get('roleId')?.value;
  }

  public get attestRoleIds(): number[] {
    return this.get('attestRoleIds')?.value || [];
  }
}
