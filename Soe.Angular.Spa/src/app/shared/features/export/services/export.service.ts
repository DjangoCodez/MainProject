import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import {
  deleteExport,
  getExport,
  getExportDefinitionsDict,
  getExportsGrid,
  saveExport,
} from '@shared/services/generated-service-endpoints/shared/Export.endpoints';
import { SoeModule } from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { SoeHttpClient } from '@shared/services/http.service';
import { ExportDTO } from '../models/export.model';
import { IExportGridDTO } from '@shared/models/generated-interfaces/ExportDTO';

@Injectable({
  providedIn: 'root',
})
export class ExportService {
  constructor(private http: SoeHttpClient) {}

  getGridAdditionalProps = {
    module: SoeModule.None,
  };
  getGrid(
    id?: number,
    additionalProps?: { module: SoeModule }
  ): Observable<IExportGridDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;
    return this.http.get<IExportGridDTO[]>(
      getExportsGrid(this.getGridAdditionalProps.module, id)
    );
  }

  getDefinitions(addEmptyRow: boolean): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(
      getExportDefinitionsDict(addEmptyRow)
    );
  }

  get(id: number): Observable<ExportDTO> {
    return this.http.get<ExportDTO>(getExport(id)).pipe(
      map((x: ExportDTO) => {
        const obj = new ExportDTO();
        Object.assign(obj, x);
        return obj;
      })
    );
  }

  save(model: ExportDTO): Observable<any> {
    return this.http.post<ExportDTO>(saveExport(), model);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(deleteExport(id));
  }
}
