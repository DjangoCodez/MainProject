// System
export enum DayOfWeek {
  Sunday = 0,
  Monday = 1,
  Tuesday = 2,
  Wednesday = 3,
  Thursday = 4,
  Friday = 5,
  Saturday = 6,
}

// Soe.Sys.Common
export enum SoeSysEntityState {
  Active = 0,
  Inactive = 1,
  Deleted = 2,
  Temporary = 3,
}

export enum SysCompanySettingType {
  WholesellerCustomerNumber = 1,
  OrganisationNumber = 2,
  SysEdiMessageTypeAndNumber = 3,
  ExternalFtp = 4,
  UserName = 10,
  Password = 11,
}

export enum SysCompDBType {}

// Soe.Edi.Common
export enum SysEdiMessageHeadStatus {
  Unknown = 0,
  Unhandled = 1,
  Handled = 2,
  NoSysCompanyFound = 5,
  SentToComp = 10,
  Error = 99,
}

// SOEMessageBox buttons
export enum SOEMessageBoxButtons {
  None = 0,
  OK = 1,
  OKCancel = 2,
  YesNo = 3,
  YesNoCancel = 4,
}

export enum SOEMessageBoxButton {
  None = 0,
  First = -1,
  OK = 1,
  Cancel = 2,
  Yes = 3,
  No = 4,
  CancelAll = 5,
}

// SOEMessageBox title image
export enum SOEMessageBoxImage {
  None = 0,
  Information = 1,
  Warning = 2,
  Error = 3,
  Question = 4,
  Forbidden = 5,
  OK = 6,
  Custom = 7,
}

// SOEMessageBox modal size
export enum SOEMessageBoxSize {
  Small = 0,
  Medium = 1,
  Large = 2,
}

export enum IconLibrary {
  Glyphicon = 0,
  FontAwesome = 1,
}

// SoeGridOptions events
export enum SoeGridOptionsEvent {
  BeginCellEdit = 1,
  AfterCellEdit = 2,
  CancelCellEdit = 3,
  RowSelectionChanged = 4,
  RowSelectionChangedBatch = 5,
  RowExpanded = 6,
  RowCollapsed = 7,
  RowsVisibleChanged = 8,
  RenderingComplete = 9,
  RowsRendered = 10,
  Navigate = 11,
  Export = 12,
  FilterChanged = 13,
  RowDoubleClicked = 14, // AgGrid only
  IsRowSelectable = 15, // AgGrid only
  RowClicked = 16, // AgGrid only
  CellFocused = 17, // AgGrid only
  IsRowMaster = 18, // AgGrid only
  ColumnVisible = 19, // AgGrid only
  UserGridStateRestored = 20, // AgGrid only
  ColumnRowGroupChanged = 21, // AgGrid only
}

export enum SoeGridOptionsColumnType {
  Bool = 'bool',
  Icon = 'icon',
  Shape = 'shape',
  Modified = 'modified',
  Text = 'text',
  Number = 'number',
  Select = 'select',
  DateTime = 'datetime',
  Date = 'date',
  Time = 'time',
  TimeSpan = 'timespan',
  Autocomplete = 'autocomplete',
}

// ReportMenu
export enum ReportMenuPages {
  Favorites = 0,
  Reports = 1,
  Analysis = 2,
  Printed = 3,
}

// AmountHelper events
export enum AmountEvent {
  SetFixedPrice = 1,
  CalculateAmounts = 2,
}

// CurrencyHelper events
export enum CurrencyEvent {
  CurrencyChanged = 1,
  CurrencyIdChanged = 2,
}

export enum SortReports {
  Name = 1,
  Number = 2,
}

export enum AccountingRowsContainers {
  Voucher = 1,
  SupplierInvoice = 2,
  CustomerInvoice = 3,
  SupplierPayment = 4,
  CustomerPayment = 5,
  PaymentImport = 6,
  SupplierInvoiceAttest = 7,
}

export enum ProductRowsContainers {
  Offer = 1,
  Order = 2,
  Invoice = 3,
  Contract = 4,
  Purchase = 5,
}

