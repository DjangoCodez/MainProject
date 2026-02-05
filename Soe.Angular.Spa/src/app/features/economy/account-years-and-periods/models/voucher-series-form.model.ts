import {
  SoeCheckboxFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { VoucherSeriesTypeDTO } from '../../models/voucher-series-type.model';

interface IVoucherSeriesForm {
  validationHandler: ValidationHandler;
  element: VoucherSeriesTypeDTO | undefined;
}
export class VoucherSeriesForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;

  constructor({ validationHandler, element }: IVoucherSeriesForm) {
    super(validationHandler, {
      voucherSeriesTypeId: new SoeTextFormControl(
        element?.voucherSeriesTypeId || '',
        {
          isIdField: true,
        }
      ),
      voucherSeriesTypeNr: new SoeNumberFormControl(
        element?.voucherSeriesTypeNr || undefined,
        { required: true, isNameField: true },
        'economy.accounting.voucherseriestype.voucherseriestypenr'
      ),
      name: new SoeTextFormControl(
        element?.name || '',
        { required: true },
        'economy.accounting.voucherseriestype.name'
      ),
      startNr: new SoeNumberFormControl(
        element?.startNr || undefined,
        {
          required: true,
        },
        'economy.accounting.voucherseriestype.startnr'
      ),
      yearEndSerie: new SoeCheckboxFormControl(element?.yearEndSerie || false),
      externalSerie: new SoeCheckboxFormControl(
        element?.externalSerie || false
      ),
    });
    this.thisValidationHandler = validationHandler;
  }

  get voucherSeriesTypeId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.voucherSeriesTypeId;
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }

  get voucherSeriesTypeNr(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.voucherSeriesTypeNr;
  }

  get startNr(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.startNr;
  }

  get yearEndSerie(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.yearEndSerie;
  }

  get externalSerie(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.externalSerie;
  }
}
