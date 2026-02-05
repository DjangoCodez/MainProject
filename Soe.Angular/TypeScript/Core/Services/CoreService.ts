import { IHttpService } from "./HttpService";
import { CoreUtility } from "../../Util/CoreUtility";
import { CalendarUtility } from "../../Util/CalendarUtility";
import { ISoeProgressInfo, IInvoiceTraceViewDTO, IVoucherTraceViewDTO, IPaymentTraceViewDTO, IProjectTraceViewDTO, IOrderTraceViewDTO, IAttestTransitionDTO, ISmallGenericType, IChecklistHeadDTO, IChecklistExtendedRowDTO, IImagesDTO, ICheckListMultipleChoiceAnswerRowDTO, IContactPersonDTO, IEmailTemplateDTO, IEvaluateWorkRulesActionResult, IActorSearchPersonDTO, IActionResult, IOfferTraceViewDTO, ICustomerInvoiceRowDetailDTO, IContractTraceViewDTO, IEmployeeSmallDTO, IFilesLookupDTO } from "../../Scripts/TypeLite.Net4";
import { ChecklistHeadRecordCompactDTO } from "../../Common/Models/ChecklistDTO";
import { SysHelpDTO } from "../Models/SysHelpDTO";
import { UserGaugeSettingDTO, WantedShiftsGaugeDTO, MyShiftsGaugeDTO, OpenShiftsGaugeDTO, EmployeeRequestsGaugeDTO, TimeStampAttendanceGaugeDTO, DashboardStatisticType, DashboardStatisticsDTO, DashboardStatisticRowDTO, DashboardStatisticPeriodDTO } from "../../Common/Models/DashboardDTOs";
import { TextBlockDTO, ProductRowDTO } from "../../Common/Models/InvoiceDTO";
import { IStorageService } from "./StorageService";
import { SoeCategoryType, TermGroup_SysContactType, Feature, SoeEntityImageType, SoeEntityType, CompanySettingType, UserSettingType, SettingMainType, CompTermsRecordType, TermGroup_AttestEntity, SoeModule, SoeOriginType, SoeOriginStatusClassification, OrderInvoiceRegistrationType, TermGroup_ChecklistHeadType, TermGroup_Sex, TermGroup_TimeStampAttendanceGaugeShowMode, TermGroup_PersonalDataInformationType, TermGroup_PersonalDataActionType, UserReplacementType, TermGroup_TimeScheduleTemplateBlockType, SoeCategoryRecordEntity, TermGroup_PerformanceTestInterval, TermGroup_TaskWatchLogResultCalculationType, SoeDataStorageRecordType, LicenseSettingType, TermGroup_EventHistoryType, SigneeStatus, UserSelectionType, IntrastatReportingType, TermGroup_SignatoryContractPermissionType, TermGroup_InvoiceRowImportType } from "../../Util/CommonEnumerations";
import { Constants } from "../../Util/Constants";
import { TrackChangesDTO, TrackChangesLogDTO } from "../../Common/Models/TrackChangesDTO";
import { PersonalDataLogMessageDTO } from "../../Common/Models/PersonalDataLogMessageDTO";
import { MessageEditDTO, MessageRecipientDTO, MessageAttachmentDTO, MessageGroupDTO, MessageGridDTO } from "../../Common/Models/MessageDTOs";
import { AccountDTO } from "../../Common/Models/AccountDTO";
import { AccountDimSmallDTO } from "../../Common/Models/AccountDimDTO";
import { UserRolesDTO, CompanyRolesDTO, UserAttestRoleDTO, UserCompanyRoleDTO, CompanyAttestRoleDTO } from "../../Common/Models/EmployeeUserDTO";
import { ActorSearchPersonDTO } from "../../Common/Models/ActorSearchPersonDTO";
import { UserReplacementDTO, UserSmallDTO } from "../../Common/Models/UserDTO";
import { AttestStateDTO } from "../../Common/Models/AttestStateDTO";
import { CategoryDTO, CompanyCategoryRecordDTO } from "../../Common/Models/Category";
import { AccountingSettingsRowDTO } from "../../Common/Models/AccountingSettingsRowDTO";
import { TimeCodeAdditionDeductionDTO } from "../../Common/Models/TimeCode";
import { PositionDTO, SysPositionGridDTO } from "../../Common/Models/EmployeePositionDTO";
import { ShiftDTO } from "../../Common/Models/TimeSchedulePlanningDTOs";
import { EmployeeRequestDTO } from "../../Common/Models/EmployeeRequestDTO";
import { SmallGenericType } from "../../Common/Models/SmallGenericType";
import { SysPositionDTO } from "../../Common/Models/PositionDTOs";
import { DocumentDTO, DataStorageRecipientDTO } from "../../Common/Models/DocumentDTOs";
import { InformationDTO, InformationRecipientDTO, SysInformationSysCompDbDTO } from "../../Common/Models/InformationDTOs";
import { UserCompanySettingEditDTO } from "../../Common/Models/UserCompanySettingsDTOs";
import { EventHistoryDTO } from "../../Common/Models/EventHistoryDTO";
import { ContactPersonDTO } from "../../Common/Models/ContactPersonDTOs";
import { ExtraFieldDTO, ExtraFieldGridDTO, ExtraFieldRecordDTO } from "../../Common/Models/ExtraFieldDTO";
import { BatchUpdateDTO, BatchUpdateModel } from "../../Common/Models/BatchUpdateDTO";
import { AttestWorkFlowHeadDTO, AttestWorkFlowRowDTO, AttestWorkFlowTemplateRowDTO } from "../../Common/Models/AttestWorkFlowDTOs";
import { TimeTerminalDTO } from "../../Common/Models/TimeTerminalDTO";
import { UserSelectionDTO } from "../../Common/Models/UserSelectionDTOs";
import { ReportPrintoutDTO } from "../../Common/Models/ReportDTOs";
import { AuthenticationResponseDTO, AuthenticationResultDTO, GetPermissionResultDTO } from "../../Common/Models/SignatoryContractDTO";

export interface ICoreService {

    // GET
    getProgressInfo(key: any): ng.IPromise<ISoeProgressInfo>;
    getAddressRowTypes(sysContactTypeId: TermGroup_SysContactType): ng.IPromise<any>;
    getAddressTypes(sysContactTypeId: TermGroup_SysContactType): ng.IPromise<any>;
    getAccountDimsSmall(onlyStandard: boolean, onlyInternal: boolean, loadAccounts: boolean, loadInternalAccounts: boolean, loadParent: boolean, includeParentAccounts: boolean, useCache?: boolean, ignoreHierarchyOnly?: boolean, companyId?: number): ng.IPromise<AccountDimSmallDTO[]>;
    getAccountDim(accountDimId: number, loadAccounts: boolean, loadInternalAccounts: boolean, loadInactiveDims: boolean, useCache?: boolean, ignoreHierarchyOnly?: boolean): ng.IPromise<AccountDimSmallDTO>;
    getAttestTransitionGridDTOs(entity: TermGroup_AttestEntity, module: SoeModule, setEntityName: boolean): ng.IPromise<any>;
    getAttestWorkFlowHead(attestWorFlowHeadId: number, loadRows: boolean): ng.IPromise<AttestWorkFlowHeadDTO>;
    getAttestWorkFlowTemplateRows(attestWorkFlowTemplateId: number): ng.IPromise<AttestWorkFlowTemplateRowDTO[]>;
    getAttestWorkFlowRowsFromRecordId(entity: SoeEntityType, recordId: number): ng.IPromise<AttestWorkFlowRowDTO[]>;
    hasAttestWorkFlowTemplates(entity: TermGroup_AttestEntity): ng.IPromise<boolean>;
    getDocumentSigningStatus(entity: SoeEntityType, recordId: number): ng.IPromise<AttestWorkFlowRowDTO[]>;
    getCurrentAccountYear(): ng.IPromise<any>;
    getAccountYearById(accountYearId: number): ng.IPromise<any>;
    getAccountYearDict(addEmptyRow: boolean): ng.IPromise<any>;
    getAccountPeriodDict(accountYearId: number, addEmptyRow: boolean): ng.IPromise<any>;
    getAccountDimStd(): ng.IPromise<any>
    getEComTypes(sysContactTypeId: TermGroup_SysContactType): ng.IPromise<any>;
    getEmployeeGroups(): ng.IPromise<any>;
    getEmployeeGroupsDict(addEmptyRow: boolean): ng.IPromise<ISmallGenericType[]>;
    getEmployeeForUser(): ng.IPromise<IEmployeeSmallDTO>;
    getAssemblyDate(): ng.IPromise<any>;
    getAssemblyVersion(): ng.IPromise<any>;
    getServerTime(): ng.IPromise<Date>;
    getSiteType(): ng.IPromise<number>;
    getCategories(soeCategoryTypeId: SoeCategoryType, loadCompanyCategoryRecord: boolean, loadChildren: boolean, loadCategoryGroups: boolean, useCache: boolean): ng.IPromise<CategoryDTO[]>;
    getCategoriesDict(soeCategoryTypeId: SoeCategoryType, addEmptyRow: boolean): ng.IPromise<SmallGenericType[]>;
    getCategoriesForRoleFromType(employeeId: number, categoryType: SoeCategoryType, isAdmin: boolean, includeSecondary: boolean, addEmptyRow: boolean): ng.IPromise<any>;
    getCategoryAccountsByAccount(accountId: number, loadCategory: boolean): ng.IPromise<any>;
    getCompanyCategoryRecords(categoryType: SoeCategoryType, categoryRecordEntity: SoeCategoryRecordEntity, recordId: number, useCache): ng.IPromise<CompanyCategoryRecordDTO[]>;
    getSysCurrenciesDict(addEmptyRow: boolean, useCode?: boolean): ng.IPromise<any>;
    getCompCurrencies(loadRates: boolean): ng.IPromise<any>;
    getCompCurrenciesDict(addEmptyRow: boolean): ng.IPromise<any>;
    getCompCurrenciesGrid(): ng.IPromise<any>;
    getCompCurrenciesSmall(): ng.IPromise<any>;
    getCompCurrencyRates(currencyId: number): ng.IPromise<any>;
    getCompCurrencyRate(sysCurrencyId: number, date: Date, rateToBase: boolean): ng.IPromise<any>;
    getCompanyBaseCurrency(): ng.IPromise<any>;
    getEnterpriseCurrency(): ng.IPromise<any>;
    getLedgerCurrency(actorId: number): ng.IPromise<any>;
    getContactPersonsForActor(actorId: number, loadPositionName: boolean, useCache: boolean): ng.IPromise<any>;
    getFormHelp(formName: string): ng.IPromise<any>;
    hasHelp(feature: Feature): ng.IPromise<boolean>;
    getHelp(feature: Feature): ng.IPromise<any>;
    getHelpTitles(): ng.IPromise<any>;
    getHouseholdTaxDeductionRowsByCustomer(customerId: number, addEmptyRow: boolean, showAllApplicants: boolean): ng.IPromise<any>;
    getImages(imageType: SoeEntityImageType, entity: SoeEntityType, recordId: number, useThumbnails: boolean, projectId?: number): ng.IPromise<IImagesDTO[]>;
    getImage(imageId: number): ng.IPromise<IImagesDTO>;
    hasNewDocuments(time: Date): ng.IPromise<boolean>;
    getNbrOfUnreadCompanyDocuments(useCache: boolean): ng.IPromise<number>;
    getCompanyDocuments(): ng.IPromise<DocumentDTO[]>;
    getMyDocuments(): ng.IPromise<DocumentDTO[]>;
    getDocument(dataStorageId: number): ng.IPromise<DocumentDTO>;
    getDocumentUrl(dataStorageId: number): ng.IPromise<string>;
    getDocumentFolders(): ng.IPromise<string[]>;
    getDocumentData(dataStorageId: number): ng.IPromise<any>;
    getDocumentRecipientInfo(dataStorageId: number): ng.IPromise<DataStorageRecipientDTO[]>;
    getExcelProductRowsTemplate(): ng.IPromise<any>;
    getFileRoleIds(dataStorageRecordId: number): ng.IPromise<number[]>;
    getCompanyInformationForEdit(informationId: number): ng.IPromise<InformationDTO>;
    getCompanyInformationFolders(): ng.IPromise<string[]>;
    getCompanyInformationHasConfirmations(informationId: number): ng.IPromise<boolean>;
    getCompanyInformationRecipientInfo(informationId: number): ng.IPromise<InformationRecipientDTO[]>;
    hasNewInformations(time: string): ng.IPromise<boolean>;
    getNbrOfUnreadInformations(useCache: boolean): ng.IPromise<number>;
    getUnreadInformations(): ng.IPromise<InformationDTO[]>;
    hasSevereUnreadInformation(useCache: boolean): ng.IPromise<boolean>;
    getCompanyInformations(): ng.IPromise<InformationDTO[]>;
    getCompanyInformation(informationId: number): ng.IPromise<InformationDTO>;
    getSysInformations(): ng.IPromise<InformationDTO[]>;
    getSysInformation(sysInformationId: number): ng.IPromise<InformationDTO>;
    hasReadOnlyPermissions(features: Feature[]): ng.IPromise<any>;
    hasModifyPermissions(features: Feature[]): ng.IPromise<any>;
    getLicenseSettings(settingTypes: LicenseSettingType[], useCache?: boolean): ng.IPromise<any>;
    getLicenseSettingsForEdit(): ng.IPromise<UserCompanySettingEditDTO[]>;
    getCompanySettings(settingTypes: CompanySettingType[], useCache?: boolean): ng.IPromise<any>;
    getUserSettings(settingTypes: UserSettingType[], useCache?: boolean): ng.IPromise<any>;
    getUserAndCompanySettings(settingTypes: UserSettingType[], useCache?: boolean): ng.IPromise<any>;
    getBoolSetting(settingMainType: SettingMainType, settingType: number): ng.IPromise<any>;
    getIntSetting(settingMainType: SettingMainType, settingType: number): ng.IPromise<any>;
    getStringSetting(settingMainType: SettingMainType, settingType: number): ng.IPromise<any>;
    getBoolConfigSetting(name: string): ng.IPromise<boolean>;
    getSysGridState(grid: string): ng.IPromise<any>;
    getUserGridState(grid: string): ng.IPromise<any>;
    getStateAnalysis(analysisIds: number[]): ng.IPromise<any>;
    getSysCountries(addEmptyRow: boolean, onlyUsedLanguages: boolean): ng.IPromise<ISmallGenericType[]>;
    getSysHolidayTypes(): ng.IPromise<any>;
    getTermGroupContent(sysTermGroupId: number, addEmptyRow: boolean, skipUnknown: boolean, sortById?: boolean): ng.IPromise<ISmallGenericType[]>;
    getTimeTerminals(type: number, onlyActive: boolean, onlyRegistered: boolean, onlySynchronized: boolean, loadSettings: boolean, loadCompanies: boolean, loadTypeNames: boolean, ignoreLimitToAccount: boolean): ng.IPromise<TimeTerminalDTO[]>
    getTranslations(recordType: CompTermsRecordType, recordId: number, loadLangName: boolean): ng.IPromise<any>;
    getInvoiceTraceViews(invoiceId: number): ng.IPromise<IInvoiceTraceViewDTO[]>;
    getVoucherTraceViews(voucherHeadId: number): ng.IPromise<IVoucherTraceViewDTO[]>;
    getPaymentTraceViews(paymentId: number): ng.IPromise<IPaymentTraceViewDTO[]>;
    getProjectTraceViews(projectId: number): ng.IPromise<IProjectTraceViewDTO[]>;
    getOrderTraceViews(orderId: number): ng.IPromise<IOrderTraceViewDTO[]>;
    getOfferTraceViews(offerId: number): ng.IPromise<IOfferTraceViewDTO[]>;
    getContractTraceViews(contractId: number): ng.IPromise<IContractTraceViewDTO[]>;
    getPurchaseTraceViews(purchaseId: number): ng.IPromise<any[]>;
    getUsersForOrigin(includeEmployeeCategories?: boolean): ng.IPromise<any[]>;
    getUsers(setDefaultRoleName: boolean, active?: boolean, skipNonEmployeeUsers?: boolean, includeEmployeesWithSameAccountOnAttestRole?: boolean, includeEmployeesWithSameAccount?: boolean, includeEmployeeCategories?: boolean): ng.IPromise<UserSmallDTO[]>;
    getUsersDict(addEmptyRow: boolean, includeKey: boolean, useFullName: boolean, includeLoginName: boolean): ng.IPromise<ISmallGenericType[]>;
    getUsersByAvailability(setDefaultRoleName: boolean, active: boolean, byCompany: boolean, dateFrom?: Date, dateTo?: Date): ng.IPromise<any>;
    getUsersWithoutEmployees(companyId: number, includeUserId: number, addEmptyRow: boolean): ng.IPromise<ISmallGenericType[]>
    getUser(userId: number): ng.IPromise<UserSmallDTO>;
    getCurrentUser(): ng.IPromise<any>;
    getAttestStateInitial(entity: TermGroup_AttestEntity): ng.IPromise<AttestStateDTO>;
    getUserAttestTransitions(entity: TermGroup_AttestEntity, dateFrom?: Date, dateTo?: Date): ng.IPromise<IAttestTransitionDTO[]>;
    getAttestStates(entity: TermGroup_AttestEntity, module: SoeModule, addEmptyRow: boolean): ng.IPromise<AttestStateDTO[]>;
    getUserValidAttestStates(entity: TermGroup_AttestEntity, dateFrom: Date, dateTo: Date, excludePayrollStates: boolean, employeeGroupId?: number)
    hasInitialAttestState(entity: TermGroup_AttestEntity, module: SoeModule): ng.IPromise<boolean>;
    getAttestRolesDict(module: SoeModule, addEmptyRow: boolean): ng.IPromise<ISmallGenericType[]>;
    canUserCreateInvoice(currentAttestStateId: number): ng.IPromise<boolean>;
    getTextBlock(textBlockId: number): ng.IPromise<any>;
    getTextBlocks(entity: number): ng.IPromise<any>;
    getCustomerInvoiceNumbersDict(customerId: number, originType: SoeOriginType, classification: SoeOriginStatusClassification, registrationType: OrderInvoiceRegistrationType, orderByNumber?: boolean): ng.IPromise<ISmallGenericType[]>;
    getCustomerInvoiceRows(invoiceId: number): ng.IPromise<ProductRowDTO[]>;
    getCustomerInvoiceRowsSmall(invoiceId: number): ng.IPromise<ICustomerInvoiceRowDetailDTO[]>;
    getServiceOrdersForAgreementDetails(invoiceId: number): ng.IPromise<any[]>;
    getUserGagues(module: SoeModule): ng.IPromise<any>;
    getUserGaugeHead(userGaugeHeadId: number, module?: SoeModule): ng.IPromise<any>;
    getUserGaugeHeads(): ng.IPromise<any>;
    getSysGagues(module: SoeModule): ng.IPromise<any>;
    getCompanyRolesDict(addEmptyRow: boolean, addEmptyRowAsAll: boolean): ng.IPromise<ISmallGenericType[]>;
    getRolesByUserDict(actorCompanyId: number): ng.IPromise<ISmallGenericType[]>;
    getUserReplacement(type: UserReplacementType, originUserId: number): ng.IPromise<UserReplacementDTO>;
    getExtraFields(entity: SoeEntityType, useCache?: boolean): ng.IPromise<ExtraFieldGridDTO[]>;
    getExtraFieldsDict(entity: SoeEntityType, connectedEntity: SoeEntityType, connectedRecordId: number, addEmptyRow: boolean, useCache?: boolean): ng.IPromise<ISmallGenericType[]>;
    getExtraFieldsGrid(entity: SoeEntityType, loadRecords?: boolean, connectedEntity?: SoeEntityType, connectedRecordId?: number, useCache?: boolean): ng.IPromise<ExtraFieldGridDTO[]>;
    getExtraField(extraFieldId: number): ng.IPromise<ExtraFieldDTO>;
    getExtraFieldRecord(extraFieldId: number, recordId: number, entity: number): ng.IPromise<ExtraFieldRecordDTO>;
    getExtraFieldWithRecords(recordId: number, entity: number, langId: number, connectedEntity: number, connectedRecordId: number): ng.IPromise<ExtraFieldRecordDTO[]>;
    getBatchUpdate(entityType: SoeEntityType): ng.IPromise<BatchUpdateDTO[]>;
    getBatchUpdateFilterOptions(entityType: SoeEntityType): ng.IPromise<SmallGenericType[]>;
    getFileContent(fileType: number): ng.IPromise<IActionResult>;
    parseRows(val: any): ng.IPromise<any[]>;