export enum ProductRowsAmountField {
  Amount,
  DiscountAmount,
  SumAmount,
  VatAmount,
  PurchasePrice,
  MarginalIncome,
  HouseholdAmount,
}

export enum EditProductRowModes {
  EditProductRow = 1,
  EditHousehold = 2,
}

export enum MassRegistrationRowFunctions {
  AddRow = 1,
  AddRows = 2,
  ExportRows = 3,
  ImportRows = 4,
}

export enum MassRegistrationFunctions {
  CreateTransactions = 1,
  DeleteTemplate = 2,
  DeleteTemplateAndTransactions = 3,
}

export enum MessageGridFunctions {
  SetAsRead = 1,
  SetAsUnread = 2,
  Delete = 3,
}

export enum EditShiftFunctions {
  FillHolesWithBreaks = 1,
  CreateBreaksFromTemplates = 2,
  ReSort = 3,
  SplitShift = 4,
  ShiftRequest = 5,
  Absence = 6,
  AbsenceRequest = 7,
  RestoreToSchedule = 8,
  History = 9,
  Accounting = 10,
  RemoveAbsenceInScenario = 11,
}

export enum SaveShiftFunctions {
  Save = 1,
  SaveAndActivate = 2,
}

export enum EditAssignmentFunctions {
  EditOrder = 1,
  SplitShift = 2,
  ShiftRequest = 3,
  Absence = 4,
  AbsenceRequest = 5,
  RestoreToSchedule = 6,
  History = 7,
  Accounting = 8,
}

export enum SchedulePlanningFunctions {
  AddShift = 1,
  EditBreaks = 2,
  EditTemplateBreaks = 3,
  EditShifts = 4,
  ShowEmployeeList = 5,
  HideEmployeeList = 6,
  DefToPrel = 7,
  PrelToDef = 8,
  CopySchedule = 9,
  PrintSchedule = 10,
  PrintTemplateSchedule = 11,
  PrintEmploymentCertificate = 12,
  EvaluateAllWorkRules = 13,
  ShowUnscheduledTasks = 14,
  HideUnscheduledTasks = 15,
  ShowDashboard = 16,
  HideDashboard = 17,
  CreateEmptyScheduleForEmployeePosts = 18,
  RegenerateScheduleForEmployeePost = 19,
  DeleteScheduleForEmployeePost = 20,
  PrintEmployeePostTemplateSchedule = 21,
  AddOrder = 22,
  SendEmploymentCertificate = 23,
  OpenActivateSchedule = 24,
  NewTemplates = 25,
  RestoreToSchedule = 26,
  AddScenario = 27,
  DeleteScenario = 28,
  ActivateScenario = 29,
  PrintScenarioSchedule = 30,
  RemoveAbsenceInScenario = 31,
  ExportToExcel = 32,
  CreateTemplateFromScenario = 33,
}

export enum StaffingNeedsFunctions {
  AddNeed = 1,
  ReloadNeed = 2,
  PrintTasksAndDeliveries = 3,
  CreateShifts = 11,
}

export enum TaskAndDeliveryFunctions {
  Save = 1,
  SaveAndNew = 2,
  SaveAndClose = 3,
}

export enum RetroactiveFunctions {
  Save = 1,
  SaveAndCalculate = 2,
  Calculate = 3,
  CreateTransactions = 4,
  DeleteTransactions = 5,
}

export enum VoucherEditFunctions {
  Copy = 1,
  Invert = 2,
}

export enum VoucherEditSaveFunctions {
  Save = 1,
  SaveAndPrint = 2,
  SaveAsTemplate = 3,
}

export enum SupplierInvoiceEditSaveFunctions {
  Save = 1,
  SaveAndClose = 2,
  SaveAndOpenNext = 3,
}

export enum CustomerInvoiceEditSaveFunctions {
  Save = 1,
  SaveAndClose = 2,
}

export enum SupplierPaymentEditSaveFunctions {
  Save = 1,
  SaveAndClose = 2,
}

