import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { InventoryWriteOffMethodsService } from './inventory-write-off-methods.service';

describe('InvoiceWriteOffMethodsService', () => {
  let service: InventoryWriteOffMethodsService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(InventoryWriteOffMethodsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
