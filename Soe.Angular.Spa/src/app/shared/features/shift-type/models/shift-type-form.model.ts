import {
  SoeCheckboxFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { arrayToFormArray, clearAndSetFormArray } from '@shared/util/form-util';
import { FormArray, FormControl } from '@angular/forms';
import { AccountingSettingsForm } from '@shared/components/accounting-settings/accounting-settings/accounting-settings-form.model';
import {
  IShiftTypeEmployeeStatisticsTargetDTO,
  IShiftTypeSkillDTO,
  IShiftTypeDTO,
  IShiftTypeHierarchyAccountDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { HierarchyAccountsForm } from './hierarchy-accounts.form.model';
import { ShiftTypeEmployeeStatisticsTargetForm } from './employee-statistic.form.model';

interface IShiftTypeForm {
  validationHandler: ValidationHandler;
  element: IShiftTypeDTO | undefined;
}
export class ShiftTypeForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;

  constructor({ validationHandler, element }: IShiftTypeForm) {
    super(validationHandler, {
      shiftTypeId: new SoeTextFormControl(element?.shiftTypeId || 0, {
        isIdField: true,
      }),
      name: new SoeTextFormControl(
        element?.name || '',
        { isNameField: true, required: true, maxLength: 100 },
        'common.name'
      ),
      description: new SoeTextFormControl(element?.description || '', {
        maxLength: 512,
      }),
      needsCode: new SoeTextFormControl(element?.needsCode || '', {
        maxLength: 10,
      }),
      accountId: new SoeNumberFormControl(element?.accountId || -1),
      timeScheduleTemplateBlockType: new SoeNumberFormControl(
        element?.timeScheduleTemplateBlockType || undefined
      ),
      timeScheduleTypeId: new SoeNumberFormControl(
        element?.timeScheduleTypeId || undefined
      ),
      color: new SoeTextFormControl(element?.color || '#707070', {
        maxLength: 10,
      }),
      externalCode: new SoeTextFormControl(element?.externalCode || '', {
        maxLength: 100,
      }),
      defaultLength: new SoeNumberFormControl(element?.defaultLength || 0),
      defaultLengthFormatted: new SoeTextFormControl('0:00'),
      handlingMoney: new SoeCheckboxFormControl(
        element?.handlingMoney || false
      ),
      categoryIds: arrayToFormArray(element?.categoryIds || []),
      shiftTypeSkills: arrayToFormArray(element?.shiftTypeSkills || []),
      employeeStatisticsTargets: arrayToFormArray(
        element?.employeeStatisticsTargets || []
      ),
      accountingSettings: new AccountingSettingsForm({
        validationHandler,
        element: element?.accountingSettings,
      }),
      hierarchyAccounts: arrayToFormArray(element?.hierarchyAccounts || []),
    });

    this.thisValidationHandler = validationHandler;
  }

  get categoryIds(): FormArray<FormControl<number>> {
    return <FormArray>this.controls.categoryIds;
  }

  get shiftTypeSkills(): FormArray<FormControl<IShiftTypeSkillDTO>> {
    return <FormArray>this.controls.shiftTypeSkills;
  }

  get employeeStatisticsTargets(): FormArray<ShiftTypeEmployeeStatisticsTargetForm> {
    return <FormArray>this.controls.employeeStatisticsTargets;
  }

  get externalCode(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.externalCode;
  }

  get defaultLength(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.defaultLength;
  }

  get defaultLengthFormatted(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.defaultLengthFormatted;
  }

  get timeScheduleTemplateBlockType(): SoeNumberFormControl {
    return <SoeTextFormControl>this.controls.timeScheduleTemplateBlockType;
  }

  get accountingSettings(): AccountingSettingsForm {
    return <AccountingSettingsForm>this.controls.accountingSettings;
  }

  get hierarchyAccounts(): FormArray<HierarchyAccountsForm> {
    return <FormArray>this.controls.hierarchyAccounts;
  }

  customPatchValue(element: IShiftTypeDTO) {
    this.reset();
    this.patchValue(element);
    if (element.categoryIds) {
      this.customCategoryIdsPatchValue(element.categoryIds);
    }
    if (element.shiftTypeSkills) {
      this.customSkillIdsPatchValue(element.shiftTypeSkills);
    }
    if (element.employeeStatisticsTargets) {
      this.customStatisticsIdsPatchValue(element.employeeStatisticsTargets);
    }
    if (element.hierarchyAccounts) {
      this.customHierarchyAccountsPatchValue(element.hierarchyAccounts);
    }
    this.markAsUntouched({ onlySelf: true });
    this.markAsPristine({ onlySelf: true });
  }

  customHierarchyAccountsPatchValue(
    hierarchyAccounts: IShiftTypeHierarchyAccountDTO[]
  ) {
    this.hierarchyAccounts.clear({ emitEvent: false });
    hierarchyAccounts.forEach(account => {
      const hierarchyAccountsForm = new HierarchyAccountsForm({
        validationHandler: this.thisValidationHandler,
        element: account,
      });
      hierarchyAccountsForm.patchValue(account);
      this.hierarchyAccounts.push(hierarchyAccountsForm);
    });
    this.hierarchyAccounts.markAsUntouched({ onlySelf: true });
    this.hierarchyAccounts.markAsPristine({ onlySelf: true });
    this.hierarchyAccounts.updateValueAndValidity();
  }

  customCategoryIdsPatchValue(categoryIds: number[]) {
    clearAndSetFormArray(categoryIds, this.categoryIds);
  }

  customStatisticsIdsPatchValue(
    employeeStatisticsTargets: IShiftTypeEmployeeStatisticsTargetDTO[]
  ) {
    this.employeeStatisticsTargets.clear({ emitEvent: false });
    employeeStatisticsTargets.forEach(statistic => {
      const statisticForm = new ShiftTypeEmployeeStatisticsTargetForm({
        validationHandler: this.thisValidationHandler,
        element: statistic,
      });
      statisticForm.patchValue(statistic);
      this.employeeStatisticsTargets.push(statisticForm);
    });
    this.employeeStatisticsTargets.markAsUntouched({ onlySelf: true });
    this.employeeStatisticsTargets.markAsPristine({ onlySelf: true });
    this.employeeStatisticsTargets.updateValueAndValidity();
  }

  customSkillIdsPatchValue(shiftTypeSkills: IShiftTypeSkillDTO[]) {
    clearAndSetFormArray(shiftTypeSkills, this.shiftTypeSkills);
  }
}
