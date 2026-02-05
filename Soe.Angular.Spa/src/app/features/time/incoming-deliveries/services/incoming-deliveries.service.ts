import { inject, Injectable } from '@angular/core';
import {
  IIncomingDeliveryGridDTO,
  IIncomingDeliveryHeadDTO,
  IIncomingDeliveryRowDTO,
  IIncomingDeliveryTypeSmallDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ProgressService } from '@shared/services/progress/progress.service'
import { SoeHttpClient } from '@shared/services/http.service';
import { Observable, tap } from 'rxjs';
import {
  deleteIncomingDelivery,
  getIncomingDeliveriesGrid,
  getIncomingDelivery,
  getIncomingDeliveryRows,
  getIncomingDeliveryTypesDict,
  getIncomingDeliveryTypesSmall,
  saveIncomingDelivery,
} from '@shared/services/generated-service-endpoints/time/IncomingDelivery.endpoints';
import { DateUtil } from '@shared/util/date-util'
import { Perform } from '@shared/util/perform.class';
import { getRecurrenceDescription } from '@shared/services/generated-service-endpoints/time/TimeScheduleTask.endpoints';
import { SmallGenericType } from '@shared/models/generic-type.model';

@Injectable({
  providedIn: 'root',
})
export class IncomingDeliveriesService {
  constructor(private http: SoeHttpClient) {}

  // Cached data
  progressService = inject(ProgressService);
  performIncomingDeliveryTypesDict = new Perform<SmallGenericType[]>(
    this.progressService
  );
  performIncomingDeliveryTypesSmall = new Perform<
    IIncomingDeliveryTypeSmallDTO[]
  >(this.progressService);

  getGrid(id?: number): Observable<IIncomingDeliveryGridDTO[]> {
    return this.http
      .get<IIncomingDeliveryGridDTO[]>(getIncomingDeliveriesGrid(id))
      .pipe(
        tap(deliveries => {
          // Format start/end
          // Can be dates or text
          deliveries.map(delivery => {
            if (
              delivery.recurrenceStartsOnDescription &&
              DateUtil.isValidDateOrString(
                delivery.recurrenceStartsOnDescription
              )
            ) {
              delivery.recurrenceStartsOnDescription = new Date(
                delivery.recurrenceStartsOnDescription
              ).toFormattedDate();
            }
            if (
              delivery.recurrenceEndsOnDescription &&
              DateUtil.isValidDateOrString(delivery.recurrenceEndsOnDescription)
            ) {
              delivery.recurrenceEndsOnDescription = new Date(
                delivery.recurrenceEndsOnDescription
              ).toFormattedDate();
            }
          });
        })
      );
  }

  getIncomingDeliveryTypesSmall(): Observable<IIncomingDeliveryTypeSmallDTO[]> {
    return this.http
      .get<IIncomingDeliveryTypeSmallDTO[]>(getIncomingDeliveryTypesSmall())
      .pipe(
        tap(x => {
          this.performIncomingDeliveryTypesSmall.data = x;
        })
      );
  }

  getIncomingDeliveryTypesDict(
    addEmptyRow: boolean
  ): Observable<SmallGenericType[]> {
    return this.http
      .get<SmallGenericType[]>(getIncomingDeliveryTypesDict(addEmptyRow))
      .pipe(
        tap(x => {
          this.performIncomingDeliveryTypesDict.data = x;
        })
      );
  }

  get(id: number): Observable<IIncomingDeliveryHeadDTO> {
    return this.http.get<IIncomingDeliveryHeadDTO>(getIncomingDelivery(id));
  }

  getRows(
    incomingDeliveryHeadId: number
  ): Observable<IIncomingDeliveryRowDTO[]> {
    return this.http.get<any>(getIncomingDeliveryRows(incomingDeliveryHeadId));
  }

  save(model: IIncomingDeliveryHeadDTO): Observable<any> {
    return this.http.post<IIncomingDeliveryHeadDTO>(
      saveIncomingDelivery(),
      model
    );
  }

  delete(id: number): Observable<any> {
    return this.http.delete(deleteIncomingDelivery(id));
  }

  getRecurrenceDescription(pattern: string): Observable<string> {
    return this.http.get(getRecurrenceDescription(pattern));
  }
}
