import { inject, Injectable } from '@angular/core';
import { ExternalCompanySearchProvider } from '@shared/models/generated-interfaces/Enumerations';
import { SoeHttpClient } from '@shared/services/http.service';
import { getExternalCompanies } from '@shared/services/generated-service-endpoints/shared/ExternalCompanySearch.endpoints';
import {
  ExternalCompanyGridRow,
  ExternalCompanySearchFilter,
} from '../models/external-company-search-dialog-data.model';
import { map } from 'rxjs';
import { IExternalCompanyResultDTO } from '@shared/models/generated-interfaces/ExternalCompanyResultDTO';

@Injectable({
  providedIn: 'root',
})
export class ExternalCompanySearchService {
  private readonly http = inject(SoeHttpClient);

  public searchCompanies(
    source: ExternalCompanySearchProvider,
    filter: ExternalCompanySearchFilter
  ) {
    return this.http
      .post<IExternalCompanyResultDTO[]>(getExternalCompanies(+source), filter)
      .pipe(map(rows => rows.map(row => new ExternalCompanyGridRow(row))));
  }
}
