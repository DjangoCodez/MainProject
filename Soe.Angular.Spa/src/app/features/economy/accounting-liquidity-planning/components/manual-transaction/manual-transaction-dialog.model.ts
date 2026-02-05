import { ValidationHandler } from '@shared/handlers';
import { LiquidityPlanningDTO } from '../../models/liquidity-planning.model';
import {
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeTextFormControl,
} from '@shared/extensions';

interface IManualTransactionDialogForm {
  validationHandler: ValidationHandler;
  element: LiquidityPlanningDTO;
}

export class ManualTransactionDialogForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IManualTransactionDialogForm) {
    super(validationHandler, {
      specification: new SoeTextFormControl(
        element?.specification || '',
        {
          required: true,
        },
        'economy.accounting.liquidityplanning.specification'
      ),
      date: new SoeDateFormControl(
        element?.date || new Date(),
        {
          required: true,
        },
        'common.date'
      ),
      total: new SoeNumberFormControl(
        element?.total || '',
        {
          required: true,
        },
        'common.amount'
      ),
      valueIn: new SoeNumberFormControl(element?.valueIn || ''),
      valueOut: new SoeNumberFormControl(element?.valueOut || ''),
      liquidityPlanningTransactionId: new SoeNumberFormControl(
        element?.liquidityPlanningTransactionId || ''
      ),
    });
  }

  get specification(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.specification;
  }

  get date(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.date;
  }

  get total(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.total;
  }

  get valueIn(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.valueIn;
  }

  get valueOut(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.valueOut;
  }

  get liquidityPlanningTransactionId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.liquidityPlanningTransactionId;
  }
}