    getPersonalDataLogsForEmployee(employeeId: number, informationType: TermGroup_PersonalDataInformationType, actionType: TermGroup_PersonalDataActionType, dateFrom: Date, dateTo: Date): ng.IPromise<PersonalDataLogMessageDTO[]>;
    getPersonalDataLogsCausedByEmployee(employeeId: number, recordId: number, informationType: TermGroup_PersonalDataInformationType, actionType: TermGroup_PersonalDataActionType, dateFrom: Date, dateTo: Date): ng.IPromise<PersonalDataLogMessageDTO[]>;
    getPersonalDataLogsCausedByUser(userId: number, recordId: number, informationType: TermGroup_PersonalDataInformationType, actionType: TermGroup_PersonalDataActionType, dateFrom: Date, dateTo: Date): ng.IPromise<PersonalDataLogMessageDTO[]>;
    getTrackChanges(entity: SoeEntityType, recordId: number, includeChildren: boolean): ng.IPromise<TrackChangesDTO[]>;
    getTrackChangesLogEntities(): ng.IPromise<ISmallGenericType[]>;
    getTrackChangesLog(entity: SoeEntityType, recordId: number, dateFrom: Date, dateTo: Date): ng.IPromise<TrackChangesLogDTO[]>;
    getTrackChangesLogForEntity(entity: SoeEntityType, dateFrom: Date, dateTo: Date, users: string[]): ng.IPromise<TrackChangesLogDTO[]>;
    searchPerson(searchString: string, categories: number[]): ng.IPromise<IActorSearchPersonDTO[]>;

    getEmployeeRequestsWidgetData(setEmployeeRequestTypeNames: boolean): ng.IPromise<EmployeeRequestsGaugeDTO[]>;
    getMyScheduleMyShifts(employeeId: number, dateFrom: Date, dateTo: Date): ng.IPromise<ShiftDTO[]>;
    getMyScheduleOpenShifts(employeeId: number, dateFrom: Date, dateTo: Date): ng.IPromise<ShiftDTO[]>;
    getMyScheduleColleaguesShifts(employeeId: number, dateFrom: Date, dateTo: Date): ng.IPromise<ShiftDTO[]>;
    getMyShiftsWidgetData(employeeId: number, dateFrom: Date, dateTo: Date): ng.IPromise<MyShiftsGaugeDTO[]>;
    getOpenShiftsWidgetData(employeeId: number, dateFrom: Date, dateTo: Date): ng.IPromise<OpenShiftsGaugeDTO[]>;
    getReportWidgetData(): ng.IPromise<any>;
    getDashboardStatisticServiceTypes(): ng.IPromise<ISmallGenericType[]>;
    getDashboardStatisticTypes(serviceType: number): ng.IPromise<DashboardStatisticType[]>;
    getPerformanceTestResults(dashboardStatisticTypeKey: string, dateFrom: Date, dateTo: Date, interval: TermGroup_PerformanceTestInterval): ng.IPromise<DashboardStatisticsDTO>;
    getTaskWatchLogTasks(dateFrom: Date, dateTo: Date, actorCompanyId?: number, userId?: number): ng.IPromise<string[]>;
    getTaskWatchLogResults(task: string, dateFrom: Date, dateTo: Date, interval: TermGroup_PerformanceTestInterval, calculationType: TermGroup_TaskWatchLogResultCalculationType, actorCompanyId?: number, userId?: number): ng.IPromise<DashboardStatisticsDTO>;
    getFileExist(entity: SoeEntityType, recordId: number, fileName: string): ng.IPromise<any>;
    getSysLogWidgetData(clientIpNr: string, nbrOfRecords: number): ng.IPromise<any>;
    getSystemInfoWidgetData(): ng.IPromise<any>;
    getWantedShiftsWidgetData(): ng.IPromise<WantedShiftsGaugeDTO[]>;
    getTimeStampAttendanceWidgetData(showMode: TermGroup_TimeStampAttendanceGaugeShowMode, onlyIn: boolean): ng.IPromise<TimeStampAttendanceGaugeDTO[]>;
    getAttestFlowWidgetData(): ng.IPromise<any>;
    getInsightsWidgetData(reportId: number, dataSelectionId: number, columnSelectionId: number): ng.IPromise<ReportPrintoutDTO>;
    getMapStartAddress(): ng.IPromise<any>;
    getMapLocations(dateFrom: Date): ng.IPromise<any>;
    getMapPlannedOrders(forDate: Date): ng.IPromise<any>;
    getXeMail(messageId: number): ng.IPromise<MessageEditDTO>;
    getXeMailItems(xeMailType: number, messageId?: number): ng.IPromise<MessageGridDTO[]>;
    getNbrOfUnreadMessages(): ng.IPromise<number>;
    getEmployeesByAccountSetting(): ng.IPromise<ISmallGenericType[]>;
    getMessageGroups(useCache: boolean): ng.IPromise<MessageGroupDTO[]>;
    getMessageGroupsDict(useCache: boolean): ng.IPromise<ISmallGenericType[]>;
    getMessageGroupUsersByAccount(accountId: number): ng.IPromise<ISmallGenericType[]>;
    getMessageGroupUsersByCategory(categoryId: number): ng.IPromise<ISmallGenericType[]>;
    getMessageGroupUsersByEmployeeGroup(employeeGroupId: number): ng.IPromise<ISmallGenericType[]>;
    getMessageGroupUsersByRole(roleId: number): ng.IPromise<ISmallGenericType[]>;
    getIdsForEmployeeAndGroup(): ng.IPromise<any>;
    getProjectTimeInvoiceTransactions(projectId: number): ng.IPromise<any>;
    getChecklistHeadsDict(type: TermGroup_ChecklistHeadType): ng.IPromise<ISmallGenericType[]>;
    getChecklistHeads(type: TermGroup_ChecklistHeadType, loadRows: boolean): ng.IPromise<IChecklistHeadDTO[]>;
    getChecklistHead(checkListHeadId: number, loadRows: boolean): ng.IPromise<IChecklistHeadDTO>;
    getChecklistHeadRecords(entity: SoeEntityType, recordId: number): ng.IPromise<ChecklistHeadRecordCompactDTO[]>;
    getChecklistRowRecords(entity: SoeEntityType, recordId: number): ng.IPromise<IChecklistExtendedRowDTO[]>;
    getChecklistSignatures(entity: SoeEntityType, recordId: number, useThumbnails: boolean): ng.IPromise<IImagesDTO[]>;
    getChecklistMultipleChoiceQuestions(questionIds: number[]): ng.IPromise<ICheckListMultipleChoiceAnswerRowDTO[]>;
    getInventoryTriggerAccounts(): ng.IPromise<any>;
    getSkills(useCache: boolean): ng.IPromise<any[]>;
    getEmployeePostSkills(employeePostId: number): ng.IPromise<any>;
    generateReportForEdi(ediEntries: any[]): ng.IPromise<any>;
    generateReportForFinvoice(ediEntries: any[]): ng.IPromise<any>;
    getLinkedShifts(shiftId): ng.IPromise<ShiftDTO[]>;
    getEmailTemplates(): ng.IPromise<any>;
    getEmailTemplatesByType(type: number): ng.IPromise<any>;
    getEmailTemplate(emailTemplateId: number): ng.IPromise<any>;
    getRecurrenceIntervalText(recurrenceInterval: string): ng.IPromise<string>;
    getNextExecutionTime(recurrenceInterval: string): ng.IPromise<Date>;
    getLastUsedSequenceNumber(entityName: string): ng.IPromise<any>;
    getSysLanguages(addEmptyRow: boolean, useCache: boolean): ng.IPromise<ISmallGenericType[]>;
    getSysPositions(countryId: number, langId: number, useCache: boolean): ng.IPromise<SysPositionGridDTO[]>;
    getSysPositionGrid(useCache: boolean): ng.IPromise<SysPositionGridDTO[]>;
    getSysPosition(sysPositionId: number): ng.IPromise<SysPositionDTO>;
    getFirstEligableTimeForEmployee(employeeId: number, date: Date): ng.IPromise<any>;
    getEmployeeScheduleAndTransactionInfo(employeeId: number, date: Date): ng.IPromise<any>;
    getPositions(loadSkills: boolean, useCache: boolean): ng.IPromise<PositionDTO[]>;
    getUserRoles(userId: number, ignoreDate: boolean): ng.IPromise<UserRolesDTO[]>;
    getCompanyRoles(isAdmin: boolean, userId: number): ng.IPromise<CompanyRolesDTO[]>
    validBankNumberSE(clearing: string, bankAccountNr: string, sysPaymentType?: number): ng.IPromise<boolean>;
    validIBANNumber(iban: string): ng.IPromise<boolean>;
    validSocialSecurityNumber(source: string, checkValidDate: boolean, mustSpecifyCentury: boolean, mustSpecifyDash: boolean, sex: TermGroup_Sex): ng.IPromise<boolean>;
    getBicFromIban(iban: string): ng.IPromise<string>;
    getContactPersonForExport(actorId: number): ng.IPromise<any>;
    getContactPersonCategories(actorId: number): ng.IPromise<any>;
    getShiftTypesForUsersCategories(employeeId: number, isAdmin: boolean, blockTypes: TermGroup_TimeScheduleTemplateBlockType[]): ng.IPromise<any>
    getAccountingFromString(accountingString: string): ng.IPromise<AccountingSettingsRowDTO>;
    getAdditionDeductionTimeCodes(checkInvoiceProduct?: boolean, isMySelf?: boolean): ng.IPromise<TimeCodeAdditionDeductionDTO[]>;
    getAccountIdsFromHierarchyByUser(dateFrom: Date, dateTo: Date, useMaxAccountDimId?: boolean, includeVirtualParented?: boolean, includeOnlyChildrenOneLevel?: boolean, onlyDefaultAccounts?: boolean, useEmployeeAccountIfNoAttestRole?: boolean, includeAbstract?: boolean, employeeId?: number): ng.IPromise<number[]>;
    getAccountsFromHierarchy(accountId: number, includeVirtualParented: boolean, includeOnlyChildrenOneLevel: boolean): ng.IPromise<AccountDTO[]>;
    getAccountsFromHierarchyByUser(dateFrom: Date, dateTo: Date, useMaxAccountDimId?: boolean, includeVirtualParented?: boolean, includeOnlyChildrenOneLevel?: boolean, onlyDefaultAccounts?: boolean, useEmployeeAccountIfNoAttestRole?: boolean, companyId?: number): ng.IPromise<AccountDTO[]>;
    getAccountsFromHierarchyByUserSetting(dateFrom: Date, dateTo: Date, useMaxAccountDimId?: boolean, includeVirtualParented?: boolean, includeOnlyChildrenOneLevel?: boolean, useDefaultEmployeeAccountDimEmployee?: boolean): ng.IPromise<AccountDTO[]>;
    getSiblingAccounts(accountId: number): ng.IPromise<AccountDTO[]>;
    getSelectableEmployeeShiftAccountIds(employeeId: number, date: Date): ng.IPromise<number[]>;
    getEmployeeRequests(employeeId: number, dateFrom: Date, dateTo: Date): ng.IPromise<EmployeeRequestDTO[]>;
    getEventHistories(type: TermGroup_EventHistoryType, entity: SoeEntityType, recordId: number, dateFrom: Date, dateTo: Date, setNames: boolean): ng.IPromise<EventHistoryDTO[]>;
    getEventHistory(eventHistroryId: number, setNames: boolean): ng.IPromise<EventHistoryDTO>;
    getNbrOfEventsInBatch(type: TermGroup_EventHistoryType, batchId: number): ng.IPromise<number>;
    getExpenseRow(expenseRowId: number): ng.IPromise<any>;
    getProductPriceForInvoice(productId: number, customerInvoiceId: number, quantity: number): ng.IPromise<number>;
    getScheduledJobHeadsDict(addEmptyRow: boolean, useCache: boolean): ng.IPromise<ISmallGenericType[]>;
    getCompany(actorCompanyId: number): ng.IPromise<any>;
    getCompaniesByLicense(licenseId: string, onlyTemplates: boolean): ng.IPromise<any>;
    getSysBanks(): ng.IPromise<any[]>;
    validateEcomDeletion(entityType: number, contactType: number, contactEcomId: number): ng.IPromise<boolean>;
    getIntrastatTransactions(invoiceId: number): ng.IPromise<any>;
    getIntrastatTransactionsForExport(intrastatType: IntrastatReportingType, dateFrom: Date, dateTo: Date): ng.IPromise<any[]>;

    getUserSelections(type: UserSelectionType): ng.IPromise<ISmallGenericType[]>;
    getUserSelection(userSelectionId: number): ng.IPromise<UserSelectionDTO>;
    getSuppliersDict(onlyActive: boolean, addEmptyRow: boolean, useCache: boolean): ng.IPromise<any>;
    getCustomers(onlyActive: boolean, addEmptyRow: boolean, useCache: boolean): ng.IPromise<any>;

