import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { SkillTypesService } from './skill-types.service';

describe('SkillTypesService', () => {
  let service: SkillTypesService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(SkillTypesService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
