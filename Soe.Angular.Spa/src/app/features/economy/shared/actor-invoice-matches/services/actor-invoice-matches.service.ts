import { Injectable } from '@angular/core';
import { ActorInvoiceMatchesFilterDTO } from '@features/economy/shared/actor-invoice-matches/models/actor-invoice-matches-filter-dto.model';
import { IInvoiceMatchingDTO } from '@shared/models/generated-interfaces/InvoiceMatchingDTO';
import { SoeHttpClient } from '@shared/services/http.service';
import { Observable } from 'rxjs';
import { searchInvoicesPaymentsAndMatches } from '@shared/services/generated-service-endpoints/economy/InvoiceMatches.endpoints';	

@Injectable()
export class ActorInvoiceMatchesService {

  constructor(private readonly http: SoeHttpClient) {}

  getGrid(
    id?: number,
    additionalProps?: {
      filter: ActorInvoiceMatchesFilterDTO
    }
  ): Observable<IInvoiceMatchingDTO[]> {

    return this.http.post<IInvoiceMatchingDTO[]>(
      searchInvoicesPaymentsAndMatches(),
      additionalProps!.filter
    );
    
  }
  
}
