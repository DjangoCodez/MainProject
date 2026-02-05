import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { PlacementsService } from './placements.service';

describe('PlacementsService', () => {
  let service: PlacementsService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(PlacementsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
