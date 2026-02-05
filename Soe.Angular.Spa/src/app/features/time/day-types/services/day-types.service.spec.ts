import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { DayTypesService } from './day-types.service';

describe('DayTypesService', () => {
  let service: DayTypesService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(DayTypesService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
