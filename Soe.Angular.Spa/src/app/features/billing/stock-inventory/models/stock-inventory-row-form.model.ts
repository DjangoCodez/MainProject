import { ValidationHandler } from '@shared/handlers';
import { StockInventoryRowDTO } from './stock-inventory.model';
import {
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeTextFormControl,
} from '@shared/extensions';

interface IStockInventoryRowForm {
  validationHandler: ValidationHandler;
  element: StockInventoryRowDTO | undefined;
}

export class StockInventoryRowForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IStockInventoryRowForm) {
    super(validationHandler, {
      stockInventoryRowId: new SoeTextFormControl(
        element?.stockInventoryRowId || 0,
        { isIdField: true }
      ),
      stockInventoryHeadId: new SoeTextFormControl(
        element?.stockInventoryHeadId || 0
      ),
      stockProductId: new SoeTextFormControl(element?.stockProductId || 0),
      startingSaldo: new SoeTextFormControl(element?.startingSaldo || 0),
      inventorySaldo: new SoeNumberFormControl(element?.inventorySaldo || 0),
      difference: new SoeTextFormControl(element?.difference || 0),
      productNumber: new SoeTextFormControl(element?.productNumber || ''),
      productName: new SoeTextFormControl(element?.productName || ''),
      unit: new SoeTextFormControl(element?.unit || ''),
      avgPrice: new SoeTextFormControl(element?.avgPrice || 0),
      shelfId: new SoeTextFormControl(element?.shelfId || 0),
      shelfCode: new SoeTextFormControl(element?.shelfCode || ''),
      shelfName: new SoeTextFormControl(element?.shelfName || ''),
      orderedQuantity: new SoeTextFormControl(element?.orderedQuantity || 0),
      reservedQuantity: new SoeTextFormControl(element?.reservedQuantity || 0),
      transactionDate: new SoeDateFormControl(
        element?.transactionDate || undefined
      ),
    });
  }
}
