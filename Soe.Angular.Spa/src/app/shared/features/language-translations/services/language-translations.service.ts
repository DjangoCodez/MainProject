import { inject, Injectable } from '@angular/core';
import { ICompTermDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import { getTranslations } from '@shared/services/generated-service-endpoints/core/Term.endpoints';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class LanguageTranslationsService {
  private readonly http = inject(SoeHttpClient);

  getTranslations(
    recordType: number,
    recordId: number,
    loadLangName: boolean
  ): Observable<ICompTermDTO[]> {
    return this.http.get<ICompTermDTO[]>(
      getTranslations(recordType, recordId, loadLangName)
    );
  }
}
