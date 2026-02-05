import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { SpDialogService } from './sp-dialog.service';

describe('SpDialogService', () => {
  let service: SpDialogService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(SpDialogService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
