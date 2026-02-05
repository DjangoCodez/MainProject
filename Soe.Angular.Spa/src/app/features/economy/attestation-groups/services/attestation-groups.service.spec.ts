import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { AttestationGroupsService } from './attestation-groups.service';

describe('AttestationGroupsService', () => {
  let service: AttestationGroupsService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(AttestationGroupsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
