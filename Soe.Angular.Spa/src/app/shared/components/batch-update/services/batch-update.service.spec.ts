/* tslint:disable:no-unused-variable */

import { TestBed, async, inject } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { BatchUpdateService } from './batch-update.service';

describe('Service: BatchUpdate', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],
      providers: [BatchUpdateService]
    });
  });

  it('should ...', inject([BatchUpdateService], (service: BatchUpdateService) => {
    expect(service).toBeTruthy();
  }));
});
