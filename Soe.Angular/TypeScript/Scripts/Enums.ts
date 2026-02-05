module Soe.Edi.Common.Enumerations {
	export const enum SysEdiMessageHeadStatus {
		Unknown = 0,
		Unhandled = 1,
		Handled = 2,
		NoSysCompanyFound = 5,
		SentToComp = 10,
		Error = 99
	}
}
module Soe.Sys.Common.Enumerations {
	export const enum SoeSysEntityState {
		Active = 0,
		Inactive = 1,
		Deleted = 2,
		Temporary = 3
	}
	export const enum SysCompanySettingType {
		WholesellerCustomerNumber = 1,
		OrganisationNumber = 2,
		SysEdiMessageTypeAndNumber = 3,
		ExternalFtp = 4,
		UserName = 10,
		Password = 11
	}
	export const enum SysCompDBType {
		Unknown = 0,
		Production = 1,
		Demo = 2,
		Test = 10
	}
}
module SoftOne.Soe.Common.DTO {
	export const enum DailyRecurrencePatternType {
		None = 0,
		Daily = 1,
		Weekly = 2,
		AbsoluteMonthly = 3,
		RelativeMonthly = 4,
		AbsoluteYearly = 5,
		RelativeYearly = 6,
		SysHoliday = 7
	}
	export const enum DailyRecurrencePatternWeekIndex {
		First = 0,
		Second = 1,
		Third = 2,
		Fourth = 3,
		Last = 4
	}
	export const enum DailyRecurrenceRangeType {
		EndDate = 0,
		NoEnd = 1,
		Numbered = 2
	}
}
module SoftOne.Soe.Common.Util {
	export const enum AccountingRowType {
		AccountingRow = 0,
		SupplierInvoiceAttestRow = 1
	}
	export const enum CompTermsRecordType {
		Unknown = 0,
		ProductName = 1,
		ProductUnitName = 2,
		ReportName = 3,
		AccountName = 4,
		Textblock = 5,
		AccountDimShortName = 6,
		AccountDimName = 7
	}
	export const enum ContactAddressItemType {
		AddressDistribution = 1,
		AddressVisiting = 2,
		AddressBilling = 3,
		AddressDelivery = 4,
		AddressBoardHQ = 5,
		EComEmail = 11,
		EComPhoneHome = 12,
		EComPhoneJob = 13,
		EComPhoneMobile = 14,
		EComFax = 15,
		EComWeb = 16,
		ClosestRelative = 17,
		EcomCompanyAdminEmail = 18,
		Coordinates = 19,
		IndividualTaxNumber = 20,
		GlnNumber = 21
	}
	export const enum ImageFormatType {
		NONE = 0,
		JPG = 1,
		PNG = 2,
		PDF = 3
	}
	export const enum ImportPaymentIOState {
		Open = 0,
		Closed = 1
	}
	export const enum ImportPaymentIOStatus {
		None = 0,
		Match = 1,
		Rest = 2,
		PartlyPaid = 3,
		Unknown = 4,
		FullyPaid = 5,
		Paid = 6,
		Error = 7
	}
	export const enum ImportPaymentType {
		None = 0,
		CustomerPayment = 1,
		SupplierPayment = 2,
		Sepa = 3,
		Professional = 4
	}
	export const enum OrderInvoiceRegistrationType {
		Unknown = 0,
		Invoice = 1,
		Ledger = 2,
		Offer = 3,
		Order = 4,
		Contract = 5
	}
	export const enum PostChange {
		Unknown = 0,
		Update = 1,
		Insert = 2,
		Delete = 3
	}
	export const enum PriceListOrigin {
		Unknown = 0,
		SysDbPriceList = 1,
		CompDbPriceList = 2,
		SysAndCompDbPriceList = 3
	}
	export const enum ProjectCentralBudgetRowType {
		Separator = 0,
		BillableMinutesInvoiced = 1,
		BillableMinutesNotInvoiced = 2,
		BillableMinutesTotal = 3,
		IncomePersonellInvoiced = 4,
		IncomeMaterialInvoiced = 5,
		IncomeInvoiced = 6,
		IncomePersonellNotInvoiced = 7,
		IncomeMaterialNotInvoiced = 8,
		IncomeNotInvoiced = 9,
		IncomePersonellTotal = 10,
		IncomeMaterialTotal = 11,
		IncomeTotal = 12,
		CostPersonell = 13,
		CostMaterial = 14,
		CostExpense = 15,
		CostTotal = 16,
		NetTotal = 17,
		ResultInvoiced = 18,
		ResultTotal = 19,
		OverheadCost = 20,
		OverheadCostPerHour = 21,
		FixedPriceTotal = 22,
		BillableMinutesInvoicedIB = 30,
		BillableMinutesNotInvoicedIB = 31,
		BillableMinutesTotalIB = 32,
		IncomePersonellTotalIB = 33,
		IncomeMaterialTotalIB = 34,
		IncomeTotalIB = 35,
		CostPersonellIB = 36,
		CostMaterialIB = 37,
		CostExpenseIB = 38,
		OverheadCostPerHourIB = 39,
		OverheadCostIB = 40,
		CostTotalIB = 41
	}
	export const enum ProjectCentralHeaderGroupType {
		None = 0,
		Time = 1,
		IncomeNotInvoiced = 2,
		IncomeInvoiced = 3,
		CostsMaterial = 4,
		CostsPersonell = 5,
		CostsExpense = 6,
		CostsOverhead = 7
	}
	export const enum ProjectCentralStatusRowType {
		None = 0,
		TimeValueRow = 1,
		TimeSummaryRow = 2,
		AmountValueRow = 3,
		AmountSummaryRow = 4,
		SeparatorRow = 5
	}
	export const enum ScanningEntryRowType {
		Unknown = 0,
		IsCreditInvoice = 1,
		InvoiceNr = 2,
		InvoiceDate = 3,
		DueDate = 4,
		OrderNr = 5,
		ReferenceYour = 6,
		ReferenceOur = 7,
		TotalAmountExludeVat = 8,
		VatAmount = 9,
		TotalAmountIncludeVat = 10,
		CurrencyCode = 11,
		OCR = 12,
		Plusgiro = 13,
		Bankgiro = 14,
		OrgNr = 15,
		IBAN = 16,
		VatRate = 17,
		VatNr = 18,
		FreightAmount = 19,
		CentRounding = 20,
		VatRegNumberFin = 21,
		SupplierBankCodeNumber1 = 22,
		BankNr = 23
	}
	export const enum SoeAttestTreeMode {
		TimeAttest = 1,
		PayrollCalculation = 2
	}
	export const enum SoeCategoryRecordEntity {
		Unknown = 0,
		Product = 1,
		Customer = 2,
		Supplier = 3,
		ContactPerson = 4,
		AttestRole = 5,
		Employee = 6,
		Project = 7,
		Contract = 8,
		Inventory = 9,
		AttestRoleSecondary = 10,
		ShiftType = 11,
		TimeTerminal = 12,
		Order = 13
	}
	export const enum SoeCategoryType {
		Unknown = 0,
		Product = 1,
		Customer = 2,
		Supplier = 3,
		ContactPerson = 4,
		AttestRole = 5,
		Employee = 6,
		Project = 7,
		Contract = 8,
		Inventory = 9,
		Order = 10,
		PayrollProduct = 11
	}
	export const enum SoeDataStorageRecordType {
		Unknown = 0,
		TimeSalaryExport = 11,
		TimeSalaryExportEmployee = 12,
		TimeSalaryExportControlInfoEmployee = 13,
		TimeSalaryExportControlInfo = 14,
		TimeSalaryExportSaumaPdf = 15,
		TimeSalaryExportCSR = 16,
		TimeKU10ExportEmployee = 17,
		TimeKU10Export = 18,
		SOPCustomerInvoiceExport = 21,
		BillingInvoicePDF = 22,
		BillingInvoiceXML = 23,
		DiRegnskapCustomerInvoiceExport = 24,
		UniMicroCustomerInvoiceExport = 25,
		FinvoiceCustomerInvoiceExport = 26,
		DnBNorCustomerInvoiceExport = 27,
		InvoicePaymentServiceExport = 28,
		XEMailFileAttachment = 31,
		UploadedFile = 41,
		InvoiceBitmap = 51,
		InvoicePdf = 52,
		OrderInvoiceFileAttachment = 53,
		OrderInvoiceFileAttachment_Thumbnail = 54,
		OrderInvoiceFileAttachment_Image = 55,
		HelpAttachment = 61,
		CustomerFileAttachment = 62,
		PayrollSlipXML = 71,
		VacationYearEndHead = 72,
		SEPAPaymentImport = 81
	}
	export const enum SoeEmployeePostStatus {
		None = 0,
		Locked = 1,
		HasSchedule = 101,
		HasEmployee = 102,
		NotFound = 404
	}
	export const enum SoeEmployeeTimePeriodStatus {
		None = 0,
		Open = 1,
		Paid = 2,
		Locked = 3
	}
	export const enum SoeEmploymentFinalSalaryStatus {
		None = 0,
		ApplyFinalSalary = 1,
		AppliedFinalSalary = 2
	}
	export const enum SoeEntityImageType {
		Unknown = 0,
		Inventory = 1,
		EmployeePortrait = 2,
		OrderInvoice = 3,
		OrderInvoiceSignature = 4,
		ChecklistHeadRecord = 5,
		ChecklistHeadRecordSignature = 6,
		CaseProjectScreenShot = 7,
		ChecklistHeadRecordSignatureExecutor = 8,
		SupplierInvoice = 9,
		Customer = 10
	}
	export const enum SoeEntityState {
		Active = 0,
		Inactive = 1,
		Deleted = 2,
		Temporary = 3
	}
	export const enum SoeEntityType {
		None = 0,
		SupplierInvoice = 1,
		CustomerInvoice = 2,
		SupplierPayment = 3,
		CustomerPayment = 4,
		Offer = 5,
		Order = 6,
		Voucher = 7,
		Contract = 8,
		Inventory = 9,
		TimeScheduleEmployeePeriod = 10,
		Employee = 11,
		User = 12,
		Category = 13,
		Role = 14,
		EmployeeSkill = 15,
		EmployeeSchedule = 16,
		EmployeeRequest = 17,
		XEMail = 18,
		SupplierPaymentSuggestion = 19,
		PaymentRow = 20,
		TimeProject = 21,
		ChecklistHeadRecord = 22,
		TimeScheduleTemplateBlock = 23,
		CaseProject = 24,
		AbsenceAnnouncement = 25,
		EmployeeTaxSE = 26,
		Supplier = 27,
		Employment = 28,
		EmploymentPriceType = 29,
		EmploymentPriceTypePeriod = 30,
		FixedPayrollRow = 31,
		AbsencePlanning = 32,
		Customer = 33,
		VacationYearEndHead = 34,
		EmployeeGroup = 35,
		MessageGroup = 36,
		EmployeePostSkill = 37
	}
	export const enum SoeInvoiceRowType {
		Unknown = 0,
		AccountingRow = 1,
		ProductRow = 2,
		TextRow = 3,
		BaseProductRow = 4,
		PageBreakRow = 5,
		SubTotalRow = 6
	}
	export const enum SoeInvoiceType {
		Unknown = 0,
		SupplierInvoice = 1,
		CustomerInvoice = 2
	}
	export const enum SoeModule {
		None = 0,
		Manage = 1,
		Economy = 2,
		Billing = 3,
		Estatus = 4,
		Time = 5,
		TimeSchedulePlanning = 6,
		Communication = 7
	}
	export const enum SoeOriginStatus {
		None = 0,
		Draft = 1,
		Origin = 2,
		Voucher = 3,
		Cancel = 4,
		Export = 5,
		Payment = 31,
		Matched = 32,
		OfferPartlyOrder = 41,
		OfferFullyOrder = 42,
		OfferPartlyInvoice = 43,
		OfferFullyInvoice = 44,
		OfferClosed = 45,
		OrderPartlyInvoice = 51,
		OrderFullyInvoice = 52,
		OrderClosed = 53,
		All = 100
	}
	export const enum SoeOriginType {
		None = 0,
		SupplierInvoice = 1,
		CustomerInvoice = 2,
		SupplierPayment = 3,
		CustomerPayment = 4,
		Offer = 5,
		Order = 6,
		Contract = 7
	}
	export const enum SoePaymentStatus {
		None = 0,
		Verified = 1,
		Error = 2,
		Cancel = 4,
		Exported = 5,
		Pending = 11,
		ManualPayment = 21,
		Checked = 31,
		MBEV0025 = 41,
		MTRV0013 = 42,
		MTRV0014 = 43,
		MTRV0015 = 44,
		MTRV0018 = 45,
		MTRV0025 = 46,
		MTRV0035 = 47,
		MTRV0038 = 48,
		MTRV0041 = 49,
		MTRV0042 = 50,
		MTRV0043 = 51,
		MTRV0046 = 52,
		MTRV0050 = 53,
		MTRV0051 = 54,
		MTRV0052 = 55,
		MTRV0055 = 56,
		MTRV0056 = 57,
		MTRV0057 = 58,
		MTRV0058 = 59,
		MTRV0059 = 60,
		MTRV0064 = 61,
		MTRV0081 = 62,
		MTRV0082 = 63,
		MTRV0110 = 64,
		MTRV0111 = 65,
		MTRV0113 = 66,
		MTRV0124 = 67,
		MTRV0126 = 68,
		MTRV0130 = 69,
		MTRV0147 = 70,
		MTRV0148 = 71,
		MTRV0149 = 72,
		MTRV0152 = 73,
		MTRV0153 = 74,
		MTRV0155 = 75,
		MTRV0156 = 76,
		MTRV0302 = 77,
		MTRV0303 = 78
	}
	export const enum SoeProductType {
		Unknown = 0,
		InvoiceProduct = 1,
		PayrollProduct = 2
	}
	export const enum SoeProgressInfoType {
		Unknown = 0,
		Budget = 1,
		ScheduleEmployeePost = 2
	}
	export const enum SoeRecalculateTimeRecordStatus {
		None = 0,
		Unprocessed = 1,
		UnderProcessing = 2,
		Processed = 3,
		Error = 4
	}
	export const enum SoeScheduleWorkRules {
		None = 0,
		OverlappingShifts = 1,
		WorkTimeWeekMaxMin = 2,
		RestDay = 3,
		AttestedDay = 4,
		RestWeek = 5,
		MinorsWorkingHours = 6,
		MinorsWorkTimeDay = 7,
		MinorsWorkTimeSchoolDay = 8,
		MinorsWorkTimeWeek = 9,
		MinorsRestDay = 10,
		MinorsRestWeek = 11,
		MinorsBreaks = 12,
		MinorsSummerHoliday = 13,
		MinorsWorkAlone = 14,
		MinorsHandlingMoney = 15,
		WorkTimeDay = 16,
		ScheduleCycleRule = 17,
		WorkTimeWeekPartTimeWorkers = 18,
		Breaks = 19
	}
	export const enum SoeSelectionData {
		Str_Account = 11,
		Int_Voucher_VoucherSeriesId = 21,
		Int_Voucher_VoucherNr = 22,
		Str_Ledger_ActorNr = 31,
		Int_Ledger_InvoiceSeqNr = 32,
		Int_Ledger_DateRegard = 33,
		Int_Ledger_SortOrder = 34,
		Int_Ledger_InvoiceSelection = 35,
		Str_Billing_CustomerNr = 41,
		Str_Billing_InvoiceNr = 42,
		Int_Billing_SortOrder = 43,
		Str_Billing_ProjectNr = 44,
		Str_Billing_EmployeeNr = 45,
		Str_Billing_ProductNr = 46,
		Int_Billing_StockLocationId = 47,
		Int_Billing_StockShelfId = 47,
		Int_Time_EmployeeId = 51,
		Int_Time_CategoryId = 52,
		Int_Time_ShiftTypeIds = 53,
		Int_Time_PayrollProductId = 54,
		Int_Export_Type = 60,
		Int_Export_File_Type = 61
	}
	export const enum SoeStaffingNeedsTaskType {
		Unknown = 0,
		Task = 1,
		Delivery = 2
	}
	export const enum SoeStatusIcon {
		None = 0,
		Attachment = 1,
		Image = 2,
		Checklist = 4,
		Email = 8,
		Imported = 16,
		ElectronicallyDistributed = 32,
		EmailError = 64
	}
	export const enum SoeSysPriceListProviderType {
		Unknown = 0,
		Electrician = 1,
		Plumbing = 2
	}
	export const enum SoeTimeCodeRuleType {
		Unknown = 0,
		TimeCodeEarlierThanStart = 1,
		TimeCodeLaterThanStop = 2,
		TimeCodeLessThanMin = 3,
		TimeCodeBetweenMinAndStd = 4,
		TimeCodeStd = 5,
		TimeCodeBetweenStdAndMax = 6,
		TimeCodeMoreThanMax = 7,
		AutogenBreakOnStamping = 10
	}
	export const enum SoeTimeCodeType {
		None = 0,
		Work = 1,
		Absense = 2,
		Break = 3,
		Addition = 5,
		Deduction = 6,
		Material = 8,
		WorkAndAbsense = 101,
		WorkAndAbsenseAndAdditionDeduction = 102,
		WorkAndMaterial = 103,
		AdditionAndDeduction = 104
	}
	export const enum SoeTimeHalfdayType {
		RelativeStartValue = 1,
		RelativeEndValue = 2,
		RelativeStartPercentage = 3,
		RelativeEndPercentage = 4,
		ClockInMinutes = 5
	}
	export const enum SoeTimePayrollScheduleTransactionType {
		None = 0,
		Schedule = 1,
		Absence = 2
	}
	export const enum SoeTimeScheduleDeviationCauseStatus {
		None = 0,
		Standard = 1,
		Planned = 2
	}
	export const enum SoeTimeScheduleTemplateBlockBreakType {
		None = 0,
		NormalBreak = 1
	}
	export const enum StaffingNeedsHeadType {
		NeedsPlanning = 0,
		NeedsShifts = 1
	}
	export const enum StaffingNeedsRowOriginType {
		StaffingNeedsAnalysisChartData = 0,
		TimeScheduleTask = 1,
		IncomingDelivery = 2
	}
	export const enum StaffingNeedsRowType {
		Normal = 0,
		ShiftTypeSummary = 1,
		TotalSummary = 2
	}
	export const enum TermGroup_AccountDistributionCalculationType {
		Percent = 1,
		Amount = 2,
		TotalAmount = 3
	}
	export const enum TermGroup_AccountDistributionPeriodType {
		Unknown = 0,
		Period = 1,
		Year = 2,
		Amount = 3
	}
	export const enum TermGroup_AccountDistributionTriggerType {
		None = 0,
		Registration = 1,
		Distribution = 2
	}
	export const enum TermGroup_AccountMandatoryLevel {
		None = 0,
		Warn = 1,
		Mandatory = 2,
		Stop = 3
	}
	export const enum TermGroup_AccountStatus {
		New = 1,
		Open = 2,
		Closed = 3,
		Locked = 4
	}
	export const enum TermGroup_AttestEntity {
		Unknown = 0,
		SupplierInvoice = 1,
		Offer = 11,
		Order = 12,
		Contract = 13,
		PayrollTime = 21,
		InvoiceTime = 22,
		CaseProject = 31
	}
	export const enum TermGroup_AttestFlowRowState {
		Unhandled = 0,
		Handled = 1,
		Deleted = 2
	}
	export const enum TermGroup_AttestRuleRowLeftValueType {
		None = 0,
		PresenceTime = 1,
		ScheduledTime = 2,
		WorkedInsideScheduledTime = 3,
		ScheduledBreakTime = 4,
		TotalBreakTime = 5,
		TimeCode = 21,
		PayrollProduct = 22,
		InvoiceProduct = 23
	}
	export const enum TermGroup_AttestRuleRowRightValueType {
		None = 0,
		PresenceTime = 1,
		ScheduledTime = 2,
		WorkedInsideScheduledTime = 3,
		ScheduledBreakTime = 4,
		TotalBreakTime = 5,
		TimeCode = 21,
		PayrollProduct = 22,
		InvoiceProduct = 23
	}
	export const enum TermGroup_AttestTreeGrouping {
		None = 0,
		Category = 1,
		EmployeeGroup = 2,
		PayrollGroup = 3,
		All = 4
	}
	export const enum TermGroup_AttestTreeSorting {
		None = 0,
		EmployeeNr = 1,
		FirstName = 2,
		LastName = 3
	}
	export const enum TermGroup_AttestWorkFlowRowProcessType {
		Unknown = 0,
		Registered = 1,
		WaitingForProcess = 2,
		Processed = 3,
		LevelNotReached = 4,
		TransferredToOtherUser = 5,
		TransferredWithReturn = 6,
		Returned = 7
	}
	export const enum TermGroup_AttestWorkFlowType {
		All = 0,
		Any = 1
	}
	export const enum TermGroup_BillingType {
		None = 0,
		Debit = 1,
		Credit = 2,
		Interest = 3,
		Reminder = 4
	}
	export const enum TermGroup_ChecklistHeadType {
		Unknown = 0,
		Order = 1
	}
	export const enum TermGroup_ChecklistRowType {
		Unknown = 0,
		String = 1,
		YesNo = 2,
		Checkbox = 3,
		MultipleChoice = 4
	}
	export const enum TermGroup_CurrencyIntervalType {
		None = 0,
		Manually = 1,
		FirstDayOfQuarter = 2,
		FirstDayOfMonth = 3,
		EveryMonday = 4,
		EveryDay = 5
	}
	export const enum TermGroup_CurrencySource {
		None = 0,
		Manually = 1,
		Daily = 2,
		Tullverket = 3,
		ECB = 4
	}
	export const enum TermGroup_EDIInvoiceStatus {
		Unprocessed = 1,
		UnderProcessing = 2,
		Processed = 3,
		Error = 4
	}
	export const enum TermGroup_EdiMessageType {
		Unknown = 0,
		SupplierInvoice = 1,
		OrderAcknowledgement = 2,
		DeliveryNotification = 3
	}
	export const enum TermGroup_EDIOrderStatus {
		Unprocessed = 1,
		UnderProcessing = 2,
		Processed = 3,
		Error = 4
	}
	export const enum TermGroup_EDISourceType {
		EDI = 1,
		Scanning = 2,
		Finvoice = 3
	}
	export const enum TermGroup_EDIStatus {
		Unprocessed = 1,
		UnderProcessing = 2,
		Processed = 3,
		Error = 4,
		Duplicate = 5
	}
	export const enum TermGroup_EmployeeDisbursementMethod {
		Unknown = 0,
		SE_CashDeposit = 1,
		SE_PersonAccount = 2,
		SE_AccountDeposit = 3
	}
	export const enum TermGroup_EmployeeFactorType {
		CalendarDayFactor = 1,
		VacationCoefficient = 2,
		AverageWorkTimeWeek = 3,
		AverageWorkTimeShift = 4,
		Net = 5,
		VacationDaysPaidByLaw = 6,
		VacationDayPercent = 7,
		VacationHourPercent = 8,
		VacationVariablePercent = 9,
		GuaranteeAmount = 10
	}
	export const enum TermGroup_EmployeePostWeekendType {
		AutomaticWeekend = 0,
		PreferEvenWeekWeekend = 1,
		PreferOddWeekWeekend = 2
	}
	export const enum TermGroup_EmployeeRequestResultStatus {
		None = 0,
		PartlyGranted = 1,
		FullyGranted = 2,
		PartlyDenied = 3,
		FullyDenied = 4,
		PartlyGrantedPartlyDenied = 5
	}
	export const enum TermGroup_EmployeeRequestStatus {
		None = 0,
		RequestPending = 1,
		Preliminary = 2,
		Definate = 3,
		PartlyDefinate = 4,
		Restored = 5
	}
	export const enum TermGroup_EmployeeRequestType {
		Undefined = 0,
		AbsenceRequest = 1,
		InterestRequest = 2,
		NonInterestRequest = 3
	}
	export const enum TermGroup_EmployeeRequestTypeFlags {
		Undefined = 0,
		AbsenceRequest = 1,
		InterestRequest = 2,
		NonInterestRequest = 3,
		PartyDefined = 8
	}
	export const enum TermGroup_EmployeeStatisticsType {
		AnsweredCalls = 1,
		CallDuration = 2,
		ConnectedTime = 3,
		NotAnsweredCalls = 4,
		Arrival = 5,
		GoHome = 6,
		ArrivalAndGoHome = 7
	}
	export const enum TermGroup_EmployeeTaxAdjustmentType {
		NotSelected = 0,
		PercentTax = 1,
		IncreasedTaxBase = 2,
		DecreasedTaxBase = 3,
		NoTax = 4
	}
	export const enum TermGroup_EmployeeTaxEmploymentAbroadCode {
		None = 0,
		SweToCan = 1,
		SweToUsa = 2,
		SweToQue = 3,
		SweToInd = 4,
		CanToSweGroup = 21,
		UsaToSweGroup = 22,
		QueToSweGroup = 23,
		CanToSwe = 41,
		UsaToSwe = 42,
		QueToSwe = 43,
		IndToSwe = 44
	}
	export const enum TermGroup_EmployeeTaxEmploymentTaxType {
		NotSelected = 0,
		EmploymentTax = 1,
		PayrollTax = 2,
		EmploymentAbroad = 5
	}
	export const enum TermGroup_EmployeeTaxSalaryDistressAmountType {
		NotSelected = 0,
		FixedAmount = 1,
		AllSalary = 2
	}
	export const enum TermGroup_EmployeeTaxSinkType {
		NotSelected = 0,
		Normal = 1,
		AthletsArtistSailors = 2
	}
	export const enum TermGroup_EmployeeTaxType {
		NotSelected = 0,
		TableTax = 1,
		SideIncomeTax = 2,
		Adjustment = 3,
		Sink = 4,
		SchoolYouth = 5,
		NoTax = 6
	}
	export const enum TermGroup_EmploymentChangeFieldType {
		DateFrom = 1,
		DateTo = 2,
		State = 3,
		EmployeeGroupId = 101,
		PayrollGroupId = 102,
		PayrollPriceTypeId = 103,
		EmploymentType = 201,
		Name = 202,
		WorkTimeWeek = 203,
		Percent = 204,
		ExperienceMonths = 205,
		ExperienceAgreedOrEstablished = 206,
		SpecialConditions = 209,
		WorkPlace = 210,
		BaseWorkTimeWeek = 212,
		SubstituteFor = 213,
		EmploymentEndReason = 214,
		PayrollPriceTypeAmount = 215,
		FixedAccounting = 216,
		SubstituteForDueTo = 217,
		WorkTasks = 218,
		ExternalCode = 219,
		EmployeeGroupName = 301,
		PayrollGroupName = 302,
		PayrollPriceTypeName = 303
	}
	export const enum TermGroup_EmploymentChangeType {
		Unknown = 0,
		Information = 1,
		DataChange = 2
	}
	export const enum TermGroup_EmploymentType {
		Unknown = 0,
		SE_Probationary = 1,
		SE_Substitute = 2,
		SE_SubstituteVacation = 3,
		SE_Permanent = 4,
		SE_FixedTerm = 5,
		SE_Seasonal = 6,
		SE_SpecificWork = 7,
		SE_Trainee = 8,
		SE_NormalRetirementAge = 9,
		SE_CallContract = 10,
		SE_LimitedAfterRetirementAge = 11,
		SE_FixedTerm14days = 12
	}
	export const enum TermGroup_InventoryStatus {
		None = 0,
		Draft = 1,
		Active = 2,
		Discarded = 3,
		Sold = 4,
		Inactive = 5
	}
	export const enum TermGroup_InventoryWriteOffMethodPeriodType {
		Period = 1,
		Year = 2
	}
	export const enum TermGroup_InvoiceProductCalculationType {
		Regular = 0,
		SupplementCharge = 1,
		FixedPrice = 2,
		Lift = 3,
		Contract = 4,
		Clearing = 5
	}
	export const enum TermGroup_InvoiceProductVatType {
		None = 0,
		Merchandise = 1,
		Service = 2
	}
	export const enum TermGroup_InvoiceVatType {
		None = 0,
		Merchandise = 1,
		Contractor = 3,
		NoVat = 4,
		EU = 5,
		NonEU = 6
	}
	export const enum TermGroup_IOImportHeadType {
		Unknown = 0,
		Supplier = 1,
		SupplierInvoice = 2,
		SupplierInvoiceAnsjo = 3,
		Customer = 4,
		CustomerInvoice = 5,
		CustomerInvoiceRow = 6,
		StaffingNeedsFrequency = 7,
		Voucher = 8,
		Project = 9,
		AccountDistributionHead = 10,
		AccountYear = 11,
		InvoiceProduct = 13,
		AccountYearBalance = 14,
		Account = 15,
		SideDictionary = 16,
		Employee = 17,
		Stock = 18,
		TimeCodeTransaction = 19,
		Inventory = 20,
		Budget = 21,
		Settings = 22,
		CompanyInformation = 23,
		Employment = 24,
		VactionDays = 25,
		Payments = 26,
		Schedule = 27,
		GrossProfitCodes = 28,
		TimePayrollTransaction = 29,
		TimeInvoiceTransaction = 30,
		Checklist = 31,
		DistributionCode = 32,
		TextBlock = 33,
		Contact = 34,
		FixedPayrollRow = 35,
		PayrollStartValueRow = 36,
		EdiMessage = 37
	}
	export const enum TermGroup_IOSource {
		Unknown = 0,
		XE = 1,
		TilTid = 2,
		FlexForce = 3,
		Connect = 4
	}
	export const enum TermGroup_IOStatus {
		Unprocessed = 1,
		UnderProcessing = 2,
		Processed = 3,
		Error = 4
	}
	export const enum TermGroup_IOType {
		Unknown = 0,
		Excel = 1,
		WebService = 2,
		XEConnect = 3
	}
	export const enum TermGroup_Languages {
		Unknown = 0,
		Swedish = 1,
		English = 2,
		Finnish = 3,
		Norwegian = 4,
		Danish = 5
	}
	export const enum TermGroup_MessageDeliveryType {
		XEmail = 1,
		Email = 2,
		SMS = 3
	}
	export const enum TermGroup_MessagePriority {
		None = 0,
		Low = 1,
		Normal = 2,
		High = 3
	}
	export const enum TermGroup_MessageTextType {
		XAML = 1,
		HTML = 2,
		RTF = 3,
		Text = 4,
		SMS = 5
	}
	export const enum TermGroup_MessageType {
		None = 0,
		UserInitiated = 1,
		ShiftRequest = 2,
		AttestInvoice = 3,
		AttestReminder = 4,
		ShiftRequestAnswer = 5,
		CaseProjectNew = 6,
		AutomaticInformation = 7
	}
	export const enum TermGroup_OrderType {
		Unspecified = 0,
		Project = 1,
		Sales = 2,
		Internal = 3
	}
	export const enum TermGroup_PayrollResultType {
		None = 0,
		Time = 1,
		Quantity = 2
	}
	export const enum TermGroup_PayrollReviewStatus {
		New = 0,
		Preliminary = 1,
		Executed = 2
	}
	export const enum TermGroup_PayrollType {
		Unknown = 0,
		Work = 1,
		Absence = 2,
		OverTime = 3,
		AddedTime = 4,
		InconvinientWorkingHours = 5,
		Addition = 6,
		Deduction = 7,
		SickLeave = 8,
		ParentalLeave = 9,
		PregnancyLeave = 10,
		TemporaryParentalLeave = 11,
		LeaveOfAbsence = 12,
		PayedAbsence = 13,
		Vacation = 14,
		CompVacant = 15,
		ShortenWorkingHours = 16,
		TimeBank = 17
	}
	export const enum TermGroup_ProjectAllocationType {
		Unknown = 0,
		External = 1,
		Internal = 2,
		InternalWithOccupancy = 3
	}
	export const enum TermGroup_ProjectStatus {
		Unknown = 0,
		Planned = 1,
		Active = 2,
		Locked = 3,
		Finished = 4,
		Hidden = 5
	}
	export const enum TermGroup_ProjectType {
		Unknown = 0,
		TimeProject = 1,
		CaseProject = 2
	}
	export const enum TermGroup_ProjectUserType {
		Unknown = 0,
		Manager = 1,
		Participant = 2
	}
	export const enum TermGroup_ReportExportFileType {
		Unknown = 0,
		Payroll_SIE_Accounting = 1,
		Payroll_SCB_Statistics = 2,
		Payroll_Visma_Accounting = 3,
		KU10 = 4,
		eSKD = 5,
		QlikViewType1 = 6,
		Collectum = 7,
		Fora = 8,
		KPA = 9,
		SCB_KSJU = 10,
		Payroll_SN_Statistics = 11
	}
	export const enum TermGroup_ReportExportType {
		Unknown = 0,
		Pdf = 1,
		Xml = 2,
		Excel = 3,
		Word = 4,
		RichText = 5,
		EditableRTF = 6,
		Text = 7,
		TabSeperatedText = 8,
		CharacterSeparatedValues = 9,
		File = 10
	}
	export const enum TermGroup_ReportGroupAndSortingTypes {
		Unknown = 0,
		AccountNr = 1,
		AccountInternalDim2 = 2,
		AccountInternalDim3 = 3,
		AccountInternalDim4 = 4,
		AccountInternalDim5 = 5,
		AccountInternalDim6 = 6,
		EmployeeCategoryName = 7,
		EmployeeGroupName = 8,
		EmployeeNr = 9,
		EmployeeLastName = 10,
		EmployeeFirstName = 11,
		EmployeeGender = 12,
		PayrollProductNr = 13,
		PayrollTypLevel1 = 14,
		PayrollTypLevel2 = 15,
		PayrollTypLevel3 = 16,
		PayrollTypLevel4 = 17,
		PayrollTransactionDate = 18,
		CustomerName = 101,
		CustomerNr = 102,
		InvoiceNr = 103,
		SupplierName = 200,
		SupplierNr = 201
	}
	export const enum TermGroup_RetroactivePayrollAccountType {
		Unknown = 0,
		TransactionAccount = 1,
		OtherAccount = 2
	}
	export const enum TermGroup_ScanningMessageType {
		Unknown = 0,
		Arrival = 1,
		SupplierInvoice = 2
	}
	export const enum TermGroup_ScanningStatus {
		Unprocessed = 1,
		Interpreted = 2,
		Processed = 3,
		Error = 4
	}
	export const enum TermGroup_Sex {
		Unknown = 0,
		Male = 1,
		Female = 2
	}
	export const enum TermGroup_ShiftHistoryType {
		Unknown = 0,
		DragShiftActionMove = 1,
		DragShiftActionCopy = 2,
		DragShiftActionReplace = 3,
		DragShiftActionReplaceAndFree = 4,
		DragShiftActionSwapEmployee = 5,
		DragShiftActionAbsence = 6,
		DragShiftActionDelete = 7,
		HandleShiftActionWanted = 11,
		HandleShiftActionChangeEmployee = 12,
		HandleShiftActionSwapEmployee = 13,
		HandleShiftActionAbsenceAnnouncement = 14,
		AbsenceRequestPlanning = 31,
		AbsencePlanning = 32,
		TaskSaveTimeScheduleShift = 33,
		TaskDeleteTimeScheduleShift = 34,
		TaskSplitTimeScheduleShift = 35,
		AbsenceRequestPlanningRestored = 36,
		ShiftRequest = 37,
		AssignTaskToEmployee = 38,
		AssignEmployeeFromQueue = 39,
		DropEmployeeOnShift = 40,
		TaskSaveOrderShift = 41,
		TaskSaveBooking = 42,
		TemplateScheduleSave = 51,
		TemplateScheduleActivate = 52,
		EditBreaks = 61
	}
	export const enum TermGroup_SoePayrollPriceType {
		Misc = 0,
		Hourly = 1,
		Monthly = 2,
		Fulltime = 3
	}
	export const enum TermGroup_SoeRetroactivePayrollEmployeeStatus {
		Unknown = 0,
		Registered = 1,
		Performed = 2,
		Error = 3,
		EmployeeExistsInOtherActiveRetro = 4,
		EmployeeHasChangedTimePeriodHead = 5,
		EmployeeHasNoBasis = 6
	}
	export const enum TermGroup_SoeRetroactivePayrollOutcomeErrorCode {
		None = 0,
		EmploymentNotFound = 1,
		SpecifiedUnitPrice = 2,
		FormulaNotFound = 3,
		RetroDontOverlapPeriod = 4
	}
	export const enum TermGroup_SoeRetroactivePayrollStatus {
		Unknown = 0,
		Registered = 1,
		Performed = 2
	}
	export const enum TermGroup_StaffingNeedsHeadStatus {
		None = 0,
		Preliminary = 1,
		Definate = 2
	}
	export const enum TermGroup_SysContactAddressRowType {
		Address = 1,
		AddressCO = 2,
		PostalCode = 3,
		PostalAddress = 4,
		Country = 5,
		StreetAddress = 6,
		EntranceCode = 7,
		Name = 8
	}
	export const enum TermGroup_SysContactAddressType {
		Undefined = 0,
		Distribution = 1,
		Visiting = 2,
		Billing = 3,
		Delivery = 4,
		BoardHQ = 5
	}
	export const enum TermGroup_SysDayType {
		Unknown = 0,
		PublicHoliday = 1,
		Workday = 2,
		Saturday = 3,
		Sunday = 4
	}
	export const enum TermGroup_SysPaymentType {
		Unknown = 0,
		BG = 1,
		PG = 2,
		Bank = 3,
		BIC = 4,
		SEPA = 5,
		Autogiro = 6,
		Nets = 7,
		SOP = 8,
		Cfp = 9
	}
	export const enum TermGroup_SysVehicleFuelType {
		Unknown = 0,
		Gasoline = 1,
		Diesel = 2,
		Electricity = 3,
		Gas = 4,
		Alcohol = 5,
		ElectricHybrid = 6,
		PlugInHybrid = 7
	}
	export const enum TermGroup_TimeCodeRegistrationType {
		Unknown = 0,
		Time = 1,
		Quantity = 2
	}
	export const enum TermGroup_TimeCodeRoundingType {
		None = 0,
		RoundUp = 1,
		RoundDown = 2,
		RoundUpWholeDay = 3,
		RoundDownWholeDay = 4
	}
	export const enum TermGroup_TimeDeviationCauseType {
		Undefined = 0,
		Absence = 1,
		Presence = 2,
		PresenceAndAbsence = 3
	}
	export const enum TermGroup_TimePeriodType {
		Unknown = 0,
		Billing = 1,
		Payroll = 2
	}
	export const enum TermGroup_TimeScheduleTemplateBlockShiftStatus {
		Open = 0,
		Assigned = 1
	}
	export const enum TermGroup_TimeScheduleTemplateBlockShiftUserStatus {
		None = 0,
		Accepted = 1,
		Unwanted = 2,
		AbsenceRequested = 3,
		AbsenceApproved = 4
	}
	export const enum TermGroup_TimeScheduleTemplateBlockType {
		Schedule = 0,
		Order = 1,
		Booking = 2,
		Employee = 101,
		Need = 102
	}
	export const enum TermGroup_VacationGroupCalculationType {
		Unknown = 0,
		DirectPayment_AccordingToVacationLaw = 1,
		DirectPayment_AccordingToCollectiveAgreement = 2,
		EarningYearIsBeforeVacationYear_PercentCalculation_AccordingToVacationLaw = 11,
		EarningYearIsBeforeVacationYear_PercentCalculation_AccordingToCollectiveAgreement = 12,
		EarningYearIsBeforeVacationYear_VacationDayAddition_AccordingToVacationLaw = 13,
		EarningYearIsBeforeVacationYear_VacationDayAddition_AccordingToCollectiveAgreement = 14,
		EarningYearIsVacationYear_ABAgreement = 21,
		EarningYearIsVacationYear_VacationDayAddition = 22
	}
	export const enum TermGroup_VacationGroupGuaranteeAmountMaxNbrOfDaysRule {
		Unknown = 0,
		All = 1,
		Max25Paid = 2
	}
	export const enum TermGroup_VacationGroupRemainingDaysRule {
		Unknown = 0,
		SavedAccordingToVacationLaw = 1,
		AllRemainingDaysSaved = 2,
		AllRemainingDaysSavedToYear1 = 3
	}
	export const enum TermGroup_VacationGroupType {
		Unknown = 0,
		NoCalculation = 1,
		DirectPayment = 2,
		EarningYearIsBeforeVacationYear = 3,
		EarningYearIsVacationYear = 4
	}
	export const enum TermGroup_VacationGroupVacationAbsenceCalculationRule {
		Unknown = 0,
		Actual = 1,
		PerDay = 2,
		PerHour = 3
	}
	export const enum TermGroup_VacationGroupVacationDaysHandleRule {
		Unknown = 0,
		VacationFactor = 1,
		Gross = 2,
		Net = 3,
		VacationCoefficient = 4
	}
	export const enum TermGroup_VacationGroupVacationHandleRule {
		Unknown = 0,
		Days = 1,
		Hours = 2,
		Shifts = 3
	}
	export const enum TermGroup_VacationGroupVacationSalaryPayoutRule {
		Unknown = 0,
		InConjunctionWithVacation = 1,
		PartlyPayoutBeforeVacation = 2,
		AllBeforeVacation = 3
	}
	export const enum TermGroup_VacationGroupYearEndOverdueDaysRule {
		Unknown = 0,
		Paid = 1,
		Saved = 2
	}
	export const enum TermGroup_VacationGroupYearEndRemainingDaysRule {
		Unknown = 0,
		Paid = 1,
		Saved = 2,
		Over20DaysSaved = 3
	}
	export const enum TermGroup_VacationGroupYearEndVacationVariableRule {
		Unknown = 0,
		Paid = 1,
		Saved = 2
	}
	export const enum TermGroup_VatDeductionType {
		None = 0
	}
	export const enum TermGroup_VehicleType {
		Unknown = 0,
		Car = 1,
		Lorry = 2
	}
	export const enum WildCard {
		LessThan = 0,
		LessThanOrEquals = 1,
		Equals = 2,
		GreaterThanOrEquals = 3,
		GreaterThan = 4
	}
	export const enum XEMailAnswerType {
		None = 0,
		Yes = 1,
		No = 2
	}
	export const enum XEMailRecipientType {
		User = 0,
		Group = 1,
		Role = 2,
		Category = 3,
		MessageGroup = 4,
		Employee = 5
	}
}
module System {
	export const enum DayOfWeek {
		Sunday = 0,
		Monday = 1,
		Tuesday = 2,
		Wednesday = 3,
		Thursday = 4,
		Friday = 5,
		Saturday = 6
	}
}

