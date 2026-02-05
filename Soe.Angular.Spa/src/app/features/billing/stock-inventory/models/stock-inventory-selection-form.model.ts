import { ValidationHandler } from '@shared/handlers';
import { StockInventoryFilterDTO } from './stock-inventory.model';
import {
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';

interface IStockInventorySelectionForm {
  validationHandler: ValidationHandler;
  element: StockInventoryFilterDTO | undefined;
}

export class StockInventorySelectionForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IStockInventorySelectionForm) {
    super(validationHandler, {
      stockId: new SoeSelectFormControl(element?.stockId || undefined),
      productNrFrom: new SoeTextFormControl(
        element?.productNrFrom || undefined
      ),
      productNrTo: new SoeTextFormControl(element?.productNrTo || undefined),
      shelfIds: new SoeSelectFormControl(element?.shelfIds || []),
      productGroupIds: new SoeSelectFormControl(element?.productGroupIds || []),

      productNrFromId: new SoeSelectFormControl(
        element?.productNrFromId || null
      ),
      productNrToId: new SoeSelectFormControl(element?.productNrToId || null),
    });
  }

  get stockId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.stockId;
  }

  get productNrFrom(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.productNrFrom;
  }

  get productNrTo(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.productNrTo;
  }

  get shelfIds(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.shelfIds;
  }

  get productGroupIds(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.productGroupIds;
  }

  get productNrFromId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.productNrFromId;
  }

  get productNrToId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.productNrToId;
  }
}
