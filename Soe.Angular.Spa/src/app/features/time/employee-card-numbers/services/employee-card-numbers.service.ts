import { Injectable } from '@angular/core';
import { SoeHttpClient } from '@shared/services/http.service';
import { Observable } from 'rxjs';
import {
  cardNumberExists,
  deleteCardNumber,
  getCardNumbers,
} from '@shared/services/generated-service-endpoints/time/CardNumber.endpoints';
import { ICardNumberGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

@Injectable({
  providedIn: 'root',
})
export class EmployeeCardNumbersService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<ICardNumberGridDTO[]> {
    return this.http.get<ICardNumberGridDTO[]>(getCardNumbers());
  }

  cardNumberExists(
    cardNumber: string,
    excludeEmployeeId: number
  ): Observable<ICardNumberGridDTO> {
    return this.http.get<ICardNumberGridDTO>(
      cardNumberExists(cardNumber, excludeEmployeeId)
    );
  }

  delete(id: number): Observable<any> {
    return this.http.delete(deleteCardNumber(id));
  }
}