    signatoryContractUsesPermission(permission: TermGroup_SignatoryContractPermissionType): ng.IPromise<boolean>;
    signatoryContractAuthorize(permission: TermGroup_SignatoryContractPermissionType): ng.IPromise<GetPermissionResultDTO>;

    // POST
    importProductRows(wholesellerId: number, invoiceId: number, actorCustomerId: number, typeId: TermGroup_InvoiceRowImportType, bytes: any[]): ng.IPromise<any>;
    checkFilesDuplicate(files: IFilesLookupDTO): ng.IPromise<string[]>;
    saveContactPerson(updatedContactPerson: IContactPersonDTO): ng.IPromise<any>;
    saveCompanyInformation(information: InformationDTO): ng.IPromise<IActionResult>;
    saveDocument(document: DocumentDTO, fileData: any): ng.IPromise<IActionResult>;
    setDocumentAsRead(dataStorageId: number, confirmed: boolean): ng.IPromise<IActionResult>;
    setInformationAsRead(informationId: number, sysInformationId, confirmed: boolean, hidden: boolean): ng.IPromise<IActionResult>;
    saveEmailTemplate(emailTemplate: IEmailTemplateDTO): ng.IPromise<any>;
    saveSysGridState(grid: string, gridState: string): ng.IPromise<any>;
    saveUserGridState(grid: string, gridState: string): ng.IPromise<any>;
    saveBoolSetting(settingMainType: number, settingTypeId: number, value: boolean): ng.IPromise<any>;
    saveIntSetting(settingMainType: number, settingTypeId: number, value: number): ng.IPromise<any>;
    saveStringSetting(settingMainType: number, settingTypeId: number, value: string): ng.IPromise<any>;
    saveUserCompanySettings(settings: UserCompanySettingEditDTO[]): ng.IPromise<IActionResult>;
    saveHelp(help: SysHelpDTO): ng.IPromise<any>;
    addUserGauge(sysGaugeId: number, sort: number, module: SoeModule, userGaugeHeadId: number): ng.IPromise<any>;
    saveUserGaugeSettings(userGaugeId: number, settings: UserGaugeSettingDTO[]): ng.IPromise<IActionResult>;
    saveUserGaugeSort(userGaugeId: number, sort: number): ng.IPromise<IActionResult>;
    saveUserGaugeHead(model: any): ng.IPromise<any>;
    saveEvaluateAllWorkRulesByPass(result: IEvaluateWorkRulesActionResult, employeeId: number): ng.IPromise<any>;
    saveTextBlock(textBlockDto: TextBlockDTO, entity: number, translations: any[]): ng.IPromise<any>;
    copyCustomerInvoiceRows(rows: ProductRowDTO[], originType: SoeOriginType, targetId: number, originId?: number, updateOrigin?: boolean, recalculate?: boolean): ng.IPromise<any>;
    validateSaveProjectTimeBlocks(items: any): ng.IPromise<any>;
    sendMessage(messageEditDto: MessageEditDTO): ng.IPromise<any>;
    setMessageAsRead(date: Date, messageId: number): ng.IPromise<any>;
    setMessagesAsRead(messageIds: number[]): ng.IPromise<IActionResult>;
    setMessagesAsUnread(messageIds: number[]): ng.IPromise<IActionResult>;
    deleteMessages(messageIds: number[], incoming: boolean): ng.IPromise<any>;
    connectDataStorageToEntity(dataStorageRecordId: number, recordId: number, removeExisting: boolean, entity: SoeEntityType, dataStorageType: SoeDataStorageRecordType): ng.IPromise<IActionResult>;
    updateFileRoleIds(dataStorageRecordId: number, roleIds: number[]): ng.IPromise<IActionResult>;
    saveEmployeeRequests(employeeId: number, deletedEmployeeRequests: EmployeeRequestDTO[], editedOrNewRequests: EmployeeRequestDTO[]): ng.IPromise<IActionResult>;
    saveExpenseRowsValidation(expenseRows: any[]): ng.IPromise<any>;
    saveExpenseRows(expenseRows: any[], customerInvoiceId: number): ng.IPromise<any>;
    getCustomerInvoicesBySearch(number: string, externalNr: string, customerNr: string, customerName: string, internalText: string, projectNr: string, projectName: string, originType: number, ignoreChildren: boolean, customerId?: number, projectId?: number, invoiceId?: number, userId?: number, includePreliminary?: boolean, includeVoucher?: boolean, fullyPaid?: boolean): ng.IPromise<any[]>;
    saveSysPosition(sysPosition: SysPositionDTO): ng.IPromise<IActionResult>;
    addSysLogMessage(requestUri: string, message: string, exception: string, isWarning: boolean);
    saveEventHistory(eventHistory: EventHistoryDTO): ng.IPromise<IActionResult>;
    saveExtraField(extraField: ExtraFieldDTO): ng.IPromise<any>;
    saveExtraFieldRecords(extraFields: ExtraFieldRecordDTO[], entity: number, recordId: number): ng.IPromise<any>;
    performBatchUpdate(model: BatchUpdateModel): ng.IPromise<IActionResult>;
    createCustomerSpecificExport(selections: any): ng.IPromise<IActionResult>;
    initiateDocumentSigning(head: AttestWorkFlowHeadDTO): ng.IPromise<IActionResult>;
    saveDocumentSigningAnswer(attestWorkFlowRowId: number, signeeStatus: SigneeStatus, comment: string): ng.IPromise<IActionResult>;
    cancelDocumentSigning(attestWorkFlowHeadId: number, comment: string): ng.IPromise<IActionResult>;
    refreshBatchUpdateOptions(entityType: SoeEntityType, batchUpdate: BatchUpdateDTO): ng.IPromise<BatchUpdateDTO>
    saveUserSelection(userSelection: UserSelectionDTO): ng.IPromise<IActionResult>;
    saveIntrastatTransactions(transactions: any[], originId: number, originType: SoeOriginType): ng.IPromise<any>;
    createIntrastatExport(selections: any): ng.IPromise<IActionResult>;
    signatoryContractAuthenticate(authenticationResponse: any): ng.IPromise<any>;



    // DELETE
    deleteDataStorage(iRecordId: number, dataStorageType: SoeDataStorageRecordType): ng.IPromise<IActionResult>
    deleteDocument(dataStorageId: number): ng.IPromise<IActionResult>;
    deleteCompanyInformation(informationId: number): ng.IPromise<IActionResult>
    deleteCompanyInformationNotificationSent(informationId: number): ng.IPromise<IActionResult>
    deleteEmailTemplate(emailTemplateId: number): ng.IPromise<any>;
    deleteSysGridState(grid: string): ng.IPromise<any>;
    deleteUserGridState(grid: string): ng.IPromise<any>;
    deleteUserGauge(userGaugeId: number): ng.IPromise<IActionResult>;
    deleteSystemInfoLogRow(rowId: number): ng.IPromise<any>;
    deleteTextBlock(textBlockId: number): ng.IPromise<any>;
    deleteExpenseRow(expenseRowId: number): ng.IPromise<any>;
    deleteSysPosition(sysPositionId: number): ng.IPromise<IActionResult>;
    deleteEventHistory(eventHistoryId: number): ng.IPromise<IActionResult>;
    deleteEventHistories(type: TermGroup_EventHistoryType, batchId: number): ng.IPromise<IActionResult>;
    deleteExtraField(extraFieldId: number): ng.IPromise<IActionResult>;
    deleteUserSelection(userSelectionId: number): ng.IPromise<IActionResult>;
}

export class CoreService implements ICoreService {
    //@ngInject
    constructor(private storageService: IStorageService, private httpService: IHttpService, private $timeout: ng.ITimeoutService, private $q: ng.IQService) {
    }
    // GET

    // ProcessInfo
    getProgressInfo(key: any) {
        return this.httpService.get(Constants.WEBAPI_CORE_PROGRESS_INFO + key, false);
    }

    // Address
    getAddressRowTypes(sysContactTypeId: TermGroup_SysContactType) {
        return this.httpService.getCache(Constants.WEBAPI_CORE_ADDRESS_ADDRESS_ROW_TYPE + sysContactTypeId, null, Constants.CACHE_EXPIRE_LONG);
    }

    getAddressTypes(sysContactTypeId: TermGroup_SysContactType) {
        return this.httpService.getCache(Constants.WEBAPI_CORE_ADDRESS_ADDRESS_TYPE + sysContactTypeId, null, Constants.CACHE_EXPIRE_LONG);
    }

