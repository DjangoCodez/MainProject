import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { DocumentService } from './document.service';

describe('DocumentService', () => {
  let service: DocumentService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(DocumentService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
