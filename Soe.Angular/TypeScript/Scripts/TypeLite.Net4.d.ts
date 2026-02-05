


import { DayOfWeek, SoeSysEntityState, SysCompanySettingType, SysCompDBType, SysEdiMessageHeadStatus } from "../Util/Enumerations";
import { SoeEntityState, SoeExportFormat, MatrixDataType, TermGroup_AccountDistributionPeriodType, TermGroup_AccountDistributionTriggerType, TermGroup_AccountDistributionCalculationType, TermGroup_AccountDistributionRegistrationType, SoeOriginStatus, SoeOriginType, AccountingRowType, TermGroup_AccountMandatoryLevel, TermGroup_TimeScheduleTemplateBlockShiftUserStatus, SoeTimePayrollScheduleTransactionType, TermGroup_TimeCodeRegistrationType, SoeTimeCodeType, SoeTimeTransactionType, SoeModule, TermGroup_AttestEntity, SoeEntityType, TermGroup_AttestWorkFlowType, TermGroup_AttestWorkFlowRowProcessType, TermGroup_AttestFlowRowState, TermGroup_BillingType, TermGroup_OrderType, SoeStatusIcon, TermGroup_InvoiceVatType, SoeCategoryType, TermGroup_ChecklistRowType, TermGroup_ChecklistHeadType, SoeCategoryRecordEntity, TermGroup_CurrencyIntervalType, TermGroup_CurrencySource, TermGroup_Languages, CompTermsRecordType, TermGroup_SysContactAddressType, ContactAddressItemType, TermGroup_SysContactAddressRowType, TermGroup_Sex, OrderInvoiceRegistrationType, SoeInvoiceRowType, TermGroup_IOType, TermGroup_IOImportHeadType, TermGroup_IOSource, TermGroup_IOStatus, DailyRecurrencePatternType, DailyRecurrencePatternWeekIndex, DailyRecurrenceRangeType, TermGroup_SysDayType, TermGroup_EDIInvoiceStatus, TermGroup_EdiMessageType, TermGroup_EDIOrderStatus, TermGroup_EDIStatus, TermGroup_EDISourceType, TermGroup_EmployeeDisbursementMethod, TermGroup_EmployeeFactorType, SoeEmployeePostStatus, TermGroup_EmployeePostWeekendType, TermGroup_EmployeeRequestResultStatus, TermGroup_EmployeeRequestStatus, TermGroup_EmployeeRequestType, TermGroup_EmployeeTaxAdjustmentType, TermGroup_EmployeeTaxEmploymentAbroadCode, TermGroup_EmployeeTaxEmploymentTaxType, TermGroup_EmployeeTaxSalaryDistressAmountType, TermGroup_EmployeeTaxSinkType, TermGroup_EmployeeTaxType, SoeEmployeeTimePeriodStatus, TermGroup_SysVehicleFuelType, TermGroup_VehicleType, TermGroup_EmploymentChangeFieldType, TermGroup_EmploymentChangeType, TermGroup_EmploymentType, SoeEmploymentFinalSalaryStatus, TermGroup_SoePayrollPriceType, TermGroup_VacationGroupCalculationType, TermGroup_VacationGroupVacationDaysHandleRule, TermGroup_VacationGroupVacationHandleRule, TermGroup_ShiftHistoryType, SoeScheduleWorkRules, SoeDataStorageRecordType, SoeInvoiceType, ImageFormatType, SoeEntityImageType, TermGroup_InventoryWriteOffMethodPeriodType, TermGroup_InventoryStatus, TermGroup_InvoiceProductCalculationType, TermGroup_InvoiceProductVatType, SoeSysPriceListProviderType, PriceListOrigin, SoePaymentStatus, XEMailAnswerType, TermGroup_MessageDeliveryType, TermGroup_MessagePriority, TermGroup_MessageTextType, TermGroup_MessageType, TermGroup_EmployeeRequestTypeFlags, XEMailRecipientType, TermGroup_TimeScheduleTemplateBlockShiftStatus, ImportPaymentType, TermGroup_SysPaymentType, ImportPaymentIOState, ImportPaymentIOStatus, TermGroup_PayrollType, TermGroup_PayrollReviewStatus, SoeProductType, ProjectCentralHeaderGroupType, ProjectCentralStatusRowType, ProjectCentralBudgetRowType, TermGroup_ProjectAllocationType, TermGroup_ProjectStatus, TermGroup_ProjectType, TermGroup_ProjectUserType, TermGroup_ReportExportFileType, TermGroup_ReportExportType, TermGroup_ReportGroupAndSortingTypes, SoeSelectionData, TermGroup_RetroactivePayrollAccountType, TermGroup_SoeRetroactivePayrollStatus, TermGroup_SoeRetroactivePayrollEmployeeStatus, TermGroup_ScanningMessageType, TermGroup_ScanningStatus, ScanningEntryRowType, TermGroup_TimeScheduleTemplateBlockType, TermGroup_EmployeeStatisticsType, SoeProgressInfoType, TermGroup_StaffingNeedsHeadStatus, StaffingNeedsHeadType, StaffingNeedsRowOriginType, StaffingNeedsRowType, SoeStaffingNeedsTaskType, TermGroup_VatDeductionType, PostChange, TermGroup_TimeCodeRoundingType, TermGroup_TimeCodeRuleType, TermGroup_TimeDeviationCauseType, TermGroup_AttestTreeGrouping, SoeAttestTreeMode, TermGroup_AttestTreeSorting, SoeTimeHalfdayType, TermGroup_TimePeriodType, SoeTimeScheduleTemplateBlockBreakType, SoeTimeScheduleDeviationCauseStatus, TermGroup_VacationGroupType, TermGroup_VacationGroupGuaranteeAmountMaxNbrOfDaysRule, TermGroup_VacationGroupRemainingDaysRule, TermGroup_VacationGroupVacationAbsenceCalculationRule, TermGroup_VacationGroupVacationSalaryPayoutRule, TermGroup_VacationGroupYearEndOverdueDaysRule, TermGroup_VacationGroupYearEndRemainingDaysRule, TermGroup_VacationGroupYearEndVacationVariableRule, TermGroup_AccountStatus, TermGroup_SoeRetroactivePayrollOutcomeErrorCode, TermGroup_PayrollResultType, WildCard, TermGroup_AttestRuleRowLeftValueType, TermGroup_AttestRuleRowRightValueType, TermGroup_TrackChangesAction, SettingDataType, TermGroup_TrackChangesColumnType, TermGroup_Country, TermGroup_TrackChangesActionMethod, DeleteEmployeeAction, UserReplacementType, DeleteUserAction, TermGroup_ContractGroupPeriod, TermGroup_ContractGroupPriceManagement, TimeTerminalType, TermGroup_FollowUpTypeType, TimeStampEntryType, SoeValidateDeviationChangeResultCode, TermGroup_ExpenseType, TermGroup_ReportPrintoutStatus, SoeReportTemplateType, CompanySettingType, CustomerAccountType, EmployeeGroupAccountType, EmploymentAccountType, PayrollGroupAccountType, ProductAccountType, ProjectAccountType, SoeTimeSalaryExportFormat, SoeTimeSalaryExportTarget, TermGroup_TimeReportType, SoeTimeBlockDateStatus, TermGroup_TimeBlockDateStampingStatus, SoeDataStorageOriginType, SoeTimeAttestFunctionOption, SoeTimeAccumulatorComparison, TermGroup_SysPayrollPriceAmountType, TermGroup_SysPayrollPriceType, TermGroup_SysPayrollPrice, TermGroup_PayrollProductCentRoundingLevel, TermGroup_PayrollProductCentRoundingType, TermGroup_PensionCompany, TermGroup_PayrollProductQuantityRoundingType, TermGroup_PayrollProductTaxCalculationType, TermGroup_PayrollProductTimeUnit, TermGroup_TimeAbsenceRuleType, TermGroup_TimeAbsenceRuleRowType, SoeFieldSettingType, TermGroup_TimeSalaryPaymentExportType, SoeTimeSalaryPaymentExportFormat, ReportUserSelectionType, DashboardStatisticsType, DashboardStatisticsRowType, TermGroup_PerformanceTestInterval, ScheduledJobRecurrenceType, ScheduledJobRetryType, ScheduledJobState, ScheduledJobType, SysJobSettingType, SoeTimeRuleDirection, SoeTimeRuleType, SupplierInvoiceOrderLinkType, SoeRecalculateTimeHeadAction, TermGroup_RecalculateTimeHeadStatus, TermGroup_RecalculateTimeRecordStatus, SoeTimeRuleComparisonOperator, SoeTimeRuleValueType, SoeTimeRuleOperatorType, TermGroup_TimeAccumulatorType, TermGroup_AccumulatorTimePeriodType, VoucherRowMergeType, TermGroup_TimeSchedulePlanningFollowUpCalculationType, TermGroup_MassRegistrationInputType, TermGroup_SysImportDefinitionType, TermGroup_VoucherHeadSourceType, SoeSelectionType, TermGroup_InformationSeverity, SoeInformationType, SoeInformationSourceType, TermGroup_InformationStickyType, InvoiceAttachmentSourceType, LiquidityPlanningTransactionType, TermGroup_TimeScheduleScenarioHeadSourceType, TimeRuleExportImportUnmatchedType, TermGroup_OrderEdiTransferMode, TermGroup_IncludeExpenseInReportType, SettingMainType, SoeProductRowType, TermGroup_EventHistoryType, TermGroup_AttestRoleUserAccountPermissionType, TermGroup_ScheduledJobLogLevel, TermGroup_ScheduledJobLogStatus, TermGroup_ReportPrintoutDeliveryType, TermGroup_PayrollImportHeadFileType, TermGroup_PayrollImportEmployeeScheduleStatus, TermGroup_PayrollImportEmployeeTransactionType, TermGroup_PayrollImportHeadStatus, TermGroup_PayrollImportEmployeeStatus, TermGroup_EmployeeSelectionAccountingType, TermGroup_ReportUserSelectionAccessType, TermGroup_DataStorageRecordAttestStatus, MatrixFieldSetting, TermGroup_InsightChartTypes, AnalysisMode, TermGroup_MatrixGroupAggOption, TermGroup_MatrixDateFormatOption, TermGroup_SysPayrollType, TermGroup_EmployeeTemplateGroupRowType, SupplierInvoiceRowType, TimeStampAdditionType, TermGroup_GrossMarginCalculationType, SoeTimeScheduleEmployeePeriodDetailType, TermGroup_ExtraFieldType, TermGroup_ExtraFieldValueType, LeisureCodeAllocationEmployeeStatus, TermGroup_TimeScheduleTemplateBlockAbsenceType, TermGroup } from "../Util/CommonEnumerations";
import { SmallGenericType } from "../Common/Models/SmallGenericType";




export interface IAccountDimDTO {
	accountDimId: number;
	accountDimNr: number;
	accounts: IAccountDTO[];
	actorCompanyId: number;
	created: Date;
	createdBy: string;
	excludeinAccountingExport: boolean;
	excludeinSalaryReport: boolean;
	isInternal: boolean;
	isStandard: boolean;
	level: number;
	linkedToProject: boolean;
	linkedToShiftType: boolean;
	mandatoryInCustomerInvoice: boolean;
	mandatoryInOrder: boolean;
	maxChar: number;
	minChar: number;
	modified: Date;
	modifiedBy: string;
	name: string;
	parentAccountDimId: number;
	parentAccountDimName: string;
	shortName: string;
	state: SoeEntityState;
	sysAccountStdTypeParentId: number;
	sysSieDimNr: number;
	useInSchedulePlanning: boolean;
	useVatDeduction: boolean;
}
export interface IAccountDimensionsDTO {
	dim1Disabled: boolean;
	dim1Id: number;
	dim1Mandatory: boolean;
	dim1ManuallyChanged: boolean;
	dim1Name: string;
	dim1Nr: string;
	dim1Stop: boolean;
	dim2Disabled: boolean;
	dim2Id: number;
	dim2Mandatory: boolean;
	dim2ManuallyChanged: boolean;
	dim2Name: string;
	dim2Nr: string;
	dim2Stop: boolean;
	dim3Disabled: boolean;
	dim3Id: number;
	dim3Mandatory: boolean;
	dim3ManuallyChanged: boolean;
	dim3Name: string;
	dim3Nr: string;
	dim3Stop: boolean;
	dim4Disabled: boolean;
	dim4Id: number;
	dim4Mandatory: boolean;
	dim4ManuallyChanged: boolean;
	dim4Name: string;
	dim4Nr: string;
	dim4Stop: boolean;
	dim5Disabled: boolean;
	dim5Id: number;
	dim5Mandatory: boolean;
	dim5ManuallyChanged: boolean;
	dim5Name: string;
	dim5Nr: string;
	dim5Stop: boolean;
	dim6Disabled: boolean;
	dim6Id: number;
	dim6Mandatory: boolean;
	dim6ManuallyChanged: boolean;
	dim6Name: string;
	dim6Nr: string;
	dim6Stop: boolean;
}
export interface IAccountDimSmallDTO {
	accountDimId: number;
	accountDimNr: number;
	accounts: IAccountDTO[];
	level: number;
	linkedToShiftType: boolean;
	mandatoryInCustomerInvoice: boolean;
	mandatoryInOrder: boolean;
	name: string;
	parentAccountDimId: number;
}
export interface IAccountDistributionEntryDTO {
	accountDistributionEntryId: number;
	accountDistributionEntryRowDTO: IAccountDistributionEntryRowDTO[];
	accountDistributionHeadId: number;
	accountDistributionHeadName: string;
	accountYearId: number;
	actorCompanyId: number;
	amount: number;
	categories: string;
	created: Date;
	createdBy: string;
	currentAmount: number;
	date: Date;
	detailVisible: boolean;
	inventoryId: number;
	inventoryName: string;
	inventoryNr: string;
	invoiceNr: string;
	isSelected: boolean;
	isSelectEnable: boolean;
	modified: Date;
	modifiedBy: string;
	periodError: boolean;
	periodType: TermGroup_AccountDistributionPeriodType;
	registrationType: TermGroup_AccountDistributionRegistrationType;
	rowId: number;
	sourceCustomerInvoiceId: number;
	sourceCustomerInvoiceSeqNr: number;
	sourceRowId: number;
	sourceSupplierInvoiceId: number;
	sourceSupplierInvoiceSeqNr: number;
	sourceVoucherHeadId: number;
	sourceVoucherNr: number;
	state: number;
	status: string;
	supplierInvoiceId: number;
	triggerType: TermGroup_AccountDistributionTriggerType;
	typeName: string;
	voucherHeadId: number;
	voucherNr: number;
	voucherSeriesTypeId: number;
	writeOffAmount: number;
	writeOffTotal: number;
	writeOffYear: number;
}
export interface IAccountDistributionEntryRowDTO {
	accountDistributionEntryId: number;
	accountDistributionEntryRowId: number;
	creditAmount: number;
	creditAmountCurrency: number;
	creditAmountEntCurrency: number;
	creditAmountLedgerCurrency: number;
	debitAmount: number;
	debitAmountCurrency: number;
	debitAmountEntCurrency: number;
	debitAmountLedgerCurrency: number;
	dim1DimName: string;
	dim1Id: number;
	dim1Name: string;
	dim1Nr: string;
	dim2DimName: string;
	dim2Id: number;
	dim2Name: string;
	dim2Nr: string;
	dim3DimName: string;
	dim3Id: number;
	dim3Name: string;
	dim3Nr: string;
	dim4DimName: string;
	dim4Id: number;
	dim4Name: string;
	dim4Nr: string;
	dim5DimName: string;
	dim5Id: number;
	dim5Name: string;
	dim5Nr: string;
	dim6DimName: string;
	dim6Id: number;
	dim6Name: string;
	dim6Nr: string;
	oppositeBalance: number;
	sameBalance: number;
}
export interface IAccountDistributionHeadDTO {
	accountDistributionHeadId: number;
	actorCompanyId: number;
	amount: number;
	amountOperator: number;
	calculate: number;
	calculationType: TermGroup_AccountDistributionCalculationType;
	created: Date;
	createdBy: string;
	dayNumber: number;
	description: string;
	dim1Expression: string;
	dim1Id: number;
	dim2Expression: string;
	dim2Id: number;
	dim3Expression: string;
	dim3Id: number;
	dim4Expression: string;
	dim4Id: number;
	dim5Expression: string;
	dim5Id: number;
	dim6Expression: string;
	dim6Id: number;
	endDate: Date;
	keepRow: boolean;
	modified: Date;
	modifiedBy: string;
	name: string;
	periodType: TermGroup_AccountDistributionPeriodType;
	periodValue: number;
	rows: IAccountDistributionRowDTO[];
	sort: number;
	startDate: Date;
	state: SoeEntityState;
	triggerType: TermGroup_AccountDistributionTriggerType;
	type: number;
	useInCustomerInvoice: boolean;
	useInImport: boolean;
	useInPayrollVacationVoucher: boolean;
	useInPayrollVoucher: boolean;
	useInSupplierInvoice: boolean;
	useInVoucher: boolean;
	voucherSeriesTypeId: number;
}
export interface IAccountDistributionRowDTO {
	accountDistributionHeadId: number;
	accountDistributionRowId: number;
	calculateRowNbr: number;
	description: string;
	dim1Disabled: boolean;
	dim1Id: number;
	dim1Mandatory: boolean;
	dim1Name: string;
	dim1Nr: string;
	dim2Disabled: boolean;
	dim2Id: number;
	dim2KeepSourceRowAccount: boolean;
	dim2Mandatory: boolean;
	dim2Name: string;
	dim2Nr: string;
	dim3Disabled: boolean;
	dim3Id: number;
	dim3KeepSourceRowAccount: boolean;
	dim3Mandatory: boolean;
	dim3Name: string;
	dim3Nr: string;
	dim4Disabled: boolean;
	dim4Id: number;
	dim4KeepSourceRowAccount: boolean;
	dim4Mandatory: boolean;
	dim4Name: string;
	dim4Nr: string;
	dim5Disabled: boolean;
	dim5Id: number;
	dim5KeepSourceRowAccount: boolean;
	dim5Mandatory: boolean;
	dim5Name: string;
	dim5Nr: string;
	dim6Disabled: boolean;
	dim6Id: number;
	dim6KeepSourceRowAccount: boolean;
	dim6Mandatory: boolean;
	dim6Name: string;
	dim6Nr: string;
	oppositeBalance: number;
	previousRowNbr: number;
	rowNbr: number;
	sameBalance: number;
	state: SoeEntityState;
}
export interface IAccountDistributionTraceViewDTO {
	accountDistributionHeadId: number;
	date: Date;
	description: string;
	invoiceId: number;
	isInvoice: boolean;
	isVoucher: boolean;
	langId: number;
	number: string;
	originStatus: SoeOriginStatus;
	originStatusName: string;
	originType: SoeOriginType;
	originTypeName: string;
	state: SoeEntityState;
	voucherHeadId: number;
}
export interface IAccountDTO {
	accountDim: IAccountDimDTO;
	accountDimId: number;
	accountDimNr: number;
	accountId: number;
	accountInternals: IAccountInternalDTO[];
	accountNr: string;
	accountTypeSysTermId: number;
	amountStop: number;
	attestWorkFlowHeadId: number;
	description: string;
	externalCode: string;
	grossProfitCode: number[];
	hasVirtualParent: boolean;
	hierarchyOnly: boolean;
	isAbstract: boolean;
	isAccrualAccount: boolean;
	name: string;
	numberName: string;
	parentAccountId: number;
	rowTextStop: boolean;
	state: SoeEntityState;
	unit: string;
	unitStop: boolean;
	virtualParentAccountId: number;
}
export interface IAccountEditDTO {
	accountDimId: number;
	accountHierachyPayrollExportExternalCode: string;
	accountHierachyPayrollExportUnitExternalCode: string;
	accountId: number;
	accountInternals: IAccountInternalDTO[];
	accountMappings: IAccountMappingDTO[];
	accountNr: string;
	accountTypeSysTermId: number;
	active: boolean;
	amountStop: number;
	attestWorkFlowHeadId: number;
	created: Date;
	createdBy: string;
	description: string;
	excludeVatVerification: boolean;
	externalCode: string;
	hierarchyOnly: boolean;
	isAccrualAccount: boolean;
	isStdAccount: boolean;
	modified: Date;
	modifiedBy: string;
	name: string;
	parentAccountId: number;
	rowTextStop: boolean;
	sieKpTyp: string;
	state: SoeEntityState;
	sysAccountSruCode1Id: number;
	sysAccountSruCode2Id: number;
	sysVatAccountId: number;
	unit: string;
	unitStop: boolean;
	useVatDeduction: boolean;
	useVatDeductionDim: boolean;
	vatDeduction: number;
}

export interface IAccountFilterSelectionDTO extends IReportDataSelectionDTO {
	from: string;
	id: number;
	to: string;
}
export interface IAccountFilterSelectionsDTO extends IReportDataSelectionDTO {
	filters: IAccountFilterSelectionDTO[];
}
export interface IAccountingPrioDTO {
	accountId: number;
	accountInternals: IAccountInternalDTO[];
	accountName: string;
	accountNr: string;
	companyType: CompanySettingType;
	customerType: CustomerAccountType;
	employeeGroupType: EmployeeGroupAccountType;
	employmentType: EmploymentAccountType;
	payrollGroupType: PayrollGroupAccountType;
	percent: number;
	productType: ProductAccountType;
	projectType: ProjectAccountType;
}
export interface IAccountingRowDTO extends IAccountDimensionsDTO {
	accountDistributionHeadId: number;
	accountDistributionNbrOfPeriods: number;
	accountDistributionStartDate: Date;
	amount: number;
	amountCurrency: number;
	amountEntCurrency: number;
	amountLedgerCurrency: number;
	amountStop: number;
	attestStatus: number;
	attestUserId: number;
	attestUserName: string;
	balance: number;
	creditAmount: number;
	creditAmountCurrency: number;
	creditAmountEntCurrency: number;
	creditAmountLedgerCurrency: number;
	date: Date;
	debitAmount: number;
	debitAmountCurrency: number;
	debitAmountEntCurrency: number;
	debitAmountLedgerCurrency: number;
	inventoryId: number;
	invoiceAccountRowId: number;
	invoiceId: number;
	invoiceRowId: number;
	isCentRoundingRow: boolean;
	isClaimRow: boolean;
	isContractorVatRow: boolean;
	isCreditRow: boolean;
	isDebitRow: boolean;
	isDeleted: boolean;
	isHouseholdRow: boolean;
	isInterimRow: boolean;
	isManuallyAdjusted: boolean;
	isModified: boolean;
	isProcessed: boolean;
	isTemplateRow: boolean;
	isVatRow: boolean;
	mergeSign: number;
	parentRowId: number;
	productName: string;
	productRowNr: number;
	quantity: number;
	quantityStop: boolean;
	rowNr: number;
	rowTextStop: boolean;
	splitPercent: number;
	splitType: number;
	splitValue: number;
	state: SoeEntityState;
	tempInvoiceRowId: number;
	tempRowId: number;
	text: string;
	type: AccountingRowType;
	unit: string;
	voucherRowId: number;
	voucherRowMergeType: VoucherRowMergeType;
    startDate?: Date;
    numberOfPeriods?: number;
}
export interface IAccountingSettingDTO {
	account10Id: number;
	account10Name: string;
	account10Nr: string;
	account1Id: number;
	account1Name: string;
	account1Nr: string;
	account2Id: number;
	account2Name: string;
	account2Nr: string;
	account3Id: number;
	account3Name: string;
	account3Nr: string;
	account4Id: number;
	account4Name: string;
	account4Nr: string;
	account5Id: number;
	account5Name: string;
	account5Nr: string;
	account6Id: number;
	account6Name: string;
	account6Nr: string;
	account7Id: number;
	account7Name: string;
	account7Nr: string;
	account8Id: number;
	account8Name: string;
	account8Nr: string;
	account9Id: number;
	account9Name: string;
	account9Nr: string;
	dimName: string;
	dimNr: number;
	percent1: number;
	percent10: number;
	percent2: number;
	percent3: number;
	percent4: number;
	percent5: number;
	percent6: number;
	percent7: number;
	percent8: number;
	percent9: number;
	type: number;
}
export interface IAccountingSettingsRowDTO {
	account1Id: number;
	account1Name: string;
	account1Nr: string;
	account2Id: number;
	account2Name: string;
	account2Nr: string;
	account3Id: number;
	account3Name: string;
	account3Nr: string;
	account4Id: number;
	account4Name: string;
	account4Nr: string;
	account5Id: number;
	account5Name: string;
	account5Nr: string;
	account6Id: number;
	account6Name: string;
	account6Nr: string;
	accountDim1Nr: number;
	accountDim2Nr: number;
	accountDim3Nr: number;
	accountDim4Nr: number;
	accountDim5Nr: number;
	accountDim6Nr: number;
	percent: number;
	type: number;
}
export interface IAccountInternalDTO {
	account: IAccountDTO;
	accountDimId: number;
	accountDimNr: number;
	accountId: number;
	accountNr: string;
	mandatoryLevel: number;
	name: string;
	sysSieDimNr: number;
	sysSieDimNrOrAccountDimNr: number;
	useVatDeduction: boolean;
	vatDeduction: number;
}
export interface IAccountIntervalSelectionDTO extends IReportDataSelectionDTO {
	value: number;
	yearId: number;
}
export interface IAccountMappingDTO {
	accountDimId: number;
	accountDimName: string;
	accountId: number;
	accounts: IAccountDTO[];
	defaultAccountId: number;
	mandatoryLevel: TermGroup_AccountMandatoryLevel;
	mandatoryLevels: IGenericType[];
}
export interface IAccountPeriodDTO {
	accountPeriodId: number;
	accountYearId: number;
	created: Date;
	createdBy: string;
	from: Date;
	hasExistingVouchers: boolean;
	isDeleted: boolean;
	modified: Date;
	modifiedBy: string;
	periodNr: number;
	startValue: string;
	status: TermGroup_AccountStatus;
	to: Date;
}
export interface IAccountsDrilldownDTO {
	accountName: string;
	accountNr: string;
	budgetPeriodAmount: number;
	budgetToPeriodEndAmount: number;
	openingBalance: number;
	periodAmount: number;
	periodBudgetDiff: number;
	periodPrevPeriodDiff: number;
	prevPeriodAmount: number;
	prevYearAmount: number;
	yearAmount: number;
	yearBudgetDiff: number;
	yearPrevYearDiff: number;
}
export interface IAccountSmallDTO {
	accountDimId: number;
	accountId: number;
	description: string;
	name: string;
	number: string;
	parentAccountId: number;
	percent: number;
}
export interface IAccountVatRateViewSmallDTO {
	accountId: number;
	accountNr: string;
	name: string;
	vatRate: number;
}
export interface IAccountYearBalanceFlatDTO {
	accountYearBalanceHeadId: number;
	accountYearId: number;
	balance: number;
	balanceEntCurrency: number;
	created: Date;
	createdBy: string;
	creditAmount: number;
	debitAmount: number;
	dim1Id: number;
	dim1Name: string;
	dim1Nr: string;
	dim1TypeName: string;
	dim2Id: number;
	dim2Name: string;
	dim2Nr: string;
	dim3Id: number;
	dim3Name: string;
	dim3Nr: string;
	dim4Id: number;
	dim4Name: string;
	dim4Nr: string;
	dim5Id: number;
	dim5Name: string;
	dim5Nr: string;
	dim6Id: number;
	dim6Name: string;
	dim6Nr: string;
	isDeleted: boolean;
	isDiffRow: boolean;
	isModified: boolean;
	modified: Date;
	modifiedBy: string;
	quantity: number;
	rowNr: number;
}
export interface IAccountYearDTO {
	accountYearId: number;
	actorCompanyId: number;
	created: Date;
	createdBy: string;
	from: Date;
	modified: Date;
	modifiedBy: string;
	noOfPeriods: number;
	periods: IAccountPeriodDTO[];
	status: TermGroup_AccountStatus;
	statusText: string;
	to: Date;
	yearFromTo: string;
}
export interface IActionResult {
	booleanValue: boolean;
	booleanValue2: boolean;
	canUserOverride: boolean;
	dates: Date[];
	dateTimeValue: Date;
	decimalValue: number;
	errorMessage: string;
	errorNumber: number;
	infoMessage: string;
	integerValue: number;
	integerValue2: number;
	keys: number[];
	modified: Date;
	modifiedBy: string;
	objectsAffected: number;
	stackTrace: string;
	strings: string[];
	stringValue: string;
	success: boolean;
	successNumber: number;
	value: any;
	value2: any;
}
export interface ICustomerInvoiceDistributionResultDTO {
	permissionCheck: ActionResult
	eInvoiceResult: ActionResult
	emailResult: ActionResult
	printResult: ActionResult
	unhandledCount: number
	emailedCount: number
	eInvoicedCount: number
	printedCount: number
}
export interface IActivateScenarioDTO {
	key: string;
	preliminaryDateFrom: Date;
	rows: IActivateScenarioRowDTO[];
	sendMessage: boolean;
	timeScheduleScenarioHeadId: number;
}
export interface IActivateScenarioRowDTO {
	date: Date;
	employeeId: number;
	key: string;
}
export interface IActivateScheduleControlDTO {
	discardCheckesAll: boolean;
	discardCheckesForAbsence: boolean;
	discardCheckesForManuallyAdjusted: boolean;
	hasWarnings: boolean;
	heads: IActivateScheduleControlHeadDTO[];
	key: string;
	resultHeads: IActivateScheduleControlHeadResultDTO[];
}
export interface IActivateScheduleControlHeadDTO {
	comment: string;
	employeeId: number;
	employeeNrAndName: string;
	employeeRequestId: number;
	reActivateAbsenceRequest: boolean;
	resultStatusName: string;
	rows: IActivateScheduleControlRowDTO[];
	startDate: Date;
	statusName: string;
	stopDate: Date;
	timeDeviationCauseId: number;
	timeDeviationCauseName: string;
	type: TermGroup_ControlEmployeeSchedulePlacementType;
	typeName: string;
}
export interface IActivateScheduleControlHeadResultDTO {
	employeeId: number;
	employeeRequestId: number;
	reActivateAbsenceRequest: boolean;
	type: TermGroup_ControlEmployeeSchedulePlacementType;
}
export interface IActivateScheduleControlRowDTO {
	date: Date;
	isWholeDayAbsence: boolean;
	scheduleStart: Date;
	scheduleStop: Date;
	start: Date;
	stop: Date;
	timeScheduleTemplateBlockId: number;
	timeScheduleTemplateBlockType: number;
	type: TermGroup_ControlEmployeeSchedulePlacementType;
}
export interface IActivateScheduleGridDTO {
	accountNamesString: string;
	categoryNamesString: string;
	employeeGroupId: number;
	employeeGroupName: string;
	employeeId: number;
	employeeName: string;
	employeeNr: string;
	employeeScheduleId: number;
	employeeScheduleStartDate: Date;
	employeeScheduleStartDayNumber: number;
	employeeScheduleStopDate: Date;
	employmentEndDate: Date;
	employeeHidden: boolean;
	isPersonalTemplate: boolean;
	isPlaced: boolean;
	isPreliminary: boolean;
	simpleSchedule: boolean;
	templateEmployeeId: number;
	templateStartDate: Date;
	timeScheduleTemplateHeadId: number;
	timeScheduleTemplateHeadName: string;
}
export interface IActorSearchPersonDTO {
	consentDate: Date;
	entityType: SoeEntityType;
	entityTypeName: string;
	hasConsent: boolean;
	hasConsentString: string;
	isPrivatePerson: boolean;
	name: string;
	number: string;
	orgNr: string;
	recordId: number;
}
export interface IAgeDistributionDTO {
	actorId: number;
	actorName: string;
	actorNr: string;
	actorNrSort: string;
	amount1: number;
	amount2: number;
	amount3: number;
	amount4: number;
	amount5: number;
	amount6: number;
	expiryDate: Date;
	invoiceDate: Date;
	invoiceId: number;
	invoiceNr: string;
	registrationType: number;
	seqNr: number;
	sumAmount: number;
}
export interface IAnnualLeaveBalance {
	employeeId: number;
	annualLeaveBalanceDays: number;
	annualLeaveBalanceMinutes: number;
}
export interface IAnnualScheduledTimeSummary {
	annualScheduledTimeMinutes: number;
	annualWorkTimeMinutes: number;
	employeeId: number;
}
export interface IApiMessageChangeGridDTO {
	error: string;
	fieldTypeName: string;
	fromDate: Date;
	fromValue: string;
	hasError: boolean;
	identifier: string;
	recordName: string;
	toDate: Date;
	toValue: string;
	typeName: string;
}
export interface IApiMessageGridDTO {
	apiMessageId: number;
	changes: IApiMessageChangeGridDTO[];
	comment: string;
	created: Date;
	hasError: boolean;
	hasFile: boolean;
	identifiers: string;
	modified: Date;
	recordCount: number;
	sourceTypeName: string;
	statusName: string;
	typeName: string;
	validationMessage: string;
}
export interface IApiSettingDTO {
	apiSettingId: number;
	booleanValue: boolean;
	created: Date;
	createdBy: string;
	dataType: SettingDataType;
	description: string;
	integerValue: number;
	isModified: boolean;
	modified: Date;
	modifiedBy: string;
	name: string;
	startDate: Date;
	state: SoeEntityState;
	stopDate: Date;
	stringValue: string;
	type: TermGroup_ApiSettingType;
}
export interface IApplyAbsenceDTO {
	date: Date;
	isVacation: boolean;
	newProductId: number;
	sysPayrollTypeLevel3: number;
	timeDeviationCauseId: number;
	timePayrollTransactionIdsToRecalculate: number[];
}
export interface IAttestEmployeeAdditionDeductionDTO {
	accounting: string;
	amount: number;
	attestStateColor: string;
	attestStateId: number;
	attestStateName: string;
	comment: string;
	customerInvoiceId: number;
	customerInvoiceRowId: number;
	customerName: string;
	customerNumber: string;
	expenseRowId: number;
	hasFiles: boolean;
	invoiceNumber: string;
	isAttested: boolean;
	isReadOnly: boolean;
	isSpecifiedUnitPrice: boolean;
	priceListInclVat: boolean;
	projectId: number;
	projectName: string;
	projectNumber: string;
	quantity: number;
	quantityText: string;
	registrationType: TermGroup_TimeCodeRegistrationType;
	start: Date;
	state: SoeEntityState;
	stop: Date;
	timeCodeComment: string;
	timeCodeCommentMandatory: boolean;
	timeCodeExpenseType: TermGroup_ExpenseType;
	timeCodeId: number;
	timeCodeName: string;
	timeCodeStopAtAccounting: boolean;
	timeCodeStopAtComment: boolean;
	timeCodeStopAtDateStart: boolean;
	timeCodeStopAtDateStop: boolean;
	timeCodeStopAtPrice: boolean;
	timeCodeStopAtVat: boolean;
	timeCodeTransactionId: number;
	timeCodeType: SoeTimeCodeType;
	timePeriodId: number;
	transactions: IAttestEmployeeAdditionDeductionTransactionDTO[];
	unitPrice: number;
	vatAmount: number;
}
export interface IAttestEmployeeAdditionDeductionTransactionDTO {
	accountingString: string;
	amount: number;
	attestStateColor: string;
	attestStateId: number;
	attestStateName: string;
	comment: string;
	date: Date;
	isAttested: boolean;
	isReadOnly: boolean;
	productId: number;
	productName: string;
	quantity: number;
	quantityText: string;
	transactionId: number;
	transactionType: SoeTimeTransactionType;
	unitPrice: number;
	vatAmount: number;
}
export interface IAttestEmployeeBreakDTO {
	isModified: boolean;
	minutes: number;
	startTime: Date;
	stopTime: Date;
	timeBlockDateId: number;
	timeBlockId: number;
}
export interface IAttestEmployeeDayDTO {
	absenceTime: System.ITimeSpan;
	alwaysDiscardBreakEvaluation: boolean;
	attestPayrollTransactions: IAttestPayrollTransactionDTO[];
	attestStateColor: string;
	attestStateId: number;
	attestStateName: string;
	attestStates: IAttestStateDTO[];
	attestStateSort: number;
	autogenTimeblocks: boolean;
	containsDuplicateTimeBlocks: boolean;
	date: Date;
	day: number;
	dayName: string;
	dayNumber: number;
	dayOfWeekNr: number;
	dayTypeId: number;
	dayTypeName: string;
	discardedBreakEvaluation: boolean;
	earningTimeAccumulatorId?: number;
	employeeGroupId: number;
	employeeId: number;
	employeeScheduleId: number;
	hasAbsenceTime: boolean;
	hasComment: boolean;
	hasDeviations: boolean;
	hasExpense: boolean;
	hasInformations: boolean;
	hasInvalidTimeStamps: boolean;
	hasNoneInitialTransactions: boolean;
	hasOvertime: boolean;
	hasPayrollImportEmployeeTransactions: boolean;
	hasPayrollImportWarnings: boolean;
	hasPayrollScheduleTransactions: boolean;
	hasPayrollTransactions: boolean;
	hasPeriodDiscardedBreakEvaluation: boolean;
	hasPeriodOvertime: boolean;
	hasPeriodTimeScheduleTypeFactorMinutes: boolean;
	hasPeriodTimeWorkReduction: boolean;
	hasSameAttestState: boolean;
	hasScheduledPlacement: boolean;
	hasScheduleWithoutTransactions: boolean;
	hasShiftSwaps: boolean;
	hasStandbyTime: boolean;
	hasTimeStampEntries: boolean;
	hasTimeStampsWithoutTransactions: boolean;
	hasTransactions: boolean;
	hasTimeWorkReduction: boolean;
	hasUnhandledExtraShiftChanges: boolean;
	hasUnhandledShiftChanges: boolean;
	hasWarnings: boolean;
	hasWorkedInsideSchedule: boolean;
	hasWorkedOutsideSchedule: boolean;
	holidayAndDayTypeName: string;
	holidayId: number;
	holidayName: string;
	isAbsenceDay: boolean;
	isAttested: boolean;
	isCompletelyAdditional: boolean;
	isGeneratingTransactions: boolean;
	isHoliday: boolean;
	informations: SoeTimeAttestInformation[];
	isNotScheduleTime: boolean;
	isPartlyAdditional: boolean;
	isPrel: boolean;
	isPreliminary: string;
	isReadonly: boolean;
	isScheduleChangedFromTemplate: boolean;
	isScheduleZeroDay: boolean;
	isTemplateScheduleZeroDay: boolean;
	isWholedayAbsence: boolean;
	manuallyAdjusted: boolean;
	noOfScheduleBreaks: number;
	occupiedTime: System.ITimeSpan;
	payrollAddedTimeMinutes: number;
	payrollImportEmployeeTransactions: IPayrollImportEmployeeTransactionDTO[];
	payrollInconvinientWorkingHoursMinutes: number;
	payrollInconvinientWorkingHoursScaledMinutes: number;
	payrollOverTimeMinutes: number;
	payrollWorkMinutes: number;
	presenceBreakItems: IAttestEmployeeBreakDTO[];
	presenceBreakMinutes: number;
	presenceBreakTime: System.ITimeSpan;
	presenceInsideScheduleTime: System.ITimeSpan;
	presenceOutsideScheduleTime: System.ITimeSpan;
	presencePayedTime: System.ITimeSpan;
	presenceStartTime: Date;
	presenceStopTime: Date;
	presenceTime: System.ITimeSpan;
	projectTimeBlocks: IProjectTimeBlockDTO[];
	recalculateTimeRecordId: number;
	recalculateTimeRecordStatus: TermGroup_RecalculateTimeRecordStatus;
	scheduleBreak1Minutes: number;
	scheduleBreak1Start: Date;
	scheduleBreak2Minutes: number;
	scheduleBreak2Start: Date;
	scheduleBreak3Minutes: number;
	scheduleBreak3Start: Date;
	scheduleBreak4Minutes: number;
	scheduleBreak4Start: Date;
	scheduleBreakMinutes: number;
	scheduleBreakTime: System.ITimeSpan;
	scheduleStartTime: Date;
	scheduleStopTime: Date;
	scheduleTime: System.ITimeSpan;
	shiftUserStatuses: TermGroup_TimeScheduleTemplateBlockShiftUserStatus[];
	standbyTime: System.ITimeSpan;
	sumExpenseAmount: number;
	sumExpenseRows: number;
	sumGrossSalaryAbsence: System.ITimeSpan;
	sumGrossSalaryAbsenceLeaveOfAbsence: System.ITimeSpan;
	sumGrossSalaryAbsenceParentalLeave: System.ITimeSpan;
	sumGrossSalaryAbsenceSick: System.ITimeSpan;
	sumGrossSalaryAbsenceTemporaryParentalLeave: System.ITimeSpan;
	sumGrossSalaryAbsenceText: string;
	sumGrossSalaryAbsenceVacation: System.ITimeSpan;
	sumGrossSalaryAdditionalTime: System.ITimeSpan;
	sumGrossSalaryDuty: System.ITimeSpan;
	sumGrossSalaryOBAddition: System.ITimeSpan;
	sumGrossSalaryOBAddition100: System.ITimeSpan;
	sumGrossSalaryOBAddition113: System.ITimeSpan;
	sumGrossSalaryOBAddition40: System.ITimeSpan;
	sumGrossSalaryOBAddition50: System.ITimeSpan;
	sumGrossSalaryOBAddition57: System.ITimeSpan;
	sumGrossSalaryOBAddition70: System.ITimeSpan;
	sumGrossSalaryOBAddition79: System.ITimeSpan;
	sumGrossSalaryOvertime: System.ITimeSpan;
	sumGrossSalaryOvertime100: System.ITimeSpan;
	sumGrossSalaryOvertime50: System.ITimeSpan;
	sumGrossSalaryOvertime70: System.ITimeSpan;
	sumGrossSalaryWeekendSalary: System.ITimeSpan;
	sumInvoicedTime: System.ITimeSpan;
	sumTimeAccumulator: System.ITimeSpan;
	sumTimeAccumulatorOverTime: System.ITimeSpan;
	sumTimeWorkedScheduledTime: System.ITimeSpan;
	templateScheduleBreak1Minutes: number;
	templateScheduleBreak1Start: Date;
	templateScheduleBreak2Minutes: number;
	templateScheduleBreak2Start: Date;
	templateScheduleBreak3Minutes: number;
	templateScheduleBreak3Start: Date;
	templateScheduleBreak4Minutes: number;
	templateScheduleBreak4Start: Date;
	templateScheduleBreakMinutes: number;
	templateScheduleBreakTime: System.ITimeSpan;
	templateScheduleStartTime: Date;
	templateScheduleStopTime: Date;
	templateScheduleTime: System.ITimeSpan;
	timeBlockDateId: number;
	timeBlockDateStampingStatus: number;
	timeBlockDateStatus: number;
	timeBlocks: IAttestEmployeeDayTimeBlockDTO[];
	timeCodeTransactions: IAttestEmployeeDayTimeCodeTransactionDTO[];
	timeDeviationCauseNames: string;
	timeInvoiceTransactions: IAttestEmployeeDayTimeInvoiceTransactionDTO[];
	timeReportType: number;
	timeScheduleTemplateHeadId: number;
	timeScheduleTemplateHeadName: string;
	timeScheduleTemplatePeriodId: number;
	timeScheduleTypeFactorMinutes: number;
	timeStampEntrys: IAttestEmployeeDayTimeStampDTO[];
	unhandledEmployee: ITimeUnhandledShiftChangesEmployeeDTO;
	uniqueId: string;
	warnings: SoeTimeAttestWarning[];
	weekInfo: string;
	weekNr: number;
	weekNrMonday: string;
	wholedayAbsenseTimeDeviationCauseFromTimeBlock: number;
}
export interface IAttestEmployeeDayShiftDTO {
	accountId: number;
	break1Id: number;
	break1Minutes: number;
	break1StartTime: Date;
	break1TimeCode: string;
	break1TimeCodeId: number;
	break2Id: number;
	break2Minutes: number;
	break2StartTime: Date;
	break2TimeCode: string;
	break2TimeCodeId: number;
	break3Id: number;
	break3Minutes: number;
	break3StartTime: Date;
	break3TimeCode: string;
	break3TimeCodeId: number;
	break4Id: number;
	break4Minutes: number;
	break4StartTime: Date;
	break4TimeCode: string;
	break4TimeCodeId: number;
	description: string;
	employeeId: number;
	link: string;
	shiftTypeColor: string;
	shiftTypeDescription: string;
	shiftTypeId: number;
	shiftTypeName: string;
	startTime: Date;
	stopTime: Date;
	timeDeviationCauseId: number;
	timeDeviationCauseName: string;
	timeScheduleTemplateBlockId: number;
	timeScheduleTypeId: number;
	timeScheduleTypeName: string;
	type: TermGroup_TimeScheduleTemplateBlockType;
}
export interface IAttestEmployeeDaySmallDTO {
	date: Date;
	employeeId: number;
	timeBlockDateId: number;
	timeScheduleTemplatePeriodId: number;
}
export interface IAttestEmployeeDayTimeBlockDTO {
	accountId: number;
	comment: string;
	deviationAccounts: AccountInternalDTO[];
	employeeChildId: number;
	employeeId: number;
	guidId: string;
	isAbsence: boolean;
	isBreak: boolean;
	isGeneratedFromBreak: boolean;
	isOutsideScheduleNotOvertime: boolean;
	isOvertime: boolean;
	isPreliminary: boolean;
	isPresence: boolean;
	isReadonlyLeft: boolean;
	isReadonlyRight: boolean;
	manuallyAdjusted: boolean;
	shiftTypeId: number;
	startTime: Date;
	stopTime: Date;
	timeBlockDateId: number;
	timeBlockId: number;
	timeCodes: ITimeCodeDTO[];
	timeDeviationCauseName: string;
	timeDeviationCauseStartId: number;
	timeDeviationCauseStopId: number;
	timeScheduleTemplateBlockBreakId: number;
	timeScheduleTemplatePeriodId: number;
	timeScheduleTypeId: number;
}
export interface IAttestEmployeeDayTimeCodeTransactionDTO {
	guidId: string;
	guidIdTimeBlock: string;
	isTimeCodeAbsenceTime: boolean;
	isTimeCodePresenceOutsideScheduleTime: boolean;
	quantity: number;
	quantityString: string;
	startTime: Date;
	stopTime: Date;
	timeBlockId: number;
	timeCodeId: number;
	timeCodeName: string;
	timeCodeRegistrationType: TermGroup_TimeCodeRegistrationType;
	timeCodeTransactionId: number;
	timeCodeType: SoeTimeCodeType;
	timeRuleId: number;
	timeRuleName: string;
	timeRuleSort: number;
}
export interface IAttestEmployeeDayTimeInvoiceTransactionDTO {
	guidId: string;
}
export interface IAttestEmployeeDayTimeStampDTO {
	accountId: number;
	accountId2: number;
	autoStampOut: boolean;
	employeeChildId: number;
	employeeId: number;
	employeeManuallyAdjusted: boolean;
	extended: ITimeStampEntryExtendedDTO[];
	isBreak: boolean;
	isDistanceWork: boolean;
	isPaidBreak: boolean;
	manuallyAdjusted: boolean;
	note: string;
	originType: TermGroup_TimeStampEntryOriginType;
	shiftTypeId: number;
	status: TermGroup_TimeStampEntryStatus;
	time: Date;
	timeBlockDateId: number;
	timeDeviationCauseId: number;
	timeScheduleTemplatePeriodId: number;
	timeScheduleTypeId: number;
	timeScheduleTypeName: string;
	timeStampEntryId: number;
	timeTerminalAccountId: number;
	timeTerminalId: number;
	type: TimeStampEntryType;
}
export interface IAttestEmployeeListDTO {
	categoryRecords: ICompanyCategoryRecordDTO[];
	employeeId: number;
	employeeNr: string;
	employeeNrSort: string;
	employments: IEmployeeListEmploymentDTO[];
	name: string;
}
export interface IAttestEmployeePeriodDTO {
	absenceTime: System.ITimeSpan;
	attestStateColor: string;
	attestStateId: number;
	attestStateName: string;
	attestStates: IAttestStateDTO[];
	attestStateSort: number;
	employeeId: number;
	employeeName: string;
	employeeNr: string;
	employeeNrAndName: string;
	employeeSex: TermGroup_Sex;
	hasAbsenceTime: boolean;
	hasExpense: boolean;
	hasInformations: boolean;
	hasInvalidTimeStamps: boolean;
	hasOvertime: boolean;
	hasPayrollImports: boolean;
	hasSameAttestState: boolean;
	hasScheduleWithoutTransactions: boolean;
	hasShiftSwaps: boolean;
	hasStandbyTime: boolean;
	hasTimeStampsWithoutTransactions: boolean;
	hasTransactions: boolean;
	hasWarnings: boolean;
	hasWorkedInsideSchedule: boolean;
	hasWorkedOutsideSchedule: boolean;
	informations: SoeTimeAttestInformation[];
	presenceBreakMinutes: number;
	presenceBreakTime: System.ITimeSpan;
	presenceBreakTimeInfo: string;
	presenceDays: number;
	presencePayedTime: System.ITimeSpan;
	presencePayedTimeInfo: string;
	presenceTime: System.ITimeSpan;
	presenceTimeInfo: string;
	scheduleBreakMinutes: number;
	scheduleBreakTime: System.ITimeSpan;
	scheduleBreakTimeInfo: string;
	scheduleDays: number;
	scheduleTime: System.ITimeSpan;
	scheduleTimeInfo: string;
	standbyTime: System.ITimeSpan;
	startDate: Date;
	stopDate: Date;
	sumExpenseAmount: number;
	sumExpenseRows: number;
	sumGrossSalaryAbsence: System.ITimeSpan;
	sumGrossSalaryAbsenceLeaveOfAbsence: System.ITimeSpan;
	sumGrossSalaryAbsenceLeaveOfAbsenceText: string;
	sumGrossSalaryAbsenceParentalLeave: System.ITimeSpan;
	sumGrossSalaryAbsenceParentalLeaveText: string;
	sumGrossSalaryAbsenceSick: System.ITimeSpan;
	sumGrossSalaryAbsenceSickText: string;
	sumGrossSalaryAbsenceTemporaryParentalLeave: System.ITimeSpan;
	sumGrossSalaryAbsenceTemporaryParentalLeaveText: string;
	sumGrossSalaryAbsenceText: string;
	sumGrossSalaryAbsenceVacation: System.ITimeSpan;
	sumGrossSalaryAbsenceVacationText: string;
	sumGrossSalaryAdditionalTime: System.ITimeSpan;
	sumGrossSalaryAdditionalTimeText: string;
	sumGrossSalaryDuty: System.ITimeSpan;
	sumGrossSalaryDutyText: string;
	sumGrossSalaryOBAddition: System.ITimeSpan;
	sumGrossSalaryOBAddition100: System.ITimeSpan;
	sumGrossSalaryOBAddition100Text: string;
	sumGrossSalaryOBAddition113: System.ITimeSpan;
	sumGrossSalaryOBAddition113Text: string;
	sumGrossSalaryOBAddition40: System.ITimeSpan;
	sumGrossSalaryOBAddition40Text: string;
	sumGrossSalaryOBAddition50: System.ITimeSpan;
	sumGrossSalaryOBAddition50Text: string;
	sumGrossSalaryOBAddition57: System.ITimeSpan;
	sumGrossSalaryOBAddition57Text: string;
	sumGrossSalaryOBAddition70: System.ITimeSpan;
	sumGrossSalaryOBAddition70Text: string;
	sumGrossSalaryOBAddition79: System.ITimeSpan;
	sumGrossSalaryOBAddition79Text: string;
	sumGrossSalaryOBAdditionText: string;
	sumGrossSalaryOvertime: System.ITimeSpan;
	sumGrossSalaryOvertime100: System.ITimeSpan;
	sumGrossSalaryOvertime100Text: string;
	sumGrossSalaryOvertime50: System.ITimeSpan;
	sumGrossSalaryOvertime50Text: string;
	sumGrossSalaryOvertime70: System.ITimeSpan;
	sumGrossSalaryOvertime70Text: string;
	sumGrossSalaryOvertimeText: string;
	sumGrossSalaryWeekendSalary: System.ITimeSpan;
	sumGrossSalaryWeekendSalaryText: string;
	sumInvoicedTime: System.ITimeSpan;
	sumInvoicedTimeText: string;
	sumTimeAccumulator: System.ITimeSpan;
	sumTimeAccumulatorOverTime: System.ITimeSpan;
	sumTimeAccumulatorOverTimeText: string;
	sumTimeAccumulatorText: string;
	sumTimeWorkedScheduledTime: System.ITimeSpan;
	sumTimeWorkedScheduledTimeText: string;
	unhandledEmployee: ITimeUnhandledShiftChangesEmployeeDTO;
	uniqueId: string;
	warnings: SoeTimeAttestWarning[];
}
export interface IAttestEmployeesDaySmallDTO {
	dateFrom: Date;
	dateTo: Date;
	employeeId: number;
}
export interface IAttestFunctionOptionDescription {
	description1: string;
	description2: string;
	description2Caption: string;
	description3: string;
	description3Caption: string;
}
export interface IAttestGroupDTO extends IAttestWorkFlowHeadDTO {
	attestGroupCode: string;
	attestGroupName: string;
	isAttestGroup: boolean;
}
export interface IAttestPayrollTransactionDTO {
	absenceIntervalNr: number;
	accountDims: IAccountDimDTO[];
	accountingDescription: string;
	accountingLongString: string;
	accountingSettings: IAccountingSettingsRowDTO[];
	accountingShortString: string;
	accountInternalIds: number[];
	accountInternals: IAccountDTO[];
	accountStd: IAccountDTO;
	accountStdId: number;
	addedDateFrom: Date;
	addedDateTo: Date;
	allTimePayrollTransactionIds: number[];
	amount: number;
	amountCurrency: number;
	amountEntCurrency: number;
	attestItemUniqueId: string;
	attestStateColor: string;
	attestStateId: number;
	attestStateInitial: boolean;
	attestStateName: string;
	attestStateSort: number;
	attestTransitionLogs: IAttestTransitionLogDTO[];
	calenderDayFactor: number;
	comment: string;
	commentGrouping: number;
	created: Date;
	createdBy: string;
	date: Date;
	employeeChildId: number;
	employeeChildName: string;
	employeeId: number;
	employeeVehicleId: number;
	formula: string;
	formulaExtracted: string;
	formulaNames: string;
	formulaOrigin: string;
	formulaPlain: string;
	guidId: string;
	guidIdTimeBlock: string;
	guidIdTimeCodeTransaction: string;
	hasAttestState: boolean;
	hasComment: boolean;
	hasInfo: boolean;
	hasSameAttestState: boolean;
	includedInPayrollProductChain: boolean;
	invoiceQuantity: number;
	isAbsence: boolean;
	isAbsenceAbsenceTime: boolean;
	isAdded: boolean;
	isAddedOrFixed: boolean;
	isAdditionOrDeduction: boolean;
	isAverageCalculated: boolean;
	isBelowEmploymentTaxLimitRuleFromPreviousPeriods: boolean;
	isBelowEmploymentTaxLimitRuleHidden: boolean;
	isCentRounding: boolean;
	isDistributed: boolean;
	isEmployeeVehicle: boolean;
	isEmploymentTaxAndHidden: boolean;
	isExported: boolean;
	isFixed: boolean;
	isModified: boolean;
	isPayrollProductChainMainParent: boolean;
	isPayrollStartValue: boolean;
	isPreliminary: boolean;
	isPresence: boolean;
	isPresenceWorkOutsideScheduleTime: boolean;
	isQuantityOrFixed: boolean;
	isQuantityRounding: boolean;
	isRetroactive: boolean;
	isReversed: boolean;
	isRounding: boolean;
	isScheduleTransaction: boolean;
	isSelectDisabled: boolean;
	isSelected: boolean;
	isSpecifiedUnitPrice: boolean;
	isUnionFee: boolean;
	isVacationFiveDaysPerWeek: boolean;
	isVacationReplacement: boolean;
	isVacationYearEnd: boolean;
	manuallyAdded: boolean;
	modified: Date;
	modifiedBy: string;
	noOfAbsenceAbsenceTime: number;
	noOfPresenceWorkOutsideScheduleTime: number;
	parentGuidId: string;
	parentId: number;
	payrollCalculationPerformed: boolean;
	payrollCalculationProductUniqueId: string;
	payrollImportEmployeeTransactionId: number;
	payrollPriceFormulaId: number;
	payrollPriceTypeId: number;
	payrollProductExport: boolean;
	payrollProductFactor: number;
	payrollProductId: number;
	payrollProductName: string;
	payrollProductNumber: string;
	payrollProductPayed: boolean;
	payrollProductShortName: string;
	payrollProductString: string;
	payrollProductSysPayrollTypeLevel1: number;
	payrollProductSysPayrollTypeLevel2: number;
	payrollProductSysPayrollTypeLevel3: number;
	payrollProductSysPayrollTypeLevel4: number;
	payrollProductUseInPayroll: boolean;
	payrollStartValueRowId: number;
	quantity: number;
	quantityCalendarDays: number;
	quantityDays: number;
	quantityString: string;
	quantityWorkDays: number;
	retroactivePayrollOutcomeId: number;
	retroTransactionType: string;
	reversedDate: Date;
	scheduleTransactionType: SoeTimePayrollScheduleTransactionType;
	showEdit: boolean;
	startTime: Date;
	startTimeString: string;
	stopTime: Date;
	stopTimeString: string;
	sysPayrollTypeLevel1: number;
	sysPayrollTypeLevel2: number;
	sysPayrollTypeLevel3: number;
	sysPayrollTypeLevel4: number;
	timeBlockDateId: number;
	timeBlockId: number;
	timeCodeRegistrationType: TermGroup_TimeCodeRegistrationType;
	timeCodeTransactionId: number;
	timeCodeType: SoeTimeCodeType;
	timePayrollTransactionId: number;
	timePeriodId: number;
	timePeriodName: string;
	timeUnit: number;
	transactionSysPayrollTypeLevel1: number;
	transactionSysPayrollTypeLevel2: number;
	transactionSysPayrollTypeLevel3: number;
	transactionSysPayrollTypeLevel4: number;
	unionFeeId: number;
	unitPrice: number;
	unitPriceCurrency: number;
	unitPriceEntCurrency: number;
	unitPriceGrouping: number;
	unitPricePayrollSlipGrouping: number;
	updateChildren: boolean;
	vacationYearEndRowId: number;
	vatAmount: number;
	vatAmountCurrency: number;
	vatAmountEntCurrency: number;
}
export interface IAttestRoleDTO {
	actorCompanyId: number;
	allowToAddOtherEmployeeAccounts: boolean;
	alsoAttestAdditionsFromTime: boolean;
	attestByEmployeeAccount: boolean;
	attestRoleId: number;
	attestRoleMapping: IAttestRoleMappingDTO[];
	created: Date;
	createdBy: string;
	defaultMaxAmount: number;
	description: string;
	externalCodes: string[];
	externalCodesString: string;
	humanResourcesPrivacy: boolean;
	isExecutive: boolean;
	modified: Date;
	modifiedBy: string;
	module: SoeModule;
	name: string;
	primaryCategoryRecords: ICompanyCategoryRecordDTO[];
	reminderAttestStateId: number;
	reminderNoOfDays: number;
	reminderPeriodType: number;
	secondaryCategoryRecords: ICompanyCategoryRecordDTO[];
	showAllCategories: boolean;
	showAllSecondaryCategories: boolean;
	showTemplateSchedule: boolean;
	showUncategorized: boolean;
	sort: number;
	staffingByEmployeeAccount: boolean;
	state: SoeEntityState;
	transitionIds: number[];
	active: boolean;
}
export interface IUpdateAttestRoleModel {
	dict: { [key: number]: boolean; };
	module: SoeModule;
}
export interface IAttestRoleMappingDTO {
	attestRoleMappingId: number;
	childtAttestRoleId: number;
	childtAttestRoleName: string;
	created: Date;
	createdBy: string;
	dateFrom: Date;
	dateTo: Date;
	entity: TermGroup_AttestEntity;
	modified: Date;
	modifiedBy: string;
	parentAttestRoleId: number;
	parentAttestRoleName: string;
	state: SoeEntityState;
}
export interface IAttestRuleHeadDTO {
	actorCompanyId: number;
	attestRuleHeadId: number;
	attestRuleRows: IAttestRuleRowDTO[];
	created: Date;
	createdBy: string;
	dayTypeCompanyId: number;
	dayTypeCompanyName: string;
	dayTypeId: number;
	dayTypeName: string;
	description: string;
	employeeGroupIds: number[];
	isSelected: boolean;
	modified: Date;
	modifiedBy: string;
	module: SoeModule;
	name: string;
	scheduledJobHeadId: number;
	state: SoeEntityState;
}
export interface IAttestRuleHeadGridDTO {
	attestRuleHeadId: number;
	dayTypeName: string;
	description: string;
	employeeGroupNames: string;
	name: string;
	state: SoeEntityState;
}
export interface IAttestRuleRowDTO {
	attestRuleHeadId: number;
	attestRuleRowId: number;
	comparisonOperator: WildCard;
	comparisonOperatorString: string;
	created: Date;
	createdBy: string;
	leftValueId: number;
	leftValueIdName: string;
	leftValueType: TermGroup_AttestRuleRowLeftValueType;
	leftValueTypeName: string;
	minutes: number;
	modified: Date;
	modifiedBy: string;
	rightValueId: number;
	rightValueIdName: string;
	rightValueType: TermGroup_AttestRuleRowRightValueType;
	rightValueTypeName: string;
	showLeftValueId: boolean;
	showRightValueId: boolean;
	state: SoeEntityState;
}
export interface IAttestStateDTO {
	actorCompanyId: number;
	attestStateId: number;
	closed: boolean;
	color: string;
	created: Date;
	createdBy: string;
	description: string;
	entity: TermGroup_AttestEntity;
	entityName: string;
	hidden: boolean;
	imageSource: string;
	initial: boolean;
	langId: number;
	locked: boolean;
	modified: Date;
	modifiedBy: string;
	module: SoeModule;
	name: string;
	sort: number;
	state: SoeEntityState;
}
export interface IAttestStateSmallDTO {
	attestStateId: number;
	closed: boolean;
	color: string;
	description: string;
	imageSource: string;
	initial: boolean;
	name: string;
	sort: number;
}
export interface IAttestTransitionDTO {
	actorCompanyId: number;
	attestStateFrom: IAttestStateDTO;
	attestStateFromId: number;
	attestStateTo: IAttestStateDTO;
	attestStateToId: number;
	attestTransitionId: number;
	created: Date;
	createdBy: string;
	modified: Date;
	modifiedBy: string;
	module: SoeModule;
	name: string;
	notifyChangeOfAttestState: boolean;
	state: SoeEntityState;
}
export interface IAttestTransitionLogDTO {
	attestStateFromName: string;
	attestStateToName: string;
	attestTransitionCreatedBySupport: boolean;
	attestTransitionDate: Date;
	attestTransitionLogId: number;
	attestTransitionUserId: number;
	attestTransitionUserName: string;
	timePayrollTransactionId: number;
}
export interface IAttestWorkFlowHeadDTO {
	actorCompanyId: number;
	adminInformation: string;
	attestWorkFlowGroupId: number;
	attestWorkFlowHeadId: number;
	attestWorkFlowTemplateHeadId: number;
	created: Date;
	createdBy: string;
	entity: SoeEntityType;
	isAttestGroup: boolean;
	isDeleted: boolean;
	modified: Date;
	modifiedBy: string;
	name: string;
	recordId: number;
	rows: IAttestWorkFlowRowDTO[];
	sendMessage: boolean;
	signInitial: boolean;
	state: SoeEntityState;
	templateName: string;
	type: TermGroup_AttestWorkFlowType;
	typeName: string;
}
export interface IAttestWorkFlowOverviewGridDTO {
	attestComments: string;
	attestFlowOverdued: boolean;
	attestStateColor: string;
	attestStateId: number;
	attestStateName: string;
	blockPayment: boolean;
	blockReason: string;
	costCentreId: number;
	costCentreName: string;
	currency: string;
	defaultDim2Id: number;
	defaultDim2Name: string;
	defaultDim3Id: number;
	defaultDim3Name: string;
	defaultDim4Id: number;
	defaultDim4Name: string;
	defaultDim5Id: number;
	defaultDim5Name: string;
	defaultDim6Id: number;
	defaultDim6Name: string;
	dueDate: Date;
	fullyPaid: boolean;
	hasPicture: boolean;
	internalDescription: string;
	invoiceDate: Date;
	invoiceId: number;
	invoiceNr: string;
	lastPaymentDate: Date;
	orderNr: number;
	ownerActorId: number;
	paidAmount: number;
	payDate: Date;
	projectNr: string;
	referenceOur: string;
	selected: boolean;
	seqNr: number;
	showAttestCommentIcon: boolean;
	supplierName: string;
	supplierNr: string;
	totalAmount: number;
	totalAmountExVat: number;
	voucherDate: Date;
}
export interface IAttestWorkFlowRowDTO {
	answer: boolean;
	answerDate: Date;
	answerText: string;
	attestRoleId: number;
	attestRoleName: string;
	attestStateFromId: number;
	attestStateFromName: string;
	attestStateSort: number;
	attestStateToName: string;
	attestTransitionId: number;
	attestTransitionName: string;
	attestWorkFlowHeadId: number;
	attestWorkFlowRowId: number;
	comment: string;
	commentDate: Date;
	commentUser: string;
	created: Date;
	createdBy: string;
	isCurrentUser: boolean;
	isDeleted: boolean;
	loginName: string;
	modified: Date;
	modifiedBy: string;
	name: string;
	originateFromRowId: number;
	processType: TermGroup_AttestWorkFlowRowProcessType;
	processTypeName: string;
	processTypeSort: number;
	state: TermGroup_AttestFlowRowState;
	type: TermGroup_AttestWorkFlowType;
	typeName: string;
	userId: number;
	workFlowRowIdToReplace: number;
}
export interface IAttestWorkFlowTemplateHeadDTO {
	actorCompanyId: number;
	attestEntity: TermGroup_AttestEntity;
	attestWorkFlowTemplateHeadId: number;
	created: Date;
	createdBy: string;
	description: string;
	modified: Date;
	modifiedBy: string;
	name: string;
	state: SoeEntityState;
	type: TermGroup_AttestWorkFlowType;
}
export interface IAttestWorkFlowTemplateHeadGridDTO {
	attestWorkFlowTemplateHeadId: number;
	description: string;
	name: string;
	type: TermGroup_AttestWorkFlowType;
}
export interface IAttestWorkFlowTemplateRowDTO {
	attestStateFromName: string;
	attestStateToColor: string;
	attestStateToName: string;
	attestTransitionId: number;
	attestTransitionName: string;
	attestWorkFlowTemplateHeadId: number;
	attestWorkFlowTemplateRowId: number;
	closed: boolean;
	initial: boolean;
	sort: number;
	type: number;
	typeName: string;
}

export interface IAutomaticAllocationResultDTO {
	employeeResults: IAutomaticAllocationEmployeeResultDTO[];
	success: boolean;
	message: string;
}

export interface IAutomaticAllocationEmployeeResultDTO {
	employeeId: number;
	dayResults: IAutomaticAllocationEmployeeDayResultDTO[];
	status: LeisureCodeAllocationEmployeeStatus;
	message: string;
}

export interface IAutomaticAllocationEmployeeDayResultDTO {
	date: Date;
	success: boolean;
	message: string;
}

export interface IAvailableEmployeesDTO {
	age: number;
	employeeId: number;
	employeeNr: string;
	employmentDays: number;
	scheduleMinutes: number;
	wantsExtraShifts: boolean;
}
export interface IBalanceRuleSettingDTO {
	balanceRuleSettingId: number;
	balanceRuleSettingTempId: System.IGuid;
	replacementTimeCodeId: number;
	timeCodeId: number;
	timeRuleGroupId: number;
}
export interface IBatchUpdateDTO {
	boolValue: boolean;
	children: IBatchUpdateDTO[];
	dataType: BatchUpdateFieldType;
	dateValue: Date;
	decimalValue: number;
	doShowFilter: boolean;
	doShowFromDate: boolean;
	doShowToDate: boolean;
	field: number;
	fromDate: Date;
	intValue: number;
	label: string;
	options: INameAndIdDTO[];
	stringValue: string;
	toDate: Date;
}
export interface IBillingInvoiceDTO {
	actorId: number;
	addAttachementsToEInvoice: boolean;
	addSupplierInvoicesToEInvoice: boolean;
	billingAddressId: number;
	billingAdressText: string;
	billingInvoicePrinted: boolean;
	billingType: TermGroup_BillingType;
	cashSale: boolean;
	categoryIds: number[];
	centRounding: number;
	checkConflictsOnSave: boolean;
	contactEComId: number;
	contactGLNId: number;
	contractNr: string;
	created: Date;
	createdBy: string;
	currencyDate: Date;
	currencyId: number;
	currencyRate: number;
	customerBlockNote: string;
	customerEmail: string;
	customerInvoiceRows: IProductRowDTO[];
	customerName: string;
	customerPhoneNr: string;
	defaultDim1AccountId: number;
	defaultDim2AccountId: number;
	defaultDim3AccountId: number;
	defaultDim4AccountId: number;
	defaultDim5AccountId: number;
	defaultDim6AccountId: number;
	deliveryAddressId: number;
	deliveryConditionId: number;
	deliveryCustomerId: number;
	deliveryDate: Date;
	deliveryDateText: string;
	deliveryTypeId: number;
	dueDate: Date;
	estimatedTime: number;
	fixedPriceOrder: boolean;
	forceSave: boolean;
	freightAmount: number;
	freightAmountCurrency: number;
	hasManuallyDeletedTimeProjectRows: boolean;
	hasOrder: boolean;
	includeOnInvoice: boolean;
	includeOnlyInvoicedTime: boolean;
	insecureDebt: boolean;
	invoiceDate: Date;
	invoiceDeliveryProvider: number;
	invoiceDeliveryType: number;
	invoiceFee: number;
	invoiceFeeCurrency: number;
	invoiceHeadText: string;
	invoiceId: number;
	invoiceLabel: string;
	invoiceNr: string;
	invoicePaymentService: number;
	invoiceText: string;
	isTemplate: boolean;
	manuallyAdjustedAccounting: boolean;
	marginalIncomeCurrency: number;
	marginalIncomeRatio: number;
	modified: Date;
	modifiedBy: string;
	nbrOfChecklists: number;
	note: string;
	orderDate: Date;
	orderNumbers: string;
	orderReference: string;
	orderType: TermGroup_OrderType;
	originDescription: string;
	originStatus: SoeOriginStatus;
	originStatusName: string;
	originUsers: IOriginUserSmallDTO[];
	paidAmount: number;
	paidAmountCurrency: number;
	paymentConditionId: number;
	plannedStartDate: Date;
	plannedStopDate: Date;
	prevInvoiceId: number;
	priceListTypeId: number;
	printTimeReport: boolean;
	priority: number;
	projectId: number;
	projectNr: string;
	referenceOur: string;
	referenceYour: string;
	remainingAmount: number;
	remainingAmountExVat: number;
	remainingTime: number;
	seqNr: number;
	shiftTypeId: number;
	statusIcon: SoeStatusIcon;
	sumAmount: number;
	sumAmountCurrency: number;
	sysWholeSellerId: number;
	totalAmount: number;
	totalAmountCurrency: number;
	transferedFromOffer: boolean;
	transferedFromOrder: boolean;
	transferedFromOriginType: SoeOriginType;
	triangulationSales: boolean;
	vatAmount: number;
	vatAmountCurrency: number;
	vatType: TermGroup_InvoiceVatType;
	voucherDate: Date;
	voucherSeriesId: number;
	workingDescription: string;
}
export interface IBoolSelectionDTO extends IReportDataSelectionDTO {
	value: boolean;
}
export interface IBudgetHeadDTO {
	accountYearId: number;
	accountYearText: string;
	actorCompanyId: number;
	budgetHeadId: number;
	created: Date;
	createdBy: string;
	createdDate: string;
	dim2Id: number;
	dim3Id: number;
	distributionCodeHeadId: number;
	fromDate: Date;
	modified: Date;
	modifiedBy: string;
	name: string;
	noOfPeriods: number;
	projectId: number;
	rows: IBudgetRowDTO[];
	status: number;
	statusName: string;
	toDate: Date;
	type: number;
	useDim2: boolean;
	useDim3: boolean;
}
export interface IBudgetHeadFlattenedDTO {
	accountYearId: number;
	accountYearText: string;
	actorCompanyId: number;
	budgetHeadId: number;
	created: Date;
	createdBy: string;
	createdDate: string;
	dim2Id: number;
	dim3Id: number;
	distributionCodeHeadId: number;
	distributionCodeSubType: number;
	modified: Date;
	modifiedBy: string;
	name: string;
	noOfPeriods: number;
	projectId: number;
	rows: IBudgetRowFlattenedDTO[];
	status: number;
	statusName: string;
	type: number;
	useDim2: boolean;
	useDim3: boolean;
}
export interface IBudgetHeadSalesDTO {
	actorCompanyId: number;
	budgetHeadId: number;
	created: Date;
	createdBy: string;
	distributionCodeHeadId: number;
	distributionCodeSubType: number;
	fromDate: Date;
	modified: Date;
	modifiedBy: string;
	name: string;
	noOfPeriods: number;
	rows: IBudgetRowSalesDTO[];
	status: number;
	statusName: string;
	toDate: Date;
	type: number;
}
export interface IBudgetPeriodDTO {
	amount: number;
	budgetRow: IBudgetRowDTO;
	budgetRowId: number;
	budgetRowPeriodId: number;
	distributionCodeHeadId: number;
	isModified: boolean;
	periodNr: number;
	quantity: number;
	startDate: Date;
	type: number;
}
export interface IBudgetPeriodSalesDTO {
	amount: number;
	budgetRowId: number;
	budgetRowNr: number;
	budgetRowPeriodId: number;
	closingHour: number;
	distributionCodeHeadId: number;
	guid: System.IGuid;
	isModified: boolean;
	parentGuid: System.IGuid;
	percent: number;
	periodNr: number;
	periods: IBudgetPeriodSalesDTO[];
	quantity: number;
	startDate: Date;
	startHour: number;
	type: number;
}
export interface IBudgetRowDTO {
	accountId: number;
	budgetHead: IBudgetHeadDTO;
	budgetHeadId: number;
	budgetRowId: number;
	budgetRowNr: number;
	dim1Id: number;
	dim1Name: string;
	dim1Nr: string;
	dim2Id: number;
	dim2Name: string;
	dim2Nr: string;
	dim3Id: number;
	dim3Name: string;
	dim3Nr: string;
	dim4Id: number;
	dim4Name: string;
	dim4Nr: string;
	dim5Id: number;
	dim5Name: string;
	dim5Nr: string;
	dim6Id: number;
	dim6Name: string;
	dim6Nr: string;
	distributionCodeHeadId: number;
	distributionCodeHeadName: string;
	isAdded: boolean;
	isDeleted: boolean;
	isModified: boolean;
	modified: string;
	modifiedBy: string;
	modifiedUserId: number;
	name: string;
	periods: IBudgetPeriodDTO[];
	shiftTypeId: number;
	timeCodeId: number;
	totalAmount: number;
	totalQuantity: number;
	type: number;
}
export interface IBudgetRowFlattenedDTO {
	accountId: number;
	amount1: number;
	amount10: number;
	amount11: number;
	amount12: number;
	amount13: number;
	amount14: number;
	amount15: number;
	amount16: number;
	amount17: number;
	amount18: number;
	amount19: number;
	amount2: number;
	amount20: number;
	amount21: number;
	amount22: number;
	amount23: number;
	amount24: number;
	amount25: number;
	amount26: number;
	amount27: number;
	amount28: number;
	amount29: number;
	amount3: number;
	amount30: number;
	amount31: number;
	amount4: number;
	amount5: number;
	amount6: number;
	amount7: number;
	amount8: number;
	amount9: number;
	budgetHeadId: number;
	budgetRowId: number;
	budgetRowNr: number;
	budgetRowPeriodId1: number;
	budgetRowPeriodId10: number;
	budgetRowPeriodId11: number;
	budgetRowPeriodId12: number;
	budgetRowPeriodId13: number;
	budgetRowPeriodId14: number;
	budgetRowPeriodId15: number;
	budgetRowPeriodId16: number;
	budgetRowPeriodId17: number;
	budgetRowPeriodId18: number;
	budgetRowPeriodId19: number;
	budgetRowPeriodId2: number;
	budgetRowPeriodId20: number;
	budgetRowPeriodId21: number;
	budgetRowPeriodId22: number;
	budgetRowPeriodId23: number;
	budgetRowPeriodId24: number;
	budgetRowPeriodId25: number;
	budgetRowPeriodId26: number;
	budgetRowPeriodId27: number;
	budgetRowPeriodId28: number;
	budgetRowPeriodId29: number;
	budgetRowPeriodId3: number;
	budgetRowPeriodId30: number;
	budgetRowPeriodId31: number;
	budgetRowPeriodId4: number;
	budgetRowPeriodId5: number;
	budgetRowPeriodId6: number;
	budgetRowPeriodId7: number;
	budgetRowPeriodId8: number;
	budgetRowPeriodId9: number;
	dim1Id: number;
	dim1Name: string;
	dim1Nr: string;
	dim2Id: number;
	dim2Name: string;
	dim2Nr: string;
	dim3Id: number;
	dim3Name: string;
	dim3Nr: string;
	dim4Id: number;
	dim4Name: string;
	dim4Nr: string;
	dim5Id: number;
	dim5Name: string;
	dim5Nr: string;
	dim6Id: number;
	dim6Name: string;
	dim6Nr: string;
	distributionCodeHeadId: number;
	distributionCodeHeadName: string;
	isDeleted: boolean;
	isModified: boolean;
	modified: string;
	modifiedBy: string;
	modifiedUserId: number;
	periodNr1: number;
	periodNr10: number;
	periodNr11: number;
	periodNr12: number;
	periodNr13: number;
	periodNr14: number;
	periodNr15: number;
	periodNr16: number;
	periodNr17: number;
	periodNr18: number;
	periodNr19: number;
	periodNr2: number;
	periodNr20: number;
	periodNr21: number;
	periodNr22: number;
	periodNr23: number;
	periodNr24: number;
	periodNr25: number;
	periodNr26: number;
	periodNr27: number;
	periodNr28: number;
	periodNr29: number;
	periodNr3: number;
	periodNr30: number;
	periodNr31: number;
	periodNr4: number;
	periodNr5: number;
	periodNr6: number;
	periodNr7: number;
	periodNr8: number;
	periodNr9: number;
	quantity1: number;
	quantity10: number;
	quantity11: number;
	quantity12: number;
	quantity13: number;
	quantity14: number;
	quantity15: number;
	quantity16: number;
	quantity17: number;
	quantity18: number;
	quantity19: number;
	quantity2: number;
	quantity20: number;
	quantity21: number;
	quantity22: number;
	quantity23: number;
	quantity24: number;
	quantity25: number;
	quantity26: number;
	quantity27: number;
	quantity28: number;
	quantity29: number;
	quantity3: number;
	quantity30: number;
	quantity31: number;
	quantity4: number;
	quantity5: number;
	quantity6: number;
	quantity7: number;
	quantity8: number;
	quantity9: number;
	shiftTypeId: number;
	startDate1: Date;
	startDate10: Date;
	startDate11: Date;
	startDate12: Date;
	startDate13: Date;
	startDate14: Date;
	startDate15: Date;
	startDate16: Date;
	startDate17: Date;
	startDate18: Date;
	startDate19: Date;
	startDate2: Date;
	startDate20: Date;
	startDate21: Date;
	startDate22: Date;
	startDate23: Date;
	startDate24: Date;
	startDate25: Date;
	startDate26: Date;
	startDate27: Date;
	startDate28: Date;
	startDate29: Date;
	startDate3: Date;
	startDate30: Date;
	startDate31: Date;
	startDate4: Date;
	startDate5: Date;
	startDate6: Date;
	startDate7: Date;
	startDate8: Date;
	startDate9: Date;
	totalAmount: number;
	totalQuantity: number;
	type: number;
}
export interface IBudgetRowSalesDTO {
	accountId: number;
	budgetHeadId: number;
	budgetRowId: number;
	budgetRowNr: number;
	dim1Id: number;
	dim1Name: string;
	dim1Nr: string;
	dim2Id: number;
	dim2Name: string;
	dim2Nr: string;
	dim3Id: number;
	dim3Name: string;
	dim3Nr: string;
	dim4Id: number;
	dim4Name: string;
	dim4Nr: string;
	dim5Id: number;
	dim5Name: string;
	dim5Nr: string;
	dim6Id: number;
	dim6Name: string;
	dim6Nr: string;
	distributionCodeHeadName: string;
	isDeleted: boolean;
	isModified: boolean;
	modified: string;
	modifiedBy: string;
	modifiedUserId: number;
	periods: IBudgetPeriodSalesDTO[];
	totalAmount: number;
	totalQuantity: number;
	type: number;
}
export interface ICalculateVacationResultContainer {
	employeeCalculateVacationResultHeadId: number;
	employeeId: number;
	employeeName: string;
	employeeNr: string;
	results: IEmployeeCalculateVacationResultDTO[];
}
export interface ICategoryAccountDTO {
	accountId: number;
	actorCompanyId: number;
	categoryAccountId: number;
	categoryId: number;
	dateFrom: Date;
	dateTo: Date;
	state: SoeEntityState;
}
export interface ICategoryDTO {
	actorCompanyId: number;
	categoryGroupId: number;
	categoryGroupName: string;
	categoryId: number;
	children: ICategoryDTO[];
	childrenNamesString: string;
	code: string;
	companyCategoryRecords: ICompanyCategoryRecordDTO[];
	isSelected: boolean;
	isVisible: boolean;
	name: string;
	parentId: number;
	state: SoeEntityState;
	type: SoeCategoryType;
}
export interface IChecklistExtendedRowDTO {
	boolData: boolean;
	checkListMultipleChoiceAnswerHeadId: number;
	comment: string;
	created: Date;
	createdBy: string;
	dataTypeId: number;
	date: Date;
	dateData: Date;
	decimalData: number;
	description: string;
	guid: System.IGuid;
	headId: number;
	headRecordId: number;
	intData: number;
	isHeadline: boolean;
	mandatory: boolean;
	modified: Date;
	modifiedBy: string;
	name: string;
	rowId: number;
	rowNr: number;
	rowRecordId: number;
	strData: string;
	text: string;
	type: TermGroup_ChecklistRowType;
	value: string;
}
export interface IChecklistHeadDTO {
	actorCompanyId: number;
	addAttachementsToEInvoice: boolean;
	checklistHeadId: number;
	checklistHeadRecordId: number;
	checklistRows: IChecklistRowDTO[];
	created: Date;
	createdBy: string;
	description: string;
	modified: Date;
	modifiedBy: string;
	name: string;
	reportId: number;
	defaultInOrder: boolean;
	state: SoeEntityState;
	type: TermGroup_ChecklistHeadType;
	typeName: string;
}
export interface IChecklistHeadRecordCompactDTO {
	addAttachementsToEInvoice: boolean;
	checklistHeadId: number;
	checklistHeadName: string;
	checklistHeadRecordId: number;
	checklistRowRecords: IChecklistExtendedRowDTO[];
	created: Date;
	recordId: number;
	rowNr: number;
	signatures: IImagesDTO[];
	state: SoeEntityState;
	tempHeadId: System.IGuid;
}
export interface IChecklistHeadRecordDTO {
	actorCompanyId: number;
	checklistHead: IChecklistHeadDTO;
	checklistHeadId: number;
	checklistHeadRecordId: number;
	created: Date;
	createdBy: string;
	entity: SoeEntityType;
	modified: Date;
	modifiedBy: string;
	recordId: number;
	state: SoeEntityState;
	tempHeadId: System.IGuid;
}
export interface ICheckListMultipleChoiceAnswerHeadDTO {
	actorCompanyId: number;
	checkListMultipleChoiceAnswerHeadId: number;
	checklistRows: IChecklistRowDTO[];
	created: Date;
	createdBy: string;
	modified: Date;
	modifiedBy: string;
	state: SoeEntityState;
	title: string;
	typeName: string;
}
export interface ICheckListMultipleChoiceAnswerRowDTO {
	checkListMultipleChoiceAnswerHeadId: number;
	checkListMultipleChoiceAnswerRowId: number;
	checklistRows: IChecklistRowDTO[];
	created: Date;
	createdBy: string;
	modified: Date;
	modifiedBy: string;
	question: string;
	state: SoeEntityState;
	typeName: string;
}
export interface IChecklistRowDTO {
	checklistHead: IChecklistHeadDTO;
	checklistHeadId: number;
	checkListMultipleChoiceAnswerHeadId: number;
	checklistRowId: number;
	created: Date;
	createdBy: string;
	guid: System.IGuid;
	isModified: boolean;
	mandatory: boolean;
	mandatoryName: string;
	modified: Date;
	modifiedBy: string;
	rowNr: number;
	state: SoeEntityState;
	text: string;
	type: TermGroup_ChecklistRowType;
	typeName: string;
}
export interface ICommodityCodeDTO {
	code: string;
	endDate: Date;
	intrastatCodeId: number;
	isActive: boolean;
	startDate: Date;
	sysIntrastatCodeId: number;
	text: string;
	useOtherQuantity: boolean;
}
export interface ICompanyAttestRoleDTO {
	alsoAttestAdditionsFromTime: boolean;
	attestByEmployeeAccount: boolean;
	attestRoleId: number;
	defaultMaxAmount: number;
	humanResourcesPrivacy: boolean;
	isExecutive: boolean;
	isNearestManager: boolean;
	moduleName: string;
	name: string;
	showAllCategories: boolean;
	showTemplateSchedule: boolean;
	showUncategorized: boolean;
	staffingByEmployeeAccount: boolean;
	state: SoeEntityState;
}
export interface ICompanyCategoryRecordDTO {
	actorCompanyId: number;
	category: ICategoryDTO;
	categoryId: number;
	companyCategoryId: number;
	dateFrom: Date;
	dateTo: Date;
	default: boolean;
	entity: SoeCategoryRecordEntity;
	isExecutive: boolean;
	recordId: number;
	uniqueId: string;
}
export interface ICompanyDTO {
	actorCompanyId: number;
	allowSupportLogin: boolean;
	allowSupportLoginTo: Date;
	demo: boolean;
	global: boolean;
	language: TermGroup_Languages;
	licenseId: number;
	licenseNr: string;
	licenseSupport: boolean;
	name: string;
	number: number;
	orgNr: string;
	shortName: string;
	sysCountryId: number;
	template: boolean;
	timeSpotId: number;
	vatNr: string;
}
export interface ICompanyEditDTO extends ICompanyDTO {
	baseEntCurrencyId: number;
	baseSysCurrencyId: number;
	companyApiKey: string;
	companyTaxSupport: boolean;
	contactAddresses: IContactAddressItem[];
	created: Date;
	createdBy: string;
	defaultSysPaymentTypeId: number;
	ediActivated: Date;
	ediActivatedBy: string;
	ediModified: Date;
	ediModifiedBy: string;
	ediPassword: string;
	ediUsername: string;
	isEdiActivated: boolean;
	isEdiGOActivated: boolean;
	maxNrOfSMS: number;
	modified: Date;
	modifiedBy: string;
	paymentInformation: IPaymentInformationDTO;
}
export interface ICompanyExternalCodeDTO {
	actorCompanyId: number;
	companyExternalCodeId: number;
	entity: TermGroup_CompanyExternalCodeEntity;
	externalCode: string;
	recordId: number;
}
export interface ICompanyExternalCodeGridDTO {
	companyExternalCodeId: number;
	entity: TermGroup_CompanyExternalCodeEntity;
	entityName: string;
	externalCode: string;
	recordId: number;
	recordName: string;
}
export interface ICompanyFieldSettingDTO {
	actorCompanyId: number;
	boldLabel: boolean;
	label: string;
	readOnly: boolean;
	skipTabStop: boolean;
	visible: boolean;
}
export interface ICompanyGroupMappingHeadDTO {
	actorCompanyId: number;
	companyGroupMappingHeadId: number;
	created: Date;
	createdBy: string;
	description: string;
	modified: Date;
	modifiedBy: string;
	name: string;
	number: number;
	rows: ICompanyGroupMappingRowDTO[];
	state: SoeEntityState;
	type: number;
}
export interface ICompanyGroupMappingRowDTO {
	childAccountFrom: number;
	childAccountFromName: string;
	childAccountTo: number;
	childAccountToName: string;
	companyGroupMappingHeadId: number;
	companyGroupMappingRowId: number;
	created: Date;
	createdBy: string;
	groupCompanyAccount: number;
	groupCompanyAccountName: string;
	isDeleted: boolean;
	isModified: boolean;
	isProcessed: boolean;
	modified: Date;
	modifiedBy: string;
	rowNr: number;
	state: SoeEntityState;
}
export interface ICompanyRolesDTO {
	actorCompanyId: number;
	attestRoles: ICompanyAttestRoleDTO[];
	companyName: string;
	roles: IUserCompanyRoleDTO[];
}
export interface ICompanyWholesellerListDTO extends SoftOne.Soe.Common.DTO.IWholesellerDTO {
	active: boolean;
	companySysWholesellerDtoId: number;
}
export interface ICompanyWholesellerPriceListViewDTO {
	actorCompanyId: number;
	companyWholesellerId: number;
	companyWholesellerPriceListId: number;
	date: Date;
	hasNewerVersion: boolean;
	isUsed: boolean;
	priceListImportedHeadId: number;
	priceListName: string;
	priceListOrigin: PriceListOrigin;
	provider: number;
	sysPriceListHeadId: number;
	sysWholesellerCountry: TermGroup_Country;
	sysWholesellerId: number;
	sysWholesellerName: string;
	version: number;
}
export interface ICompCurrencyDTO {
	code: string;
	compCurrencyRates: ICompCurrencyRateDTO[];
	currencyId: number;
	date: Date;
	name: string;
	rateToBase: number;
	sysCurrencyId: number;
}
export interface ICompCurrencyRateDTO {
	code: string;
	currencyId: number;
	currencyRateId: number;
	date: Date;
	intervalType: TermGroup_CurrencyIntervalType;
	intervalTypeName: string;
	name: string;
	rateToBase: number;
	source: TermGroup_CurrencySource;
	sourceName: string;
}
export interface ICompTermDTO {
	compTermId: number;
	lang: TermGroup_Languages;
	langName: string;
	name: string;
	recordId: number;
	recordType: CompTermsRecordType;
	state: SoeEntityState;
}
export interface IContactAddressDTO {
	address: string;
	contactAddressId: number;
	contactAddressRows: IContactAddressRowDTO[];
	contactId: number;
	created: Date;
	createdBy: string;
	isSecret: boolean;
	modified: Date;
	modifiedBy: string;
	name: string;
	sysContactAddressTypeId: TermGroup_SysContactAddressType;
}
export interface IContactAddressItem {
	address: string;
	addressCO: string;
	addressCOIsSecret: boolean;
	addressIsSecret: boolean;
	addressName: string;
	contactAddressId: number;
	contactAddressItemType: ContactAddressItemType;
	contactEComId: number;
	contactId: number;
	country: string;
	countryIsSecret: boolean;
	displayAddress: string;
	eComDescription: string;
	eComIsSecret: boolean;
	eComText: string;
	entranceCode: string;
	entranceCodeIsSecret: boolean;
	icon: string;
	isAddress: boolean;
	isSecret: boolean;
	name: string;
	postalAddress: string;
	postalCode: string;
	postalIsSecret: boolean;
	streetAddress: string;
	streetAddressIsSecret: boolean;
	sysContactAddressTypeId: number;
	sysContactEComTypeId: number;
	typeName: string;
}
export interface IContactAddressRowDTO {
	contactAddressId: number;
	created: Date;
	createdBy: string;
	modified: Date;
	modifiedBy: string;
	rowNr: number;
	sysContactAddressRowTypeId: TermGroup_SysContactAddressRowType;
	text: string;
}
export interface IContactAdressIODTO {
	address: string;
	coAddress: string;
	contactAddressId: number;
	country: string;
	name: string;
	postalAddress: string;
	postalCode: string;
}
export interface IContactEComIODTO {
	contactEComId: number;
	name: string;
	text: string;
}
export interface IContactPersonDTO {
	actorContactPersonId: number;
	consentDate: Date;
	consentModified: Date;
	consentModifiedBy: string;
	created: Date;
	createdBy: string;
	description: string;
	email: string;
	firstAndLastName: string;
	firstName: string;
	hasConsent: boolean;
	lastName: string;
	modified: Date;
	modifiedBy: string;
	phoneNumber: string;
	position: number;
	positionName: string;
	sex: TermGroup_Sex;
	socialSec: string;
	state: SoeEntityState;
}
export interface IContractGroupDTO {
	actorCompanyId: number;
	contractGroupId: number;
	created: Date;
	createdBy: string;
	dayInMonth: number;
	description: string;
	interval: number;
	invoiceTemplate: number;
	invoiceText: string;
	invoiceTextRow: string;
	modified: Date;
	modifiedBy: string;
	name: string;
	orderTemplate: number;
	period: TermGroup_ContractGroupPeriod;
	priceManagement: TermGroup_ContractGroupPriceManagement;
	state: SoeEntityState;
}
export interface IContractGroupExtendedGridDTO extends IContractGroupGridDTO {
	dayInMonth: number;
	interval: number;
	periodId: number;
	periodText: string;
	priceManagementText: string;
}
export interface IContractGroupGridDTO {
	contractGroupId: number;
	description: string;
	name: string;
}
export interface IContractTraceViewDTO {
	amount: number;
	amountCurrency: number;
	billingType: TermGroup_BillingType;
	billingTypeName: string;
	contractId: number;
	currencyCode: string;
	currencyRate: number;
	date: Date;
	description: string;
	foreign: boolean;
	invoiceId: number;
	isInvoice: boolean;
	isOrder: boolean;
	isProject: boolean;
	langId: number;
	number: string;
	orderId: number;
	originStatus: SoeOriginStatus;
	originStatusName: string;
	originType: SoeOriginType;
	originTypeName: string;
	projectId: number;
	state: SoeEntityState;
	sysCurrencyId: number;
	vatAmount: number;
	vatAmountCurrency: number;
}
export interface ICopyFromTemplateCompanyInputDTO {
	actorCompanyId: number;
	copyDict: System.Collections.Generic.IKeyValuePair[];
	liberCopy: boolean;
	templateCompanyId: number;
	update: boolean;
	userId: number;
}
export interface ICreateTemplateFromScenarioDTO {
	dateFrom: Date;
	dateTo: Date;
	rows: ICreateTemplateFromScenarioRowDTO[];
	timeScheduleScenarioHeadId: number;
	weekInCycle: number;
}
export interface ICreateTemplateFromScenarioRowDTO {
	date: Date;
	employeeId: number;
}
export interface ICreateVacantEmployeeDTO {
	accounts: IEmployeeAccountDTO[];
	categories: ICompanyCategoryRecordDTO[];
	employeeGroupId: number;
	employeeNr: string;
	employmentDateFrom: Date;
	firstName: string;
	lastName: string;
	percent: number;
	workTimeWeek: number;
}
export interface ICsrResponseDTO {
	employeeId: number;
	errorMessage: string;
	felkod: string;
	felmeddelande: string;
	giltigfrom: string;
	giltigtom: string;
	personnummer: string;
	procentbeslut: number;
	skatteform: string;
	skattetabell: string;
	staendejamkningprocent: number;
	year: number;
}
export interface ICustomerDTO {
	accountingSettings: IAccountingSettingsRowDTO[];
	active: boolean;
	actorCustomerId: number;
	addAttachementsToEInvoice: boolean;
	addSupplierInvoicesToEInvoice: boolean;
	agreementTemplate: number;
	bankAccountNr: string;
	billingTemplate: number;
	blockInvoice: boolean;
	blockNote: string;
	blockOrder: boolean;
	categoryIds: number[];
	consentDate: Date;
	consentModified: Date;
	consentModifiedBy: string;
	contactAddresses: IContactAddressItem[];
	contactEComId: number;
	contactGLNId: number;
	contactPersons: number[];
	contractNr: string;
	created: Date;
	createdBy: string;
	creditLimit: number;
	currencyId: number;
	customerNr: string;
	customerProducts: ICustomerProductPriceSmallDTO[];
	customerUsers: ICustomerUserDTO[];
	deliveryConditionId: number;
	deliveryTypeId: number;
	departmentNr: string;
	disableInvoiceFee: boolean;
	discountMerchandise: number;
	discountService: number;
	files: IFileUploadDTO[];
	discount2Merchandise: number;
	discount2Service: number;
	finvoiceAddress: string;
	finvoiceOperator: string;
	gracePeriodDays: number;
	hasConsent: boolean;
	importInvoicesDetailed: boolean;
	invoiceDeliveryProvider: number;
	invoiceDeliveryType: number;
	invoiceLabel: string;
	invoicePaymentService: number;
	invoiceReference: string;
	isCashCustomer: boolean;
	isEUCountryBased: boolean;
	isFinvoiceCustomer: boolean;
	isOneTimeCustomer: boolean;
	isPrivatePerson: boolean;
	manualAccounting: boolean;
	modified: Date;
	modifiedBy: string;
	name: string;
	note: string;
	offerTemplate: number;
	orderContactEComId: number;
	orderTemplate: number;
	orgNr: string;
	payingCustomerId: number;
	paymentConditionId: number;
	paymentMorale: number;
	priceListTypeId: number;
	reminderContactEComId: number;
	showNote: boolean;
	state: SoeEntityState;
	supplierNr: string;
	sysCountryId: number;
	sysLanguageId: number;
	sysWholeSellerId: number;
	triangulationSales: boolean;
	vatNr: string;
	vatType: TermGroup_InvoiceVatType;
}
export interface ICustomerInvoiceAccountRowDTO {
	amount: number;
	amountCurrency: number;
	amountEntCurrency: number;
	amountLedgerCurrency: number;
	balance: number;
	creditAmount: number;
	creditAmountCurrency: number;
	creditAmountEntCurrency: number;
	creditAmountLedgerCurrency: number;
	debitAmount: number;
	debitAmountCurrency: number;
	debitAmountEntCurrency: number;
	debitAmountLedgerCurrency: number;
	dim1Disabled: boolean;
	dim1Id: number;
	dim1Mandatory: boolean;
	dim1ManuallyChanged: boolean;
	dim1Name: string;
	dim1Nr: string;
	dim1Stop: boolean;
	dim2Disabled: boolean;
	dim2Id: number;
	dim2Mandatory: boolean;
	dim2ManuallyChanged: boolean;
	dim2Name: string;
	dim2Nr: string;
	dim2Stop: boolean;
	dim3Disabled: boolean;
	dim3Id: number;
	dim3Mandatory: boolean;
	dim3ManuallyChanged: boolean;
	dim3Name: string;
	dim3Nr: string;
	dim3Stop: boolean;
	dim4Disabled: boolean;
	dim4Id: number;
	dim4Mandatory: boolean;
	dim4ManuallyChanged: boolean;
	dim4Name: string;
	dim4Nr: string;
	dim4Stop: boolean;
	dim5Disabled: boolean;
	dim5Id: number;
	dim5Mandatory: boolean;
	dim5ManuallyChanged: boolean;
	dim5Name: string;
	dim5Nr: string;
	dim5Stop: boolean;
	dim6Disabled: boolean;
	dim6Id: number;
	dim6Mandatory: boolean;
	dim6ManuallyChanged: boolean;
	dim6Name: string;
	dim6Nr: string;
	dim6Stop: boolean;
	invoiceAccountRowId: number;
	invoiceRowId: number;
	isContractorVatRow: boolean;
	isCreditRow: boolean;
	isDebitRow: boolean;
	isDeleted: boolean;
	isModified: boolean;
	isVatRow: boolean;
	parentRowId: number;
	quantity: number;
	rowNr: number;
	splitPercent: number;
	splitType: number;
	splitValue: number;
	state: SoeEntityState;
	tempInvoiceRowId: number;
	tempRowId: number;
	text: string;
	type: AccountingRowType;
}
export interface ICustomerInvoiceDTO extends IInvoiceDTO {
	addAttachementsToEInvoice: boolean;
	addSupplierInvoicesToEInvoice: boolean;
	billingAddressId: number;
	billingAdressText: string;
	billingInvoicePrinted: boolean;
	cashSale: boolean;
	centRounding: number;
	contractGroupId: number;
	customerBlockNote: string;
	customerInvoicePaymentService: number;
	customerInvoiceRows: ICustomerInvoiceRowDTO[];
	customerNameFromInvoice: string;
	deliveryAddressId: number;
	deliveryConditionId: number;
	deliveryDate: Date;
	deliveryDateText: string;
	deliveryTypeId: number;
	estimatedTime: number;
	externalDescription: string;
	externalId: string;
	fixedPriceOrder: boolean;
	freightAmount: number;
	freightAmountCurrency: number;
	freightAmountEntCurrency: number;
	freightAmountLedgerCurrency: number;
	hasHouseholdTaxDeduction: boolean;
	hasManuallyDeletedTimeProjectRows: boolean;
	hasOrder: boolean;
	includeOnInvoice: boolean;
	includeOnlyInvoicedTime: boolean;
	insecureDebt: boolean;
	internalDescription: string;
	invoiceDeliveryType: number;
	invoiceFee: number;
	invoiceFeeCurrency: number;
	invoiceFeeEntCurrency: number;
	invoiceFeeLedgerCurrency: number;
	invoiceHeadText: string;
	invoiceLabel: string;
	invoicePaymentService: number;
	invoiceText: string;
	marginalIncome: number;
	marginalIncomeCurrency: number;
	marginalIncomeEntCurrency: number;
	marginalIncomeLedgerCurrency: number;
	marginalIncomeRatio: number;
	multipleAssetRows: boolean;
	nextContractPeriodDate: Date;
	nextContractPeriodValue: number;
	nextContractPeriodYear: number;
	noOfReminders: number;
	orderDate: Date;
	orderType: TermGroup_OrderType;
	originateFrom: number;
	paymentConditionId: number;
	plannedStartDate: Date;
	plannedStopDate: Date;
	priceListTypeId: number;
	printTimeReport: boolean;
	priority: number;
	registrationType: OrderInvoiceRegistrationType;
	remainingTime: number;
	shiftTypeId: number;
	sumAmount: number;
	sumAmountCurrency: number;
	sumAmountEntCurrency: number;
	sumAmountLedgerCurrency: number;
	sysWholeSellerId: number;
	workingDescription: string;
}
export interface ICustomerInvoiceGridDTO {
	actorCustomerId: number;
	actorCustomerName: string;
	actorCustomerNr: string;
	actorCustomerNrName: string;
	attestStateNames: string;
	attestStates: ICustomerInvoiceRowAttestStateViewDTO[];
	bankFee: number;
	billingAddress: string;
	billingAddressId: number;
	billingInvoicePrinted: boolean;
	billingTypeId: number;
	billingTypeName: string;
	categories: string;
	contactEComId: number;
	contactEComText: string;
	contractGroupName: string;
	contractYearlyValue: number;
	contractYearlyValueExVat: number;
	created: Date;
	currencyCode: string;
	currencyRate: number;
	customerCategories: string;
	customerGracePeriodDays: number;
	customerInvoiceId: number;
	customerPaymentId: number;
	customerPaymentRowId: number;
	defaultDim2AccountId: number;
	defaultDim2AccountName: string;
	defaultDim3AccountId: number;
	defaultDim3AccountName: string;
	defaultDim4AccountId: number;
	defaultDim4AccountName: string;
	defaultDim5AccountId: number;
	defaultDim5AccountName: string;
	defaultDim6AccountId: number;
	defaultDim6AccountName: string;
	defaultDimAccountNames: string;
	deliverDateText: string;
	deliveryAddress: string;
	deliveryAddressId: number;
	deliveryDate: Date;
	deliveryType: number;
	deliveryTypeName: string;
	dueDate: Date;
	einvoiceDistStatus: number;
	exportStatus: number;
	exportStatusName: string;
	fixedPriceOrder: boolean;
	fixedPriceOrderName: string;
	fullyPaid: boolean;
	guid: System.IGuid;
	hasHouseholdTaxDeduction: boolean;
	hasInterest: boolean;
	hasVoucher: boolean;
	householdTaxDeductionType: number;
	infoIcon: number;
	insecureDebt: boolean;
	internalText: string;
	invoiceDate: Date;
	invoiceDeliveryProvider: number;
	invoiceDeliveryProviderName: string;
	invoiceHeadText: string;
	invoiceLabel: string;
	invoiceNr: string;
	invoicePaymentServiceId: number;
	invoicePaymentServiceName: string;
	isCashSales: boolean;
	isCashSalesText: string;
	isOverdued: boolean;
	isSelectDisabled: boolean;
	isTotalAmountPaid: boolean;
	lastCreatedReminder: Date;
	mainUserName: string;
	multipleAssetRows: boolean;
	myReadyState: number;
	nextContractPeriod: string;
	nextInvoiceDate: Date;
	noOfPrintedReminders: number;
	noOfReminders: number;
	onlyPayment: boolean;
	orderNumbers: string;
	orderReadyStatePercent: number;
	orderReadyStateText: string;
	orderType: number;
	orderTypeName: string;
	originType: number;
	ownerActorId: number;
	paidAmount: number;
	paidAmountCurrency: number;
	paidAmountCurrencyText: string;
	paidAmountText: string;
	payAmount: number;
	payAmountCurrency: number;
	payAmountCurrencyText: string;
	payAmountText: string;
	payDate: Date;
	paymentAmount: number;
	paymentAmountCurrency: number;
	paymentAmountDiff: number;
	paymentNr: string;
	paymentSeqNr: number;
	priceListName: string;
	projectName: string;
	projectNr: string;
	referenceOur: string;
	referenceYour: string;
	registrationType: number;
	remainingAmount: number;
	remainingAmountExVat: number;
	remainingAmountExVatText: string;
	remainingAmountText: string;
	reminderContactEComId: number;
	reminderContactEComText: string;
	seqNr: number;
	shiftTypeColor: string;
	shiftTypeName: string;
	status: number;
	statusIcon: number;
	statusName: string;
	sysCurrencyId: number;
	totalAmount: number;
	totalAmountCurrency: number;
	totalAmountCurrencyText: string;
	totalAmountExVat: number;
	totalAmountExVatCurrency: number;
	totalAmountExVatCurrencyText: string;
	totalAmountExVatText: string;
	totalAmountText: string;
	useClosedStyle: boolean;
	users: string;
	vATAmount: number;
	vATAmountCurrency: number;
	vatRate: number;
}
export interface ICustomerInvoiceIODTO {
	actorCompanyId: number;
	attestStateNames: string[];
	attestStates: number[];
	autoCreateCustomer: boolean;
	batchId: string;
	billingAddressAddress: string;
	billingAddressCity: string;
	billingAddressCO: string;
	billingAddressCountry: string;
	billingAddressName: string;
	billingAddressPostNr: string;
	billingType: number;
	billingTypeName: string;
	centRounding: number;
	claimAccountNr: string;
	claimAccountNrDim2: string;
	claimAccountNrDim3: string;
	claimAccountNrDim4: string;
	claimAccountNrDim5: string;
	claimAccountNrDim6: string;
	claimAccountNrSieDim1: string;
	claimAccountNrSieDim6: string;
	contractEndDay: number;
	contractEndMonth: number;
	contractEndYear: number;
	contractGroupDayInMonth: number;
	contractGroupDecription: string;
	contractGroupId: number;
	contractGroupInterval: number;
	contractGroupInvoiceRowText: string;
	contractGroupInvoiceTemplate: number;
	contractGroupInvoiceText: string;
	contractGroupName: string;
	contractGroupOrderTemplate: number;
	contractGroupPeriod: string;
	contractGroupPriceManagementName: string;
	contractNr: string;
	createAccountingInXE: boolean;
	created: Date;
	createdBy: string;
	createDeliveryAddressAsTextOnly: boolean;
	currency: string;
	currencyDate: Date;
	currencyId: number;
	currencyRate: number;
	customerExternalNr: string;
	customerGlnNr: string;
	customerId: number;
	customerInvoiceHeadIOId: number;
	customerInvoiceNr: string;
	customerName: string;
	customerNr: string;
	customerOrgnr: string;
	debetInvoiceNr: string;
	deliveryAddressAddress: string;
	deliveryAddressCity: string;
	deliveryAddressCO: string;
	deliveryAddressCountry: string;
	deliveryAddressName: string;
	deliveryAddressPostNr: string;
	deliveryCondition: string;
	deliveryConditionId: number;
	deliveryDate: Date;
	deliveryType: string;
	deliveryTypeId: number;
	dueDate: Date;
	email: string;
	errorMessage: string;
	externalDescription: string;
	externalId: string;
	freightAmount: number;
	freightAmountCurrency: number;
	fullyPayed: boolean;
	import: boolean;
	importHeadType: number;
	internalDescription: string;
	invoiceDate: Date;
	invoiceDeliveryType: number;
	invoiceFee: number;
	invoiceFeeCurrency: number;
	invoiceHeadText: string;
	invoiceId: number;
	invoiceLabel: string;
	invoiceState: number;
	isClosed: boolean;
	isModified: boolean;
	isSelected: boolean;
	language: string;
	modified: Date;
	modifiedBy: string;
	nextContractPeriodDate: Date;
	nextContractPeriodValue: number;
	nextContractPeriodYear: number;
	note: string;
	nrOfProductRows: number;
	ocr: string;
	offerNr: string;
	orderNr: string;
	orderType: number;
	originStatus: number;
	originType: number;
	paidAmount: number;
	paidAmountCurrency: number;
	paymentCondition: string;
	paymentConditionCode: string;
	paymentConditionId: number;
	paymentNr: string;
	priceListTypeId: number;
	projectId: number;
	projectNr: string;
	referenceOur: string;
	referenceYour: string;
	registrationType: number;
	remainingAmount: number;
	saleAccountNr: string;
	saleAccountNrDim2: string;
	saleAccountNrDim3: string;
	saleAccountNrDim4: string;
	saleAccountNrDim5: string;
	saleAccountNrDim6: string;
	saleAccountNrSieDim1: string;
	saleAccountNrSieDim6: string;
	seqNr: number;
	source: number;
	status: number;
	statusName: string;
	sumAmount: number;
	sumAmountCurrency: number;
	totalAmount: number;
	totalAmountCurrency: number;
	transferType: string;
	type: number;
	useFixedPriceArticle: boolean;
	vatAccountNr: string;
	vATAmount: number;
	vatAmount1: number;
	vatAmount2: number;
	vatAmount3: number;
	vATAmountCurrency: number;
	vatRate1: number;
	vatRate2: number;
	vatRate3: number;
	vatType: number;
	voucherDate: Date;
	voucherNr: string;
	workingDescription: string;
}
export interface ICustomerInvoiceRowAttestStateViewDTO {
	actorCompanyId: number;
	attestStateId: number;
	color: string;
	invoiceId: number;
	name: string;
	sort: number;
}
export interface ICustomerInvoiceRowDetailDTO {
	amountCurrency: number;
	attestStateColor: string;
	attestStateId: number;
	attestStateName: string;
	currencyCode: string;
	customerInvoiceRowId: number;
	discountType: number;
	discountValue: number;
	ediEntryId: number;
	ediTextValue: string;
	fromDate: Date;
	invoiceId: number;
	isExpenseRow: boolean;
	isTimeBillingRow: boolean;
	isTimeProjectRow: boolean;
	marginalIncomeLimit: number;
	previouslyInvoicedQuantity: number;
	productId: number;
	productName: string;
	productNr: string;
	productUnitCode: string;
	quantity: number;
	rowNr: number;
	sumAmountCurrency: number;
	text: string;
	toDate: Date;
	type: SoeInvoiceRowType;
}
export interface ICustomerInvoiceRowDTO {
	accountingRows: IAccountingRowDTO[];
	amount: number;
	amountCurrency: number;
	amountEntCurrency: number;
	amountFormula: string;
	amountLedgerCurrency: number;
	attestStateColor: string;
	attestStateId: number;
	attestStateName: string;
	created: Date;
	createdBy: string;
	currencyCode: string;
	customerInvoiceInterestId: number;
	customerInvoiceReminderId: number;
	customerInvoiceRowId: number;
	date: Date;
	dateTo: Date;
	deliveryDateText: string;
	detailVisible: boolean;
	discountAmount: number;
	discountAmountCurrency: number;
	discountAmountEntCurrency: number;
	discountAmountLedgerCurrency: number;
	discountPercent: number;
	discountType: number;
	discountValue: number;
	ediEntryId: number;
	ediTextValue: string;
	hasMultipleSalesRows: boolean;
	householdAmount: number;
	householdAmountCurrency: number;
	householdApartmentNbr: string;
	householdApplied: boolean;
	householdAppliedDate: Date;
	householdCooperativeOrgNbr: string;
	householdDeductionType: number;
	householdName: string;
	householdProperty: string;
	householdReceived: boolean;
	householdReceivedDate: Date;
	householdSocialSecNbr: string;
	houseHoldTaxDeductionType: number;
	intrastatCodeId: number;
	intrastatTransactionId: number;
	invoiceId: number;
	invoiceNr: string;
	invoiceQuantity: number;
	isCentRoundingRow: boolean;
	isClearingProduct: boolean;
	isContractProduct: boolean;
	isFixedPriceProduct: boolean;
	isFreightAmountRow: boolean;
	isHouseholdTextRow: boolean;
	isInterestRow: boolean;
	isInvoiceFeeRow: boolean;
	isLiftProduct: boolean;
	isLocked: boolean;
	isManuallyAdjusted: boolean;
	isModified: boolean;
	isReminderRow: boolean;
	isSelectDisabled: boolean;
	isSelected: boolean;
	isStockRow: boolean;
	isSupplementChargeProduct: boolean;
	isTimeProjectRow: boolean;
	marginalIncome: number;
	marginalIncomeCurrency: number;
	marginalIncomeEntCurrency: number;
	marginalIncomeLedgerCurrency: number;
	marginalIncomeLimit: number;
	marginalIncomeRatio: number;
	modified: Date;
	modifiedBy: string;
	originType: SoeOriginType;
	parentRowId: number;
	previouslyInvoicedQuantity: number;
	productId: number;
	productName: string;
	productNr: string;
	productUnitCode: string;
	productUnitId: number;
	projectId: number;
	purchasePrice: number;
	purchasePriceCurrency: number;
	purchasePriceEntCurrency: number;
	purchasePriceLedgerCurrency: number;
	quantity: number;
	rowNr: number;
	rowState: number;
	rowStateName: string;
	state: SoeEntityState;
	stockCode: string;
	stockId: number;
	sumAmount: number;
	sumAmountCurrency: number;
	sumAmountEntCurrency: number;
	sumAmountLedgerCurrency: number;
	supplementCharge: number;
	supplementChargePercent: number;
	supplierInvoiceId: number;
	sysCountryId: number;
	sysWholesellerName: string;
	targetRowId: number;
	tempRowId: number;
	text: string;
	timeManuallyChanged: boolean;
	timeManuallyChangedText: string;
	type: SoeInvoiceRowType;
	vatAccountEnabled: boolean;
	vatAccountId: number;
	vatAccountName: string;
	vatAccountNr: string;
	vatAmount: number;
	vatAmountCurrency: number;
	vatAmountEntCurrency: number;
	vatAmountLedgerCurrency: number;
	vatCodeCode: string;
	vatCodeId: number;
	vatRate: number;
}
export interface ICustomerInvoiceRowIODTO {
	accountDim2Nr: string;
	accountDim3Nr: string;
	accountDim4Nr: string;
	accountDim5Nr: string;
	accountDim6Nr: string;
	accountNr: string;
	accountSieDim1: string;
	accountSieDim6: string;
	actorCompanyId: number;
	amount: number;
	amountCurrency: number;
	attestState: number;
	attestStateName: string;
	batchId: string;
	categoryIds: number[];
	claimAccountNr: string;
	claimAccountNrDim2: string;
	claimAccountNrDim3: string;
	claimAccountNrDim4: string;
	claimAccountNrDim5: string;
	claimAccountNrDim6: string;
	claimAccountNrSieDim1: string;
	claimAccountNrSieDim6: string;
	created: Date;
	createdBy: string;
	customerInvoiceHeadIOId: number;
	customerInvoiceRowIOId: number;
	customerRowType: SoeInvoiceRowType;
	deliveryDateText: string;
	discount: number;
	discountAmount: number;
	discountAmountCurrency: number;
	errorMessage: string;
	externalId: string;
	import: boolean;
	importHeadType: number;
	invoiceId: number;
	invoiceNr: string;
	invoiceQuantity: number;
	invoiceRowId: number;
	isModified: boolean;
	isReadonly: boolean;
	isSelected: boolean;
	marginalIncome: number;
	marginalIncomeCurrency: number;
	modified: Date;
	modifiedBy: string;
	previouslyInvoicedQuantity: number;
	productGroupId: number;
	productId: number;
	productName: string;
	productName2: string;
	productNr: string;
	productUnitId: number;
	purchasePrice: number;
	purchasePriceCurrency: number;
	quantity: number;
	rowDate: Date;
	rowNr: number;
	rowStatus: string;
	source: number;
	state: number;
	status: number;
	statusName: string;
	stock: string;
	stockId: number;
	sumAmount: number;
	sumAmountCurrency: number;
	text: string;
	type: TermGroup_IOType;
	unit: string;
	unitPrice: number;
	vatAccountnr: string;
	vatAmount: number;
	vatAmountCurrency: number;
	vatCode: string;
	vatCodeId: number;
	vatRate: number;
}
export interface ICustomerInvoiceRowPurchaseDTO {
	attestStateColor: string;
	attestStateId: number;
	attestStatus: string;
	customerInvoiceRowId: number;
	deliveredPurchaseQuantity: number;
	deliveryDate: Date;
	invoicedQuantity: number;
	invoiceId: number;
	invoiceQuantity: number;
	invoiceSeqNr: number;
	invoiceStatus: number;
	productId: number;
	productNr: string;
	purchaseRowCount: number;
	purchaseRows: IPurchaseRowGridDTO[];
	quantity: number;
	text: string;
	unit: string;
}
export interface ICustomerInvoiceRowSmallDTO {
	amountCurrency: number;
	attestStateId: number;
	customerInvoiceRowId: number;
	deliveryDateText: string;
	ediEntryId: number;
	productId: number;
	quantity: number;
	rowNr: number;
	sumAmountCurrency: number;
	text: string;
	type: number;
	vATAmountCurrency: number;
	vatRate: number;
}
export interface ICustomerInvoiceSaveDTO {
	actorId: number;
	addAttachementsToEInvoice: boolean;
	addSupplierInvoicesToEInvoices: boolean;
	billingAddressId: number;
	billingAdressText: string;
	billingType: TermGroup_BillingType;
	cashSale: boolean;
	centRounding: number;
	checkConflictsOnSave: boolean;
	contactEComId: number;
	contactGLNId: number;
	contractGroupId: number;
	currencyDate: Date;
	currencyId: number;
	currencyRate: number;
	deliveryAddressId: number;
	deliveryConditionId: number;
	deliveryCustomerId: number;
	deliveryDate: Date;
	deliveryDateText: string;
	deliveryTypeId: number;
	dim1AccountId: number;
	dim2AccountId: number;
	dim3AccountId: number;
	dim4AccountId: number;
	dim5AccountId: number;
	dim6AccountId: number;
	dueDate: Date;
	estimatedTime: number;
	externalDescription: string;
	externalId: string;
	fixedPriceOrder: boolean;
	forceSave: boolean;
	freightAmount: number;
	freightAmountCurrency: number;
	fullyPayed: boolean;
	includeOnInvoice: boolean;
	includeOnlyInvoicedTime: boolean;
	internalDescription: string;
	invoiceDate: Date;
	invoiceDeliveryProvider: number;
	invoiceDeliveryType: number;
	invoiceFee: number;
	invoiceFeeCurrency: number;
	invoiceHeadText: string;
	invoiceId: number;
	invoiceLabel: string;
	invoiceNr: string;
	invoicePaymentService: number;
	invoiceText: string;
	isTemplate: boolean;
	keepAsPlanned: boolean;
	manuallyAdjustedAccounting: boolean;
	marginalIncome: number;
	marginalIncomeCurrency: number;
	marginalIncomeRatio: number;
	modified: Date;
	modifiedBy: string;
	nextContractPeriodDate: Date;
	nextContractPeriodValue: number;
	nextContractPeriodYear: number;
	ocr: string;
	orderDate: Date;
	orderNumbers: string;
	orderType: TermGroup_OrderType;
	originDescription: string;
	originStatus: SoeOriginStatus;
	paymentConditionId: number;
	plannedStartDate: Date;
	plannedStopDate: Date;
	prevInvoiceId: number;
	priceListTypeId: number;
	printTimeReport: boolean;
	priority: number;
	projectId: number;
	projectNr: string;
	referenceOur: string;
	referenceYour: string;
	registrationType: OrderInvoiceRegistrationType;
	remainingAmount: number;
	remainingTime: number;
	shiftTypeId: number;
	statusIcon: SoeStatusIcon;
	sumAmount: number;
	sumAmountCurrency: number;
	sysWholeSellerId: number;
	totalAmount: number;
	totalAmountCurrency: number;
	vatAmount: number;
	vatAmountCurrency: number;
	vatType: TermGroup_InvoiceVatType;
	voucherDate: Date;
	voucherHeadId: number;
	voucherSeriesId: number;
	workingDescription: string;
}
export interface ICustomerIODTO {
	accountsReceivableAccountInternal1: string;
	accountsReceivableAccountInternal2: string;
	accountsReceivableAccountInternal3: string;
	accountsReceivableAccountInternal4: string;
	accountsReceivableAccountInternal5: string;
	accountsReceivableAccountNr: string;
	accountsReceivableAccountSieDim1: string;
	accountsReceivableAccountSieDim6: string;
	actorCompanyId: number;
	agreementTemplate: number;
	batchId: string;
	billingAddress: string;
	billingAddresses: IContactAdressIODTO[];
	billingCoAddress: string;
	billingCountry: string;
	billingPostalAddress: string;
	billingPostalCode: string;
	billingTemplate: number;
	blockInvoice: boolean;
	blockNote: string;
	blockOrder: boolean;
	boardHQAddress: string;
	boardHQCountry: string;
	categoryCode1: string;
	categoryCode2: string;
	categoryCode3: string;
	categoryCode4: string;
	categoryCode5: string;
	categoryIds: number[];
	contactEmail: string;
	contactFirstName: string;
	contactLastName: string;
	country: string;
	created: Date;
	createdBy: string;
	creditLimit: number;
	currency: string;
	customerId: number;
	customerIOId: number;
	customerNr: string;
	customerState: number;
	defaultPriceListType: string;
	defaultPriceListTypeId: number;
	defaultWholeseller: string;
	deliveryAddress: string;
	deliveryCoAddress: string;
	deliveryCondition: string;
	deliveryCountry: string;
	deliveryMethod: string;
	deliveryPostalAddress: string;
	deliveryPostalCode: string;
	disableInvoiceFee: boolean;
	discountMerchandise: number;
	discountService: number;
	distributionAddress: string;
	distributionCoAddress: string;
	distributionCountry: string;
	distributionPostalAddress: string;
	distributionPostalCode: string;
	email1: string;
	email2: string;
	errorMessage: string;
	externalNbrs: string[];
	fax: string;
	finvoiceAddress: string;
	finvoiceOperator: string;
	gln: string;
	gLNNbrs: IContactEComIODTO[];
	gracePeriodDays: number;
	import: boolean;
	importHeadType: TermGroup_IOImportHeadType;
	importInvoiceDetailed: boolean;
	invoiceDeliveryEmail: string;
	invoiceDeliveryType: number;
	invoiceLabel: string;
	invoiceReference: string;
	isCashCustomer: boolean;
	isFinvoiceCustomer: boolean;
	isModified: boolean;
	isPrivatePerson: boolean;
	isSelected: boolean;
	language: string;
	manualAccounting: boolean;
	modified: Date;
	modifiedBy: string;
	name: string;
	note: string;
	offerTemplate: number;
	orderTemplate: number;
	orgNr: string;
	paymentCondition: string;
	phoneHome: string;
	phoneJob: string;
	phoneMobile: string;
	productPrices: ICustomerProductPriceIODTO[];
	salesAccountInternal1: string;
	salesAccountInternal2: string;
	salesAccountInternal3: string;
	salesAccountInternal4: string;
	salesAccountInternal5: string;
	salesAccountNr: string;
	salesAccountSieDim1: string;
	salesAccountSieDim6: string;
	showNote: boolean;
	source: TermGroup_IOSource;
	state: number;
	status: TermGroup_IOStatus;
	statusName: string;
	supplierNr: number;
	sysLanguageId: number;
	type: TermGroup_IOType;
	vATAccountNr: string;
	vATCodeNr: string;
	vatNr: string;
	vatType: number;
	vatTypeName: string;
	visitingAddress: string;
	visitingCoAddress: string;
	visitingCountry: string;
	visitingPostalAddress: string;
	visitingPostalCode: string;
	webpage: string;
}
export interface ICustomerLedgerSaveDTO {
	actorId: number;
	billingType: TermGroup_BillingType;
	cashSale: boolean;
	centRounding: number;
	currencyDate: Date;
	currencyId: number;
	currencyRate: number;
	dueDate: Date;
	freightAmount: number;
	freightAmountCurrency: number;
	fullyPayed: boolean;
	internalDescription: string;
	invoiceDate: Date;
	invoiceFee: number;
	invoiceFeeCurrency: number;
	invoiceId: number;
	invoiceNr: string;
	invoiceText: string;
	marginalIncome: number;
	marginalIncomeCurrency: number;
	marginalIncomeRatio: number;
	ocr: string;
	originDescription: string;
	originStatus: SoeOriginStatus;
	paidAmount: number;
	paidAmountCurrency: number;
	paymentConditionId: number;
	paymentNr: string;
	referenceOur: string;
	referenceYour: string;
	remainingAmount: number;
	seqNr: number;
	sumAmount: number;
	sumAmountCurrency: number;
	sysPaymentTypeId: number;
	totalAmount: number;
	totalAmountCurrency: number;
	vatAmount: number;
	vatAmountCurrency: number;
	vatCodeId: number;
	vatType: TermGroup_InvoiceVatType;
	voucherDate: Date;
	voucherSeriesId: number;
	voucherSeriesTypeId: number;
}
export interface ICustomerProductPriceSmallDTO {
	customerProductId: number;
	name: string;
	number: string;
	price: number;
	productId: number;
}
export interface ICustomerStatisticsDTO {
	attestStateColor: string;
	attestStateId: number;
	attestStateName: string;
	contractCategory: string;
	costCentre: string;
	currencyCode: string;
	customerCategory: string;
	customerCountry: string;
	customerName: string;
	customerPostalAddress: string;
	customerPostalCode: string;
	customerStreetAddress: string;
	date: Date;
	invoiceNr: string;
	orderCategory: string;
	orderNr: string;
	orderType: number;
	orderTypeName: string;
	originType: SoeOriginType;
	originUsers: string;
	originOwner: string;
	parentProductCategories: string;
	payingCustomerName: string;
	productCategory: string;
	productGroupId: number;
	productGroupName: string;
	productMarginalIncome: number;
	productMarginalRatio: number;
	productName: string;
	productNr: string;
	productPrice: number;
	productPurchaseAmount: number;
	productPurchasePrice: number;
	productQuantity: number;
	productSumAmount: number;
	productSumAmountCurrency: number;
	projectNr: string;
	referenceOur: string;
	timeCodeId: number;
	timeCodeName: string;
	wholeSellerName: string;
}
export interface ICustomerUserDTO {
	actorCompanyId: number;
	actorCustomerId: number;
	created: Date;
	createdBy: string;
	customerUserId: number;
	loginName: string;
	main: boolean;
	modified: Date;
	modifiedBy: string;
	name: string;
	state: SoeEntityState;
	userId: number;
}
export interface IDailyRecurrenceBase {
}
export interface IDailyRecurrenceDatesOutput {
	recurrenceDates: Date[];
	removedDates: Date[];
}
export interface IDailyRecurrencePatternDTO extends IDailyRecurrenceBase {
	dayOfMonth: number;
	daysOfWeek: DayOfWeek[];
	firstDayOfWeek: DayOfWeek;
	interval: number;
	month: number;
	sysHolidayTypeIds: number[];
	type: DailyRecurrencePatternType;
	weekIndex: DailyRecurrencePatternWeekIndex;
}
export interface IDailyRecurrenceRangeDTO extends IDailyRecurrenceBase {
	endDate: Date;
	numberOfOccurrences: number;
	startDate: Date;
	type: DailyRecurrenceRangeType;
}
export interface IDashboardStatisticPeriodDTO {
	dashboardStatisticsPeriodRowType: DashboardStatisticsRowType;
	from: Date;
	to: Date;
	value: number;
}
export interface IDashboardStatisticRowDTO {
	dashboardStatisticPeriods: IDashboardStatisticPeriodDTO[];
	dashboardStatisticsRowType: DashboardStatisticsRowType;
	name: string;
}
export interface IDashboardStatisticsDTO {
	dashboardStatisticRows: IDashboardStatisticRowDTO[];
	dashboardStatisticsType: DashboardStatisticsType;
	description: string;
	interval: TermGroup_PerformanceTestInterval;
	name: string;
}
export interface IDashboardStatisticType {
	dashboardStatisticsType: DashboardStatisticsType;
	decription: string;
	key: string;
	name: string;
}
export interface IDataStorageDTO {
	actorCompanyId: number;
	created: Date;
	createdBy: string;
	createdOrModified: Date;
	data: number[];
	dataStorageId: number;
	dataStorageRecipients: IDataStorageRecipientDTO[];
	dataStorageRecords: IDataStorageRecordDTO[];
	description: string;
	downloadURL: string;
	employeeId: number;
	exportDate: Date;
	extension: string;
	fileName: string;
	fileSize: number;
	folder: string;
	information: string;
	modified: Date;
	modifiedBy: string;
	name: string;
	needsConfirmation: boolean;
	originType: SoeDataStorageOriginType;
	parentDataStorageId: number;
	seqNr: number;
	state: SoeEntityState;
	timePeriodId: number;
	type: SoeDataStorageRecordType;
	userId: number;
	validFrom: Date;
	validTo: Date;
	xml: string;
}
export interface IDataStorageRecipientDTO {
	confirmedDate: Date;
	dataStorageId: number;
	dataStorageRecipientId: number;
	employeeNrAndName: string;
	readDate: Date;
	state: SoeEntityState;
	userId: number;
	userName: string;
}
export interface IDataStorageRecordDTO {
	attestStateColor: string;
	attestStateId: number;
	attestStateName: string;
	attestStatus: TermGroup_DataStorageRecordAttestStatus;
	currentAttestUsers: string;
	data: number[];
	dataStorageRecordId: number;
	entity: SoeEntityType;
	recordId: number;
	roleIds: number[];
	type: SoeDataStorageRecordType;
}
export interface IDateChart {
	data: IDateChartData[];
}
export interface IDateChartData {
	date: Date;
	values: IDateChartValue[];
}
export interface IDateChartValue {
	type: number;
	value: number;
}
export interface IDateRangeDTO {
	comment: string;
	minutes: number;
	start: Date;
	stop: Date;
}
export interface IDateRangeSelectionDTO extends IReportDataSelectionDTO {
	from: Date;
	id: number;
	rangeType: string;
	to: Date;
	useMinMaxIfEmpty: boolean;
}
export interface IDateSelectionDTO extends IReportDataSelectionDTO {
	date: Date;
	id: number;
}
export interface IDatesSelectionDTO extends IReportDataSelectionDTO {
	dates: Date[];
}
export interface IDayTypeDTO {
	actorCompanyId: number;
	created: Date;
	createdBy: string;
	dayTypeId: number;
	description: string;
	employeeGroups: IEmployeeGroupDTO[];
	import: boolean;
	modified: Date;
	modifiedBy: string;
	name: string;
	standardWeekdayFrom: number;
	standardWeekdayTo: number;
	state: SoeEntityState;
	sysDayTypeId: number;
	timeHalfdays: ITimeHalfdayDTO[];
	type: TermGroup_SysDayType;
	weekendSalary: boolean;
}
export interface IDeleteEmployeeDTO {
	action: DeleteEmployeeAction;
	employeeId: number;
	removeInfoAbsenceParentalLeave: boolean;
	removeInfoAbsenceSick: boolean;
	removeInfoAddress: boolean;
	removeInfoBankAccount: boolean;
	removeInfoClosestRelative: boolean;
	removeInfoEmail: boolean;
	removeInfoImage: boolean;
	removeInfoMeeting: boolean;
	removeInfoNote: boolean;
	removeInfoOtherContactInfo: boolean;
	removeInfoPhone: boolean;
	removeInfoSalaryDistress: boolean;
	removeInfoSkill: boolean;
	removeInfoUnionFee: boolean;
}
export interface IDeleteUserDTO {
	action: DeleteUserAction;
	disconnectEmployee: boolean;
	removeInfoAddress: boolean;
	removeInfoClosestRelative: boolean;
	removeInfoEmail: boolean;
	removeInfoOtherContactInfo: boolean;
	removeInfoPhone: boolean;
	userId: number;
}
export interface IDistributionCodeGridDTO {
	accountDim: string;
	distributionCodeHeadId: number;
	fromDate: Date;
	name: string;
	noOfPeriods: number;
	openingHour: string;
	subLevel: string;
	type: string;
	typeId: number;
	typeOfPeriod: string;
	typeOfPeriodId: number;
}
export interface IDistributionCodeHeadDTO {
	accountDimId: number;
	actorCompanyId: number;
	created: Date;
	createdBy: string;
	distributionCodeHeadId: number;
	fromDate: Date;
	isInUse: boolean;
	modified: Date;
	modifiedBy: string;
	name: string;
	noOfPeriods: number;
	openingHoursId: number;
	parentId: number;
	periods: IDistributionCodePeriodDTO[];
	subType: number;
	type: number;
}
export interface IDistributionCodePeriodDTO {
	comment: string;
	distributionCodePeriodId: number;
	isAdded: boolean;
	isModified: boolean;
	number: number;
	parentToDistributionCodePeriodId: number;
	percent: number;
	periodSubTypeName: string;
}
export interface IDocumentDTO {
	answerDate: Date;
	answerType: XEMailAnswerType;
	created: Date;
	createdBy: string;
	dataStorageId: number;
	description: string;
	displayName: string;
	extension: string;
	fileName: string;
	fileSize: number;
	folder: string;
	messageGroupIds: number[];
	messageId: number;
	modified: Date;
	modifiedBy: string;
	name: string;
	needsConfirmation: boolean;
	readDate: Date;
	recipients: IDataStorageRecipientDTO[];
	records: IDataStorageRecordDTO[];
	userId: number;
	validFrom: Date;
	validTo: Date;
}

export interface IDownloadFileDTO {
	success: boolean;
	fileName: string;
	content: string;
	fileType: string;
	errorMessage: string;
}
export interface IDrillDownReportDTO {
	accountName: string;
	accountNr: string;
	budgetAmount: number;
	closingBalance: number;
	groupName: string;
	headerName: string;
	openingBalance: number;
	periodAmount: number;
	periodVsBudgetDiff: number;
	periodVsYearDiff: number;
	previousYearAmount: number;
	voucherDescription: string;
	yearAmount: number;
}
export interface IDrilldownReportGridDTO {
	reportGroups: IReportGroupsDrilldownDTO[];
	reportId: number;
	reportName: string;
	reportNr: number;
	sysReportTemplateTypeId: number;
}
export interface IEdiConnectionDTO {
	ediConnectionId: number;
	wholesellerCustomerNr: string;
}
export interface IEdiEntryDTO {
	actorCompanyId: number;
	actorSupplierId: number;
	bankGiro: string;
	billingType: TermGroup_BillingType;
	buyerId: string;
	buyerReference: string;
	created: Date;
	createdBy: string;
	currencyDate: Date;
	currencyId: number;
	currencyRate: number;
	date: Date;
	dueDate: Date;
	ediEntryId: number;
	errorCode: number;
	fileName: string;
	hasPDF: boolean;
	iban: string;
	invoiceDate: Date;
	invoiceId: number;
	invoiceNr: string;
	invoiceStatus: TermGroup_EDIInvoiceStatus;
	messageType: TermGroup_EdiMessageType;
	modified: Date;
	modifiedBy: string;
	ocr: string;
	orderId: number;
	orderNr: string;
	orderStatus: TermGroup_EDIOrderStatus;
	pdf: number[];
	postalGiro: string;
	scanningEntryArrivalId: number;
	scanningEntryInvoice: IScanningEntryDTO;
	scanningEntryInvoiceId: number;
	sellerOrderNr: string;
	seqNr: number;
	state: SoeEntityState;
	status: TermGroup_EDIStatus;
	sum: number;
	sumCurrency: number;
	sumVat: number;
	sumVatCurrency: number;
	sysWholesellerId: number;
	type: TermGroup_EDISourceType;
	vatRate: number;
	wholesellerName: string;
	xml: string;
}
export interface IEdiEntryViewDTO {
	actorCompanyId: number;
	billingType: TermGroup_BillingType;
	billingTypeName: string;
	buyerId: string;
	buyerReference: string;
	created: Date;
	currencyCode: string;
	currencyId: number;
	currencyRate: number;
	customerId: number;
	customerName: string;
	customerNr: string;
	date: Date;
	dueDate: Date;
	ediEntryId: number;
	ediMessageType: TermGroup_EdiMessageType;
	ediMessageTypeName: string;
	errorCode: number;
	errorMessage: string;
	hasPdf: boolean;
	importSource: EdiImportSource;
	invoiceDate: Date;
	invoiceId: number;
	invoiceNr: string;
	invoiceStatus: TermGroup_EDIInvoiceStatus;
	invoiceStatusName: string;
	isModified: boolean;
	isSelectDisabled: boolean;
	isSelected: boolean;
	isVisible: boolean;
	langId: number;
	operatorMessage: string;
	orderId: number;
	orderNr: string;
	orderStatus: TermGroup_EDIOrderStatus;
	orderStatusName: string;
	roundedInterpretation: number;
	scanningEntryId: number;
	scanningMessageType: TermGroup_ScanningMessageType;
	scanningMessageTypeName: string;
	scanningStatus: TermGroup_ScanningStatus;
	sellerOrderNr: string;
	seqNr: number;
	sourceTypeName: string;
	state: SoeEntityState;
	status: TermGroup_EDIStatus;
	statusName: string;
	sum: number;
	sumCurrency: number;
	sumVat: number;
	sumVatCurrency: number;
	supplierAttestGroupId: number;
	supplierAttestGroupName: string;
	supplierId: number;
	supplierName: string;
	supplierNr: string;
	sysCurrencyId: number;
	type: TermGroup_EDISourceType;
	wholesellerId: number;
	wholesellerName: string;
}
export interface IEDITraceViewBase {
	ediEntryId: number;
	ediHasPdf: boolean;
	isEdi: boolean;
}
export interface IEmailTemplateDTO {
	actorCompanyId: number;
	body: string;
	bodyIsHTML: boolean;
	emailTemplateId: number;
	name: string;
	subject: string;
	type: number;
	typename: string;
}
export interface IEmployeeAccountDTO {
	accountId: number;
	addedOtherEmployeeAccount: boolean;
	children: IEmployeeAccountDTO[];
	created: Date;
	createdBy: string;
	dateFrom: Date;
	dateTo: Date;
	default: boolean;
	employeeAccountId: number;
	employeeId: number;
	mainAllocation: boolean;
	modified: Date;
	modifiedBy: string;
	parentEmployeeAccountId: number;
	state: SoeEntityState;
}
export interface IEmployeeAccumulatorDTO {
	accumulatorAccTodayDates: string;
	accumulatorAccTodayValue: number;
	accumulatorAmount: number;
	accumulatorDiff: number;
	accumulatorId: number;
	accumulatorName: string;
	accumulatorPeriodDates: string;
	accumulatorPeriodValue: number;
	accumulatorRuleMaxMinutes: number;
	accumulatorRuleMaxWarningMinutes: number;
	accumulatorRuleMinMinutes: number;
	accumulatorRuleMinWarningMinutes: number;
	accumulatorShowError: boolean;
	accumulatorShowWarning: boolean;
	accumulatorStatus: SoeTimeAccumulatorComparison;
	accumulatorStatusName: string;
	employeeId: number;
	employeeName: string;
	employeeNr: string;
	ownLimitDiff: number;
	ownLimitMax: number;
	ownLimitMin: number;
	ownLimitShowError: boolean;
	ownLimitStatus: SoeTimeAccumulatorComparison;
	ownLimitStatusName: string;
}
export interface IEmployeesAttestResult {
	attestStateToId: number;
	employeeResults: EmployeeAttestResult[];
	success: boolean;
}
export interface IEmployeeAttestResult {
	employeeId: number;
	numberAndName: string;
	success: boolean;
	noOfTranscationsAttested: number;
	noOfTranscationsFailed: number;
	noOfDaysFailed: number;
	noOfDaysWithStampingErrors: number;
	datesFailedString: string;
}
export interface IEmployeeCalculatedCostDTO {
	calculatedCostPerHour: number;
	employeeCalculatedCostId: number;
	employeeId: number;
	fromDate: Date;
	isDeleted: boolean;
	isModified: boolean;
	projectId: number;
}
export interface IEmployeeCalculateVacationResultDTO {
	created: Date;
	createdBy: string;
	employeeCalculateVacationResultHeadId: number;
	employeeCalculateVacationResultId: number;
	employeeId: number;
	error: string;
	formulaExtracted: string;
	formulaNames: string;
	formulaOrigin: string;
	formulaPlain: string;
	isDeleted: boolean;
	modified: Date;
	modifiedBy: string;
	name: string;
	state: SoeEntityState;
	success: boolean;
	type: number;
	value: number;
}
export interface IEmployeeCalculateVacationResultFlattenedDTO {
	actorCompanyId: number;
	bSTRAValue: number;
	created: Date;
	date: Date;
	dateStr: string;
	employeeCalculateVacationResultHeadId: number;
	employeeId: number;
	employeeName: string;
	employeeNr: string;
	employeeNrAndName: string;
	sSTRAValue: number;
	totValue: number;
	totVSTRValue: number;
	vBDValue: number;
	vBSTRValue: number;
	vFDValue: number;
	vIDValue: number;
	vISTRValue: number;
	vSDValue: number;
	vSSTRValue: number;
}
export interface IEmployeeCalculateVacationResultHeadDTO {
	actorCompanyId: number;
	created: Date;
	createdBy: string;
	date: Date;
	employeeCalculateVacationResultHeadId: number;
	employeeContainer: ICalculateVacationResultContainer[];
	modified: Date;
	modifiedBy: string;
	state: SoeEntityState;
}
export interface IEmployeeChildCareDTO {
	dateFrom: Date;
	dateTo: Date;
	daysLeft: number;
	name: string;
	nbrOfDays: number;
	openingBalanceUsedDays: number;
	usedDays: number;
	usedDaysText: string;
}
export interface IEmployeeChildDTO {
	birthDate: Date;
	employeeChildId: number;
	employeeId: number;
	firstName: string;
	lastName: string;
	name: string;
	openingBalanceUsedDays: number;
	singleCustody: boolean;
	state: SoeEntityState;
	usedDays: number;
	usedDaysText: string;
}
export interface IEmployeeCollectiveAgreementDTO {
	actorCompanyId: number;
	annualLeaveGroupId: number;
	annualLeaveGroupName: string;
	code: string;
	created: Date;
	createdBy: string;
	description: string;
	employeeCollectiveAgreementId: number;
	employeeGroupId: number;
	employeeGroupName: string;
	externalCode: string;
	modified: Date;
	modifiedBy: string;
	name: string;
	payrollGroupId: number;
	payrollGroupName: string;
	state: SoeEntityState;
	vacationGroupId: number;
	vacationGroupName: string;
}
export interface IEmployeeCollectiveAgreementGridDTO {
	code: string;
	description: string;
	employeeCollectiveAgreementId: number;
	employeeGroupName: string;
	employeeTemplateNames: string;
	externalCode: string;
	name: string;
	payrollGroupName: string;
	state: SoeEntityState;
	vacationGroupName: string;
}
export interface IEmployeeCSRExportDTO {
	csrExportDate: Date;
	csrImportDate: Date;
	employeeId: number;
	employeeName: string;
	employeeNr: string;
	employeeSocialSec: string;
	employeeTaxId: number;
	year: number;
}
export interface IEmployeeDatesDTO {
	dateRangeText: string;
	dates: Date[];
	employeeId: number;
	startDate: Date;
	stopDate: Date;
}
export interface IEmployeeDeviationAfterEmploymentDTO {
	employeeDates: IEmployeeDatesDTO;
	employeeId: number;
	employeeNr: string;
	employmentStopDate: Date;
	name: string;
	timePayrollTransactionIds: number[];
	timeSchedulePayrollTransactionIds: number[];
}
export interface IEmployeeDTO {
	absence105DaysExcluded: boolean;
	absence105DaysExcludedDays: number;
	activeString: string;
	actorCompanyId: number;
	calculatedCostPerHour: number;
	cardNumber: string;
	categoryNames: string[];
	categoryNamesString: string;
	contactPersonId: number;
	created: Date;
	createdBy: string;
	currentEmployeeGroupId: number;
	currentEmployeeGroupName: string;
	currentEmployeeGroupTimeDeviationCauseId: number;
	currentEmploymentDateFromString: string;
	currentEmploymentDateToString: string;
	currentEmploymentPercent: number;
	currentEmploymentTypeString: string;
	currentPayrollGroupId: number;
	currentPayrollGroupName: string;
	currentVacationGroupId: number;
	currentVacationGroupName: string;
	deleted: Date;
	deletedBy: string;
	disbursementAccountNr: string;
	disbursementAccountNrIsMissing: boolean;
	disbursementClearingNr: string;
	disbursementMethod: TermGroup_EmployeeDisbursementMethod;
	disbursementMethodIsCash: boolean;
	disbursementMethodIsUnknown: boolean;
	disbursementMethodName: string;
	disbursementCountryCode: string;
	disbursementBIC: string;
	disbursementIBAN: string;
	dontValidateDisbursementAccountNr: boolean;
	employeeGroupNames: string[];
	employeeGroupNamesString: string;
	employeeId: number;
	employeeNr: string;
	employeeNrSort: string;
	employeeTaxSE: IEmployeeTaxSEDTO;
	employeeVacationSE: IEmployeeVacationSEDTO;
	employmentDate: Date;
	employments: IEmploymentDTO[];
	endDate: Date;
	excludeFromPayroll: boolean;
	factors: IEmployeeFactorDTO[];
	finalSalaryAppliedTimePeriodId: number;
	finalSalaryEndDate: Date;
	finalSalaryEndDateApplied: Date;
	firstName: string;
	hidden: boolean;
	highRiskProtection: boolean;
	highRiskProtectionTo: Date;
	lastName: string;
	medicalCertificateDays: number;
	medicalCertificateReminder: boolean;
	modified: Date;
	modifiedBy: string;
	name: string;
	note: string;
	orderDate: Date;
	payrollGroupNames: string[];
	payrollGroupNamesString: string;
	projectDefaultTimeCodeId: number;
	roleNames: string[];
	roleNamesString: string;
	showNote: boolean;
	socialSec: string;
	state: SoeEntityState;
	taxSettingsAreMissing: boolean;
	timeCodeId: number;
	timeCodeName: string;
	timeDeviationCauseId: number;
	useFlexForce: boolean;
	userId: number;
	vacant: boolean;
}
export interface IEmployeeEarnedHolidayDTO {
	employeeId: number;
	employeeName: string;
	employeeNr: string;
	employeePercent: number;
	hasTransaction: boolean;
	hasTransactionString: string;
	suggestion: boolean;
	suggestionNote: string;
	suggestionString: string;
	work5DaysPerWeek: boolean;
	work5DaysPerWeekString: string;
}
export interface IEmployeeFactorDTO {
	employeeFactorId: number;
	factor: number;
	fromDate: Date;
	isCurrent: boolean;
	isReadOnly: boolean;
	type: TermGroup_EmployeeFactorType;
	typeName: string;
	vacationGroupId: number;
	vacationGroupName: string;
}
export interface IEmployeeGridDTO {
	accountNamesString: string;
	age: number;
	categoryNamesString: string;
	currentVacationGroupName: string;
	employeeGroupNamesString: string;
	employeeId: number;
	employeeNr: string;
	employmentEndDate: Date;
	employmentStart: Date;
	employmentStop: Date;
	employmentTypeString: string;
	name: string;
	payrollGroupNamesString: string;
	percent: number;
	roleNamesString: string;
	sex: TermGroup_Sex;
	sexString: string;
	socialSec: string;
	state: SoeEntityState;
	userBlockedFromDate: Date;
	vacant: boolean;
	workTimeWeek: number;
}
export interface IEmployeeGroupDTO {
	actorCompanyId: number;
	alwaysDiscardBreakEvaluation: boolean;
	autogenBreakOnStamping: boolean;
	autogenTimeblocks: boolean;
	breakDayMinutesAfterMidnight: number;
	created: Date;
	createdBy: string;
	dayTypesNames: string;
	deviationAxelStartHours: number;
	deviationAxelStopHours: number;
	employeeGroupId: number;
	externalCodes: string[];
	externalCodesString: string;
	invoiceProductAccountingPrio: string;
	keepStampsTogetherWithinMinutes: number;
	maxScheduleTimeFullTime: number;
	maxScheduleTimePartTime: number;
	maxScheduleTimeWithoutBreaks: number;
	mergeScheduleBreaksOnDay: boolean;
	minScheduleTimeFullTime: number;
	minScheduleTimePartTime: number;
	modified: Date;
	modifiedBy: string;
	name: string;
	payrollProductAccountingPrio: string;
	qualifyingDayCalculationRule: TermGroup_QualifyingDayCalculationRule;
	ruleRestTimeDay: number;
	ruleRestTimeWeek: number;
	ruleWorkTimeDayMaximumWeekend: number;
	ruleWorkTimeDayMaximumWorkDay: number;
	ruleWorkTimeDayMinimum: number;
	ruleWorkTimeWeek: number;
	ruleWorkTimeYear: number;
	state: SoeEntityState;
	timeCodeId: number;
	timeDeviationCause: ITimeDeviationCauseDTO;
	timeDeviationCauseId: number;
	timeDeviationCausesNames: string;
	timeReportType: number;
	timeReportTypeName: string;
}
export interface IEmployeeGroupSmallDTO {
	autogenTimeblocks: boolean;
	employeeGroupId: number;
	name: string;
	ruleWorkTimeWeek: number;
}
export interface IEmployeeListAvailabilityDTO {
	comment: string;
	minutes: number;
	start: Date;
	stop: Date;
}
export interface IEmployeeListDTO {
	absenceApproved: IDateRangeDTO[];
	absenceRequest: IDateRangeDTO[];
	accounts: IEmployeeAccountDTO[];
	active: boolean;
	annualScheduledTimeMinutes: number;
	annualWorkTimeMinutes: number;
	available: IEmployeeListAvailabilityDTO[];
	categoryRecords: ICompanyCategoryRecordDTO[];
	childPeriodBalanceTimeMinutes: number;
	childRuleWorkedTimeMinutes: number;
	currentDate: Date;
	description: string;
	employeeId: number;
	employeeNr: string;
	employeeNrSort: string;
	employeePostId: number;
	employeePostStatus: SoeEmployeePostStatus;
	employeeSchedules: IDateRangeDTO[];
	employeeSkills: IEmployeeSkillDTO[];
	employments: IEmployeeListEmploymentDTO[];
	firstName: string;
	groupName: string;
	hasAbsenceApproved: boolean;
	hasAbsenceRequest: boolean;
	hibernatingText: string;
	hidden: boolean;
	image: number[];
	imageSource: string;
	isGroupHeader: boolean;
	isSelected: boolean;
	lastName: string;
	name: string;
	parentPeriodBalanceTimeMinutes: number;
	parentRuleWorkedTimeMinutes: number;
	parentScheduledTimeMinutes: number;
	parentWorkedTimeMinutes: number;
	sex: TermGroup_Sex;
	templateSchedules: ITimeScheduleTemplateHeadSmallDTO[];
	unavailable: IEmployeeListAvailabilityDTO[];
	vacant: boolean;
}
export interface IEmployeeListEmploymentDTO {
	allowShiftsWithoutAccount: boolean;
	breakDayMinutesAfterMidnight: number;
	dateFrom: Date;
	dateTo: Date;
	employeeGroupId: number;
	employeeGroupName: string;
	isTemporaryPrimary: boolean;
	maxScheduleTime: number;
	minScheduleTime: number;
	percent: number;
	workTimeWeekMinutes: number;
	extraShiftAsDefault: boolean;
	annualLeaveGroupId: number;
}
export interface IEmployeeListSmallDTO {
	accounts: IEmployeeAccountDTO[];
	employeeId: number;
	employeeNr: string;
	employeeNrSort: string;
	firstName: string;
	hidden: boolean;
	lastName: string;
	name: string;
	userId: number;
	vacant: boolean;
}
export interface IEmployeeMeetingDTO {
	attestRoleIds: number[];
	completed: boolean;
	created: Date;
	createdBy: string;
	employeeCanEdit: boolean;
	employeeId: number;
	employeeMeetingId: number;
	followUpTypeId: number;
	followUpTypeName: string;
	modified: Date;
	modifiedBy: string;
	note: string;
	otherParticipants: string;
	participantIds: number[];
	participantNames: string;
	reminder: boolean;
	startTime: Date;
	state: SoeEntityState;
}
export interface IEmployeePeriodTimeSummary {
	employeeId: number;
	timePeriodId: number;
	parentScheduledTimeMinutes: number;
	parentWorkedTimeMinutes: number;
	parentRuleWorkedTimeMinutes: number;
	parentPeriodBalanceTimeMinutes: number;
	childScheduledTimeMinutes: number;
	childWorkedTimeMinutes: number;
	childRuleWorkedTimeMinutes: number;
	childPeriodBalanceTimeMinutes: number;
}
export interface IEmployeePositionDTO {
	default: boolean;
	employeeId: number;
	employeePositionId: number;
	employeePositionName: string;
	positionId: number;
	sysPositionCode: string;
	sysPositionDescription: string;
	sysPositionName: string;
}
export interface IEmployeePostDTO {
	accountId: number;
	accountName: string;
	actorCompanyId: number;
	created: Date;
	createdBy: string;
	dateFrom: Date;
	dateTo: Date;
	dayOfWeekIds: number[];
	dayOfWeeks: string;
	dayOfWeeksGenericType: ISmallGenericType[];
	dayOfWeeksGridString: string;
	description: string;
	employeeGroupDTO: IEmployeeGroupDTO;
	employeeGroupId: number;
	employeeGroupName: string;
	employeePostId: number;
	employeePostSkillDTOs: IEmployeePostSkillDTO[];
	employeePostWeekendType: TermGroup_EmployeePostWeekendType;
	freeDays: DayOfWeek[];
	hasMinMaxTimeSpan: boolean;
	ignoreDaysOfWeekIds: boolean;
	modified: Date;
	modifiedBy: string;
	name: string;
	overWriteDayOfWeekIds: number[];
	remainingWorkDaysWeek: number;
	scheduleCycleDTO: IScheduleCycleDTO;
	scheduleCycleId: number;
	skillNames: string;
	state: SoeEntityState;
	status: SoeEmployeePostStatus;
	validShiftTypes: IShiftTypeDTO[];
	workDaysWeek: number;
	workTimeCycle: number;
	workTimePercent: number;
	workTimePerDay: number;
	workTimeWeek: number;
	workTimeWeekMax: number;
	workTimeWeekMin: number;
}
export interface IEmployeePostSkillDTO {
	dateTo: Date;
	employeePostId: number;
	employeePostSkillId: number;
	skillDTO: ISkillDTO;
	skillId: number;
	skillLevel: number;
	skillLevelStars: number;
	skillLevelUnreached: boolean;
	skillName: string;
	skillTypeName: string;
}
export interface IEmployeeProjectInvoiceDTO {
	defaultTimeCodeId: number;
	employeeId: number;
	invoices: IProjectInvoiceSmallDTO[];
	projects: IProjectSmallDTO[];
}
export interface IEmployeeRequestDTO {
	accountNamesString: string;
	actorCompanyId: number;
	categoryNamesString: string;
	comment: string;
	created: Date;
	createdBy: string;
	createdString: string;
	employeeChildId: number;
	employeeChildName: string;
	employeeId: number;
	employeeName: string;
	employeeRequestId: number;
	extendedSettings: IExtendedAbsenceSettingDTO;
	intersectMessage: string;
	isSelected: boolean;
	modified: Date;
	modifiedBy: string;
	reActivate: boolean;
	requestIntersectsWithCurrent: boolean;
	resultStatus: TermGroup_EmployeeRequestResultStatus;
	resultStatusName: string;
	start: Date;
	startString: string;
	state: SoeEntityState;
	status: TermGroup_EmployeeRequestStatus;
	statusName: string;
	stop: Date;
	stopString: string;
	timeDeviationCauseId: number;
	timeDeviationCauseName: string;
	type: TermGroup_EmployeeRequestType;
}
export interface IEmployeeRequestsGaugeDTO {
	appliedDate: Date;
	employeeId: number;
	employeeName: string;
	employeeRequestType: TermGroup_EmployeeRequestType;
	employeeRequestTypeName: string;
	requestId: number;
	start: Date;
	status: number;
	statusName: string;
	stop: Date;
	timeDeviationCauseId: number;
	timeDeviationCauseName: string;
}
export interface IEmployeeScheduleDTO {
	created: Date;
	createdBy: string;
	employeeId: number;
	employeeScheduleId: number;
	isPreliminary: boolean;
	modified: Date;
	modifiedBy: string;
	startDate: Date;
	startDayNumber: number;
	state: SoeEntityState;
	stopDate: Date;
	timeScheduleTemplateHeadId: number;
}
export interface IEmployeeSchedulePlacementGridViewDTO {
	actorCompanyId: number;
	alwaysDiscardBreakEvaluation: boolean;
	autogenBreakOnStamping: boolean;
	autogenTimeblocks: boolean;
	breakDayMinutesAfterMidnight: number;
	employeeEndDate: Date;
	employeeFirstName: string;
	employeeGroupId: number;
	employeeGroupName: string;
	employeeGroupWorkTimeWeek: number;
	employeeId: number;
	employeeInfo: string;
	employeeLastName: string;
	employeeName: string;
	employeeNr: string;
	employeeNrSort: string;
	employeePosition: number;
	employeeScheduleId: number;
	employeeScheduleStartDate: Date;
	employeeScheduleStartDayNumber: number;
	employeeScheduleStopDate: Date;
	employeeWorkPercentage: number;
	employments: IEmploymentDTO[];
	isModified: boolean;
	isPersonalTemplate: boolean;
	isPlaced: boolean;
	isPreliminary: boolean;
	isSelected: boolean;
	isVisible: boolean;
	keepStampsTogetherWithinMinutes: number;
	mergeScheduleBreaksOnDay: boolean;
	templateEmployeeId: number;
	templateStartDate: Date;
	timeScheduleTemplateHeadId: number;
	timeScheduleTemplateHeadName: string;
	timeScheduleTemplateHeadNoOfDays: number;
}
export interface IEmployeeScheduleTransactionInfoDTO {
	autoGenTimeAndBreakForProject: boolean;
	date: Date;
	employeeGroupId: number;
	employeeId: number;
	scheduleBlocks: IProjectTimeBlockDTO[];
	timeBlocks: IProjectTimeBlockDTO[];
	timeDeviationCauseId: number;
}
export interface IEmployeeSelectionDTO extends IReportDataSelectionDTO {
	accountIds: number[];
	accountingType: TermGroup_EmployeeSelectionAccountingType;
	categoryIds: number[];
	doValidateEmployment: boolean;
	employeeGroupIds: number[];
	employeeIds: number[];
	employeeNrs: string[];
	employeePostIds: number[];
	includeEnded: boolean;
	includeHidden: boolean;
	includeInactive: boolean;
	includeSecondary: boolean;
	includeVacant: boolean;
	isEmployeePost: boolean;
	onlyInactive: boolean;
	payrollGroupIds: number[];
	vacationGroupIds: number[];
}
export interface IEmployeeSettingDTO {
	actorCompanyId: number;
	boolData: boolean;
	created: Date;
	createdBy: string;
	dataType: SettingDataType;
	dateData: Date;
	decimalData: number;
	employeeId: number;
	employeeSettingId: number;
	employeeSettingAreaType: TermGroup_EmployeeSettingType;
	employeeSettingGroupType: TermGroup_EmployeeSettingType;
	employeeSettingType: TermGroup_EmployeeSettingType;
	intData: number;
	modified: Date;
	modifiedBy: string;
	name: string;
	state: SoeEntityState;
	strData: string;
	timeData: Date;
	validFromDate: Date;
	validToDate: Date;
}
export interface IEmployeeSettingTypeDTO {
	dataType: SettingDataType;
	employeeSettingAreaType: TermGroup_EmployeeSettingType;
	employeeSettingGroupType: TermGroup_EmployeeSettingType;
	employeeSettingType: TermGroup_EmployeeSettingType;
	maxLength: number;
	name: string;
	options: ISmallGenericType[];
}
export interface IEmployeeSkillDTO {
	dateTo: Date;
	employeeId: number;
	employeeSkillId: number;
	skillId: number;
	skillLevel: number;
	skillLevelStars: number;
	skillLevelUnreached: boolean;
	skillName: string;
	skillTypeName: string;
}
export interface IEmployeeSmallDTO {
	employeeId: number;
	employeeNr: string;
	name: string;
}
export interface IEmployeeStatisticsChartData {
	color: string;
	date: Date;
	toolTip: string;
	value: number;
}
export interface IEmployeeTaxSEDTO {
	adjustmentPeriodFrom: Date;
	adjustmentPeriodTo: Date;
	adjustmentType: TermGroup_EmployeeTaxAdjustmentType;
	adjustmentValue: number;
	applyEmploymentTaxMinimumRule: boolean;
	birthPlace: string;
	countryCode: string;
	countryCodeBirthPlace: string;
	countryCodeCitizen: string;
	created: Date;
	createdBy: string;
	csrExportDate: Date;
	csrImportDate: Date;
	employeeId: number;
	employeeTaxId: number;
	employmentAbroadCode: TermGroup_EmployeeTaxEmploymentAbroadCode;
	employmentTaxType: TermGroup_EmployeeTaxEmploymentTaxType;
	estimatedAnnualSalary: number;
	firstEmployee: boolean;
	mainEmployer: boolean;
	modified: Date;
	modifiedBy: string;
	oneTimeTaxPercent: number;
	regionalSupport: boolean;
	salaryDistressAmount: number;
	salaryDistressAmountType: TermGroup_EmployeeTaxSalaryDistressAmountType;
	salaryDistressCase: string;
	salaryDistressReservedAmount: number;
	schoolYouthLimitInitial: number;
	sinkType: TermGroup_EmployeeTaxSinkType;
	state: SoeEntityState;
	taxRate: number;
	taxRateColumn: number;
	tinNumber: string;
	type: TermGroup_EmployeeTaxType;
	typeName: string;
	year: number;
}
export interface IEmployeeTemplateDisbursementAccountDTO {
	accountNr: string;
	clearingNr: string;
	dontValidateAccountNr: boolean;
	method: number;
}
export interface IEmployeeTemplateDTO {
	actorCompanyId: number;
	code: string;
	created: Date;
	createdBy: string;
	description: string;
	employeeCollectiveAgreementId: number;
	employeeTemplateGroups: IEmployeeTemplateGroupDTO[];
	employeeTemplateId: number;
	externalCode: string;
	modified: Date;
	modifiedBy: string;
	name: string;
	state: SoeEntityState;
	title: string;
}
export interface IEmployeeTemplateEmployeeAccountDTO {
	accountDimId: number;
	accountId: number;
	childAccountId: number;
	dateFrom: Date;
	dateFromString: string;
	dateTo: Date;
	dateToString: string;
	default: boolean;
	mainAllocation: boolean;
	subChildAccountId: number;
}
export interface IEmployeeTemplateEmploymentPriceTypeDTO {
	amount: number;
	fromDate: Date;
	fromDateString: string;
	payrollGroupAmount: number;
	payrollLevelId: number;
	payrollPriceTypeId: number;
	payrollPriceTypeName: string;
}
export interface IEmployeeTemplateGridDTO {
	code: string;
	description: string;
	employeeCollectiveAgreementName: string;
	employeeTemplateId: number;
	externalCode: string;
	name: string;
	state: SoeEntityState;
}
export interface IEmployeeTemplateGroupDTO {
	code: string;
	created: Date;
	createdBy: string;
	description: string;
	employeeTemplateGroupId: number;
	employeeTemplateGroupRows: IEmployeeTemplateGroupRowDTO[];
	employeeTemplateId: number;
	modified: Date;
	modifiedBy: string;
	name: string;
	newPageBefore: boolean;
	sortOrder: number;
	state: SoeEntityState;
	type: TermGroup_EmployeeTemplateGroupType;
}
export interface IEmployeeTemplateGroupRowDTO {
	comment: string;
	created: Date;
	createdBy: string;
	defaultValue: string;
	employeeTemplateGroupId: number;
	employeeTemplateGroupRowId: number;
	entity: SoeEntityType;
	format: string;
	hideInEmploymentRegistration: boolean;
	hideInRegistration: boolean;
	hideInReport: boolean;
	hideInReportIfEmpty: boolean;
	mandatoryLevel: number;
	modified: Date;
	modifiedBy: string;
	recordId: number;
	registrationLevel: number;
	row: number;
	spanColumns: number;
	startColumn: number;
	state: SoeEntityState;
	title: string;
	type: TermGroup_EmployeeTemplateGroupRowType;
}
export interface IEmployeeTemplatePositionDTO {
	default: boolean;
	positionId: number;
}
export interface IEmployeeTimeCodeDTO extends IEmployeeSmallDTO {
	autoGenTimeAndBreakForProject: boolean;
	defaultTimeCodeId: number;
	employeeGroupId: number;
	timeDeviationCauseId: number;
}
export interface IEmployeeTimePeriodDTO {
	actorCompanyId: number;
	created: Date;
	createdBy: string;
	employeeId: number;
	employeeTimePeriodId: number;
	modified: Date;
	modifiedBy: string;
	salarySpecificationPublishDate: Date;
	status: SoeEmployeeTimePeriodStatus;
	timePeriodId: number;
}
export interface IEmployeeTimeWorkAccountDTO {
	actorCompanyId: number;
	dateFrom: Date;
	dateTo: Date;
	employeeId: number;
	employeeTimeWorkAccountId: number;
	state: SoeEntityState;
	timeWorkAccountId: number;
	timeWorkAccountName: string;
}
export interface IEmployeeUnionFeeDTO {
	employeeId: number;
	employeeUnionFeeId: number;
	fromDate: Date;
	state: SoeEntityState;
	toDate: Date;
	unionFeeId: number;
	unionFeeName: string;
}
export interface IEmployeeUserDTO {
	absence105DaysExcluded: boolean;
	absence105DaysExcludedDays: number;
	accounts: IEmployeeAccountDTO[];
	actorCompanyId: number;
	actorContactPersonId: number;
	aFACategory: number;
	aFAParttimePensionCode: boolean;
	aFASpecialAgreement: number;
	aFAWorkplaceNr: string;
	aGIPlaceOfEmploymentAddress: string;
	aGIPlaceOfEmploymentCity: string;
	aGIPlaceOfEmploymentIgnore: boolean;
	attestRoleIds: number[];
	benefitAsPension: boolean;
	blockedFromDate: Date;
	bygglosenAgreementArea: string;
	bygglosenAllocationNumber: string;
	bygglosenMunicipalCode: string;
	bygglosenSalaryFormula: number;
	bygglosenSalaryFormulaName: string;
	bygglosenWorkPlace: string;
	bygglosenProfessionCategory: string;
	bygglosenLendedToOrgNr: string;
	bygglosenSalaryType: number;
	bygglosenAgreedHourlyPayLevel: number;
	calculatedCosts: IEmployeeCalculatedCostDTO[];
	cardNumber: string;
	categoryId: number;
	categoryRecords: ICompanyCategoryRecordDTO[];
	changePassword: boolean;
	childCares: IEmployeeChildCareDTO[];
	clearScheduleFrom: Date;
	collectumAgreedOnProduct: string;
	collectumCancellationDate: Date;
	collectumCancellationDateIsLeaveOfAbsence: boolean;
	collectumCostPlace: string;
	collectumITPPlan: number;
	created: Date;
	createdBy: string;
	currentEmployeeGroupId: number;
	defaultActorCompanyId: number;
	disbursementAccountNr: string;
	disbursementClearingNr: string;
	disbursementCountryCode: string;
	disbursementBIC: string;
	disbursementIBAN: string;
	disbursementMethod: TermGroup_EmployeeDisbursementMethod;
	disconnectExistingEmployee: boolean;
	disconnectExistingUser: boolean;
	dontNotifyChangeOfAttestState: boolean;
	dontNotifyChangeOfDeviations: boolean;
	dontValidateDisbursementAccountNr: boolean;
	email: string;
	emailCopy: boolean;
	employeeChilds: IEmployeeChildDTO[];
	employeeId: number;
	employeeMeetings: IEmployeeMeetingDTO[];
	employeeNr: string;
	employeeNrAndName: string;
	employeeSettings: IEmployeeSettingDTO[];
	employeeSkills: IEmployeeSkillDTO[];
	employeeTemplateId: number;
	employeeTemplateName: string;
	employeeTimeWorkAccounts: IEmployeeTimeWorkAccountDTO[];
	employeeVacationSE: IEmployeeVacationSEDTO;
	employmentDate: Date;
	employments: IEmploymentDTO[];
	endDate: Date;
	estatusLoginId: string;
	excludeFromPayroll: boolean;
	externalAuthId: string;
	externalAuthIdModified: boolean;
	externalCode: string;
	extraFieldRecords: IExtraFieldRecordDTO[];
	factors: IEmployeeFactorDTO[];
	firstName: string;
	found: boolean;
	gtpAgreementNumber: number;
	gtpExcluded: boolean;
	hasNotAttestRoleToSeeEmployee: boolean;
	highRiskProtection: boolean;
	highRiskProtectionTo: Date;
	iFAssociationNumber: number;
	iFPaymentCode: number;
	iFWorkPlace: string;
	isEmployeeMeetingsChanged: boolean;
	isEmploymentsChanged: boolean;
	isMobileUser: boolean;
	isPayrollUpdated: boolean;
	isTemplateGroupsChanged: boolean;
	kpaAgreementType: number;
	kpaBelonging: number;
	kpaEndCode: number;
	kpaRetirementAge: number;
	langId: number;
	lastName: string;
	licenseId: number;
	lifetimeSeconds: number;
	lifetimeSecondsModified: boolean;
	loginName: string;
	medicalCertificateDays: number;
	medicalCertificateReminder: boolean;
	modified: Date;
	modifiedBy: string;
	name: string;
	newPassword: string;
	note: string;
	parentalLeaves: IEmployeeChildDTO[];
	partnerInCloseCompany: boolean;
	password: number[];
	passwordHomePage: string;
	payrollReportsCFARNumber: number;
	payrollReportsPersonalCategory: number;
	payrollReportsSalaryType: number;
	payrollReportsWorkPlaceNumber: number;
	payrollReportsWorkTimeCategory: number;
	portraitConsent: boolean;
	portraitConsentDate: Date;
	saveEmployee: boolean;
	saveUser: boolean;
	sex: TermGroup_Sex;
	showNote: boolean;
	socialSec: string;
	state: SoeEntityState;
	templateGroups: ITimeScheduleTemplateGroupEmployeeDTO[];
	tempTaxRate: number;
	timeCodeId: number;
	timeDeviationCauseId: number;
	unionFees: IEmployeeUnionFeeDTO[];
	useFlexForce: boolean;
	userId: number;
	userLinkConnectionKey: string;
	userRoles: IUserRolesDTO[];
	vacant: boolean;
	wantsExtraShifts: boolean;
	workPlaceSCB: string;
}
export interface IEmployeeVacationPeriodDTO {
	calculateHours: boolean;
	calculateHoursDayFactor: number;
	daysAdvance: number;
	daysPaid: number;
	daysSaved: number;
	daysSum: number;
	daysUnpaid: number;
	earnedDaysRemainingHoursAdvance: number;
	earnedDaysRemainingHoursOverdue: number;
	earnedDaysRemainingHoursPaid: number;
	earnedDaysRemainingHoursUnpaid: number;
	earnedDaysRemainingHoursYear1: number;
	earnedDaysRemainingHoursYear2: number;
	earnedDaysRemainingHoursYear3: number;
	earnedDaysRemainingHoursYear4: number;
	earnedDaysRemainingHoursYear5: number;
	employeeId: number;
	periodDaysAdvance: number;
	periodDaysOverdue: number;
	periodDaysPaid: number;
	periodDaysSavedYear1: number;
	periodDaysSavedYear2: number;
	periodDaysSavedYear3: number;
	periodDaysSavedYear4: number;
	periodDaysSavedYear5: number;
	periodDaysUnpaid: number;
	periodVacationCompensationPaidCount: number;
	periodVacationCompensationSavedCount: number;
	remainingDaysAdvance: number;
	remainingDaysOverdue: number;
	remainingDaysPaid: number;
	remainingDaysUnpaid: number;
	remainingDaysYear1: number;
	remainingDaysYear2: number;
	remainingDaysYear3: number;
	remainingDaysYear4: number;
	remainingDaysYear5: number;
	timePeriodId: number;
}
export interface IEmployeeVacationPrelUsedDaysDTO {
	details: string;
	employeeId: number;
	isHours: boolean;
	sum: number;
}
export interface IEmployeeVacationSEDTO {
	adjustmentDate: Date;
	created: Date;
	createdBy: string;
	debtInAdvanceAmount: number;
	debtInAdvanceDelete: boolean;
	debtInAdvanceDueDate: Date;
	earnedDaysAdvance: number;
	earnedDaysPaid: number;
	earnedDaysRemainingHoursAdvance: number;
	earnedDaysRemainingHoursOverdue: number;
	earnedDaysRemainingHoursPaid: number;
	earnedDaysRemainingHoursUnpaid: number;
	earnedDaysRemainingHoursYear1: number;
	earnedDaysRemainingHoursYear2: number;
	earnedDaysRemainingHoursYear3: number;
	earnedDaysRemainingHoursYear4: number;
	earnedDaysRemainingHoursYear5: number;
	earnedDaysUnpaid: number;
	employeeId: number;
	employeeVacationSEId: number;
	employmentRateOverdue: number;
	employmentRatePaid: number;
	employmentRateYear1: number;
	employmentRateYear2: number;
	employmentRateYear3: number;
	employmentRateYear4: number;
	employmentRateYear5: number;
	modified: Date;
	modifiedBy: string;
	paidVacationAllowance: number;
	paidVacationVariableAllowance: number;
	prelPayedDaysYear1: number;
	remainingDaysAdvance: number;
	remainingDaysAllowanceYear1: number;
	remainingDaysAllowanceYear2: number;
	remainingDaysAllowanceYear3: number;
	remainingDaysAllowanceYear4: number;
	remainingDaysAllowanceYear5: number;
	remainingDaysAllowanceYearOverdue: number;
	remainingDaysOverdue: number;
	remainingDaysPaid: number;
	remainingDaysUnpaid: number;
	remainingDaysVariableAllowanceYear1: number;
	remainingDaysVariableAllowanceYear2: number;
	remainingDaysVariableAllowanceYear3: number;
	remainingDaysVariableAllowanceYear4: number;
	remainingDaysVariableAllowanceYear5: number;
	remainingDaysVariableAllowanceYearOverdue: number;
	remainingDaysYear1: number;
	remainingDaysYear2: number;
	remainingDaysYear3: number;
	remainingDaysYear4: number;
	remainingDaysYear5: number;
	savedDaysOverdue: number;
	savedDaysYear1: number;
	savedDaysYear2: number;
	savedDaysYear3: number;
	savedDaysYear4: number;
	savedDaysYear5: number;
	state: SoeEntityState;
	totalRemainingDays: number;
	totalRemainingHours: number;
	usedDaysAdvance: number;
	usedDaysOverdue: number;
	usedDaysPaid: number;
	usedDaysUnpaid: number;
	usedDaysYear1: number;
	usedDaysYear2: number;
	usedDaysYear3: number;
	usedDaysYear4: number;
	usedDaysYear5: number;
}
export interface IEmployeeVehicleDeductionDTO {
	created: Date;
	createdBy: string;
	employeeVehicleDeductionId: number;
	employeeVehicleId: number;
	fromDate: Date;
	modified: Date;
	modifiedBy: string;
	price: number;
	state: SoeEntityState;
}
export interface IEmployeeVehicleDTO {
	actorCompanyId: number;
	benefitValueAdjustment: number;
	codeForComparableModel: string;
	comparablePrice: number;
	created: Date;
	createdBy: string;
	deduction: IEmployeeVehicleDeductionDTO[];
	employeeId: number;
	employeeVehicleId: number;
	equipment: IEmployeeVehicleEquipmentDTO[];
	fromDate: Date;
	fuelType: TermGroup_SysVehicleFuelType;
	hasExtensiveDriving: boolean;
	licensePlateNumber: string;
	modelCode: string;
	modified: Date;
	modifiedBy: string;
	price: number;
	priceAdjustment: number;
	registeredDate: Date;
	state: SoeEntityState;
	sysVehicleTypeId: number;
	tax: IEmployeeVehicleTaxDTO[];
	taxableValue: number;
	toDate: Date;
	type: TermGroup_VehicleType;
	vehicleMake: string;
	vehicleModel: string;
	year: number;
}
export interface IEmployeeVehicleEquipmentDTO {
	created: Date;
	createdBy: string;
	description: string;
	employeeVehicleEquipmentId: number;
	employeeVehicleId: number;
	fromDate: Date;
	modified: Date;
	modifiedBy: string;
	price: number;
	state: SoeEntityState;
	toDate: Date;
}
export interface IEmployeeVehicleGridDTO {
	employeeId: number;
	employeeName: string;
	employeeNr: string;
	employeeVehicleId: number;
	equipmentSum: number;
	fromDate: Date;
	licensePlateNumber: string;
	netSalaryDeduction: number;
	price: number;
	taxableValue: number;
	toDate: Date;
	vehicleMakeAndModel: string;
}
export interface IEmployeeVehicleTaxDTO {
	amount: number;
	created: Date;
	createdBy: string;
	employeeVehicleId: number;
	employeeVehicleTaxId: number;
	fromDate: Date;
	modified: Date;
	modifiedBy: string;
	state: SoeEntityState;
}
export interface IEmploymentChangeDTO {
	actorCompanyId: number;
	comment: string;
	created: Date;
	createdBy: string;
	employeeId: number;
	employmentChangeId: number;
	employmentId: number;
	fieldType: TermGroup_EmploymentChangeFieldType;
	fieldTypeName: string;
	fromDate: Date;
	fromValue: string;
	fromValueName: string;
	isDeleted: boolean;
	state: SoeEntityState;
	toDate: Date;
	toValue: string;
	toValueName: string;
	type: TermGroup_EmploymentChangeType;
}
export interface IEmploymentDTO {
	accountingSettings: IAccountingSettingsRowDTO[];
	actorCompanyId: number;
	baseWorkTimeWeek: number;
	calculatedExperienceMonths: number;
	changes: IEmploymentChangeDTO[];
	comment: string;
	currentApplyChangeDate: Date;
	currentChangeDateFrom: Date;
	currentChangeDateTo: Date;
	currentChanges: IEmploymentChangeDTO[];
	dateFrom: Date;
	dateTo: Date;
	employeeGroupId: number;
	employeeGroupName: string;
	employeeGroupTimeCodes: number[];
	employeeGroupWorkTimeWeek: number;
	employeeId: number;
	employeeName: string;
	employmentEndReason: number;
	employmentEndReasonName: string;
	employmentId: number;
	employmentType: number;
	employmentTypeName: string;
	employmentVacationGroup: IEmploymentVacationGroupDTO[];
	experienceAgreedOrEstablished: boolean;
	experienceMonths: number;
	externalCode: string;
	finalSalaryStatus: SoeEmploymentFinalSalaryStatus;
	fixedAccounting: boolean;
	hibernatingTimeDeviationCauseId: number;
	isAddingEmployment: boolean;
	isChangingEmployment: boolean;
	isChangingEmploymentDates: boolean;
	isDeletingEmployment: boolean;
	isNewFromCopy: boolean;
	isReadOnly: boolean;
	isSecondaryEmployment: boolean;
	isTemporaryPrimary: boolean;
	name: string;
	payrollGroupId: number;
	payrollGroupName: string;
	percent: number;
	priceTypes: IEmploymentPriceTypeDTO[];
	specialConditions: string;
	state: SoeEntityState;
	substituteFor: string;
	substituteForDueTo: string;
	uniqueId: string;
	updateExperienceMonthsReminder: boolean;
	workPlace: string;
	workTasks: string;
	workTimeWeek: number;
	fullTimeWorkTimeWeek: number;
	excludeFromWorkTimeWeekCalculationOnSecondaryEmployment?: boolean;
}
export interface IEmploymentPriceTypeChangeDTO {
	amount: number;
	code: string;
	fromDate: Date;
	isPayrollGroupPriceType: boolean;
	isSecondaryEmployment: boolean;
	name: string;
	payrollGroupAmount: number;
	payrollLevelId: number;
	payrollLevelName: string;
	payrollPriceType: TermGroup_SoePayrollPriceType;
	payrollPriceTypeId: number;
	toDate: Date;
}
export interface IEmploymentPriceTypeDTO {
	code: string;
	employeeId: number;
	employmentId: number;
	employmentPriceTypeId: number;
	isPayrollGroupPriceType: boolean;
	name: string;
	payrollGroupAmount: number;
	payrollGroupAmountDate: Date;
	payrollPriceType: TermGroup_SoePayrollPriceType;
	payrollPriceTypeId: number;
	periods: IEmploymentPriceTypePeriodDTO[];
	readOnly: boolean;
	sort: number;
	type: IPayrollPriceTypeDTO;
}
export interface IEmploymentPriceTypePeriodDTO {
	amount: number;
	employmentPriceTypeId: number;
	employmentPriceTypePeriodId: number;
	fromDate: Date;
	hidden: boolean;
	payrollLevelId: number;
	payrollLevelName: string;
}
export interface IEmploymentTypeSmallDTO {
	active: boolean;
	id: number;
	name: string;
	type: number;
}
export interface IEmploymentVacationGroupDTO {
	calculationType: TermGroup_VacationGroupCalculationType;
	created: Date;
	createdBy: string;
	employmentId: number;
	employmentVacationGroupId: number;
	fromDate: Date;
	modified: Date;
	modifiedBy: string;
	name: string;
	state: SoeEntityState;
	type: number;
	vacationDaysHandleRule: TermGroup_VacationGroupVacationDaysHandleRule;
	vacationGroupId: number;
	vacationHandleRule: TermGroup_VacationGroupVacationHandleRule;
}
export interface IEvaluateAllWorkRulesActionResult {
	evaluatedRuleResults: IEvaluateAllWorkRulesResultDTO[];
	result: IActionResult;
}
export interface IEvaluateAllWorkRulesResultDTO {
	employeeId: number;
	violations: string[];
}
export interface IEvaluateWorkRuleResultDTO {
	action: TermGroup_ShiftHistoryType;
	canUserOverrideRuleForMinorsViolation: boolean;
	canUserOverrideRuleViolation: boolean;
	date: Date;
	employeeId: number;
	errorMessage: string;
	errorNumber: number;
	evaluatedWorkRule: SoeScheduleWorkRules;
	isRuleForMinors: boolean;
	isRuleRestTimeDayMandatory: boolean;
	isRuleRestTimeWeekMandatory: boolean;
	restTimeDayReachedDateFrom: Date;
	restTimeDayReachedDateTo: Date;
	success: boolean;
	workTimeReachedDateFrom: Date;
	workTimeReachedDateTo: Date;
}
export interface IEvaluateWorkRulesActionResult {
	allRulesSucceded: boolean;
	canUserOverrideRuleViolation: boolean;
	errorMessage: string;
	evaluatedRuleResults: IEvaluateWorkRuleResultDTO[];
	result: IActionResult;
}
export interface IEventHistoryDTO {
	actorCompanyId: number;
	batchId: number;
	booleanValue: boolean;
	created: Date;
	createdBy: string;
	dateValue: Date;
	decimalValue: number;
	entity: SoeEntityType;
	entityName: string;
	eventHistoryId: number;
	integerValue: number;
	modified: Date;
	modifiedBy: string;
	recordId: number;
	recordName: string;
	state: SoeEntityState;
	stringValue: string;
	type: TermGroup_EventHistoryType;
	typeName: string;
	userId: number;
}
export interface IExpenseHeadDTO {
	accounting: string;
	actorCompanyId: number;
	comment: string;
	created: Date;
	createdBy: string;
	employeeId: number;
	expenseHeadId: number;
	expenseRows: IExpenseRowDTO[];
	modified: Date;
	modifiedBy: string;
	projectId: number;
	start: Date;
	state: SoeEntityState;
	stop: Date;
	timeBlockDateId: number;
}
export interface IExpenseRowDTO {
	accounting: string;
	actorCompanyId: number;
	amount: number;
	amountCurrency: number;
	amountEntCurrency: number;
	amountLedgerCurrency: number;
	comment: string;
	created: Date;
	createdBy: string;
	customerInvoiceId: number;
	customerInvoiceNr: string;
	customerInvoiceRowAttestStateId: number;
	customerInvoiceRowId: number;
	employeeId: number;
	expenseHeadId: number;
	expenseRowId: number;
	externalComment: string;
	files: IFileUploadDTO[];
	hasFiles: boolean;
	invoicedAmount: number;
	invoicedAmountCurrency: number;
	invoicedAmountEntCurrency: number;
	invoicedAmountLedgerCurrency: number;
	invoiceRowAttestStateColor: string;
	invoiceRowAttestStateId: number;
	invoiceRowAttestStateName: string;
	isDeleted: boolean;
	isReadOnly: boolean;
	isSpecifiedUnitPrice: boolean;
	isTimeReadOnly: boolean;
	modified: Date;
	modifiedBy: string;
	payrollAttestStateColor: string;
	payrollAttestStateId: number;
	payrollAttestStateName: string;
	projectId: number;
	quantity: number;
	standOnDate: Date;
	start: Date;
	stop: Date;
	timeCodeId: number;
	timeCodeName: string;
	timePayrollAttestStateId: number;
	timePeriodId: number;
	transferToOrder: boolean;
	unitPrice: number;
	unitPriceCurrency: number;
	unitPriceEntCurrency: number;
	unitPriceLedgerCurrency: number;
	vat: number;
	vatCurrency: number;
	vatEntCurrency: number;
	vatLedgerCurrency: number;
}
export interface IExpenseRowGridDTO {
	actorCustomerId: number;
	amount: number;
	amountCurrency: number;
	amountExVat: number;
	comment: string;
	customerName: string;
	employeeId: number;
	employeeName: string;
	employeeNumber: string;
	expenseHeadId: number;
	expenseRowId: number;
	externalComment: string;
	from: Date;
	hasFiles: boolean;
	invoicedAmount: number;
	invoicedAmountCurrency: number;
	invoiceRowAttestStateColor: string;
	invoiceRowAttestStateId: number;
	invoiceRowAttestStateName: string;
	isSpecifiedUnitPrice: boolean;
	orderId: number;
	orderNr: string;
	payrollAttestStateColor: string;
	payrollAttestStateId: number;
	payrollAttestStateName: string;
	payrollTransactionDate: Date;
	projectId: number;
	projectName: string;
	projectNr: string;
	quantity: number;
	timeCodeId: number;
	timeCodeName: string;
	timeCodeRegistrationType: number;
	timePayrollTransactionIds: number[];
	unitPrice: number;
	vat: number;
	vatCurrency: number;
}
export interface IExtendedAbsenceSettingDTO {
	absenceFirstAndLastDay: boolean;
	absenceFirstDayStart: Date;
	absenceLastDayStart: Date;
	absenceWholeFirstDay: boolean;
	absenceWholeLastDay: boolean;
	adjustAbsenceAllDaysStart: Date;
	adjustAbsenceAllDaysStop: Date;
	adjustAbsenceFriStart: Date;
	adjustAbsenceFriStop: Date;
	adjustAbsenceMonStart: Date;
	adjustAbsenceMonStop: Date;
	adjustAbsencePerWeekDay: boolean;
	adjustAbsenceSatStart: Date;
	adjustAbsenceSatStop: Date;
	adjustAbsenceSunStart: Date;
	adjustAbsenceSunStop: Date;
	adjustAbsenceThuStart: Date;
	adjustAbsenceThuStop: Date;
	adjustAbsenceTueStart: Date;
	adjustAbsenceTueStop: Date;
	adjustAbsenceWedStart: Date;
	adjustAbsenceWedStop: Date;
	extendedAbsenceSettingId: number;
	percentalAbsence: boolean;
	percentalAbsenceOccursEndOfDay: boolean;
	percentalAbsenceOccursStartOfDay: boolean;
	percentalValue: number;
}
export interface IExtraFieldDTO {
	connectedEntity: number;
	connectedRecordId: number;
	entity: SoeEntityType;
	extraFieldId: number;
	extraFieldRecords: IExtraFieldRecordDTO[];
	extraFieldValues: IExtraFieldValueDTO[];
	text: string;
	translations: ICompTermDTO[];
	type: TermGroup_ExtraFieldType;
	externalCodesString: string;
}
export interface IExtraFieldGridDTO {
	accountDimId: number;
	accountDimName: string;
	extraFieldId: number;
	extraFieldValues: IExtraFieldValueDTO[];
	hasRecords: boolean;
	text: string;
	type: number;
}
export interface IExtraFieldRecordDTO {
	boolData: boolean;
	comment: string;
	dataTypeId: number;
	dateData: Date;
	decimalData: number;
	extraFieldId: number;
	extraFieldRecordId: number;
	extraFieldText: string;
	extraFieldType: number;
	extraFieldValues: IExtraFieldValueDTO[];
	intData: number;
	recordId: number;
	strData: string;
	value: string;
}
export interface IExtraFieldValueDTO {
	created: Date;
	createdBy: string;
	extraFieldId: number;
	extraFieldValueId: number;
	modified: Date;
	modifiedBy: string;
	sort: number;
	state: SoeEntityState;
	type: TermGroup_ExtraFieldValueType;
	value: string;
}
export interface IFieldSettingDTO {
	companySetting: ICompanyFieldSettingDTO;
	companySettingsSummary: string;
	fieldId: number;
	fieldName: string;
	formId: number;
	formName: string;
	roleSettings: IRoleFieldSettingDTO[];
	roleSettingsSummary: string;
	type: SoeFieldSettingType;
}
export interface IFilesLookupDTO {
	entity: SoeEntityType;
	files: ImportFileDTO[];
}
export interface IImportFileDTO {
	dataStorageId: number,
	fileName: string
}
export interface IFileUploadDTO {
	dataStorageRecordType: SoeDataStorageRecordType;
	description: string;
	fileName: string;
	id: number;
	imageId: number;
	includeWhenDistributed: boolean;
	includeWhenTransfered: boolean;
	invoiceAttachmentId: number;
	isDeleted: boolean;
	isSupplierInvoice: boolean;
	sourceType: InvoiceAttachmentSourceType;
	recordId: number;
}
export interface IFixedPayrollRowDTO {
	actorCompanyId: number;
	amount: number;
	created: Date;
	createdBy: string;
	distribute: boolean;
	employeeId: number;
	fixedPayrollRowId: number;
	fromDate: Date;
	isReadOnly: boolean;
	isSpecifiedUnitPrice: boolean;
	modified: Date;
	modifiedBy: string;
	payrollProductNrAndName: string;
	productId: number;
	quantity: number;
	state: SoeEntityState;
	toDate: Date;
	unitPrice: number;
	vatAmount: number;
}
export interface IFollowUpTypeGridDTO {
	followUpTypeId: number;
	isActive: boolean;
	name: string;
	state: SoeEntityState;
	type: TermGroup_FollowUpTypeType;
}
export interface IForaColletiveAgrementDTO {
	id: number;
	longText: string;
	shortText: string;
}
export interface IGeneralReportSelectionDTO extends IReportDataSelectionDTO {
	exportType: TermGroup_ReportExportType;
}
export interface IGenerateStockPurchaseSuggestionDTO {
	defaultDeliveryAddress: string;
	excludeMissingPurchaseQuantity: boolean;
	excludeMissingTriggerQuantity: boolean;
	excludePurchaseQuantityZero: boolean;
	productNrFrom: string;
	productNrTo: string;
	purchaseGenerationType: TermGroup_StockPurchaseGenerationOptions;
	purchaser: string;
	stockPlaceIds: number[];
	triggerQuantityPercent: number;
}
export interface IGenericImageDTO {
	connectedTypeName: string;
	description: string;
	filename: string;
	format: ImageFormatType;
	id: number;
	image: number[];
	imageFormatType: SoeDataStorageRecordType;
	images: number[];
	invoiceAttachments: IInvoiceAttachmentDTO[];
	sourceType: InvoiceAttachmentSourceType;
}
export interface IGenericType {
	description: string;
	id: number;
	isAll: boolean;
	isAllOrNone: boolean;
	isNone: boolean;
	isSelected: boolean;
	isVisible: boolean;
	isVisibleOrSelected: boolean;
	name: string;
}
export interface IGetLiquidityPlanningDTO {
	actorNrFrom: string;
	actorNrTo: string;
	compareDate: Date;
	currencyType: number;
	expDateFrom: Date;
	expDateTo: Date;
	insecureDebts: boolean;
	invDateFrom: Date;
	invDateTo: Date;
	invNrFrom: string;
	invNrTo: string;
	seqNrFrom: number;
	seqNrTo: number;
	type: SoeInvoiceType;
}
export interface IGrossProfitCodeDTO {
	accountDateFrom: Date;
	accountDateTo: Date;
	accountDimId: number;
	accountId: number;
	accountYear: string;
	accountYearId: number;
	actorCompanyId: number;
	code: number;
	created: Date;
	createdBy: string;
	description: string;
	grossProfitCodeId: number;
	modified: Date;
	modifiedBy: string;
	name: string;
	openingBalance: number;
	period1: number;
	period10: number;
	period11: number;
	period12: number;
	period2: number;
	period3: number;
	period4: number;
	period5: number;
	period6: number;
	period7: number;
	period8: number;
	period9: number;
}
export interface IHandleBillingRowDTO {
	actorCustomerId: number;
	amount: number;
	amountCurrency: number;
	attestStateColor: string;
	attestStateId: number;
	attestStateName: string;
	created: Date;
	currencyCode: string;
	currencyId: number;
	customer: string;
	customerInvoiceRowId: number;
	date: Date;
	description: string;
	discountAmount: number;
	discountAmountCurrency: number;
	discountPercent: number;
	discountType: number;
	ediEntryId: number;
	ediTextValue: string;
	householdDeductionType: number;
	invoiceId: number;
	invoiceNr: string;
	invoiceQuantity: number;
	isStockRow: boolean;
	isTimeProjectRow: boolean;
	marginalIncome: number;
	marginalIncomeCurrency: number;
	marginalIncomeLimit: number;
	marginalIncomeRatio: number;
	previouslyInvoicedQuantity: number;
	productCalculationType: TermGroup_InvoiceProductCalculationType;
	productId: number;
	productName: string;
	productNr: string;
	productRowType: SoeProductRowType;
	productUnitCode: string;
	productUnitId: number;
	project: string;
	projectId: number;
	projectNr: string;
	purchasePrice: number;
	purchasePriceCurrency: number;
	quantity: number;
	rowNr: number;
	status: number;
	sumAmount: number;
	sumAmountCurrency: number;
	text: string;
	timeManuallyChanged: boolean;
	type: SoeInvoiceRowType;
	validForInvoice: boolean;
	vatAmount: number;
	vatAmountCurrency: number;
}
export interface IHolidayDTO {
	actorCompanyId: number;
	created: Date;
	createdBy: string;
	date: Date;
	dayType: IDayTypeDTO;
	dayTypeId: number;
	dayTypeName: string;
	description: string;
	holidayId: number;
	import: boolean;
	isRedDay: boolean;
	modified: Date;
	modifiedBy: string;
	name: string;
	state: SoeEntityState;
	sysHolidayId: number;
	sysHolidayTypeId: number;
	sysHolidayTypeName: string;
}
export interface IHolidaySmallDTO {
	date: Date;
	description: string;
	holidayId: number;
	isRedDay: boolean;
	name: string;
}
export interface IHouseholdTaxDeductionApplicantDTO {
	apartmentNr: string;
	cooperativeOrgNr: string;
	customerInvoiceRowId: number;
	hidden: boolean;
	householdTaxDeductionApplicantId: number;
	identifierString: string;
	name: string;
	property: string;
	share: number;
	showButton: boolean;
	socialSecNr: string;
	state: SoeEntityState;
	comment: string;
}
export interface IHouseholdTaxDeductionFileRowDTO {
	customerInvoiceRowId: number;
	invoiceNr: string;
	name: string;
	socialSecNr: string;
	property: string;
	apartmentNr: string;
	cooperativeOrgNr: string;
	invoiceTotalAmount: number;
	workAmount: number;
	paidAmount: number;
	appliedAmount: number;
	nonValidAmount: number;
	comment: string;
	paidDate?: Date;
	houseHoldTaxDeductionType: TermGroup_HouseHoldTaxDeductionType;
	types: IHouseholdTaxDeductionFileRowTypeDTO[];
}
export interface IHouseholdTaxDeductionFileRowTypeDTO {
	sysHouseholdTypeId: number;
	text: string;
	hours: number;
	amount: number;
}
export interface IIdListSelectionDTO extends IReportDataSelectionDTO {
	ids: number[];
}
export interface IIdSelectionDTO extends IReportDataSelectionDTO {
	id: number;
}
export interface IImagesDTO {
	attestStateColor: string;
	attestStateId: number;
	attestStateName: string;
	attestStatus: TermGroup_DataStorageRecordAttestStatus;
	canDelete: boolean;
	confirmed: boolean;
	confirmedDate: Date;
	connectedTypeName: string;
	created: Date;
	currentAttestUsers: string;
	dataStorageRecordType: SoeDataStorageRecordType;
	description: string;
	fileName: string;
	formatType: ImageFormatType;
	image: number[];
	imageId: number;
	includeWhenDistributed: boolean;
	includeWhenTransfered: boolean;
	invoiceAttachmentId: number;
	lastSentDate: Date;
	needsConfirmation: boolean;
	sourceType: InvoiceAttachmentSourceType;
	type: SoeEntityImageType;
	recordId: number;
}
export interface IImportDTO {
	importId: number;
	actorCompanyId: number;
	accountYearId?: number;
	voucherSeriesId?: number;
	importDefinitionId: number;
	module: number;
	name: string;
	headName: string;
	state: SoeEntityState;
	importHeadType: TermGroup_IOImportHeadType;
	type: TermGroup_SysImportDefinitionType;
	typeText: string;
	useAccountDistribution: boolean;
	useAccountDimensions: boolean;
	updateExistingInvoice: boolean;
	guid: System.IGuid;
	specialFunctionality: string;
	dim1AccountId?: number;
	dim2AccountId?: number;
	dim3AccountId?: number;
	dim4AccountId?: number;
	dim5AccountId?: number;
	dim6AccountId?: number;
	isStandard: boolean;
	isStandardText: string;
	created: Date;
	createdBy: string;
	modified: Date;
	modifiedBy: string;
}
export interface IImportDynamicDTO {
	fields: IImportFieldDTO[];
	options: IImportOptionsDTO;
}
export interface IImportDynamicLogDTO {
	message: string;
	rowNr: number;
	type: number;
}
export interface IImportDynamicResultDTO {
	logs: IImportDynamicLogDTO[];
	message: string;
	newCount: number;
	skippedCount: number;
	success: boolean;
	totalCount: number;
	updateCount: number;
}
export interface IImportFieldDTO {
	availableValues: ISmallGenericType[];
	dataType: SettingDataType;
	defaultBoolValue: boolean;
	defaultDateTimeValue: Date;
	defaultDecimalValue: number;
	defaultGenericTypeValue: ISmallGenericType;
	defaultIntValue: number;
	defaultStringValue: string;
	enableValueMapping: boolean;
	field: string;
	index: number;
	isConfigured: boolean;
	isRequired: boolean;
	label: string;
	valueMapping: System.Collections.Generic.IKeyValuePair[];
}
export interface IImportOptionsDTO {
	importNew: boolean;
	skipFirstRow: boolean;
	updateExisting: boolean;
}
export interface IImportSelectionGridRowDTO {
	fileName: string;
	fileType: string;
	dataStorageId: number;
	import: ImportDTO;
	importId?: number;
	importName?: string;
	message: string;
	doImport: boolean;
	disableImport: boolean;
}
export interface IInactivateEmployeeDTO {
	employeeId: number;
	employeeName: string;
	employeeNr: string;
	message: string;
	success: boolean;
}
export interface IIncomingDeliveryHeadDTO {
	accountId: number;
	accountName: string;
	created: Date;
	createdBy: string;
	description: string;
	excludedDates: Date[];
	incomingDeliveryHeadId: number;
	modified: Date;
	modifiedBy: string;
	name: string;
	nbrOfOccurrences: number;
	recurrenceEndsOnDescription: string;
	recurrencePattern: string;
	recurrencePatternDescription: string;
	recurrenceStartsOnDescription: string;
	recurringDates: IDailyRecurrenceDatesOutput;
	rows: IIncomingDeliveryRowDTO[];
	startDate: Date;
	state: SoeEntityState;
	stopDate: Date;
}
export interface IIncomingDeliveryRowDTO {
	account2Id: number;
	account3Id: number;
	account4Id: number;
	account5Id: number;
	account6Id: number;
	allowOverlapping: boolean;
	created: Date;
	createdBy: string;
	description: string;
	dontAssignBreakLeftovers: boolean;
	headAccountId: number;
	headAccountName: string;
	incomingDeliveryHeadId: number;
	incomingDeliveryRowId: number;
	incomingDeliveryTypeDTO: IIncomingDeliveryTypeDTO;
	incomingDeliveryTypeId: number;
	length: number;
	minSplitLength: number;
	modified: Date;
	modifiedBy: string;
	name: string;
	nbrOfPackages: number;
	nbrOfPersons: number;
	offsetDays: number;
	onlyOneEmployee: boolean;
	shiftTypeId: number;
	shiftTypeName: string;
	startTime: Date;
	state: SoeEntityState;
	stopTime: Date;
	typeName: string;
}
export interface IIncomingDeliveryTypeDTO {
	accountId: number;
	accountName: string;
	actorCompanyId: number;
	created: Date;
	createdBy: string;
	description: string;
	incomingDeliveryTypeId: number;
	length: number;
	modified: Date;
	modifiedBy: string;
	name: string;
	nbrOfPersons: number;
	state: SoeEntityState;
}
export interface IInformationDTO {
	actorCompanyId: number;
	answerDate: Date;
	answerType: XEMailAnswerType;
	created: Date;
	createdBy: string;
	createdOrModified: Date;
	displayDate: Date;
	folder: string;
	hasText: boolean;
	informationId: number;
	licenseId: number;
	messageGroupIds: number[];
	modified: Date;
	modifiedBy: string;
	needsConfirmation: boolean;
	notificationSent: Date;
	notify: boolean;
	readDate: Date;
	recipients: IInformationRecipientDTO[];
	severity: TermGroup_InformationSeverity;
	severityName: string;
	shortText: string;
	showInMobile: boolean;
	showInTerminal: boolean;
	showInWeb: boolean;
	sourceType: SoeInformationSourceType;
	stickyType: TermGroup_InformationStickyType;
	subject: string;
	sysCompDbIds: number[];
	sysFeatureIds: number[];
	sysInformationSysCompDbs: ISysInformationSysCompDbDTO[];
	sysLanguageId: number;
	text: string;
	type: SoeInformationType;
	validFrom: Date;
	validTo: Date;
}
export interface IInformationGridDTO {
	folder: string;
	informationId: number;
	needsConfirmation: boolean;
	notificationSent: Date;
	notify: boolean;
	severity: TermGroup_InformationSeverity;
	severityName: string;
	shortText: string;
	showInMobile: boolean;
	showInTerminal: boolean;
	showInWeb: boolean;
	subject: string;
	validFrom: Date;
	validTo: Date;
}
export interface IInformationRecipientDTO {
	companyName: string;
	confirmedDate: Date;
	employeeNrAndName: string;
	hideDate: Date;
	informationId: number;
	informationRecipientId: number;
	readDate: Date;
	sysInformationId: number;
	userId: number;
	userName: string;
}
export interface IInsight {
	defaultChartType: TermGroup_InsightChartTypes;
	insightId: number;
	name: string;
	possibleChartTypes: TermGroup_InsightChartTypes[];
	possibleColumns: IMatrixLayoutColumn[];
	readOnly: boolean;
}
export interface IIntDateType {
	date: Date;
	number: number;
}
export interface IIntKeyValue {
	key: number;
	value: number;
}
export interface IIntrastatTransactionDTO {
	amount: number;
	customerInvoiceRowId: number;
	intrastatCodeId: number;
	intrastatTransactionId: number;
	intrastatTransactionType: number;
	netWeight: number;
	notIntrastat: boolean;
	originId: number;
	otherQuantity: string;
	productName: string;
	productNr: string;
	productUnitCode: string;
	productUnitId: number;
	quantity: number;
	rowNr: number;
	state: SoeEntityState;
	sysCountryId: number;
}
export interface IInventoryDTO {
	accountingSettings: IAccountingSettingsRowDTO[];
	actorCompanyId: number;
	categoryIds: number[];
	created: Date;
	createdBy: string;
	customerInvoiceId: number;
	customerInvoiceInfo: string;
	description: string;
	endAmount: number;
	inventoryFiles: IFileUploadDTO[];
	inventoryId: number;
	inventoryNr: string;
	inventoryWriteOffMethodId: number;
	modified: Date;
	modifiedBy: string;
	name: string;
	notes: string;
	parentId: number;
	parentName: string;
	periodType: TermGroup_InventoryWriteOffMethodPeriodType;
	periodValue: number;
	purchaseAmount: number;
	purchaseDate: Date;
	state: SoeEntityState;
	status: TermGroup_InventoryStatus;
	statusName: string;
	supplierInvoiceId: number;
	supplierInvoiceInfo: string;
	voucherSeriesTypeId: number;
	writeOffAmount: number;
	writeOffDate: Date;
	writeOffPeriods: number;
	writeOffRemainingAmount: number;
	writeOffSum: number;
}
export interface IInventoryNoteDTO {
	actorCompanyId: number;
	inventoryId: number;
	description: string;
	notes: string;
}
export interface IInvoiceAttachmentDTO {
	addAttachmentsOnEInvoice: boolean;
	addAttachmentsOnTransfer: boolean;
	dataStorageRecordId: number;
	ediEntryId: number;
	invoiceAttachmentId: number;
	invoiceId: number;
	lastDistributedDate: Date;
}
export interface IInvoiceDTO {
	actorId: number;
	billingType: TermGroup_BillingType;
	claimAccountId: number;
	contactEComId: number;
	contactGLNId: number;
	created: Date;
	createdBy: string;
	currencyDate: Date;
	currencyId: number;
	currencyRate: number;
	defaultDim1AccountId: number;
	defaultDim2AccountId: number;
	defaultDim3AccountId: number;
	defaultDim4AccountId: number;
	defaultDim5AccountId: number;
	defaultDim6AccountId: number;
	deliveryCustomerId: number;
	dueDate: Date;
	fullyPayed: boolean;
	invoiceDate: Date;
	invoiceId: number;
	invoiceNr: string;
	isTemplate: boolean;
	manuallyAdjustedAccounting: boolean;
	modified: Date;
	modifiedBy: string;
	ocr: string;
	onlyPayment: boolean;
	originDescription: string;
	originStatus: SoeOriginStatus;
	originStatusName: string;
	originUsers: IOriginUserDTO[];
	paidAmount: number;
	paidAmountCurrency: number;
	paidAmountEntCurrency: number;
	paidAmountLedgerCurrency: number;
	paymentNr: string;
	projectId: number;
	projectName: string;
	projectNr: string;
	referenceOur: string;
	referenceYour: string;
	remainingAmount: number;
	remainingAmountExVat: number;
	seqNr: number;
	state: SoeEntityState;
	statusIcon: SoeStatusIcon;
	sysPaymentTypeId: number;
	timeDiscountDate: Date;
	timeDiscountPercent: number;
	totalAmount: number;
	totalAmountCurrency: number;
	totalAmountEntCurrency: number;
	totalAmountLedgerCurrency: number;
	type: SoeInvoiceType;
	vatAmount: number;
	vatAmountCurrency: number;
	vatAmountEntCurrency: number;
	vatAmountLedgerCurrency: number;
	vatCodeId: number;
	vatType: TermGroup_InvoiceVatType;
	voucheHead2Id: number;
	voucheHeadId: number;
	voucherDate: Date;
	voucherSeriesId: number;
}
export interface IInvoiceProductCopyResult extends IInvoiceProductPriceResult {
	product: IInvoiceProductDTO;
	productIsSupplementCharge: boolean;
}
export interface IInvoiceProductDTO extends IProductDTO {
	accountingSettings: IAccountingSettingsRowDTO[];
	calculationType: TermGroup_InvoiceProductCalculationType;
	categoryIds: number[];
	defaultGrossMarginCalculationType: number;
	dontUseDiscountPercent: boolean;
	ean: string;
	guaranteePercentage: number;
	householdDeductionPercentage: number;
	householdDeductionType: number;
	intrastatCodeId: number;
	isExternal: boolean;
	isStockProduct: boolean;
	isSupplementCharge: boolean;
	priceListOrigin: number;
	priceLists: IPriceListDTO[];
	purchasePrice: number;
	salesPrice: number;
	showDescriptionAsTextRow: boolean;
	showDescrAsTextRowOnPurchase: boolean;
	sysCountryId: number;
	sysPriceListHeadId: number;
	sysProductId: number;
	sysProductType: number;
	sysWholesellerName: string;
	timeCodeId: number;
	useCalculatedCost: boolean;
	vatCodeId: number;
	vatFree: boolean;
	vatType: TermGroup_InvoiceProductVatType;
	weight: number;
}
export interface IInvoiceProductPriceResult {
	currencyDiffer: boolean;
	customerDiscountPercent: number;
	discountPercent: number;
	errorMessage: string;
	errorNumber: number;
	inclusiveVat: boolean;
	isForTimeProjectInvoiceProductRow: boolean;
	priceFormula: string;
	productIsSupplementCharge: boolean;
	productUnit: string;
	purchasePrice: number;
	rowId: number;
	salesPrice: number;
	success: boolean;
	sysWholesellerName: string;
	warning: boolean;
}
export interface IInvoiceProductPriceSearchViewDTO {
	code: string;
	companyWholesellerPriceListId: number;
	customerPrice: number;
	gnp: number;
	marginalIncome: number;
	marginalIncomeRatio: number;
	name: string;
	nettoNettoPrice: number;
	number: string;
	priceFormula: string;
	priceListOrigin: number;
	priceListType: string;
	priceStatus: number;
	productId: number;
	productProviderType: SoeSysPriceListProviderType;
	productType: number;
	purchaseUnit: string;
	salesUnit: string;
	sysPriceListHeadId: number;
	sysWholesellerId: number;
	type: number;
	wholeseller: string;
}
export interface IInvoiceProductSearchViewDTO {
	extendedInfo: string;
	externalId: number;
	imageUrl: string;
	manufacturer: string;
	name: string;
	number: string;
	priceListOrigin: PriceListOrigin;
	productIds: number[];
	type: SoeSysPriceListProviderType;
	externalUrl: string;
	endAt: Date;
	endAtTooltip: string;
	endAtIcon: string;
}
export interface IInvoiceTraceViewDTO extends IEDITraceViewBase {
	accountDistributionHeadId: number;
	accountDistributionName: string;
	amount: number;
	amountCurrency: number;
	billingType: TermGroup_BillingType;
	billingTypeName: string;
	contractId: number;
	currencyCode: string;
	currencyRate: number;
	date: Date;
	description: string;
	foreign: boolean;
	interestInvoiceId: number;
	inventoryDescription: string;
	inventoryId: number;
	inventoryName: string;
	inventoryStatusId: TermGroup_InventoryStatus;
	inventoryStatusName: string;
	inventoryTypeName: string;
	invoiceId: number;
	isAccountDistribution: boolean;
	isContract: boolean;
	isInterestInvoice: boolean;
	isInventory: boolean;
	isInvoice: boolean;
	isOffer: boolean;
	isOrder: boolean;
	isPayment: boolean;
	isProject: boolean;
	isReminderInvoice: boolean;
	isScanning: boolean;
	isVoucher: boolean;
	langId: number;
	mappedInvoiceId: number;
	number: string;
	offerId: number;
	orderId: number;
	originStatus: SoeOriginStatus;
	originStatusName: string;
	originType: SoeOriginType;
	originTypeName: string;
	paymentRowId: number;
	paymentStatusId: SoePaymentStatus;
	paymentStatusName: string;
	projectId: number;
	reminderInvoiceId: number;
	state: SoeEntityState;
	sysCurrencyId: number;
	triggerType: number;
	triggerTypeName: string;
	vatAmount: number;
	vatAmountCurrency: number;
	voucherHeadId: number;
}
export interface ILiquidityPlanningDTO {
	created: Date;
	createdBy: string;
	date: Date;
	invoiceId: number;
	invoiceNr: string;
	liquidityPlanningTransactionId: number;
	modified: Date;
	modifiedBy: string;
	originType: SoeOriginType;
	specification: string;
	total: number;
	transactionType: LiquidityPlanningTransactionType;
	transactionTypeName: string;
	valueIn: number;
	valueOut: number;
}
export interface IMarkupDTO {
	actorCompanyId: number;
	actorCustomerId: number;
	categoryId: number;
	categoryName: string;
	code: string;
	created: Date;
	createdBy: string;
	customerName: string;
	discountPercent: number;
	markupId: number;
	markupPercent: number;
	modified: Date;
	modifiedBy: string;
	productIdFilter: string;
	state: SoeEntityState;
	sysWholesellerId: number;
	wholesellerDiscountPercent: number;
	wholesellerName: string;
}
export interface IMassRegistrationGridDTO {
	isRecurring: boolean;
	massRegistrationTemplateHeadId: number;
	name: string;
	recurringDateTo: Date;
	state: SoeEntityState;
}
export interface IMassRegistrationTemplateHeadDTO {
	actorCompanyId: number;
	comment: string;
	created: Date;
	createdBy: string;
	dateFrom: Date;
	dateTo: Date;
	dim1Id: number;
	dim2Id: number;
	dim3Id: number;
	dim4Id: number;
	dim5Id: number;
	dim6Id: number;
	hasCreatedTransactions: boolean;
	inputType: TermGroup_MassRegistrationInputType;
	isRecurring: boolean;
	isSpecifiedUnitPrice: boolean;
	massRegistrationTemplateHeadId: number;
	modified: Date;
	modifiedBy: string;
	name: string;
	payrollProductId: number;
	quantity: number;
	recurringDateTo: Date;
	rows: IMassRegistrationTemplateRowDTO[];
	state: SoeEntityState;
	stopOnDateFrom: boolean;
	stopOnDateTo: boolean;
	stopOnIsSpecifiedUnitPrice: boolean;
	stopOnProduct: boolean;
	stopOnQuantity: boolean;
	stopOnUnitPrice: boolean;
	unitPrice: number;
}
export interface IMassRegistrationTemplateRowDTO {
	created: Date;
	createdBy: string;
	dateFrom: Date;
	dateTo: Date;
	dim1Id: number;
	dim1Name: string;
	dim1Nr: string;
	dim2DimNr: number;
	dim2Id: number;
	dim2Name: string;
	dim2Nr: string;
	dim3DimNr: number;
	dim3Id: number;
	dim3Name: string;
	dim3Nr: string;
	dim4DimNr: number;
	dim4Id: number;
	dim4Name: string;
	dim4Nr: string;
	dim5DimNr: number;
	dim5Id: number;
	dim5Name: string;
	dim5Nr: string;
	dim6DimNr: number;
	dim6Id: number;
	dim6Name: string;
	dim6Nr: string;
	employeeId: number;
	employeeName: string;
	employeeNr: string;
	employeeNrSort: string;
	errorMessage: string;
	isSpecifiedUnitPrice: boolean;
	massRegistrationTemplateHeadId: number;
	massRegistrationTemplateRowId: number;
	modified: Date;
	modifiedBy: string;
	paymentDate: Date;
	productId: number;
	productName: string;
	productNr: string;
	quantity: number;
	state: SoeEntityState;
	unitPrice: number;
}
export interface IMatrixColumnSelectionDTO extends IReportDataSelectionDTO {
	field: string;
	matrixDataType: MatrixDataType;
	options: IMatrixDefinitionColumnOptions;
	sort: number;
}
export interface IMatrixColumnsSelectionDTO extends IReportDataSelectionDTO {
	analysisMode: AnalysisMode;
	chartType: TermGroup_InsightChartTypes;
	columns: IMatrixColumnSelectionDTO[];
	insightId: number;
	insightName: string;
	valueType: number;
}
export interface IMatrixDefinition {
	key: number;
	matrixDefinitionColumns: IMatrixDefinitionColumn[];
}
export interface IMatrixDefinitionColumn {
	columnNumber: number;
	field: string;
	key: System.IGuid;
	matrixDataType: MatrixDataType;
	matrixLayoutColumn: IMatrixLayoutColumn;
	options: IMatrixDefinitionColumnOptions;
	title: string;
}
export interface IMatrixDefinitionColumnOptions {
	alignLeft: boolean;
	alignRight: boolean;
	changed: boolean;
	clearZero: boolean;
	dateFormatOption: TermGroup_MatrixDateFormatOption;
	decimals: number;
	groupBy: boolean;
	groupOption: TermGroup_MatrixGroupAggOption;
	hidden: boolean;
	key: string;
	labelPostValue: string;
	minutesToDecimal: boolean;
	minutesToTimeSpan: boolean;
	formatTimeWithSeconds: boolean;
	formatTimeWithDays: boolean;
}
export interface IMatrixField {
	columnKey: System.IGuid;
	key: number;
	matrixDataType: MatrixDataType;
	matrixFieldOptions: IMatrixFieldOption[];
	rowNumber: number;
	value: any;
}
export interface IMatrixFieldOption {
	matrixFieldSetting: MatrixFieldSetting;
	stringValue: string;
}
export interface IMatrixLayoutColumn {
	field: string;
	matrixDataType: MatrixDataType;
	options: IMatrixDefinitionColumnOptions;
	sort: number;
	title: string;
	visible: boolean;
}
export interface IMatrixResult {
	jsonRows: System.Collections.Generic.IKeyValuePair[];
	key: number;
	matrixDefinition: IMatrixDefinition;
	matrixDefinitions: IMatrixDefinition[];
	matrixFields: IMatrixField[];
}
export interface IMessageAttachmentDTO {
	data: number[];
	dataStorageId: number;
	filesize: number;
	isUploadedAsImage: boolean;
	messageAttachmentId: number;
	name: string;
}
export interface IMessageDTO {
	actorCompanyId: number;
	answerDate: Date;
	created: Date;
	deletedDate: Date;
	expirationDate: Date;
	hasAttachment: boolean;
	hasBeenConfirmed: string;
	hasBeenRead: string;
	isExpired: boolean;
	isHandledByJob: boolean;
	isSelected: boolean;
	isUnRead: boolean;
	isVisible: boolean;
	messageId: number;
	messageTextId: number;
	messageType: TermGroup_MessageType;
	needsConfirmation: boolean;
	needsConfirmationAnswer: boolean;
	priority: number;
	readDate: Date;
	recieversName: string;
	recipientList: IMessageRecipientDTO[];
	senderName: string;
	sentDate: Date;
	subject: string;
}
export interface IMessageEditDTO {
	absenceRequestEmployeeId: number;
	absenceRequestEmployeeUserId: number;
	actorCompanyId: number;
	answerType: XEMailAnswerType;
	attachments: IMessageAttachmentDTO[];
	copyToSMS: boolean;
	created: Date;
	deletedDate: Date;
	entity: SoeEntityType;
	expirationDate: Date;
	forwardDate: Date;
	licenseId: number;
	markAsOutgoing: boolean;
	messageDeliveryType: TermGroup_MessageDeliveryType;
	messageId: number;
	messagePriority: TermGroup_MessagePriority;
	messageTextId: number;
	messageTextType: TermGroup_MessageTextType;
	messageType: TermGroup_MessageType;
	parentId: number;
	recievers: IMessageRecipientDTO[];
	recordId: number;
	replyDate: Date;
	roleId: number;
	senderEmail: string;
	senderName: string;
	senderUserId: number;
	sentDate: Date;
	shortText: string;
	subject: string;
	text: string;
}
export interface IMessageGridDTO {
	answerDate: Date;
	created: Date;
	deletedDate: Date;
	firstTextRow: string;
	forwardDate: Date;
	hasAttachment: boolean;
	hasBeenConfirmed: string;
	hasBeenRead: string;
	messageId: number;
	messageType: TermGroup_MessageType;
	needsConfirmation: boolean;
	readDate: Date;
	recieversName: string;
	replyDate: Date;
	senderName: string;
	sentDate: Date;
	subject: string;
}
export interface IMessageGroupDTO {
	actorCompanyId: number;
	description: string;
	groupMembers: IMessageGroupMemberDTO[];
	isPublic: boolean;
	licenseId: number;
	messageGroupId: number;
	name: string;
	noUserValidation: boolean;
	userId: number;
}
export interface IMessageGroupMemberDTO {
	entity: SoeEntityType;
	messageGroupId: number;
	name: string;
	recordId: number;
	username: string;
}
export interface IMessageRecipientDTO {
	answerDate: Date;
	answerType: XEMailAnswerType;
	createdById: number;
	deletedDate: Date;
	emailAddress: string;
	employeeRequestType: TermGroup_EmployeeRequestType;
	employeeRequestTypeFlags: TermGroup_EmployeeRequestTypeFlags;
	externalId: number;
	forwardDate: Date;
	isCC: boolean;
	isSelected: boolean;
	isVisible: boolean;
	name: string;
	readDate: Date;
	recipientId: number;
	replyDate: Date;
	sendCopyAsEmail: boolean;
	signeeKey: string;
	type: XEMailRecipientType;
	userId: number;
	userName: string;
}
export interface IMyShiftsGaugeDTO {
	date: Date;
	shiftStatus: TermGroup_TimeScheduleTemplateBlockShiftStatus;
	shiftStatusName: string;
	shiftTypeId: number;
	shiftTypeName: string;
	shiftUserStatus: TermGroup_TimeScheduleTemplateBlockShiftUserStatus;
	shiftUserStatusName: string;
	time: string;
	timeScheduleTemplateBlockId: number;
}
export interface INameAndIdDTO {
	id: number;
	name: string;
}
export interface IOfferTraceViewDTO {
	amount: number;
	amountCurrency: number;
	billingType: TermGroup_BillingType;
	billingTypeName: string;
	currencyCode: string;
	currencyRate: number;
	date: Date;
	description: string;
	foreign: boolean;
	invoiceId: number;
	isInvoice: boolean;
	isOrder: boolean;
	isProject: boolean;
	langId: number;
	number: string;
	offerId: number;
	orderId: number;
	originStatus: SoeOriginStatus;
	originStatusName: string;
	originType: SoeOriginType;
	originTypeName: string;
	projectId: number;
	state: SoeEntityState;
	sysCurrencyId: number;
	vatAmount: number;
	vatAmountCurrency: number;
}
export interface IOpeningHoursDTO {
	accountId: number;
	accountName: string;
	actorCompanyId: number;
	closingTime: Date;
	created: Date;
	createdBy: string;
	description: string;
	fromDate: Date;
	modified: Date;
	modifiedBy: string;
	name: string;
	openingHoursId: number;
	openingTime: Date;
	specificDate: Date;
	standardWeekDay: number;
	state: number;
}
export interface IOpenShiftsGaugeDTO {
	date: Date;
	iamInQueue: boolean;
	link: string;
	nbrInQueue: number;
	openType: number;
	openTypeName: string;
	shiftTypeId: number;
	shiftTypeName: string;
	time: string;
	timeScheduleTemplateBlockId: number;
}
export interface IOrderDTO {
	actorId: number;
	addAttachementsToEInvoice: boolean;
	addSupplierInvoicesToEInvoice: boolean;
	billingAddressId: number;
	billingAdressText: string;
	billingType: TermGroup_BillingType;
	categoryIds: number[];
	centRounding: number;
	checkConflictsOnSave: boolean;
	contactEComId: number;
	contactGLNId: number;
	contractGroupId: number;
	contractNr: string;
	created: Date;
	createdBy: string;
	currencyDate: Date;
	currencyId: number;
	currencyRate: number;
	customerBlockNote: string;
	customerEmail: string;
	customerInvoiceRows: IProductRowDTO[];
	customerName: string;
	customerPhoneNr: string;
	defaultDim1AccountId: number;
	defaultDim2AccountId: number;
	defaultDim3AccountId: number;
	defaultDim4AccountId: number;
	defaultDim5AccountId: number;
	defaultDim6AccountId: number;
	deliveryAddressId: number;
	deliveryConditionId: number;
	deliveryCustomerId: number;
	deliveryDate: Date;
	deliveryDateText: string;
	deliveryTypeId: number;
	dueDate: Date;
	ediTransferMode: TermGroup_OrderEdiTransferMode;
	estimatedTime: number;
	fixedPriceOrder: boolean;
	forceSave: boolean;
	freightAmount: number;
	freightAmountCurrency: number;
	hasManuallyDeletedTimeProjectRows: boolean;
	includeExpenseInReport: TermGroup_IncludeExpenseInReportType;
	includeOnInvoice: boolean;
	includeOnlyInvoicedTime: boolean;
	invoiceDate: Date;
	invoiceDeliveryType: number;
	invoiceFee: number;
	invoiceFeeCurrency: number;
	invoiceHeadText: string;
	invoiceId: number;
	invoiceLabel: string;
	invoiceNr: string;
	invoicePaymentService: number;
	invoiceText: string;
	isMainInvoice: boolean;
	isTemplate: boolean;
	keepAsPlanned: boolean;
	mainInvoice: string;
	mainInvoiceId: number;
	mainInvoiceNr: string;
	marginalIncomeCurrency: number;
	marginalIncomeRatio: number;
	modified: Date;
	modifiedBy: string;
	nbrOfChecklists: number;
	nextContractPeriodDate: Date;
	nextContractPeriodValue: number;
	nextContractPeriodYear: number;
	note: string;
	orderDate: Date;
	orderInvoiceTemplateId: number;
	orderReference: string;
	orderType: TermGroup_OrderType;
	originDescription: string;
	originStatus: SoeOriginStatus;
	originStatusName: string;
	originUsers: IOriginUserSmallDTO[];
	payingCustomerId: number;
	paymentConditionId: number;
	plannedStartDate: Date;
	plannedStopDate: Date;
	prevInvoiceId: number;
	priceListTypeId: number;
	printTimeReport: boolean;
	priority: number;
	projectId: number;
	projectIsActive: boolean;
	projectNr: string;
	referenceOur: string;
	referenceYour: string;
	remainingAmount: number;
	remainingAmountExVat: number;
	remainingTime: number;
	seqNr: number;
	shiftTypeId: number;
	showNote: boolean;
	statusIcon: SoeStatusIcon;
	sumAmount: number;
	sumAmountCurrency: number;
	sysWholeSellerId: number;
	totalAmount: number;
	totalAmountCurrency: number;
	transferAttachments: boolean;
	triangulationSales: boolean;
	vatAmount: number;
	vatAmountCurrency: number;
	vatType: TermGroup_InvoiceVatType;
	voucherDate: Date;
	voucherSeriesId: number;
	workingDescription: string;
}
export interface IOrderListDTO {
	attestStateColor: string;
	attestStateName: string;
	customerId: number;
	customerName: string;
	customerNr: string;
	deliveryAddress: string;
	estimatedTime: number;
	internalDescription: string;
	keepAsPlanned: boolean;
	orderId: number;
	orderNr: number;
	plannedStartDate: Date;
	plannedStopDate: Date;
	priority: number;
	projectId: number;
	projectName: string;
	projectNr: string;
	remainingTime: number;
	shiftTypeColor: string;
	shiftTypeId: number;
	shiftTypeName: string;
	workingDescription: string;
}
export interface IOrderShiftDTO {
	date: Date;
	employeeName: string;
	from: string;
	shiftTypeName: string;
	timeDeviationCauseId: number;
	timeDeviationCauseName: string;
	timeScheduleTemplateBlockId: number;
	to: string;
}
export interface IOrderTraceViewDTO extends IEDITraceViewBase {
	amount: number;
	amountCurrency: number;
	billingType: TermGroup_BillingType;
	billingTypeName: string;
	contractId: number;
	currencyCode: string;
	currencyRate: number;
	date: Date;
	description: string;
	foreign: boolean;
	invoiceId: number;
	isContract: boolean;
	isInvoice: boolean;
	isOffer: boolean;
	isProject: boolean;
	isPurchase: boolean;
	isSupplierInvoice: boolean;
	langId: number;
	number: string;
	offerId: number;
	orderId: number;
	originStatus: SoeOriginStatus;
	originStatusName: string;
	originType: SoeOriginType;
	originTypeName: string;
	projectId: number;
	purchaseId: number;
	state: SoeEntityState;
	supplierInvoiceId: number;
	sysCurrencyId: number;
	vatAmount: number;
	vatAmountCurrency: number;
}
export interface IOriginUserDTO {
	created: Date;
	createdBy: string;
	loginName: string;
	main: boolean;
	modified: Date;
	modifiedBy: string;
	name: string;
	originId: number;
	originUserId: number;
	readyDate: Date;
	roleId: number;
	state: SoeEntityState;
	userId: number;
}
export interface IOriginUserSmallDTO {
	isReady: boolean;
	main: boolean;
	name: string;
	originUserId: number;
	userId: number;
}
export interface IPaymentImportDTO {
	actorCompanyId: number;
	batchId: number;
	created: Date;
	createdBy: string;
	filename: string;
	importDate: Date;
	importType: ImportPaymentType;
	modified: Date;
	modifiedBy: string;
	numberOfPayments: number;
	paymentImportId: number;
	paymentLabel: string;
	paymentMethodName: string;
	state: SoeEntityState;
	statusName: string;
	sysPaymentTypeId: TermGroup_SysPaymentType;
	totalAmount: number;
	transferStatus: number;
	type: number;
	typeName: string;
}
export interface IPaymentImportIODTO {
	actorCompanyId: number;
	amountDiff: number;
	batchNr: number;
	currency: string;
	customer: string;
	customerId: number;
	dueDate: Date;
	importType: ImportPaymentType;
	invoiceAmount: number;
	invoiceDate: Date;
	invoiceId: number;
	invoiceNr: string;
	invoiceSeqnr: string;
	isFullyPaid: boolean;
	isSelected: boolean;
	isVisible: boolean;
	matchCodeId: number;
	matchCodeName: string;
	ocr: string;
	paidAmount: number;
	paidAmountCurrency: number;
	paidDate: Date;
	paymentImportIOId: number;
	paymentRowId: number;
	paymentRowSeqNr: number;
	paymentTypeName: string;
	restAmount: number;
	state: ImportPaymentIOState;
	stateName: string;
	status: ImportPaymentIOStatus;
	statusName: string;
	type: TermGroup_BillingType;
	typeName: string;
}
export interface IPaymentInformationDTO {
	actorId: number;
	created: Date;
	createdBy: string;
	defaultSysPaymentTypeId: number;
	modified: Date;
	modifiedBy: string;
	paymentInformationId: number;
	rows: IPaymentInformationRowDTO[];
	state: SoeEntityState;
}
export interface IPaymentInformationRowDTO {
	bankConnected: boolean;
	bic: string;
	billingType: TermGroup_BillingType;
	chargeCode: number;
	chargeCodeName: string;
	clearingCode: string;
	created: Date;
	createdBy: string;
	currencyAccount: string;
	currencyCode: string;
	currencyId: number;
	default: boolean;
	intermediaryCode: number;
	intermediaryCodeName: string;
	modified: Date;
	modifiedBy: string;
	payerBankId: string;
	paymentCode: string;
	paymentForm: number;
	paymentFormName: string;
	paymentInformationId: number;
	paymentInformationRowId: number;
	paymentMethodCode: number;
	paymentMethodCodeName: string;
	paymentNr: string;
	shownInInvoice: boolean;
	state: SoeEntityState;
	sysPaymentTypeId: number;
	sysPaymentTypeName: string;
}
export interface IPaymentInformationViewDTO {
	actorId: number;
	default: boolean;
	defaultSysPaymentTypeId: number;
	langId: number;
	name: string;
	paymentInformationRowId: number;
	paymentNr: string;
	paymentNrDisplay: string;
	sysPaymentTypeId: number;
}
export interface IPaymentMethodDTO {
	accountId: number;
	accountNr: string;
	actorCompanyId: number;
	customerNr: string;
	name: string;
	payerBankId: string;
	paymentInformationRow: IPaymentInformationRowDTO;
	paymentInformationRowId: number;
	paymentMethodId: number;
	paymentNr: string;
	paymentType: SoeOriginType;
	state: SoeEntityState;
	sysPaymentMethodId: number;
	sysPaymentMethodName: string;
	sysPaymentTypeId: number;
	transactionCode: number;
	useInCashSales: boolean;
	useRoundingInCashSales: boolean;
}
export interface IPaymentRowDTO {
	actorId: number;
	amount: number;
	amountCurrency: number;
	amountDiff: number;
	amountDiffCurrency: number;
	amountDiffEntCurrency: number;
	amountDiffLedgerCurrency: number;
	amountEntCurrency: number;
	amountLedgerCurrency: number;
	bankFee: number;
	bankFeeCurrency: number;
	bankFeeEntCurrency: number;
	bankFeeLedgerCurrency: number;
	billingType: TermGroup_BillingType;
	created: Date;
	createdBy: string;
	currencyDate: Date;
	currencyId: number;
	currencyRate: number;
	description: string;
	fullyPaid: boolean;
	hasPendingAmountDiff: boolean;
	hasPendingBankFee: boolean;
	invoiceId: number;
	invoiceTotalAmount: number;
	invoiceTotalAmountCurrency: number;
	isRestPayment: boolean;
	isSuggestion: boolean;
	modified: Date;
	modifiedBy: string;
	originDescription: string;
	originStatus: number;
	paidAmount: number;
	paidAmountCurrency: number;
	payDate: Date;
	paymentAccountRows: IAccountingRowDTO[];
	paymentId: number;
	paymentImportId: number;
	paymentMethodId: number;
	paymentNr: string;
	paymentRowId: number;
	seqNr: number;
	state: SoeEntityState;
	status: number;
	statusMsg: string;
	statusName: string;
	text: string;
	sysPaymentTypeId: number;
	transferStatus: number;
	vatAmount: number;
	vatAmountCurrency: number;
	voucherDate: Date;
	voucherHasMultiplePayments: boolean;
	voucherHeadId: number;
	voucherSeriesId: number;
	voucherSeriesTypeId: number;
}
export interface IPaymentRowSaveDTO extends SoftOne.Soe.Common.DTO.IPaymentRowSaveBaseDTO {
	accountYearId: number;
	amount: number;
	amountCurrency: number;
	amountDiff: number;
	amountDiffCurrency: number;
	billingType: TermGroup_BillingType;
	currencyDate: Date;
	currencyId: number;
	fullyPayed: boolean;
	hasPendingAmountDiff: boolean;
	hasPendingBankFee: boolean;
	importDate: Date;
	importFilename: string;
	invoiceId: number;
	invoiceNr: string;
	invoiceType: SoeInvoiceType;
	isRestPayment: boolean;
	isSuperSupportSave: boolean;
	onlyPayment: boolean;
	originDescription: string;
	originStatus: SoeOriginStatus;
	originType: SoeOriginType;
	paidAmount: number;
	paidAmountCurrency: number;
	paymentMethodId: number;
	paymentNr: string;
	paymentRowId: number;
	paymentStatus: SoePaymentStatus;
	seqNr: number;
	state: SoeEntityState;
	text: string;
	sysPaymentTypeId: number;
	totalAmount: number;
	totalAmountCurrency: number;
	vatAmount: number;
	vatAmountCurrency: number;
	voucherHeadId: number;
	voucherSeriesId: number;
}
export interface IPaymentTraceViewDTO {
	amount: number;
	amountCurrency: number;
	currencyCode: string;
	currencyRate: number;
	date: Date;
	description: string;
	foreign: boolean;
	invoiceId: number;
	isInvoice: boolean;
	isProject: boolean;
	isVoucher: boolean;
	langId: number;
	number: string;
	originStatus: SoeOriginStatus;
	originStatusName: string;
	originType: SoeOriginType;
	originTypeName: string;
	paymentRowId: number;
	projectId: number;
	registrationType: OrderInvoiceRegistrationType;
	sequenceNumber: number;
	state: SoeEntityState;
	sysCurrencyId: number;
	vatAmount: number;
	vatAmountCurrency: number;
	voucherHeadId: number;
}
export interface IPayrollCalculationEmployeePeriodDTO {
	attestStateColor: string;
	attestStateId: number;
	attestStateName: string;
	attestStates: IAttestStateDTO[];
	attestStateSort: number;
	createdOrModified: Date;
	employeeId: number;
	employeeName: string;
	employeeNr: string;
	employeeNrAndName: string;
	hasAttestStates: boolean;
	periodSum: IPayrollCalculationPeriodSumDTO;
	timePeriodId: number;
}
export interface IPayrollCalculationPeriodSumDTO {
	benefitInvertExcluded: number;
	compensation: number;
	deduction: number;
	employmentTaxCredit: number;
	employmentTaxDebit: number;
	gross: number;
	hasEmploymentTaxDiff: boolean;
	hasNetSalaryDiff: boolean;
	hasSupplementChargeDiff: boolean;
	hasWarning: boolean;
	isEmploymentTaxMissing: boolean;
	isGrossSalaryNegative: boolean;
	isNetSalaryMissing: boolean;
	isNetSalaryNegative: boolean;
	isTaxMissing: boolean;
	net: number;
	supplementChargeCredit: number;
	supplementChargeDebit: number;
	tax: number;
	transactionNet: number;
}
export interface IPayrollCalculationProductDTO {
	accountDims: IAccountDimDTO[];
	accountingLongString: string;
	accountingShortString: string;
	accountInternals: IAccountDTO[];
	accountStd: IAccountDTO;
	amount: number;
	amountCurrency: number;
	amountEntCurrency: number;
	attestPayrollTransactions: IAttestPayrollTransactionDTO[];
	attestStateColor: string;
	attestStateId: number;
	attestStateName: string;
	attestStates: IAttestStateDTO[];
	attestStateSort: number;
	attestTransitionLogs: IAttestTransitionLogDTO[];
	calenderDayFactor: number;
	dateFrom: Date;
	dateFromString: string;
	dateTo: Date;
	dateToString: string;
	employeeId: number;
	hasAttestStates: boolean;
	hasComment: boolean;
	hasInfo: boolean;
	hasPayrollScheduleTransactions: boolean;
	hasPayrollTransactions: boolean;
	hasSameAttestState: boolean;
	hideDate: boolean;
	hideQuantity: boolean;
	hideUnitprice: boolean;
	includedInPayrollProductChain: boolean;
	isAdded: boolean;
	isAverageCalculated: boolean;
	isBelowEmploymentTaxLimitRuleFromPreviousPeriods: boolean;
	isBelowEmploymentTaxLimitRuleHidden: boolean;
	isCentRounding: boolean;
	isMonthlySalary: boolean;
	isQuantityDays: boolean;
	isQuantityOrFixed: boolean;
	isQuantityRounding: boolean;
	isRetroactive: boolean;
	isRounding: boolean;
	isVacationFiveDaysPerWeek: boolean;
	payrollProductExport: boolean;
	payrollProductFactor: number;
	payrollProductId: number;
	payrollProductName: string;
	payrollProductNumber: string;
	payrollProductNumberSort: string;
	payrollProductPayed: boolean;
	payrollProductShortName: string;
	payrollProductString: string;
	quantity: number;
	quantity_HH_DD_String: string;
	quantityCalendarDays: number;
	quantityString: string;
	quantityWorkDays: number;
	sysPayrollTypeLevel1: number;
	sysPayrollTypeLevel1Name: string;
	sysPayrollTypeLevel2: number;
	sysPayrollTypeLevel2Name: string;
	sysPayrollTypeLevel3: number;
	sysPayrollTypeLevel3Name: string;
	sysPayrollTypeLevel4: number;
	sysPayrollTypeLevel4Name: string;
	timeUnit: number;
	transactionComment: string;
	uniqueId: string;
	unitPrice: number;
	unitPriceCurrency: number;
	unitPriceCurrencyString: string;
	unitPriceEntCurrency: number;
	unitPriceEntCurrencyString: string;
	unitPriceString: string;
}
export interface IPayrollGroupAccountsDTO {
	employmentTaxAccountId: number;
	employmentTaxAccountName: string;
	employmentTaxAccountNr: string;
	employmentTaxPercent: number;
	fromInterval: number;
	isModified: boolean;
	ownSupplementChargeAccountId: number;
	ownSupplementChargeAccountName: string;
	ownSupplementChargeAccountNr: string;
	ownSupplementChargePercent: number;
	payrollTaxAccountId: number;
	payrollTaxAccountName: string;
	payrollTaxAccountNr: string;
	payrollTaxPercent: number;
	toInterval: number;
}
export interface IPayrollGroupDTO extends SoftOne.Soe.Common.DTO.IPayrollGroupBaseDTO {
	created: Date;
	createdBy: string;
	externalCodes: string[];
	externalCodesString: string;
	modified: Date;
	modifiedBy: string;
	oneTimeTaxFormulaId: number;
	payrollProducts: IPayrollGroupPayrollProductDTO[];
	priceFormulas: IPayrollGroupPriceFormulaDTO[];
	priceTypes: IPayrollGroupPriceTypeDTO[];
	reportIds: number[];
	reports: IPayrollGroupReportDTO[];
	timePeriodHead: ITimePeriodHeadDTO;
	vacations: IPayrollGroupVacationGroupDTO[];
}
export interface IPayrollGroupGridDTO {
	name: string;
	payrollGroupId: number;
	state: SoeEntityState;
	timePeriodHeadName: string;
}
export interface IPayrollGroupPayrollProductDTO {
	distribute: boolean;
	payrollGroupId: number;
	payrollGroupPayrollProductId: number;
	productId: number;
	productName: string;
	productNr: string;
	state: SoeEntityState;
}
export interface IPayrollGroupPriceFormulaDTO {
	formulaExtracted: string;
	formulaName: string;
	formulaNames: string;
	formulaPlain: string;
	fromDate: Date;
	payrollGroupId: number;
	payrollGroupPriceFormulaId: number;
	payrollPriceFormulaId: number;
	result: number;
	showOnEmployee: boolean;
	toDate: Date;
}
export interface IPayrollGroupPriceTypeDTO {
	currentAmount: number;
	payrollGroupId: number;
	payrollGroupPriceTypeId: number;
	payrollLevelId: number;
	payrollLevelName: string;
	payrollPriceType: IPayrollPriceTypeDTO;
	payrollPriceTypeCurrentAmount: number;
	payrollPriceTypeId: number;
	periods: IPayrollGroupPriceTypePeriodDTO[];
	priceTypeCode: string;
	priceTypeLevel: IPriceTypeLevelDTO;
	priceTypeName: string;
	readOnlyOnEmployee: boolean;
	showOnEmployee: boolean;
	sort: number;
}
export interface IPayrollGroupPriceTypePeriodDTO {
	amount: number;
	fromDate: Date;
	payrollGroupPriceTypeId: number;
	payrollGroupPriceTypePeriodId: number;
}
export interface IPayrollGroupReportDTO {
	actorCompanyId: number;
	created: Date;
	createdBy: string;
	employeeTemplateId: number;
	modified: Date;
	modifiedBy: string;
	payrollGroupId: number;
	payrollGroupReportId: number;
	reportDescription: string;
	reportId: number;
	reportName: string;
	reportNameDesc: string;
	reportNr: number;
	state: SoeEntityState;
	sysReportTemplateTypeId: number;
}
export interface IPayrollGroupSmallDTO {
	name: string;
	payrollGroupId: number;
	priceTypeLevels: IPriceTypeLevelDTO[];
	priceTypes: IPayrollGroupPriceTypeDTO[];
}
export interface IPayrollGroupVacationGroupDTO {
	calculationType: TermGroup_VacationGroupCalculationType;
	created: Date;
	createdBy: string;
	isDefault: boolean;
	modified: Date;
	modifiedBy: string;
	name: string;
	payrollGroupId: number;
	payrollGroupVacationGroupId: number;
	state: SoeEntityState;
	type: number;
	vacationDaysHandleRule: TermGroup_VacationGroupVacationDaysHandleRule;
	vacationGroupId: number;
	vacationHandleRule: TermGroup_VacationGroupVacationHandleRule;
}
export interface IPayrollImportEmployeeDTO {
	employeeId: number;
	employeeInfo: string;
	payrollImportEmployeeId: number;
	payrollImportHeadId: number;
	schedule: IPayrollImportEmployeeScheduleDTO[];
	scheduleBreakQuantity: System.ITimeSpan;
	scheduleQuantity: System.ITimeSpan;
	scheduleRowCount: number;
	state: SoeEntityState;
	status: TermGroup_PayrollImportEmployeeStatus;
	statusName: string;
	transactionAmount: number;
	transactionQuantity: number;
	transactionRowCount: number;
	transactions: IPayrollImportEmployeeTransactionDTO[];
}
export interface IPayrollImportEmployeeScheduleDTO {
	date: Date;
	errorMessage: string;
	isBreak: boolean;
	payrollImportEmployeeId: number;
	payrollImportEmployeeScheduleId: number;
	quantity: number;
	startTime: Date;
	state: SoeEntityState;
	status: TermGroup_PayrollImportEmployeeScheduleStatus;
	statusName: string;
	stopTime: Date;
}
export interface IPayrollImportEmployeeTransactionAccountInternalDTO {
	accountCode: string;
	accountDimNr: number;
	accountId: number;
	accountName: string;
	accountNr: string;
	accountSIEDimNr: number;
	payrollImportEmployeeTransactionAccountInternalId: number;
	payrollImportEmployeeTransactionId: number;
}
export interface IPayrollImportEmployeeTransactionDTO {
	accountCode: string;
	accountInternals: IPayrollImportEmployeeTransactionAccountInternalDTO[];
	accountStdDimNr: number;
	accountStdId: number;
	accountStdName: string;
	accountStdNr: string;
	amount: number;
	code: string;
	date: Date;
	errorMessage: string;
	name: string;
	note: string;
	payrollImportEmployeeId: number;
	payrollImportEmployeeTransactionId: number;
	payrollImportEmployeeTransactionLinks: IPayrollImportEmployeeTransactionLinkDTO[];
	payrollProductId: number;
	quantity: number;
	startTime: Date;
	state: SoeEntityState;
	status: TermGroup_PayrollImportEmployeeTransactionStatus;
	statusName: string;
	stopTime: Date;
	timeCodeAdditionDeductionId: number;
	timeDeviationCauseId: number;
	type: TermGroup_PayrollImportEmployeeTransactionType;
	typeName: string;
}
export interface IPayrollImportEmployeeTransactionLinkDTO {
	attestStateColor: string;
	attestStateName: string;
	date: Date;
	employeeId: number;
	payrollImportEmployeeTransactionId: number;
	payrollImportEmployeeTransactionLinkId: number;
	productName: string;
	productNr: string;
	quantity: number;
	start: Date;
	state: SoeEntityState;
	stop: Date;
	timeBlockId: number;
	timePayrollTransactionId: number;
}
export interface IPayrollImportHeadDTO {
	actorCompanyId: number;
	checksum: string;
	comment: string;
	created: Date;
	createdBy: string;
	dateFrom: Date;
	dateTo: Date;
	employees: IPayrollImportEmployeeDTO[];
	errorMessage: string;
	file: number[];
	fileType: TermGroup_PayrollImportHeadFileType;
	fileTypeName: string;
	modified: Date;
	modifiedBy: string;
	name: string;
	nrOfEmployees: number;
	paymentDate: Date;
	payrollImportHeadId: number;
	state: SoeEntityState;
	status: TermGroup_PayrollImportHeadStatus;
	statusName: string;
	type: TermGroup_PayrollImportHeadType;
	typeName: string;
}
export interface IPayrollLevelDTO {
	actorCompanyId: number;
	code: string;
	created: Date;
	createdBy: string;
	description: string;
	externalCode: string;
	isActive: boolean;
	modified: Date;
	modifiedBy: string;
	name: string;
	nameAndDesc: string;
	payrollLevelId: number;
	state: SoeEntityState;
}
export interface IPayrollPriceFormulaDTO {
	actorCompanyId: number;
	code: string;
	created: Date;
	createdBy: string;
	description: string;
	formula: string;
	formulaPlain: string;
	isActive: boolean;
	modified: Date;
	modifiedBy: string;
	name: string;
	payrollPriceFormulaId: number;
	state: SoeEntityState;
}
export interface IPayrollPriceFormulaResultDTO {
	amount: number;
	formula: string;
	formulaExtracted: string;
	formulaNames: string;
	formulaOrigin: string;
	formulaPlain: string;
	payrollPriceFormulaId: number;
	payrollPriceTypeId: number;
}
export interface IPayrollPriceTypeAndFormulaDTO {
	id: number;
	name: string;
	payrollPriceFormulaId: number;
	payrollPriceTypeId: number;
}
export interface IPayrollPriceTypeDTO {
	actorCompanyId: number;
	code: string;
	conditionAgeYears: number;
	conditionEmployeedMonths: number;
	conditionExperienceMonths: number;
	created: Date;
	createdBy: string;
	description: string;
	modified: Date;
	modifiedBy: string;
	name: string;
	payrollPriceTypeId: number;
	periods: IPayrollPriceTypePeriodDTO[];
	state: SoeEntityState;
	type: number;
	typeName: string;
}
export interface IPayrollPriceTypeGridDTO {
	code: string;
	description: string;
	name: string;
	payrollPriceTypeId: number;
	typeName: string;
}
export interface IPayrollPriceTypePeriodDTO {
	amount: number;
	fromDate: Date;
	payrollPriceTypeId: number;
	payrollPriceTypePeriodId: number;
}
export interface IPayrollPriceTypeSelectionDTO extends IReportDataSelectionDTO {
	ids: number[];
	typeIds: number[];
}
export interface IPayrollProductDTO extends IProductDTO {
	averageCalculated: boolean;
	dontUseFixedAccounting: boolean;
	excludeInWorkTimeSummary: boolean;
	export: boolean;
	externalNumber: string;
	factor: number;
	includeAmountInExport: boolean;
	isAbsence: boolean;
	payed: boolean;
	payrollType: TermGroup_PayrollType;
	resultType: TermGroup_PayrollResultType;
	settings: IPayrollProductSettingDTO[];
	shortName: string;
	sysPayrollProductId: number;
	sysPayrollTypeLevel1: number;
	sysPayrollTypeLevel2: number;
	sysPayrollTypeLevel3: number;
	sysPayrollTypeLevel4: number;
	useInPayroll: boolean;
}
export interface IPayrollProductGridDTO {
	averageCalculated: boolean;
	excludeInWorkTimeSummary: boolean;
	export: boolean;
	externalNumber: string;
	factor: number;
	includeAmountInExport: boolean;
	isAbsence: boolean;
	isSelected: boolean;
	isVisible: boolean;
	name: string;
	number: string;
	numberSort: string;
	payed: boolean;
	payrollType: TermGroup_PayrollType;
	productId: number;
	resultType: TermGroup_PayrollResultType;
	resultTypeText: string;
	shortName: string;
	state: SoeEntityState;
	sysPayrollTypeLevel1: number;
	sysPayrollTypeLevel1Name: string;
	sysPayrollTypeLevel2: number;
	sysPayrollTypeLevel2Name: string;
	sysPayrollTypeLevel3: number;
	sysPayrollTypeLevel3Name: string;
	sysPayrollTypeLevel4: number;
	sysPayrollTypeLevel4Name: string;
	sysPayrollTypeName: string;
	useInPayroll: boolean;
}
export interface IPayrollProductPriceFormulaDTO {
	formulaName: string;
	fromDate: Date;
	payrollPriceFormulaId: number;
	payrollProductPriceFormulaId: number;
	payrollProductSettingId: number;
	toDate: Date;
}
export interface IPayrollProductPriceTypeAndFormulaDTO {
	amount: number;
	fromDate: Date;
	name: string;
	payrollPriceFormulaId: number;
	payrollPriceTypeId: number;
	payrollProductPriceFormulaId: number;
	payrollProductPriceTypeId: number;
	payrollProductPriceTypePeriodId: number;
}
export interface IPayrollProductPriceTypeDTO {
	payrollPriceTypeId: number;
	payrollProductPriceTypeId: number;
	payrollProductSettingId: number;
	periods: IPayrollProductPriceTypePeriodDTO[];
	priceTypeName: string;
	priceTypePeriods: IPayrollPriceTypePeriodDTO[];
}
export interface IPayrollProductPriceTypePeriodDTO {
	amount: number;
	fromDate: Date;
	payrollProductPriceTypeId: number;
	payrollProductPriceTypePeriodId: number;
}
export interface IPayrollProductRowSelectionDTO extends IReportDataSelectionDTO {
	payrollProductIds: number[];
	sysPayrollTypeLevel1: number;
	sysPayrollTypeLevel2: number;
	sysPayrollTypeLevel3: number;
	sysPayrollTypeLevel4: number;
}
export interface IPayrollProductSettingDTO {
	accountingPrio: string;
	accountingSettings: IAccountingSettingsRowDTO[];
	calculateSicknessSalary: boolean;
	calculateSupplementCharge: boolean;
	centRoundingLevel: TermGroup_PayrollProductCentRoundingLevel;
	centRoundingType: TermGroup_PayrollProductCentRoundingType;
	childProductId: number;
	dontIncludeInAbsenceCost: boolean;
	dontIncludeInRetroactivePayroll: boolean;
	dontPrintOnSalarySpecificationWhenZeroAmount: boolean;
	extraFields: IExtraFieldRecordDTO[];
	isReadOnly: boolean;
	isSelected: boolean;
	payrollGroupId: number;
	payrollGroupName: string;
	payrollProductSettingId: number;
	pensionCompany: TermGroup_PensionCompany;
	priceFormulas: IPayrollProductPriceFormulaDTO[];
	priceTypes: IPayrollProductPriceTypeDTO[];
	printDate: boolean;
	printOnSalarySpecification: boolean;
	productId: number;
	quantityRoundingMinutes: number;
	quantityRoundingType: TermGroup_PayrollProductQuantityRoundingType;
	taxCalculationType: TermGroup_PayrollProductTaxCalculationType;
	timeUnit: TermGroup_PayrollProductTimeUnit;
	unionFeePromoted: boolean;
	vacationSalaryPromoted: boolean;
	workingTimePromoted: boolean;
}
export interface IPayrollReviewEmployeeDTO {
	employeeId: number;
	employeeNr: string;
	employmentAmount: number;
	name: string;
	payrollGroupAmount: number;
	payrollGroupId: number;
	payrollLevelId: number;
	payrollPriceTypeId: number;
	readOnly: boolean;
	selectableLevels: IPayrollReviewSelectableLevelDTO[];
}
export interface IPayrollReviewHeadDTO {
	created: Date;
	createdBy: string;
	dateFrom: Date;
	modified: Date;
	modifiedBy: string;
	name: string;
	payrollGroupIds: number[];
	payrollGroupNames: string;
	payrollLevelIds: number[];
	payrollLevelNames: string;
	payrollPriceTypeIds: number[];
	payrollPriceTypeNames: string;
	payrollReviewHeadId: number;
	rows: IPayrollReviewRowDTO[];
	state: SoeEntityState;
	status: TermGroup_PayrollReviewStatus;
	statusName: string;
}
export interface IPayrollReviewRowDTO {
	adjustment: number;
	amount: number;
	employeeId: number;
	employeeName: string;
	employeeNr: string;
	employmentAmount: number;
	errorMessage: string;
	isModified: boolean;
	payrollGroupAmount: number;
	payrollGroupId: number;
	payrollGroupName: string;
	payrollLevelId: number;
	payrollLevelName: string;
	payrollPriceTypeId: number;
	payrollPriceTypeName: string;
	payrollReviewHeadId: number;
	payrollReviewRowId: number;
	readOnly: boolean;
	warningMessage: string;
}
export interface IPayrollReviewSelectableLevelDTO {
	amount: number;
	fromDate: Date;
	name: string;
	payrollLevelId: number;
}
export interface IPayrollStartValueHeadDTO {
	actorCompanyId: number;
	created: Date;
	createdBy: string;
	dateFrom: Date;
	dateTo: Date;
	importedFrom: string;
	payrollStartValueHeadId: number;
	rows: IPayrollStartValueRowDTO[];
}
export interface IPayrollStartValueRowDTO {
	absenceTimeMinutes: number;
	actorCompanyId: number;
	amount: number;
	appellation: string;
	date: Date;
	doCreateTransaction: boolean;
	employeeId: number;
	employeeNr: string;
	payrollStartValueHeadId: number;
	payrollStartValueRowId: number;
	productId: number;
	productName: string;
	productNr: string;
	productNrAndName: string;
	quantity: number;
	scheduleTimeMinutes: number;
	state: SoeEntityState;
	sysPayrollStartValueId: number;
	sysPayrollTypeLevel1: TermGroup_SysPayrollType;
	sysPayrollTypeLevel2: TermGroup_SysPayrollType;
	sysPayrollTypeLevel3: TermGroup_SysPayrollType;
	sysPayrollTypeLevel4: TermGroup_SysPayrollType;
	timePayrollTransactionId: number;
	transactionAmount: number;
	transactionComment: string;
	transactionDate: Date;
	transactionLevel1: TermGroup_SysPayrollType;
	transactionLevel2: TermGroup_SysPayrollType;
	transactionLevel3: TermGroup_SysPayrollType;
	transactionLevel4: TermGroup_SysPayrollType;
	transactionProductId: number;
	transactionProductName: string;
	transactionProductNr: string;
	transactionProductNrAndName: string;
	transactionQuantity: number;
	transactionUnitPrice: number;
}
export interface IPeriodCalculationResultDTO {
	employeeId: number;
	periods: ITimePeriodDTO[];
	currentPeriod: string;
	parentPeriod: string;
}
export interface IPersonalDataLogMessageDTO {
	actionTypeText: string;
	batch: System.IGuid;
	batchNbr: number;
	informationTypeText: string;
	message: string;
	timeStamp: Date;
	url: string;
	userName: string;
}
export interface IPositionDTO {
	actorCompanyId: number;
	code: string;
	created: Date;
	createdBy: string;
	description: string;
	modified: Date;
	modifiedBy: string;
	name: string;
	nameAndCode: string;
	positionId: number;
	positionSkills: IPositionSkillDTO[];
	state: SoeEntityState;
	sysPositionId: number;
}
export interface IPlanningPeriod {
	timePeriodId: number;
	name: string;
	startDate: Date;
	stopDate: Date;
}
export interface IPlanningPeriodHead {
	timePeriodHeadId: number;
	name: string;
	childId: number;
	childName: string;
	parentPeriods: IPlanningPeriod[];
	childPeriods: IPlanningPeriod[];
}
export interface IPositionGridDTO {
	code: string;
	description: string;
	name: string;
	positionId: number;
	sysPositionId: number;
}
export interface IPositionSkillDTO {
	missing: boolean;
	positionId: number;
	positionName: string;
	positionSkillId: number;
	skillId: number;
	skillLevel: number;
	skillLevelStars: number;
	skillLevelUnreached: boolean;
	skillName: string;
	skillTypeName: string;
}
export interface IPreviewActivateScenarioDTO {
	date: Date;
	employeeId: number;
	name: string;
	shiftTextScenario: string;
	shiftTextSchedule: string;
	statusMessage: string;
	statusName: string;
	workRule: SoeScheduleWorkRules;
	workRuleCanOverride: boolean;
	workRuleName: string;
	workRuleText: string;
}
export interface IPreviewCreateTemplateFromScenarioDTO {
	date: Date;
	employeeId: number;
	name: string;
	shiftTextScenario: string;
	templateDateFrom: Date;
	templateDateTo: Date;
	workRule: SoeScheduleWorkRules;
	workRuleCanOverride: boolean;
	workRuleName: string;
	workRuleText: string;
}
export interface IPriceBasedMarkupDTO {
	created: Date;
	createdBy: string;
	markupPercent: number;
	maxPrice: number;
	minPrice: number;
	modified: Date;
	modifiedBy: string;
	priceBasedMarkupId: number;
	priceListName: string;
	priceListTypeId: number;
	state: SoeEntityState;
}
export interface IPriceListDTO {
	created: Date;
	createdBy: string;
	modified: Date;
	modifiedBy: string;
	price: number;
	priceListId: number;
	priceListTypeId: number;
	productId: number;
	quantity: number;
	startDate: Date;
	state: SoeEntityState;
	stopDate: Date;
	sysPriceListTypeName: string;
}
export interface IPriceListTypeDTO {
	created: Date;
	createdBy: string;
	currencyId: number;
	description: string;
	discountPercent: number;
	inclusiveVat: boolean;
	isProjectPriceList: boolean;
	modified: Date;
	modifiedBy: string;
	name: string;
	priceListTypeId: number;
	state: SoeEntityState;
}
export interface IPriceRuleDTO {
	companyWholesellerPriceListId: number;
	lExampleType: number;
	lRule: IPriceRuleDTO;
	lRuleId: number;
	lValue: number;
	lValueType: number;
	modified: Date;
	modifiedBy: string;
	operatorType: number;
	priceListImportedHeadId: number;
	priceListTypeId: number;
	rExampleType: number;
	rRule: IPriceRuleDTO;
	rRuleId: number;
	ruleId: number;
	rValue: number;
	rValueType: number;
	useNetPrice: boolean;
}
export interface IPriceTypeLevelDTO {
	hasLevels: boolean;
	levelIsMandatory: boolean;
	payrollGroupId: number;
	payrollPriceTypeId: number;
	selectableLevelIds: number[];
}
export interface IProductAccountsItem {
	productId: number;
	purchaseAccountDim1Id: number;
	purchaseAccountDim1Name: string;
	purchaseAccountDim1Nr: string;
	purchaseAccountDim2Id: number;
	purchaseAccountDim2Name: string;
	purchaseAccountDim2Nr: string;
	purchaseAccountDim3Id: number;
	purchaseAccountDim3Name: string;
	purchaseAccountDim3Nr: string;
	purchaseAccountDim4Id: number;
	purchaseAccountDim4Name: string;
	purchaseAccountDim4Nr: string;
	purchaseAccountDim5Id: number;
	purchaseAccountDim5Name: string;
	purchaseAccountDim5Nr: string;
	purchaseAccountDim6Id: number;
	purchaseAccountDim6Name: string;
	purchaseAccountDim6Nr: string;
	rowId: number;
	salesAccountDim1Id: number;
	salesAccountDim1Name: string;
	salesAccountDim1Nr: string;
	salesAccountDim2Id: number;
	salesAccountDim2Name: string;
	salesAccountDim2Nr: string;
	salesAccountDim3Id: number;
	salesAccountDim3Name: string;
	salesAccountDim3Nr: string;
	salesAccountDim4Id: number;
	salesAccountDim4Name: string;
	salesAccountDim4Nr: string;
	salesAccountDim5Id: number;
	salesAccountDim5Name: string;
	salesAccountDim5Nr: string;
	salesAccountDim6Id: number;
	salesAccountDim6Name: string;
	salesAccountDim6Nr: string;
	vatAccountDim1Id: number;
	vatAccountDim1Name: string;
	vatAccountDim1Nr: string;
	vatRate: number;
}
export interface IProductComparisonDTO extends IProductSmallDTO {
	comparisonPrice: number;
	price: number;
	purchasePrice: number;
	startDate: Date;
	stopDate: Date;
}
export interface IProductDTO extends IProductSmallDTO {
	accountingPrio: string;
	created: Date;
	createdBy: string;
	description: string;
	modified: Date;
	modifiedBy: string;
	productGroupId: number;
	productUnitCode: string;
	productUnitId: number;
	state: SoeEntityState;
	type: SoeProductType;
}
export interface IProductPricesRequestDTO {
	checkProduct: boolean;
	copySysProduct: boolean;
	currencyId: number;
	customerId: number;
	includeCustomerPrices: boolean;
	priceListTypeId: number;
	products: IProductPricesRowRequestDTO[];
	returnFormula: boolean;
	timeRowIsLoadingProductPrice: boolean;
	wholesellerId: number;
}
export interface IProductPricesRowRequestDTO {
	productId: number;
	purchasePrice: number;
	quantity: number;
	tempRowId: number;
	wholesellerName: string;
}
export interface IProductRowDTO {
	amount: number;
	amountCurrency: number;
	attestStateId: number;
	created: Date;
	createdBy: string;
	customerInvoiceInterestId: number;
	customerInvoiceReminderId: number;
	customerInvoiceRowId: number;
	date: Date;
	dateTo: Date;
	deliveryDateText: string;
	discountAmount: number;
	discountAmountCurrency: number;
	discountPercent: number;
	discountType: number;
	discountValue: number;
	ediEntryId: number;
	householdAmount: number;
	householdAmountCurrency: number;
	householdApartmentNbr: string;
	householdCooperativeOrgNbr: string;
	householdDeductionType: number;
	householdName: string;
	householdProperty: string;
	householdSocialSecNbr: string;
	householdTaxDeductionType: TermGroup_HouseHoldTaxDeductionType;
	intrastatCodeId: number;
	intrastatTransactionId: number;
	invoiceQuantity: number;
	isCentRoundingRow: boolean;
	isClearingProduct: boolean;
	isContractProduct: boolean;
	isExpenseRow: boolean;
	isFixedPriceProduct: boolean;
	isFreightAmountRow: boolean;
	isInterestRow: boolean;
	isInvoiceFeeRow: boolean;
	isLiftProduct: boolean;
	isReminderRow: boolean;
	isStockRow: boolean;
	isSupplementChargeProduct: boolean;
	isTimeBillingRow: boolean;
	isTimeProjectRow: boolean;
	marginalIncome: number;
	marginalIncomeCurrency: number;
	marginalIncomeRatio: number;
	mergeToId: number;
	modified: Date;
	modifiedBy: string;
	parentRowId: number;
	prevCustomerInvoiceRowId: number;
	previouslyInvoicedQuantity: number;
	productId: number;
	productUnitId: number;
	purchasePrice: number;
	purchasePriceCurrency: number;
	quantity: number;
	rowNr: number;
	splitAccountingRows: ISplitAccountingRowDTO[];
	state: SoeEntityState;
	stockCode: string;
	stockId: number;
	sumAmount: number;
	sumAmountCurrency: number;
	supplierInvoiceId: number;
	sysCountryId: number;
	sysWholesellerName: string;
	tempRowId: number;
	text: string;
	timeManuallyChanged: boolean;
	type: SoeInvoiceRowType;
	vatAccountId: number;
	vatAmount: number;
	vatAmountCurrency: number;
	vatCodeId: number;
	vatRate: number;
}
export interface IProductRowsProductDTO {
	calculationType: TermGroup_InvoiceProductCalculationType;
	description: string;
	dontUseDiscountPercent: boolean;
	grossMarginCalculationType: TermGroup_GrossMarginCalculationType;
	guaranteePercentage: number;
	householdDeductionPercentage: number;
	householdDeductionType: number;
	intrastatCodeId: number;
	isExternal: boolean;
	isInactive: boolean;
	isLiftProduct: boolean;
	isStockProduct: boolean;
	isSupplementCharge: boolean;
	name: string;
	number: string;
	productId: number;
	productUnitCode: string;
	productUnitId: number;
	purchasePrice: number;
	salesPrice: number;
	showDescriptionAsTextRow: boolean;
	showDescrAsTextRowOnPurchase: boolean;
	sysCountryId: number;
	sysProductId: number;
	sysWholesellerName: string;
	vatCodeId: number;
	vatType: TermGroup_InvoiceProductVatType;
	weight: number;
}
export interface IProductSmallDTO {
	name: string;
	number: string;
	numberName: string;
	productId: number;
}
export interface IProductUnitConvertDTO {
	baseProductUnitId: number;
	baseProductUnitName: string;
	convertFactor: number;
	isDeleted: boolean;
	isModified: boolean;
	productId: number;
	productName: string;
	productNr: string;
	productUnitConvertId: number;
	productUnitId: number;
	productUnitName: string;
}
export interface IProductUnitDTO {
	actorCompanyId: number;
	code: string;
	created: Date;
	createdBy: string;
	modified: Date;
	modifiedBy: string;
	name: string;
	productUnitId: number;
}
export interface IProductUnitSmallDTO {
	code: string;
	name: string;
	productUnitId: number;
}
export interface IProjectCentralStatusDTO {
	actorName: string;
	associatedId: number;
	budget: number;
	budgetTime: number;
	costTypeName: string;
	date: Date;
	description: string;
	diff: number;
	diff2: number;
	employeeId: number;
	employeeName: string;
	groupRowType: ProjectCentralHeaderGroupType;
	groupRowTypeName: string;
	hasInfo: boolean;
	info: string;
	isEditable: boolean;
	isVisible: boolean;
	modified: string;
	modifiedBy: string;
	name: string;
	originType: SoeOriginType;
	rowType: ProjectCentralStatusRowType;
	type: ProjectCentralBudgetRowType;
	typeName: string;
	value: number;
	value2: number;
}
export interface IProjectDTO {
	accountingSettings: IAccountingSettingsRowDTO[];
	actorCompanyId: number;
	allocationType: TermGroup_ProjectAllocationType;
	attestWorkFlowHeadId: number;
	budgetHead: IBudgetHeadDTO;
	budgetId: number;
	created: Date;
	createdBy: string;
	customerId: number;
	defaultDim1AccountId: number;
	defaultDim2AccountId: number;
	defaultDim3AccountId: number;
	defaultDim4AccountId: number;
	defaultDim5AccountId: number;
	defaultDim6AccountId: number;
	description: string;
	invoiceId: number;
	modified: Date;
	modifiedBy: string;
	name: string;
	note: string;
	number: string;
	parentProjectId: number;
	priceListTypeId: number;
	projectId: number;
	startDate: Date;
	state: SoeEntityState;
	status: TermGroup_ProjectStatus;
	statusName: string;
	stopDate: Date;
	type: TermGroup_ProjectType;
	useAccounting: boolean;
	workSiteKey: string;
	workSiteNumber: string;
}
export interface IProjectGridDTO {
	categories: string;
	childProjects: string;
	customerId: number;
	customerName: string;
	customerNr: string;
	defaultDim2AccountName: string;
	defaultDim3AccountName: string;
	defaultDim4AccountName: string;
	defaultDim5AccountName: string;
	defaultDim6AccountName: string;
	description: string;
	isSelected: boolean;
	isVisible: boolean;
	loadOnlyPlannedAndActive: boolean;
	loadOrders: boolean;
	managerName: string;
	managerUserId: number;
	name: string;
	number: string;
	orderNr: string;
	parentProjectId: number;
	projectId: number;
	projectsWithoutCustomer: boolean;
	startDate: Date;
	state: SoeEntityState;
	status: TermGroup_ProjectStatus;
	statusName: string;
	stopDate: Date;
}

export interface IProjectTinyDTO {
	projectId: number;
	number: string;
	name: string;
	status: TermGroup_ProjectStatus;
	parentProjectId: number;
	useAccounting: boolean;
}
export interface IProjectInvoiceSmallDTO {
	customerName: string;
	invoiceId: number;
	invoiceNr: string;
	numberName: string;
	projectId: number;
}
export interface IProjectSmallDTO {
	allocationType: TermGroup_ProjectAllocationType;
	customerId: number;
	customerName: string;
	customerNumber: string;
	invoices: IProjectInvoiceSmallDTO[];
	name: string;
	number: string;
	numberName: string;
	projectEmployees: number[];
	projectId: number;
	projectUsers: number[];
	timeCodeId: number;
}
export interface IProjectTimeBlockDTO {
	additionalTime: boolean;
	allocationType: TermGroup_ProjectAllocationType;
	created: Date;
	createdBy: string;
	customerId: number;
	customerInvoiceId: number;
	customerInvoiceRowAttestStateColor: string;
	customerInvoiceRowAttestStateId: number;
	customerInvoiceRowAttestStateName: string;
	customerName: string;
	date: Date;
	employeeChildId: number;
	employeeChildName: string;
	employeeId: number;
	employeeIsInactive: boolean;
	employeeName: string;
	employeeNr: string;
	externalNote: string;
	hasComment: boolean;
	internalNote: string;
	internOrderText: string;
	invoiceNr: string;
	invoiceQuantity: number;
	isEditable: boolean;
	isPayrollEditable: boolean;
	isSalaryPayrollType: boolean;
	mandatoryTime: boolean;
	modified: Date;
	modifiedBy: string;
	month: string;
	orderClosed: boolean;
	projectId: number;
	projectInvoiceWeekId: number;
	projectName: string;
	projectNr: string;
	projectTimeBlockId: number;
	referenceOur: string;
	registrationType: OrderInvoiceRegistrationType;
	scheduledQuantity: number;
	showInvoiceRowAttestState: boolean;
	showPayrollAttestState: boolean;
	startTime: Date;
	stopTime: Date;
	timeBlockDateId: number;
	timeCodeId: number;
	timeCodeName: string;
	timeDeviationCauseId: number;
	timeDeviationCauseName: string;
	timeInvoiceTransactionId: number;
	timePayrollAttestStateColor: string;
	timePayrollAttestStateId: number;
	timePayrollAttestStateName: string;
	timePayrollQuantity: number;
	timePayrollTransactionIds: number[];
	timeSheetWeekId: number;
	week: string;
	weekDay: string;
	year: string;
	yearMonth: string;
	yearWeek: string;
}
export interface IProjectTimeBlockSaveDTO {
	actorCompanyId: number;
	autoGenTimeAndBreakForProject: boolean;
	customerInvoiceId: number;
	date: Date;
	employeeChildId: number;
	employeeId: number;
	externalNote: string;
	from: Date;
	internalNote: string;
	invoiceQuantity: number;
	isFromTimeSheet: boolean;
	mandatoryTime: boolean;
	projectId: number;
	projectInvoiceDayId: number;
	projectInvoiceWeekId: number;
	projectTimeBlockId: number;
	state: SoeEntityState;
	timeBlockDateId: number;
	timeCodeId: number;
	timeDeviationCauseId: number;
	timePayrollQuantity: number;
	timeSheetWeekId: number;
	to: Date;
}
export interface IProjectTimeMatrixDTO {
	customerId: number;
	customerInvoiceId: number;
	customerName: string;
	employeeId: number;
	invoiceNr: string;
	projectId: number;
	projectInvoiceWeekId: number;
	projectName: string;
	projectNr: string;
	rows: IProjectTimeMatrixSaveRowDTO[];
	timeCodeId: number;
	timeCodeName: string;
	timeDeviationCauseId: number;
	timeDeviationCauseName: string;
}
export interface IProjectTimeMatrixSaveDTO {
	customerInvoiceId: number;
	employeeId: number;
	isDeleted: boolean;
	projectId: number;
	projectInvoiceWeekId: number;
	rows: IProjectTimeMatrixSaveRowDTO[];
	timeCodeId: number;
	timeDeviationCauseId: number;
	weekDate: Date;
}
export interface IProjectTimeMatrixSaveRowDTO {
	employeeChildId: number;
	externalNote: string;
	internalNote: string;
	invoiceQuantity: number;
	invoiceStateColor: string;
	isInvoiceEditable: boolean;
	isPayrollEditable: boolean;
	payrollQuantity: number;
	payrollStateColor: string;
	projectTimeBlockId: number;
	weekDay: number;
}
export interface IProjectTraceViewDTO {
	amount: number;
	amountCurrency: number;
	billingType: TermGroup_BillingType;
	billingTypeName: string;
	contractId: number;
	currencyCode: string;
	currencyRate: number;
	customerInvoiceId: number;
	date: Date;
	description: string;
	isContract: boolean;
	isCustomerInvoice: boolean;
	isOffer: boolean;
	isOrder: boolean;
	isPayment: boolean;
	isSupplierInvoice: boolean;
	langId: number;
	number: string;
	offerId: number;
	orderId: number;
	originStatus: SoeOriginStatus;
	originStatusName: string;
	originType: SoeOriginType;
	originTypeName: string;
	paymentRowId: number;
	paymentStatusId: SoePaymentStatus;
	paymentStatusName: string;
	projectId: number;
	state: SoeEntityState;
	supplierInvoiceId: number;
	sysCurrencyId: number;
	vatAmount: number;
	vatAmountCurrency: number;
}
export interface IProjectUserDTO {
	created: Date;
	createdBy: string;
	dateFrom: Date;
	dateTo: Date;
	employeeCalculatedCost: number;
	internalId: number;
	modified: Date;
	modifiedBy: string;
	name: string;
	projectId: number;
	projectUserId: number;
	state: SoeEntityState;
	timeCodeId: number;
	timeCodeName: string;
	type: TermGroup_ProjectUserType;
	typeName: string;
	userId: number;
}
export interface IProjectWeekTotal {
	employeeId: number;
	invoiceProductId: number;
	invoiceTimeInMinutes: number;
	timeCodeId: number;
	weekNumber: number;
	workTimeInMinutes: number;
	year: number;
}
export interface IPriceListTypeMarkupDTO {
	priceListTypeId: number;
	markup: number;
}
export interface IReportPrintDTO {
	reportId: number;
	ids: number[];
	queue: boolean;
}

export interface IProjectPrintDTO extends IReportPrintDTO {
	reportId: number;
	sysReportTemplateTypeId: SoeReportTemplateType;
	includeChildProjects: boolean;
	dateFrom: Date;
	dateTo: Date;
}

export interface IProjectTimeBookPrintDTO extends IReportPrintDTO {
	invoiceId: number;
	projectId: number;
	dateFrom: Date;
	dateTo: Date;
}

export interface IBalanceListPrintDTO extends IReportPrintDTO {
	companySettingType: CompanySettingType;
}

export interface IHouseholdTaxDeductionPrintDTO extends IReportPrintDTO {
	sequenceNumber: number;
	sysReportTemplateTypeId: SoeReportTemplateType;
	useGreen: boolean;
}

export interface ICustomerInvoicePrintDTO extends IReportPrintDTO {
	sysReportTemplateTypeId: SoeReportTemplateType;
	attachmentIds: number[];
	checklistIds: number[];
	printTimeReport: boolean;
	includeOnlyInvoiced: boolean;
	orderInvoiceRegistrationType: OrderInvoiceRegistrationType;
	invoiceCopy: boolean;
	asReminder: boolean;
	mergePdfs: boolean;
	reportLanguageId?: number;
}

export interface IPurchaseDeliveryDTO {
	created: Date;
	createdBy: string;
	deliveryDate: Date;
	deliveryNr: number;
	modified: Date;
	modifiedBy: string;
	purchaseDeliveryId: number;
	supplierId: number;
	supplierName: string;
	supplierNr: string;
}
export interface IPurchaseDeliveryGridDTO {
	created: Date;
	deliveryDate: Date;
	deliveryNr: number;
	purchaseDeliveryId: number;
	purchaseNr: string;
	supplierName: string;
	supplierNr: string;
}
export interface IPurchaseDeliveryInvoiceDTO {
	askedPrice: number;
	deliveredQuantity: number;
	isDeleted: boolean;
	linkToInvoice: boolean;
	price: number;
	productId: number;
	productName: string;
	productNumber: string;
	purchaseDeliveryInvoiceId: number;
	purchaseId: number;
	purchaseNr: string;
	purchaseQuantity: number;
	purchaseRowDisplayName: string;
	purchaseRowId: number;
	purchaseRowNr: number;
	quantity: number;
	supplierinvoiceId: number;
	supplierInvoiceSeqNr: number;
	supplierProductId: number;
	supplierProductName: string;
	supplierProductNr: string;
	text: string;
}
export interface IPurchaseDeliveryRowDTO {
	deliveredQuantity: number;
	deliveryDate: Date;
	isLocked: boolean;
	modified: Date;
	modifiedBy: string;
	productName: string;
	productNr: string;
	purchaseDeliveryId: number;
	purchaseDeliveryRowId: number;
	purchaseNr: string;
	purchasePrice: number;
	purchasePriceCurrency: number;
	purchaseQuantity: number;
	purchaseRowId: number;
	remainingQuantity: number;
	state: SoeEntityState;
	stockCode: string;
	tempRowId: number;
}
export interface IPurchaseDeliverySaveDTO {
	deliveryDate: Date;
	purchaseDeliveryId: number;
	rows: IPurchaseDeliverySaveRowDTO[];
	supplierId: number;
}
export interface IPurchaseDeliverySaveRowDTO {
	deliveredQuantity: number;
	deliveryDate: Date;
	isModified: boolean;
	purchaseDeliveryRowId: number;
	purchaseNr: string;
	purchasePrice: number;
	purchasePriceCurrency: number;
	purchaseRowId: number;
	setRowAsDelivered: boolean;
}
export interface IPurchaseDTO {
	confirmedDeliveryDate: Date;
	contactEComId: number;
	created: Date;
	createdBy: string;
	currencyDate: Date;
	currencyId: number;
	currencyRate: number;
	defaultDim1AccountId: number;
	defaultDim2AccountId: number;
	defaultDim3AccountId: number;
	defaultDim4AccountId: number;
	defaultDim5AccountId: number;
	defaultDim6AccountId: number;
	deliveryAddress: string;
	deliveryAddressId: number;
	deliveryConditionId: number;
	deliveryTypeId: number;
	modified: Date;
	modifiedBy: string;
	orderId: number;
	orderNr: string;
	origindescription: string;
	originStatus: SoeOriginStatus;
	originUsers: IOriginUserSmallDTO[];
	paymentConditionId: number;
	projectId: number;
	projectNr: string;
	purchaseDate: Date;
	purchaseId: number;
	purchaseLabel: string;
	purchaseNr: string;
	purchaseRows: IPurchaseRowDTO[];
	referenceOur: string;
	referenceYour: string;
	statusName: string;
	supplierCustomerNr: string;
	supplierEmail: string;
	supplierId: number;
	totalAmountCurrency: number;
	totalAmountExVatCurrency: number;
	vatAmountCurrency: number;
	vatType: number;
	wantedDeliveryDate: Date;
}
export interface IPurchaseGridDTO {
	confirmedDate: Date;
	currencyCode: string;
	deliveryDate: Date;
	deliveryStatus: PurchaseDeliveryStatus;
	origindescription: string;
	originStatus: SoeOriginStatus;
	projectNr: string;
	purchaseDate: Date;
	purchaseId: number;
	purchaseNr: string;
	statusIcon: number;
	statusName: string;
	supplierName: string;
	supplierNr: string;
	sysCurrencyId: number;
	totalAmount: number;
	totalAmountExVat: number;
	totalAmountExVatCurrency: number;
}
export interface IPurchaseRowDTO {
	accDeliveryDate: Date;
	customerInvoiceRowIds: number[];
	deliveredQuantity: number;
	deliveryDate: Date;
	discountAmount: number;
	discountAmountCurrency: number;
	discountPercent: number;
	discountType: number;
	intrastatCodeId: number;
	intrastatTransactionId: number;
	isLocked: boolean;
	modified: Date;
	modifiedBy: string;
	orderId: number;
	orderNr: string;
	parentRowId: number;
	productId: number;
	productName: string;
	productNr: string;
	purchaseId: number;
	purchaseNr: string;
	purchasePrice: number;
	purchasePriceCurrency: number;
	purchaseRowId: number;
	purchaseUnitId: number;
	quantity: number;
	rowNr: number;
	state: SoeEntityState;
	status: number;
	statusName: string;
	stockCode: string;
	stockId: number;
	sumAmount: number;
	sumAmountCurrency: number;
	supplierProductId: number;
	supplierProductNr: string;
	sysCountryId: number;
	tempRowId: number;
	text: string;
	type: PurchaseRowType;
	vatAmount: number;
	vatAmountCurrency: number;
	vatCodeCode: string;
	vatCodeId: number;
	vatCodeName: string;
	vatRate: number;
	wantedDeliveryDate: Date;
}
export interface IPurchaseRowFromStockDTO {
	availableStockQuantity: number;
	currencyCode: string;
	currencyId: number;
	deliveryAddress: string;
	deliveryLeadTimeDays: number;
	discountPercentage: number;
	exclusivePurchase: boolean;
	multipleSupplierMatches: boolean;
	packSize: number;
	price: number;
	productId: number;
	productName: string;
	productNr: string;
	productUnitCode: string;
	purchasedQuantity: number;
	purchaseId: number;
	purchaseNr: string;
	quantity: number;
	referenceOur: string;
	requestedDeliveryDate: Date;
	reservedStockQauntity: number;
	stockId: number;
	stockName: string;
	stockPurchaseQuantity: number;
	stockPurchaseTriggerQuantity: number;
	sum: number;
	supplierId: number;
	supplierName: string;
	supplierNr: string;
	supplierProductId: number;
	supplierUnitCode: string;
	supplierUnitId: number;
	tempId: number;
	totalStockQuantity: number;
	unitCode: string;
	unitId: number;
	vatAmount: number;
	vatCodeId: number;
	vatRate: number;
}
export interface IPurchaseRowGridDTO {
	confirmedDate: Date;
	customerInvoiceRowCount: number;
	deliveredQuantity: number;
	deliveryDate: Date;
	productNr: string;
	purchaseId: number;
	purchaseNr: string;
	purchaseQuantity: number;
	purchaseRowId: number;
	purchaseStatus: number;
	purchaseStatusName: string;
	remainingQuantity: number;
	requestedDate: Date;
	rowStatus: number;
	rowStatusName: string;
	supplier: string;
	supplierName: string;
	supplierNr: string;
	text: string;
	unit: string;
}
export interface IPurchaseRowSmallDTO {
	deliveredQuantity: number;
	displayName: string;
	price: number;
	productId: number;
	productName: string;
	productNumber: string;
	purchaseRowId: number;
	purchaseRowNr: number;
	supplierProductId: number;
	supplierProductName: string;
	supplierProductNr: string;
	text: string;
}
export interface IPurchaseSmallDTO {
	displayName: string;
	name: string;
	originDescription: string;
	purchaseId: number;
	purchaseNr: string;
	status: number;
	supplierId: number;
	supplierName: string;
	supplierNr: string;
}
export interface IRecalculateTimeHeadDTO {
	action: SoeRecalculateTimeHeadAction;
	actorCompanyId: number;
	created: Date;
	createdBy: string;
	excecutedStartTime: Date;
	excecutedStopTime: Date;
	recalculateTimeHeadId: number;
	records: IRecalculateTimeRecordDTO[];
	startDate: Date;
	status: TermGroup_RecalculateTimeHeadStatus;
	statusName: string;
	stopDate: Date;
	userId: number;
}
export interface IRecalculateTimeRecordDTO {
	employeeId: number;
	employeeName: string;
	errorMsg: string;
	recalculateTimeHeadId: number;
	recalculateTimeRecordId: number;
	recalculateTimeRecordStatus: TermGroup_RecalculateTimeRecordStatus;
	startDate: Date;
	statusName: string;
	stopDate: Date;
	warningMsg: string;
}
export interface IReportDataSelectionDTO {
	key: string;
	typeName: string;
}
export interface IReportDTO {
	actorCompanyId: number;
	created: Date;
	createdBy: string;
	description: string;
	detailedInformation: boolean;
	exportFileType: TermGroup_ReportExportFileType;
	exportType: TermGroup_ReportExportType;
	filePath: string;
	groupByLevel1: TermGroup_ReportGroupAndSortingTypes;
	groupByLevel2: TermGroup_ReportGroupAndSortingTypes;
	groupByLevel3: TermGroup_ReportGroupAndSortingTypes;
	groupByLevel4: TermGroup_ReportGroupAndSortingTypes;
	importCompanyId: number;
	importReportId: number;
	includeAllHistoricalData: boolean;
	includeBudget: boolean;
	isNewGroupsAndHeaders: boolean;
	isSortAscending: boolean;
	modified: Date;
	modifiedBy: string;
	module: SoeModule;
	name: string;
	noOfYearsBackinPreviousYear: number;
	original: boolean;
	reportId: number;
	reportNr: number;
	reportSelectionDate: IReportSelectionDateDTO[];
	reportSelectionId: number;
	reportSelectionInt: IReportSelectionIntDTO[];
	reportSelectionStr: IReportSelectionStrDTO[];
	reportSelectionText: string;
	reportTemplateId: number;
	roleIds: number[];
	showInAccountingReports: boolean;
	sortByLevel1: TermGroup_ReportGroupAndSortingTypes;
	sortByLevel2: TermGroup_ReportGroupAndSortingTypes;
	sortByLevel3: TermGroup_ReportGroupAndSortingTypes;
	sortByLevel4: TermGroup_ReportGroupAndSortingTypes;
	special: string;
	settings: IReportSettingDTO[];
	standard: boolean;
	state: SoeEntityState;
	nrOfDecimals?: number;
	showRowsByAccount: boolean;
	sysReportTemplateTypeId: number;
	sysReportTemplateTypeSelectionType: number;
	reportTemplateSettings: IReportTemplateSettingDTO[];
}

export interface IReportSettingDTO {
	reportSettingId: number;
	reportId: number;
	type: TermGroup_ReportSettingType;
	dataTypeId: SettingDataType
	value: any;
	intData: number;
	boolData: boolean;
	strData: string;
}

export interface IReportGroupsDrilldownDTO {
	budgetPeriodAmount: number;
	budgetToPeriodEndAmount: number;
	openingBalance: number;
	periodAmount: number;
	periodBudgetDiff: number;
	periodPrevPeriodDiff: number;
	prevPeriodAmount: number;
	prevYearAmount: number;
	reportGroupName: string;
	reportGroupOrder: number;
	reportHeaders: IReportHeadersDrilldownDTO[];
	yearAmount: number;
	yearBudgetDiff: number;
	yearPrevYearDiff: number;
}
export interface IReportHeadersDrilldownDTO {
	accounts: IAccountsDrilldownDTO[];
	budgetPeriodAmount: number;
	budgetToPeriodEndAmount: number;
	openingBalance: number;
	periodAmount: number;
	periodBudgetDiff: number;
	periodPrevPeriodDiff: number;
	prevPeriodAmount: number;
	prevYearAmount: number;
	reportHeaderName: string;
	reportHeaderOrder: number;
	yearAmount: number;
	yearBudgetDiff: number;
	yearPrevYearDiff: number;
}
export interface IReportItemDTO {
	defaultExportType: ISmallGenericType;
	description: string;
	exportFileType: number;
	includeBudget: boolean;
	reportId: number;
	supportedExportTypes: ISmallGenericType[];
}
export interface IReportJobDefinitionDTO {
	exportType: TermGroup_ReportExportType;
	forceValidation: boolean;
	langId: number;
	reportId: number;
	selections: IReportDataSelectionDTO[];
	sysReportTemplateTypeId: SoeReportTemplateType;
}
export interface IReportJobStatusDTO {
	exportType: TermGroup_ReportExportType;
	name: string;
	printoutDelivered: Date;
	printoutErrorMessage: string;
	printoutRequested: Date;
	printoutStatus: TermGroup_ReportPrintoutStatus;
	reportPrintoutId: number;
	sysReportTemplateTypeId: SoeReportTemplateType;
}
export interface IReportMenuDTO {
	active: boolean;
	description: string;
	groupName: string;
	groupOrder: number;
	isCompanyTemplate: boolean;
	isFavorite: boolean;
	isStandard: boolean;
	module: SoeModule;
	name: string;
	noPrintPermission: boolean;
	noRolesSpecified: boolean;
	printableFromMenu: boolean;
	reportId: number;
	reportNr: number;
	reportTemplateId: number;
	sysReportTemplateTypeId: number;
	sysReportType: SoeReportType;
}
export interface IReportPrintoutDTO {
	actorCompanyId: number;
	cleanedTime: Date;
	created: Date;
	createdBy: string;
	data: number[];
	datas: number[];
	deliveredTime: Date;
	deliveryType: TermGroup_ReportPrintoutDeliveryType;
	emailFileName: string;
	emailMessage: string;
	emailRecipients: number[];
	emailTemplateId: number;
	exportFormat: SoeExportFormat;
	exportType: TermGroup_ReportExportType;
	forceValidation: boolean;
	isEmailValid: boolean;
	json: string;
	modified: Date;
	modifiedBy: string;
	orderedDeliveryTime: Date;
	reportFileName: string;
	reportFileType: string;
	reportId: number;
	reportName: string;
	reportPackageId: number;
	reportPrintoutId: number;
	reportTemplateId: number;
	reportUrlId: number;
	resultMessage: number;
	resultMessageDetails: string;
	roleId: number;
	selection: string;
	singleRecipient: string;
	status: number;
	sysReportTemplateTypeId: number;
	userId: number;
	xml: string;
	xMLCompressed: number[];
}
export interface IReportSelectionDateDTO {
	order: number;
	reportSelectionDateId: number;
	reportSelectionId: number;
	reportSelectionType: SoeSelectionData;
	selectFrom: Date;
	selectGroup: number;
	selectTo: Date;
}
export interface IReportSelectionIntDTO {
	order: number;
	reportSelectionId: number;
	reportSelectionIntId: number;
	reportSelectionType: SoeSelectionData;
	selectFrom: number;
	selectGroup: number;
	selectTo: number;
}
export interface IReportSelectionStrDTO {
	order: number;
	reportSelectionId: number;
	reportSelectionStrId: number;
	reportSelectionType: SoeSelectionData;
	selectFrom: string;
	selectGroup: number;
	selectTo: string;
}
export interface IReportSmallDTO {
	name: string;
	reportId: number;
	reportNr: number;
}
export interface IReportTemplateDTO {
	actorCompanyId: number;
	created: Date;
	createdBy: string;
	description: string;
	fileName: string;
	groupByLevel1: number;
	groupByLevel2: number;
	groupByLevel3: number;
	groupByLevel4: number;
	isSortAscending: boolean;
	isSystem: boolean;
	modified: Date;
	modifiedBy: string;
	module: SoeModule;
	name: string;
	reportNr: number;
	reportTemplateId: number;
	showGroupingAndSorting: boolean;
	showOnlyTotals: boolean;
	sortByLevel1: number;
	sortByLevel2: number;
	sortByLevel3: number;
	sortByLevel4: number;
	special: string;
	state: SoeEntityState;
	sysCountryIds: number[];
	sysReportTemplateTypeId: number;
	sysReportTemplateTypeName: string;
	sysReportTypeId: number;
	validExportTypes: number[];
	reportTemplateSettings: IReportTemplateSettingDTO[];
}
export interface IReportTemplateSettingDTO {
	reportTemplateSettingId: number;
	reportTemplateId: number;
	settingField: number;
	settingType: number;
	settingValue: string;
	isModified: boolean;
	state: SoeEntityState;
}
export interface IReportUserSelectionAccessDTO {
	created: Date;
	createdBy: string;
	messageGroupId: number;
	modified: Date;
	modifiedBy: string;
	reportUserSelectionAccessId: number;
	reportUserSelectionId: number;
	roleId: number;
	state: SoeEntityState;
	type: TermGroup_ReportUserSelectionAccessType;
}
export interface IReportUserSelectionDTO {
	access: IReportUserSelectionAccessDTO[];
	actorCompanyId: number;
	description: string;
	name: string;
	reportId: number;
	reportUserSelectionId: number;
	scheduledJobHeadId: number;
	selections: IReportDataSelectionDTO[];
	state: SoeEntityState;
	type: ReportUserSelectionType;
	userId: number;
	validForScheduledJobHead: boolean;
}
export interface IReportViewDTO {
	actorCompanyId: number;
	exportType: number;
	reportDescription: string;
	reportId: number;
	reportName: string;
	reportNameDesc: string;
	reportNr: number;
	reportSelectionId: number;
	showInAccountingReports: boolean;
	sysReportTemplateTypeId: number;
	sysReportTypeName: string;
}
export interface IReportViewGridDTO extends IReportViewDTO {
	exportTypeName: string;
	isMigrated: boolean;
	original: boolean;
	reportSelectionText: string;
	roleNames: string;
	selectionType: SoeSelectionType;
	standard: boolean;
}
export interface IRetroactivePayrollAccountDTO {
	accountDim: IAccountDimDTO;
	accountDimId: number;
	accountId: number;
	created: Date;
	createdBy: string;
	modified: Date;
	modifiedBy: string;
	retroactivePayrollAccountId: number;
	retroactivePayrollId: number;
	state: SoeEntityState;
	type: TermGroup_RetroactivePayrollAccountType;
}
export interface IRetroactivePayrollDTO {
	actorCompanyId: number;
	created: Date;
	createdBy: string;
	dateFrom: Date;
	dateTo: Date;
	modified: Date;
	modifiedBy: string;
	name: string;
	note: string;
	nrOfEmployees: number;
	retroactivePayrollAccounts: IRetroactivePayrollAccountDTO[];
	retroactivePayrollEmployees: IRetroactivePayrollEmployeeDTO[];
	retroactivePayrollId: number;
	state: SoeEntityState;
	status: TermGroup_SoeRetroactivePayrollStatus;
	statusName: string;
	timePeriodHeadId: number;
	timePeriodHeadName: string;
	timePeriodId: number;
	timePeriodName: string;
	timePeriodPaymentDate: Date;
	totalAmount: number;
}
export interface IRetroactivePayrollEmployeeDTO {
	actorCompanyId: number;
	categoryIds: number[];
	created: Date;
	createdBy: string;
	employeeId: number;
	employeeName: string;
	employeeNr: string;
	hasOutcomes: boolean;
	hasTransactions: boolean;
	modified: Date;
	modifiedBy: string;
	note: string;
	payrollGroupId: number;
	retroactivePayrollEmployeeId: number;
	retroactivePayrollId: number;
	retroactivePayrollOutcomes: IRetroactivePayrollOutcomeDTO[];
	state: SoeEntityState;
	status: TermGroup_SoeRetroactivePayrollEmployeeStatus;
	statusName: string;
	totalAmount: number;
}
export interface IRetroactivePayrollOutcomeDTO {
	actorCompanyId: number;
	amount: number;
	created: Date;
	createdBy: string;
	employeeId: number;
	errorCode: TermGroup_SoeRetroactivePayrollOutcomeErrorCode;
	errorCodeText: string;
	hasTransactions: boolean;
	isQuantity: boolean;
	isReadOnly: boolean;
	isRetroCalculated: boolean;
	isReversed: boolean;
	isSpecifiedUnitPrice: boolean;
	modified: Date;
	modifiedBy: string;
	payrollProductName: string;
	payrollProductNumber: string;
	payrollProductNumberSort: string;
	payrollProductString: string;
	productId: number;
	quantity: number;
	quantityString: string;
	resultType: TermGroup_PayrollResultType;
	retroactivePayrolIEmployeeId: number;
	retroactivePayrollOutcomeId: number;
	retroUnitPrice: number;
	specifiedUnitPrice: number;
	state: SoeEntityState;
	transactions: IAttestPayrollTransactionDTO[];
	transactionUnitPrice: number;
}
export interface IReverseTransactionsValidationDTO {
	applySilent: boolean;
	canContinue: boolean;
	message: string;
	success: boolean;
	title: string;
	usePayroll: boolean;
	validCauses: ITimeDeviationCauseDTO[];
	validDates: Date[];
	validPeriods: ITimePeriodDTO[];
}
export interface IRoleDTO {
	actorCompanyId: number;
	actualName: string;
	created: Date;
	createdBy: string;
	externalCodes: string[];
	externalCodesString: string;
	modified: Date;
	modifiedBy: string;
	name: string;
	roleId: number;
	sort: number;
	state: SoeEntityState;
	termId: number;
}
export interface IRoleEditDTO {
	active: boolean;
	created: Date;
	createdBy: string;
	externalCodesString: string;
	favoriteOption: number;
	isAdmin: boolean;
	modified: Date;
	modifiedBy: string;
	name: string;
	roleId: number;
	sort: number;
	state: SoeEntityState;
	templateRoleId: number;
	updateStartPage: boolean;
}
export interface IRoleFieldSettingDTO {
	boldLabel: boolean;
	label: string;
	readOnly: boolean;
	roleId: number;
	roleName: string;
	skipTabStop: boolean;
	visible: boolean;
	visibleName: string;
}
export interface IRoleGridDTO {
	isActive: boolean;
	name: string;
	externalCodesString: string;
	roleId: number;
	sort: number;
	state: SoeEntityState;
}
export interface ISaveAttestEmployeeDayDTO {
	date: Date;
	originalUniqueId: string;
	timeBlockDateId: number;
}
export interface ISaveEmployeeFromTemplateHeadDTO {
	date: Date;
	employeeId: number;
	employeeTemplateId: number;
	hasExtraFields: boolean;
	printEmploymentContract: boolean;
	rows: ISaveEmployeeFromTemplateRowDTO[];
}
export interface ISaveEmployeeFromTemplateRowDTO {
	entity: SoeEntityType;
	extraValue: string;
	isExtraField: boolean;
	recordId: number;
	sort: number;
	startDate: Date;
	stopDate: Date;
	type: TermGroup_EmployeeTemplateGroupRowType;
	value: string;
}
export interface IScanningEntryDTO {
	actorCompanyId: number;
	batchId: string;
	companyId: string;
	created: Date;
	createdBy: string;
	errorCode: number;
	image: number[];
	messageType: TermGroup_ScanningMessageType;
	modified: Date;
	modifiedBy: string;
	nrOfInvoices: number;
	nrOfPages: number;
	operatorMessage: string;
	scanningEntryId: number;
	scanningEntryRow: IScanningEntryRowDTO[];
	state: SoeEntityState;
	status: TermGroup_ScanningStatus;
	type: number;
	xml: string;
}
export interface IScanningEntryRowDTO {
	created: Date;
	createdBy: string;
	format: string;
	modified: Date;
	modifiedBy: string;
	name: string;
	newText: string;
	pageNumber: string;
	position: string;
	state: SoeEntityState;
	text: string;
	type: ScanningEntryRowType;
	typeName: string;
	validationError: string;
}
export interface IScheduleCycleDTO {
	accountId: number;
	accountName: string;
	actorCompanyId: number;
	created: Date;
	createdBy: string;
	description: string;
	modified: Date;
	modifiedBy: string;
	name: string;
	nbrOfWeeks: number;
	scheduleCycleId: number;
	scheduleCycleRuleDTOs: IScheduleCycleRuleDTO[];
	state: SoeEntityState;
}
export interface IScheduleCycleRuleDTO {
	created: Date;
	createdBy: string;
	maxOccurrences: number;
	minOccurrences: number;
	modified: Date;
	modifiedBy: string;
	scheduleCycleId: number;
	scheduleCycleRuleId: number;
	scheduleCycleRuleTypeDTO: IScheduleCycleRuleTypeDTO;
	scheduleCycleRuleTypeId: number;
	state: SoeEntityState;
}
export interface IScheduleCycleRuleTypeDTO {
	accountId: number;
	accountName: string;
	actorCompanyId: number;
	created: Date;
	createdBy: string;
	dayOfWeekIds: number[];
	dayOfWeeks: string;
	dayOfWeeksGridString: string;
	hours: number;
	isWeekEndOnly: boolean;
	lenght: number;
	modified: Date;
	modifiedBy: string;
	name: string;
	scheduleCycleRuleTypeId: number;
	startTime: Date;
	state: SoeEntityState;
	stopTime: Date;
}
export interface IScheduledJobHeadDTO {
	created: Date;
	createdBy: string;
	description: string;
	logs: IScheduledJobLogDTO[];
	modified: Date;
	modifiedBy: string;
	name: string;
	parentId: number;
	rows: IScheduledJobRowDTO[];
	scheduledJobHeadId: number;
	settings: IScheduledJobSettingDTO[];
	sharedOnLicense: boolean;
	sort: number;
	state: SoeEntityState;
}
export interface IScheduledJobHeadGridDTO {
	description: string;
	name: string;
	scheduledJobHeadId: number;
	sharedOnLicense: boolean;
	sort: number;
	state: SoeEntityState;
}
export interface IScheduledJobLogDTO {
	batchNr: number;
	logLevel: TermGroup_ScheduledJobLogLevel;
	logLevelName: string;
	message: string;
	scheduledJobHeadId: number;
	scheduledJobLogId: number;
	scheduledJobRowId: number;
	status: TermGroup_ScheduledJobLogStatus;
	statusName: string;
	time: Date;
}
export interface IScheduledJobRowDTO {
	created: Date;
	createdBy: string;
	modified: Date;
	modifiedBy: string;
	nextExecutionTime: Date;
	recurrenceInterval: string;
	recurrenceIntervalText: string;
	scheduledJobHeadId: number;
	scheduledJobRowId: number;
	state: SoeEntityState;
	sysTimeIntervalId: number;
	timeIntervalText: string;
}
export interface IScheduledJobSettingDTO {
	boolData: boolean;
	dataType: SettingDataType;
	dateData: Date;
	decimalData: number;
	intData: number;
	name: string;
	options: ISmallGenericType[];
	scheduledJobHeadId: number;
	scheduledJobSettingId: number;
	state: SoeEntityState;
	strData: string;
	timeData: Date;
	type: TermGroup_ScheduledJobSettingType;
}
export interface ISchoolHolidayDTO {
	accountId: number;
	accountName: string;
	actorCompanyId: number;
	created: Date;
	createdBy: string;
	dateFrom: Date;
	dateTo: Date;
	isSummerHoliday: boolean;
	modified: Date;
	modifiedBy: string;
	name: string;
	schoolHolidayId: number;
	state: SoeEntityState;
}
export interface ISearchAgeDistributionDTO {
	actorNrFrom: string;
	actorNrTo: string;
	compareDate: Date;
	currencyType: number;
	expDateFrom: Date;
	expDateTo: Date;
	insecureDebts: boolean;
	invDateFrom: Date;
	invDateTo: Date;
	invNrFrom: string;
	invNrTo: string;
	seqNrFrom: number;
	seqNrTo: number;
	type: SoeInvoiceType;
}
export interface ISearchEmployeeSkillDTO {
	accountName: string;
	employeeId: number;
	employeeName: string;
	endDate: Date;
	positions: string;
	skillId: number;
	skillLevel: number;
	skillLevelDifference: number;
	skillLevelDifferenceStars: number;
	skillLevelPosition: number;
	skillLevelPositionStars: number;
	skillLevelStars: number;
	skillName: string;
}
export interface ISearchSysLogsDTO {
	companySearch: string;
	exExceptionSearch: string;
	exlMessageSearch: string;
	fromDate: Date;
	fromTime: Date;
	incExceptionSearch: string;
	incMessageSearch: string;
	level: string;
	licenseSearch: string;
	noOfRecords: number;
	roleSearch: string;
	showUnique: boolean;
	toDate: Date;
	toTime: Date;
	userSearch: string;
}
export interface ISelectablePayrollMonthYearDTO {
	displayName: string;
	id: number;
	timePeriodIds: number[];
}
export interface ISelectablePayrollTypeDTO {
	id: number;
	name: string;
	parentSysTermId: number;
	sysTermId: number;
}
export interface ISelectableTimePeriodDTO {
	displayName: string;
	id: number;
	paymentDate: Date;
	start: Date;
	stop: Date;
}
export interface IShiftAccountingDTO {
	dim2DimName: string;
	dim2Id: number;
	dim2Name: string;
	dim2Nr: string;
	dim3DimName: string;
	dim3Id: number;
	dim3Name: string;
	dim3Nr: string;
	dim4DimName: string;
	dim4Id: number;
	dim4Name: string;
	dim4Nr: string;
	dim5DimName: string;
	dim5Id: number;
	dim5Name: string;
	dim5Nr: string;
	dim6DimName: string;
	dim6Id: number;
	dim6Name: string;
	dim6Nr: string;
}
export interface IShiftHistoryDTO {
	absenceRequestApprovedText: string;
	created: Date;
	createdBy: string;
	dateAndTimeChanged: boolean;
	employeeChanged: boolean;
	extraShiftChanged: boolean;
	fromDateAndTime: string;
	fromEmployeeId: number;
	fromEmployeeName: string;
	fromEmployeeNr: string;
	fromExtraShift: string;
	fromShiftStatus: string;
	fromShiftType: string;
	fromShiftUserStatus: string;
	fromStart: string;
	fromStop: string;
	fromTime: string;
	fromTimeDeviationCause: string;
	originEmployeeName: string;
	originEmployeeNr: string;
	shiftStatusChanged: boolean;
	shiftTypeChanged: boolean;
	shiftUserStatusChanged: boolean;
	timeChanged: boolean;
	timeDeviationCauseChanged: boolean;
	timeScheduleTemplateBlockId: number;
	toDateAndTime: string;
	toEmployeeId: number;
	toEmployeeName: string;
	toEmployeeNr: string;
	toExtraShift: string;
	toShiftStatus: string;
	toShiftType: string;
	toShiftUserStatus: string;
	toStart: string;
	toStop: string;
	toTime: string;
	toTimeDeviationCause: string;
	typeName: string;
}
export interface IShiftRequestStatusDTO {
	messageId: number;
	recipients: IShiftRequestStatusRecipientDTO[];
	senderName: string;
	sentDate: Date;
	text: string;
}
export interface IShiftRequestStatusRecipientDTO {
	answerDate: Date;
	answerType: XEMailAnswerType;
	employeeId: number;
	employeeName: string;
	employeeNr: string;
	modified: Date;
	modifiedBy: string;
	readDate: Date;
	state: SoeEntityState;
	userId: number;
}
export interface IShiftTypeDTO {
	accountId: number;
	accountingSettings: IAccountingSettingsRowDTO;
	accountInternalIds: number[];
	accountIsNotActive: boolean;
	accountNrAndName: string;
	actorCompanyId: number;
	categoryIds: number[];
	childHierarchyAccountIds: number[];
	color: string;
	created: Date;
	createdBy: string;
	defaultLength: number;
	description: string;
	employeeStatisticsTargets: IShiftTypeEmployeeStatisticsTargetDTO[];
	externalCode: string;
	externalId: number;
	handlingMoney: boolean;
	hierarchyAccounts: IShiftTypeHierarchyAccountDTO[];
	linkedShiftTypeIds: number[];
	modified: Date;
	modifiedBy: string;
	name: string;
	needsCode: string;
	shiftTypeId: number;
	shiftTypeSkills: IShiftTypeSkillDTO[];
	startTime: Date;
	state: SoeEntityState;
	stopTime: Date;
	timeScheduleTemplateBlockType: TermGroup_TimeScheduleTemplateBlockType;
	timeScheduleTypeId: number;
}
export interface IShiftTypeEmployeeStatisticsTargetDTO {
	created: Date;
	createdBy: string;
	employeeStatisticsType: TermGroup_EmployeeStatisticsType;
	employeeStatisticsTypeName: string;
	fromDate: Date;
	modified: Date;
	modifiedBy: string;
	shiftTypeEmployeeStatisticsTargetId: number;
	shiftTypeId: number;
	state: SoeEntityState;
	targetValue: number;
}
export interface IShiftTypeGridDTO {
	accountId: number;
	accountingStringAccountNames: string;
	accountIsNotActive: boolean;
	categoryNames: string;
	color: string;
	defaultLength: number;
	description: string;
	externalCode: string;
	name: string;
	needsCode: string;
	needsCodeName: string;
	shiftTypeId: number;
	skillNames: string;
	timeScheduleTemplateBlockType: TermGroup_TimeScheduleTemplateBlockType;
	timeScheduleTemplateBlockTypeName: string;
	timeScheduleTypeId: number;
	timeScheduleTypeName: string;
}
export interface IShiftTypeHierarchyAccountDTO {
	accountId: number;
	accountPermissionType: TermGroup_AttestRoleUserAccountPermissionType;
	shiftTypeHierarchyAccountId: number;
}
export interface IShiftTypeLinkDTO {
	actorCompanyId: number;
	guid: string;
	shiftTypes: IShiftTypeDTO[];
}
export interface IShiftTypeSkillDTO {
	missing: boolean;
	shiftTypeId: number;
	shiftTypeSkillId: number;
	skillId: number;
	skillLevel: number;
	skillLevelStars: number;
	skillName: string;
	skillTypeName: string;
}
export interface ISkillDTO {
	actorCompanyId: number;
	created: Date;
	createdBy: string;
	description: string;
	modified: Date;
	modifiedBy: string;
	name: string;
	skillId: number;
	skillTypeDTO: ISkillTypeDTO;
	skillTypeId: number;
	skillTypeName: string;
	state: SoeEntityState;
}
export interface ISkillTypeDTO {
	actorCompanyId: number;
	created: Date;
	createdBy: string;
	description: string;
	modified: Date;
	modifiedBy: string;
	name: string;
	skillTypeId: number;
	state: SoeEntityState;
}
export interface ISmallGenericType {
	id: number;
	name: string;
}
export interface ISoeProgressInfo {
	abort: boolean;
	actorCompanyId: number;
	age: System.ITimeSpan;
	baseMessage: string;
	created: Date;
	done: boolean;
	error: boolean;
	errorMessage: string;
	message: string;
	pollingKey: System.IGuid;
	soeProgressInfoType: SoeProgressInfoType;
	timeSinceLastAction: System.ITimeSpan;
}
export interface ISplitAccountingRowDTO {
	amountCurrency: number;
	creditAmountCurrency: number;
	debitAmountCurrency: number;
	dim1Disabled: boolean;
	dim1Id: number;
	dim1Mandatory: boolean;
	dim1Name: string;
	dim1Nr: string;
	dim1Stop: boolean;
	dim2Disabled: boolean;
	dim2Id: number;
	dim2Mandatory: boolean;
	dim2Name: string;
	dim2Nr: string;
	dim2Stop: boolean;
	dim3Disabled: boolean;
	dim3Id: number;
	dim3Mandatory: boolean;
	dim3Name: string;
	dim3Nr: string;
	dim3Stop: boolean;
	dim4Disabled: boolean;
	dim4Id: number;
	dim4Mandatory: boolean;
	dim4Name: string;
	dim4Nr: string;
	dim4Stop: boolean;
	dim5Disabled: boolean;
	dim5Id: number;
	dim5Mandatory: boolean;
	dim5Name: string;
	dim5Nr: string;
	dim5Stop: boolean;
	dim6Disabled: boolean;
	dim6Id: number;
	dim6Mandatory: boolean;
	dim6Name: string;
	dim6Nr: string;
	dim6Stop: boolean;
	invoiceAccountRowId: number;
	isCreditRow: boolean;
	isDebitRow: boolean;
	splitPercent: number;
	splitType: number;
	splitValue: number;
}
export interface IStaffingNeedsCalculationTimeSlot {
	calculationGuid: System.IGuid;
	from: Date;
	isBreak: boolean;
	isFixed: boolean;
	maxTo: Date;
	middle: Date;
	minFrom: Date;
	minutes: number;
	shiftTypeId: number;
	timeSlotLength: number;
	to: Date;
}
export interface IStaffingNeedsHeadDTO {
	accountId: number;
	date: Date;
	dayTypeId: number;
	description: string;
	fromDate: Date;
	interval: number;
	name: string;
	parentId: number;
	periodGuid: System.IGuid;
	rows: IStaffingNeedsRowDTO[];
	staffingNeedsHeadId: number;
	staffingNeedsHeadUsers: IStaffingNeedsHeadUserDTO[];
	status: TermGroup_StaffingNeedsHeadStatus;
	type: StaffingNeedsHeadType;
	weekday: DayOfWeek;
}
export interface IStaffingNeedsHeadSmallDTO {
	date: Date;
	dayTypeId: number;
	interval: number;
	name: string;
	staffingNeedsHeadId: number;
	status: TermGroup_StaffingNeedsHeadStatus;
	type: StaffingNeedsHeadType;
	weekday: DayOfWeek;
}
export interface IStaffingNeedsHeadUserDTO {
	loginName: string;
	main: boolean;
	name: string;
	staffingNeedsHeadId: number;
	staffingNeedsHeadUserId: number;
	userId: number;
}
export interface IStaffingNeedsRowDTO {
	name: string;
	originType: StaffingNeedsRowOriginType;
	periods: IStaffingNeedsRowPeriodDTO[];
	rowFrequencys: IStaffingNeedsRowFrequencyDTO[];
	rowNr: number;
	shiftTypeColor: string;
	shiftTypeId: number;
	shiftTypeName: string;
	staffingNeedsHeadId: number;
	staffingNeedsLocationGroupId: number;
	staffingNeedsRowId: number;
	tasks: IStaffingNeedsRowTaskDTO[];
	tempId: number;
	toolTip: string;
	type: StaffingNeedsRowType;
}
export interface IStaffingNeedsRowFrequencyDTO {
	actualStartTime: Date;
	actualStopTime: Date;
	date: Date;
	interval: number;
	shiftTypeId: number;
	staffingNeedsRowFrequencyId: number;
	staffingNeedsRowId: number;
	startTime: Date;
	value: number;
}
export interface IStaffingNeedsRowPeriodDTO {
	date: Date;
	incomingDeliveryRowId: number;
	interval: number;
	isBaseNeed: boolean;
	isBreak: boolean;
	isRemovedNeed: boolean;
	isSpecificNeed: boolean;
	length: number;
	parentId: number;
	periodGuid: System.IGuid;
	shiftTypeColor: string;
	shiftTypeId: number;
	shiftTypeName: string;
	shiftTypeNeedsCode: string;
	staffingNeedsRowId: number;
	staffingNeedsRowPeriodId: number;
	startTime: Date;
	timeScheduleTaskId: number;
	timeSlot: IStaffingNeedsCalculationTimeSlot;
	value: number;
}
export interface IStaffingNeedsRowTaskDTO {
	staffingNeedsRowId: number;
	staffingNeedsRowTaskId: number;
	startTime: Date;
	stopTime: Date;
	task: string;
}
export interface IStaffingNeedsTaskDTO {
	account2Id: number;
	account3Id: number;
	account4Id: number;
	account5Id: number;
	account6Id: number;
	accountId: number;
	accountName: string;
	color: string;
	description: string;
	id: number;
	isFixed: boolean;
	length: number;
	name: string;
	recurrencePattern: string;
	shiftTypeId: number;
	shiftTypeName: string;
	startTime: Date;
	stopTime: Date;
	type: SoeStaffingNeedsTaskType;
}
export interface IStaffingStatisticsInterval {
	employeeId: number;
	interval: Date;
	rows: IStaffingStatisticsIntervalRow[];
}
export interface IStaffingStatisticsIntervalRow {
	budget: IStaffingStatisticsIntervalValue;
	forecast: IStaffingStatisticsIntervalValue;
	key: number;
	modifiedCalculationType: TermGroup_TimeSchedulePlanningFollowUpCalculationType;
	need: number;
	needFrequency: number;
	needRowFrequency: number;
	schedule: IStaffingStatisticsIntervalValue;
	scheduleAndTime: IStaffingStatisticsIntervalValue;
	targetCalculationType: TermGroup_TimeSchedulePlanningFollowUpCalculationType;
	templateSchedule: IStaffingStatisticsIntervalValue;
	templateScheduleForEmployeePost: IStaffingStatisticsIntervalValue;
	time: IStaffingStatisticsIntervalValue;
}
export interface IStaffingStatisticsIntervalValue {
	bpat: number;
	fpat: number;
	hours: number;
	lpat: number;
	personelCost: number;
	salaryPercent: number;
	sales: number;
}
export interface IStockDTO {
	actorCompanyId: number;
	avgPrice: number;
	code: string;
	created: Date;
	createdBy: string;
	deliveryLeadTimeDays: number;
	isExternal: boolean;
	modified: Date;
	modifiedBy: string;
	name: string;
	purchaseQuantity: number;
	purchaseTriggerQuantity: number;
	saldo: number;
	state: SoeEntityState;
	stockId: number;
	stockProductId: number;
	stockShelfId: number;
	stockShelfName: string;
}
export interface IStockProductDTO {
	avgPrice: number;
	created: Date;
	createdBy: string;
	invoiceProductId: number;
	isInInventory: boolean;
	modified: Date;
	modifiedBy: string;
	orderedQuantity: number;
	productName: string;
	productNumber: string;
	productUnit: string;
	purchasedQuantity: number;
	purchaseQuantity: number;
	purchaseTriggerQuantity: number;
	quantity: number;
	reservedQuantity: number;
	stockId: number;
	stockName: string;
	stockProductId: number;
	stockShelfCode: string;
	stockShelfId: number;
	stockShelfName: string;
	stockValue: number;
	warningLevel: number;
}
export interface IStringKeyValue {
	key: string;
	value: string;
}
export interface IStringKeyValueList {
	id: number;
	values: IStringKeyValue[];
}
export interface ISupplierAgreementDTO {
	categoryId: number;
	code: string;
	codeType: number;
	date: Date;
	discountPercent: number;
	priceListTypeId: number;
	priceListTypeName: string;
	rebateListId: number;
	state: number;
	sysWholesellerId: number;
	wholesellerName: string;
}
export interface ISupplierDTO {
	accountingSettings: IAccountingSettingsRowDTO[];
	active: boolean;
	actorSupplierId: number;
	attestWorkFlowGroupId: number;
	bic: string;
	blockPayment: boolean;
	categoryIds: number[];
	consentDate: Date;
	consentModified: Date;
	consentModifiedBy: string;
	contactAddresses: IContactAddressItem[];
	contactEcomId: number;
	contactPersons: number[];
	copyInvoiceNrToOcr: boolean;
	created: Date;
	createdBy: string;
	currencyId: number;
	deliveryConditionId: number;
	deliveryTypeId: number;
	factoringSupplierId: number;
	hasConsent: boolean;
	interim: boolean;
	intrastatCodeId: number;
	invoiceReference: string;
	isEDISupplier: boolean;
	isEUCountryBased: boolean;
	isPrivatePerson: boolean;
	manualAccounting: boolean;
	modified: Date;
	modifiedBy: string;
	name: string;
	note: string;
	orgNr: string;
	ourCustomerNr: string;
	ourReference: string;
	paymentConditionId: number;
	paymentInformationDomestic: IPaymentInformationDTO;
	paymentInformationForegin: IPaymentInformationDTO;
	riksbanksCode: string;
	showNote: boolean;
	state: SoeEntityState;
	supplierNr: string;
	sysCountryId: number;
	sysLanguageId: number;
	sysWholeSellerId: number;
	templateAttestHead: IAttestWorkFlowHeadDTO;
	vatCodeId: number;
	vatNr: string;
	vatType: TermGroup_InvoiceVatType;
}
export interface ISupplierInvoiceCostAllocationDTO {
	attestStateColor: string;
	attestStateId: number;
	attestStateName: string;
	chargeCostToProject: boolean;
	customerInvoiceNumberName: string;
	customerInvoiceRowId: number;
	employeeDescription: string;
	employeeId: number;
	employeeName: string;
	employeeNr: string;
	includeSupplierInvoiceImage: boolean;
	isConnectToProjectRow: boolean;
	isReadOnly: boolean;
	isTransferToOrderRow: boolean;
	orderAmount: number;
	orderAmountCurrency: number;
	orderId: number;
	orderNr: string;
	productId: number;
	productName: string;
	productNr: string;
	projectAmount: number;
	projectAmountCurrency: number;
	projectId: number;
	projectName: string;
	projectNr: string;
	rowAmount: number;
	rowAmountCurrency: number;
	state: SoeEntityState;
	supplementCharge: number;
	supplierInvoiceId: number;
	timeCodeCode: string;
	timeCodeDescription: string;
	timeCodeId: number;
	timeCodeName: string;
	timeCodeTransactionId: number;
	timeInvoiceTransactionId: number;
}
export interface ISupplierInvoiceCostOverviewDTO {
	attestGroupName: string;
	diffAmount: number;
	diffPercent: number;
	dueDate: Date;
	internalText: string;
	invoiceDate: Date;
	invoiceNr: string;
	orderAmount: number;
	orderIds: number[];
	orderNr: string;
	projectAmount: number;
	projectId: number;
	projectIds: number[];
	projectNr: string;
	seqNr: string;
	status: number;
	statusName: string;
	supplierId: number;
	supplierInvoiceId: number;
	supplierName: string;
	supplierNr: string;
	totalAmountCurrency: number;
	totalAmountExVat: number;
	vATAmountCurrency: number;
}
export interface ISupplierInvoiceDTO extends IInvoiceDTO {
	attestGroupId: number;
	attestStateId: number;
	attestStateName: string;
	blockPayment: boolean;
	blockReason: string;
	blockReasonTextId: number;
	ediEntryId: number;
	hasImage: boolean;
	image: IGenericImageDTO;
	interimInvoice: boolean;
	multipleDebtRows: boolean;
	orderCustomerInvoiceId: number;
	orderCustomerName: string;
	orderNr: number;
	orderProjectId: number;
	paymentMethodId: number;
	prevInvoiceId: number;
	scanningImage: IGenericImageDTO;
	supplierInvoiceCostAllocationRows: ISupplierInvoiceCostAllocationDTO[];
	supplierInvoiceFiles: IFileUploadDTO[];
	supplierInvoiceOrderRows: ISupplierInvoiceOrderRowDTO[];
	supplierInvoiceProjectRows: ISupplierInvoiceProjectRowDTO[];
	supplierInvoiceRows: ISupplierInvoiceRowDTO[];
	vatDeductionAccountId: number;
	vatDeductionPercent: number;
	vatDeductionType: TermGroup_VatDeductionType;
}
export interface ISupplierInvoiceGridDTO {
	attestGroupId: number;
	attestGroupName: string;
	attestStateId: number;
	attestStateName: string;
	billingTypeId: number;
	billingTypeName: string;
	blockPayment: boolean;
	blockReason: string;
	created: Date;
	currencyCode: string;
	currencyRate: number;
	currentAttestUserName: string;
	dueDate: Date;
	ediEntryId: number;
	ediMessageType: number;
	ediMessageTypeName: string;
	ediType: number;
	errorCode: number;
	errorMessage: string;
	fullyPaid: boolean;
	guid: System.IGuid;
	hasAttestComment: boolean;
	hasPDF: boolean;
	hasVoucher: boolean;
	internalText: string;
	invoiceDate: Date;
	invoiceNr: string;
	invoiceStatus: number;
	isAttestRejected: boolean;
	isOverdue: boolean;
	isSelectDisabled: boolean;
	multipleDebtRows: boolean;
	noOfCheckedPaymentRows: number;
	noOfPaymentRows: number;
	ocr: string;
	operatorMessage: string;
	ownerActorId: number;
	paidAmount: number;
	paidAmountCurrency: number;
	payAmount: number;
	payAmountCurrency: number;
	payAmountCurrencyText: string;
	payAmountText: string;
	payDate: Date;
	paymentStatuses: string;
	projectAmount: number;
	projectInvoicedAmount: number;
	roundedInterpretation: number;
	scanningEntryId: number;
	seqNr: string;
	sourceTypeName: string;
	status: number;
	statusIcon: number;
	statusName: string;
	supplierId: number;
	supplierInvoiceId: number;
	supplierName: string;
	supplierNr: string;
	sysCurrencyId: number;
	timeDiscountDate: Date;
	timeDiscountPercent: number;
	totalAmount: number;
	totalAmountCurrency: number;
	totalAmountCurrencyText: string;
	totalAmountExVat: number;
	totalAmountExVatCurrency: number;
	totalAmountExVatCurrencyText: string;
	totalAmountExVatText: string;
	totalAmountText: string;
	type: number;
	typeName: string;
	useClosedStyle: boolean;
	vATAmount: number;
	vATAmountCurrency: number;
	vatRate: number;
	vatType: number;
	voucherDate: Date;
}
export interface ISupplierInvoiceOrderGridDTO {
	amount: number;
	billingType: TermGroup_BillingType;
	customerInvoiceId: number;
	customerInvoiceRowId: number;
	hasImage: boolean;
	icon: string;
	includeImageOnInvoice: boolean;
	invoiceAmountExVat: number;
	invoiceDate: Date;
	invoiceNr: string;
	seqNr: number;
	supplierInvoiceId: number;
	supplierInvoiceOrderLinkType: SupplierInvoiceOrderLinkType;
	supplierName: string;
	supplierNr: string;
	timeCodeTransactionId: number;
}
export interface ISupplierInvoiceOrderRowDTO {
	amount: number;
	amountCurrency: number;
	amountEntCurrency: number;
	amountLedgerCurrency: number;
	attestStateColor: string;
	attestStateId: number;
	attestStateName: string;
	customerInvoiceCustomerName: string;
	customerInvoiceDescription: string;
	customerInvoiceId: number;
	customerInvoiceNr: string;
	customerInvoiceNumberName: string;
	customerInvoiceRowId: number;
	includeSupplierInvoiceImage: boolean;
	invoiceProductId: number;
	isModified: boolean;
	isReadOnly: boolean;
	projectDescription: string;
	projectId: number;
	projectName: string;
	projectNr: string;
	state: SoeEntityState;
	sumAmount: number;
	sumAmountCurrency: number;
	sumAmountEntCurrency: number;
	sumAmountLedgerCurrency: number;
	supplementCharge: number;
	supplierInvoiceId: number;
}
export interface ISupplierInvoiceProductRowDTO {
	amountCurrency: number;
	created: Date;
	createdBy: string;
	customerInvoiceRowId: number;
	customerInvoiceNumber: string;
	customerInvoiceId: number;
	modified: Date;
	modifiedBy: string;
	priceCurrency: number;
	quantity: number;
	rowType: SupplierInvoiceRowType;
	sellerProductNumber: string;
	state: SoeEntityState;
	supplierInvoiceId: number;
	supplierInvoiceProductRowId: number;
	text: string;
	unitCode: string;
	vatAmountCurrency: number;
	vatRate: number;
}
export interface ISupplierInvoiceProjectRowDTO {
	amount: number;
	amountCurrency: number;
	amountEntCurrency: number;
	amountLedgerCurrency: number;
	chargeCostToProject: boolean;
	customerInvoiceCustomerName: string;
	customerInvoiceDescription: string;
	customerInvoiceId: number;
	customerInvoiceNr: string;
	customerInvoiceNumberName: string;
	date: Date;
	employeeDescription: string;
	employeeId: number;
	employeeName: string;
	employeeNr: string;
	includeSupplierInvoiceImage: boolean;
	orderIds: number[];
	orderNr: string;
	projectDescription: string;
	projectId: number;
	projectName: string;
	projectNr: string;
	state: SoeEntityState;
	supplierInvoiceId: number;
	timeBlockDateId: number;
	timeCodeCode: string;
	timeCodeDescription: string;
	timeCodeId: number;
	timeCodeName: string;
	timeCodeTransactionId: number;
	timeInvoiceTransactionId: number;
}
export interface ISupplierInvoiceRowDTO {
	accountingRows: IAccountingRowDTO[];
	amount: number;
	amountCurrency: number;
	amountEntCurrency: number;
	amountLedgerCurrency: number;
	created: Date;
	createdBy: string;
	invoiceId: number;
	modified: Date;
	modifiedBy: string;
	quantity: number;
	state: SoeEntityState;
	supplierInvoiceRowId: number;
	vatAmount: number;
	vatAmountCurrency: number;
	vatAmountEntCurrency: number;
	vatAmountLedgerCurrency: number;
}
export interface ISupplierPaymentGridDTO {
	attestStateId: number;
	attestStateName: string;
	bankFee: number;
	billingTypeId: number;
	billingTypeName: string;
	blockPayment: boolean;
	blockReason: string;
	currencyCode: string;
	currencyRate: number;
	currentAttestUserName: string;
	description: string;
	dueDate: Date;
	fullyPaid: boolean;
	guid: System.IGuid;
	hasAttestComment: boolean;
	hasVoucher: boolean;
	invoiceDate: Date;
	invoiceNr: string;
	invoiceSeqNr: string;
	isModified: boolean;
	multipleDebtRows: boolean;
	ownerActorId: number;
	paidAmount: number;
	paidAmountCurrency: number;
	payAmount: number;
	payAmountCurrency: number;
	payDate: Date;
	paymentAmount: number;
	paymentAmountCurrency: number;
	paymentAmountDiff: number;
	paymentMethodName: string;
	paymentNr: string;
	paymentNrString: string;
	paymentRowId: number;
	paymentSeqNr: number;
	paymentStatuses: string;
	sequenceNumber: number;
	sequenceNumberRecordId: number;
	status: number;
	statusIcon: number;
	statusName: string;
	supplierBlockPayment: boolean;
	supplierId: number;
	supplierInvoiceId: number;
	supplierName: string;
	supplierNr: string;
	supplierPaymentId: number;
	sysCurrencyId: number;
	sysPaymentMethodId: number;
	sysPaymentTypeId: number;
	timeDiscountDate: Date;
	timeDiscountPercent: number;
	totalAmount: number;
	totalAmountCurrency: number;
	totalAmountExVat: number;
	totalAmountExVatCurrency: number;
	vATAmount: number;
	vATAmountCurrency: number;
	vatRate: number;
	voucherDate: Date;
}
export interface ISupplierProductDTO {
	created: Date;
	createdBy: string;
	deliveryLeadTimeDays: number;
	intrastatCodeId: number;
	modified: Date;
	modifiedBy: string;
	packSize: number;
	productId: number;
	supplierId: number;
	supplierProductCode: string;
	supplierProductId: number;
	supplierProductName: string;
	supplierProductNr: string;
	supplierProductUnitId: number;
	sysCountryId: number;
}
export interface ISupplierProductGridDTO {
	productId: number;
	productName: string;
	productNr: string;
	supplierId: number;
	supplierName: string;
	supplierNr: string;
	supplierProductCode: string;
	supplierProductId: number;
	supplierProductName: string;
	supplierProductNr: string;
	supplierProductUnitName: string;
}
export interface ISupplierProductImportDTO {
	importPrices: boolean;
	importToPriceList: boolean;
	options: IImportOptionsDTO;
	priceListId: number;
	rows: ISupplierProductImportRawDTO[];
	supplierId: number;
}
export interface ISupplierProductImportRawDTO {
	salesProductNumber: string;
	supplierNumber: string;
	supplierProductCode: string;
	supplierProductLeadTime: number;
	supplierProductName: string;
	supplierProductNr: string;
	supplierProductPackSize: number;
	supplierProductPriceCurrencyCode: string;
	supplierProductPriceDate: Date;
	supplierProductPriceDateStop: Date;
	supplierProductPricePrice: number;
	supplierProductPriceQuantity: number;
	supplierProductUnit: string;
}
export interface ISupplierProductPriceComparisonDTO extends ISupplierProductPriceDTO {
	compareEndDate: Date;
	comparePrice: number;
	compareQuantity: number;
	compareStartDate: Date;
	compareSupplierProductPriceId: number;
	ourProductName: string;
	productName: string;
	productNr: string;
}
export interface ISupplierProductPriceDTO {
	currencyCode: string;
	currencyId: number;
	endDate: Date;
	price: number;
	quantity: number;
	startDate: Date;
	state: SoeEntityState;
	supplierProductId: number;
	supplierProductPriceId: number;
	supplierProductPriceListId: number;
	sysCurrencyId: number;
}
export interface ISupplierProductPricelistDTO {
	created: Date;
	createdBy: string;
	currencyCode: string;
	currencyId: number;
	endDate: Date;
	modified: Date;
	modifiedBy: string;
	startDate: Date;
	supplierId: number;
	supplierName: string;
	supplierNr: string;
	supplierProductPriceListId: number;
	sysCurrencyId: number;
	sysWholeSellerId: number;
	sysWholeSellerName: string;
	sysWholeSellerType: number;
	sysWholeSellerTypeName: string;
}
export interface ISupplierProductPriceSearchDTO {
	compareDate: Date;
	currencyId: number;
	includePricelessProducts: boolean;
	supplierId: number;
}
export interface ISupplierProductSaveDTO {
	priceRows: ISupplierProductPriceDTO[];
	product: ISupplierProductDTO;
}
export interface ISupplierProductSearchDTO {
	invoiceProductId: number;
	product: string;
	productName: string;
	supplierIds: number[];
	supplierProduct: string;
	supplierProductName: string;
}
export interface ISupplierProductSmallDTO {
	name: string;
	number: string;
	numberName: string;
	supplierProductId: number;
}
export interface ISysAccountStdDTO {
	accountNr: string;
	accountTypeSysTermId: number;
	amountStop: number;
	name: string;
	sysAccountSruCodeIds: number[];
	sysAccountStdId: number;
	sysAccountStdTypeId: number;
	sysVatAccountId: number;
	unit: string;
	unitStop: boolean;
}
export interface ISysCompanyDTO {
	actorCompanyId: number;
	companyApiKey: System.IGuid;
	dbName: string;
	isSOP: boolean;
	licenseId: number;
	licenseName: string;
	licenseNumber: string;
	name: string;
	number: string;
	serverName: string;
	state: SoeSysEntityState;
	sysCompanyId: number;
	sysCompanySettingDTOs: Soe.Sys.Common.DTO.ISysCompanySettingDTO[];
	sysCompDBDTO: ISysCompDBDTO;
	sysCompDbId: number;
}
export interface ISysCompDBDTO {
	apiUrl: string;
	description: string;
	name: string;
	sysCompDbId: number;
	sysCompServerDTO: ISysCompServerDTO;
	sysCompServerId: number;
	type: SysCompDBType;
}
export interface ISysCompServerDTO {
	name: string;
	sysCompServerId: number;
	sysServiceUrl: string;
}
export interface ISysEdiMessageHeadDTO {
	buyerAddress: string;
	buyerCountryCode: string;
	buyerDeliveryAddress: string;
	buyerDeliveryCoAddress: string;
	buyerDeliveryCountryCode: string;
	buyerDeliveryGoodsMarking: string;
	buyerDeliveryName: string;
	buyerDeliveryNoteText: string;
	buyerDeliveryPostalAddress: string;
	buyerDeliveryPostalCode: string;
	buyerEmailAddress: string;
	buyerFax: string;
	buyerId: string;
	buyerName: string;
	buyerOrganisationNumber: string;
	buyerPhone: string;
	buyerPostalAddress: string;
	buyerPostalCode: string;
	buyerReference: string;
	buyerVatNumber: string;
	created: Date;
	eDISourceType: number;
	errorMessage: string;
	headBank: string;
	headBankGiro: string;
	headBicAddress: string;
	headBonusAmount: number;
	headBuyerInstallationNumber: string;
	headBuyerOrderNumber: string;
	headBuyerProjectNumber: string;
	headCurrencyCode: string;
	headDeliveryDate: Date;
	headDiscountAmount: number;
	headFreightFeeAmount: number;
	headHandlingChargeFeeAmount: number;
	headIbanNumber: string;
	headInsuranceFeeAmount: number;
	headInterestPaymentPercent: string;
	headInterestPaymentText: string;
	headInvoiceArrival: string;
	headInvoiceAuthorized: string;
	headInvoiceAuthorizedBy: string;
	headInvoiceDate: Date;
	headInvoiceDueDate: Date;
	headInvoiceGrossAmount: number;
	headInvoiceNetAmount: number;
	headInvoiceNumber: string;
	headInvoiceOcr: string;
	headInvoiceType: string;
	headPaymentConditionDays: number;
	headPaymentConditionText: string;
	headPostalGiro: string;
	headRemainingFeeAmount: number;
	headRoundingAmount: number;
	headSellerOrderNumber: string;
	headVatAmount: number;
	headVatBasisAmount: number;
	headVatPercentage: number;
	lastSendTry: Date;
	messageDate: Date;
	messageSenderId: string;
	messageType: string;
	sellerAddress: string;
	sellerCountryCode: string;
	sellerFax: string;
	sellerId: string;
	sellerName: string;
	sellerOrganisationNumber: string;
	sellerPhone: string;
	sellerPostalAddress: string;
	sellerPostalCode: string;
	sellerReference: string;
	sellerReferencePhone: string;
	sellerVatNumber: string;
	sent: Date;
	sysCompanyId: number;
	sysEdiEdiMessageRowDTOs: ISysEdiMessageRowDTO[];
	sysEdiMessageHeadGuid: System.IGuid;
	sysEdiMessageHeadId: number;
	sysEdiMessageHeadStatus: SysEdiMessageHeadStatus;
	sysEdiMessageRawId: number;
	sysEdiMsgId: number;
	sysEdiType: number;
	sysWholesellerId: number;
	xDocument: string;
}
export interface ISysEdiMessageHeadGridDTO {
	buyerId: string;
	buyerName: string;
	created: Date;
	errorMessage: string;
	headInvoiceDate: Date;
	headSellerOrderNumber: string;
	sendDate: Date;
	sysCompanyName: string;
	sysEdiMessageHeadGuid: System.IGuid;
	sysEdiMessageHeadId: number;
	sysEdiMessageHeadStatus: SysEdiMessageHeadStatus;
	sysWholesellerName: string;
}
export interface ISysEdiMessageRawDTO {
	checksum: string;
	errorMessage: string;
	filename: string;
	fileString: string;
	guid: System.IGuid;
	name: string;
	number: string;
	rows: number;
	size: number;
	state: number;
	status: number;
	sysCompanyId: number;
	sysEdiMessageRawId: number;
	sysWholesellerId: number;
	time: Date;
	xDocument: string;
}
export interface ISysEdiMessageRowDTO {
	externalProductId: number;
	rowBuyerArticleNumber: string;
	rowBuyerObjectId: string;
	rowBuyerReference: string;
	rowBuyerRowNumber: string;
	rowDeliveryDate: Date;
	rowDiscountAmount: number;
	rowDiscountAmount1: number;
	rowDiscountAmount2: number;
	rowDiscountPercent: number;
	rowDiscountPercent1: number;
	rowDiscountPercent2: number;
	rowNetAmount: number;
	rowQuantity: number;
	rowSellerArticleDescription1: string;
	rowSellerArticleDescription2: string;
	rowSellerArticleNumber: string;
	rowSellerRowNumber: string;
	rowUnitCode: string;
	rowUnitPrice: string;
	rowVatAmount: number;
	rowVatPercentage: number;
	stockCode: string;
	sysEdiMessageHeadId: number;
	sysEdiMessageRowId: number;
}
export interface ISysGaugeDTO {
	created: Date;
	createdBy: string;
	gaugeName: string;
	isSelected: boolean;
	modified: Date;
	modifiedBy: string;
	name: string;
	postChange: PostChange;
	state: SoeEntityState;
	sysFeatureId: number;
	sysGaugeId: number;
	sysTermId: number;
}
export interface ISysHelpDTO {
	created: Date;
	createdBy: string;
	language: string;
	modified: Date;
	modifiedBy: string;
	plainText: string;
	state: SoeEntityState;
	sysFeatureId: number;
	sysHelpId: number;
	sysLanguageId: number;
	text: string;
	title: string;
	versionNr: number;
}
export interface ISysHelpSmallDTO {
	plainText: string;
	sysFeatureId: number;
	sysHelpId: number;
	text: string;
	title: string;
}
export interface ISysImportDefinitionDTO {
	created: Date;
	createdBy: string;
	guid: System.IGuid;
	modified: Date;
	modifiedBy: string;
	module: SoeModule;
	name: string;
	separator: string;
	specialFunctionality: string;
	state: SoeEntityState;
	sysImportDefinitionId: number;
	sysImportDefinitionLevels: ISysImportDefinitionLevelDTO[];
	sysImportHeadId: number;
	type: TermGroup_SysImportDefinitionType;
	xmlTagHead: string;
}
export interface ISysImportDefinitionLevelColumnSettings {
	characters: number;
	column: string;
	convert: string;
	from: number;
	isModified: boolean;
	level: number;
	position: number;
	standard: string;
	sysImportDefinitionLevelColumnSettingsId: number;
	text: string;
	updateTypeId: number;
	updateTypeText: string;
	xmlTag: string;
}
export interface ISysImportDefinitionLevelDTO {
	columns: ISysImportDefinitionLevelColumnSettings[];
	level: number;
	sysImportDefinitionId: number;
	sysImportDefinitionLevelId: number;
	xml: string;
}
export interface ISysImportHeadDTO {
	created: Date;
	createdBy: string;
	description: string;
	modified: Date;
	modifiedBy: string;
	module: SoeModule;
	name: string;
	sortorder: number;
	state: SoeEntityState;
	sysImportHeadId: number;
	sysImportRelations: ISysImportRelationDTO[];
	sysImportSelects: ISysImportSelectDTO[];
}
export interface ISysImportRelationDTO {
	sysImportHeadId: number;
	sysImportRelationId: number;
	tableChild: string;
	tableParent: string;
}
export interface ISysImportSelectColumnSettings {
	column: string;
	dataType: string;
	mandatory: boolean;
	position: number;
	sysImportSelectColumnSettingsId: number;
	text: string;
}
export interface ISysImportSelectDTO {
	groupBy: string;
	level: number;
	name: string;
	orderBy: string;
	select: string;
	settingObjects: ISysImportSelectColumnSettings[];
	settings: string;
	sysImportHeadId: number;
	sysImportSelectId: number;
	where: string;
}
export interface ISysInformationSysCompDbDTO {
	notificationSent: Date;
	siteName: string;
	sysCompDbId: number;
}
export interface ISysJobDTO {
	allowParallelExecution: boolean;
	assemblyName: string;
	className: string;
	created: Date;
	createdBy: string;
	description: string;
	modified: Date;
	modifiedBy: string;
	name: string;
	state: SoeEntityState;
	sysJobId: number;
	sysJobSettings: ISysJobSettingDTO[];
}
export interface ISysJobSettingDTO {
	boolData: boolean;
	dataType: SettingDataType;
	dateData: Date;
	decimalData: number;
	intData: number;
	name: string;
	strData: string;
	sysJobSettingId: number;
	timeData: Date;
	type: SysJobSettingType;
}
export interface ISysPayrollPriceDTO {
	amount: number;
	amountType: TermGroup_SysPayrollPriceAmountType;
	amountTypeName: string;
	code: string;
	created: Date;
	createdBy: string;
	fromDate: Date;
	intervals: ISysPayrollPriceIntervalDTO[];
	isModified: boolean;
	modified: Date;
	modifiedBy: string;
	name: string;
	state: SoeEntityState;
	sysCountryId: number;
	sysPayrollPriceId: number;
	sysTermId: number;
	type: TermGroup_SysPayrollPriceType;
	typeName: string;
}
export interface ISysPayrollPriceIntervalDTO {
	amount: number;
	amountType: TermGroup_SysPayrollPriceAmountType;
	amountTypeName: string;
	created: Date;
	createdBy: string;
	fromInterval: number;
	modified: Date;
	modifiedBy: string;
	state: SoeEntityState;
	sysPayrollPrice: TermGroup_SysPayrollPrice;
	sysPayrollPriceId: number;
	sysPayrollPriceIntervalId: number;
	toInterval: number;
}
export interface ISysPositionDTO {
	code: string;
	created: Date;
	createdBy: string;
	description: string;
	modified: Date;
	modifiedBy: string;
	name: string;
	state: SoeEntityState;
	sysCountryCode: string;
	sysCountryId: number;
	sysLanguageCode: string;
	sysLanguageId: number;
	sysPositionId: number;
}
export interface ISysPositionGridDTO {
	code: string;
	description: string;
	isLinked: boolean;
	name: string;
	selected: boolean;
	sysCountryCode: string;
	sysLanguageCode: string;
	sysPositionId: number;
}
export interface ISysReportTemplateViewGridDTO {
	description: string;
	groupName: string;
	name: string;
	reportNr: number;
	sysCountryIds: number[];
	sysReportTemplateId: number;
	sysReportTemplateTypeName: string;
}
export interface ISysScheduledJobDTO {
	allowParallelExecution: boolean;
	created: Date;
	createdBy: string;
	databaseName: string;
	description: string;
	executeTime: Date;
	executeUserId: number;
	jobStatusMessage: string;
	modified: Date;
	modifiedBy: string;
	name: string;
	recurrenceCount: number;
	recurrenceDate: Date;
	recurrenceInterval: string;
	recurrenceType: ScheduledJobRecurrenceType;
	retryCount: number;
	retryTypeForExternalError: ScheduledJobRetryType;
	retryTypeForInternalError: ScheduledJobRetryType;
	state: ScheduledJobState;
	stateName: string;
	sysJob: ISysJobDTO;
	sysJobId: number;
	sysJobSettings: ISysJobSettingDTO[];
	sysScheduledJobId: number;
	type: ScheduledJobType;
}
export interface ISysScheduledJobLogDTO {
	batchNr: number;
	logLevel: number;
	logLevelName: string;
	message: string;
	sysScheduledJobId: number;
	sysScheduledJobLogId: number;
	sysScheduledJobName: string;
	time: Date;
}
export interface ISysTermDTO {
	created: Date;
	createdBy: string;
	langId: number;
	modified: Date;
	modifiedBy: string;
	name: string;
	postChange: PostChange;
	sysTermGroupId: number;
	sysTermId: number;
	translationKey: string;
}
export interface ISysTimeIntervalDTO {
	name: string;
	period: TermGroup_TimeIntervalPeriod;
	sort: number;
	start: TermGroup_TimeIntervalStart;
	startOffset: number;
	stop: TermGroup_TimeIntervalStop;
	stopOffset: number;
	sysTermId: number;
	sysTimeIntervalId: number;
}
export interface ISysVehicleDTO {
	codeForComparableModel: string;
	comparableModel: ISysVehicleDTO;
	fuelType: TermGroup_SysVehicleFuelType;
	manufacturingYear: number;
	modelCode: string;
	price: number;
	priceAdjustment: number;
	priceAfterReduction: number;
	vehicleMake: string;
	vehicleModel: string;
}
export interface ISysWholesellerDTO {
	hasEdiFeature: boolean;
	isOnlyInComp: boolean;
	messageTypes: string;
	name: string;
	sysCountryId: number;
	sysCurrencyId: number;
	sysWholesellerEdiId: number;
	sysWholesellerId: number;
	sysWholeSellerSettingDTOs: ISysWholeSellerSettingDTO[];
	type: number;
}
export interface ISysWholeSellerSettingDTO {
	boolvalue: boolean;
	decimalValue: number;
	intValue: number;
	settingType: number;
	stringValue: string;
	sysWholesellerId: number;
	sysWholesellerSettingId: number;
}
export interface ITemplateScheduleEmployeeDTO {
	copyFromEmployeeId: number;
	copyFromTemplateHeadId: number;
	currentTemplate: string;
	currentTemplateNbrOfWeeks: number;
	employeeId: number;
	employeeNr: string;
	employeeNrSort: string;
	isRunning: boolean;
	isSelected: boolean;
	name: string;
	nbrOfWeeks: number;
	resultError: string;
	resultSuccess: boolean;
	templateStartDate: Date;
	templateStopDate: Date;
}
export interface ITextblockDTO extends SoftOne.Soe.Common.DTO.ITextblockDTOBase {
	actorCompanyId: number;
	created: Date;
	createdBy: string;
	headline: string;
	modified: Date;
	modifiedBy: string;
	showInContract: boolean;
	showInInvoice: boolean;
	showInOffer: boolean;
	showInOrder: boolean;
	showInPurchase: boolean;
	state: SoeEntityState;
	type: number;
}
export interface ITextSelectionDTO extends IReportDataSelectionDTO {
	text: string;
}
export interface ITimeAbsenceDetailDTO {
	created: Date;
	createdBy: string;
	date: Date;
	dayName: string;
	dayOfWeekNr: number;
	dayTypeId: number;
	dayTypeName: string;
	employeeId: number;
	employeeNrAndName: string;
	holidayAndDayTypeName: string;
	holidayId: number;
	holidayName: string;
	isHoliday: boolean;
	manuallyAdjusted: boolean;
	modified: Date;
	modifiedBy: string;
	ratio: number;
	ratioText: string;
	sysPayrollTypeLevel3: number;
	sysPayrollTypeLevel3Name: string;
	timeBlockDateDetailId: number;
	timeBlockDateId: number;
	timeDeviationCauseId: number;
	timeDeviationCauseName: string;
	weekInfo: string;
	weekNr: number;
}
export interface ITimeAbsenceRuleHeadDTO {
	actorCompanyId: number;
	companyName: string;
	created: Date;
	createdBy: string;
	description: string;
	employeeGroupIds: number[];
	employeeGroupNames: string;
	modified: Date;
	modifiedBy: string;
	name: string;
	state: SoeEntityState;
	timeAbsenceRuleHeadId: number;
	timeAbsenceRuleRows: ITimeAbsenceRuleRowDTO[];
	timeCode: ITimeCodeDTO;
	timeCodeId: number;
	timeCodeName: string;
	type: TermGroup_TimeAbsenceRuleType;
	typeName: string;
}
export interface ITimeAbsenceRuleRowDTO {
	created: Date;
	createdBy: string;
	hasMultiplePayrollProducts: boolean;
	modified: Date;
	modifiedBy: string;
	payrollProductId: number;
	payrollProductName: string;
	payrollProductNr: string;
	payrollProductRows: ITimeAbsenceRuleRowPayrollProductsDTO[];
	scope: TermGroup_TimeAbsenceRuleRowScope;
	scopeName: string;
	start: number;
	state: SoeEntityState;
	stop: number;
	timeAbsenceRuleHeadId: number;
	timeAbsenceRuleRowId: number;
	type: TermGroup_TimeAbsenceRuleRowType;
	typeName: string;
}
export interface ITimeAbsenceRuleRowPayrollProductsDTO {
	sourcePayrollProductId: number;
	sourcePayrollProductName: string;
	sourcePayrollProductNr: string;
	targetPayrollProductId: number;
	targetPayrollProductName: string;
	targetPayrollProductNr: string;
	timeAbsenceRuleRowPayrollProductsId: number;
}
export interface ITimeAccumulatorDTO {
	actorCompanyId: number;
	created: Date;
	createdBy: string;
	description: string;
	employeeGroupRules: ITimeAccumulatorEmployeeGroupRuleDTO[];
	finalSalary: boolean;
	invoiceProducts: ITimeAccumulatorInvoiceProductDTO[];
	modified: Date;
	modifiedBy: string;
	name: string;
	payrollProducts: ITimeAccumulatorPayrollProductDTO[];
	showInTimeReports: boolean;
	state: SoeEntityState;
	timeAccumulatorId: number;
	timeCodeId: number;
	timeCodes: ITimeAccumulatorTimeCodeDTO[];
	timeWorkReductionEarningId: number;
	timePeriodHeadId: number;
	timePeriodHeadName: string;
	type: TermGroup_TimeAccumulatorType;
	typeName: string;
	useTimeWorkAccount: boolean;
	useTimeWorkReductionWithdrawal: boolean;
}
export interface ITimeAccumulatorEmployeeGroupRuleDTO {
	employeeGroupId: number;
	maxMinutes: number;
	maxMinutesWarning: number;
	maxTimeCodeId: number;
	minMinutes: number;
	minMinutesWarning: number;
	minTimeCodeId: number;
	scheduledJobHeadId: number;
	showOnPayrollSlip: boolean;
	type: TermGroup_AccumulatorTimePeriodType;
	thresholdMinutes: number;
}
export interface ITimeAccumulatorTimeWorkReductionEarningEmployeeGroupDTO {
	timeAccumulatorTimeWorkReductionEarningEmployeeGroupId: number;
	employeeGroupId: number;
	timeWorkReductionEarningId: number;
	dateFrom?: Date;
	dateTo?: Date;
	state: SoeEntityState;
}
export interface ITimeWorkReductionEarningDTO {
	timeWorkReductionEarningId: number;
	minutesWeight: number;
	periodType: number
	state: SoeEntityState;

	timeAccumulatorTimeWorkReductionEarningEmployeeGroup: ITimeAccumulatorTimeWorkReductionEarningEmployeeGroupDTO[];
}
export interface ITimeAccumulatorGridDTO {
	description: string;
	isSelected: boolean;
	name: string;
	state: SoeEntityState;
	timeAccumulatorId: number;
	type: TermGroup_TimeAccumulatorType;
	typeName: string;
}
export interface ITimeAccumulatorInvoiceProductDTO {
	factor: number;
	invoiceProductId: number;
}
export interface ITimeAccumulatorItem {
	accTodayStartDate: Date;
	accTodayStopDate: Date;
	employeeGroupRuleBoundaries: string;
	employeeGroupRules: ITimeAccumulatorRuleItem[];
	hasPlanningPeriod: boolean;
	hasTimePeriod: boolean;
	name: string;
	planningPeriodDatesText: string;
	planningPeriodName: string;
	planningPeriodStartDate: Date;
	planningPeriodStopDate: Date;
	sumAccToday: number;
	sumAccTodayIsQuantity: boolean;
	sumAccTodayValue: number;
	sumInvoiceAccToday: number;
	sumInvoicePeriod: number;
	sumInvoicePlanningPeriod: number;
	sumInvoiceToday: number;
	sumInvoiceYear: number;
	sumPayrollAccToday: number;
	sumPayrollPeriod: number;
	sumPayrollPlanningPeriod: number;
	sumPayrollToday: number;
	sumPayrollYear: number;
	sumPeriod: number;
	sumPeriodIsQuantity: boolean;
	sumPlanningPeriod: number;
	sumPlanningPeriodIsQuantity: boolean;
	sumTimeCodeAccToday: number;
	sumTimeCodePeriod: number;
	sumTimeCodePlanningPeriod: number;
	sumTimeCodeToday: number;
	sumTimeCodeYear: number;
	sumToday: number;
	sumTodayIsQuantity: boolean;
	sumYear: number;
	sumYearIsQuantity: boolean;
	sumYearWithIB: number;
	timeAccumulatorBalanceYear: number;
	timeAccumulatorId: number;
	timePeriodDatesText: string;
	timePeriodName: string;
}
export interface ITimeAccumulatorPayrollProductDTO {
	factor: number;
	payrollProductId: number;
}
export interface ITimeAccumulatorRuleItem {
	comparison: SoeTimeAccumulatorComparison;
	diff: string;
	diffMinutes: number;
	diffValue: System.ITimeSpan;
	periodType: TermGroup_AccumulatorTimePeriodType;
	showError: boolean;
	showWarning: boolean;
	valueMinutes: number;
	warningMinutes: number;
}
export interface ITimeAccumulatorTimeCodeDTO {
	factor: number;
	importDefault: boolean;
	isHeadTimeCode: boolean;
	timeCodeId: number;
}
export interface ITimeAttestCalculationFunctionValidationDTO {
	applySilent: boolean;
	canOverride: boolean;
	message: string;
	option: SoeTimeAttestFunctionOption;
	success: boolean;
	title: string;
	validItems: IAttestEmployeeDaySmallDTO[];
}
export interface ITimeBlockDateDTO {
	date: Date;
	discardedBreakEvaluation: boolean;
	employeeId: number;
	stampingStatus: TermGroup_TimeBlockDateStampingStatus;
	status: SoeTimeBlockDateStatus;
	timeBlockDateId: number;
}
export interface ITimeBreakTemplateGridDTO {
	actorCompanyId: number;
	dayOfWeeks: ISmallGenericType[];
	dayTypes: IDayTypeDTO[];
	majorMinTimeAfterStart: number;
	majorMinTimeBeforeEnd: number;
	majorNbrOfBreaks: number;
	majorTimeCodeBreakGroupId: number;
	majorTimeCodeBreakGroupName: string;
	minorMinTimeAfterStart: number;
	minorMinTimeBeforeEnd: number;
	minorNbrOfBreaks: number;
	minorTimeCodeBreakGroupId: number;
	minorTimeCodeBreakGroupName: string;
	minTimeBetweenBreaks: number;
	rowNr: number;
	shiftLength: number;
	shiftStartFromTimeMinutes: number;
	shiftTypes: IShiftTypeDTO[];
	startDate: Date;
	state: SoeEntityState;
	stopDate: Date;
	timeBreakTemplateId: number;
	useMaxWorkTimeBetweenBreaks: boolean;
	validationResult: IActionResult;
}
export interface ITimeCalendarPeriodDTO {
	date: Date;
	dayDescription: string;
	payrollProducts: ITimeCalendarPeriodPayrollProductDTO[];
	type1: number;
	type1ToolTip: string;
	type2: number;
	type2ToolTip: string;
	type3: number;
	type3ToolTip: string;
	type4: number;
	type4ToolTip: string;
}
export interface ITimeCalendarPeriodPayrollProductDTO {
	amount: number;
	name: string;
	number: string;
	payrollProductId: number;
	sysPayrollTypeLevel1: number;
	sysPayrollTypeLevel2: number;
	sysPayrollTypeLevel3: number;
	sysPayrollTypeLevel4: number;
}
export interface ITimeCalendarSummaryDTO {
	amount: number;
	days: number;
	name: string;
	number: string;
	occations: number;
	payrollProductId: number;
}
export interface ITimeCodeAbsenceDTO extends ITimeCodeBaseDTO {
	adjustQuantityByBreakTime: TermGroup_AdjustQuantityByBreakTime;
	adjustQuantityTimeCodeId: number;
	adjustQuantityTimeScheduleTypeId: number;
	isAbsence: boolean;
	kontekId: number;
}
export interface ITimeCodeAdditionDeductionDTO extends ITimeCodeBaseDTO {
	comment: string;
	commentMandatory: boolean;
	expenseType: TermGroup_ExpenseType;
	hasInvoiceProducts: boolean;
	hideForEmployee: boolean;
	fixedQuantity: number;
	showInTerminal: boolean;
	stopAtAccounting: boolean;
	stopAtComment: boolean;
	stopAtDateStart: boolean;
	stopAtDateStop: boolean;
	stopAtPrice: boolean;
	stopAtVat: boolean;
}
export interface ITimeCodeBaseDTO {
	actorCompanyId: number;
	classification: TermGroup_TimeCodeClassification
	code: string;
	created: Date;
	createdBy: string;
	description: string;
	factorBasedOnWorkPercentage: boolean;
	invoiceProducts: ITimeCodeInvoiceProductDTO[];
	minutesByConstantRules: number;
	modified: Date;
	modifiedBy: string;
	name: string;
	payed: boolean;
	payrollProducts: ITimeCodePayrollProductDTO[];
	registrationType: TermGroup_TimeCodeRegistrationType;
	roundingType: TermGroup_TimeCodeRoundingType;
	roundingValue: number;
	roundingTimeCodeId: number;
	roundingInterruptionTimeCodeId: number;
	roundingGroupKey: string;
	roundStartTime: boolean;
	state: SoeEntityState;
	timeCodeId: number;
	timeCodeRuleTime: Date;
	timeCodeRuleType: number;
	timeCodeRuleValue: number;
	type: SoeTimeCodeType;
}
export interface ITimeCodeBreakDTO extends ITimeCodeBaseDTO {
	defaultMinutes: number;
	employeeGroupIds: number[];
	maxMinutes: number;
	minMinutes: number;
	startTime: Date;
	startTimeMinutes: number;
	startType: number;
	stopTimeMinutes: number;
	stopType: number;
	template: boolean;
	timeCodeBreakGroupId: number;
	timeCodeDeviationCauses: ITimeCodeBreakTimeCodeDeviationCauseDTO[];
	timeCodeRules: ITimeCodeRuleDTO[];
}
export interface ITimeCodeBreakGroupGridDTO {
	description: string;
	name: string;
	timeCodeBreakGroupId: number;
}
export interface ITimeCodeBreakSmallDTO {
	code: string;
	defaultMinutes: number;
	name: string;
	startTime: Date;
	startTimeMinutes: number;
	stopTimeMinutes: number;
	timeCodeId: number;
}
export interface ITimeCodeBreakTimeCodeDeviationCauseDTO {
	timeCodeBreakId: number;
	timeCodeBreakTimeCodeDeviationCauseId: number;
	timeCodeDeviationCauseId: number;
	timeCodeId: number;
}
export interface ITimeCodeDTO {
	actorCompanyId: number;
	code: string;
	companyName: string;
	created: Date;
	createdBy: string;
	defaultMinutes: number;
	description: string;
	factorBasedOnWorkPercentage: boolean;
	isAbsence: boolean;
	isWorkOutsideSchedule: boolean;
	kontekId: number;
	maxMinutes: number;
	minMinutes: number;
	minutesByConstantRules: number;
	modified: Date;
	modifiedBy: string;
	name: string;
	note: string;
	payed: boolean;
	payrollProductNames: string;
	payrollProducts: IPayrollProductGridDTO[];
	registrationType: TermGroup_TimeCodeRegistrationType;
	roundingType: TermGroup_TimeCodeRoundingType;
	roundingValue: number;
	roundingTimeCodeId: number;
	roundingGroupKey: string;
	roundStartTime: boolean;
	startTime: Date;
	startTimeMinutes: number;
	startType: number;
	state: SoeEntityState;
	stopTimeMinutes: number;
	stopType: number;
	template: boolean;
	timeCodeBreakEmployeeGroupNames: string;
	timeCodeBreakGroupId: number;
	timeCodeBreakGroupName: string;
	timeCodeId: number;
	timeCodeRules: ITimeCodeRuleDTO[];
	type: SoeTimeCodeType;
}
export interface ITimeCodeGridDTO {
	actorCompanyId: number;
	code: string;
	description: string;
	isActive: boolean;
	name: string;
	payrollProductNames: string;
	state: SoeEntityState;
	templateText: string;
	timeCodeBreakEmployeeGroupNames: string;
	timeCodeBreakGroupName: string;
	timeCodeId: number;
}
export interface ITimeCodeInvoiceProductDTO {
	factor: number;
	invoiceProductId: number;
	invoiceProductPrice: number;
	timeCodeId: number;
	timeCodeInvoiceProductId: number;
}
export interface ITimeCodeMaterialDTO extends ITimeCodeBaseDTO {
	note: string;
}
export interface ITimeCodePayrollProductDTO {
	factor: number;
	payrollProductId: number;
	timeCodeId: number;
	timeCodePayrollProductId: number;
}
export interface ITimeCodeRuleDTO {
	time: Date;
	type: number;
	value: number;
}
export interface ITimeCodeSaveDTO {
	adjustQuantityByBreakTime: TermGroup_AdjustQuantityByBreakTime;
	adjustQuantityTimeCodeId: number;
	adjustQuantityTimeScheduleTypeId: number;
	classification: TermGroup_TimeCodeClassification;
	code: string;
	comment: string;
	commentMandatory: boolean;
	defaultMinutes: number;
	description: string;
	employeeGroupIds: number[];
	expenseType: TermGroup_ExpenseType;
	factorBasedOnWorkPercentage: boolean;
	fixedQuantity: number;
	invoiceProducts: ITimeCodeInvoiceProductDTO[];
	isAbsence: boolean;
	isWorkOutsideSchedule: boolean;
	kontekId: number;
	maxMinutes: number;
	minMinutes: number;
	minutesByConstantRules: number;
	name: string;
	note: string;
	payed: boolean;
	payrollProducts: ITimeCodePayrollProductDTO[];
	registrationType: TermGroup_TimeCodeRegistrationType;
	roundingType: TermGroup_TimeCodeRoundingType;
	roundingValue: number;
	roundingTimeCodeId: number;
	roundingGroupKey: string;
	roundStartTime: boolean;
	startTime: Date;
	startTimeMinutes: number;
	startType: number;
	state: SoeEntityState;
	stopAtAccounting: boolean;
	stopAtComment: boolean;
	stopAtDateStart: boolean;
	stopAtDateStop: boolean;
	stopAtPrice: boolean;
	stopAtVat: boolean;
	stopTimeMinutes: number;
	stopType: number;
	template: boolean;
	timeCodeBreakGroupId: number;
	timeCodeDeviationCauses: ITimeCodeBreakTimeCodeDeviationCauseDTO[];
	timeCodeId: number;
	timeCodeRules: ITimeCodeRuleDTO[];
	timeCodeRuleTime: Date;
	timeCodeRuleType: TermGroup_TimeCodeRuleType;
	timeCodeRuleValue: number;
	type: SoeTimeCodeType;
}
export interface ITimeCodeWorkDTO extends ITimeCodeBaseDTO {
	adjustQuantityByBreakTime: TermGroup_AdjustQuantityByBreakTime;
	adjustQuantityTimeCodeId: number;
	adjustQuantityTimeScheduleTypeId: number;
	isWorkOutsideSchedule: boolean;
}
export interface ITimeDeviationCauseDTO {
	actorCompanyId: number;
	adjustTimeInsideOfPlannedAbsence: number;
	adjustTimeOutsideOfPlannedAbsence: number;
	allowGapToPlannedAbsence: boolean;
	attachZeroDaysNbrOfDaysAfter: number;
	attachZeroDaysNbrOfDaysBefore: number;
	calculateAsOtherTimeInSales: boolean;
	candidateForOvertime: boolean;
	changeCauseInsideOfPlannedAbsence: number;
	changeCauseOutsideOfPlannedAbsence: number;
	changeDeviationCauseAccordingToPlannedAbsence: boolean;
	created: Date;
	createdBy: string;
	description: string;
	employeeGroupIds: number[];
	employeeRequestPolicyNbrOfDaysBefore: number;
	employeeRequestPolicyNbrOfDaysBeforeCanOverride: boolean;
	excludeFromPresenceWorkRules: boolean;
	excludeFromScheduleWorkRules: boolean;
	extCode: string;
	externalCodes: string[];
	imageSource: string;
	isAbsence: boolean;
	isPresence: boolean;
	isVacation: boolean;
	mandatoryNote: boolean;
	mandatoryTime: boolean;
	modified: Date;
	modifiedBy: string;
	name: string;
	notChargeable: boolean;
	onlyWholeDay: boolean;
	payed: boolean;
	showZeroDaysInAbsencePlanning: boolean;
	specifyChild: boolean;
	state: SoeEntityState;
	timeCode: ITimeCodeDTO;
	timeCodeId: number;
	timeCodeName: string;
	timeDeviationCauseId: number;
	type: TermGroup_TimeDeviationCauseType;
	typeName: string;
	validForHibernating: boolean;
	validForStandby: boolean;
}
export interface ITimeDeviationCauseGridDTO {
	candidateForOvertime: boolean;
	description: string;
	imageSource: string;
	mandatoryNote: boolean;
	name: string;
	specifyChild: boolean;
	timeCodeName: string;
	timeDeviationCauseId: number;
	type: TermGroup_TimeDeviationCauseType;
	typeName: string;
	validForHibernating: boolean;
	validForStandby: boolean;
}
export interface ITimeEmployeeTreeDTO {
	actorCompanyId: number;
	cacheKey: string;
	grouping: TermGroup_AttestTreeGrouping;
	groupNodes: ITimeEmployeeTreeGroupNodeDTO[];
	mode: SoeAttestTreeMode;
	sorting: TermGroup_AttestTreeSorting;
	startDate: Date;
	stopDate: Date;
	settings: ITimeEmployeeTreeSettings;
	timePeriod: ITimePeriodDTO;
}
export interface ITimeEmployeeTreeGroupNodeDTO {
	attestStateColor: string;
	attestStateId: number;
	attestStateName: string;
	attestStates: IAttestStateDTO[];
	attestStateSort: number;
	childGroupNodes: ITimeEmployeeTreeGroupNodeDTO[];
	code: string;
	definedSort: number;
	employeeNodes: ITimeEmployeeTreeNodeDTO[];
	expanded: boolean;
	guid: System.IGuid;
	hasEnded: boolean;
	hasWarningsTime: boolean;
	hasWarningsPayroll: boolean;
	hasWarningsPayrollStopping: boolean;
	id: number;
	isAdditional: boolean;
	name: string;
	timeEmployeePeriods: IAttestEmployeePeriodDTO[];
	type: number;
	warningMessageTime: string;
	warningMessagePayroll: string;
	warningMessagePayrollStopping: string;
}



export interface ITimeEmployeeTreeNodeDTO {
	additionalOnAccountIds: number[];
	attestStateColor: string;
	attestStateId: number;
	attestStateName: string;
	attestStates: IAttestStateDTO[];
	attestStateSort: number;
	autogenTimeblocks: boolean;
	disbursementAccountNr: string;
	disbursementAccountNrIsMissing: boolean;
	disbursementMethod: number;
	disbursementMethodIsCash: boolean;
	disbursementMethodIsUnknown: boolean;
	disbursementMethodName: string;
	employeeEndDate: Date;
	employeeFirstName: string;
	employeeGroupId: number;
	employeeId: number;
	employeeLastName: string;
	employeeName: string;
	employeeNr: string;
	employeeNrAndName: string;
	employeeSex: TermGroup_Sex;
	finalSalaryStatus: SoeEmploymentFinalSalaryStatus;
	groupId: number;
	guid: System.IGuid;
	hasEnded: boolean;
	hasWarningsTime: boolean;
	hasWarningsPayroll: boolean;
	hasWarningsPayrollStopping: boolean;
	hibernatingText: string;
	isAdditional: boolean;
	isStamping: boolean;
	socialSec: string;
	taxSettingsAreMissing: boolean;
	timeReportType: TermGroup_TimeReportType;
	tooltip: string;
	userId: number;
	visible: boolean;
	warningMessageTime: string;
	warningMessagePayroll: string;
	warningMessagePayrollStopping: string;
}

export interface ITimeTreeEmployeeWarning {
	employeeId: number;
	isStopping: boolean;
	key: number;
	message: string;
	warningGroup: SoeTimeAttestWarningGroup;
}

export interface ITimeEmployeeTreeSettings {
	cacheKeyToUse: string;
	doNotShowAttested: boolean;
	doNotShowCalculated: boolean;
	doNotShowDaysOutsideEmployeeAccount: boolean;
	doNotShowWithoutTransactions: boolean;
	doRefreshFinalSalaryStatus: boolean;
	doShowOnlyShiftSwaps: boolean;
	excludeDuplicateEmployees: boolean;
	filterAttestStateIds: number[];
	filterEmployeeAuthModelIds: number[];
	filterEmployeeIds: number[];
	filterMessageGroupId: number;
	includeAdditionalEmployees: boolean;
	includeEmptyGroups: boolean;
	includeEnded: boolean;
	isProjectAttest: boolean;
	searchPattern: string;
	showOnlyAppliedFinalSalary: boolean;
	showOnlyApplyFinalSalary: boolean;
}

export interface ITimeHalfdayDTO {
	created: Date;
	createdBy: string;
	dayTypeId: number;
	dayTypeName: string;
	description: string;
	modified: Date;
	modifiedBy: string;
	name: string;
	state: SoeEntityState;
	timeCodeBreaks: ITimeCodeDTO[];
	timeHalfdayId: number;
	type: SoeTimeHalfdayType;
	typeName: string;
	value: number;
}
export interface ITimeHibernatingAbsenceHeadDTO {
	actorCompanyId: number;
	created: Date;
	createdBy: string;
	employeeId: number;
	employeeName: string;
	employeeNr: string;
	employment: IEmploymentDTO;
	employmentId: number;
	modified: Date;
	modifiedBy: string;
	rows: ITimeHibernatingAbsenceRowDTO[];
	state: SoeEntityState;
	timeDeviationCauseId: number;
	timeHibernatingAbsenceHeadId: number;
}
export interface ITimeLeisureCodeSmallDTO {
	code: string;
	name: string;
	timeLeisureCodeId: number;
}
export interface ITimePeriodDTO {
	comment: string;
	created: Date;
	createdBy: string;
	extraPeriod: boolean;
	hasRequiredPayrollProperties: boolean;
	isModified: boolean;
	modified: Date;
	modifiedBy: string;
	name: string;
	nameAndPaymentDate: string;
	paymentDate: Date;
	paymentDateString: string;
	payrollStartDate: Date;
	payrollStopDate: Date;
	rowNr: number;
	showAsDefault: boolean;
	sortDate: Date;
	startDate: Date;
	stopDate: Date;
	timePeriodHead: ITimePeriodHeadDTO;
	timePeriodHeadId: number;
	timePeriodId: number;
}
export interface ITimePeriodHeadDTO {
	accountId: number;
	actorCompanyId: number;
	childId: number;
	created: Date;
	createdBy: string;
	description: string;
	modified: Date;
	modifiedBy: string;
	name: string;
	state: SoeEntityState;
	timePeriodHeadId: number;
	timePeriods: ITimePeriodDTO[];
	timePeriodType: TermGroup_TimePeriodType;
	timePeriodTypeName: string;
}
export interface ITimePeriodHeadGridDTO {
	accountName: string;
	childName: string;
	description: string;
	name: string;
	timePeriodHeadId: number;
	timePeriodType: TermGroup_TimePeriodType;
	timePeriodTypeName: string;
}
export interface ITimeProjectDTO extends IProjectDTO {
	hasInvoices: boolean;
	invoiceProductAccountingPrio: string;
	numberOfInvoices: number;
	orderTemplateId: number;
	parentProjectName: string;
	parentProjectNr: string;
	payrollProductAccountingPrio: string;
	projectWeekTotals: IProjectWeekTotal[];
}
export interface ITimeRuleEditDTO {
	adjustStartToTimeBlockStart: boolean;
	breakIfAnyFailed: boolean;
	created: Date;
	createdBy: string;
	dayTypeIds: number[];
	description: string;
	employeeGroupIds: number[];
	exportImportUnmatched: ITimeRuleExportImportUnmatchedDTO[];
	exportStartExpression: string;
	exportStopExpression: string;
	factor: number;
	imported: boolean;
	importStartExpression: string;
	importStopExpression: string;
	inconvenientWorkHourRule: ITimeRuleEditIwhDTO;
	isInconvenientWorkHours: boolean;
	isStandby: boolean;
	modified: Date;
	modifiedBy: string;
	name: string;
	ruleStartDirection: number;
	ruleStopDirection: number;
	sort: number;
	standardMinutes: number;
	startDate: Date;
	state: SoeEntityState;
	stopDate: Date;
	timeCodeId: number;
	timeCodeMaxLength: number;
	timeCodeMaxPerDay: boolean;
	timeDeviationCauseIds: number[];
	timeRuleExpressions: ITimeRuleExpressionDTO[];
	timeRuleId: number;
	timeScheduleTypeIds: number[];
	type: SoeTimeRuleType;
}
export interface ITimeRuleEditIwhDTO {
	information: string;
	length: string;
	payrollProductFactor: number;
	payrollProductName: string;
	startTime: Date;
	stopTime: Date;
}
export interface ITimeRuleExportImportDayTypeDTO {
	dayTypeId: number;
	matchedDayTypeId: number;
	name: string;
	sysDayTypeId: number;
	type: TermGroup_SysDayType;
}
export interface ITimeRuleExportImportDTO {
	dayTypes: ITimeRuleExportImportDayTypeDTO[];
	employeeGroups: ITimeRuleExportImportEmployeeGroupDTO[];
	exportedFromCompany: string;
	filename: string;
	originalJson: string;
	timeCodes: ITimeRuleExportImportTimeCodeDTO[];
	timeDeviationCauses: ITimeRuleExportImportTimeDeviationCauseDTO[];
	timeRules: ITimeRuleEditDTO[];
	timeScheduleTypes: ITimeRuleExportImportTimeScheduleTypeDTO[];
}
export interface ITimeRuleExportImportEmployeeGroupDTO {
	employeeGroupId: number;
	matchedEmployeeGroupId: number;
	name: string;
}
export interface ITimeRuleExportImportTimeCodeDTO {
	code: string;
	matchedTimeCodeId: number;
	name: string;
	timeCodeId: number;
}
export interface ITimeRuleExportImportTimeDeviationCauseDTO {
	matchedTimeDeviationCauseId: number;
	name: string;
	timeDeviationCauseId: number;
	type: TermGroup_TimeDeviationCauseType;
}
export interface ITimeRuleExportImportTimeScheduleTypeDTO {
	code: string;
	matchedTimeScheduleTypeId: number;
	name: string;
	timeScheduleTypeId: number;
}
export interface ITimeRuleExportImportUnmatchedDTO {
	code: string;
	id: number;
	name: string;
	type: TimeRuleExportImportUnmatchedType;
}
export interface ITimeRuleExpressionDTO {
	isStart: boolean;
	timeRuleExpressionId: number;
	timeRuleId: number;
	timeRuleOperands: ITimeRuleOperandDTO[];
}
export interface ITimeRuleGridDTO {
	actorCompanyId: number;
	adjustStartToTimeBlockStart: string;
	breakIfAnyFailed: string;
	dayTypeNames: string;
	description: string;
	employeeGroupNames: string;
	imported: boolean;
	internal: boolean;
	isActive: boolean;
	isInconvenientWorkHours: string;
	isStandby: string;
	name: string;
	sort: number;
	standardMinutes: string;
	startDate: Date;
	startDirection: SoeTimeRuleDirection;
	startDirectionName: string;
	startExpression: string;
	stopDate: Date;
	stopExpression: string;
	timeCodeId: number;
	timeCodeMaxLength: number;
	timeCodeName: string;
	timeDeviationCauseNames: string;
	timeRuleId: number;
	timeScheduleTypesNames: string;
	type: SoeTimeRuleType;
	typeName: string;
}
export interface ITimeRuleImportedDetailsDTO {
	companyName: string;
	imported: Date;
	importedBy: string;
	json: string;
	originalJson: string;
}
export interface ITimeRuleOperandDTO {
	comparisonOperator: SoeTimeRuleComparisonOperator;
	leftValueId: number;
	leftValueType: SoeTimeRuleValueType;
	minutes: number;
	operatorType: SoeTimeRuleOperatorType;
	orderNbr: number;
	rightValueId: number;
	rightValueType: SoeTimeRuleValueType;
	timeRuleExpressionId: number;
	timeRuleExpressionRecursive: ITimeRuleExpressionDTO;
	timeRuleExpressionRecursiveId: number;
	timeRuleOperandId: number;
}
export interface ITimeSalaryExportDTO {
	actorCompanyId: number;
	comment: string;
	created: Date;
	createdBy: string;
	exportDate: Date;
	exportFormat: SoeTimeSalaryExportFormat;
	exportTarget: SoeTimeSalaryExportTarget;
	extension: string;
	file1: number[];
	file2: number[];
	isPreliminary: boolean;
	isPreliminaryText: string;
	modified: Date;
	modifiedBy: string;
	startInterval: Date;
	state: SoeEntityState;
	stopInterval: Date;
	targetName: string;
	timeSalaryExportId: number;
}
export interface ITimeSalaryExportSelectionDTO {
	actorCompanyId: number;
	attestStateColor: string;
	attestStateId: number;
	attestStateName: string;
	dateFrom: Date;
	dateTo: Date;
	entirePeriodValidForExport: boolean;
	timeSalaryExportSelectionGroups: ITimeSalaryExportSelectionGroupDTO[];
}
export interface ITimeSalaryExportSelectionEmployeeDTO {
	attestStateColor: string;
	attestStateId: number;
	attestStateName: string;
	employeeId: number;
	employeeNr: string;
	entirePeriodValidForExport: boolean;
	name: string;
}
export interface ITimeSalaryExportSelectionGroupDTO {
	attestStateColor: string;
	attestStateId: number;
	attestStateName: string;
	entirePeriodValidForExport: boolean;
	id: number;
	name: string;
	timeSalaryExportSelectionEmployees: ITimeSalaryExportSelectionEmployeeDTO[];
	timeSalaryExportSelectionSubGroups: ITimeSalaryExportSelectionGroupDTO[];
}
export interface ITimeSalaryPaymentExportDTO {
	accountDepositNetAmount: number;
	accountDepositNetAmountCurrency: number;
	actorCompanyId: number;
	cashDepositNetAmount: number;
	created: Date;
	createdBy: string;
	exportDate: Date;
	exportFormat: SoeTimeSalaryPaymentExportFormat;
	exportType: TermGroup_TimeSalaryPaymentExportType;
	extension: string;
	file: number[];
	hasEmployeeDetails: boolean;
	isSelected: boolean;
	modified: Date;
	modifiedBy: string;
	paymentDate: Date;
	paymentDateString: string;
	payrollDateInterval: string;
	state: SoeEntityState;
	timePeriodHeadName: string;
	timePeriodId: number;
	timePeriodName: string;
	timeSalaryPaymentExportEmployees: ITimeSalaryPaymentExportEmployeeDTO[];
	timeSalaryPaymentExportId: number;
	typeName: string;
}
export interface ITimeSalaryPaymentExportEmployeeDTO {
	accountNr: string;
	accountNrGridStr: string;
	disbursementMethod: number;
	disbursementMethodName: string;
	employeeId: number;
	employeeNr: string;
	isSECashDeposit: boolean;
	isSEExtendedSelection: boolean;
	name: string;
	netAmount: number;
	netAmountCurrency: number;
	paymentRowKey: string;
}
export interface ITimeSalaryPaymentExportGridDTO {
	accountDepositNetAmount: number;
	accountDepositNetAmountCurrency: number;
	cashDepositNetAmount: number;
	currencyCode: string;
	currencyDate: Date;
	currencyRate: number;
	debitDate: Date;
	employees: ITimeSalaryPaymentExportEmployeeDTO[];
	exportDate: Date;
	exportType: TermGroup_TimeSalaryPaymentExportType;
	msgKey: string;
	paymentDate: Date;
	paymentKey: string;
	payrollDateInterval: string;
	salarySpecificationPublishDate: Date;
	timePeriodHeadName: string;
	timePeriodName: string;
	timeSalaryPaymentExportId: number;
	typeName: string;
}
export interface ITimeScheduleEventDTO {
	actorCompanyId: number;
	created: Date;
	createdBy: string;
	date: Date;
	description: string;
	modified: Date;
	modifiedBy: string;
	name: string;
	state: SoeEntityState;
	timeScheduleEventId: number;
	timeScheduleEventMessageGroups: ITimeScheduleEventMessageGroupDTO[];
}
export interface ITimeScheduleEmployeePeriodDetailDTO {
	date: Date;
	employeeId: number;
	state: SoeEntityState;
	timeLeisureCodeId: number;
	timeScheduleEmployeePeriodDetailId: number;
	timeScheduleEmployeePeriodId: number;
	timeScheduleScenarioHeadId: number;
	type: SoeTimeScheduleEmployeePeriodDetailType;
}
export interface ITimeScheduleEventForPlanningDTO {
	closingTime: Date;
	date: Date;
	description: string;
	name: string;
	openingHoursId: number;
	openingTime: Date;
	timeScheduleEventId: number;
}
export interface ITimeScheduleEventMessageGroupDTO {
	messageGroupId: number;
	timeScheduleEventId: number;
	timeScheduleEventMessageGroupId: number;
}
export interface ITimeSchedulePlanningDayDTO {
	absenceRequestShiftPlanningAction: number;
	absenceStartTime: Date;
	absenceStopTime: Date;
	absenceType: TermGroup_TimeScheduleTemplateBlockAbsenceType;
	accountId: number;
	accountIds: number[];
	accountName: string;
	approvalTypeId: number;
	belongsToNextDay: boolean;
	belongsToPreviousDay: boolean;
	break1Id: number;
	break1IsPreliminary: boolean;
	break1LinkStr: string;
	break1Minutes: number;
	break1StartTime: Date;
	break1TimeCodeId: number;
	break2Id: number;
	break2IsPreliminary: boolean;
	break2LinkStr: string;
	break2Minutes: number;
	break2StartTime: Date;
	break2TimeCodeId: number;
	break3Id: number;
	break3IsPreliminary: boolean;
	break3LinkStr: string;
	break3Minutes: number;
	break3StartTime: Date;
	break3TimeCodeId: number;
	break4Id: number;
	break4IsPreliminary: boolean;
	break4LinkStr: string;
	break4Minutes: number;
	break4StartTime: Date;
	break4TimeCodeId: number;
	breakTime: System.ITimeSpan;
	calculationGuid: System.IGuid;
	costPerHour: number;
	dayName: DayOfWeek;
	dayNumber: number;
	description: string;
	employeeChildId: number;
	employeeChildName: string;
	employeeId: number;
	employeeInfo: string;
	employeeName: string;
	employeePostId: number;
	employmentTaxCost: number;
	extraShift: boolean;
	grossNetDiff: System.ITimeSpan;
	grossTime: number;
	hasMultipleEmployeeAccountsOnDate: boolean;
	hasShiftRequest: boolean;
	hasSwapRequest: boolean;
	iamInQueue: boolean;
	incomingDeliveryRowId: number;
	isAbsenceRequest: boolean;
	isDeleted: boolean;
	isHiddenEmployee: boolean;
	isLended: boolean;
	isPreliminary: boolean;
	isVacant: boolean;
	iwhTime: System.ITimeSpan;
	linkStr: string;
	nbrOfWantedInQueue: number;
	nbrOfWeeks: number;
	netTime: number;
	originalBlockId: number;
	periodGuid: System.IGuid;
	plannedTime: number;
	shiftRequestAnswerType: XEMailAnswerType;
	shiftStatus: TermGroup_TimeScheduleTemplateBlockShiftStatus;
	shiftStatusName: string;
	shiftTypeCode: string;
	shiftTypeColor: string;
	shiftTypeDesc: string;
	shiftTypeId: number;
	shiftTypeName: string;
	shiftTypeTimeScheduleTypeCode: string;
	shiftTypeTimeScheduleTypeId: number;
	shiftTypeTimeScheduleTypeName: string;
	shiftUserStatus: TermGroup_TimeScheduleTemplateBlockShiftUserStatus;
	shiftUserStatusName: string;
	sourceTimeScheduleTemplateBlockId: number;
	staffingNeedsRowId: number;
	staffingNeedsRowPeriodId: number;
	startTime: Date;
	stopTime: Date;
	substituteShift: boolean;
	supplementChargeCost: number;
	swapShiftInfo: string;
	tasks: ITimeScheduleTemplateBlockTaskDTO[];
	tempTimeScheduleTemplateBlockId: number;
	timeCodeId: number;
	timeDeviationCauseId: number;
	timeDeviationCauseName: string;
	timeScheduleEmployeePeriodId: number;
	timeScheduleScenarioHeadId: number;
	timeScheduleTaskId: number;
	timeScheduleTemplateBlockId: number;
	timeScheduleTemplateHeadId: number;
	timeScheduleTemplatePeriodId: number;
	timeScheduleTypeCode: string;
	timeScheduleTypeFactors: ITimeScheduleTypeFactorSmallDTO[];
	timeScheduleTypeId: number;
	timeScheduleTypeIsNotScheduleTime: boolean;
	timeScheduleTypeName: string;
	totalCost: number;
	totalCostIncEmpTaxAndSuppCharge: number;
	type: TermGroup_TimeScheduleTemplateBlockType;
	userId: number;
	weekNr: number;
}
export interface ITimeSchedulePlanningMonthDetailDTO {
	absenceApproved: ITimeSchedulePlanningMonthDetailShiftDTO[];
	absenceRequested: ITimeSchedulePlanningMonthDetailShiftDTO[];
	assigned: ITimeSchedulePlanningMonthDetailShiftDTO[];
	date: Date;
	open: ITimeSchedulePlanningMonthDetailShiftDTO[];
	preliminary: ITimeSchedulePlanningMonthDetailShiftDTO[];
	unwanted: ITimeSchedulePlanningMonthDetailShiftDTO[];
	wanted: ITimeSchedulePlanningMonthDetailShiftDTO[];
}
export interface ITimeSchedulePlanningMonthDetailShiftDTO {
	deviationCauseName: string;
	employeeId: number;
	employeeName: string;
	employeeNr: string;
	isPreliminary: boolean;
	queue: string;
	shiftTimeRange: string;
	shiftTypeName: string;
}
export interface ITimeScheduleScenarioAccountDTO {
	accountId: number;
	accountName: string;
	timeScheduleScenarioAccountId: number;
	timeScheduleScenarioHeadId: number;
}
export interface ITimeScheduleScenarioEmployeeDTO {
	employeeId: number;
	employeeName: string;
	employeeNumberAndName: string;
	needsReplacement: boolean;
	replacementEmployeeId: number;
	replacementEmployeeNumberAndName: string;
	timeScheduleScenarioEmployeeId: number;
	timeScheduleScenarioHeadId: number;
}
export interface ITimeScheduleScenarioHeadDTO {
	accounts: ITimeScheduleScenarioAccountDTO[];
	actorCompanyId: number;
	created: Date;
	createdBy: string;
	dateFrom: Date;
	dateTo: Date;
	employees: ITimeScheduleScenarioEmployeeDTO[];
	modified: Date;
	modifiedBy: string;
	name: string;
	sourceDateFrom: Date;
	sourceDateTo: Date;
	sourceType: TermGroup_TimeScheduleScenarioHeadSourceType;
	state: SoeEntityState;
	timeScheduleScenarioHeadId: number;
}
export interface ITimeScheduleShiftQueueDTO {
	date: Date;
	employeeAgeDays: number;
	employeeId: number;
	employeeName: string;
	employmentDays: number;
	sort: number;
	timeScheduleTemplateBlockId: number;
	type: number;
	typeName: string;
}
export interface ITimeScheduleTaskDTO {
	account2Id: number;
	account3Id: number;
	account4Id: number;
	account5Id: number;
	account6Id: number;
	accountId: number;
	accountName: string;
	allowOverlapping: boolean;
	created: Date;
	createdBy: string;
	description: string;
	dontAssignBreakLeftovers: boolean;
	excludedDates: Date[];
	isStaffingNeedsFrequency: boolean;
	length: number;
	minSplitLength: number;
	modified: Date;
	modifiedBy: string;
	name: string;
	nbrOfOccurrences: number;
	nbrOfPersons: number;
	onlyOneEmployee: boolean;
	recurrenceEndsOnDescription: string;
	recurrencePattern: string;
	recurrencePatternDescription: string;
	recurrenceStartsOnDescription: string;
	recurringDates: IDailyRecurrenceDatesOutput;
	shiftTypeId: number;
	startDate: Date;
	startTime: Date;
	state: SoeEntityState;
	stopDate: Date;
	stopTime: Date;
	timeScheduleTaskId: number;
	timeScheduleTaskTypeId: number;
}
export interface ITimeScheduleTaskGeneratedNeedDTO {
	date: Date;
	occurs: string;
	staffingNeedsRowId: number;
	staffingNeedsRowPeriodId: number;
	startTime: Date;
	stopTime: Date;
	type: string;
	weekDay: DayOfWeek;
}
export interface ITimeScheduleTaskGridDTO {
	accountName: string;
	allowOverlapping: boolean;
	description: string;
	dontAssignBreakLeftovers: boolean;
	isStaffingNeedsFrequency: boolean;
	length: number;
	name: string;
	nbrOfPersons: number;
	onlyOneEmployee: boolean;
	recurrenceEndsOnDescription: string;
	recurrencePatternDescription: string;
	recurrenceStartsOnDescription: string;
	shiftTypeId: number;
	shiftTypeName: string;
	startTime: Date;
	state: SoeEntityState;
	stopTime: Date;
	timeScheduleTaskId: number;
	typeId: number;
	typeName: string;
}
export interface ITimeScheduleTaskTypeDTO {
	accountId: number;
	accountName: string;
	created: Date;
	createdBy: string;
	description: string;
	modified: Date;
	modifiedBy: string;
	name: string;
	state: SoeEntityState;
	timeScheduleTaskTypeId: number;
}
export interface ITimeScheduleTaskTypeGridDTO {
	accountId: number;
	accountName: string;
	description: string;
	name: string;
	timeScheduleTaskTypeId: number;
}
export interface ITimeScheduleTemplateBlockDTO {
	accountId: number;
	accountInternals: IAccountDTO[];
	accountName: string;
	actualDate: Date;
	actualStartTime: Date;
	actualStopTime: Date;
	belongsToNextDay: boolean;
	belongsToPreviousDay: boolean;
	break1Id: number;
	break1IsPreliminary: boolean;
	break1Link: System.IGuid;
	break1Minutes: number;
	break1StartTime: Date;
	break1TimeCodeDefaultMinutes: number;
	break1TimeCodeDescription: string;
	break1TimeCodeId: number;
	break1TimeCodeName: string;
	break2Id: number;
	break2IsPreliminary: boolean;
	break2Link: System.IGuid;
	break2Minutes: number;
	break2StartTime: Date;
	break2TimeCodeDefaultMinutes: number;
	break2TimeCodeDescription: string;
	break2TimeCodeId: number;
	break2TimeCodeName: string;
	break3Id: number;
	break3IsPreliminary: boolean;
	break3Link: System.IGuid;
	break3Minutes: number;
	break3StartTime: Date;
	break3TimeCodeDefaultMinutes: number;
	break3TimeCodeDescription: string;
	break3TimeCodeId: number;
	break3TimeCodeName: string;
	break4Id: number;
	break4IsPreliminary: boolean;
	break4Link: System.IGuid;
	break4Minutes: number;
	break4StartTime: Date;
	break4TimeCodeDefaultMinutes: number;
	break4TimeCodeDescription: string;
	break4TimeCodeId: number;
	break4TimeCodeName: string;
	breakType: SoeTimeScheduleTemplateBlockBreakType;
	customerInvoiceId: number;
	date: Date;
	dayName: string;
	dayNumber: number;
	description: string;
	dim2Description: string;
	dim2Id: number;
	dim2Name: string;
	dim2Nr: string;
	dim3Description: string;
	dim3Id: number;
	dim3Name: string;
	dim3Nr: string;
	dim4Description: string;
	dim4Id: number;
	dim4Name: string;
	dim4Nr: string;
	dim5Description: string;
	dim5Id: number;
	dim5Name: string;
	dim5Nr: string;
	dim6Description: string;
	dim6Id: number;
	dim6Name: string;
	dim6Nr: string;
	dimIds: number[];
	employeeChildId: number;
	employeeId: number;
	extraShift: boolean;
	hasAttestedTransactions: boolean;
	hasBreakTimes: boolean;
	holidayName: string;
	isAdded: boolean;
	isBreak: boolean;
	isHoliday: boolean;
	isModified: boolean;
	isPreliminary: boolean;
	length: System.ITimeSpan;
	link: System.IGuid;
	overlapping: boolean;
	overlappingBreaks: boolean;
	plannedTime: number;
	projectId: number;
	recalculateTimeRecordId: number;
	recalculateTimeRecordStatus: TermGroup_RecalculateTimeRecordStatus;
	shiftStatus: TermGroup_TimeScheduleTemplateBlockShiftStatus;
	shiftTypeDescription: string;
	shiftTypeId: number;
	shiftTypeName: string;
	shiftTypeTimeScheduleTypeId: number;
	shiftUserStatus: TermGroup_TimeScheduleTemplateBlockShiftUserStatus;
	staffingNeedsRowId: number;
	staffingNeedsRowPeriodId: number;
	startTime: Date;
	state: SoeEntityState;
	stopTime: Date;
	substituteShift: boolean;
	tasks: ITimeScheduleTemplateBlockTaskDTO[];
	timeCode: ITimeCodeDTO;
	timeCodeId: number;
	timeDeviationCauseId: number;
	timeDeviationCauseStatus: SoeTimeScheduleDeviationCauseStatus;
	timeScheduleEmployeePeriodId: number;
	timeScheduleTemplateBlockId: number;
	timeScheduleTemplatePeriodId: number;
	timeScheduleTypeId: number;
	timeScheduleTypeName: string;
	totalMinutes: number;
	type: TermGroup_TimeScheduleTemplateBlockType;
}
export interface ITimeScheduleTemplateBlockTaskDTO {
	created: Date;
	createdBy: string;
	description: string;
	incomingDeliveryRowId: number;
	isIncomingDeliveryRow: boolean;
	isTimeScheduleTask: boolean;
	modified: Date;
	modifiedBy: string;
	name: string;
	startTime: Date;
	state: SoeEntityState;
	stopTime: Date;
	timeScheduleTaskId: number;
	timeScheduleTemplateBlockId: number;
	timeScheduleTemplateBlockTaskId: number;
}
export interface ITimeScheduleTemplateChangeDTO {
	date: Date;
	dayTypeName: string;
	hasAbsence: boolean;
	hasInvalidDayType: boolean;
	hasManualChanges: boolean;
	hasWarnings: boolean;
	shiftsBeforeUpdate: string;
	warnings: string;
	workRulesResults: IEvaluateWorkRuleResultDTO[];
}
export interface ITimeScheduleTemplateGroupDTO {
	created: Date;
	createdBy: string;
	description: string;
	employees: ITimeScheduleTemplateGroupEmployeeDTO[];
	modified: Date;
	modifiedBy: string;
	name: string;
	rows: ITimeScheduleTemplateGroupRowDTO[];
	state: SoeEntityState;
	templateNames: string;
	timeScheduleTemplateGroupId: number;
}
export interface ITimeScheduleTemplateGroupEmployeeDTO {
	created: Date;
	createdBy: string;
	employeeId: number;
	employeeName: string;
	employeeNr: string;
	fromDate: Date;
	group: ITimeScheduleTemplateGroupDTO;
	modified: Date;
	modifiedBy: string;
	state: SoeEntityState;
	timeScheduleTemplateGroupEmployeeId: number;
	timeScheduleTemplateGroupId: number;
	toDate: Date;
}
export interface ITimeScheduleTemplateGroupGridDTO {
	description: string;
	name: string;
	nbrOfEmployees: number;
	nbrOfRows: number;
	timeScheduleTemplateGroupId: number;
}
export interface ITimeScheduleTemplateGroupRowDTO {
	created: Date;
	createdBy: string;
	modified: Date;
	modifiedBy: string;
	nextStartDate: Date;
	recurrencePattern: string;
	startDate: Date;
	state: SoeEntityState;
	stopDate: Date;
	timeScheduleTemplateGroupId: number;
	timeScheduleTemplateGroupRowId: number;
	timeScheduleTemplateHeadId: number;
}
export interface ITimeScheduleTemplateHeadDTO {
	actorCompanyId: number;
	created: Date;
	createdBy: string;
	description: string;
	employeeId: number;
	employeeName: string;
	employeePostId: number;
	employeeSchedules: IEmployeeScheduleDTO[];
	firstMondayOfCycle: Date;
	flexForceSchedule: boolean;
	lastPlacementStartDate: Date;
	lastPlacementStopDate: Date;
	locked: boolean;
	modified: Date;
	modifiedBy: string;
	name: string;
	noOfDays: number;
	simpleSchedule: boolean;
	startDate: Date;
	startOnFirstDayOfWeek: boolean;
	state: SoeEntityState;
	stopDate: Date;
	timeScheduleTemplateHeadId: number;
	timeScheduleTemplatePeriods: ITimeScheduleTemplatePeriodDTO[];
}
export interface ITimeScheduleTemplateHeadRangeDTO {
	employeeId: number;
	employeeScheduleId: number;
	employeeScheduleStartDate: Date;
	employeeScheduleStopDate: Date;
	firstMondayOfCycle: Date;
	noOfDays: number;
	startDate: Date;
	stopDate: Date;
	templateName: string;
	timeScheduleTemplateGroupId: number;
	timeScheduleTemplateGroupName: string;
	timeScheduleTemplateHeadId: number;
}
export interface ITimeScheduleTemplateHeadSmallDTO {
	accountId: number;
	accountName: string;
	employeeId: number;
	firstMondayOfCycle: Date;
	locked: boolean;
	name: string;
	noOfDays: number;
	simpleSchedule: boolean;
	startDate: Date;
	stopDate: Date;
	timeScheduleTemplateGroupId: number;
	timeScheduleTemplateGroupName: string;
	timeScheduleTemplateHeadId: number;
	virtualStopDate: Date;
}
export interface ITimeScheduleTemplateHeadsRangeDTO {
	heads: ITimeScheduleTemplateHeadRangeDTO[];
}
export interface ITimeScheduleTemplatePeriodDTO {
	created: Date;
	createdBy: string;
	date: Date;
	dayName: string;
	dayNumber: number;
	hasAttestedTransactions: boolean;
	holidayName: string;
	isHoliday: boolean;
	modified: Date;
	modifiedBy: string;
	state: SoeEntityState;
	timeScheduleTemplateBlocks: ITimeScheduleTemplateBlockDTO[];
	timeScheduleTemplateHeadId: number;
	timeScheduleTemplatePeriodId: number;
}
export interface ITimeScheduleTemplatePeriodSmallDTO {
	dayNumber: number;
	timeScheduleTemplateHeadId: number;
	timeScheduleTemplatePeriodId: number;
}
export interface ITimeScheduleTypeDTO {
	actorCompanyId: number;
	code: string;
	created: Date;
	createdBy: string;
	description: string;
	factors: ITimeScheduleTypeFactorDTO[];
	isActive: boolean;
	isAll: boolean;
	isBilagaJ: boolean;
	isNotScheduleTime: boolean;
	modified: Date;
	modifiedBy: string;
	name: string;
	state: SoeEntityState;
	timeDeviationCauseId: number;
	timeDeviationCauseName: string;
	timeScheduleTypeId: number;
	useScheduleTimeFactor: boolean;
	ignoreIfExtraShift: boolean;
}
export interface ITimeScheduleTypeFactorDTO {
	created: Date;
	createdBy: string;
	factor: number;
	fromTime: Date;
	modified: Date;
	modifiedBy: string;
	state: SoeEntityState;
	timeScheduleTypeFactorId: number;
	timeScheduleTypeId: number;
	toTime: Date;
}
export interface ITimeScheduleTypeFactorSmallDTO {
	factor: number;
	fromTime: Date;
	toTime: Date;
}
export interface ITimeScheduleTypeSmallDTO {
	code: string;
	factors: ITimeScheduleTypeFactorSmallDTO[];
	name: string;
	timeScheduleTypeId: number;
}
export interface ITimeSheetRowDTO {
	allocationType: TermGroup_ProjectAllocationType;
	attestStateColor: string;
	attestStateId: number;
	attestStateName: string;
	customerId: number;
	customerName: string;
	fridayInvoiceQuantity: number;
	fridayNoteExternal: string;
	fridayNoteInternal: string;
	fridayQuantity: number;
	invoiceId: number;
	invoiceNr: string;
	mondayInvoiceQuantity: number;
	mondayNoteExternal: string;
	mondayNoteInternal: string;
	mondayQuantity: number;
	projectId: number;
	projectInvoiceWeekId: number;
	projectName: string;
	projectNr: string;
	projectNumberName: string;
	registrationType: OrderInvoiceRegistrationType;
	rowNr: number;
	saturdayInvoiceQuantity: number;
	saturdayNoteExternal: string;
	saturdayNoteInternal: string;
	saturdayQuantity: number;
	sundayInvoiceQuantity: number;
	sundayNoteExternal: string;
	sundayNoteInternal: string;
	sundayQuantity: number;
	thursdayInvoiceQuantity: number;
	thursdayNoteExternal: string;
	thursdayNoteInternal: string;
	thursdayQuantity: number;
	timeCodeId: number;
	timeCodeName: string;
	timeSheetWeekId: number;
	tuesdayInvoiceQuantity: number;
	tuesdayNoteExternal: string;
	tuesdayNoteInternal: string;
	tuesdayQuantity: number;
	wednesdayInvoiceQuantity: number;
	wednesdayNoteExternal: string;
	wednesdayNoteInternal: string;
	wednesdayQuantity: number;
	weekNoteExternal: string;
	weekNoteInternal: string;
	weekStartDate: Date;
	weekSumInvoiceQuantity: number;
	weekSumQuantity: number;
}
export interface ITimeStampAdditionDTO {
	fixedQuantity: number;
	id: number;
	name: string;
	type: TimeStampAdditionType;
}
export interface ITimeStampAttendanceGaugeDTO {
	accountName: string;
	employeeId: number;
	employeeNr: string;
	isBreak: boolean;
	isDistanceWork: boolean;
	isMissing: boolean;
	isPaidBreak: boolean;
	name: string;
	scheduleStartTime: Date;
	time: Date;
	timeDeviationCauseName: string;
	timeStr: string;
	timeTerminalName: string;
	type: TimeStampEntryType;
	typeName: string;
}
export interface ITimeStampEntryDTO {
	accountId: number;
	accountName: string;
	accountNr: string;
	actorCompanyId: number;
	adjustedTime: Date;
	adjustedTimeBlockDateDate: Date;
	created: Date;
	createdBy: string;
	date: Date;
	employeeChildId: number;
	employeeId: number;
	employeeManuallyAdjusted: boolean;
	employeeName: string;
	employeeNr: string;
	extended: ITimeStampEntryExtendedDTO[];
	isBreak: boolean;
	isDistanceWork: boolean;
	isModified: boolean;
	isPaidBreak: boolean;
	manuallyAdjusted: boolean;
	modified: Date;
	modifiedBy: string;
	note: string;
	originalTime: Date;
	originType: TermGroup_TimeStampEntryOriginType;
	shiftTypeId: number;
	state: SoeEntityState;
	status: TermGroup_TimeStampEntryStatus;
	terminalStampData: string;
	time: Date;
	timeBlockDateDate: Date;
	timeBlockDateId: number;
	timeDeviationCauseId: number;
	timeDeviationCauseName: string;
	timeScheduleTemplatePeriodId: number;
	timeScheduleTypeId: number;
	timeScheduleTypeName: string;
	timeStampEntryId: number;
	timeTerminalAccountId: number;
	timeTerminalId: number;
	timeTerminalName: string;
	type: TimeStampEntryType;
	typeName: string;
}
export interface ITimeStampEntryExtendedDTO {
	accountId: number;
	created: Date;
	createdBy: string;
	modified: Date;
	modifiedBy: string;
	quantity: number;
	state: SoeEntityState;
	timeCodeId: number;
	timeScheduleTypeId: number;
	timeStampEntryExtendedId: number;
	timeStampEntryId: number;
}
export interface ITimeTerminalDTO {
	actorCompanyId: number;
	categoryIds: number[];
	companyApiKey: string;
	companyName: string;
	created: Date;
	createdBy: string;
	lastSync: Date;
	macAddress: string;
	macName: string;
	macNumber: number;
	modified: Date;
	modifiedBy: string;
	name: string;
	registered: boolean;
	state: SoeEntityState;
	sysCompDBId: number;
	terminalDbSchemaVersion: number;
	terminalVersion: string;
	timeTerminalGuid: System.IGuid;
	timeTerminalId: number;
	timeTerminalSettings: ITimeTerminalSettingDTO[];
	type: TimeTerminalType;
	typeName: string;
	uri: string;
}
export interface ITimeTerminalSettingDTO extends SoftOne.Soe.Common.DTO.ISettingsDTO {
	children: ITimeTerminalSettingDTO[];
	parentId: number;
	timeTerminalSettingId: number;
}
export interface ITimeUnhandledShiftChangesEmployeeDTO {
	employeeId: number;
	hasDays: boolean;
	hasExtraShiftDays: boolean;
	hasShiftDays: boolean;
	weeks: ITimeUnhandledShiftChangesWeekDTO[];
}
export interface ITimeUnhandledShiftChangesWeekDTO {
	dateFrom: Date;
	dateTo: Date;
	extraShiftDays: ITimeBlockDateDTO[];
	hasDays: boolean;
	hasExtraShiftDays: boolean;
	hasShiftDays: boolean;
	shiftDays: ITimeBlockDateDTO[];
	weekNr: number;
}
export interface ITimeWorkAccountDTO {
	actorCompanyId: number;
	code: string;
	created: Date;
	createdBy: string;
	defaultPaidLeaveNotUsed: TermGroup_TimeWorkAccountWithdrawalMethod;
	defaultWithdrawalMethod: TermGroup_TimeWorkAccountWithdrawalMethod;
	modified: Date;
	modifiedBy: string;
	name: string;
	state: SoeEntityState;
	timeWorkAccountId: number;
	timeWorkAccountYears: ITimeWorkAccountYearDTO[];
	useDirectPayment: boolean;
	usePaidLeave: boolean;
	usePensionDeposit: boolean;
}
export interface ITimeWorkAccountWorkTimeWeekDTO {
	created: Date;
	createdBy: string;
	modified: Date;
	modifiedBy: string;
	paidLeaveTime: number;
	state: SoeEntityState;
	timeWorkAccountWorkTimeWeekId: number;
	timeWorkAccountYearId: number;
	workTimeWeekFrom: number;
	workTimeWeekTo: number;
}
export interface ITimeWorkAccountYearDTO {
	created: Date;
	createdBy: string;
	directPaymentLastDate: Date;
	directPaymentPercent: number;
	earningStart: Date;
	earningStop: Date;
	employeeLastDecidedDate: Date;
	modified: Date;
	modifiedBy: string;
	paidAbsenceStopDate: Date;
	paidLeavePercent: number;
	pensionDepositPercent: number;
	state: SoeEntityState;
	timeWorkAccountId: number;
	timeWorkAccountWorkTimeWeeks: ITimeWorkAccountWorkTimeWeekDTO[];
	timeWorkAccountYearEmployees: ITimeWorkAccountYearEmployeeDTO[];
	timeWorkAccountYearId: number;
	withdrawalStart: Date;
	withdrawalStop: Date;
}
export interface ITimeWorkAccountYearEmployeeDTO {
	calculatedDirectPaymentAmount: number;
	calculatedPaidLeaveAmount: number;
	calculatedPaidLeaveMinutes: number;
	calculatedPensionDepositAmount: number;
	calculatedWorkingTimePromoted: number;
	created: Date;
	createdBy: string;
	earningStart: Date;
	earningStop: Date;
	employeeId: number;
	employeeName: string;
	employeeNumber: string;
	modified: Date;
	modifiedBy: string;
	selectedDate: Date;
	selectedWithdrawalMethod: TermGroup_TimeWorkAccountWithdrawalMethod;
	state: SoeEntityState;
	status: TermGroup_TimeWorkAccountYearEmployeeStatus;
	timeWorkAccountId: number;
	timeWorkAccountYearEmployeeId: number;
}
export interface ITrackChangesDTO {
	action: TermGroup_TrackChangesAction;
	actionMethod: TermGroup_TrackChangesActionMethod;
	actorCompanyId: number;
	batch: System.IGuid;
	columnName: string;
	columnType: TermGroup_TrackChangesColumnType;
	created: Date;
	createdBy: string;
	dataType: SettingDataType;
	entity: SoeEntityType;
	fromValue: string;
	fromValueName: string;
	parentEntity: SoeEntityType;
	parentRecordId: number;
	recordId: number;
	role: string;
	topEntity: SoeEntityType;
	topRecordId: number;
	toValue: string;
	toValueName: string;
	trackChangesId: number;
}
export interface ITrackChangesLogDTO {
	actionMethodText: string;
	actionText: string;
	batch: System.IGuid;
	batchNbr: number;
	columnText: string;
	created: Date;
	createdBy: string;
	entity: SoeEntityType;
	entityText: string;
	fromValueText: string;
	recordId: number;
	recordName: string;
	role: string;
	topRecordName: string;
	toValueText: string;
	trackChangesId: number;
}
export interface IUpdateEdiEntryDTO {
	attestGroupId: number;
	ediEntryId: number;
	orderNr: string;
	scanningEntryId: number;
	supplierId: number;
}
export interface IUserAgentClientInfoDTO {
	data: string;
	deviceBrand: string;
	deviceFamily: string;
	deviceModel: string;
	osFamily: string;
	osVersion: string;
	userAgentFamily: string;
	userAgentVersion: string;
}
export interface IUserAttestRoleDTO {
	accountDimId: number;
	accountDimName: string;
	accountId: number;
	accountName: string;
	accountPermissionType: TermGroup_AttestRoleUserAccountPermissionType;
	accountPermissionTypeName: string;
	attestRoleId: number;
	attestRoleUserId: number;
	children: IUserAttestRoleDTO[];
	dateFrom: Date;
	dateTo: Date;
	isDelegated: boolean;
	isExecutive: boolean;
	isModified: boolean;
	isNearestManager: boolean;
	maxAmount: number;
	moduleName: string;
	name: string;
	parentAttestRoleUserId: number;
	prevAccountId: number;
	userId: number;
	roleId: number;
	state: SoeEntityState;
}
export interface IUserCompanyRoleDelegateHistoryGridDTO {
	attestRoleNames: string;
	byUserId: number;
	byUserName: string;
	created: Date;
	dateFrom: Date;
	dateTo: Date;
	fromUserId: number;
	fromUserName: string;
	roleNames: string;
	showDelete: boolean;
	state: SoeEntityState;
	toUserId: number;
	toUserName: string;
	userCompanyRoleDelegateHistoryHeadId: number;
}
export interface IUserCompanyRoleDelegateHistoryHeadDTO {
	actorCompanyId: number;
	byUserId: number;
	created: Date;
	createdBy: string;
	fromUserId: number;
	modified: Date;
	modifiedBy: string;
	rows: IUserCompanyRoleDelegateHistoryRowDTO[];
	state: SoeEntityState;
	toUserId: number;
	userCompanyRoleDelegateHistoryHeadId: number;
}
export interface IUserCompanyRoleDelegateHistoryRowDTO {
	accountId: number;
	attestRoleId: number;
	created: Date;
	createdBy: string;
	dateFrom: Date;
	dateTo: Date;
	modified: Date;
	modifiedBy: string;
	parentId: number;
	roleId: number;
	state: SoeEntityState;
	userCompanyRoleDelegateHistoryHeadId: number;
	userCompanyRoleDelegateHistoryRowId: number;
}
export interface IUserCompanyRoleDelegateHistoryUserDTO {
	loginName: string;
	name: string;
	possibleTargetAttestRoles: IUserAttestRoleDTO[];
	possibleTargetRoles: IUserCompanyRoleDTO[];
	targetAttestRoles: IUserAttestRoleDTO[];
	targetRoles: IUserCompanyRoleDTO[];
	userId: number;
}
export interface IUserCompanyRoleDTO {
	actorCompanyId: number;
	dateFrom: Date;
	dateTo: Date;
	default: boolean;
	isDelegated: boolean;
	isModified: boolean;
	name: string;
	roleId: number;
	state: SoeEntityState;
	userCompanyRoleId: number;
	userId: number;
}
export interface IUserCompanySettingEditDTO {
	booleanValue: boolean;
	dataType: SettingDataType;
	dateValue: Date;
	decimalValue: number;
	groupLevel1: string;
	groupLevel2: string;
	groupLevel3: string;
	integerValue: number;
	isModified: boolean;
	name: string;
	options: ISmallGenericType[];
	settingMainType: SettingMainType;
	settingTypeId: number;
	stringValue: string;
	userCompanySettingId: number;
	visibleOnlyForSupportAdmin: boolean;
}
export interface IUserDataSelectionDTO extends IReportDataSelectionDTO {
	ids: number[];
	includeInactive: boolean;
}
export interface IUserDTO {
	blockedFromDate: Date;
	changePassword: boolean;
	defaultActorCompanyId: number;
	email: string;
	emailCopy: boolean;
	estatusLoginId: string;
	hasUserVerifiedEmail: boolean;
	idLoginGuid: System.IGuid;
	isAdmin: boolean;
	isMobileUser: boolean;
	isSuperAdmin: boolean;
	langId: number;
	licenseId: number;
	licenseNr: string;
	loginName: string;
	name: string;
	state: SoeEntityState;
	userId: number;
}
export interface IUserGaugeDTO {
	actorCompanyId: number;
	module: SoeModule;
	roleId: number;
	sort: number;
	sysGaugeId: number;
	sysGaugeName: string;
	userGaugeHeadId: number;
	userGaugeId: number;
	userGaugeSettings: IUserGaugeSettingDTO[];
	userId: number;
	windowState: number;
}
export interface IUserGaugeHeadDTO {
	actorCompanyId: number;
	created: Date;
	createdBy: string;
	description: string;
	modified: Date;
	modifiedBy: string;
	module: SoeModule;
	name: string;
	priority: number;
	userGaugeHeadId: number;
	userGauges: IUserGaugeDTO[];
	userId: number;
}
export interface IUserGaugeSettingDTO {
	boolData: boolean;
	dataType: number;
	dateData: Date;
	decimalData: number;
	intData: number;
	name: string;
	strData: string;
	timeData: Date;
	userGaugeId: number;
	userGaugeSettingId: number;
}
export interface IUserGridDTO {
	defaultActorCompanyId: number;
	defaultRoleName: string;
	email: string;
	externalAuthId: string;
	softOneIdLoginName: string;
	idLoginActive: boolean;
	loginName: string;
	name: string;
	state: SoeEntityState;
	userId: number;
}
export interface IUserReplacementDTO {
	actorCompanyId: number;
	originUserId: number;
	replacementUserId: number;
	startDate: Date;
	state: SoeEntityState;
	stopDate: Date;
	type: UserReplacementType;
	userReplacementId: number;
}
export interface IUserRolesDTO {
	actorCompanyId: number;
	attestRoles: IUserAttestRoleDTO[];
	companyName: string;
	defaultCompany: boolean;
	roles: IUserCompanyRoleDTO[];
}
export interface IUserSelectionAccessDTO {
	created: Date;
	createdBy: string;
	messageGroupId: number;
	modified: Date;
	modifiedBy: string;
	roleId: number;
	state: SoeEntityState;
	type: TermGroup_ReportUserSelectionAccessType;
	userSelectionAccessId: number;
	userSelectionId: number;
}
export interface IUserSelectionDTO {
	access: IUserSelectionAccessDTO[];
	actorCompanyId: number;
	description: string;
	name: string;
	selections: IReportDataSelectionDTO[];
	state: SoeEntityState;
	type: UserSelectionType;
	userId: number;
	userSelectionId: number;
}
export interface IUserSmallDTO {
	allowSupportLogin: boolean;
	attestFlowHasAnswered: boolean;
	attestFlowIsRequired: boolean;
	attestFlowRowId: number;
	attestRoleId: number;
	blockedFromDate: Date;
	categories: string;
	changePassword: boolean;
	defaultActorCompanyId: number;
	defaultRoleName: string;
	email: string;
	hideEditButton: boolean;
	idLoginActive: boolean;
	isSelected: boolean;
	isSelectedForEmail: boolean;
	langId: number;
	licenseId: number;
	licenseNr: string;
	loginName: string;
	main: boolean;
	name: string;
	state: SoeEntityState;
	userId: number;
}
export interface IVacationGroupDTO {
	actorCompanyId: number;
	created: Date;
	createdBy: string;
	externalCodes: string[];
	externalCodesString: string;
	fromDate: Date;
	latesVacationYearEnd: Date;
	modified: Date;
	modifiedBy: string;
	name: string;
	realDateFrom: Date;
	realDateTo: Date;
	state: SoeEntityState;
	type: TermGroup_VacationGroupType;
	typeName: string;
	vacationDaysPaidByLaw: number;
	vacationGroupId: number;
	vacationGroupSE: IVacationGroupSEDTO;
}
export interface IVacationGroupSEDTO {
	additionalVacationDays1: number;
	additionalVacationDays2: number;
	additionalVacationDays3: number;
	additionalVacationDaysFromAge1: number;
	additionalVacationDaysFromAge2: number;
	additionalVacationDaysFromAge3: number;
	calculationType: TermGroup_VacationGroupCalculationType;
	created: Date;
	createdBy: string;
	earningYearAmountFromDate: Date;
	earningYearVariableAmountFromDate: Date;
	employmentTaxAccountInternalOnCredit: boolean;
	employmentTaxAccountInternalOnDebit: boolean;
	employmentTaxCredidAccountId: number;
	employmentTaxDebitAccountId: number;
	guaranteeAmountAccordingToHandels: boolean;
	guaranteeAmountEmployedNbrOfYears: number;
	guaranteeAmountJuvenile: boolean;
	guaranteeAmountJuvenileAgeLimit: number;
	guaranteeAmountJuvenilePerDayPriceTypeId: number;
	guaranteeAmountMaxNbrOfDaysRule: TermGroup_VacationGroupGuaranteeAmountMaxNbrOfDaysRule;
	guaranteeAmountPerDayPriceTypeId: number;
	hourlySalaryFormulaId: number;
	maxRemainingDays: number;
	modified: Date;
	modifiedBy: string;
	monthlySalaryFormulaId: number;
	nbrOfAdditionalVacationDays: number;
	ownGuaranteeAmount: number;
	remainingDaysPayoutMonth: number;
	remainingDaysRule: TermGroup_VacationGroupRemainingDaysRule;
	replacementTimeDeviationCauseId: number;
	showHours: boolean;
	supplementChargeAccountInternalOnCredit: boolean;
	supplementChargeAccountInternalOnDebit: boolean;
	supplementChargeCreditAccountId: number;
	supplementChargeDebitAccountId: number;
	useAdditionalVacationDays: boolean;
	useEmploymentTaxAcccount: boolean;
	useFillUpToVacationDaysPaidByLawRule: boolean;
	useGuaranteeAmount: boolean;
	useMaxRemainingDays: boolean;
	useOwnGuaranteeAmount: boolean;
	useSupplementChargeAccount: boolean;
	vacationAbsenceCalculationRule: TermGroup_VacationGroupVacationAbsenceCalculationRule;
	vacationDayAdditionPercent: number;
	vacationDayAdditionPercentPriceTypeId: number;
	vacationDayPercent: number;
	vacationDayPercentPriceTypeId: number;
	vacationDaysGrossUseFiveDaysPerWeek: boolean;
	vacationDaysHandleRule: TermGroup_VacationGroupVacationDaysHandleRule;
	vacationGroupId: number;
	vacationGroupSEId: number;
	vacationGroupSEDayTypes: IVacationGroupSEDayTypeDTO[];
	vacationHandleRule: TermGroup_VacationGroupVacationHandleRule;
	vacationSalaryPayoutDays: number;
	vacationSalaryPayoutMonth: number;
	vacationSalaryPayoutRule: TermGroup_VacationGroupVacationSalaryPayoutRule;
	vacationVariablePayoutDays: number;
	vacationVariablePayoutMonth: number;
	vacationVariablePayoutRule: TermGroup_VacationGroupVacationSalaryPayoutRule;
	vacationVariablePercent: number;
	vacationVariablePercentPriceTypeId: number;
	valueDaysAccountInternalOnCredit: boolean;
	valueDaysAccountInternalOnDebit: boolean;
	valueDaysCreditAccountId: number;
	valueDaysDebitAccountId: number;
	yearEndOverdueDaysRule: TermGroup_VacationGroupYearEndOverdueDaysRule;
	yearEndRemainingDaysRule: TermGroup_VacationGroupYearEndRemainingDaysRule;
	yearEndVacationVariableRule: TermGroup_VacationGroupYearEndVacationVariableRule;
}
export interface IVacationGroupSEDayTypeDTO {
	vacationGroupSEDayTypeId: number;
	dayTypeId: number;
	vacationGroupSEId: number;
	type: SoeVacationGroupDayType;
}
export interface IVacationYearEndResultDTO {
	result: IActionResult;
	employeeResults: IVacationYearEndEmployeeResultDTO[];
}
export interface IVacationYearEndEmployeeResultDTO {
	employeeId: number;
	employeeNrAndName: string;
	vacationGroupName: string;
	message: string;
	status: TermGroup_VacationYearEndStatus;
	statusName: string;
}
export interface IValidateDeviationChangeResult {
	applyAbsenceItems: IApplyAbsenceDTO[];
	generatedTimeBlocks: IAttestEmployeeDayTimeBlockDTO[];
	generatedTimeCodeTransactions: IAttestEmployeeDayTimeCodeTransactionDTO[];
	generatedTimePayrollTransactions: IAttestPayrollTransactionDTO[];
	message: string;
	resultCode: SoeValidateDeviationChangeResultCode;
	success: boolean;
	timeDeviationCauses: ITimeDeviationCauseGridDTO[];
}
export interface IValidatePossibleDeleteOfEmployeeAccountDTO {
	employeeId: number;
	rows: IValidatePossibleDeleteOfEmployeeAccountRowDTO[];
}
export interface IValidatePossibleDeleteOfEmployeeAccountRowDTO {
	dateFrom: Date;
	dateTo: Date;
	employeeAccountId: number;
}
export interface IVatCodeDTO {
	accountId: number;
	accountNr: string;
	actorCompanyId: number;
	code: string;
	created: Date;
	createdBy: string;
	modified: Date;
	modifiedBy: string;
	name: string;
	percent: number;
	purchaseVATAccountId: number;
	purchaseVATAccountNr: string;
	state: SoeEntityState;
	vatCodeId: number;
}
export interface IVoucherHeadDTO {
	accountIds: number[];
	accountIdsHandled: boolean;
	accountPeriodId: number;
	accountYearId: number;
	actorCompanyId: number;
	budgetAccountId: number;
	companyGroupVoucher: boolean;
	created: Date;
	createdBy: string;
	date: Date;
	isSelected: boolean;
	modified: Date;
	modifiedBy: string;
	note: string;
	rows: IVoucherRowDTO[];
	sourceType: TermGroup_VoucherHeadSourceType;
	sourceTypeName: string;
	status: TermGroup_AccountStatus;
	template: boolean;
	text: string;
	typeBalance: boolean;
	vatVoucher: boolean;
	voucherHeadId: number;
	voucherNr: number;
	voucherSeriesId: number;
	voucherSeriesTypeId: number;
	voucherSeriesTypeName: string;
	voucherSeriesTypeNr: number;
}
export interface IVoucherRowDTO {
	accountDistributionHeadId: number;
	accountInternalDTO_forReports: IAccountInternalDTO[];
	amount: number;
	amountEntCurrency: number;
	date: Date;
	dim1AccountType: number;
	dim1AmountStop: number;
	dim1Id: number;
	dim1Name: string;
	dim1Nr: string;
	dim1UnitStop: boolean;
	dim2Id: number;
	dim2Name: string;
	dim2Nr: string;
	dim3Id: number;
	dim3Name: string;
	dim3Nr: string;
	dim4Id: number;
	dim4Name: string;
	dim4Nr: string;
	dim5Id: number;
	dim5Name: string;
	dim5Nr: string;
	dim6Id: number;
	dim6Name: string;
	dim6Nr: string;
	merged: boolean;
	parentRowId: number;
	quantity: number;
	rowNr: number;
	state: SoeEntityState;
	sysVatAccountId: number;
	tempRowId: number;
	text: string;
	voucherHeadId: number;
	voucherNr: number;
	voucherRowId: number;
	voucherSeriesTypeName: string;
	voucherSeriesTypeNr: number;
}
export interface IVoucherSeriesDTO {
	accountYearId: number;
	created: Date;
	createdBy: string;
	isDeleted: boolean;
	isModified: boolean;
	modified: Date;
	modifiedBy: string;
	status: TermGroup_AccountStatus;
	voucherDateLatest: Date;
	voucherNrLatest: number;
	voucherSeriesId: number;
	voucherSeriesTypeId: number;
	voucherSeriesTypeIsTemplate: boolean;
	voucherSeriesTypeName: string;
	voucherSeriesTypeNr: number;
}
export interface IVoucherTraceViewDTO {
	accountDistributionHeadId: number;
	accountDistributionName: string;
	amount: number;
	amountCurrency: number;
	currencyCode: string;
	currencyRate: number;
	date: Date;
	description: string;
	foreign: boolean;
	inventoryDescription: string;
	inventoryId: number;
	inventoryName: string;
	inventoryStatusId: TermGroup_InventoryStatus;
	inventoryStatusName: string;
	inventoryTypeName: string;
	invoiceId: number;
	isAccountDistribution: boolean;
	isInventory: boolean;
	isInvoice: boolean;
	isPayment: boolean;
	langId: number;
	number: string;
	originStatus: SoeOriginStatus;
	originStatusName: string;
	originType: SoeOriginType;
	originTypeName: string;
	paymentRowId: number;
	paymentStatus: SoePaymentStatus;
	paymentStatusName: string;
	registrationType: OrderInvoiceRegistrationType;
	state: SoeEntityState;
	sysCurrencyId: number;
	vatAmount: number;
	vatAmountCurrency: number;
	voucherHeadId: number;
}
export interface IWantedShiftsGaugeDTO {
	date: Date;
	employee: string;
	employeeId: number;
	employeesInQueue: string;
	link: string;
	openType: number;
	openTypeName: string;
	shiftTypeId: number;
	shiftTypeName: string;
	time: string;
	timeScheduleTemplateBlockId: number;
}
export interface IYearAndPeriodSelectionDTO extends IReportDataSelectionDTO {
	from: string;
	id: number;
	rangeType: string;
	to: string;
}
declare namespace Soe.Sys.Common.DTO {
	export interface ISysCompanySettingDTO {
		boolvalue: boolean;
		childSysCompanySettingDTOs: Soe.Sys.Common.DTO.ISysCompanySettingDTO[];
		decimalValue: number;
		intValue: number;
		settingType: SysCompanySettingType;
		stringValue: string;
		sysCompanyId: number;
		sysCompanySettingId: number;
	}
}
declare namespace SoftOne.Soe.Common.DTO {
	export interface ICustomerProductPriceIODTO {
		price: number;
		productId: number;
	}
	export interface IPaymentRowSaveBaseDTO {
		actorId: number;
		currencyRate: number;
		invoiceDate: Date;
		originId: number;
		paymentDate: Date;
		voucherDate: Date;
	}
	export interface IPayrollGroupBaseDTO {
		actorCompanyId: number;
		name: string;
		payrollGroupId: number;
		state: SoeEntityState;
		timePeriodHeadId: number;
	}
	export interface ISettingsDTO {
		boolData: boolean;
		created: Date;
		createdBy: string;
		dateData: Date;
		decimalData: number;
		id: number;
		intData: number;
		modified: Date;
		modifiedBy: string;
		name: string;
		state: SoeEntityState;
		strData: string;
		timeData: Date;
		timeTerminalId: number;
	}
	export interface ITextblockDTOBase {
		isModified: boolean;
		text: string;
		textblockId: number;
	}
	export interface ITimeHibernatingAbsenceRowDTO {
		absenceTimeMinutes: number;
		actorCompanyId: number;
		created: Date;
		createdBy: string;
		date: Date;
		employeeId: number;
		employmeeChildId: number;
		modified: Date;
		modifiedBy: string;
		scheduleTimeMinutes: number;
		state: SoeEntityState;
		timeHibernatingAbsenceHead: ITimeHibernatingAbsenceHeadDTO;
		timeHibernatingAbsenceHeadId: number;
		timeHibernatingAbsenceRowId: number;
	}
	export interface IWholesellerDTO {
		name: string;
		sysWholesellerId: number;
	}
}
declare namespace System {
	export interface IGuid {
	}
	export interface ITimeSpan {
		days: number;
		hours: number;
		milliseconds: number;
		minutes: number;
		seconds: number;
		ticks: number;
		totalDays: number;
		totalHours: number;
		totalMilliseconds: number;
		totalMinutes: number;
		totalSeconds: number;
	}
}
declare namespace System.Collections.Generic {
	export interface IKeyValuePair {
		key: ITKey;
		value: ITValue;
	}
}


