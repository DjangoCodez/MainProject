import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { SelectProjectService } from './select-project.service';

describe('SelectProjectService', () => {
  let service: SelectProjectService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(SelectProjectService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
