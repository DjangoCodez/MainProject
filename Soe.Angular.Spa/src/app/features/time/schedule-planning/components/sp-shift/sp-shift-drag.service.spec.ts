import { TestBed } from '@angular/core/testing';

import { SpShiftDragService } from './sp-shift-drag.service';

describe('SpShiftDragService', () => {
  let service: SpShiftDragService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SpShiftDragService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
