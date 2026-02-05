import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { ProjectCentralService } from './project-central.service';

describe('ProjectCentralService', () => {
  let service: ProjectCentralService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(ProjectCentralService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
