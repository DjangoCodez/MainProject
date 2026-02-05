import { Injectable } from '@angular/core';
import { SoeHttpClient } from '@shared/services/http.service';
import { Observable } from 'rxjs';
import {
  getExcelImportGrid,
  importExcelFile,
} from '@shared/services/generated-service-endpoints/shared/ImportV2.endpoints';
import { IExcelImportTemplateDTO } from '@shared/models/generated-interfaces/Excel';
import { ExcelImportDTO } from '../model/excel-import.model';

@Injectable({
  providedIn: 'root',
})
export class ExcelImportService {
  constructor(private http: SoeHttpClient) {}

  getGrid(): Observable<IExcelImportTemplateDTO[]> {
    return this.http.get<IExcelImportTemplateDTO[]>(getExcelImportGrid());
  }

  importFile(model: ExcelImportDTO): Observable<any> {
    return this.http.post(importExcelFile(), model);
  }
}
