import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { InventoryWriteoffsService } from './inventory-writeoffs.service';

describe('InventoryWriteoffsService', () => {
  let service: InventoryWriteoffsService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(InventoryWriteoffsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
