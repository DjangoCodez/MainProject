import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { EInvoiceRecipientSearchDTO } from '../models/search-einvoice-recipient-dialog.model';
import { getEInvoiceRecipients } from '@shared/services/generated-service-endpoints/shared/CustomerV2.endpoints';
import { SoeHttpClient } from '@shared/services/http.service';

@Injectable({
  providedIn: 'root',
})
export class SearchEinvoiceRecipientService {
  constructor(private http: SoeHttpClient) {}

  getRecipientsBySearch(
    searchDto: EInvoiceRecipientSearchDTO
  ): Observable<EInvoiceRecipientSearchDTO[]> {
    return this.http.post<EInvoiceRecipientSearchDTO[]>(
      getEInvoiceRecipients(),
      searchDto
    );
  }
}
