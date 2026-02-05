import {
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
} from '@shared/extensions';
import { GetLiquidityPlanningModel } from './liquidity-planning.model';
import { ValidationHandler } from '@shared/handlers';

interface ILiquidityPlanningFilterForm {
  validationHandler: ValidationHandler;
  element: GetLiquidityPlanningModel | undefined;
}
export class LiquidityPlanningFilterForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ILiquidityPlanningFilterForm) {
    super(validationHandler, {
      from: new SoeDateFormControl(element?.from || new Date()),
      to: new SoeDateFormControl(element?.to || new Date()),
      exclusion: new SoeDateFormControl(element?.exclusion || undefined),
      balance: new SoeNumberFormControl(element?.balance || 0),
      selectedPaymentStatuses: new SoeSelectFormControl(
        element?.selectedPaymentStatuses || []
      ),
    });
  }

  get from(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.from;
  }

  get to(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.to;
  }

  get exclusion(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.exclusion;
  }

  get balance(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.balance;
  }

  get selectedPaymentStatuses(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.selectedPaymentStatuses;
  }
}
