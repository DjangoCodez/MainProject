import { Injectable } from '@angular/core';
import {
  IProjectGridDTO,
  IProjectTinyDTO,
} from '@shared/models/generated-interfaces/ProjectDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  changeProjectStatus,
  deleteProject,
  getBudgetHeadGridForProject,
  getProjectGridDTO,
  getProjectList,
  getProjectsSmall,
  getProjectUsers,
  getTimeProject,
  saveProject,
} from '@shared/services/generated-service-endpoints/billing/InvoiceProject.endpoints';
import { map, Observable } from 'rxjs';
import {
  ProjectExtendedGridDTO,
  SaveInvoiceProjectModel,
  TimeProjectDTO,
} from '../models/project.model';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { getOrderTemplates } from '@shared/services/generated-service-endpoints/billing/OrderV2.endpoints';
import {
  IBudgetHeadGridDTO,
  IProjectUserDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import {
  getPriceListsDict,
  getProductPriceLists,
  savePriceListType,
  savePriceList,
} from '@shared/services/generated-service-endpoints/billing/PriceList.endpoints';
import { IPriceListDTO } from '@shared/models/generated-interfaces/PriceListDTOs';
import { IPriceListTypeDTO } from '@shared/models/generated-interfaces/PriceListTypeDTOs';
import { IProjectProductRowDTO } from '@shared/models/generated-interfaces/ProjectProductRowDTO';
import { getProductRows } from '@shared/services/generated-service-endpoints/billing/ProjectProduct.endpoints';
import { TermGroup_ProjectStatus } from '@shared/models/generated-interfaces/Enumerations';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class ProjectService {
  constructor(private http: SoeHttpClient) {}

  get(projectId: number): Observable<TimeProjectDTO> {
    return this.http.get<TimeProjectDTO>(getTimeProject(projectId));
  }

  getGrid(
    id: number = 0,
    additionalProps?: { projectStatuses: number[]; onlyMine: boolean }
  ): Observable<ProjectExtendedGridDTO[]> {
    return this.http
      .get<
        ProjectExtendedGridDTO[]
      >(getProjectList(id, additionalProps?.projectStatuses ?? [TermGroup_ProjectStatus.Active], additionalProps?.onlyMine ?? false))
      .pipe(
        map((projects: ProjectExtendedGridDTO[]) => {
          projects.forEach(p => {
            p.categoriesArray = p.categories
              ? p.categories.split(',').flatMap(c => {
                  const trimmed = c.trim();
                  return trimmed ? [trimmed] : [];
                })
              : [];
          });
          return projects;
        })
      );
  }

  save(model: SaveInvoiceProjectModel): Observable<BackendResponse> {
    return this.http.post<BackendResponse>(saveProject(), model);
  }

  delete(projectId: number) {
    return this.http.delete<BackendResponse>(deleteProject(projectId));
  }

  updateProjectStatus(ids: number[], newStatus: number) {
    const model = {
      Ids: ids,
      NewState: newStatus,
    };

    return this.http.post<BackendResponse>(changeProjectStatus(), model);
  }

  getProjectsSmall(
    onlyActive: boolean,
    hidden: boolean,
    sortOnNumber: boolean
  ): Observable<IProjectTinyDTO[]> {
    return this.http.get<IProjectTinyDTO[]>(
      getProjectsSmall(onlyActive, hidden, sortOnNumber)
    );
  }

  getOrderTemplates(useCache: boolean): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(getOrderTemplates(), {
      useCache,
    });
  }

  getProjectPersons(
    projectId: number,
    loadTypeNames: boolean
  ): Observable<IProjectUserDTO[]> {
    return this.http.get<IProjectUserDTO[]>(
      getProjectUsers(projectId, loadTypeNames)
    );
  }

  getProjectPricelists(
    comparisonPriceListTypeId: number,
    priceListTypeId: number,
    loadAll: boolean,
    priceDate: string
  ): Observable<IPriceListDTO[]> {
    return this.http.get<IPriceListDTO[]>(
      getProductPriceLists(
        comparisonPriceListTypeId,
        priceListTypeId,
        loadAll,
        priceDate
      )
    );
  }

  getProductRows(
    projectId: number,
    originType: number,
    includeChildProjects: boolean,
    fromDate: string,
    toDate: string
  ): Observable<IProjectProductRowDTO[]> {
    return this.http.get<IProjectProductRowDTO[]>(
      getProductRows(
        projectId,
        originType,
        includeChildProjects,
        fromDate,
        toDate
      )
    );
  }

  getBudgetHeadGridForProject(projectId: number, actorCompanyId: number) {
    return this.http.get<IBudgetHeadGridDTO[]>(
      getBudgetHeadGridForProject(projectId, actorCompanyId)
    );
  }

  getPriceListsDict(addEmptyRow: boolean) {
    return this.http.get<ISmallGenericType[]>(getPriceListsDict(addEmptyRow));
  }

  savePriceListType(head: IPriceListTypeDTO, prices: IPriceListDTO[]) {
    const dto = {
      priceListType: head,
      priceLists: prices,
    };
    return this.http.post<BackendResponse>(savePriceListType(), dto);
  }

  savePriceList(model: IPriceListTypeDTO) {
    return this.http.post<BackendResponse>(savePriceList(), model);
  }

  getProjectGridDTO(projectId: number): Observable<IProjectGridDTO> {
    return this.http.get<IProjectGridDTO>(getProjectGridDTO(projectId));
  }
}
