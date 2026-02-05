import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { ProjectBudgetService } from './project-budget.service';

describe('ProjectBudgetService', () => {
  let service: ProjectBudgetService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(ProjectBudgetService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
