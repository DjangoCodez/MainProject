import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { EmailTemplateService } from './email-template.service';

describe('EmailTemplateService', () => {
  let service: EmailTemplateService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(EmailTemplateService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
