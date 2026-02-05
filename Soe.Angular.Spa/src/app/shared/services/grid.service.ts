import { Injectable } from '@angular/core';
import { SaveUserGridStateModel } from '@shared/models/grid.model';
import { Observable } from 'rxjs';
import {
  deleteSysGridState,
  deleteUserGridState,
  getSysGridState,
  getUserGridState,
  saveSysGridState,
  saveUserGridState,
} from './generated-service-endpoints/core/Grid.endpoints';
import { SoeHttpClient } from './http.service';
import { ColumnState } from 'ag-grid-community';

@Injectable({
  providedIn: 'root',
})
export class GridService {
  constructor(private http: SoeHttpClient) {}

  getSysGridState(grid: string): Observable<string> {
    return this.http.get<any>(getSysGridState(grid.toSnakeCase()));
  }

  saveSysGridState(grid: string, state: ColumnState[]): Observable<any> {
    const model = new SaveUserGridStateModel(
      grid.toSnakeCase(),
      JSON.stringify(state)
    );

    return this.http.post<SaveUserGridStateModel>(saveSysGridState(), model);
  }

  deleteSysGridState(grid: string): Observable<any> {
    return this.http.delete<any>(deleteSysGridState(grid.toSnakeCase()));
  }

  getUserGridState(grid: string): Observable<string> {
    return this.http.get<any>(getUserGridState(grid.toSnakeCase()));
  }

  saveUserGridState(grid: string, state: ColumnState[]): Observable<any> {
    const model = new SaveUserGridStateModel(
      grid.toSnakeCase(),
      JSON.stringify(state)
    );

    return this.http.post<SaveUserGridStateModel>(saveUserGridState(), model);
  }

  deleteUserGridState(grid: string): Observable<any> {
    return this.http.delete<any>(deleteUserGridState(grid.toSnakeCase()));
  }
}
