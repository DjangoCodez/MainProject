import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { SpSlotService } from './sp-slot.service';

describe('SpSlotService', () => {
  let service: SpSlotService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(SpSlotService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
