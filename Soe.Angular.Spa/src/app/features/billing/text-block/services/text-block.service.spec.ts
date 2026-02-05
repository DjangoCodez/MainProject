import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { TextBlockService } from './text-block.service';

describe('TextBlockService', () => {
  let service: TextBlockService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(TextBlockService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
