import { Injectable } from '@angular/core';
import { CoreService } from '@shared/services/core.service';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  getInventoryWriteOffMethodsGrid,
  getInventoryWriteOffMethod,
  getInventoryWriteOffMethodsDict,
  saveInventoryWriteOffMethod,
  deleteInventoryWriteOffMethod,
} from '@shared/services/generated-service-endpoints/economy/InventoryWriteOffMethod.endpoints';
import { Observable, forkJoin } from 'rxjs';
import { InventoryWriteOffMethodDTO } from '../models/inventory-write-off-method.model';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { TermGroup } from '@shared/models/generated-interfaces/Enumerations';
import { map } from 'rxjs/operators';
import { IInventoryWriteOffMethodGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class InventoryWriteOffMethodsService {
  constructor(
    private http: SoeHttpClient,
    private coreService: CoreService
  ) {}

  getGrid(id?: number): Observable<IInventoryWriteOffMethodGridDTO[]> {
    return forkJoin({
      periodTypes: this.coreService.getTermGroupContent(
        TermGroup.InventoryWriteOffMethodPeriodType,
        false,
        false
      ),
      writeOffMethodTypes: this.coreService.getTermGroupContent(
        TermGroup.InventoryWriteOffMethodType,
        false,
        false
      ),
      writeOffMethods: this.http.get<IInventoryWriteOffMethodGridDTO[]>(
        getInventoryWriteOffMethodsGrid(id)
      ),
    }).pipe(
      map(
        ({
          periodTypes,
          writeOffMethodTypes,
          writeOffMethods,
        }): IInventoryWriteOffMethodGridDTO[] => {
          writeOffMethods.forEach(el => {
            el.typeName =
              writeOffMethodTypes?.find(w => w.id == el.type)?.name || '';
            el.periodTypeName =
              periodTypes?.find(t => t.id == el.periodType)?.name || '';
          });

          return writeOffMethods;
        }
      )
    );
  }

  get(id: number): Observable<InventoryWriteOffMethodDTO> {
    return forkJoin({
      periodTypes: this.coreService.getTermGroupContent(
        TermGroup.InventoryWriteOffMethodPeriodType,
        false,
        false
      ),
      writeOffMethodTypes: this.coreService.getTermGroupContent(
        TermGroup.InventoryWriteOffMethodType,
        false,
        false
      ),
      writeOffMethod: this.http.get<InventoryWriteOffMethodDTO>(
        getInventoryWriteOffMethod(id)
      ),
    }).pipe(
      map(
        ({
          periodTypes,
          writeOffMethodTypes,
          writeOffMethod,
        }): InventoryWriteOffMethodDTO => {
          if (writeOffMethod) {
            writeOffMethod.typeName =
              writeOffMethodTypes?.find(w => w.id == writeOffMethod?.type)
                ?.name || '';
            writeOffMethod.periodTypeName =
              periodTypes?.find(t => t.id == writeOffMethod?.periodType)
                ?.name || '';
          }
          return writeOffMethod;
        }
      )
    );
  }

  getDict(addEmptyValue: boolean): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(
      getInventoryWriteOffMethodsDict(addEmptyValue)
    );
  }

  save(model: InventoryWriteOffMethodDTO): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(
      saveInventoryWriteOffMethod(),
      model
    );
  }

  delete(id: number): Observable<BackendResponse> {
    return this.http.delete<BackendResponse>(deleteInventoryWriteOffMethod(id));
  }
}
