import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { LanguageTranslationsService } from './language-translations.service';

describe('LanguageTranslationsService', () => {
  let service: LanguageTranslationsService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],});
    service = TestBed.inject(LanguageTranslationsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
