/* tslint:disable:no-unused-variable */

import { TestBed, async, inject } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { SearchInvoiceProductDialogServiceService } from './search-invoice-product-dialog.service';

describe('Service: SearchInvoiceProductDialogService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],
      providers: [SearchInvoiceProductDialogServiceService],
    });
  });

  it('should ...', inject(
    [SearchInvoiceProductDialogServiceService],
    (service: SearchInvoiceProductDialogServiceService) => {
      expect(service).toBeTruthy();
    }
  ));
});
