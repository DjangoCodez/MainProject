import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '../../../../../../shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { IPriceListUpdateModel } from '@shared/models/generated-interfaces/BillingModels';
import { IPriceUpdateDTO } from '../../../../../../shared/models/generated-interfaces/PriceUpdateDTO';
import { Validators } from '@angular/forms';

interface IPriceUpdateForm {
  validationHandler: ValidationHandler;
  priceUpdateModel: IPriceListUpdateModel | undefined;
}

export class PriceUpdateDTO implements IPriceUpdateDTO {
  amount: number;
  percentage: number;
  rounding: number;
  constructor() {
    this.amount = 0;
    this.percentage = 0.0;
    this.rounding = 0;
  }
}

export class PriceUpdateModel implements IPriceListUpdateModel {
  priceListTypeIds: number[] = [];
  priceUpdate: IPriceUpdateDTO = new PriceUpdateDTO();
  updateExisting: boolean = false;
  dateFrom? = new Date();
  dateTo?: Date;
  productNrFrom: string = '';
  productNrTo: string = '';
  materialCodeId: number = 0;
  productGroupId: number = 0;
  vatType: number = 0;
  priceComparisonDate: Date = new Date();
  quantityFrom: number = 0;
  quantityTo?: number;

  constructor(priceListTypeIds: number[]) {
    this.priceListTypeIds = priceListTypeIds;
  }
}

export class PriceUpdateForm extends SoeFormGroup {
  constructor({ validationHandler, priceUpdateModel }: IPriceUpdateForm) {
    super(validationHandler, {
      priceListTypeIds: new SoeFormControl(
        priceUpdateModel?.priceListTypeIds || [],
        undefined,
        undefined,
        undefined,
        'billing.products.pricelists.pricelist'
      ),
      updateExisting: new SoeCheckboxFormControl(
        priceUpdateModel?.updateExisting || false,
        {},
        'billing.product.pricelist.updateexisting'
      ),
      dateFrom: new SoeDateFormControl(
        priceUpdateModel?.dateFrom || null,
        {
          required: true,
        },
        'billing.product.pricelist.priceisfrom'
      ),
      dateTo: new SoeDateFormControl(
        priceUpdateModel?.dateTo || null,
        {
          greaterThanDate: 'dateFrom',
        },
        'billing.product.pricelist.priceisto'
      ),
      priceUpdate: new SoeFormGroup(validationHandler, {
        amount: new SoeNumberFormControl(
          priceUpdateModel?.priceUpdate.amount || 0,
          {},
          'common.amount'
        ),
        percentage: new SoeNumberFormControl(
          priceUpdateModel?.priceUpdate.percentage || 0.0,
          {
            minDecimals: 0,
            maxDecimals: 4,
          },
          'common.percentage'
        ),
        rounding: new SoeSelectFormControl(
          priceUpdateModel?.priceUpdate.rounding || 0,
          {},
          'common.rounding'
        ),
        decimalCount: new SoeNumberFormControl(
          1,
          {
            maxValue: 4,
            minValue: 1,
          },
          'common.decimalcount'
        ),
      }),
      productNrFrom: new SoeTextFormControl(
        priceUpdateModel?.productNrFrom || '',
        {},
        'common.customerspecificexports.productnrfrom'
      ),
      productNrTo: new SoeTextFormControl(priceUpdateModel?.productNrTo || ''),
      materialCodeId: new SoeSelectFormControl(
        priceUpdateModel?.materialCodeId || 0,
        {},
        'billing.product.materialcode'
      ),
      productGroupId: new SoeSelectFormControl(
        priceUpdateModel?.productGroupId || 0,
        {},
        'billing.product.productgroup'
      ),
      vatType: new SoeSelectFormControl(priceUpdateModel?.vatType || 0),
      priceComparisonDate: new SoeDateFormControl(
        priceUpdateModel?.priceComparisonDate,
        { required: true },
        'billing.product.pricelist.comparisondate'
      ),
      // quantityFrom: new SoeNumberFormControl(
      //   priceUpdateModel?.quantityFrom || 0,
      //   {},
      //   'billing.product.pricelist.quantityfrom'
      // ),
      // quantityTo: new SoeNumberFormControl(
      //   priceUpdateModel?.quantityTo || undefined,
      //   {},
      //   'billing.product.pricelist.quantityto'
      // ),
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
