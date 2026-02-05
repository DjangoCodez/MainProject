import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { PositionsService } from './positions.service';

describe('PositionsService', () => {
  let service: PositionsService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(PositionsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
