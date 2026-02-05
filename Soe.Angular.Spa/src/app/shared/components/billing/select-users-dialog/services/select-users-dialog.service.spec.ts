import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { SelectUsersDialogService } from './select-users-dialog.service';

describe('SelectUsersDialogService', () => {
  let service: SelectUsersDialogService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(SelectUsersDialogService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
