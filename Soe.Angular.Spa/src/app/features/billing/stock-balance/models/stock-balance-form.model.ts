import {
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import {
  IAutocompleteIStockProductDTO,
  StockProductDTO,
  StockTransactionDTO,
} from './stock-balance.model';
import { StockBalanceTransactionForm } from './stock-balance-transaction-form.model';
import { TermGroup_StockTransactionType } from '@shared/models/generated-interfaces/Enumerations';

interface IStockBalanceForm {
  validationHandler: ValidationHandler;
  element: StockProductDTO | undefined;
}
export class StockBalanceForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IStockBalanceForm) {
    super(validationHandler, {
      stockProductId: new SoeSelectFormControl(
        element?.stockProductId,
        {
          isIdField: true,
          required: true,
        },
        'billing.stock.stocks.stock'
      ),
      productNumber: new SoeTextFormControl(element?.productNumber || '', {
        maxLength: 20,
      }),
      productName: new SoeTextFormControl(element?.productName || '', {
        isNameField: true,
        maxLength: 100,
      }),
      stockId: new SoeSelectFormControl(element?.stockId || 0),
      stockName: new SoeTextFormControl(element?.stockName || '', {
        maxLength: 100,
      }),
      stockShelfName: new SoeTextFormControl(element?.stockShelfName || '', {
        maxLength: 100,
      }),
      productUnit: new SoeTextFormControl(element?.productUnit || '', {
        maxLength: 100,
      }),
      invoiceProductId: new SoeSelectFormControl(
        element?.invoiceProductId || null,
        { required: true },
        'billing.stock.stocksaldo.productnumber'
      ),
      avgPrice: new SoeNumberFormControl(element?.avgPrice || ''),
      transaction: new StockBalanceTransactionForm({
        validationHandler: validationHandler,
        element: new StockTransactionDTO(),
      }),
    });

    this.productUnit.disable();
    this.stockShelfName.disable();

    this.invoiceProductId.valueChanges.subscribe(x => {
      this.transaction.patchValue({ productId: x });
    });

    this.stockId.valueChanges.subscribe(x => {
      this.transaction.patchValue({ stockId: x });
    });

    this.avgPrice.valueChanges.subscribe(avgPrice => {
      this.transaction.patchValue({ price: avgPrice });
    });
  }

  get stockProductId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.stockProductId;
  }
  get productNumber(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.productNumber;
  }
  get stockName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.stockName;
  }
  get stockShelfName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.stockShelfName;
  }
  get productUnit(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.productUnit;
  }
  get avgPrice(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.avgPrice;
  }

  get stockId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.stockId;
  }

  get transaction(): StockBalanceTransactionForm {
    return <StockBalanceTransactionForm>this.controls.transaction;
  }

  get invoiceProductId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.invoiceProductId;
  }

  customPatch(stockProductDto: StockProductDTO) {
    this.patchValue(stockProductDto);
    this.transaction.patchValue({
      stockProductId: stockProductDto.stockProductId,
      productId: stockProductDto.invoiceProductId,
      stockId: stockProductDto.stockId,
      stockName: stockProductDto.stockName,
      price: stockProductDto.transactionPrice
        ? Number(stockProductDto.transactionPrice)
        : null,
    });
  }

  patchStockProductId(id: number) {
    this.patchValue({
      stockProductId: id,
      transaction: {
        stockProductId: id,
      },
    });
  }

  patchTransaction(stockProduct?: IAutocompleteIStockProductDTO) {
    if (stockProduct) {
      this.patchValue({
        stockShelfName: this.stockProductId.value
          ? stockProduct.stockShelfName
          : null,
        productUnit: stockProduct.productUnit,
        transaction: {
          stockId: stockProduct.stockId,
          stockProductId: stockProduct.stockProductId,
          price: stockProduct.transactionPrice
            ? Number(stockProduct.transactionPrice)
            : null,
        },
      });
    }
  }

  initForm() {
    if (this.isNew) {
      this.patchValue({
        invoiceProductId: 0,
        stockProductId: 0,
        stockShelfName: '',
        productUnit: '',
        transaction: {
          quantity: undefined,
          price: undefined,
          targetStockId: undefined,
        },
      });
    }

    const trDto = <StockTransactionDTO>{
      transactionDate: new Date(),
      actionType: TermGroup_StockTransactionType.Add,
      price:
        this.invoiceProductId && this.avgPrice.value
          ? Number(this.avgPrice.value)
          : 0.0,
      stockProductId: this.stockProductId.value,
      productId: this.invoiceProductId.value,
      stockId: this.stockId.value,
      stockName: this.stockName.value,
    };

    this.transaction.reset();
    this.transaction.patchValue(trDto);
    this.markAsUntouched();
    this.markAsPristine();
  }

  clearDataAdding() {
    if (this.isNew) {
      this.patchValue({
        invoiceProductId: 0,
        stockShelfName: '',
        productUnit: '',
        transaction: {
          quantity: undefined,
          price: undefined,
          targetStockId: undefined,
          note: '',
        },
      });
    }
  }
}
