import { Injectable } from '@angular/core';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { SoeCategoryType } from '@shared/models/generated-interfaces/Enumerations';
import {
  ICategoryDTO,
  ICategoryGridDTO,
  ICompanyCategoryRecordDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  deleteCategory,
  getCategoriesDict,
  getCategory,
  getCategoryGrid,
  getCategoryTypesByPermission,
  getCompCategoryRecords,
  saveCategory,
} from '@shared/services/generated-service-endpoints/core/Category.endpoints';
import { Observable, of } from 'rxjs';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class CategoryService {
  constructor(private http: SoeHttpClient) {}

  getGridAdditionalProps = {
    categoryType: SoeCategoryType.Unknown,
  };
  getGrid(
    id?: number,
    additionalProps?: { categoryType: SoeCategoryType }
  ): Observable<ICategoryGridDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;

    if (this.getGridAdditionalProps.categoryType === SoeCategoryType.Unknown)
      return of([] as ICategoryGridDTO[]);

    return this.http.get<ICategoryGridDTO[]>(
      getCategoryGrid(this.getGridAdditionalProps.categoryType, id)
    );
  }

  get(id: number): Observable<ICategoryDTO> {
    return this.http.get(getCategory(id));
  }

  getCategoriesDict(
    categoryType: SoeCategoryType,
    addEmptyRow: boolean = false,
    excludeCategoryId?: number
  ): Observable<SmallGenericType[]> {
    return this.http.get(
      getCategoriesDict(+categoryType, addEmptyRow, excludeCategoryId)
    );
  }

  getCompCategoryRecords(
    soeCategoryTypeId: number,
    categoryRecordEntity: number,
    recordId: number
  ): Observable<ICompanyCategoryRecordDTO[]> {
    return this.http.get<ICompanyCategoryRecordDTO[]>(
      getCompCategoryRecords(soeCategoryTypeId, categoryRecordEntity, recordId)
    );
  }

  getCategoryTypesByPermission(): Observable<SmallGenericType[]> {
    return this.http.get<SmallGenericType[]>(getCategoryTypesByPermission());
  }

  save(model: unknown): Observable<BackendResponse> {
    return this.http.post(saveCategory(), model);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(deleteCategory(id));
  }
}
