import { Injectable } from '@angular/core';
import { IUpdateEntityStatesModel } from '@shared/models/generated-interfaces/CoreModels';
import { SoeTimeCodeType } from '@shared/models/generated-interfaces/Enumerations';
import {
  ITimeCodeGridDTO,
  ITimeCodeSaveDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import {
  deleteTimeCode,
  getTimeCode,
  getTimeCodesGrid,
  saveTimeCode,
  updateTimeCodeState,
} from '@shared/services/generated-service-endpoints/time/TimeCode.endpoints';
import { SoeHttpClient } from '@shared/services/http.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class TimeCodeAdditionDeductionService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<ITimeCodeGridDTO[]> {
    const onlyActive: boolean = false;
    const loadPayrollProducts: boolean = true;

    return this.http.get<ITimeCodeGridDTO[]>(
      getTimeCodesGrid(
        SoeTimeCodeType.AdditionDeduction,
        onlyActive,
        loadPayrollProducts,
        id
      )
    );
  }

  get(timeCodeId: number): Observable<ITimeCodeSaveDTO> {
    const loadInvoiceProducts: boolean = true;
    const loadPayrollProducts: boolean = true;
    const loadTimeCodeDeviationCauses: boolean = false;
    const loadEmployeeGroups: boolean = false;

    return this.http.get<ITimeCodeSaveDTO>(
      getTimeCode(
        SoeTimeCodeType.AdditionDeduction,
        timeCodeId,
        loadInvoiceProducts,
        loadPayrollProducts,
        loadTimeCodeDeviationCauses,
        loadEmployeeGroups
      )
    );
  }

  save(model: any): Observable<any> {
    return this.http.post<any>(saveTimeCode(), model);
  }

  delete(timeCodeId: number): Observable<any> {
    return this.http.delete(deleteTimeCode(timeCodeId));
  }

  updateTimeCodeState(model: IUpdateEntityStatesModel): Observable<any> {
    return this.http.post<IUpdateEntityStatesModel>(
      updateTimeCodeState(),
      model
    );
  }
}
