import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { ManageService } from './manage.service';

describe('ManageService', () => {
  let service: ManageService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(ManageService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
