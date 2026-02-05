import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
} from '../../../../../../shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { ISupplierProductPriceUpdateModel } from '@shared/models/generated-interfaces/BillingModels';
import { IPriceUpdateDTO } from '../../../../../../shared/models/generated-interfaces/PriceUpdateDTO';
import { Validators } from '@angular/forms';

interface IPriceUpdateForm {
  validationHandler: ValidationHandler;
  priceUpdateModel: ISupplierProductPriceUpdateModel | undefined;
}

export class PriceUpdateDTO implements IPriceUpdateDTO {
  amount: number;
  percentage: number;
  rounding: number;
  constructor() {
    this.amount = 0;
    this.percentage = 0;
    this.rounding = 0;
  }
}

export class PriceUpdateModel implements ISupplierProductPriceUpdateModel {
  supplierProductIds: number[] = [];
  priceUpdate: IPriceUpdateDTO = new PriceUpdateDTO();
  updateExisting = false;
  dateFrom? = new Date();
  dateTo?: Date;
  priceComparisonDate = new Date();
  currencyId = 0;
  quantityFrom?: number;
  quantityTo?: number;

  constructor(supplierProductIds: number[]) {
    this.supplierProductIds = supplierProductIds;
  }
}

export class PriceUpdateForm extends SoeFormGroup {
  constructor({ validationHandler, priceUpdateModel }: IPriceUpdateForm) {
    super(validationHandler, {
      supplierProductIds: new SoeFormControl(
        priceUpdateModel?.supplierProductIds || [],
        undefined,
        undefined,
        undefined,
        'billing.purchase.product.products'
      ),
      updateExisting: new SoeCheckboxFormControl(
        priceUpdateModel?.updateExisting || false,
        {},
        'billing.purchase.product.updateexisting'
      ),
      dateFrom: new SoeDateFormControl(
        priceUpdateModel?.dateFrom || null,
        {
          required: true,
        },
        'billing.purchase.product.priceisfrom'
      ),
      dateTo: new SoeDateFormControl(
        priceUpdateModel?.dateTo || null,
        {
          greaterThanDate: 'dateFrom',
        },
        'billing.purchase.product.priceisto'
      ),
      priceUpdate: new SoeFormGroup(validationHandler, {
        amount: new SoeNumberFormControl(
          priceUpdateModel?.priceUpdate.amount || 0,
          {},
          'common.amount'
        ),
        percentage: new SoeNumberFormControl(
          priceUpdateModel?.priceUpdate.percentage || 0,
          {},
          'common.percentage'
        ),
        rounding: new SoeSelectFormControl(
          priceUpdateModel?.priceUpdate.rounding || 0,
          {},
          'common.rounding'
        ),
      }),
      priceComparisonDate: new SoeDateFormControl(
        priceUpdateModel?.priceComparisonDate,
        { required: true },
        'billing.purchase.product.comparisondate'
      ),
      currencyId: new SoeSelectFormControl(
        priceUpdateModel?.currencyId || 0,
        {
          required: true,
          zeroNotAllowed: true,
        },
        'common.currency'
      ),
      quantityFrom: new SoeNumberFormControl(
        priceUpdateModel?.quantityFrom || undefined,
        {},
        'billing.purchase.product.quantityfrom'
      ),
      quantityTo: new SoeNumberFormControl(
        priceUpdateModel?.quantityTo || undefined,
        {},
        'billing.purchase.product.quantityto'
      ),
    });

    this.setChangeHandlers();
  }
  setChangeHandlers() {
    this.get(['priceUpdate', 'amount'])?.valueChanges.subscribe(value => {
      if (value && value != 0) {
        this.get(['priceUpdate', 'percentage'])?.setValue(0);
      }
    });

    this.get(['priceUpdate', 'percentage'])?.valueChanges.subscribe(value => {
      if (value && value != 0) {
        this.get(['priceUpdate', 'amount'])?.setValue(0);
      }
    });

    this.get('dateFrom')?.valueChanges.subscribe((fromDateValue: Date) => {
      const toDate = this.get('dateTo');
      const toDateValue = toDate?.value as Date;
      if (fromDateValue && toDateValue && fromDateValue > toDateValue) {
        toDate!.patchValue(fromDateValue);
      }
    });

    this.controls.updateExisting.valueChanges.subscribe(value => {
      this.controls.dateFrom.setValidators(
        value ? null : [Validators.required]
      );
      this.controls.dateFrom.updateValueAndValidity();
    });
  }
}
