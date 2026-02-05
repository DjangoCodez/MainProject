/* tslint:disable:no-unused-variable */

import { TestBed, async, inject } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { AccountingPeriodSelectionService } from './accounting-period-selection.service';

describe('Service: AccountingPeriodSelection', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],
      providers: [AccountingPeriodSelectionService]
    });
  });

  it('should ...', inject([AccountingPeriodSelectionService], (service: AccountingPeriodSelectionService) => {
    expect(service).toBeTruthy();
  }));
});
