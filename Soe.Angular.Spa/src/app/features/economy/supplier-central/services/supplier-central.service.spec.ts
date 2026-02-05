import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { SupplierCentralService } from './supplier-central.service';

describe('SupplierCentralService', () => {
  let service: SupplierCentralService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(SupplierCentralService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
