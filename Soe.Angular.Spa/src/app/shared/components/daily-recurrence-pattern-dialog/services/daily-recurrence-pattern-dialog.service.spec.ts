import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { DailyRecurrencePatternDialogService } from './daily-recurrence-pattern-dialog.service';

describe('DailyRecurrencePatternDialogService', () => {
  let service: DailyRecurrencePatternDialogService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(DailyRecurrencePatternDialogService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
