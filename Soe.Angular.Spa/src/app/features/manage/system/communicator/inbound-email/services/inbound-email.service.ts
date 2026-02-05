import { inject, Injectable } from '@angular/core';
import {
  IIncomingEmailDTO,
  IIncomingEmailFilterDTO,
  IIncomingEmailGridDTO,
} from '@shared/models/generated-interfaces/IncomingEmailDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  getIncomingEmail,
  getIncomingEmailAttachment,
  getIncomingEmails,
} from '../../../../../../shared/services/generated-service-endpoints/manage/System.endpoints';
import { map, Observable, of } from 'rxjs';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class InboundEmailService {
  private readonly http = inject(SoeHttpClient);

  private deliveryStatusOptions: ISmallGenericType[] = [
    { id: 0, name: 'New' },
    { id: 1, name: 'Valid Domain' },
    { id: 2, name: 'Sent to Queue' },
    { id: 10, name: 'Success - Delivered to API Internal' },
    { id: 11, name: 'Success - Forwarded Email' },
    { id: 12, name: 'Success - No Action' },
    { id: 20, name: 'Error - Invalid Domain' },
    { id: 21, name: 'Error - Sys Lookup' },
    { id: 22, name: 'Error - Add to Queue' },
    { id: 23, name: 'Error - Send to API Internal' },
    { id: 24, name: 'Error - Forward Email' },
    { id: 40, name: 'Dead Lettered - Too Many Attempts' },
  ];

  getGrid(
    id?: number,
    additionalPros?: { filter: IIncomingEmailFilterDTO }
  ): Observable<IIncomingEmailGridDTO[]> {
    if (!additionalPros || !additionalPros.filter) return of([]);

    return this.http
      .post(getIncomingEmails(), additionalPros?.filter)
      .pipe(map(emails => <IIncomingEmailGridDTO[]>emails ?? []));
  }

  get(id: number): Observable<IIncomingEmailDTO> {
    return this.http.get<IIncomingEmailDTO>(getIncomingEmail(id));
  }

  getDeliveryStatusOptions(): Observable<ISmallGenericType[]> {
    return of(this.deliveryStatusOptions);
  }

  getAttachement(id: number): Observable<BackendResponse> {
    return this.http.get<BackendResponse>(getIncomingEmailAttachment(id));
  }
}