export enum CustomerPaymentEditSaveFunctions {
  Save = 1,
  SaveAndClose = 2,
}

export enum OrderEditProjectFunctions {
  Create = 1,
  Link = 2,
  Change = 3,
  Remove = 4,
  OpenProjectCentral = 5,
  Open = 6,
}

export enum OrderEditSaveFunctions {
  Save = 1,
  SaveAndClose = 2,
}

export enum OrderEditTransferFunctions {
  None = 0,
  TransferToPreliminaryInvoice = 1,
  TransferToDefinitiveInvoice = 2,
  TransferToOrder = 3,
}

export enum OrderInvoiceEditPrintFunctions {
  Print = 1,
  eMail = 2,
  ReportDialog = 3,
  EInvoice = 4,
  EInvoiceDownload = 5,
  PrintWithAttachments = 6,
}

export enum PurchaseDeliveryEditSaveFunctions {
  Save = 1,
  SaveAndClose = 2,
}

export enum PurchaseEditSaveFunctions {
  Save = 1,
  SaveAndClose = 2,
}

export enum PurchaseEditPrintFunctions {
  Print = 1,
  eMail = 2,
  ReportDialog = 3,
}

export enum PurchaseRowsRowFunctions {
  Add = 1,
  Delete = 2,
  AddText = 3,
}

export enum PurchaseRowsRowFeatureFunctions {
  SetAcknowledgedDeliveryDate = 1,
  Intrastat = 2,
}

export enum StandardRowFunctions {
  Add = 1,
  Delete = 2,
}

export enum ProductRowsRowFunctions {
  AddProduct,
  RefreshProducts,

  ChangeWholeseller,
  ChangeDiscount,
  RecalculatePrices,
  SetStock,
  SortRowsByProductNr,

  CopyRows,
  CopyRowsToContract,
  MergeRows,
  MoveRowsWithinOrder,
  MoveRows,
  DeleteRows,

  ShowAllSums,
  ShowProductPicture, // Finland EL customers

  CreatePurchase,

  // Superadmin
  ShowDeletedRows,
  UnlockRows,
  RenumberRows,
  MoveRowsToStock,

  ShowTimeRows,
  SplitAccounting,
  RecalculateTimeRow,
  ChangeDeductionType,
  ChangeIntrastatCode,
}

export enum ProductRowsAddRowFunctions {
  Product = 1,
  Text = 2,
  PageBreak = 3,
  SubTotal = 4,
}

export enum SupplierGridButtonFunctions {
  SaveAsDefinitiv = 1,
  TransferToVoucher = 2,
  CreateInvoiceOutOfEdi = 3,
  CloseEdi = 4,
  RemoveDraftOrEdi = 5,
  AddToAttestFlow = 6,
  SendAttestReminder = 7,
  StartAttestFlow = 8,
  Save = 9,
  CreatePDF = 10,
  HideUnhandled = 11,
  PrintInvoiceImages = 12,
  BatchOnwardInvoice = 13,
}

export enum SupplierPaymentGridButtonFunctions {
  CreateSuggestion = 1,
  Match = 2,
  CreatePaymentFile = 3,
  DeleteSuggestion = 4,
  ChangeDateVoucher = 5,
  TransferToVoucher = 6,
  ChangePayDate = 7,
  SaveChanges = 8,
  DeletePaymentFile = 9,
  SaveChangesTransferToVoucher = 10,
  SendPaymentFile = 11,
}