    getAccountDimsSmall(onlyStandard: boolean, onlyInternal: boolean, loadAccounts: boolean, loadInternalAccounts: boolean, loadParent: boolean, includeParentAccounts: boolean, useCache = true, ignoreHierarchyOnly: boolean = false, companyId: number = 0) {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_DIM + "?onlyStandard=" + onlyStandard + "&onlyInternal=" + onlyInternal + "&loadAccounts=" + loadAccounts + "&loadInternalAccounts=" + loadInternalAccounts + "&loadParent=" + loadParent + "&includeParentAccounts=" + includeParentAccounts + "&ignoreHierarchyOnly=" + ignoreHierarchyOnly + "&companyId=" + companyId, Constants.WEBAPI_ACCEPT_SMALL_DTO, Constants.CACHE_EXPIRE_LONG, !useCache).then((x: AccountDimSmallDTO[]) => {
            return x.map(y => {
                let obj = new AccountDimSmallDTO();
                angular.extend(obj, y);
                if (obj.accounts) {
                    obj.accounts = obj.accounts.map(a => {
                        let aObj = new AccountDTO();
                        angular.extend(aObj, a);
                        return aObj;
                    });
                }
                return obj;
            })
        });
    }

    getAccountDim(accountDimId: number, loadAccounts: boolean, loadInternalAccounts: boolean, loadInactiveDims: boolean, useCache: boolean = true, ignoreHierarchyOnly: boolean = false) {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_DIM + "?accountDimId=" + accountDimId + "&loadAccounts=" + loadAccounts + "&loadInternalAccounts=" + loadInternalAccounts + "&loadInactiveDims=" + loadInactiveDims + "&ignoreHierarchyOnly=" + ignoreHierarchyOnly, Constants.WEBAPI_ACCEPT_SMALL_DTO, Constants.CACHE_EXPIRE_LONG, !useCache).then(x => {
            let obj = new AccountDimSmallDTO();
            angular.extend(obj, x);
            if (obj.accounts) {
                obj.accounts = obj.accounts.map(a => {
                    let aObj = new AccountDTO();
                    angular.extend(aObj, a);
                    return aObj;
                });
            }
            return obj;
        });
    }

    getAttestTransitionGridDTOs(entity: TermGroup_AttestEntity, module: SoeModule, setEntityName: boolean) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_TRANSITION + entity + "/" + module + "/" + setEntityName, false, Constants.WEBAPI_ACCEPT_GRID_DTO);
    }

    getAttestWorkFlowTemplateRows(attestWorkFlowTemplateId: number): ng.IPromise<AttestWorkFlowTemplateRowDTO[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_WORK_FLOW_TEMPLATE_ATTEST_WORK_FLOW_TEMPLATE_ROWS + attestWorkFlowTemplateId, false).then(x => {
            return x.map(y => {
                let obj = new AttestWorkFlowTemplateRowDTO();
                angular.extend(obj, y);
                return obj;
            });
        });
    }

    getAttestWorkFlowHead(attestWorFlowHeadId: number, loadRows: boolean): ng.IPromise<AttestWorkFlowHeadDTO> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_WORK_FLOW_ATTEST_WORK_FLOW_HEAD + attestWorFlowHeadId + "/" + loadRows, false).then(x => {
            let obj = new AttestWorkFlowHeadDTO();
            angular.extend(obj, x);
            obj.setTypes();
            return obj;
        });
    }

    getAttestWorkFlowRowsFromRecordId(entity: SoeEntityType, recordId: number): ng.IPromise<AttestWorkFlowRowDTO[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_WORK_FLOW_ATTEST_WORK_FLOW_ROWS_FROM_RECORD_ID + entity + "/" + recordId, false).then(x => {
            return x.map(y => {
                let obj = new AttestWorkFlowRowDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    hasAttestWorkFlowTemplates(entity: TermGroup_AttestEntity): ng.IPromise<boolean> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_WORK_FLOW_TEMPLATE_HAS_TEMPLATES + entity, false);
    }

    getDocumentSigningStatus(entity: SoeEntityType, recordId: number): ng.IPromise<AttestWorkFlowRowDTO[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_WORK_FLOW_GET_DOCUMENT_SIGNING_STATUS + entity + "/" + recordId, false).then(x => {
            return x.map(y => {
                let obj = new AttestWorkFlowRowDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            })
        });
    }

    getAttestRolesDict(module: SoeModule, addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_ROLE + "?module=" + module + "&addEmptyRow=" + addEmptyRow, false, Constants.WEBAPI_ACCEPT_GENERIC_TYPE);
    }

    getCurrentAccountYear() {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_ACCOUNTING_CURRENT_ACCOUNT_YEAR, null, Constants.CACHE_EXPIRE_LONG);
    }

    getAccountYearById(accountYearId: number) {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNTYEAR + accountYearId + "/" + false, null, Constants.CACHE_EXPIRE_LONG);
    }

    getAccountYearDict(addEmptyRow: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNTYEAR_DICT + addEmptyRow, null, Constants.CACHE_EXPIRE_LONG);
    }

    getAccountPeriodDict(accountYearId: number, addEmptyRow: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_PERIOD + accountYearId + "/" + addEmptyRow, null, Constants.CACHE_EXPIRE_LONG);
    }

    getAccountDimStd() {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_DIM_STD, null, Constants.CACHE_EXPIRE_LONG);
    }

    getEComTypes(sysContactTypeId: TermGroup_SysContactType) {
        return this.httpService.getCache(Constants.WEBAPI_CORE_ADDRESS_ECOM_TYPE + sysContactTypeId, null, Constants.CACHE_EXPIRE_LONG);
    }

    getEmployeeGroups() {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_GROUP, false);
    }

    getEmployeeGroupsDict(addEmptyRow: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_GROUP + "?addEmptyRow=" + addEmptyRow, Constants.WEBAPI_ACCEPT_GENERIC_TYPE, Constants.CACHE_EXPIRE_LONG);
    }

    getEmployeeForUser() {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_FOR_USER, false);
    }
    // Assembly info
    getAssemblyDate() {
        return this.httpService.get(Constants.WEBAPI_CORE_ASSEMBLY_INFO_DATE, false);
    }

    getAssemblyVersion() {
        return this.httpService.get(Constants.WEBAPI_CORE_ASSEMBLY_INFO_VERSION, false);
    }

    getServerTime(): ng.IPromise<Date> {
        return this.httpService.get(Constants.WEBAPI_CORE_SERVER_TIME, false).then(x => {
            return CalendarUtility.convertToDate(x);
        });
    }

    getSiteType() {
        return this.httpService.get(Constants.WEBAPI_CORE_SITE_TYPE, true);
    }

    // Category
    getCategories(soeCategoryTypeId: SoeCategoryType, loadCompanyCategoryRecord: boolean, loadChildren: boolean, loadCategoryGroups: boolean, useCache: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_CORE_CATEGORY + "?soeCategoryTypeId=" + soeCategoryTypeId + "&loadCompanyCategoryRecord=" + loadCompanyCategoryRecord + "&loadChildren=" + loadChildren + "&loadCategoryGroups=" + loadCategoryGroups, Constants.WEBAPI_ACCEPT_DTO, Constants.CACHE_EXPIRE_VERY_SHORT, !useCache);
    }

    getCategoriesDict(soeCategoryTypeId: SoeCategoryType, addEmptyRow: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_CORE_CATEGORY + "?soeCategoryTypeId=" + soeCategoryTypeId + "&addEmptyRow=" + addEmptyRow, Constants.WEBAPI_ACCEPT_GENERIC_TYPE, Constants.CACHE_EXPIRE_MEDIUM);
    }

    getCategoriesForRoleFromType(employeeId: number, categoryType: SoeCategoryType, isAdmin: boolean, includeSecondary: boolean, addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_CORE_CATEGORY_ACCOUNTS_BY_FORROLEFROMTYPE + employeeId + "/" + categoryType + "/" + isAdmin + "/" + includeSecondary + "/" + addEmptyRow, true);
    }

    getCategoryAccountsByAccount(accountId: number, loadCategory: boolean) {
        return this.httpService.get(Constants.WEBAPI_CORE_CATEGORY_ACCOUNTS_BY_ACCOUNT + accountId + "/" + loadCategory, false);
    }

    getCompanyCategoryRecords(categoryType: SoeCategoryType, categoryRecordEntity: SoeCategoryRecordEntity, recordId: number, useCache: boolean): ng.IPromise<CompanyCategoryRecordDTO[]> {
        return this.httpService.get(Constants.WEBAPI_CORE_COMP_CATEGORY_RECORDS + categoryType + "/" + categoryRecordEntity + "/" + recordId, useCache);
    }

    // Currency
    getSysCurrenciesDict(addEmptyRow: boolean, useCode = false) {
        return this.httpService.getCache(Constants.WEBAPI_CORE_CURRENCY_SYS + "?addEmptyRow=" + addEmptyRow + "&useCode=" + useCode, Constants.WEBAPI_ACCEPT_GENERIC_TYPE, Constants.CACHE_EXPIRE_MEDIUM);
    }

    getCompCurrencies(loadRates: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_CORE_CURRENCY_COMP + "?loadRates=" + loadRates, Constants.WEBAPI_ACCEPT_DTO, Constants.CACHE_EXPIRE_MEDIUM);
    }

    getCompCurrenciesDict(addEmptyRow: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_CORE_CURRENCY_COMP + "?addEmptyRow=" + addEmptyRow, Constants.WEBAPI_ACCEPT_GENERIC_TYPE, Constants.CACHE_EXPIRE_MEDIUM);
    }

    getCompCurrenciesGrid() {
        return this.httpService.getCache(Constants.WEBAPI_CORE_CURRENCY_COMP, Constants.WEBAPI_ACCEPT_GRID_DTO, Constants.CACHE_EXPIRE_MEDIUM);
    }

    getCompCurrenciesSmall() {
        return this.httpService.getCache(Constants.WEBAPI_CORE_CURRENCY_COMP, Constants.WEBAPI_ACCEPT_SMALL_DTO, Constants.CACHE_EXPIRE_MEDIUM);
    }

    getCompCurrencyRates(currencyId: number) {
        return this.httpService.getCache(Constants.WEBAPI_CORE_CURRENCY_COMP + currencyId, null, Constants.CACHE_EXPIRE_MEDIUM);
    }

    getCompCurrencyRate(sysCurrencyId: number, date: Date, rateToBase: boolean) {
        let dateString: string = null;
        if (date)
            dateString = date.toDateTimeString();
        return this.httpService.getCache(Constants.WEBAPI_CORE_CURRENCY_COMP + sysCurrencyId + "/" + dateString + "/" + rateToBase, null, Constants.CACHE_EXPIRE_MEDIUM);
    }

    getCompanyBaseCurrency() {
        return this.httpService.getCache(Constants.WEBAPI_CORE_CURRENCY_COMP_BASE, null, Constants.CACHE_EXPIRE_MEDIUM);
    }

    getEnterpriseCurrency() {
        return this.httpService.getCache(Constants.WEBAPI_CORE_CURRENCY_ENTERPRISE, null, Constants.CACHE_EXPIRE_MEDIUM);
    }

    getLedgerCurrency(actorId: number) {
        return this.httpService.getCache(Constants.WEBAPI_CORE_CURRENCY_LEDGER + actorId, null, Constants.CACHE_EXPIRE_MEDIUM);
    }

    // Contact person
    getContactPersonsForActor(actorId: number, useCache: boolean): ng.IPromise<any> {
        return this.httpService.getCache(Constants.WEBAPI_CORE_CONTACT_PERSON_CONTACT_PERSONS_BY_ACTOR + actorId, null, Constants.CACHE_EXPIRE_LONG, useCache);
    }

    getContactPersonCategories(actorId: number) {
        return this.httpService.get(Constants.WEBAPI_CORE_CONTACT_PERSON_CATEGORIES + actorId, false);
    }

    // Help
    getFormHelp(formName: string) {
        return this.httpService.getCache(Constants.WEBAPI_CORE_HELP_FORM + formName, null, Constants.CACHE_EXPIRE_LONG);
    }

    getHelpTitles(): ng.IPromise<any> {
        return this.httpService.getCache(Constants.WEBAPI_CORE_HELP + CoreUtility.language, null, Constants.CACHE_EXPIRE_LONG)
    }

    hasHelp(feature: Feature): ng.IPromise<any> {
        if (!feature)
            feature = Feature.None;
        return this.httpService.getCache(Constants.WEBAPI_CORE_HELP_EXISTS + feature + "/" + CoreUtility.language, null, Constants.CACHE_EXPIRE_LONG);
    }

    getHelp(feature: Feature): ng.IPromise<any> {
        if (!feature)
            feature = Feature.None;
        return this.httpService.get(Constants.WEBAPI_CORE_HELP + feature + "/" + CoreUtility.language, true);
    }

    // Household tax deduxction
    getHouseholdTaxDeductionRowsByCustomer(customerId: number, addEmptyRow: boolean, showAllApplicants: boolean) {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_HOUSEHOLD_CUSTOMER + customerId + "/" + addEmptyRow + "/" + showAllApplicants, false);
    }

    // Images
    getImages(imageType: SoeEntityImageType, entity: SoeEntityType, recordId: number, useThumbnails: boolean, projectId?: number) {
        return this.httpService.get(Constants.WEBAPI_CORE_IMAGE + imageType + "/" + entity + "/" + recordId + "/" + useThumbnails + "/" + (projectId ? projectId : 0), false);
    }

    getImage(imageId: number): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_CORE_IMAGE + imageId, true);
    }

    // Documents
    hasNewDocuments(time: Date): ng.IPromise<boolean> {
        return this.httpService.get(Constants.WEBAPI_CORE_DOCUMENT_NEW_SINCE + time, false, null, true);
    }

    getNbrOfUnreadCompanyDocuments(useCache: boolean): ng.IPromise<number> {
        return this.httpService.getCache(Constants.WEBAPI_CORE_DOCUMENT_COMPANY_UNREAD_COUNT, null, Constants.CACHE_EXPIRE_SHORT, !useCache, true);
    }

    getCompanyDocuments(): ng.IPromise<DocumentDTO[]> {
        return this.httpService.get(Constants.WEBAPI_CORE_DOCUMENT_COMPANY, false).then(x => {
            return x.map(y => {
                let obj = new DocumentDTO();
                angular.extend(obj, y);
                obj.fixDates();
                obj.setTypes();
                return obj;
            });
        });
    }

    getMyDocuments(): ng.IPromise<DocumentDTO[]> {
        return this.httpService.get(Constants.WEBAPI_CORE_DOCUMENT_MY, false).then(x => {
            return x.map(y => {
                let obj = new DocumentDTO();
                angular.extend(obj, y);
                obj.fixDates();
                obj.setTypes();
                return obj;
            });
        });
    }

    getDocument(dataStorageId: number): ng.IPromise<DocumentDTO> {
        return this.httpService.get(Constants.WEBAPI_CORE_DOCUMENT + dataStorageId, false).then(x => {
            let obj = new DocumentDTO();
            angular.extend(obj, x);
            obj.fixDates();
            obj.setTypes();
            return obj;
        });
    }

    getDocumentUrl(dataStorageId: number): ng.IPromise<string> {
        return this.httpService.get(Constants.WEBAPI_CORE_DOCUMENT_URL + dataStorageId, false);
    }

    getDocumentFolders(): ng.IPromise<string[]> {
        return this.httpService.get(Constants.WEBAPI_CORE_DOCUMENT_FOLDERS, false);
    }

    getDocumentData(dataStorageId: number): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_CORE_DOCUMENT_DATA + dataStorageId, false);
    }

    getDocumentRecipientInfo(dataStorageId: number): ng.IPromise<DataStorageRecipientDTO[]> {
        return this.httpService.get(Constants.WEBAPI_CORE_DOCUMENT_RECIPIENT_INFO + dataStorageId, false).then(x => {
            return x.map(y => {
                let obj = new DataStorageRecipientDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    getExcelProductRowsTemplate(): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_CORE_EXCEL_TEMPLATES_PRODUCT_ROWS, false);
    }

    getFileRoleIds(dataStorageRecordId: number): ng.IPromise<number[]> {
        return this.httpService.get(Constants.WEBAPI_CORE_FILES_ROLEIDS + dataStorageRecordId, false);
    }

    // Information
    getCompanyInformationForEdit(informationId: number): ng.IPromise<InformationDTO> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_REGISTRY_COMPANY_INFORMATION + informationId, false).then(x => {
            let obj: InformationDTO = new InformationDTO();
            angular.extend(obj, x);
            obj.fixDates();
            return obj;
        });
    }

    getCompanyInformationFolders(): ng.IPromise<string[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_REGISTRY_COMPANY_INFORMATION_FOLDERS, false);
    }

    getCompanyInformationHasConfirmations(informationId: number): ng.IPromise<boolean> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_REGISTRY_COMPANY_INFORMATION_HAS_CONFIRMATIONS + informationId, false);
    }

    getCompanyInformationRecipientInfo(informationId: number): ng.IPromise<InformationRecipientDTO[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_REGISTRY_COMPANY_INFORMATION_RECIPIENT_INFO + informationId, false).then(x => {
            return x.map(y => {
                let obj: InformationRecipientDTO = new InformationRecipientDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    hasNewInformations(time: string): ng.IPromise<boolean> {
        return this.httpService.get(Constants.WEBAPI_CORE_INFORMATION_NEW_SINCE + time, false, null, true);
    }

    getNbrOfUnreadInformations(useCache: boolean): ng.IPromise<number> {
        return this.httpService.getCache(Constants.WEBAPI_CORE_INFORMATION_UNREAD_COUNT + CoreUtility.language, null, Constants.CACHE_EXPIRE_LONG, !useCache, true);
    }

    getUnreadInformations(): ng.IPromise<InformationDTO[]> {
        return this.httpService.get(Constants.WEBAPI_CORE_INFORMATION_UNREAD + CoreUtility.language, false).then(x => {
            return x.map(y => {
                let obj = new InformationDTO();
                angular.extend(obj, y);
                obj.fixDates();
                obj.setTypes();
                return obj;
            });
        });
    }

    hasSevereUnreadInformation(useCache: boolean): ng.IPromise<boolean> {
        return this.httpService.getCache(Constants.WEBAPI_CORE_INFORMATION_UNREAD_SEVERE + CoreUtility.language, null, Constants.CACHE_EXPIRE_LONG, !useCache, true);
    }

    getCompanyInformations(): ng.IPromise<InformationDTO[]> {
        return this.httpService.get(Constants.WEBAPI_CORE_INFORMATION_COMPANY + CoreUtility.language, false).then(x => {
            return x.map(y => {
                let obj = new InformationDTO();
                angular.extend(obj, y);
                obj.fixDates();
                obj.setTypes();
                return obj;
            });
        });
    }

    getCompanyInformation(informationId: number): ng.IPromise<InformationDTO> {
        return this.httpService.get(Constants.WEBAPI_CORE_INFORMATION_COMPANY + CoreUtility.language + "/" + informationId, false).then(x => {
            let obj = new InformationDTO();
            angular.extend(obj, x);
            obj.fixDates();
            obj.setTypes();
            return obj;
        });
    }

    getSysInformations(): ng.IPromise<InformationDTO[]> {
        return this.httpService.get(Constants.WEBAPI_CORE_INFORMATION_SYS + CoreUtility.language, false).then(x => {
            return x.map(y => {
                let obj = new InformationDTO();
                angular.extend(obj, y);
                obj.fixDates();
                obj.setTypes();

                if (obj.sysInformationSysCompDbs) {
                    obj.sysInformationSysCompDbs = obj.sysInformationSysCompDbs.map(c => {
                        let cObj: SysInformationSysCompDbDTO = new SysInformationSysCompDbDTO();
                        angular.extend(cObj, c);
                        cObj.fixDates();
                        return cObj;
                    });
                } else {
                    obj.sysInformationSysCompDbs = [];
                }

                return obj;
            });
        });
    }

    getSysInformation(sysInformationId: number): ng.IPromise<InformationDTO> {
        return this.httpService.get(Constants.WEBAPI_CORE_INFORMATION_SYS + CoreUtility.language + "/" + sysInformationId, false).then(x => {
            let obj = new InformationDTO();
            angular.extend(obj, x);
            obj.fixDates();
            obj.setTypes();

            if (obj.sysInformationSysCompDbs) {
                obj.sysInformationSysCompDbs = obj.sysInformationSysCompDbs.map(c => {
                    let cObj: SysInformationSysCompDbDTO = new SysInformationSysCompDbDTO();
                    angular.extend(cObj, c);
                    cObj.fixDates();
                    return cObj;
                });
            } else {
                obj.sysInformationSysCompDbs = [];
            }

            return obj;
        });
    }

    // Permissions
    hasReadOnlyPermissions(features: Feature[]) {
        return this.httpService.get(Constants.WEBAPI_CORE_FEATURE_READ_ONLY_PERMISSION + "?featureIds=" + features.join(','), true);
    }

    hasModifyPermissions(features: Feature[]) {
        return this.httpService.get(Constants.WEBAPI_CORE_FEATURE_MODIFY_PERMISSION + "?featureIds=" + features.join(','), false);
    }

    getMessageGroups(useCache: boolean): ng.IPromise<MessageGroupDTO[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_REGISTRY_MESSAGE_GROUP, useCache);
    }

    getMessageGroupsDict(useCache: boolean): ng.IPromise<ISmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_REGISTRY_MESSAGE_GROUP, useCache, Constants.WEBAPI_ACCEPT_GENERIC_TYPE);
    }

    getMessageGroupUsersByAccount(accountId: number): ng.IPromise<ISmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_REGISTRY_MESSAGE_GROUP_USERS_BY_ACCOUNT + accountId, false);
    }

    getMessageGroupUsersByCategory(categoryId: number): ng.IPromise<ISmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_REGISTRY_MESSAGE_GROUP_USERS_BY_CATEGORY + categoryId, false);
    }

    getMessageGroupUsersByEmployeeGroup(employeeGroupId: number): ng.IPromise<ISmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_REGISTRY_MESSAGE_GROUP_USERS_BY_EMPLOYEE_GROUP + employeeGroupId, false);
    }

    getMessageGroupUsersByRole(roleId: number): ng.IPromise<ISmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_REGISTRY_MESSAGE_GROUP_USERS_BY_ROLE + roleId, false);
    }

    // Settings
    getLicenseSettings(settingTypes: LicenseSettingType[], useCache?: boolean): ng.IPromise<any> {
        if (useCache === undefined) {
            useCache = true;
        }
        return this.httpService.get(Constants.WEBAPI_CORE_USER_COMPANY_SETTING_LICENSE + "?settingTypes=" + settingTypes.join(','), useCache);
    }

    getLicenseSettingsForEdit(): ng.IPromise<UserCompanySettingEditDTO[]> {
        return this.httpService.get(Constants.WEBAPI_CORE_USER_COMPANY_SETTING_LICENSE_FOR_EDIT, false).then((x => {
            return x.map(y => {
                var obj = new UserCompanySettingEditDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        }));
    }

    getCompanySettings(settingTypes: CompanySettingType[], useCache?: boolean) {
        if (useCache === undefined) {
            useCache = true;
        }
        return this.httpService.get(Constants.WEBAPI_CORE_USER_COMPANY_SETTING_COMPANY + "?settingTypes=" + settingTypes.join(','), useCache);
    }

    getUserSettings(settingTypes: UserSettingType[], useCache?: boolean) {
        return this.httpService.get(Constants.WEBAPI_CORE_USER_COMPANY_SETTING_USER + "?settingTypes=" + settingTypes.join(','), useCache);
    }

    getUserAndCompanySettings(settingTypes: UserSettingType[], useCache?: boolean) {
        return this.httpService.get(Constants.WEBAPI_CORE_USER_COMPANY_SETTING_USER_AND_COMPANY + "?settingTypes=" + settingTypes.join(','), useCache);
    }

    getBoolSetting(settingMainType: SettingMainType, settingType: number) {
        return this.httpService.get(Constants.WEBAPI_CORE_USER_COMPANY_SETTING_BOOL + settingMainType + "/" + settingType, true);
    }

    getIntSetting(settingMainType: SettingMainType, settingType: number) {
        return this.httpService.get(Constants.WEBAPI_CORE_USER_COMPANY_SETTING_INT + settingMainType + "/" + settingType, true);
    }

    getStringSetting(settingMainType: SettingMainType, settingType: number) {
        return this.httpService.get(Constants.WEBAPI_CORE_USER_COMPANY_SETTING_STRING + settingMainType + "/" + settingType, true);
    }

    getBoolConfigSetting(name: string): ng.IPromise<boolean> {
        return this.httpService.get(Constants.WEBAPI_CORE_CONFIG_SETTING_BOOL + name, true);
    }

    // UserGridState
    getSysGridState(grid: string) {
        return this.httpService.get(Constants.WEBAPI_CORE_SYS_GRID_STATE + grid, false);
    }

    getUserGridState(grid: string) {
        return this.httpService.get(Constants.WEBAPI_CORE_USER_GRID_STATE + grid, false);
    }

    // StateAnalysis
    getStateAnalysis(analysisIds: number[]) {
        return this.httpService.get(Constants.WEBAPI_CORE_STATE_ANALYSIS + "?analysisIds=" + analysisIds.join(','), false);
    }

    // SysCountry
    getSysCountries(addEmptyRow: boolean, onlyUsedLanguages: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_CORE_SYS_COUNTRY + addEmptyRow + "/" + onlyUsedLanguages, null, Constants.CACHE_EXPIRE_LONG);
    }

    // SysHoliday
    getSysHolidayTypes() {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_HOLIDAY_SYS_HOLIDAY_TYPES, true);
    }

    // Terms

    getTermGroupContent(sysTermGroupId: number, addEmptyRow: boolean, skipUnknown: boolean, sortById: boolean = false): ng.IPromise<ISmallGenericType[]> {
        var deferred = this.$q.defer<ISmallGenericType[]>();

        this.$timeout(() => {
            var cacheKey = this.storageService.createTermGroupKey(sysTermGroupId, Constants.WEBAPI_CORE_SYS_TERM_GROUP + sysTermGroupId + "/" + addEmptyRow + "/" + skipUnknown + "/" + sortById + "/" + CoreUtility.languageId);
            var result = this.storageService.fetch(cacheKey);
            if (!result)
                result = this.httpService.get(Constants.WEBAPI_CORE_SYS_TERM_GROUP + sysTermGroupId + "/" + addEmptyRow + "/" + skipUnknown + "/" + sortById, true).then((x) => {
                    if (x) {
                        result = x;
                        this.storageService.clearOld(cacheKey);
                        this.storageService.add(cacheKey, x)
                        deferred.resolve(x);
                    };
                });
            else
                deferred.resolve(result);
        });

        return deferred.promise;
    }

    // PersonalDataLogs
    getPersonalDataLogsForEmployee(employeeId: number, informationType: TermGroup_PersonalDataInformationType, actionType: TermGroup_PersonalDataActionType, dateFrom: Date, dateTo: Date): ng.IPromise<PersonalDataLogMessageDTO[]> {
        var dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        var dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_CORE_TRACK_PERSONALDATALOGS_EMPLOYEE + employeeId + "/" + informationType + "/" + actionType + "/" + dateFromString + "/" + dateToString + "/" + 0, false).then((x: PersonalDataLogMessageDTO[]) => {
            return x.map(y => {
                var obj = new PersonalDataLogMessageDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    getPersonalDataLogsCausedByEmployee(employeeId: number, recordId: number, informationType: TermGroup_PersonalDataInformationType, actionType: TermGroup_PersonalDataActionType, dateFrom: Date, dateTo: Date): ng.IPromise<PersonalDataLogMessageDTO[]> {
        var dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        var dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();

        var url = Constants.WEBAPI_CORE_TRACK_PERSONALDATALOGS_CAUSEDBYEMPLOYEE + employeeId + "/" + recordId + "/" + informationType + "/" + actionType + "/" + dateFromString + "/" + dateToString + "/" + 0;

        return this.httpService.get(url, false).then((x: PersonalDataLogMessageDTO[]) => {
            return x.map(y => {
                var obj = new PersonalDataLogMessageDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    getPersonalDataLogsCausedByUser(userId: number, recordId: number, informationType: TermGroup_PersonalDataInformationType, actionType: TermGroup_PersonalDataActionType, dateFrom: Date, dateTo: Date): ng.IPromise<PersonalDataLogMessageDTO[]> {
        var dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        var dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();

        var url = Constants.WEBAPI_CORE_TRACK_PERSONALDATALOGS_CAUSEDBYUSER + userId + "/" + recordId + "/" + informationType + "/" + actionType + "/" + dateFromString + "/" + dateToString + "/" + 0;

        return this.httpService.get(url, false).then((x: PersonalDataLogMessageDTO[]) => {
            return x.map(y => {
                var obj = new PersonalDataLogMessageDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    // Track changes
    getTrackChanges(entity: SoeEntityType, recordId: number, includeChildren: boolean): ng.IPromise<TrackChangesDTO[]> {
        return this.httpService.get(Constants.WEBAPI_CORE_TRACK_CHANGES + entity + "/" + recordId + "/" + includeChildren, false).then((x: TrackChangesDTO[]) => {
            return x.map(y => {
                var obj = new TrackChangesDTO();
                angular.extend(obj, y);
                obj.fixDates();

                return obj;
            });
        });
    }

    getTrackChangesLogEntities(): ng.IPromise<ISmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_CORE_TRACK_CHANGES_LOG_ENTITIES, true);
    }

    getTrackChangesLog(entity: SoeEntityType, recordId: number, dateFrom: Date, dateTo: Date): ng.IPromise<TrackChangesLogDTO[]> {
        var dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        var dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_CORE_TRACK_CHANGES_LOG + entity + "/" + recordId + "/" + dateFromString + "/" + dateToString, false).then((x: TrackChangesLogDTO[]) => {
            return x.map(y => {
                var obj = new TrackChangesLogDTO();
                angular.extend(obj, y);
                obj.fixDates();

                return obj;
            });
        });
    }

    getTrackChangesLogForEntity(entity: SoeEntityType, dateFrom: Date, dateTo: Date, users: string[]) {
        var model = {
            entityType: entity,
            dateFrom: dateFrom,
            dateTo: dateTo,
            users: users
        };

        return this.httpService.post(Constants.WEBAPI_CORE_TRACK_CHANGES_LOG_FOR_ENTITY, model).then((x: TrackChangesLogDTO[]) => {
            return x.map(y => {
                var obj = new TrackChangesLogDTO();
                angular.extend(obj, y);
                obj.fixDates();

                return obj;
            });
        });
    }

    searchPerson(searchString: string, searchEntities: number[]): ng.IPromise<IActorSearchPersonDTO[]> {
        return this.httpService.get(Constants.WEBAPI_CORE_SEARCH_PERSON + searchString + "/" + searchEntities, false).then((x: IActorSearchPersonDTO[]) => {
            return x.map(y => {
                var obj = new ActorSearchPersonDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    // Translations
    getTranslations(recordType: CompTermsRecordType, recordId: number, loadLangName: boolean) {
        return this.httpService.get(Constants.WEBAPI_CORE_TRANSLATION + recordType + "/" + recordId + "/" + loadLangName, false);
    }

    // TraceViews
    getInvoiceTraceViews(invoiceId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_GETINVOICETRACEVIEWS + invoiceId, false);
    }

    getVoucherTraceViews(voucherHeadId: number): ng.IPromise<IVoucherTraceViewDTO[]> {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_VOUCHER_GETVOUCHERTRACEVIEWS + voucherHeadId, false).then((x: IVoucherTraceViewDTO[]) => {
            _.forEach(x, y => {
                y.date = CalendarUtility.convertToDate(y.date);
            });
            return x;
        });
    }

    getPaymentTraceViews(paymentId: number): ng.IPromise<IPaymentTraceViewDTO[]> {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_PAYMENT_GETPAYMENTTACEVIEWS + paymentId, false);
    }

    getProjectTraceViews(projectId: number): ng.IPromise<IProjectTraceViewDTO[]> {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_PROJECT_GETPROJECTTRACEVIEWS + projectId, false);
    }

    getCompanyRolesDict(addEmptyRow: boolean, addEmptyRowAsAll: boolean): ng.IPromise<ISmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ROLE + addEmptyRow + "/" + addEmptyRowAsAll, false);
    }

    getRolesByUserDict(actorCompanyId: number): ng.IPromise<ISmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ROLE_BY_USER + actorCompanyId, false);
    }

    getOrderTraceViews(orderId: number): ng.IPromise<IOrderTraceViewDTO[]> {
        return this.httpService.get(Constants.WEBAPI_BILLING_ORDER_GETORDERTRACEVIEWS + orderId, false);
    }

    getOfferTraceViews(offerId: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_OFFER_GETOFFERTRACEVIEWS + offerId, false);
    }

    getContractTraceViews(contractId: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_CONTRACT_GETCONTRACTTRACEVIEWS + contractId, false);
    }

    getPurchaseTraceViews(purchaseId: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_PURCHASE_TRACEVIEWS + purchaseId, false);
    }

    // User
    getUsersForOrigin(includeEmployeeCategories: boolean): ng.IPromise<any[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_USER_FOR_ORIGIN + includeEmployeeCategories, false);
    }

    getUsers(setDefaultRoleName: boolean, active?: boolean, skipNonEmployeeUsers: boolean = false, includeEmployeesWithSameAccountOnAttestRole: boolean = false, includeEmployeesWithSameAccount: boolean = false, includeEmployeeCategories = false): ng.IPromise<UserSmallDTO[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_USER + "?setDefaultRoleName=" + setDefaultRoleName + "&active=" + active + "&skipNonEmployeeUsers=" + skipNonEmployeeUsers + "&includeEmployeesWithSameAccountOnAttestRole=" + includeEmployeesWithSameAccountOnAttestRole + "&includeEmployeesWithSameAccount=" + includeEmployeesWithSameAccount + "&includeEmployeeCategories=" + includeEmployeeCategories, false, Constants.WEBAPI_ACCEPT_SMALL_DTO).then(x => {
            return x.map(y => {
                let obj = new UserSmallDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            })
        });
    }

    getUsersDict(addEmptyRow: boolean, includeKey: boolean, useFullName: boolean, includeLoginName: boolean): ng.IPromise<ISmallGenericType[]> {
        return this.httpService.getCache(Constants.WEBAPI_MANAGE_USER + "?addEmptyRow=" + addEmptyRow + "&includeKey=" + includeKey + "&useFullName=" + useFullName + "&includeLoginName=" + includeLoginName, Constants.WEBAPI_ACCEPT_GENERIC_TYPE, Constants.CACHE_EXPIRE_LONG);
    }

    getUsersByAvailability(setDefaultRoleName: boolean, active: boolean, byCompany: boolean, dateFrom?: Date, dateTo?: Date): ng.IPromise<any> {
        var dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        var dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_CORE_USER + "?setDefaultRoleName=" + setDefaultRoleName + "&onlyActive=" + active + "&byCompany=" + byCompany + "&dateFrom=" + dateFromString + "&dateTo=" + dateToString, false);
    }

    getUsersWithoutEmployees(companyId: number, includeUserId: number, addEmptyRow: boolean): ng.IPromise<ISmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_USER_USERS_WITHOUT_EMPLOYEES + companyId + "/" + (includeUserId || 0) + "/" + addEmptyRow, false);
    }

    getUser(userId: number) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_USER + userId, false).then(x => {
            let obj = new UserSmallDTO();
            angular.extend(obj, x);
            obj.fixDates();

            return obj;
        });
    }

    getCurrentUser() {
        return this.httpService.get(Constants.WEBAPI_MANAGE_USER_CURRENT, false);
    }

    getUserRoles(userId: number, ignoreDate: boolean): ng.IPromise<UserRolesDTO[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ROLE_USER_ROLES + userId + "/" + ignoreDate, false).then(x => {
            return x.map(y => {
                let obj = new UserRolesDTO();
                angular.extend(obj, y);

                obj.roles = obj.roles.map(r => {
                    let rObj = new UserCompanyRoleDTO();
                    angular.extend(rObj, r);
                    rObj.fixDates();
                    return rObj;
                });

                obj.attestRoles = obj.attestRoles.map(a => {
                    let aObj = new UserAttestRoleDTO();
                    angular.extend(aObj, a);
                    aObj.fixDates();
                    return aObj;
                });

                return obj;
            });
        });
    }

    getCompanyRoles(isAdmin: boolean, userId: number): ng.IPromise<CompanyRolesDTO[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ROLE_COMPANY_ROLES + isAdmin + "/" + userId, false).then(x => {
            return x.map(y => {
                let obj = new CompanyRolesDTO();
                angular.extend(obj, y);

                if (obj.roles) {
                    obj.roles = obj.roles.map(r => {
                        let rObj = new UserCompanyRoleDTO();
                        angular.extend(rObj, r);
                        rObj.fixDates();
                        return rObj;
                    });
                } else {
                    obj.roles = [];
                }

                if (obj.attestRoles) {
                    obj.attestRoles = obj.attestRoles.map(a => {
                        let aObj = new CompanyAttestRoleDTO();
                        angular.extend(aObj, a);
                        return aObj;
                    });
                } else {
                    obj.attestRoles = [];
                }

                return obj;
            });
        })
    }

    getUserReplacement(type: UserReplacementType, originUserId: number) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_USER_USER_REPLACEMENT + type + "/" + originUserId, false).then(x => {
            if (x) {
                let obj = new UserReplacementDTO();
                angular.extend(obj, x);
                obj.fixDates();
                return obj;
            } else {
                return x;
            }
        });
    }

    getExtraFields(entity: SoeEntityType, useCache = true): ng.IPromise<ExtraFieldGridDTO[]> {
        return this.httpService.get(Constants.WEBAPI_CORE_EXTRA_FIELDS + entity, useCache).then(x => {
            return x.map(y => {
                let obj = new ExtraFieldGridDTO();
                angular.extend(obj, y);
                return obj;
            });
        })
    }

    getExtraFieldsDict(entity: SoeEntityType, connectedEntity: SoeEntityType, connectedRecordId: number, addEmptyRow: boolean, useCache?: boolean): ng.IPromise<ISmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_CORE_EXTRA_FIELDS + entity + "/" + connectedEntity + "/" + connectedRecordId + "/" + addEmptyRow, useCache);
    }

    getExtraFieldsGrid(entity: SoeEntityType, loadRecords: boolean = false, connectedEntity: SoeEntityType = SoeEntityType.None, connectedRecordId: number = 0, useCache = true) {
        return this.httpService.get(Constants.WEBAPI_CORE_EXTRA_FIELDS_GRID + entity + "/" + loadRecords + "/" + (connectedEntity || 0).toString() + "/" + (connectedRecordId || 0).toString(), useCache);
    }

    getExtraField(extraFieldId: number) {
        return this.httpService.get(Constants.WEBAPI_CORE_EXTRA_FIELD + extraFieldId, false);
    }

    getExtraFieldRecord(extraFieldId: number, recordId: number, entity: number): ng.IPromise<ExtraFieldRecordDTO> {
        return this.httpService.get(Constants.WEBAPI_CORE_EXTRA_FIELD_RECORD + extraFieldId + "/" + recordId + "/" + entity, false);
    }

    getExtraFieldWithRecords(recordId: number, entity: number, langId: number, connectedEntity: number = 0, connectedRecordId: number = 0): ng.IPromise<ExtraFieldRecordDTO[]> {
        return this.httpService.get(Constants.WEBAPI_CORE_EXTRA_FIELDS_WITH_RECORDS + (recordId || 0) + "/" + entity + "/" + langId + "/" + (connectedEntity || 0).toString() + "/" + (connectedRecordId || 0).toString(), false).then(x => {
            return x.map(y => {
                let obj = new ExtraFieldRecordDTO();
                angular.extend(obj, y);
                return obj;
            });
        });
    }

    getBatchUpdate(entityType: SoeEntityType) {
        return this.httpService.get(Constants.WEBAPI_CORE_BATCHUPDATE_GETBATCHUPDATEFORENTITY + entityType, false)
            .then(x => {
                return x.map(y => {
                    let obj = new BatchUpdateDTO();
                    angular.extend(obj, y);
                    obj.added = false;
                    return obj;
                });
            });
    }

    getBatchUpdateFilterOptions(entityType: SoeEntityType) {
        return this.httpService.get(Constants.WEBAPI_CORE_BATCHUPDATE_FILTEROPTIONS + entityType, false);
    }

    getFileContent(fileType: number) {
        return this.httpService.post(Constants.WEBAPI_CORE_IMPORTDYNAMIC_GETFILECONTENT + fileType, false)
    }

    parseRows(val: any) {
        return this.httpService.post(Constants.WEBAPI_CORE_IMPORTDYNAMIC_PARSEROWS, val, false)
    }

    // Attest
    getAttestStateInitial(entity: TermGroup_AttestEntity) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_STATE_INITIAL + entity, false);
    }

    getUserAttestTransitions(entity: TermGroup_AttestEntity, dateFrom?: Date, dateTo?: Date) {
        let dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        let dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();
        return this.httpService.get(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_TRANSITION_USER + entity + "/" + dateFromString + "/" + dateToString, false);
    }

    getAttestStates(entity: TermGroup_AttestEntity, module: SoeModule, addEmptyRow: boolean): ng.IPromise<AttestStateDTO[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_STATE + entity + "/" + module + "/" + addEmptyRow, false);
    }

    getUserValidAttestStates(entity: TermGroup_AttestEntity, dateFrom: Date, dateTo: Date, excludePayrollStates: boolean, employeeGroupId?: number) {
        var dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        var dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();
        if (!employeeGroupId)
            employeeGroupId = 0;
        return this.httpService.get(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_STATE_USER_VALID + entity + "/" + dateFromString + "/" + dateToString + "/" + excludePayrollStates + "/" + employeeGroupId, false);
    }

    hasInitialAttestState(entity: TermGroup_AttestEntity, module: SoeModule): ng.IPromise<boolean> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_STATE_HAS_INITIAL + entity + "/" + module, false);
    }

    canUserCreateInvoice(currentAttestStateId: number): ng.IPromise<boolean> {
        return this.httpService.get(Constants.WEBAPI_BILLING_ORDER_CAN_USER_CREATE_INVOICE + currentAttestStateId, false);
    }

    // Dashboard
    getUserGagues(module: SoeModule): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_CORE_DASHBOARD_USER_GAUGE + module, false);
    }

    getSysGagues(module: SoeModule): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_CORE_DASHBOARD_SYS_GAUGE + module, false);
    }

    getUserGaugeHead(userGaugeHeadId: number = 0, module?: SoeModule): ng.IPromise<any> {
        return this.httpService.get(`${Constants.WEBAPI_CORE_DASHBOARD_USER_GAUGE_HEAD}${userGaugeHeadId}/${module || 0}`, false);
    }

    getUserGaugeHeads(): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_CORE_DASHBOARD_USER_GAUGE_HEADS, false);
    }

    // Widgets
    getAttestFlowWidgetData(): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_CORE_DASHBOARD_WIDGET_ATTESTFLOW, false);
    }

    getMapStartAddress(): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_CORE_DASHBOARD_WIDGET_MAP_START, false);
    }

    getMapLocations(dateFrom: Date): ng.IPromise<any> {
        var dateString: string = (dateFrom) ? dateFrom.toDateTimeString() : null;
        return this.httpService.get(Constants.WEBAPI_CORE_DASHBOARD_WIDGET_MAP_LOCATIONS + dateString, false);
    }

    getMapPlannedOrders(forDate: Date): ng.IPromise<any> {
        var dateString: string = (forDate) ? forDate.toDateTimeString() : null;
        return this.httpService.get(Constants.WEBAPI_CORE_DASHBOARD_WIDGET_MAP_PLANNED_ORDERS + dateString, false);
    }

    getReportWidgetData(): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_CORE_DASHBOARD_WIDGET_REPORTS, false);
    }

    getInsightsWidgetData(reportId: number, dataSelectionId: number, columnSelectionId: number): ng.IPromise<ReportPrintoutDTO> {
        return this.httpService.get(`${Constants.WEBAPI_CORE_DASHBOARD_WIDGET_INSIGHTS}${reportId}/${dataSelectionId}/${columnSelectionId}`, false);
    }

    getDashboardStatisticServiceTypes(): ng.IPromise<ISmallGenericType[]> {
        return this.httpService.getCache(Constants.WEBAPI_CORE_DASHBOARD_WIDGET_PERFORMANCE_ANALYZER_SERVICE_TYPES, null, Constants.CACHE_EXPIRE_LONG);
    }

    getDashboardStatisticTypes(serviceType: number): ng.IPromise<DashboardStatisticType[]> {
        return this.httpService.get(Constants.WEBAPI_CORE_DASHBOARD_WIDGET_PERFORMANCE_ANALYZER_PERFORMANCE_TEST + serviceType, false).then(x => {
            return x.map(y => {
                let obj = new DashboardStatisticType();
                angular.extend(obj, y);
                return obj;
            });
        });
    }

    getPerformanceTestResults(dashboardStatisticTypeKey: string, dateFrom: Date, dateTo: Date, interval: TermGroup_PerformanceTestInterval): ng.IPromise<DashboardStatisticsDTO> {
        var dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        var dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_CORE_DASHBOARD_WIDGET_PERFORMANCE_ANALYZER_PERFORMANCE_TEST + dashboardStatisticTypeKey + "/" + dateFromString + "/" + dateToString + "/" + interval, false).then(x => {
            let obj = new DashboardStatisticsDTO();
            angular.extend(obj, x);

            if (obj.dashboardStatisticRows) {
                obj.dashboardStatisticRows = obj.dashboardStatisticRows.map(r => {
                    let rObj = new DashboardStatisticRowDTO();
                    angular.extend(rObj, r);

                    if (rObj.dashboardStatisticPeriods) {
                        rObj.dashboardStatisticPeriods = rObj.dashboardStatisticPeriods.map(p => {
                            let pObj = new DashboardStatisticPeriodDTO();
                            angular.extend(pObj, p);
                            pObj.fixDates();
                            return pObj;
                        });
                    } else {
                        rObj.dashboardStatisticPeriods = [];
                    }
                    return rObj;
                });
            } else {
                obj.dashboardStatisticRows = [];
            }

            return obj;
        });
    }

    getTaskWatchLogTasks(dateFrom: Date, dateTo: Date, actorCompanyId?: number, userId?: number): ng.IPromise<string[]> {
        var dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        var dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_CORE_DASHBOARD_WIDGET_TASK_WATCH_LOG_TASKS + dateFromString + "/" + dateToString + "/" + (actorCompanyId ? actorCompanyId : "0") + "/" + (userId ? userId : "0"), false);
    }

    getTaskWatchLogResults(task: string, dateFrom: Date, dateTo: Date, interval: TermGroup_PerformanceTestInterval, calculationType: TermGroup_TaskWatchLogResultCalculationType, actorCompanyId?: number, userId?: number): ng.IPromise<DashboardStatisticsDTO> {
        var dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        var dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_CORE_DASHBOARD_WIDGET_TASK_WATCH_LOG_RESULTS + task + "/" + dateFromString + "/" + dateToString + "/" + interval + "/" + calculationType + "/" + (actorCompanyId ? actorCompanyId : "0") + "/" + (userId ? userId : "0"), false).then(x => {
            let obj = new DashboardStatisticsDTO();
            angular.extend(obj, x);

            if (obj.dashboardStatisticRows) {
                obj.dashboardStatisticRows = obj.dashboardStatisticRows.map(r => {
                    let rObj = new DashboardStatisticRowDTO();
                    angular.extend(rObj, r);

                    if (rObj.dashboardStatisticPeriods) {
                        rObj.dashboardStatisticPeriods = rObj.dashboardStatisticPeriods.map(p => {
                            let pObj = new DashboardStatisticPeriodDTO();
                            angular.extend(pObj, p);
                            pObj.fixDates();
                            return pObj;
                        });
                    } else {
                        rObj.dashboardStatisticPeriods = [];
                    }
                    return rObj;
                });
            } else {
                obj.dashboardStatisticRows = [];
            }

            return obj;
        });
    }

    getFileExist(entity: SoeEntityType, recordId: number, fileName: string): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_CORE_FILES_EXIST + entity + "/" + recordId + "/" + fileName, false);
    }

    importProductRows(wholesellerId: number, invoiceId: number, actorCustomerId: number, typeId: TermGroup_InvoiceRowImportType, bytes: any[]) {
        const model = {
            wholesellerId: wholesellerId,
            invoiceId: invoiceId,
            actorCustomerId: actorCustomerId,
            typeId: typeId,
            bytes: bytes,
        };
        return this.httpService.post(Constants.WEBAPI_CORE_CUSTOMERINVOICES_IMPORT, model);
    }

    checkFilesDuplicate(files: IFilesLookupDTO): ng.IPromise<string[]> {
        return this.httpService.post(Constants.WEBAPI_CORE_FILES_EXISTING, files);
    }

    getSysLogWidgetData(clientIpNr: string, nbrOfRecords: number): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_CORE_DASHBOARD_WIDGET_SYS_LOGS + clientIpNr + "/" + nbrOfRecords, false);
    }

    getSystemInfoWidgetData(): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_CORE_DASHBOARD_WIDGET_SYSTEM_INFO, false);
    }

    getTimeTerminals(type: number, onlyActive: boolean, onlyRegistered: boolean, onlySynchronized: boolean, loadSettings: boolean, loadCompanies: boolean, loadTypeNames: boolean, ignoreLimitToAccount: boolean): ng.IPromise<TimeTerminalDTO[]> {
        return this.httpService.get(Constants.WEBAPI_CORE_DASHBOARD_WIDGET_TIME_TERMINAL + "?type=" + type + "&onlyActive=" + onlyActive + "&onlyRegistered=" + onlyRegistered + "&onlySynchronized=" + onlySynchronized + "&loadSettings=" + loadSettings + "&loadCompanies=" + loadCompanies + "&loadTypeNames=" + loadTypeNames + "&ignoreLimitToAccount=" + ignoreLimitToAccount, false).then(x => {
            return x.map(y => {
                let obj = new TimeTerminalDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    getEmployeeRequestsWidgetData(setEmployeeRequestTypeNames: boolean): ng.IPromise<EmployeeRequestsGaugeDTO[]> {
        return this.httpService.get(Constants.WEBAPI_CORE_DASHBOARD_WIDGET_EMPLOYEE_REQUESTS + setEmployeeRequestTypeNames, false).then(x => {
            return x.map(y => {
                let obj = new EmployeeRequestDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    getTimeStampAttendanceWidgetData(showMode: TermGroup_TimeStampAttendanceGaugeShowMode, onlyIn: boolean): ng.IPromise<TimeStampAttendanceGaugeDTO[]> {
        return this.httpService.get(Constants.WEBAPI_CORE_DASHBOARD_WIDGET_TIMESTAMP_ATTENDANCE + showMode + "/" + onlyIn, false);
    }

    getMyScheduleMyShifts(employeeId: number, dateFrom: Date, dateTo: Date): ng.IPromise<ShiftDTO[]> {
        var dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        var dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_CORE_DASHBOARD_WIDGET_MY_SCHEDULE + employeeId + "/" + dateFromString + "/" + dateToString, false).then(x => {
            return x.map(y => {
                var obj = new ShiftDTO();
                angular.extend(obj, y);
                obj.fixDates();
                obj.fixColors();

                return obj;
            });
        });
    }

    getMyScheduleOpenShifts(employeeId: number, dateFrom: Date, dateTo: Date): ng.IPromise<ShiftDTO[]> {
        var dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        var dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_CORE_DASHBOARD_WIDGET_MY_SCHEDULE_OPEN_SHIFTS + employeeId + "/" + dateFromString + "/" + dateToString, false).then(x => {
            return x.map(y => {
                var obj = new ShiftDTO();
                angular.extend(obj, y);
                obj.fixDates();
                obj.fixColors();

                return obj;
            });
        });
    }

    getMyScheduleColleaguesShifts(employeeId: number, dateFrom: Date, dateTo: Date): ng.IPromise<ShiftDTO[]> {
        var dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        var dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_CORE_DASHBOARD_WIDGET_MY_SCHEDULE_MY_COLLEAGUES_SCHEDULE + employeeId + "/" + dateFromString + "/" + dateToString, false).then(x => {
            return x.map(y => {
                var obj = new ShiftDTO();
                angular.extend(obj, y);
                obj.fixDates();
                obj.fixColors();

                return obj;
            });
        });
    }

    getMyShiftsWidgetData(employeeId: number, dateFrom: Date, dateTo: Date): ng.IPromise<MyShiftsGaugeDTO[]> {
        var dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        var dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_CORE_DASHBOARD_WIDGET_MY_SHIFTS + employeeId + "/" + dateFromString + "/" + dateToString, false).then(x => {
            return x.map(y => {
                var obj = new MyShiftsGaugeDTO();
                angular.extend(obj, y);

                obj.fixDates();
                if (obj.date)
                    obj.dayName = CalendarUtility.getDayName(obj.date.dayOfWeek()).toUpperCaseFirstLetter();

                return obj;
            });
        });
    }

    getOpenShiftsWidgetData(employeeId: number, dateFrom: Date, dateTo: Date): ng.IPromise<OpenShiftsGaugeDTO[]> {
        var dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        var dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();
        return this.httpService.get(Constants.WEBAPI_CORE_DASHBOARD_WIDGET_OPEN_SHIFTS + employeeId + "/" + dateFromString + "/" + dateToString, false).then(x => {
            return x.map(y => {
                var obj = new OpenShiftsGaugeDTO();
                angular.extend(obj, y);

                obj.fixDates();
                if (obj.date)
                    obj.dayName = CalendarUtility.getDayName(obj.date.dayOfWeek()).toUpperCaseFirstLetter();

                return obj;
            });
        });
    }

    getWantedShiftsWidgetData(): ng.IPromise<WantedShiftsGaugeDTO[]> {
        return this.httpService.get(Constants.WEBAPI_CORE_DASHBOARD_WIDGET_WANTED_SHIFTS, false).then(x => {
            return x.map(y => {
                var obj = new WantedShiftsGaugeDTO();
                angular.extend(obj, y);

                obj.fixDates();
                if (obj.date)
                    obj.dayName = CalendarUtility.getDayName(obj.date.dayOfWeek()).toUpperCaseFirstLetter();

                return obj;
            });
        });
    }

    getIdsForEmployeeAndGroup(): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_CORE_DASHBOARD_WIDGET_USER_GROUP_IDS, false);
    }

    getProjectTimeInvoiceTransactions(projectId: number): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_CORE_PROJECT_TIME_INVOICE_TRANSACTIONS + projectId, false);
    }

    getTextBlock(textBlockId: number): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_CORE_TEXTBLOCK + textBlockId, false);
    }

    getTextBlocks(entity: number): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_CORE_TEXTBLOCKS + entity, false);
    }

    getCustomerInvoiceNumbersDict(customerId: number, originType: SoeOriginType, classification: SoeOriginStatusClassification, registrationType: OrderInvoiceRegistrationType, orderByNumber: boolean = false) {
        return this.httpService.get(Constants.WEBAPI_CORE_CUSTOMERINVOICES_NUMBERSDICT + customerId + "/" + originType + "/" + classification + "/" + registrationType + "/" + orderByNumber, false);
    }

    getCustomerInvoiceRows(invoiceId: number): ng.IPromise<ProductRowDTO[]> {
        return this.httpService.get(Constants.WEBAPI_CORE_CUSTOMERINVOICES_ROWS + invoiceId, false);
    }

    getCustomerInvoiceRowsSmall(invoiceId: number): ng.IPromise<ICustomerInvoiceRowDetailDTO[]> {
        return this.httpService.get(Constants.WEBAPI_CORE_CUSTOMERINVOICES_ROWSSMALL + invoiceId, false);
    }

    getServiceOrdersForAgreementDetails(invoiceId: number): ng.IPromise<any[]> {
        return this.httpService.get(Constants.WEBAPI_CORE_CUSTOMERINVOICES_SERVICEORDERSFORAGREEMENT + invoiceId, false);
    }

    getChecklistHeadsDict(type: TermGroup_ChecklistHeadType) {
        return this.httpService.get(Constants.WEBAPI_CORE_CHECKLISTS_HEADS_DICT + type, false);
    }

    getChecklistHeads(type: TermGroup_ChecklistHeadType, loadRows: boolean): ng.IPromise<IChecklistHeadDTO[]> {
        return this.httpService.get(Constants.WEBAPI_CORE_CHECKLISTS_HEADS + type + "/" + loadRows, false);
    }
    getChecklistHead(checkListHeadId: number, loadRows: boolean): ng.IPromise<IChecklistHeadDTO> {
        return this.httpService.get(Constants.WEBAPI_CORE_CHECKLISTS_HEADS_HEAD + checkListHeadId + "/" + loadRows, false);
    }

    getChecklistHeadRecords(entity: SoeEntityType, recordId: number): ng.IPromise<ChecklistHeadRecordCompactDTO[]> {
        return this.httpService.get(Constants.WEBAPI_CORE_CHECKLISTS_HEADRECORDS + entity + "/" + recordId, false);
    }

    getChecklistRowRecords(entity: SoeEntityType, recordId: number): ng.IPromise<IChecklistExtendedRowDTO[]> {
        return this.httpService.get(Constants.WEBAPI_CORE_CHECKLISTS_ROWRECORDS + entity + "/" + recordId, false);
    }

    getChecklistSignatures(entity: SoeEntityType, recordId: number, useThumbnails: boolean): ng.IPromise<IImagesDTO[]> {
        return this.httpService.get(Constants.WEBAPI_CORE_CHECKLISTS_SIGNATURES + entity + "/" + recordId + "/" + useThumbnails, false);
    }

    getChecklistMultipleChoiceQuestions(questionIds: number[]): ng.IPromise<ICheckListMultipleChoiceAnswerRowDTO[]> {
        const model = { Numbers: questionIds };
        return this.httpService.post(Constants.WEBAPI_CORE_CHECKLISTS_MULTICHOICEQ, model);
    }

    getSkills(useCache: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_TIME_SCHEDULE_SKILL, Constants.WEBAPI_ACCEPT_DTO, Constants.CACHE_EXPIRE_LONG, !useCache);
    }

    getEmployeePostSkills(employeePostId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_SKILL_POST + employeePostId, false);
    }

    getInventoryTriggerAccounts() {
        return this.httpService.get(Constants.WEBAPI_CORE_INVENTORYTRIGGERACCOUNTS, false);
    }

    generateReportForEdi(ediEntries: any[]) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_TRANSFER_EDI_GENERATEPDF, ediEntries);
    }

    generateReportForFinvoice(ediEntries: any[]) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_TRANSFER_FINVOICE_GENERATEPDF, ediEntries);
    }

    getLinkedShifts(shiftId: number) {
        return this.httpService.get(Constants.WEBAPI_CORE_LINKED_SHIFTS + shiftId, false).then(x => {
            return x.map(y => {
                var obj = new ShiftDTO(y.type);
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    getEmailTemplates() {
        return this.httpService.get(Constants.WEBAPI_CORE_EMAILTEMPLATES, false);
    }

    getEmailTemplatesByType(type: number) {
        return this.httpService.get(Constants.WEBAPI_CORE_EMAILTEMPLATES_BYTYPE + type, false);
    }

    getEmailTemplate(emailTemplateId: number) {
        return this.httpService.get(Constants.WEBAPI_CORE_EMAILTEMPLATE + emailTemplateId, false);
    }

    getXeMail(messageId: number) {
        return this.httpService.get(Constants.WEBAPI_CORE_XEMAIL + messageId, false).then((x: MessageEditDTO) => {
            if (x) {
                let obj = new MessageEditDTO();
                angular.extend(obj, x);
                obj.fixDates();

                obj.recievers = obj.recievers.map(r => {
                    let rObj = new MessageRecipientDTO();
                    angular.extend(rObj, r);
                    rObj.fixDates();
                    return rObj;
                });

                obj.attachments = obj.attachments.map(a => {
                    let rObj = new MessageAttachmentDTO();
                    angular.extend(rObj, a);
                    return rObj;
                });

                return obj;
            } else {
                return x;
            }
        });
    }

    getXeMailItems(xeMailType: number, messageId?: number): ng.IPromise<MessageGridDTO[]> {
        return this.httpService.get(Constants.WEBAPI_CORE_XEMAIL_GRID + xeMailType + "/" + (messageId ? messageId : 0), false).then(x => {
            return x.map(y => {
                let obj = new MessageGridDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        })
    }

    getNbrOfUnreadMessages(): ng.IPromise<number> {
        return this.httpService.get(Constants.WEBAPI_CORE_XEMAIL_NBR_OF_UNREAD_MESSAGES, false, null, true);
    }

    getEmployeesByAccountSetting(): ng.IPromise<ISmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_CORE_XEMAIL_EMPLOYEES_BY_ACCOUNT, false);
    }

    getRecurrenceIntervalText(recurrenceInterval: string): ng.IPromise<string> {
        // Can't send * in query string, replace with |
        return this.httpService.get(Constants.WEBAPI_CORE_RECURRENCE_INTERVAL_TEXT + recurrenceInterval.replace(/\*/g, '|'), false);
    }

    getNextExecutionTime(recurrenceInterval: string): ng.IPromise<Date> {
        // Can't send * in query string, replace with |
        return this.httpService.get(Constants.WEBAPI_CORE_RECURRENCE_INTERVAL_NEXT_EXECUTION_TIME + recurrenceInterval.replace(/\*/g, '|'), false).then(x => {
            return CalendarUtility.convertToDate(x);
        });
    }

    getLastUsedSequenceNumber(entityName: string) {
        return this.httpService.get(Constants.WEBAPI_CORE_SEQUENCENUMBER_LASTUSED + entityName, false);
    }

    getSysLanguages(addEmptyRow: boolean, useCache: boolean): ng.IPromise<ISmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_CORE_SYS_LANGUAGE + addEmptyRow, useCache);
    }

    getSysPositions(countryId: number, langId: number, useCache: boolean): ng.IPromise<SysPositionGridDTO[]> {
        return this.httpService.get(Constants.WEBAPI_CORE_SYS_POSITION + countryId + "/" + langId, useCache).then(x => {
            return x.map(y => {
                let obj = new SysPositionGridDTO();
                angular.extend(obj, y);
                return obj;
            });
        });
    }

    getSysPositionGrid(useCache: boolean): ng.IPromise<SysPositionGridDTO[]> {
        return this.httpService.get(Constants.WEBAPI_CORE_SYS_POSITION_GRID, useCache).then(x => {
            return x.map(y => {
                let obj = new SysPositionGridDTO();
                angular.extend(obj, y);
                return obj;
            });
        });
    }

    getSysPosition(sysPositionId: number): ng.IPromise<SysPositionDTO> {
        return this.httpService.get(Constants.WEBAPI_CORE_SYS_POSITION + sysPositionId, false);
    }

    getFirstEligableTimeForEmployee(employeeId: number, date: Date) {
        return this.httpService.get(Constants.WEBAPI_CORE_PROJECT_EMPLOYEEFIRSTTIME + employeeId + "/" + date.toDateTimeString(), false);
    }

    getEmployeeScheduleAndTransactionInfo(employeeId: number, date: Date) {
        return this.httpService.get(Constants.WEBAPI_CORE_PROJECT_EMPLOYEESCHEDULEANDTRANSACTIONINFO + employeeId + "/" + date.toDateTimeString(), false);
    }

    getPositions(loadSkills: boolean, useCache: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_TIME_EMPLOYEE_POSITION + "?loadSkills=" + loadSkills, Constants.WEBAPI_ACCEPT_DTO, Constants.CACHE_EXPIRE_LONG, !useCache);
    }

    validBankNumberSE(clearing: string, bankAccountNr: string, sysPaymentType?: number): ng.IPromise<boolean> {
        var deferred = this.$q.defer<boolean>();

        if (!clearing || !bankAccountNr) {
            var result = false;
            deferred.resolve(false);
        }
        else {
            this.httpService.get(Constants.WEBAPI_CORE_VALIDATOR_VALIDBANKNUMBERSE + "?clearing=" + clearing + "&bankAccountNr=" + bankAccountNr + "&sysPaymentType=" + sysPaymentType, false).then((x) => {
                deferred.resolve(x);
            });
        }

        return deferred.promise;
    }

    validIBANNumber(iban: string): ng.IPromise<boolean> {
        var deferred = this.$q.defer<boolean>();

        this.httpService.get(Constants.WEBAPI_CORE_VALIDATOR_VALIDIBANNUMBER + "?iban=" + iban, false).then((x) => {
            deferred.resolve(x);
        });

        return deferred.promise;
    }

    validSocialSecurityNumber(source: string, checkValidDate: boolean, mustSpecifyCentury: boolean, mustSpecifyDash: boolean, sex: TermGroup_Sex): ng.IPromise<boolean> {
        var deferred = this.$q.defer<boolean>();

        if (!source) {
            var result = false;
            deferred.resolve(false);
        }
        else {
            this.httpService.get(Constants.WEBAPI_CORE_VALIDATOR_VALIDSOCIALSECURITYNUMBER + "?source=" + source + "&checkValidDate=" + checkValidDate + "&mustSpecifyCentury=" + mustSpecifyCentury + "&mustSpecifyDash=" + mustSpecifyDash + "&sex=" + sex, false).then((x) => {
                deferred.resolve(x);
            });
        }

        return deferred.promise;
    }

    getBicFromIban(iban: string): ng.IPromise<string> {
        return this.httpService.get(Constants.WEBAPI_CORE_PAYMENTINFORMATION_BICFROMIBAN + iban, false);
    }

    getContactPersonForExport(actorId: number) {
        return this.httpService.get(Constants.WEBAPI_CORE_CONTACT_PERSON_EXPORT + actorId, false);
    }

    getShiftTypesForUsersCategories(employeeId: number, isAdmin: boolean, blockTypes: TermGroup_TimeScheduleTemplateBlockType[]) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_SHIFT_TYPE_GET_SHIFT_TYPES_FOR_USERS_CATEGORIES + "?employeeId=" + employeeId + "&isAdmin=" + isAdmin + "&blockTypes=" + blockTypes.join(','), false, Constants.WEBAPI_ACCEPT_GRID_DTO);
    }

    getAccountingFromString(accountingString: string): ng.IPromise<AccountingSettingsRowDTO> {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_GET_ACCOUNTING_FROM_STRING + accountingString, false).then(x => {
            var obj = new AccountingSettingsRowDTO(x.type);
            angular.extend(obj, x);
            return obj;
        });
    }

    getAdditionDeductionTimeCodes(checkInvoiceProduct: boolean = false, isMySelf: boolean = false) {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_CODE_ADDITIONDEDUCTION + checkInvoiceProduct + "/" + isMySelf, false);
    }

    getAccountIdsFromHierarchyByUser(dateFrom: Date, dateTo: Date, useMaxAccountDimId: boolean = false, includeVirtualParented: boolean = false, includeOnlyChildrenOneLevel: boolean = false, onlyDefaultAccounts: boolean = true, useEmployeeAccountIfNoAttestRole: boolean = false, includeAbstract: boolean = false, employeeId: number = 0): ng.IPromise<number[]> {
        let dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        let dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_MANAGE_USER_ACCOUNTIDS_FROM_HIERARCHY_BY_USER + dateFromString + "/" + dateToString + "/" + useMaxAccountDimId + "/" + includeVirtualParented + "/" + includeOnlyChildrenOneLevel + "/" + onlyDefaultAccounts + "/" + useEmployeeAccountIfNoAttestRole + "/" + includeAbstract + "/" + employeeId, false);
    }

    getAccountsFromHierarchy(accountId: number, includeVirtualParented: boolean, includeOnlyChildrenOneLevel: boolean): ng.IPromise<AccountDTO[]> {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_ACCOUNTS_FROM_HIERARCHY + (accountId || 0) + "/" + includeVirtualParented + "/" + includeOnlyChildrenOneLevel, false);
    }

    getAccountsFromHierarchyByUser(dateFrom: Date, dateTo: Date, useMaxAccountDimId: boolean = false, includeVirtualParented: boolean = false, includeOnlyChildrenOneLevel: boolean = false, onlyDefaultAccounts: boolean = true, useEmployeeAccountIfNoAttestRole: boolean = false, companyId: number = 0): ng.IPromise<AccountDTO[]> {
        let dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        let dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_MANAGE_USER_ACCOUNTS_FROM_HIERARCHY_BY_USER + dateFromString + "/" + dateToString + "/" + useMaxAccountDimId + "/" + includeVirtualParented + "/" + includeOnlyChildrenOneLevel + "/" + onlyDefaultAccounts + "/" + useEmployeeAccountIfNoAttestRole + "/" + companyId, false);
    }

    getAccountsFromHierarchyByUserSetting(dateFrom: Date, dateTo: Date, useMaxAccountDimId: boolean = false, includeVirtualParented: boolean = false, includeOnlyChildrenOneLevel: boolean = false, useDefaultEmployeeAccountDimEmployee: boolean = false): ng.IPromise<AccountDTO[]> {
        let dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        let dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_MANAGE_USER_ACCOUNTS_FROM_HIERARCHY_BY_USERSETTING + dateFromString + "/" + dateToString + "/" + useMaxAccountDimId + "/" + includeVirtualParented + "/" + includeOnlyChildrenOneLevel + "/" + useDefaultEmployeeAccountDimEmployee, false);
    }

    getSiblingAccounts(accountId: number): ng.IPromise<AccountDTO[]> {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_ACCOUNTS_SIBLING_ACCOUNTS + accountId, false);
    }

    getSelectableEmployeeShiftAccountIds(employeeId: number, date: Date): ng.IPromise<number[]> {
        let dateString: string = null;
        if (date)
            dateString = date.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_ACCOUNTS_SELECTABLE_EMPLOYEE_SHIFT_ACCOUNTIDS + employeeId + "/" + dateString, false);
    }

    getEmployeeRequests(employeeId: number, dateFrom: Date, dateTo: Date): ng.IPromise<EmployeeRequestDTO[]> {
        let dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        let dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_EMPLOYEE_REQUEST + employeeId + "/" + dateFromString + "/" + dateToString, false).then(x => {
            return x.map(y => {
                let obj = new EmployeeRequestDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    getEventHistories(type: TermGroup_EventHistoryType, entity: SoeEntityType, recordId: number, dateFrom: Date, dateTo: Date, setNames: boolean): ng.IPromise<EventHistoryDTO[]> {
        let dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        let dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_CORE_EVENT_HISTORY + type + "/" + entity + "/" + recordId + "/" + dateFromString + "/" + dateToString + "/" + setNames, false).then(x => {
            return x.map(y => {
                let obj = new EventHistoryDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    getEventHistory(eventHistroryId: number, setNames: boolean): ng.IPromise<EventHistoryDTO> {
        return this.httpService.get(Constants.WEBAPI_CORE_EVENT_HISTORY + eventHistroryId + "/" + setNames, false).then(x => {
            let obj = new EventHistoryDTO();
            angular.extend(obj, x);
            obj.fixDates();
            return obj;
        });
    }

    getNbrOfEventsInBatch(type: TermGroup_EventHistoryType, batchId: number): ng.IPromise<number> {
        return this.httpService.get(Constants.WEBAPI_CORE_EVENT_HISTORY_BATCH_COUNT + type + "/" + batchId, false);
    }

    getExpenseRow(expenseRowId: number) {
        return this.httpService.get(Constants.WEBAPI_EXPENSE_ROW + expenseRowId, false);
    }

    getProductPriceForInvoice(productId: number, customerInvoiceId: number, quantity: number): ng.IPromise<number> {
        return this.httpService.get(Constants.WEBAPI_BILLING_PRODUCT_PRICES_CUSTOMERINVOICE + productId + "/" + customerInvoiceId + "/" + quantity, false);
    }

    getScheduledJobHeadsDict(addEmptyRow: boolean, useCache: boolean): ng.IPromise<ISmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_REGISTRY_SCHEDULED_JOB_HEAD + "?addEmptyRow=" + addEmptyRow, useCache, Constants.WEBAPI_ACCEPT_GENERIC_TYPE);
    }

    getCompany(actorCompanyId: number) {
        return this.httpService.get(Constants.WEBAPI_CORE_COMPANY + actorCompanyId, false);
    }

    getSysBanks() {
        return this.httpService.get(Constants.WEBAPI_CORE_COMPANY, true);
    }

    getCompaniesByLicense(licenseId: string, onlyTemplates: boolean) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_COMPANY_COMPANIES_BY_LICENSE + licenseId + "/" + onlyTemplates, false);
    }

    getUserSelections(type: UserSelectionType): ng.IPromise<ISmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_USER_USER_SELECTION_LIST + (type || 0), false);
    }

    getUserSelection(userSelectionId: number): ng.IPromise<UserSelectionDTO> {
        return this.httpService.get(Constants.WEBAPI_MANAGE_USER_USER_SELECTION + userSelectionId, false).then(x => {
            let obj = new UserSelectionDTO();
            angular.extend(obj, x);
            obj.setTypes();
            obj.setSelectionTypes();
            return obj;
        });
    }
    getSuppliersDict(onlyActive: boolean, addEmptyRow: boolean, useCache: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_SUPPLIER_SUPPLIER + "?onlyActive=" + onlyActive + "&addEmptyRow=" + addEmptyRow, Constants.WEBAPI_ACCEPT_GENERIC_TYPE, Constants.CACHE_EXPIRE_VERY_SHORT, !useCache).then(suppliers => {
            return _.sortBy(suppliers, 'name');
        });
    }

    getCustomers(onlyActive: boolean, addEmptyRow: boolean, useCache: boolean): ng.IPromise<SmallGenericType[]> {
        var customerDict: SmallGenericType[] = [];
        return this.httpService.getCache(Constants.WEBAPI_CORE_CUSTOMER_CUSTOMER + "?onlyActive=" + onlyActive + "&addEmptyRow=" + addEmptyRow, Constants.WEBAPI_ACCEPT_GENERIC_TYPE, Constants.CACHE_EXPIRE_VERY_SHORT, !useCache).then(customers => {
            customerDict = customers;
            return _.sortBy(customerDict, 'name');
        });
    }

    validateEcomDeletion(entityType: number, contactType: number, contactEcomId: number) {
        return this.httpService.get(Constants.WEBAPI_CORE_VALIDATOR_VALIDATEECOMDELETION + entityType + "/" + contactType + "/" + contactEcomId, false);
    }

    getIntrastatTransactions(invoiceId: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_ORDER_INTRASTAT_TRANSACTIONS + invoiceId, false);
    }

    getIntrastatTransactionsForExport(intrastatType: IntrastatReportingType, dateFrom: Date, dateTo: Date) {
        var dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        var dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_BILLING_ORDER_INTRASTAT_TRANSACTIONS_FOREXPORT + intrastatType + "/" + dateFromString + "/" + dateToString, false);
    }

    signatoryContractUsesPermission(permission: TermGroup_SignatoryContractPermissionType): ng.IPromise<boolean> {
        return this.httpService.get(Constants.WEBAPI_CORE_SIGNATORYCONTRACT_USESFORPERMISSION + permission, false);
    }
    signatoryContractAuthorize(permission: TermGroup_SignatoryContractPermissionType): ng.IPromise<GetPermissionResultDTO> {
        return this.httpService.get(Constants.WEBAPI_CORE_SIGNATORYCONTRACT_AUTHORIZE + permission, false);
    }

    // POST
    saveContactPerson(updatedContactPerson: ContactPersonDTO): ng.IPromise<any> {
        updatedContactPerson.categoryRecords = []
        updatedContactPerson.categoryIds.forEach(id => {
            updatedContactPerson.categoryRecords.push({ categoryId: id, default: false })
        })
        return this.httpService.post(Constants.WEBAPI_CORE_CONTACT_PERSON, updatedContactPerson);
    }

    saveCompanyInformation(information: InformationDTO): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_MANAGE_REGISTRY_COMPANY_INFORMATION, information);
    }

    saveDocument(document: DocumentDTO, fileData: any): ng.IPromise<IActionResult> {
        var model = {
            document: document,
            fileData: fileData
        }

        return this.httpService.post(Constants.WEBAPI_CORE_DOCUMENT, model);
    }

    setDocumentAsRead(dataStorageId: number, confirmed: boolean): ng.IPromise<IActionResult> {
        var model = {
            dataStorageId: dataStorageId,
            confirmed: confirmed
        }

        return this.httpService.post(Constants.WEBAPI_CORE_DOCUMENT_SET_AS_READ, model);
    }

    setInformationAsRead(informationId: number, sysInformationId, confirmed: boolean, hidden: boolean): ng.IPromise<IActionResult> {
        var model = {
            informationId: informationId,
            sysInformationId: sysInformationId,
            confirmed: confirmed,
            hidden: hidden
        }

        return this.httpService.post(Constants.WEBAPI_CORE_INFORMATION_SET_AS_READ, model);
    }

    saveEmailTemplate(emailTemplate: IEmailTemplateDTO): ng.IPromise<any> {
        return this.httpService.post(Constants.WEBAPI_CORE_EMAILTEMPLATE, emailTemplate);
    }

    saveSysGridState(grid: string, gridState: string) {
        var model = { grid: grid, gridState: gridState };
        return this.httpService.post(Constants.WEBAPI_CORE_SYS_GRID_STATE, model);
    }

    saveUserGridState(grid: string, gridState: string): ng.IPromise<IActionResult> {
        var model = { grid: grid, gridState: gridState };
        return this.httpService.post(Constants.WEBAPI_CORE_USER_GRID_STATE, model);
    }

    saveBoolSetting(settingMainType: number, settingTypeId: number, value: boolean) {
        var model = { settingMainType: settingMainType, settingTypeId: settingTypeId, boolValue: value };
        return this.httpService.post(Constants.WEBAPI_CORE_USER_COMPANY_SETTING_BOOL, model);
    }

    saveIntSetting(settingMainType: number, settingTypeId: number, value: number) {
        var model = { settingMainType: settingMainType, settingTypeId: settingTypeId, intValue: value };
        return this.httpService.post(Constants.WEBAPI_CORE_USER_COMPANY_SETTING_INT, model);
    }

    saveStringSetting(settingMainType: number, settingTypeId: number, value: string) {
        var model = { settingMainType: settingMainType, settingTypeId: settingTypeId, stringValue: value };
        return this.httpService.post(Constants.WEBAPI_CORE_USER_COMPANY_SETTING_STRING, model);
    }

    saveUserCompanySettings(settings: UserCompanySettingEditDTO[]): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_CORE_USER_COMPANY_SETTING, settings);
    }

    saveHelp(help: SysHelpDTO): ng.IPromise<any> {
        help.language = CoreUtility.language;
        return this.httpService.post(Constants.WEBAPI_CORE_HELP, help);
    }

    saveUserGaugeHead(head: any) {
        return this.httpService.post(Constants.WEBAPI_CORE_DASHBOARD_USER_GAUGE_HEAD, head);
    }

    addUserGauge(sysGaugeId: number, sort: number, module: SoeModule, userGaugeHeadId?: number) {
        var model = { sysGaugeId: sysGaugeId, sort: sort, module: module, userGaugeHeadId: userGaugeHeadId };
        return this.httpService.post(Constants.WEBAPI_CORE_DASHBOARD_USER_GAUGE, model);
    }

    saveUserGaugeSettings(userGaugeId: number, settings: UserGaugeSettingDTO[]): ng.IPromise<IActionResult> {
        const model = {
            userGaugeId: userGaugeId,
            settings: settings
        };
        return this.httpService.post(Constants.WEBAPI_CORE_DASHBOARD_USER_GAUGE_SETTING, model);
    }

    saveUserGaugeSort(userGaugeId: number, sort: number): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_CORE_DASHBOARD_USER_GAUGE_SORT + userGaugeId + "/" + sort, null);
    }

    saveEvaluateAllWorkRulesByPass(result: IEvaluateWorkRulesActionResult, employeeId: number) {
        const model = {
            result: result,
            employeeId: employeeId,
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_EVALUATE_WORK_RULE_SAVE_BYPASS, model);
    }

    saveTextBlock(textBlockDto: TextBlockDTO, entity: number, translations: any[]): ng.IPromise<any> {
        const model = {
            TextBlock: textBlockDto,
            Entity: entity,
            translations: translations
        };
        return this.httpService.post(Constants.WEBAPI_CORE_TEXTBLOCK, model);
    }

    copyCustomerInvoiceRows(rows: ProductRowDTO[], originType: SoeOriginType, targetId: number, originId = 0, updateOrigin = false, recalculate = false): ng.IPromise<any> {
        const model = {
            RowsToCopy: rows,
            OriginType: originType,
            TargetId: targetId,
            UpdateOrigin: updateOrigin,
            Recalculate: recalculate,
            OriginId: originId,
        };
        return this.httpService.post(Constants.WEBAPI_CORE_CUSTOMERINVOICES_COPYROWS, model);
    }

    sendMessage(messageEditDto: MessageEditDTO): ng.IPromise<any> {
        if (!messageEditDto.shortText)
            messageEditDto.shortText = messageEditDto.text;
        return this.httpService.post(Constants.WEBAPI_CORE_XEMAIL, messageEditDto);
    }

    setMessageAsRead(date: Date, messageId: number): ng.IPromise<any> {
        let dateString: string = null;
        if (date)
            dateString = date.toDateTimeString();

        return this.httpService.post(Constants.WEBAPI_CORE_XEMAIL_SET_READ_DATE + dateString + "/" + messageId, null);
    }

    setMessagesAsRead(messageIds: number[]): ng.IPromise<IActionResult> {
        const model = {
            numbers: messageIds
        }

        return this.httpService.post(Constants.WEBAPI_CORE_XEMAIL_SET_AS_READ, model);
    }

    setMessagesAsUnread(messageIds: number[]): ng.IPromise<IActionResult> {
        var model = {
            numbers: messageIds
        }

        return this.httpService.post(Constants.WEBAPI_CORE_XEMAIL_SET_AS_UNREAD, model);
    }

    deleteMessages(messageIds: number[], incoming: boolean) {
        var model = {
            numbers: messageIds,
        }

        return this.httpService.post(incoming ? Constants.WEBAPI_CORE_XEMAIL_DELETE_MESSAGES_INCOMING : Constants.WEBAPI_CORE_XEMAIL_DELETE_MESSAGES_OUTGOING, model);
    }

    validateSaveProjectTimeBlocks(items: any) {
        return this.httpService.post(Constants.WEBAPI_CORE_PROJECT_VALIDATEPROJECTTIMEBLOCKSAVEDTO, items);
    }

    connectDataStorageToEntity(dataStorageRecordId: number, recordId: number, removeExisting: boolean, entity: SoeEntityType, dataStorageType: SoeDataStorageRecordType): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_CORE_FILES_DATASTORAGE_CONNECT_TO_ENTITY + dataStorageRecordId + "/" + recordId + "/" + removeExisting + "/" + entity + "/" + dataStorageType, null);
    }

    updateFileRoleIds(dataStorageRecordId: number, roleIds: number[]): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_CORE_FILES_ROLEIDS + dataStorageRecordId + "/" + roleIds.join(','), null);
    }

    saveEmployeeRequests(employeeId: number, deletedEmployeeRequests: EmployeeRequestDTO[], editedOrNewRequests: EmployeeRequestDTO[]): ng.IPromise<IActionResult> {
        var model = {
            employeeId: employeeId,
            deletedEmployeeRequests: deletedEmployeeRequests,
            editedOrNewRequests: editedOrNewRequests
        }

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_EMPLOYEE_REQUEST, model);
    }

    saveExpenseRowsValidation(expenseRows: any[]) {
        var model = {
            expenseRows: expenseRows
        }

        return this.httpService.post(Constants.WEBAPI_EXPENSE_ROWS_VALIDATE, model);
    }

    saveExpenseRows(expenseRows: any[], customerInvoiceId: number) {
        var model = {
            customerInvoiceId: customerInvoiceId,
            expenseRows: expenseRows
        }

        return this.httpService.post(Constants.WEBAPI_EXPENSE_ROWS, model);
    }

    getCustomerInvoicesBySearch(number: string, externalNr: string, customerNr: string, customerName: string, internalText: string, projectNr: string, projectName: string, originType: number, ignoreChildren: boolean, customerId?: number, projectId?: number, invoiceId?: number, userId?: number, includePreliminary?: boolean, includeVoucher?: boolean, fullyPaid?: boolean) {
        var model = {
            number: number,
            customerNr: customerNr,
            customerName: customerName,
            internalText: internalText,
            projectNr: projectNr,
            projectName: projectName,
            originType: originType,
            customerId: customerId,
            projectId: projectId,
            ignoreChildren: ignoreChildren,
            ignoreInvoiceId: invoiceId,
            userId: userId,
            includePreliminary: includePreliminary,
            includeVoucher: includeVoucher,
            fullyPaid: fullyPaid,
            externalNr: externalNr
        };

        return this.httpService.post(Constants.WEBAPI_CORE_CUSTOMERINVOICES_SEARCHSMALL, model);
    }

    saveSysPosition(sysPosition: SysPositionDTO): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_CORE_SYS_POSITION, sysPosition);
    }

    addSysLogMessage(requestUri: string, message: string, exception: string, isWarning: boolean) {
        var model = {
            requestUri: requestUri,
            message: message,
            exception: exception,
            isWarning: isWarning
        }
        return this.httpService.post(Constants.WEBAPI_CORE_SYS_LOG_ADD, model);
    }

    saveEventHistory(eventHistory: EventHistoryDTO): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_CORE_EVENT_HISTORY, eventHistory);
    }

    saveExtraField(extraField: ExtraFieldDTO): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_CORE_EXTRA_FIELD, extraField);
    }

    saveExtraFieldRecords(extraFields: ExtraFieldRecordDTO[], entity: number, recordId: number): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_CORE_EXTRA_FIELDS_WITH_RECORDS, { records: extraFields.map(r => r.toPlainObject()), entity: entity, recordId: recordId });
    }
    performBatchUpdate(model: BatchUpdateModel): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_CORE_BATCHUPDATE_PERFORMBATCHUPDATE, model);
    }

    createCustomerSpecificExport(selection: any) {
        return this.httpService.post(Constants.WEBAPI_CORE_CUSTOMER_EXPORTS, selection);
    }

    initiateDocumentSigning(head: AttestWorkFlowHeadDTO): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_WORK_FLOW_INITIATE_DOCUMENT_SIGNING, head);
    }

    saveDocumentSigningAnswer(attestWorkFlowRowId: number, signeeStatus: SigneeStatus, comment: string): ng.IPromise<IActionResult> {
        let model = {
            attestWorkFlowRowId: attestWorkFlowRowId,
            signeeStatus: signeeStatus,
            comment: comment
        }

        return this.httpService.post(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_WORK_FLOW_SAVE_DOCUMENT_SIGNING_ANSWER, model);
    }

    cancelDocumentSigning(attestWorkFlowHeadId: number, comment: string): ng.IPromise<IActionResult> {
        let model = {
            attestWorkFlowHeadId: attestWorkFlowHeadId,
            comment: comment
        }

        return this.httpService.post(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_WORK_FLOW_CANCEL_DOCUMENT_SIGNING, model);
    }

    refreshBatchUpdateOptions(entityType: SoeEntityType, batchUpdate: BatchUpdateDTO): ng.IPromise<BatchUpdateDTO> {
        let model = {
            entityType: entityType,
            batchUpdate: batchUpdate
        }

        return this.httpService.post(Constants.WEBAPI_CORE_BATCHUPDATE_REFRESHBATCHUPDATEOPTIONS, model).then(data => {
            return data;
        });
    }

    saveUserSelection(userSelection: UserSelectionDTO): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_MANAGE_USER_USER_SELECTION, userSelection);
    }

    saveIntrastatTransactions(transactions: any[], originId: number, originType: SoeOriginType): ng.IPromise<any> {
        const model = {
            transactions: transactions,
            originId: originId,
            originType: originType,
        };
        return this.httpService.post(Constants.WEBAPI_BILLING_ORDER_INTRASTAT_TRANSACTIONS, model);
    }

    createIntrastatExport(selections: any) {
        return this.httpService.post(Constants.WEBAPI_BILLING_ORDER_INTRASTAT_TRANSACTIONS_EXPORT, selections);
    }

    signatoryContractAuthenticate(authenticationResponse: AuthenticationResponseDTO): ng.IPromise<AuthenticationResultDTO> {
        return this.httpService.post(Constants.WEBAPI_CORE_SIGNATORYCONTRACT_AUTHENTICATE, authenticationResponse, false);
    }


    // DELETE

    deleteDataStorage(iRecordId: number, dataStorageType: SoeDataStorageRecordType): ng.IPromise<IActionResult> {
        return this.httpService.delete(Constants.WEBAPI_CORE_FILES_DATASTORAGE + iRecordId + "/" + dataStorageType);
    }

    deleteCompanyInformation(informationId: number): ng.IPromise<IActionResult> {
        return this.httpService.delete(Constants.WEBAPI_MANAGE_REGISTRY_COMPANY_INFORMATION + informationId);
    }

    deleteCompanyInformationNotificationSent(informationId: number): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_MANAGE_REGISTRY_COMPANY_INFORMATION_DELETE_NOTIFICATION_SENT + informationId, null);
    }

    deleteDocument(dataStorageId: number): ng.IPromise<IActionResult> {
        return this.httpService.delete(Constants.WEBAPI_CORE_DOCUMENT + dataStorageId);
    }

    deleteEmailTemplate(emailTemplateId: number) {
        return this.httpService.delete(Constants.WEBAPI_CORE_EMAILTEMPLATE + emailTemplateId);
    }

    deleteSysGridState(grid: string) {
        return this.httpService.delete(Constants.WEBAPI_CORE_SYS_GRID_STATE + grid);
    }

    deleteUserGridState(grid: string) {
        return this.httpService.delete(Constants.WEBAPI_CORE_USER_GRID_STATE + grid);
    }

    deleteUserGauge(userGaugeId: number): ng.IPromise<IActionResult> {
        return this.httpService.delete(Constants.WEBAPI_CORE_DASHBOARD_USER_GAUGE + userGaugeId);
    }

    deleteSystemInfoLogRow(rowId: number) {
        return this.httpService.delete(Constants.WEBAPI_CORE_DASHBOARD_WIDGET_SYSTEM_INFO_DELETE + rowId);
    }

    deleteTextBlock(textBlockId: number) {
        return this.httpService.delete(Constants.WEBAPI_CORE_TEXTBLOCK + textBlockId);
    }

    deleteExpenseRow(expenseRowId: number) {
        return this.httpService.delete(Constants.WEBAPI_EXPENSE_ROW + expenseRowId);
    }

    deleteSysPosition(sysPositionId: number): ng.IPromise<IActionResult> {
        return this.httpService.delete(Constants.WEBAPI_CORE_SYS_POSITION + sysPositionId);
    }

    deleteEventHistory(eventHistoryId: number): ng.IPromise<IActionResult> {
        return this.httpService.delete(Constants.WEBAPI_CORE_EVENT_HISTORY + eventHistoryId);
    }

    deleteEventHistories(type: TermGroup_EventHistoryType, batchId: number): ng.IPromise<IActionResult> {
        return this.httpService.delete(Constants.WEBAPI_CORE_EVENT_HISTORY_BATCH + type + "/" + batchId);
    }

    deleteExtraField(extraFieldId: number): ng.IPromise<IActionResult> {
        return this.httpService.delete(Constants.WEBAPI_CORE_EXTRA_FIELD + extraFieldId);
    }

    deleteUserSelection(userSelectionId: number): ng.IPromise<IActionResult> {
        return this.httpService.delete(Constants.WEBAPI_MANAGE_USER_USER_SELECTION + userSelectionId);
    }
}
