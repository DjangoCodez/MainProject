import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { TextBlockDialogService } from './text-block-dialog.service';

describe('TextBlockDialogService', () => {
  let service: TextBlockDialogService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(TextBlockDialogService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