export enum CustomerInvoiceGridButtonFunctions {
  SaveAsDefinitiv = 1,
  TransferToVoucher = 2,
  ExportSOP = 3,
  ExportDI = 4,
  ExportUniMicro = 5,
  ExportDnB = 6,
  TransferToPreliminarInvoice = 7,
  TransferToInvoiceAndMergeOrders = 8,
  TransferToInvoiceAndPrint = 9,
  Match = 10,
  CreatePaymentFile = 11,
  TransferPaymentToVoucher = 12,
  PrintReminder = 13,
  CreateReminder = 14,
  CreateReminderAndMerge = 15,
  ChangeReminderLevel = 16,
  CreateInterestInvoice = 17,
  CreateInterestInvoiceMerge = 18,
  CloseInvoice = 19,
  SaveAsDefinitiveAndPrint = 20,
  PrintInvoices = 21,
  SaveAsDefinitiveAndCreateEInvoice = 22,
  SendasEInvoice = 23,
  TransferToOrder = 24,
  SendAsEmail = 25,
  PrintOrder = 26,
  SaveAsDefinitiveAndSendAsEmail = 27,
  SendReminderAsEmail = 28,
  UpdatePrices = 29,
  CloseContracts = 30,
  PrintInterestRateCalculation = 31,
  Delete = 32,
  SplitTimeRows = 33,
  DownloadEInvoice = 34,
  SaveAsDefinitiveAndSendEInvoice = 35,
  ExportFortnox = 36,
}

export enum PurchaseButtonFunctions {
  Print = 1,
  SendAsEmail = 2,
}

export enum CustomerInvoiceTemplateGridFunctions {
  CreateOffer = 1,
  CreateOrder = 2,
  CreateInvoice = 3,
}

export enum FinvoiceGridFunctions {
  Save = 1,
  CreatePdf = 2,
  CreateSupplierInvoice = 3,
  TransferToOrder = 4,
  Delete = 5,
}

export enum InsecureDebtsButtonFunctions {
  InsecureDebts = 1,
  NotInsecureDebts = 2,
}

export enum InventoryAdjustFunctions {
  OverWriteOff = 1,
  UnderWriteOff = 2,
  WriteUp = 3,
  WriteDown = 4,
  Sold = 5,
  Discarded = 6,
  WrittenOff = 7,
}

export enum SupplierInvoiceAttestFlowButtonFunctions {
  Accept = 1,
  TransferToOther = 2,
  TransferToOtherWithReturn = 3,
  Reject = 4,
  BlockPayment = 5,
  UnBlockPayment = 6,
}

export enum ReportTransferButtonFunctions {
  Export = 1,
  Import = 2,
}

export enum TimeTreeViewMode {
  None = 0,
  Group = 1,
  Employee = 2,
}

export enum TimePeriodSelectorType {
  None = 0,
  Day = 1,
  Week = 2,
  Month = 3,
  Period = 4,
}

export enum PayrollCalculationContentViewMode {
  None = 0,
  Calculation = 1,
  Fixed = 2,
  Retroactive = 3,
  Control = 4,
  Calendar = 5,
  AdditionAndDeduction = 6,
  AbsenceDetails = 7,
}

export enum TimeAttestContentViewMode {
  None = 0,
  AttestEmployee = 1,
  AdditionAndDeduction = 2,
  TimeCalendar = 3,
  AbsenceDetails = 4,
}

export enum PayrollCalculationFunctions {
  LockPeriod = 1,
  UnLockPeriod = 2,
  CreateFinalSalary = 3,
  DeleteFinalSalary = 4,
  GetUnhandledTransactionsBackwards = 5,
  GetUnhandledTransactionsForward = 6,
  AttestReminder = 7,
}

export enum PayrollCalculationReloadFunctions {
  Reload = 1,
  ReloadDetailed = 2,
}

export enum PayrollCalculationRecalculateFunctions {
  Recalculate = 1,
  RecalculateIncPrelTransaction = 2,
  RecalculateAccounting = 3,
}

export enum PlanningTabs {
  StaffingNeeds = 0,
  SchedulePlanning = 1,
}

export enum PlanningGroupBy {
  Employees = 0,
  Categories = 1,
  ShiftTypes = 2,
}

export enum PlanningSortBy {
  Firstname = 0,
  Lastname = 1,
  EmployeeNr = 2,
}

export enum PlanningEditModes {
  ReadOnly = 0,
  Shifts = 1,
  Breaks = 2,
  TemplateBreaks = 3,
}

export enum PlanningStatusFilterItems {
  Open = 0,
  Assigned = 1,
  Accepted = 2,
  Wanted = 3,
  Unwanted = 4,
  AbsenceRequested = 5,
  AbsenceApproved = 6,
  Preliminary = 7,
  HideAbsenceRequested = 8,
  HideAbsenceApproved = 9,
  HidePreliminary = 10,
}

