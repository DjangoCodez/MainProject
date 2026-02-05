import { Injectable } from '@angular/core';
import { SoeHttpClient } from '@shared/services/http.service';
import { Observable, forkJoin } from 'rxjs';
import { InventoryWriteOffTemplatesDTO } from '../models/inventory-write-off-templates.model';
import {
  deleteInventoryWriteOffTemplate,
  getInventoryWriteOffTemplate,
  getInventoryWriteOffTemplateGrid,
  getInventoryWriteOffTemplates,
  getInventoryWriteOffTemplatesDict,
  saveInventoryWriteOffTemplate,
} from '@shared/services/generated-service-endpoints/economy/InventoryWriteOffTemplate.endpoints';
import { IInventoryWriteOffTemplateGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { VoucherSeriesTypeService } from '../../services/voucher-series-type.service';
import { InventoryWriteOffMethodsService } from '../../inventory-write-off-methods/services/inventory-write-off-methods.service';
import { map } from 'rxjs/operators';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class InventoryWriteOffTemplatesService {
  constructor(
    private http: SoeHttpClient,
    private voucherSeriesTypeService: VoucherSeriesTypeService,
    private inventoryWriteOffMethodsService: InventoryWriteOffMethodsService
  ) {}

  getGrid(id?: number): Observable<IInventoryWriteOffTemplateGridDTO[]> {
    return forkJoin({
      methods: this.inventoryWriteOffMethodsService.getDict(true),
      vouchers: this.voucherSeriesTypeService.getGrid(),
      writeOffTemplate: this.http.get<IInventoryWriteOffTemplateGridDTO[]>(
        getInventoryWriteOffTemplateGrid(id)
      ),
    }).pipe(
      map(
        ({
          methods,
          vouchers,
          writeOffTemplate,
        }): IInventoryWriteOffTemplateGridDTO[] => {
          writeOffTemplate.forEach(el => {
            el.inventoryWriteOffName =
              methods?.find(m => m.id == el.inventoryWriteOffMethodId)?.name ??
              '';
            el.voucherSeriesName =
              vouchers?.find(
                v => v.voucherSeriesTypeId == el.voucherSeriesTypeId
              )?.name ?? '';
          });
          return writeOffTemplate;
        }
      )
    );
  }

  getDict(): Observable<SmallGenericType[]> {
    return this.http.get<SmallGenericType[]>(
      getInventoryWriteOffTemplatesDict(false)
    );
  }

  get(id: number): Observable<InventoryWriteOffTemplatesDTO> {
    return forkJoin({
      writeOffTemplate: this.http.get<InventoryWriteOffTemplatesDTO>(
        getInventoryWriteOffTemplate(id)
      ),
      method: this.inventoryWriteOffMethodsService.getDict(false),
      voucher: this.voucherSeriesTypeService.getVoucherSeriesTypesByCompany(),
    }).pipe(
      map(
        ({
          method,
          voucher,
          writeOffTemplate,
        }): InventoryWriteOffTemplatesDTO => {
          if (writeOffTemplate) {
            writeOffTemplate.inventoryWriteOffName =
              method?.find(
                m => m.id == writeOffTemplate.inventoryWriteOffMethodId
              )?.name ?? '';
            writeOffTemplate.voucherSeriesName =
              voucher?.find(m => m.id == writeOffTemplate.voucherSeriesTypeId)
                ?.name ?? '';
          }
          return writeOffTemplate;
        }
      )
    );
  }

  getAll(): Observable<InventoryWriteOffTemplatesDTO[]> {
    const inventoryWriteOffTemplates = this.http.get<
      InventoryWriteOffTemplatesDTO[]
    >(getInventoryWriteOffTemplates());

    return forkJoin({
      methods: this.inventoryWriteOffMethodsService.getDict(true),
      vouchers: this.voucherSeriesTypeService.getGrid(),
      writeOffTemplate: inventoryWriteOffTemplates,
    }).pipe(
      map(
        ({
          methods,
          vouchers,
          writeOffTemplate,
        }): InventoryWriteOffTemplatesDTO[] => {
          writeOffTemplate.forEach(el => {
            el.inventoryWriteOffName =
              methods?.find(m => m.id == el.inventoryWriteOffMethodId)?.name ??
              '';
            el.voucherSeriesName =
              vouchers?.find(
                v => v.voucherSeriesTypeId == el.voucherSeriesTypeId
              )?.name ?? '';
          });
          return writeOffTemplate;
        }
      )
    );
  }

  save(model: any) {
    return this.http.post<BackendResponse>(
      saveInventoryWriteOffTemplate(),
      model
    );
  }

  delete(id: number): Observable<BackendResponse> {
    return this.http.delete<BackendResponse>(
      deleteInventoryWriteOffTemplate(id)
    );
  }
}
