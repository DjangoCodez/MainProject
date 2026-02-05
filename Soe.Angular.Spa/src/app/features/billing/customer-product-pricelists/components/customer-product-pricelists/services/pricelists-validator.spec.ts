import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { PriceListsValidatorService } from './pricelists-validator.service';
import { CustomerProductPriceListsService } from './customer-product-pricelists.service';
import { PriceListDTO } from '@features/billing/models/pricelist.model';

describe('PriceListsValidatorService', () => {
  let validator: PriceListsValidatorService;
  let handler: CustomerProductPriceListsService;

  const getRows = (): PriceListDTO[] => [
    getDefaultDTO(),
    getDefaultDTO(),
    getDefaultDTO(),
  ];

  const getDefaultDTO = (): PriceListDTO => {
    return {
      productId: 0,
      priceListTypeId: 0,
      startDate: new Date(),
      stopDate: new Date(),
      price: 0,
      isModified: false,
      priceListId: 0,
      sysPriceListTypeName: '',
      quantity: 0,
      createdBy: '',
      modifiedBy: '',
      state: 0,
      // Add the missing properties here
    };
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],
      providers: [CustomerProductPriceListsService, PriceListsValidatorService],
    });
    validator = TestBed.inject(PriceListsValidatorService);
    handler = TestBed.inject(CustomerProductPriceListsService);
  });

  it('Date validation', () => {
    const row = getDefaultDTO();

    // row.startDate = undefined;
    row.startDate = new Date();
    expect(validator.validDates(row)).toBeTruthy();

    // row.startDate = undefined;
    row.stopDate = new Date();
    expect(validator.validDates(row)).toBeTruthy();

    row.startDate = new Date('2020-01-01');
    row.stopDate = new Date('2020-01-02');
    expect(validator.validDates(row)).toBeTruthy();

    row.startDate = new Date('2020-01-02');
    row.stopDate = new Date('2020-01-01');
    expect(validator.validDates(row)).toBeFalsy();
  });

  it('Product validation', () => {
    const row = getDefaultDTO();

    row.productId = 0;
    expect(validator.validProduct(true, row)).toBeFalsy();
    expect(validator.validProduct(false, row)).toBeTruthy();

    row.productId = 1;
    expect(validator.validProduct(true, row)).toBeTruthy();
  });

  it('Price list type validation', () => {
    const row = getDefaultDTO();

    row.priceListTypeId = 0;
    expect(validator.validPriceListType(true, row)).toBeFalsy();
    expect(validator.validPriceListType(false, row)).toBeTruthy();

    row.priceListTypeId = 1;
    expect(validator.validPriceListType(true, row)).toBeTruthy();
  });

  it('Unique condition default', () => {
    expect(validator.uniqueCondition(getRows(), false, false)).toBeFalsy();
  });

  it('Unique condition product', () => {
    const rowsDifferentProducts = getRows().map((r, i) => {
      r.productId = i + 1;
      return r;
    });
    expect(
      validator.uniqueCondition(rowsDifferentProducts, false, true)
    ).toBeTruthy();
  });

  it('Unique condition type', () => {
    const rowsDifferentType = getRows().map((r, i) => {
      r.priceListTypeId = i + 1;
      return r;
    });
    expect(
      validator.uniqueCondition(rowsDifferentType, true, false)
    ).toBeTruthy();
  });

  it('Unique condition date', () => {
    const rowsDifferentType = getRows().map((r, i) => {
      r.priceListTypeId = i + 1;
      r.startDate = new Date();
      return r;
    });
    expect(
      validator.uniqueCondition(rowsDifferentType, true, false)
    ).toBeFalsy();
  });

  it('Unique condition date2', () => {
    const start = new Date();
    const rowsDifferentType = getRows().map((r, i) => {
      r.priceListTypeId = i + 1;
      r.startDate = new Date();
      r.startDate.setDate(start.getDate() + i);
      return r;
    });
    expect(
      validator.uniqueCondition(rowsDifferentType, true, false)
    ).toBeTruthy();
  });
});
