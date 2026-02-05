import {
  SoeCheckboxFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { BudgetRowFlattenedDTO } from './budget.model';

interface IBudgetRowForm {
  validationHandler: ValidationHandler;
  element: BudgetRowFlattenedDTO | undefined;
}

export class BudgetRowForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IBudgetRowForm) {
    super(validationHandler, {
      budgetRowId: new SoeTextFormControl(element?.budgetRowId || 0, {
        isIdField: true,
      }),
      accountId: new SoeTextFormControl(element?.accountId || 0),
      distributionCodeHeadId: new SoeTextFormControl(
        element?.distributionCodeHeadId || 0
      ),
      totalAmount: new SoeNumberFormControl(element?.totalAmount),
      dim1Id: new SoeNumberFormControl(
        element?.dim1Id ?? 0,
        { 
          required: true, 
          zeroNotAllowed: true 
        },
        'economy.accounting.account'
      ),
      dim2Id: new SoeNumberFormControl(element?.dim2Id || 0),
      dim3Id: new SoeNumberFormControl(element?.dim3Id || 0),
      amount2: new SoeNumberFormControl(element?.amount2 || 0),
      amount1: new SoeNumberFormControl(element?.amount1 || 0),
      amount3: new SoeNumberFormControl(element?.amount3 || 0),
      amount4: new SoeNumberFormControl(element?.amount4 || 0),
      amount5: new SoeNumberFormControl(element?.amount5 || 0),
      amount6: new SoeNumberFormControl(element?.amount6 || 0),
      amount7: new SoeNumberFormControl(element?.amount7 || 0),
      amount8: new SoeNumberFormControl(element?.amount8 || 0),
      amount9: new SoeNumberFormControl(element?.amount9 || 0),
      amount10: new SoeNumberFormControl(element?.amount10 || 0),
      amount11: new SoeNumberFormControl(element?.amount11 || 0),
      amount12: new SoeNumberFormControl(element?.amount12 || 0),
      amount13: new SoeNumberFormControl(element?.amount13 || 0),
      amount14: new SoeNumberFormControl(element?.amount14 || 0),
      amount15: new SoeNumberFormControl(element?.amount15 || 0),
      amount16: new SoeNumberFormControl(element?.amount16 || 0),
      amount17: new SoeNumberFormControl(element?.amount17 || 0),
      amount18: new SoeNumberFormControl(element?.amount18 || 0),
      budgetRowPeriodId1: new SoeNumberFormControl(element?.budgetRowPeriodId1 ?? 0),
      budgetRowPeriodId2: new SoeNumberFormControl(element?.budgetRowPeriodId2 ?? 0),
      budgetRowPeriodId3: new SoeNumberFormControl(element?.budgetRowPeriodId3 ?? 0),
      budgetRowPeriodId4: new SoeNumberFormControl(element?.budgetRowPeriodId4 ?? 0),
      budgetRowPeriodId5: new SoeNumberFormControl(element?.budgetRowPeriodId5 ?? 0),
      budgetRowPeriodId6: new SoeNumberFormControl(element?.budgetRowPeriodId6 ?? 0),
      budgetRowPeriodId7: new SoeNumberFormControl(element?.budgetRowPeriodId7 ?? 0),
      budgetRowPeriodId8: new SoeNumberFormControl(element?.budgetRowPeriodId8 ?? 0),
      budgetRowPeriodId9: new SoeNumberFormControl(element?.budgetRowPeriodId9 ?? 0),
      budgetRowPeriodId10: new SoeNumberFormControl(element?.budgetRowPeriodId10 ?? 0),
      budgetRowPeriodId11: new SoeNumberFormControl(element?.budgetRowPeriodId11 ?? 0),
      budgetRowPeriodId12: new SoeNumberFormControl(element?.budgetRowPeriodId12 ?? 0),
      budgetRowPeriodId13: new SoeNumberFormControl(element?.budgetRowPeriodId13 ?? 0),
      budgetRowPeriodId14: new SoeNumberFormControl(element?.budgetRowPeriodId14 ?? 0),
      budgetRowPeriodId15: new SoeNumberFormControl(element?.budgetRowPeriodId15 ?? 0),
      budgetRowPeriodId16: new SoeNumberFormControl(element?.budgetRowPeriodId16 ?? 0),
      budgetRowPeriodId17: new SoeNumberFormControl(element?.budgetRowPeriodId17 ?? 0),
      budgetRowPeriodId18: new SoeNumberFormControl(element?.budgetRowPeriodId18 ?? 0),
      periodNr1: new SoeNumberFormControl(element?.periodNr1 ?? 0),
      periodNr2: new SoeNumberFormControl(element?.periodNr2 ?? 0),
      periodNr3: new SoeNumberFormControl(element?.periodNr3 ?? 0),
      periodNr4: new SoeNumberFormControl(element?.periodNr4 ?? 0),
      periodNr5: new SoeNumberFormControl(element?.periodNr5 ?? 0),
      periodNr6: new SoeNumberFormControl(element?.periodNr6 ?? 0),
      periodNr7: new SoeNumberFormControl(element?.periodNr7 ?? 0),
      periodNr8: new SoeNumberFormControl(element?.periodNr8 ?? 0),
      periodNr9: new SoeNumberFormControl(element?.periodNr9 ?? 0),
      periodNr10: new SoeNumberFormControl(element?.periodNr10 ?? 0),
      periodNr11: new SoeNumberFormControl(element?.periodNr11 ?? 0),
      periodNr12: new SoeNumberFormControl(element?.periodNr12 ?? 0),
      periodNr13: new SoeNumberFormControl(element?.periodNr13 ?? 0),
      periodNr14: new SoeNumberFormControl(element?.periodNr14 ?? 0),
      periodNr15: new SoeNumberFormControl(element?.periodNr15 ?? 0),
      periodNr16: new SoeNumberFormControl(element?.periodNr16 ?? 0),
      periodNr17: new SoeNumberFormControl(element?.periodNr17 ?? 0),
      periodNr18: new SoeNumberFormControl(element?.periodNr18 ?? 0),
      isDeleted: new SoeCheckboxFormControl(element?.isDeleted || false),
      isModified: new SoeCheckboxFormControl(element?.isModified ?? false),
    });
  }

  get budgetRowId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.budgetRowId;
  }

  get accountId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.accountId;
  }
  get distributionCodeHeadId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.distributionCodeHeadId;
  }
  get totalAmount(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.totalAmount;
  }
  get dim1Id(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.dim1Id;
  }

  get dim2Id(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.dim2Id;
  }
  get dim3Id(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.dim3Id;
  }

  get isDeleted(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.isDeleted;
  }
}
