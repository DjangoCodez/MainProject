/* tslint:disable:no-unused-variable */

import { TestBed, async, inject } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { ProductsUnitConversionService } from './products-unit-conversion-dialog.service';

describe('Service: ProductsUnitConversion', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],
      providers: [ProductsUnitConversionService],
    });
  });

  it('should ...', inject(
    [ProductsUnitConversionService],
    (service: ProductsUnitConversionService) => {
      expect(service).toBeTruthy();
    }
  ));
});
