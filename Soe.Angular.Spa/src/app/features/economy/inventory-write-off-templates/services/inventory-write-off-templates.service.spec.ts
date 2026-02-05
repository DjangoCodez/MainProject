import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { InventoryWriteOffTemplatesService } from './inventory-write-off-templates.service';

describe('InventoryWriteOffTemplatesService', () => {
  let service: InventoryWriteOffTemplatesService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(InventoryWriteOffTemplatesService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
