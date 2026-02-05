import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { MarkupService } from './markup.service';

describe('MarkupService', () => {
  let service: MarkupService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(MarkupService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
