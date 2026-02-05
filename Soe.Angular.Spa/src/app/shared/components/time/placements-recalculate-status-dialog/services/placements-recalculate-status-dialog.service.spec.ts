import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { PlacementsRecalculateStatusDialogService } from './placements-recalculate-status-dialog.service';

describe('PlacementRecalculateStatusDialogService', () => {
  let service: PlacementsRecalculateStatusDialogService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(PlacementsRecalculateStatusDialogService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
