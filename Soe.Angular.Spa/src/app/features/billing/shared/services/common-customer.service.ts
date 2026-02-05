import { Injectable } from '@angular/core';
import { CoreService } from '@shared/services/core.service'
import { ProgressService } from '@shared/services/progress/progress.service'
import { SoeHttpClient } from '@shared/services/http.service';
import { Observable } from 'rxjs';
import { Perform } from '@shared/util/perform.class';
import { getContactAddresses } from '@shared/services/generated-service-endpoints/core/ContactAddress.endpoints';
import { IContactAddressDTO } from '@shared/models/generated-interfaces/ContactDTO';
import { getPriceLists } from '@shared/services/generated-service-endpoints/billing/InvoicePriceLists.endpoints';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { getSmallGenericSysWholesellers } from '@shared/services/generated-service-endpoints/billing/SysWholeseller.endpoints';
import { getCustomer, getCustomerEmailAddresses, getCustomerGlnNumbers, getCustomersByCompanyDict, getCustomerStatistics } from '@shared/services/generated-service-endpoints/shared/CustomerV2.endpoints';
import { getDeliveryTypesDict } from '@shared/services/generated-service-endpoints/billing/DeliveryType.endpoints';
import { getDeliveryConditions } from '@shared/services/generated-service-endpoints/billing/DeliveryCondition.endpoints';
import { getSmallGenericTypePaymentConditions } from '@shared/services/generated-service-endpoints/economy/PaymentCondition.endpoints';
import { CustomerDTO } from '@shared/features/customer/models/customer.model';
import { getProductsSmall } from '@shared/services/generated-service-endpoints/billing/BillingProduct.endpoints';
import { IProductSmallDTO } from '@shared/models/generated-interfaces/ProductDTOs';
import { ICustomerStatisticsDTO } from '@shared/models/generated-interfaces/CustomerStatisticsDTO';
import { getCustomerCentralCountersAndBalance } from '@shared/services/generated-service-endpoints/shared/CustomerV2.endpoints';
import { IChangeStatusGridViewBalanceDTO } from '@shared/models/generated-interfaces/ChangeStatusGridViewDTO';
import { getInvoicesForCustomerCentral } from '@shared/services/generated-service-endpoints/core/CustomerInvoices.endpoints';
import { CustomerInvoiceGridDTO } from '@shared/features/customer-central/models/customer-central.model';

@Injectable({
  providedIn: 'root',
})
export class CommonCustomerService {
  //#region Fields, setters & getters

  //#endregion

  performLoadContactAddresses = new Perform<IContactAddressDTO[]>(
    this.progress
  );

  constructor(
    //@Inject(FlowHandlerService) private handler: FlowHandlerService,
    private readonly coreService: CoreService,
    private readonly progress: ProgressService,
    private http: SoeHttpClient
  ) {}

  //#region Lookups
  getContactAddresses(
    actorId: number,
    type: number,
    addEmptyRow: boolean,
    includeRows: boolean,
    includeCareOf: boolean
  ): Observable<IContactAddressDTO[]> {
    return this.performLoadContactAddresses.load$(
      this.http.get(
        getContactAddresses(
          actorId,
          type,
          addEmptyRow,
          includeRows,
          includeCareOf
        )
      )
    );
  }

  getCustomerCentralCountersAndBalance(counterTypes: number[], customerId: number, accountYearId: number, baseSysCurrencyId: number) : Observable<IChangeStatusGridViewBalanceDTO[]> {
    const model = {
      CounterTypes: counterTypes,
      CustomerId: customerId,
      AccountYearId: accountYearId,
      baseSysCurrencyId: baseSysCurrencyId
    };

    return this.http.post(getCustomerCentralCountersAndBalance(), model);
  }

  getCustomerInvoicesForCustomerCentral(classification: number, originType: number, actorCustomerId: number, onlyMine: boolean) : Observable<CustomerInvoiceGridDTO[]> {
    const model = {
      classification: classification,
      originType: originType,
      actorCustomerId: actorCustomerId,
      onlyMine: onlyMine
    };
    return this.http.post(getInvoicesForCustomerCentral(), model)
  }


  //#endregion

  //#region Public methods

  getCustomer(customerId: number, loadActor: boolean, loadAccount: boolean, loadNote: boolean, loadCustomerUser: boolean, loadContactAddresses: boolean, loadCategories: boolean) {
    return this.http.get<CustomerDTO>(getCustomer(customerId, loadActor, loadAccount, loadNote, loadCustomerUser, loadContactAddresses, loadCategories));
  }

  getPriceListsDict(addEmptyRow: boolean, useCache: boolean): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(getPriceLists(addEmptyRow), { useCache});
  }

  getSysWholesellersDict(addEmptyRow: boolean) {
    return this.http.get<ISmallGenericType[]>(getSmallGenericSysWholesellers(addEmptyRow));
  }

  getCustomerEmails(customerId: number, loadContactPersonsEmails: boolean, addEmptyRow: boolean) {
    return this.http.get<ISmallGenericType[]>(getCustomerEmailAddresses(customerId, loadContactPersonsEmails, addEmptyRow));
  }
  
  getCustomerGLNs(customerId: number, addEmptyRow: boolean) {
    return this.http.get<ISmallGenericType[]>(getCustomerGlnNumbers(customerId, addEmptyRow));
  }

  getDeliveryTypesDict(addEmptyRow: boolean) {
    return this.http.get<ISmallGenericType[]>(getDeliveryTypesDict(addEmptyRow));
  }

  getDeliveryConditionsDict(addEmptyRow: boolean) {
    return this.http.get<ISmallGenericType[]>(getDeliveryConditions(addEmptyRow));
  }

  getPaymentConditionsDict(addEmptyRow: boolean) {
    return this.http.get<ISmallGenericType[]>(getSmallGenericTypePaymentConditions(addEmptyRow));
  }

  getCustomersDict(onlyActive: boolean, addEmptyRow: boolean, useCache: boolean) {
    return this.http.get<ISmallGenericType[]>(getCustomersByCompanyDict(onlyActive, addEmptyRow), { useCache });
  }

  getInvoiceProductsSmall(excludeExternal: boolean) {
    return this.http.get<IProductSmallDTO[]>(getProductsSmall());
  }

  getCustomerStatistics(customerId: number, allItemSelection: number) {
    const model = { CustomerId: customerId, AllItemSelection: allItemSelection };
    return this.http.post<ICustomerStatisticsDTO[]>(getCustomerStatistics(), model);
  }
  
  //#endregion
}
