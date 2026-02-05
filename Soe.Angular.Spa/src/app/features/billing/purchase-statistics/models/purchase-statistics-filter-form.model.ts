import { SoeDateFormControl, SoeFormGroup } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { PurchaseStatisticsFilterDTO } from './purchase-statistics.model';

interface IPurchaseStatisticsFilterForm {
  validationHandler: ValidationHandler;
  element: PurchaseStatisticsFilterDTO | undefined;
}
export class PurchaseStatisticsFilterForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IPurchaseStatisticsFilterForm) {
    super(validationHandler, {
      fromDate: new SoeDateFormControl(element?.fromDate || new Date()),
      toDate: new SoeDateFormControl(element?.toDate || new Date()),
    });
  }

  get fromDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.fromDate;
  }

  get toDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.toDate;
  }
}
