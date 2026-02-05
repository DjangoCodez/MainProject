/* tslint:disable:no-unused-variable */

import { TestBed, inject } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { FInvoiceFileUploaderService } from './finvoice-file-uploader.service';

describe('Service: FinvoiceFileUploader', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],
      providers: [FInvoiceFileUploaderService],
    });
  });

  it('should ...', inject(
    [FInvoiceFileUploaderService],
    (service: FInvoiceFileUploaderService) => {
      expect(service).toBeTruthy();
    }
  ));
});
