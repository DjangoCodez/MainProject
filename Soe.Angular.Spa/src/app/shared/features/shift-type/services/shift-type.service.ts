import { Injectable, inject, signal } from '@angular/core';
import {
  IShiftTypeDTO,
  IShiftTypeGridDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ProgressService } from '@shared/services/progress/progress.service';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  deleteShiftType,
  deleteShiftTypes,
  getShiftType,
  getShiftTypeGrid,
  getShiftTypes,
  getShiftTypesDict,
  saveShiftType,
} from '@shared/services/generated-service-endpoints/time/ShiftType.endpoints';
import { map, Observable, tap } from 'rxjs';
import { AccountDimDTO } from 'src/app/features/economy/accounting-coding-levels/models/accounting-coding-levels.model';
import { getShiftTypeAccountDim } from '@shared/services/generated-service-endpoints/economy/Account.endpoints';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { Perform } from '@shared/util/perform.class';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class ShiftTypeService {
  constructor(private http: SoeHttpClient) {}

  // Cached data
  progressService = inject(ProgressService);
  performShiftTypes = new Perform<SmallGenericType[]>(this.progressService);

  showShiftTypesWithInactiveAccounts = signal(false);

  getGridAdditionalProps = {
    loadAccounts: false,
    loadSkills: false,
    loadEmployeeStatisticsTargets: false,
    setTimeScheduleTemplateBlockTypeName: false,
    setCategoryNames: false,
    setAccountingString: false,
    setSkillNames: false,
    setTimeScheduleTypeName: false,
    loadHierarchyAccounts: false,
  };
  getGrid(
    id?: number,
    additionalProps?: {
      loadAccounts: boolean;
      loadSkills: boolean;
      loadEmployeeStatisticsTargets: boolean;
      setTimeScheduleTemplateBlockTypeName: boolean;
      setCategoryNames: boolean;
      setAccountingString: boolean;
      setSkillNames: boolean;
      setTimeScheduleTypeName: boolean;
      loadHierarchyAccounts: boolean;
    }
  ): Observable<IShiftTypeGridDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;
    // Default values
    else {
      this.getGridAdditionalProps = {
        loadAccounts: false,
        loadSkills: false,
        loadEmployeeStatisticsTargets: false,
        setTimeScheduleTemplateBlockTypeName: true,
        setCategoryNames: true,
        setAccountingString: true,
        setSkillNames: true,
        setTimeScheduleTypeName: true,
        loadHierarchyAccounts: false,
      };
    }
    return this.http
      .get<
        IShiftTypeGridDTO[]
      >(getShiftTypeGrid(this.getGridAdditionalProps.loadAccounts, this.getGridAdditionalProps.loadSkills, this.getGridAdditionalProps.loadEmployeeStatisticsTargets, this.getGridAdditionalProps.setTimeScheduleTemplateBlockTypeName, this.getGridAdditionalProps.setCategoryNames, this.getGridAdditionalProps.setAccountingString, this.getGridAdditionalProps.setSkillNames, this.getGridAdditionalProps.setTimeScheduleTypeName, this.getGridAdditionalProps.loadHierarchyAccounts, id))
      .pipe(
        map(data =>
          data.filter(x => {
            return (
              this.showShiftTypesWithInactiveAccounts() || !x.accountIsNotActive
            );
          })
        )
      );
  }

  get(
    id: number,
    loadAccounts: boolean,
    loadSkills: boolean,
    loadEmployeeStatisticsTargets: boolean,
    setTimeScheduleTemplateBlockTypeName: boolean,
    loadCategories: boolean,
    loadHierarchyAccounts: boolean
  ): Observable<IShiftTypeDTO> {
    return this.http.get(
      getShiftType(
        id,
        loadAccounts,
        loadSkills,
        loadEmployeeStatisticsTargets,
        setTimeScheduleTemplateBlockTypeName,
        loadCategories,
        loadHierarchyAccounts
      )
    );
  }

  getShiftTypes(
    loadAccountInternals: boolean,
    loadAccounts: boolean,
    loadSkills: boolean,
    loadEmployeeStatisticsTargets: boolean,
    setTimeScheduleTemplateBlockTypeName: boolean,
    setCategoryNames: boolean,
    setAccountingString: boolean,
    setSkillNames: boolean,
    setTimeScheduleTypeName: boolean,
    loadHierarchyAccounts: boolean
  ): Observable<IShiftTypeDTO[]> {
    return this.http.get<IShiftTypeDTO[]>(
      getShiftTypes(
        loadAccountInternals,
        loadAccounts,
        loadSkills,
        loadEmployeeStatisticsTargets,
        setTimeScheduleTemplateBlockTypeName,
        setCategoryNames,
        setAccountingString,
        setSkillNames,
        setTimeScheduleTypeName,
        loadHierarchyAccounts
      )
    );
  }

  getShiftTypesDict(addEmptyRow: boolean): Observable<SmallGenericType[]> {
    return this.http
      .get<SmallGenericType[]>(getShiftTypesDict(addEmptyRow))
      .pipe(
        tap(x => {
          this.performShiftTypes.data = x;
        })
      );
  }

  getShiftTypeAccountDim(
    loadAccounts: boolean,
    useCache: boolean
  ): Observable<AccountDimDTO> {
    return this.http.get<AccountDimDTO>(
      getShiftTypeAccountDim(loadAccounts, useCache)
    );
  }

  save(model: IShiftTypeDTO): Observable<BackendResponse> {
    return this.http.post(saveShiftType(), model);
  }

  delete(id: number): Observable<BackendResponse> {
    return this.http.delete(deleteShiftType(id));
  }

  bulkDelete(ids: string): Observable<BackendResponse> {
    return this.http.delete(deleteShiftTypes(ids));
  }
}
