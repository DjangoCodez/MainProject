import { SoeFormGroup, SoeSelectFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { VoucherSeriesDTO } from './account-years-and-periods.model';

interface IVoucherSeriesRowsForm {
  validationHandler: ValidationHandler;
  element: VoucherSeriesDTO | undefined;
}

export class VoucherSeriesRowsForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IVoucherSeriesRowsForm) {
    super(validationHandler, {
      voucherSeriesTypes: new SoeSelectFormControl(
        element?.voucherSeriesTypeId || 0
      ),
    });
  }

  get voucherSeriesTypes(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.voucherSeriesTypeId;
  }
}
