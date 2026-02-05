import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { ContactPersonsService } from './contact-persons.service';

describe('ContactPersonsService', () => {
  let service: ContactPersonsService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(ContactPersonsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
