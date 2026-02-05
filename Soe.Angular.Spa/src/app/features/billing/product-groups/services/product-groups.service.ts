import { Injectable } from '@angular/core';
import {
  IProductGroupDTO,
  IProductGroupGridDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  saveProductGroup,
  getProductGroup,
  getProductGroupsGrid,
  deleteProductGroup,
} from '@shared/services/generated-service-endpoints/billing/ProductGroup.endpoints';
import { Observable } from 'rxjs';
import { ProductGroupDTO } from '../models/product-groups.model';

@Injectable({
  providedIn: 'root',
})
export class ProductGroupsService {
  constructor(private http: SoeHttpClient) {}

  getGrid(id?: number): Observable<IProductGroupGridDTO[]> {
    return this.http.get<IProductGroupGridDTO[]>(getProductGroupsGrid(id));
  }

  get(id: number): Observable<ProductGroupDTO> {
    return this.http.get<ProductGroupDTO>(getProductGroup(id));
  }

  save(model: IProductGroupDTO): Observable<any> {
    return this.http.post<IProductGroupDTO>(saveProductGroup(), model);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(deleteProductGroup(id));
  }
}
