import { Injectable } from '@angular/core';
import { SoeHttpClient } from './http.service';
import { getNbrOfUnreadMessages } from './generated-service-endpoints/core/Communication.endpoints';
import { Observable } from 'rxjs';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';

@Injectable({
  providedIn: 'root',
})
export class CommunicationService {
  constructor(private http: SoeHttpClient) {}

  getNbrOfUnreadMessages(): Observable<number> {
    return this.http.get<number>(getNbrOfUnreadMessages());
  }
}
