import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeSelectFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { SoeOriginType } from '@shared/models/generated-interfaces/Enumerations';
import { ProductStatisticsRequest } from './product-statistics.model';

interface IProductStatisticsFilterForm {
  validationHandler: ValidationHandler;
  element: ProductStatisticsRequest | undefined;
}
export class ProductStatisticsFilterForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IProductStatisticsFilterForm) {
    super(validationHandler, {
      productIds: new SoeSelectFormControl(element?.productIds || []),
      originType: new SoeSelectFormControl(
        element?.originType || SoeOriginType.None,
      ),
      fromDate: new SoeSelectFormControl(element?.fromDate || new Date()),
      toDate: new SoeSelectFormControl(element?.toDate || new Date()),
      includeServiceProducts: new SoeCheckboxFormControl(element?.includeServiceProducts || false)
    });
  }

  get productIds(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.productIds;
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

  get includeServiceProducts(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.includeServiceProducts;
  }
}
