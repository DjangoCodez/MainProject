import {
  SoeCheckboxFormControl,
  SoeFormControl,
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { PriceListTypeDTO } from './customer-product-pricelist.model';
import { PriceListDTO } from '@features/billing/models/pricelist.model';

const PRICELISTS_KEY: keyof PriceListTypeDTO = 'priceLists';

interface ICustomerProductPriceListTypeForm {
  validationHandler: ValidationHandler;
  element: PriceListTypeDTO | undefined;
}

export class CustomerProductPriceListTypeForm extends SoeFormGroup {
  constructor({
    validationHandler,
    element,
  }: ICustomerProductPriceListTypeForm) {
    super(validationHandler, {
      priceListTypeId: new SoeTextFormControl(element?.priceListTypeId || 0, {
        isIdField: true,
      }),
      name: new SoeTextFormControl(
        element?.name || '',
        {
          isNameField: true,
          required: true,
          maxLength: 50,
          minLength: 1,
        },
        'common.name'
      ),
      description: new SoeTextFormControl(
        element?.description || '',
        { maxLength: 100 },
        'common.description'
      ),
      isProjectPriceList: new SoeCheckboxFormControl(
        element?.isProjectPriceList,
        {},
        'common.customer.invoices.projectpricelist'
      ),
      inclusiveVat: new SoeCheckboxFormControl(
        element?.inclusiveVat,
        {},
        'common.incvat'
      ),
      currencyId: new SoeSelectFormControl(
        element?.currencyId || 0,
        {
          required: true,
          zeroNotAllowed: true,
        },
        'common.currency'
      ),
      priceLists: new SoeFormControl(
        element?.priceLists || [],
        undefined,
        undefined,
        undefined,
        'billing.products.pricelists.pricelist'
      ),
    });

    this.controls.isProjectPriceList.disable();

    this.onCopy = this.doOnCopy.bind(this);
  }
  public setPriceLists(priceLists: PriceListDTO[]) {
    this.getPriceLists()?.setValue(priceLists);
  }
  public getPriceLists() {
    return this.get(PRICELISTS_KEY);
  }
  public doOnCopy() {
    return {
      priceListType: { ...this.value, [this.getIdFieldName()]: 0 },
      priceListRows:
        this.get(PRICELISTS_KEY)?.value?.map((r: PriceListDTO) => {
          r.priceListTypeId = 0;
          r.priceListId = 0;
          r.isModified = true;
          return r;
        }) || [],
    };
  }
}
