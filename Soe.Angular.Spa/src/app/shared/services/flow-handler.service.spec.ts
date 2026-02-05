import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { FlowHandlerService } from './flow-handler.service';

describe('FlowHandlerService', () => {
  let service: FlowHandlerService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(FlowHandlerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
