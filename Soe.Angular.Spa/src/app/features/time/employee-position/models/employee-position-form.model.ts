import {
  SoeCheckboxFormControl,
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { FormArray } from '@angular/forms';
import { arrayToFormArray } from '@shared/util/form-util';
import { EmployeePositionSkillForm } from './employee-position-skill-form.model';
import {
  IPositionDTO,
  IPositionSkillDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface IEmployeePositionForm {
  validationHandler: ValidationHandler;
  element: IPositionDTO | undefined;
}
export class EmployeePositionForm extends SoeFormGroup {
  skillValidationHandler: ValidationHandler;

  constructor({ validationHandler, element }: IEmployeePositionForm) {
    super(validationHandler, {
      positionId: new SoeTextFormControl(element?.positionId || 0, {
        isIdField: true,
      }),
      name: new SoeTextFormControl(
        element?.name || '',
        { isNameField: true, required: true, maxLength: 100, minLength: 1 },
        'common.name'
      ),
      code: new SoeTextFormControl(
        element?.code || '',
        { maxLength: 50 },
        'common.code'
      ),
      description: new SoeTextFormControl(
        element?.description || '',
        { maxLength: 512 },
        'common.description'
      ),
      sysPositionId: new SoeSelectFormControl(
        element?.sysPositionId || undefined
      ),
      isLinked: new SoeCheckboxFormControl(element?.sysPositionId || false),
      positionSkills: arrayToFormArray(element?.positionSkills || []),
    });

    this.skillValidationHandler = validationHandler;
    this.customPositionSkillsPatchValue(element?.positionSkills ?? []);
  }

  get positionSkills(): FormArray<EmployeePositionSkillForm> {
    return <FormArray>this.controls.positionSkills;
  }

  customPositionSkillsPatchValue(rows: IPositionSkillDTO[]) {
    this.patchPositionSkillsRows(rows);
  }

  private patchPositionSkillsRows(rows: IPositionSkillDTO[]) {
    this.positionSkills?.clear({ emitEvent: false });
    if (rows && rows.length > 0) {
      rows.forEach(r => {
        this.positionSkills?.push(
          new EmployeePositionSkillForm({
            validationHandler: this.skillValidationHandler,
            element: r,
          }),
          { emitEvent: false }
        );
      });

      this.positionSkills?.updateValueAndValidity();
    }
  }

  setInitialDisabledState(modifyPermission: boolean) {
    if (this.controls.isLinked.value && modifyPermission) {
      this.controls.sysPositionId.enable({ emitEvent: false });
    }
    if (!this.controls.isLinked.value) {
      this.controls.sysPositionId.disable({ emitEvent: false });
    }
  }

  eventSetDisabledState(event: boolean, modifyPermission: boolean) {
    if (event) {
      if (modifyPermission) {
        this.controls.sysPositionId.enable({ emitEvent: false });
      }
    } else {
      this.controls.sysPositionId.setValue(undefined, { emitEvent: false });
      this.controls.sysPositionId.disable({ emitEvent: false });
    }
  }
}
