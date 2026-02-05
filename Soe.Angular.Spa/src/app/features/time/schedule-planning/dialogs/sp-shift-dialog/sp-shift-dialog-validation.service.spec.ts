import { TestBed } from '@angular/core/testing';

import { SpShiftDialogValidationService } from './sp-shift-dialog-validation.service';

describe('SpShiftDialogValidationService', () => {
  let service: SpShiftDialogValidationService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SpShiftDialogValidationService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
