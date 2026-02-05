import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { SpWorkRuleService } from './sp-work-rule.service';

describe('SpWorkRuleService', () => {
  let service: SpWorkRuleService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(SpWorkRuleService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
