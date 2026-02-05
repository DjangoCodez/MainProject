import { FormArray } from '@angular/forms';
import {
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import {
  IScheduleCycleDTO,
  IScheduleCycleRuleDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { arrayToFormArray } from '@shared/util/form-util';
import { ScheduleCycleRuleForm } from './sc-rules-form.model';

interface IScheduleCyclesForm {
  validationHandler: ValidationHandler;
  element: IScheduleCycleDTO | undefined;
}

export class ScheduleCyclesForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;

  constructor({ validationHandler, element }: IScheduleCyclesForm) {
    super(validationHandler, {
      scheduleCycleId: new SoeNumberFormControl(element?.scheduleCycleId || 0, {
        isIdField: true,
      }),
      name: new SoeTextFormControl(
        element?.name || '',
        { isNameField: true, required: true, maxLength: 100, minLength: 1 },
        'common.name'
      ),
      description: new SoeTextFormControl(
        element?.description || '',
        { maxLength: 100 },
        'common.description'
      ),
      nbrOfWeeks: new SoeNumberFormControl(
        element?.nbrOfWeeks || 0,
        {},
        'time.schedule.schedulecycle.nbrofweeks'
      ),
      accountId: new SoeSelectFormControl(
        element?.accountId || null,
        {},
        'common.user.attestrole.accounthierarchy'
      ),
      scheduleCycleRuleDTOs: arrayToFormArray(
        element?.scheduleCycleRuleDTOs || []
      ),
    });

    this.thisValidationHandler = validationHandler;
  }

  get scheduleCycleRuleDTOs(): FormArray<ScheduleCycleRuleForm> {
    return <FormArray>this.controls.scheduleCycleRuleDTOs;
  }

  get name() {
    return <SoeTextFormControl>this.controls.name;
  }

  get description() {
    return <SoeTextFormControl>this.controls.description;
  }

  get nbrOfWeeks() {
    return <SoeNumberFormControl>this.controls.nbrOfWeeks;
  }

  get accountId() {
    return <SoeSelectFormControl>this.controls.accountId;
  }

  customPatchValue(element: IScheduleCycleDTO) {
    this.patchValue(element);
    this.customScheduleCycleRulesPatchValue(element?.scheduleCycleRuleDTOs ?? []);

    this.markAsUntouched({ onlySelf: true });
    this.markAsPristine({ onlySelf: true });
  }

  customScheduleCycleRulesPatchValue(rules: IScheduleCycleRuleDTO[]) {
    this.scheduleCycleRuleDTOs.clear({ emitEvent: false });
    rules.forEach(rule => {
      const form = new ScheduleCycleRuleForm({
        validationHandler: this.thisValidationHandler,
        element: rule,
      });
      form.customPatchValue(rule);
      this.scheduleCycleRuleDTOs.push(form);
    });
    this.scheduleCycleRuleDTOs.markAsUntouched({ onlySelf: true });
    this.scheduleCycleRuleDTOs.markAsPristine({ onlySelf: true });
    this.scheduleCycleRuleDTOs.updateValueAndValidity();
  }
}
