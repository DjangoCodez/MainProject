import { ValidationHandler } from '@shared/handlers';
import { VoucherSearchSummaryDTO } from './voucher-search.model';
import { SoeFormGroup, SoeNumberFormControl } from '@shared/extensions';

interface IVoucherSearchSummaryForm {
  validationHandler: ValidationHandler;
  element?: VoucherSearchSummaryDTO;
}

export class VoucherSearchSummaryForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IVoucherSearchSummaryForm) {
    super(validationHandler, {
      creditTotal: new SoeNumberFormControl(element?.creditTotal || 0.0),
      debitTotal: new SoeNumberFormControl(element?.debitTotal || 0.0),
      balance: new SoeNumberFormControl(element?.balance || 0.0),
      creditTotalSelected: new SoeNumberFormControl(
        element?.creditTotalSelected || 0.0
      ),
      debitTotalSelected: new SoeNumberFormControl(
        element?.debitTotalSelected || 0.0
      ),
      balanceSelected: new SoeNumberFormControl(
        element?.balanceSelected || 0.0
      ),
    });
  }
  get creditTotal(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.creditTotal;
  }

  get debitTotal(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.debitTotal;
  }

  get balance(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.balance;
  }

  get creditTotalSelected(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.creditTotalSelected;
  }

  get debitTotalSelected(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.debitTotalSelected;
  }

  get balanceSelected(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.balanceSelected;
  }
}