export enum PlanningEmployeeListSortBy {
  Firstname = 0,
  Lastname = 1,
  EmployeeNr = 2,
  Availability = 3,
}

export enum PlanningOrderListSortBy {
  Priority = 0,
  PlannedStartDate = 1,
  PlannedStopDate = 2,
  RemainingTime = 3,
  OrderNr = 4,
}

export enum EmployeeAvailabilitySortOrder {
  FullyAvailable = 0,
  PartlyAvailable = 1,
  MixedAvailable = 2,
  NotSpecified = 3,
  PartlyUnavailable = 4,
  FullyUnavailable = 5,
}

export enum StaffingNeedsViewDefinitions {
  StaffingNeeds_TasksAndDeliveries_Day = 0,
  StaffingNeeds_TasksAndDeliveries_Schedule = 1,
  StaffingNeeds_Planning_Day = 2,
  StaffingNeeds_Planning_Schedule = 3,
  StaffingNeeds_Shifts_Day = 4,
  StaffingNeeds_Shifts_Schedule = 5,
  StaffingNeeds_EmployeePosts_Day = 6,
  StaffingNeeds_EmployeePosts_Schedule = 7,
}

export enum TemplateScheduleModes {
  New = 1,
  Edit = 2,
  Activate = 3,
}

export enum MomentUnitOfTime {
  year,
  month,
  week,
  day,
  hour,
  minute,
  second,
}

export enum MomentUnitsOfTime {
  years,
  quarters,
  months,
  weeks,
  days,
  hours,
  minutes,
  seconds,
  milliseconds,
}

export enum HouseholdDeductionGridButtonFunctions {
  SaveApplied = 1,
  SaveReceived = 2,
  SaveDenied = 3,
  DeleteRow = 4,
  SaveXML = 5,
  Print = 6,
  PrintAndSave = 7,
  SavePartiallyApproved = 8,
    WithdrawApplied = 9,
    EditAndSaveXML = 10,
}

export enum ProjectListGridButtonFunctions {
  Planned = 1,
  Active = 2,
  Locked = 3,
  Ended = 4,
  Hidden = 5,
}

export enum TimeProjectContainer {
  Order = 1,
  TimeSheet = 2,
  OrderRows = 3,
  ProjectCentral = 4,
}

export enum AbsenceRequestViewMode {
  Employee = 1,
  Attest = 2,
}

export enum AbsenceRequestGuiMode {
  EmployeeRequest = 1,
  AbsenceDialog = 2,
}

export enum AbsenceRequestParentMode {
  SchedulePlanning = 1,
  TimeAttest = 2,
}

export enum ProjectTimeRegistrationType {
  Order = 1,
  TimeSheet = 2,
  Attest = 3,
}

export enum EditUserFunctions {
  NewUser = 0,
  ConnectUser = 1,
  ModifyUser = 2,
  DisconnectUser = 3,
  DisconnectEmployee = 4,
}

export enum EditUserEmploymentFunctions {
  ChangeEmployment = 0,
  ChangeEmploymentDates = 1,
  ChangeToNotTemporary = 2,
}

export enum CreateFromEmployeeTemplateMode {
  NewEmployee = 0,
  NewEmployment = 1,
}

export enum TimeProjectSearchFunctions {
  SearchIntervall = 0,
  GetAll = 1,
  SearchWithGroupOnDate = 2,
  SearchIncPlannedAbsence = 3,
}

export enum TimeProjectButtonFunctions {
  AddRow = 0,
  DeleteRow = 1,
  MoveRow = 2,
  MoveRowToNewInvoiceRow = 3,
  MoveRowToExistingInvoiceRow = 4,
  Save = 5,
  CopyLastWeek = 6,
  ChangeDate = 7,
}

export enum SupplierAgreementButtonFunctions {
  AddRow = 1,
  AddAgreement = 2,
  RemoveAgreement = 3,
}
