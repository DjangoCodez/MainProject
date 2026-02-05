import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { SpSettingService } from './sp-setting.service';

describe('SpSettingService', () => {
  let service: SpSettingService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(SpSettingService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
