import { TestBed } from '@angular/core/testing';

import { SearchEinvoiceRecipientService } from './select-einvoice-recipient.service';
import { SoeHttpClient } from '@shared/services/http.service';

describe('SearchEinvoiceRecipientService', () => {
  let service: SearchEinvoiceRecipientService;
  let mockHttpClient: any;

  beforeEach(() => {
    mockHttpClient = {
      post: vi.fn(),
    };

    TestBed.configureTestingModule({
      providers: [
        SearchEinvoiceRecipientService,
        { provide: SoeHttpClient, useValue: mockHttpClient },
      ],
    });
    service = TestBed.inject(SearchEinvoiceRecipientService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
