import {
  SoeCheckboxFormControl,
  SoeFormGroup,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { StockShelfDTO } from './stock-warehouse.model';

interface IWarehouseCodeShelfForm {
  validationHandler: ValidationHandler;
  element: StockShelfDTO | undefined;
}
export class WarehouseCodeShelfForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IWarehouseCodeShelfForm) {
    super(validationHandler, {
      stockShelfId: new SoeTextFormControl(element?.stockShelfId || 0, {
        isIdField: true,
      }),
      stockId: new SoeTextFormControl(element?.stockId || 0),
      code: new SoeTextFormControl(
        element?.code || '',
        {},
        'billing.stock.stockplaces.code'
      ),
      name: new SoeTextFormControl(
        element?.name || '',
        {
          isNameField: true,
        },
        'billing.stock.stockplaces.name'
      ),
      stockName: new SoeTextFormControl(element?.stockName || ''),
      isDelete: new SoeCheckboxFormControl(element?.isDelete || false),
    });
  }

  get stockShelfId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.stockShelfId;
  }
  get stockId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.stockId;
  }
  get code(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.code;
  }
  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }
  get stockName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.stockName;
  }
  get isDelete(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.isDelete;
  }
}
