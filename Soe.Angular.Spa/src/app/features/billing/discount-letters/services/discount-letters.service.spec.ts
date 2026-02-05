import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { DiscountLettersService } from './discount-letters.service';

describe('DiscountLettersService', () => {
  let service: DiscountLettersService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(DiscountLettersService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
