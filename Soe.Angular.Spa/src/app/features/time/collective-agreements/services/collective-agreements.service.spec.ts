import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { CollectiveAgreementsService } from './collective-agreements.service';

describe('CollectiveAgreementsService', () => {
  let service: CollectiveAgreementsService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(CollectiveAgreementsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
