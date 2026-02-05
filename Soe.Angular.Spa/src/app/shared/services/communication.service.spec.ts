import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { CommunicationService } from './communication.service';

describe('CommunicationService', () => {
  let service: CommunicationService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(CommunicationService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
