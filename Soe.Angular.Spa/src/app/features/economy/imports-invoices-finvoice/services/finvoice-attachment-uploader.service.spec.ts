/* tslint:disable:no-unused-variable */

import { TestBed, inject } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { FInvoiceAttachmentUploaderService } from './finvoice-attachment-uploader.service';

describe('Service: FinvoiceAttachmentUploader.service', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],
      providers: [FInvoiceAttachmentUploaderService],
    });
  });

  it('should ...', inject(
    [FInvoiceAttachmentUploaderService],
    (service: FInvoiceAttachmentUploaderService) => {
      expect(service).toBeTruthy();
    }
  ));
});
