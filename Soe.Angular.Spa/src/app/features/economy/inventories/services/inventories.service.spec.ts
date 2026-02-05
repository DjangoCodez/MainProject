import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { InventoriesService } from './inventories.service';

describe('InventoriesService', () => {
  let service: InventoriesService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(InventoriesService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
