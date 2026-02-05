import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import {
  ExportDefinitionDTO,
  ExportDefinitionGridDTO,
} from '../../../models/export.model';
import {
  getExportDefinition,
  getExportDefinitionsGrid,
  saveExportDefinition,
} from '@shared/services/generated-service-endpoints/shared/Export.endpoints';
import { SoeHttpClient } from '@shared/services/http.service';

@Injectable({
  providedIn: 'root',
})
export class ExportStandardDefinitionsService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<ExportDefinitionGridDTO[]> {
    return this.http
      .get<ExportDefinitionGridDTO[]>(getExportDefinitionsGrid(id))
      .pipe(
        map(datas =>
          datas.map(data => {
            const obj = new ExportDefinitionGridDTO();
            Object.assign(obj, data);
            return obj;
          })
        )
      );
  }

  get(id: number): Observable<ExportDefinitionDTO> {
    return this.http.get<ExportDefinitionDTO>(getExportDefinition(id)).pipe(
      map(data => {
        const obj = new ExportDefinitionDTO();
        Object.assign(obj, data);
        return obj;
      })
    );
  }

  save(model: ExportDefinitionDTO): Observable<any> {
    return this.http.post<ExportDefinitionDTO>(saveExportDefinition(), model);
  }

  delete(id: number): Observable<any> {
    // TODO: Finish the WebApi endpoint on serverside.
    return new Observable(); //this.http.delete(deleteExportDefinition(id));
  }
}
