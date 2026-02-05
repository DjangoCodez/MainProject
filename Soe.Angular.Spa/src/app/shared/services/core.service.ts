import { Injectable } from '@angular/core';
import {
  CACHE_EXPIRE_VERY_SHORT,
  CacheSettingsFactory,
  SoeHttpClient,
} from './http.service';
import { Observable } from 'rxjs';
import {
  CompanySettingType,
  Feature,
  LicenseSettingType,
  SoeModule,
  TermGroup_AttestEntity,
  TermGroup_SysContactAddressType,
  TermGroup_SysContactType,
  UserSettingType,
} from '../models/generated-interfaces/Enumerations';
import {
  hasModifyPermissions,
  hasReadOnlyPermissions,
} from './generated-service-endpoints/core/Feature.endpoints';
import {
  getTermGroupContent,
  getTranslationPart,
} from './generated-service-endpoints/core/Term.endpoints';
import {
  getCategoriesDict,
  getCategoriesGrid,
  getCategoryAccounts,
} from './generated-service-endpoints/core/Category.endpoints';
import {
  getUserAndCompanySettings,
  getLicenseSettings,
  saveBoolSetting,
  saveIntSetting,
  getCompanySettings,
  getUserSettings,
  saveStringSetting,
  getBoolSetting,
} from './generated-service-endpoints/core/Settings.endpoints';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import {
  IAccountDimSmallDTO,
  IAccountDistributionTraceViewDTO,
  IAccountYearDTO,
  IAttestStateDTO,
  ICategoryAccountDTO,
  ICategoryDTO,
  ICompanyDTO,
  ICompCurrencyDTO,
  ICompCurrencySmallDTO,
  IContractTraceViewDTO,
  IEmailTemplateDTO,
  IHouseholdTaxDeductionApplicantDTO,
  IImagesDTO,
  IOfferTraceViewDTO,
  IPaymentTraceViewDTO,
  IPriceOptimizationTraceDTO,
  IProjectTraceViewDTO,
  IPurchaseTraceViewDTO,
  IUserSmallDTO,
  IVoucherTraceViewDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { getAccountDimsSmall } from './generated-service-endpoints/economy/Account.endpoints';
import {
  getContactAddresses,
  getSysContactAddressRowTypeIds,
  getSysContactAddressTypeIds,
  getSysContactEComTypeIds,
} from './generated-service-endpoints/core/ContactAddress.endpoints';
import { IContactAddressDTO } from '@shared/models/generated-interfaces/ContactDTO';
import { SaveUserCompanySettingModel } from '@shared/components/select-project-dialog/models/select-project-dialog.model';
import { getSysCountries } from './generated-service-endpoints/core/SysCountry.endpoints';
import { getSysLanguages } from './generated-service-endpoints/core/SysLanguage.endpoints';
import {
  getUser,
  getSmallDTOUsers,
  getSmallGenericUsers,
} from './generated-service-endpoints/manage/UserV2.endpoints';
import { generateReportForEdi } from './generated-service-endpoints/economy/SupplierV2.endpoints';
import { getVoucherTaceViews } from './generated-service-endpoints/billing/InvoiceVoucher.endpoints';
import {
  getInvoiceTraceViews,
  getInvoices,
} from './generated-service-endpoints/economy/SupplierInvoice.endpoints';
import {
  InvoiceTraceViewDTO,
  OrderTraceViewDTO,
} from '@shared/components/trace-rows/models/trace-rows.model';
import { getOrderTraceViews } from './generated-service-endpoints/billing/OrderV2.endpoints';
import { getPaymentTraceViews } from './generated-service-endpoints/billing/InvoicePayment.endpoints';
import { getProjectTraceViews } from './generated-service-endpoints/billing/InvoiceProject.endpoints';
import { getAccountDistributionTraceViews } from './generated-service-endpoints/economy/AccountDistribution.endpoints';
import { getOfferTraceViews } from './generated-service-endpoints/billing/OfferV2.endpoints';
import { getContractTraceViews } from './generated-service-endpoints/billing/ContractGroup.endpoints';
import { getPurchaseTraceViews } from './generated-service-endpoints/billing/PurchaseOrders.endpoints';
import {
  getEmailTemplates,
  getEmailTemplatesByType,
} from './generated-service-endpoints/core/EmailTemplates.endpoints';

import {
  getCompCurrencies,
  getEnterpriseCurrency,
  getCompCurrencyRate,
  getLedgerCurrency,
  getCompCurrenciesDictSmall,
  getCompanyCurrency,
  getCompCurrenciesDict,
} from './generated-service-endpoints/core/CoreCurrency.endpoints';
import { ISupplierInvoiceGridDTO } from '@shared/models/generated-interfaces/SupplierInvoiceDTOs';
import { ICustomerInvoicesGridModel } from '@shared/models/generated-interfaces/EconomyModels';
import { getImages } from './generated-service-endpoints/core/Image.endpoints';
import { getInvoicesForProjectCentral } from './generated-service-endpoints/core/CustomerInvoices.endpoints';
import {
  uploadInvoiceFile,
  uploadInvoiceFileByEntityType,
} from './generated-service-endpoints/core/File.endpoints';
import { getCustomerInvoiceRowsSmallForInvoice } from './generated-service-endpoints/core/CustomerInvoices.endpoints';
import { uploadFile } from './generated-service-endpoints/core/File.endpoints';
import {
  ICustomerInvoiceGridDTO,
  ICustomerInvoiceRowDetailDTO,
} from '@shared/models/generated-interfaces/CustomerInvoiceDTOs';
import { getProgressInfo } from './generated-service-endpoints/core/Process.endpoints';
import { ISoeProgressInfo } from '@shared/models/generated-interfaces/Monitoring';
import {
  getInitialAttestState,
  getUserValidAttestStates,
} from './generated-service-endpoints/manage/AttestState.endpoints';
import { UserCompanySettingCollection } from '@shared/util/settings-util';
import { HttpHeaders } from '@angular/common/http';
import { getAttestStates } from './generated-service-endpoints/manage/AttestState.endpoints';
import { getHouseholdTaxDeductionRowsByCustomer } from './generated-service-endpoints/billing/HouseholdTaxDeduction.endpoints';
import { getCurrentAccountYear } from './generated-service-endpoints/economy/AccountYear.endpoints';
import { getCompany } from './generated-service-endpoints/core/Company.endpoints';
import { getScheduledJobHeadsDict } from './generated-service-endpoints/core/ScheduledJob.endpoints';
import { getPriceOptimizationTraceRows } from './generated-service-endpoints/billing/PriceOptimization.endpoints';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class CoreService {
  constructor(private http: SoeHttpClient) {}

  getCurrentAccountYear(): Observable<IAccountYearDTO> {
    return this.http.get<IAccountYearDTO>(getCurrentAccountYear());
  }

  hasReadOnlyPermissions(features: Feature[]): Observable<any> {
    return this.http.get<any>(hasReadOnlyPermissions(features.join(',')), {
      useCache: true,
      cacheOptions: { expires: CACHE_EXPIRE_VERY_SHORT },
    });
  }

  hasModifyPermissions(features: Feature[]): Observable<any> {
    return this.http.get<any>(hasModifyPermissions(features.join(',')), {
      useCache: true,
      cacheOptions: { expires: CACHE_EXPIRE_VERY_SHORT },
    });
  }

  getProgressInfo(key: any): Observable<ISoeProgressInfo> {
    return this.http.get(getProgressInfo(key));
  }

  getTranslationPart(
    lang: string,
    part: string
  ): Observable<{ [translationKey: string]: string }> {
    return this.http.get<any>(getTranslationPart(lang, part));
  }

  getTermGroupContent(
    sysTermGroupId: number,
    addEmptyRow: boolean,
    skipUnknown: boolean,
    sortById = false,
    useCache = false
  ): Observable<SmallGenericType[]> {
    return this.http.get<SmallGenericType[]>(
      getTermGroupContent(sysTermGroupId, addEmptyRow, skipUnknown, sortById),
      { useCache }
    );
  }

  getUserAndCompanySettings(
    settingTypes: UserSettingType[],
    useCache?: boolean
  ): Observable<UserCompanySettingCollection | any> {
    if (useCache === undefined) {
      useCache = true;
    }
    return this.http.get(getUserAndCompanySettings(settingTypes.join(',')));
  }

  getUserSettings(
    settingTypes: UserSettingType[],
    useCache?: boolean
  ): Observable<UserCompanySettingCollection | any> {
    if (useCache === undefined) {
      useCache = true;
    }
    return this.http.get(getUserSettings(settingTypes.join(',')));
  }

  getCompanySettings(
    settingTypes: CompanySettingType[],
    useCache?: boolean
  ): Observable<UserCompanySettingCollection | any> {
    if (useCache === undefined) {
      useCache = true;
    }
    return this.http.get(getCompanySettings(settingTypes.join(',')));
  }

  getLicenseSettings(
    settingTypes: LicenseSettingType[],
    useCache?: boolean
  ): Observable<UserCompanySettingCollection | any> {
    if (useCache === undefined) {
      useCache = true;
    }
    return this.http.get(getLicenseSettings(settingTypes.join(',')));
  }

  getBoolSetting(
    settingMainType: number,
    settingType: number
  ): Observable<UserCompanySettingCollection | any> {
    return this.http.get(getBoolSetting(settingMainType, settingType));
  }

  getCompany(actorCompanyId: number): Observable<ICompanyDTO | any> {
    return this.http.get(getCompany(actorCompanyId));
  }

  getUserCompanySettingForCompany(
    settingTypes: CompanySettingType[],
    useCache?: boolean
  ): Observable<UserCompanySettingCollection | any> {
    if (useCache === undefined) {
      useCache = true;
    }
    return this.http.get(getCompanySettings(settingTypes.join(',')));
  }

  getCategoriesGrid(
    soeCategoryTypeId: number,
    loadCompanyCategoryRecord: boolean,
    loadChildren: boolean,
    loadCategoryGroups: boolean
  ): Observable<ICategoryDTO[]> {
    return this.http.get<ICategoryDTO[]>(
      getCategoriesGrid(
        soeCategoryTypeId,
        loadCompanyCategoryRecord,
        loadChildren,
        loadCategoryGroups
      )
    );
  }

  getCategoriesDict(
    soeCategoryTypeId: number,
    addEmptyRow: boolean,
    useCache = false
  ): Observable<ISmallGenericType[]> {
    const options = useCache ? CacheSettingsFactory.long() : {};
    return this.http.get<ISmallGenericType[]>(
      getCategoriesDict(soeCategoryTypeId, addEmptyRow),
      options
    );
  }

  getCategoryAccounts(
    accountId: number,
    loadCategory: boolean
  ): Observable<ICategoryAccountDTO[]> {
    return this.http.get<ICategoryAccountDTO[]>(
      getCategoryAccounts(accountId, loadCategory)
    );
  }

  getAccountDimsSmall(
    onlyStandard: boolean,
    onlyInternal: boolean,
    loadAccounts: boolean,
    loadInternalAccounts: boolean,
    loadParent: boolean,
    loadInactives: boolean,
    loadInactiveDims: boolean,
    includeParentAccounts: boolean,
    useCache = true,
    ignoreHierarchyOnly: boolean = false,
    actorCompanyId: number = 0,
    includeOrphanAccounts: boolean = false
  ): Observable<IAccountDimSmallDTO[]> {
    return this.http.get<IAccountDimSmallDTO[]>(
      getAccountDimsSmall(
        onlyStandard,
        onlyInternal,
        loadAccounts,
        loadInternalAccounts,
        loadParent,
        loadInactives,
        loadInactiveDims,
        includeParentAccounts,
        ignoreHierarchyOnly,
        actorCompanyId,
        includeOrphanAccounts
      ),
      { useCache: useCache }
    );
  }

  getAddressRowTypes(
    sysContactTypeId: TermGroup_SysContactType
  ): Observable<{ field1: number; field2: number }[]> {
    return this.http.get<{ field1: number; field2: number }[]>(
      getSysContactAddressRowTypeIds(sysContactTypeId),
      { useCache: true }
    );
  }

  getAddressTypes(
    sysContactTypeId: TermGroup_SysContactType
  ): Observable<number[]> {
    return this.http.get<number[]>(
      getSysContactAddressTypeIds(sysContactTypeId),
      { useCache: true }
    );
  }

  getEComTypes(
    sysContactTypeId: TermGroup_SysContactType
  ): Observable<number[]> {
    return this.http.get<number[]>(getSysContactEComTypeIds(sysContactTypeId), {
      useCache: true,
    });
  }

  getContactAddresses(
    actorId: number,
    type: TermGroup_SysContactAddressType,
    addEmptyRow: boolean,
    includeRows: boolean,
    includeCareOf: boolean,
    useCache: boolean = false
  ): Observable<IContactAddressDTO[]> {
    return this.http.get<IContactAddressDTO[]>(
      getContactAddresses(
        actorId,
        type,
        addEmptyRow,
        includeRows,
        includeCareOf
      ),
      { useCache }
    );
  }

  saveBoolSetting(model: SaveUserCompanySettingModel): Observable<any> {
    return this.http.post<SaveUserCompanySettingModel>(
      saveBoolSetting(),
      model
    );
  }

  saveIntSetting(model: any): Observable<any> {
    return this.http.post<any>(saveIntSetting(), model);
  }

  saveStringSetting(model: any): Observable<any> {
    return this.http.post<any>(saveStringSetting(), model);
  }

  getCountries(
    addEmptyRow: boolean,
    onlyUsedLanguages: boolean
  ): Observable<SmallGenericType[]> {
    return this.http.get(getSysCountries(addEmptyRow, onlyUsedLanguages), {
      useCache: true,
    });
  }

  getLanguages(addEmptyRow: boolean): Observable<SmallGenericType[]> {
    return this.http.get(getSysLanguages(addEmptyRow), { useCache: true });
  }

  getUsers(
    setDefaultRoleName: boolean,
    active?: boolean,
    skipNonEmployeeUsers?: boolean,
    includeEmployeesWithSameAccountOnAttestRole?: boolean,
    includeEmployeeCategories?: boolean,
    showEnded?: boolean
  ): Observable<IUserSmallDTO[]> {
    return this.http.get<IUserSmallDTO[]>(
      getSmallDTOUsers(
        setDefaultRoleName,
        active ?? false,
        skipNonEmployeeUsers ?? false,
        includeEmployeesWithSameAccountOnAttestRole ?? false,
        includeEmployeeCategories ?? false,
        showEnded ?? false
      )
    );
  }

  getUsersDict(
    addEmptyRow: boolean,
    includeKey: boolean,
    useFullName: boolean,
    includeLoginName: boolean,
    useCache: boolean
  ): Observable<ISmallGenericType[]> {
    return this.http.get<ISmallGenericType[]>(
      getSmallGenericUsers(
        addEmptyRow,
        includeKey,
        useFullName,
        includeLoginName
      ),
      { useCache }
    );
  }

  getUser(userId: number): Observable<IUserSmallDTO> {
    return this.http.get<IUserSmallDTO>(getUser(userId));
  }

  generateReportForEdi(ediEntryIds: number[]): Observable<any> {
    return this.http.post<any>(generateReportForEdi(ediEntryIds), ediEntryIds);
  }

  getVoucherTaceViews(
    voucherHeadId: number
  ): Observable<IVoucherTraceViewDTO[]> {
    return this.http.get<IVoucherTraceViewDTO[]>(
      getVoucherTaceViews(voucherHeadId)
    );
  }

  //#region SupplierInvoice
  getInvoiceTraceViews(invoiceId: number): Observable<InvoiceTraceViewDTO[]> {
    return this.http.get<InvoiceTraceViewDTO[]>(
      getInvoiceTraceViews(invoiceId)
    );
  }

  getInvoices(
    loadOpen: boolean,
    loadClosed: boolean,
    onlyMine: boolean,
    allItemsSelection: number,
    includeChildProjects: boolean,
    projectId?: number
  ): Observable<ISupplierInvoiceGridDTO[]> {
    return this.http.get<ISupplierInvoiceGridDTO[]>(
      getInvoices(
        loadOpen,
        loadClosed,
        onlyMine,
        allItemsSelection,
        projectId ? projectId : 0,
        includeChildProjects
      )
    );
  }

  //#endregion SupplierInvoice

  getOrderTraceViews(orderId: number): Observable<OrderTraceViewDTO[]> {
    return this.http.get<OrderTraceViewDTO[]>(getOrderTraceViews(orderId));
  }

  getPaymentTraceViews(
    paymentRowId: number
  ): Observable<IPaymentTraceViewDTO[]> {
    return this.http.get<IPaymentTraceViewDTO[]>(
      getPaymentTraceViews(paymentRowId)
    );
  }
  getProjectTraceViews(projectId: number): Observable<IProjectTraceViewDTO[]> {
    return this.http.get<IProjectTraceViewDTO[]>(
      getProjectTraceViews(projectId)
    );
  }
  getAccountDistributionTraceViews(
    accountDistributionHeadId: number
  ): Observable<IAccountDistributionTraceViewDTO[]> {
    return this.http.get<IAccountDistributionTraceViewDTO[]>(
      getAccountDistributionTraceViews(accountDistributionHeadId)
    );
  }
  getOfferTraceViews(offerId: number): Observable<IOfferTraceViewDTO[]> {
    return this.http.get<IOfferTraceViewDTO[]>(getOfferTraceViews(offerId));
  }
  getContractTraceViews(
    contractId: number
  ): Observable<IContractTraceViewDTO[]> {
    return this.http.get<IContractTraceViewDTO[]>(
      getContractTraceViews(contractId)
    );
  }

  getPriceOptimizationTraceViews(
    priceOptimizationId: number
  ): Observable<IPriceOptimizationTraceDTO[]> {
    return this.http.get<IPriceOptimizationTraceDTO[]>(
      getPriceOptimizationTraceRows(priceOptimizationId)
    );
  }

  getPurchaseTraceViews(
    purchaseId: number
  ): Observable<IPurchaseTraceViewDTO[]> {
    return this.http.get<IPurchaseTraceViewDTO[]>(
      getPurchaseTraceViews(purchaseId)
    );
  }

  getEmailTemplatesByType(type: number): Observable<IEmailTemplateDTO[]> {
    return this.http.get<IEmailTemplateDTO[]>(getEmailTemplatesByType(type));
  }

  getEmailTemplates(): Observable<IEmailTemplateDTO[]> {
    return this.http.get<IEmailTemplateDTO[]>(getEmailTemplates());
  }

  //#region Currency

  getCompCurrencies(
    loadRates: boolean,
    useCache = true
  ): Observable<ICompCurrencyDTO[]> {
    return this.http.get<ICompCurrencyDTO[]>(getCompCurrencies(loadRates), {
      useCache,
    });
  }

  getEnterpriseCurrency(useCache = true): Observable<ICompCurrencyDTO> {
    return this.http.get<ICompCurrencyDTO>(getEnterpriseCurrency(), {
      useCache,
    });
  }

  getCompCurrencyRate(
    sysCurrencyId: number,
    date: string,
    rateToBase: boolean,
    useCache = true
  ): Observable<number> {
    return this.http.get<number>(
      getCompCurrencyRate(sysCurrencyId, date, rateToBase),
      {
        useCache,
      }
    );
  }

  getLedgerCurrency(
    actorId: number,
    useCache = true
  ): Observable<ICompCurrencyDTO> {
    return this.http.get<ICompCurrencyDTO>(getLedgerCurrency(actorId), {
      useCache,
    });
  }

  getBaseCurrency(useCache = true): Observable<ICompCurrencyDTO> {
    const options = useCache ? CacheSettingsFactory.long() : {};
    return this.http.get<ICompCurrencyDTO>(getCompanyCurrency(), options);
  }

  getCompCurrenciesSmall(useCache = true): Observable<ICompCurrencySmallDTO[]> {
    return this.http.get<ICompCurrencySmallDTO[]>(
      getCompCurrenciesDictSmall(),
      {
        useCache,
      }
    );
  }

  getCompCurrenciesDict(addEmptyRow: boolean): Observable<SmallGenericType[]> {
    return this.http.get<SmallGenericType[]>(
      getCompCurrenciesDict(addEmptyRow)
    );
  }

  //#endregion

  //#region CustomerInvoices
  getCustomerInvoices(
    model: ICustomerInvoicesGridModel
  ): Observable<ICustomerInvoiceGridDTO[]> {
    return this.http.post<ICustomerInvoiceGridDTO[]>(
      getInvoicesForProjectCentral(),
      model
    );
  }

  getCustomerInvoiceRowsSmall(
    invoiceId: number
  ): Observable<ICustomerInvoiceRowDetailDTO[]> {
    return this.http.get<ICustomerInvoiceRowDetailDTO[]>(
      getCustomerInvoiceRowsSmallForInvoice(invoiceId)
    );
  }

  //#endregion

  //#region HouseholdTaxDeduction

  getHouseholdTaxDeductionRowsByCustomer(
    customerId: number,
    addEmptyRow: boolean,
    showAllApplicants: boolean,
    useCache: boolean
  ): Observable<IHouseholdTaxDeductionApplicantDTO[]> {
    return this.http.get<IHouseholdTaxDeductionApplicantDTO[]>(
      getHouseholdTaxDeductionRowsByCustomer(
        customerId,
        addEmptyRow,
        showAllApplicants
      ),
      { useCache }
    );
  }

  //endregion

  //#region Images
  getImages(
    imageType: number,
    entity: number,
    recordId: number,
    useThumbnails: boolean,
    projectId: number
  ): Observable<IImagesDTO[]> {
    return this.http.get<IImagesDTO[]>(
      getImages(imageType, entity, recordId, useThumbnails, projectId)
    );
  }
  //#endregion

  //#region  AttestStates
  getAttestStates(
    entity: TermGroup_AttestEntity,
    module: SoeModule,
    addEmptyRow: boolean
  ): Observable<IAttestStateDTO[]> {
    return this.http.get(getAttestStates(entity, module, false));
  }

  getUserValidAttestStates(
    entity: number,
    dateFrom: string,
    dateTo: string,
    excludePayrollStates: boolean,
    employeeGroupId?: number
  ): Observable<IAttestStateDTO[]> {
    return this.http.get(
      getUserValidAttestStates(
        entity,
        dateFrom,
        dateTo,
        excludePayrollStates,
        employeeGroupId
      )
    );
  }
  //#endregion

  //#region File
  uploadFile(entity: number, type: number): Observable<any> {
    return this.http.post<any>(uploadFile(entity, type), entity, type);
  }

  uploadInvoiceFile(
    entity: number,
    type: number,
    recordId: number,
    binaryContent: Uint8Array,
    fileName: string,
    extractZip = false
  ) {
    const formData = new FormData();
    // Ensure binaryContent is a valid BlobPart (Uint8Array backed by ArrayBuffer)
    const uint8 = new Uint8Array(binaryContent);
    const fileBlob = new Blob([uint8]);
    formData.append('file', fileBlob, fileName);

    const headers = new HttpHeaders();

    return this.http.post<BackendResponse | BackendResponse[]>(
      uploadInvoiceFile(entity, type, recordId, extractZip),
      formData,
      { headers }
    );
  }

  uploadInvoiceFileByEntityType(
    entity: number,
    binaryContent: Uint8Array,
    name: string
  ): Observable<BackendResponse> {
    const formData = new FormData();
    // Ensure binaryContent is a valid BlobPart (Uint8Array backed by ArrayBuffer)
    const uint8 = new Uint8Array(binaryContent);
    const fileBlob = new Blob([uint8]);
    formData.append('file', fileBlob, name);

    const headers = new HttpHeaders();
    return this.http.post<BackendResponse>(
      uploadInvoiceFileByEntityType(entity),
      formData,
      { headers }
    );
  }
  //#endregion

  getAttestStateInitial(entity: number): Observable<IAttestStateDTO> {
    return this.http.get<IAttestStateDTO>(getInitialAttestState(entity));
  }

  getScheduledJobHeadsDict(
    addEmptyRow: boolean,
    includeSharedOnLicense: boolean
  ): Observable<SmallGenericType[]> {
    return this.http.get<SmallGenericType[]>(
      getScheduledJobHeadsDict(addEmptyRow, includeSharedOnLicense)
    );
  }
}
