import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { MatchCodeService } from './match-codes.service';

describe('MatchCodeService', () => {
  let service: MatchCodeService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(MatchCodeService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
