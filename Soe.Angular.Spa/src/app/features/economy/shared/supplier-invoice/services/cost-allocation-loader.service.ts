import { inject, Injectable, signal, WritableSignal } from '@angular/core';
import { SoeTimeCodeType } from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { IProductSmallDTO } from '@shared/models/generated-interfaces/ProductDTOs';
import {
  IEmployeeSmallDTO,
  ITimeCodeDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { finalize, forkJoin, Observable, of, tap } from 'rxjs';
import { SupplierInvoiceService } from './supplier-invoice.service';
import { orderBy } from 'lodash';
import { ProgressService } from '@shared/services/progress';
import { Perform } from '@shared/util/perform.class';
import { SupplierInvoiceCostAllocationDTO } from '../models/supplier-invoice.model';
import { SupplierInvoiceSettingsService } from './supplier-invoice-settings.service';

@Injectable({
  providedIn: 'root',
})
export class CostAllocationLoaderService {
  readonly service = inject(SupplierInvoiceService);
  readonly progressService = inject(ProgressService);
  private readonly settingService = inject(SupplierInvoiceSettingsService);
  readonly performLoadData = new Perform<any>(this.progressService);

  public employees: WritableSignal<IEmployeeSmallDTO[]> = signal([]);
  public timeCodes: WritableSignal<ISmallGenericType[]> = signal([]);
  public products: WritableSignal<IProductSmallDTO[]> = signal([]);

  private loadingState: Observable<any> | null = null;
  private loaded = false;

  //#region Data Loading

  private performLoad() {
    this.loadingState = this.performLoadData.load$(
      forkJoin([
        this.loadCustomerTimeCodes(),
        this.loadProducts(),
        this.loadCustomerEmployees(),
      ]).pipe(
        tap(() => {
          this.loaded = true;
        }),
        finalize(() => (this.loadingState = null))
      )
    );
    return this.loadingState;
  }

  public load() {
    if (this.loadingState) return this.loadingState;
    if (this.loaded) return of(true);
    return this.performLoad();
  }

  private loadCustomerTimeCodes(): Observable<ITimeCodeDTO[]> {
    return this.service
      .getTimeCodes(SoeTimeCodeType.WorkAndMaterial, true, false)
      .pipe(
        tap((codes: ITimeCodeDTO[]) => {
          const timeCodesArray: ISmallGenericType[] = [];
          timeCodesArray.push({ id: 0, name: '' } as ISmallGenericType);

          const timeCodeSorted = orderBy(
            codes,
            ['timeCodeId', 'name'],
            ['asc', 'asc']
          ).map(
            timeCode =>
              ({
                id: timeCode.timeCodeId,
                name: timeCode.name,
              }) as ISmallGenericType
          );
          timeCodesArray.push(...timeCodeSorted);
          this.timeCodes.set(timeCodesArray);
        })
      );
  }

  private loadProducts(): Observable<IProductSmallDTO[]> {
    return this.service.getInvoiceProductsSmall().pipe(
      tap((products: IProductSmallDTO[]) => {
        const productsSorted = orderBy(
          products,
          ['number', 'name'],
          ['asc', 'asc']
        );
        this.products.set(productsSorted);
      })
    );
  }

  private loadCustomerEmployees(): Observable<IEmployeeSmallDTO[]> {
    return this.service
      .getAllEmployeeSmallDTOs(true, false, false, true, true)
      .pipe(
        tap(result => {
          const employeesSorted = orderBy(
            result,
            ['employeeNr', 'name'],
            ['asc', 'asc']
          );
          let employeesArray: IEmployeeSmallDTO[] = [];
          employeesArray = [
            { employeeId: 0, employeeNr: '', name: '' } as IEmployeeSmallDTO,
          ];

          employeesArray.push(...employeesSorted);
          this.employees.set(employeesArray);
        })
      );
  }

  //#endregion

  //#region Helper Methods
  public setTimeCodeDetails(row: SupplierInvoiceCostAllocationDTO) {
    row.timeCodeId = this.settingService.projectDefaultTimeCodeId ?? 0;

    if (row.timeCodeId && this.timeCodes()) {
      const timeCode = this.timeCodes().find(tc => tc.id === row.timeCodeId);
      if (timeCode) {
        row.timeCodeName = timeCode.name;
      }
    }
  }
  //#endregion
}
