import {
  SoeFormGroup,
  SoeTextFormControl,
  SoeSelectFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { ISieExportVoucherSelectionDTO } from '@shared/models/generated-interfaces/SieExportDTO';

interface ISieExportVoucherSelectionForm {
  validationHandler: ValidationHandler;
  element?: ISieExportVoucherSelectionDTO;
}

export class SieExportVoucherSelectionForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ISieExportVoucherSelectionForm) {
    super(validationHandler, {
      voucherSeriesId: new SoeSelectFormControl(element?.voucherSeriesId || 0),
      voucherNoFrom: new SoeTextFormControl(element?.voucherNoFrom || ''),
      voucherNoTo: new SoeTextFormControl(element?.voucherNoTo || ''),
    });
  }

  get voucherSeriesId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.voucherSeriesId;
  }
  get voucherNoFrom(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.voucherNoFrom;
  }
  get voucherNoTo(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.voucherNoTo;
  }
}
