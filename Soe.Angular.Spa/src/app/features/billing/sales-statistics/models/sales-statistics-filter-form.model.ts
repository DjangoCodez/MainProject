import {
  SoeDateFormControl,
  SoeFormGroup,
  SoeSelectFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { GeneralProductStatisticsDTO } from './sales-statistics.model';
import { SoeOriginType } from '@shared/models/generated-interfaces/Enumerations';

interface ISalesStatisticsFilterForm {
  validationHandler: ValidationHandler;
  element: GeneralProductStatisticsDTO | undefined;
}
export class SalesStatisticsFilterForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ISalesStatisticsFilterForm) {
    super(validationHandler, {
      originType: new SoeSelectFormControl(
        element?.originType || SoeOriginType.CustomerInvoice
      ),
      fromDate: new SoeSelectFormControl(element?.fromDate || new Date()),
      toDate: new SoeSelectFormControl(element?.toDate || new Date()),
    });
  }

  get originType(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.originType;
  }

  get fromDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.fromDate;
  }

  get toDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.toDate;
  }
}
