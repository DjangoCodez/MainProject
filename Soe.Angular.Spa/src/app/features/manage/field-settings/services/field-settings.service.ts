import { Injectable } from '@angular/core';
import {
  IFieldSettingGridDTO,
  IFieldSettingDTO,
} from '@shared/models/generated-interfaces/FieldSettingDTO';
import {
  getFieldSettings,
  saveFieldSetting,
} from '@shared/services/generated-service-endpoints/manage/FieldSetting.endpoints';
import { SoeHttpClient } from '@shared/services/http.service';
import { map } from 'rxjs';
import { Observable } from 'rxjs/internal/Observable';

@Injectable({
  providedIn: 'root',
})
export class FieldSettingsService {
  constructor(private http: SoeHttpClient) {}

  getGridAdditionalProps = {
    type: 0,
  };

  getGrid(
    fieldId: number | undefined,
    additionalProps?: {
      type: number;
    }
  ): Observable<IFieldSettingGridDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;
    return this.http.get<IFieldSettingGridDTO[]>(
      getFieldSettings(this.getGridAdditionalProps.type, fieldId)
    );
  }

  get(type: number, fieldId: number): Observable<IFieldSettingDTO> {
    return this.http
      .get<IFieldSettingDTO[]>(getFieldSettings(type, fieldId))
      .pipe(map(settings => settings[0] || ({} as IFieldSettingDTO)));
  }

  save(model: IFieldSettingDTO): Observable<any> {
    return this.http.post<IFieldSettingDTO>(saveFieldSetting(), model);
  }

  delete(id: number): Observable<any> {
    return new Observable<any>();
  }
}
