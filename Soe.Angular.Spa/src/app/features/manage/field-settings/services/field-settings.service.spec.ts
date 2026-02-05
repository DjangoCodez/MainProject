import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { FieldSettingsService } from './field-settings.service';

describe('FieldSettingsService', () => {
  let service: FieldSettingsService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(FieldSettingsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
