import {
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { VoucherGridFilterDTO } from './voucher.model';

interface IVoucherGridFilterForm {
  validationHandler: ValidationHandler;
  element: VoucherGridFilterDTO | undefined;
}
export class VoucherGridFilterForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IVoucherGridFilterForm) {
    super(validationHandler, {
      accountYearId: new SoeTextFormControl(element?.accountYearId),
      voucherSeriesTypeId: new SoeSelectFormControl(
        element?.voucherSeriesTypeId || 0,
        { zeroNotAllowed: false }
      ),
    });
  }

  get accountYearId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.accountYearId;
  }

  get voucherSeriesTypeId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.voucherSeriesTypeId;
  }

  customVoucherSeriesTypeIdPatch(voucherSeriesTypeId: number) {
    this.patchValue({ voucherSeriesTypeId: voucherSeriesTypeId });
  }
}
