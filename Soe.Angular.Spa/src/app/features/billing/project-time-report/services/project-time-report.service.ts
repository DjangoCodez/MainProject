import { Injectable } from '@angular/core';
import {
  IEmployeeScheduleTransactionInfoDTO,
  IProjectTimeBlockSaveDTO,
  ITimeDeviationCauseDTO,
  IValidateProjectTimeBlockSaveDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import { getCategoriesGrid } from '@shared/services/generated-service-endpoints/time/EmployeeCategory.endpoints';
import { Observable, of } from 'rxjs';
import { getTimeDeviationCauses } from '@shared/services/generated-service-endpoints/time/TimeDeviationCause.endpoints';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  getEmployeeChildren,
  getEmployeeFirstEligibleTime,
  getEmployeeScheduleAndTransactionInfo,
  getEmployeesForProject,
  getEmployeesForTimeProjectRegistrationSmall,
  getProjectsForTimeSheet,
  getProjectsForTimeSheetEmployees,
  getTimeBlocksForTimeSheetFiltered,
  moveTimeRowsToDate,
  moveTimeRowsToOrder,
  moveTimeRowsToOrderRow,
  recalculateWorkTime,
  saveNotesForProjectTimeBlock,
  saveProjectTimeBlockSaveDTO,
  validateProjectTimeBlockSaveDTO,
} from '@shared/services/generated-service-endpoints/billing/ProjectTime.endpoints';
import {
  saveAttestForTransactions,
  saveAttestForTransactionsValidation,
} from '@shared/services/generated-service-endpoints/time/AttestTime.endpoints';
import { getEmployeeForUserWithTimeCode } from '@shared/services/generated-service-endpoints/time/EmployeeV2.endpoints';
import {
  ISaveAttestForTransactionsModel,
  ISaveAttestForTransactionsValidationModel,
} from '@shared/models/generated-interfaces/TimeModels';
import {
  IEmployeeProjectInvoiceDTO,
  IProjectSmallDTO,
} from '@shared/models/generated-interfaces/ProjectDTOs';
import { IGetProjectEmployeesModel } from '@shared/models/generated-interfaces/BillingModels';
import {
  IEmployeeTimeCodeDTO,
  ProjectTimeBlockDTO,
  ProjectTimeBlockSaveDTO,
} from '../models/project-time-report.model';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import {
  IGetProjectTimeBlocksForTimesheetModel,
  IMoveProjectTimeBlocksToDateModel,
  IMoveProjectTimeBlocksToOrderModel,
} from '@shared/models/generated-interfaces/CoreModels';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class ProjectTimeReportService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<ProjectTimeBlockDTO[]> {
    return of([]);
  }

  getProjectsForTimeSheetEmployees(
    empIds: number[],
    projectId?: number
  ): Observable<IEmployeeProjectInvoiceDTO[]> {
    return this.http.get<IEmployeeProjectInvoiceDTO[]>(
      getProjectsForTimeSheetEmployees(empIds, projectId)
    );
  }

  getEmployeesForProjectTimeCode(
    model: IGetProjectEmployeesModel
  ): Observable<IEmployeeTimeCodeDTO[]> {
    return this.http.post<IEmployeeTimeCodeDTO[]>(
      getEmployeesForProject(),
      model
    );
  }

  getEmployeeChildren(employeeId: number): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(getEmployeeChildren(employeeId));
  }

  getProjectsForTimeSheet(employeeId: number): Observable<IProjectSmallDTO[]> {
    return this.http.get<IProjectSmallDTO[]>(
      getProjectsForTimeSheet(employeeId)
    );
  }

  setNotesIcon(item: ProjectTimeBlockDTO) {
    if (item.internalNote || item.externalNote) {
      item.noteIcon = 'file-alt';
    } else {
      item.noteIcon = 'file';
    }
  }

  //#region TimeDeviationCause
  getTimeDeviationCauses(
    employeeGroupId: number,
    getEmployeeGroups: boolean,
    onlyUseInTimeTerminal: boolean
  ): Observable<ITimeDeviationCauseDTO[]> {
    return this.http.get<ITimeDeviationCauseDTO[]>(
      getTimeDeviationCauses(
        employeeGroupId,
        getEmployeeGroups,
        onlyUseInTimeTerminal
      )
    );
  }
  //#endregion

  //#region EmployeeCategory
  getEmployeeCategory(
    soeCategoryTypeId: number,
    loadCompanyCategoryRecord: boolean,
    loadChildren: boolean,
    loadCategoryGroups: boolean
  ): Observable<SmallGenericType[]> {
    return this.http.get<SmallGenericType[]>(
      getCategoriesGrid(
        soeCategoryTypeId,
        loadCompanyCategoryRecord,
        loadChildren,
        loadCategoryGroups
      )
    );
  }
  //#endregion

  //#region AttestTime

  saveAttestForTransactionsValidation(
    searchDto: ISaveAttestForTransactionsValidationModel
  ): Observable<any> {
    return this.http.post<any>(
      saveAttestForTransactionsValidation(),
      searchDto
    );
  }

  saveAttestForTransactions(
    searchDto: ISaveAttestForTransactionsModel
  ): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(
      saveAttestForTransactions(),
      searchDto
    );
  }

  //#endregion

  //#region EmployeeV2
  getEmployeeForUserWithTimeCode(
    date: string
  ): Observable<IEmployeeTimeCodeDTO> {
    return this.http.get<IEmployeeTimeCodeDTO>(
      getEmployeeForUserWithTimeCode(date)
    );
  }
  //#endregion

  //#region TimeTransactions

  getTimeBlocksForTimeSheetFiltered(
    model: IGetProjectTimeBlocksForTimesheetModel
  ): Observable<ProjectTimeBlockDTO[]> {
    return this.http.post<ProjectTimeBlockDTO[]>(
      getTimeBlocksForTimeSheetFiltered(),
      model
    );
  }

  recalculateWorkTime(
    model: IProjectTimeBlockSaveDTO[]
  ): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(recalculateWorkTime(), model);
  }

  moveTimeRowsToDate(
    model: IMoveProjectTimeBlocksToDateModel
  ): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(moveTimeRowsToDate(), model);
  }

  moveTimeRowsToOrder(
    model: IMoveProjectTimeBlocksToOrderModel
  ): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(moveTimeRowsToOrder(), model);
  }

  MoveTimeRowsToOrderRow(
    model: IMoveProjectTimeBlocksToOrderModel
  ): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(moveTimeRowsToOrderRow(), model);
  }

  saveNotesForProjectTimeBlock(
    model: IProjectTimeBlockSaveDTO
  ): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(
      saveNotesForProjectTimeBlock(),
      model
    );
  }

  validateProjectTimeBlockSaveDTO(
    model: IValidateProjectTimeBlockSaveDTO[]
  ): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(
      validateProjectTimeBlockSaveDTO(),
      model
    );
  }

  saveProjectTimeBlockSaveDTO(
    model: ProjectTimeBlockSaveDTO[]
  ): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(
      saveProjectTimeBlockSaveDTO(),
      model
    );
  }

  getEmployeesForTimeProjectRegistrationSmall(
    projectId: number,
    fromDateString: string,
    toDateString: string
  ): Observable<IEmployeeTimeCodeDTO[]> {
    return this.http.get(
      getEmployeesForTimeProjectRegistrationSmall(
        projectId,
        fromDateString,
        toDateString
      )
    );
  }

  GetEmployeeScheduleAndTransactionInfo(
    employeeId: number,
    date: string
  ): Observable<IEmployeeScheduleTransactionInfoDTO> {
    return this.http.get(
      getEmployeeScheduleAndTransactionInfo(employeeId, date)
    );
  }

  GetEmployeeFirstEligibleTime(
    employeeId: number,
    date: string
  ): Observable<Date> {
    return this.http.get(getEmployeeFirstEligibleTime(employeeId, date));
  }

  //#endregion
}
