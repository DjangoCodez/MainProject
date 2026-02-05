import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { SpGraphicsService } from './sp-graphics.service';

describe('SpGraphicsService', () => {
  let service: SpGraphicsService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(SpGraphicsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
