import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { PricelistTypeDialogService } from './pricelist-type-dialog.service';

describe('PricelistTypeDialogService', () => {
  let service: PricelistTypeDialogService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(PricelistTypeDialogService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
