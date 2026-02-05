import { Injectable } from '@angular/core';
import { SoeHttpClient } from '@shared/services/http.service';
import { UiComponentsTestDTO } from '../models/ui-components-test.model';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class UiComponentsTestService {
  getAllTest = () => ``;
  getTest = (id: number) => ``;
  saveTest = () => ``;
  deleteTest = (id: number) => ``;

  constructor(private http: SoeHttpClient) {}

  getAll(): Observable<UiComponentsTestDTO[]> {
    return this.http.get<UiComponentsTestDTO[]>(this.getAllTest());
  }

  get(id: number): Observable<UiComponentsTestDTO> {
    return this.http.get<UiComponentsTestDTO>(this.getTest(id));
  }

  save(model: UiComponentsTestDTO): Observable<any> {
    return this.http.post<UiComponentsTestDTO>(this.saveTest(), model);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(this.deleteTest(id));
  }
}
