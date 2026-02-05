import { Injectable } from '@angular/core';
import { IPaymentConditionDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Observable, forkJoin, of, tap } from 'rxjs';
import { Perform } from '@shared/util/perform.class';
import { SupplierService } from '@src/app/features/economy/services/supplier.service';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { SupplierDTO } from '@src/app/features/economy/suppliers/models/supplier.model';
import { SmallGenericType } from '@shared/models/generic-type.model';

@Injectable({ providedIn: 'root' })
export class SupplierHelper {
  //#region Fields, setters & getters
  suppliers: ISmallGenericType[] = [];
  supplier?: SupplierDTO;
  supplierReferences: ISmallGenericType[] = [];
  supplierEmails: ISmallGenericType[] = [];
  paymentConditions: ISmallGenericType[] = [];
  supplierId!: number;
  sysLanguageId!: number;
  _selectedSupplier!: ISmallGenericType;

  public get selectedSupplier(): ISmallGenericType {
    return this._selectedSupplier;
  }
  set selectedSupplier(item: ISmallGenericType) {
    this._selectedSupplier = item;
    if (this.selectedSupplier) {
      if (this.supplierId !== this.selectedSupplier.id) {
        this.supplierId = this.selectedSupplier.id;
        this.loadSupplier(this.selectedSupplier.id);
      }
    }
  }
  //#endregion
  performLoad = new Perform<any>(this.progress);

  constructor(
    //@Inject(FlowHandlerService) private handler: FlowHandlerService,
    private readonly supplierService: SupplierService,
    private readonly progress: ProgressService
  ) {}

  //#region Lookups

  loadSuppliers(
    useCache: boolean,
    setSelected = false
  ): Observable<ISmallGenericType[]> {
    return this.performLoad.load$(
      this.supplierService.getSupplierDict(true, true, useCache).pipe(
        tap(data => {
          this.suppliers = data;
        })
      )
    );
  }

  loadSupplier(supplierId: number): Observable<unknown> {
    if (supplierId) {
      return forkJoin([
        this.supplierService.getSupplier(supplierId, false, true, false, false),
        this.supplierService.getSupplierReferences(supplierId),
        this.supplierService.getSupplierEmails(supplierId, true, true),
      ]).pipe(
        tap(([supplier, reference, email]) => {
          this.supplier = supplier;
          this.supplierReferences = reference;
          this.supplierEmails = email;
          this.sysLanguageId = this.supplier.sysLanguageId ?? 0;
        })
      );
    } else {
      this.supplier = undefined;
      this.supplierReferences = [];
      this.supplierEmails = [];
      return of();
    }
  }

  loadPaymentConditions(
    useCache: boolean = false
  ): Observable<IPaymentConditionDTO[]> {
    return this.performLoad.load$(
      this.supplierService.getPaymentConditions(useCache).pipe(
        tap(data => {
          this.paymentConditions.push(new SmallGenericType(0, ' '));
          data.forEach(f => {
            this.paymentConditions.push(
              new SmallGenericType(f.paymentConditionId, f.name)
            );
          });
        })
      )
    );
  }

  //#endregion

  //#region Public methods

  //#endregion
}
