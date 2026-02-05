import {
  NumberFormControlOptions,
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { StockTransactionDTO } from './stock-balance.model';
import { TermGroup_StockTransactionType } from '@shared/models/generated-interfaces/Enumerations';
import { Validators } from '@angular/forms';

interface IStockBalanceTransactionForm {
  validationHandler: ValidationHandler;
  element: StockTransactionDTO | undefined;
}
export class StockBalanceTransactionForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IStockBalanceTransactionForm) {
    super(validationHandler, {
      stockTransactionId: new SoeTextFormControl(
        element?.stockTransactionId || 0
      ),

      productName: new SoeTextFormControl(element?.productName || '', {
        isNameField: true,
      }),
      stockProductId: new SoeNumberFormControl(element?.stockProductId || 0, {
        isIdField: true,
      }),
      actionType: new SoeSelectFormControl(
        element?.actionType || TermGroup_StockTransactionType.Add
      ),
      actionTypeName: new SoeTextFormControl(element?.actionTypeName || ''),
      quantity: new SoeNumberFormControl(
        element?.quantity,
        {},
        'billing.stock.stocksaldo.actionquantity'
      ),
      price: new SoeNumberFormControl(
        element?.price,
        { decimals: 2, minDecimals: 2, maxDecimals: 2, required: true },
        'billing.stock.stocksaldo.actionprice'
      ),
      note: new SoeTextFormControl(element?.note || ''),
      productUnitConvertId: new SoeSelectFormControl(
        element?.productUnitConvertId || undefined
      ),
      transactionDate: new SoeDateFormControl(
        element?.transactionDate || new Date()
      ),
      created: new SoeTextFormControl(element?.created || ''),
      createdBy: new SoeTextFormControl(element?.createdBy || ''),
      targetStockId: new SoeSelectFormControl(
        element?.targetStockId || undefined,
        {},
        'billing.stock.stocksaldo.stockplaceto'
      ),
      stockId: new SoeTextFormControl(element?.stockId || 0),
      stockName: new SoeTextFormControl(element?.stockName || ''),
      productId: new SoeTextFormControl(element?.productId || 0),
    });
  }

  get stockTransactionId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.stockTransactionId;
  }

  get stockProductId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.stockProductId;
  }

  get productName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.productName;
  }

  get actionTypeName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.actionTypeName;
  }

  get actionType(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.actionType;
  }

  get quantity(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.quantity;
  }

  get price(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.price;
  }

  get note(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.note;
  }

  get productUnitConvertId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.productUnitConvertId;
  }

  get transactionDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.transactionDate;
  }

  get created(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.created;
  }

  get createdBy(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.createdBy;
  }

  get targetStockId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.targetStockId;
  }

  get stockId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.stockId;
  }

  get stockName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.stockName;
  }

  get productId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.productId;
  }

  removePriceValidations() {
    this.price.clearValidators();
    this.price.clearAsyncValidators();
    this.updateValueAndValidity();
  }

  addPriceValidations() {
    this.price.addValidators(Validators.required);
    this.price.addAsyncValidators(
      NumberFormControlOptions.getAsyncValidatorFns(<NumberFormControlOptions>{
        decimals: 2,
        minDecimals: 2,
        maxDecimals: 2,
      }) ?? []
    );

    this.updateValueAndValidity();
  }
}
