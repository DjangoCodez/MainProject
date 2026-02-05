import { SoeFormGroup, SoeTextFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { CommodityCodeDTO } from '../../../manage/commodity-codes/models/commodity-codes.model';

interface IStatisticalCommodityCodesForm {
  validationHandler: ValidationHandler;
  element: CommodityCodeDTO | undefined;
}
export class StatisticalCommodityCodesForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IStatisticalCommodityCodesForm) {
    super(validationHandler, {
      intrastatCodeId: new SoeTextFormControl(element?.intrastatCodeId || 0, {
        isIdField: true,
      }),
    });
  }

  get intrastatCodeId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.intrastatCodeId;
  }
}
