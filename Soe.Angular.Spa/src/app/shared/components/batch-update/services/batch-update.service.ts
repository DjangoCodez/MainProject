import { Injectable } from '@angular/core';
import { SoeEntityType } from '@shared/models/generated-interfaces/Enumerations';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  getBatchUpdateForEntity,
  getContactAddressItemsDict,
  performBatchUpdate,
  refreshBatchUpdateOptions,
} from '@shared/services/generated-service-endpoints/core/BatchUpdate.endpoints';
import { Observable, map } from 'rxjs';
import {
  BatchUpdateDTO,
  PerformBatchUpdateModel,
  RefreshBatchUpdateOptionsModel,
} from '../models/batch-update.model';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class BatchUpdateService {
  constructor(private http: SoeHttpClient) {}

  getBatchUpdateForEntity(
    entityType: SoeEntityType
  ): Observable<BatchUpdateDTO[]> {
    return this.http
      .get<BatchUpdateDTO[]>(getBatchUpdateForEntity(entityType))
      .pipe(
        map(batchUpdats => {
          return batchUpdats.map(batchUpdate => {
            const obj = this.getObject(BatchUpdateDTO, batchUpdate);
            obj.added = false;
            return obj;
          });
        })
      );
  }

  refreshBatchUpdateOptions(
    model: RefreshBatchUpdateOptionsModel
  ): Observable<BatchUpdateDTO> {
    return this.http.post<BatchUpdateDTO>(refreshBatchUpdateOptions(), model);
  }

  getBatchUpdateFilterOptions(
    entityType: SoeEntityType
  ): Observable<SmallGenericType[]> {
    return this.http.get<SmallGenericType[]>(
      getContactAddressItemsDict(entityType)
    );
  }

  performBatchUpdate(
    model: PerformBatchUpdateModel
  ): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(performBatchUpdate(), model);
  }

  private getObject<T extends object>(
    TargetType: { new (): T },
    source: unknown
  ): T {
    const obj = new TargetType();
    Object.assign(obj, source);
    return obj;
  }
}
