import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { ExtraFieldsService } from './extra-fields.service';

describe('ExtraFieldsService', () => {
  let service: ExtraFieldsService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(ExtraFieldsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
