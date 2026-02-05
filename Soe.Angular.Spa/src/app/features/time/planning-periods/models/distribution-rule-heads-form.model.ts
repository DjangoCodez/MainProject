import { FormArray } from '@angular/forms';
import { SoeFormGroup, SoeTextFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import {
  IPayrollProductDistributionRuleDTO,
  IPayrollProductDistributionRuleHeadDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { arrayToFormArray } from '@shared/util/form-util';
import { DistributionRulesForm } from './dr-rule-form.model';

interface IDistributionRuleHeadsForm {
  validationHandler: ValidationHandler;
  element: IPayrollProductDistributionRuleHeadDTO | undefined;
}
export class DistributionRuleHeadsForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;
  constructor({ validationHandler, element }: IDistributionRuleHeadsForm) {
    super(validationHandler, {
      payrollProductDistributionRuleHeadId: new SoeTextFormControl(
        element?.payrollProductDistributionRuleHeadId || 0,
        {
          isIdField: true,
        }
      ),
      name: new SoeTextFormControl(
        element?.name || '',
        { isNameField: true, required: true, maxLength: 100, minLength: 1 },
        'common.name'
      ),
      description: new SoeTextFormControl(
        element?.description || '',
        { isNameField: false, required: false, maxLength: 256, minLength: 1 },
        'common.description'
      ),

      rules: arrayToFormArray(element?.rules || []),
    });
    this.thisValidationHandler = validationHandler;
  }

  set rules(formArray: FormArray) {
    this.controls.rules = formArray;
  }
  get rules(): FormArray {
    return <FormArray>this.controls.rules;
  }

  customPatchValue(
    element: IPayrollProductDistributionRuleHeadDTO,
    isCopy: boolean = false
  ) {
    this.patchValue(element);
    this.customPatchhRules(element.rules ?? [], isCopy);
  }
  customPatchhRules(
    rules: IPayrollProductDistributionRuleDTO[],
    isCopy: boolean = false
  ): void {
    this.rules.clear({ emitEvent: false });
    rules.forEach(r => {
      const dto = isCopy ? { ...r, payrollProductDistributionRuleId: 0 } : r;
      const ruleForm = new DistributionRulesForm({
        validationHandler: this.thisValidationHandler,
        element: dto,
      });
      this.rules.push(ruleForm);
    });
    this.rules.markAsUntouched({ onlySelf: true });
    this.rules.markAsPristine({ onlySelf: true });
    this.rules.updateValueAndValidity({ emitEvent: true });
  }
}
