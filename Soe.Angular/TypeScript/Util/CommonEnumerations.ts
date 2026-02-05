// Automatically generated
// Do not manually change this file. Use PreBuild/AngularProjectPreProcess. Source code can be found on TFS/XE/Main/Tools/

export enum AbsenceRequestShiftPlanningAction {
	Undefined = 0, //No action has been choosen
	ReplaceWithOtherEmployee = 1, //Absencerequest is approved. Shift will be given to an other employee
	ReplaceWithHiddenEmployee = 2, //Absencerequest is approved. Shift will be availble for others to take
	NoReplacement = 3, //Absencerequest is approved but Shift will not be replace with another employee
	NotApproved = 4, // Absencerequest is not approved. The Shift (scheduleblock) will be marked as not approved
}

export enum ActorConsentType {
	Unspecified = 0,
	EmployeePortrait = 1
}

export enum AccountHierarchyParamType {
	None = 0,
	UseDefaultEmployeeAccountDimSelector = 1,
	UseDefaultEmployeeAccountDimEmployee = 2,
	IncludeOnlyChildrenOneLevel = 3,
	IncludeVirtualParented = 4,
	IgnoreAttestRoleDates = 5,
	OnlyDefaultAccounts = 6,
	UseEmployeeAccountIfNoAttestRole = 7,
	IncludeAbstract = 8
}

export enum AdminTaskType {
	Unknown = 0,
	RestoreTermCache = 1,
	RestoreSysCache = 2,
	RestoreCompCache = 3,
	//AdminMessage = 4,
	ClearTimeRuleCache = 5,
}

export enum ActionResultSelect {
	// Common errors
	Unknown = 0,
	EntityIsNull = 1,
	EntityNotFound = 2,
	NothingSelected = 3,
	
	// Entity specific errors
	
	// GetSysProductPrice
	ProductNotFound = 20,
	PriceNotFound = 21,
	
	//TimeSalaryFileExport
	TimeExportNoData = 30,
	TimeExportXmlFormattingFailed = 31,
	TimeExportXSLTransformationFailed = 32,
	TimeExportValidAttestTransitionNotFound = 33,
	
	// Checks
	TimeBlocksAreManuallyAdded = 40,
	TransactionsAreManuallyAdded = 41,
	TransactionsAreHasNotIntialState = 42,
	TransactionsAreTransferredToSalary = 43,
	
	// TimeTerminal
	EmployeeFoundButIsNotInTimeTerminalCategory = 50,
}

export enum ActionResultDelete {
	// Common errors
	Unknown = 0,
	EntityIsNull = 1,
	EntityNotFound = 2,
	NothingSaved = 3,
	NothingDeleted = 4,
	InsufficientInput = 5,
	EntityInUse = 6,
	
	// Entity specific errors
	
	// Account
	AccountStdExists = 10,
	AccountInternalExists = 11,
	AccountInternalUsedInEmployeeAccount = 12,
	
	// AccountDim
	AccountDimHasAccounts = 12,
	
	// Company
	CompanyHasRoles = 20,
	
	// Invoice
	InvoiceHasPayments = 30,
	InvoiceHasVouchers = 31,
	OfferIsTransferredToOrder = 32,
	OfferIsTransferredToInvoice = 33,
	OrderIsTransferredToInvoice = 34,
	
	// License
	LicenseHasCompanies = 40,
	
	// Report
	ReportGroupInUse = 50,
	ReportHeaderInUse = 51,
	ReportTemplateHasReports = 52,
	ReportHasReportPackages = 53,
	ReportHasReportPayrollGroups = 54,
	ReportHasReportChecklists = 55,
	ReportHasCustomers = 56,
	
	// Role
	RoleHasUsers = 60,
	
	// User
	UserHasAccountHistory = 70,
	UserHasVoucherRowHistory = 71,
	UserIsSysUser = 72,
	
	// Voucher
	OnlyTemplateCanBeRemoved = 80,
	VoucherRowAccountNotDeleted = 81,
	VoucherRowHistoryNotDeleted = 82,
	
	// VoucherSeries
	VoucherSeriesTypeHasVoucherSeries = 90,
	VoucherSeriesHasVoucherSeries = 91,
	
	// Category
	CategoryHasCompanyCategoryRecords = 100,
	
	// ContactPerson
	UserInUse = 110,
	ContactPersonDeleted = 111,
	ContactPersonNotDeleted = 112,
	
	// AttestRole
	AttestRoleHasUsers = 120,
	AttestRoleIsInUse = 121,
	
	// AttestState
	AttestStateHasTransitions = 130,
	AttestStateHasTransactions = 131,
	
	// DeleteUser
	UserDeleted = 140,
	UserNotDeleted = 141,
	
	// Scheduled jobs
	JobHasScheduledJobs = 150,
	
	// InvoiceProduct
	ProductHasInvoiceRows = 160,
	ProductHasInterestRows = 161,
	ProductHasReminderRows = 162,
	ProductNotDeleted = 163,
	ProductHasStockBalanse = 164,
	
	// PayrollProduct
	ProductHasTransactions = 170,
	ProductHasTimeCodes = 171,
	ProductHasPayrollProductPriceFormulas = 172,
	ProductHasPayrollProductPriceTypes = 173,
	ProductHasTimeAbsenceRules = 174,
	ProductHasTimeAbsenceRulePayrollProducts = 175,
	
	// Customer
	CustomerHasOffers = 180,
	CustomerHasContracts = 181,
	CustomerHasOrders = 182,
	CustomerHasInvoices = 183,
	
	// Supplier
	SupplierHasInvoices = 190,
	
	// EDI
	EdiFailedToDelete = 201,
	
	// Attest
	AttestTransitionInUse_Role = 211,
	AttestTransitionInUse_EmployeeGroup = 212,
	AttestTransitionInUse_Workflow = 213,
	AttestTransitionInUse_WorkflowTemplate = 214,
	
	// Inventory
	InventoryHasLogs = 301,
	InventoryWriteOffMethodHasInventories = 302,
	InventoryWriteOffMethodHasTemplates = 303,
	InventoryLinkedWithVouchers = 304,
	
	// TimePeriodHead
	NotUsed = 401,
	
	// TimeCode
	TimeCodeDeleted = 501,
	TimeCodeHasEmployees = 502,
	TimeCodeHasEmployeeGroups = 503,
	TimeCodeHasEmployeeGroupTimeDeviationMappings = 504,
	TimeCodeHasTimeAbsenceRuleHeads = 505,
	TimeCodeHasTimeAccumulatorEmployeeGroupRuleMappings = 506,
	TimeCodeHasTimeAccumulators = 507,
	TimeCodeHasTimeBlocks = 508,
	TimeCodeBreakHasTimeCodeDeviationCauses = 509,
	TimeCodeHasInvoiceProducts = 510,
	TimeCodeHasPayrollProducts = 511,
	TimeCodeHasTransactions = 512,
	TimeCodeHasDeviationCauses = 513,
	TimeCodeHasTemplateBlocks = 514,
	TimeCodeHasTimeRules = 515,
	TimeCodeHasTimeCodeRanking = 516,
	
	// ShiftType
	ShiftTypeInUse = 591,
	
	// SkillType
	SkillTypeHasSkills = 601,
	
	// Skill
	SkillHasEmployees = 611,
	SkillHasPositions = 612,
	SkillHasShiftTypes = 613,
	
	// DayType
	DayTypeHasHolidays = 621,
	DayTypeHasTimeHalfdays = 622,
	DayTypeHasTimeRules = 623,
	DayTypeHasAttestRuleHeads = 624,
	DayTypeHasEmployeeGroups = 625,
	
	// VatCode
	VatCodeInUse_CompanySetting = 631,
	VatCodeInUse_Product = 632,
	VatCodeInUse_InvoiceRow = 633,
	
	// TimeScheduleType
	TimeScheduleTypeHasRules = 641,
	TimeScheduleTypeHasBlocks = 642,
	
	// StaffingNeedsLocationGroup
	StaffingNeedsLocationGroupHasLocations = 651,
	StaffingNeedsLocationGroupHasRules = 652,
	
	// SysPosition
	SysPositionHasCompPositions = 661,
	
	// Sprint
	SprintInUse = 671,
	
	//EndReason
	EndReasonInUse = 681,
	
	//FollowUpType
	FollowUpTypeInUse = 691,
	
	PayrollPriceTypeInUse = 701,
	PayrollPriceTypeInUseInFormula = 702,
	PayrollPriceFormulaInUse = 703,
	PayrollPriceFormulaInUseInFormula = 704,
	PayrollGroupInUse = 705,
	
	// ScheduleJobHead
	ScheduledJobHeadHasAttestRuleHeads = 721,
	ScheduledJobHeadHasTimeAccumulatorEmployeeGroupRules = 722,
	
	// VacationYearEnd
	TimePeriodLocked = 801,
	
	//FinalSalary
	FinalSalary_TimePayrollTransactionsNotFound = 901,
	
	// TimeSchedulePlanning
	TimeSchedulePlanning_ShiftIsNull = 1101,
	TimeSchedulePlanning_UserNotFound = 1102,
	
	// Import/Export
	SysImportHeadHasDefinitions = 1201,
	SysExportHeadHasDefinitions = 1202,
	
	// CompanyGroup
	CompanyGroupMappingNotDeleted = 1300,
	
	// StaffingNeeds
	TimeScheduleTaskIsUsed = 1401,
	IncomingDeliveryIsUsed = 1411,
	
	// EmployeeGroup
	EmployeeGroupHasEmployments = 1501,
	EmployeeGroupHasEmploymentChanges = 1502,
	EmployeeGroupHasEmployeePosts = 1503,
	EmployeeGroupHasTimeAccumulators = 1504,
	EmployeeGroupHasAttestRules = 1505,
	EmployeeGroupHasTimeAbsenceRules = 1506,
	EmployeeGroupHasTimeRules = 1507,
	
	// TimeDeviationCause
	TimeDeviationCauseHasEmployee = 1601,
	TimeDeviationCauseHasEmployeeGroup = 1602,
	TimeDeviationCauseHasEmployeeRequest = 1603,
	
	// TimeRule
	TimeRuleIsUsed = 1701,
	
	// TimeWorkAccount
	TimeWorkAccountHasYears = 1801,
	TimeWorkAccountHasEmployees = 1802,
	TimeWorkAccountYearHasEmployees = 1803,
	
	// AnnualLeave
	AnnualLeaveGroupHasEmployments = 1901,
}

export enum ActionResultSave {
	// Common errors
	Unknown = 0,
	EntityIsNull = 1,
	EntityNotFound = 2,
	EntityNotActive = 3,
	NothingSaved = 4,
	EntityNotUpdated = 5,
	EntityNotCreated = 6,
	MaximumLengthExceed = 7,
	InsufficientInput = 8,
	Duplicate = 9,
	IncorrectInput = 10,
	EntityExists = 11,
	PartylySaved = 12,
	InsufficienPermissionToSave = 13,
	NotSupported = 14,
	CompletedWithWarnings = 15,
	Locked = 16,
	NumberExists = 17,
	
	// Entity specific errors
	
	// Customer
	CustomerExists = 20,
	CustomerNotSaved = 21,
	CustomerNotUpdated = 22,
	CustomerContactsAndTeleComNotSaved = 23,
	CustomerAccountsNotSaved = 24,
	CustomerCompanyCategoryNotSaved = 25,
	CustomerIsBlocked = 26,
	
	// License/Company/Permission
	SupportLicenseAlreadyExists = 30,
	PermissionCantAddReadIfModifyExist = 31,
	CompanyExists = 32,
	CompanyCannotBeAddedLicenseViolation = 33,
	
	// Account
	AccountDimRuleNotFulfilled = 40,
	AccountYearNotOpen = 41,
	AccountPeriodNotOpen = 42,
	AccountBalanceExists = 43,
	VoucherExists = 44,
	AccountYearVoucherDateDoNotMatch = 45,
	AccountYearNotFound = 46,
	AccountPeriodNotFound = 47,
	
	// Supplier
	SupplierExists = 50,
	SupplierNotSaved = 51,
	SupplierNotUpdated = 52,
	SupplierContactsAndTeleComNotSaved = 53,
	SupplierAccountsNotSaved = 54,
	SupplierCompanyCategoryNotSaved = 55,
	SupplierPaymentInformationNotSaved = 56,
	
	// Account
	AccountExist = 60,
	
	// AccountDim
	ProjectAccountDimExists = 65,
	ShiftTypeAccountDimExists = 66,
	
	// AddForm
	FormExist = 70,
	FieldExist = 71,
	
	// Origin state transitions
	InvalidStateTransition = 80,
	NoItemsToProcess = 81,
	RowsHasInsufficientInformation = 82,
	
	// Payment
	PaymentFileBgMaxFailed = 90,
	PaymentFileLbFailed = 91,
	PaymentFilePgFailed = 92,
	PaymentIncorrectDateAccountYear = 93,
	PaymentNotFound = 94,
	PaymentIncorrectAmount = 95,
	PaymentInvoiceAssetRowNotFound = 96,
	PaymentSysPaymentTypeMissing = 97,
	PaymentIncorrectPaymentType = 98,
	PaymentFilePaymentInformationMissing = 99,
	PaymentIncorrectDateAccountPeriod = 100,
	PaymentSuggestionExistsOnCompSettDisabled = 101,
	PaymentNrCannotBeEmpty = 102,
	PaymentInformationRowUsedByPaymentMethod = 103,
	PaymentIncorrectCreditAmount = 104,
	AccountCustomerOverpayNotSpecified = 105,
	
	// PriceListType
	//PriceListTypeExists = 110,
	
	// User
	UserInvalidUserName = 120,
	UserDefaultCompanyNotFound = 121,
	UserDefaultRoleNotFound = 122,
	UserInvalidOldPassword = 123,
	UserNewPasswordSameAsOld = 124,
	UserCannotBeAddedLicenseViolation = 125,
	
	// Reminder and Interest
	CustomerInvoiceReminderProductNotFound = 131,
	CustomerInvoiceInterestProductNotFound = 132,
	CustomerInvoiceReminderProductAccountSalesNotFound = 133,
	CustomerInvoiceReminderProductAccountPurchaseNotFound = 134,
	CustomerInvoiceInterestProductAccountSalesNotFound = 135,
	CustomerInvoiceInterestProductAccountPurchaseNotFound = 136,
	CustomerInvoiceFailedCopy = 137,
	CustomerInvoiceRowFailedCreate = 138,
	CustomerInvoiceNumberExists = 139,
	CustomerInvoiceInterestNotFullyPayed = 140,
	CustomerInvoiceInterestBelowAccumulatedBeforeInvoice = 141,
	CustomerInvoiceInterestAlreadyExists = 142,
	CustomerInvoiceReminderIsFullyPayed = 143,
	CustomerInvoiceReminderDateToNearAfterPrevClaim = 143,
	CustomerInvoiceReminderNotActivated = 144,
	CustomerInvoiceReminderCannotSetReminderLevelInvoiceNotDraft = 145,
	CustomerInvoiceIsTemplate = 146,
	
	// ProductGroup
	ProductGroupExists = 150,
	
	// ContactPerson
	ContactPersonSaved = 160,
	ContactPersonSavedWithErrors = 161,
	ContactPersonNotSaved = 162,
	ContactPersonUpdated = 163,
	ContactPersonUpdatedWithErrors = 164,
	ContactPersonNotUpdated = 165,
	ContactPersonsMappedWithErrors = 166,
	
	// TimeCode
	TimeCodeExist = 170,
	TimeCodeSaved = 171,
	TimeCodeNotSaved = 172,
	TimeCodeUpdated = 173,
	TimeCodeNotUpdated = 174,
	TimeCodeSavedWithProductErrors = 175,
	TimeCodeUpdatedWithProductErrors = 176,
	TimeCodeMandatoryPayrollProduct = 177,
	TimeCodeMaterialStandardMissing = 178,
	TimeCodeAbsenceCannotHaveMultipleSysPayrollTypeLevel3 = 179,
	
	// TimeCodeBreak
	TimeCodeBreakMandatoryStartAndStopType = 180,
	TimeCodeBreakEmployeeGroupMappedToOtherBreakWithSameBreakGroup = 181,
	
	// Transfer Offer/Order to/Order/Invoice
	TransferOfferOrderInvalidAttestStateInitial = 191,
	TransferOfferOrderInvalidAttestStateTransferred = 192,
	TransferOfferOrderInvalidAttestTransitions = 193,
	TransferOfferOrderHasInvalidatedItems = 194,
	TransferOfferOrderHasDifferentVatTypes = 195,
	
	// DayType
	DayTypeExists = 200,
	
	// Holiday
	HolidayExists = 205,
	
	// TimeRule
	TimeRuleNameInUse = 210,
	TimeRuleTimeCodeCannotBeUsedInBalanceOperand = 211,
	
	// Salary export
	TimeSalaryExportTransactionUpdateFailed = 220,
	
	// AttestState
	DuplicateInitialState = 230,
	TimeInvoiceTransactionCannotDeleteNotInitialAttestState = 231,
	TimePayrollTransactionCannotDeleteNotInitialAttestState = 232,
	TimePayrollTransactionCannotDeleteReversedTransactions = 233,
	TimePayrollTransactionCannotDeleteIsPayroll = 234,
	HiddenAttestStateMissing = 235,
	CannotSetAttestState = 236,
	
	// EmployeeSchedule
	EmployeeScheduleStopDateEarlierThanStartDate = 240,
	EmployeeScheduleOverlappingDates = 241,
	EmployeeScheduleStartDateMissing = 242,
	EmployeeScheduleCannotExtendUnplaced = 243,
	EmployeeScheduleCantBeRemoved = 244,
	EmployeeScheduleStopDateCannotBeAfterEmploymentStopDate = 245,
	EmployeeScheduleStopDateCannotBeAfterTemplateStopDate = 246,
	EmployeeScheduleStartDateCannotbeBeforeTemplateStartDate = 247,
	EmployeeScheduleTemplateHeadsWithSameStartDate = 248,
	EmployeeScheduleNothingPlaced = 249,
	
	// TimePeriod
	TimePeriodHeadNameMandatory = 250,
	TimePeriodHeadTypeMandatory = 251,
	TimePeriodEmployeeNotPlaced = 252,
	TimePeriodIsLocked = 253,
	TimePeriodHasTransactions = 253,
	TimePeriodHasEmployeeGroupRuleWork = 254,
	TimePeriodHasEmployeeTimePeriod = 255,
	TimePeriodHeadIsParent = 256,
	
	// User
	UserExists = 260,
	UserMandatoryCompanyAndRole = 261,
	UserPasswordDontMatch = 262,
	UserPasswordNotStrong = 263,
	UserLicenseViolation = 264,
	UserSaved = 265,
	UserSavedWithErrors = 266,
	UserNotSaved = 267,
	UserUpdated = 268,
	UserNotUpdated = 269,
	
	// SaveAttestDeviations
	SaveAttestDeviationsTimeBlocksFailed = 270,
	SaveAttestDeviationsRulesFailed = 271,
	
	// SaveAttest
	SaveAttestNoAttestTransitions = 280,
	SaveAttestNoValidAttestTransitionsAttestRole = 281,
	SaveAttestNoValidAttestTransitionsEmployeeGroup = 282,
	SaveAttestNoValidAttestTransition = 237,
	SaveAttestDuplicateTimeBlocks = 238,
	
	// Product
	ProductExists = 290,
	ProductNotSaved = 291,
	ProductPriceListNotSaved = 292,
	ProductCategoriesNotSaved = 293,
	ProductAccountsNotSaved = 294,
	ProductNotFound = 295,
	ProductWithSysPayrollTypeCannotBeDuplicate = 296,
	ProductSettingNotSaved = 297,
	ProductChainWronglyConfigured = 298,
	ProductInUse = 299,
	ProductWeightInvalid = 300,
	ProductUnitConversionNotSaved = 351,
	
	// EDI
	EdiInvalidType = 301,
	EdiInvalidUri = 302,
	EdiFailedFileListing = 303,
	EdiFailedParse = 304,
	EdiFailedUnknown = 305,
	EdiUnknownSourceType = 306,
	EdiFailedTransferEntityNotFound = 307,
	EdiFailedTransferToInvoiceInvalidStatus = 308,
	EdiFailedTransferToInvoiceInvalidData = 309,
	EdiFailedTransferToOrderInvalidStatus = 310,
	EdiOrderNotAcceptingEdiTransfer = 311,
	
	// Voucher
	VatVoucherExists = 311,
	HasUnbalancedAccountingRows = 312,
	
	// SysPosition
	SysPositionExists = 321,
	PositionExists = 322,
	
	// Category
	CategoryExists = 331,
	CategoryInvalidChain = 332,
	
	// GrossProfit
	GrossProfitCodeExists = 350,
	
	// TimeStamp
	TimeStampFailedToUpdateStatuses = 401,
	EmployeeNotValidForTerminal = 402,
	EmployeeHasNoGroup = 403,
	EmployeeGroupNotValidForTerminal = 404,
	EmployeeNrNotFound = 405,
	CardNumberExistsOnAnotherEmployee = 406,
	EmployeeHasAnotherCardNumber = 407,
	
	// Scanning
	ScanningFailedParse = 501,
	ScanningFailedSave = 502,
	ScanningFailedNoArrivalFound = 503,
	ScanningFailedUnknown = 504,
	
	ScanningFailed_ParseRawFile = 505,
	ScanningFailed_UploadToDataStorage = 506,
	ScanningFailed_CreateScanningEntry = 507,
	ScanningFailed_UploadToProvider = 508,
	ScanningFailed_HandleProviderResponse = 509,
	ScanningFailed_ExtractInterpretationAll = 510,
	ScanningFailed_ExtractInterpretationPartial = 511,
	ScanningFailed_NotActivatedAtProvider = 512,
	
	// Budget and distribution
	DistributionCodeInUse = 550,
	
	// Project
	ProjectAccountsNotSaved = 601,
	ProjectTransactionsNotCreated = 602,
	ProjectCategoryNotSaved = 603,
	
	// Customer Invoice
	NegativeTransferAmount = 650,
	
	// Inventory
	InventoryExists = 701,
	InventoryNotSaved = 702,
	InventoryNotUpdated = 703,
	InventoryAccountsNotSaved = 704,
	InventoryCompanyCategoryNotSaved = 705,
	InventoryDoesNotExist = 706,
	
	// Inventory WriteOffMethod
	InventoryWriteOffMethodNotSaved = 751,
	InventoryWriteOffMethodNotUpdated = 752,
	
	// Inventory WriteOffTemplate
	InventoryWriteOffTemplateNotSaved = 761,
	InventoryWriteOffTemplateNotUpdated = 762,
	
	// Inventory Log
	InventoryLogNotSaved = 771,
	
	// TFS
	TFSUnavailable = 801,
	
	// SysPayrollPrice
	SysPayrollPriceLastCannotBeDeleted = 851,
	SysPayrollPriceCannotHaveSameFromDate = 852,
	
	// Payment cont.
	PaymentInvalidType = 900,
	PaymentInvalidAccountNumber = 901,
	PaymentCountryMissing = 902,
	PaymentCountryCodeMissing = 903,
	PaymentInvalidData = 904,
	PaymentInvalidBICIBAN = 905,
	PaymentsNotExported = 906,
	
	// TimeEngineManager
	TimeEngineAttestTimeStampEntryStatusError = 1001,
	TimeEngineReverseTransactionsError = 1002,
	
	// TimeSchedulePlanning
	TimeSchedulePlanning_ShiftIsNull = 1101,
	TimeSchedulePlanning_UserNotFound = 1102,
	TimeSchedulePlanning_HiddenEmployeeNotFound = 1103,
	TimeSchedulePlanning_PeriodNotFound = 1104,
	TimeSchedulePlanning_EmployeePeriodCouldNotBeCreated = 1105,
	TimeSchedulePlanning_PreliminaryNotUpdated = 1106,
	TimeSchedulePlanning_MandatoryAccountMissing = 1107,
	TimeSchedulePlanning_AccountNotValidForEmployee = 1108,
	
	// Handle shifts
	HandleTimeScheduleShift_ShiftAssignedOK = 1121,
	HandleTimeScheduleShift_EmployeeQueued = 1122,
	HandleTimeScheduleShift_EmployeeAlreadyInQueue = 1123,
	HandleTimeScheduleShift_EmployeeRemovedFromQueue = 1124,
	HandleTimeScheduleShift_ShiftUnwantedOK = 1125,
	HandleTimeScheduleShift_ShiftUndoUnwantedOK = 1126,
	HandleTimeScheduleShift_ShiftAbsenceQueued = 1127,
	HandleTimeScheduleShift_ShiftUndoAbsenceOK = 1128,
	HandleTimeScheduleShift_ShiftUndoAbsenceNotFound = 1129,
	HandleTimeScheduleShift_ShiftUndoAbsenceNotPending = 1130,
	HandleTimeScheduleShift_EmployeeChangedOK = 1131,
	HandleTimeScheduleShift_EmployeeChangeQueued = 1132,
	HandleTimeScheduleShift_EmployeeNotShiftOwner = 1133,
	HandleTimeScheduleShift_EmployeeSwappedOK = 1134,
	HandleTimeScheduleShift_EmployeeSwapQueued = 1135,
	HandleTimeScheduleShift_AbsenceAnnouncementOK = 1136,
	HandleTimeScheduleShift_AbsenceAnnouncementOKXEmailSentWithErrors = 1137,
	
	// Split shifts
	SplitTimeScheduleShift_ShiftsSplittedOK = 1141,
	
	// Communication
	Communication_ObjectIsNull = 1201,
	Communication_ShiftIsPassed = 1202,
	
	// TimeScheduleTemplate
	TimeScheduleTemplateExistsCannotChangePeriods = 1301,
	TimeScheduleTemplateEmployeeStartDateMandatory = 1302,
	TimeScheduleTemplateNotDeletedEmployeeScheduleExists = 1303,
	TimeScheduleTemplateEmployeeTemplateExists = 1304,
	TimeScheduleTemplateEmployeeTemplateOverlapping = 1305,
	TimeScheduleTemplateEmployeePlacementsOutOfRange = 1306,
	TimeScheduleTemplateEmployeeStartsFirstDayOnWeekInvalid = 1307,
	TimeScheduleTemplateEmployeeGroupDoesNotAllowShiftsWithoutAccount = 1308,
	
	// SaveTemplateSchedule
	SaveTemplateSchedule_TemplateSaved = 1351,
	SaveTemplateSchedule_PlacementNotValid = 1352,
	SaveTemplateSchedule_PlacementAlreadyInitiated = 1353,
	
	// Concurrency check
	EntityIsModifiedByOtherUser = 1401,
	
	// EmployeeUser
	EmployeeUserContactsAndTeleComNotSaved = 1501,
	EmployeeUserCategoryNotSaved = 1502,
	EmployeeUserMandatoryFieldsMissing = 1503,
	EmployeeUserSkillsNotSaved = 1504,
	EmployeeUserPositionsNotSaved = 1505,
	EmployeeUserCannotDeleteSysUser = 1506,
	EmployeeUserAttestReplacementNotSaved = 1507,
	EmployeeUserAttestContactPersonNotFound = 1508,
	EmploymentMandatory = 1509,
	EmployeeEmploymentsNotSaved = 1510,
	EmployeeCategoriesMandatory = 1511,
	EmployeeUserChildsNotSaved = 1512,
	EmployeeUserCannotInactivateWhenScheduledPlacementExists = 1513,
	EmployeeMeetingsNotSaved = 1514,
	EmployeeUnionFeesNotSaved = 1515,
	EmploymentFinalSalaryIsApplied = 1516,
	EmployeeEmploymentsInvalidFixedTerm14days = 1517,
	EmployeeEmploymentsInvalidWorkTimeWeek = 1518,
	EmployeeCategoriesEndingBeforeLastEmployment = 1519,
	EmployeeChildBirthDateMissing = 1520,
	EmployeeUserRolesNotSaved = 1521,
	EmployeeUserAttestRolesNotSaved = 1522,
	EmployeeUserDefaultRoleMandatory = 1523,
	EmploymentVacationGroupsCannotBeDuplicate = 1524,
	EmployeeTemplateGroupsNotSaved = 1525,
	EmployeeTimeWorkAccountsNotSaved = 1526,
	EmployeeDateToCannotBeShortendWhenChangingToNotTemporary = 1527,
	EmployeeSocialSecNotAllowedAccordingToCompanySetting = 1528,
	EmploymentFinalSalaryATKFailed = 1529,
	EmployeeGroupMandatory = 1530,
	
	// EmployeeRequest
	EmployeeRequestIntersectsWithExisting = 1531,
	
	// Cancel Payment
	CancelPaymentFailedTransferredToVoucher = 1601,
	CancelPaymentFailedAlreadyCancelled = 1602,
	
	// SMS
	SendSMSNoSender = 1701,
	SendSMSNoReceivers = 1702,
	SendSMSNoMessage = 1703,
	SendSMSResponseIsNull = 1704,
	SendSMSResponseReturnedFailed = 1705,
	SMSNoProviderExists = 1706,
	AreaCodeNotFound = 1707,
	SMSInsuficientData = 1708,
	SMSCompanyLimitReached = 1709,
	
	//Work
	RuleWorkTimeWeekMaxMinReached = 1801,
	RuleRestDayReached = 1802,
	RuleShiftsOverlap = 1803,
	RuleContainsAttestedTransactions = 1804,
	RuleRestWeekReached = 1805,
	RuleMinorsWorkingHoursViolated = 1806,
	RuleMinorsWorkTimeDayReached = 1807,
	RuleSummerHolidayViolated = 1808,
	RuleMinorsBreaksViolated = 1809,
	RuleMinorsWorkTimeWeekReached = 1810,
	RuleMinorsRestDayReached = 1811,
	RuleMinorsRestWeekReached = 1812,
	RuleMinorsWorkAloneViolated = 1813,
	RuleMinorsHandlingMoneyViolated = 1814,
	RuleWorkTimeDayMinimumNotReached = 1815,
	ScheduleCycleRuleMinOccurrencesNotReached = 1816,
	ScheduleCycleRuleMaxOccurrencesViolated = 1817,
	RuleWorkTimeWeekForParttimeWorkersViolated = 1818,
	RuleBreaksViolated = 1819,
	RuleWorkTimeDayMaximumWorkDayViolated = 1820,
	RuleWorkTimeDayMaximumWeekendViolated = 1821,
	RuleDayIsLocked = 1822,
	RuleScheduleFreeWeekendsMinimumYear = 1823,
	RuleScheduledDaysMaximumWeek = 1824,
	RuleCoherentSheduleTimeViolated = 1825,
	RuleShiftRequestPreventTooEarlyWarn = 1826,
	RuleShiftRequestPreventTooEarlyStop = 1827,
	RuleLeisureCodes = 1828,
	RuleAnnualLeave = 1829,
	
	//Employee
	EmployeeNumberExists = 1901,
	EmployeeGroupAutogenTimeBlocksIsTrue = 1902,
	EmployeeCannotBeAddedLicenseViolation = 1903,
	EmploymentNotFound = 1904,
	EmployeeGroupNotFound = 1905,
	EmploymentVacationGroupInUse = 1906,
	EmployeeEmailMandatory = 1907,
	EmployeeOverlappingMainAffiliation = 1908,
	
	
	
	// Checklist
	//ChecklistHeadNameMandatory = 2001,
	//ChecklistHeadTypeMandatory = 2002,
	//ChecklistHeadRowMandatory = 2003,
	//ChecklistHeadIsUsed = 2004,
	//ChecklistRowIsUsed = 2005,
	
	// Currency
	CurrencyGetRatesFailed = 2101,
	
	// EmailTemplate
	EmailTemplateExists = 2201,
	
	//Mobile
	MobileOrderIllegalSave = 2301,
	MobileOrderCustomerBlocked = 2302,
	
	//Attest
	AttestLastEntryMustHavePropertyClosed = 2401,
	
	//Project
	ProjectRowsAmountInvalid = 2501,
	ProjectRowsAccountInvalid = 2502,
	ProjectRowsTimeCodeInvalid = 2503,
	ProjectRowsInvoiceProductInvalid = 2504,
	OneToOneRelationshipRequiredToUpdateProjectCustomer = 2505,
	
	//PushNotification
	PushNotificationNoReciever = 2601,
	PushNotificationNoMessage = 2602,
	PushNotificationSendFailed = 2603,
	PushNotificationReadFailed = 2604,
	
	//Dates
	DatesInvalid = 2701,
	DatesOverlapping = 2702,
	DatesMustHaveEmptyStartDate = 2703,
	DatesMustHaveEmptyStopDate = 2704,
	DatesMustBeConnected = 2705,
	
	//CopySchedule
	CopyScheduleInvalidDates = 2801,
	CopyScheduleHiddenEmployeeNotSupported = 2802,
	CopyScheduleSameEmployeeNotSupported = 2803,
	CopyScheduleSourceHasTemplateHeadsWithSameStartDate = 2804,
	CopyScheduleSourceHasAttestedTransactions = 2805,
	CopyScheduleTemplateTargetEndsInSourceDateRange = 2806,
	CopyScheduleTemplateTargetStopAfterSourceStart = 2807,
	CopyScheduleEmployeeSourceHasOverlappingPlacements = 2808,
	CopyScheduleEmployeeScheduleOverlappingDates = 2809,
	CopyScheduleEmployeeScheduleTemplateNotFound = 2810,
	CopyScheduleTargetSourceDifferentEmployeeGroup = 2811,
	
	//TimeScheduleType
	TimeScheduleTypeAllAlreadyExists = 2901,
	
	// ShiftType
	ShiftTypeCompanyCategoryNotSaved = 2911,
	ShiftTypeNeedsCodeInUse = 2912,
	ShiftTypeSimilarNeedsCodeExist = 2913,
	
	// PayrollPriceFormula
	PayrollPriceTypeOrFormulaCodeExists = 2921,
	
	// PayrollPriceType
	PayrollPriceTypeInUse = 2931,
	
	// PayrollCalculation
	PayrollCalculationMissingAccountEmployeeGroupCost = 2941,
	PayrollCalculationMissingAttestStateReg = 2942,
	PayrollCalculationMissingPayrollProductTableTax = 2943,
	PayrollCalculationMissingPayrollProductOneTimeTax = 2944,
	PayrollCalculationMissingPayrollProductEmploymentTax = 2945,
	PayrollCalculationMissingTimePeriodPaymentDate = 2946,
	PayrollCalculationPayrollTransactionCouldNotBeCreated = 2947,
	PayrollCalculationMissingAccountEmployeeGroupIncome = 2948,
	PayrollCalculationMissingAccountPayrollOwnSupplementCharge = 2949,
	PayrollCalculationMissingPayrollProductSupplementCharge = 2950,
	PayrollCalculationMissingPaymentDate = 2951,
	PayrollCalculationMissingPayrollProductNetSalary = 2952,
	PayrollCalculationMissingPayrollProductNetSalaryRound = 2953,
	PayrollCalculationMissingPayrollProductSINKTax = 2954,
	PayrollCalculationMissingPayrollProductASINKTax = 2955,
	
	//PayrollPeriodChange
	PayrollPeriodChangeLastPayrollPeriodLockChangeNotFound = 2991,
	
	//VacationGroups
	VacationGroupsFailedReport = 3001,
	
	// VacationYearEnd
	VacationYearEndAlreadyCreated = 3051,
	VacationYearEndFailed = 3052,
	VacationYearEndCreatedBeforeFinalSalary = 3053,
	
	// SysVehicleType
	SysVehicleTypeInvalidXMLYearNotFound = 3101,
	
	// AccountProvision
	AccountProvisionUnlockFailedAttestedTransactions = 4101,
	
	//PayrollStartValue
	PayrollStartValueTransactionsAlreadyCreated = 5101,
	PayrollStartValueCreateTransactionsFailed = 5102,
	PayrollStartValueRowDateIsOutsideHeadDateRange = 5103,
	
	//Translations
	TranslationsSaveFailed = 5200,
	
	//SchoolHoliday
	SchoolHolidaySummerAlreadyExists = 5301,
	SchoolHolidaySummerMustBeSameYear = 5302,
	
	//TimeBreakTemplate
	TimeBreakTemplateCannotDeleteUsed = 5401,
	TimeBreakTemplateMoreBreaksThanAllowedMajor = 5402,
	TimeBreakTemplateMoreBreaksThanAllowedMinor = 5403,
	TimeBreakTemplateMajorBreaksMissing = 5404,
	TimeBreakTemplateMinorBreaksMissing = 5405,
	TimeBreakTemplateNotValid = 5406,
	
	//TimeScheduleTemplateHead
	TimeScheduleTemplateHeadForEmployeePostAndEmployeeAlreadyExists = 5501,
	
	//PayrollGroup
	PayrollGroup_UsedInPayroll = 5601,
	
	//Retroactive Payroll
	RetroactivePayrollNotFound = 5601,
	RetroactivePayrollHasTransactions = 5601,
	
	//ReportPrintout
	ReportPrintoutWarning = 5701,
	ReportPrintoutError = 5702,
	PrintEmploymentContractFromTemplateFailed = 5703,
	
	// TimeDeviationCause
	TimeDeviationCause_TypeInvalidForStandby = 5801,
	
	// EmploymentHibernating
	TemporaryPrimaryEmploymentMustHaveDateFromAndDateTo = 6001,
	TemporaryPrimaryEmploymentMustHaveEmploymentToHibernateWholeInterval = 6002,
	TemporaryPrimaryAlreadyExistsInInterval = 6003,
	TemporaryPrimaryHibernatingHeadNotFound = 6004,
	TemporaryPrimaryCannotBeSecondary = 6005,
	TemporaryPrimaryExistsAttestedTransactions = 6006,
	TemporaryPrimaryExistsLockedTransactions = 6007,
	TemporaryPrimaryHasNoPerissionToCreateDeleteShortenExtend = 6008,
	
	// TimeWorkAccount
	TimeWorkAccountOverlapping = 6101,
	
	// PlannedPeriodCalculationCalculation
	PlannedPeriodCalculationCalculationSummaryFailed = 6201,
	
	// EmployeeGroup
	InvalidBreakSettings = 6301,
	StandardDeviationCauseMissing = 6302,
	StandardDeviationCauseNotInEmployeeGroup = 6303,
	EmployeeGroupDuplicate = 6404,
	
	// SignatoryContract
	SignatoryContractInvalidUser = 6501,
	SignatoryContractInvalidPermission = 6502,
	SignatoryContractInvalidAuthenticationMethodType = 6503,
	SignatoryContractDuplicateChildren = 6504,
	SignatoryContractSubContractInvalidPermission = 6505,
	SignatoryContractAddContractWithSubContracts = 6506,
	
	// ClientManagement
	ClientManagementNoConnectionCreationPermission = 6600,
}

export enum AngularJsLegacyType {
	None = 0,
	
	RightMenu = 10000,
	RightMenu_Information = 10100,
	RightMenu_Help = 10200,
	RightMenu_Message = 10300,
	RightMenu_Report = 10400,
	RightMenu_Document = 10500,
}

export enum TimeAttestMode {
	Time = 1,
	Project = 2,
	TimeUser = 3,
}

export enum AttestFlowType {
	//Type of required users (will be more in the future
	All = 0,
	Any = 1,
};

export enum AttestStatus_SupplierInvoice {
	//Status of the attestflow
	AttestFlowOnGoing = 0,
	AttestFlowApproved = 1,
	AttestFlowCanceled = 2,
};

export enum AttestFlow_ReplaceUserReason {
	Remove = 1,
	Transfer = 2,
	TransferWithReturn = 3
}

export enum SupplierInvoiceAccountRowAttestStatus {
	Unknown = 0,
	New = 1,
	Processed = 2,
	Deleted = 3
}

export enum SigneeStatus {
	Unknown = 0,
	Opened = 1,
	Rejected = 2,
	Signed = 3,
}

export enum AttestPeriodType {
	Unknown = 0,
	Day = 1,
	Week = 2,
	Month = 3,
	Period = 4,
};

export enum AltInnPeriodTypeEnum {
	Månedlig = 5,
	ToMånedlig = 6,
	Årlig = 7
}

export enum BalanceSubmitType {
	Unknown = 0,
	Save = 1,
	LoadBalance = 2,
};

export enum BalanceErrorStatus {
	Ok = 0,
	Unknown = 1,
	MissingAccountYear = 2,
	InvalidAccounts = 3,
	DuplicateAccounts = 4,
	Unbalanced = 5,
};

export enum DistributionCodeBudgetType {
	AccountingBudget = 1,
	TimeBudget = 2,
	ProjectBudget = 3,
	SalesBudget = 4,
	SalesBudgetTime = 5,
	SalesBudgetSalaryCost = 6,
	ProjectBudgetIB = 7,
	ProjectBudgetForecast = 8,
	ProjectBudgetChangeWork = 9,
	ProjectBudgetExtended = 10,
}

export enum BudgetHeadStatus {
	Preliminary = 1,
	Active = 2,
}

export enum BudgetRowPeriodType {
	None = 0,
	Year = 1,
	SixMonths = 2,
	Quarter = 3,
	Month = 4,
	Week = 5,
	Day = 6,
	Hour = 7,
}

export enum DatePeriodType {
	None = 0,
	Day = 1,
	Week = 2,
	Month = 3,
	Quarter = 4,
	HalfYear = 5,
	Year = 6
}

export enum BusinessCacheType {
	Unknown = 0,
	AllEmployees = 1,
	Employments = 2,
	EmployeeGroups = 3,
	PayrollGroups = 4,
	CategoryRecords = 5,
	EmployeeAccounts = 6,
	TimePayrollTransactions = 7,
	FeaturePermissions = 8,
	Holiday = 9,
	AccountDim = 10,
	AttestRoleUser = 11,
	TimeDeviationCause = 12,
	EmployeesWithRepository = 13,
	CategoryAccount = 14,
	EmploymentType = 15,
	AccountInternal = 16,
	TimeRules = 17,
	AccountHierarchyStrings = 18,
	ShiftTypes = 19,
	TimeScheduleType = 20,
	EmployeeChild = 21,
	HasTimeScheduletemplateGroups = 22,
	VacationGroups = 23,
	PayrollPriceTypes = 24,
	AttestRole = 25,
	UserRole = 26,
	ReportRolePermission = 27,
	AttestTransitionsForEmployeeGroup = 28,
	AttestTransitions = 29,
	HasPayrollImportHeads = 30,
	PayrollProductDTOs = 31,
	CompanySysCountryId = 32,
	PayrollPriceFormulas = 33,
	PayrollPriceTypeDTOs = 34,
	PayrollProduct = 35,
	SysTermWithDescription = 36,
	HiddenEmployeeId = 37,
	SysReportTemplatesWithTemplateType = 38,
	HasTimeValidRuleWorkTimePeriodSettings = 39,
	UseAccountHierarchy = 40,
	Positions = 41,
	AccountDimIdOnAccount = 42,
	StaffingNeedsFrequency = 43,
	ShiftTypeLink = 44,
	TimeCodeBreak = 45,
	HasEmployeeTemplates = 46,
	PayrollLevel = 47,
	UsesWeekendSalary = 48,
	HolidaySalaryHoliday = 49,
	EmployeeGroupsWithDayTypes = 50,
	IsAgreement100 = 51,
	PayrollProducts = 52,
	EndReason = 53,
	PayrollGroupPriceTypes = 54,
	PayrollProductPriceTypes = 55,
	AttestStatesTime = 56,
	AccountDimShiftType = 57,
	EmploymentPriceTypes = 58,
	EmployeeFactors = 59,
	TimeWorkAccount = 60,
	HasEventActivatedScheduledJob = 70,
	EmploymentPriceTypeDTOs = 71,
	PayrollProductReportSettings = 75,
	TimeCode = 76,
	DayType = 77,
	TimeScheduleTasks = 78,
	IncomingDeliveryHeads = 79,
	HasStaffingNeedsSetting = 80,
	LicenseById = 81,
	Company = 82,
	CompaniesOnLicense = 83,
	LicenseByNr = 84,
	UserCompanyRole = 85,
	UsePayroll = 86,
	DefaultEmployeeAccountDim = 87,
	HasCalculatePayrollOnChanges = 88,
	HasTimeLeisureCodes = 89,
	AnnualLeaveGroups = 90,
	UseTimeWorkReduction = 91,
	
	PersonalDataRepoEmployeeGroups = 101,
	PersonalDataRepoPayrollGroups = 102,
	PersonalDataRepoPayrollPriceTypes = 103,
	ExtraField = 104,
	PersonalDataRepoAnnualLeaveGroups = 105,
	
	MiscWithKey = 500,
	Employers = 501,
	EmployeeEmployers = 502,
}

export enum CalculationPeriodItemGroupByType {
	Unknown = 0,
	NoGrouping = 1,
	ShiftType = 2,
	Skills = 3,
	ShiftTypeLink = 4,
}

export enum CaseProjectNoteType {
	Unknown = 0,
	ActionPerformed = 1,
	ExpectedResult = 2,
	ActualResult = 3,
	Answer = 4,
	Internal = 5,
	Question = 6,
	FeatureRequest = 7
}

export enum CaseProjectDisplayMode {
	User = 1,
	Admin = 2
}

export enum CaseProjectGridType {
	New = 0,
	My = 1,
	Open = 2,
	Closed = 3,
}

export enum ClientManagementResourceType {
	GetSupplierInvoices = 1,
	GetSupplierInvoicesSummary = 2
}

export enum PostChange {
	Unknown = 0,
	Update = 1,
	Insert = 2,
	Delete = 3,
}

export enum SoeFileType {
	Unknown = 0,
	Pdf = 1,
	Xml = 2,
	Txt = 3,
	Excel = 4,
	Image = 5,
	Zip = 6
}

export enum CompTermsRecordType {
	Unknown = 0,
	ProductName = 1,
	ProductUnitName = 2,
	ReportName = 3,
	AccountName = 4,
	Textblock = 5,
	AccountDimName = 7,
	ExtraField = 8,
}

export enum CompanyState {
	Unknown = -1,
	Active = 0,
	Inactive = 1,
	Deleted = 2,
	Temporary = 3,
	SOPCustomer = 4,
}

export enum TemplateCompanyCopy {
	All = 1,
	
	//Core
	RolesAndFeatures = 101,
	CompanyFieldSettings = 102,
	
	//Settings
	ManageSettings = 201,
	AccountingSettings = 202,
	SupplierSettings = 203,
	CustomerSettings = 204,
	BillingSettings = 205,
	TimeSettings = 206,
	PayrollSettings = 207,
	ProjectSettings = 208,
	SigningSettings = 209,
	
	//Reports
	ReportSettings = 301,
	ReportSelections = 302,
	ReportsAndReportTemplates = 303,
	ReportGroupsAndReportHeaders = 304,
	
	//Economy
	CompanyAttestSupplier = 401,
	AccountStds = 403,
	AccountInternals = 404,
	AccountYearsAndPeriods = 405,
	VoucherSeriesTypes = 406,
	PaymentMethods = 407,
	PaymentConditions = 408,
	GrossProfitCodes = 409,
	Inventory = 410,
	AutomaticAccountDistributionTemplates = 411,
	PeriodAccountDistributionTemplates = 412,
	DistributionCodes = 413,
	VoucherTemplates = 414,
	ResidualCodes = 415,
	Suppliers = 416,
	
	//Billing
	CompanyAttestBilling = 501,
	BaseProductsBilling = 502,
	CompanyProducts = 503,
	PricesLists = 504,
	SupplierAgreements = 505,
	Checklists = 506,
	EmailTemplates = 507,
	VatCodes = 508,
	CompanyWholesellerPricelists = 509,
	Customers = 510,
	Contracts = 511,
	Orders = 512,
	OrderTemplates = 513,
	PriceRules = 514,
	CompanyExternalProducts = 515,
	
	//Time
	CompanyAttestTime = 601,
	BaseAccountsTime = 602,
	EmployeeGroups = 603,
	PayrollProductsAndTimeCodes = 604,
	DeviationCauses = 605,
	DaytypesHalfDaysAndHolidays = 606,
	TimePeriods = 607,
	Positions = 608,
	VacationGroups = 609,
	TimeScheduleTypesAndShiftTypes = 610,
	Skills = 611,
	ScheduleCykles = 612,
	PayrollGroupsPriceTypesAndPriceFormulas = 613,
	FollowUpTypes = 614,
	TimeAccumulators = 615,
	TimeRules = 616,
	TimeAbsenseRules = 617,
	TimeAttestRules = 618,
	TimeBreakTemplate = 619,
	EmploymentTypes = 620,
}

export enum ChildCopyItemRequestType {
	None = 0,
	TimeDeviationCause = 100,
	TimeCodes = 101,
	PriceTypes = 102,
	PriceFormulas = 103,
}

export enum CompanyGroupTransferType {
	None = 0,
	Consolidation = 1,
	Budget = 2,
	Balance = 3,
}

export enum CompanyGroupTransferStatus {
	None = 0,
	Transfered = 1,
	PartlyDeleted = 2,
	Deleted = 3,
}

export enum DashboardStatisticsType {
	PerformanceAnalyzer = 0,
}

export enum DashboardStatisticsRowType {
	TimeValue = 0,
}

export enum DashboardStatisticsPeriodType {
	DecimalValue = 0,
	intvalue = 1,
}

export enum DailyRecurrencePatternType {
	None = 0,               // None
	Daily = 1,              // Every day
	Weekly = 2,             // Every week
	AbsoluteMonthly = 3,    // Same day every month
	RelativeMonthly = 4,    // Same week every month
	AbsoluteYearly = 5,     // Same day every year
	RelativeYearly = 6,     // Same week every year
	SysHoliday = 7          // System holiday
}

export enum DailyRecurrencePatternWeekIndex {
	First = 0,
	Second = 1,
	Third = 2,
	Fourth = 3,
	Last = 4
}

export enum DailyRecurrenceRangeType {
	EndDate = 0,    // End by specific date
	NoEnd = 1,      // No end date
	Numbered = 2    // End after specific number of occurrences
}

export enum TextBlockDictType {
	Unknown = 0,
	Task = 1,
	Where = 2,
	How = 3,
}

export enum EdiRecivedMsgState {
	Unknown = 0,
	UnderProgress = 1,
	Transferred = 2,
	Retry = 3,
	Deleted = 4,
	Duplicate = 7,
	
	Error = 10,
	ErrorFailedToParseFiles = 11,
	ErrorCouldNotRemoveFromFtp = 12,
}

export enum EdiTransferState {
	Unknown = 0,
	UnderProgress = 1,
	Transferred = 2,
	Retry = 3,
	Deleted = 4,
	EdiCompNotFoundTryingOtherEnv = 5,
	NoSoftOneCustomer = 6,
	Duplicate = 7,
	TooManySoftOneCustomers = 8,
	
	// Errorstates > 9
	Error = 10,
	EdiCompanyNotFound = 11,
}

export enum SoeEDIClassificationGroupStatus {
	Unknown = 0,
	InProgress = 1,
	Done = 2,
	Deleted = 3,
}

export enum EdiUrlSource {
	Unknown = 0,
	Web = 1,
	FTP = 2,
	Rest = 3,
}

export enum EdiImportSource {
	Undefined = 0,
	EDI = 1,
	ReadSoft = 2,
	Inexchange = 3,
	BankIntegration = 4,
	FileImport = 5,
}

export enum EmailTemplateType {
	Invoice = 0,
	Reminder = 1,
	PurchaseOrder = 2,
}

export enum EmployeeFactorDisplayMode {
	Absence = 1,
	Vacation = 2,
}

export enum EmployeePostSortType {
	Unknown = 0,
	Uniques = 1,
	CompareScore = 2,
	NumberOfPossibleWeeks = 3,
	WorkTimePerDayDescending = 4,
	WorkTimePercentDescending = 5,
	DayOfWeeksCount = 6,
	NbrOfWeeks = 7,
	NameWorkTimePercentDescending = 8,
	NbrOfWeeksDecending = 9,
}

export enum EmployeeUserEditDisplayMode {
	Employee = 1,
	User = 2
}

export enum SaveEmployeeUserResult {
	ActorContactPersonId = 1,
	UserId = 2,
	EmployeeId = 3,
}

export enum DeleteEmployeeAction {
	Cancel = 0,
	Inactivate = 1,
	RemoveInfo = 2,
	Unidentify = 3,
	Delete = 4
}

export enum DeleteUserAction {
	Cancel = 0,
	Inactivate = 1,
	RemoveInfo = 2,
	Unidentify = 3,
	Delete = 4
}

export enum AccountingRowType {
	AccountingRow = 0,
	SupplierInvoiceAttestRow = 1,
}

export enum PurchaseRowType {
	Unknown = 0,
	PurchaseRow = 1,
	TextRow = 2,
}

export enum SupplierInvoiceRowType {
	Unknown = 0,
	ProductRow = 1,
	TextRow = 2,
}

export enum SupplierInvoiceCostLinkType {
	Undefined = 0,
	OrderRow = 1,
	ProjectRow = 2,
}

export enum SoeAccountDistributionType {
	Auto = 1,
	Period = 2,
	Inventory_Purchase = 3,       // Inköp
	Inventory_WriteOff = 4,       // Avskrivning
	Inventory_OverWriteOff = 5,   // Överavskrivning
	Inventory_UnderWriteOff = 6,  // Underavskrivning
	Inventory_WriteUp = 7,        // Uppskrivning
	Inventory_WriteDown = 8,      // Nerskrivning
	Inventory_Discarded = 9,      // Utrangering
	Inventory_Sold = 10,          // Försäljning
}

export enum SoeActorType {
	Company = 0,
	Customer = 1,
	Supplier = 2,
	ContactPerson = 3,
};

export enum SoeCategoryType {
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
	PayrollProduct = 11,
	Dokument = 12,
};

export enum SoeCategoryRecordEntity {
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
	Order = 13,
}

export enum SoeInvoiceType {
	Unknown = 0,
	SupplierInvoice = 1,
	CustomerInvoice = 2
};

export enum SoeInvoiceDeliveryType {
	Unknown = 0,
	Print = 1,
	Email = 2,
	Electronic = 3,
	//InexchangeAPI = 4,
	EDI = 5
}

export enum SoeInvoiceDeliveryProvider {
	Unknown = 0,
	Inexchange = 1,
	Intrum = 2,
}

export enum SoeInvoicePaymentServiceType {
	Unknown = 0,
	Autogiro = 1,
}

export enum SoeProjectRangeType {
	Unknown = 0,
	External = 1,
	Internal = 2,
}

export enum SoeOriginType {
	//OBS! Changes here also affects SQL-views!
	
	None = 0,
	SupplierInvoice = 1,
	CustomerInvoice = 2,
	SupplierPayment = 3,
	CustomerPayment = 4,
	Offer = 5,
	Order = 6,
	Contract = 7,
	Purchase = 8
};

export enum SoeOriginInvoiceMappingType {
	//OBS! Changes here also affects SQL-views!
	
	None = 0,
	SupplierInvoice = 1,
	CustomerInvoice = 2,    // Ledger
	SupplierPayment = 3,
	CustomerPayment = 4,
	DebitInvoice = 5,
	CreditInvoice = 6,
	InterestInvoice = 7,
	Offer = 8,
	Order = 9,
	Contract = 10,
	ClaimInvoice = 11,
};

export enum SoeEntityType {
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
	EmployeePostSkill = 37,
	ContactPerson = 38,
	HouseholdTaxDeductionApplicant = 39,
	XEConnectImport = 40,
	TimeRule = 41,
	UserCompanySetting_Application = 42,
	UserCompanySetting_License = 43,
	UserCompanySetting_Company = 44,
	UserCompanySetting_UserAndCompany = 45,
	UserCompanySetting_User = 46,
	EventHistory = 47,
	Account = 48,
	FileIdentifier = 49,
	PaymentInformationRow = 50,
	UserCompanyRole = 51,
	AttestRoleUser = 52,
	RoleFeature = 53,
	AttestRole = 54,
	AttestRole_AttestTransition = 55,
	AttestRole_PrimaryCategory = 56,
	AttestRole_SecondaryCategory = 57,
	EmployeeAccount = 58,
	Expense = 59,
	EmployeeRequest_Availability = 60,
	InvoiceProduct = 61,
	DataStorageRecord = 62,
	Company = 63,
	SupplierProduct = 64,
	Purchase = 65,
	AccountDim = 66,
	PayrollProduct = 67,
	TimeWorkAccountYearEmployee = 68,
	EmployeeSetting = 69,
	TimeStampEntry = 70,
	ContactAddress = 71,
	ContactECom = 72,
	TimeStampEntryExtended = 73,
	EdiEntry = 74,
	ScanningEntry = 75,
	CustomerCentral = 76,
	PayrollProductSetting = 77,
	
	// Important!!!
	// When adding entities here, remember to att terms in database to TermGroup 549
};

export enum SoeEntityImageType {
	Unknown = 0,
	Inventory = 1,
	EmployeePortrait = 2,
	OrderInvoice = 3,
	OrderInvoiceSignature = 4,
	ChecklistHeadRecord = 5, //future use
	ChecklistHeadRecordSignature = 6,
	CaseProjectScreenShot = 7,
	ChecklistHeadRecordSignatureExecutor = 8,
	SupplierInvoice = 9,
	Customer = 10,
	ReportTemplate = 11,
	EmployeeFile = 12,
	Expense = 13,
	XEMailAttachment = 14,
}

export enum SoeProductType {
	Unknown = 0,
	InvoiceProduct = 1,
	PayrollProduct = 2,
};

export enum ExternalProductType {
	Unknown = 0,
	Electric = 1,
	Plumbing = 2,
	LockSmith = 3,
	Ahlsell = 4,
	BadVarme = 5,
	Bevego = 6,
	Comfort = 7,
	Lindab = 8,
}

export enum StaffingNeedsFrequencyType {
	Unknown = 0,
	NbrOfItems = 1,
	NbrOfCustomers = 2,
	Amount = 3,
}

export enum FrequencyType {
	Actual = 0,
	Budget = 1,
	Forecast = 2,
	StaffingNeedsRow = 3,
}

export enum SupplierInvoiceOrderLinkType {
	LinkToOrder = 0,
	LinkToProject = 1,
	Transfered = 2
}

export enum SoeEntityState {
	Active = 0,
	Inactive = 1,
	Deleted = 2,
	Temporary = 3,
	Hidden = 4
};

export enum SoeEntityStateTransition {
	None = 0,
	ActiveToInactive = 1,
	ActiveToDeleted = 2,
	InactiveToActive = 3,
	DeletedToActive = 4,
	ChangeLoginName = 5,
}

export enum SoeHouseholdClassificationGroup {
	None = 0,
	Apply = 1,
	Applied = 2,
	Received = 3,
	Denied = 4,
	All = 5
}

export enum SoeOriginStatus {
	//OBS! Changes here also affects SQL-views and SysTerms!
	
	None = 0,
	
	//General 1-10
	Draft = 1,
	Origin = 2,
	Voucher = 3,
	Cancel = 4,
	Export = 5,
	
	//SupplierInvoice 11-20
	
	//CustomerInvoice 21-30
	
	//Payment 31-40
	Payment = 31,
	Matched = 32,
	
	//Offer 41-50
	OfferPartlyOrder = 41,
	OfferFullyOrder = 42,
	OfferPartlyInvoice = 43,
	OfferFullyInvoice = 44,
	OfferClosed = 45,
	
	//Order 51-60
	OrderPartlyInvoice = 51,
	OrderFullyInvoice = 52,
	OrderClosed = 53,
	
	//Contract 61-69
	ContractClosed = 61,
	
	//Purchase
	PurchaseDone = 70,
	PurchaseSent = 71,
	PurchaseAccepted = 72,
	PurchasePartlyDelivered = 73,
	PurchaseDeliveryCompleted = 74,
	
	All = 100,
};

export enum SoeOriginStatusGroup {
	None = 0,
	GeneralStart = 1,
	GeneralStop = 10,
	SupplierInvoiceStart = 11,
	SupplierInvoiceStop = 20,
	CustomerInvoiceStart = 21,
	CustomerInvoiceStop = 30,
	PaymentStart = 31,
	PaymentStop = 40,
	OfferStart = 41,
	OfferStop = 50,
	OrderStart = 51,
	OrderStop = 60,
	ContractStart = 61,
	ContractStop = 70,
}

export enum SoeOriginStatusChange {
	None = 0,
	
	//General 1-100
	DraftToOrigin = 1,
	OriginToVoucher = 2,
	OriginToMatched = 3,
	
	//SupplierInvoice 101-200
	SupplierInvoice_OriginToPayment = 101,
	SupplierInvoice_OriginToPaymentForeign = 102,
	SupplierInvoice_OriginToPaymentSuggestion = 103,
	SupplierInvoice_OriginToPaymentSuggestionForeign = 104,
	SupplierInvoice_UnhandledToAttest = 105,
	
	//CustomerInvoice 201-300
	CustomerInvoice_OriginToPayment = 201,
	CustomerInvoice_OriginToPaymentForeign = 202,
	CustomerInvoice_OriginToExportSOP = 203,
	CustomerInvoice_DraftToInvoiceAndPrintInvoice = 204,
	CustomerInvoice_PrintInvoice = 205,
	CustomerInvoice_PrintReminder = 206,
	CustomerInvoice_InvoiceToReminder = 207,
	CustomerInvoice_UpdateReminderLevel = 208,
	CustomerInvoice_InvoiceToInterest = 209,
	CustomerInvoice_InvoiceToClosed = 210,
	CustomerInvoice_OriginToExportDIRegnskap = 211,
	CustomerInvoice_OriginToExportUniMicro = 212,
	CustomerInvoice_DraftToInvoice_And_SendEInvoice = 213,
	CustomerInvoice_EInvoice_Send = 214,
	CustomerInvoice_OriginToExportDnBNor = 215,
	CustomerInvoice_SendAsEmail = 216,
	CustomerInvoice_DraftToInvoiceAndSendAsEmail = 217,
	CustomerInvoice_SendReminderAsEmail = 218,
	CustomerInvoice_PrintInterestRateCalculation = 219,
	CustomerInvoice_Delete = 220,
	CustomerInvoice_EInvoice_Create = 221,
	CustomerInvoice_DraftToInvoice_And_CreateEInvoice = 222,
	CustomerInvoice_OriginToExportFortnox = 223,
	CustomerInvoice_OriginToExportZetes = 224,
	CustomerInvoice_OriginToExportVismaEAccounting = 225,
	
	//SupplierPayment 301-400
	SupplierPayment_PaymentSuggestionToPayed = 301,
	SupplierPayment_PaymentSuggestionToPayedForeign = 302,
	SupplierPayment_PaymentSuggestionToCancel = 303,
	SupplierPayment_PayedEditDateAndAmount = 305,
	SupplierPayment_PayedEditDateAndAmountForeign = 306,
	SupplierPayment_PayedToVoucher = 307,
	SupplierPayment_PayedToVoucherForeign = 308,
	SupplierPayment_PayedToCancel = 309,
	SupplierPayment_ChangePayDate = 310,
	SupplierPayment_ChangePayDateToVoucher = 311,
	SupplierPayment_PayedEditDateAndAmountToVoucher = 312,
	
	//CustomerPayment 301-400
	CustomerPayment_PayedToVoucher = 401,
	CustomerPayment_PayedToVoucherForeign = 402,
	CustomerPayment_PayedToExportDIRegnskap = 403,
	CustomerPayment_PayedToZetes = 404,
	
	//Billing  401-500
	Billing_InvoiceUser = 501,
	Billing_OfferUser = 502,
	Billing_OrderUser = 503,
	Billing_DraftToOffer = 504,
	Billing_DraftToOrder = 505,
	Billing_OfferToOrder = 506,
	Billing_OfferToInvoice = 507,
	Billing_OrderToInvoice = 508,
	Billing_ContractUser = 509,
	Billing_OfferToPriceOptimization = 510,
	Billing_OrderToPriceOptimization = 511,
	Billing_ContractToOrder = 600,
	Billing_ContractToInvoice = 601,
	Billing_OrderToInvoiceAndPrint = 602,
	Billing_OrderToContract = 603,
	Billing_PrintOrder = 604,
	Billing_ContractToClosed = 605,
	Billing_OrderToDeleted = 606,
	Billing_ContractToServiceOrder = 607,
};

export enum SoeOriginStatusClassification {
	None = 0,
	
	
	//SupplierInvoice (SoeOriginType 1)
	SupplierInvoicesOpen = 101,
	SupplierInvoicesOpenForeign = 102,
	SupplierInvoicesClosed = 103,
	SupplierInvoicesClosedForeign = 104,
	SupplierInvoicesOverdue = 105,
	SupplierInvoicesOverdueForeign = 106,
	SupplierInvoicesAttestFlowMyActive = 107,
	SupplierInvoicesAttestFlowMyClosed = 108,
	SupplierInvoicesAttestFlowUnhandled = 109,
	SupplierInvoicesAttestFlowUnhandledForeign = 110,
	SupplierInvoicesAttestFlowHandled = 111,
	SupplierInvoicesAttestFlowHandledForeign = 112,
	SupplierInvoicesAttestFlowAttested = 113,
	SupplierInvoicesAttestFlowAttestedForeign = 114,
	SupplierInvoicesAttestFlowRejected = 115,
	SupplierInvoicesAttestFlowGauge = 116,
	SupplierInvoicesProjectCentral = 117,
	SupplierInvoicesAll = 118,
	SupplierInvoicesAllForeign = 119,
	SupplierInvoicesSupplierCentral = 120,
	
	//CustomerInvoice (SoeOriginType 2)
	CustomerInvoicesOpenUser = 201,
	CustomerInvoicesOpenUserForeign = 202,
	CustomerInvoicesOpen = 203,
	CustomerInvoicesOpenForeign = 204,
	CustomerInvoicesClosedUser = 205,
	CustomerInvoicesClosedUserForeign = 206,
	CustomerInvoicesClosed = 207,
	CustomerInvoicesClosedForeign = 208,
	CustomerInvoicesReminder = 209,
	CustomerInvoicesReminderForeign = 210,
	CustomerInvoicesInterest = 211,
	CustomerInvoicesInterestForeign = 212,
	CustomerInvoicesCustomerCentral = 213,
	CustomerInvoicesCustomerCentralUser = 214,
	CustomerInvoicesCustomerCentralForeign = 215,
	CustomerInvoicesCustomerCentralUserForeign = 216,
	CustomerInvoicesProjectCentral = 217,
	CustomerInvoicesAll = 218,
	CustomerInvoicesAllForeign = 219,
	
	//SupplierPayment (SoeOriginType 3)
	SupplierPaymentsUnpayed = 301,
	SupplierPaymentsUnpayedForeign = 302,
	SupplierPaymentSuggestions = 303,
	SupplierPaymentSuggestionsForeign = 304,
	SupplierPaymentsPayed = 305,
	SupplierPaymentsPayedForeign = 306,
	SupplierPaymentsVoucher = 307,
	SupplierPaymentsVoucherForeign = 308,
	SupplierPaymentsSupplierCentralUnpayed = 309,
	SupplierPaymentsSupplierCentralUnpayedForeign = 310,
	SupplierPaymentsSupplierCentralPayed = 311,
	SupplierPaymentsSupplierCentralPayedForeign = 312,
	
	//CustomerPayment (SoeOriginType 4)
	CustomerPaymentsUnpayed = 401,
	CustomerPaymentsUnpayedForeign = 402,
	CustomerPaymentsPayed = 403,
	CustomerPaymentsPayedForeign = 404,
	CustomerPaymentsVoucher = 405,
	CustomerPaymentsVoucherForeign = 406,
	
	//Offer (SoeOriginType 5)
	OffersOpenUser = 501,
	OffersOpenUserForeign = 502,
	OffersOpen = 503,
	OffersOpenForeign = 504,
	OffersClosedUser = 505,
	OffersClosedUserForeign = 506,
	OffersClosed = 507,
	OffersClosedForeign = 508,
	OffersCustomerCentral = 509,
	OffersCustomerCentralUser = 510,
	OffersCustomerCentralForeign = 511,
	OffersCustomerCentralUserForeign = 512,
	OffersAll = 513,
	OffersAllForeign = 514,
	
	//Order (SoeOriginType 6)
	OrdersOpenUser = 601,
	OrdersOpenUserForeign = 602,
	OrdersOpen = 603,
	OrdersOpenForeign = 604,
	OrdersClosedUser = 605,
	OrdersClosedUserForeign = 606,
	OrdersClosed = 607,
	OrdersClosedForeign = 608,
	OrdersCustomerCentral = 609,
	OrdersCustomerCentralUser = 610,
	OrdersCustomerCentralForeign = 611,
	OrdersCustomerCentralUserForeign = 612,
	OrdersProjectCentral = 613,
	OrdersAll = 614,
	OrdersAllForeign = 615,
	
	//Contract (SoeOriginType 7)
	ContractsRunningUser = 701,
	ContractsRunning = 702,
	ContractsOpenUser = 703,
	ContractsOpen = 704,
	ContractsClosedUser = 705,
	ContractsClosed = 706,
	ContractsCustomerCentral = 707,
	ContractsCustomerCentralUser = 708,
	ContractsAll = 709,
}

export enum SoeOriginStatusClassificationGroup {
	None = 0,
	
	HandleSupplierInvoices = 1,
	HandleSupplierInvoicesForeign = 2,
	HandleSupplierInvoicesAttestFlow = 3,
	HandleSupplierInvoicesAttestFlowForeign = 4,
	HandleSupplierPayments = 5,
	HandleSupplierPaymentsForeign = 6,
	HandleCustomerInvoices = 7,
	HandleCustomerInvoicesForeign = 8,
	HandleCustomerPayments = 9,
	HandleCustomerPaymentsForeign = 10,
	HandleOffers = 11,
	HandleOffersForeign = 12,
	HandleOrders = 13,
	HandleOrdersForeign = 14,
	HandleContracts = 15,
	HandleContractsForeign = 16,
	
	StateAnalysis = 301,
}

export enum SoePaymentStatus {
	//OBS! Changes here also affects SQL-views and SysTerms!
	
	None = 0,
	
	//General 1-10
	Verified = 1,
	Error = 2,
	Cancel = 4,
	Exported = 5,
	
	//SupplierInvoice 11-20
	Pending = 11,
	
	//CustomerInvoice 21-30
	ManualPayment = 21,
	
	//Completed 31-40
	Checked = 31,
	
	//Error codes 41-100
	//Not added to SysTermGroup because they not should show in any dropdown. Any ErrorCode are shown as Error(3)
	MBEV0025 = 41, //error text can be retrieved from LbCommon.GetErrorMessageFromErrorCode(enumKey)
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
	MTRV0303 = 78,
}

export enum SoePaymentStatusGroup {
	None = 0,
	GeneralStart = 1,
	GeneralStop = 10,
	SupplierInvoiceStart = 11,
	SupplierInvoiceStop = 20,
	CustomerInvoiceStart = 21,
	CustomerInvoiceStop = 30,
	CompletedStart = 31,
	CompletedStop = 40,
	ErrorCodesStart = 41,
	ErrorCodesStop = 100,
}

export enum SoeTimeBlockDateStatus {
	None = 0,
	Regenerating = 1,
	RegenerateAccordingToSchedule = 2,
	RegenerateAccordingToStampEntries = 3,
	RegenerateAccordingToModifiedTimeBlocks = 4,
	Locked = 5,
}

export enum ExcelColumnECom {
	Email = 1,
	Email2 = 2,
	PhoneHome = 3,
	PhoneJob = 4,
	PhoneMobile = 5,
	Fax = 6,
	Web = 7,
	GlnNumber = 8,
}

export enum ExcelColumnAddress {
	DistributionAddress = 1,
	DistributionCoAddress = 2,
	DistributionPostalCode = 3,
	DistributionPostalAddress = 4,
	DistributionCountry = 5,
	BillingAddress = 6,
	BillingCoAddress = 7,
	BillingPostalCode = 8,
	BillingPostalAddress = 9,
	BillingCountry = 10,
	HeadquarterAddress = 11,
	HeadquarterCountry = 12,
	VisitingAddress = 13,
	VisitingDoorCode = 14,
	VisitingPostalCode = 15,
	VisitingPostalAddress = 16,
	VisitingCountry = 17,
	DeliveryAddress = 18,
	DeliveryCoAddress = 19,
	DeliveryPostalCode = 20,
	DeliveryPostalAddress = 21,
	DeliveryCountry = 22,
	DeliveryAddressName = 23,
}

export enum ExcelColumnPaymentInformation {
	StandardPaymentType = 1,
	BgNr = 2,
	PgNr = 3,
	BankNr = 4,
	BicIbanNr = 5,
	Bic = 6,
	Iban = 7,
}

export enum ExcelColumnCustomer {
	CustomerNr = 1,
	Name = 2,
	OrgNr = 3,
	VatNr = 4,
	VatType = 5,
	SupplierNr = 6,
	Country = 7,
	Currency = 8,
	DistributionAddress = 9,
	DistributionCoAddress = 10,
	DistributionPostalCode = 11,
	DistributionPostalAddress = 12,
	DistributionCountry = 13,
	BillingAddress = 14,
	BillingCoAddress = 15,
	BillingPostalCode = 16,
	BillingPostalAddress = 17,
	BillingCountry = 18,
	HeadquarterAddress = 19,
	HeadquarterCountry = 20,
	VisitingAddress = 21,
	VisitingDoorCode = 22,
	VisitingPostalCode = 23,
	VisitingPostalAddress = 24,
	VisitingCountry = 25,
	DeliveryAddressName = 26,
	DeliveryAddress = 27,
	DeliveryCoAddress = 27,
	DeliveryPostalCode = 29,
	DeliveryPostalAddress = 30,
	DeliveryCountry = 31,
	Email = 32,
	Email2 = 33,
	PhoneHome = 34,
	PhoneJob = 35,
	PhoneMobile = 36,
	Fax = 37,
	Web = 38,
	PaymentCondition = 39,
	GracePeriod = 40,
	BillingTemplate = 41,
	InvoiceReference = 42,
	DeliveryMethod = 43,
	DeliveryCondition = 44,
	DefaultPriceListType = 45,
	DefaultWholeseller = 46,
	DiscountMerchandise = 47,
	DiscountService = 48,
	DebitAccountStd = 49,
	DebitAccountInternal1 = 50,
	DebitAccountInternal2 = 51,
	DebitAccountInternal3 = 52,
	DebitAccountInternal4 = 53,
	DebitAccountInternal5 = 54,
	CreditAccountStd = 55,
	CreditAccountInternal1 = 56,
	CreditAccountInternal2 = 57,
	CreditAccountInternal3 = 58,
	CreditAccountInternal4 = 59,
	CreditAccountInternal5 = 60,
	VatAccountStd = 61,
	CategoryCode1 = 62,
	CategoryCode2 = 63,
	CategoryCode3 = 64,
	CategoryCode4 = 65,
	CategoryCode5 = 66,
	Active = 67,
	FinvoiceAddress = 68,
	FinvoiceOperator = 69,
	Language = 70,
	PayingCustomerId = 71,
	InvoiceDeliveryType = 72,
	GLNNumber = 73,
	Note = 74,
}

export enum ExcelColumnSupplier {
	SupplierNr = 1,
	Name = 2,
	OrgNr = 3,
	VatNr = 4,
	VatType = 5,
	VatCode = 6,
	RiksbanksCode = 7,
	OurCustomerNr = 8,
	FactoringSupplier = 9,
	Country = 10,
	Currency = 11,
	StandardPaymentType = 12,
	BgNr = 13,
	PgNr = 14,
	BankNr = 15,
	Bic = 16,
	Iban = 17,
	DistributionAddress = 18,
	DistributionCoAddress = 19,
	DistributionPostalCode = 20,
	DistributionPostalAddress = 21,
	DistributionCountry = 22,
	BillingAddress = 23,
	BillingCoAddress = 24,
	BillingPostalCode = 25,
	BillingPostalAddress = 26,
	BillingCountry = 27,
	HeadquarterAddress = 28,
	HeadquarterCountry = 29,
	VisitingAddress = 30,
	VisitingDoorCode = 31,
	VisitingPostalCode = 32,
	VisitingPostalAddress = 33,
	VisitingCountry = 34,
	DeliveryAddress = 35,
	DeliveryCoAddress = 36,
	DeliveryPostalCode = 37,
	DeliveryPostalAddress = 38,
	DeliveryCountry = 39,
	Email = 40,
	Email2 = 41,
	PhoneHome = 42,
	PhoneJob = 43,
	PhoneMobile = 44,
	Fax = 45,
	Web = 46,
	PaymentCondition = 47,
	CopyInvoiceNrToOcr = 48,
	BlockPayment = 49,
	ConfirmAccounts = 50,
	CreditAccountStd = 51,
	CreditAccountInternal1 = 52,
	CreditAccountInternal2 = 53,
	CreditAccountInternal3 = 54,
	CreditAccountInternal4 = 55,
	CreditAccountInternal5 = 56,
	DebitAccountStd = 57,
	DebitAccountInternal1 = 58,
	DebitAccountInternal2 = 59,
	DebitAccountInternal3 = 60,
	DebitAccountInternal4 = 61,
	DebitAccountInternal5 = 62,
	VatAccountStd = 63,
	InterimAccountStd = 64,
	Active = 65,
}

export enum ExcelColumnContactPerson {
	FirstName = 1,
	LastName = 2,
	Position = 3,
	Sex = 4,
	DistributionAddress = 5,
	DistributionCoAddress = 6,
	DistributionPostalCode = 7,
	DistributionPostalAddress = 8,
	DistributionCountry = 9,
	BillingAddress = 10,
	BillingCoAddress = 11,
	BillingPostalCode = 12,
	BillingPostalAddress = 13,
	BillingCountry = 14,
	HeadquarterAddress = 15,
	HeadquarterCountry = 16,
	VisitingAddress = 17,
	VisitingDoorCode = 18,
	VisitingPostalCode = 19,
	VisitingPostalAddress = 20,
	VisitingCountry = 21,
	DeliveryAddress = 22,
	DeliveryCoAddress = 23,
	DeliveryPostalCode = 24,
	DeliveryPostalAddress = 25,
	DeliveryCountry = 26,
	Email = 27,
	Email2 = 28,
	PhoneHome = 29,
	PhoneJob = 30,
	PhoneMobile = 31,
	Fax = 32,
	Web = 33,
	MapToSupplierNr = 34,
	MapToCustomerNr = 35,
}

export enum ExcelColumnProduct {
	ProductNr = 1,
	Name = 2,
	Description = 3,
	Unit = 4,
	EAN = 5,
	VatType = 6,
	ProductGroup = 7,
	PurchasePrice = 8,
	Price = 9,
	PriceListType = 10,
	DebitAccountStd = 11,
	DebitAccountInternal1 = 12,
	DebitAccountInternal2 = 13,
	DebitAccountInternal3 = 14,
	DebitAccountInternal4 = 15,
	DebitAccountInternal5 = 16,
	CreditAccountStd = 17,
	CreditAccountInternal1 = 18,
	CreditAccountInternal2 = 19,
	CreditAccountInternal3 = 20,
	CreditAccountInternal4 = 21,
	CreditAccountInternal5 = 22,
	VatFreeAccountStd = 23,
	VatAccountStd = 24,
	CategoryCode1 = 25,
	CategoryCode2 = 26,
	CategoryCode3 = 27,
	CategoryCode4 = 28,
	CategoryCode5 = 29,
	VatCode = 30,
}

export enum ExcelColumnEmployee {
	EmployeeId = 1,
	
	//Personal information
	FirstName = 2,
	LastName = 3,
	SocialSec = 4,
	Sex = 5,
	Email = 6,
	DistributionAddress = 7,
	DistributionCoAddress = 8,
	DistributionPostalCode = 9,
	DistributionPostalAddress = 10,
	DistributionCountry = 11,
	PhoneHome = 12,
	PhoneMobile = 13,
	PhoneJob = 14,
	ClosestRelativeNr = 15,
	ClosestRelativeName = 16,
	ClosestRelativeRelation = 17,
	ClosestRelativeNr2 = 18,
	ClosestRelativeName2 = 19,
	ClosestRelativeRelation2 = 20,
	DisbursementMethod = 21,
	DisbursementClearingNr = 22,
	DisbursementAccountNr = 23,
	LoginName = 24,
	LangId = 25,
	DefaultCompanyName = 26,
	RoleName1 = 27,
	RoleName2 = 28,
	RoleName3 = 29,
	AttestRoleName1 = 30,
	AttestRoleName2 = 31,
	AttestRoleName3 = 32,
	AttestRoleName4 = 33,
	AttestRoleName5 = 34,
	AttestRoleNameAccount1 = 35,
	AttestRoleNameAccount2 = 36,
	AttestRoleNameAccount3 = 37,
	AttestRoleNameAccount4 = 38,
	AttestRoleNameAccount5 = 39,
	
	//Employment information
	EmployeeNr = 40,
	EmploymentType = 41,
	EmployeeGroupName = 42,
	PayrollGroupName = 43,
	VacationGroupName = 44,
	EmploymentDate = 45,
	EndDate = 46,
	WorkTimeWeek = 47,
	EmploymentPriceTypeCode = 48,
	EmploymentPayrollLevelCode = 49,
	EmploymentPriceTypeFromDate = 50,
	EmploymentPriceTypeAmount = 51,
	CostAccountStd = 52,
	CostAccountInternal1 = 53,
	CostAccountInternal2 = 54,
	CostAccountInternal3 = 55,
	CostAccountInternal4 = 56,
	CostAccountInternal5 = 57,
	IncomeAccountStd = 58,
	IncomeAccountInternal1 = 59,
	IncomeAccountInternal2 = 60,
	IncomeAccountInternal3 = 61,
	IncomeAccountInternal4 = 62,
	IncomeAccountInternal5 = 63,
	
	EarnedDaysPaid = 64, //Sem bet Intjänade dgr
	UsedDaysPaid = 65, //Sem bet Uttagna dgr
	RemainingDaysPaid = 66,  //Sem bet Kvarvarande dgr
	EmploymentRatePaid = 67, //Sem bet Syssgrad
	Paidvacationallowance = 68, //Utb semtillägg
	
	EarnedDaysUnpaid = 69, //Sem obet Intjänade dgr
	UsedDaysUnpaid = 70, //Sem obet Uttagna dgr
	RemainingDaysUnpaid = 71, //SSem 0bet Kvarvarande dgr
	
	EarnedDaysAdvance = 72, //Sem förskott Intjänade dgr
	UsedDaysAdvance = 73, //Sem förskott Uttagna dgr
	RemainingDaysAdvance = 74, //Sem förskott Kvarvarande dgr
	DebtInAdvanceAmount = 75, //Skuld förskott belopp
	DebtInAdvanceDueDate = 76, //Skuld förskott förfaller
	
	SavedDaysYear1 = 77, //Sparat år 1 sparade dgr
	UsedDaysYear1 = 78, //Sparat år 1 uttagna dgr
	RemainingDaysYear1 = 79, //Sparat år 1 kvarvarande  dgr
	EmploymentRateYear1 = 80, //Sparat år 1 syssgrad
	
	SavedDaysYear2 = 81, //Sparat år 2 sparade dgr
	UsedDaysYear2 = 82, //Sparat år 2 uttagna dgr
	RemainingDaysYear2 = 83, //Sparat år 2 kvarvarande  dgr
	EmploymentRateYear2 = 84, //Sparat år 2 syssgrad
	
	SavedDaysYear3 = 85, //Sparat år 3 sparade dgr
	UsedDaysYear3 = 86, //Sparat år 3 uttagna dgr
	RemainingDaysYear3 = 87, //Sparat år 3 kvarvarande  dgr
	EmploymentRateYear3 = 88, //Sparat år 3 syssgrad
	
	SavedDaysYear4 = 89, //Sparat år 4 sparade dgr
	UsedDaysYear4 = 90, //Sparat år 4 uttagna dgr
	RemainingDaysYear4 = 91, //Sparat år 4 kvarvarande dgr
	EmploymentRateYear4 = 92, //Sparat år 4 syssgrad
	
	SavedDaysYear5 = 93, //Sparat år 5 sparade dgr
	UsedDaysYear5 = 94, //Sparat år 5 uttagna dgr
	RemainingDaysYear5 = 95, //Sparat år 5 kvarvarande  dgr
	EmploymentRateYear5 = 96, //Sparat år 5 syssgrad
	
	SavedDaysOverdue = 97, //Förfallna sparade dgr
	UsedDaysOverdue = 98, //Förfallna uttagna dgr
	RemainingDaysOverdue = 99, //Förfallna kvarvarande  dgr
	EmploymentRateOverdue = 100, //Förfallna syssgrad
	
	HighRiskProtection = 101,
	HighRiskProtectionTo = 102,
	MedicalCertificateReminder = 103,
	MedicalCertificateDays = 104,
	Absence105DaysExcluded = 105,
	Absence105DaysExcludedDays = 106,
	EmployeeFactorType = 107,
	EmployeeFactorFromDate = 108,
	EmployeeFactorFactor = 109,
	PayrollStatisticsPersonalCategory = 110,
	PayrollStatisticsWorkTimeCategory = 111,
	PayrollStatisticsSalaryType = 112,
	PayrollStatisticsWorkPlaceNumber = 113,
	PayrollStatisticsCFARNumber = 114,
	WorkPlaceSCB = 115,
	AFACategory = 116,
	AFASpecialAgreement = 117,
	AFAWorkplaceNr = 118,
	CollectumITPPlan = 119,
	CollectumCostPlace = 120,
	CollectumAgreedOnProduct = 121,
	CategoryCode1 = 122,
	CategoryCode2 = 123,
	CategoryCode3 = 124,
	CategoryCode4 = 125,
	CategoryCode5 = 126,
	SecondaryCategoryCode1 = 127,
	SecondaryCategoryCode2 = 128,
	SecondaryCategoryCode3 = 129,
	SecondaryCategoryCode4 = 130,
	SecondaryCategoryCode5 = 131,
	EmployeeAccount1 = 132,
	EmployeeAccountDateFrom1 = 133,
	EmployeeAccountDefault1 = 134,
	EmployeeAccount2 = 135,
	EmployeeAccountDateFrom2 = 136,
	EmployeeAccountDefault2 = 137,
	EmployeeAccount3 = 138,
	EmployeeAccountDateFrom3 = 139,
	EmployeeAccountDefault3 = 140,
	DefaultTimeDeviationCauseName = 141,
	DefaultTimeCodeName = 142,
	ExperienceMonths = 143,
	ExperienceAgreedOrEstablished = 144,
	WorkPlace = 145,
	
	//HR
	EmployeePositionCode = 146,
	Note = 147,
	
	//Core
	State = 148,
}

export enum ExcelColumnProductGroup {
	Code = 0,
	Name = 1,
}

export enum ExcelColumnCustomeCategory {
	Code = 0,
	Name = 1,
}

export enum ExcelColumnPayrollStartValue {
	Code = 1,
	Appellation = 2,
	EmployeeNr = 3,
	Quantity = 4,
	Amount = 5,
	PayrollProductNr = 6,
	Date = 7,
	ScheduleTime = 8,
	AbsenceTime = 9,
}

export enum ExcelColumnAccount {
	AccountNr = 1,
	AccountName = 2,
	Description = 3,
	AccountDimNr = 4,
	AccountDimSieNr = 5,
	AccountType = 6,
	VatAccountNr = 7,
	IsAccrualAccount = 8,
	AmountStop = 9,
	RowTextStop = 10,
	UnitStop = 11,
	Unit = 12,
	SRUCode1 = 13,
	SRUCode2 = 14,
	AccountDim2Nr = 15,
	AccountDim2Default = 16,
	AccountDim2MandatoryLevel = 17,
	AccountDim3Nr = 18,
	AccountDim3Default = 19,
	AccountDim3MandatoryLevel = 20,
	AccountDim4Nr = 21,
	AccountDim4Default = 22,
	AccountDim4MandatoryLevel = 23,
	AccountDim5Nr = 24,
	AccountDim5Default = 25,
	AccountDim5MandatoryLevel = 26,
	AccountDim6Nr = 27,
	AccountDim6Default = 28,
	AccountDim6MandatoryLevel = 29,
	ExcludeVatVerification = 30,
}

export enum ExcelColumnTaxDeductionContact {
	CustomerNr = 1,
	CustomerName = 2,
	SocialSeqNr = 3,
	Name = 4,
	Property = 5,
	ApartmentNr = 6,
	CooperativeOrgNr = 7,
}

export enum ExcelColumnPricelist {
	Name = 1,
	Description = 2,
	Currency = 3,
	ProductNr = 4,
	ProductName = 5,
	Price = 6,
	Quantity = 7,
	StartDate = 8,
	EndDate = 9,
}

export enum ExcelColumnAgreement {
	CustomerNr = 1,
	AgreementGroup = 2,
	InternalText = 3,
	Marking = 4,
	AgreementNr = 5,
	DeliveryName = 6,
	DeliveryStreet = 7,
	DeliveryPostalCode = 8,
	DeliveryPostalAddress = 9,
	NextInvoiceDate = 10,
	Category1 = 11,
	Category2 = 12,
	Category3 = 13,
}

export enum ExternalCompanySearchProvider {
	None = 0,
	PRH = 1
}

export enum CustomerAccountType {
	Unknown = 0,
	Credit = 1,
	Debit = 2,
	VAT = 3,
};

export enum EmploymentAccountType {
	Unknown = 0,
	Cost = 1,       // Debit (Purchase)
	Income = 2,     // Credit (Sales)
	Fixed1 = 3,     // Fixed accounting 1
	Fixed2 = 4,     // Fixed accounting 2
	Fixed3 = 5,     // Fixed accounting 3
	Fixed4 = 6,     // Fixed accounting 4
	Fixed5 = 7,     // Fixed accounting 5
	Fixed6 = 8,     // Fixed accounting 6
	Fixed7 = 9,     // Fixed accounting 7
	Fixed8 = 10,    // Fixed accounting 8
};

export enum EmployeeGroupAccountType {
	Unknown = 0,
	Cost = 1,   // Debit (Purchase)
	Income = 2, // Credit (Sales)
};

export enum InventoryAccountType {
	Unknown = 0,
	Inventory = 1,
	AccWriteOff = 2,
	WriteOff = 3,
	AccOverWriteOff = 4,
	OverWriteOff = 5,
	AccWriteDown = 6,
	WriteDown = 7,
	AccWriteUp = 8,
	WriteUp = 9,
	SalesProfit = 10,
	SalesLoss = 11,
	Sales = 12,
};

export enum PayrollGroupAccountType {
	Unknown = 0,
	
	EmploymentTax = 1,
	PayrollTax = 2,
	OwnSupplementCharge = 3
};

export enum ProductAccountType {
	Unknown = 0,
	Purchase = 1,
	Sales = 2,
	VAT = 3,
	SalesNoVat = 4,
	SalesContractor = 5,
	StockIn = 6,  //for connecting to stocktransactions
	StockInChange = 7,
	StockOut = 8,
	StockOutChange = 9,
	StockInv = 10,
	StockInvChange = 11,
	StockLoss = 12,
	StockLossChange = 13,
	ExportWithinEU = 14,
	ExportOutsideEU = 15,
	TripartiteTrade = 16,
	StockTransferChange = 17,
};

export enum ProjectAccountType {
	Unknown = 0,
	Credit = 1,
	Debit = 2,
	SalesNoVat = 3,
	SalesContractor = 4,
};

export enum SupplierAccountType {
	Unknown = 0,
	Credit = 1,
	Debit = 2,
	VAT = 3,
	Interim = 4,
};

export enum ImageFormatType {
	NONE = 0,
	JPG = 1,
	PNG = 2,
	PDF = 3,
};

export enum SoeInvoiceRowDiscountType {
	Unknown = 0,
	Percent = 1,
	Amount = 2,
};

export enum SoeInvoiceRowType {
	Unknown = 0,
	AccountingRow = 1,
	ProductRow = 2,
	TextRow = 3,
	BaseProductRow = 4,
	PageBreakRow = 5,
	SubTotalRow = 6,
};

export enum SoeProductRowType {
	//is a enum flag type
	None = 0,
	TimeRow = 1,
	ExpenseRow = 2,
	FromSupplierInvoice = 4,
	FromEDI = 8,
	TimeBillingRow = 16,
	HouseholdTaxDeduction = 32,
	//next = 64
};

export enum SoeDayTypeClassification {
	Undefined = 0,
	Weekday = 1,
	Saturday = 2,
	Sunday = 3,
	Holiday = 4,
}

export enum SoeTimeHalfdayType {
	RelativeStartValue = 1,
	RelativeEndValue = 2,
	RelativeStartPercentage = 3,
	RelativeEndPercentage = 4,
	ClockInMinutes = 5,
}

export enum ContactAddressItemType {
	Unknown = 1,
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
	IndividualTaxNumber = 20,  // For Finland & other countries where skattenummer <> personalnummer
	GlnNumber = 21
}

export enum TimeSchedulePlanningMonthDTOStatusType {
	Open = 1,
	Assigned = 2,
	Wanted = 3,
	Unwanted = 4,
	AbsenceRequested = 5,
	AbsenceApproved = 6,
	Preliminary = 7
}

export enum SoeSettingType {
	Field = 1,
	Form = 2,
};

export enum SoeSetting {
	Label = 1,
	Visible = 2,
	SkipTabStop = 3,
	ReadOnly = 4,
	BoldLabel = 5,
};

export enum SoeFormMode {
	Save = 1,
	Update = 2,
	Register = 3,
	Delete = 4,
	Copy = 5,
	RegisterFromCopy = 6,
	Repopulate = 7,
	StopSettings = 8,
	NoSettingsApplied = 9,
	RunSettings = 10,
	WithSettingsApplied = 11,
	Prev = 12,
	Next = 13,
	RunReport = 14,
	ExecuteAdminTask = 15,
	Back = 16,
};

export enum SelectEntryValidation  {
	//Enumeration also in SoftOne.Soe.Business.Util)
	NotEmpty = 1,
}

export enum TextEntryValidation {
	// Enum for all validation types (Enumeration also in SoftOne.Soe.Web.UI.WebControls.SoeFormInputEntryBase)
	
	Required = 1,
	//RequiredGroup = 2,
	Email = 4,
	//MinLength = 8,
	//Match = 16,
	Luhn = 32
}

export enum SoeFormIntervalEntryType {
	NoType = 0,
	Text = 1,
	Select = 2,
	Date = 3,
	Numeric = 4,
};

export enum Feature {
	None = 0,
	
	Favorite = 9960,
	
	
	Economy = 1,
	
	Economy_Dashboard = 524,
	Economy_AttestFlowGauge = 642,
	Economy_CompanyNewsGauge = 806, // NOT USED
	Economy_UploadedFilesGauge = 807,
	
	Economy_Accounting = 2,
	Economy_Accounting_Vouchers = 11,
	Economy_Accounting_Vouchers_Edit = 9,
	Economy_Accounting_VoucherTemplateList = 12,
	Economy_Accounting_AccountDistributionEntry = 474,
	Economy_Accounting_Balance = 17,
	Economy_Accounting_BalanceChange = 248,
	Economy_Accounting_Accounts = 5,
	Economy_Accounting_Accounts_Edit = 6,
	Economy_Accounting_Accounts_Edit_History = 7,
	Economy_Accounting_Accounts_BatchUpdate = 1088,
	Economy_Accounting_AccountRoles = 3,
	Economy_Accounting_AccountRoles_Edit = 4,
	Economy_Accounting_AccountRoles_Inactivate = 2024,
	Economy_Accounting_VoucherSeries = 13,
	Economy_Accounting_VoucherSeries_Edit = 14,
	Economy_Accounting_AccountPeriods = 15,
	Economy_Accounting_AccountPeriods_MapToVoucherSeries = 16,
	Economy_Accounting_AccountPeriods_UnlockYear = 1112,
	Economy_Accounting_VoucherHistory = 10,
	Economy_Accounting_Budget = 693,
	Economy_Accounting_Budget_Edit = 694,
	Economy_Accounting_Budget_Edit_Definite = 708,
	Economy_Accounting_Budget_Edit_Unlock = 713,
	Economy_Accounting_Reconciliation = 749,
	Economy_Accounting_CompanyGroup = 957,
	Economy_Accounting_CompanyGroup_Companies = 958,
	Economy_Accounting_CompanyGroup_Companies_Edit = 961,
	Economy_Accounting_CompanyGroup_TransferDefinitions = 959,
	Economy_Accounting_CompanyGroup_TransferDefinitions_Edit = 962,
	Economy_Accounting_CompanyGroup_Transfers = 960,
	Economy_Accounting_CompanyGroup_Transfers_Edit = 963,
	Economy_Accounting_VatVerification = 999,
	Economy_Accounting_VoucherSearch = 1000,
	Economy_Accounting_SalesBudget = 2018,
	Economy_Accounting_SalesForecast = 2022,
	Economy_Accounting_LiquidityPlanning = 2023,
	
	Economy_Supplier = 18,
	Economy_Supplier_Suppliers = 19,
	Economy_Supplier_Suppliers_Edit = 20,
	Economy_Supplier_Suppliers_Documents = 1034,
	Economy_Supplier_Suppliers_TrackChanges = 1035,
	Economy_Supplier_Suppliers_BatchUpdate = 2050,
	Economy_Supplier_Invoice = 21,
	Economy_Supplier_Invoice_Invoices = 94,
	Economy_Supplier_Invoice_Incoming = 99,
	Economy_Supplier_Invoice_Invoices_All = 951,
	Economy_Supplier_Invoice_Invoices_Edit = 22,
	Economy_Supplier_Invoice_Invoices_Edit_UnlockAccounting = 1004,
	Economy_Supplier_Invoice_Status = 23,
	Economy_Supplier_Invoice_Status_Foreign = 833,
	Economy_Supplier_Invoice_Status_DraftToOrigin = 95,
	Economy_Supplier_Invoice_Status_OriginToVoucher = 97,
	Economy_Supplier_Invoice_Status_OriginToPaymentSuggestion = 618,
	Economy_Supplier_Invoice_Status_OriginToPaymentSuggestionForeign = 619,
	Economy_Supplier_Invoice_Status_PaymentSuggestionToCancel = 638,
	Economy_Supplier_Invoice_Status_PaymentSuggestionToCancelForeign = 639,
	Economy_Supplier_Invoice_Status_OriginToPayment = 96,
	Economy_Supplier_Invoice_Status_OriginToPaymentForeign = 120,
	Economy_Supplier_Invoice_Status_PayedEditDateAndAmount = 636,
	Economy_Supplier_Invoice_Status_PayedEditDateAndAmountForeign = 637,
	Economy_Supplier_Invoice_Status_PayedToCancel = 568,
	Economy_Supplier_Invoice_Status_PayedToCancelForeign = 640,
	Economy_Supplier_Invoice_Status_PayedToVoucher = 98,
	Economy_Supplier_Invoice_Status_PayedToVoucherForeign = 143,
	Economy_Supplier_Invoice_Status_PaymentVoucher = 470,
	Economy_Supplier_Invoice_Status_PaymentVoucherForeign = 471,
	Economy_Supplier_Invoice_Status_SendPayment = 1087,
	Economy_Supplier_Invoice_Unlock = 586,
	Economy_Supplier_Invoice_Scanning = 477,
	Economy_Supplier_Invoice_Finvoice = 511,
	Economy_Supplier_Invoice_AttestFlow = 605,
	Economy_Supplier_Invoice_AttestFlow_Overview = 831,
	Economy_Supplier_Invoice_AttestFlow_Overview_Multiselect = 851,
	Economy_Supplier_Invoice_AttestFlow_Admin = 685,
	Economy_Supplier_Invoice_AttestFlow_Edit = 629,
	Economy_Supplier_Invoice_AttestFlow_Add = 646,
	Economy_Supplier_Invoice_AttestFlow_Cancel = 647,
	Economy_Supplier_Invoice_AttestFlow_TransferToLedger = 648,
	Economy_Supplier_Invoice_AttestFlow_MyItems = 686,
	Economy_Supplier_Invoice_AttestFlow_ImageOnly = 1012,
	Economy_Supplier_Invoice_Project = 684,
	Economy_Supplier_Invoice_AgeDistribution = 610,
	Economy_Supplier_Invoice_LiquidityPlanning = 628,
	Economy_Supplier_Invoice_Matching = 625,
	Economy_Supplier_Invoice_Matches = 626,
	Economy_Supplier_Invoice_AddImage = 669,
	Economy_Supplier_Invoice_ChangeCompany = 814,
	Economy_Supplier_Invoice_Order = 1028,
	Economy_Supplier_Invoice_ProductRows = 1094,
	Economy_Supplier_Invoice_BatchInvoicing = 1101,
	Economy_Supplier_Payment = 126,
	Economy_Supplier_Payment_Payments = 128,
	Economy_Supplier_Payment_Payments_Edit = 127,
	Economy_Supplier_Payment_Payments_Edit_UnlockAccounting = 1005,
	Economy_Supplier_Payment_Send_Notification = 1115,
	Economy_Supplier_Invoice_AttestFlow_Overview_APP_ChangeAccount = 1041,
	Economy_Supplier_Invoice_AttestFlow_Overview_APP_ChangeInternalAccount = 1042,
	Economy_Supplier_Invoice_AttestFlow_Upload_Documents = 1121,
	
	Economy_Customer = 24,
	Economy_Customer_Customers = 25,
	Economy_Customer_Customers_BatchUpdate = 1057,
	Economy_Customer_Customers_Edit = 26,
	Economy_Customer_Customers_Edit_Users = 711,
	Economy_Customer_Customers_Edit_OnlyPersonal = 781,
	Economy_Customer_Customers_Edit_Documents = 1044,
	Economy_Customer_Invoice = 27,
	Economy_Customer_Invoice_Invoices = 135,
	Economy_Customer_Invoice_Invoices_All = 952,
	Economy_Customer_Invoice_Invoices_Edit = 28,
	Economy_Customer_Invoice_Invoices_Edit_UnlockAccounting = 585,
	Economy_Customer_Invoice_Invoices_Edit_ExportSOP = 560,
	Economy_Customer_Invoice_Invoices_Edit_ExportDIRegnskap = 689,
	Economy_Customer_Invoice_Invoices_Edit_ExportUniMicro = 709,
	Economy_Customer_Invoice_Invoices_Edit_ExportDnBNor = 932,
	Economy_Customer_Invoice_Matching = 633,
	Economy_Customer_Invoice_Matches = 634,
	Economy_Customer_Invoice_Status = 29,
	Economy_Customer_Invoice_Status_Foreign = 832,
	Economy_Customer_Invoice_Status_DraftToOrigin = 136,
	Economy_Customer_Invoice_Status_OriginToVoucher = 138,
	Economy_Customer_Invoice_Status_OriginToPayment = 146,
	Economy_Customer_Invoice_Status_OriginToPaymentForeign = 147,
	Economy_Customer_Invoice_Status_InvoiceToReminderAndInterest = 583,
	Economy_Customer_Invoice_Status_PayedToVoucher = 139,
	Economy_Customer_Invoice_Status_PayedToVoucherForeign = 144,
	Economy_Customer_Invoice_Status_PaymentVoucher = 468,
	Economy_Customer_Invoice_Status_PaymentVoucherForeign = 469,
	Economy_Customer_Invoice_AgeDistribution = 609,
	Economy_Customer_Invoice_LiquidityPlanning = 627,
	Economy_Customer_Invoice_InsecureDebts = 630,
	Economy_Customer_Invoice_Statistics = 1001,
	Economy_Customer_Invoice_Payment_Extended_Rights = 3021,
	Economy_Customer_Payment = 132,
	Economy_Customer_Payment_Payments = 134,
	Economy_Customer_Payment_Payments_Edit = 133,
	Economy_Customer_Payment_Payments_Edit_AllowNegativeBalance = 1040,
	
	Economy_Inventory = 479,
	Economy_Inventory_Inventories = 480,
	Economy_Inventory_Inventories_Edit = 484,
	Economy_Inventory_WriteOffTemplates = 481,
	Economy_Inventory_WriteOffTemplates_Edit = 485,
	Economy_Inventory_WriteOffMethods = 482,
	Economy_Inventory_WriteOffMethods_Edit = 486,
	Economy_Inventory_WriteOffs = 483,
	Economy_Inventory_WriteOffs_Edit = 487,
	
	Economy_Import = 30,
	Economy_Import_Payments = 129,
	Economy_Import_Payments_Supplier = 130,
	Economy_Import_Payments_Customer = 131,
	//Economy_Import_Payments_PG = 180,
	Economy_Import_Payments_SOP = 846,
	Economy_Import_Invoices = 478,
	Economy_Import_Invoices_Finvoice = 509,
	Economy_Import_Invoices_Automaster = 989,
	Economy_Import_Sie = 31,
	Economy_Import_Sie_Account = 104,
	Economy_Import_Sie_Voucher = 105,
	Economy_Import_Sie_AccountBalance = 106,
	Economy_Import_ExcelImport = 246,
	Economy_Import_XEConnect = 707,
	
	Economy_Export = 32,
	Economy_Export_Payments = 181,
	Economy_Export_Payments_LB = 122,
	Economy_Export_Payments_PG = 123,
	Economy_Export_Payments_SEPA = 521,
	Economy_Export_Payments_Nets = 838,
	Economy_Export_Invoices = 564,
	Economy_Export_Invoices_SOP = 561,
	Economy_Export_Invoices_DIRegnskap = 690,
	Economy_Export_Invoices_UniMicro = 710,
	Economy_Export_Invoices_DnBNor = 930,
	Economy_Export_Invoices_PaymentService = 954,
	Economy_Export_Sie = 33,
	Economy_Export_Sie_Type1 = 100,
	Economy_Export_Sie_Type2 = 101,
	Economy_Export_Sie_Type3 = 102,
	Economy_Export_Sie_Type4 = 103,
	Economy_Export_Finnish_Tax = 721,
	Economy_Export_Finnish_Tax_VAT_Report = 722,
	//Economy_Export_AltInnExport = 734,
	Economy_Export_ICABalanceExport = 849,
	Economy_Export_Payments_Cfp = 918,
	//Economy_Export_GMReporting = 964,
	Economy_Export_CustomerSpecific = 1049,
	Economy_Export_CustomerSpecific_Pirat = 1050,
	Economy_Export_CustomerSpecific_Safilo = 1100,
	Economy_Export_SAFT = 1099,
	Economy_Export_SalesEU = 3081,
	
	Economy_Distribution = 34,
	Economy_Distribution_Reports = 35,
	Economy_Distribution_Reports_Edit = 36,
	Economy_Distribution_Reports_Selection = 37,
	Economy_Distribution_Reports_Selection_Preview = 38,
	Economy_Distribution_Reports_Selection_Download = 39,
	Economy_Distribution_Reports_ReportGroupMapping = 40,
	Economy_Distribution_Reports_ReportRolePermission = 556,
	Economy_Distribution_Reports_SavePublicSelections = 1062,
	Economy_Distribution_DrillDownReports = 923,
	Economy_Distribution_Packages = 41,
	Economy_Distribution_Packages_Edit = 42,
	Economy_Distribution_Groups = 43,
	Economy_Distribution_Groups_Edit = 44,
	Economy_Distribution_Headers = 45,
	Economy_Distribution_Headers_Edit = 46,
	Economy_Distribution_Headers_OppositeCharacter = 925,
	Economy_Distribution_Templates = 47,
	Economy_Distribution_Templates_Edit = 48,
	Economy_Distribution_Templates_Edit_Download = 201,
	Economy_Distribution_SysTemplates = 49,
	Economy_Distribution_SysTemplates_Edit = 50,
	Economy_Distribution_SysTemplates_Edit_Download = 202,
	Economy_Distribution_ReportTransfer = 3028,
	//Economy_Distribution_SalesEU = 3081,
	
	Economy_Analysis = 1053,
	Economy_Insights = 1072,
	
	Economy_Intrastat = 1091,
	Economy_Intrastat_Administer = 1092,
	Economy_Intrastat_ReportsAndExport = 1093,
	
	Economy_Preferences = 51,
	Economy_Preferences_CompSettings = 52,
	Economy_Preferences_AdvCompSettings = 53,
	Economy_Preferences_PayCondition = 60,
	Economy_Preferences_PayCondition_Edit = 61,
	Economy_Preferences_Currency = 141,
	Economy_Preferences_Currency_Edit = 142,
	Economy_Preferences_VoucherSettings = 54,
	Economy_Preferences_VoucherSettings_Accounts = 148,
	Economy_Preferences_VoucherSettings_VatCodes = 705,
	Economy_Preferences_VoucherSettings_VatCodes_Edit = 706,
	Economy_Preferences_VoucherSettings_AccountDistributionAuto = 475,
	Economy_Preferences_VoucherSettings_AccountDistributionPeriod = 476,
	Economy_Preferences_VoucherSettings_MatchCodes = 635,
	Economy_Preferences_VoucherSettings_DistributionCodes = 695,
	Economy_Preferences_VoucherSettings_DistributionCodes_Edit = 696,
	Economy_Preferences_VoucherSettings_GrossProfitCodes = 727,
	Economy_Preferences_VoucherSettings_GrossProfitCodes_Edit = 728,
	Economy_Preferences_SuppInvoiceSettings = 55,
	Economy_Preferences_SuppInvoiceSettings_Attest = 302,
	Economy_Preferences_SuppInvoiceSettings_Accounts = 149,
	Economy_Preferences_SuppInvoiceSettings_PaymentMethods = 56,
	Economy_Preferences_SuppInvoiceSettings_PaymentMethods_Edit = 57,
	Economy_Preferences_SuppInvoiceSettings_AttestGroups = 912,
	Economy_Preferences_CustInvoiceSettings = 121,
	Economy_Preferences_CustInvoiceSettings_Accounts = 150,
	Economy_Preferences_CustInvoiceSettings_PaymentMethods = 178,
	Economy_Preferences_CustInvoiceSettings_PaymentMethods_Edit = 179,
	Economy_Preferences_InventorySettings = 489,
	Economy_Preferences_InventorySettings_Accounts = 492,
	
	
	
	Billing = 161,
	
	Billing_Dashboard = 525,
	Billing_MapGauge = 620,
	Billing_OpenShiftsGauge = 799,
	Billing_WantedShiftsGauge = 801,
	Billing_EmployeeRequestsGauge = 802,
	Billing_CompanyNewsGauge = 803, // NOT USED
	Billing_SystemInfoGauge = 804,
	Billing_UploadedFilesGauge = 805,
	Billing_MyShiftsGauge = 810,
	
	Billing_Contract = 447,
	Billing_Contract_ContractsUser = 449,
	Billing_Contract_Contracts = 451,
	Billing_Contract_Contracts_Edit = 448,
	Billing_Contract_Contracts_Edit_Invoice = 460,
	Billing_Contract_Contracts_Edit_Contract = 463,
	Billing_Contract_Contracts_Edit_ProductRows = 461,
	Billing_Contract_Contracts_Edit_ProductRows_Copy = 955,
	Billing_Contract_Contracts_Edit_ProductRows_Merge = 956,
	Billing_Contract_Contracts_Edit_AccountingRows = 535,
	Billing_Contract_Contracts_Edit_Images = 615,
	Billing_Contract_Contracts_Edit_ChangeInvoiceDate = 967,
	Billing_Contract_Contracts_Edit_Tracing = 462,
	Billing_Contract_Status = 450,
	Billing_Contract_Status_ContractToOrder = 452,
	Billing_Contract_Status_ContractToInvoice = 453,
	Billing_Contract_Groups = 454,
	Billing_Contract_Groups_Edit = 455,
	Billing_Contract_ContractsAll = 947,
	
	Billing_Offer = 399,
	Billing_Offer_OffersUser = 401,
	Billing_Offer_Offers = 418,
	Billing_Offer_Offers_Edit = 400,
	Billing_Offer_Offers_Edit_Invoice = 411,
	Billing_Offer_Offers_Edit_ProductRows = 412,
	Billing_Offer_Offers_Edit_ProductRows_Copy = 1009,
	Billing_Offer_Offers_Edit_AccountingRows = 534,
	Billing_Offer_Offers_Edit_Images = 614,
	Billing_Offer_Offers_Edit_Tracing = 413,
	Billing_Offer_Offers_Edit_Unlock = 1117,
	Billing_Offer_Offers_Edit_Close = 1118,
	Billing_Offer_Status = 402,
	Billing_Offer_Status_Foreign = 834,
	//Billing_Offer_Status_DraftToOffer = 420,
	Billing_Offer_Status_OfferToOrder = 421,
	Billing_Offer_Status_OfferToInvoice = 422,
	Billing_Offer_OffersAll = 948,
	
	Billing_Order = 405,
	Billing_Order_OrdersUser = 407,
	Billing_Order_Orders = 419,
	Billing_Order_Orders_Edit = 406,
	Billing_Order_Orders_Edit_Invoice = 414,
	Billing_Order_Orders_Edit_ProductRows = 416,
	Billing_Order_Orders_Edit_ProductRows_Copy = 571,
	Billing_Order_Orders_Edit_ProductRows_Merge = 687,
	Billing_Order_Orders_Edit_ProductRows_Stock = 740,
	Billing_Order_Orders_Edit_ProductRows_QuantityWarning = 837,
	Billing_Order_Orders_Edit_ProductRows_NoDeletion = 862,
	Billing_Order_Orders_Edit_ProductRows_AllowUpdatePurchasePrice = 1116,
	Billing_Order_Orders_Edit_AccountingRows = 536,
	Billing_Order_Orders_Edit_Images = 616,
	Billing_Order_Orders_Edit_Tracing = 417,
	Billing_Order_Orders_Edit_Unlock = 565,
	Billing_Order_Orders_Edit_Close = 920,
	Billing_Order_Orders_Edit_Expenses = 1011,
	Billing_Order_Orders_Edit_Delete = 1013,
	Billing_Order_Orders_Edit_MainOrder = 1022,
	Billing_Order_Orders_Edit_Splitt_TimeRows = 1025,
	Billing_Order_Orders_Edit_Project = 1065,
	Billing_Order_Orders_Edit_Customer = 1066,
	Billing_Order_Orders_Edit_DirectInvoicing = 1105,
	
	Billing_Order_Status = 408,
	Billing_Order_Status_Foreign = 835,
	//Billing_Order_Status_DraftToOrder = 423,
	Billing_Order_Status_OrderToInvoice = 424,
	Billing_Order_Status_OrderToContract = 854,
	Billing_Order_Checklists = 623,
	Billing_Order_Checklists_AddChecklists = 621,
	Billing_Order_Checklists_AnswerChecklists = 622,
	Billing_Order_Planning = 773,
	Billing_Order_Planning_CalendarView = 774,
	Billing_Order_Planning_DayView = 775,
	Billing_Order_Planning_ScheduleView = 776,
	Billing_Order_Planning_Bookings = 825,
	Billing_Order_PlanningUser = 777,
	Billing_Order_PlanningUser_CalendarView = 778,
	Billing_Order_PlanningUser_DayView = 779,
	Billing_Order_PlanningUser_ScheduleView = 780,
	Billing_Order_ShowOnMap = 839,
	Billing_Order_UseDiffWarning = 905,
	Billing_Order_OrdersAll = 949,
	Billing_Order_Only_ChangeRowState_IfOwner = 1010,
	Billing_Order_SupplierInvoices = 1020,
	Billing_Order_HandleBilling = 3046,
	
	Billing_Invoice = 162,
	Billing_Invoice_InvoicesUser = 245,
	Billing_Invoice_Invoices = 200,
	Billing_Invoice_Invoices_Edit = 167,
	Billing_Invoice_Invoices_Edit_Invoice = 396,
	Billing_Invoice_Invoices_Edit_ProductRows = 397,
	Billing_Invoice_Invoices_Edit_ProductRows_Copy = 855,
	Billing_Invoice_Invoices_Edit_ProductRows_AllowUpdatePurchasePrice = 1033,
	Billing_Invoice_Invoices_Edit_AccountingRows = 537,
	Billing_Invoice_Invoices_Edit_Images = 617,
	Billing_Invoice_Invoices_Edit_Tracing = 398,
	Billing_Invoice_Invoices_Edit_Unlock = 473,
	Billing_Invoice_Invoices_Edit_Delete = 1014,
	Billing_Invoice_Invoices_Edit_EInvoice = 909,
	Billing_Invoice_Invoices_Edit_EInvoice_CreateSvefaktura = 513,
	Billing_Invoice_Invoices_Edit_EInvoice_CreateFinvoice = 530,
	Billing_Invoice_Invoices_Edit_EInvoice_CreateIntruminvoice = 1083,
	Billing_Invoice_Invoices_Edit_EInvoice_SendFinvoice = 1096,
	Billing_Invoice_Invoices_Edit_ExportSOP = 559,
	Billing_Invoice_Invoices_Edit_ExportFortnox = 1102,
	Billing_Invoice_Invoices_Edit_CreateEHFinvoice = 688,
	Billing_Invoice_Invoices_Edit_ExportZetes = 1108,
	Billing_Invoice_Invoices_Edit_ExportVismaEAccounting = 1111,
	
	Billing_Invoice_Status = 168,
	Billing_Invoice_Status_Foreign = 836,
	Billing_Invoice_Status_DraftToOrigin = 198,
	Billing_Invoice_Status_OriginToPayment = 385,
	Billing_Invoice_Status_OriginToPaymentForeign = 386,
	//Billing_Invoice_Status_InvoiceToReminderAndInterest = 199, //Only in Economy from sprint 34
	Billing_Invoice_Household_ROT = 262,
	Billing_Invoice_RUT = 863,
	Billing_Invoice_InvoicesAll = 950,
	Billing_Invoice_Household = 1032,
	
	Billing_Project = 678,
	Billing_Project_TimeSheetUser = 692,
	Billing_Project_TimeSheetUser_OtherEmployees = 751,
	Billing_Project_List = 679,
	Billing_Project_Edit = 681,
	Billing_Project_Edit_Budget = 744,
	Billing_Project_Central = 680,
	Billing_Project_Central_TimeSheetUser = 1027,
	Billing_Project_Attest = 1006,
	Billing_Project_Attest_User = 1007,
	Billing_Project_Attest_Other = 1008,
	Billing_Project_ProjectsUser = 1026,
	Billing_Project_EmployeeCalculateCost = 1064,
	
	Billing_Product = 472,
	Billing_Product_Products = 185,
	Billing_Product_Products_Edit = 186,
	Billing_Product_Products_ShowPurchasePrice = 361,
	Billing_Product_Products_ShowSalesPrice = 437,
	Billing_Product_Products_ShowPriceSearch_In_Mobile = 766,
	Billing_Product_Products_FixedPrice = 996,
	Billing_Product_Products_ExtraFields = 1047,
	Billing_Product_Products_ExtraFields_Edit = 1048,
	Billing_Product_Products_BatchUpdate = 1055,
	
	Billing_Stock = 859,
	Billing_Stock_Place = 860, //Stock saldos and transactions
	//Billing_Stock_Shelf = 861, // Stock new/edit/delete
	Billing_Stock_Change_AvgPrice = 1095,
	Billing_Stock_Purchase = 1086,
	Billing_Stock_ViewAvgPriceAndValue = 1106,
	Billing_Stock_Inventory = 1109,
	Billing_Stock_Saldo = 1110,
	
	
	Billing_Purchase = 1036,
	Billing_Purchase_Purchase = 1037,
	Billing_Purchase_Purchase_List = 1038,
	Billing_Purchase_Purchase_Edit = 1039,
	Billing_Purchase_Purchase_Edit_TraceRows = 1070,
	Billing_Purchase_Delivery = 1058,
	Billing_Purchase_Delivery_List = 1059,
	Billing_Purchase_Delivery_Edit = 1060,
	Billing_Purchase_Products = 1069,
	Billing_Purchase_Products_PriceUpdate = 1104,
	Billing_Purchase_Pricelists = 1114,
	Billing_Price_Optimization = 3086,
	
	Billing_Statistics = 1003,
	Billing_Statistics_Purchase = 1097,
	Billing_Statistics_Product = 8751,
	
	Billing_Customer = 182,
	Billing_Customer_Customers = 183,
	Billing_Customer_Customers_BatchUpdate = 1056,
	Billing_Customer_Customers_Edit = 184,
	Billing_Customer_Customers_Edit_Users = 712,
	Billing_Customer_Customers_Edit_OnlyPersonal = 782,
	Billing_Customer_Customers_Edit_Documents = 1046,
	Billing_Customer_Customers_Edit_HouseholdTaxDeductionApplicants = 1068,
	
	Billing_Asset = 3090,
	Billing_Asset_List = 3091,
	
	Billing_Import = 234,
	Billing_Import_EDI = 428,
	Billing_Import_EDI_All = 953,
	Billing_Import_XEEdi = 902,
	Billing_Import_ImportSupplierAgreement = 235,
	Billing_Import_DeleteSupplierAgreement = 566,
	Billing_Import_ExcelImport = 247,
	Billing_Import_Pricelist = 676,
	Billing_Import_XEConnect = 720,
	
	Billing_Export = 187,
	Billing_Export_Email = 188,
	Billing_Export_Invoices = 562,
	Billing_Export_Invoices_SOP = 563,
	Billing_Export_Invoices_Svefaktura = 998,
	
	Billing_Distribution = 164,
	Billing_Distribution_Reports = 209,
	Billing_Distribution_Reports_Edit = 210,
	Billing_Distribution_Reports_Selection = 211,
	Billing_Distribution_Reports_Selection_Preview = 212,
	Billing_Distribution_Reports_Selection_Download = 213,
	Billing_Distribution_Reports_ReportGroupMapping = 214,
	Billing_Distribution_Reports_ReportRolePermission = 557,
	Billing_Distribution_Reports_SavePublicSelections = 1063,
	Billing_Distribution_Packages = 215,
	Billing_Distribution_Packages_Edit = 216,
	Billing_Distribution_Groups = 217,
	Billing_Distribution_Groups_Edit = 218,
	Billing_Distribution_Headers = 219,
	Billing_Distribution_Headers_Edit = 220,
	Billing_Distribution_Templates = 221,
	Billing_Distribution_Templates_Edit = 222,
	Billing_Distribution_Templates_Edit_Download = 223,
	Billing_Distribution_SysTemplates = 224,
	Billing_Distribution_SysTemplates_Edit = 225,
	Billing_Distribution_SysTemplates_Edit_Download = 226,
	
	Billing_Analysis = 1052,
	Billing_Insights = 1071,
	
	Billing_Preferences = 163,
	Billing_Preferences_CompSettings = 165,
	Billing_Preferences_PayCondition = 243,
	Billing_Preferences_PayCondition_Edit = 244,
	Billing_Preferences_DeliveryCondition = 172,
	Billing_Preferences_DeliveryCondition_Edit = 173,
	Billing_Preferences_DeliveryType = 174,
	Billing_Preferences_DeliveryType_Edit = 175,
	Billing_Preferences_EmailTemplate = 207,
	Billing_Preferences_EmailTemplate_Edit = 208,
	Billing_Preferences_Textblock = 933,
	Billing_Preferences_Textblock_Edit = 934,
	Billing_Preferences_Wholesellers = 857,
	Billing_Preferences_Wholesellers_Edit = 858,
	Billing_Preferences_ProductSettings = 281,
	Billing_Preferences_ProductSettings_Accounts = 176,
	Billing_Preferences_ProductSettings_Products = 203,
	Billing_Preferences_ProductSettings_ProductGroup = 241,
	Billing_Preferences_ProductSettings_ProductGroup_Edit = 242,
	Billing_Preferences_ProductSettings_ProductUnit = 170,
	Billing_Preferences_ProductSettings_ProductUnit_Edit = 171,
	Billing_Preferences_ProductSettings_MaterialCode = 670,
	Billing_Preferences_ProductSettings_MaterialCode_Edit = 671,
	Billing_Preferences_InvoiceSettings = 166,
	Billing_Preferences_InvoiceSettings_Pricelists = 204,
	Billing_Preferences_InvoiceSettings_Pricelists_Edit = 205,
	Billing_Preferences_InvoiceSettings_Pricelists_PriceUpdate = 1103,
	Billing_Preferences_InvoiceSettings_WholeSellerPriceList = 233,
	Billing_Preferences_InvoiceSettings_SupplierAgreement = 236,
	Billing_Preferences_InvoiceSettings_SupplierAgreement_Edit = 283,
	Billing_Preferences_InvoiceSettings_Markup = 395,
	Billing_Preferences_InvoiceSettings_PriceBasedMarkup = 1080,
	Billing_Preferences_InvoiceSettings_PriceRules = 239,
	Billing_Preferences_InvoiceSettings_PriceRules_Edit = 240,
	Billing_Preferences_InvoiceSettings_MarkupDiscount = 674,
	Billing_Preferences_EDISettings = 429,
	Billing_Preferences_InvoiceSettings_Templates = 762,
	Billing_Preferences_InvoiceSettings_Templates_Edit = 763,
	Billing_Preferences_InvoiceSettings_ShiftType = 787,
	Billing_Preferences_InvoiceSettings_ShiftType_Edit = 788,
	Billing_Preferences_ProjectSettings = 391,
	
	
	
	Time = 249,
	
	Time_Dashboard = 526,
	Time_NewsGauge = 591,
	Time_SystemInfoGauge = 592,
	Time_UploadedFiles = 602,
	Time_TimeStampAttendanceGauge = 714,
	Time_TimeStampAttendanceGauge_ShowNotStampedIn = 997,
	Time_TerminalGauge = 587,
	Time_OpenShiftsGauge = 588,
	Time_WantedShiftsGauge = 589,
	Time_MyScheduleGauge = 2020,
	Time_MyShiftsGauge = 608,
	Time_EmployeeRequestsGauge = 590,
	Time_SysScheduledJobsGauge = 765,
	
	Time_Employee = 253,
	Time_Employee_Employees = 263,
	Time_Employee_Employees_Edit = 264,
	Time_Employee_Employees_Edit_MySelf = 864,
	Time_Employee_Employees_Edit_MySelf_Contact = 866,
	Time_Employee_Employees_Edit_MySelf_Contact_DisbursementAccount = 900,
	Time_Employee_Employees_Edit_MySelf_Contact_Children = 867,
	Time_Employee_Employees_Edit_MySelf_Contact_NoExtraShift = 3074,
	Time_Employee_Employees_Edit_MySelf_Employments = 868,
	Time_Employee_Employees_Edit_MySelf_Employments_Employment = 869,
	Time_Employee_Employees_Edit_MySelf_Employments_Payroll = 870,
	Time_Employee_Employees_Edit_MySelf_Employments_Accounts = 872,
	Time_Employee_Employees_Edit_MySelf_Employments_Additions = 3075,
	Time_Employee_Employees_Edit_MySelf_AbsenceVacation = 873,
	Time_Employee_Employees_Edit_MySelf_AbsenceVacation_Absence = 874,
	Time_Employee_Employees_Edit_MySelf_AbsenceVacation_Vacation = 875,
	Time_Employee_Employees_Edit_MySelf_Tax = 876,
	Time_Employee_Employees_Edit_MySelf_Categories = 877,
	Time_Employee_Employees_Edit_MySelf_Schedule = 2043,
	Time_Employee_Employees_Edit_MySelf_Skills = 878,
	Time_Employee_Employees_Edit_MySelf_User = 879,
	Time_Employee_Employees_Edit_MySelf_Time = 880,
	Time_Employee_Employees_Edit_MySelf_Time_CalculatedCostPerHour = 881,
	Time_Employee_Employees_Edit_MySelf_Reports = 941,
	Time_Employee_Employees_Edit_MySelf_EmployeeMeeting = 943,
	Time_Employee_Employees_Edit_MySelf_Note = 882,
	Time_Employee_Employees_Edit_MySelf_UnionFee = 987,
	Time_Employee_Employees_Edit_MySelf_WorkTimeAccount = 3067,
	Time_Employee_Employees_Edit_MySelf_WorkRules = 3072,
	Time_Employee_Employees_Edit_OtherEmployees = 865,
	Time_Employee_Employees_Edit_OtherEmployees_Contact = 883,
	Time_Employee_Employees_Edit_OtherEmployees_Contact_DisbursementAccount = 901,
	Time_Employee_Employees_Edit_OtherEmployees_Contact_Children = 884,
	Time_Employee_Employees_Edit_OtherEmployees_Employments = 885,
	Time_Employee_Employees_Edit_OtherEmployees_Employments_Employment = 886,
	Time_Employee_Employees_Edit_OtherEmployees_Employments_Employment_CreateDeleteShortenExtendHibernating = 3069,
	Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll = 3030,
	Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll_Salary = 887,
	Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll_Salary_IncludeAllPeriods = 3082,
	Time_Employee_Employees_Edit_OtherEmployees_Employments_Accounts = 889,
	Time_Employee_Employees_Edit_OtherEmployees_Employments_Additions = 3076,
	Time_Employee_Employees_Edit_OtherEmployees_AbsenceVacation = 890,
	Time_Employee_Employees_Edit_OtherEmployees_AbsenceVacation_Absence = 891,
	Time_Employee_Employees_Edit_OtherEmployees_AbsenceVacation_Vacation = 892,
	Time_Employee_Employees_Edit_OtherEmployees_Tax = 893,
	Time_Employee_Employees_Edit_OtherEmployees_Categories = 894,
	Time_Employee_Employees_Edit_OtherEmployees_Schedule = 2044,
	Time_Employee_Employees_Edit_OtherEmployees_Skills = 895,
	Time_Employee_Employees_Edit_OtherEmployees_User = 896,
	Time_Employee_Employees_Edit_OtherEmployees_User_CardNumber = 927,
	Time_Employee_Employees_Edit_OtherEmployees_Time = 897,
	Time_Employee_Employees_Edit_OtherEmployees_Time_CalculatedCostPerHour = 898,
	Time_Employee_Employees_Edit_OtherEmployees_Reports = 942,
	Time_Employee_Employees_Edit_OtherEmployees_EmployeeMeeting = 944,
	Time_Employee_Employees_Edit_OtherEmployees_Note = 899,
	Time_Employee_Employees_Edit_OtherEmployees_Files = 1015,
	Time_Employee_Employees_Edit_OtherEmployees_Files_InitSigning = 1067,
	Time_Employee_Employees_Edit_OtherEmployees_UnionFee = 988,
	Time_Employee_Employees_Edit_OtherEmployees_NotAllowedToInactivate = 998,
	Time_Employee_Employees_Edit_OtherEmployees_SocialSec = 3032,
	Time_Employee_Employees_Edit_OtherEmployees_SeAllEmployees = 3034,
	Time_Employee_Employees_Edit_OtherEmployees_WorkTimeAccount = 3068,
	Time_Employee_Employees_Edit_OtherEmployees_WorkRules = 3073,
	Time_Employee_Employees_Create_Vacant_Employees = 2025,
	Time_Employee_Employees_Ratios = 2026,
	Time_Employee_SendToArbetsgivarIntygNU = 2028,
	
	Time_Employee_Groups = 265,
	Time_Employee_Groups_Edit = 266,
	Time_Employee_PayrollGroups = 760,
	Time_Employee_PayrollGroups_Edit = 761,
	Time_Employee_VacationGroups = 906,
	Time_Employee_AnnualLeaveGroups = 3085,
	Time_Employee_AnnualLeaveBalance = 3087,
	Time_Employee_EmployeeCollectiveAgreements = 2047,
	Time_Employee_Positions = 594,
	Time_Employee_Positions_Edit = 595,
	Time_Employee_Positions_CopyFromSys = 757,
	Time_Employee_EndReasons = 911,
	Time_Employee_CardNumbers = 928,
	Time_Employee_MassUpdateEmployeeFields = 3060,
	Time_Employee_PayrollReview = 840,
	Time_Employee_Csr_Export = 852,
	Time_Employee_Csr_Import = 853,
	Time_Employee_Statistics = 921,
	Time_Employee_Statistics_OtherEmployees = 982,
	Time_Employee_FollowUpTypes = 940,
	Time_Employee_Vehicles = 985,
	Time_Employee_Vehicles_Edit = 986,
	Time_Employee_SocialSec_Show = 1002,//Show social sec number in employeegrid
	Time_Employee_Skills = 1017,
	Time_Employee_EventHistory = 2030,
	Time_Employee_VacationDebt = 3048,
	Time_Employee_Accumulators = 3049,
	Time_Employee_EmploymentTypes = 3052,
	Time_Employee_EmployeeTemplates = 2046,
	Time_Employee_StampInApp = 3061,
	Time_Employee_PayrollLevels = 3062,
	Time_Employee_EvacuationList = 3070,
	Time_Employee_EvacuationList_ShowAll = 3071,
	
	Time_Schedule = 252,
	Time_Schedule_SchedulePlanningUser = 553,
	//Time_Schedule_SchedulePlanningUser_CalendarView = 701,
	//Time_Schedule_SchedulePlanningUser_DayView = 702,
	//Time_Schedule_SchedulePlanningUser_ScheduleView = 703,
	Time_Schedule_SchedulePlanningUser_HandleShiftWanted = 572,
	Time_Schedule_SchedulePlanningUser_HandleShiftWanted_AutoAssignOpenShift = 577,
	Time_Schedule_SchedulePlanningUser_HandleShiftUnwanted = 573,
	Time_Schedule_SchedulePlanningUser_HandleShiftShowQueue = 624,
	Time_Schedule_SchedulePlanningUser_HandleShiftAbsence = 574,
	Time_Schedule_SchedulePlanningUser_HandleShiftAbsence_AutoCreateAbsence = 578,
	Time_Schedule_SchedulePlanningUser_HandleShiftChangeEmployee = 575,
	Time_Schedule_SchedulePlanningUser_HandleShiftChangeEmployee_AutoAssign = 579,
	Time_Schedule_SchedulePlanningUser_HandleShiftSwapEmployee = 576,
	Time_Schedule_SchedulePlanningUser_HandleShiftSwapEmployee_AutoAssign = 580,
	Time_Schedule_SchedulePlanningUser_HandleShiftAbsenceAnnouncement = 768,
	Time_Schedule_SchedulePlanningUser_SeeOtherEmployeesShifts = 746,
	Time_Schedule_SchedulePlanningUser_SeeTimeScheduleTemplateBlockTasks = 3029,
	Time_Schedule_SchedulePlanning = 531,
	Time_Schedule_SchedulePlanning_Beta = 1119,
	Time_Schedule_SchedulePlanning_Dashboard = 913,
	Time_Schedule_SchedulePlanning_Dashboard_AssignedShiftsQuotaGauge = 914,
	Time_Schedule_SchedulePlanning_Dashboard_AbsenceQuotaGauge = 915,
	Time_Schedule_SchedulePlanning_Dashboard_ShiftTypesQuotaGauge = 916,
	Time_Schedule_SchedulePlanning_Dashboard_AdjustKPIs = 1023,
	Time_Schedule_SchedulePlanning_CalendarView = 697,
	Time_Schedule_SchedulePlanning_DayView = 698,
	Time_Schedule_SchedulePlanning_ScheduleView = 699,
	Time_Schedule_SchedulePlanning_TemplateDayView = 704,
	Time_Schedule_SchedulePlanning_TemplateScheduleView = 700,
	Time_Schedule_SchedulePlanning_EmployeePostDayView = 1018,
	Time_Schedule_SchedulePlanning_EmployeePostScheduleView = 1019,
	Time_Schedule_SchedulePlanning_ScenarioDayView = 3043,
	Time_Schedule_SchedulePlanning_ScenarioScheduleView = 3044,
	Time_Schedule_SchedulePlanning_StandbyDayView = 1030,
	Time_Schedule_SchedulePlanning_StandbyScheduleView = 1031,
	Time_Schedule_SchedulePlanning_TemplateSchedule_EditHiddenEmployee = 3042,
	Time_Schedule_SchedulePlanning_Bookings = 826,
	Time_Schedule_SchedulePlanning_StandbyShifts = 1029,
	Time_Schedule_SchedulePlanning_OnDutyShifts = 1098,
	Time_Schedule_SchedulePlanning_PreliminaryShifts = 567,
	Time_Schedule_SchedulePlanning_CopySchedule = 723,
	Time_Schedule_SchedulePlanning_LockTemplateSchedule = 753,
	Time_Schedule_SchedulePlanning_ShowCosts = 798,
	Time_Schedule_SchedulePlanning_Placement = 856,
	Time_Schedule_SchedulePlanning_ShowUnscheduledTasks = 3025,
	Time_Schedule_SchedulePlanning_LoggedWarnings = 3027,
	Time_Schedule_SchedulePlanning_SalesCalender = 3026,
	Time_Schedule_AbsenceRequestsUser = 532,
	Time_Schedule_AbsenceRequests = 533,
	Time_Schedule_Availability = 683,
	Time_Schedule_Availability_EditOnOtherEmployees = 1045,
	Time_Schedule_AvailabilityUser = 682,
	Time_Schedule_AvailabilityUser_Available = 716,
	Time_Schedule_AvailabilityUser_NotAvailable = 717,
	Time_Schedule_Templates = 280,
	Time_Schedule_Templates_Edit = 279,
	Time_Schedule_Templates_Edit_ChangeAccount = 581,
	Time_Schedule_Placement = 282,
	Time_Schedule_Placement_ChangeAccount = 582,
	Time_Schedule_Needs = 715,                      // NOT USED
	Time_Schedule_Needs_Analysis = 827,             // NOT USED
	Time_Schedule_Needs_Planning = 828,             // NOT USED
	Time_Schedule_Needs_Shifts = 829,               // NOT USED
	Time_Schedule_Needs_To_TemplateSchedule = 830,  // NOT USED
	Time_Schedule_StaffingNeeds = 2008,
	Time_Schedule_StaffingNeeds_EmployeePost = 3020,
	Time_Schedule_StaffingNeeds_Tasks = 2009,
	Time_Schedule_StaffingNeeds_TaskTypes = 3019,
	Time_Schedule_StaffingNeeds_IncomingDeliveries = 2010,
	Time_Schedule_StaffingNeeds_Analysis = 2011,
	Time_Schedule_StaffingNeeds_Planning = 2012,
	Time_Schedule_StaffingNeeds_Shifts = 2013,      // Currently inactivated in db
	Time_Schedule_StaffingNeeds_Services = 2014,
	Time_Schedule_StaffingNeeds_To_TemplateSchedule = 2015,
	Time_Schedule_StaffingNeeds_ScheduleCycleRuleType = 3023,
	Time_Schedule_StaffingNeeds_ScheduleCycle = 3024,
	Time_Schedule_Sync = 506,
	Time_Schedule_Sync_ManualSync = 507,
	Time_Schedule_Sync_Settings = 510,
	Time_Schedule_TemplateGroups = 2045,
	Time_Schedule_TimeBreakTemplate = 2016,
	
	Time_Time = 668,
	Time_Time_AttestUser = 250,
	Time_Time_AttestUser_Edit = 319,
	Time_Time_AttestUser_TimeStamp_EditTimeStampWithOutComment = 3040,
	Time_Time_AttestUser_EditSchedule = 459,
	Time_Time_AttestUser_EditAbsence = 3041,
	Time_Time_AttestUser_AbsenceDetails = 3063,
	Time_Time_AttestUser_RestoreToSchedule = 458,
	Time_Time_AttestUser_RestoreScheduleToTemplate = 747,
	Time_Time_AttestUser_ReGenerateDaysBasedOnTimeStamps = 604,
	Time_Time_AttestUser_RegenerateTransactions = 496,
	Time_Time_AttestUser_DeleteTransactions = 497,
	Time_Time_AttestUser_AdditionAndDeduction = 498,
	Time_Time_AttestUser_ShowAccumulators = 314,
	Time_Time_AttestUser_ShowPayrollTransactions = 444,
	Time_Time_AttestUser_ShowInvoiceTransactions = 445,
	Time_Time_AttestUser_ShowTimeCodeTransactions = 446,
	Time_Time_AttestUser_DontSeeFuturePlacements = 519,
	Time_Time_AttestUser_SpecifyAccountingOnDeviations = 3084,
	Time_Time_AttestUser_TimeCalendar = 936,
	Time_Time_TimeSalarySpecification = 741,
	Time_Time_Attest = 304,
	Time_Time_Attest_Edit = 439,
	Time_Time_Attest_EditSchedule = 456,
	Time_Time_Attest_EditTransactions = 3039,
	Time_Time_Attest_AbsenceDetails = 3064,
	Time_Time_Attest_RestoreToSchedule = 438,
	Time_Time_Attest_RestoreScheduleToTemplate = 748,
	Time_Time_Attest_RestoreFromTimeStamps = 603,
	Time_Time_Attest_RegenerateTransactions = 494,
	Time_Time_Attest_DeleteTransactions = 495,
	Time_Time_Attest_AdditionAndDeduction = 493,
	Time_Time_Attest_ShowAccumulators = 440,
	Time_Time_Attest_ShowTimeCodeTransactions = 443,
	Time_Time_Attest_ShowInvoiceTransactions = 442,
	Time_Time_Attest_ShowPayrollTransactions = 441,
	Time_Time_Attest_ReverseTransactions = 785,
	Time_Time_Attest_SendAttestReminder = 3050,
	Time_Time_Attest_RecalculateAccunting = 3059,
	Time_Time_Attest_TimeCalendar = 937,
	Time_Time_Attest_TimeStamp = 516,
	Time_Time_Attest_TimeStamp_EditTimeStampEntries = 517,
	Time_Time_Attest_TimeStamp_EditTimeStampEntryTime = 518,
	Time_Time_Attest_TimeStamp_EditOthersTimeStampEntries = 522,
	Time_Time_Attest_TimeStamp_EditOthersTimeStampEntryTime = 523,
	Time_Time_Attest_TimeStamp_DiscardBreakEvaluation = 529,
	Time_Time_Attest_TimeStamp_CreateTimeStampsAccourdingToSchedule = 750,
	Time_Time_Attest_TimeStamp_EditTimeStampWithOutComment = 922,
	Time_Time_Attest_AdjustTimeStamps = 554,
	Time_Time_Attest_Recalculate = 643,
	Time_Time_Attest_Overview = 3045,
	Time_Time_Attest_SpecifyAccountingOnDeviations = 3083,
	
	Time_Time_TimeCalendar = 935,
	Time_Time_TimeSheetUser = 691,
	Time_Time_TimeSheetUser_OtherEmployees = 752,
	Time_Time_EarnedHoliday = 2007,
	Time_Time_TimeWorkReduction = 3088,
	
	//Time_Travel = 968,
	//Time_Travel_TravelExpenseUser = 969,
	//Time_Travel_TravelExpense = 970,
	//Time_Travel_EventsFollowUp = 971,
	//Time_Travel_Propellant = 972,
	//Time_Travel_TravelAdvanceApply = 973,
	//Time_Travel_TravelAdvance = 974,
	//Time_Travel_CardTransactionsImport = 975,
	//Time_Travel_CardTransactionsControl = 976,
	//Time_Travel_CardConnections = 977,
	//Time_Travel_Events = 978,
	//Time_Travel_Expense = 979,
	//Time_Travel_Distance = 980,
	//Time_Travel_NormalAmountsTax = 981,
	
	Time_Payroll = 818,
	Time_Payroll_CalculationUser = 819,
	Time_Payroll_CalculationUser_Edit = 821,
	Time_Payroll_CalculationUser_ShowAccumulators = 822,
	Time_Payroll_CalculationUser_TimeCalendar = 939,
	Time_Payroll_Calculation = 820,
	Time_Payroll_Calculation_Edit = 823,
	Time_Payroll_Calculation_ShowAccumulators = 824,
	Time_Payroll_Calculation_FixedPayrollRows = 848,
	Time_Payroll_Calculation_TimeCalendar = 938,
	Time_Payroll_Payment = 926,
	Time_Payroll_MassRegistration = 931,
	Time_Payroll_Retroactive = 3031,
	Time_Payroll_VacationYearEnd = 966,
	Time_Payroll_UnionFee = 984,
	Time_Payroll_TimeWorkAccount = 3066,
	Time_Payroll_Provision = 993,
	Time_Payroll_Provision_AccountProvisionBase = 990,
	Time_Payroll_Provision_AccountProvisionTransaction = 992,
	
	Time_Project = 315,
	Time_Project_List = 316,
	Time_Project_Edit = 317,
	Time_Project_Invoice_Edit = 392,
	Time_Project_Invoice_WorkedTime = 393,
	Time_Project_Invoice_InvoicedTime = 555,
	Time_Project_Invoice_ShowAllPersons = 394,
	Time_Project_Invoice_ShowProjectsWithoutCustomer = 786,
	Time_Project_Central = 677,
	
	Time_Import = 254,
	Time_Import_Salary = 512,
	Time_Import_PayrollStartValuesImport = 994, //No longer used --- USED IN TYPESCRIPT code...
	Time_Import_PayrollStartValuesImported = 995,
	Time_Import_PayrollImport = 3051,
	Time_Import_ExcelImport = 520,
	Time_Import_XEConnect = 745,
	Time_Import_API = 3047,
	
	Time_Export = 255,
	Time_Export_Salary = 358,
	Time_Export_XEConnect = 370,
	Time_Export_StandardDefinitions = 371,
	
	Time_Distribution = 256,
	Time_Distribution_Reports = 338,
	Time_Distribution_Reports_Edit = 339,
	Time_Distribution_Reports_Selection = 340,
	Time_Distribution_Reports_Selection_Preview = 341,
	Time_Distribution_Reports_Selection_Download = 342,
	Time_Distribution_Reports_ReportGroupMapping = 343,
	Time_Distribution_Reports_ReportRolePermission = 558,
	Time_Distribution_Reports_SavePublicSelections = 1061,
	Time_Distribution_Packages = 344,
	Time_Distribution_Packages_Edit = 345,
	Time_Distribution_Groups = 346,
	Time_Distribution_Groups_Edit = 347,
	Time_Distribution_Headers = 348,
	Time_Distribution_Headers_Edit = 349,
	Time_Distribution_Templates = 350,
	Time_Distribution_Templates_Edit = 351,
	Time_Distribution_Templates_Edit_Download = 352,
	Time_Distribution_SysTemplates = 353,
	Time_Distribution_SysTemplates_Edit = 354,
	Time_Distribution_SysTemplates_Edit_Download = 355,
	
	Time_Analysis = 1054,
	Time_Insights = 1073,
	
	Time_Preferences = 257,
	Time_Preferences_TimeSetupWizard = 515,
	Time_Preferences_CompSettings = 258,
	//Time_Preferences_ProjectSettings = 391,
	Time_Preferences_ScheduleSettings = 362,
	Time_Preferences_ScheduleSettings_ShiftType = 547,
	Time_Preferences_ScheduleSettings_ShiftType_Edit = 548,
	Time_Preferences_ScheduleSettings_ShiftTypeLink = 3022,
	Time_Preferences_ScheduleSettings_SkillType = 549,
	Time_Preferences_ScheduleSettings_SkillType_Edit = 551,
	Time_Preferences_ScheduleSettings_Skill = 550,
	Time_Preferences_ScheduleSettings_Skill_Edit = 552,
	Time_Preferences_ScheduleSettings_DayTypes = 363,
	Time_Preferences_ScheduleSettings_DayTypes_Edit = 364,
	Time_Preferences_ScheduleSettings_Holidays = 366,
	Time_Preferences_ScheduleSettings_Holidays_Edit = 367,
	Time_Preferences_ScheduleSettings_Halfdays = 288,
	Time_Preferences_ScheduleSettings_Halfdays_Edit = 289,
	Time_Preferences_ScheduleSettings_IncomingDeliveryType = 2006,
	Time_Preferences_ScheduleSettings_LeisureCodeType = 3077,
	Time_Preferences_ScheduleSettings_LeisureCodeType_Edit = 3079,
	Time_Preferences_ScheduleSettings_LeisureCode = 3078,
	Time_Preferences_ScheduleSettings_LeisureCode_Edit = 3080,
	Time_Preferences_NeedsSettings = 729,
	Time_Preferences_NeedsSettings_LocationGroups = 732,
	Time_Preferences_NeedsSettings_LocationGroups_Edit = 733,
	Time_Preferences_NeedsSettings_Locations = 730,
	Time_Preferences_NeedsSettings_Locations_Edit = 731,
	Time_Preferences_NeedsSettings_Rules = 735,
	Time_Preferences_NeedsSettings_Rules_Edit = 736,
	Time_Preferences_TimeSettings = 259,
	Time_Preferences_TimeSettings_Accounts = 292,
	Time_Preferences_TimeSettings_TimeTerminals = 389,
	Time_Preferences_TimeSettings_TimeTerminals_Edit = 390,
	Time_Preferences_TimeSettings_TimeTerminals_StampingStatistics = 743,
	Time_Preferences_TimeSettings_TimePeriodHead = 312,
	Time_Preferences_TimeSettings_TimePeriodHead_Edit = 313,
	Time_Preferences_TimeSettings_PlanningPeriod = 2021,
	Time_Preferences_TimeSettings_TimeAccumulator = 286,
	Time_Preferences_TimeSettings_TimeAccumulator_Edit = 287,
	Time_Preferences_TimeSettings_InvoiceProduct = 359,
	Time_Preferences_TimeSettings_InvoiceProduct_Edit = 360,
	Time_Preferences_TimeSettings_TimeCodeWork = 269,
	Time_Preferences_TimeSettings_TimeCodeWork_Edit = 270,
	Time_Preferences_TimeSettings_TimeCodeAbsense = 271,
	Time_Preferences_TimeSettings_TimeCodeAbsense_Edit = 272,
	Time_Preferences_TimeSettings_TimeDeviationCause = 275,
	Time_Preferences_TimeSettings_TimeDeviationCause_Edit = 276,
	Time_Preferences_TimeSettings_TimeScheduleType = 718,
	Time_Preferences_TimeSettings_TimeScheduleType_Edit = 719,
	Time_Preferences_TimeSettings_TimeCodeBreak = 273,
	Time_Preferences_TimeSettings_TimeCodeBreak_Edit = 274,
	Time_Preferences_TimeSettings_TimeCodeBreakGroup = 789,
	Time_Preferences_TimeSettings_TimeCodeBreakGroup_Edit = 790,
	Time_Preferences_TimeSettings_TimeCodeAdditionDeduction = 501,
	Time_Preferences_TimeSettings_TimeCodeAdditionDeduction_Edit = 502,
	Time_Preferences_TimeSettings_TimeRuleGroup = 290,
	Time_Preferences_TimeSettings_TimeRuleGroup_Edit = 291,
	Time_Preferences_TimeSettings_TimeAbsenceRule = 499,
	Time_Preferences_TimeSettings_TimeAbsenceRule_Edit = 500,
	Time_Preferences_TimeSettings_TimeRule = 284,
	Time_Preferences_TimeSettings_TimeRule_Edit = 285,
	Time_Preferences_TimeSettings_TimeCodeRanking = 3089,
	//Time_Preferences_TimeSettings_TimeRule_ExportImport = 2027,
	Time_Preferences_SalarySettings = 261,
	Time_Preferences_SalarySettings_PayrollProduct = 268,
	Time_Preferences_SalarySettings_PayrollProduct_Edit = 277,
	Time_Preferences_SalarySettings_PayrollProduct_MassUpdate = 3065,
	Time_Preferences_SalarySettings_PriceType = 758,
	Time_Preferences_SalarySettings_PriceType_Edit = 759,
	Time_Preferences_SalarySettings_PriceFormula = 811,
	Time_Preferences_SalarySettings_PriceFormula_Edit = 812,
	
	
	
	Communication = 538,
	
	Communication_Dashboard = 544,
	
	Communication_XEmail = 539,
	Communication_XEmail_Inbox = 541,
	Communication_XEmail_Send = 542,
	Communication_XEmail_Delete = 543,
	Communication_XEmail_Send_SMS = 600,
	
	Communication_Support = 724,
	Communication_Support_Cases = 725,
	Communication_Support_Cases_Edit = 726,
	
	
	
	Estatus = 62,
	
	
	
	Manage = 63,
	
	Manage_Dashboard = 527,
	Manage_SysScheduledJobsGauge = 764,
	Manage_CompanyNewsGauge = 808,  // NOT USED
	Manage_UploadedFilesGauge = 809,
	Manage_SysLogGauge = 813,
	Manage_StatisticsGauge = 843,
	Manage_PerformanceMonitorGauge = 910,
	Manage_PerformanceAnalyzerGauge = 1016,
	Manage_TaskWatchLogGauge = 1021,
	
	Manage_Contracts = 64,
	Manage_Contracts_Edit = 65,
	Manage_Contracts_Edit_Permission = 66,
	
	Manage_Companies = 67,
	Manage_Companies_Edit = 68,
	Manage_Companies_Edit_Permission = 69,
	
	Manage_Roles = 70,
	Manage_Roles_Edit = 71,
	Manage_Roles_Edit_Permission = 72,
	
	Manage_Users = 73,
	Manage_Users_Edit = 74,
	Manage_Users_Edit_UserMapping = 75,
	Manage_Users_Edit_AttestRoleMapping = 303,
	Manage_Users_Edit_AttestReplacementMapping = 645,
	Manage_Users_Edit_Delegate_MySelf = 2032,
	Manage_Users_Edit_Delegate_MySelf_OwnRolesAndAttestRoles = 2033,
	Manage_Users_Edit_Delegate_MySelf_AllRoles = 2034,
	Manage_Users_Edit_Delegate_MySelf_AllAttestRoles = 2035,
	Manage_Users_Edit_Delegate_OtherUsers = 2036,
	Manage_Users_Edit_Delegate_OtherUsers_OwnRolesAndAttestRoles = 2037,
	Manage_Users_Edit_Delegate_OtherUsers_AllRoles = 2038,
	Manage_Users_Edit_Delegate_OtherUsers_AllAttestRoles = 2039,
	Manage_Users_Edit_Password = 76,    // NOT_USED
	Manage_Users_Edit_Sessions = 77,
	Manage_Users_SimplifiedRegistration = 847,
	Manage_Users_ServiceUsers = 2052,
	Manage_Users_ServiceUsers_Edit = 2053,
	
	Manage_ContactPersons = 78,
	Manage_ContactPersons_Edit = 79,
	
	Manage_Attest = 649,
	Manage_Attest_Time = 305,
	Manage_Attest_Time_AttestRoles = 306,
	Manage_Attest_Time_AttestRoles_Edit = 307,
	Manage_Attest_Time_AttestStates = 308,
	Manage_Attest_Time_AttestStates_Edit = 309,
	Manage_Attest_Time_AttestTransitions = 310,
	Manage_Attest_Time_AttestTransitions_Edit = 311,
	Manage_Attest_Time_AttestRules = 464,
	Manage_Attest_Time_AttestRules_Edit = 465,
	Manage_Attest_Customer = 430,
	Manage_Attest_Customer_AttestRoles = 431,
	Manage_Attest_Customer_AttestRoles_Edit = 432,
	Manage_Attest_Customer_AttestStates = 433,
	Manage_Attest_Customer_AttestStates_Edit = 434,
	Manage_Attest_Customer_AttestTransitions = 435,
	Manage_Attest_Customer_AttestTransitions_Edit = 436,
	Manage_Attest_Supplier = 295,
	Manage_Attest_Supplier_AttestRoles = 296,
	Manage_Attest_Supplier_AttestRoles_Edit = 297,
	Manage_Attest_Supplier_AttestStates = 298,
	Manage_Attest_Supplier_AttestStates_Edit = 299,
	Manage_Attest_Supplier_AttestTransitions = 300,
	Manage_Attest_Supplier_AttestTransitions_Edit = 301,
	Manage_Attest_Supplier_WorkFlowTemplate = 606,
	Manage_Attest_Supplier_WorkFlowTemplate_Edit = 607,
	Manage_Attest_Supplier_WorkFlowTemplate_Supplier = 845,
	Manage_Attest_CaseProject = 791,
	Manage_Attest_CaseProject_AttestRoles = 792,
	Manage_Attest_CaseProject_AttestRoles_Edit = 793,
	Manage_Attest_CaseProject_AttestStates = 794,
	Manage_Attest_CaseProject_AttestStates_Edit = 795,
	Manage_Attest_CaseProject_AttestTransitions = 796,
	Manage_Attest_CaseProject_AttestTransitions_Edit = 797,
	
	Manage_Signing = 3053,
	Manage_Signing_Document = 3054,
	Manage_Signing_Document_Roles = 3055,
	Manage_Signing_Document_States = 3056,
	Manage_Signing_Document_Transitions = 3057,
	Manage_Signing_Document_Templates = 3058,
	
	Manage_Preferences = 80,
	Manage_Preferences_LicenseSettings = 2029,
	Manage_Preferences_CompSettings = 81,
	Manage_Preferences_CompSettings_Zetes = 1107,
	Manage_Preferences_CompSettings_Intrum = 1113,
	
	Manage_Preferences_SystemInfoSettings = 599,
	Manage_Preferences_FieldSettings = 631,
	Manage_Preferences_FieldSettings_Edit = 632,
	Manage_Preferences_LogotypeSettings = 177,
	Manage_Preferences_CheckSettings = 457,
	Manage_Preferences_CompanyInformation = 1024,
	Manage_Preferences_CompanyNews = 597,       // NOT USED
	Manage_Preferences_CompanyNews_Edit = 598,  // NOT USED
	Manage_Preferences_UploadedFiles = 601,
	
	Manage_Preferences_Registry = 83,
	Manage_Preferences_Registry_DayTypes = 84,
	Manage_Preferences_Registry_DayTypes_Edit = 85,
	Manage_Preferences_Registry_Holidays = 86,
	Manage_Preferences_Registry_Holidays_Edit = 87,
	Manage_Preferences_Registry_SchoolHoliday = 2004,
	Manage_Preferences_Registry_OpeningHours = 2005,
	Manage_Preferences_Registry_Checklists = 611,
	Manage_Preferences_Registry_Checklists_Edit = 612,
	Manage_Preferences_Registry_EventReceiverGroups = 2019,
	Manage_Preferences_Registry_ScheduledJobs = 2031,
	Manage_Preferences_Registry_ExportDefinitions = 672,
	Manage_Preferences_Registry_ExportDefinitions_Edit = 673,
	Manage_Preferences_Registry_Positions = 755,
	Manage_Preferences_Registry_Positions_Edit = 756,
	Manage_Preferences_Registry_ExternalCodes = 1051,
	Manage_Preferences_Registry_SignatoryContract = 124,
	Manage_Preferences_Registry_SignatoryContract_Edit = 125,
	
	Manage_Support = 88,
	Manage_Support_Logs = 89,
	Manage_Support_Logs_Edit = 90,
	Manage_Support_Logs_System = 107,
	Manage_Support_Logs_License = 108,
	Manage_Support_Logs_Company = 109,
	Manage_Support_Logs_Role = 110,
	Manage_Support_Logs_User = 111,
	Manage_Support_Logs_Machine = 112,
	
	Manage_Support_Cases = 737,         // NOT USED
	Manage_Support_Cases_Edit = 738,    // NOT USED
	Manage_Support_Sprints = 816,       // NOT USED
	Manage_Support_Sprints_Edit = 817,  // NOT USED
	
	Manage_System = 206,
	Manage_System_Intrastat = 1089,
	Manage_System_Intrastat_StatisticalCommodityCodes = 1090,
	Manage_System_BankIntegration = 2000,
	Manage_System_Communicator = 2001,
	Manage_System_Price_List = 2002,
	
	Manage_GDPR = 3035,
	Manage_GDPR_Logs = 3036,
	Manage_GDPR_Registry = 3037,
	Manage_GRPR_Registry_HandlePersonalInfo = 3038,
	
	Manage_Logs = 2040,
	Manage_Logs_ChangeLogs = 2041,
	Manage_Logs_ChangeLogs_Search = 2042,
	
	
	
	Common = 91,
	
	Common_Field = 92,
	Common_Field_Role = 113,
	Common_Field_Company = 114,
	
	Common_Form = 93,
	Common_Form_Role = 115,
	Common_Form_Company = 116,
	
	Common_Help = 466,
	Common_Help_Modify = 467,
	Common_Help_Show_Instructional_Videos_In_Mobile = 767,
	
	Common_Language = 508,
	
	Common_Categories = 651,
	Common_Categories_Product = 656,
	Common_Categories_Product_Edit = 661,
	Common_Categories_Customer = 652,
	Common_Categories_Customer_Edit = 657,
	Common_Categories_Supplier = 653,
	Common_Categories_Supplier_Edit = 658,
	Common_Categories_ContactPersons = 654,
	Common_Categories_ContactPersons_Edit = 659,
	Common_Categories_AttestRole = 662,
	Common_Categories_AttestRole_Edit = 663,
	Common_Categories_Employee = 655,
	Common_Categories_Employee_Edit = 660,
	Common_Categories_Project = 664,
	Common_Categories_Project_Edit = 665,
	Common_Categories_Contract = 666,
	Common_Categories_Contract_Edit = 667,
	Common_Categories_Inventory = 490,
	Common_Categories_Inventory_Edit = 491,
	Common_Categories_Order = 783,
	Common_Categories_Order_Edit = 784,
	Common_Categories_PayrollProduct = 841,
	Common_Categories_PayrollProduct_Edit = 842,
	Common_Categories_Document = 1122,
	Common_Categories_Document_Edit = 1123,
	
	Common_ExtraFields = 1074,
	Common_ExtraFields_Supplier = 1075,
	Common_ExtraFields_Supplier_Edit = 1076,
	Common_ExtraFields_Customer = 1077,
	Common_ExtraFields_Customer_Edit = 1078,
	Common_ExtraFields_Employee = 1081,
	Common_ExtraFields_Employee_Edit = 1082,
	Common_ExtraFields_Account = 2048,
	Common_ExtraFields_Account_Edit = 2049,
	Common_ExtraFields_PayrollProductSetting = 1120,
	
	Common_HideStateAnalysis = 742,
	
	Common_Dashboard = 1084,
	Common_Dashboard_Insight = 1085,
	
	Common_AI = 2051,
	
	
	
	Archive = 903,
	
	
	
	Statistics = 904,
	
	
	ClientManagement = 9000,
	ClientManagement_Clients = 9001,
	ClientManagement_Suppliers = 9002,
	ClientManagement_Supplier_Invoices = 9003,
	
};

export enum SoeFeatureType {
	None = 0,
	License = 1,
	Company = 2,
	Role = 3,
	
	SysXEArticle = 11,
};

export enum Permission {
	None = 0,
	Readonly = 1,
	Modify = 2,
};

export enum FeaturePermissionType {
	Unknown = 0,
	License = 1,
	Company = 2,
	Role = 3,
}

export enum UnauthorizationType {
	None = 0,
	FeaturePermissionMissing = 1,
	DataAuthorityMissing = 2,
	ReportPermissionMissing = 3,
	UnknownLogin = 4,
}

export enum RemoteLoginFailedType {
	None = 0,
	Failed = 1,
	NotAllowed = 2,
	InvalidLicense = 3, //Can only happen if HTML DOM is manipulated
	InvalidServer = 4,
}

export enum FontAwesomeIconSource {
	FontAwesome_Kit = 0,
	FontAwesome_CDN = 1,
	SoftOne_Embedded = 2
}

export enum HelpType {
	Label = 0,
	WaterMark = 1,
	ToolTip = 2,
	ShortHelp = 3,
	Help = 4,
}

export enum IntrastatReportingType {
	None = 0,
	Import = 1,
	Export = 2,
	Both = 3,
}

export enum ImportPaymentIOStatus {
	None = 0,
	Match = 1,
	Rest = 2,
	PartlyPaid = 3,
	Unknown = 4,
	FullyPaid = 5,
	Paid = 6,
	Error = 7,
	Manual = 8,
	Deleted = 9,
	ManuallyHandled = 10,
}

export enum ImportPaymentIOState {
	Open = 0,
	Closed = 1,
	Deleted = 2
}

export enum ImportPaymentType {
	None = 0,
	CustomerPayment = 1,
	SupplierPayment = 2,
	Sepa = 3,
	Professional = 4
}

export enum InExchangeStatusType {
	PendingInPlatform = 0,
	Sent = 1,
	Error = 2,
	Stopped = 3,
	Unknown = 4,
}

export enum SoeInformationSourceType {
	Company = 1,    // Internal information
	Sys = 2         // Information from SoftOne
}

export enum SoeInformationType {
	Information = 1,
}

export enum InputLoadType {
	None = 0,
	
	//Base
	Holiday,
	DayType,
	
	//Time
	TemplateSchedule,
	Schedule,
	Shifts ,
	GrossNetCost,
	TimeStamps,
	TimeBlocks,
	ProjectTimeBlocks,
	TimeCodeTransactions,
	TimePayrollTransactions,
	TimeInvoiceTransactions,
	AttestState,
	AttestTransitionLog,
	PresenceAbsenceDetails,
	UnhandledShiftChanges,
	
	//Sums expense
	SumsAll,
	SumExpenseRows,                              // Utlägg rader
	//Sums time
	SumExpenseAmount,                            // Utlägg belopp
	SumTimeWorkedScheduledTime,                  // Arbetad schematid
	SumTimeAccumulator,                          // Tidbank
	SumTimeAccumulatorOverTime,                  // Tidbank övertid
	//Sums absence
	SumGrossSalaryAbsence,                       // Frånvaro
	SumGrossSalaryAbsenceVacation,               // Semester
	SumGrossSalaryAbsenceSick,                   // Sjuk
	SumGrossSalaryAbsenceParentalLeave,          // Föräldraledig
	SumGrossSalaryAbsenceLeaveOfAbsence,         // Tjänstledig
	SumGrossSalaryAbsenceTemporaryParentalLeave, // Tillfällig föräldrapenning
	//Sums weekend salary
	SumGrossSalaryWeekendSalary,                 // Helglön
	//Sums duty
	SumGrossSalaryDuty,                          // Jour
	//Sums additional time
	SumGrossSalaryAdditionalTime,                // Mertid/fyllnadstid
	SumGrossSalaryAdditionalTime35,              // Mertid/fyllnadstid 35%
	SumGrossSalaryAdditionalTime70 ,              // Mertid/fyllnadstid 70%
	SumGrossSalaryAdditionalTime100,             // Mertid/fyllnadstid 100%
	//Sums OB addition
	SumGrossSalaryOBAddition,                    // OB-tillägg
	SumGrossSalaryOBAddition40,                  // OB-tillägg 40%
	SumGrossSalaryOBAddition50,                  // OB-tillägg 50%
	SumGrossSalaryOBAddition57,                  // OB-tillägg 57%
	SumGrossSalaryOBAddition70,                  // OB-tillägg 70%
	SumGrossSalaryOBAddition79,                  // OB-tillägg 79%
	SumGrossSalaryOBAddition100,                 // OB-tillägg 100%
	SumGrossSalaryOBAddition113,                 // OB-tillägg 113%
	//Sums overtime
	SumGrossSalaryOvertime,                      // Övertid
	SumGrossSalaryOvertime35,                    // Övertid 35%
	SumGrossSalaryOvertime50,                    // Övertid 50%
	SumGrossSalaryOvertime70,                    // Övertid 70%
	SumGrossSalaryOvertime100,                   // Övertid 100%
	//Sums invoiced time
	SumInvoicedTime,                             // Fakturerad tid
}

export enum InvoiceAttachmentSourceType {
	None = 0,
	Edi = 1,
	DataStorage = 2,
}

export enum InvoiceAttachmentConnectType {
	Manual = 0,
	Edi = 1,
	SupplierInvoice = 2,
}

export enum InvoiceRowInfoFlag {
	None = 0,
	Info = 1,
	Error = 2,
	HouseHold = 4,
}

export enum InvoiceTextType {
	Unknown = 0,
	SupplierInvoiceBlockReason = 1,
	UnderInvestigationReason = 2,
}

export enum LiquidityPlanningTransactionType {
	None = 0,
	IncomingBalance = 1,
	CustomerInvoice = 2,
	SupplierInvoice = 3,
	Manual = 4,
	TotalBalance = 5,
}

export enum MatrixDataType {
	String = 1,
	Integer = 2,
	Boolean = 3,
	Date = 4,
	Decimal = 5,
	Time = 6,
	DateAndTime = 7,
};

export enum MatrixFieldSetting {
	BackgroundColor = 1,
	FontColor = 2,
	BoldFont = 3,
	AlignLeft = 4,
	AlignRight = 5,
	ClearZero = 6,
	Decimals = 7,
	MinutesToDecimal = 8,
	MinutesToTimeSpan = 9
}

export enum AnalysisMode {
	Analysis = 0,
	Insights = 1
}

export enum MapLocationType {
	Unknown = 0,
	GPSLocation = 1,
}

export enum MapItemType {
	Default = 0,
	Employee = 1,
	Order = 2,
}

export enum MenuItemType {
	TopLink = 1,
	SubHeader = 2,
	SubLink = 3,
}

export enum MenuSelectorType {
	Company = 1,
	Role = 2,
	Module = 3,
	Language = 4,
}

export enum TopMenuSelectorType {
	Role = 2,
	User = 3,
	Favorites = 4,
	Help = 5,   // NOT USED
	AccountYear = 6,
	PageStatusBeta = 7,
	PageStatusLive = 8,
	AccountHierarchy = 9,
}

export enum MultiSelectDialogType {
	EmployeeGroup = 1,
	DayType = 2,
	TimeDeviationCause = 3,
	TimeScheduleType = 4,
}

export enum NameStandard {
	Unknown = 0,
	FirstNameThenLastName = 1,
	LastNameThenFirstName = 2,
}

export enum OrderInvoiceRegistrationType {
	Unknown = 0,
	Invoice = 1, // The invoice is registered as an invoice (with product rows as primary source)
	Ledger = 2,  // The invoice is registered as a ledger (with accounting rows as primary source)
	Offer = 3,   // The invoice is an offer
	Order = 4,   // The invoice is an order
	Contract = 5 // The invoice is a contract
};

export enum OrderContractType {
	Continuous = 0,
	Fixed = 1,
};

export enum OriginUserStatus {
	Undefined = 0,
	UnRead = 1,
	Read = 2
};

export enum ProjectCentralBudgetRowType {
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

export enum ProjectCentralHeaderGroupType {
	None = 0,
	Time = 1,
	IncomeNotInvoiced = 2,
	IncomeInvoiced = 3,
	CostsMaterial = 4,
	CostsPersonell = 5,
	CostsExpense = 6,
	CostsOverhead = 7
}

export enum ProjectCentralStatusRowType {
	None = 0,
	TimeValueRow = 1,
	TimeSummaryRow = 2,
	AmountValueRow = 3,
	AmountSummaryRow = 4,
	SeparatorRow = 5
}

export enum ProductRowsContainers {
	Offer = 1,
	Order = 2,
	Invoice = 3,
	Contract = 4,
	Purchase = 5,
}

export enum PayrollStartValueUpdateType {
	None = 0,
	Insert = 1,
	OverWrite = 2,
}

export enum PersonalDataBatchType {
	Unspecified = 0,
	Mobile = 1,
	ReportPdf = 2,
	ReportFile = 3,
	ReportRaw = 4,
}

export enum PurchaseDeliveryStatus {
	Unknown = 0,
	Late = 8,
	PartlyDelivered = 9,
	Delivered = 10,
	Accepted = 11
};

export enum PurchaseCustomerInvoiceViewType {
	Unknown = 0,
	FromPurchase = 1,
	FromPurchaseDelivery = 2,
	FromCustomerInvoice = 3,
	FromCustomerInvoiceRow = 4,
};

export enum ReadSoftPartiesType {
	Unknown = 0,
	Buyer = 1,
	Supplier = 2,
}

export enum ScanningEntryRowType {
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
	BankNr = 23,
}

export enum ScanningFeebackType {
	Other = 0,
	SupplierNotFound = 1,
	InterpretationProblem = 2,
	ValidationProblem = 3,
}

export enum ScanningLogLevel {
	NotSent = 0,
	SentWithException = 1,
	SentWithFalseReturn = 2,
	SentWithTrueReturn = 3,
	DoNotSend = 99,
}

export enum ScanningProvider {
	Unknown = 0,
	ReadSoft = 1,
	AzoraOne = 2
}

export enum AzoraOneStatus {
	Deactivated = 0,
	ActivatedInBackground = 1,
	ActivatedWithAlternative = 2,
	ActivatedWithoutAlternative = 3,
}

export enum SoeSelectionData {
	//Date_AccountYear = 1,
	//Date_AccountPeriod = 2,
	//Date_Date = 3,
	
	//Account
	Str_Account = 11,
	
	//Voucher
	Int_Voucher_VoucherSeriesId = 21,
	Int_Voucher_VoucherNr = 22,
	
	//Budget
	Int_BudgetId = 25,
	
	//Ledger
	Str_Ledger_ActorNr = 31,
	Int_Ledger_InvoiceSeqNr = 32,
	Int_Ledger_DateRegard = 33,
	Int_Ledger_SortOrder = 34,
	Int_Ledger_InvoiceSelection = 35,
	
	//Billing
	Str_Billing_CustomerNr = 41,
	Str_Billing_InvoiceNr = 42,
	Int_Billing_SortOrder = 43,
	Str_Billing_ProjectNr = 44,
	Str_Billing_EmployeeNr = 45,
	Str_Billing_ProductNr = 46,
	Int_Billing_StockLocationId = 47,
	Int_Billing_StockShelfId = 47,
	Str_Billing_Period = 49,
	
	//Time
	Int_Time_EmployeeId = 51,
	Int_Time_CategoryId = 52,
	Int_Time_ShiftTypeIds = 53,
	Int_Time_PayrollProductId = 54,
	
	//Export
	Int_Export_Type = 60,
	Int_Export_File_Type = 61,
	
	
};

export enum SoeSelectionType {
	None = 0,
	
	Accounting = 1,
	Accounting_ExcludeVoucher = 2,
	Accounting_ExcludeVoucherAndDate = 3,
	Accounting_ExcludeVoucherAccountAndDate = 4,
	Accounting_FixedAssets = 5,
	
	Ledger_Supplier = 11,
	Ledger_Customer = 12,
	Ledger_Supplier_ExcludeAllButNr = 13,
	Ledger_Customer_ExcludeAllButNr = 14,
	
	Billing_Invoice = 21,
	Billing_HousholdTaxDeduction = 22,
	Billing_ProjectTransactions = 23,
	Billing_Stock = 24,
	
	Time_Report = 31,
};

export enum SoeReportSortOrder {
	SeqNr = 1,
	ActorNr = 2,
	ActorName = 3,
	Date = 4,
}

export enum SoeReportType {
	// See SysReportType and SysTermGroup 16
	Unknown = 0,
	CrystalReport = 1,
	Analysis = 2,
	ExportFile = 3,
	ApiResult = 4,
	DevExpressReport = 5
};

export enum SoeReportTemplateType {
	// See SysReportTemplateType and SysTermGroup 19
	
	// CrystalReports
	Unknown = 0,
	VoucherList = 1,
	GeneralLedger = 2,
	BalanceReport = 3,
	ResultReport = 4,
	TaxAudit = 5,
	SruReport = 6,
	SupplierBalanceList = 7,
	SupplierInvoiceJournal = 8,
	CustomerBalanceList = 9,
	CustomerInvoiceJournal = 10,
	BillingInvoice = 11,
	BillingInvoiceInterest = 12,
	HousholdTaxDeduction = 13,
	TimeMonthlyReport = 14,
	TimePayrollTransactionReport = 15,
	TimeEmployeeSchedule = 16,
	TimeCategorySchedule = 17,
	HouseholdTaxDeductionFile = 18,
	TimeProjectReport = 19,
	BillingOrder = 20,
	BillingOffer = 21,
	ProjectStatisticsReport = 22,
	SymbrioEdiSupplierInvoice = 23,
	TimeAccumulatorReport = 24,
	BillingContract = 25,
	ReadSoftScanningSupplierInvoice = 26,
	CustomerPaymentJournal = 27,
	SupplierPaymentJournal = 28,
	TimeCategoryStatistics = 29,
	TimeStampEntryReport = 30,
	FinvoiceEdiSupplierInvoice = 31,
	TimeSalarySpecificationReport = 32, // Classic
	Svefaktura = 33,
	SEPA = 34,
	Finvoice = 35,
	OrderChecklistReport = 36,
	BillingInvoiceReminder = 37,
	ProjectTransactionsReport = 38,
	EfhInvoice = 39,
	TimeSalaryControlInfoReport = 40,
	SupplierInvoiceImage = 41,
	//Empty = 42,
	EmployeeListReport = 43,
	CustomerListReport = 44,
	SupplierListReport = 45,
	TimeEmployeeTemplateSchedule = 46,
	TimeEmploymentContract = 47,
	OriginStatisticsReport = 48,
	PayrollSlip = 49,
	TimeScheduleBlockHistory = 50,
	TaxAudit_FI = 51,
	CSR = 52,
	UserListReport = 53,
	IOVoucher = 54,
	IOCustomerInvoice = 55,
	//IOSupplierInvoice = 56,
	ConstructionEmployeesReport = 57,
	TimeSaumaSalarySpecificationReport = 58,
	PayrollTransactionStatisticsReport = 59,
	PayrollAccountingReport = 60,
	PayrollVacationPayReport = 61, //not used
	ResultReportV2 = 62,
	EmployeeVacationInformationReport = 63,
	EmployeeVacationDebtReport = 64,
	PayrollProductReport = 65,
	StockSaldoListReport = 66,
	StockTransactionListReport = 67,
	KU10Report = 68,
	SKDReport = 69,
	CertificateOfEmploymentReport = 70, //Arbetsgivarintyg
	CollectumReport = 71,
	SEPAPaymentImportReport = 72,
	PeriodAccountingRegulationsReport = 73,
	EmployeeTimePeriodReport = 74,
	PayrollPeriodWarningCheck = 75,
	FixedAssets = 76,
	SCB_SLPReport = 77,
	SNReport = 78,
	KPAReport = 79,
	StockInventoryReport = 80,
	ForaReport = 81,
	SCB_KSPReport = 82,
	SCB_KSJUReport = 83,
	SCB_KLPReport = 84,
	ReportTransfer = 85,
	TimeScheduleTasksAndDeliverysReport = 86,
	AgdEmployeeReport = 87,
	ProductListReport = 88,
	TimePayrollTransactionSmallReport = 89,
	TimeEmployeeScheduleSmallReport = 90,
	TimeAbsenceReport = 91,
	InterestRateCalculation = 92,
	TimeEmployeeLineSchedule = 93,
	ProjectTimeReport = 94,
	PeriodAccountingForecastReport = 95,
	TimeAccumulatorDetailedReport = 96,
	RoleReport = 97,
	ExpenseReport = 98,
	KPADirektReport = 99,
	PayrollVacationAccountingReport = 100,
	Bygglosen = 101,
	Kronofogden = 102,
	BillingStatisticsReport = 103,
	PurchaseOrder = 104,
	BillingOrderOverview = 105,
	TimeEmploymentDynamicContract = 106,
	ForaMonthlyReport = 108,
	OrderContractChange = 109,
	TimeScheduleCopyReport = 110,
	TaxReductionBalanceListReport = 120,
	
	FolksamGTP = 500,
	SkandiaPension = 501,
	IFMetall = 502,
	SEF = 503,
	AgiAbsence = 504,
	
	Generic = 1000,
	
	// Analysis
	TimeTransactionAnalysis = 1001,
	PayrollTransactionAnalysis = 1002,
	EmployeeAnalysis = 1003,
	ScheduleAnalysis = 1004,
	EmployeeDateAnalysis = 1005,
	TimeStampEntryAnalysis = 1006,
	UserAnalysis = 1007,
	SupplierAnalysis = 1008,
	InvoiceProductAnalysis = 1009,
	StaffingneedsFrequencyAnalysis = 1011,
	CustomerAnalysis = 1010,
	EmployeeSkillAnalysis = 1012,
	OrganisationHrAnalysis = 1013,
	ShiftTypeSkillAnalysis = 1014,
	EmployeeEndReasonsAnalysis = 1015,
	EmployeeSalaryAnalysis = 1016,
	EmployeeTimePeriodAnalysis = 1017,
	StaffingStatisticsAnalysis = 1018,
	AggregatedTimeStatisticsAnalysis = 1019,
	EmployeeMeetingAnalysis = 1020,
	TimeScheduledSummary = 1021,
	EmployeeExperienceAnalysis = 1022,
	EmployeeDocumentAnalysis = 1023,
	EmployeeAccountAnalysis = 1024,
	ReportStatisticsAnalysis = 1025,
	EmployeeFixedPayLinesAnalysis = 1026,
	EmploymentHistoryAnalysis = 1027,
	PayrollProductsAnalysis = 1028,
	EmployeeSalaryDistressAnalysis = 1029,
	OrderAnalysis = 1030,
	EmployeeSalaryUnionFeesAnalysis = 1031,
	EmploymentDaysAnalysis = 1032,
	InvoiceAnalysis = 1033,
	AccountHierachyAnalysis = 1035,
	AnnualProgressAnalysis = 1036,
	LongtermAbsenceAnalysis = 1037,
	VacationBalanceAnalysis = 1038,
	ShiftQueueAnalysis = 1039,
	ShiftHistoryAnalysis = 1040,
	ShiftRequestAnalysis = 1041,
	AbsenceRequestAnalysis = 1042,
	TimeStampHistoryAnalysis = 1043,
	VerticalTimeTrackerAnalysis = 1044,
	HorizontalTimeTrackerAnalysis = 1045,
	AgiAbsenceAnalysis = 1046,
	InvoiceProductUnitConvertAnalysis = 1047,
	InventoryAnalysis = 1048,
	EmployeeChildAnalysis = 1049,
	EmployeePayrollAdditionsAnalysis = 1050,
	AnnualLeaveTransactionAnalysis = 1051,
	DepreciationAnalysis = 1052,
	SwapShiftAnalysis = 1053,
	
	//Bridge
	VismaPayrollChangesAnalysis = 5001,
	
	//Status
	SoftOneStatusResultAnalysis = 10001,
	SoftOneStatusEventAnalysis = 10002,
	SoftOneStatusUpTimeAnalysis = 10003,
	LicenseInformationAnalysis = 10004,
};

export enum SoeExportFormat {
	Unknown = 0,
	
	//Data
	Pdf = 1,
	Xml = 2,
	Excel = 3,
	Word = 4,
	RichText = 5,
	EditableRTF = 6,
	Text = 7,
	TabSeperatedText = 8,
	CharacterSeparatedValues = 9,
	Zip = 10,
	MergedPDF = 11,
	Payroll_SIE_Accounting = 12,
	Payroll_Visma_Accounting = 13,
	Payroll_SCB_Statistics = 14,
	KU10 = 15,
	eSKD = 16,
	QlikViewType1 = 17,
	Collectum = 18,
	Payroll_SN_Statistics = 19,
	EmployerDeclarationIndividual = 20,
	ExcelXlsx = 34,
	Payroll_EKS_Accounting = 35,
	SkandiaPension = 36,
	Payroll_SAP_Accounting = 37,
	AGD_Franvarouppgift = 38,
	
	//Images
	Gif = 21,
	Png = 22,
	Jpeg = 23,
	
	//Video
	Wmf = 31,
	Avi = 32,
	Mpeg = 33,
	
	//Data2
	KPADirekt = 50,
	SCB_KLP = 51,
	
	//Other
	Json = 60,
	
};

export enum SoeReportContentGroup {
	Category = 1,
	EmployeeGroup = 2,
}

export enum StatisticFileType {
	none = 0,
	SCB = 1,
	SN = 2,
	Fremia = 3
}

export enum SoeReportSettingField {
	IncludeAllHistoricalData = 1,
	IncludeBudget = 2,
	ShowRowsByAccount = 3,
	Nrofdecimals = 4,
	NoOfYearsBackinPreviousYear = 5,
	IncludeDetailedInformation = 6,
	ShowInAccountingReports = 7
}

export enum SoeReportSettingFieldType {
	Unknown = 0,
	Boolean = 1, // true/false
	Number = 2, // integer/decimal value
	String = 3, // string value
}

export enum SoeReportSettingFieldMetaData {
	IsVisible = 1,
	DefaultValue = 2,
	ForceDefaultValue = 3,
}

export enum ReportUserSelectionType {
	Unknown = 0,
	DataSelection = 1,
	AnalysisColumnSelection = 2,
	InsightsColumnSelection = 3
}

export enum ReconciliationRowType {
	Voucher = 1,
	CustomerInvoice = 2,
	SupplierInvoice = 3,
	Payment = 4
}

export enum RuleEvaluationResult {
	undefined = 0,
	succeeded = 1,
	falsified = 2,
	and = 3,
	or = 4,
}

export enum ScheduledJobRecurrenceType {
	RunOnce = 0,        // Run once (no recurrence)
	RunInfinite = 1,    // Run until manually disabled
	RunNbrOfTimes = 2,  // Run specified number of times (Column RecurrenceCount)
	RunUntilDate = 3    // Run until specified date (Column RecurrenceDate)
};

export enum ScheduledJobRetryType {
	Abort = 0,          // Job will not retry if interrupted
	Immediately = 1,    // Job will retry immediately if interrupted
	NextInterval = 2    // Job will retry on next job interval (RecurrenceIntervalValue)
};

export enum ScheduledJobState {
	Inactive = 0,       // Default. The job will not run, not even when ExecuteTime is reached
	Active = 1,         // The job is ready to run. Will be executed when ExecuteTime is reached
	Running = 2,        // The job is currently running
	Finished = 3,       // The job is finished without errors
	Interrupted = 4,    // The job was interrupted, either manually or by an error, se log for details
	Deleted = 5         // The job is deleted
};

export enum ScheduledJobType {
	Task = 0,
	Service = 1,
}

export enum SysJobSettingType {
	Unknown = 0,
	Job = 1,
	ScheduledJob = 2
}

export enum ScheduledJobLogLevel {
	None = 0,
	Information = 1,
	Success = 2,
	Warning = 3,
	Error = 4
};

export enum ScheduleSwapLengthComparisonType {
	Equal = 0,
	Shorter = 1,
	Longer = 2,
}

export enum ReportScheduleType {
	Unknown = 0,
	AddedTime = 1,
	OverTime = 2,
}

export enum SieImportType {
	Account = 1,
	Voucher = 2,
	AccountBalance = 3,
	Account_Voucher_AccountBalance = 4,
};

export enum SieExportType {
	Type1 = 1,
	Type2 = 2,
	Type3 = 3,
	Type4 = 4,
};

export enum SieConflict {
	//General
	None = 0,
	Exception = 1,
	
	//Import common 1001-1100
	Import_NameConflict = 1001,
	Import_AddFailed = 1002,
	Import_UpdateFailed = 1003,
	Import_InvalidLine = 1004,
	
	//Import Mandatory field missing 1101-1200
	Import_MandatoryFieldMissing_Dim_dimensionsnr = 1101,
	Import_MandatoryFieldMissing_Dim_namn = 1102,
	Import_MandatoryFieldMissing_Objekt_dimensionsnr = 1103,
	Import_MandatoryFieldMissing_Objekt_objektkod = 1104,
	Import_MandatoryFieldMissing_Objekt_objektnamn = 1105,
	Import_MandatoryFieldMissing_Konto_kontonr = 1106,
	Import_MandatoryFieldMissing_Konto_kontonamn = 1107,
	Import_MandatoryFieldMissing_Ktyp_kontotyp = 1108,
	Import_MandatoryFieldMissing_Ver_verdatum = 1109,
	Import_MandatoryFieldMissing_Trans_kontonr = 1110,
	Import_MandatoryFieldMissing_Trans_belopp = 1111,
	Import_MandatoryFieldMissing_AccountBalance_accountyear = 1112,
	Import_MandatoryFieldMissing_AccountBalance_accountnr = 1113,
	Import_MandatoryFieldMissing_AccountBalance_balance = 1114,
	
	//Import Account 1201-1300
	Import_DimNotFound = 1201,
	Import_ObjectRuleFailed = 1202,
	Import_AccountRuleFailed = 1203,
	Import_AccountHasNoAccountType = 1204,
	
	//Import AccountBalance 1301-1400
	Import_AccountBalanceHasUnknownAccountYear = 1301,
	Import_AccountBalanceExistInAccountYear = 1302,
	Import_AccountBalanceUseUBInsteadOfIB_PreviousYearNotInFile = 1303,
	
	//Import Voucher 1401-1500
	Import_VoucherHasNoStartLabel = 1401,
	Import_VoucherHasNoEndLabel = 1402,
	//Import_VoucherHasNoTransactions = 1403,
	//Import_VoucherHasInvalidBalance = 1404,
	Import_VoucherSeriesTypeDoesNotExist = 1405,
	Import_VoucherHasNoVoucherNr = 1406,
	Import_VoucherAlreadyExist = 1407,
	Import_VouchersAccountYearDoesNotMatchAccountYearDefault = 1408,
	Import_VouchersAccountYearDoesNotExist = 1409,
	Import_VouchersAccountYearIsNotOpen = 1410,
	Import_VouchersAccountPeriodDoesNotExist = 1411,
	Import_VouchersAccountDoesNotExist = 1412,
	Import_VouchersObjectDoesNotExist = 1413,
	Import_VouchersAccountPeriodIsNotOpen = 1414,
	
	//Import AccountYear 1501-1600
	Import_AccountYearIsNotOpen = 1501,
	Import_AccountDoesNotExist = 1502,
	
	//Export common 2001-2100
	Export_WriteFailed = 2001,
	
	//Export AccountStd 2101-2200
	Export_AccountStdIsNotNumeric = 2101,
};

export enum SMSProvider {
	Unknown = 0,
	Pixie = 1,
}

export enum SignatoryContractAuthenticationMethodType {
	None = 0,
	Password = 1,
	PasswordSMSCode = 2,
}

export enum SoeAttestTreeMode {
	TimeAttest = 1,
	PayrollCalculation = 2,
}

export enum SoeAttestTreeLoadMode {
	Full = 1,
	OnlyEmployees = 2,
	OnlyEmployeesAndIsAttested = 3,
}

export enum SoeAttestDevice {
	Web = 1,
	Mobile = 2,
}

export enum SoeTimeAttestWarningGroup {
	Time = 0,
	Payroll = 1,
}

export enum SoeTimeAttestWarning {
	None = 0,
	//Deprecated: ScheduleIsChanged = 1,
	ScheduleIsChangedFromTemplate = 2,
	ScheduleWithoutTransactions = 3,
	PlacementIsScheduled = 4,
	TimeScheduleTypeFactorMinutes = 5,
	DiscardedBreakEvaluation = 6,
	TimeStampsWithoutTransactions = 7,
	TimeStampErrors = 8,
	ContainsDuplicateTimeBlocks = 9,
	PayrollImport = 10,
	
	PayrollControlFunction = 101,
}

export enum SoeTimeAttestInformation {
	None = 0,
	HasShiftSwaps = 1
}

export enum SoeBillingInvoiceReportType {
	Unknown = 0,
	Contract = 1,
	Offer = 2,
	Order = 3,
	Invoice = 4,
	ClaimQuick = 5, //Directly from invoice with use of the checkbox
	Claim = 6,
	Interest = 7,
};

export enum SoeCSRState {
	NotExported = 0,
	Exported = 1,
	All = 2,
};

export enum SoeDataStorageOriginType {
	Unknown = 0,
	
	FileName = 1,
	Xml = 2,
	Data = 3,
	Json = 4
}

export enum SoeDataStorageRecordType {
	Unknown = 0,
	
	//TimeSalary
	TimeSalaryExport = 11,
	TimeSalaryExportEmployee = 12,
	TimeSalaryExportControlInfoEmployee = 13,
	TimeSalaryExportControlInfo = 14,
	TimeSalaryExportSaumaPdf = 15,
	TimeSalaryExportCSR = 16,
	TimeKU10ExportEmployee = 17,
	TimeKU10Export = 18,
	
	//Invoice export
	SOPCustomerInvoiceExport = 21,
	BillingInvoicePDF = 22,
	BillingInvoiceXML = 23,
	DiRegnskapCustomerInvoiceExport = 24,
	UniMicroCustomerInvoiceExport = 25,
	FinvoiceCustomerInvoiceExport = 26,
	DnBNorCustomerInvoiceExport = 27,
	InvoicePaymentServiceExport = 28,
	FinvoiceCustomerInvoiceExportAttachments = 29,
	
	//XEMail
	XEMailFileAttachment = 31,
	
	//Files
	UploadedFile = 41,
	MessageGroup = 42,
	
	//Invoice
	InvoiceBitmap = 51,
	InvoicePdf = 52,
	OrderInvoiceFileAttachment = 53,
	OrderInvoiceFileAttachment_Thumbnail = 54,
	OrderInvoiceFileAttachment_Image = 55,
	OrderInvoiceSignature = 56,
	
	//Help
	HelpAttachment = 61,
	
	//Customer
	CustomerFileAttachment = 62,
	
	//Payroll
	PayrollSlipXML = 71,
	VacationYearEndHead = 72,
	
	//Payment
	SEPAPaymentImport = 81,
	
	//Voucher
	VoucherFileAttachment = 91,
	
	InventoryFileAttachment = 95,
	
	// Employment
	TimeEmploymentContract = 100,
	EmployeePortrait = 101,
	
	// Checklist
	ChecklistHeadRecordSignature = 120,
	ChecklistHeadRecordSignatureExecutor = 121,
	ChecklistHeadRecord = 122,
	
	// TimeRule
	TimeRuleImport_ExportedRules = 130,
	TimeRuleImport_Rule = 131,
	
	// Supplier
	SupplierFileAttachment = 140,
	
	// Common
	Expense = 150,
	
	// EdiEntry
	EdiEntry_Document = 160,
	EdiEntry_RawData = 161,
	
	//ScanningEntry
	ScanningEntry_Document = 171,
	ScanningEntry_RawData = 172,
	
	// Project
	ProjectFileAttachment = 180,
};

export enum SoeEmployeePostStatus {
	None = 0,
	Locked = 1,
	
	//Calculation statuses
	HasSchedule = 101,
	HasEmployee = 102,
	NotFound = 404,
}

export enum SoeEmployeeTimePeriodStatus {
	None = 0,
	Open = 1,
	Paid = 2,
	Locked = 3,
}

export enum SoeEmployeeTimePeriodValueType {
	None = 0,
	TableTax = 1,
	OneTimeTax = 2,
	EmploymentTaxCredit = 3,
	SupplementChargeCredit = 4,
	GrossSalary = 5,
	NetSalary = 6,
	VacationCompensation = 7,
	Benefit = 8,
	Compensation = 9,
	Deduction = 10,
	UnionFee = 11,
	OptionalTax = 12,
	SINKTax = 13,
	ASINKTax = 14,
	EmploymentTaxBasis = 15,
}

export enum SoeEmploymentFinalSalaryStatus {
	None = 0,
	ApplyFinalSalary = 1,
	AppliedFinalSalary = 2,
	AppliedFinalSalaryManually = 3,
}

export enum SoeEmploymentDateChangeType {
	None = 0,
	Delete = 1,
	ShortenStart = 2,
	ShortenStop = 3,
	ExtendStop = 4,
}

export enum SoeVacationYearEndType {
	None = 0,
	VacationYearEnd = 1,
	FinalSalary = 2,
}

export enum SoeFavoriteOption {
	//Manage 100-199
	
	//Economy 200-299
	Economy_Supplier = 201,
	Economy_Accounting = 202,
	
	//Billing 300-399
	Billing_Offer = 101,
	Billing_Order = 102,
	Billing_Invoice = 103,
	
	//Estatus 400-499
	
	//Time  500-599
	Time_Process = 301,
	Time_Attest = 302,
}

export enum SoeFieldSettingType {
	Unknown = 0,
	Web = 1,
	Mobile = 2,
}

export enum SoeInvoiceMatchingType {
	Unknown = 0,
	SupplierInvoiceMatching = 1,
	CustomerInvoiceMatching = 2,
	General = 3,
}

export enum SoeInvoiceInterestHandlingType {
	NoInterest = 0,
	CreateNewInvoice = 1,
	AddToNextInvoice = 2,
};

export enum SoeInvoiceReminderHandlingType {
	None = 0,
	CreateNewInvoice = 1,
	AddToNextInvoice = 2,
};

export enum SoeInvoiceToVoucherHeadType {
	None = 0,
	VoucherPerInvoice = 1,
	MergeVoucherOnVoucherDate = 2,
	MergeAllInvoices = 3,
};

export enum SoeInvoiceToVoucherRowType {
	None = 0,
	VoucherRowPerInvoiceRow = 1,
	MergeVoucherRowsOnAccount = 2,
};

export enum SoeInvoiceExportStatusType {
	NotExported = 0,
	ExportedAndClosed = 1,
	ExportedAndOpen = 2,
};

export enum SoeInvoiceImageStorageType {
	NoImage = 0,
	StoredInEdiEntry = 1,
	StoredInScanningEntry = 2,
	StoredInInvoiceDataStorage = 3,
	StoredInEdiDataStorage = 4
}

export enum SoeSupplierPaymentObservationMethod {
	None = 0,
	Observation = 1,
	ObservationTotalAmount = 2,
};

export enum SoeForeignPaymentBankCode {
	None = 0,
	Handelsbanken = 1,
	SEB = 2,
	Swedbank = 3,
	Nordea = 4,
	Other = 5,
};

export enum SoeForeignPaymentForm {
	None = 0,
	Account = 1,
	Check = 2,
};

export enum SoeForeignPaymentMethod {
	None = 0,
	Normal = 1,
	Express = 2,
	CompanyGroup = 3,
};

export enum SoeForeignPaymentChargeCode {
	None = 0,
	SenderDomesticCosts = 1,
	SenderAllCosts = 2,
	RecieverAllCosts = 3,
};

export enum SoeForeignPaymentIntermediaryCode {
	None = 0,
	BGC = 1,
};

export enum SoeLoginState {
	NotLoggedIn = 0,
	OK = 1,
	BadLogin = 2,
	ConcurrentUserViolation = 3,
	DuplicateUserLogin = 4,
	BadDefaultCompany = 5,
	RoleNotConnectedToCompany = 6,
	LicenseTerminated = 7,
	IsNotMobileUser = 8,
	LoginInvalidOrExceededTimeout = 9,
	LoginServerNotFound = 10,
	SoftOneOnlineError = 11,
	BlockedFromDatePassed = 12,
	Unknown = 100,
};

export enum SoeSupportLoginState {
	NotLoggedIn = 0,
	OK = 1,
	NotAllowed = 2,
	InvalidLicense = 3, //Can only happen if HTML DOM is manipulated
	
	Unknown = 100,
}

export enum SoeLogType {
	System_Search = 1,
	System_All = 2,
	System_All_Today = 3,
	System_Error = 4,
	System_Error_Today = 5,
	System_Warning = 6,
	System_Warning_Today = 7,
	System_Information = 8,
	System_Information_Today = 9,
	License = 10,
	Company = 11,
	Role = 12,
	User = 13,
	Machine = 14,
};

export enum SoeMobileType {
	Unknown = 0,
	XE = 1,
	Professional = 2,
	Sauma = 3,
}

export enum SoeMobileAppType {
	Unknown = 0,
	Personal = 1,
	Order = 2,
	GO = 3,
}

export enum SoeModule {
	None = 0,
	Manage = 1,
	Economy = 2,
	Billing = 3,
	Estatus = 4,
	Time = 5,
	TimeSchedulePlanning = 6,   // Used for TimeSchedulePlanning Dashboard
	ClientManagement = 7,
}

export enum SoePayrollPriceFormulaFixedValue {
	Unknown = 0,
	
	// Sweden 1-100
	SE_EmploymentRate = 1,  // Sysselsättningsgrad
	SE_BaseAmount = 2,      // Basbelopp
	
	// Finland 101-200
	
	// Norway 201-300
}

export enum SoePaymentExportCancelledStates {
	Active = 0,
	PartyCancelled = 1,
	Cancelled = 2,
}

export enum SoeProductPriceStatus {
	Undefined = 0,
	NewProduct = 1,
	NoPriceChange = 2,
	PriceChange = 3,
	PricedOnRequest = 4,
};

export enum PriceListOrigin {
	Unknown = 0,
	SysDbPriceList = 1,
	CompDbPriceList = 2,
	/// <summary>
	/// SysAndComp are never used in the db
	/// </summary>
	SysAndCompDbPriceList = 3,
	CompDbNetPriceList = 4,
}

export enum SoeCompPriceListProvider {
	AhlsellVerktyg = 1,
	AhlsellBygg = 2,
	TBEl = 3,
	Bevego = 4,
	Trebolit = 5,
	//Consultec1 = 6,
	//Consultec2 = 7,
	//Consultec3 = 8,
	//Consultec4 = 9,
	//Consultec5 = 10,
	//Consultec6 = 11,
	//AssaAbloy = 12,
	Etman = 13,
	EtmanPipe = 14,
	MalmbergFI = 15,
	Lunda = 16,
	LundaStyckNetto = 17,
	//LundaMängdNetto = 18,
	//RexelNetto = 19,
	LundaBrutto = 20,
	RexelFINetto = 21,
	AhlsellFINetto = 22,
	AhlsellFIPLNetto = 23,
	SoneparFINetto = 24,
	OnninenFINettoS = 25,
	DahlFINetto = 26,
	OnninenFINettoLVI = 27,
	Alcadon = 28,
	StorelNetto = 29,
	GeliaNetto = 30
}

export enum SoeSysPriceListProvider {
	//EIO = 1,
	Sonepar = 2,
	AhlsellEl = 4,
	AhlsellVvs = 5,
	Solar = 6,
	Rexel = 7,
	Storel7 = 8,
	//Storel8 = 9,
	Dahl = 10,
	Onninen = 11,
	SolarVVS = 12,
	//ElektroskandiaNetto = 13,
	SthlmElgross = 14,
	Moel = 15,
	//Lunda = 16,
	//LundaNetto = 17,
	//LundaMängd = 18,
	Malmbergs = 19,
	//Elgrossen = 20,
	//Consultec1 = 21,
	//Consultec2 = 22,
	//Consultec3 = 23,
	//Consultec4 = 24,
	//Consultec5 = 25,
	//Consultec6 = 26,
	RexelFI = 27,
	AhlsellFI = 28,
	OnninenFI = 29,
	DahlFI = 30,
	SoneparFI = 31,
	//WarlaFI = 32,
	Bragross = 33,
	Carpings = 34,
	VVScentrum = 35,
	AhlsellFIPL = 36,
	OnninenFIPL = 37,
	//ElektroskandiaNO = 38,
	RobHolmqvistVVS = 39,
	//OtraNO = 40,
	//AhlsellNO = 41,
	//OnninenNO = 42,
	//BrødreneDahlNO = 43,
	//EltmannNO = 44,
	//BergårdAndAmundsenNO = 45,
	JohnFredrik = 46,
	Gelia = 47,
	Elkedjan = 48,
	Instaoffice = 49,
	//OnninenVVSNO = 50,
	SolarNO = 51,
	PistesarjaFI = 52,
	//Comfortleverantören = 53,
	E2Teknik = 54,
	ByggOle = 55,
	VSProdukter = 56,
	OnninenFI_SE = 57,
	OnninenFI_SE_PL = 58,
	//ElPartsFI = 59,
	LVIWaBeKFIPL = 60,
	Copiax = 61,
	SLR = 62,
	AhlsellIsolering = 63,
	AhlsellBygg = 64,
	AhlsellVerktyg = 65,
	Bad_Värme = 66,
	AhlsellKyla = 67,
	AhlsellVentilation = 68,
	AhlsellMetall = 69,
	Bevego = 70,
	Comfort_Ahlsell = 71,
	Comfort_Direkt = 72,
	Comfort_Solar = 73,
	Comfort_Bevego = 74,
	Comfort_Dahl = 75,
	Comfort_Elektroskandia = 76,
	Thermotech = 77,
	Currentum_Dahl = 78,
	Currentum_Solar = 79,
	Lindab = 80,
	Currentum_Ahlsell = 81,
};

export enum SoeSysPriceListProviderType {
	Unknown = 0,
	Electrician = 1,
	Plumbing = 2,
	LockSmith = 3,
	Ahlsell = 4,
	BadVarme = 5,
	Bevego = 6,
	Comfort = 7,
	Lindab = 8,
}

export enum SoeSupplierAgreementProvider {
	Ahlsell = 1,
	Dahl = 2,
	Sonepar = 3,
	Rexel = 4,
	Solar = 5,
	SthlmElgross = 6,
	Storel = 7,
	//Moel = 8,
	Onninen = 9,
	SolarVVS = 10,
	//Elgrossen = 11,
	AhlsellFI = 12,
	DahlFI = 13,
	RexelFI = 14,
	OnninenFI = 15,
	SoneparFI = 16,
	//WarlaFI = 17,
	Bragross = 18,
	Carpings = 19,
	VVSCentrum = 20,
	//Lunda = 21,
	//ElektroskandiaNO = 22,
	RobHolmqvistVVS = 23,
	//OtraNO = 24,
	//BergårdAndAmundsenNO = 25,
	//AhlsellNO = 26,
	//OnninenNO = 27,
	//BrødreneDahlNO = 28,
	//EltmannNO = 29,
	OnninenFIPL = 30,
	AhlsellFIPL = 31,
	//SelgaV2 = 32,
	JohnFredrik = 33,
	Gelia = 47,
	Elkedjan = 48,
	//Instaoffice = 49,
	//OnninenVVSNO = 50,
	//SolarNO = 51,
	PistesarjaFI = 52,
	//Comfortleverantören = 53,
	E2Teknik = 54,
	VSProdukter = 55,
	OnninenFI_SE = 57,
	OnninenFI_SE_PL = 58,
	//ElPartsFI = 59,
	LVIWaBeKFIPL = 60,
	Copiax = 61,
	SLR = 62,
	Malmberg = 63,
	Bevego = 64,
	Thermotech = 65,
	Lindab = 66,
};

export enum SoeSupplierAgreemntCodeType {
	MaterialCode = 1,
	Generic = 2,
	Product = 3,
}

export enum SoeWholeseller {
	Unknown = 0,
	Sonepar = 1,
	Ahlsell = 2,
	Rexel = 3,
	Solar = 5,
	Elkedjan = 6,
	Dahl = 10,
	Lunda = 20,
	AhlsellFI = 34,
	RexelFI = 36,
	SoneparFI = 38,
	AhlsellFIPL = 40,
	Comfort = 65
}

export enum SoeProgressInfoType {
	Unknown = 0,
	Budget = 1,
	ScheduleEmployeePost = 2
}

export enum SoeProjectRecordType {
	Invoice = 1,
	Order = 2,
	TimeSheet = 3,
	Purchase = 4
}

export enum SoeProjectDayType {
	Monday = 1,
	Tuesday = 2,
	Wednesday = 3,
	Thursday = 4,
	Friday = 5,
	Saturday = 6,
	Sunday = 7,
	CompensateDay = 8,
}

export enum SoeRecalculateTimeHeadAction {
	Placement = 1,
}

export enum SoeReportDataResultMessage {
	Success = 0,
	
	
	Error = 1,
	EmptyInput = 2,
	ReportTemplateDataNotFound = 3,
	DocumentNotCreated = 4,
	ReportFailed = 5,
	ExportFailed = 6,
	
	
	
	BalanceReportHasNoGroupsOrHeaders = 101,
	ResultReportHasNoGroupsOrHeaders = 102,
	CreateVoucherFailed = 103,
	
	
	
	EdiEntryNotFound = 301,
	EdiEntryCouldNotParseXML = 302,
	EdiEntryCouldNotSavePDF = 303,
	
	
	
	ReportsNotAuthorized = 401,
	
}

export enum SoeReportDataHistoryHeadTag {
	//BillingInvoice
	BillingInvoice_ReportHeader = 1,
	BillingInvoice_InvoiceHead = 2,
}

export enum SoeReportDataHistoryTag {
	//BillingInvoice_PageHeaderLabels 1-100
	BillingInvoice_ReportHeader_CompanyName = 4,
	BillingInvoice_ReportHeader_CompanyOrgnr = 5,
	BillingInvoice_ReportHeader_CompanyVatNr = 12,
	BillingInvoice_ReportHeader_CompanyDistributionAddress = 13,
	BillingInvoice_ReportHeader_CompanyDistributionAddressCO = 14,
	BillingInvoice_ReportHeader_CompanyDistributionPostalCode = 15,
	BillingInvoice_ReportHeader_CompanyDistributionPostalAddress = 16,
	BillingInvoice_ReportHeader_CompanyDistributionCountry = 17,
	BillingInvoice_ReportHeader_CompanyBoardHQPostalAddress = 18,
	BillingInvoice_ReportHeader_CompanyBoardHQCountry = 19,
	BillingInvoice_ReportHeader_CompanyEmail = 20,
	BillingInvoice_ReportHeader_CompanyPhoneHome = 21,
	BillingInvoice_ReportHeader_CompanyPhoneJob = 22,
	BillingInvoice_ReportHeader_CompanyPhoneMobile = 23,
	BillingInvoice_ReportHeader_CompanyFax = 24,
	BillingInvoice_ReportHeader_CompanyWebAddress = 25,
	BillingInvoice_ReportHeader_CompanyBg = 26,
	BillingInvoice_ReportHeader_CompanyPg = 27,
	BillingInvoice_ReportHeader_CompanyBank = 28,
	BillingInvoice_ReportHeader_CompanyBic = 29,
	BillingInvoice_ReportHeader_CompanySepa = 30,
	BillingInvoice_ReportHeader_CompanyBicNR = 31,
	BillingInvoice_ReportHeader_CompanyBicBIC = 32,
	
	//BillingInvoice_InvoiceHead 101-200
	BillingInvoice_InvoiceHead_CustomerName = 101,
	BillingInvoice_InvoiceHead_CustomerNr = 102,
	BillingInvoice_InvoiceHead_CustomerOrgNr = 103,
	BillingInvoice_InvoiceHead_CustomerVatNr = 104,
	BillingInvoice_InvoiceHead_CustomerBillingAddress = 105,
	BillingInvoice_InvoiceHead_CustomerBillingAddressCO = 106,
	BillingInvoice_InvoiceHead_CustomerBillingPostalCode = 107,
	BillingInvoice_InvoiceHead_CustomerBillingPostalAddress = 108,
	BillingInvoice_InvoiceHead_CustomerBillingCountry = 109,
	BillingInvoice_InvoiceHead_CustomerDeliveryAddress = 110,
	BillingInvoice_InvoiceHead_CustomerDeliveryAddressCO = 111,
	BillingInvoice_InvoiceHead_CustomerDeliveryPostalCode = 112,
	BillingInvoice_InvoiceHead_CustomerDeliveryPostalAddress = 113,
	BillingInvoice_InvoiceHead_CustomerDeliveryCountry = 114,
	BillingInvoice_InvoiceHead_CustomerDeliveryName = 115,
	BillingInvoice_InvoiceHead_CustomerRegDate = 116,
	BillingInvoice_InvoiceHead_CustomerEmail = 117,
}

export enum SoeRuleCopySource {
	Undefined = 0,
	Existing = 1,
	LocalTemplate = 2,
	GlobalTemplate = 3,
}

export enum PriceRuleItemType {
	Addition = 1,
	Subtraction = 2,
	Multiplication = 3,
	StartParanthesis = 4,
	EndParanthesis = 5,
	GNP = 6,
	Category = 7,
	SupplierAgreement = 8,
	MaterialClass = 9,
	Gain = 10,
	CustomerDiscount = 11,
	Markup = 13,
	PriceBasedMarkup = 14,
	NetPrice = 15,
	Or = 16
}

export enum PriceRuleValueType {
	Numeric = 0,
	NegativePercent = 1,
	PositivePercent = 2,
	Percent = 3,
	Replace = 4,
}

export enum SoeTimeRuleType {
	Unknown = 0,
	Presence = 1,
	Absence = 2,
	Constant = 3,
}

export enum SoeTimeRuleOperatorType {
	Unspecified = 0,
	TimeRuleOperatorAnd = 1,
	TimeRuleOperatorOr = 2,
	TimeRuleOperatorBalance = 3,
	TimeRuleOperatorScheduleIn = 4,
	TimeRuleOperatorScheduleOut = 5,
	TimeRuleOperatorClock = 6,
	TimeRuleOperatorNot = 7,
	TimeRuleOperatorStartParanthesis = 8,
	TimeRuleOperatorEndParanthesis = 9,
}

export enum SoeTimeRuleComparisonOperator {
	Unspecified = 0,
	TimeRuleComparisonOperatorLessThan = 1,
	TimeRuleComparisonOperatorLessThanOrEqualsTo = 2,
	TimeRuleComparisonOperatorEqualsTo = 3,
	TimeRuleComparisonOperatorGreaterThanOrEqualsTo = 4,
	TimeRuleComparisonOperatorGreaterThan = 5,
	TimeRuleComparisonOperatorNotEqual = 6,
	TimeRuleComparisonClockPositive = 7,
	TimeRuleComparisonClockNegative = 8,
}

export enum SoeTimeRuleDirection {
	Forward = 1,
	Backward = 2,
}

export enum SoeTimeRuleValueType {
	Undefined = 0,
	
	TimeCodeLeft = 1,
	TimeCodeRight = 3,
	
	ScheduleLeft = 6,
	ScheduleAndBreakLeft = 17,
	ScheduleRight = 2,
	SchedulePlusOvertimeInOvertimePeriod = 15,
	
	Presence = 4,
	PresenceWithinSchedule = 5,
	PresenceBeforeSchedule = 7,
	PresenceAfterSchedule = 8,
	PresenceInScheduleHole = 14,
	
	Payed = 9,
	PayedBeforeSchedule = 10,
	PayedBeforeSchedulePlusSchedule = 11,
	PayedAfterSchedule = 12,
	PayedAfterSchedulePlusSchedule = 13,
	
	FulltimeWeek = 16,
}

export enum SoeStatesAnalysis {
	//General 1-20
	Role = 1,
	User = 2,
	Employee = 3,
	Customer = 4,
	Supplier = 5,
	InvoiceProduct = 6,
	
	//Billing 21-40
	Offer = 21,
	Contract = 22,
	Order = 23,
	Invoice = 24,
	OrderRemaingAmount = 25,
	
	//HouseHoldTaxDeduction
	HouseHoldTaxDeductionApply = 30,
	HouseHoldTaxDeductionApplied = 31,
	HouseHoldTaxDeductionReceived = 32,
	HouseHoldTaxDeductionDenied = 33,
	
	//CustomerInvoice 41-50
	CustomerInvoicesOpen = 41,
	CustomerPaymentsUnpayed = 42,
	CustomerInvoicesOverdued = 43,
	
	//SupplierInvoice = 51-60
	SupplierInvoicesOpen = 51,
	SupplierInvoicesUnpayed = 52,
	SupplierInvoicesOverdued = 53,
	
	//EDI 61-70
	EdiError = 61,
	EdiOrderError = 62,
	EdiInvoicError = 63,
	
	//Scanning 71-80
	ScanningError = 71,
	ScanningInvoiceError = 72,
	ScanningUnprocessedArrivals = 73,
	
	//Time 81-90
	ActiveTerminals = 81,
	InActiveTerminals = 82,
	
	//Communication 91-99
	NewMessages = 91,
}

export enum SoeStatesAnalysisGroup {
	General = 1,
	BillingAndLedger = 2,
	Edi = 3,
	Scanning = 4,
	Communication = 5,
	HouseholdTaxDeduction = 6,
}

export enum SoeStatusIcon {
	None = 0,
	Attachment = 1,
	Image = 2,
	Checklist = 4,
	Email = 8,
	Imported = 16,
	ElectronicallyDistributed = 32,
	EmailError = 64,
	DownloadEinvoice = 128,
}

export enum SoeRecalculateAccountingMode {
	FromShiftType = 1,
	FromSchedule = 2,
	FromTime = 3,
}

export enum SoeScheduleWorkRules {
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
	Breaks = 19,
	ScheduleFreeWeekends = 20,
	ScheduledDaysMaximum = 21,
	CoherentSheduleTime = 22,
	HoursBeforeAssignShift = 23,
	HoursBeforeShiftRequest = 24,
	LeisureCodes = 25,
	AnnualLeave = 26,
}

export enum SoeStaffingNeedsTaskType {
	Unknown = 0,
	Task = 1,
	Delivery = 2,
}

export enum SoeStaffingNeedType {
	Unknown = 0,
	EmployeePost = 1,
	Template = 2,
	Employee = 3,
}

export enum SoeTabViewType {
	Unknown = 0,
	Edit = 1,
	Setting = 2,
	View = 3,
	Admin = 4,
	Import = 5,
	Export = 6,
};

export enum SoeTimeAccumulatorComparison {
	OK = 0,
	LessThanMin = 1,
	LessThanMinWarning = 2,
	MoreThanMaxWarning = 3,
	MoreThanMax = 4,
}

export enum SoeTimeAccumulatorBalanceType {
	Year = 1,
};

export enum SoeTimeBlockDeviationChange {
	None = 0,
	Move = 1,
	ResizeStartLeftDragLeft = 2,
	ResizeStartLeftDragRight = 3,
	ResizeStartRightDragLeft = 4,
	ResizeStartRightDragRight = 5,
	ResizeBothSides = 6,
	NewTimeBlock = 7,
	DeleteTimeBlock = 8,
	DeleteTimeBlockAdvancedFromLeft = 9,
	DeleteTimeBlockAdvancedFromRight = 10,
}

export enum SoeTimeBlockClientChange {
	None = 0,
	Left = 1,
	Right = 2,
}

export enum SoeTimeBreakTemplateType {
	None = 0,
	Major = 1,
	Minor = 2,
}

export enum SoeTimeBreakTemplateEvaluation {
	Test = 0,
	Automatic = 1,
	Manual = 2,
	RegisterEvaluation = 3,
}

export enum SoeTimeScheduleEmployeePeriodDetailType {
	Unknown = 0,
	LeisureCode = 1,
}

export enum SoeTimeScheduleDeviationCauseStatus {
	None = 0,
	Standard = 1,
	Planned = 2,
}

export enum SoeTimePayrollScheduleTransactionType {
	None = 0,
	Schedule = 1,
	Absence = 2,
}

export enum SoeTimeBlockType {
	Unknown = 0,
	Presence = 1,
	Absense = 2,
	Break = 3,
}

export enum SoeTimeBlockDateDetailType {
	Unknown = 0,
	Absence = 1,
	
	//Not persistent
	Read = 101,
}

export enum SoeTimeCodeType {
	//Readme: When changing/adding, also change methods in Common.PayrollRulesUtil, Common.Extensions and Data.ExtensionsComp
	
	None = 0,
	Work = 1,
	Absense = 2,
	Break = 3,
	//NOTUSED = 4,
	//Addition = 5,
	//Deduction = 6,
	//NOTUSED = 7,
	Material = 8,
	AdditionDeduction = 9,
	
	// Used in queries
	WorkAndAbsense = 101,
	WorkAndAbsenseAndAdditionDeduction = 102,
	WorkAndMaterial = 103,
	//AdditionAndDeduction = 104,
}

export enum SoeTimeCodeBreakTimeType {
	ScheduleIn = 1,
	ScheduleOut = 2,
	//removed the other 2 choices in termgroup, leaving space if they should be readded
	Clock = 5,
}

export enum SoeValidateDeviationChangeResultCode {
	Unknown = 0,
	ChooseDeviationCause = 1,
	Generated = 2,
	
	ErrorInvalidInputTimeBlocks = 101,
	ErrorInvalidInputTimeBlockGuid = 102,
	ErrorDayIsAttested = 103,
	ErrorEmployeeNotFound = 104,
	ErrorEmployeeGroupNotFound = 105,
	ErrorEmployeeGroupIsStamping = 106,
	ErrorTimeBlockDateNotFound = 107,
	ErrorTimeBlockNotFound = 108,
	ErrorAttestStateInitialNotFound = 109,
	ErrorTimeDeviationCauseNotSpecified = 110,
	ErrorTimeDeviationCauseNotFound = 111,
	ErrorTimeDeviationCauseOnTimeBlockNotFound = 112,
	ErrorInvalidChange = 113,
	ErrorFailedGenerate = 114,
}

export enum SoeValidateBreakChangeError {
	Unknown = 0,
	NoSchedule = 1,
	BreakNotFound = 2,
	BreakOverlapsAnotherBreak = 3,
	BreakOverlapsUnlinkedShifts = 4,
	BeforeScheduleIn = 5,
	AfterScheduleOut = 6,
	StartsWithBreak = 7,
	EndsWithBreak = 8,
	EmployeeNotFound = 9,
	EmployeeGroupNotFound = 10,
	TimeCodeBreakGroupsNotFound = 11,
	TimeCodeBreaksNotFound = 12,
	TimeCodeBreakForLengthNotFound = 13,
	TimeCodeBreakChanged = 14,
	TimeCodeBreakWindowInvalid = 15,
}

export enum SoeTimeEngineTask {
	Unknown = 0,
	
	
	GetTimeScheduleTemplate = 11,
	//GetTimeSchedule = 12 is not longer used
	GetSequentialSchedule = 13,
	SaveTimeScheduleTemplate = 14,
	SaveTimeScheduleTemplateStaffing = 15,
	//SaveTimeSchedule = 16 is no longer used
	UpdateTimeScheduleTemplateStaffing = 17,
	SaveShiftPrelToDef = 18,
	SaveShiftDefToPrel = 19,
	CopySchedule = 20,
	DeleteTimeScheduleTemplate = 21,
	RemoveEmployeeFromTimeScheduleTemplate = 22,
	RestoreDaysToSchedule = 23,
	RestoreDaysToTemplateSchedule = 24,
	CreateTemplateFromScenario = 25,
	//ReSyncFlexForceSchedules = 26,          // NOT USED
	//ReSyncFlexForceLeaveApplications = 27,  // NOT USED
	RestoreToScheduleDiscardDeviations = 28,
	
	SaveEmployeeSchedulePlacement = 31,
	SaveEmployeeSchedulePlacementFromJob = 32,
	SaveEmployeeSchedulePlacementStaffing = 33,
	DeleteEmployeeSchedulePlacement = 34,
	ControlEmployeeSchedulePlacement = 35,
	
	GetEmployeeRequests = 41,
	LoadEmployeeRequest = 42,
	SaveEmployeeRequest = 43,
	SaveOrDeleteEmployeeRequest = 44,
	DeleteEmployeeRequest = 45,
	
	GetAvailableEmployees = 51,
	GetAvailableTime = 52,
	SaveTimeScheduleShift = 53,
	SaveOrderShift = 54,
	SaveOrderAssignments = 55,
	DeleteTimeScheduleShift = 56,
	HandleTimeScheduleShift = 57,
	SplitTimeScheduleShift = 58,
	DragTimeScheduleShift = 59,
	DragTimeScheduleShiftMultipel = 60,
	DragTemplateTimeScheduleShift = 61,
	DragTemplateTimeScheduleShiftMultipel = 62,
	RemoveEmployeeFromShiftQueue = 63,
	//GenerateAndSaveDeviationsFromShiftsJob = 64,  //NOT USED
	AssignTaskToEmployee = 65,
	AssignTemplateShiftTask = 66,
	AssignTimeScheduleTemplateToEmployee = 67,
	SplitTemplateTimeScheduleShift = 68,
	ActivateScenario = 69,
	SaveTimeScheduleScenarioHead = 70,
	GenerateAndSaveAbsenceFromStaffing = 71,
	PerformAbsenceRequestPlanningAction = 72,
	PerformRestoreAbsenceRequestedShifts = 73,
	GetBreaksForScheduleBlock = 74,
	HasEmployeeValidTimeCodeBreak = 75,
	ValidateBreakChange = 76,
	RemoveAbsenceInScenario = 77,
	
	SaveEvaluateAllWorkRulesByPass = 81,
	EvaluateAllWorkRules = 82,
	EvaluatePlannedShiftsAgainstWorkRules = 83,
	EvaluatePlannedShiftsAgainstWorkRulesEmployeePost = 84,
	EvaluateAbsenceRequestPlannedShiftsAgainstWorkRules = 85,
	EvaluateDragShiftAgainstWorkRules = 86,
	EvaluateDragShiftAgainstWorkRulesMultipel = 87,
	EvaluateDragTemplateShiftAgainstWorkRules = 88,
	EvaluateDragTemplateShiftAgainstWorkRulesMultipel = 89,
	EvaluateAssignTaskToEmployeeAgainstWorkRules = 90,
	EvaluateSplitShiftAgainstWorkRules = 91,
	EvaluateSplitTemplateShiftAgainstWorkRules = 92,
	EvaluateActivateScenarioAgainstWorkRules = 93,
	EvaluateDeviationsAgainstWorkRulesAndSendXEMail = 94,
	EvaluateScenarioToTemplateAgainstWorkRules = 95,
	EvaluateScheduleSwapAgainstWorkRules = 96,
	IsDayAttested = 97,
	
	ImportHolidays = 101,
	CalculateDayTypeForEmployee = 102,
	CalculateDayTypesForEmployee = 103,
	CalculateDayTypeForEmployees = 104,
	SaveUniqueday = 105,
	UpdateUniqueDayFromHalfDay = 106,
	SaveUniqueDayFromHalfDay = 107,
	AddUniqueDayFromHoliday = 108,
	DeleteUniqueDayFromHoliday = 109,
	UpdateUniqueDayFromHoliday = 110,
	CreateTransactionsForEarnedHoliday = 111,
	DeleteTransactionsForEarnedHoliday = 112,
	
	EmployeeActiveScheduleImport = 113,
	
	
	
	//GetSequentialDeviations = 201, //Not used anymore
	//GetTimeCodesGeneratedByTimeRules = 202, //Not used anymore
	//GenerateDeviations = 203, //Not used anymore
	GenerateDeviationsFromTimeInterval = 204,
	//GenerateAndSaveDeviations = 205, //Not used anymore
	//ReGenerateTransactions = 206, //Not used anymore
	ReGenerateTransactionsDiscardAttest = 207,
	//RearrangeTimeBlockAgainstExisting = 208, //Not used anymore
	//SaveGeneratedDeviationsSL = 209, //Not used anymore
	SaveGeneratedDeviations = 210,
	SaveWholedayDeviations = 211,
	SaveTimeCodeTransactions = 212,
	SaveTimeBlocksFromProjectTimeBlock = 213,
	CleanDays = 214,
	MobileModifyBreak = 215,
	AddModifyTimeBlocks = 216,
	ValidateDeviationChange = 217,
	//SaveAttestEmployeeAdditionDeduction = 218, //Not used anymore
	//GetUnhandledShiftChangesInputDTO = 219, //Not used anymore
	RecalculateUnhandledShiftChanges = 220,
	RecalculateAccounting = 221,
	GetDayOfAbsenceNumber = 222,
	CreateAbsenceDetails = 223,
	SaveAbsenceDetailsRatio = 224,
	GetDeviationsAfterEmployment = 225,
	DeleteDeviationsDaysAfterEmployment = 226,
	CreateTransactionsForPlannedPeriodCalculation = 227,
	
	SynchTimeStamps = 301,
	ReGenerateDayBasedOnTimeStamps = 302,
	SaveTimeStampsFromJob = 303,
	SynchGTSTimeStamps = 304,
	
	SaveAttestForEmployee = 401,
	SaveAttestForEmployees = 402,
	SaveAttestForTransactions = 403,
	//SaveAttestForExternalTransactions = 404, //Not used anymore
	RunAutoAttest = 405,
	//HasAttestedDeviations = 406, //Not used anymore
	SendAttestReminder = 407,
	
	
	
	GetUnhandledPayrollTransactions = 501,
	LockPayrollPeriod = 502,
	UnLockPayrollPeriod = 503,
	RecalculatePayrollPeriod = 504,
	RecalculateAccountingFromPayroll = 505,
	RecalculateExportedEmploymentTaxJOB = 506,
	//ReverseTransactions = 507, //Not used anymore
	SavePayrollTransactionAmounts = 508,
	AssignPayrollTransactionsToTimePeriod = 509,
	ReverseTransactionsValidation = 510,
	ReverseTransactions = 511,
	SaveExpenseValidation = 512,
	SaveExpense = 513,
	DeleteExpense = 514,
	ClearPayrollCalculation = 515,
	
	SaveFixedPayrollRows = 521,
	SaveAddedTransaction = 522,
	CreateAddedTransactionsFromTemplate = 523,
	SavePayrollScheduleTransactions = 524,
	
	SaveVacationYearEnd = 531,
	DeleteVacationYearEnd = 532,
	CreateFinalSalary = 533,
	DeleteFinalSalary = 534,
	ValidateVacationYearEnd = 535,
	
	SaveAccountProvisionBase = 541,
	LockAccountProvisionBase = 542,
	UnLockAccountProvisionBase = 543,
	UpdateAccountProvisionTransactions = 544,
	SaveAttestForAccountProvision = 545,
	
	SavePayrollStartValues = 551,
	SaveTransactionsForPayrollStartValues = 552,
	DeleteTransactionsForPayrollStartValues = 553,
	DeletePayrollStartValueHead = 554,
	
	SaveRetroactivePayroll = 561,
	SaveRetroactivePayrollOutcome = 562,
	CalculateRetroactivePayroll = 563,
	DeleteRetroactivePayroll = 564,
	DeleteRetroactivePayrollOutcomes = 565,
	CreateRetroactivePayrollTransactions = 566,
	DeleteRetroactivePayrollTransactions = 567,
	
	PayrollImport = 570,
	RollbackPayrollImport = 571,
	ValidatePayrollImport = 572,
	
	InitiateScheduleSwap = 580,
	ApproveScheduleSwap = 581,
	
	CalculateTimeWorkAccountYearEmployee = 590,
	TimeWorkAccountChoiceSendXEMail = 591,
	TimeWorkAccountGenerateOutcome = 592,
	TimeWorkAccountReverseTransaction = 593,
	TimeWorkAccountGenerateUnusedPaidBalance = 594,
	CalculateTimeWorkAccountYearEmployeeBasis = 595,
	TimeWorkAccountYearReversePaidBalance = 596,
	
	RecalculatePayrollControllResult = 597,
	
	CalculateTimeWorkReductionReconciliationYearEmployee = 598,
	TimeWorkReductionReconciliationYearEmployeeGenerateOutcome = 599,
	TimeWorkReductionReconciliationYearEmployeeReverseTransactions = 600,
	
}

export enum SoeTimeEngineTemplateType {
	None = 0,
	TimeBlocksFromTemplate = 1,
	TransactionsFromTimeBlocks = 2,
}

export enum SoeTimeSalaryExportTarget {
	Undefined = 0,
	SoeXe = 1,
	KontekLon = 2,
	SoftOne = 3,
	Carat2000 = 4,
	SvenskLon = 5,
	AgdaLon = 6,
	Hogia214006 = 7,
	Hogia214002 = 8,
	Spcs = 9,
	Personec = 10,
	Sauma = 11,
	PAxml = 12,
	DiLonn = 13,
	Flex = 14,
	Tikon = 15,
	DLPrime3000 = 16,
	BlueGarden = 17,
	Orkla = 18,
	Fivaldi = 19,
	TikonCSV = 20,
	Hogia214007 = 21,
	AditroL1 = 22,
	HuldtOgLillevik = 23,
	Netvisor = 24,
	PAxml2_1 = 25,
	Pol = 26,
	SDWorx = 27,
	Lessor = 28,
}

export enum SoeTimeSalaryExportFormat {
	XML = 1,
	Text = 2,
	Zip = 3
}

export enum SoeTimeSalaryPaymentExportFormat {
	Unknown = 0,
	XML = 1,
	Text = 2,
}

export enum SoeTimeAttestFunctionOption {
	RestoreToSchedule = 1,
	RestoreScheduleToTemplate = 2,
	ReGenerateDaysBasedOnTimeStamps = 3,
	Deprecated = 4,
	DeleteTimeBlocksAndTransactions = 5,
	ReGenerateTransactionsDiscardAttest = 6,
	ReGenerateVacationsTransactionsDiscardAttest = 7,
	OpenAbsenceDialog = 8,
	ReverseTransactions = 9,
	ScenarioRemoveAbsence = 10,
	AttestReminder = 11,
	RunAutoAttest = 12,
	RecalculateAccounting = 13,
	RestoreToScheduleDiscardDeviations = 14,
	UpdateAbsenceDetails = 15,
	CalculatePeriods = 16,
}

export enum SoeTimePayrollCalculationOption {
	RecalculatePeriod = 1,
	LockPeriod = 2,
	UnLockPeriod = 3,
	FinalSalary = 4,
	GetUnhandledPayrollTransactions = 5,
}

export enum TimeScheduledTimeSummaryType {
	Both = 0,    // Used in queries
	ScheduledTime = 1,
	TemplateScheduledTime = 2
}

export enum SoeTimeScheduleTemplateBlockBreakType {
	None = 0,
	NormalBreak = 1,
}

export enum SoeTimeScheduleTemplateBlockLendedType {
	None = 0,
	LendedToOther = 1,
	LendedInFromOther = 2,
}

export enum SoeTimeTransactionType {
	Unknown = 0,
	TimeCode = 1,
	TimePayroll = 2,
	TimeInvoice = 3,
}

export enum SoeModuleIconType {
	Unknown = 0,
	Personell = 7099,
	Schedule = 7100,
	Time = 7101,
	Payroll = 7102,
}

export enum SoeTimePeriodAccountValueType {
	Unknown = 0,
	Provision = 1,
}

export enum SoeTimePeriodAccountValueStatus {
	Open = 0,
	Locked = 1,
}

export enum SoeTimeWorkAccountYearOutcomeType {
	Selection = 0,
	AccAdjustment = 1,
}

export enum SoeTimeUniqueDayEdit {
	Undefined = 0,
	AddHalfDay = 1,
	UpdateHalfDay = 2,
	DeleteHalfDay = 3,
	AddHoliday = 4,
	UpdateHoliday = 5,
	DeleteHoliday = 6,
}

export enum SoeTranslationClient {
	GoogleTranslate = 1,
	BingTranslate = 2,
}

export enum SoeSysLogRecordType {
	Unknown = 0,
	TimeTerminal = 1,
}

export enum SoeVacationGroupDayType {
	Undefined = 0,
	VacationFiveDaysPerWeek = 1
}

export enum IdSuperKeyType {
	Unknown = 0,
	SoftOneIdRequest = 1,
	SoftOneStatusRequest = 2
}

export enum SysParameterType {
	Undefined = 0,
	StatisticsCentralBureauCountiesAndTownsList = 1,
}

export enum SoeXeArticle {
	AccountingStart = 1,
	Billing = 2,
	CustomerLedger = 3,
	PriceList = 4,
	SupplierLedgerStart = 5,
	User = 6,
	Currency = 7,
	TimeProjectBilling = 8,
	Offert = 9,
	Order = 10,
	EDI = 11,
	Contract = 12,
	Scanning = 13,
	Inventory = 14,
	TimeStart = 15,
	TimeAutoAttest = 16,
	TimeTerminal = 17,
	AccountingStandard = 18,
	AccountingPlus = 19,
	SupplierLedgerStandard = 20,
	TimeProjectBillingReport = 24,
}

export enum StaffingNeedsHeadType {
	NeedsPlanning = 0,
	NeedsShifts = 1
}

export enum StaffingNeedsRowType {
	Normal = 0,
	ShiftTypeSummary = 1,
	TotalSummary = 2,
}

export enum StaffingNeedsRowOriginType {
	StaffingNeedsAnalysisChartData = 0,
	TimeScheduleTask = 1,
	IncomingDelivery = 2,
}

export enum SysExtraFieldType {
	AveragingOvertimeCalculation = 101,
	
	// See also TermGroup_SysExtraField
}

export enum SystemInfoLogLevel {
	None = 0,
	Information = 1,
	Warning = 2,
	Error = 3
};

export enum SystemInfoType {
	
	Unknown = 0,
	EmployeeSkill_Use = 1,
	EmployeeSkill_Ends = 2,
	EmployeeSchedule_Use = 3,
	EmployeeSchedule_Ends = 4,
	ClosePreliminaryTimeScheduleTemplateBlocks_Use = 5,
	ClosePreliminaryTimeScheduleTemplateBlocks = 6,
	AttestReminder_Use = 7,
	PublishScheduleAutomaticly_Use = 8,
	PublishScheduleAutomaticly = 9,
	BirthdayReminder_Use = 10,
	BirthdayReminder = 11,
	ReminderOrderSchedule_Use = 12,
	ReminderOrderSchedule = 13,
	ReminderIllness = 14,
	ReminderIllnessDays = 15,
	ReminderIllnessSocialInsuranceAgency = 16,
	ReminderIllnessDaysSocialInsuranceAgency = 17,
	ReminderEmployment = 18,
	ReminderEmploymentDays = 19,
	TimeStamp_TimeScheduleTemplatePeriodMissing = 20,
	TimeStamp_EmployeeMissing = 21,
	RemoveEmployee = 22,
	UseEmployeeExperienceReminder = 23,
	EmployeeExperienceReminderMonths = 24,
	ReminderDaysBeforeEmployeeExperienceReached = 25,
	UseEmployeeAgeReminder = 26,
	EmployeeAgeReminderAges = 27,
	ReminderDaysBeforeEmployeeAgeReached = 28,
	ReminderAfterLongAbsence = 29,
	ReminderAfterLongAbsenceDaysInAdvance = 30,
	IsReminderAfterLongAbsenceAfterDays = 31,
	TimeAccumulatorWarning = 32,
	UseUpdateEmployeeExperienceReminder = 33,
	ReminderIllnessEmailSocialInsuranceAgency = 34,
	
};

export enum SysWholesellerEdiIdEnum {
	Unknown = 0,
	Ahlsell = 1,
	Elektroskandia = 2,
	Selga = 4,
	Storel = 8,
	Solar = 13,
	Dahl = 20,
	Lundagrossisten = 22,
	Moel = 23,
	VVSCentrum = 25,
	NelfoGeneric = 36,
	Comfort = 40,
	LvisNetGeneric = 42,
	Onninen = 43,
	Elkedjan = 44,
	Elgrossen = 45,
	EELTeknik = 46,
	BraGross = 47,
}

export enum SysLinkTableRecordType {
	Default = 0,  // Not set
	LinkSysReportTemplateToCountryId = 1,  // With the value 1, record is used to link sysreport into countrycode / Jukka
}

export enum SysLinkTableIntegerValueType   {// Target field to ease remembering
	Default = 0,  // Not set
	SysCountryId = 1,  // With this value the dbo.SysLinkTable -> SysLinkTableIntegerValue is linked to dbo.SysCountry -> SysCountryId
}

export enum TemplateDesignerComponent {
	TextBox = 1,
	TextArea = 2,
	CheckBox = 3,
	RadioButton = 4,
	DatePicker = 5,
	Select = 6,
	TypeAhead = 7,
	Instruction = 8,
	
	// Special
	EmployeeAccount = 21,
	DisbursementAccount = 22,
	EmploymentPriceTypes = 23,
	Position = 24
}

export enum TermGroup {
	Unknown = 0,
	General = 1,
	AccountType = 2,
	YesNoDefault = 3,
	Language = 4,
	Role = 5,
	VatAccount = 6,
	AmountStop = 7,
	SysPaymentType = 8,
	SysContactEComType = 9,
	SysContactType = 10,
	SysContactAddressType = 11,
	SysContactAddressRowType = 12,
	AccountStatus = 13,
	SieAccountDim = 14,
	SruCode = 15,
	SysReportType = 16,
	ReportExportType = 17,
	Report = 18,
	SysReportTemplateType = 19,
	ReportGroupHeader = 20,
	VoucherRowHistorySortField = 21,
	VoucherRowHistorySortOrder = 22,
	SysDayType = 23,
	SysHoliday = 24,
	SysCountry = 25,
	SysCurrency = 26,
	InvoiceBillingType = 27,
	InvoiceVatType = 28,
	PaymentMethod = 29,
	OriginStatus = 30,
	OriginType = 31,
	ChangeStatusGrid = 32,
	//SupplierInvoiceEdit = 33,
	//CustomerInvoiceEdit = 34,
	PaymentStatus = 35,
	SysLbError = 36,
	ReportLedgerInvoiceSelection = 37,
	AccountingRows = 38,
	InvoiceEditUtility = 39,
	ReportSupplierLedgerSortOrder = 40,
	ReportLedgerDateRegard = 41,
	//SupplierPaymentEdit = 42,
	//CustomerPaymentEdit = 43,
	Grid = 44,
	ReportCustomerLedgerSortOrder = 45,
	InvoiceProductRows = 46,
	VoucherGrid = 47,
	InvoiceProductVatType = 48,
	SplitAccountingDialog = 49,
	ReportBillingInvoiceSortOrder = 50,
	ContactPersonPosition = 51,
	TabUtility = 52,
	InvoiceProductSearchDialog = 53,
	Tracing = 54,
	HouseholdTaxDeductionDialog = 55,
	StatusBar = 56,
	HouseholdTaxDeductionGrid = 57,
	TimeCodeBreakStartStopTypes = 58,
	TimeScheduleTemplateGrid = 59,
	GridHeader = 60,
	EmployeeSchedulePlacementGrid = 61,
	PriceRuleEditor = 62,
	TimeRuleGrid = 63,
	TimeRuleGroupGrid = 64,
	NavigationButtons = 65,
	TimeDeviationEdit = 66,
	AttestEntity = 67,
	TimeRuleCopyGrid = 68,
	TimePeriodHeadType = 69,
	TimeAccumulators = 70,
	TimeTransactions = 71,
	TimePeriodsEdit = 72,
	TimeAttestTree = 73,
	TimeProjectEdit = 74,
	TimePeriodDeviation = 75,
	AbsenseDialog = 76,
	SimpleTextEditorDialog = 77,
	TimePeriodSelector = 78,
	UserSelectDialog = 79,
	TimeSelection = 80,
	PayrollType = 81,
	CompanyPayrollProductAccountingPrio = 82,
	CompanyInvoiceProductAccountingPrio = 83,
	EmployeeGroupPayrollProductAccountingPrio = 84,
	EmployeeGroupInvoiceProductAccountingPrio = 85,
	TimeProjectPayrollProductAccountingPrio = 86,
	TimeProjectInvoiceProductAccountingPrio = 87,
	PayrollProductAccountingPrio = 88,
	InvoiceProductAccountingPrio = 89,
	SOEMessageBox = 90,
	AccountingSettings = 91,
	TimeSalaryExport = 92,
	MergeInvoiceProductRows = 93,
	TimeDeviationCauseType = 94,
	TimeCodeRoundingType = 95,
	ScheduledJobRecurrenceType = 96,
	ScheduledJobRetryType = 97,
	ScheduledJobState = 98,
	//SysJobGrid = 99,
	//SysScheduledJobGrid = 100,
	JobSettings = 101,
	CrontabDialog = 102,
	ScheduledJobLogLevel = 103,
	AccumulatorTimePeriodType = 104,
	TimeTerminalType = 105,
	//ProjectInvoiceEdit = 106,
	InitProductSearch = 107,
	ContactAddresses = 108,
	CustomerGrid = 109,
	InvoiceProductGrid = 110,
	ActorPayment = 111,
	Categories = 112,
	InvoiceProductPricelists = 113,
	EDIOrderStatus = 114,
	EDIInvoiceStatus = 115,
	EDISourceType = 116,
	EDIArrivalsGrid = 117,
	EDIStatus = 118,
	//OrderInvoiceEdit = 119,
	StandardDayOfWeek = 120,
	ReportBillingOrderSortOrder = 121,
	ReportBillingOfferSortOrder = 122,
	ProjectCentral = 123,
	TimePeriodDeviationViewMode = 124,
	OrderInvoiceTransferDialog = 125,
	CustomerInvoiceOriginateFrom = 126,
	ContractGroupPeriod = 127,
	ContractGroupPriceManagement = 128,
	ContractGroupEdit = 129,
	ReportBillingContractSortOrder = 130,
	CheckSettingsArea = 131,
	CheckSettingsResultType = 132,
	CheckSettings = 133,
	AttestRuleRowLeftValueType = 134,
	AttestRuleRowRightValueType = 135,
	AttestRuleGrid = 136,
	HelpTextGrid = 137,
	HelpType = 138,
	AccountDistributionGrid = 139,
	AccountDistributionEntry = 140,
	AccountDistributionTriggerType = 141,
	AccountDistributionCalculationType = 142,
	ScanningStatus = 143,
	ScanningMessageType = 144,
	AccountDistributionPeriodType = 145,
	ImageViewer = 146,
	AccountDistributionPeriodDialog = 147,
	ScanningInterpretation = 148,
	InventoryWriteOffMethodType = 149,
	InventoryWriteOffMethodPeriodType = 150,
	InventoryStatus = 151,
	InventoryLogType = 152,
	StateAnalysis = 153,
	StartPage = 154,
	InventoryWriteOffMethodGrid = 155,
	InventoryWriteOffTemplateGrid = 156,
	//InventorySearchDialog = 157,
	//SupplierInvoiceSearchDialog = 158,
	//CustomerInvoiceSearchDialog = 159,
	InventoryAdjustmentDialog = 160,
	AccountSelectDialog = 161,
	InventoryGrid = 162,
	EmployeeGroups = 163,
	AccountDistributionEntryGrid = 164,
	TimeRuleType = 165,
	TimeRuleDirection = 166,
	TimeAbsenceRuleType = 167,
	TimeAbsenceRuleGrid = 168,
	TimeAbsenceRuleRowType = 169,
	TimeStampEntries = 170,
	TimeStampEntryType = 171,
	TimeRuleCopySourceGroup = 172,
	TimeBlockDateStampingStatus = 173,
	TimeRuleCopySource = 174,
	ShowHelp = 175,
	ImageThumbnail = 176,
	GridBase = 177,
	LanguageTranslation = 178,
	TimeScheduleSyncBatchType = 179,
	TimeScheduleSyncBatchStatus = 180,
	TimeScheduleSyncEntryStatus = 181,
	TimeScheduleSyncGrid = 182,
	EdiMessageType = 183,
	SoeControls = 184,
	ToolBar = 185,
	GridFooter = 186,
	ConverterTerms = 187,
	SysMediaType = 188,
	MediaType = 189,
	MediaFormat = 190,
	//MovieViewer = 191,
	//SysMediaGrid = 192,
	//VideoHelpGrid = 193,
	TimeScheduleSyncLeaveApplicationStatus = 194,
	CloseEdiEntryCondition = 195,
	Dashboard = 196,
	//SysWholesellerPrices = 197,
	TimeScheduleSyncEntryType = 198,
	AutoSaveInterval = 199,
	TimeCodeRegistrationType = 200,
	TimeAdditionDeductionGrid = 201,
	TimeScheduleEmployeePeriodStatus = 202,
	TimeScheduleEmployeePeriodUserStatus = 203,
	TimeScheduleTemplateBlockQueueType = 204,
	TimeSchedulePlanning = 205,
	Sex = 206,
	EmployeeRequestGrid = 207,
	EmployeeRequestType = 208,
	EmployeeRequestStatus = 209,
	Wizard = 210,
	TimeSetupWizard = 211,
	MessageType = 212,
	MessagePriority = 213,
	MessageDeliveryType = 214,
	MessageTextType = 215,
	XEMailGrid = 216,
	ShiftTypeGrid = 217,
	TimeSchedulePlanningViews = 218,
	TimeSchedulePlanningVisibleDays = 219,
	SkillTypeGrid = 220,
	SkillGrid = 221,
	SkillSelector = 222,
	SkillMatcher = 223,
	MultiSelectFilter = 224,
	ContactPersons = 225,
	EmployeeUserEdit = 226,
	PasswordEdit = 227,
	AdjustTimeStampGrid = 228,
	DataStorageGrid = 229,
	InvoiceProductCalculationType = 230,
	CopyCustomerInvoiceRowsDialog = 231,
	SupplierGrid = 232,
	SearchVoucherGrid = 233,
	TimeAccumulatorType = 234,
	AbsenceRequestPlanning = 235,
	SearchEmployeeSkillsGrid = 236,
	PositionGrid = 237,
	YesNo = 238,
	Positions = 239,
	CompanyNewsGrid = 240, // NOT USED
	UploadedFilesGrid = 241,
	EDIPriceSettingRule = 242,
	AttestWorkFlowTemplateGrid = 243,
	AttestWorkFlowTemplateRows = 244,
	AttestWorkFlowArrivalsGrid = 245,
	//CustomerSearchDialog = 246,
	InsertAttestFlowToInvoice = 247,
	UploadFiles = 248,
	AgeDistributionGrid = 249,
	ChecklistHeadType = 250,
	ChecklistEdit = 251,
	ImageGallery = 252,
	ChecklistRowType = 253,
	RadUpload = 254,
	HandleMessagesJob = 255,
	ChecklistRecords = 256,
	MapControl = 257,
	RadMap = 258,
	CurrencySource = 259,
	CurrencyIntervalType = 260,
	RadGridView = 261,
	CompanySetupWizard = 262,
	CurrencyType = 263,
	FieldSettingEdit = 264,
	InsecureDebtsGrid = 265,
	MatchCodeGrid = 266,
	MobileFields = 267,
	MobileForms = 268,
	ChangeWholesaleCustInvRowsDialog = 269,
	MatchCodeType = 270,
	InvoiceMatchingGrid = 271,
	SysExportHeadGrid = 272,
	SysImportHeadGrid = 273,
	ChangeProductsDiscountDialog = 274,
	IOStatus = 275,
	IOType = 276,
	IOSource = 277,
	SysExportDefinitionGrid = 278,
	SysImportDefinitionGrid = 279,
	SysExportDefinitionType = 280,
	SysImportDefinitionType = 281,
	TimeRecalculate = 282,
	AttestFlowRowState = 283,
	AttestWorkFlowType = 284,
	ClaimLevels = 285,
	InvoiceMatchesGrid = 286,
	ProjectStatus = 287,
	ProjectUserType = 288,
	ProjectSearchDialog = 289,
	//ProjectUsers = 290,
	StaffingNeedsAnalysisNbrOfDecimals = 291,
	ShiftHistoryType = 292,
	ShiftHistory = 293,
	PriceListComparison = 294,
	ProjectSelector = 295,
	ProjectSelection = 296,
	ProjectType = 297,
	AttestWorkFlowRowProcessType = 298,
	SupplierInvoiceProjectRows = 299,
	AvailabilityEdit = 300,
	SysNewsDisplayType = 301,
	NewsDialog = 302,
	TimeStamp = 303, // Used for time terminal, should start at 100 since 304 is merged with 303.
	TimeStampNotRegistred = 304, // Used for time terminal login screen
	TimeSheet = 305,
	DistributionCodeGrid = 306,
	SelectAccountsDialog = 307,
	ProjectAllocationType = 308,
	BudgetGrid = 309,
	SysImportDefinitionUpdateType = 310,
	ProductSearchFilterMode = 311,
	VatCodeGrid = 312,
	ImportGrid = 313,
	MultiSelectDialog = 314,
	ImportFile = 315,
	IOImportHeadType = 316,
	ImportBatchesGrid = 317,
	IOGridBase = 318,
	TimeStampAttendanceGaugeShowMode = 319,
	StaffingNeedsAnalysis = 320,
	StaffingNeedsAnalysisInterval = 321,
	StaffingNeedsAnalysisRefType = 322,
	StaffingNeedsAnalysisDateComparer = 323,
	CompanyEdiType = 324,
	TimeScheduleTypeGrid = 325,
	StaffingNeedsAnalysisChartType = 326,
	AttestReminder = 327,
	//CaseProjectGrid = 328,
	//GrossProfitCodeGrid = 329,
	CaseProjectResult = 330,
	CaseProjectPriority = 331,
	CaseProjectType = 332,
	StaffingNeedsLocationGroupGrid = 333,
	StaffingNeedsLocationGrid = 334,
	ShiftTypes = 335,
	StaffingNeedsRuleUnit = 336,
	StaffingNeedsRuleGrid = 337,
	CaseProjectChannel = 338,
	CaseProjectArea = 339,
	CaseProjectApplication = 340,
	Stopwatch = 341,
	SysLogItemEdit = 342,
	TimeTerminalGrid = 343,
	TimeStampStatisticsInterval = 344,
	EmployeeRequestResultStatus = 345,
	ReconciliationGrid = 346,
	AddCheckListMultipleChoiceAnswerDialog = 347,
	FindHelpTabControl = 348,
	SysPositionGrid = 349,
	TextBlockDictEdit = 350,
	EmploymentType = 351,
	PayrollPriceTypeGrid = 352,
	PayrollGroupGrid = 353,
	PayrollGroupEdit = 354,
	//InvoiceTemplateGrid = 355,
	EmploymentPriceTypes = 356,
	ExtendedAbsenceSetting = 357,
	TimeCodeBreakGroupGrid = 358,
	PayrollPriceFormulaGrid = 359,
	PayrollPriceFormulaBuilder = 360,
	SysPayrollPrice = 361,
	VoucherPrintDialog = 362,
	SupplierInvoiceOrderRows = 363,
	//SprintGrid = 364,
	TimeScheduleTemplateBlockType = 365,
	AccountDimsSelector = 366,
	DateSelectDialog = 367,
	PayrollCalculation = 368,
	PayrollCalculationViewMode = 369,
	TimeAttestTreeGrouping = 370,
	TimeAttestTreeSorting = 371,
	EmployeeDisbursementMethod = 372,
	StaffingNeedsHeadInterval = 373,
	StaffingNeedsEdit = 374,
	StaffingNeedsHeadStatus = 375,
	EmploymentChangeFieldType = 376,
	TimeSchedulePlanningShiftStyle = 377,
	EmploymentChangeType = 378,
	PayrollGroupReports = 379,
	EmploymentEndReason = 380,
	PayrollGroupPriceTypes = 381,
	PayrollGroupPriceFormulas = 382,
	SysPayrollPriceType = 383,
	SysPayrollPriceGrid = 384,
	SysPayrollType = 385,
	EmploymentPriceFormulas = 386,
	SysPayrollPriceAmountType = 387,
	PayrollPriceTypePeriods = 388,
	PayrollReviewGrid = 389,
	PayrollReviewStatus = 390,
	PayrollProductGrid = 391,
	PayrollProductPriceTypesAndFormulas = 392,
	PayrollProductCentRoundingType = 393,
	PayrollProductTaxCalculationType = 394,
	CustomerCentral = 395,
	EmployeeTaxType = 396,
	EmployeeTaxAdjustmentType = 397,
	EmployeeTaxSinkType = 398,
	EmployeeTaxSalaryDistressAmountType = 399,
	EmployeeTaxSE = 400,
	TrackChangesAction = 401,
	SelectPayrollTransactionsDialog = 402,
	AddedTransactionDialog = 403,
	EmployeeTaxEmploymentTaxType = 404,
	FixedPayrollRowsGrid = 405,
	PayrollPriceFormulaResultType = 406,
	EmployeeTaxEmploymentAbroadCode = 407,
	VacationGroupType = 408,
	PensionCompany = 409,
	PayrollGroupVacationGroups = 410,
	EmploymentVacationGroup = 411,
	EmployeeChildGrid = 412,
	Stock = 414,
	EdiRecivedMsgState = 415,
	EdiTransferState = 416,
	SupplierPaymentObservationMethod = 417,
	ReportSelectDialog = 418,
	TimeCalendar = 419,
	StockTransactionType = 420,
	ReportPrintoutDeliveryType = 421,
	ReportPrintoutStatus = 422,
	EInvoiceDistributor = 423,
	EInvoiceFormat = 424,
	SysPerformanceMonitorTask = 425,
	InvoiceDeliveryType = 426,
	EndReasonGrid = 427,
	CustomerPaymentDialog = 428,
	VatVerificationGrid = 429,
	PayrollProductCentRoundingLevel = 430,
	ReportExportFileType = 431,
	PayrollProductTimeUnit = 432,
	EmployeeStatistics = 433,
	EmployeeStatisticsType = 434,
	ForeignPaymentBankCode = 435,
	ForeignPaymentForm = 436,
	ForeignPaymentMethod = 437,
	ForeignPaymentChargeCode = 438,
	ForeignPaymentIntermediaryCode = 439,
	QuantityRoundingType = 440,
	EmployeeStatisticsAverageType = 442,
	ReportDrilldownGrid = 441,
	OrderContractType = 443,
	ImportPaymentIOStatus = 444,
	EmployeeVacationSE = 445,
	VacationGroupSE = 446,
	VacationGroupCalculationType = 447,
	VacationGroupRemainingDaysRule = 448,
	PayrollReportsPersonalCategory = 449,
	PayrollReportsWorkTimeCategory = 450,
	PayrollReportsSalaryType = 451,
	VacationGroupVacationDaysHandleRule = 452,
	VacationGroupVacationSalaryPayoutRule = 453,
	VacationGroupVacationAbsenceCalculationRule = 454,
	VacationGroupGuaranteeAmountMaxNbrOfDaysRule = 455,
	VacationGroupVacationHandleRule = 456,
	TimeSalaryPaymentExportType = 457,
	TimeSalaryPaymentExportGrid = 458,
	VacationGroupGrid = 459,
	CardNumberGrid = 460,
	EmployeeFactorType = 462,
	EmployeeFactors = 463,
	ValidateUserCredentials = 464,
	SysHouseholdType = 465,
	MassRegistrationTemplateGrid = 466,
	MassRegistrationTemplateInputType = 467,
	ChangePayrollProductSettingsPeriodDialog = 468,
	VoucherRowHistoryEvent = 469,
	VoucherRowHistoryField = 470,
	AttestGroupEdit = 471,
	//StockGrid = 472,
	FollowUpTypeGrid = 473,
	FollowUpTypeType = 474,
	CategoryType = 475,
	ChangeStatusGridAllItemsSelection = 476,
	SupplierCentral = 477,
	InvoicePaymentService = 478,
	TransferRowsToContract = 479,
	SupplierSearchDialog = 480,
	ReportGroupAndSortingTypes = 481,
	ExportPaymentServiceGrid = 482,
	ExportPaymentServiceEdit = 483,
	AttestGridFilterDialog = 484,
	AccountMandatoryLevel = 485,
	CompanyGroup = 486,
	VacationGroupYearEndRemainingDaysRule = 487,
	VacationGroupYearEndOverdueDaysRule = 488,
	VacationGroupYearEndVacationVariableRule = 489,
	PayrollPriceTypes = 490,
	SendEmployeeReminderJob = 491,
	SysVatAccountName = 492,
	PayrollGroupPayrollProducts = 493,
	VehicleType = 494,
	EmployeeUnionFees = 495,
	SysPayrollStartValue = 496,
	SiteType = 497, //SSU
	EventType = 498, //SSU
	PerformanceResultType = 499, //SSU
	PerformanceSettingParameterType = 500, //SSU
	SysPageStatusSiteType = 501,
	SysPageStatusStatusType = 502,
	BudgetType = 503,
	BudgetStatus = 504,
	SupplierInvoiceType = 505,
	AttestWorkFlowApproverType = 506,
	PayrollReportsAFACategory = 507,
	PayrollReportsAFASpecialAgreement = 508,
	PayrollReportsCollectumITPplan = 509,
	AttestGroupSuggestionPrio = 510,
	OrderType = 511,
	ReportBillingStockSortOrder = 512,
	CsrGridSelection = 513,
	TimeSchedulePlanningDayViewGroupBy = 514,
	TimeSchedulePlanningDayViewSortBy = 515,
	TimeSchedulePlanningScheduleViewGroupBy = 516,
	TimeSchedulePlanningScheduleViewSortBy = 517,
	SysWholesellerType = 518,
	SysWholesellerSettingType = 519,
	TimeSchedulePlanningEmployeeListSortBy = 520,
	StaffingNeedsDayViewGroupBy = 521,
	StaffingNeedsScheduleViewGroupBy = 522,
	StaffingNeedsDayViewSortBy = 523,
	DailyRecurrencePatternType = 524,
	DailyRecurrencePatternWeekIndex = 525,
	DailyRecurrenceRangeType = 526,
	AccountingBudgetSubType = 527,
	AccountingBudgetType = 528,
	WebTimeStamp = 529,
	RecurrencePattern = 530,
	SalesBudgetInterval = 531,
	StaffingNeedHeadsFilterType = 532,
	EmployeePostWeekendType = 533,
	SysHolidayTypes = 534,
	RetroactivePayrollStatus = 535,
	RetroactivePayrollEmployeeStatus = 536,
	RetroactivePayrollAccountType = 537,
	TimeSchedulePlanningFollowUpCalculationType = 538,
	SysVehicleFuelType = 539,
	EdistributionTypes = 540,
	EDistributionStatusType = 541,
	GridDateSelectionType = 542,
	RetroactivePayrollOutcomeErrorCode = 543,
	AccountDistributionRegistrationType = 544,
	TimeSchedulePlanningBreakVisibility = 545,
	TrackChangesColumnType = 546,
	PersonalDataActionType = 547,
	PersonalDataInformationType = 548,
	SoeEntityType = 549,
	TrackChangesActionMethod = 550,
	ScanningReferenceTargetField = 551,
	ScanningCodeTargetField = 552,
	PersonalDataType = 553,
	OrderPlanningShiftInfo = 554,
	AssignmentTimeAdjustmentType = 555,
	TemplateScheduleActivateFunctions = 556,
	ExpenseType = 557,
	SysReportTemplateTypeGroup = 558,
	TimeReportType = 559,
	QualifyingDayCalculationRule = 560,
	TextBlockType = 561,
	NOT_USED = 562,
	PerformanceTestInterval = 563,
	TaskWatchLogResultCalculationType = 564,
	RecalculateTimeHeadStatus = 565,
	RecalculateTimeRecordStatus = 566,
	VoucherRowMergeType = 567,
	VoucherHeadSourceType = 568,
	InformationSeverity = 569,
	InformationStickyType = 570,
	SoeScheduleWorkRules = 571,
	MassRegistrationImportType = 572,
	TimeScheduleScenarioHeadSourceType = 573,
	SeleniumBrowser = 574,
	TestCaseType = 575,
	TestCaseSettingType = 576,
	SeleniumType = 577,
	OrderEdiTransferMode = 578,
	FileDisplaySortBy = 579,
	IncludeExpenseInReportType = 580,
	KPAAgreementType = 581,
	KPABelonging = 582,
	KPAEndCode = 583,
	ApiMessageType = 584,
	ApiMessageStatus = 585,
	ApiMessageChangeType = 586,
	EmployeeChangeFieldType = 587,
	PrognosTypes = 588,
	ApiMessageSourceType = 589,
	GoTimeStampIdentifyType = 590,
	EventHistoryType = 591,
	AttestRoleUserAccountPermissionType = 592,
	HouseHoldTaxDeductionType = 593,
	AdjustQuantityByBreakTime = 594,
	MobileHouseHoldTaxDeductionType = 595,
	TimeIntervalPeriod = 596,
	TimeIntervalStart = 597,
	TimeIntervalStop = 598,
	SysTimeInterval = 599,
	ScheduledJobLogStatus = 600,
	AverageSalaryCostChartSeriesType = 601,
	VacationBalance = 602,
	TimeStampEntryStatus = 603,
	TimeStampEntryOriginType = 604,
	ApiMessageState = 605,
	PayrollImportEmployeeScheduleStatus = 606,
	PayrollImportEmployeeTransactionType = 607,
	PayrollImportEmployeeTransactionStatus = 608,
	PayrollImportHeadType = 609,
	PayrollImportHeadFileType = 610,
	VacationYearEndHeadContentType = 611,
	CustomerSpecificExports = 612,
	TimeTransactionMatrixColumns = 613,
	PayrollTransactionMatrixColumns = 614,
	EmployeeListMatrixColumns = 615,
	CompanyExternalCodeEntity = 616,
	TimeTerminalAttendanceViewSortOrder = 617,
	PayrollImportHeadStatus = 618,
	PayrollImportEmployeeStatus = 619,
	ScheduleTransactionMatrixColumns = 620,
	TimeCodeRuleType = 621,
	EmployeeDateMatrixColumns = 622,
	TimeStampEntryMatrixColumns = 623,
	EmployeeSelectionAccountingType = 624,
	TimeAccumulatorCompareModel = 626,
	ReportUserSelectionAccessType = 627,
	PurchaseOrderSortOrder = 628,
	ApiSettingType = 629,
	DataStorageRecordAttestStatus = 630,
	UserMatrixColumns = 631,
	SupplierMatrixColumns = 632,
	InvoiceProductMatrixColumns = 633,
	CustomerMatrixColumns = 634,
	CompanyTemplateCopyValues = 635,
	StaffingneedsFrequencyMatrixColumns = 636,
	EmployeeSkillMatrixColumns = 637,
	OrganisationHrMatrixColumns = 638,
	ShiftTypeSkillMatrixColumns = 639,
	TimeSalaryPaymentExportBank = 640,
	EmployeeEndReasonsMatrixColumns = 641,
	EmployeeSalaryMatrixColumns = 642,
	InsightChartTypes = 643,
	EmployeeTimePeriodMatrixColumns = 644,
	StaffingStatisticsMatrixColumns = 645,
	AggregatedTimeStatisticsMatrixColumns = 646,
	FixedInsights = 647,
	EmployeeMeetingMatrixColumns = 648,
	MatrixGroupAggOption = 649,
	MatrixDateFormatOption = 650,
	ScheduledTimeSummaryMatrixColumns = 651,
	EmployeeExperienceMatrixColumns = 652,
	EmployeeDocumentMatrixColumns = 653,
	ImportDynamicFileTypes = 654,
	ExtraFieldTypes = 655,
	EmployeeAccountMatrixColumns = 656,
	TimeScheduleScenarioEmployeeStatus = 657,
	InvoiceDeliveryProvider = 658,
	TimeScheduleSwapRequestStatus = 659,
	TimeScheduleSwapRequestRowStatus = 660,
	GTPAgreementNumber = 661,
	ReportStatisticsMatrixColumns = 662,
	StockPurchaseGenerationOptions = 663,
	EmployeeTemplateGroupRowType = 664,
	SoftOneStatusResultMatrixColumns = 665,
	SoftOneStatusUpTimeMatrixColumns = 666,
	SoftOneStatusEventMatrixColumns = 667,
	EmployeeFixedPayLinesMatrixColumns = 668,
	TimeAbsenceRuleRowScope = 669,
	PaymentTransferStatus = 670,
	EmploymentHistoryMatrixColumns = 671,
	PayrollProductsMatrixColumns = 672,
	EmployeeSalaryDistressMatrixColumns = 673,
	OrderAnalysisMatrixColumns = 674,
	IntrastatTransactionType = 675,
	EmployeeSalaryUnionFeesMatrixColumns = 676,
	EmploymentDaysMatrixColumns = 677,
	TimeWorkAccountWithdrawalMethod = 678,
	TimeWorkAccountYearEmployeeStatus = 679,
	EmployeeTemplateGroupType = 680,
	SalaryExportUseSocSecFormat = 681,
	InvoiceAnalysisMatrixColumns = 682,
	ScheduledJobSettingType = 683,
	BridgeJobType = 684,
	AccountHierarchyMatrixColumns = 685,
	TimeSchedulePlanningShiftTypePosition = 687,
	TimeSchedulePlanningTimePosition = 688,
	MonthlyWorkTimeCalculationType = 689,
	ApiEmployee = 690,
	ControlEmployeeSchedulePlacementType = 691,
	IFPaymentCode = 692,
	UnionFeeAssociation = 693,
	PayrollProductReportSettingType = 694,
	AnnualProgressMatrixColumns = 695,
	BridgeJobFileType = 696,
	LongtermAbsenceMatrixColumns = 697,
	VacationBalanceMatrixColumns = 698,
	TimeWorkAccountYearResultCode = 699,
	SkandiaPensionType = 700,
	SkandiaPensionCategory = 701,
	SkandiaPensionReportType = 702,
	TimeWorkAccountYearSendMailCode = 703,
	TimeWorkAccountGenerateOutcomeCode = 704,
	ShiftQueueMatrixColumns = 705,
	ShiftHistoryMatrixColumns = 706,
	ShiftRequestMatrixColumns = 707,
	AbsenceRequestMatrixColumns = 708,
	TimeScheduleTemplateBlockQueueStatus = 709,
	SEFPaymentCode = 710,
	EmployeeSettingType = 711,
	ReportBillingDateRegard = 712,
	VismaPayrollChangesMatrixColumns = 713,
	TimeStampHistoryMatrixColumns = 714,
	BrandingCompanies = 715,
	CentralBankCode = 716,
	VacationYearEndResult = 717,
	VerticalTimeTrackerMatrixColumns = 718,
	HorizontalTimeTrackerMatrixColumns = 719,
	LicenseInformationMatrixColumns = 720,
	ReportSettingType = 721,
	FinnishTaxReturnExportTaxPeriodLength = 722,
	FinnishTaxReturnExportCause = 723,
	AccountYearStatus = 724,
	PayrollControlFunctionType = 725,
	PayrollControlFunctionStatus = 726,
	PayrollControlFunctionOutcomeChangeType = 727,
	PayrollControlFunctionOutcomeChangeFieldType = 728,
	GrossMarginCalculationType = 729,
	DatePeriodSelection = 730,
	ISOPaymentAccountType = 731,
	TimeLeisureCodeSettingType = 732,
	TimeLeisureCodeType = 733,
	TimeTreeWarningFilter = 734,
	ScheduledJobSpecifiedType = 735,
	SieExportType = 736,
	ChainAffiliation = 737,
	ExtraFieldValueType = 738,
	AgiAbsenceMatrixColumns = 739,
	InvoiceProductUnitConvertMatrixColumns = 740,
	SieExportVoucherSort = 741,
	TimeTerminalLogLevel = 742,
	BygglosenSalaryType = 743,
	SupplierInvoiceSource = 744,
	SupplierInvoiceState = 745,
	SignatoryContractPermissionType = 746,
	ExcludeFromWorkTimeWeekCalculationItems = 747,
	InventoryMatrixColumns = 748,
	FileImportStatus = 749,
	EmployeeChildMatrixColumns = 751,
	EmployeePayrollAdditionsMatrixColumns = 752,
	ProjectBudgetPeriodType = 753,
	AnnualLeaveGroupType = 754,
	SignatoryContractAuthenticationMethodType = 755,
	TimeScheduleTemplateBlockAbsenceType = 756,
	TimeSchedulePlanningAnnualLeaveBalanceFormat = 757,
	AnnualLeaveTransactionMatrixColumns = 758,
	PurchaseCartStatus = 759,
	InvoiceRowImportType = 760,
	TimeWorkReductionPeriodType = 761,
	TimeWorkReductionCalculationRule = 762,
	AnnualLeaveTransactionType = 763,
	TimeWorkReductionWithdrawalMethod = 764,
	DepreciationMatrixColumns = 765,
	TimeSchedulePlanningShiftStartsOnDay = 766,
	TimeWorkReductionReconciliationEmployeeStatus = 767,
	TimeWorkReductionReconciliationResultCode = 768,
	PurchaseCartPriceStrategy = 769,
	SysExtraField = 771,
	TimeCodeRankingOperatorType = 772,
	TimeScheduleCopyHeadType = 773,
	TimeScheduleCopyRowType = 774,
	TimeCodeClassification = 775,
	PayrollReportsJobStatus = 776,
	ImportPaymentType = 777,
	SwapShiftMatrixColumns = 778,
	TaxDeductionType = 780,
	SoeHouseholdClassificationGroup = 781,
	TaxDeductionBalanceListReportSortOrder = 782,
	
	// Below are reserved for angular
	AngularCore = 1000,         // Core terms, used in whole system
	AngularError = 1001,        // Common error messages
	AngularCommon = 1002,       // Common terms, such as Code, Name etc
	AngularEconomy = 1003,      // Terms specific for Economy module
	AngularTime = 1004,         // Terms specific for Time module
	AngularBilling = 1005,      // Terms specific for Billing module
	AngularManage = 1006,       // Terms specific for Manage module
	AngularAgGrid = 1007,       // Terms specific for AgGrid used in AngularSpa project
	AngularClientManagement = 1008, // Terms specific for Client Management module
};

export enum TermGroup_AccountType {
	Asset = 1,
	Debt = 2,
	Income = 3,
	Cost = 4,
};

export enum TermGroup_YesNoDefault {
	No = 0,
	Yes = 1,
	Default = 2,
};

export enum TermGroup_Languages {
	Unknown = 0,
	Swedish = 1,
	English = 2,
	Finnish = 3,
	Norwegian = 4,
	Danish = 5,
};

export enum TermGroup_Roles {
	Systemadmin = 1,
	Administration = 2,
	Developer = 3,
	Seller = 4,
	Employee = 10, //WT: 0
	Approval = 11, //WT: 1
	Attest = 12, //WT: 2
}

export enum TermGroup_AmountStop {
	Debit = 1,
	Credit = 2,
};

export enum TermGroup_SysPaymentType {
	Unknown = 0,
	BG = 1,
	PG = 2,
	Bank = 3,
	BIC = 4,
	SEPA = 5,
	Autogiro = 6,
	Nets = 7,
	//SOP = 8,
	Cfp = 9,
}

export enum TermGroup_SysContactEComType {
	Unknown = 0,
	Email = 1,
	PhoneHome = 2,
	PhoneJob = 3,
	PhoneMobile = 4,
	Fax = 5,
	Web = 6,
	ClosestRelative = 7,
	CompanyAdminEmail = 8,
	Coordinates = 9,
	IndividualTaxNumber = 10,
	GlnNumber = 11
};

export enum TermGroup_SysContactType {
	Undefined = 0,
	Company = 1,
	Employee = 2,
}

export enum TermGroup_SysContactAddressType {
	Undefined = 0,
	Distribution = 1,
	Visiting = 2,
	Billing = 3,
	Delivery = 4,
	BoardHQ = 5,
};

export enum TermGroup_SysContactAddressRowType {
	Unknown = 0,
	Address = 1,
	AddressCO = 2,
	PostalCode = 3,
	PostalAddress = 4,
	Country = 5,
	StreetAddress = 6,
	EntranceCode = 7,
	Name = 8,
};

export enum TermGroup_AccountStatus {
	New = 1,
	Open = 2,
	Closed = 3,
	Locked = 4,
};

export enum TermGroup_SieAccountDim {
	CostCentre = 1,
	CostUnit = 2,
	Project = 6,
	Employee = 7,
	Customer = 8,
	Supplier = 9,
	Invoice = 10,
	Region = 30,
	Shop = 40,
	Department = 50,
};

export enum TermGroup_ReportExportType {
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
	File = 10,
	
	NoExport = 101,     //Not included in dropdowns (no terms)
	MatrixGrid = 200,
	MatrixExcel = 201,
	MatrixText = 202,
	Insight = 300       //Not included in dropdowns (no terms)
};

export enum TermGroup_VoucherRowHistorySortField {
	VoucherNr = 1,
	AccountNr = 2,
	RegDate = 3,
};

export enum TermGroup_VoucherRowHistorySortOrder {
	Ascending = 1,
	Descending = 2,
};

export enum TermGroup_SysDayType {
	Unknown = 0,
	PublicHoliday = 1,
	Workday = 2,
	Saturday = 3,
	Sunday = 4,
};

export enum TermGroup_Country {
	Uknown = 0,
	SE = 1,
	GB = 2,
	FI = 3,
	NO = 4,
	DK = 6,
};

export enum TermGroup_Currency {
	SEK = 1,
	USD = 2,
	EUR = 3,
	GBP = 4,
	CHF = 5,
	DKK = 6,
	NOK = 7,
	AUD = 8,
	RUB = 9,
	JPY = 10,
	BGN = 11,
	CZK = 12,
	HUF = 13,
	PLN = 14,
	RON = 15,
	HRK = 16,
	TRY = 17,
	BRL = 18,
	CAD = 19,
	CNY = 20,
	HKD = 21,
	IDR = 22,
	ILS = 23,
	INR = 24,
	KRW = 25,
	MXN = 26,
	MYR = 27,
	NZD = 28,
	PHP = 29,
	SGD = 30,
	THB = 31,
	ZAR = 32,
	AED = 33,
	MAD = 34,
	OMR = 35,
	ISK = 36,
	LKR = 37
};

export enum TermGroup_BillingType {
	None = 0,
	Debit = 1,
	Credit = 2,
	Interest = 3,
	Reminder = 4,
};

export enum TermGroup_InvoiceVatType {
	None = 0,
	Merchandise = 1,
	//Service = 2,
	Contractor = 3,
	NoVat = 4,
	EU = 5,
	NonEU = 6,
	ExportWithinEU = 7,
	ExportOutsideEU = 8,
};

export enum TermGroup_VatDeductionType {
	None = 0,
};

export enum TermGroup_SysPaymentMethod {
	None = 0,
	LB = 1,
	PG = 2,
	BGMax = 3,
	SEPA = 4,
	Autogiro = 5,
	Cash = 6,
	Nets = 7,
	SOP = 8,
	Cfp = 9,
	NordeaCA = 10,
	TOTALIN = 11,
	ISO20022 = 12,
	Intrum = 13,
	BBSOCR = 14,
};

export enum TermGroup_ReportLedgerInvoiceSelection {
	All = 1,
	FullyPayed = 2,
	PartlyPayed = 3,
	NotPayed = 4,
	NotPayedAndPartlyPayed = 5,
	FullyPayedAndPartlyPayed = 6,
	Reconciliation = 7,
};

export enum TermGroup_ReportLedgerDateRegard {
	InvoiceDate = 1,
	VoucherDate = 2,
	DueDate = 3,
	PaymentDate = 4,
}

export enum TermGroup_InvoiceProductVatType {
	None = 0,
	Merchandise = 1,
	Service = 2,
};

export enum TermGroup_ReportBillingInvoiceSortOrder {
	InvoiceNr = 1,
	CustomerNr = 2,
	ProjectNr = 3,
}

export enum TermGroup_ContactPersonPosition {
	Unknown = 0,
	Other = 1,
	Buyer = 2,
	FinanceManager = 3,
	Receptionist = 4,
	Seller = 5,
	Assistant = 6,
	MarketingManager = 7,
	Consultant = 8,
	Auditor = 9,
	Lawyer = 10,
	ProductOwner = 11,
}

export enum TermGroup_AttestEntity {
	Unknown = 0,
	
	//Economy
	SupplierInvoice = 1,
	
	//Billing
	Offer = 11,
	Order = 12,
	Contract = 13,
	
	//Time
	PayrollTime = 21,
	InvoiceTime = 22,
	
	//Manage
	CaseProject = 31,
	SigningDocument = 32,
};

export enum TermGroup_AttestEntityGroup {
	EconomyStart = 1,
	EconomyStop = 10,
	BillingStart = 11,
	BillingStop = 20,
	TimeStart = 21,
	TimeStop = 30,
}

export enum TermGroup_TimePeriodType {
	Unknown = 0,
	Billing = 1,
	Payroll = 2,
	RuleWorkTime = 3
}

export enum TermGroup_PayrollType {
	Unknown = 0,
	Work = 1,
	Absence = 2,
	OverTime = 3,
	AddedTime = 4,
	InconvinientWorkingHours = 5,
	Addition = 6,
	Deduction = 7,
	
	//added for reports according to email from PJ
	//Different kind of Absence, replaces Absence = 2, when needed
	//Is extension, but not in AttestManager because of view.
	SickLeave = 8,
	ParentalLeave = 9,
	PregnancyLeave = 10,
	TemporaryParentalLeave = 11,
	LeaveOfAbsence = 12,
	PayedAbsence = 13,
	Vacation = 14,
	CompVacant = 15,
	ShortenWorkingHours = 16,
	TimeBank = 17,
}

export enum TermGroup_CompanyPayrollProductAccountingPrio {
	NotUsed = 0,
	Project = 1,
	Customer = 2,
	EmploymentAccount = 3,
	PayrollProduct = 4,
	EmployeeGroup = 5,
	EmployeeAccount = 6, //Tillhörighet
}

export enum TermGroup_CompanyInvoiceProductAccountingPrio {
	NotUsed = 0,
	Project = 1,
	Customer = 2,
	EmploymentAccount = 3,
	InvoiceProduct = 4,
	EmployeeAccount = 5, //Tillhörighet
}

export enum TermGroup_EmployeeGroupPayrollProductAccountingPrio {
	NotUsed = 0,
	Project = 1,
	Customer = 2,
	EmploymentAccount = 3,
	PayrollProduct = 4,
	EmployeeGroup = 5,
	EmployeeAccount = 6, //Tillhörighet
}

export enum TermGroup_EmployeeGroupInvoiceProductAccountingPrio {
	NotUsed = 0,
	Project = 1,
	Customer = 2,
	EmploymentAccount = 3,
	InvoiceProduct = 4,
	EmployeeAccount = 5, //Tillhörighet
}

export enum TermGroup_TimeProjectPayrollProductAccountingPrio {
	NotUsed = 0,
	Project = 1,
	Customer = 2,
	EmploymentAccount = 3,
	PayrollProduct = 4,
	EmployeeGroup = 5,
	EmployeeAccount = 6, //Tillhörighet
}

export enum TermGroup_TimeProjectInvoiceProductAccountingPrio {
	NotUsed = 0,
	Project = 1,
	Customer = 2,
	EmploymentAccount = 3,
	InvoiceProduct = 4,
	EmployeeAccount = 5, //Tillhörighet
}

export enum TermGroup_PayrollProductAccountingPrio {
	NotUsed = 0,
	NoAccounting = 1,
	Project = 2,
	Customer = 3,
	EmploymentAccount = 4,
	PayrollProduct = 5,
	EmployeeGroup = 6,
	PayrollGroup = 7,
	Company = 8,
	EmployeeAccount = 9 //Tillhörughet
	
	// Sort order
	// 1. Not used          (Sök kontering)
	// 2. PayrollProduct    (Löneart)
	// 3. Employee          (Person)
	// 4. Project           (Projekt)
	// 5. Customer          (Kund)
	// 6. PayrollGroup      (Löneavtal)
	// 7. EmployeeGroup     (Tidavtal)
	// 8. Company           (Företag)
	// 9. NoAccounting      (Ingen kontering)
}

export enum TermGroup_InvoiceProductAccountingPrio {
	NotUsed = 0,
	NoAccounting = 1,
	Project = 2,
	Customer = 3,
	EmploymentAccount = 4,
	InvoiceProduct = 5,
	EmployeeAccount = 6, //Tillhörighet
}

export enum TermGroup_MergeInvoiceProductRows {
	Never = 1,
	Ask = 2,
	Always = 3,
}

export enum TermGroup_TimeDeviationCauseType {
	Undefined = 0,
	Absence = 1,
	Presence = 2,
	PresenceAndAbsence = 3,
	
	// Duplicate exists in the GoTimeStamp project!
}

export enum TermGroup_TimeCodeRoundingType {
	None = 0,
	RoundUp = 1,
	RoundDown = 2,
	RoundUpWholeDay = 3,
	RoundDownWholeDay = 4,
}

export enum TermGroup_ScheduledJobLogLevel {
	None = 0,
	Information = 1,
	Success = 2,
	Warning = 3,
	Error = 4
}

export enum TermGroup_AccumulatorTimePeriodType {
	Unknown = 0,
	Day = 1,
	Week = 2,
	Month = 3,
	Period = 4,
	AccToday = 5,
	Year = 6,
	PlanningPeriod = 7,
	
	//Only used in code, not in termgroup
	PlanningPeriodRunning = 101,
	Range = 102,
}

export enum TermGroup_InitProductSearch {
	Automatic = 1,
	WithEnter = 2
}

export enum TermGroup_EDIOrderStatus {
	Unprocessed = 1,
	UnderProcessing = 2,
	Processed = 3,
	Error = 4,
}

export enum TermGroup_EDIInvoiceStatus {
	Unprocessed = 1,
	UnderProcessing = 2,
	Processed = 3,
	Error = 4,
}

export enum TermGroup_EDISourceType {
	Unset = 0,
	EDI = 1,
	Scanning = 2,
	Finvoice = 3,
	InExchange = 4
}

export enum TermGroup_EDIStatus {
	Unprocessed = 1,
	UnderProcessing = 2,
	Processed = 3,
	Error = 4,
	Duplicate = 5,
}

export enum TermGroup_StandardDayOfWeek {
	Sunday = 0,
	Monday = 1,
	Tuesday = 2,
	WednesDay = 3,
	Thursday = 4,
	Friday = 5,
	Saturday = 6,
}

export enum TermGroup_TimePeriodDeviationViewMode {
	Attest = 1,
	Schedule = 2,
	Restore = 3,
	EditDeviation = 4,
	AdditionAndDeduction = 5,
}

export enum TermGroup_CustomerInvoiceOriginateFrom {
	Nothing = 0,
	Offer = 1,
	Order = 2,
	Invoice = 3,
	Contract = 4,
}

export enum TermGroup_ContractGroupPeriod {
	Week = 1,
	Month = 2,
	Quarter = 3,
	Year = 4,
	CalendarYear = 5,
}

export enum TermGroup_ContractGroupPriceManagement {
	Fixed = 1,
	UpdateFromPriceList = 2,
}

export enum TermGroup_CheckSettingsArea {
	SupplierInvoice = 1,        // Leverantörsreskontra
	CustomerLedger = 2,         // Kundreskontra
	Voucher = 3,                // Verifikatregistrering
	Offer = 4,                  // Offert
	Order = 5,                  // Order
	CustomerInvoice = 6,        // Kundfaktura
	Contract = 7,               // Avtal
	HouseholdTaxDeduction = 8,  // ROT-avdrag
	ProjectInvoice = 9,         // Tid och projekt
	Time = 10,
	Edi = 11,
}

export enum TermGroup_CheckSettingsResultType {
	NotChecked = 0,
	Passed = 1,
	Warning = 2,
	Error = 3
}

export enum TermGroup_AttestRuleRowLeftValueType {
	None = 0,                       // Ingen vald
	PresenceTime = 1,               // Närvarotid
	ScheduledTime = 2,              // Schematid
	WorkedInsideScheduledTime = 3,  // Närvaro inom schema
	ScheduledBreakTime = 4,         // Schemalagd rast
	TotalBreakTime = 5,             // Total rast
	
	// The following items use LeftValueId for its corresponding record id
	TimeCode = 21,                  // Tidkod
	PayrollProduct = 22,            // Löneart
	InvoiceProduct = 23,            // Artikel
}

export enum TermGroup_AttestRuleRowRightValueType {
	None = 0,                       // Ingen vald
	PresenceTime = 1,               // Närvarotid
	ScheduledTime = 2,              // Schematid
	WorkedInsideScheduledTime = 3,  // Närvaro inom schema
	ScheduledBreakTime = 4,         // Schemalagd rast
	TotalBreakTime = 5,             // Total rast
	
	// The following items use LeftValueId for its corresponding record id
	TimeCode = 21,                  // Tidkod
	PayrollProduct = 22,            // Löneart
	InvoiceProduct = 23,            // Artikel
}

export enum TermGroup_AccountDistributionTriggerType {
	None = 0,
	Registration = 1,
	Distribution = 2,
	//      YearBalance = 3
}

export enum TermGroup_AccountDistributionCalculationType {
	Percent = 1,
	Amount = 2,
	TotalAmount = 3 // Only used for SoeAccountDistributionType = Period
}

export enum TermGroup_AccountDistributionRegistrationType {
	None = 0,
	SupplierInvoice = 1,
	CustomerInvoice = 2,
	Voucher = 3
}

export enum TermGroup_ScanningStatus {
	Unprocessed = 1,
	Interpreted = 2,
	Processed = 3,
	Error = 4,
}

export enum TermGroup_ScanningMessageType {
	Unknown = 0,
	Arrival = 1,
	SupplierInvoice = 2,
}

export enum TermGroup_AccountDistributionPeriodType {
	Unknown = 0,
	Period = 1,
	Year = 2,
	Amount = 3
}

export enum TermGroup_ScanningInterpretation {
	ValueIsValid = 0,
	ValueIsUnsettled = 1,
	ValueNotFound = 2,
	ValueIsNotInterpreted = 3,
	ValueIsInterpretationDerived = 4,
	ValueIsBusinessRuleDerived = 5,
}

export enum TermGroup_InventoryWriteOffMethodType {
	Immediate = 1,
	AccordingToTheBooks_MainRule = 2,
	AccordingToTheBooks_ComplementaryRule = 3,
	BookValueWriteOff = 4,
}

export enum TermGroup_InventoryWriteOffMethodPeriodType {
	Period = 1,
	Year = 2
}

export enum TermGroup_InventoryStatus {
	None = 0,
	Draft = 1,      // Utkast / Preliminär
	Active = 2,     // Aktiv
	Discarded = 3,  // Utrangerad
	Sold = 4,       // Såld
	Inactive = 5,    // Inaktiv
	WrittenOff = 6, // Avskriven
}

export enum TermGroup_InventoryLogType {
	Purchase = 1,       // Inköp
	WriteOff = 2,       // Avskrivning
	OverWriteOff = 3,   // Överavskrivning
	UnderWriteOff = 4,  // Underavskrivning
	WriteUp = 5,        // Uppskrivning
	WriteDown = 6,      // Nerskrivning
	Discarded = 7,      // Utrangering
	Sold = 8,           // Försäljning
	Prognose = 9,       // Prognos
	Reversed = 10,      // Backad
	Reversal = 11,      // Backning
}

export enum TermGroup_TimeRuleType {
	Presence = 1,
	Absence = 2,
	Constant = 3,
}

export enum TermGroup_TimeRuleDirection {
	Forward = 1,
	Backward = 2,
}

export enum TermGroup_TimeAbsenceRuleType {
	None = 0,
	SickDuringInconvenientWorkingHours_PAID = 1,   // Sjuk OB Semgr
	SickDuringInconvenientWorkingHours_UNPAID = 2, // Sjuk OB Ej Semgr
	Sick_PAID = 3,                                 // Sjuk Semgr
	Sick_UNPAID = 4,                               // Sjuk Ej Semgr
	WorkInjury_PAID = 5,                           // Arbetsskada Semgr
	TemporaryParentalLeave_PAID = 6,               // Tillfällig föräldrarledig Semgr
	TemporaryParentalLeave_UNPAID = 7,             // Tillfällig föräldrarledig Ej Semgr
	PregnancyMoney_PAID = 8,                       // Graviditetspenning Semgr
	ParentalLeave_PAID = 9,                        // Föräldrarledig Semgr
	ParentalLeave_UNPAID = 10,                     // Föräldrarledig Ej Semgr
	DiseaseCarrier_PAID = 11,                      // Smittbärare Semgr
	DiseaseCarrier_UNPAID = 12,                    // Smittbärare Ej Semgr
	UnionEducation_PAID = 13,                      // Utbildning Semgr
	UnionEducation_UNPAID = 14,                    // Utbildning Ej Semgr
	MilitaryService_PAID = 15,                     // Totalförsvarsplikt Semgr
	MilitaryService_UNPAID = 16,                   // Totalförsvarsplikt Ej Semgr
	SwedishForImmigrants_PAID = 17,                // Svenska för invandrare Semgr
	PayedAbsence_PAID = 18,                        // Betald frånvaro Semgr
	LeaveOfAbsence_UNPAID = 19,                    // Ledighet utan lön (Tjänstledighet) Ej Semgr
	RelativeCare_PAID = 20,                        // Närståendevård Semgr
	RelativeCare_UNPAID = 21,                      // Närståendevård Ej Semgr
	Vacation = 22,
	SickDuringStandby_PAID = 23,                   // Sjuk Beredskap Semgr
	SickDuringStandby_UNPAID = 24,                 // Sjuk Beredskap Ej Semgr
	MilitaryService_Total_PAID = 25,               // Totalförsvarsplikt Total Semgr
	MilitaryService_Total_UNPAID = 26,             // Totalförsvarsplikt Total Ej Semgr
}

export enum TermGroup_TimeAbsenceRuleRowType {
	Unknown = 0,
	WholeDay_QualifyingDay = 1,                     // Karens hel dag
	CalendarDay = 2,                                // Kalenderdag
	PartOfDay = 3,                                  // Del av dag
	PartOfDay_1Year = 4,                            // Del av dag 1 år
	CalendarDay_105 = 5,                            // Kalenderdag 105
	//NotUsed = 6,                                    //
	PartOfDay_QualifyingDay = 7,                    // Karens del av dag
	WholeDay_QualifyingDayHighRiskProtection = 8,   // Karens hel dag Högriskskydd
	PartOfDay_QualifyingDayHighRiskProtection = 9,  // Karens del av dag Högriskskydd
}

export enum TermGroup_TimeStampEntryType {
	Unknown = 0,
	In = 1,
	Out = 2
}

export enum TermGroup_TimeBlockDateStampingStatus {
	NoStamps = 0,
	Complete = 1,
	FirstStampIsNotIn = 2,
	OddNumberOfStamps = 3,
	InvalidSequenceOfStamps = 4,
	StampsWithInvalidType = 5,
	AttestedDay = 6,
	
	//Completed with errors
	COMPLETED_WITH_ERRORS = 10, //Only used in querys
	InvalidDoubleStamp = 11,
}

export enum TermGroup_TimeScheduleSyncBatchType {
	Automatic = 1,  // Executed by scheduled job
	Manual = 2,     // Executed manually in GUI
	Import = 3,     // Import from webservice
}

export enum TermGroup_TimeScheduleSyncBatchStatus {
	Initialized = 0,
	FetchingSchedules = 1,
	SchedulesFetchedSuccessfully = 2,
	FailedToFetchSchedules = 3,
	StoringRawSchedules = 4,
	RawSchedulesStoredSuccessfully = 5,
	FailedToStoreRawSchedules = 6,
	StoringRawLeaveApplications = 7,
	RawLeaveApplicationsStoredSuccessfully = 8,
	FailedToStoreRawLeaveApplications = 9,
	CreatingSchedules = 10,
	SchedulesCreatedSuccessfully = 11,
	FailedToCreateSchedules = 12,
	CreatingAbsence = 13,
	AbsenceCreatedSuccessfully = 14,
	FailedToCreateAbsence = 15
}

export enum TermGroup_TimeScheduleSyncEntryStatus {
	Stored = 0,
	CreatingSchedule = 1,
	ScheduleCreatedSuccessfully = 2,
	FailedToCreateSchedule = 3
}

export enum TermGroup_EdiMessageType {
	Unknown = 0,
	SupplierInvoice = 1,
	OrderAcknowledgement = 2,
	DeliveryNotification = 3,
}

export enum TermGroup_SysMediaType {
	Unspecified = 0,    // Used in queries
	VideoHelp = 1
}

export enum TermGroup_MediaType {
	Unspecified = 0,    // Used in queries
	Video = 1,
	Image = 2,
	Audio = 3
}

export enum TermGroup_MediaFormat {
	Unspecified = 0,    // Used in queries
	VideoWMV = 11,
	VideoMP4 = 12,
	ImagePNG = 21,
	ImageJPG = 22,
	AudioWMA = 31,
	AudioMP3 = 32
}

export enum TermGroup_TimeScheduleSyncLeaveApplicationStatus {
	Stored = 0,
	CreatingAbsence = 1,
	AbsenceCreatedSuccessfully = 2,
	FailedToCreateAbsence = 3
}

export enum TermGroup_CloseEdiEntryCondition {
	HandledManually = 0,
	WhenTransferedToOrder = 1,
	WhenTransferedToSupplierInvoice = 2,
	WhenTransferedToOrderAndSupplierInvoice = 3,
	WhenTransferedToOrderOrSupplierInvoice = 4,
}

export enum TermGroup_TimeScheduleSyncEntryType {
	Unknown = 0,
	Presence = 1,
	Absence = 2,
	Deleted = 3
}

export enum TermGroup_TimeCodeRegistrationType {
	Unknown = 0,
	Time = 1,
	Quantity = 2,
}

export enum TermGroup_TimeScheduleTemplateBlockShiftStatus {
	Open = 0,       // Ledigt pass
	Assigned = 1,   // Tilldelat pass
}

export enum TermGroup_TimeScheduleTemplateBlockShiftUserStatus {
	None = 0,               // T.ex. ledigt pass
	Accepted = 1,           // Accepterat pass
	Unwanted = 2,           // Erbjud pass
	AbsenceRequested = 3,   // Kan ej ta pass, ex sjuk eller semester är ansökt
	AbsenceApproved = 4,    // Frånvaro beviljad
}

export enum TermGroup_TimeScheduleTemplateBlockQueueType {
	Unspecified = 0,    // Used in queries
	Wanted = 1,         // Önskat pass
	Suggestion = 2,     // Föreslaget pass
}

export enum TermGroup_Sex {
	Unknown = 0,
	Male = 1,
	Female = 2
}

export enum TermGroup_EmployeeRequestType {
	Undefined = 0,
	AbsenceRequest = 1,
	InterestRequest = 2,
	NonInterestRequest = 3
}

export enum TermGroup_EmployeeRequestTypeFlags {
	Undefined = 0,
	AbsenceRequest = 1,
	InterestRequest = 2,
	NonInterestRequest = 3,
	
	//The following is not used for storage but can be used with combination to the other
	PartyDefined = 0x8, // 8
}

export enum TermGroup_EmployeeRequestStatus {
	None = 0,
	RequestPending = 1,
	Preliminary = 2,
	Definate = 3,
	PartlyDefinate = 4,
	Restored = 5,
}

export enum TermGroup_MessageType {
	None = 0,
	UserInitiated = 1,
	ShiftRequest = 2,
	AttestInvoice = 3,
	AttestReminder = 4,
	ShiftRequestAnswer = 5,
	CaseProjectNew = 6,
	AutomaticInformation = 7,
	NeedsConfirmation = 8,
	NeedsConfirmationAnswer = 9,
	AbsenceRequest = 10,
	Delegate = 11,
	DocumentSigningRequest = 12,        // Request for users to sign
	DocumentSigningConfirmation = 13,   // Confirmation to signee itself when answered
	DocumentSigningRejection = 14,      // Information to other signees when one has rejected
	DocumentSigningCancellation = 15,   // Information to other signees when flow is cancelled
	DocumentSigningFullySigned = 16,    // Information to other signees when document is fully signed
	SwapRequest = 17,
	PayrollSlip = 18,
	TimeWorkAccountYearEmployeeOption = 19,
	OrderAssigned = 20,
	AbsenceAnnouncement = 21,
	AbsencePlanning = 22,
	ScheduledChanged = 23
}

export enum TermGroup_MessagePriority {
	None = 0,
	Low = 1,
	Normal = 2,
	High = 3
}

export enum TermGroup_MessageDeliveryType {
	XEmail = 1,
	Email = 2,
	SMS = 3
}

export enum TermGroup_MessageTextType {
	XAML = 1,
	HTML = 2,
	RTF = 3,
	Text = 4,
	SMS = 5
}

export enum TermGroup_TimeSchedulePlanningViews {
	Calendar = 0,
	Day = 1,
	Schedule = 2,
	TemplateDay = 3,
	TemplateSchedule = 4,
	EmployeePostsDay = 5,
	EmployeePostsSchedule = 6,
	ScenarioDay = 7,
	ScenarioSchedule = 8,
	StandbyDay = 9,
	StandbySchedule = 10,
	TasksAndDeliveriesDay = 11,
	TasksAndDeliveriesSchedule = 12,
	StaffingNeedsDay = 13,
	StaffingNeedsSchedule = 14,
	ScenarioComplete = 15
}

export enum TermGroup_TimeSchedulePlanningVisibleDays {
	Custom = 0,
	Day = 1,
	WorkWeek = 5,
	Week = 7,
	TwoWeeks = 14,
	ThreeWeeks = 21,
	FourWeeks = 28,
	Month = 31,
	FiveWeeks = 35,
	SixWeeks = 42,
	SevenWeeks = 49,
	EightWeeks = 56,
	NineWeeks = 63,
	TenWeeks = 70,
	ElevenWeeks = 77,
	TwelveWeeks = 84,
	ThirteenWeeks = 91,
	FourteenWeeks = 98,
	FifteenWeeks = 105,
	EighteenWeeks = 126,
	TwentySixWeeks = 182,
	Year = 365
}

export enum TermGroup_InvoiceProductCalculationType {
	Regular = 0,
	SupplementCharge = 1,
	FixedPrice = 2,
	Lift = 3,
	Contract = 4,
	Clearing = 5,
}

export enum TermGroup_TimeAccumulatorType {
	Rolling = 1,
	PerYear = 2,
};

export enum TermGroup_YesNo {
	Unknown = 0,
	Yes = 1,
	No = 2,
};

export enum TermGroup_EDIPriceSettingRule {
	UsePriceRules = 0,
	//UseEDIPurchasePriceWithSupplementCharge = 1,, not used any more...
	//UseHighestPrice = 2, not used any more...
	//UseLowestPrice = 3, not used any more...
	UsePriceRulesKeepEDIPurchasePrice = 4,
	UsePriceRulesAndPurchasePriceFromPriceList = 5,
}

export enum TermGroup_ChecklistHeadType {
	Unknown = 0,
	Order = 1,
}

export enum TermGroup_ChecklistRowType {
	Unknown = 0,
	String = 1,
	YesNo = 2,
	Checkbox = 3,
	MultipleChoice = 4,
	Image = 5
}

export enum TermGroup_CurrencySource {
	None = 0,
	Manually = 1,
	Daily = 2,
	Tullverket = 3,
	ECB = 4,
}

export enum TermGroup_CurrencyIntervalType {
	None = 0,
	Manually = 1,
	FirstDayOfQuarter = 2,
	FirstDayOfMonth = 3,
	EveryMonday = 4,
	EveryDay = 5,
}

export enum TermGroup_CurrencyType {
	BaseCurrency = 1,
	TransactionCurrency = 2,
	EnterpriseCurrency = 3,
	LedgerCurrency = 4
}

export enum TermGroup_MobileFields {
	//OrderEdit
	OrderEdit_VatType = 101,
	OrderEdit_SalesPriceList = 102,
	OrderEdit_HHDeduction = 103,
	OrderEdit_Label = 104,
	OrderEdit_HeadText = 105,
	OrderEdit_InternalText = 106,
	OrderEdit_Currency = 107,
	OrderEdit_DeliveryAddress = 108,
	OrderEdit_InvoiceAddress = 109,
	OrderEdit_OurReference = 110,
	OrderEdit_Amount = 111,
	OrderEdit_WholeSeller = 112,
	OrderEdit_YourReference = 113,
	OrderEdit_DDate = 114,
	
	//OrderGrid
	OrderGrid_VatType = 201,
	OrderGrid_SalesPriceList = 202,
	OrderGrid_HHDeduction = 203,
	OrderGrid_Label = 204,
	OrderGrid_HeadText = 205,
	OrderGrid_InternalText = 206,
	OrderGrid_Currency = 207,
	OrderGrid_DeliveryAddress = 208,
	OrderGrid_InvoiceAddress = 209,
	OrderGrid_Reference = 210,
	OrderGrid_Amount = 211,
	OrderGrid_ReAmount = 212,
	OrderGrid_Owners = 213,
	OrderGrid_ODate = 214,
	OrderGrid_ORowStatus = 215,
	OrderGrid_DDate = 216,
	OrderGrid_WorkDescr = 217,
	
	//CustomerEdit
	CustomerEdit_OrganisationNr = 301,
	CustomerEdit_VatNr = 302,
	CustomerEdit_Reference = 303,
	CustomerEdit_EmailAddress = 304,
	CustomerEdit_PhoneHome = 305,
	CustomerEdit_PhoneJob = 306,
	CustomerEdit_PhoneMobile = 307,
	CustomerEdit_Fax = 308,
	CustomerEdit_InvoiceAddress = 309,
	CustomerEdit_IAPostalCode = 310,
	CustomerEdit_IAPostalAddress = 311,
	CustomerEdit_DeliveryAddress1 = 312,
	CustomerEdit_DA1PostalCode = 313,
	CustomerEdit_DA1PostalAddress = 314,
	CustomerEdit_VatType = 315,
	CustomerEdit_PaymentCondition = 316,
	CustomerEdit_SalesPriceList = 317,
	CustomerEdit_StandardWholeSeller = 318,
	CustomerEdit_DiscountArticles = 319,
	CustomerEdit_DiscountServices = 320,
	CustomerEdit_Currency = 321,
	CustomerEdit_Note = 322,
	CustomerEdit_InvoiceDeliveryType = 323,
	CustomerEdit_IACountry = 324,
	CustomerEdit_IAAddressCO = 325,
	CustomerEdit_DA1Country = 326,
	CustomerEdit_DA1AddressCO = 327,
	CustomerEdit_DA1Name = 328,
	
	
	//CustomerGrid
	CustomerGrid_OrganisationNr = 401,
	CustomerGrid_VatNr = 402,
	CustomerGrid_Reference = 403,
	CustomerGrid_EmailAddress = 404,
	CustomerGrid_PhoneHome = 405,
	CustomerGrid_PhoneJob = 406,
	CustomerGrid_PhoneMobile = 407,
	CustomerGrid_Fax = 408,
	CustomerGrid_InvoiceAddress = 409,
	CustomerGrid_DeliveryAddress1 = 410,
	CustomerGrid_VatType = 411,
	CustomerGrid_PaymentCondition = 412,
	CustomerGrid_SalesPriceList = 413,
	CustomerGrid_StandardWholeSeller = 414,
	CustomerGrid_DiscountArticles = 415,
	CustomerGrid_DiscountServices = 416,
	CustomerGrid_Currency = 417,
	CustomerGrid_Note = 418,
	
	//OrderHead
	OrderHead_ProjectNr = 501,
	OrderHead_VatType = 502,
	OrderHead_InvoiceLabel = 503,
	OrderHead_DeliveryAddress = 504,
	OrderHead_YourReference = 505,
	OrderHead_OrderNr = 506,
	OrderHead_Customer = 507,
	OrderHead_TotalAmount = 508,
	OrderHead_OrderDescription = 509,
	OrderHead_BillingAddress = 510,
	
	// Order order row
	OrderOrderRow_SalesPrice = 601,
	OrderOrderRow_TotalPrice = 602,
}

export enum TermGroup_MobileForms {
	OrderEdit = 1,
	OrderGrid = 2,
	CustomerEdit = 3,
	CustomerGrid = 4,
	OrderHead = 5,
	OrderOrderRow = 6,
}

export enum TermGroup_IOStatus {
	Unprocessed = 1,
	UnderProcessing = 2,
	Processed = 3,
	Error = 4,
}

export enum TermGroup_IOType {
	Unknown = 0,
	Excel = 1,
	WebService = 2,
	XEConnect = 3,
	WebAPI = 4,
	Inexchange = 5,
	Bridge = 6,
}

export enum TermGroup_IOSource {
	Unknown = 0,
	XE = 1,
	TilTid = 2,
	FlexForce = 3,
	Connect = 4,
	Bridge = 5,
}

export enum TermGroup_SysExportDefinitionType {
	XML = 0,
	Separator = 1,
	Fixed = 2
}

export enum TermGroup_SysImportDefinitionType {
	XML = 0,
	Separator = 1,
	Fixed = 2
}

export enum TermGroup_AttestFlowRowState {
	Unhandled = 0,  // No answer yet
	Handled = 1,    // Either answered or state set by someone elses answer because type is 'Any'
	Deleted = 2,    // Deleted but still shown in the GUI as deleted
}

export enum TermGroup_AttestWorkFlowType {
	All = 0,    // Every user needs to answer
	Any = 1     // Only one user needs to answer
}

export enum TermGroup_InvoiceClaimLevel {
	None = 0,
	Claim1 = 1,
	Claim2 = 2,
	Claim3 = 3,
	Claim4 = 4,
	Collection = 5, //Inkasso
};

export enum TermGroup_ProjectStatus {
	Unknown = 0,
	Planned = 1,
	Active = 2,
	Locked = 3,
	Finished = 4,
	Hidden = 5,
	Guarantee = 6,
}

export enum TermGroup_ProjectUserType {
	Unknown = 0,
	Manager = 1,
	Participant = 2,
	Buyer = 3,
}

export enum TermGroup_ShiftHistoryType {
	Unknown = 0,
	
	//DragShiftAction
	DragShiftActionMove = 1,                //Change
	DragShiftActionCopy = 2,                //New
	DragShiftActionReplace = 3,             //Delete and New
	DragShiftActionReplaceAndFree = 4,      //Change - inc hidden
	DragShiftActionSwapEmployee = 5,        //Change
	DragShiftActionAbsence = 6,
	DragShiftActionDelete = 7,
	DragShiftActionMoveWithCycle = 8,
	DragShiftActionCopyWithCycle = 9,
	
	//HandleShiftAction
	HandleShiftActionWanted = 11,           //- (only status change)
	HandleShiftActionChangeEmployee = 12,   //Change (1 row)
	HandleShiftActionSwapEmployee = 13,     //Change (4 rows)
	HandleShiftActionAbsenceAnnouncement = 14,
	
	//Misc
	AbsenceRequestPlanning = 31,            //Absence and maybe New (2 rows)
	AbsencePlanning = 32,                   //Absence and maybe New (2 rows)
	TaskSaveTimeScheduleShift = 33,         //Change (1+ row)
	TaskDeleteTimeScheduleShift = 34,
	TaskSplitTimeScheduleShift = 35,        //Change and New (2 rows)
	AbsenceRequestPlanningRestored = 36,
	ShiftRequest = 37,
	AssignTaskToEmployee = 38,
	AssignEmployeeFromQueue = 39,
	DropEmployeeOnShift = 40,
	ScheduleSwapRequest = 91,
	
	//Orderplanning
	TaskSaveOrderShift = 41,
	TaskSaveBooking = 42,
	
	//Template schedule
	TemplateScheduleSave = 51,
	TemplateScheduleActivate = 52,
	TemplateScheduleSaveAndActive = 53,
	
	EditBreaks = 61,
	
	//Scenario
	ActivateScenario = 71,
	CreateScenario = 72,
	
	//ImportPayroll
	ImportPayroll = 81,
	RevertedImportPayroll = 82,
}

export enum TermGroup_ProjectType {
	Unknown = 0,
	TimeProject = 1,
	CaseProject = 2,
}

export enum TermGroup_AttestWorkFlowRowProcessType {
	Unknown = 0,
	Registered = 1,
	WaitingForProcess = 2,
	Processed = 3,
	LevelNotReached = 4,
	TransferredToOtherUser = 5,
	TransferredWithReturn = 6,
	Returned = 7
}

export enum TermGroup_SysNewsDisplayType {
	News = 1,
	Popup = 2,
	NewsAndPopup = 3,
}

export enum TermGroup_ProjectAllocationType {
	Unknown = 0,
	External = 1,
	Internal = 2,
	InternalWithOccupancy = 3
}

export enum SysImportDefinitionUpdateType {
	None = 0,
	Always = 1,
	OnlyNew = 2,
	IfBlankOrZero = 3,
}

export enum TermGroup_ProductSearchFilterMode {
	StartsWidth = 0,
	Contains = 1,
	Equals = 2
}

export enum TermGroup_IOImportHeadType {
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
	//PayrollStartValueRow = 36, //Not used anymore
	EdiMessage = 37,
	TimeMigrationExcel = 38,
	TimeBalance = 39,
	TimeCodeTransactionSimple = 40
}

export enum IODictionaryType {
	Unkown = 0,
	AccountNr = 1,
	AccountInternalDim2Nr = 2,
	AccountInternalDim3Nr = 3,
	AccountInternalDim4Nr = 4,
	AccountInternalDim5Nr = 5,
	AccountInternalDim6Nr = 6,
	InvoiceProduct = 7,
	PayrollProduct = 8,
	EmployeeGroup = 9,
	PayrollGroup = 10,
	VacationGroup = 11,
}

export enum IOImportSideDictionaryType {
	Unkown = 0,
	VatCodes = 1,
	PaymentConditions = 2,
	DeliveryConditions = 3,
	Currency = 4,
	DeliveryTypes = 5,
	PricelistTypes = 6,
	Categories = 7,
	ProductGroup = 8,
	CategoryGroups = 9,
	Stocks = 10,
	PayrollProduct = 11,
	TimeCode = 12,
	PaymentMethod = 13,
	Endreason = 14,
	ProductUnit = 15
}

export enum TermGroup_TimeStampAttendanceGaugeShowMode {
	AllLast24Hours = 0,
	AllToday = 1
}

export enum TermGroup_StaffingNeedsAnalysisRefType {
	// These are minus because StaffingNeedsRules are added to the list
	Unknown = 0,
	Schedule = -1,
	Presence = -2
}

export enum TermGroup_StaffingNeedsAnalysisDateComparer {
	None = 0,
	SameDayPreviousWeek = 1,
	SameDayPreviousMonth = 2,
	SameDayPreviousYear = 3,
}

export enum TermGroup_CompanyEdiType {
	Unknown = 0,
	Symbrio = 1,
	Nelfo = 2,
	LvisNet = 3,
}

export enum TermGroup_StaffingNeedsAnalysisChartType {
	NeedAsLine = 1,
	NeedAsBar = 2,
}

export enum TermGroup_CaseProjectResult {
	None = 0,
	
	Closed_Dismissed = 1,
	Closed_Duplicate = 2,
	Closed_FutureDevelopment = 3,
	
	Resolved_Environment = 11,
	Resolved_Database = 12,
	Resolved_Report = 13,
	Resolved_Development = 14,
}

export enum TermGroup_CaseProjectPriority {
	Unknown = 0,
	Urgent = 1,
	High = 2,
	Medium = 3,
	Low = 4
}

export enum TermGroup_CaseProjectType {
	Unknown = 0,
	Bug = 1,
	Question = 2,
	Feature = 3
}

export enum TermGroup_StaffingNeedsRuleUnit {
	Unknown = 0,
	ItemsPerMinute = 1,
	CustomersPerMinute = 2,
	AmountPerMinute = 3
}

export enum TermGroup_CaseProjectChannel {
	Unknown = 0,
	Internal = 1,
	XE = 2,
	Telephone = 3,
	Email = 4,
	Web = 5,
	UserConference = 6,
	Other = 99
}

export enum TermGroup_CaseProjectArea {
	Unknown = 0,
	Economy = 1,
	Invoice = 2,
	Project = 3,
	Time = 21,
	Staffing = 22,
	XEmail = 41,
	Mobile = 51,
	Terminal = 61,
	Other = 99
}

export enum TermGroup_CaseProjectApplication {
	Unknown = 0,
	XE = 1,
	Professional = 2,
	Business = 3,
	Salary = 4
}

export enum TermGroup_EmployeeRequestResultStatus {
	None = 0,
	PartlyGranted = 1,
	FullyGranted = 2,
	PartlyDenied = 3,
	FullyDenied = 4,
	PartlyGrantedPartlyDenied = 5,
}

export enum TermGroup_EmploymentType {
	Unknown = 0,
	
	// Sweden 1-100
	SE_Probationary = 1,                // Provanställning
	SE_Substitute = 2,                  // Vikariat
	SE_SubstituteVacation = 3,          // Semestervikarie
	SE_Permanent = 4,                   // Tillsvidareanställning
	SE_FixedTerm = 5,                   // Allmän visstidsanställning
	SE_Seasonal = 6,                    // Säsongsarbete
	SE_SpecificWork = 7,                // Visst arbete
	SE_Trainee = 8,                     // Praktikantanställning
	SE_NormalRetirementAge = 9,         // Tjänsteman som uppnått den ordinarie pensionsåldern enligt ITP-planen
	SE_CallContract = 10,               // Behovsanställning
	SE_LimitedAfterRetirementAge = 11,  // Tidsbegränsad anställning för personer fyllda 67 år (enligt lag)
	SE_FixedTerm14days = 12,            // Allmän visstidsanställning 14 dagar
	SE_Apprentice = 13,                 // Lärling
	SE_SpecialFixedTerm = 14,           // Särskild visstidsanställning
	
	// Finland 101-200
	
	// Norway 201-300
}

export enum TermGroup_SysPayrollPrice {
	Unknown = 0,
	
	
	// Prisbasbelopp
	SE_BaseAmount = 1,                              // Prisbasbelopp
	SE_IncreasedBaseAmount = 2,                     // Förhöjt prisbasbelopp
	SE_IncomeBaseAmount = 3,                        // Inkomstbasbelopp
	
	// Arbetsgivaravgifter, egenavgifter och särskild löneskatt
	SE_EmploymentTax = 13,                          // Arbetsgivaravgift
	SE_PayrollTax = 20,                             // Egenavgift
	SE_SpecialPayrollTax = 25,                      // Särskild löneskatt
	SE_DefaultOneTimeTax = 26,                      // Standard procentsats för engångskatt
	SE_SchoolYouthLimit = 27,                       // Gränsvärde skolungdom
	SE_SINKTax = 28,                                // SINK-skatt
	
	// Statslåneränta
	SE_GovernmentLoanInterest = 31,                 // Statslåneränta (sista november)
	
	// Egen bil i tjänsten
	SE_PrivateCar = 41,                             // Egen bil, skattefri ersättning (mil)
	
	// Förmånsbil i tjänsten
	SE_CompanyCarGasoline = 51,                     // Förmånsbil bensin, etanol m.m. skattefri ersättning (mil)
	SE_CompanyCarDiesel = 52,                       // Förmånsbil diesel, skattefri ersättning (mil)
	
	// Skattefria traktamenten
	SE_AllowanceHalfDay = 61,                       // Traktamente halv dag
	SE_AllowanceWholeDay = 62,                      // Traktamente hel dag
	SE_AllowanceAfterThreeMonths = 63,              // Traktamente efter tre månader
	SE_AllowanceAfterTwoYears = 64,                 // Traktamente efter två år
	SE_AllowanceNight = 65,                         // Traktamente efter två år
	
	// Reducering av traktamente
	SE_FoodWholeDay_HalfDay = 71,                   // Mat hela dagen (halv dag)
	SE_FoodWholeDay_WholeDay = 72,                  // Mat hela dagen (hel dag)
	SE_FoodWholeDay_AfterThreeMonth = 73,           // Mat hela dagen (efter tre månader)
	SE_FoodWholeDay_AfterTwoYears = 74,             // Mat hela dagen (efter två år)
	
	SE_LunchAndDinner_HalfDay = 75,                 // Lunch och middag (halv dag)
	SE_LunchAndDinner_WholeDay = 76,                // Lunch och middag (hel dag)
	SE_LunchAndDinner_AfterThreeMonth = 77,         // Lunch och middag (efter tre månader)
	SE_LunchAndDinner_AfterTwoYears = 78,           // Lunch och middag (efter två år)
	
	SE_LunchOrDinner_HalfDay = 79,                  // Lunch eller middag (halv dag)
	SE_LunchOrDinner_WholeDay = 80,                 // Lunch eller middag (hel dag)
	SE_LunchOrDinner_AfterThreeMonth = 81,          // Lunch eller middag (efter tre månader)
	SE_LunchOrDinner_AfterTwoYears = 82,            // Lunch eller middag (efter två år)
	
	SE_Breakfast_HalfDay = 83,                      // Frukost (halv dag)
	SE_Breakfast_WholeDay = 84,                     // Frukost (hel dag)
	SE_Breakfast_AfterThreeMonth = 85,              // Frukost (efter tre månader)
	SE_Breakfast_AfterTwoYears = 86,                // Frukost (efter två år)
	
	// Kostförmån
	SE_Breakfast = 91,                              // Frukost
	SE_LunchDinner = 92,                            // Lunch/middag
	SE_FreeMeals = 93,                              // Helt fri kost
	
	// Frånvaro
	SE_Absence_Sick = 101,                          // Sjuk (180 dagar per intjänandeår)
	SE_Absence_WorkRelatedInjury = 102,             // Arbetsskada (365 dagar per intjänandeår)
	SE_Absence_PregnancyCompensation = 103,         // Graviditetspenning (365 dagar per intjänandeår)
	SE_Absence_RelativeCare = 104,                  // Närståendevård (45 dagar per intjänandeår)
	SE_Absence_Education = 105,                     // Utbildning (45 dagar per intjänandeår)
	SE_Absence_Contaminated = 106,                  // Smittbärare (180 dagar per intjänandeår)
	SE_Absence_SwedishForImmigrants = 107,          // Svenska för invandrare (365 dagar per intjänandeår)
	SE_Absence_MedicalCertificateDays = 108,        // Påminnelse sjukintyg efter dagar (8)
	SE_Absence_MaxSGI = 109,                        // Inkomsttak för SGI
	SE_Absence_MaxParentalCompensation = 110,       // Inkomsttak för föräldrapenning
	
	// Semester
	SE_Vacation_VacationDayPercent = 121,           // Procentsats semesterlön enligt semesterlagen
	SE_Vacation_VacationDayAdditionPercent = 122,   // Procentsats semestertillägg enligt semesterlagen
	SE_Vacation_HandelsGuaranteeAmount = 123,       // Garantibelopp enligt Handels
	
	
	
	
	// Note!
	// Values below are grouped in area
	// Pay attention to numbers when adding new!
	
	// Common
	SE_InputValue = 200,                                        // Ingående värde
	SE_CalenderDaysInPeriod = 203,                              // Kalenderdagar i period
	SE_WorkDaysInPeriod = 204,                                  // Arbetsdagar i period
	SE_EmploymentDaysInPeriod = 221,                            // Anställningsdagar i period
	SE_WorkTimeInHoursInPeriod = 224,                           // Arbetad tid i period (ATPERIOD)
	SE_SalaryPromotedAbsenceInPeriod = 232,                     // Semestergrundande frånvarotid i period (SEMGRFRVPERIOD)
	
	//Schedule
	SE_ScheduledHoursInPeriod = 228,                            // Schematimmar i period (SCHPER)
	SE_StandbyHoursInPeriod = 229,                              // Beredskapstimmar i period (BERSCHPER)
	SE_ScheduleTimeNoScheduleAccordingToScheduleType = 230,     // Schematyp ej schematid (STEJSCH)
	SE_ScheduledHoursInPeriodWithFactorFromScheduleType = 231,  // Schematimmar i period med faktor (STFAKT)
	SE_ScheduleWorkTimeWeekDays = 234,                          // Arbetsdagar per vecka (SCHADPV)
	SE_ScheduleWorkTimeInWeek = 235,                            // Arbetstimmar i veckan (SCHATPV)
	
	//Grundschema
	SE_TemplateScheduleGrossTimeHolidaySalary = 250,            // Grundschema bruttotid helglön (GRUNDBTHL)
	SE_TemplateScheduleGrossTime = 251,                         // Grundschema bruttotid (GRUNDBT)
	SE_TemplateScheduleGrossCostHolidaySalary = 252,            // Grundschema bruttokostnad (GRUNDBKHL)
	SE_TemplateScheduleGrossCost = 253,                         // Grundschema bruttokostnad (GRUNDBK)
	SE_TemplateScheduleWorkDayAverage = 254,                    // Grundschema arbetsdagar i snitt (GRUNDADS)
	
	// EmployeeGroup
	SE_EmployeeGroup_WorkTimeWeek = 202,                        // Arbetstimmar per vecka
	
	// PayrollGroup
	SE_PayrollGroup_DivisorHourlyWagesByMonthlyWages = 207,     // Divisor timlön vid månadslön
	
	// Employee
	SE_Employee_WorkTimeWeek = 201,                             // Veckoarbetstid
	SE_Employee_WorkPercentage = 208,                           // Sysselsättningsgrad
	SE_Employee_Age = 226,                                      // Ålder år
	SE_Employee_AgeMonths = 227,                                // Ålder månader
	
	// EmployeeFactor
	SE_EmployeeFactor_CalenderDayFactor = 205,                  // Kalenderdagsfaktor
	SE_EmployeeFactor_VacationCoefficient = 206,                // Semesterkoefficient
	SE_EmployeeFactor_VacationDayPercent = 216,                 // Semesterlön per dag
	SE_EmployeeFactor_VacationVariablePercent = 217,            // Semestertillägg rörligt
	SE_EmployeeFactor_Net = 222,                                // Netto
	SE_EmployeeFactor_TimeWorkAccount_PaidLeave = 236,          // Betald ledighet per timme (ATK)
	SE_EmployeeFactor_VacationDayPercentFinalSalary = 237,      // Semesterlön slutlön
	SE_EmployeeFactor_VacationHourPercent = 256,                // Semesterlön per timme
	
	// EmployeeVacation
	SE_EmployeeVacation_WorkPercentage_EarningYear = 209,       // Sysselsättningsgrad intjänandeår
	SE_EmployeeVacation_WorkPercentage_SavedYear1 = 210,        // Sysselsättningsgrad sparad år 1
	SE_EmployeeVacation_WorkPercentage_SavedYear2 = 211,        // Sysselsättningsgrad sparad år 2
	SE_EmployeeVacation_WorkPercentage_SavedYear3 = 212,        // Sysselsättningsgrad sparad år 3
	SE_EmployeeVacation_WorkPercentage_SavedYear4 = 213,        // Sysselsättningsgrad sparad år 4
	SE_EmployeeVacation_WorkPercentage_SavedYear5 = 214,        // Sysselsättningsgrad sparad år 5
	SE_EmployeeVacation_WorkPercentage_SavedOverdue = 215,      // Sysselsättningsgrad förfallna dagar
	SE_EmployeeFactor_VacationVariableAmountPerDay = 225,       // Rörligt semestertillägg per dag
	
	// Absence
	SE_EmployeeAbsence_AbsenceEntirePeriod = 218,               // Frånvaro hel period
	SE_EmployeeAbsence_AbsencePercentOfDay = 219,               // Frånvaroomfattning per dag
	SE_EmployeeAbsence_AbsencePercentSameEntirePeriod = 220,    // Frånvaroomfattning samma hela månaden
	SE_EmployeeAbsence_CalculateSicknessSalary = 223,           // Beräkningsunderlag sjuklön per vecka
	SE_EmployeeAbsence_AbsencePercentPreviousDay = 233,         // Frånvaroomfattning föregående dag (FOMFFGD)
	SE_EmployeeAbsence_AbsencePercentNextDay = 255,             // Frånvaroomfattning kommande dag (FOMFKD)
	
}

export enum TermGroup_TimeScheduleTemplateBlockType {
	Schedule = 0,
	Order = 1,
	Booking = 2,
	Standby = 3,
	OnDuty = 4,
	Employee = 101, // Only used in drag n drop
	Need = 102,     // Only used in drag n drop
}

export enum TermGroup_AttestTreeGrouping {
	None = 0,
	EmployeeAuthModel = 1, //Category or Account
	EmployeeGroup = 2,
	PayrollGroup = 3,
	All = 4,
	AttestState = 5,
}

export enum TermGroup_AttestTreeSorting {
	None = 0,
	EmployeeNr = 1,
	FirstName = 2,
	LastName = 3,
}

export enum TermGroup_EmployeeDisbursementMethod {
	Unknown = 0,
	
	// Sweden 1-100
	SE_CashDeposit = 1,                 // Kontant
	SE_PersonAccount = 2,               // Personkonto
	SE_AccountDeposit = 3,              // Kontoinsättning
	SE_NorweiganAccount = 4             // Norskt kontonummer
	//SE_IBAN = 5,                        // IBAN
	
	// Finland 101-200
	
	// Norway 201-300
	
}

export enum TermGroup_StaffingNeedsHeadInterval {
	FifteenMinutes = 15,
	ThirtyMinutes = 30,
	SixtyMinutes = 60
}

export enum TermGroup_StaffingNeedsHeadStatus {
	None = 0,
	Preliminary = 1,
	Definate = 2,
}

export enum TermGroup_EmploymentChangeFieldType {
	//Information 1-100
	DateFrom = 1,
	DateTo = 2,
	State = 3,
	Hibernating = 4,
	HibernatingClosed = 5,
	
	//Relation changes 101-200
	EmployeeGroupId = 101,
	PayrollGroupId = 102,
	PayrollPriceTypeId = 103,
	AnnualLeaveGroupId = 105,
	
	//Field changes 201-300
	EmploymentType = 201,
	Name = 202,
	WorkTimeWeek = 203,
	Percent = 204,
	ExperienceMonths = 205,
	ExperienceAgreedOrEstablished = 206,
	//VacationDaysPayed = 207,      //Deprecated since sprint 56
	//VacationDaysUnpayed = 208,    //Deprecated since sprint 56
	SpecialConditions = 209,
	WorkPlace = 210,
	//TaxRate = 211, //Deprecated since sprint 47
	BaseWorkTimeWeek = 212,
	SubstituteFor = 213,
	EmploymentEndReason = 214,
	PayrollPriceTypeAmount = 215,
	FixedAccounting = 216,
	SubstituteForDueTo = 217,
	WorkTasks = 218,
	ExternalCode = 219,
	IsSecondaryEmployment = 220,
	FullTimeWorkTimeWeek = 254,
	ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment = 255,
	
	//Relation name changes 301-400
	EmployeeGroupName = 301,
	PayrollGroupName = 302,
	PayrollPriceTypeName = 303,
}

export enum TermGroup_TimeSchedulePlanningShiftStyle {
	Detailed = 0,
	ActualTime = 1,
	ActualTimeCompressed = 2,
	DetailedCompressed = 3
}

export enum TermGroup_EmploymentChangeType {
	Unknown = 0,
	Information = 1,
	DataChange = 2,
}

export enum TermGroup_EmploymentEndReason {
	None = 0,
	
	//Sweden (-1) - (-100)
	SE_EmploymentChanged = -1,
	SE_CompanyChanged = -2,
	SE_Deceased = -3,
	SE_OwnRequest = -4,
	SE_Retirement = -5,
	SE_TemporaryEmploymentEnds = -6,
	SE_LaidOfDueToRedundancy = -7,
	SE_Fired = -8,
}

export enum TermGroup_SysPayrollPriceType {
	SystemPrice = 1,        // Values in SysPayrollPrice table
	RecordRelatedValue = 2, // Values in other tables
}

export enum TermGroup_SysPayrollType {
	None = 0,
	
	SE_GrossSalary = 1000000,                                                       // (1) Bruttolön
	SE_GrossSalary_Contract = 1010000,                                              //   (2) Ackord
	SE_GrossSalary_WorkingAccount = 1020000,                                        //   (2) Arbetstidskonto
	SE_GrossSalary_Standby = 1030000,                                               //   (2) Beredskap
	SE_GrossSalary_CarAllowanceFlat = 1040000,                                      //   (2) Bilersättning > schablon
	SE_GrossSalary_NonAFAPromoted = 1050000,                                        //   (2) Ej AFA grundande
	SE_GrossSalary_Absence = 1060000,                                               //   (2) Frånvaro
	SE_GrossSalary_Absence_WorkInjury = 1060100,                                    //     (3) Arbetsskada
	SE_GrossSalary_Absence_WorkInjury_QualifyingDay = 1060101,                      //       (4) Karensdag
	SE_GrossSalary_Absence_WorkInjury_NoVacation = 1060102,                         //       (4) Ej semestergrundande
	SE_GrossSalary_Absence_UnionWork = 1060200,                                     //     (3) Fackligt arbete (företaget betalar)
	SE_GrossSalary_Absence_UnionWork_NoVacation = 1060201,                          //       (4) Ej semestergrundande
	SE_GrossSalary_Absence_UnionEduction = 1060300,                                 //     (3) Facklig utbildning (facket betalar)
	SE_GrossSalary_Absence_UnionEduction_NoVacation = 1060301,                      //       (4) Ej semestergrundande
	SE_GrossSalary_Absence_ParentalLeave = 1060400,                                 //     (3) Föräldraledig
	SE_GrossSalary_Absence_ParentalLeave_NoVacation = 1060401,                      //       (4) Ej semestergrundande
	SE_GrossSalary_Absence_PregnancyCompensation = 1060500,                         //     (3) Graviditetspenning
	SE_GrossSalary_Absence_PregnancyCompensation_NoVacation = 1060501,              //       (4) Ej semestergrundande
	SE_GrossSalary_Absence_RelativeCare = 1060600,                                  //     (3) Närståendevård
	SE_GrossSalary_Absence_RelativeCare_NoVacation = 1060601,                       //       (4) Ej semestergrundande
	SE_GrossSalary_Absence_Permission = 1060700,                                    //     (3) Permission
	SE_GrossSalary_Absence_PayedAbsence = 1060800,                                  //     (3) Permittering
	SE_GrossSalary_Absence_Vacation = 1060900,                                      //     (3) Semester
	SE_GrossSalary_Absence_Vacation_Paid = 1060901,                                 //       (4) Betalda dagar
	SE_GrossSalary_Absence_Vacation_Unpaid = 1060902,                               //       (4) Obetalda dagar
	SE_GrossSalary_Absence_Vacation_Advance = 1060903,                              //       (4) Förskottsdagar
	SE_GrossSalary_Absence_Vacation_SavedYear1 = 1060904,                           //       (4) Sparade dagar år 1
	SE_GrossSalary_Absence_Vacation_SavedYear2 = 1060905,                           //       (4) Sparade dagar år 2
	SE_GrossSalary_Absence_Vacation_SavedYear3 = 1060906,                           //       (4) Sparade dagar år 3
	SE_GrossSalary_Absence_Vacation_SavedYear4 = 1060907,                           //       (4) Sparade dagar år 4
	SE_GrossSalary_Absence_Vacation_SavedYear5 = 1060908,                           //       (4) Sparade dagar år 5
	SE_GrossSalary_Absence_Vacation_SavedOverdue = 1060909,                         //       (4) Förfallna dagar
	SE_GrossSalary_Absence_Vacation_NoVacationDaysDeducted = 1060910,               //       (4) Inga dagar dras
	SE_GrossSalary_Absence_Vacation_Paid_Secondary = 1060911,                       //       (4) Betalda dagar, Sekundär
	SE_GrossSalary_Absence_Vacation_Unpaid_Secondary = 1060912,                     //       (4) Obetalda dagar, Sekundär
	SE_GrossSalary_Absence_Vacation_Advance_Secondary = 1060913,                    //       (4) Förskottsdagar, Sekundär
	SE_GrossSalary_Absence_Vacation_SavedYear1_Secondary = 1060914,                 //       (4) Sparade dagar år 1, Sekundär
	SE_GrossSalary_Absence_Vacation_SavedYear2_Secondary = 1060915,                 //       (4) Sparade dagar år 2, Sekundär
	SE_GrossSalary_Absence_Vacation_SavedYear3_Secondary = 1060916,                 //       (4) Sparade dagar år 3, Sekundär
	SE_GrossSalary_Absence_Vacation_SavedYear4_Secondary = 1060917,                 //       (4) Sparade dagar år 4, Sekundär
	SE_GrossSalary_Absence_Vacation_SavedYear5_Secondary = 1060918,                 //       (4) Sparade dagar år 5, Sekundär
	SE_GrossSalary_Absence_Vacation_SavedOverdue_Secondary = 1060919,               //       (4) Förfallna dagar, Sekundär
	SE_GrossSalary_Absence_Vacation_NoVacationDaysDeducted_Secondary = 1060920,     //       (4) Inga dagar dras, Sekundär
	SE_GrossSalary_Absence_Sick = 1061000,                                          //     (3) Sjuk
	SE_GrossSalary_Absence_Sick_QualifyingDay = 1061001,                            //       (4) Karensavdrag
	SE_GrossSalary_Absence_Sick_Day2_14 = 1061002,                                  //       (4) Dag 2-14
	SE_GrossSalary_Absence_Sick_Day15 = 1061003,                                    //       (4) Dag 15-
	SE_GrossSalary_Absence_Sick_NoVacation_Day15 = 1061004,                         //       (4) Dag 15- Ej semestergrundande
	SE_GrossSalary_Absence_Sick_NoVacation_QualifyingDay = 1061005,                 //       (4) Karensavdrag Ej semestergrundande
	SE_GrossSalary_Absence_Sick_NoVacation_Day2_14 = 1061006,                       //       (4) Dag 2-14 Ej semestergrundande
	SE_GrossSalary_Absence_SwedishForImmigrants = 1061100,                          //     (3) Svenskundervisning
	SE_GrossSalary_Absence_SwedishForImmigrants_NoVacation = 1061101,               //       (4) Ej semestergrundande
	SE_GrossSalary_Absence_TemporaryParentalLeave = 1061200,                        //     (3) Tillfällig föräldrapenning
	SE_GrossSalary_Absence_TemporaryParentalLeave_ChildBirth = 1061201,             //       (4) Barns födelse
	SE_GrossSalary_Absence_TemporaryParentalLeave_ChildCare = 1061202,              //       (4) Vård av barn
	SE_GrossSalary_Absence_TemporaryParentalLeave_ChildDeceased = 1061203,          //       (4) Barn har avlidit
	SE_GrossSalary_Absence_TemporaryParentalLeave_NoVacation = 1061204,             //       (4) Ej semestergrundande
	SE_GrossSalary_Absence_MilitaryService = 1061300,                               //     (3) Totalförsvarsplikt
	SE_GrossSalary_Absence_MilitaryService_NoVacation = 1061301,                    //       (4) Ej semestergrundande
	SE_GrossSalary_Absence_MilitaryService_Total = 1061310,                         //     (3) Totalförsvarsplikt totalt
	SE_GrossSalary_Absence_MilitaryService_Total_NoVacation = 1061311,
	SE_GrossSalary_Absence_LeaveOfAbsence = 1061400,                                //     (3) Tjänstledig
	SE_GrossSalary_Absence_LeaveOfAbsence_NoVacation = 1061401,                     //       (4) Ej semestergrundande
	SE_GrossSalary_Absence_TransmissionOfInfection = 1061500,                       //     (3) Överföring av smitta
	SE_GrossSalary_Absence_TransmissionOfInfection_NoVacation = 1061501,            //       (4) Ej semestergrundande
	SE_GrossSalary_WeekendSalary = 1070000,                                         //   (2) Helglön
	SE_GrossSalary_Duty = 1080000,                                                  //   (2) Jour
	SE_GrossSalary_Salary = 1090000,                                                //   (2) Lön
	SE_GrossSalary_Salary_BoardRemuneration = 1090100,                              //      (3) Lön Styrelearvode
	SE_GrossSalary_Salary_RoleSupplement = 1090200,                                 //      (3) Lön Rolltillägg
	SE_GrossSalary_Salary_ActivitySupplement = 1090201,                             //      (3) Lön Aktivitetstillägg
	SE_GrossSalary_Salary_CompetenceSupplement = 1090202,                           //      (3) Lön Kompetenstillägg
	SE_GrossSalary_Salary_ResponsibilitySupplement = 1090203,                       //      (3) Lön Ansvarstillägg
	SE_GrossSalary_SalaryCostReduction = 1100000,                                   //   (2) Lön Kostnadsavdrag
	SE_GrossSalary_SalaryRetroactive = 1110000,                                     //   (2) Lön Retroaktiv
	SE_GrossSalary_AdditionalTime = 1120000,                                        //   (2) Mertid/fyllnadstid
	SE_GrossSalary_AdditionalTime_Compensation = 1120100,                           //      (3) Mertid/fyllnadstidsersättning
	SE_GrossSalary_AdditionalTime_Addition = 1120200,                               //      (3) Mertid/fyllnadstidstillägg
	SE_GrossSalary_AdditionalTime_Compensation35 = 1120300,                         //      (3) Ersättning 35%
	SE_GrossSalary_AdditionalTime_Compensation70 = 1120400,                         //      (3) Ersättning 70%
	SE_GrossSalary_AdditionalTime_Compensation100 = 1120500,                        //      (3) Ersättning 100%
	SE_GrossSalary_OBAddition = 1130000,                                            //   (2) OB-tillägg
	SE_GrossSalary_OBAddition_OBAddition50 = 1130100,                               //      (3) OB-tillägg 50%
	SE_GrossSalary_OBAddition_OBAddition70 = 1130200,                               //      (3) OB-tillägg 70%
	SE_GrossSalary_OBAddition_OBAddition100 = 1130300,                              //      (3) OB-tillägg 100%%
	SE_GrossSalary_OBAddition_OBAddition40 = 1130400,                               //      (3) OB-tillägg 40%
	SE_GrossSalary_OBAddition_OBAddition57 = 1130500,                               //      (3) OB-tillägg 57%
	SE_GrossSalary_OBAddition_OBAddition79 = 1130600,                               //      (3) OB-tillägg 79%
	SE_GrossSalary_OBAddition_OBAddition113 = 1130700,                              //      (3) OB-tillägg 113%
	SE_GrossSalary_LayOffSalary = 1140000,                                          //   (2) Permitteringslön
	SE_GrossSalary_Commission = 1150000,                                            //   (2) Provision
	SE_GrossSalary_TravelTime = 1160000,                                            //   (2) Restid
	SE_GrossSalary_TravelTime_InsideWork = 1160100,                                 //     (3) Inom ordinarie arbete
	SE_GrossSalary_TravelTime_OutsideWork = 1160200,                                //     (3) Utom ordinarie arbete
	SE_GrossSalary_VacationCompensation = 1170000,                                  //   (2) Semesterersättning
	SE_GrossSalary_VacationCompensation_Earned = 1170100,                           //     (3) Intjänad
	SE_GrossSalary_VacationCompensation_Paid = 1170200,                             //     (3) Betald
	SE_GrossSalary_VacationCompensation_SavedYear1 = 1170300,                       //     (3) Sparade dagar år 1
	SE_GrossSalary_VacationCompensation_DirectPaid = 1170400,                       //     (3) Direktutbetald
	SE_GrossSalary_VacationCompensation_Advance = 1170500,                          //     (3) Förskott
	SE_GrossSalary_VacationCompensation_SavedYear2 = 1170600,                       //     (3) Sparade dagar år 2
	SE_GrossSalary_VacationCompensation_SavedYear3 = 1170700,                       //     (3) Sparade dagar år 3
	SE_GrossSalary_VacationCompensation_SavedYear4 = 1170800,                       //     (3) Sparade dagar år 4
	SE_GrossSalary_VacationCompensation_SavedYear5 = 1170900,                       //     (3) Sparade dagar år 5
	SE_GrossSalary_VacationCompensation_SavedOverdue = 1171000,                     //     (3) Förfallna dagar
	SE_GrossSalary_VacationCompensation_SavedOverdueVariable = 1172000,             //     (3) Förfallna rörligt dagar
	SE_GrossSalary_VacationSalary = 1180000,                                        //     (2) Semesterlön
	SE_GrossSalary_VacationSalary_Paid = 1180100,                                   //     (3) Betalda dagar
	SE_GrossSalary_VacationSalary_Unpaid = 1180200,                                 //     (3) Obetalda dagar
	SE_GrossSalary_VacationSalary_Advance = 1180300,                                //     (3) Förskottsdagar
	SE_GrossSalary_VacationSalary_SavedYear1 = 1180400,                             //     (3) Sparade dagar år 1
	SE_GrossSalary_VacationSalary_SavedYear2 = 1180500,                             //     (3) Sparade dagar år 2
	SE_GrossSalary_VacationSalary_SavedYear3 = 1180600,                             //     (3) Sparade dagar år 3
	SE_GrossSalary_VacationSalary_SavedYear4 = 1180700,                             //     (3) Sparade dagar år 4
	SE_GrossSalary_VacationSalary_SavedYear5 = 1180800,                             //     (3) Sparade dagar år 5
	SE_GrossSalary_VacationSalary_SavedOverdue = 1180900,                           //     (3) Förfallna dagar
	SE_GrossSalary_VacationAddition = 1190000,                                      //   (2) Semestertillägg
	SE_GrossSalary_VacationAddition_Paid = 1190100,                                 //     (3) Betalda dagar
	SE_GrossSalary_VacationAddition_Unpaid = 1190200,                               //     (3) Obetalda dagar
	SE_GrossSalary_VacationAddition_Advance = 1190300,                              //     (3) Förskottsdagar
	SE_GrossSalary_VacationAddition_SavedYear1 = 1190400,                           //     (3) Sparade dagar år 1
	SE_GrossSalary_VacationAddition_SavedYear2 = 1190500,                           //     (3) Sparade dagar år 2
	SE_GrossSalary_VacationAddition_SavedYear3 = 1190600,                           //     (3) Sparade dagar år 3
	SE_GrossSalary_VacationAddition_SavedYear4 = 1190700,                           //     (3) Sparade dagar år 4
	SE_GrossSalary_VacationAddition_SavedYear5 = 1190800,                           //     (3) Sparade dagar år 5
	SE_GrossSalary_VacationAddition_SavedOverdue = 1190900,                         //     (3) Förfallna dagar
	SE_GrossSalary_VacationAdditionVariable = 1191000,                              //   (2) Rörligt semestertillägg
	SE_GrossSalary_VacationAdditionVariable_Paid = 1191100,                         //     (3) Betalda
	SE_GrossSalary_VacationAdditionVariable_Advance = 1191200,                      //     (3) Förskott
	SE_GrossSalary_SicknessSalary = 1200000,                                        //   (2) Sjuklön
	SE_GrossSalary_SicknessSalary_Day2_14 = 1200100,                                //     (3) Dag 2-14
	SE_GrossSalary_SicknessSalary_Day15 = 1200200,                                  //     (3) Avtal dag 15-
	SE_GrossSalary_SicknessSalary_Deduction = 1200300,                              //     (3) Karensavdrag
	SE_GrossSalary_ShiftAddition = 1210000,                                         //   (2) Skiftformstillägg
	SE_GrossSalary_AllowanceStandard = 1220000,                                     //   (2) Traktamente > schablon
	SE_GrossSalary_OvertimeCompensation = 1230000,                                  //   (2) Övertidsersättning
	SE_GrossSalary_OvertimeCompensation_OBAddition50 = 1230100,                     //   (3) Övertidsersättning 50%
	SE_GrossSalary_OvertimeCompensation_OBAddition70 = 1230200,                     //   (3) Övertidsersättning 70%
	SE_GrossSalary_OvertimeCompensation_OBAddition100 = 1230300,                    //   (3) Övertidsersättning 100%
	SE_GrossSalary_OvertimeCompensation_OBAddition35 = 1230400,                     //   (3) Övertidsersättning 35%
	SE_GrossSalary_OvertimeAddition = 1240000,                                      //   (2) Övertidstillägg
	SE_GrossSalary_OvertimeAddition_OBAddition50 = 1240100,                         //   (3) Övertidstillägg 50%
	SE_GrossSalary_OvertimeAddition_OBAddition70 = 1240200,                         //   (3) Övertidstillägg 70%
	SE_GrossSalary_OvertimeAddition_OBAddition100 = 1240300,                        //   (3) Övertidstillägg 100%
	SE_GrossSalary_OvertimeAddition_OBAddition35 = 1240400,                         //   (3) Övertidstillägg 35%
	SE_GrossSalary_TimeSalary = 1250000,                                            //   (2) Tidlön
	SE_GrossSalary_HourlySalary = 1260000,                                          //   (2) Timlön
	SE_GrossSalary_MonthlySalary = 1270000,                                         //   (2) Månadslön
	SE_GrossSalary_VacationAdditionOrSalaryPrepayment = 1280000,                    //   (2) Semestertillägg/lön förutbetald
	SE_GrossSalary_VacationAdditionOrSalaryPrepayment_Paid = 1280100,               //     (3) Betald
	SE_GrossSalary_VacationAdditionOrSalaryPrepayment_Invert = 1280200,             //     (3) Motbokning
	SE_GrossSalary_VacationAdditionOrSalaryVariablePrepayment = 1281000,            //   (2) Rörligt Semestertillägg/lön förutbetald
	SE_GrossSalary_VacationAdditionOrSalaryVariablePrepayment_Paid = 1281100,       //     (3) Betald
	SE_GrossSalary_VacationAdditionOrSalaryVariablePrepayment_Invert = 1281200,     //     (3) Motbokning
	SE_GrossSalary_EarnedHolidayPayment = 1290000,                                  //   (2) Utbetalning röda dagar
	SE_GrossSalary_TimeWorkReduction = 1300000,                                    //   (2) Arbetstidsförkortning
	
	
	SE_Benefit = 2000000,                                                           // (1) Förmån
	SE_Benefit_Other = 2010000,                                                     //   (2) Annan
	SE_Benefit_PropertyNotHouse = 2020000,                                          //   (2) Bostad ej småhus
	SE_Benefit_PropertyNotHouse_PartlyFree = 2020100,                               //     (3) Delvis fri
	SE_Benefit_PropertyNotHouse_Free = 2020200,                                     //     (3) Helt fri
	SE_Benefit_PropertyHouse = 2030000,                                             //   (2) Bostad småhus
	SE_Benefit_PropertyHouse_PartlyFree = 2030100,                                  //     (3) Delvis fri
	SE_Benefit_PropertyHouse_Free = 2030200,                                        //     (3) Helt fri
	SE_Benefit_Fuel = 2040000,                                                      //   (2) Drivmedel
	SE_Benefit_Fuel_PartNotAnnualized = 2040100,                                    //     (3) Del ej uppräknad
	SE_Benefit_Fuel_PartAnnualized = 2040200,                                       //     (3) Del uppräknad
	SE_Benefit_ROT = 2050000,                                                       //   (2) ROT
	SE_Benefit_RUT = 2060000,                                                       //   (2) RUT
	SE_Benefit_Food = 2070000,                                                      //   (2) Kost
	SE_Benefit_BorrowedComputer = 2080000,                                          //   (2) Lånedator
	SE_Benefit_Parking = 2090000,                                                   //   (2) Parkering
	SE_Benefit_Interest = 2100000,                                                  //   (2) Ränta
	SE_Benefit_CompanyCar = 2110000,                                                //   (2) Bilförmån
	SE_Benefit_Invert = 2120000,                                                    //   (2) Motbokning förmån
	SE_Benefit_Invert_Other = 2120100,                                              //      (3) Annan
	SE_Benefit_Invert_PropertyNotHouse = 2120200,                                   //      (3) Bostad ej småhus
	SE_Benefit_Invert_PropertyHouse = 2120300,                                      //      (3) Bostad småhus
	SE_Benefit_Invert_Fuel = 2120400,                                               //      (3) Drivmedel
	SE_Benefit_Invert_ROT = 2120500,                                                //      (3) ROT
	SE_Benefit_Invert_RUT = 2120600,                                                //      (3) RUT
	SE_Benefit_Invert_Food = 2120700,                                               //      (3) Kost
	SE_Benefit_Invert_BorrowedComputer = 2120800,                                   //      (3) Lånedator
	SE_Benefit_Invert_Parking = 2120900,                                            //      (3) Parkering
	SE_Benefit_Invert_Interest = 2121000,                                           //      (3) Ränta
	SE_Benefit_Invert_CompanyCar = 2121100,                                         //      (3) Bilförmån
	SE_Benefit_Invert_Standard = 2121200,                                           //      (3) Standard
	
	SE_Tax = 3000000,                                                               // (1) Skatt
	SE_Tax_TableTax = 3010000,                                                      //   (2) Enligt tabell
	SE_Tax_OneTimeTax = 3020000,                                                    //   (2) Engångsskatt
	SE_Tax_Optional = 3030000,                                                      //   (2) Frivillig
	SE_Tax_SINK = 3040000,                                                          //   (2) Sink
	SE_Tax_ASINK = 3050000,                                                         //   (2) A-Sink
	
	
	SE_Compensation = 4000000,                                                      // (1) Ersättning
	SE_Compensation_CarCompensation = 4010000,                                      //   (2) Bilersättning
	SE_Compensation_CarCompensation_BenefitCar = 4010100,                           //      (3) Förmånsbil
	SE_Compensation_CarCompensation_PrivateCar = 4010200,                           //      (3) Privat bil
	SE_Compensation_RentalCompensation = 4020000,                                   //   (2) Hyresersättning
	SE_Compensation_SportsActivity = 4030000,                                       //   (2) Idrottsutövning
	SE_Compensation_Representation = 4040000,                                       //   (2) Representation
	SE_Compensation_TravelCost = 4050000,                                           //   (2) Resekostnad
	SE_Compensation_Accomodation = 4055000,                                         //   (2) Logi
	SE_Compensation_TravelAllowance = 4060000,                                      //   (2) Traktamente
	SE_Compensation_TravelAllowance_DomesticShortTerm = 4060100,                    //     (3) Inrikes korttid
	SE_Compensation_TravelAllowance_DomesticLongTerm = 4060200,                     //     (3) Inrikes lång tid
	SE_Compensation_TravelAllowance_DomesticOverTwoYears = 4060300,                 //     (3) Inrikes > 2 år
	SE_Compensation_TravelAllowance_ForeignShortTerm = 4060400,                     //     (3) Utrikes korttid
	SE_Compensation_TravelAllowance_ForeignLongTerm = 4060500,                      //     (3) Utrikes lång tid
	SE_Compensation_TravelAllowance_ForeignOverTwoYears = 4060600,                  //     (3) Utrikes > 2 år
	SE_Compensation_Other = 4070000,                                                //   (2) Övrigt
	SE_Compensation_Other_TaxFree = 4070100,                                        //     (3) Skattefri
	SE_Compensation_Other_Taxable = 4070200,                                        //     (3) Skattepliktig
	SE_Compensation_Vat = 4080000,                                                  //   (2) Moms
	SE_Compensation_Vat_6Percent = 4080100,                                         //     (3) Moms 6 %
	SE_Compensation_Vat_12Percent = 4080200,                                        //     (3) Moms 12 %
	SE_Compensation_Vat_25Percent = 4080300,                                        //     (3) Moms 25 %
	
	SE_Deduction = 5000000,                                                         // (1) Avdrag
	SE_Deduction_CarBenefit = 5010000,                                              //   (2) Förmånsbil
	SE_Deduction_HouseKeeping = 5020000,                                            //   (2) Hushållsarbete
	SE_Deduction_NetSalary = 5030000,                                               //   (2) Nettolön
	SE_Deduction_UnionFee = 5040000,                                                //   (2) Fackavgift
	SE_Deduction_UnionFee_InspectionFee = 5040100,                                  //      (3) Kontrollavgift
	SE_Deduction_ReviewFee = 5050000,                                               //   (2) Granskning/mätningsavgift
	SE_Deduction_InspectionFee = 5060000,                                           //   (2) Kontrollavgift
	SE_Deduction_InterestDeduction = 5070000,                                       //   (2) Ränteavdrag
	SE_Deduction_Other = 5080000,                                                   //   (2) Övrigt
	SE_Deduction_SalaryDistress = 5090000,                                          //   (2) Löneutmätning
	SE_Deduction_SalaryDistressAmount = 5090100,                                    //     (3) Utmätningsbelopp
	SE_Deduction_SalaryDistressAdjustment = 5090200,                                //     (3) Justering utmätning
	
	SE_CostDeduction = 6000000,                                                     // (1) Kostnadsavdrag
	
	SE_OccupationalPension = 7000000,                                               // (1) Tjänstepension
	
	SE_Time = 8000000,                                                              // (1) Tid
	SE_Time_Accumulator = 8010000,                                                  //   (2) Saldo
	SE_Time_Accumulator_AccumulatorPlaceholder = 8010100,                           //     (3) Saldon (dummy)
	SE_Time_Accumulator_PlusTime = 8010101,                                         //       (4) Plustid
	SE_Time_Accumulator_MinusTime = 8010102,                                        //       (4) Minustid
	SE_Time_Accumulator_Time = 8010103,                                             //       (4) Tid
	SE_Time_Accumulator_OverTime = 8010104,                                         //       (4) Övertid
	SE_Time_Accumulator_Withdrawal = 8010105,                                       //       (4) Uttag
	SE_Time_Accumulator_Payment = 8010106,                                          //       (4) Utbetalning
	SE_Time_Accumulator_AddedTime = 8010107,                                        //       (4) Mertid
	SE_Time_Accumulator_OnCallTime = 8010108,                                       //       (4) Jourtid
	SE_Time_WorkedScheduledTime = 8020000,                                          //   (2) Arbetad schematid
	SE_Time_AbsenceOutsideSchedule = 8030000,                                       //   (2) Frånvaro utanför schematid
	
	SE_EmploymentTaxCredit = 9000000,                                               // (1) Arbetsgivaravgift Kredit
	
	SE_EmploymentTaxDebit = 10000000,                                               // (1) Arbetsgivaravgift Debet
	
	SE_SupplementChargeCredit = 11000000,                                           // (1) Påslag Kredit
	
	SE_SupplementChargeDebit = 12000000,                                            // (1) Påslag Debet
	
	SE_NetSalary = 13000000,                                                        // (1) Nettolön
	SE_NetSalary_Paid = 13010000,                                                   // (2) Utbetald
	SE_NetSalary_Rounded = 13020000,                                                // (2) Avrundad
	
	SE_PensionPremium = 14000000,                                                  // (1) Pensionspremie
	SE_PensionPremium_WorkingAccount = 14010000,                                   // (2) Arbetstidskonto
	SE_PensionPremium_TimeWorkReduction = 14020000,                                // (2) Arbetstidsförkortning
	
	SE_Statistic = 15000000,                                                        // (1) Statistik
	SE_BygglosenPaidoutExcess = 15010000,                                           // (2) Bygglosen Utbetalt överskott
}

export enum TermGroup_SysPayrollPriceAmountType {
	Amount = 0,
	Percent = 1,
	Days = 2,
	Hours = 3,
	Number = 4
}

export enum TermGroup_PayrollReviewStatus {
	New = 0,
	Preliminary = 1,
	Executed = 2
}

export enum TermGroup_PayrollProductCentRoundingType {
	None = 0,
	Mathematical = 1,
	Up = 2,
	Down = 3
}

export enum TermGroup_PayrollProductTaxCalculationType {
	TableTax = 0,
	OneTimeTax = 1
}

export enum TermGroup_EmployeeTaxType {
	NotSelected = 0,
	TableTax = 1,               // Enligt skattetabell
	SideIncomeTax = 2,          // Sidoinkomst 30 % skatt
	Adjustment = 3,             // Särskild beräkningsgrund (jämkning)
	Sink = 4,                   // Särskild inkomstskatt (SINK)
	SchoolYouth = 5,            // Skolungdom
	NoTax = 6                   // Ej skatt
}

export enum TermGroup_EmployeeTaxAdjustmentType {
	NotSelected = 0,
	PercentTax = 1,             // Procentskatt
	IncreasedTaxBase = 2,       // Ökat skatteunderlag (kr)
	DecreasedTaxBase = 3,       // Minskat skatteunderlag (kr)
	NoTax = 4                   // Ej skatt
}

export enum TermGroup_EmployeeTaxSinkType {
	NotSelected = 0,
	Normal = 1,                 // SINK (25%)
	AthletsArtistSailors = 2,   // A-SINK (15%)
	NoTax = 3,                  // SINK (0%)
}

export enum TermGroup_EmployeeTaxSalaryDistressAmountType {
	NotSelected = 0,
	FixedAmount = 1,
	AllSalary = 2,
}

export enum TermGroup_TrackChangesAction {
	Insert = 1,
	Update = 2,
	Delete = 3
}

export enum TermGroup_EmployeeTaxEmploymentTaxType {
	NotSelected = 0,
	EmploymentTax = 1,      // Arbetsgivaravgift
	PayrollTax = 2,         // Egenavgift
	EmploymentAbroad = 5,   // Utsänd personal
}

export enum TermGroup_PayrollResultType {
	None = 0,
	Time = 1,
	Quantity = 2,
}

export enum TermGroup_EmployeeTaxEmploymentAbroadCode {
	None = 0,
	
	SweToCan = 1,       // Arbetsgivare i Sverige för utsänd personal till Kanada
	SweToUsa = 2,       // Arbetsgivare i Sverige för utsänd personal till USA
	SweToQue = 3,       // Arbetsgivare i Sverige för utsänd personal till Québec
	SweToInd = 4,       // Arbetsgivare i Sverige för utsänd personal till Indien
	
	CanToSweGroup = 21, // Utsänd personal till Sverige från koncernbolag i Kanada
	UsaToSweGroup = 22, // Utsänd personal till Sverige från koncernbolag i USA
	QueToSweGroup = 23, // Utsänd personal till Sverige från koncernbolag i Québec
	
	CanToSwe = 41,      // Arbetsgivare i Kanada för utsänd personal till Sverige
	UsaToSwe = 42,      // Arbetsgivare i USA för utsänd personal till Sverige
	QueToSwe = 43,      // Arbetsgivare i Québec för utsänd personal till Sverige
	IndToSwe = 44,      // Arbetsgivare i Indien för utsänd personal till Sverige
}

export enum TermGroup_VacationGroupType {
	Unknown = 0,
	NoCalculation = 1,                      // Ingen beräkning
	DirectPayment = 2,                      // Direktutbetalning
	EarningYearIsBeforeVacationYear = 3,    // Intjänandeår = 12 månadsperiod närmast före semesterår
	EarningYearIsVacationYear = 4,          // Intjänandeår = Semesterår
}

export enum TermGroup_PensionCompany {
	NotSelected = 0,
	
	SE_ITP1 = 1,
	SE_ITP2 = 2,
	SE_ITP1_ITP2 = 3,
	SE_KPA = 4,
	SE_FORA = 5,
	SE_GTP = 6, // (Folksam)
	SE_SKANDIA = 7,
}

export enum TermGroup_StockTransactionType {
	Unknown = 0,
	Take = 1,
	Add = 2,
	Correction = 3,
	Reserve = 4,
	Loss = 5,
	AveragePriceChange = 6,
	StockTransfer = 7
};

export enum TermGroup_ReportPrintoutDeliveryType {
	Unknown = 0,
	Instant = 1,
	Generate = 2,
	XEMail = 3,
	Email = 4,
};

export enum TermGroup_ReportPrintoutStatus {
	Unknown = 0,
	Ordered = 1,
	Delivered = 2,
	Sent = 3,
	SentFailed = 4,
	Cleaned = 5,
	Error = 6,
	DeletedByUser = 7,
	Queued = 8,
	Warning = 9,
	Internal = 10
};

export enum TermGroup_EInvoiceDistributor {
	Unknown = 0,
	InExchange = 1,
	ReadSoft = 2,
}

export enum TermGroup_EInvoiceFormat {
	Unknown = 0,
	Finvoice = 1,
	Svefaktura = 2,
	SvefakturaTidbok = 3,
	SvefakturaAPI = 4,
	//SvefakturaAPITidbok = 5,
	Finvoice2 = 6,
	Finvoice3 = 7,
	Intrum = 8,
	Peppol = 9,
}

export enum TermGroup_SysPerformanceMonitorTask {
	Unknown = 0,
	TimeSchedulePlanning_LoadCategories = 1,        // Load all categories for specified user
	TimeSchedulePlanning_LoadEmployees = 2,         // Load all employees within loaded categories
	TimeSchedulePlanning_LoadSchedules = 3,         // Load all schedules (shifts) for loaded employees (4 weeks)
	TimeSchedulePlanning_LoadTemplateSchedules = 4, // Load all template schedules for loaded employees (4 weeks)
	ChangeStatusGrid_LoadChangeStatusGridViews = 5, // Load all open orders
	OrderInvoiceEdit_LoadInvoice = 6,               // Load one order
	OrderInvoiceEdit_SaveInvoice = 7,               // Save one order
}

export enum TermGroup_PayrollProductCentRoundingLevel {
	None = 0,
	Thousands = 1,
	Hundred = 2,
	Ten = 3,
	One = 4,
	OneDecimal = 5,
	TwoDecimals = 6
}

export enum TermGroup_ReportExportFileType {
	Unknown = 0,
	Payroll_SIE_Accounting = 1,
	Payroll_SCB_Statistics = 2,
	Payroll_Visma_Accounting = 3,
	KU10 = 4,   // IncomeStatement
	eSKD = 5,   // Skattedeklaration HR och ERP
	QlikViewType1 = 6, //MatHem
	Collectum = 7,
	Fora = 8,
	KPA = 9,
	SCB_KSJU = 10,
	Payroll_SN_Statistics = 11,
	AGD = 12,
	KPADirekt = 13,
	SCB_KLP = 14,
	Payroll_SIE_VacationAccounting = 15,
	Payroll_Visma_VacationAccounting = 16,
	Bygglosen = 17,
	Kronofogden = 18,
	ICACustomerBalance = 19,
	PIRATVoucher = 20,
	ICACustomerBalanceMyStore = 21,
	ICACustomerMyStore = 22,
	FolksamGTP = 23,
	Payroll_EKS_Accounting = 24,
	Payroll_EKS_VacationAccounting = 25,
	IntrastatExport = 26,
	IntrastatImport = 27,
	SafiloCustomers = 28,
	SafiloInvoices = 29,
	IFMetall = 30,
	SkandiaPension = 31,
	ForaMonthly = 32,
	SEF = 33,
	Payroll_SAP_Accounting = 34,
	Payroll_SAP_VacationAccounting = 35,
	Payroll_SAP_HRL = 36,
	Payroll_Fremia_Statistics = 37,
	AGD_Franvarouppgift = 38,
	
};

export enum TermGroup_PayrollProductTimeUnit {
	Hours = 0,
	WorkDays = 1,
	CalenderDays = 2,
	CalenderDayFactor = 3,
	VacationCoefficient = 4
}

export enum TermGroup_EmployeeStatisticsType {
	AnsweredCalls = 1,      // Besvarade samtal (st)
	CallDuration = 2,       // Samtalslängd (sek)
	ConnectedTime = 3,      // Inloggad tid (%)
	NotAnsweredCalls = 4,   // Förbiringda (ej tagna samtal) (%)
	Arrival = 5,            // Sen ankomst (min)
	GoHome = 6,             // Tidig hemgång (min)
	ArrivalAndGoHome = 7    // Sen ankomst och tidig hemgång (min)
}

export enum TermGroup_ForeignPaymentBankCode {
	None = 0,
	Handelsbanken = 1,
	SEB = 2,
	Swedbank = 3,
	Nordea = 4,
	Other = 5,
};

export enum TermGroup_ForeignPaymentForm {
	None = 0,
	Account = 1,
	Check = 2,
};

export enum TermGroup_ForeignPaymentMethod {
	None = 0,
	Normal = 1,
	Express = 2,
	CompanyGroup = 3,
};

export enum TermGroup_ForeignPaymentChargeCode {
	None = 0,
	SenderDomesticCosts = 1,
	SenderAllCosts = 2,
	RecieverAllCosts = 3,
};

export enum TermGroup_ForeignPaymentIntermediaryCode {
	None = 0,
	BGC = 1,
};

export enum TermGroup_PayrollProductQuantityRoundingType {
	None = 0,
	Up = 1,
	Down = 2
}

export enum TermGroup_EmployeeStatisticsAverageType {
	None = 0,
	ShiftType = 1,
	Category = 2,
	Company = 3
}

export enum TermGroup_VacationGroupCalculationType {
	Unknown = 0,
	
	DirectPayment_AccordingToVacationLaw = 1,                                                   // Enligt semesterlagen
	DirectPayment_AccordingToCollectiveAgreement = 2,                                           // Enligt kollektivavtal
	
	EarningYearIsBeforeVacationYear_PercentCalculation_AccordingToVacationLaw = 11,             // Procentuell beräkning enligt semesterlagen
	EarningYearIsBeforeVacationYear_PercentCalculation_AccordingToCollectiveAgreement = 12,     // Procentuell beräkning enligt kollektivavtal
	EarningYearIsBeforeVacationYear_VacationDayAddition_AccordingToVacationLaw = 13,            // Semestertillägg enligt semesterlagen
	EarningYearIsBeforeVacationYear_VacationDayAddition_AccordingToCollectiveAgreement = 14,    // Semestertillägg enligt kollektivavtal
	
	EarningYearIsVacationYear_ABAgreement = 21,                                                 // AB-avtal
	EarningYearIsVacationYear_VacationDayAddition = 22                                          // Semestertillägg
}

export enum TermGroup_VacationGroupRemainingDaysRule {
	Unknown = 0,
	SavedAccordingToVacationLaw = 1,    // Sparas enligt semesterlagen
	AllRemainingDaysSaved = 2,          // Alla kvarvarande dagar sparas
	AllRemainingDaysSavedToYear1 = 3,   // Alla kvarvarande dagar sparas till år 1
}

export enum TermGroup_PayrollExportPersonalCategory {
	Unknown = 0,
	Employee = 1,
	Others = 2,
	EmployeeRedAgreement = 3,
	EmployeeBlueAgreement = 4,
	EmployeeElectricAgreement = 5,
	CoEmployeeGreenAgreement = 6,
	SIF = 7,
	CF = 8,
	LEDARNA = 9,
}

export enum TermGroup_PayrollExportWorkTimeCategory {
	Unknown = 0,
	DayWork = 1,
	TwoShifts = 2,
	ThreeShiftsNonContinuous = 3,
	ThreeShiftsContinuous = 4,
	ContinuousMajorPublicWorkUnderground = 5,
	PermanentNightShift = 6,
	PartTimeJob = 7,
	PartTimeRetired = 8
}

export enum TermGroup_PayrollExportSalaryType {
	Unknown = 0,
	Monthly = 1,
	Weekly = 2,
	Hourly = 3,
}

export enum TermGroup_VacationGroupVacationDaysHandleRule {
	Unknown = 0,
	VacationFactor = 1,         // Semesterfaktor
	Gross = 2,                  // Brutto
	Net = 3,                    // Netto
	VacationCoefficient = 4     // Semesterkoefficient
}

export enum TermGroup_VacationGroupVacationSalaryPayoutRule {
	Unknown = 0,
	InConjunctionWithVacation = 1,      // I samband med semesterledighet
	PartlyPayoutBeforeVacation = 2,     // Delutbetalning före semesterledighet
	AllBeforeVacation = 3               // Alla före semesterledighet
}

export enum TermGroup_VacationGroupVacationAbsenceCalculationRule {
	Unknown = 0,
	Actual = 1,     // Faktisk ersättning
	PerDay = 2,     // Uppräkning per dag
	PerHour = 3,    // Uppräkning per timme
}

export enum TermGroup_VacationGroupGuaranteeAmountMaxNbrOfDaysRule {
	Unknown = 0,
	All = 1,        // Samtliga
	Max25Paid = 2   // Max 25 betalda
}

export enum TermGroup_VacationGroupVacationHandleRule {
	Unknown = 0,
	Days = 1,       // Dagar
	Hours = 2,      // Timmar
	Shifts = 3      // Arbetspass
}

export enum TermGroup_TimeSalaryPaymentExportType {
	Undefined = 0,
	SUS = 1, // Swedbank
	Nordea = 2, //Nordea personkonto
	BGCLB = 3,
	BGCKI = 4,
	ISO20022 = 5, //pain001
}

export enum TermGroup_EmployeeFactorType {
	CalendarDayFactor = 1,              // Kalenderdagsfaktor
	VacationCoefficient = 2,            // Semesterkoefficient
	AverageWorkTimeWeek = 3,            // Snittarbetstid/vecka
	AverageWorkTimeShift = 4,           // Snittarbetstid/arbetspass
	Net = 5,                            // Netto
	VacationDaysPaidByLaw = 6,          // Semesterrätt dagar
	VacationDayPercent = 7,             // Semesterlön/dag
	VacationHourPercent = 8,            // Semesterlön/timme
	VacationVariablePercent = 9,        // Semestertillägg
	GuaranteeAmount = 10,               // Garantibelopp
	VacationVariableAmountPerDay = 11,  // Rörligt semestertillägg per dag
	VacationHoursPaid = 12,             // Semesterrätt timmar
	CurrentLasDays = 13,                // Aktuella LAS-dagar
	BalanceLasDays = 14,                // Ingående LAS-dagar
	BalanceLasDaysAva = 15,             // Ingående LAS-dagar (AVA)
	BalanceLasDaysSva = 16,             // Ingående LAS-dagar (SVA)
	BalanceLasDaysVik = 17,             // Ingående LAS-dagar (VIK)
	TimeWorkAccountPaidLeave = 18,      // ATK Betald ledighet
	VacationDayPercentFinalSalary = 19, // Semesterlön/dag slutlön intjänade dagar
}

export enum TermGroup_MassRegistrationInputType {
	Unknown = 0,
	Manual = 1,
	Automatic = 2,
}

export enum TermGroup_VoucherRowHistoryEvent {
	Unknown = 0,
	New = 1,
	Modified = 2,
	Removed = 3,
}

export enum TermGroup_VoucherRowHistoryField {
	Unknown = 0,
	VoucherDate = 1,
	VoucherText = 2,
	Account = 3,
	RowText = 4,
	Amount = 5,
	Quantity = 6,
	InternalAccount = 7,
}

export enum TermGroup_FollowUpTypeType {
	Own = 0,
	Introduction = 1,               //Introduktion
	PerformanceReview = 2,          //Utvecklingssamtal
	DevelopmentPlan = 3,            //Utvecklingsplan
	SalaryReview = 4,               //Lönesamtal
	Rehabilitation = 5,             //Rehabilitering
	EmploymentEndReview = 6,        //Avgångsintervju
}

export enum TermGroup_ChangeStatusGridAllItemsSelection {
	None = 0,
	One_Month = 1,
	Tree_Months = 3,
	Six_Months = 6,
	Twelve_Months = 12,
	TwentyFour_Months = 24,
	All = 99,
}

export enum TermGroup_SysPaymentService {
	Unknown = 0,
	Autogiro = 1,
}

export enum TermGroup_ReportGroupAndSortingTypes {
	//General
	
	Unknown = 0,
	
	AccountNr = 1,
	AccountInternalDim2 = 2,
	AccountInternalDim3 = 3,
	AccountInternalDim4 = 4,
	AccountInternalDim5 = 5,
	AccountInternalDim6 = 6,
	
	//Personell (TimePayrollTransactionReport)
	
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
	
	//ERP
	
	CustomerName = 101,
	CustomerNr = 102,
	InvoiceNr = 103,
	ProjectNr = 104,
	
	SupplierName = 200,
	SupplierNr = 201,
	
};

export enum TermGroup_AccountMandatoryLevel {
	None = 0,
	Warn = 1,
	Mandatory = 2,
	Stop = 3,
}

export enum TermGroup_VacationGroupYearEndRemainingDaysRule {
	Unknown = 0,
	Paid = 1,
	Saved = 2,
	Over20DaysSaved = 3
}

export enum TermGroup_VacationGroupYearEndOverdueDaysRule {
	Unknown = 0,
	Paid = 1,
	Saved = 2
}

export enum TermGroup_VacationGroupYearEndVacationVariableRule {
	Unknown = 0,
	Paid = 1,
	Saved = 2
}

export enum TermGroup_SoePayrollPriceType {
	Misc = 0,
	Hourly = 1,
	Monthly = 2,
	Fulltime = 3,
}

export enum TermGroup_VehicleType {
	Unknown = 0,
	Car = 1,
	Lorry = 2,
}

export enum TermGroup_SysPayrollStartValue {
	//PJ specar
}

export enum TermGroup_SysPageStatusSiteType {
	Test = 0,
	Beta = 1,
	Live = 2
}

export enum TermGroup_SysPageStatusStatusType {
	Blocked = 0,
	RFT = 1,
	Active = 2,
	AngularJsBlocked = 3,
	AngularJsFirst = 4,
	ActiveForCompany = 5,
}

export enum TermGroup_SupplierInvoiceType {
	None = 0,
	Invoice = 1,
	Scanning = 2,
	EDI = 3,
	Finvoice = 4,
	Uploaded = 5,
}

export enum TermGroup_AttestWorkFlowApproverType {
	User = 0,
	AttestRole = 1
}

export enum TermGroup_AfaCategory {
	None = 0,
	Arbetare = 1,
	Tjansteman = 2,
	Undantas = 3,
	
}

export enum TermGroup_AfaSpecialAgreement {
	None = 0,
	EgetAvtalTjansteman = 1
}

export enum TermGroup_PayrollReportsCollectumITPplan {
	None = 0,
	ITP1 = 1,
	ITP2 = 2,
}

export enum TermGroup_SupplierInvoiceAttestGroupSuggestionPrio {
	None = 0,
	Supplier = 1,
	Costplace = 2,
	Project = 3,
	OurReference = 4
}

export enum TermGroup_OrderType {
	Unspecified = 0,
	Project = 1,
	Sales = 2,
	Internal = 3,
	Service = 4,
	ATA = 5,
	Contract = 6,
}

export enum TermGroup_TimeSchedulePlanningDayViewGroupBy {
	Employee = 0,
	Category = 1,
	ShiftType = 2,
	ShiftTypeFirstOnDay = 3,
}

export enum TermGroup_TimeSchedulePlanningDayViewSortBy {
	Firstname = 0,
	Lastname = 1,
	EmployeeNr = 2,
	StartTime = 3,
}

export enum TermGroup_TimeSchedulePlanningScheduleViewGroupBy {
	Employee = 0,
	Category = 1,
	ShiftType = 2
}

export enum TermGroup_TimeSchedulePlanningScheduleViewSortBy {
	Firstname = 0,
	Lastname = 1,
	EmployeeNr = 2
}

export enum TermGroup_TimeSchedulePlanningEmployeeListSortBy {
	Firstname = 0,
	Lastname = 1,
	EmployeeNr = 2,
	Availability = 3
}

export enum TermGroup_StaffingNeedsDayViewGroupBy {
	None = 0,
	AccountDim2 = 1,
	AccountDim3 = 2,
	AccountDim4 = 3,
	AccountDim5 = 4,
	AccountDim6 = 5,
	ShiftType = 6
}

export enum TermGroup_StaffingNeedsScheduleViewGroupBy {
	None = 0,
	AccountDim2 = 1,
	AccountDim3 = 2,
	AccountDim4 = 3,
	AccountDim5 = 4,
	AccountDim6 = 5,
	ShiftType = 6
}

export enum TermGroup_StaffingNeedsDayViewSortBy {
	Name = 0,
	StartTime = 1,
}

export enum TermGroup_DailyRecurrencePatternType {
	Daily = 0,              // Every day
	Weekly = 1,             // Every week
	AbsoluteMonthly = 2,    // Same day every month
	RelativeMonthly = 3,    // Same week every month
	AbsoluteYearly = 4,     // Same day every year
	RelativeYearly = 5,     // Same week every year
	SysHoliday = 6          // System holiday
}

export enum TermGroup_DailyRecurrencePatternWeekIndex {
	First = 0,
	Second = 1,
	Third = 2,
	Fourth = 3,
	Last = 4
}

export enum TermGroup_DailyRecurrenceRangeType {
	EndDate = 0,    // End by specific date
	NoEnd = 1,      // No end date
	Numbered = 2    // End after specific number of occurrences
}

export enum TermGroup_AccountingBudgetSubType {
	Year = 0,
	January = 1,
	February = 2,
	March = 3,
	April = 4,
	May = 5,
	June = 6,
	July = 7,
	August = 8,
	September = 9,
	October = 10,
	November = 11,
	December = 12,
	Week = 13,
	Day = 14,
	YearWeek = 15,
}

export enum TermGroup_AccountingBudgetType {
	AccountingBudget = 1,
	StaffBudget = 2,
	ProjectBudget = 3,
	SalesBudget = 4,
	SalesBudgetTime = 5,
	SalesBudgetSalaryCost = 6,
	Project_Prognosis,
	Project_Budget,
	Project_IB,
	Project_ATA
}

export enum TermGroup_SalesBudgetInterval {
	Year = 1,
	MontWeekly = 2,
	MonthDaily = 3,
	Week = 4,
	Day = 5
}

export enum TermGroup_StaffingNeedHeadsFilterType {
	None = 0,
	ActualNeed = 1,
	BaseNeed = 2,
	SpecificNeed = 3,
}

export enum TermGroup_EmployeePostWeekendType {
	AutomaticWeekend = 0,
	PreferEvenWeekWeekend = 1,
	PreferOddWeekWeekend = 2,
}

export enum TermGroup_SoeRetroactivePayrollStatus {
	Unknown = 0,
	Saved = 1,
	Multiple = 2,
	PartlyCalculated = 11,
	Calculated = 12,
	PartyPayroll = 21,
	Payroll = 22,
	PartlyLocked = 31,
	Locked = 32,
}

export enum TermGroup_SoeRetroactivePayrollEmployeeStatus {
	Unknown = 0,
	Saved = 1,
	Calculated = 11,
	Payroll = 21,
	Locked = 31,
	
	Error = 101,
	EmployeeExistsInOtherActiveRetro = 102,
	EmployeeHasChangedTimePeriodHead = 103,
	EmployeeHasNoBasis = 104,
}

export enum TermGroup_RetroactivePayrollAccountType {
	Unknown = 0,
	TransactionAccount = 1,
	OtherAccount = 2,
}

export enum TermGroup_TimeSchedulePlanningFollowUpCalculationType {
	All = 0,
	Sales = 1,
	Hours = 2,
	PersonelCost = 3,
	LPAT = 4,
	FPAT = 5,
	BPAT = 6,
	SalaryPercent = 7,
	
	//Calculated
	ActualHours = 11,
}

export enum TermGroup_SysVehicleFuelType {
	Unknown = 0,
	Gasoline = 1,       // Bensin
	Diesel = 2,         // Diesel
	Electricity = 3,    // El
	Gas = 4,            // Gas
	Alcohol = 5,        // Alkohol (etanol)
	ElectricHybrid = 6, // Elhybrid
	PlugInHybrid = 7,   // Laddhybrid
	HydrogenGas = 8     // Vätgas
}

export enum TermGroup_EDistributionType {
	Unknown = 0,
	Email = 1,
	Inexchange = 2,
	Intrum = 3,
	Finvoice = 4,
	Fortnox = 5,
	VismaEAccounting = 6,
}

export enum TermGroup_EDistributionStatusType {
	PendingInPlatform = 0,
	Sent = 1,
	Error = 2,
	Stopped = 3,
	Unknown = 4,
}

export enum TermGroup_GridDateSelectionType {
	None = 0,
	One_Day = 1,
	One_Week = 7,
	One_Month = 30,
	Tree_Months = 90,
	Six_Months = 180,
	Twelve_Months = 365,
	TwentyFour_Months = 730,
	All = 999,
}

export enum TermGroup_SoeRetroactivePayrollOutcomeErrorCode {
	None = 0,
	EmploymentNotFound = 1,
	SpecifiedUnitPrice = 2,
	FormulaNotFound = 3,
	RetroDontOverlapPeriod = 4,
}

export enum TermGroup_TimeSchedulePlanningBreakVisibility {
	Hidden = 0,
	TotalMinutes = 1,
	Details = 2,
	Holes = 3
}

export enum TermGroup_TrackChangesColumnType {
	Unspecified = 0,
	
	// EmployeeTaxSE
	EmployeeTaxSE_MainEmployer = 101,
	EmployeeTaxSE_Type = 102,
	EmployeeTaxSE_TaxRate = 103,
	EmployeeTaxSE_TaxRateColumn = 104,
	EmployeeTaxSE_OneTimeTaxPercent = 105,
	EmployeeTaxSE_EstimatedAnnualSalary = 106,
	EmployeeTaxSE_AdjustmentType = 107,
	EmployeeTaxSE_AdjustmentValue = 108,
	EmployeeTaxSE_AdjustmentPeriodFrom = 109,
	EmployeeTaxSE_AdjustmentPeriodTo = 110,
	EmployeeTaxSE_SchoolYouthLimitInitial = 111,
	EmployeeTaxSE_SinkType = 112,
	EmployeeTaxSE_EmploymentTaxType = 113,
	EmployeeTaxSE_EmploymentAbroadCode = 114,
	EmployeeTaxSE_RegionalSupport = 115,
	EmployeeTaxSE_SalaryDistressAmount = 116,
	EmployeeTaxSE_SalaryDistressAmountType = 117,
	EmployeeTaxSE_SalaryDistressReserveAmount = 118,
	EmployeeTaxSE_CSRExportDate = 119,
	EmployeeTaxSE_CSRImportDate = 120,
	EmployeeTaxSE_TinNumber = 121,
	EmployeeTaxSE_CountryCode = 122,
	EmployeeTaxSE_State = 123,
	EmployeeTaxSE_ApplyEmploymentTaxMinimumRule = 124,
	EmployeeTaxSE_FirstEmployee = 125,
	EmployeeTaxSE_SalaryDistressCase = 126,
	EmployeeTaxSE_BirthPlace = 127,
	EmployeeTaxSE_CountryCodeBirthPlace = 128,
	EmployeeTaxSE_CountryCodeCitizen = 129,
	EmployeeTaxSE_SecondEmployee = 130,
	
	// EmploymentPriceTypePeriod
	EmploymentPriceTypePeriod_FromDate = 151,
	EmploymentPriceTypePeriod_Amount = 152,
	EmploymentPriceTypePeriod_Level = 153,
	
	// FixedPayrollRow
	FixedPayrollRow_FromDate = 201,
	FixedPayrollRow_ToDate = 202,
	FixedPayrollRow_UnitPrice = 203,
	FixedPayrollRow_Quantity = 204,
	FixedPayrollRow_Amount = 205,
	FixedPayrollRow_VatAmount = 206,
	FixedPayrollRow_IsSpecifiedUnitPrice = 207,
	FixedPayrollRow_Distribute = 208,
	FixedPayrollRow_Product = 209,
	
	//Employee
	Employee_EmployeeNr = 300,
	Employee_EmploymentDate = 301,
	Employee_EndDate = 302,
	Employee_Cardnumber = 303,
	Employee_CalculatedCostPerHour = 304,
	Employee_Note = 305,
	Employee_DisbursementMethod = 306,
	Employee_DisbursementClearingNr = 307,
	Employee_DisbursementAccountNr = 308,
	Employee_DontValidateDisbursementAccountNr = 309,
	Employee_ShowNote = 310,
	Employee_HighRiskProtection = 311,
	Employee_HighRiskProtectionTo = 312,
	Employee_MedicalCertificateReminder = 313,
	Employee_MedicalCertificateDays = 314,
	Employee_Absence105DaysExcluded = 315,
	Employee_Absence105DaysExcludedDays = 316,
	Employee_ExternalCode = 317,
	Employee_PayrollStatisticsPersonalCategory = 318,
	Employee_PayrollStatisticsWorkTimeCategory = 319,
	Employee_PayrollStatisticsSalaryType = 320,
	Employee_PayrollStatisticsWorkPlaceNumber = 321,
	Employee_PayrollStatisticsCFARNumber = 322,
	Employee_WorkPlaceSCB = 323,
	Employee_PartnerInCloseCompany = 324,
	Employee_BenefitAsPension = 325,
	Employee_AFACategory = 326,
	Employee_AFASpecialAgreement = 327,
	Employee_AFAWorkplaceNr = 328,
	Employee_AFAParttimePensionCode = 329,
	Employee_CollectumITPPlan = 330,
	Employee_CollectumAgreedOnProduct = 331,
	Employee_CollectumCostPlace = 332,
	Employee_CollectumCancellationDate = 333,
	Employee_CollectumCancellationDateIsLeaveOfAbsence = 334,
	Employee_WantsExtraShifts = 335,
	Employee_KPARetirementAge = 336,
	Employee_KPABelonging = 337,
	Employee_KPAEndCode = 338,
	Employee_DontNotifyChangeOfDeviations = 339,
	Employee_DontNotifyChangeOfAttestState = 340,
	Employee_BygglosenAgreementArea = 341,
	Employee_BygglosenAllocationNumber = 342,
	Employee_BygglosenMunicipalCode = 343,
	Employee_BygglosenSalaryFormula = 344,
	Employee_KPAAgreementType = 345,
	Employee_DisbursementCountryCode = 346,
	Employee_DisbursementBIC = 347,
	Employee_DisbursementIBAN = 348,
	
	//User
	User_DefaultRoleId = 350,
	User_DefaultActorCompanyId = 351,
	User_LangId = 352,
	User_DepartmentId = 353,
	User_LoginName = 354,
	User_Name = 355,
	User_Email = 356,
	User_EmailCopy = 357,
	User_ExternalAuthId = 358,
	User_LifetimeSeconds = 359,
	User_UserLinkConnectionKey = 360,
	
	//ContactPerson
	ContactPerson_FirstName = 400,
	ContactPerson_LastName = 401,
	ContactPerson_Position = 402,
	ContactPerson_Description = 403,
	ContactPerson_SocialSec = 404,
	ContactPerson_Sex = 405,
	
	//ContactEcom
	ContactEcom_Name = 450,
	ContactEcom_Text = 451,
	ContactEcom_Description = 452,
	ContactEcom_IsSecret = 453,
	
	//ContactPersonAddress
	ContactPerson_Address = 500,
	
	// UserCompanySetting
	UserCompanySetting_String = 550,
	UserCompanySetting_Integer = 551,
	UserCompanySetting_Decimal = 552,
	UserCompanySetting_Boolean = 553,
	UserCompanySetting_Date = 554,
	
	// EventHistory
	EventHistory_UserId = 600,
	EventHistory_StrData = 601,
	EventHistory_IntData = 602,
	EventHistory_DecimalData = 603,
	EventHistory_BoolData = 604,
	EventHistory_DateData = 605,
	
	//Supplier
	Supplier_Name = 700,
	Supplier_OrgNr = 701,
	Supplier_VatNr = 702,
	Supplier_PaymentCondition = 703,
	Supplier_Nr = 704,
	
	//Inventory
	Inventory_Nr = 750,
	Inventory_Name = 751,
	Inventory_Description = 752,
	Inventory_Status = 753,
	Inventory_PurchaseDate = 754,
	Inventory_WriteOffDate = 755,
	Inventory_PurchaseAmount = 756,
	Inventory_WriteOffAmount = 757,
	Inventory_WriteOffSum = 758,
	Inventory_WriteOffPeriods = 759,
	Inventory_WriteOffRemainingAmount = 760,
	Inventory_EndAmount = 761,
	Inventory_PeriodType = 762,
	Inventory_PeriodValue = 763,
	Inventory_State = 764,
	Inventory_Company = 765,
	Inventory_WriteOffMethod = 766,
	Inventory_VoucherSeries = 767,
	
	//Payment
	Payment_PaymentNr = 800,
	Payment_BIC = 801,
	Payment_PaymentForm = 802,
	Payment_PaymentMethod = 803,
	Payment_ChargeCode = 804,
	Payment_IntermediaryCode = 805,
	Payment_PaymentCode = 806,
	Payment_ClearingCode = 807,
	Payment_CurrencyAccount = 808,
	
	// UserCompanyRole
	UserCompanyRole_Role = 900,
	UserCompanyRole_DateFrom = 901,
	UserCompanyRole_DateTo = 902,
	UserCompanyRole_Default = 903,
	
	// AttestRoleUser
	AttestRoleUser_AttestRole = 950,
	AttestRoleUser_DateFrom = 951,
	AttestRoleUser_DateTo = 952,
	AttestRoleUser_MaxAmount = 953,
	AttestRoleUser_Account = 954,
	AttestRoleUser_IsExecutive = 955,
	AttestRoleUser_AccountPermissionType = 956,
	AttestRoleUser_IsNearestManager = 957,
	
	// Role
	Role_Name = 1000,
	Role_ExternalCodes = 1001,
	Role_FavoriteOption = 1002,
	Role_Permission = 1003,
	Role_State = 1004,
	Role_Sort = 1005,
	
	// AttestRole
	AttestRole_Name = 1050,
	AttestRole_Description = 1051,
	AttestRole_ExternalCodes = 1052,
	AttestRole_DefaultMaxAmount = 1053,
	AttestRole_ShowUncategorized = 1054,
	AttestRole_ShowAllCategories = 1055,
	AttestRole_ShowAllSecondaryCategories = 1056,
	AttestRole_ShowTemplateSchedule = 1057,
	AttestRole_AlsoAttestAdditionsFromTime = 1058,
	AttestRole_HumanResourcesPrivacy = 1059,
	AttestRole_AttestByEmployeeAccount = 1060,
	AttestRole_StaffingByEmployeeAccount = 1061,
	AttestRole_ReminderAttestStateId = 1062,
	AttestRole_ReminderNoOfDays = 1063,
	AttestRole_ReminderPeriodType = 1064,
	AttestRole_AttestTransition = 1065,
	AttestRole_PrimaryCategory = 1066,
	AttestRole_SecondaryCategory = 1067,
	AttestRole_Sort = 1068,
	AttestRole_AllowToAddOtherEmployeeAccounts = 1069,
	
	// EmployeeAccount
	EmployeeAccount_Account = 1100,
	EmployeeAccount_DateFrom = 1101,
	EmployeeAccount_DateTo = 1102,
	EmployeeAccount_Default = 1103,
	
	// EmployeeRequest
	EmployeeRequest_Type = 1150,
	EmployeeRequest_Start = 1151,
	EmployeeRequest_Stop = 1152,
	EmployeeRequest_Comment = 1153,
	
	EmployeeSetting_AreaType = 1200,
	EmployeeSetting_GroupType = 1201,
	EmployeeSetting_Type = 1202,
	EmployeeSetting_ValidFromDate = 1203,
	EmployeeSetting_ValidToDate = 1204,
	EmployeeSetting_Value = 1205,
	
	// TimeStampEntry
	TimeStampEntry_Time = 1250,
	TimeStampEntry_TimeDeviationCauseId = 1251,
	TimeStampEntry_AccountId = 1252,
	TimeStampEntry_TimeBlockDateId = 1253,
	TimeStampEntry_Type = 1254,
	TimeStampEntry_Note = 1255,
	TimeStampEntry_EmployeeChildId = 1256,
	TimeStampEntry_TimeTerminalAccountId = 1257,
	TimeStampEntry_AutoStampOut = 1258,
	
	// TimeStampEntryExtended
	TimeStampEntryExtended_TimeScheduleTypeId = 1275,
	TimeStampEntryExtended_TimeCodeId = 1276,
	TimeStampEntryExtended_AccountId = 1277,
	TimeStampEntryExtended_Quantity = 1278,
	
	// TimeScheduleTemplateBlock
	TimeScheduleTemplateBlock_ShiftStatus = 1300,
	TimeScheduleTemplateBlock_ShiftUserStatus = 1301,
	TimeScheduleTemplateBlock_Planning = 1302,
	TimeScheduleTemplateBlock_ShiftType = 1303,
	TimeScheduleTemplateBlock_Employee = 1304,
	TimeScheduleTemplateBlock_StartTime = 1305,
	TimeScheduleTemplateBlock_StopTime = 1306,
	TimeScheduleTemplateBlock_ExtraShift = 1307,
	TimeScheduleTemplateBlock_TimeDeviationCause = 1308,
	
	// Parent
	Employee_Parent = 1400,
	
}

export enum TermGroup_PersonalDataActionType {
	Unspecified = 0,
	Read = 1,
	Modify = 2,
}

export enum TermGroup_PersonalDataInformationType {
	Unspecified = 0,
	
	//Employee
	SocialSec = 1,
	EmployeeMeeting = 2,
	IllnessInformation = 3,
	ParentalLeaveAndChild = 4,
	SalaryDistress = 5,
	Unionfee = 6,
	VehicleInformation = 7,
	Ecom = 8,
	Address = 9,
	ClosestRelative = 10,
	
	
	//Actor
	PrivateCustomer = 101,
	PrivateSupplier = 102,
	
	//Household Deduction
	HouseholdDeduction = 200
}

export enum TermGroup_TrackChangesActionMethod {
	Unspecified = 0,
	CommonInsert = 1,
	CommonUpdate = 2,
	CommonDelete = 3,
	
	Employee_Save = 4,
	Employee_CsrInquiry = 5,
	Employee_Import = 13,
	Employee_CreateVacant = 14,
	Employee_FromTerminal = 15,
	
	PayrollReview = 6,
	
	FixedPayrollRow = 7,
	
	User_Save = 8,
	
	DeleteEmployee_Inactivate = 9,
	DeleteEmployee_RemoveInfo = 10,
	DeleteEmployee_Unidentify = 11,
	DeleteEmployee_Delete = 12,
	
	UserCompanySetting_Save = 16,
	
	TimeStampEntry_Save_AttestTime = 17,
	TimeStampEntry_Save_MyTime = 18,
	TimeStampEntry_Save_Adjust = 19,
	TimeStampEntry_Job = 20,
}

export enum TermGroup_ScanningReferenceTargetField {
	Unspecified = 0,
	Costplace = 1,
	Project = 2,
	Order = 3,
}

export enum TermGroup_ScanningCodeTargetField {
	Unspecified = 0,
	OrderNumber = 1,
	BillingProject = 2,
	AccountingProject = 3,
	Costplace = 4,
}

export enum TermGroup_PersonalDataType {
	Unknown = 0,
	Employee = 1,
	ContactPerson = 2,
	Customer = 3,
	Supplier = 4,
	HouseholdApplicant = 5,
	User = 6,
}

export enum TermGroup_OrderPlanningShiftInfo {
	NoInfo = 0,
	ShiftType = 1,
	CustomerName = 2,
	DeliveryAddress = 3
}

export enum TermGroup_AssignmentTimeAdjustmentType {
	OneDay = 0,
	FillToZeroRemaining = 1,
	FillToEndDate = 2,
}

export enum TermGroup_TemplateScheduleActivateFunctions {
	NewPlacement = 1,
	ChangeStopDate = 2
}

export enum TermGroup_ExpenseType {
	Unknown = 0,
	Mileage = 1,
	AllowanceDomestic = 2,
	AllowanceAbroad = 3,
	Expense = 4,
	TravellingTime = 5,
	Time = 6,
}

export enum TermGroup_SysReportTemplateTypeGroup {
	None = 0,
	Employee = 1,
	Schedule = 2,
	Time = 3,
	Payroll = 4,
	HR = 5,
	Registry = 6,
	Accounting = 7,
	AccountsRecievable = 8,
	AccountsPayable = 9,
	Sales = 10,
	Project = 11,
	Stock = 12,
	Purchase = 13,
	Inventory = 14,
}

export enum TermGroup_TimeReportType {
	Stamp = 0,
	Deviation = 1,
	ERP = 2,
	TimeImport = 3,
}

export enum TermGroup_QualifyingDayCalculationRule {
	UseWorkTimeWeek = 0,
	UseWorkTimeWeekPlusExtraShifts = 1,
	UseWorkTimeWeekPlusAdditionalContract = 2,
	UseAverageCalculationInTimePeriod = 3,
}

export enum TermGroup_TimeSchedulePlanningFollowUpCalculateBy {
	Budget = 1,
	Forecast = 2
}

export enum TermGroup_PerformanceTestInterval {
	All = 0,
	Hour = 1,
	Day = 2
}

export enum TermGroup_TaskWatchLogResultCalculationType {
	Total = 0,
	Record = 1
}

export enum TermGroup_RecalculateTimeHeadStatus {
	Started = 0,
	Unprocessed = 1,
	UnderProcessing = 2,
	Processed = 3,
	Error = 4,
	Cancelled = 5
}

export enum TermGroup_RecalculateTimeRecordStatus {
	Waiting = 0,
	Unprocessed = 1,
	UnderProcessing = 2,
	Processed = 3,
	Error = 4,
	Cancelled = 5,
	
	None = 100, //Only used for attest for TimeScheduleTemplateBlock without RecalculateTimeRecord
}

export enum VoucherRowMergeType {
	DoNotMerge = 0,
	Merge = 1,
	MergeDebitCredit = 2,
}

export enum TermGroup_VoucherHeadSourceType {
	Unknown = 0,
	Payroll = 1,
	MergeDebitCredit = 2,
	PayrollVacation = 3,
}

export enum TermGroup_InformationSeverity {
	Information = 1,
	Important = 2,
	Emergency = 3
}

export enum TermGroup_InformationStickyType {
	CanHide = 0,            // If hidden, will not show again
	CanHideTemporary = 1,   // If hidden, will show on page reload
	CanNotHide = 2          // Unable to hide
	
}

export enum TermGroup_MassRegistrationImportType {
	Unknown = 0,
	Excel = 1,
	SoftOneClassic = 2,
	PaXml = 3,
}

export enum TermGroup_TimeScheduleScenarioHeadSourceType {
	Empty = 0,
	Schedule = 1,
	Template = 2,
	Scenario = 3
}

export enum TermGroup_OrderEdiTransferMode {
	None = 0,
	Disable = 1,
	SetPricesToZero = 2,
}

export enum TermGroup_FileDisplaySortBy {
	Description = 1,
	Created = 2
}

export enum TermGroup_IncludeExpenseInReportType {
	None = 0,
	All = 1,
	OnlyInvoiced = 2,
}

export enum KpaAgreementType {
	Unknown = 0,
	PFA01 = 1,
	PFA98 = 2,
	KAP_KL = 6,
	AKAP_KL = 8,
	AKAP_KR = 13,
}

export enum KpaBelonging {
	Unknown = 0,
	BEA = 1,
	PAN = 2,
	Medstud = 3,
}

export enum KpaEndCode {
	U1 = 1,
	U3 = 2,
	US = 3,
	UD = 4,
}

export enum TermGroup_ApiMessageType {
	Unkown = 0,
	Employee = 1,
	ActiveSchedule = 2
}

export enum TermGroup_ApiMessageStatus {
	Unknown = 0,
	Initialized = 1,
	Processing = 2,
	Processed = 3,
	Verified = 4,
	
	FailedValidation = 11,
	Error = 12,
}

export enum TermGroup_ApiMessageChangeType {
	Unknown = 0,
	Employee = 1,
	Employment = 2,
}

export enum TermGroup_PrognoseInterval {
	None = 0,
	Month = 1,
	Quarter = 2,
	Season = 3,
	Year = 4,
}

export enum TermGroup_ApiMessageSourceType {
	API = 0,
	APIManual = 1,
	APIManualOnlyLogging = 2,
	UnitTest = 3,
	MassUpdateEmployeeFields = 4,
	EmployeeTemplate = 5,
	
	//Only used in querys
	AllAPI = 101,
}

export enum TermGroup_GoTimeStampIdentifyType {
	OnlyTag = 1,
	TagAndEmployeeNumber = 2,
	OnlyEmployeeNumber = 3,
	EmployeeNumberAndTag = 4,
}

export enum TermGroup_EventHistoryType {
	Unspecified = 0,
	CollectumNyAnmalan = 1,
	CollectumLoneAndring = 2,
	ByggLosenNyckel = 3,
	Kronofogden = 4,
	KPA_EmployeeKpaDirektAnstallningKap = 5,
	KPA_EmployeeKpaDirektAnstallningPfa = 6,
	KPA_EmployeeKpaOverenskommenLon = 7,
	GTP_Folksam = 8,
	CollectumAvAnmalan = 9,
	ForaFokId = 10,
	ForaChoice1 = 11,
	ForaChoice2 = 12,
}

export enum TermGroup_AttestRoleUserAccountPermissionType {
	Complete = 0,
	Secondary = 1,
	ReadOnly = 2
}

export enum TermGroup_HouseHoldTaxDeductionType {
	None = 0,
	ROT = 1,
	RUT = 2,
	GREEN = 3
}

export enum TermGroup_AdjustQuantityByBreakTime {
	None = 0,
	Add = 1,
	Remove = 2,
}

export enum TermGroup_MobileHouseHoldTaxDeductionType {
	None = 0,
	ROT = 1,
	RUT = 2,
	SolarPanels = 3,
	EneryStorage = 4,
	ChargePoint = 5,
	ROT_50 = 6
}

export enum TermGroup_TimeIntervalPeriod {
	Day = 1,
	Week = 7,
	Month = 30,
	Quarter = 90,
	HalfYear = 180,
	Year = 365
}

export enum TermGroup_TimeIntervalStart {
	BeginningOfPeriod = 0,
	CurrentTime = 1
}

export enum TermGroup_TimeIntervalStop {
	EndOfPeriod = 0,
	CurrentTime = 1
}

export enum TermGroup_ScheduledJobLogStatus {
	Started = 1,
	Running = 2,
	Aborted = 3,
	Finished = 4
}

export enum TermGroup_AverageSalaryCostChartSeriesType {
	Men = 1,
	Women = 2,
	Total = 3,
	Median = 4
}

export enum TermGroup_VacationBalance {
	None = 0,
	RemainingDays = -1, //(Återstående semester)
	RemainingDaysPaid = -2, //(Betald semester)
	RemainingDaysUnpaid = -3, //(Obetald semester)
	RemainingDaysAdvance = -4, //(Förskottssemester)
	RemainingDaysYear1 = -5, //(Sparad semester år 1)
	RemainingDaysYear2 = -6, //(Sparad semester år 2)
	RemainingDaysYear3 = -7, //(Sparad semester år 3)
	RemainingDaysYear4 = -8, //(Sparad semester år 4)
	RemainingDaysYear5 = -9, //(Sparad semester år 5)
	RemainingDaysOverdue = -10, //(Förfallen semester)
}

export enum TermGroup_TimeStampEntryStatus {
	New = 0,                    // Entry not processed
	Processing = 1,             // Entry currently being processed
	Processed = 2,              // Entry processed, time block created
	Partial = 3,                // A new entry will revert old processed to partial
	ProcessedWithNoResult = 4   // Entry processed several times without creating any time blocks
}

export enum TermGroup_TimeStampEntryOriginType {
	Unknown = 0,
	AutoStampOutJob = 1,
	WebByEmployee = 10,
	WebByAdmin = 11,
	TerminalUnspecified = 20,
	TerminalByEmployeeNumber = 21,
	TerminalByCardNumber = 22,
	
	// Duplicate exists in the GoTimeStamp project!
}

export enum TermGroup_PayrollImportEmployeeScheduleStatus {
	Unprocessed = 0,
	Error = 1,
	Processed = 2,
}

export enum TermGroup_PayrollImportEmployeeTransactionType {
	Unknown = 0,
	PayrollProduct = 1,
	SalaryAddition = 2,
	DeviationCause = 3,
	Expense = 4,
}

export enum TermGroup_PayrollImportEmployeeTransactionStatus {
	Unprocessed = 0,
	Error = 1,
	Processed = 2,
}

export enum TermGroup_PayrollImportHeadType {
	Unknown = 0,
	File = 1,
	API = 2,
}

export enum TermGroup_PayrollImportHeadFileType {
	Unknown = 0,
	SoftOneClassic = 1,
	TimeRegistrationInformation = 2,
	API = 3,
	PaXml = 4
}

export enum TermGroup_VacationYearEndHeadContentType {
	VacationGroup = 0,
	Employee = 1,
}

export enum TermGroup_CustomerSpecificExports {
	None = 0,
	
	// 101 - 200 Pirat
	PirateProducts = 101,
	PirateAveragePrice = 102,
	PirateRoyaltyRates = 103,
	PiratePrinted = 104,
	PirateCostPlaces = 105,
	PirateVouchers = 106,
	
	// 201 - 300 ICA
	ICACustomerBalance = 201,
	ICACustomerBalanceMyStore = 202,
	ICACustomersMyStore = 203,
	
	// 301 - 320
	SafiloCustomerRegister = 301,
	SafiloOpenInvoices = 302,
}

export enum TermGroup_TimeTransactionMatrixColumns {
	Unknown = 0,
	EmployeeNr = 1,
	EmployeeName = 2,
	EmployeeCategoryCode = 10,
	EmployeeCategoryName = 15,
	PayrollGroup = 16,
	EmployeeGroup = 17,
	EmploymentType = 18,
	PayrollTransactionQuantity = 20,
	PayrollTransactionQuantityWorkDays = 25,
	PayrollTransactionQuantityCalendarDays = 30,
	PayrollTransactionCalenderDayFactor = 35,
	PayrollTransactionTimeUnit = 40,
	PayrollTransactionTimeUnitName = 45,
	IsRegistrationTypeQuantity = 50,
	PayrollTransactionUnitPrice = 55,
	Ratio = 61,
	//PayrollTransactionUnitPriceCurrency = 60,
	//PayrollTransactionUnitPriceEntCurrency = 65,
	PayrollTransactionAmount = 70,
	//PayrollTransactionAmountCurrency = 75,
	//PayrollTransactionAmountEntCurrency = 80,
	PayrollTransactionVATAmount = 85,
	//PayrollTransactionVATAmountCurrency = 90,
	//PayrollTransactionVATAmountEntCurrency = 95,
	PayrollTransactionDate = 100,
	PayrollTransactionExported = 105,
	PayrollTransactionPayrollTypeLevel1 = 106,
	PayrollTransactionPayrollTypeLevel2 = 107,
	PayrollTransactionPayrollTypeLevel3 = 108,
	PayrollTransactionPayrollTypeLevel4 = 109,
	AttestStateName = 110,
	PayrollProductNumber = 115,
	PayrollProductName = 120,
	PayrollProductDescription = 125,
	PayrollProductExternalNumber = 126,
	PayrollType = 130,
	IsAbsence = 131,
	PayrollTypeName = 135,
	PayrollTypeLevel1 = 140,
	PayrollTypeLevel2 = 145,
	PayrollTypeLevel3 = 150,
	PayrollTypeLevel4 = 155,
	IsPreliminary = 160,
	IsFixed = 165,
	ScheduleTransaction = 166,
	ScheduleTransactionType = 167,
	IsManuallyAddedPayroll = 170,
	IsManuallyAddedTime = 175,
	CurrencyName = 180,
	CurrencyCode = 185,
	Note = 190,
	Formula = 195,
	FormulaExtracted = 200,
	FormulaNames = 205,
	FormulaOrigin = 210,
	PayrollCalculationPerformed = 215,
	Created = 220,
	Modified = 225,
	CreatedBy = 230,
	ModfiedBy = 235,
	TimeCodeName = 240,
	TimeCodeCode = 245,
	TimeRuleName = 250,
	AccountNr = 255,
	AccountName = 260,
	AccountInternalNrs = 265,
	AccountInternalNames = 270,
	StartTime = 400,
	StopTime = 401,
	UserName = 500,
	ExternalAuthId = 501,
	EmployeeExternalCode = 502,
	EmployeeAccountInternalNrs = 600,
	EmployeeAccountInternalNames = 601,
	EmployeeHierachicalAccountInternalNrs = 610,
	EmployeeHierachicalAccountInternalNames = 611,
}

export enum TermGroup_PayrollTransactionMatrixColumns {
	Unknown = 0,
	EmployeeNr = 1,
	EmployeeName = 2,
	EmployeeFirstName = 3,
	EmployeeLastName = 4,
	EmployeeSocialSec = 5,
	EmployeeSex = 6,
	EmployeeDisbursementClearingNr = 10,
	EmployeeDisbursementAccountNr = 11,
	EmployeeDisbursementCountryCode = 12,
	EmployeeDisbursementBIC = 13,
	EmployeeDisbursementIBAN = 14,
	HighRiskProtection = 15,
	HighRiskProtectionTo = 16,
	MedicalCertificateReminder = 20,
	MedicalCertificateDays = 21,
	Note = 25,
	SSG = 40,
	TimeBlockDate = 100,
	PaymentDate = 105,
	AttestState = 110,
	PayrollProductName = 115,
	PayrollProductNumber = 120,
	PayrollProductDescription = 125,
	TimeCodeName = 130,
	TimeCodeNumber = 135,
	TimeCodeDescription = 140,
	TimeBlockStartTime = 145,
	TimeBlockStopTime = 150,
	ScheduleTransaction = 151,
	ScheduleTransactionType = 152,
	SysPayrollTypeLevel1 = 155,
	SysPayrollTypeLevel2 = 160,
	SysPayrollTypeLevel3 = 165,
	SysPayrollTypeLevel4 = 170,
	UnitPrice = 175,
	Amount = 195,
	VatAmount = 215,
	Ratio = 231,
	Quantity = 235,
	QuantityWorkDays = 240,
	QuantityCalendarDays = 245,
	CalenderDayFactor = 250,
	TimeUnit = 255,
	TimeUnitName = 260,
	ManuallyAdded = 265,
	AutoAttestFailed = 270,
	Exported = 275,
	IsPreliminary = 280,
	IsRetroactive = 285,
	Formula = 290,
	FormulaPlain = 295,
	FormulaExtracted = 300,
	FormulaNames = 305,
	FormulaOrigin = 310,
	WorkTimeWeek = 315,
	EmployeeGroupName = 320,
	EmployeeGroupWorkTimeWeek = 325,
	PayrollGroupName = 335,
	PayrollCalculationPerformed = 340,
	Created = 345,
	Modified = 350,
	Comment = 355,
	CreatedBy = 360,
	ModifiedBy = 365,
	Pensioncompany = 370,
	Vacationsalarypromoted = 371,
	Unionfeepromoted = 372,
	WorkingTimePromoted = 373,
	AccountString = 450,
	AccountNr = 455,
	AccountName = 460,
	AccountInternalNrs = 465,
	AccountInternalNames = 470,
}

export enum TermGroup_EmployeeListMatrixColumns {
	Unknown = 0,
	EmployeeId = 1,
	EmployeeNr = 10,
	EmployeeName = 15,
	FirstName = 16,
	LastName = 17,
	EmployeeExternalCode = 18,
	ExternalAuthId = 19,
	SocialSec = 20,
	Gender = 25,
	Age = 26,
	BirthDate = 27,
	EmployeeGroupName = 30,
	UserName = 35,
	Language = 40,
	DefaultCompany = 45,
	DefaultRole = 50,
	IsMobileUser = 55,
	IsSysUser = 65,
	
	EmployeeCalculatedCostPerHour = 70,
	Note = 75,
	HasSecondaryEmployment = 79,
	EmploymentDate = 80,
	FirstEmploymentDate = 81,
	EndDate = 85,
	EmploymentTypeName = 86,
	WorkTimeWeekMinutes = 87,
	WorkTimeWeekPercent = 88,
	WorkPlace = 89,
	LASDays = 90,
	Salary = 95,
	MonthlySalary = 96,
	HourlySalary = 97,
	DisbursementMethodText = 100,
	DisbursementClearingNr = 105,
	DisbursementAccountNr = 110,
	DisbursementCountryCode = 111,
	DisbursementBIC = 112,
	DisbursementIBAN = 113,
	SSYKCode = 115,
	SSYKName = 120,
	PositionCode = 121,
	PositionName = 122,
	PositionSysName = 123,
	ExternalCode = 124,
	PayrollStatisticsPersonalCategory = 125,
	PayrollStatisticsWorkTimeCategory = 130,
	PayrollStatisticsSalaryType = 135,
	PayrollStatisticsWorkPlaceNumber = 140,
	PayrollStatisticsCFARNumber = 145,
	WorkPlaceSCB = 150,
	PartnerInCloseCompany = 155,
	BenefitAsPension = 160,
	AFACategory = 165,
	AFASpecialAgreement = 170,
	AFAWorkplaceNr = 175,
	AFAParttimePensionCode = 180,
	CollectumITPPlan = 185,
	CollectumAgreedOnProduct = 190,
	CollectumCostPlace = 195,
	CollectumCancellationDate = 200,
	CollectumCancellationDateIsLeaveOfAbsence = 205,
	KPARetirementAge = 210,
	KPABelonging = 215,
	KPAEndCode = 220,
	KPAAgreementType = 225,
	BygglosenAgreementArea = 230,
	BygglosenAllocationNumber = 235,
	BygglosenMunicipalCode = 240,
	BygglosenAgreedHourlyPayLevel = 241,
	BygglosenSalaryFormula = 245,
	BygglosenProfessionCategory = 246,
	BygglosenSalaryType = 247,
	BygglosenWorkPlaceNumber = 248,
	BygglosenLendedToOrgNr = 249,
	GTPAgreementNumber = 250,
	GTPExcluded = 255,
	AGIPlaceOfEmploymentAddress = 260,
	AGIPlaceOfEmploymentCity = 261,
	AGIPlaceOfEmploymentIgnore = 262,
	IFAssociationNumber = 271,
	IFPaymentCode = 272,
	IFWorkPlace = 273,
	Created = 290,
	CreatedBy = 291,
	Modified = 295,
	ModifiedBy = 296,
	Email = 300,
	CellPhone = 400,
	HomePhone = 401,
	ClosestRelative = 402,
	DistributionAddress = 403,
	DistributionAddressRow = 404,
	DistributionAddressRow2 = 405,
	DistributionZipCode = 406,
	DistributionCity = 407,
	ExcludeFromPayroll = 420,
	Vacant = 425,
	PayrollGroupName = 430,
	VacationGroupName = 435,
	VacationDaysPaidByLaw = 440,
	AccountNr = 455,
	AccountName = 460,
	AccountInternalNrs = 507,
	AccountInternalNames = 508,
	CategoryName = 600,
	ExtraFieldEmployee = 700,
	NearestExecutiveUserName = 710,
	NearestExecutiveName = 711,
	NearestExecutiveEmail = 713,
	NearestExecutiveSocialSec = 714,
	NearestExecutiveCellphone = 715,
	EmploymentTypeOnSecondaryEmployment = 720,
	EmplymentExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment = 721,
	EmplymentTypeExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment = 722,
}

export enum TermGroup_CompanyExternalCodeEntity {
	Unknown = 0,
	Customer = 1,
	Supplier = 2,
	EmployeeGroup = 100,
	PayrollGroup = 101,
	VacationGroup = 102,
	AttestRole = 200,
	Role = 201,
	PayrollProduct = 300,
	AccountHierachyPayrollExport = 310,
	AccountHierachyPayrollExportUnit = 311,
	// TimeDeviationCode = 310,
	InboundEmailMessageEmail = 400,
	ExtraField = 500,
	CustomerContact_InexchangeCompanyId = 600,
	Customer_InexchangeCompanyId = 601
}

export enum TermGroup_TimeTerminalAttendanceViewSortOrder {
	Name = 0,
	Time = 1
}

export enum TermGroup_PayrollImportHeadStatus {
	Unprocessed = 0,
	Error = 1,
	Processed = 2,
	PartlyProcessed = 3
}

export enum TermGroup_PayrollImportEmployeeStatus {
	Unprocessed = 0,
	Error = 1,
	Processed = 2,
	PartlyProcessed = 3
}

export enum TermGroup_ScheduleTransactionMatrixColumns {
	Unknown = 0,
	EmployeeNr = 1,
	EmployeeName = 2,
	SocialSec = 3,
	Date = 10,
	StartTime = 20,
	StopTime = 25,
	NetMinutes = 50,
	GrossMinutes = 51,
	NetHours = 55,
	NetHoursString = 56,
	GrossHours = 57,
	GrossHoursString = 58,
	NetCost = 65,
	GrossCost = 66,
	IsBreak = 70,
	ExtraShift = 71,
	SubstituteShift = 72,
	IsPreliminary = 73,
	Description = 75,
	SubstituteShiftCalculated = 80,
	EmploymentPercent = 81,
	EmployeeGroup = 82,
	ShiftTypeName = 100,
	ShiftTypeScheduleTypeName = 102,
	ScheduleTypeName = 120,
	Created = 220,
	Modified = 225,
	CreatedBy = 230,
	ModifiedBy = 235,
	TimeCodeName = 240,
	TimeCodeCode = 245,
	TimeRuleName = 250,
	AccountNr = 255,
	AccountName = 260,
	AccountInternalNr1 = 265,
	AccountInternalName1 = 270,
	AccountInternalNr2 = 275,
	AccountInternalName2 = 280,
	AccountInternalNr3 = 285,
	AccountInternalName3 = 290,
	AccountInternalNr4 = 295,
	AccountInternalName4 = 300,
	AccountInternalNr5 = 305,
	AccountInternalName5 = 306,
	AccountInternalNrs = 507,
	AccountInternalNames = 508,
	ExtraFieldEmployee = 700,
}

export enum TermGroup_TimeCodeRuleType {
	Unknown = 0,
	TimeCodeEarlierThanStart = 1,
	TimeCodeLaterThanStop = 2,
	TimeCodeLessThanMin = 3,
	TimeCodeBetweenMinAndStd = 4,
	TimeCodeStd = 5,
	TimeCodeBetweenStdAndMax = 6,
	TimeCodeMoreThanMax = 7,
	
	AdjustQuantityOnTime = 11,
	AdjustQuantityOnScheduleInNextDay = 12,
	
	//only used in code
	AutogenBreakOnStamping = 101,
}

export enum TermGroup_EmployeeDateMatrixColumns {
	EmployeeNr = 1,
	EmployeeName = 2,
	Date = 3,
	DateTypeName = 4,
	EmployeeId = 5,
	EmployeeGroupName = 10,
	PayrollGroupName = 20,
	VacationGroupName = 30,
	EmploymentPercent = 100,
	EmploymentFte = 101,
	ScheduleTime = 200,
	ScheduleAbsenceTime = 201,
	PercentScheduleAbsenceTime = 202,
}

export enum TermGroup_TimeStampMatrixColumns {
	Unknown = 0,
	EmployeeNr = 1,
	EmployeeName = 2,
	AccountName = 20,
	Time = 30,
	OriginalTime = 32,
	Date = 35,
	IsBreak = 40,
	ShiftTypeName = 50,
	ScheduleTypeName = 55,
	DeviationCauseName = 60,
	TypeName = 70,
	Note = 80,
	OriginType = 90,
	TimeTerminalName = 100,
	Status = 110,
	IsRemoved = 120,
	Created = 900,
	CreatedBy = 901,
	Modified = 905,
	ModifiedBy = 906
}

export enum TermGroup_EmployeeSelectionAccountingType {
	EmployeeCategory = 0,
	EmployeeAccount = 1,
	EmploymentAccountInternal = 2,
	TimeScheduleTemplateBlock = 3,
	TimeScheduleTemplateBlockAccount = 4,
	TimePayrollTransactionAccount = 5
}

export enum TermGroup_TimeAccumulatorCompareModel {
	Unknown = 0,
	SelectedRange = 1,
	AccToday = 2,
}

export enum TermGroup_ReportUserSelectionAccessType {
	Private = 0,
	Public = 1,
	Role = 2,
	MessageGroup = 3
}

export enum TermGroup_PurchaseOrderSortOrder {
	PurchaseNr = 1,
	SupplierNr = 2
}

export enum TermGroup_PurchaseCartStatus {
	Open = 1,
	Transferred = 2,
	Closed = 3
}

export enum TermGroup_PurchaseCartPriceStrategy {
	Unknown = 1,
	WholesalerPriceList = 2,
	CheapestPriceList = 3,
	CustomerPriceList = 4,
}

export enum TermGroup_InvoiceRowImportType {
	Excel = 1,
	Jcad = 2,
}

export enum TermGroup_TimeWorkReductionPeriodType {
	Week = 1,
}

export enum TermGroup_ApiSettingType {
	Uknown = 0,
	
	//Employment
	KeepEmploymentAccount = 1,
	KeepEmploymentPriceType = 2,
	
	//Flags
	DoNotSetChiefToStandard = 101,
	DoNotCloseEmployeeAccountAndAttestRole = 102,
	AccountDimIdForEmployeeAccount = 201,
	DoSetDefaultCompanyWhenUpdatingRoles = 401,
}

export enum TermGroup_DataStorageRecordAttestStatus {
	None = 0,
	Initialized = 1,
	PartlySigned = 2,
	Signed = 3,
	Rejected = 4,
	Cancelled = 5
}

export enum TermGroup_UserMatrixColumns {
	Unknown = 0,
	LoginName = 1,
	Name = 2,
	Email = 3,
	EmployeeNr = 5,
	Roles = 10,
	AttestRoles = 11,
	AttestRoleAccount = 12,
	DateCreated = 15,
	DateModified = 16,
	CreatedBy = 17,
	ModifiedBy = 18,
	IsActive = 20,
	IsMobileUser = 25,
	LastLogin = 30,
	RoleDateFrom = 40,
	RoleDateTo = 41,
	AttestRoleDateFrom = 42,
	AttestRoleDateTo = 43,
	ShowAllCategories = 50,
	ShowUncategorized = 51,
	AttestRoleAccountName = 60,
}

export enum TermGroup_SupplierMatrixColumns {
	Unknown = 0,
	SupplierName = 1,
	SupplierNr = 2,
	SupplierOrgNr = 3,
	SupplierVatNr = 4,
	Country = 5,
	Currency = 6,
	OurCustomerNr = 7,
	Reference = 8,
	VatType = 9,
	PaymentCondition = 10,
	FactoringSupplier = 11,
	BIC = 12,
	StopPayment = 13,
	EDISupplier = 14,
	DefaultPaymentInformation = 15,
	PhoneJob = 16,
	Email = 17,
	Web = 18,
	Fax = 19,
	DeliveryAddress = 20,
	DeliveryAddressStreet = 21,
	DeliveryAddressCO = 22,
	DeliveryAddressPostalCode = 23,
	DeliveryAddressPostalAddress = 24,
	DistributionAddress = 30,
	DistributionAddressStreet = 31,
	DistributionAddressCO = 32,
	DistributionAddressPostalCode = 33,
	DistributionAddressPostalAddress = 34,
	DistributionAddressCountry = 35,
	VisitingAddress = 40,
	VisitingAddressStreet = 41,
	VisitingAddressCO = 42,
	VisitingAddressPostalCode = 43,
	VisitingAddressPostalAddress = 44,
	VisitingAddressCountry = 45,
	IsActive = 50,
	Bankgiro = 51,
	Plusgiro = 52,
	Cfp = 53,
	Sepa = 54,
	InvoiceAddress = 55,
	InvoiceAddressStreet = 56,
	InvoiceAddressCO = 57,
	InvoiceAddressPostalCode = 58,
	InvoiceAddressPostalAddress = 59,
	InvoiceAddressCountry = 60,
	
}

export enum TermGroup_InvoiceProductMatrixColumns {
	Unknown = 0,
	ProductNr = 1,
	ProductName = 2,
	Description = 3,
	IsActive = 4,
	IsImported = 5,
	ProductType = 10,
	CalculationType = 11,
	ProductGroupCode = 13,
	ProductGroupName = 14,
	ProductCategoryNames = 15,
	ProductUnitName = 16,
	ProductEAN = 20,
	VatCodeName = 25,
	HouseholdDeductionType = 29,
	HouseholdDeductionPercentage = 30,
	Weight = 40,
	SalesAmount = 100,
	SalesQuantity = 101,
}

export enum TermGroup_CustomerMatrixColumns {
	Unknown = 0,
	CustomerName = 1,
	CustomerOrgNr = 2,
	CustomerNr = 3,
	CustomerVatNr = 4,
	Country = 5,
	Currency = 6,
	CustomerSupNr = 15,
	PhoneJob = 16,
	Email = 17,
	Web = 18,
	Fax = 19,
	DeliveryAddress = 20,
	DeliveryAddressStreet = 21,
	DeliveryAddressCO = 22,
	DeliveryAddressPostalCode = 23,
	DeliveryAddressPostalAddress = 24,
	DistributionAddress = 30,
	DistributionAddressStreet = 31,
	DistributionAddressCO = 32,
	DistributionAddressPostalCode = 33,
	DistributionAddressPostalAddress = 34,
	DistributionAddressCountry = 35,
	VisitingAddress = 40,
	VisitingAddressStreet = 41,
	VisitingAddressCO = 42,
	VisitingAddressPostalCode = 43,
	VisitingAddressPostalAddress = 44,
	VisitingAddressCountry = 45,
	IsActive = 50,
	BillingAddress = 60,
	BillingAddressStreet = 61,
	BillingAddressCO = 62,
	BillingAddressPostalCode = 63,
	BillingAddressPostalAddress = 64,
	BillingAddressCountry = 65,
	DiscountMerchandise = 70,
	InvoiceReference = 71,
	DisableInvoiceFee = 72,
	InvoiceDeliveryType = 73,
	ContactGLN = 74,
	InvoiceLabel = 75,
	PaymentCondition = 85,
	ImportInvoicesDetailed = 95,
}

export enum TermGroup_StaffingneedsFrequencyMatrixColumns {
	Unknown = 0,
	AccountName = 1,
	AccountNumber = 2,
	AccountParentName = 3,
	AccountParentNumber = 4,
	TimeFrom = 5,
	TimeTo = 6,
	NbrOfItems = 7,
	NbrOfCustomers = 8,
	NbrOfMinutes = 9,
	Amount = 10,
	Cost = 11,
	FrequencyType = 12,
	ExternalCode = 13,
	ParentExternalCode = 14,
}

export enum TermGroup_EmployeeSkillMatrixColumns {
	Unknown = 0,
	EmployeeNr = 1,
	Name = 2,
	FirstName = 3,
	LastName = 4,
	BirthYear = 5,
	Gender = 6,
	PositionName = 7,
	SSYKCode = 8,
	EmploymentTypeName = 9,
	SkillName = 10,
	SkillDate = 11,
	SkillLevel = 12,
	SkillDescription = 13,
	SkillTypeName = 20,
	SkillTypeId = 21,
	SkillTypeDescription = 22,
	CategoryName = 30,
	AccountName = 31,
}

export enum TermGroup_OrganisationHrMatrixColumns {
	Unknown = 0,
	EmployeeNr = 1,
	FirstName = 2,
	LastName = 3,
	Name = 4,
	DateFrom = 10,
	DateTo = 11,
	AccountIsPrimary = 14,
	CategoryIsDefault = 15,
	CategoryName = 20,
	CategoryGroup = 21,
	SubCategory = 22,
	AccountInternalNrs = 507,
	AccountInternalNames = 508,
	ExtraFieldAccount = 700,
}

export enum TermGroup_ShiftTypeSkillMatrixColumns {
	Unknown = 0,
	ShiftTypeName = 1,
	ShiftType = 2,
	ShiftTypeDescription = 3,
	ShiftTypeCatagory = 4,
	ScheduleTypeName = 5,
	ShiftTypeNumber = 6,
	ShiftTypeScheduleTypeName = 10,
	ShiftTypeScheduleType = 11,
	ExtternalCode = 12,
	Skill = 20,
	SkillLevel = 21,
	Accountingsettings = 30,
	AccountNr = 50,
	AccountName = 51,
}

export enum TermGroup_TimeSalaryPaymentExportBank {
	Unknown = 0,
	Handelsbanken = 1,
	Danskebank = 2,
	Nordea = 3,
	BNP = 4,
	DNB = 5,
}

export enum TermGroup_EmployeeEndReasonsMatrixColumns {
	Unknown = 0,
	EmployeeNr = 1,
	EmployeeName = 2,
	FirstName = 3,
	LastName = 4,
	BirthYear = 5,
	Gender = 6,
	DefaultRole = 10,
	SSYKCode = 11,
	EmploymentDate = 20,
	EndDate = 21,
	EmploymentTypeName = 22,
	EndReason = 30,
	Comment = 31,
	CategoryName = 32,
}

export enum TermGroup_EmployeeSalaryMatrixColumns {
	Unknown = 0,
	EmployeeNr = 1,
	EmployeeName = 2,
	FirstName = 3,
	LastName = 4,
	Gender = 5,
	EmployeeId = 6,
	EmploymentTypeName = 10,
	Position = 11,
	SalaryType = 20,
	SalaryTypeName = 21,
	SalaryTypeCode = 22,
	SalaryTypeDesc = 23,
	SalaryDateFrom = 25,
	SalaryAmount = 30,
	PayrollLevel = 40,
	ExperienceTot = 41,
	BirthYearMonth = 42,
	Age = 43,
	SalaryFromPayrollGroup = 44,
	SalaryDiff = 45,
	CategoryName = 60,
	AccordingToPayrollGroup = 70,
	IsSecondaryEmployment = 75,
	Created = 100,
	CreatedBy = 101,
	Modified = 102,
	ModifiedBy = 103,
	AccountInternalNrs = 507,
	AccountInternalNames = 508,
	ExtraFieldEmployee = 700,
}

export enum TermGroup_InsightChartTypes {
	None = 0,
	Pie = 1,
	Doughnut = 2,
	Line = 11,
	Bar = 21,
	Column = 22,
	Area = 31,
	Scatter = 41,
	Bubble = 42,
	Histogram = 51,
	Treemap = 61
}

export enum TermGroup_EmployeeTimePeriodMatrixColumns {
	Unknown = 0,
	EmployeeNr = 1,
	EmployeeName = 2,
	SocialSec = 3,
	PaymentDate = 20,
	StartDate = 21,
	StopDate = 22,
	PayrollStartDate = 23,
	PayrollStopDate = 24,
	Tax = 30,
	TableTax = 31,
	OneTimeTax = 32,
	EmploymentTaxCredit = 33,
	SupplementChargeCredit = 34,
	GrossSalary = 35,
	NetSalary = 36,
	VacationCompensation = 37,
	Benefit = 38,
	Compensation = 39,
	Deduction = 40,
	UnionFee = 41,
	OptionalTax = 42,
	SINKTax = 43,
	ASINKTax = 44,
	EmploymentTaxBasis = 45
}

export enum TermGroup_StaffingStatisticsMatrixColumns {
	Unknown = 0,
	Date = 1,
	ScheduleSales = 20,
	ScheduleHours = 21,
	SchedulePersonelCost = 22,
	ScheduleSalaryPercent = 23,
	ScheduleLPAT = 24,
	ScheduleFPAT = 25,
	ScheduleBPAT = 26,
	
	TimeSales = 30,
	TimeHours = 31,
	TimePersonelCost = 32,
	TimeSalaryPercent = 33,
	TimeLPAT = 34,
	TimeFPAT = 35,
	TimeBPAT = 36,
	
	ForecastSales = 40,
	ForecastHours = 41,
	ForecastPersonelCost = 42,
	ForecastSalaryPercent = 43,
	ForecastLPAT = 44,
	ForecastFPAT = 45,
	ForecastBPAT = 46,
	
	BudgetSales = 50,
	BudgetHours = 51,
	BudgetPersonelCost = 52,
	BudgetSalaryPercent = 53,
	BudgetLPAT = 54,
	BudgetFPAT = 55,
	BudgetBPAT = 56,
	
	TemplateScheduleSales = 60,
	TemplateScheduleHours = 61,
	TemplateSchedulePersonelCost = 62,
	TemplateScheduleSalaryPercent = 63,
	TemplateScheduleLPAT = 64,
	TemplateScheduleFPAT = 65,
	TemplateScheduleBPAT = 66,
	
	ScheduleAndTimeHours = 70,
	ScheduleAndTimePersonalCost = 71,
	
	AccountInternalNrs = 101,
	AccountInternalNames = 102,
	EmployeeName = 103,
}

export enum TermGroup_AggregatedTimeStatisticsMatrixColumns {
	Unknown = 0,
	AccountDimName = 1,
	AccountNr = 2,
	AccountName = 3,
	
	//Employee
	EmployeeNr = 10,
	EmployeeName = 11,
	EmployeePosition = 12,
	
	EmployeeWeekWorkHours = 20,
	EmployeeSalary = 21,
	EmployeeFullTimeWorkHours = 22,
	
	// Time
	WorkHoursTotal = 30,
	InconvinientWorkingHours = 32,
	InconvinientWorkingHoursLevel50 = 33,
	InconvinientWorkingHoursLevel70 = 34,
	InconvinientWorkingHoursLevel100 = 35,
	AddedTimeHours = 36,
	AddedTimeHoursLevel35 = 131,
	AddedTimeHoursLevel70 = 132,
	AddedTimeHoursLevel100 = 133,
	OverTimeHours = 37,
	OverTimeHoursLevel50 = 38,
	OverTimeHoursLevel70 = 39,
	OverTimeHoursLevel100 = 40,
	OverTimeHoursLevel35 = 41,
	SicknessHours = 42,
	VacationHours = 43,
	AbsenceHours = 45,
	InconvinientWorkingHoursLevel40 = 46,
	InconvinientWorkingHoursLevel57 = 47,
	InconvinientWorkingHoursLevel79 = 48,
	InconvinientWorkingHoursLevel113 = 49,
	
	CostTotal = 50,
	CostNetHours = 51,
	CostCalenderDayWeek = 52,
	InconvinientWorkingCost = 53,
	InconvinientWorkingCostLevel50 = 54,
	InconvinientWorkingCostLevel70 = 55,
	InconvinientWorkingCostLevel100 = 56,
	AddedTimeCost = 60,
	AddedTimeCostLevel35 = 61,
	AddedTimeCostLevel70 = 62,
	AddedTimeCostLevel100 = 63,
	OverTimeCost = 69,
	OverTimeCostLevel35 = 70,
	OverTimeCostLevel50 = 71,
	OverTimeCostLevel70 = 72,
	OverTimeCostLevel100 = 73,
	SicknessCost = 74,
	VacationCost = 75,
	AbsenceCost = 76,
	CostCalenderDay = 77,
	InconvinientWorkingCostLevel40 = 81,
	InconvinientWorkingCostLevel57 = 82,
	InconvinientWorkingCostLevel79 = 83,
	InconvinientWorkingCostLevel113 = 84,
	
	//Schedule
	ScheduleNetQuantity = 100,
	ScheduleGrossQuantity = 101,
	ScheduleNetAmount = 110,
	ScheduleGrossAmount = 111,
}

export enum TermGroup_FixedInsights {
	Custom = 1,
	
	// EmployeeList
	EmployeeList_EmploymentType = 100301,
	EmployeeList_WorkTimeWeekPercent = 100302,
	EmployeeList_WorkTimeWeekMinutes = 100303,
	EmployeeList_Gender = 100304,
	EmployeeList_WorkTimeWeekPercentByAccount = 100305,
	
	// Schedule
	Schedule_ShiftType = 100401,
	Schedule_NetHours = 100402,
	
	// EmployeeDate
	EmployeeDate_WorkTimeWeekPercent = 100501,
	
	// TimeStamp
	TimeStamp_OriginType = 100601,
	
	// User
	User_Role = 100701,
	User_AttestRole = 100702,
	
	// Supplier
	Supplier_Country = 100801,
	Supplier_VatType = 100802,
	
	// InvoiceProduct
	InvoiceProduct_ProductType = 100901,
	InvoiceProduct_VatCode = 100902,
	
	// Customer
	Customer_VisitingPostalAdressByCustomer = 101001,
	
	// EmployeeSkill
	EmployeeSkill_Skill = 101201,
	EmployeeSkill_GenderwithSkill = 101202,
	
	//ShiftTypeSkill
	ShiftTypeSkill_Skill = 101401,
	ShiftTypeSkill_ShiftTypeScheduleTypeName = 101402,
	
	// EmployeeEndReason
	EmployeeEndReasons_Endreason = 101501,
	EmployeeEndReasons_GenderwithEndreason = 101502,
	
	// EmployeeSalary
	EmployeeSalary_PayrollType = 101601,
	EmployeeSalary_GenderWithPayrollType = 101602,
	
	// EmployeeMeeting
	EmployeeMeeting_MeetingType = 102001,
	EmployeeMeeting_GenderWithMeetingType = 102002,
	
	//EmployeeExperience
	EmployeeExperience_ExperienceTot = 102201,
	
	//ReportStatisticsAnalysis
	ReportStatistics_Reports = 102501,
	ReportStatistics_Reports_Period = 102502,
}

export enum TermGroup_EmployeeMeetingMatrixColumns {
	Unknown = 0,
	EmployeeNr = 1,
	EmployeeName = 2,
	FirstName = 3,
	LastName = 4,
	Gender = 5,
	BirthDate = 6,
	EmploymentTypeName = 20,
	Position = 25,
	SSYKCode = 26,
	StartDate = 40,
	StartTime = 41,
	Completed = 42,
	Participants = 43,
	OtherParticipants = 44,
	MeetingType = 45,
	Reminder = 46,
	AccountInternalName1 = 60,
	AccountInternalName2 = 61,
	AccountInternalName3 = 62,
	AccountInternalName4 = 63,
	AccountInternalName5 = 64,
	CategoryName = 70,
	AccountInternalNrs = 71,
	AccountInternalNames = 72,
	ExtraFieldEmployee = 73
}

export enum TermGroup_MatrixGroupAggOption {
	Sum = 1,
	Min = 2,
	Max = 3,
	Count = 4,
	Average = 5,
	Median = 6,
	None = 100,
}

export enum TermGroup_MatrixDateFormatOption {
	
	DateShort = 1,          // YY-MM-DD
	DateLong = 2,           // YYYY-MM-DD
	
	DayOfMonth = 11,        // D (1-31)
	DayOfMonthPadded = 12,  // DD (01-31)
	
	DayOfYear = 21,         // DDD (1-366)
	
	WeekOfYear = 31,        // W (1-53)
	
	DayOfWeekShort = 41,    // ddd (mon)
	DayOfWeekFull = 42,     // dddd (monday)
	
	Month = 51,             // M (1-12)
	MonthPadded = 52,       // MM (01-12)
	NameOfMonthShort = 53,  // MMM (jan)
	NameOfMonthFull = 54,   // MMMM (january)
	
	Quarter = 61,           // Q (1-4)
	
	YearShort = 71,         // YY
	YearFull = 72,          // YYYY
	
	YearMonth = 81,         // YYYY-MM
	
	YearWeek = 91           // YYYY-W
}

export enum TermGroup_ScheduledTimeSummaryMatrixColumns {
	Unknown = 0,
	EmployeeNr = 1,
	EmployeeName = 2,
	Date = 3,
	Time = 4,
	Type = 5,
}

export enum TermGroup_EmployeeExperienceMatrixColumns {
	Unknown = 0,
	EmployeeNr = 1,
	EmployeeName = 2,
	EmployeeGroupName = 3,
	FirstName = 4,
	LastName = 5,
	SSN = 6,
	Age = 7,
	ExperienceIn = 10,
	ExperienceTot = 11,
	ExperienceType = 12,
	SalaryType = 20,
	SalaryDate = 21,
	Salary = 22,
	SalaryTypeName = 23,
}

export enum TermGroup_EmployeeDocumentMatrixColumns {
	Unknown = 0,
	EmployeeNr = 1,
	EmployeeName = 2,
	FirstName = 3,
	LastName = 4,
	FileName = 10,
	FileType = 11,
	Description = 12,
	Created = 15,
	NeedsConfirmation = 18,
	Confirmed = 19,
	Read = 20,
	Answered = 21,
	AnswerType = 22,
	ByMessage = 25,
	ValidFrom = 28,
	ValidTo = 29,
	AttestStatus = 35,
	AttestState = 36,
	CurrentAttestUsers = 37,
	HasSecondaryEmployment = 79,
	CategoryName = 100,
	AccountInternalName1 = 101,
	AccountInternalName2 = 102,
	AccountInternalName3 = 103,
	AccountInternalName4 = 104,
	AccountInternalName5 = 105,
	AccountInternalNames = 106,
	AccountInternalNrs = 107,
	ExtraFieldEmployee = 108,
	ExtraFieldEmployee4 = 109
	
}

export enum TermGroup_ImportDynamicFileType {
	TextSemicolonSeparated = 1,
	TextTabSeparated = 2,
	TextCommaSeparated = 3,
}

export enum TermGroup_ExtraFieldType {
	FreeText = 1,
	Integer = 2,
	Decimal = 3,
	YesNo = 4,
	Checkbox = 5,
	Date = 6,
	SingleChoice = 7,
	MultiChoice = 8
}

export enum TermGroup_EmployeeAccountMatrixColumns {
	Unknown = 0,
	EmployeeNr = 1,
	EmployeeName = 2,
	FirstName = 3,
	LastName = 4,
	EmployeeId = 5,
	FixedAccounting = 10,
	Fixed = 12,
	DateFrom = 15,
	Type = 20,
	Percent = 25,
	CategoryName = 98,
	AccountStdName = 99,
	AccountInternalStd = 100,
	AccountInternalName1 = 101,
	AccountInternalName2 = 102,
	AccountInternalName3 = 103,
	AccountInternalName4 = 104,
	AccountInternalName5 = 105,
	Default = 120,
	AccountInternalNrs = 507,
	AccountInternalNames = 508,
	ExtraFieldEmployee = 700
}

export enum TermGroup_TimeScheduleScenarioEmployeeStatus {
	None = 0,
	Initiated = 1,
	Error = 2,
	Done = 3,
}

export enum TermGroup_TimeScheduleSwapRequestStatus {
	Initiated = 0,
	Done = 1,
}

export enum TermGroup_TimeScheduleSwapRequestRowStatus {
	Initiated = 0,
	ApprovedByEmployee = 1,
	NotApprovedByEmployee = 2,
	ApprovedByAdmin = 3,
	NotApprovedByAdmin = 4,
}

export enum TermGroup_GTPAgreementNumber {
	NotSelected = 0,
	
	Fremia_HRF = 2,
	Fremia_Handels = 101,
	Fremia_Livs = 173
}

export enum TermGroup_ReportStatisticsMatrixColumns {
	Unknown = 0,
	ReportName = 1,
	SystemReportName = 2,
	AmountPrintOut = 3,
	AverageTime = 4,
	MedianTime = 5,
	Period = 6,
	AmountOfUniqueUsers = 7,
	AmountOfFailed = 8,
	Date = 9,
	DelTime = 10,
	UserId = 11,
}

export enum TermGroup_StockPurchaseGenerationOptions {
	TotalStockCompareToTriggerQuantity = 1,
	AvailableStockCompareToTriggerQuantity = 2,
	PurchaseQuantity = 3,
}

export enum TermGroup_EmployeeTemplateGroupRowType {
	Unknown = 0,
	FirstName = 1,
	LastName = 2,
	Name = 3,
	SocialSec = 4,
	EmployeeNr = 5,
	
	EmploymentStartDate = 10,
	EmploymentStopDate = 11,
	EmploymentType = 12,
	EmploymentWorkTimeWeek = 13,
	EmploymentPercent = 14,
	IsSecondaryEmployment = 15,
	EmploymentFullTimeWorkWeek = 16,
	PrimaryEmploymentWorkTimeWeek = 17,
	TotalEmploymentWorkTimeWeek = 18,
	ExcludeFromWorkTimeWeekCalculationOnSecondaryEmployment = 19,
	
	ExperienceMonths = 20,
	ExperienceAgreedOrEstablished = 21,
	VacationDaysPayed = 22,
	VacationDaysUnpayed = 23,
	TaxRate = 24,
	VacationDaysAdvance = 25,
	
	PayrollFormula = 32,
	EmploymentPriceTypes = 33,
	
	Address = 49,
	AddressRow = 50,
	AddressRow2 = 51,
	ZipCode = 52,
	City = 53,
	ZipCity = 54,
	Telephone = 55,
	Email = 56,
	
	DisbursementMethod = 57,
	DisbursementAccountNr = 58,
	DisbursementAccount = 59,
	
	HierarchicalAccount = 65,
	
	Position = 70,
	WorkTasks = 71,
	Department = 72,
	SpecialConditions = 73,
	SubstituteFor = 74,
	SubstituteForDueTo = 75,
	ExternalCode = 76,
	WorkPlace = 77,
	
	CompanyName = 99,
	CompanyOrgNr = 100,
	CompanyAddress = 101,
	CompanyAddressRow = 102,
	CompanyAddressRow2 = 103,
	CompanyZipCode = 104,
	CompanyCity = 105,
	CompanyZipCity = 106,
	CompanyTelephone = 107,
	CompanyEmail = 108,
	
	CityAndDate = 500,
	SignatureEmployee = 501,
	SignatureEmployer = 502,
	
	ExtraFieldEmployee = 600,
	ExtraFieldAccount = 601,
	
	PayrollStatisticsPersonalCategory = 701,
	PayrollStatisticsWorkTimeCategory = 702,
	PayrollStatisticsSalaryType = 703,
	PayrollStatisticsWorkPlaceNumber = 704,
	PayrollStatisticsCFARNumber = 705,
	
	ControlTaskWorkPlaceSCB = 711,
	ControlTaskPartnerInCloseCompany = 712,
	ControlTaskBenefitAsPension = 713,
	
	AFACategory = 721,
	AFASpecialAgreement = 722,
	AFAWorkplaceNr = 723,
	AFAParttimePensionCode = 724,
	
	CollectumITPPlan = 731,
	CollectumAgreedOnProduct = 732,
	CollectumCostPlace = 733,
	CollectumCancellationDate = 734,
	CollectumCancellationDateIsLeaveOfAbsence = 735,
	
	KPARetirementAge = 741,
	KPABelonging = 742,
	KPAEndCode = 743,
	KPAAgreementType = 744,
	
	BygglosenAgreementArea = 751,
	BygglosenAllocationNumber = 752,
	BygglosenMunicipalCode = 753,
	BygglosenSalaryFormula = 754,
	BygglosenProfessionCategory = 755,
	BygglosenSalaryType = 756,
	BygglosenWorkPlaceNumber = 757,
	BygglosenLendedToOrgNr = 758,
	BygglosenAgreedHourlyPayLevel = 759,
	
	GTPAgreementNumber = 761,
	GTPExcluded = 762,
	
	AGIPlaceOfEmploymentAddress = 771,
	AGIPlaceOfEmploymentCity = 772,
	AGIPlaceOfEmploymentIgnore = 773,
	
	TaxTinNumber = 781,
	TaxCountryCode = 782,
	TaxBirthPlace = 783,
	TaxCountryCodeBirthPlace = 784,
	TaxCountryCodeCitizen = 785,
	
	GeneralText = 1000,
}

export enum TermGroup_SoftOneStatusResultMatrixColumns {
	Unknown = 0,
	Date = 1,
	Created = 2,
	From = 3,
	To = 4,
	Hour = 5,
	Failed = 6,
	Succeded = 7,
	Min = 10,
	Median = 11,
	Average = 12,
	Max = 13,
	Percential10 = 14,
	Percential90 = 15,
	ServiceTypeName = 16,
}

export enum TermGroup_SoftOneStatusUpTimeMatrixColumns {
	Unknown = 0,
	StatusServiceGroupName = 1,
	Date = 2,
	UpTimeOnDate = 3,
	TotalUpTimeOnDate = 4,
	WebUpTimeOnDate = 5,
	MobileUpTimeOnDate = 6,
}

export enum TermGroup_SoftOneStatusEventMatrixColumns {
	Unknown = 0,
	Prio = 1,
	Url = 2,
	StatusServiceTypeName = 3,
	Start = 4,
	End = 5,
	Minutes = 6,
	LastMessageSent = 7,
	Message = 8,
	StatusEventTypeName = 9,
	JobDescriptionName = 10
}

export enum TermGroup_EmployeeFixedPayLinesMatrixColumns {
	Unknown = 0,
	EmployeeNr = 1,
	EmployeeName = 2,
	FirstName = 3,
	LastName = 4,
	BirthYear = 5,
	Gender = 6,
	Position = 10,
	SSYKCode = 11,
	EmploymentTypeName = 20,
	EmploymentStartDate = 21,
	PayrollGroup = 30,
	ProductNr = 41,
	ProuctName = 42,
	FromDate = 43,
	ToDate = 44,
	Quantity = 45,
	IsSpecifiedUnitPrice = 46,
	Distribute = 47,
	UnitPrice = 48,
	VatAmount = 49,
	Amount = 50,
	FromPayrollGroup = 60,
	
}

export enum TermGroup_TimeAbsenceRuleRowScope {
	Coherent = 0,
	Calendaryear = 1,
}

export enum TermGroup_PaymentTransferStatus {
	None = 0,
	PendingTransfer = 5,
	Transfered = 10,
	Pending = 11,
	PartlyRejected = 12,
	Completed = 20,
	BankError = 97,
	AvaloError = 98,
	SoftoneError = 99,
}

export enum TermGroup_EmploymentHistoryMatrixColumns {
	Unknown = 0,
	EmploymentNumber = 1,
	FirstName = 2,
	LastName = 3,
	EmploymentPercentage = 4,
	WorkingPlace = 5,
	EmploymentStartDate = 6,
	EmploymentEndDate = 7,
	EmploymentType = 8,
	EmploymentEndReason = 9,
	TotalEmploymentDays = 10,
	LASDays = 11,
}

export enum TermGroup_PayrollProductsMatrixColumns {
	Unknown = 0,
	Number = 1,
	Name = 2,
	ShortName = 3,
	ExternalNumber = 4,
	ProductFactor = 7,
	ResultType = 8,
	PayrollProductPayed = 10,
	ExcludeInWorkTimeSummary = 11,
	AverageCalculated = 12,
	UseInPayroll = 13,
	DontUseFixedAccounting = 14,
	ProductExport = 15,
	IncludeAmountInExport = 16,
	Payrollgroup = 20,
	CentroundingType = 24,
	CentroundingLevel = 25,
	TaxCalculationType = 26,
	PensionCompany = 27,
	TimeUnit = 28,
	QuantityRoundingType = 29,
	QuantityRoundingMinutes = 30,
	ChildProduct = 31,
	PrintOnSalaryspecification = 35,
	DontPrintOnSalarySpecificationWhenZeroAmount = 36,
	ShowPrintDate = 37,
	DontIncludeInRetroactivePayroll = 38,
	VacationSalaryPromoted = 39,
	UnionFeePromoted = 40,
	WorkingTimePromoted = 41,
	CalculateSupplementCharge = 42,
	CalculateSicknessSalary = 43,
	Payrollpricetypes = 50,
	Payrollpriceformulas = 51,
	AccountingPurchase = 60,
	AccountingPrioName = 70,
	Syspayrolltypelevel1 = 80,
	Syspayrolltypelevel2 = 81,
	Syspayrolltypelevel3 = 82,
	Syspayrolltypelevel4 = 83,
	PayrollProductId = 100,
}

export enum TermGroup_EmployeeSalaryDistressMatrixColumns {
	Unknown = 0,
	EmployeeNr = 1,
	Name = 2,
	FirstName = 3,
	LastName = 4,
	SSN = 5,
	Gender = 6,
	Date = 10,
	PaymentDate = 11,
	PayrollProductNumber = 15,
	PayrollProductName = 16,
	UnitPrice = 20,
	Quantity = 21,
	Amount = 22,
	ManualAdded = 24,
	SalaryDistressResAmount = 40,
	CaseNumber = 41,
	SeizureAmountType = 43,
	SalaryDistressAmount = 44,
	Absence = 50,
}

export enum TermGroup_OrderAnalysisMatrixColumns {
	Unknown = 0,
	CustomerNumber = 1,
	CustomerName = 2,
	OrderNumber = 3,
	OrderDate = 4,
	DeliveryDate = 5,
	PurchaseDate = 6,
	ProjectNumber = 7,
	ProjectName = 8,
	AmountExVAT = 9,
	ToInvoiceExVAT = 10,
	OurReference = 12,
	SalesPriceList = 13,
	AssignmentType = 14,
	ReadyStateMy = 15,
	ReadyStateAll = 16,
	Created = 17,
	CreatedBy = 18,
	Changed = 19,
	ChangedBy = 20,
	AccountInternalName1 = 101,
	AccountInternalName2 = 102,
	AccountInternalName3 = 103,
	AccountInternalName4 = 104,
	AccountInternalName5 = 105
}

export enum TermGroup_EmployeeSalaryUnionFeesMatrixColumns {
	Unknown = 0,
	EmployeeNr = 1,
	Name = 2,
	FirstName = 3,
	LastName = 4,
	SSN = 5,
	PaymentDate = 11,
	PayrollProductNumber = 15,
	PayrollProductName = 16,
	UnitPrice = 20,
	Quantity = 21,
	Amount = 22,
	UnionName = 30,
	PayrollPriceTypeIdPercentName = 31,
	PayrollPriceTypeIdPercentCeilingName = 32,
	PayrollPriceTypeIdFixedAmountName = 33,
	
}

export enum TermGroup_EmploymentDaysMatrixColumns {
	Unknown = 0,
	EmploymentNumber = 1,
	Name = 2,
	FirstName = 3,
	LastName = 4,
	WorkingPlace = 8,
	EmploymentStartDate = 10,
	EmploymentEndDate = 11,
	EmploymentType = 12,
	TimeAgreement = 20,
	EmploymentLASTypeAvaDays = 31,
	EmploymentLASTypeSvaDays = 32,
	EmploymentLASTypeVikDays = 33,
	EmploymentLASTypeOtherDays = 34,
	TotalEmploymentDays = 50,
}

export enum TermGroup_TimeWorkAccountWithdrawalMethod {
	NotChoosed = 0,
	PensionDeposit = 1,
	PaidLeave = 2,
	DirectPayment = 3,
}

export enum TermGroup_TimeWorkAccountYearEmployeeStatus {
	NotCalculated = 0,
	Created = 1,
	Calculated = 2,
	Choosed = 3,
	Outcome = 4,
	PaidBalance = 5,
}

export enum TermGroup_EmployeeTemplateGroupType {
	Normal = 0,
	SubstituteShifts = 1
}

export enum TermGroup_SalaryExportUseSocSecFormat {
	KeepEmployeeNr = 0,
	YYYYMMDD_dash_XXXX = 1,
	YYYYMMDDXXXX = 2,
	YYMMDD_dash_XXXX = 3,
	YYMMDDXXXX = 4,
}

export enum TermGroup_InvoiceAnalysisMatrixColumns {
	Unknown = 0,
	CustomerNumber = 1,
	CustomerName = 2,
	InvoiceNumber = 3,
	InvoiceDate = 4,
	DueDate = 5,
	OrderDate = 6,
	DeliveryDate = 7,
	InvoiceType = 8,
	Status = 9,
	ProjectNumber = 10,
	ProjectName = 11,
	AmountExVAT = 12,
	ToInvoiceExVAT = 13,
	VATType = 14,
	Currency = 15,
	OurReference = 17,
	OriginDescription = 18,
	InvoiceLabel = 19,
	SalesPriceList = 20,
	InvoiceAddress = 21,
	DeliveryAddress = 22,
	Created = 23,
	CreatedBy = 24,
	Modified = 25,
	ModifiedBy = 26,
	AccountInternalName1 = 101,
	AccountInternalName2 = 102,
	AccountInternalName3 = 103,
	AccountInternalName4 = 104,
	AccountInternalName5 = 105
}

export enum TermGroup_AccountHierarchyMatrixColumns {
	Unknown = 0,
	AccountNames = 1,
	AccountNumbers = 2,
	AccountNamesAndNumbers = 3,
	AccountExecutiveUsername = 4,
	AccountExecutiveName = 5,
	AccountExecutiveEmail = 6,
}

export enum TermGroup_ScheduledJobSettingType {
	Unknown = 0,
	
	//Bridge
	BridgeJob = 1000,
	BridgeJobType = 1001,
	BridgeJobRunChildJobs = 1002,
	
	//Bridge - Setup
	BridgeSetupCallBackUrl = 1005,
	BridgeSetupCallBackTimeOutInSeconds = 1010,
	BridgeSetupAddress = 1015,
	BridgeSetupPath = 1020,
	BridgeSetupPathTransfer = 1021,
	BridgeSetupContainer = 1025,
	BridgeSetupFileName = 1026,
	BridgeSetupImportKey = 1030,
	BridgeSetupImportSettings = 1031,
	
	//Bridge - Credentials
	BridgeCredentialSecret = 1050,
	BridgeCredentialUser = 1055,
	BridgeCredentialPassword = 1060,
	BridgeCredentialTokenEndPoint = 1061,
	BridgeCredentialGrantType = 1062,
	BridgeCredentialTenent = 1063,
	BridgeCredentialConnectionString = 1064,
	BridgeCredentialToken = 1065,
	
	// Merge
	BridgeMergeFileWithPrevious = 1070,
	
	//Export
	ExportId = 1100,
	ExportKey = 1102,
	ExportIsPreliminary = 1103,
	ExportLock = 1104,
	ExportCustomJob = 1105,
	
	//Bridge - Fileinformation
	BridgeFileInformationMatchExpression = 1080,
	BridgeJobFileType = 1081,
	BridgeFileInformationDefinitionId = 1082,
	BridgeFileInformationImportHeadId = 1083,
	
	//Event
	EventActivationType = 2100,
	
	//TimeAccumulator
	TimeAccumulator_SendToUser = 3001,
	TimeAccumulator_SendToExecutive = 3002,
	TimeAccumulator_AdjustCurrentBalance = 3003,
	TimeAccumulator_IncludeFutureMonth = 3004,
	TimeAccumulator_AdjustCurrentBalanceDate = 3005,
	
	//Specified Type
	SpecifiedType = 4001,
}

export enum TermGroup_BridgeJobType {
	Unknown = 0,
	
	FTP_File_Transfer_Upload = 10,
	SFTP_File_Transfer_Upload = 15,
	AzureStorage_File_Transfer_Upload = 20,
	VismaPayroll = 30,
	FTP_File_Transfer_Download = 110,
	SFTP_File_Transfer_Download = 115,
	AzureStorage_File_Transfer_Download = 120,
	Mqqt_Add_Message_To_Queue = 121,
}

export enum TermGroup_TimeSchedulePlanningShiftTypePosition {
	Left = 0,
	Center = 1
}

export enum TermGroup_TimeSchedulePlanningTimePosition {
	Left = 0,
	ShiftEdges = 1,
	DayEdges = 2,
	Hidden = 3
}

export enum TermGroup_MonthlyWorkTimeCalculationType {
	Divisor = 0,
	ScheduledHoursPerMonth = 1
}

export enum TermGroup_ControlEmployeeSchedulePlacementType {
	ShortenHasAbsenceRequest = 1,
	ShortenHasAbsenceDays = 2,
	ShortenHasChangedSchedule = 3,
	ShortenHasChangedTimeBlocks = 4,
	ShortenIsHidden = 5,
}

export enum TermGroup_ScheduleJobEventActivationType {
	Unknown = 0,
	EmployeeCreated = 101,
	TimeStampCreated = 102,
}

export enum TermGroup_IFPaymentCode {
	Unknown = 0,
	PayedFee = 1,
	LeaveOfAbsence = 3,
	OtherReason = 8,
	EmploymentEnding = 19,
	PowerOfAttorneyMissing = 33,
}

export enum TermGroup_UnionFeeAssociation {
	Other = 0,
	IFMetall = 1,
	SEF = 2,
}

export enum TermGroup_PayrollProductReportSettingType {
	Unknown = 0,
	Field = 1,
	NotReportSpecificField = 2,
	StaffingStatistics_IsWorkTime = 101
}

export enum TermGroup_AnnualProgressMatrixColumns {
	Unknown = 0,
	Date = 1,
	AccountInternalNrs = 2,
	AccountInternalNames = 3,
	GoalPerWeek = 10,
	GoalPerMonth = 11,
	LastYearAveragePerWeek = 20,
	LastYearAveragePerMonth = 21,
	DifferenceAverageFromLastYearPerWeek = 30,
	DifferenceAverageFromLastYearPerMonth = 31,
	DifferenceAverageFromGoalPerWeek = 40,
	DifferenceAverageFromGoalPerMonth = 41,
	RemainingYearAveragePerWeek = 50,
	RemainingYearAveragePerMonth = 51,
	SalesToDate = 60,
	WorkingHoursToDate = 61,
	FPATGoal = 70,
	FPATToDate = 71,
	AveragePerWeekToDate = 80,
	AveragePerMonthToDate = 81,
}

export enum TermGroup_BridgeJobFileType {
	Unknown = 0,
	StaffingneedFrequencyIODTO = 1,
	StaffingneedFrequencyFile = 2,
	SalaryExportFile = 3,
}

export enum TermGroup_LongtermAbsenceMatrixColumns {
	Unknown = 0,
	EmployeeNr = 1,
	Name = 2,
	FirstName = 3,
	LastName = 4,
	SocialSec = 5,
	PayrollTypeLevel1Name = 101,
	PayrollTypeLevel2Name = 102,
	PayrollTypeLevel3Name = 103,
	PayrollTypeLevel1 = 201,
	PayrollTypeLevel2 = 202,
	PayrollTypeLevel3 = 203,
	StartDateInInterval = 300,
	StopDateInInterval = 301,
	NumberOfDaysInInterval = 302,
	EntireSelectedPeriod = 303,
	StartDate = 401,
	StopDate = 402,
	NumberOfDaysTotal = 403,
	NumberOfDaysBeforeInterval = 404,
	NumberOfDaysAfterInterval = 405,
	AccountInternalNrs = 500,
	AccountInternalNames = 501,
	Ratio = 502,
	EmployeeAccountInternalNrs = 600,
	EmployeeAccountInternalNames = 601,
	Created = 800,
	Modified = 801
}

export enum TermGroup_VacationBalanceMatrixColumns {
	Unknown = 0,
	EmploymentNr = 1,
	Name = 2,
	FirstName = 3,
	LastName = 4,
	SocialSecurityNumber = 5,
	Active = 6,
	BirthYear = 7,
	Age = 8,
	Gender = 9,
	Roles = 10,
	EmploymentPosition = 11,
	PayrollAgreement = 12,
	ContractGroup = 13,
	VacationAgreement = 14,
	WeeklyWorkingHours = 15,
	EmploymentRate = 16,
	BasicWeeklyWorkingHours = 17,
	Categories = 18,
	PaidEarnedDays = 19,
	PaidSelectedDays = 20,
	PaidRemainingDays = 21,
	PaidSysDegreeEarned = 22,
	PaidHolidayAllowance = 23,
	PaidVariableVacationSupplementsSelectedDays = 24,
	UnpaidEarnedDays = 25,
	UnpaidSelectedDays = 26,
	UnpaidRemainingDays = 27,
	AdvanceEarnedDays = 28,
	AdvanceSelectedDays = 29,
	AdvanceRemaininDays = 30,
	DebtCashAdvancesAmount = 31,
	DebtCashAdvancesDecay = 32,
	SavedYear1EarnedDays = 33,
	SavedYear1SelectedDays = 34,
	SavedYear1RemaininDays = 35,
	SavedYear1SysDegreeEarned = 36,
	SavedYear2EarnedDays = 37,
	SavedYear2SelectedDays = 38,
	SavedYear2RemaininDays = 39,
	SavedYear2SysDegreeEarned = 40,
	SavedYear3EarnedDays = 41,
	SavedYear3SelectedDays = 42,
	SavedYear3RemaininDays = 43,
	SavedYear3SysDegreeEarned = 44,
	SavedYear4EarnedDays = 45,
	SavedYear4SelectedDays = 46,
	SavedYear4RemaininDays = 47,
	SavedYear4SysDegreeEarned = 48,
	SavedYear5EarnedDays = 49,
	SavedYear5SelectedDays = 50,
	SavedYear5RemaininDays = 51,
	SavedYear5SysDegreeEarned = 52,
	OverdueDaysEarnedDays = 53,
	OverdueDaysSelectedDays = 54,
	OverdueDaysRemainingDays = 55,
	OverdueDaysSysDegreeEarned = 56,
	PreliminaryWithdrawnRemaininDays = 57,
	RemainingSelectedDays = 58,
	RemainingRemainingDays = 59,
	AccountInternalNrs = 507,
	AccountInternalNames = 508,
	ExtraFieldEmployee = 700,
}

export enum TermGroup_TimeWorkAccountYearResultCode {
	Succeeded = 0,
	Deleted = 2,
	
	//Fail statuses below (logic rely on fail status being above 10)
	
	InvalidStatus = 11,
	NoValidAccounts = 12,
	
	CalculationFailed = 101,
	SaveFailed = 102,
}

export enum TermGroup_SkandiaPensionType {
	Unknown = 0,
	AKAP_KR = 1,
}

export enum TermGroup_SkandiaPensionCategory {
	Unknown = 0,
	AB_MÅNADSAVLÖNAD = 1,
	BRANDMAN_MÅNADSAVLÖNAD = 2,
	BRANDMAN_UTAN_RÄTT_TILL_SAP = 3,
	MEDICINE_STUDERANDE_MÅNADSAVLÖNAD = 4,
	BEA_MÅNADSAVLÖNAD = 5,
	BEA_TIMAVLÖNAD = 6,
	AB_TIMAVLÖNAD = 7,
	PAN_RIB_TIMAVLÖNAD = 8
}

export enum TermGroup_SkandiaPensionReportType {
	Unknown = 0,
	AnnualReport = 1, //Årsrapport
	InterimReport = 2, //Delårsrapport
	SemiAnnualReport = 3, //Halvårsrapport
	StartReporting = 4,//Startrapportering
}

export enum TermGroup_TimeWorkAccountYearSendMailCode {
	Succeeded = 0,
	SendFailed = 1,
	EmployeeNotFound = 2,
	EmployeeHasChoosen = 3,
	EmployeeNotCalculated = 4,
	EmployeeNoAmount = 5,
}

export enum TermGroup_TimeWorkAccountTransactionActionCode {
	CreateSucess = 0,
	DeleteSuccess = 2,
	BalanceSuccess = 3,
	
	EmployeeNotFound = 11,
	EmployeeHasntChoosen = 14,
	EmployeeNotCalculated = 15,
	EmployeeAlreadyGenerated = 16,
	EmployeeNotConnectedToTimeAccumulator = 17,
	EmployeeNotGenerated = 18,
	
	TimePeriodNotFound = 21,
	TimePeriodLocked = 22,
	PayrollPeriodDirectPaymentNotFound = 23,
	PayrollPeriodPensionDepositNotFound = 24,
	TimeCodePaidLeaveNotFound = 25,
	PayrollProductPaidLeaveNotFound = 26,
	
	PayrollTransactionWrongState = 31,
	
	BalanceAlreadyPaid = 41,
	BalanceNotGenerated = 42,
	BalanceNotChoosen = 43,
	BalanceNotFound = 44,
	
	GenerationFailed = 101,
	SaveFailed = 102,
}

export enum TermGroup_ShiftQueueMatrixColumns {
	EmployeeId = 0,
	EmployeeNr = 1,
	EmployeeName = 2,
	Created = 3,
	Creator = 4,
	Modifier = 5,
	Date = 6,
	QueueTimeBeforeShiftStartInHours = 7,
	QueueTimeBeforeQueueWasHandledInHours = 8,
	TypeName = 9,
	StartTime = 10,
	StopTime = 11,
	CurrentEmployee = 12,
	QueueTimeSinceShiftCreatedInHours = 13,
	DateHandled = 14,
	CurrentEmployeeIsHidden = 15,
	QueueTimeHandledBeforeShiftStartInHours = 16,
}

export enum TermGroup_ShiftHistoryMatrixColumns {
	EmployeeId = 0,
	TimeScheduleTemplateBlockId = 5,
	TypeName = 10,
	FromShiftStatus = 15,
	ToShiftStatus = 20,
	ShiftStatusChanged = 25,
	FromShiftUserStatus = 30,
	ToShiftUserStatus = 35,
	ShiftUserStatusChanged = 40,
	FromEmployeeName = 45,
	ToEmployeeName = 50,
	FromEmployeeNr = 55,
	ToEmployeeNr = 60,
	EmployeeChanged = 65,
	FromTime = 70,
	ToTime = 75,
	TimeChanged = 80,
	FromDateAndTime = 85,
	ToDateAndTime = 90,
	DateAndTimeChanged = 95,
	FromShiftType = 100,
	ToShiftType = 105,
	ShiftTypeChanged = 110,
	FromTimeDeviationCause = 115,
	ToTimeDeviationCause = 120,
	TimeDeviationCauseChanged = 125,
	Created = 130,
	CreatedBy = 135,
	AbsenceRequestApprovedText = 140,
	FromStart = 145,
	FromStop = 150,
	ToStart = 155,
	ToStop = 160,
	OriginEmployeeNr = 165,
	OriginEmployeeName = 170,
	FromEmployeeId = 175,
	ToEmployeeId = 180,
	FromExtraShift = 185,
	ToExtraShift = 190,
	ExtraShiftChanged = 195
}

export enum TermGroup_ShiftRequestMatrixColumns {
	EmployeeId = 0,
	EmployeeNr = 1,
	EmployeeName = 2,
	RequestCreated = 3,
	RequestCreatedBy = 4,
	Sender = 5,
	SentDate = 6,
	ReadDate = 7,
	Answer = 8,
	AnswerDate = 9,
	Subject = 10,
	Text = 11,
	ShiftDate = 20,
	ShiftStartTime = 21,
	ShiftStopTime = 22,
	ShiftTypeName = 23,
	ShiftCreated = 24,
	ShiftCreatedBy = 25,
	ShiftModified = 26,
	ShiftModifiedBy = 27,
	ShiftDeleted = 28,
	ShiftAccountNr = 29,
	ShiftAccountName = 30,
}

export enum TermGroup_AbsenceRequestMatrixColumns {
	EmployeeId = 0,
	EmployeeNr = 1,
	EmployeeName = 2,
	Created = 3,
	Creator = 4,
	Modifier = 5
}

export enum TermGroup_TimeScheduleTemplateBlockQueueStatus {
	Active = 0, // (I kön)
	Assigned = 1, // (Tilldelad)
	AssignedToOtherEmployee = 2, //(Tilldelad till annan anställd)
	DeletedByEmployee = 3, //(Borttagen av anställd)
	DeletedByAdmin = 4, //(Borttagen av admin)
	ShiftDeletedByAdmin = 5 //(Passet borttaget)
}

export enum TermGroup_SEFPaymentCode {
	Unknown = 0,
	PayedFee = 1,
	SickLeave = 4,
	MilitaryService = 5,
	EmploymentEnding = 19,
}

export enum TermGroup_EmployeeSettingType {
	None = 0,
	
	WorkTimeRule = 10000,                       // (1) Arbetstidsregler
	WorkTimeRule_WeeklyRest = 10100,            //   (2) Veckovila
	WorkTimeRule_WeeklyRest_Weekday = 10101,    //     (3) Veckodag
	WorkTimeRule_WeeklyRest_TimeOfDay = 10102,  //     (3) Klockslag
	WorkTimeRule_DailyRest = 10200,             //   (2) Dygnsvila
	WorkTimeRule_DailyRest_TimeOfDay = 10201,   //     (3) Klockslag
	
	Reporting = 20000,                          // (1) Rapportering
	Reporting_Fora = 20100,                     //   (2) Fora
	Reporting_Fora_FokId = 20101,               //     (3) Fok-Id
	Reporting_Fora_Option1 = 20102,             //     (3) Tillval 1
	Reporting_Fora_Option2 = 20103,             //     (3) Tillval 2
	Reporting_SEF = 20200,                      //   (2) SEF
	Reporting_SEF_AssociationNumber = 20201,    //     (3) Förbundsnummer
	Reporting_SEF_WorkPlace = 20202,            //     (3) Arbetsställenummer
	Reporting_SEF_PaymentCode = 20203,          //     (3) Betalkod
	Reporting_SN = 20300,                       //   (2) SN
	Reporting_SN_Jobstatus = 20301,             //     (3) Jobb status
	
	Additions = 30000,                          // (1) Lönetillägg
	Additions_ScheduleType = 30100,             //   (2) SchemaTyper
}

export enum TermGroup_ReportBillingDateRegard {
	OrderDate = 1,
	DeliveryDate = 2,
}

export enum TermGroup_VismaPayrollChangesMatrixColumns {
	Unknown = 0,
	Time = 1,
	VismaPayrollChangeId = 2,
	VismaPayrollBatchId = 3,
	PersonId = 4,
	VismaPayrollEmploymentId = 5,
	Entity = 6,
	Info = 7,
	Field = 8,
	OldValue = 9,
	NewValue = 10,
	EmployerRegistrationNumber = 11,
	PersonName = 12,
}

export enum TermGroup_TimeStampHistoryMatrixColumns {
	Unknown = 0,
	EntityText = 1,
	ColumnName = 2,
	ActionMethod = 3,
	Action = 4,
	FromValue = 5,
	ToValue = 6,
	Role = 7,
	RecordName = 8,
	Created = 9,
	CreatedBy = 10,
	Batch = 11,
	BatchNr = 12,
	TopEntity1Text = 13,
	TopEntity2Text = 14,
	TopRecordName = 15,
}

export enum TermGroup_BrandingCompanies {
	SoftOne = 0,
	FlexiblaKontoret = 1,
}

export enum TermGroup_VacationYearEndStatus {
	Ongoing = -1,
	Succeded = 0,
	Failed = 1,
}

export enum TermGroup_VerticalTimeTrackerMatrixColumns {
	Date = 1,
	StartDate = 2,
	EndDate = 3,
	StartTime = 4,
	StopTime = 5,
	TimeInterval = 6,
	Time = 7,
	TimeCost = 8,
	Schedule = 9,
	StartYear = 20,
	StartMonth = 21,
	StartDay = 22,
	StartHour = 23,
	StartMinute = 24,
	ScheduleCost = 10,
	EmployeeName = 101,
	EmployeeNr = 102,
	AccountInternalNrs = 200,
	AccountInternalNames = 201
}

export enum TermGroup_HorizontalTimeTrackerMatrixColumns {
	Date = 1,
	StartDate = 2,
	EndDate = 3,
	StartTime = 4,
	StopTime = 5,
	TimeInterval = 6,
	Time = 7,
	TimeCost = 8,
	Schedule = 9,
	ScheduleCost = 10,
	EmployeeName = 101,
	EmployeeNr = 102,
	AccountInternalNrs = 200,
	AccountInternalNames = 201
}

export enum TermGroup_LicenseInformationMatrixColumns {
	Database = 1,
	LicenseNr = 2,
	LicenseName = 3,
	LastLogin = 100,
}

export enum TermGroup_ReportSettingType {
	AttachmentExtraShiftsHeader = 1,
	//SuppressSubstituteCauseText = 2, Not used anymore
	ProjectOverviewExtendedInfo = 3,
	StockInventoryExcludeZeroQuantity = 4,
	ExcludeItemsWithZeroQuantityForSpecificDate = 5,
	HidePriceAndRowSum = 6,
	GroupedInvoiceByTaxDeduction = 7,
	GroupedOfferByTaxDeduction = 8,
}

export enum TermGroup_FinnishTaxReturnExportTaxPeriodLength {
	Month = 1,      //K
	Quarter = 2,    //Q
	Year = 3,       //V
}

export enum TermGroup_AccountYearStatus {
	NotStarted = 1,
	Open = 2,
	Closed = 3,
	Locked = 4,
}

export enum TermGroup_PayrollControlFunctionType {
	None = 0,
	TaxMissing = 1,
	EmploymentTaxMissing = 2,
	EmploymentTaxDiff = 3,
	SupplementChargeDiff = 4,
	GrossSalaryNegative = 5,
	NetSalaryMissing = 6,
	NetSalaryNegative = 7,
	NetSalaryDiff = 8,
	NegativeVacationDays = 9,
	VacationGroupMissing = 10,
	PeriodHasNotBeenCalculated = 11,
	DisbursementMethodIsUnknown = 12,
	DisbursementMethodIsCash = 13,
	DisbursementAccountNrIsMissing = 14,
	BenefitNegative = 15,
	NewEmployeeInPeriod = 16,
	EndDateInPeriodFinalSalaryNotChosen = 17,
	NewAgreementInPeriod = 18,
	PeriodHasChanged = 19,
	OverlappingMainAffiliation = 20,
	EmployeePositionMissing = 21,
}

export enum TermGroup_PayrollControlFunctionStatus {
	Opened = 1,
	HideforPeriod = 2,
	Attention = 3,
}

export enum TermGroup_PayrollControlFunctionOutcomeChangeType {
	Manual = 1,
	Automatic = 2,
}

export enum TermGroup_PayrollControlFunctionOutcomeChangeFieldType {
	Status = 1,
	Comment = 2,
	State = 3,
	Value = 4,
}

export enum TermGroup_GrossMarginCalculationType {
	Unknown = 0,
	PurchasePrice = 1,
	StockAveragePrice = 2,
}

export enum TermGroup_DatePeriodSelection {
	Today = 1,
	Yesterday = 2,
	CurrentWeek = 3,
	PreviousWeek = 4,
	CurrentMonth = 5,
	PreviousMonth = 6,
	CurrentYear = 7,
	PreviousYear = 8,
	CurrentFinancialYear = 9,
	PreviousFinancialYear = 10,
	Custom = 999
}

export enum TermGroup_ISOPaymentAccountType {
	Unknown = 0,
	BGNR = 1,
	BBAN = 2,
	IBAN = 3,
	
}

export enum TermGroup_TimeLeisureCodeSettingType {
	LeisureHours = 1,
	ScheduleType = 2,
	PreferredWeekDay = 3,
}

export enum TermGroup_TimeLeisureCodeType {
	None = 0,
	V = 1, // Weekly rest day
	X = 2, // Extra rest day
}

export enum TermGroup_TimeTreeWarningFilter {
	None = 0,
	Any = 1,
	Time = 2,
	Payroll = 3,
	PayrollStopping = 4,
}

export enum TermGroup_SpecifiedType {
	None = 0,
	PresenceWarningJob = 1,
}

export enum TermGroup_ChainAffiliation {
	None = 0,
	Bad_Varme = 1,
	Comfort = 2,
	Currentum = 3
}

export enum TermGroup_ExtraFieldValueType {
	Unknown = 0,
	String = 1
}

export enum TermGroup_AgiAbsenceMatrixColumns {
	Unknown = 0,
	EmployeeNr = 1,
	EmployeeName = 2,
	SocialSec = 3,
	PaymentDate = 11,
	Date = 12,
	ProductNr = 21,
	ProductName = 22,
	Type = 23,
	Quantity = 24,
	ParentalLeave = 31,
	TemporaryParentalLeave = 41,
}

export enum TermGroup_InvoiceProductUnitConvertMatrixColumns {
	Unknown = 0,
	ProductNr = 1,
	ProductName = 2,
	ProductUnitName = 3,
	ConvertUnitName = 4,
	ConvertUnitFactor = 5,
	CreatedBy = 6,
	Created = 7,
	ModifiedBy = 8,
	Modified = 9,
}

export enum TermGroup_SieExportVoucherSort {
	Unknown = 0,
	ByDate = 1,
	ByVoucherNr = 2
}

export enum TermGroup_TimeTerminalLogLevel {
	Debug = 0,
	Information = 1,
	Warning = 2,
	Error = 3
}

export enum TermGroup_BygglosenSalaryType {
	Unknown = 0,
	TimeSalary = 1,
	PerformanceSalary = 2,
	PerformanceSalaryTIA = 3,
}

export enum TermGroup_SupplierInvoiceSource {
	Unknown = 0,
	EDI = 1,
	EInvoice = 2,
	FInvoice = 3,
	Interpreted = 4,
	Upload = 5,
	UserEntered = 6,
}

export enum TermGroup_SupplierInvoiceStatus {
	New = 1,
	InProgress = 2,
}

export enum TermGroup_SignatoryContractPermissionType {
	Unknown = 0,
	// Core features 1-99
	
	SignatoryContract_EditContracts = 1,
	
	// Account payables 100-120
	AccountsPayable_SendPaymentToBank = 100,
}

export enum TermGroup_ExcludeFromWorkTimeWeekCalculationItems {
	UseSettingOnEmploymentType = 1,
	No = 2,
	Yes = 3
}

export enum TermGroup_InventoryMatrixColumns {
	Unknown = 0,
	InventoryNumber = 1,
	InventoryName = 2,
	InventoryNumberName = 3,
	InventoryStatus = 4,
	InventoryDescription = 5,
	InventoryAccount = 6,
	AcquisitionDate = 7,
	AcquisitionValue = 8,
	AcquisitionsForThePeriod = 9,
	DepreciationValue = 10,
	BookValue = 11,
	DepreciationForThePeriod = 12,
	Disposals = 13,
	Scrapped = 14,
	AccumulatedDepreciationTotal = 15,
	DepriciationMethod = 16,
	InventoryCategories = 17,
}

export enum TermGroup_FileImportStatus {
	InProgress = 1,
	Success = 2,
	Error = 3,
	Reversed = 4
}

export enum TermGroup_EmployeeChildMatrixColumns {
	Unknown = 0,
	EmployeeNr = 1,
	EmployeeName = 2,
	ChildFirstName = 10,
	ChildLastName = 11,
	ChildDateOfBirth = 12,
	ChildSingelCustody = 13,
	AmountOfDays = 20,
	AmountOfDaysUsed = 21,
	AmountOfDaysLeft = 22,
	Openingbalanceuseddays = 25
}

export enum TermGroup_EmployeePayrollAdditionsMatrixColumns {
	Unknown = 0,
	EmployeeNr = 1,
	EmployeeName = 2,
	SalaryType = 10,
	Group = 11,
	Type = 12,
	FromDate = 20,
	ToDate = 21,
}

export enum TermGroup_ProjectBudgetPeriodType {
	SinglePeriod = 1,
	Monthly = 2,
	Quarterly = 3,
}

export enum TermGroup_AnnualLeaveGroupType {
	Unknown = 0,
	Commercial = 1,
	HotelRestaurant = 2,
	HotelRestaurantNight = 3,
}

export enum TermGroup_TimeScheduleTemplateBlockAbsenceType {
	Standard = 0,
	AnnualLeave = 1,
}

export enum TermGroup_TimeSchedulePlanningAnnualLeaveBalanceFormat {
	Days = 0,
	Hours = 1,
}

export enum TermGroup_AnnualLeaveTransactionMatrixColumns {
	Unknown = 0,
	EmployeeNr = 1,
	EmployeeName = 2,
	DateEarned = 10,
	DateSpent = 11,
	YearEarned = 12,
	Accumulation = 20,
	Hours = 21,
	EarnedHours = 30,
	EarnedDays = 31,
	SpentHours = 32,
	SpentDays = 33,
	BalanceHours = 40,
	BalanceDays = 41,
	Type = 50,
}

export enum TermGroup_TimeWorkReductionCalculationRule {
	UseWorkTimeWeek = 0,
	UseWorkTimeWeekPlusExtraShifts = 1,
	UseWorkTimeWeekPlusAdditionalContract = 2,
}

export enum TermGroup_AnnualLeaveTransactionType {
	Calculated = 0,
	YearlyBalance = 1,
	ManuallyEarned = 2,
	ManuallySpent = 3,
	Imported = 4,
}

export enum TermGroup_TimeWorkReductionWithdrawalMethod {
	NotChoosed = 0,
	PensionDeposit = 1,
	DirectPayment = 2,
}

export enum TermGroup_DepreciationMatrixColumns {
	Unknown = 0,
	InventoryNumber = 1,
	InventoryName = 2,
	RemainingValue = 3,
	InventoryStatus = 4,
	InventoryCategories = 5,
	TotalDepreciationAmount = 6,
	PeriodicDepreciations = 7
}

export enum TermGroup_TimeSchedulePlanningShiftStartsOnDay {
	PreviousDay = -1,
	CurrentDay = 0,
	NextDay = 1,
}

export enum TermGroup_TimeWorkReductionReconciliationEmployeeStatus {
	NotCalculated = 0,
	Created = 1,
	Calculated = 2,
	Choosed = 3,
	Outcome = 4,
}

export enum TermGroup_TimeWorkReductionReconciliationResultCode {
	Succeeded = 0,
	Deleted = 2,
	
	//Fail statuses below (logic rely on fail status being above 10)
	
	InvalidStatus = 11,
	NoValidAccumulators = 12,
	CalculatedInOtherReconsilation = 13,
	
	EmployeeNotFound = 21,
	TimePeriodNotFound = 22,
	TimePeriodLocked = 23,
	PayrollPeriodDirectPaymentNotFound = 24,
	PayrollPeriodPensionDepositNotFound = 25,
	EmployeeAlreadyGenerated = 26,
	EmployeeNotCalculated = 27,
	EmployeeHasntChoosen = 28,
	NoAmountToWithdraw = 29,
	
	EmployeeNotGenerated = 31,
	PayrollTransactionWrongState = 32,
	
	CalculationFailed = 101,
	SaveFailed = 102,
	GenerationFailed = 103,
	DeleteSuccess = 104
}

export enum TermGroup_SysExtraField {
	AveragingOvertimeCalculation = 101,
	
	// See also SysExtraFieldType
}

export enum TermGroup_TimeCodeRankingOperatorType {
	GreaterThan = 0,
	LesserThan = 1,
}

export enum TermGroup_TimeScheduleCopyHeadType {
	Unknown = 0,
	PrelToDef = 1,
	DefToPrel = 2,
}

export enum TermGroup_TimeScheduleCopyRowType {
	Default = 0,
}

export enum TermGroup_TimeCodeClassification {
	None = 0,
	InconvinientWorkingHours = 1,
	AdditionalHours = 2,
	OvertimeHours = 3,
}

export enum TermGroup_PayrollReportsJobStatus {
	Other = 0,
	Manager = 1,
	CEO = 2,
	Supervisor = 3,
	Foreman = 4,
	Apprentice = 5,
}

export enum TermGroup_ImportPaymentType {
	None = 0,
	CustomerPayment = 1,
	SupplierPayment = 2,
	Sepa = 3,
	Professional = 4
};

export enum TermGroup_SwapShiftMatrixColumns {
	Unknown = 0,
	EmployeeNr = 1,
	EmployeeName = 2,
	Date = 3,
	HasShift = 4,
	ShiftType = 5,
	InitiatorEmployeeNr = 11,
	InitiatorEmployeeName = 12,
	InitiatorTime = 13,
	SwappedToEmployeeNr = 21,
	SwappedToEmployeeName = 22,
	AcceptorEmployeeNr = 31,
	AcceptorEmployeeName = 32,
	AcceptorDate = 33,
	ApprovedBy = 41,
	ApprovedDate = 42,
	ShiftLength  = 51,
}

export enum TextBlockType {
	TextBlockEntity = 1,
	Dictionary = 2,
	WorkingDescription = 3,
}

export enum SimpleTextEditorDialogMode {
	Base = 0,
	NewTextBlock = 1,
	EditTextBlock = 2,
	EditInvoiceText = 3,
	EditInvoiceDescription = 4,
	EditWorkingDescription = 5,
	EditInvoiceRowText = 6,
	AddTextblockToInvoiceRow = 7,
	AddSupplierInvoiceBlockReason = 8,
	
}

export enum TFSWorkItemType {
	Bug = 1,
	Task = 2
}

export enum TimeCodeTransactionType {
	Unknown = 0,
	Time = 1,
	TimeProject = 2,
	InvoiceProduct = 3,
	TimeSheet = 4,
}

export enum LeisureCodeAllocationEmployeeStatus {
	AllSuccess = 1,
	PartialSuccess = 2,
	AllFailed = 3,
	ProcessedWithInformation = 4,
}

export enum LeisureCodeAllocationWeekInvalidation {
	WeekHasToFewWorkDays = 1,
	WeekHasNoDaysToAllocateTo = 2,
	UnassignedAllocationsLeft = 3, // rest days earned that couldn't be allocated
}

export enum TimeRuleExportImportUnmatchedType {
	TimeCode = 1,
	EmployeeGroup = 2,
	TimeScheduleType = 3,
	TimeDeviationCause = 4,
	DayType = 5
}

export enum AbsenceType {
	Fulltime = 0,
	PartTime = 1,
}

export enum AbsenceTypePartTimeOptionpay {
	WholeFirstDay = 1,
	WholeLastDay = 2,
	PercentalBeginningOfDay = 3,
	PercentalEndOfDay = 4,
	AdjustmentPerDay = 5,
}

export enum TimeSchedulePlanningMode {
	SchedulePlanning = 1,
	OrderPlanning = 2,
}

export enum TimeSchedulePlanningDisplayMode {
	User = 1,
	Admin = 2,
	AdminAvailability = 3,
}

export enum DragShiftAction {
	Cancel = 0,
	Move = 1,
	Copy = 2,
	Replace = 3,
	ReplaceAndFree = 4,
	SwapEmployee = 5,
	Absence = 6,
	ShiftRequest = 7,
	Delete = 8,
	CopyWithCycle = 9,
	MoveWithCycle = 10,
}

export enum HandleShiftAction {
	Cancel = 0,
	Wanted = 1,
	Unwanted = 2,
	Absence = 3,
	ChangeEmployee = 4,
	SwapEmployee = 5,
	AbsenceAnnouncement = 6,
	
	UndoWanted = 11,
	UndoUnwanted = 12,
	UndoAbsence = 13,
	UndoChangeEmployee = 14,
	UndoSwapEmployee = 15,
	UndoAbsenceAnnouncement = 16, // Currently not implemented
}

export enum DropEmployeeAction {
	Cancel = 0,
	New = 1,
	Assign = 2
}

export enum EmployeeRequestStatus {
	None = 0,
	Preliminary = 1,
	Definitive = 2,
	
	//Only queries
	PreliminaryAndDefinitive = 10,
}

export enum TimeStampAdditionType {
	Unknown = 0,
	TimeScheduleType = 1,
	TimeCodeConstantValue = 2,
	TimeCodeVariableValue = 3,
	Account = 4,    // Only used when saving in TimeStampEntryExtended
}

export enum TimeTerminalType {
	Unknown = 0,
	TimeSpot = 1,
	XETimeStamp = 2,
	WebTimeStamp = 3,
	GoTimeStamp = 4
	
	// Duplicate exists in the GoTimeStamp project!
}

export enum TimeStampEntryType {
	Unknown = 0,
	In = 1,
	Out = 2
	
	// Duplicate exists in the GoTimeStamp project!
}

export enum TimeTerminalSettingType {
	Unknown = 0,
	SyncInterval = 1,                   // (int) Interval between terminal syncs (settings etc) (XETimeStamp)
	InactivityDelay = 2,                // (int) Inactive seconds until terminal goes back to initial state
	AccountDim = 3,                     // (int + string) AccountDimId and name for selectable accounts in the terminal
	SyncClockWithServer = 4,            // DEPRECATED
	SyncClockWithServerDiff = 5,        // DEPRECATED
	MaximizeWindow = 6,                 // (bool) Maximize terminal window (XETimeStamp)
	NewEmployee = 7,                    // (bool) Possibility to add a new employee from the terminal (XETimeStamp)
	SysCountryId = 8,                   // (int) LanguageId (XETimeStamp)
	LimitTimeTerminalToCategories = 9,  // (bool) Valid employees for the terminal is limited to selected categories (stored in CompanyCategoryRecord with entity = SoeCategoryRecordEntity.TimeTerminal and Type = SoeCategoryType.Employee)
	ForceCauseGraceMinutes = 10,        // (int) Minutes outside schedule that is valid without having to specify deviation cause (XETimeStamp)
	ForceCauseIfOutOfSchedule = 11,     // (bool) Force stamp with deviation case if stamping outside schedule (XETimeStamp)
	OnlyStampWithTag = 12,              // (bool) Only allow stamp with tag (XETimeStamp)
	ForceSocialSecNbr = 13,             // (bool) Social security number mandatory if creating a new employee from the terminal (XETimeStamp)
	UseAutoStampOut = 14,               // (bool) Create automatic stamp with type out if forgotten to stamp out at end of day
	UseAutoStampOutTime = 15,           // (int) Time on auto created stamp out. Stored as number of minutes from midnight (e.g. 1380 = 23:00)
	HideAttendanceView = 16,            // (bool) If attendance view should not be visible in terminal
	ShowTimeInAttendanceView = 17,      // (bool) Show stamp time in attendance view together with the employee name
	InternalAccountDim1Id = 18,         // (int) AccountId that will be set on time stamp
	ShowPicturesInAttendenceView = 19,  // (bool) Show employee image in attendance view (WebTimeStamp)
	IpFilter = 20,                      // (string) Valid IP-address(es) (WebTimeStamp, GoTimeStamp)
	AdjustTime = 21,                    // (int) Number of hours that will be added to the time stamp when saved (time zone handling for XETimeStamp, WebTimeStamp)
	ShowOnlyBreakAcc = 22,              // (bool) If set, only break accumulators are displayed (XETimeStamp)
	HideInformationButton = 23,         // (bool) Hide information button compleately (XETimeStamp)
	LimitTimeTerminalToAccount = 24,    // (bool) Valid employees for the terminal is limited to selected accounts
	LimitAccount = 25,                  // (string) Comma separated string of AccountIds
	ForceCorrectTypeTimelineOrder = 26, // (bool) If set, employee can't add two stamps of same type after each other (must be in, out, in, out...)
	OnlyDigitsInCardNumber = 27,        // (bool) If set, all characters except digits are removed from card number before identifying tag (XETimeStamp)
	
	// All settings below this line is only used by GoTimeStamp
	StartpageSubject = 28,                      // (string) Subject on start page
	StartpageShortText = 29,                    // (string) Short text on start page
	StartpageText = 30,                         // (string) Longer text on start page
	IdentifyType = 31,                          // (int) Possible ways to identify an employee (tag, employee number etc)
	Languages = 32,                             // (int) LanguageId (setting can exist multiple times, the one without ParentId is primary)
	ShowActionBreak = 33,                       // (bool) Show break button
	TimeZone = 34,                              // (string) Selected TimeZone, will convert time on server side
	ShowCurrentSchedule = 35,                   // (bool) Show todays schedule
	ShowNextSchedule = 36,                      // (bool) Show tomorrows schedule
	ShowBreakAcc = 37,                          // (bool) Show break accumulator
	ShowOtherAcc = 38,                          // (bool) Show other accumulators
	LimitSelectableAccounts = 39,               // (bool) Valid accounts for the terminal is limited to selected accounts
	SelectedAccounts = 40,                      // (string) Comma separated string of AccountIds
	ForceCauseGraceMinutesOutsideSchedule = 41, // (int) Minutes outside schedule that is valid without having to specify deviation cause
	ForceCauseGraceMinutesInsideSchedule = 42,  // (int) Minutes inside schedule that is valid without having to specify deviation cause
	ValidateNoSchedule = 43,                    // (bool) Show warning if employee stamp on a day without schedule
	ValidateAbsence = 44,                       // (bool) Show warning if employee stamp on a day with absence
	AttendanceViewSortOrder = 45,               // (int) Sort attendance view by name or time
	AttendanceViewUseSignalR = 46,              // DEPRECATED
	ShowNotificationWhenStamping = 47,          // (bool) Show a message on every stamp
	IgnoreForceCauseOnBreak = 48,               // (bool) Do not validate time for forced deviation cause when going on or coming back from a break
	TerminalGroupName = 49,                     // (string) Group name to be able to group terminals together. Employees will be visible in attendance views on all terminals that belongs to the same group
	ShowUnreadInformation = 50,                 // (bool) Will show that employee has unread information on identify
	ShowTimeStampHistory = 51,                  // (bool) Show button for time stamp history
	UseDistanceWork = 52,                       // (bool) Show button for distance work
	DistanceWorkButtonName = 53,                // (string) Name on button for stamp in on distance
	DistanceWorkButtonIcon = 54,                // (string) Icon on button for stamp in on distance
	BreakIsPaid = 55,                           // (bool) If set, a time block will be created during the break
	BreakTimeDeviationCause = 56,               // (int) TimeDeviationCauseId to be set on time block created by paid break
	BreakButtonName = 57,                       // (string) Label on break button
	BreakButtonIcon = 58,                       // (string) Icon on break button
	ShowActionBreakAlt = 59,                    // (bool) Show alternative (second) break button
	BreakAltIsPaid = 60,                        // (bool) If set, a time block will be created during the alternative break
	BreakAltTimeDeviationCause = 61,            // (int) TimeDeviationCauseId to be set on time block created by paid alternative break
	BreakAltButtonName = 62,                    // (string) Label on alternative break button
	BreakAltButtonIcon = 63,                    // (string) Icon on alternative break button
	LimitSelectableAdditions = 64,              // (bool) Valid additions for the terminal is limited to selected additions
	SelectedAdditions = 65,                     // (string) Comma and hash separated string of Additions of type IntKeyValue
	AccountDim2 = 66,                           // (int + string) AccountDimId and name for secondary selectable accounts in the terminal
	LimitSelectableAccounts2 = 67,              // (bool) Valid accounts for the terminal is limited to selected accounts
	SelectedAccounts2 = 68,                     // (string) Comma separated string of AccountIds
	AccountDimsInHierarchy = 69,                // (bool) Specifies if the AccountDims are in hierarchy or separately
	SelectAccountsWithoutImmediateStamp = 70,   // (bool) Will not automatically stamp out/in when selecting an account
	RememberAccountsAfterBreak = 71,            // (bool) Will load last stamp in and suggest same accounts when stamping in after a break
	LogLevel = 72,                              // (int) Log level for console logging
	LimitTimeTerminalToAccountDim = 73,         // (int) AccountDimId for accounts selected in LimitTimeTerminalToAccount
	
	// Company settings sent to terminal as terminal settings
	
	PossibilityToRegisterAdditionsInTerminal = 1001,    // (bool) Use TimeStampAdditions in terminal
	
	// License settings sent to terminal as terminal settings
	
	GtsHandshakeInterval = 2001,    // Interval in milliseconds between handshakes
	
	// Duplicate exists in the GoTimeStamp project!
}

export enum TimeTerminalSettingDataType {
	Unknown = 0,
	String = 1,
	Integer = 2,
	Decimal = 3,
	Boolean = 4,
	Date = 5,
	Time = 6
	
	// Duplicate exists in the GoTimeStamp project!
}

export enum UserSelectionType {
	Unknown = 0,
	
	// Copied from ReportUserSelectionType
	// TODO: We might replace ReportUserSelection with UserSelection later
	DataSelection = 1,
	AnalysisColumnSelection = 2,
	InsightsColumnSelection = 3,
	
	// Schedule planning
	TimeSchedulePlanningView_Calendar = 100,
	TimeSchedulePlanningView_Day = 101,
	TimeSchedulePlanningView_Schedule = 102,
	TimeSchedulePlanningView_TemplateDay = 103,
	TimeSchedulePlanningView_TemplateSchedule = 104,
	TimeSchedulePlanningView_EmployeePostsDay = 105,
	TimeSchedulePlanningView_EmployeePostsSchedule = 106,
	TimeSchedulePlanningView_ScenarioDay = 107,
	TimeSchedulePlanningView_ScenarioSchedule = 108,
	TimeSchedulePlanningView_StandbyDay = 109,
	TimeSchedulePlanningView_StandbySchedule = 110,
	TimeSchedulePlanningView_TasksAndDeliveriesDay = 111,
	TimeSchedulePlanningView_TasksAndDeliveriesSchedule = 112,
	TimeSchedulePlanningView_StaffingNeedsDay = 113,
	TimeSchedulePlanningView_StaffingNeedsSchedule = 114,
	
	// New schedule planning
	SchedulePlanningView_Day = 201,
	SchedulePlanningView_Schedule = 202,
}

export enum PayrollGroupSettingType {
	None = 0,
	Exception2to6InWorkingAgreement = 1,    // Undantag från §§ 2-6 i arbetstidsavtalet
	OverTimeCompensation = 2,               // Övertidskompensation
	TravelCompensation = 3,                 // Restidsersättning
	VacationRights = 4,                     // Semesterrätt (dagar)
	WorkTimeShiftCompensation = 5,          // Ersättning för förskjuten ordinarie arbetstid
	MonthlyWorkTime = 6,                    // Används för kalkylerad kostnad i planering
	PayrollReportsWorkTimeCategory = 7,     // Used in SN/KFO reporting
	PayrollReportsPersonalCategory = 8,     // Used in SN/KFO reporting
	PayrollReportsSalaryType = 9,           // Used in SN/KFO reporting
	PartnerNumber = 10,                     // Used in SN/KFO reporting
	AgreementCode = 11,                     // Used in SN/KFO reporting
	PayrollFormula = 12,                    // Used in SN/KFO reporting
	KPAAgreementNumber = 13,                // KPA Avtalsnummer
	ForaCollectiveAgreement = 14,           // Kollektivavtal Fora
	EarnedHoliday = 15,                     // Avtalet omfattas av intjänade röda dagar HRF
	ExperienceMonthsFormula = 16,
	KPAAgreementType = 17,                  // KPA Typ avtal
	KPABelonging = 18,                      // KPA Tillhörighet
	KPAFireman = 19,                        // KPA Brandman
	KPARetirementAge = 20,                  // KPA Pensionsålder (int 2 tecken)
	KPAPercentBelowBaseAmount = 21,         // KPA Procentsats för avgiftsbaserad ålderspension under 7,5 inkomstbasbelopp
	KPAPercentAboveBaseAmount = 22,         // KPA Procentsats för avgiftsbaserad ålderspension över 7,5 inkomstbasbelopp
	SicknessSalaryRegulation = 23,          // Rätten till sjuklön regleras enligt lagen 1991:1047
	BygglosenAgreementArea = 24,
	BygglosenAllocationNumber = 25,
	BygglosenSalaryFormula = 26,
	KPADirektSalaryFormula = 27,
	GTPAgreementNumber = 28,                // Folksam
	MonthlyWorkTimeCalculationType = 29,    // Beräkning av kostnad för månadsanställda
	SkandiaPensionType = 30,                    // Skandia Pension - Pensionsbestämmelse
	SkandiaPensionCategory = 31,                // Skandia Pension - Anställningskategori
	SkandiaPensionPercentBelowBaseAmount = 32,  // Skandia Pension - Procentsats för avgiftsbaserad ålderspension under 7,5 inkomstbasbelopp
	SkandiaPensionPercentAboveBaseAmount = 33,  // Skandia Pension - Procentsats för avgiftsbaserad ålderspension över 7,5 inkomstbasbelopp
	SkandiaPensionSalaryFormula = 34,           // SKandia Pension - Formula for salary amount
	ForaCategory = 35,                          // Fora - Anställningskategori
	SkandiaPensionStartDate = 36,               // SKandia Pension - Datum för uppstart
	ForaFok = 37,                          // Fora - Fok
	BygglosenSalaryType = 38,                // Bygglosen - Lönetyp
	ByggLosenWorkPlaceNr = 39,               // Bygglosen - Arbetsplatsnummer
	BygglosenAgreedHourlyPayLevel = 40,      // Bygglosen - Utbetalningsnivå per timme
	PayrollReportsJobStatus = 41,            // Used in SN/KFO reporting
}

export enum PushNotificationType {
	None = 0,
	XEMail = 1,
	Order = 2,
	SysInformation = 3,
	CompInformation = 4,
}

export enum PayrollPeriodChangeHeadType {
	Lock = 1,
	UnLock = 2,
}

export enum PayrollPeriodChangeRowField {
	//Table: EmployeeVacationSE
	UsedDaysPaid = 1,
	UsedDaysUnpaid = 2,
	UsedDaysAdvance = 3,
	UsedDaysYear1 = 4,
	UsedDaysYear2 = 5,
	UsedDaysYear3 = 6,
	UsedDaysYear4 = 7,
	UsedDaysYear5 = 8,
	UsedDaysOverdue = 9,
	PaidVacationAllowance = 10,
	DebtInAdvanceAmount = 11,
	PaidVacationVariableAllowance = 12,
	RemainingDaysAllowanceYear1 = 13,
	RemainingDaysAllowanceYear2 = 14,
	RemainingDaysAllowanceYear3 = 15,
	RemainingDaysAllowanceYear4 = 16,
	RemainingDaysAllowanceYear5 = 17,
	RemainingDaysAllowanceYearOverdue = 18,
	RemainingDaysVariableAllowanceYear1 = 19,
	RemainingDaysVariableAllowanceYear2 = 20,
	RemainingDaysVariableAllowanceYear3 = 21,
	RemainingDaysVariableAllowanceYear4 = 22,
	RemainingDaysVariableAllowanceYear5 = 23,
	RemainingDaysVariableAllowanceYearOverdue = 24,
	EarnedDaysPaid = 25,
	RemainingDaysPaid = 26,
}

export enum SettingDataType {
	Undefined = 0,
	String = 1,
	Integer = 2,
	Boolean = 3,
	Date = 4,
	Decimal = 5,
	Time = 6,
	Image = 7
};

export enum SettingMainType {
	User = 1,
	Company = 2,
	UserAndCompany = 3,
	Application = 4,
	License = 5
};

export enum UserSettingTypeGroup {
	All = 0,
	Core = 1,
	Accounting = 2,
	Supplier = 3,
	Customer = 4,
	Billing = 5,
	Time = 6,
	Employee = 7,  // Employee settings
}

export enum UserSettingType {
	
	CoreLangId = 1,
	CoreCompanyId = 2,
	CoreRoleId = 3,
	
	//Temp
	CoreHasUserVerifiedEmail = 31,
	
	//Misc
	//CoreShowFeedReader = 51,
	CoreFeedUrl = 52,
	CoreShowAnimations = 53,
	
	//ExternalAuthId (temp storage)
	ExternalAuthId = 55,
	
	//TTL on token
	LifetimeSeconds = 56,
	
	UseCollapsedMenu = 97,
	// Bootstrap (left) menu vs classic (top) menu
	UseBootstrapMenu = 98,
	
	// Angular vs Silverligt
	UseAngular = 99,
	
	
	
	//Core
	AccountingAccountYear = 101,
	AccountHierarchyId = 102,
	
	//VoucherSeries
	AccountingVoucherSeriesId = 111,
	VoucherSeriesSelection = 112,
	
	// Voucher
	KeepNewVoucherAfterSave = 121,
	
	// Liquidity planning
	LiquidityPlanningPreSelectUnpaid = 151,
	LiquidityPlanningPreSelectPaidUnchecked = 152,
	LiquidityPlanningPreSelectPaidChecked = 153,
	
	// Edi
	EdiOrdersAllItemsSelection = 171,
	EdiSupplierInvoicesAllItemsSelection = 172,
	
	// Inventory
	InventoryPreSelectedStatuses = 181,
	
	// Import/Export
	ExportedPaymentsAllItemsSelection = 191,
	
	
	
	// Invoice registration
	SupplierInvoiceSimplifiedRegistration = 201,    // Skip some fields when tabbing
	SupplierInvoiceAllItemsSelection = 202,
	SupplierInvoiceAllForeignItemsSelection = 203,
	SupplierPaymentsAllItemsSelection = 204,
	SupplierInvoicesAttestFlowMyClosed = 205,
	SupplierInvoiceShowInputFieldsWhenImage = 206,
	SupplierInvoiceCostOverviewAllItemsSelection = 207,
	SupplierInvoiceFinvoiceUnhandledSelection = 208,
	SupplierInvoiceSowPDFEnhanced = 209,
	SupplierInvoiceTransferInvoiceRows = 210,
	
	//PaymentExport
	PaymentExportType = 230,
	
	// PaymentImport
	SupplierPaymentImportAllItemsSelection = 241,
	
	
	
	CustomerInvoiceAllItemsSelection = 301,
	CustomerInvoiceAllForeignItemsSelection = 302,
	//ImportedInvoicesToClosedState
	CustomerInvoicesTransferTypesClosedAfterImport = 303,
	
	// PaymentImport
	CustomerPaymentImportAllItemsSelection = 321,
	
	
	
	// Invoice registration - Product rows
	BillingProductSearchFilterMode = 401,       // Product filter popup search mode
	BillingProductSearchMinPrefixLength = 402,  // Product filter popup will be displayed after this number of characters
	BillingProductSearchMinPopulateDelay = 403, // Product filter popup will be displayed after this number of milliseconds
	BillingDisableWarningPopupWindows = 404,    // Warnings regarding error retreiving price will not be displayed
	BillingShowWarningBeforeDeletingRow = 406,  // Show warning before deleting invoice row
	BillingCheckConflictsOnSave = 407,          // Check time conflicts when saving
	
	// Invoice registration - Our referece
	BillingInvoiceOurReference = 405,           // Our reference user id
	
	
	
	// Offer and Order status
	BillingOfferLatestAttestStateTo = 413,      // Senaste statusändringen för offert
	BillingOrderLatestAttestStateTo = 414,      // Senaste statusändringen för order
	
	// Order planning
	BillingOrderPlanningDefaultView = 421,
	BillingOrderPlanningDefaultInterval = 423,
	BillingOrderPlanningShiftInfoTopRight = 441,
	BillingOrderPlanningShiftInfoBottomLeft = 442,
	BillingOrderPlanningShiftInfoBottomRight = 443,
	
	// Open Expanders
	BillingSupplierInvoiceDefaultExpanders = 422,
	BillingOrderDefaultExpanders = 439,
	BillingInvoiceDefaultExpanders = 440,
	BillingOfferDefaultExpanders = 444,
	BillingContractDefaultExpanders = 445,
	
	BillingSupplierInvoiceSlider = 448,
	BillingSupplierAttestSlider = 449,
	
	BillingSupplierInvoiceScale = 6027,
	
	//
	BillingOrderIncomeRatioVisibility = 424,
	
	//ChangeStatusGridSelection
	BillingOfferAllItemsSelection = 431,
	BillingOfferAllForeignItemsSelection = 432,
	BillingOrderAllItemsSelection = 433,
	BillingOrderAllForeignItemsSelection = 434,
	BillingOfferShowOnlyMineSelection = 435,
	BillingOrderShowOnlyMineSelection = 436,
	BillingInvoiceShowOnlyMineSelection = 437,
	BillingContractShowOnlyMineSelection = 446,
	BillingContractAllItemsSelection = 447,
	BillingHandleBillingOnlyMine = 450,
	BillingHandleBillingOnlyValid = 451,
	
	BillingInvoiceDefaultHouseholdTaxType = 452,
	BillingInvoiceDefaultHouseholdPrintButtonOption = 462,
	BillingDefaultStockPlace = 453,
	BillingUseOneTimeCustomerAsDefault = 454,
	
	BillingPurchaseAllItemsSelection = 455,
	BillingDefaultOrderType = 456,              // User default order type
	BillingMergePdfs = 457,
	BillingOfferShowVatFree = 458,
	BillingOrderShowVatFree = 459,
	BillingInvoiceShowVatFree = 460,
	BillingContractShowVatFree = 461,
	
	//Project 471-480
	ProjectDefaultExcludeMissingCustomer = 471,
	ProjectUseDetailedViewInProjectOverview = 472,
	ProjectUseChildProjectsInProjectOverview = 473,
	
	//Purchase 481-500
	DeliveredQtySameAsPurchased = 481,
	
	
	
	//Attest (TimePeriodDeviation)
	/*
	TimeMinimizeLeftContent = 501,
	TimeMaximizeTimeDeviation = 502,
	*/
	TimeDisableApplyRestoreWarning = 502,                       // Disable warning when restoring days
	TimeHideZeroScheduleDays = 503,
	TimeLatestAttestStateTo = 504,
	TimeLatestTimePeriodType = 505,
	TimeSchedule = 506,
	TimeHideSchedule = 507,
	TimeShowTemplateSchedule = 508,
	TimeHideUnchangedTemplateScheduleDays = 509,
	TimeDisableApplySaveAttestWarning = 510,                    // Disable warning when attesting days
	
	// TimeSchedulePlanning 511-530
	TimeSchedulePlanningDefaultView = 511,
	TimeSchedulePlanningDefaultInterval = 525,
	TimeSchedulePlanningCalendarViewShowToolTipInfo = 512,
	TimeSchedulePlanningDontSendXEMailOnChange = 513,
	TimeSchedulePlanningDisableCheckBreakTimesWarning = 514,        // Disable warning message when dragging a shift
	TimeSchedulePlanningShowAvailability = 515,                     // NOT USED
	TimeSchedulePlanningShowEmployeeImageInToolTip = 526,           // NOT USED
	TimeSchedulePlanningShowScheduledTimeSummaryOnEmployee = 516,   // NOT USED
	TimeSchedulePlanningCalendarViewCountType = 517,
	TimeSchedulePlanningStartWeek = 518,
	TimeSchedulePlanningShowEmployeeList = 519,
	TimeSchedulePlanningDisableBreaksWithinHolesWarning = 520,
	TimeSchedulePlanningDisableAutoLoad = 521,                      // Do not initially load any shifts
	TimeSchedulePlanningDisableSaveOnNavigateWarning = 522,
	TimeSchedulePlanningDefaultShiftStyle = 523,
	TimeSchedulePlanningDisableTemplateScheduleWarning = 524,       // Disable warning message in template schedule view
	TimeSchedulePlanningDayViewDefaultGroupBy = 527,
	TimeSchedulePlanningDayViewDefaultSortBy = 528,
	TimeSchedulePlanningScheduleViewDefaultGroupBy = 529,
	TimeSchedulePlanningScheduleViewDefaultSortBy = 530,
	
	// Contract, Order and Invoice column settings 531-540
	//ShowOrderDateColumn = 531,
	//ShowDeliveryDateColumn = 532,
	//ShowProjectNumberColumn = 533,
	//ShowRemainingAmountIncVatColumn = 534,
	//ShowRemainingAmountExVatColumn = 535,
	//ShowOrderNumbersColumn = 536,
	//ShowInvoiceDeliveryTypeColumn = 537,
	//ShowOrderAmountIncVatColumn = 538,
	//ShowOrderAmountExVatColumn = 539,
	//ShowInvoiceAmountIncVatColumn = 540,
	//Continue on 581
	//ShowInvoiceAmountExVatColumn = 581,
	//ShowContractAmountIncVatColumn = 582,
	//ShowContractAmountExVatColumn = 583,
	//ShowOrderTotalAmountIncVatColumn = 584,
	//ShowOrderTotalAmountExVatColumn = 585,
	//ShowOrderPaymentServiceColumn = 586,
	//ShowInvoicePaymentServiceColumn = 587,
	//ShowContractPaymentServiceColumn = 588,
	
	// TimeSheet 541-550
	TimeSheetShowWeekend = 541,
	
	// PayrollCalculation/Attest 551-590
	PayrollLatestAttestStateTo = 551,
	PayrollLatestTimePeriodType = 552,
	PayrollShowDetailedInfo = 553,
	PayrollDisableApplyRecalculateWarning = 554,                  // Disable warning when recalculating period
	PayrollDisableApplySaveAttestWarning = 555,                   // Disable warning when attesting days
	PayrollCalculationTreeDoNotShowCalculated = 556,
	TimeAttestTreeLatestGrouping = 561,
	TimeAttestTreeLatestSorting = 562,
	PayrollCalculationTreeLatestGrouping = 563,
	PayrollCalculationTreeLatestSorting = 564,
	PayrollCalculationDisableApplySaveAttestWarning = 565,        // Disable warning when attesting days
	TimeAttestTreeDisableAutoLoad = 566,
	PayrollCalculationTreeDisableAutoLoad = 567,
	TimeAttestTreeDoNotShowAttested = 568,
	TimeAttestTreeDoNotShowWithoutTransactions = 569,
	TimeAttestTreeDoShowEmptyGroups = 570,
	PayrollReviewDisableShowUpdateInfo = 571,
	TimeAttestTreeDoShowOnlyWithWarnings = 572,
	TimeAttestTreeIncludeAdditionalEmployees = 573,               //Not used for now, overrided by company setting
	TimeAttestTreeDoNotShowDaysOutsideEmployeeAccount = 574,
	PayrollCalculationDisableSaveAttestWarning = 575,
	PayrollCalculationDisableRecalculatePeriodWarning = 576,
	PayrollCalculationDisableRecalculateAccountingWarning = 577,
	PayrollCalculationDisableRecalculateExportedEmploymentTaxWarning = 578,
	PayrollCalculationDisableGetUnhandledTransactionsBackwardsWarning = 579,
	PayrollCalculationDisableGetUnhandledTransactionsForwardsWarning = 580,
	PayrollCalculationTreeDoShowOnlyWithWarnings = 581, //TODO: Delete
	PayrollCalculationTreeWarningFilter = 582,
	//Do not use (Earlier: ShowContractAmountExVatColumn = 583),
	//Do not use (Earlier: ShowOrderTotalAmountIncVatColumn = 584),
	//Do not use (Earlier: ShowOrderTotalAmountExVatColumn = 585),
	//Do not use (Earlier: ShowOrderPaymentServiceColumn = 586),
	//Do not use (Earlier: ShowInvoicePaymentServiceColumn = 587),
	//Do not use (Earlier: ShowContractPaymentServiceColumn = 588),
	TimeAttestTreeMessageGroupId = 589,
	TimeAttestTreeShowOnlyShiftSwaps = 590,
	
	// StaffingNeeds
	StaffingNeedsDayViewShowDiagram = 701,
	StaffingNeedsDayViewShowDetailedSummary = 702,
	StaffingNeedsScheduleViewShowDetailedSummary = 703,
	
	// Employee
	EmployeeGridDisableAutoLoad = 711,
	EmployeeGridShowEnded = 712,
	EmployeeGridShowNotStarted = 713,
	
	// More TimeSchedulePlanning
	TimeSchedulePlanningSelectableInformationSettingsCalendarView = 751,
	TimeSchedulePlanningSelectableInformationSettingsDayView = 752,
	TimeSchedulePlanningSelectableInformationSettingsScheduleView = 753,
	TimeSchedulePlanningSelectableInformationSettingsTemplateDayView = 754,
	TimeSchedulePlanningSelectableInformationSettingsTemplateScheduleView = 755,
	TimeSchedulePlanningSelectableInformationSettingsEmployeePostDayView = 756,
	TimeSchedulePlanningSelectableInformationSettingsEmployeePostScheduleView = 757,
	TimeSchedulePlanningSelectableInformationSettingsScenarioDayView = 758,
	TimeSchedulePlanningSelectableInformationSettingsScenarioScheduleView = 759,
	TimeSchedulePlanningSelectableInformationSettingsStandbyDayView = 760,
	TimeSchedulePlanningSelectableInformationSettingsStandbyScheduleView = 761,
	TimeSchedulePlanningSelectableInformationSettingsTasksAndDeliveriesDayView = 762,
	TimeSchedulePlanningSelectableInformationSettingsTasksAndDeliveriesScheduleView = 763,
	TimeSchedulePlanningSelectableInformationSettingsStaffingNeedsDayView = 764,
	TimeSchedulePlanningSelectableInformationSettingsStaffingNeedsScheduleView = 765,
	
	
	
	//UserGrid
	ManageUserGridShowInactive = 801,
	ManageUserGridShowEnded = 802,
	ManageUserGridShowNotStarted = 803,
	
};

export enum UserReplacementType {
	Unknown = 0, //Otherwise it cant be serialized through wcf
	AttestFlow = 1,
}

export enum CompanySettingType {
	Unknown = 0,
	
	
	//Currency
	CoreBaseCurrency = 1,
	CoreBaseEntCurrency = 2,
	
	// Password
	CorePasswordMinLength = 11,
	CorePasswordMaxLength = 12,
	
	//Intrum settings
	IntrumClientNo = 30,
	IntrumHubNo = 31,
	IntrumLedgerNo = 32,
	IntrumTestMode = 33,
	IntrumUser = 34,
	IntrumPwd = 35,
	
	//Kivra
	KivraTenentKey = 36,
	
	//Finvoice
	FinvoiceUseBankIntegration = 37,
	
	//Ftp-settings for e-invoicing
	InExchangeFtpAddressTest = 40,      //test settings
	InExchangeFtpUsernameTest = 41,
	InExchangeFtpPasswordTest = 42,
	InExchangeFtpAttachmentAddressTest = 46,
	InExchangeFtpAddress = 43,          //production settings
	InExchangeFtpUsername = 44,
	InExchangeFtpPassword = 45,
	InExchangeFtpAttachmentAddress = 47,
	
	//APi for InExchange
	InExchangeAPISendRegistered = 48,
	InExchangeAPIReciveRegistered = 57,
	InExchangeAPIActivated = 49,
	InExchangeAPIKey = 54,  //Api key for production
	InExchangeAPIRegisteredDate = 60,
	InExchangeLastStatusFetchDate = 62,
	
	// Misc
	CoreCompanyLogo = 51,                       // Företags logotyp
	DashboardRefreshInterval = 52,
	UseDeliveryAddressAsInvoiceAddress = 53,    //If you wan't to use deliveryadress as invoiceaddress Task 14728
	//54, Do not use, InExchangeAPIKey = 54
	UseProjectTimeBlocks = 55,                  // Temporary setting to enable some companies to use the new time project registration
	UseMissingMandatoryInformation = 56,
	
	MaxAllowedSimultaneousPrintJobsPerUser = 59,
	
	// EntityHistory
	CoreEntityHistoryInterval = 61,
	
	//Zetes
	ZetesUser = 63,
	ZetesPwd = 64,
	ZetesClientCode = 65,
	ZetesStakeholderCode = 66,
	ZetesTestMode = 67,
	
	ChainAffiliation = 68,
	
	//Email
	DefaultEmailAddress = 71,
	UseDefaultEmailAddress = 72,
	UseCommunicator = 73,
	DisableMessageOnInboundEmailError = 74,
	
	//External links
	UseExternalLinks = 81,
	ExternalLink1 = 82,
	ExternalLink2 = 83,
	ExternalLink3 = 84,
	
	//Module
	TimeModuleHeader = 91,
	TimeModuleIcon = 92,
	
	// Bootstrap (left) menu vs classic (top) menu
	UseBootstrapMenu = 98,
	
	// Angular vs Silverligt
	UseAngular = 99,
	
	
	
	//Year and Period
	AccountingMaxYearOpen = 106,
	AccountingMaxPeriodOpen = 101,
	AccountingAllowMultiplePeriodChange = 102,
	
	//Voucher
	AccountingAllowUnbalancedVoucher = 103,
	AccountingAllowEditVoucher = 104,
	AccountingAllowEditVoucherDate = 109,
	AccountingUseQuantityInVoucher = 105,
	AccountingDisableInlineValidation = 107,
	AccountingAutomaticAccountDistribution = 108,   // Generera automatkontering utan fråga
	AccountingAllowUnbalancedAccountDistribution = 110,
	AccountingSeparateVouchersInPeriodAccounting = 113,
	AccountingCreateVouchersForStockTransactions = 114,
	AccountingCreateDiffRowOnBalanceTransfer = 115,
	
	//VoucherSeries
	AccountingVoucherSeriesTypeManual = 111,
	AccountingVoucherSeriesTypeVat = 112,
	AccountdistributionVoucherSeriesType = 172,
	
	//Accruals
	AccountCommonAccrualCostAccount = 163,
	AccountCommonAccrualRevenueAccount = 164,
	
	// Currency
	AccountingShowEnterpriseCurrency = 117,
	
	//Invoice to Voucher
	AccountingInvoiceToVoucherHeadType = 121,
	AccountingInvoiceToVoucherRowType = 122,
	
	//Reports
	AccountingDefaultAccountingOrder = 131,
	AccountingDefaultVoucherList = 132,
	AccountingDefaultAnalysisReport = 133,
	
	//Currency
	AccountingCurrencySource = 141,
	AccountingCurrencyIntervalType = 142,
	
	//Budget
	AccountingBudgetUseForecasts = 143,
	
	//Import
	AccountingVoucherImportVoucherSerie = 161,
	AccountingVoucherImportStandardAccount = 162,
	
	//Dims
	AccountingUseDimsInRegistration = 171,
	MapCompanyToAccountDimInConsolidation = 173,
	
	// VAT
	AccountingDefaultVatCode = 182,
	
	// Intrastat
	IntrastatImportOriginType = 181,
	
	
	
	//VoucherSeries
	SupplierInvoiceVoucherSeriesType = 201,
	SupplierPaymentVoucherSeriesType = 202,
	
	// Currency
	SupplierShowTransactionCurrency = 206,
	SupplierShowEnterpriseCurrency = 207,
	SupplierShowLedgerCurrency = 208,
	
	//Invoice
	SupplierInvoiceTransferToVoucher = 211,
	SupplierInvoiceAskPrintVoucherOnTransfer = 293,
	SupplierInvoiceDefaultDraft = 212,
	SupplierInvoiceAllowInterim = 213,
	SupplierInvoiceAllowEditOrigin = 214,
	SupplierCloseInvoicesWhenTransferredToVoucher = 215,
	SupplierSetPaymentDefaultPayDateAsDueDate = 216,
	SupplierUsePaymentSuggestions = 217,
	SupplierInvoiceTransferToVoucherOnAcceptedAttest = 218,
	SupplierUseTimeDiscount = 297,
	SupplierHideAutogiroInvoicesFromUnpaid = 298,
	SupplierInvoiceAllowEditAccountingRows = 299,
	UseInternalAccountsWithBalanceSheetAccounts = 7203,
	SupplierInvoiceUseQuantityInAccountingRows = 7213,
	SupplierInvoiceUseAutoAccountDistributionOnVoucher = 7217,
	
	//Payment
	SupplierPaymentDefaultPaymentCondition = 221,   // Betalningsvillkor
	SupplierPaymentDefaultPaymentMethod = 223,      // Betalmetod
	SupplierPaymentSettlePaymentMethod = 7201,      // Utjämnings betalmetod
	SupplierPaymentManualTransferToVoucher = 222,   // Om leverantörsbetalningar ska föras över direkt till verifikat
	SupplierInvoicesShowOnlyAttestedAtUnpayed = 287,
	CreateAutoAttestFromSupplierOnEDI = 288,   // Skapa attest automatiskt på edi fakturor om attestmall finns på leverantören
	SaveSupplierInvoiceAttestType = 289,         // Spara lev.faktura som preliminär eller underlag
	SupplierPaymentObservationMethod = 291,          // Bevakningstyp
	SupplierPaymentObservationDays = 292,          // Bevakningsdagar
	SupplierPaymentAskPrintVoucherOnTransfer = 294,
	SupplierPaymentForeignBankCode = 296,            // Bank for International payments
	SupplierPaymentNotificationRecipientGroup = 7215,
	SupplierAggregatePaymentsInSEPAExportFile = 224,
	
	//Invoice
	SupplierInvoiceSeqNbrPerType = 231,
	SupplierDefaultBalanceList = 232,
	SupplierDefaultChecklistPayments = 7208,
	SupplierInvoiceSeqNbrStart = 233,
	SupplierInvoiceSeqNbrStartDebit = 234,
	SupplierInvoiceSeqNbrStartCredit = 235,
	SupplierInvoiceSeqNbrStartInterest = 236,
	SupplierInvoiceAutomaticAccountDistribution = 237,  // Generera automatkontering utan fråga
	SupplierInvoiceDefaultVatType = 238,                // Standard momstyp
	SupplierDefaultPaymentSuggestionList = 239,
	SupplierInvoiceSeqNbrStartClaim = 240,
	SupplierInvoiceKeepSupplier = 7202,
	SupplierInvoiceRoundVAT = 7204,
	SupplierInvoiceGetInternalAccountsFromOrder = 7205,
	SupplierInvoiceAutoTransferAutogiroInvoicesToPayment = 7206,
	SupplierInvoiceAutoTransferAutogiroPaymentsToVoucher = 7207,
	
	//EDI
	ScanningTransferToSupplierInvoice = 241,
	FinvoiceTransferToSupplierInvoice = 242,
	ScanningCloseWhenTransferedToSupplierInvoice = 243,
	ScanningReferenceTargetField = 247,
	ScanningCodeTargetField = 248,
	FinvoiceUseTransferToOrder = 249,
	FinvoiceImportOnlyForCompany = 250,
	ScanningCalcDueDateFromSupplier = 7212,
	
	//Scanning
	ScanningUsesAzoraOne = 7214,
	
	//Product rows
	SupplierInvoiceProductRowsImport = 7209,
	SupplierInvoiceDetailedCodingRowsBasedOnProductRows = 7216,
	
	//Invoice to Voucher
	SupplierInvoiceToVoucherHeadType = 251,
	SupplierInvoiceToVoucherRowType = 252,
	SupplierPaymentToVoucherHeadType = 253,
	SupplierPaymentToVoucherRowType = 254,
	
	//SupplierInvoice AttestFlow
	SupplierInvoiceAttestFlowStatusToStartAccountsPayableFlow = 261,
	SupplierInvoiceAttestFlowDefaultAttestTemplate = 262,
	SupplierInvoiceAttestFlowAmountWhenUserIdIsRequired = 263,
	SupplierInvoiceAttestFlowUserIdRequired = 264,
	SupplierInvoiceAttestFlowDueDays = 265,
	SupplierInvoicesShowNonAttestedInvoices = 266,
	SupplierInvoiceAttestFlowProjectLeaderLevel = 267,
	SupplierInvoiceAttestFlowDefaultAttestGroup = 268,
	SupplierInvoiceAttestGroupSuggestionPrio1 = 269,
	SupplierInvoiceAttestGroupSuggestionPrio2 = 270,
	SupplierInvoiceAttestGroupSuggestionPrio3 = 245,
	SupplierInvoiceAttestGroupSuggestionPrio4 = 246,
	
	// Age distribution
	SupplierInvoiceAgeDistributionNbrOfIntervals = 271,
	SupplierInvoiceAgeDistributionInterval1 = 272,
	SupplierInvoiceAgeDistributionInterval2 = 273,
	SupplierInvoiceAgeDistributionInterval3 = 274,
	SupplierInvoiceAgeDistributionInterval4 = 275,
	SupplierInvoiceAgeDistributionInterval5 = 276,
	
	// Liquidity planning
	SupplierInvoiceLiquidityPlanningNbrOfIntervals = 281,
	SupplierInvoiceLiquidityPlanningInterval1 = 282,
	SupplierInvoiceLiquidityPlanningInterval2 = 283,
	SupplierInvoiceLiquidityPlanningInterval3 = 284,
	SupplierInvoiceLiquidityPlanningInterval4 = 285,
	SupplierInvoiceLiquidityPlanningInterval5 = 286,
	
	// Batch onward invoicing
	SupplierInvoiceBatchOnwardInvoicingOrderTemplate = 7210,
	SupplierInvoiceBatchOnwardInvoicingAttachImage = 7211,
	
	// Misc
	SupplierInvoiceReportShowPendingPayments = 290,
	
	//FI
	FISupplierInvoiceOCRCheckReference = 295,
	
	SupplierInvoiceAutoReminder = 244,
	
	
	
	
	//VoucherSeries
	CustomerInvoiceVoucherSeriesType = 301,
	CustomerPaymentVoucherSeriesType = 302,
	
	// Currency
	CustomerShowTransactionCurrency = 306,
	CustomerShowEnterpriseCurrency = 307,
	CustomerShowLedgerCurrency = 308,
	
	//Invoice
	CustomerInvoiceTransferToVoucher = 311,
	CustomerInvoiceAskPrintVoucherOnTransfer = 378,
	CustomerInvoiceDefaultDraft = 312,
	CustomerInvoiceAllowEditOrigin = 313,
	CustomerInvoiceSeqNbrPerType = 314,
	CustomerInvoiceTemplate = 315,                                  // Fakturamall
	CustomerDefaultBalanceList = 316,
	CustomerInvoiceOurReference = 317,
	CustomerInvoiceDnBNorClientId = 380,
	CustomerInvoiceSeqNbrStart = 323,
	CustomerInvoiceSeqNbrStartDebit = 324,
	CustomerInvoiceSeqNbrStartCredit = 325,
	CustomerInvoiceSeqNbrStartInterest = 326,
	CustomerInvoiceAutomaticAccountDistribution = 327,              // Generera automatkontering utan fråga
	CustomerInvoiceDefaultVatType = 328,                            // Standard momstyp
	CustomerCloseInvoicesWhenTransferredToVoucher = 330,            // För över fakturor till huvudboken
	CustomerCloseInvoicesWhenExported = 377,                        // Stäng vid export
	CustomerDefaultCreditLimit = 357,
	CustomerInvoiceSeqNbrStartClaim = 358,
	CustomerDefaultReminderTemplate = 359,                          // Standard kravbrevsmall
	CustomerDefaultInterestTemplate = 360,                          // Standard räntefakturamall
	CustomerDefaultInterestRateCalculationTemplate = 390,           // Standard ränteberäkningmall
	CustomerInvoiceSeqNbrStartCash = 347,                           // Startnumber for cash invoices
	CustomerInvoiceUseDeliveryCustomer = 381,                         // Use delivery customer field in invoicing
	CustomerInvoiceAutogiroClientId = 382,
	CustomerPaymentServiceOnlyToContract = 383,
	CustomerInvoiceAllowEditAccountingRows = 384,                   //Allow changing of invoice's accounting rows even when payment and/or voucher is made (and period is still open)
	CustomerInvoiceCombineImportedAccountingRows = 385,             //Combine accounting rows in import (Automaster)
	CustomerInvoiceUseAutomaticDistributionInImport = 386,          //Use automatic distribution when importing invoice accounting rows (Automaster)
	CustomerInvoiceTransferTypesToBeClosed = 387,                   //List of transfer types which will close the invoice after creating voucher (Automaster)
	CustomerInvoiceCashCustomerNumber = 388,
	CustomerInvoiceTriangulationSales = 389,
	CustomerInvoiceUseAutoAccountDistributionOnVoucher = 393,
	AllowChangesToInternalAccountsOnPaidCustomerInvoice = 394,
	AutomaticLedgerInvoiceNrWhenImport = 395,
	CustomerInvoiceApplyQuantitiesDuringInvoiceEntry = 396,
	
	//Payment
	CustomerPaymentDefaultPaymentCondition = 321,                   // Betalningsvillkor
	CustomerPaymentManualTransferToVoucher = 322,                   // Om kundbetalningar ska föras över direkt till verifikat
	CustomerPaymentDefaultPaymentConditionHouseholdDeduction = 329, //Betalningsvillkor vid husavdrag
	CustomerPaymentDefaultPaymentMethod = 376,                      //Standard betalningsmethod
	CustomerPaymentAskPrintVoucherOnTransfer = 379,
	CustomerPaymentSettlePaymentMethod = 391,      // Utjämnings betalmetod
	CustomerPaymentAddCustomerNameToInternaDescr = 392,
	
	//Invoice to Voucher
	CustomerInvoiceToVoucherHeadType = 331,
	CustomerInvoiceToVoucherRowType = 332,
	CustomerPaymentToVoucherHeadType = 333,
	CustomerPaymentToVoucherRowType = 334,
	
	// Age distribution
	CustomerInvoiceAgeDistributionNbrOfIntervals = 341,
	CustomerInvoiceAgeDistributionInterval1 = 342,
	CustomerInvoiceAgeDistributionInterval2 = 343,
	CustomerInvoiceAgeDistributionInterval3 = 344,
	CustomerInvoiceAgeDistributionInterval4 = 345,
	CustomerInvoiceAgeDistributionInterval5 = 346,
	
	// Liquidity planning
	CustomerInvoiceLiquidityPlanningNbrOfIntervals = 351,
	CustomerInvoiceLiquidityPlanningInterval1 = 352,
	CustomerInvoiceLiquidityPlanningInterval2 = 353,
	CustomerInvoiceLiquidityPlanningInterval3 = 354,
	CustomerInvoiceLiquidityPlanningInterval4 = 355,
	CustomerInvoiceLiquidityPlanningInterval5 = 356,
	
	// Interest and Reminder
	CustomerGracePeriodDays = 361,                                  // Tidsfrist innan ränta påskrivs
	CustomerInterestAccumulatedBeforeInvoice = 362,                 // Belopp innan ränta faktureras
	CustomerInterestHandlingType = 363,                             // Om ränta skall generera ny faktura eller skrivas till nästkommande
	CustomerInterestPercent = 364,                                  // Ränta %
	CustomerReminderGenerateProductRow = 365,                       // Skapa artikelrad med påminnelseavgift
	CustomerReminderHandlingType = 366,                             // Om påminnelse skall generera ny faktura eller skrivas till nästkommande
	//CustomerInterestLeaveOverduedFullyPayedForInterest = 367,     //Lämna för sent betalda fakturor i förfallna för räntefaktureringar (Not used since 2012-05-08)
	CustomerClaimLevel1Text = 368,                                  // Kravnivå ett text
	CustomerClaimLevel2Text = 369,                                  // Kravnivå två text
	CustomerClaimLevel3Text = 370,                                  // Kravnivå tre text
	CustomerClaimLevel4Text = 371,                                  // Kravnivå fyra text
	CustomerClaimLevelDebtCollectionText = 372,                     // Kravnivå inkasso (kravet efter BillingNrOfClaimLevels)
	CustomerReminderMinNrOfDaysForNewClaim = 373,                   // Minsta antal dagar för att få skicka ett nytt kravbrev till samma kund
	CustomerReminderNrOfClaimLevels = 374,                          // Antal kravnivåer som företaget använder.
	CustomerDefaultPaymentConditionClaimAndInterest = 375,          // Betalningsvillkor vid krav och räntefakturering.
	
	
	
	//Common
	BillingDefaultVatCode = 480,                        // Standard momskod
	BillingUseExternalProductInfoLink = 6002,                // Show external product info link in productrows
	BillingUseAzureSearch = 6100,
	BillingUseQuantityPrices = 6025,                    //Show quantity fields in pricehandling and then recalculate prices when changeing quantity
	
	// Offer
	BillingNbrOfOfferCopies = 447,                      // Antal offertkopior vid utskrift
	BillingHideRowsTransferredToOrderInvoiceFromOffer = 461, // Dölja rader överförda till faktura eller order från offert
	BillingOfferSeqNbrStart = 6001,                      // Startnummer för löpnummerserien
	
	// Order
	BillingNbrOfOrderCopies = 446,                      // Antal orderkopior vid utskrift
	BillingOrderAskForWholeseller = 452,                // Fråga efter grossist vid val av artikel (order)
	BillingHideRowsTransferredToInvoiceFromOrder = 462, // Dölja rader överförda till faktura från order
	BillingOrderSeqNbrStart = 466,                      // Startnummer för löpnummerserien
	BillingOrderMergeOrderWithExtendedInfo = 411,
	BillingOrderIncludeTimeProjectinReport = 490,      //Inkludera och visa tidboken i samma XML som order
	BillingHideStatusOrderReadyForMobile = 494,
	BillingAskOpenInvoiceWhenCreateInvoiceFromOrder = 497,
	BillingAskCreateInvoiceWhenOrderReady = 498,
	BillingUseProductGroupCustomerCategoryDiscount = 415,
	BillingInternalAccountsFromUser = 430,              //Get internal accounts from user (owner)
	BillingAutoCreateDateOnProductRows = 6007,
	BillingCalculateMarginalIncomeForRowsWithZeroPurchasePrice = 6012,
	BillingSetOurReferenceOnMergedInvoices = 6019,
	BillingShowExtendedInfoInExternalSearch = 6034,     // Product groups and extend info external search grid
	BillingOrderSeqNbrStartInternal = 6102,
	BillingOrderUseSeqNbrForInternal = 6103,
	BillingCreateSubtotalRowInConsolidatedInvoices = 6105,
	BillingUseAdditionalDiscount = 6106,
	BillingShowImportProductRows = 6107,
	
	// Invoice
	BillingUseInvoiceFee = 401,                         // Använd fakturerinsavgift
	BillingUseInvoiceFeeLimit = 474,                    // Använd avgränsning för faktureringsavgift
	BillingUseInvoiceFeeLimitAmount = 475,              // Belopp för avgränsning
	BillingUseFreightAmount = 471,                      // Använd frakt
	BillingUseCentRounding = 404,                       // Använd öresutjämning
	BillingUseCashSales = 477,                          // Company uses cash sales
	BillingCopyInvoiceNrToOcr = 402,                    // OCR samma som fakturanummer
	BillingDefaultPriceListType = 403,                  // Standardprislista
	BillingInvoiceNumberLength = 405,                   // Antal tecken i fakturanummer, används i postgiro import
	BillingInvoiceText = 406,                           // Fakturatext som visas på alla fakturor, kan skrivas över på respektive faktura
	BillingDefaultDeliveryType = 408,                   // Leveranssätt
	BillingDefaultDeliveryCondition = 409,              // Leveransvillkor
	BillingDefaultInvoiceProductVatType = 410,          // Typ
	BillingDefaultInvoiceProductUnit = 418,             // Enhet
	BillingStandardMaterialCode = 476,                  // Standard materialkod
	BillingMergeInvoiceProductRowsMerchandise = 419,    // Slå ihop artikelrader med samma artiklar (varor)
	BillingMergeInvoiceProductRowsService = 420,        // Slå ihop artikelrader med samma artiklar (tjänster)
	BillingNbrOfCopies = 421,                           // Antal fakturakopior vid utskrift
	BillingDefaultWholeseller = 423,                    // Grossist
	BillingInitProductSearch = 425,                     // Starta sökning av externa artiklar (Automatiskt eller med Enter)
	BillingPrintTaxBillText = 434,                      // Skriva ut "F-skattsedel" i fakturautskriften
	BillingShowOrdernrOnInvoiceReport = 444,            // Om ordernr ska vara med i Fakturarapporten
	BillingOfferValidNoOfDays = 6031,
	//BillingEnableCunsultec = 445,                       // Should Consultec pricelists be available
	BillingProductRowMarginalLimit = 449,               // Marginal income (%) below this value will turn row red
	BillingInvoiceAskForWholeseller = 451,              // Fråga efter grossist vid val av artikel (faktura)
	BillingInvoiceShowOnlyProductNumber = 482,          // Show only productnumber in productnumber field (by default both name and productnumber is shown)
	BillingCCInvoiceMailToSelf = 453,                   // Ska kopia på email skickas till det egna företaget då faktura mailas till kund
	BillingOfferAutoSaveInterval = 456,                 // Automatspara offert (antal minuter)
	BillingOrderAutoSaveInterval = 457,                 // Automatspara order (antal minuter)
	BillingInvoiceAutoSaveInterval = 458,               // Automatspara faktura (antal minuter)
	BillingContractAutoSaveInterval = 459,              // Automatspara avtal (antal minuter)
	BillingFormReferenceNumberToOCR = 467,              // RF - Reference to OCR field
	BillingFormFIReferenceNumberToOCR = 412,            // FI - Reference to OCR field (Selection overrides 467 to form Finnish Reference without RF part. Requires RF to be selected also
	BillingShowCOLabelOnReport = 473,                   // Visar en rubrik C/O, C/O adressen skrivs dock alltid ut.
	BillingHideWholesaleSettings = 468,                 // Dölj entreprenadspecifika fält
	BillingHideVatWarnings = 479,                       // Dölj momsvarning
	BillingShowZeroRowWarning = 481,                    // Visa varning för nollrader och negativt tb
	BillingIncludeWorkDescriptionOnInvoice = 483,
	BillingDontUseCashCustomerAsDefault = 484,          //Dont use cashcustomer as default when creating new invoice
	BillingMandatoryChecklist = 485,
	BillingPrintChecklistWithOrder = 486,
	BillingAutomaticCustomerOwner = 487,
	BillingIncludeRemainingAmountOnInvoice = 488,
	BillingUsePartialInvoicingOnOrderRow = 489,
	BillingIncludeTimeProjectinReport = 491,            //Inkludera och visa tidboken i samma XML som fakturan
	BillingHideVatRate = 492,                           // Dölj momssatsen
	BillingCustomerHideTaxDeductionContacts = 6101,
	BillingDefaultHouseholdDeductionType = 500,
	BillingDefaultOneTimeCustomer = 6011,
	BillingCCInvoiceMailAddress = 6013,
	BillingBCCInvoiceMailAddress = 6014,
	BillingUseExternalInvoiceNr = 6026,
	BillingShowPurchaseDateOnInvoice = 6042,
	
	BillingHideArticleNrOnSvefaktura = 499,
	//Stock
	BillingDefaultStock = 413,                          // Stock (BillingNotUsed4 = 413,)
	// Contract
	BillingNbrOfContractCopies = 448,                   // Antal avtalskopior vid utskrift
	BillingContractSeqNbrStart = 6018,                  // Startnummer för löpnummerserien
	
	// Reports
	BillingDefaultInvoiceTemplate = 407,                // Standard fakturamall
	BillingDefaultTimeProjectReportTemplate = 424,      // TidProjekt rapport
	BillingDefaultEmailTemplate = 426,                  // Faktura mall för epost
	BillingOfferDefaultEmailTemplate = 6028,            // Faktura mall för epost offert
	BillingOrderDefaultEmailTemplate = 6029,            // Faktura mall för epost order
	BillingContractDefaultEmailTemplate = 6030,         // Faktura mall för epost avtal
	BillingDefaultOrderTemplate = 427,                  // Standard ordermall
	BillingDefaultWorkingOrderTemplate = 478,           // Standard arbetsordermall
	BillingDefaultOfferTemplate = 428,                  // Standard offertmall
	BillingDefaultContractTemplate = 429,               // Standard avtalsmall
	BillingDefaultHouseholdDeductionTemplate = 495,     // Standardmall husavdrag
	//BillingDefaultRUTDeductionTemplate = 496,           // Standardmall rut-avdrag
	BillingDefaultExpenseReportTemplate = 6008,           // Standardmall utlägg
	BillingDefaultInvoiceTemplateCashSales = 6009,      // Standard fakturamall - kontantförsäljning
	BillingDefaultEmailTemplateCashSales = 6010,        // Faktura mall för epost - kontantförsäljning
	BillingDefaultPurchaseOrderReportTemplate = 6015,    // Standardmall beställning
	BillingDefaultEmailTemplatePurchase = 6016,         // Standardmall email beställning
	
	
	//Offer and Order status
	BillingStatusTransferredOfferToOrder = 431,         // Status för överföring offert till order
	BillingStatusTransferredOfferToInvoice = 432,       // Status för överföring offert till faktura
	BillingStatusTransferredOrderToInvoice = 433,       // Status för överföring order till faktura
	BillingStatusOrderReadyMobile = 463,                // Status för klarmarkerad order (mobil-app)
	BillingStatusTransferredOrderToContract = 493,      // Status för överföring order till avtal
	BillingStatusTransferredOrderToInvoiceAndPrint = 414,// Överför order till faktura och skriv ut (via splitbutton)
	BillingStatusOrderDeliverFromStock = 6003,
	
	//EDI
	BillingEdiTransferToOrder = 441,                    // Överför autamatiskt när XML hämtas från FTP (GAMMAL: bool)
	BillingEdiTransferToOrderAdvanced = 460,            // Överför autamatiskt när XML hämtas från FTP (NY: string)
	BillingEdiTransferToSupplierInvoice = 442,          // Överför autamatiskt när XML hämtas från FTP
	BillingCloseEdiEntryCondition = 450,
	BillingEDIPriceSettingRule = 469,
	BillingEdiTransferCreditInvoiceToOrder = 6017,
	BillingUseEDIPriceForSalesPriceRecalculation = 6040,
	//BillingEDISupplementCharge = 470, not used anymore...
	
	//E-invoice
	BillingFinvoiceAddress = 464,                       // Companys finvoice address when sending FinvoiceMaterial
	BillingFinvoiceOperator = 465,                      // Companys finvoice operator when sending FinvoiceMaterial
	BillingFinvoiceSingleInvoicePerFile = 6020,         // Finvoice single file per invoice
	BillingFinvoiceInvoiceLabelToOrderIdentifier = 6033,
	BillingEInvoiceDistributor = 435,                   // Companys E-Invoice distributor (ReadSoft / InExchange)
	BillingEInvoiceFormat = 436,                        // E-Invoice format (finvoice / svefaktura)
	BillingSveFakturaToFile = 416,                      // svefaktura create file, bulk
	BillingSveFakturaToAPITestMode = 437,                       // sveFaktura to API
	BillingUseInvoiceDeliveryProvider = 6039,           //Adds the field for invoice delivery provider on customer and invoice
	BillingUseInExchangeDeliveryProvider = 6041,        //Use InExchange as invoice delivery provider for not only e-invoices
	
	//ReportDataHistory
	BillingUseInvoiceReportDataHistory = 472,           // ReportDataHistory for BillingInvoice
	
	//Checklist
	//BillingDefaultChecklistInOrder = 422,
	
	// Product
	BillingCopyProductPrices = 6021,
	BillingCopyProductAccounts = 6022,
	BillingCopyProductStock = 6023,
	
	BillingUseProductUnitConvert = 6004,
	BillingManuallyUpdatedAvgPrices = 6032,
	AllowInvoiceOfCreditOrders = 6005,
	BillingShowStartStopInTimeReport = 6006,
	BillingDefaultGrossMarginCalculationType = 6038,
	BillingProductRowTextUppercase = 6104,
	
	//Fortnox
	BillingFortnoxRefreshToken = 6024,
	BillingFortnoxLastSync = 6037,
	
	//Visma eAccounting
	BillingVismaEAccountingRefreshToken = 6035,
	BillingVismaEAccountingLastSync = 6036,
	
	//Not used (moved)
	BillingNotUsed8 = 417,
	
	
	
	// Time (501-509)
	TimeDefaultTimeCode = 501,
	TimeCodeBreakShowInvoiceProducts = 502,
	TimeCodeBreakShowPayrollProducts = 503,
	TimeDefaultTimePeriodHead = 504,
	TimeDefaultEmployeeGroup = 505,
	TimeDefaultMonthlyReport = 506,
	TimeDefaultPreviousTimePeriod = 507,
	TimeDefaultTimeSalarySpecificationReport = 508,
	TimeDefaultTimeSalaryControlInfoReport = 509,
	
	// Schedule (510-519)
	TimeMaxNoOfBrakes = 510,
	TimeDefaultStartOnFirstDayOfWeek = 511,
	TimeUseStopDateOnTemplate = 512,
	//TimeDontShowBreaksInShiftViewInMobile = 513,
	TimeCreateShiftsThatStartsAfterMidnigtInMobile = 514,
	
	// AccountingPrio (520-529)
	TimeCompanyPayrollProductAccountingPrio = 520,
	TimeCompanyInvoiceProductAccountingPrio = 521,
	UseTimeScheduleTypeFromTime = 522,
	
	// SalaryExport (530-539)
	SalaryExportPayrollMinimumAttestStatus = 530,
	SalaryExportPayrollResultingAttestStatus = 531,
	SalaryExportInvoiceMinimumAttestStatus = 532,
	SalaryExportInvoiceResultingAttestStatus = 533,
	SalaryExportTarget = 534,
	SalaryExportExternalExportID = 535,
	SalaryExportVatProductId = 536,
	SalaryExportEmail = 537,
	SalaryExportEmailCopy = 538,
	SalaryExportNoComments = 539,
	
	// Attest (540-550)
	TimeAutoAttestRunService = 540,                 // (Boolean) Mark if auto attest should be run by service
	TimeAutoAttestSourceAttestStateId = 541,        // (Integer) AttestStateId on TimePayrollTransactions to run auto attest on
	TimeAutoAttestSourceAttestStateId2 = 544,       // (Integer) Second Attestlevel
	TimeAutoAttestTargetAttestStateId = 542,        // (Integer) AttestStateId to set on successful auto attest TimePayrollTransactions
	MobileTimeAttestResultingAttestStatus = 543,    // Status after attest(set time as ready) in mobile
	TimeAutoAttestEmployeeManuallyAdjustedTimeStamps = 545,
	TimeAttestTreeIncludeAdditionalEmployees = 546,
	TimeSplitBreakOnAccount = 547,
	
	// Staffing (551-570)
	TimeUseStaffing = 551,
	TimeStaffingShiftAccountDimId = 552,
	//TimeSchedulePlanningShowTotalBreak = 553,
	TimeNbrOfSkillLevels = 554,
	TimeSkillLevelHalfPrecision = 555,
	TimeShiftTypeMandatory = 556,
	TimeSchedulePlanningDayViewMinorTickLength = 557,
	TimeSchedulePlanningSendXEMailOnChange = 558,
	//TimeSchedulePlanningShowBreaksInDayView = 559,
	TimeDefaultDoNotKeepShiftsTogether = 560,
	TimeSchedulePlanningDayViewStartTime = 561,
	TimeSchedulePlanningDayViewEndTime = 562,
	TimeUseVacant = 563,
	TimeSchedulePlanningClockRounding = 564,
	TimeStaffingNeedsAnalysisChartType = 565,
	TimeCalculatePlanningPeriodScheduledTime = 566,
	TimeSetApprovedYesAsDefault = 567,
	TimeOnlyNoReplacementIsSelectable = 568,
	TimeSchedulePlanningCalendarViewShowDaySummary = 569,
	TimeSchedulePlanningBreakVisibility = 570,
	
	// Placement (571-580)
	TimePlacementDefaultPreliminary = 571,
	TimeDefaultEmployeeScheduleDayReport = 572,
	TimeDefaultEmployeeTemplateScheduleDayReport = 573,
	TimeDefaultEmployeeScheduleWeekReport = 574,
	TimePlacementHideShiftTypes = 575,
	TimePlacementHideAccountDims = 576,
	TimePlacementHidePreliminary = 577,
	TimeDefaultScheduleTasksAndDeliverysDayReport = 578,
	TimeDefaultScheduleTasksAndDeliverysWeekReport = 579,
	TimeDefaultEmployeeTemplateScheduleWeekReport = 580,
	
	// Stamping (581-590)
	TimeIgnoreOfflineTerminals = 581,
	EmployeeSeqNbrStart = 582,
	
	//More Time (591-600)
	TimeSuggestEmployeeNrAsUsername = 591,
	TimeStaffingNeedsAnalysisRatioSalesPerScheduledHour = 592,
	TimeStaffingNeedsAnalysisRatioSalesPerWorkHour = 593,
	TimeStaffingNeedsAnalysisRatioFrequencyAverage = 594,
	TimeEditShiftShowEmployeeInGridView = 595,
	TimeEditShiftShowDateInGridView = 596,
	TimeForceSocialSecNbr = 597,
	TimeDefaultTimeEmploymentContract = 598,//obsolete since since sprint 46
	TimeDoNotModifyTimeStampEntryType = 599,
	TimeStaffingNeedsWorkingPeriodMaxLength = 600,
	
	// More Staffing (901-960)
	TimeSkillCantBeOverridden = 901,
	StaffingUseTemplateCost = 902,
	StaffingTemplateCost = 903,
	TimeSchedulePlanningSkipWorkRules = 904,
	TimeEditShiftAllowHoles = 905,
	CreateEmployeeRequestWhenDeniedWantedShift = 906,
	TimeAvailabilityLockDaysBefore = 907,
	HideRecipientsInShiftRequest = 908,
	TimeSchedulePlanningOverrideWorkRuleWarningsForMinors = 909,
	MinorsSchoolDayStartMinutes = 910,
	MinorsSchoolDayStopMinutes = 911,
	TimeSchedulePlanningUseWorkRulesForMinors = 912,
	StaffingNeedRoundUp = 913,
	TimeSchedulePlanningRuleRestTimeDayMandatory = 914,
	TimeSchedulePlanningRuleRestTimeWeekMandatory = 915,
	TimeSchedulePlanningRuleWorkTimeWeekDontEvaluateInSchedule = 916,
	TimeSchedulePlanningUseRuleWorkTimeWeekForParttimeWorkersInSchedule = 917,
	TimeDefaultEmployeePostTemplateScheduleDayReport = 918,
	TimeDefaultEmployeePostTemplateScheduleWeekReport = 919,
	TimeSchedulePlanningGaugeSalesThreshold1 = 920,
	TimeSchedulePlanningGaugeSalesThreshold2 = 921,
	TimeSchedulePlanningGaugeHoursThreshold1 = 922,
	TimeSchedulePlanningGaugeHoursThreshold2 = 923,
	TimeSchedulePlanningGaugeSalaryCostThreshold1 = 924,
	TimeSchedulePlanningGaugeSalaryCostThreshold2 = 925,
	TimeSchedulePlanningGaugeFPATThreshold1 = 926,
	TimeSchedulePlanningGaugeFPATThreshold2 = 927,
	TimeSchedulePlanningGaugeLPATThreshold1 = 928,
	TimeSchedulePlanningGaugeLPATThreshold2 = 929,
	TimeSchedulePlanningGaugeBPATThreshold1 = 930,
	TimeSchedulePlanningGaugeBPATThreshold2 = 931,
	ShowTemplateScheduleForEmployeesInApp = 932,
	TimeSchedulePlanningGaugeSalaryPercentThreshold1 = 933,
	TimeSchedulePlanningGaugeSalaryPercentThreshold2 = 934,
	TimeSchedulePlanningSortQueueByLas = 935,
	TimeSchedulePlanningSetShiftAsExtra = 936,
	TimeSchedulePlanningSetShiftAsSubstitute = 937,
	TimeCalculatePlanningPeriodScheduledTimeIncludeExtraShift = 938,
	TimeDefaultScenarioScheduleDayReport = 939,
	TimeDefaultScenarioScheduleWeekReport = 940,
	AbsenceRequestPlanningIncludeNoteInMessages = 941,
	UseMultipleScheduleTypes = 942,
	TimeSchedulePlanningContactAddressTypes = 943,
	TimeSchedulePlanningContactEComTypes = 944,
	SubstituteShiftIsAssignedDueToAbsenceOnlyIfSameBatch = 945,
	ValidateVacationWholeDayWhenSaving = 946,
	SubstituteShiftDontIncludeCopiedOrMovedShifts = 947,
	StaffingNeedsFrequencyAccountDim = 948,
	StaffingNeedsFrequencyParentAccountDim = 949,
	TimeSchedulePlanningRuleWorkTimeHoursBeforeAssignShift = 950,
	OrderPlanningIgnoreScheduledBreaksOnAssignment = 951,
	TimeSchedulePlanningShiftRequestPreventTooEarly = 952,
	TimeSchedulePlanningShiftRequestPreventTooEarlyWarnHoursBefore = 953,
	TimeSchedulePlanningShiftRequestPreventTooEarlyStopHoursBefore = 954,
	TimeSchedulePlanningInactivateLending = 955,
	ExtraShiftAsDefaultOnHidden = 956,
	TimeCalculatePlanningPeriodScheduledTimeUseAveragingPeriod = 957,
	IncludeSecondaryEmploymentInWorkTimeWeek = 958,
	UseLeisureCodes = 959,
	TimeCalculatePlanningPeriodScheduledTimeColors = 960,
	
	//Even more time (961-999)
	TimeSetEmploymentPercentManually = 961,
	TimeDontValidateSocialSecNbr = 962,
	TimeSetNextFreePersonNumberAutomatically = 963,
	TimeDefaultKU10Report = 964,
	TimeDefaultTimeCodeEarnedHoliday = 965,
	DefaultEmploymentContractShortSubstituteReport = 966,
	TimeEmployeePostPrefix = 967,
	//VacationFiveDaysPerWeek = 968, //Only set in code for beta test, now removed and available for all
	EmployeeKeepNbrOfYearsAfterEnd = 969,
	LimitAttendanceViewToStampedTerminal = 970,
	UseEmploymentExperienceAsStartValue = 971,
	TimeDefaultPlanningPeriod = 972,
	EmployeeIncludeNbrOfMonthsAfterEnded = 973,
	TimeDefaultTimeDeviationCause = 974,
	TimeDefaultPayrollGroup = 975,
	TimeDefaultVacationGroup = 976,
	SalaryExportLockPeriod = 977,
	UseSimplifiedEmployeeRegistration = 978,
	SalaryExportAllowPreliminary = 979,
	
	VacationValueDaysCreditAccountId = 980,
	VacationValueDaysDebitAccountId = 981,
	UseHibernatingEmployment = 986,
	SalaryPaymentExportUseExtendedCurrencyNOK = 987,
	SalaryPaymentExportExtendedAgreementNumber = 988,
	SalaryPaymentExportExtendedSenderIdentification = 989,
	SalaryPaymentExportExtendedPaymentAccount = 990,
	DontAllowIdenticalSSN = 991,
	PossibilityToRegisterAdditionsInTerminal = 992,
	SalaryExportExternalExportSubId = 993,
	DoNotUseMessageGroupInAttest = 994,
	CalculatePayrollFromChanges = 995,
	RecalculateFutureAccountingWhenChangingMainAllocation = 996,
	UseAnnualLeave = 997,
	RemoveScheduleTypeOnAbsence = 998,
	TimeSchedulePlanningDragDropMoveAsDefault = 999,
	UseIsNearestManagerOnAttestRoleUser = 8000,
	TimeSchedulePlanningSaveCopyOnPublish = 8001,
	PrintAgreementOnAssignFromFreeShift = 8002,
	
	
	
	ProjectAutoGenerateOnNewInvoice = 601,
	ProjectCreateInvoiceRowFromTransaction = 602,
	ProjectIncludeTimeProjectReport = 603,
	ProjectSuggestOrderNumberAsProjectNumber = 604,
	ProjectLimitOrderToProjectUsers = 605,
	//ProjectKeepEmployeesOnWeekChange = 606,
	ProjectChargeCostsToProject = 607,
	ProjectIncludeOnlyInvoicedTimeInTimeProjectReport = 608,
	ProjectUseCustomerNameAsProjectName = 609,
	ProjectOverheadCostAsFixedAmount = 610,
	ProjectOverheadCostAsAmountPerHour = 611,
	ProjectAutosaveOnWeekChangeInOrder = 612,
	//ProjectAllowLargerAmountsInTimeSheet = 613,
	ProjectInvoiceTimeAsWorkTime = 614,
	ProjectDefaultTimeCodeId = 615,
	ProjectUseExtendedTimeRegistration = 616,
	ProjectCreateTransactionsBaseOnTimeRules = 617,
	GetPurchasePriceFromInvoiceProduct = 619,
	UseDateIntervalInIncomeNotInvoiced = 621,
	
	ProjectAutoUpdateAccountSettings = 618,
	ProjectAutoUpdateInternalAccounts = 622,
	
	ProjectBlockTimeBlockWithZeroStartTime = 620,
	
	// CaseProject
	CaseProjectAttestStateReceived = 651,
	
	
	
	InventoryEditTriggerAccounts = 701,
	InventorySeparateVouchersInWriteOffs = 702,
	
	
	
	StockDefaultVoucherSeriesType = 751,
	
	
	
	DefaultRole = 801,
	CompanyAPIKey = 802,
	DefaultMobileUser = 803,
	DoNotAddToSoftOneIdDirectlyOnSave = 804,
	UseAccountHierarchy = 805,
	DefaultEmployeeAccountDimEmployee = 806,
	DefaultEmployeeAccountDimSelector = 807,
	CleanReportPrintoutAfterNrOfDays = 808,
	BlockFromDateOnUserAfterNrOfDays = 809,
	UseExtendedEmployeeAccountDimLevels = 810,
	AccountHierachySiblingsHaveSameParent = 811,
	UseLimitedEmployeeAccountDimLevels = 812,
	FallbackOnEmployeeAccountInPrio = 813,
	BaseSelectableAccountsOnEmployeeInsteadOfAttestRole = 814,
	SendReminderToExecutivesBasedOnEmployeeAccountOnly = 815,
	
	
	
	// When adding accounts here, the name of the enum must start with "Account".
	// See AccountManager.GetCompanyAccounts() for more information.
	
	
	AccountCommonVatPayable1 = 1001,              // Utgående moms 1
	AccountCommonVatPayable2 = 1002,              // Utgående moms 2
	AccountCommonVatPayable3 = 1003,              // Utgående moms 3
	AccountCommonVatReceivable = 1004,            // Ingående moms
	AccountCommonVatPayable1Reversed = 1005,      // Utgående moms 1 (omvänd skattskyldighet)
	AccountCommonVatPayable2Reversed = 1006,      // Utgående moms 2 (omvänd skattskyldighet)
	AccountCommonVatPayable3Reversed = 1007,      // Utgående moms 3 (omvänd skattskyldighet)
	AccountCommonVatReceivableReversed = 1008,    // Ingående moms   (omvänd skattskyldighet)
	AccountCommonCheck = 1009,                    // Kassa/checkkonto
	AccountCommonPG = 1010,                       // PlusGiro
	AccountCommonBG = 1011,                       // BankGiro
	AccountCommonAG = 1012,                       // Autogiro
	AccountCommonCentRounding = 1013,             // Öresutjämning
	AccountCommonCurrencyProfit = 1014,           // Valutavinst
	AccountCommonCurrencyLoss = 1015,             // Valutaförlust
	AccountCommonBankFee = 1016,                  // Bankavgift
	AccountCommonDiff = 1017,                     // Differens
	AccountCommonReverseVatSales = 1018,          // Försäljningskonto för omvänd moms
	AccountCommonVatAccountingKredit = 1019,       // Momsredovisning Kredit
	AccountCommonReverseVatPurchase = 1020,       // Inköpskonto för omvänd moms
	AccountCommonMixedVat = 1021,                 // Blandad moms, för ICA
	AccountCommonVatPayable1EUImport = 1022,        // Utgående moms 1 (eu import)
	AccountCommonVatPayable2EUImport = 1023,        // Utgående moms 2 (eu import)
	AccountCommonVatPayable3EUImport = 1024,        // Utgående moms 3 (eu import)
	AccountCommonVatReceivableEUImport = 1025,      // Ingående moms   (eu import)
	AccountCommonVatPurchaseEUImport = 1026,       // Inköpskonto för eu import
	
	AccountCommonVatPayable1NonEUImport = 1027,        // Utgående moms 1 (ej eu import)
	AccountCommonVatPayable2NonEUImport = 1028,        // Utgående moms 2 (ej eu import)
	AccountCommonVatPayable3NonEUImport = 1029,        // Utgående moms 3 (ej eu import)
	AccountCommonVatReceivableNonEUImport = 1030,      // Ingående moms   (ej eu import)
	AccountCommonVatPurchaseNonEUImport = 1031,       // Inköpskonto för ej eu import
	AccountCommonVatAccountingDebet = 1032,       // Momsredovisning Debet
	
	
	AccountSupplierPurchase = 1101,   // Inköp
	AccountSupplierDebt = 1102,       // Leverantörsskuld
	AccountSupplierInterim = 1103,    // Interim
	AccountSupplierUnderpay = 1104,   // Underbetalning
	AccountSupplierOverpay = 1105,    // Överbetalning
	
	
	
	AccountCustomerSalesVat = 1201,             // Försäljning momspliktig
	AccountCustomerSalesNoVat = 1202,           // Försäljning momsfri
	AccountCustomerFreight = 1203,              // Frakt
	AccountCustomerOrderFee = 1204,             // Expeditionsavgift
	AccountCustomerInsurance = 1205,            // Försäkring
	AccountCustomerClaim = 1206,                // Kundfordran
	AccountCustomerUnderpay = 1207,             // Underbetalning
	AccountCustomerOverpay = 1208,              // Överbetalning
	AccountCustomerPenaltyInterest = 1209,      // Dröjsmålsränta
	AccountCustomerClaimCharge = 1210,          // Kravavgift
	AccountCustomerPaymentFromTaxAgency = 1211, // Inbetalning från Skatteverket (ROT-avdrag)
	AccountUncertainCustomerClaim = 1212,       // Osäkra kundfordringar
	AccountCustomerDiscount = 1213,             // Kundrabatt
	AccountCustomerDiscountOffset = 1214,       // Kundrabatt - motkonto
	AccountCustomerSalesWithinEU = 1215,        // Försäljning inom EU - varor
	AccountCustomerSalesOutsideEU = 1216,       // Försäljning utanför EU - varor
	AccountCustomerSalesTripartiteTrade = 1217, // Kundrabatt - motkonto
	AccountCustomerSalesWithinEUService = 1218, // Försäljning inom EU - tjänster
	AccountCustomerSalesOutsideEUService = 1219,    // Försäljning utanför EU - tjänster
	
	
	
	AccountInvoiceProductSales = 1301,               // Standard kontosträng för försäljning av artiklar
	AccountInvoiceProductSalesInternal1 = 1302,
	AccountInvoiceProductSalesInternal2 = 1303,
	AccountInvoiceProductSalesInternal3 = 1304,
	AccountInvoiceProductSalesInternal4 = 1305,
	AccountInvoiceProductSalesInternal5 = 1306,
	AccountInvoiceProductPurchase = 1307,            // Standard kontosträng för inköp av artiklar
	AccountInvoiceProductPurchaseInternal1 = 1308,
	AccountInvoiceProductPurchaseInternal2 = 1309,
	AccountInvoiceProductPurchaseInternal3 = 1310,
	AccountInvoiceProductPurchaseInternal4 = 1311,
	AccountInvoiceProductPurchaseInternal5 = 1312,
	AccountInvoiceProductSalesVatFree = 1313,               // Standard kontosträng för momsfri försäljning av artiklar
	AccountInvoiceProductSalesVatFreeInternal1 = 1314,
	AccountInvoiceProductSalesVatFreeInternal2 = 1315,
	AccountInvoiceProductSalesVatFreeInternal3 = 1316,
	AccountInvoiceProductSalesVatFreeInternal4 = 1317,
	AccountInvoiceProductSalesVatFreeInternal5 = 1318,
	
	
	
	AccountEmployeeGroupIncome = 1401,        // Intäkt
	AccountEmployeeGroupCost = 1402,          // Kostnad
	
	
	
	AccountPayrollEmploymentTax = 1421,         // Arbetsgivaravgift
	AccountPayrollPayrollTax = 1422,            // Egenavgift
	AccountPayrollOwnSupplementCharge = 1423,   // Egna påslag
	
	AccountPayrollMaxRegionalSupportDim1 = 1431,
	AccountPayrollMaxRegionalSupportDim2 = 1432,
	AccountPayrollMaxRegionalSupportDim3 = 1433,
	AccountPayrollMaxRegionalSupportDim4 = 1434,
	AccountPayrollMaxRegionalSupportDim5 = 1435,
	AccountPayrollMaxRegionalSupportDim6 = 1436,
	
	// Payroll Voucher Exports
	PayrollAccountExportVoucherSeriesType = 1441,
	
	
	AccountEmployeeIncome = 1501,        // Intäkt
	AccountEmployeeCost = 1502,          // Kostnad
	
	
	
	
	
	AccountInventoryInventories = 1601,
	AccountInventoryAccWriteOff = 1602,
	AccountInventoryWriteOff = 1603,
	AccountInventoryAccOverWriteOff = 1604,
	AccountInventoryOverWriteOff = 1605,
	AccountInventoryAccWriteUp = 1606,
	AccountInventoryWriteUp = 1607,
	AccountInventoryAccWriteDown = 1608,
	AccountInventoryWriteDown = 1609,
	AccountInventorySalesProfit = 1610,
	AccountInventorySalesLoss = 1611,
	AccountInventorySales = 1612,
	
	
	
	AccountStockIn = 1701,
	AccountStockInChange = 1702,
	AccountStockOut = 1703,
	AccountStockOutChange = 1704,
	AccountStockInventory = 1705,
	AccountStockInventoryChange = 1706,
	AccountStockLoss = 1707,
	AccountStockLossChange = 1708,
	AccountStockTransferChange = 1709,
	
	
	
	
	ProductFreight = 2001,                      // Frakt
	ProductInvoiceFee = 2004,                   // Faktureringsavgift
	ProductCentRounding = 2007,                 // Öresutjämning
	ProductHouseholdTaxDeduction = 2010,        // ROT-avdrag
	ProductHouseholdTaxDeductionDenied = 2012,  // ROT-avdrag avslag
	ProductFlatPrice = 2011,                    // Fast pris
	ProductMisc = 2013,                         // Ströartikel
	ProductGuarantee = 2014,                    // Garantibelopp
	ProductFlatPriceKeepPrices = 2015,          // Fastprisartikel som inte tömmer övriga priser
	ProductRUTTaxDeduction = 2016,              // RUT-avdrag
	ProductRUTTaxDeductionDenied = 2017,        // RUT-avdrag avslag
	ProductGreen15TaxDeduction = 2018,          // Grön teknik-avdrag 15
	ProductGreen15TaxDeductionDenied = 2019,    // Grön teknik-avdrag 15 avslag
	ProductGreen50TaxDeduction = 2020,          // Grön teknik-avdrag 50
	ProductGreen50TaxDeductionDenied = 2021,    // Grön teknik-avdrag 50 avslag
	ProductGreen20TaxDeduction = 2022,          // Grön teknik-avdrag 20
	ProductGreen20TaxDeductionDenied = 2023,    // Grön teknik-avdrag 20 avslag
	ProductHousehold50TaxDeduction = 2024,          // Grön teknik-avdrag 50
	ProductHousehold50TaxDeductionDenied = 2025,    // Grön teknik-avdrag 50 avslag
	
	// Interest and Reminder
	ProductReminderFee = 2051,                  // Påminnelseavgift
	ProductInterestInvoicing = 2052,            // Räntefakturering
	
	
	
	// PayrollAgreement (3001-3100)
	PayrollGroupMandatory = 3009,
	
	PayrollAgreementUseException2to6InWorkingAgreement = 3001,
	PayrollAgreementUseOverTimeCompensation = 3002,
	PayrollAgreementUseTravelCompansation = 3003,
	PayrollAgreementUseVacationRights = 3004,
	PayrollAgreementUseWorkTimeShiftCompensation = 3005,
	PayrollAgreementUseGrossNetTimeInStaffing = 3006,
	PayrollAgreementUsePayrollTax = 3007,
	PayrollAgreementUseGrossNetTimeInStaffing_ByTransactions = 3008,
	
	// PayrollSettings
	PayrollSettingsDefaultReport = 3020,
	
	// EmploymentType Sweden 3101-3200 (see TermGroup_EmploymentType)
	PayrollEmploymentTypeUse_SE_Probationary = 3101,                // Provanställning
	PayrollEmploymentTypeUse_SE_Substitute = 3102,                  // Vikariat
	PayrollEmploymentTypeUse_SE_SubstituteVacation = 3103,          // Semestervikarie
	PayrollEmploymentTypeUse_SE_Permanent = 3104,                   // Tillsvidareanställning
	PayrollEmploymentTypeUse_SE_FixedTerm = 3105,                   // Allmän visstidsanställning
	PayrollEmploymentTypeUse_SE_Seasonal = 3106,                    // Säsongsarbete
	PayrollEmploymentTypeUse_SE_SpecificWork = 3107,                // Visst arbete
	PayrollEmploymentTypeUse_SE_Trainee = 3108,                     // Praktikantanställning
	PayrollEmploymentTypeUse_SE_NormalRetirementAge = 3109,         // Tjänsteman som uppnått den ordinarie pensionsåldern enligt ITP-planen
	PayrollEmploymentTypeUse_SE_CallContract = 3110,                // Behovsanställning
	PayrollEmploymentTypeUse_SE_LimitedAfterRetirementAge = 3111,   // Tidsbegränsad anställning för personer fyllda 67 år (enligt lag)
	PayrollEmploymentTypeUse_SE_FixedTerm14days = 3112,             // Allmän visstidsanställning 14 dagar
	PayrollEmploymentTypeUse_SE_Apprentice = 3113,                  // Lärling
	PayrollEmploymentTypeUse_SE_SpecialFixedTerm = 3114,            //Särskild Visstidsanställning
	
	// EmploymentType Finland 3201-3300 (see TermGroup_EmploymentType)
	
	// EmploymentType Norway 3301-3400 (see TermGroup_EmploymentType)
	
	UsePayroll = 3401,
	PayrollMaxRegionalSupportAmount = 3402,
	PayrollMaxRegionalSupportPercent = 3403,
	PayrollMaxResearchSupportAmount = 3404,
	DefaultPayrollSlipReport = 3405,                                //XE payroll
	DefaultEmployeeVacationDebtReport = 3406,
	UsedPayrollSince = 3407,
	CalculateExperienceFrom = 3408,
	
	// Payroll statistics export settings, Sweden (3500-3599)
	PayrollExportForaAgreementNumber = 3500,
	PayrollExportITP1Number = 3501,
	PayrollExportITP2Number = 3502,
	PayrollExportKPAAgreementNumber = 3503,
	PayrollExportSNKFOMemberNumber = 3504,
	PayrollExportSNKFOWorkPlaceNumber = 3505,
	PayrollExportSNKFOAffiliateNumber = 3506,
	PayrollExportSNKFOAgreementNumber = 3507,
	PayrollExportCommunityCode = 3508,
	PayrollExportSCBWorkSite = 3509,
	PayrollExportCFARNumber = 3510,
	PayrollArbetsgivarintygnuApiNyckel = 3511,
	PayrollArbetsgivarintygnuArbetsgivarId = 3512,
	PayrollExportKPAManagementNumber = 3513,
	PayrollExportFolksamCustomerNumber = 3514,
	PayrollExportPlaceOfEmploymentAddress = 3515,
	PayrollExportPlaceOfEmploymentCity = 3516,
	PayrollExportSkandiaSortingConcept = 3517,
	
	//SalaryPaymentExport (3600 - 3699)
	SalaryPaymentLockedAttestStateId = 3600,
	SalaryPaymentApproved1AttestStateId = 3601,
	SalaryPaymentApproved2AttestStateId = 3602,
	SalaryPaymentExportFileCreatedAttestStateId = 3603,
	SalaryPaymentExportType = 3604,
	SalaryPaymentExportSenderIdentification = 3605,
	SalaryPaymentExportSenderBankGiro = 3606,
	SalaryPaymentExportCompanyIsRegisterHolder = 3607,
	SalaryPaymentExportBank = 3608,
	SalaryPaymentExportUseAccountNrAsBBAN = 3609,
	SalaryPaymentExportUsePaymentDateAsExecutionDate = 3610,
	SalaryPaymentExportPaymentAccount = 3611,
	SalaryPaymentExportAgreementNumber = 3612,
	SalaryExportUseSocSecFormat = 3613,
	SalaryPaymentExportUseIBANOnEmployee = 3614,
	SalaryPaymentExportDivisionName = 3615,
	
	//Misc settings (3700 - 3799)
	PayrollAccountingDistributionPayrollProduct = 3700,
	PayrollAccountProvisionTimeCode = 3701,
	PayrollAccountProvisionAccountDim = 3702,
	PublishPayrollSlipWhenLockingPeriod = 3703,
	SendNoticeWhenPayrollSlipPublished = 3704,
	PayrollGrossalaryRoundingPayrollProduct = 3705,
	UsePayrollControlFunctions = 3706,
	
	
	
	UseBridge = 6500,
	
	
	
	
	StageSync = 7000,
	OrderSync = 7001,
	CustomerInvoiceSync = 7002,
	OfferSync = 7003,
	ContractSync = 7004,
	SupplierInvoiceSync = 7005,
	TimeCodeTransactionSync = 7006,
	TimePayrollTransactionSync = 7007,
	ScheduleSync = 7008,
	Vouchers = 7009,
	
}

export enum CompanySettingTypeGroup {
	Unknown = 0,
	
	//Modules and Areas
	Core = 1,
	Accounting = 2,
	Supplier = 3,
	Customer = 4,
	Billing = 5,
	Time = 6,
	Project = 7,
	Inventory = 8,
	Manage = 9,
	Payroll = 10,
	PayrollAgreement = 11,
	PayrollEmploymentTypes_SE = 12,
	PayrollEmploymentTypes_FI = 13,
	PayrollEmploymentTypes_NO = 14,
	SoftOneStage = 15,
	
	//BaseAccounts
	BaseAccounts = 100,
	BaseAccountsCommon = 101,
	BaseAccountsSupplier = 102,
	BaseAccountsCustomer = 103,
	BaseAccountsInvoiceProduct = 104,
	BaseAccountsEmployeeGroup = 105,
	BaseAccountsEmployee = 106,
	BaseAccountsInventory = 107,
	
	//BaseProducts
	BaseProducts = 108,
}

export enum LicenseSettingType {
	
	SSO_ForceLogin = 100,               // User must login with SSO
	SSO_Key = 101,                      // String (Guid)
	SSO_SoftForce = 102,                // If user has Externalid, force login
	SSO_SkipActivationEmailOnSSO = 103,
	
	
	LifetimeSecondsEnabledOnUser = 200,
	
	BrandingCompany = 300,
	
	AllLicensesOnServer = 999,
}

export enum ApplicationSettingType {
	
	AppDirectory = 1,
	XapVersion = 2,
	AttachmentFileSize = 3,
	MinimumTimeStampVersion = 4,
	CaseProjectSupportCompanyId = 5,
	ReleaseMode = 6,
	BingMapKey = 7,
	LiberAutoCopying = 8,
	UseWebService = 9,
	CrystalServiceUrl = 10,
	UseWebApiInternal = 12,
	WebApiInternalUrl = 13,
	UseCrGen = 14,
	PushNotificationUseGuid = 15,
	UseBulkInSalaryExport = 16,
	CommitCountInSalaryExport = 17,
	UseSoftOneId = 18,                       //Do not user this. Use setting in config instead
	WebApiInternalUrlSecondary = 19,
	SkipTaskWatchLog = 20,
	FontAwesomeSource = 21,
	UseAzureSearch = 22,
	SkipAttestTransition = 23,
	UseStoredProcedureInSalaryExport = 24,
	GtsHandshakeInterval = 25,  // Interval in milliseconds between handshakes
	UseZerosInTaxDeductionFile = 26,
	
	
	
	SyncToSysService = 50,
	sysCompDbId = 51,
	
	
	
	ShowVacationYearEndTestDate = 91,
	
	UseBootstrapMenu = 98,                  // Bootstrap (left) menu vs classic (top) menu
	UseAngular = 99,                        // Angular vs Silverligt
	
	
	
	WholesaleSaveFolder = 100,
	WholesaleRecreateFolder = 101,
	MsgTempFolder = 102,
	MsgErrorFolder = 103,
	MsgSaveFolder = 104,
	MsgRecreateFolder = 105,
	ImageTempFolder = 106,
	ImageErrorFolder = 107,
	ImageSaveFolder = 108,
	FtpMsgInputFolder = 109,
	FtpImageInputFolder = 110,
	FtpRootOutputFolder = 111,
	ReceivedMessagesFunction = 112,
	FtpReceivedMessagesFolder = 113,
	FileZillaFolder = 114,
	FtpUser = 115,
	FtpPassword = 116,
	EmailAddress = 117,
	IntervalSecond = 118,
	StandardTemplatesFolder = 119,
	SetupErrorEmailAddress = 120,
	WholesaleErrorFolder = 121,
	WholesaleTempFolder = 122,
	XECustomersFolder = 123,
	FtpCustomerNotFoundFolder = 124,
	OnlyMoveFilesMode = 125,
	
	
	
	SysServiceUri1 = 130,
	SysServiceUri2 = 131,
	SysServiceUri3 = 132,
	SysServiceUri4 = 133,
	
	
	
	UseL1AndL2Cache = 1027,
	CacheAcceptSeconds = 1028,
	CacheCheckIntervalSeconds = 1029,
	CacheDefaultLocalTtlSeconds = 1030,
	CacheLeaseSeconds = 1031,
}

export enum WildCard {
	LessThan = 0,
	LessThanOrEquals = 1,
	Equals = 2,
	GreaterThanOrEquals = 3,
	GreaterThan = 4,
	NotEquals = 5
}

export enum OrderBy {
	Ascending = 0,
	Descending = 1,
	None = 2,
}

export enum SearchLevel {
	None = 0,
	TitleWithLimit = 1,
	ContentWithLimit = 2,
	Title = 3,
	Content = 4,
	Html = 5,
}

export enum XEMailType {
	Incoming = 0,
	Outgoing = 1,
	Sent = 2,
	Deleted = 3,
}

export enum XEMailRecipientType {
	User = 0,
	Group = 1,
	Role = 2,
	Category = 3,
	MessageGroup = 4,
	Employee = 5,
	Account = 6
}

export enum XEMailAnswerType {
	None = 0,
	Yes = 1,
	No = 2,
}

export enum Browsers {
	Unknown = 0,
	InternetExplorer = 1,
	Chrome = 2,
	Safari = 3,
}

export enum MobileDeviceType {
	Unknown = 0,
	Android = 1,
	IOS = 2
};

export enum BatchUpdateFieldType {
	Unknown = 0,
	String = 1,
	Integer = 2,
	Boolean = 3,
	Date = 4,
	Decimal = 5,
	Time = 6,
	Id = 7,
	DecimalNull = 8
}

export enum BatchUpdateAccountStd {
	AccountType = 1,
	SysVatAccount = 2,
	IsAccrualAccount = 3,
	ExcludeVatVerification = 4,
	AmountStop = 5,
	AccountUnit = 6,
	AccountUnitStop = 7,
	AccountTextStop = 8,
	SruCode1 = 9,
	SruCode2 = 10,
	Active = 11,
	
	//Internal accounts navigation
	AccountDim1Default = 101,
	AccountDim1NavigationType = 102,
	AccountDim2Default = 103,
	AccountDim2NavigationType = 104,
	AccountDim3Default = 105,
	AccountDim3NavigationType = 106,
	AccountDim4Default = 107,
	AccountDim4NavigationType = 108,
	AccountDim5Default = 109,
	AccountDim5NavigationType = 110,
	AccountDim6Default = 111,
	AccountDim6NavigationType = 112,
}

export enum BatchUpdateCustomer {
	VatType = 1,
	DefaultPricelist = 2,
	DefaultWholeseller = 3,
	DiscountMerchandise = 4,
	DiscountService = 5,
	CreditLimit = 6,
	InvoiceReference = 7,
	InvoiceDeliveryType = 8,
	DisableInvoiceFee = 9,
	AddAttachmentsToEInvoice = 10,
	AddSupplierInvoicesToEInvoice = 11,
	InvoiceLabel = 12,
	PaymentCondition = 13,
	Active = 14,
	ImportInvoicesDetailed = 15,
	ShowNote = 16,
}

export enum BatchUpdateEmployee {
	Active = 1,
	ExternalCode = 7,
	HierarchicalAccount = 50,
	AccountNrSieDim = 70,
	ExcludeFromPayroll = 119,
	TimeWorkAccount = 120,
	WantsExtraShifts = 121,
	EmployeeGroup = 202,
	PayrollGroup = 203,
	VacationGroup = 204,
	WorkTimeWeekMinutes = 205,
	EmploymentPercent = 206,
	EmploymentExternalCode = 207,
	EmploymentType = 208,
	EmployeePositions = 209,
	WorkTasks = 223,
	WorkPlace = 225,
	DoNotValidateAccount = 261,
	EmploymentPriceType = 300,
	UserRole = 400,
	AttestRole = 401,
	BlockedFromDate = 402,
	PayrollStatisticsPersonalCategory = 601,
	PayrollStatisticsWorkTimeCategory = 602,
	PayrollStatisticsSalaryType = 603,
	PayrollStatisticsWorkPlaceNumber = 604,
	PayrollStatisticsCFARNumber = 605,
	ControlTaskWorkPlacSCB = 611,
	ControlTaskPartnerInCloseCompany = 612,
	ControlTaskBenefitAsPension = 613,
	AFACategory = 621,
	AFASpecialAgreement = 622,
	AFAWorkplaceNr = 623,
	AFAParttimePensionCode = 624,
	CollectumITPPlan = 631,
	CollectumAgreedOnProduct = 632,
	CollectumCostPlace = 633,
	CollectumCancellationDate = 634,
	CollectumCancellationDateIsLeaveOfAbsence = 635,
	KPARetirementAge = 641,
	KPABelonging = 642,
	KPAEndCode = 643,
	KPAAgreementType = 644,
	BygglosenAgreementArea = 651,
	BygglosenAllocationNumber = 652,
	BygglosenSalaryFormula = 653,
	BygglosenMunicipalCode = 654,
	BygglosenProfessionCategory = 655,
	BygglosenSalaryType = 656,
	BygglosenWorkPlaceNumber = 657,
	BygglosenLendedToOrgNr = 658,
	BygglosenAgreedHourlyPayLevel = 659,
	GTPAgreementNumber = 661,
	GTPExcluded = 662,
	AGIPlaceOfEmploymentAddress = 671,
	AGIPlaceOfEmploymentCity = 672,
	AGIPlaceOfEmploymentIgnore = 673,
	IFAssociationNumber = 681,
	IFPaymentCode = 682,
	IFWorkPlace = 683,
}

export enum BatchUpdateInvoiceProduct {
	MaterialCode = 1,
	ProductUnit = 2,
	Active = 3,
	IsStockProduct = 4,
	Description = 5,
	ProductGroup = 6,
	Type = 7,
	VatCode = 8,
	IntrastatCode = 9,
	CountryOfOrigin = 10,
}

export enum BatchUpdatePayrollProduct {
	Active = 1,
	Payed = 2,
	ExcludeInWorkTimeSummary = 3,
	AverageCalculated = 4,
	UseInPayroll = 5,
	DontUseFixedAccounting = 6,
	Export = 11,
	Export_IncludeAmountInExport = 12,
	
	PrintOnSalarySpecification = 101,
	DontPrintOnSalarySpecificationWhenZeroAmount = 102,
	PrintDate = 103,
	DontIncludeInRetroactivePayroll = 104,
	VacationSalaryPromoted = 105,
	UnionFeePromoted = 106,
	WorkingTimePromoted = 107,
	CalculateSupplementCharge = 108,
	CalculateSicknessSalary = 109,
	PensionCompany = 151,
	DontIncludeInAbsenceCost = 152,
	TimeUnit = 153,
	TaxCalculationType = 154,
	AccountInternal = 161,
	AccountingPrio = 162,
	PayrollProductPriceTypesAndFormulas = 163,
	
}

export enum BatchUpdateSupplier {
	Active = 1,
	VatType = 2,
	PaymentCondition = 3,
	AttestWorkFlowGroup = 4,
	DeliveryType = 5,
	DeliveryCondition = 6
}

