import { inject, Injectable, signal } from '@angular/core';
import { SupplierInvoiceService } from './supplier-invoice.service';
import { finalize, forkJoin, Observable, of, tap } from 'rxjs';
import { SupplierService } from '@features/economy/services/supplier.service';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { PaymentConditionDTO } from '@shared/features/payment-conditions/models/payment-condition.model';
import { CoreService } from '@shared/services/core.service';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  TermGroup,
  TermGroup_InvoiceVatType,
} from '@shared/models/generated-interfaces/Enumerations';
import { VatCodeDTO } from '@features/economy/models/vat-code.model';
import { getVatCodes } from '@shared/services/generated-service-endpoints/economy/VatCode.endpoints';
import { SupplierInvoiceSettingsService } from './supplier-invoice-settings.service';
import { ICompCurrencySmallDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SupplierInvoiceFeatureService } from './supplier-invoice-feature.service';
import { addEmptyOption } from '@shared/util/array-util';

@Injectable({
  providedIn: 'root',
})
export class SupplierInvoiceLoaderService {
  // This service is responsible for loading required data for the supplier invoice edit component.
  // By having it as a separate service, we can inject it at an earlier stage in the component lifecycle and prefetch data if needed.
  readonly supplierInvoiceService: SupplierInvoiceService = inject(
    SupplierInvoiceService
  );
  readonly supplierInvoiceSettingsService = inject(
    SupplierInvoiceSettingsService
  );
  readonly supplierInvoiceFeatureService = inject(
    SupplierInvoiceFeatureService
  );
  readonly supplierService = inject(SupplierService);
  readonly coreService = inject(CoreService);
  readonly http = inject(SoeHttpClient);

  private static readonly APPLICABLE_VAT_TYPES = [
    TermGroup_InvoiceVatType.Contractor,
    TermGroup_InvoiceVatType.NoVat,
    TermGroup_InvoiceVatType.EU,
    TermGroup_InvoiceVatType.NonEU,
    TermGroup_InvoiceVatType.Merchandise,
  ];

  public supplierDict = signal<SmallGenericType[]>([]);
  public paymentConditions = signal<PaymentConditionDTO[]>([]);
  public vatTypes = signal<SmallGenericType[]>([]);
  public vatCodes = signal<VatCodeDTO[]>([]);
  public sourceTypes = signal<SmallGenericType[]>([]);
  public statusTypes = signal<SmallGenericType[]>([]);
  public currencies = signal<ICompCurrencySmallDTO[]>([]);

  public loadingState: Observable<any> | null = null;
  public loaded = false;

  public performLoad() {
    this.loadingState = forkJoin([
      this.loadSuppliers(),
      this.loadPaymentConditions(),
      this.loadVatCodes(),
      this.loadVatTypes(),
      this.loadSourceTypes(),
      this.loadStatusTypes(),
      this.loadCurrencies(),
      this.supplierInvoiceSettingsService.loadSettings(),
      this.supplierInvoiceFeatureService.loadFeatures(),
    ]).pipe(
      tap(() => {
        this.loaded = true;
      }),
      finalize(() => (this.loadingState = null))
    );
    return this.loadingState;
  }

  public load() {
    if (this.loadingState) return this.loadingState;
    if (this.loaded) return of(true);
    return this.performLoad();
  }

  public loadSuppliers() {
    return this.supplierService.getSupplierDict(true, true, false).pipe(
      tap(suppliers => {
        this.supplierDict.set(suppliers);
      })
    );
  }
  public upsertSupplier(supplier: SmallGenericType) {
    const suppliers = this.supplierDict();
    const index = suppliers.findIndex(s => s.id === supplier.id);
    if (index > -1) {
      suppliers[index] = supplier;
    } else {
      suppliers.push(supplier);
    }
    this.supplierDict.set([...suppliers]);
  }

  public loadVatTypes() {
    return this.coreService
      .getTermGroupContent(TermGroup.InvoiceVatType, false, false, false)
      .pipe(
        tap(vatTypes => {
          this.vatTypes.set(
            vatTypes.filter(vt =>
              SupplierInvoiceLoaderService.APPLICABLE_VAT_TYPES.includes(vt.id)
            )
          );
        })
      );
  }

  public loadVatCodes() {
    return this.http.get<VatCodeDTO[]>(getVatCodes()).pipe(
      tap(vatCodes => {
        addEmptyOption(vatCodes);
        this.vatCodes.set(vatCodes);
      })
    );
  }

  public loadPaymentConditions() {
    return this.supplierService.getPaymentConditions().pipe(
      tap(paymentConditions => {
        this.paymentConditions.set(paymentConditions);
      })
    );
  }

  public loadStatusTypes() {
    return this.coreService
      .getTermGroupContent(
        TermGroup.SupplierInvoiceState,
        false,
        false,
        false,
        true
      )
      .pipe(
        tap(statusTypes => {
          this.statusTypes.set(statusTypes);
        })
      );
  }

  public loadSourceTypes() {
    return this.coreService
      .getTermGroupContent(
        TermGroup.SupplierInvoiceSource,
        false,
        false,
        false,
        true
      )
      .pipe(
        tap(sourceTypes => {
          this.sourceTypes.set(sourceTypes);
        })
      );
  }

  private loadCurrencies() {
    return this.coreService.getCompCurrenciesSmall().pipe(
      tap(currencies => {
        this.currencies.set(currencies);
      })
    );
  }
}
