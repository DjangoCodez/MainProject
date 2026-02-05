import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { CurrenciesService } from './currencies.service';

describe('MatchCodeService', () => {
  let service: CurrenciesService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(CurrenciesService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
