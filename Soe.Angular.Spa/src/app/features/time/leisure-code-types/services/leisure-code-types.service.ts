import { Injectable } from '@angular/core';
import { TermGroup } from '@shared/models/generated-interfaces/Enumerations';
import {
  ITimeLeisureCodeDTO,
  ITimeLeisureCodeGridDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { SoeHttpClient } from '@shared/services/http.service';
import { getTermGroupContent } from '@shared/services/generated-service-endpoints/core/Term.endpoints';
import {
  deleteTimeLeisureCode,
  getTimeLeisureCode,
  getTimeLeisureCodesGrid,
  saveTimeLeisureCode,
} from '@shared/services/generated-service-endpoints/time/TimeLeisureCode.endpoints';
import { tap } from 'rxjs/operators';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class LeisureCodeTypesService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<ITimeLeisureCodeGridDTO[]> {
    return this.http.get<ITimeLeisureCodeGridDTO[]>(
      getTimeLeisureCodesGrid(id)
    );
  }

  get(id: number): Observable<ITimeLeisureCodeDTO> {
    return this.http.get<ITimeLeisureCodeDTO>(getTimeLeisureCode(id));
  }

  save(model: ITimeLeisureCodeDTO): Observable<any> {
    return this.http.post<ITimeLeisureCodeDTO>(saveTimeLeisureCode(), model);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(deleteTimeLeisureCode(id));
  }

  getTypes(): Observable<SmallGenericType[]> {
    return this.http
      .get<
        SmallGenericType[]
      >(getTermGroupContent(TermGroup.TimeLeisureCodeType, false, false, false))
      .pipe(
        tap(data => {
          data.sort((a, b) => {
            if (a.id < b.id) {
              return -1;
            }
            if (a.id > b.id) {
              return 1;
            }
            return 0;
          });
        })
      );
  }
}
