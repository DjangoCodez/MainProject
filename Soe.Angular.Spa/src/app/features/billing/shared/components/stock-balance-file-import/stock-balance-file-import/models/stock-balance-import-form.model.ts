import {
  SoeCheckboxFormControl,
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { ImportStockBalancesDTO } from './stock-balance-file-import.model';

interface IStockBalanceImportForm {
  validationHandler: ValidationHandler;
  element: ImportStockBalancesDTO | undefined;
}
export class StockBalanceImportForm extends SoeFormGroup {
  //stockBalanceValidationHandler: ValidationHandler;

  constructor({ validationHandler, element }: IStockBalanceImportForm) {
    super(validationHandler, {
      stockInventoryHeadId: new SoeTextFormControl(
        element?.stockInventoryHeadId || undefined
      ),
      wholesellerId: new SoeSelectFormControl(
        element?.wholesellerId || undefined,
        {},
        'common.customer.customer.wholesellername'
      ),
      stockId: new SoeSelectFormControl(
        element?.stockId || undefined,
        {},
        'billing.stock.stocks.stock'
      ),
      createVoucher: new SoeCheckboxFormControl(
        element?.createVoucher || false
      ),
      fromInventory: new SoeCheckboxFormControl(
        element?.fromInventory || false
      ),
      fileString: new SoeTextFormControl(element?.fileString || ''),
      fileName: new SoeTextFormControl(
        element?.fileName || '',
        {
          required: true,
        },
        'core.filename'
      ),
    });
  }

  get stockInventoryHeadId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.stockInventoryHeadId;
  }
  get wholesellerId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.wholesellerId;
  }
  get stockId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.stockId;
  }
  get createVoucher(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.createVoucher;
  }
  get fromInventory(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.fromInventory;
  }
  get fileString(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.fileString;
  }
  get fileName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.fileName;
  }
}
