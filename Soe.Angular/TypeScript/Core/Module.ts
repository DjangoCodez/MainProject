import '../Libs/Bundle';

import { HttpServiceProvider, IHttpServiceProvider } from "./Services/httpservice";
import { LazyLoadService } from "./Services/LazyLoadService";
import { IUrlHelperService, IUrlHelperServiceProvider, UrlHelperServiceProvider } from "./Services/UrlHelperService";
import { TermPartsLoaderProvider } from "./Services/termpartsloader";
import { FileUploaderFactoryProvider, IFileUploaderFactoryProvider } from "./Services/fileuploaderfactory";
import { TranslationService, ITranslationService } from "./Services/TranslationService";
import { MessagingService } from "./Services/MessagingService";
import { NotificationService } from "./Services/NotificationService";
import { ReportService } from "./Services/reportservice";
import { CoreService } from "./Services/CoreService";
import { StorageService } from "./Services/storageservice";
import { FocusService } from "./Services/focusservice";
import { DateHelperService } from "./Services/datehelperservice";
import { AngularFeatureCheckService } from "./Services/AngularFeatureCheckService";
import { ProgressHandlerFactory } from "./Handlers/progresshandlerfactory";
import { ControllerFlowHandlerFactory } from "./Handlers/controllerflowhandlerfactory";
import { ToolbarFactory } from "./Handlers/ToolbarFactory";
import { ValidationSummaryHandlerFactory } from "./Handlers/validationsummaryhandlerfactory";
import { MessagingHandlerFactory } from "./Handlers/messaginghandlerfactory";
import { TabHandlerFactory } from "./Handlers/tabhandlerfactory";
import { DirtyHandlerFactory } from "./Handlers/DirtyHandlerFactory";
import { GridHandlerFactory } from "./Handlers/gridhandlerfactory";
import { AuthenticationServiceProvider, IAuthenticationServiceProvider } from "./Services/authenticationservice";
import { DatespickerDirectiveFactory } from "./Directives/datespicker/datespickerdirective";
import { ParseDateDirectiveFactory } from "./Directives/parsedatedirective";
import { ParseTimeDirectiveFactory } from "./Directives/parsetimedirective";
import { DebugDirectiveFactory } from "./Directives/debugdirective";
import { DecimalPercentageDirectiveFactory } from "./Directives/decimalpercentagedirective";
import { DecimalDirectiveFactory } from "./Directives/decimaldirective";
import { ConfirmOnExitDirectiveFactory } from "./Directives/confirmonexitdirective";
import { CreatedModifiedDirectiveFactory } from "./Directives/createdmodifieddirective";
import { FormStateDirectiveFactory } from "./Directives/formstatedirective";
import { L10NBindDirectiveFactory } from "./Directives/l10nbinddirective";
import { EnterDirectiveFactory } from "./Directives/enterdirective";
import { PreventEnterDirectiveFactory } from "./Directives/prevententerdirective";
import { EnterFocusDirectiveFactory } from "./Directives/enterfocusdirective";
import { EventFocusDirectiveFactory } from "./Directives/eventfocusdirective";
import { FocusOnShowDirectiveFactory } from './Directives/FocusOnShowDirective';
import { SetFocusDirectiveFactory } from "./Directives/setfocusdirective";
import { TabWithEnterDirectiveFactory } from "./Directives/tabwithenterdirective";
import { HeightCheckerFactory } from "./Directives/heightcheckerEx";
import { SoePanelDirectiveFactory } from "./Directives/soepaneldirective";
import { SoeToolbarDirectiveFactory } from "./Directives/soetoolbardirective";
import { SoeLabelDirectiveFactory } from "./Directives/soelabeldirective";
import { SoeTextDirectiveFactory } from "./Directives/soetextdirective";
import { SoeInstructionDirectiveFactory } from "./Directives/soeinstructiondirective";
import { SoeTextboxDirectiveFactory } from "./Directives/soetextboxdirective";
import { SoeTextareaDirectiveFactory } from "./Directives/soetextareadirective";
import { TextareaAutoSizeDirectiveFactory } from "./Directives/textareaautosizedirective";
import { SoeCheckboxDirectiveFactory } from "./Directives/soecheckboxdirective";
import { SoeRadiobuttonDirectiveFactory } from "./Directives/soeradiobuttondirective";
import { SoeColorpickerDirectiveFactory } from "./Directives/soecolorpickerdirective";
import { SoeDatepickerDirectiveFactory } from "./Directives/soedatepickerdirective";
import { SoeTimepickerDirectiveFactory } from "./Directives/soetimepickerdirective";
import { SoeSelectDirectiveFactory } from "./Directives/soeselectdirective";
import { SoeTypeaheadDirectiveFactory } from "./Directives/SoeTypeahead/soetypeaheaddirective";
import { SoeMultiselectDirectiveFactory } from "./Directives/soemultiselectdirective";
import { SoeButtonDirectiveFactory } from "./Directives/soebuttondirective";
import { SoeProgressbarDirectiveFactory } from "./Directives/soeprogressbardirective";
import { SoeCategoryAccountsDirectiveFactory } from "./Directives/soecategoryaccountsdirective";
import { SoeMenubuttonDirectiveFactory } from "./Directives/soemenubuttondirective";
import { SoeNavigationMenuDirectiveFactory } from "./Directives/soenavigationmenudirective";
import { SoeSplitbuttonDirectiveFactory } from "./Directives/soesplitbuttondirective";
import { DateFilterFactory } from "./Directives/datefilter";
import { ShapeFactory } from "./Directives/shapefilter";
import { ValidateAlphaNumericFactory } from "./Validators/ValidateAlphaNumeric";
import { ValidateEmailFactory } from "./Validators/ValidateEmail";
import { ValidateNumeric } from "./Validators/ValidateNumeric";
import { ValidateNumericNotZero } from "./Validators/ValidateNumericNotZero";
import { ValidateSocialSecurityNumberFactory } from "./Validators/ValidateSocialSecurityNumber";
import { GridKeypressDirectiveFactory } from "./Directives/GridKeypressDirective";
import { TabControllerDirectiveFactory } from "./Directives/TabControllerDirective";
import { HelpMenuDirectiveFactory, HelpMenuContentFactory } from "./RightMenu/HelpMenu/HelpMenuDirective";
import { ConditionalFocusDirective } from "./Directives/ConditionalFocusDirective";
import { IgnoreDirtyDirectiveFactory } from "./Directives/IgnoreDirtyDirective";
import { AbsFilter } from "./Filters/AbsFilter";
import { AmountFilter } from "./Filters/AmountFilter";
import { FormatHtmlFilter } from "./Filters/FormatHtmlFilter";
import { GetDayNameFilter } from "./Filters/GetDayNameFilter";
import { IsSameDateFilter } from "./Filters/IsSameDateFilter";
import { IsSameMonthFilter } from "./Filters/IsSameMonthFilter";
import { MinutesToTimeSpanFilter } from "./Filters/MinutesToTimeSpanFilter";
import { OrderObjectByFilter } from "./Filters/OrderObjectByFilter";
import { PercentFilter } from "./Filters/PercentFilter";
import { SubstringFilter } from './Filters/SubstringFilter';
import { ToUpperCaseFirstLetterFilter } from "./Filters/ToUpperCaseFirstLetterFilter";
import { uiGridEditDropdownWithFocusDelay } from "./UiGridPatches/uiGridEditDropdownWithFocusDelay";
import { uiGridTypeaheadEditor } from "./UiGridPatches/uiGridTypeaheadEditor";
import { uiGridUiSelect } from "./UiGridPatches/uiGridUiSelect";
import { UIGridEditDatepickerFactory } from "./Directives/UIGridEditDatepicker";
import { SoeAccordionDirectiveFactory } from "./Directives/SoeAccordionDirective";
import { ShortCutService } from "./Services/ShortCutService";
import { ContextMenuHandlerFactory } from "./Handlers/ContextMenuHandlerFactory";
import { CoreUtility } from "../Util/CoreUtility";
import { DraggableDialogDirectiveFactory } from '../Common/Directives/DraggableDialog/DraggableDialogDirective';
import { SoeTimeboxDirectiveFactory } from './Directives/SoeTimeboxDirective';
import { ParseTimeboxDirectiveFactory } from './Directives/ParseTimeboxDirective';
import { SelectedItemsService } from './Services/SelectedItemsService';
import { ScopeTreeTraverserService } from "./Services/ScopeTreeTraverserService";
import { ScopeWatcherService } from "./Services/ScopeWatcherService";
import { ReportMenuDirectiveFactory } from './RightMenu/ReportMenu/ReportMenuDirective';
import { EmployeeListReport } from './RightMenu/ReportMenu/ReportTypes/EmployeeListReport/EmployeeListReportComponent';
import { BalanceReport } from './RightMenu/ReportMenu/ReportTypes/Economy/BalanceReport/BalanceReportComponent';
import { StockListReport } from './RightMenu/ReportMenu/ReportTypes/Billing/StockListReport/StockListReportComponent';
import { ProductListReport } from './RightMenu/ReportMenu/ReportTypes/Billing/ProductListReport/ProductListReportComponent';
import { OfferReport } from './RightMenu/ReportMenu/ReportTypes/Billing/OfferReport/OfferReportComponent';
import { InvoiceReport } from './RightMenu/ReportMenu/ReportTypes/Billing/InvoiceReport/InvoiceReportComponent';
import { ProjektstatistikReport } from './RightMenu/ReportMenu/ReportTypes/Billing/ProjektstatistikReport/ProjektstatistikReportComponent';
import { ProjekttidReport } from './RightMenu/ReportMenu/ReportTypes/Billing/ProjekttidReport/ProjekttidReportComponent';
import { StockInventoryReport } from './RightMenu/ReportMenu/ReportTypes/Billing/StockInventoryReport/StockInventoryReportComponent';
import { InvoiceJournalCustomerReport } from './RightMenu/ReportMenu/ReportTypes/Economy/InvoiceJournalCustomerReport/InvoiceJournalCustomerReportComponent';
import { CustomerPaymentJournalReport } from './RightMenu/ReportMenu/ReportTypes/Economy/CustomerPaymentJournalReport/CustomerPaymentJournalReportComponent';
import { GeneralLedgerReport } from './RightMenu/ReportMenu/ReportTypes/Economy/GeneralLedgerReport/GeneralLedgerReportComponent';
import { VoucherListReport } from './RightMenu/ReportMenu/ReportTypes/Economy/VoucherListReport/VoucherListReportComponent';
import { PeriodAccountingForecastReport } from './RightMenu/ReportMenu/ReportTypes/Economy/PeriodAccountingForecastReport/PeriodAccountingForecastReportComponent';
import { StockHistoryReport } from './RightMenu/ReportMenu/ReportTypes/Billing/StockHistoryReport/StockHistoryReportComponent';
import { PeriodAccountingRegulationsReport } from './RightMenu/ReportMenu/ReportTypes/Economy/PeriodAccountingRegulationsReport/PeriodAccountingRegulationsReportComponent';
import { TimeMonthlyReport } from './RightMenu/ReportMenu/ReportTypes/TimeMonthlyReport/TimeMonthlyReportComponent';
import { TimeStampEntryReport } from './RightMenu/ReportMenu/ReportTypes/TimeStampEntryReport/TimeStampEntryReportComponent';
import { ReportDataService } from './RightMenu/ReportMenu/ReportDataService';
import { ResultReport } from './RightMenu/ReportMenu/ReportTypes/Economy/ResultReport/ResultReportComponent';
import { SruReport } from './RightMenu/ReportMenu/ReportTypes/Economy/SruReport/SruReportComponent';
import { CustomerBalanceListReport } from './RightMenu/ReportMenu/ReportTypes/Economy/CustomerBalanceListReport/CustomerBalanceListReportComponent';
import { SupplierBalanceListReport } from './RightMenu/ReportMenu/ReportTypes/Economy/SupplierBalanceListReport/SupplierBalanceListReportComponent';
import { InvoiceJournalSupplierReport } from './RightMenu/ReportMenu/ReportTypes/Economy/InvoiceJournalSupplierReport/InvoiceJournalSupplierReportComponent';
import { EmployeeEndReasonsReport } from './RightMenu/ReportMenu/ReportTypes/EmployeeEndReasonsReport/EmployeeEndReasonsReportComponent';
import { EmployeeFixedPayLinesReport } from './RightMenu/ReportMenu/ReportTypes/EmployeeFixedPayLinesReport/EmployeeFixedPayLinesReportComponent';
import { PayrollProductsReport } from './RightMenu/ReportMenu/ReportTypes/PayrollProductsReport/PayrollProductsReportComponent';
import { EmployeeSalaryDistressReport } from './RightMenu/ReportMenu/ReportTypes/EmployeeSalaryDistressReport/EmployeeSalaryDistressReportComponent';
import { EmployeeSalaryUnionFeesReport } from './RightMenu/ReportMenu/ReportTypes/EmployeeSalaryUnionFeesReport/EmployeeSalaryUnionFeesReportComponent';
import { TaxReturnReport } from './RightMenu/ReportMenu/ReportTypes/Economy/TaxReturnReport/TaxReturnReportComponent';
import { TaxReturnFnReport } from './RightMenu/ReportMenu/ReportTypes/Economy/TaxReturnFnReport/TaxReturnFnReportComponent';
import { FixedAssetReport } from './RightMenu/ReportMenu/ReportTypes/Economy/FixedAssetReport/FixedAssetReportComponent';
import { TimeReport } from './RightMenu/ReportMenu/ReportTypes/Billing/TimeReport/TimeReportComponent';
import { TimeEmployeeScheduleReport } from './RightMenu/ReportMenu/ReportTypes/TimeEmployeeScheduleReport/TimeEmployeeScheduleReportComponent';
import { TimeEmployeeLineScheduleReport } from './RightMenu/ReportMenu/ReportTypes/TimeEmployeeLineScheduleReport/TimeEmployeeLineScheduleReportComponent';
import { TimePayrollTransactionReport } from './RightMenu/ReportMenu/ReportTypes/TimePayrollTransactionReport/TimePayrollTransactionReportComponent';
import { TimeAccumulatorReport } from './RightMenu/ReportMenu/ReportTypes/TimeAccumulatorReport/TimeAccumulatorReportComponent';
import { TimeAccumulatorDetailedReport } from './RightMenu/ReportMenu/ReportTypes/TimeAccumulatorDetailedReport/TimeAccumulatorDetailedReportComponent';
import { PayrollTransactionStatisticsReport } from './RightMenu/ReportMenu/ReportTypes/PayrollTransactionStatisticsReport/PayrollTransactionStatisticsReportComponent';
import { TimeCategoryScheduleReport } from './RightMenu/ReportMenu/ReportTypes/TimeCategoryScheduleReport/TimeCategoryScheduleReportComponent';
import { TimeScheduleBlockHistoryReport } from './RightMenu/ReportMenu/ReportTypes/TimeScheduleBlockHistoryReport/TimeScheduleBlockHistoryReportComponent';
import { TimeCategoryStatisticsReport } from './RightMenu/ReportMenu/ReportTypes/TimeCategoryStatisticsReport/TimeCategoryStatisticsReportComponent';
import { WhenRenderedDirectiveFactory } from '../Common/Directives/WhenRendered/WhenRenderedDirective';
import { WindowBlurDirectiveFactory } from './Directives/WindowBlurDirective';
import { WindowFocusDirectiveFactory } from './Directives/WindowFocusDirective';
import { TimeEmploymentContractReport } from './RightMenu/ReportMenu/ReportTypes/TimeEmploymentContractReport/TimeEmploymentContractReportComponent';
import { TimeSalaryControlInfoReport } from './RightMenu/ReportMenu/ReportTypes/TimeSalaryControlInfoReport/TimeSalaryControlInfoReportComponent';
import { TimeSalarySpecificationReport } from './RightMenu/ReportMenu/ReportTypes/TimeSalarySpecificationReport/TimeSalarySpecificationReportComponent';
import { TimeScheduleTasksAndDeliverysReport } from './RightMenu/ReportMenu/ReportTypes/TimeScheduleTasksAndDeliverysReport/TimeScheduleTasksAndDeliverysReportComponent';
import { EmployeeVacationDebtReport } from './RightMenu/ReportMenu/ReportTypes/EmployeeVacationDebtReport/EmployeeVacationDebtReportComponent';
import { PayrollProductReport } from './RightMenu/ReportMenu/ReportTypes/PayrollProductReport/PayrollProductReportComponent';
import { CertificateOfEmploymentReport } from './RightMenu/ReportMenu/ReportTypes/CertificateOfEmploymentReport/CertificateOfEmploymentReportComponent';
import { EmployeeVacationInformationReport } from './RightMenu/ReportMenu/ReportTypes/EmployeeVacationInformationReport/EmployeeVacationInformationReportComponent';
import { PayrollAccountingReport } from './RightMenu/ReportMenu/ReportTypes/PayrollAccountingReport/PayrollAccountingReportComponent';
import { EmployeeTimePeriodReport } from './RightMenu/ReportMenu/ReportTypes/EmployeeTimePeriodReport/EmployeeTimePeriodReportComponent';
import { PayrollPeriodWarningCheckReport } from './RightMenu/ReportMenu/ReportTypes/PayrollPeriodWarningCheckReport/PayrollPeriodWarningCheckReportComponent';
import { SoeInfobarDirectiveFactory } from "./Directives/SoeInfoBarDirective";
import { SKDReport } from './RightMenu/ReportMenu/ReportTypes/SKDReport/SKDReportComponent';
import { ReportJobDefinitionFactory } from './Handlers/ReportJobDefinitionFactory';
import { GenericReport } from './RightMenu/ReportMenu/ReportTypes/Generic/GenericReportComponent';
import { AgdEmployeeReport } from './RightMenu/ReportMenu/ReportTypes/AgdEmployeeReport/AgdEmployeeReportComponent';
import { AgdEmployeeAbsenceReport } from './RightMenu/ReportMenu/ReportTypes/AgdEmployeeAbsenceReport/AgdEmployeeAbsenceReportComponent';
import { CollectumReport } from './RightMenu/ReportMenu/ReportTypes/CollectumReport/CollectumReportComponent';
import { ForaReport } from './RightMenu/ReportMenu/ReportTypes/ForaReport/ForaReportComponent';
import { KPADirektReport } from './RightMenu/ReportMenu/ReportTypes/KPADirektReport/KPADirektReportComponent';
import { KPAReport } from './RightMenu/ReportMenu/ReportTypes/KPAReport/KPAReportComponent';
import { SCBKLPReport } from './RightMenu/ReportMenu/ReportTypes/SCBKLPReport/SCBKLPReportComponent';
import { SCBKSJUReport } from './RightMenu/ReportMenu/ReportTypes/SCBKSJUReport/SCBKSJUReportComponent';
import { SCBKSPReport } from './RightMenu/ReportMenu/ReportTypes/SCBKSPReport/SCBKSPReportComponent';
import { SCBSLPReport } from './RightMenu/ReportMenu/ReportTypes/SCBSLPReport/SCBSLPReportComponent';
import { SNReport } from './RightMenu/ReportMenu/ReportTypes/SNReport/SNReportComponent';
import { PayrollSlipReport } from './RightMenu/ReportMenu/ReportTypes/PayrollSlipReport/PayrollSlipReportComponent';
import { AcademyMenuDirectiveFactory } from './RightMenu/AcademyMenu/AcademyMenuDirective';
import { Ku10Report } from './RightMenu/ReportMenu/ReportTypes/Ku10Report/Ku10ReportComponent';
import { MessageMenuDirectiveFactory } from './RightMenu/MessageMenu/MessageMenuDirective';
import { DocumentMenuDirectiveFactory } from './RightMenu/DocumentMenu/DocumentMenuDirective';
import { PdfViewerDirective } from '../Common/PdfViewer/pdfViewerDirective';
import { InformationMenuDirectiveFactory } from './RightMenu/InformationMenu/InformationMenuDirective';
import { CommonCustomerService } from "../Common/Customer/CommonCustomerService";
import 'angular-bootstrap-contextmenu';
import { RoleReport } from './RightMenu/ReportMenu/ReportTypes/RoleReport/RoleReportComponent';
import { FileUploadDirectiveFactory } from '../Common/Directives/FileUpload/FileUploadDirective';
import { FileDisplayDirectiveFactory } from '../Common/Directives/FileDisplay/FileDisplayDirective';
import { FileItemDisplayDirectiveFactory } from '../Common/Directives/FileItemDisplay/FileItemDisplayDirective';
import { PayrollVacationAccountingReport } from './RightMenu/ReportMenu/ReportTypes/PayrollVacationAccountingReport/PayrollVacationAccountingReportComponent';
import { BygglosenReport } from './RightMenu/ReportMenu/ReportTypes/BygglosenReport/BygglosenReportComponent';
import { KronofogdenReport } from './RightMenu/ReportMenu/ReportTypes/KronofogdenReport/KronofogdenReportComponent';
import { PaymentJournalSupplierReport } from './RightMenu/ReportMenu/ReportTypes/Economy/PaymentJournalSupplierReport/PaymentJournalSupplierReportComponent';
import { OrderReport } from './RightMenu/ReportMenu/ReportTypes/Billing/OrderReport/OrderReportComponent';
import { ExpenseReport } from './RightMenu/ReportMenu/ReportTypes/Billing/ExpenseReport/ExpenseReportComponent';
import { ContractReport } from './RightMenu/ReportMenu/ReportTypes/Billing/ContractReport/ContractReportComponent';
import { PurchaseOrderReport } from './RightMenu/ReportMenu/ReportTypes/Billing/PurchaseOrderReport/PurchaseOrderReportComponent';
import { TaxReductionReport } from './RightMenu/ReportMenu/ReportTypes/Billing/TaxReductionReport/TaxReductionReportComponent';
import { EmployeeDateReport } from './RightMenu/ReportMenu/ReportTypes/EmployeeDateReport/EmployeeDateReportComponent';
import { RecordNavigatorDirectiveFactory } from '../Common/Directives/RecordNavigator/RecordNavigatorDirective';
import { UserReport } from './RightMenu/ReportMenu/ReportTypes/UserReport/UserReportComponent';
import { SupplierReport } from './RightMenu/ReportMenu/ReportTypes/Economy/SupplierReport/SupplierReportComponent';
import { ReportStatisticsReport } from './RightMenu/ReportMenu/ReportTypes/ReportStatisticsReport/ReportStatisticsReportComponent';
import { EmployeeExperienceReport } from './RightMenu/ReportMenu/ReportTypes/EmployeeExperienceReport/EmployeeExperienceComponent';
import { InvoiceProductReport } from './RightMenu/ReportMenu/ReportTypes/Billing/InvoiceProductReport/InvoiceProductReportComponent';
import { InvoiceProductUnitConvertReport } from './RightMenu/ReportMenu/ReportTypes/Billing/InvoiceProductUnitConvertReport/InvoiceProductUnitConvertReportComponent';
import { StaffingneedsFrequencyReport } from './RightMenu/ReportMenu/ReportTypes/StaffingneedsFrequencyReport/StaffingneedsFrequencyReportComponent';
import { EmployeeSkillReport } from './RightMenu/ReportMenu/ReportTypes/EmployeeSkillReport/EmployeeSkillReportComponent';
import { OrganisationHrReport } from './RightMenu/ReportMenu/ReportTypes/OrganisationHrReport/OrganisationHrReportComponent';
import { ShiftTypeSkillReport } from './RightMenu/ReportMenu/ReportTypes/ShiftTypeSkillReport/ShiftTypeSkillReportComponent';
import { EmployeeSalaryReport } from './RightMenu/ReportMenu/ReportTypes/EmployeeSalaryReport/EmployeeSalaryReportComponent';
import { EmployeeMeetingReport } from './RightMenu/ReportMenu/ReportTypes/EmployeeMeetingReport/EmployeeMeetingReportComponent';
import { EmployeeDocumentReport } from './RightMenu/ReportMenu/ReportTypes/EmployeeDocumentReport/EmployeeDocumentReportComponent';
import { EmployeeAccountReport } from './RightMenu/ReportMenu/ReportTypes/EmployeeAccountReport/EmployeeAccountReportComponent';
import { CustomerReport } from './RightMenu/ReportMenu/ReportTypes/Economy/CustomerReport/CustomerReportComponent';
import { ResultReportV2 } from './RightMenu/ReportMenu/ReportTypes/Economy/ResultReportV2/ResultReportV2Component';
import { SalesStatistikReport } from './RightMenu/ReportMenu/ReportTypes/Billing/SalesStatistikReport/SalesStatistikReportComponent';
import { ProjectTransactionsReport } from './RightMenu/ReportMenu/ReportTypes/Billing/ProjectTransactionsReport/ProjectTransactionsReportComponent';
import { InterestInvoiceReport } from './RightMenu/ReportMenu/ReportTypes/Billing/InterestInvoiceReport/InterestInvoiceReportComponent';
import { TimeStampHistoryReport } from './RightMenu/ReportMenu/ReportTypes/TimeStampHistoryReport/TimeStampHistoryReportComponent'
import { AgiAbsenceReport } from './RightMenu/ReportMenu/ReportTypes/AgiAbsenceReport/AgiAbsenceReportComponent'; 
import { UserSelection } from '../Common/Components/UserSelection/UserSelectionComponent';
import { UserSelections } from './RightMenu/ReportMenu/Components/UserSelections/UserSelectionsComponent';

import { AccountingYearAndPeriodRangeSelection } from './RightMenu/ReportMenu/Selections/AccountingYearAndPeriodRangeSelection/AccountingYearAndPeriodRangeSelectionComponent';
import { AccountRangeSelection } from './RightMenu/ReportMenu/Selections/AccountRangeSelection/AccountRangeSelectionComponent';
import { BoolSelection } from './RightMenu/ReportMenu/Selections/BoolSelection/BoolSelectionComponent';
import { CategorySelection } from './RightMenu/ReportMenu/Selections/CategorySelection/CategorySelectionComponent';
import { DateSelection } from './RightMenu/ReportMenu/Selections/DateSelection/DateSelectionComponent';
import { DateTimeIntervalSelection } from './RightMenu/ReportMenu/Selections/DateTimeIntervalSelection/DateTimeIntervalSelectionComponent';
import { EmployeeRangeSelection } from './RightMenu/ReportMenu/Selections/EmployeeRangeSelection/EmployeeRangeSelectionComponent';
import { EmployeesSelection } from './RightMenu/ReportMenu/Selections/EmployeesSelection/EmployeesSelectionComponent';
import { IdListSelection } from './RightMenu/ReportMenu/Selections/IdListSelection/IdListSelectionComponent';
import { IdSelection } from './RightMenu/ReportMenu/Selections/IdSelection/IdSelectionComponent';
import { InventoryAndCategoryRangeSelection } from './RightMenu/ReportMenu/Selections/InventoryAndCategoryRangeSelection/InventoryAndCategoryRangeSelectionComponent';
import { JournalRangeSelection } from './RightMenu/ReportMenu/Selections/JournalRangeSelection/JournalRangeSelectionComponent';
import { MatrixColumnSelection } from './RightMenu/ReportMenu/Selections/MatrixColumnSelection/MatrixColumnSelectionComponent';
import { MonthSelection } from './RightMenu/ReportMenu/Selections/MonthSelection/MonthSelectionComponent';
import { PayrollMonthYearSelection } from './RightMenu/ReportMenu/Selections/PayrollMonthYearSelection/PayrollMonthYearSelectionComponent';
import { PayrollPeriodSelection } from './RightMenu/ReportMenu/Selections/PayrollPeriodSelection/PayrollPeriodSelectionComponent';
import { PayrollPriceTypeSelection } from './RightMenu/ReportMenu/Selections/PayrollPriceTypeSelection/PayrollPriceTypeSelectionComponent';
import { PayrollProductRowSelection } from './RightMenu/ReportMenu/Selections/PayrollProductSelection/PayrollProductRowSelectionComponent';
import { PayrollProductSelection } from './RightMenu/ReportMenu/Selections/PayrollProductSelection/PayrollProductSelectionComponent';
import { PlanningPeriodSelection } from './RightMenu/ReportMenu/Selections/PlanningPeriodSelection/PlanningPeriodSelectionComponent';
import { ProjectSelection } from './RightMenu/ReportMenu/Selections/ProjectSelection/ProjectSelectionComponent';
import { ShiftTypeSelection } from './RightMenu/ReportMenu/Selections/ShiftTypeSelection/ShiftTypeSelectionComponent';
import { TestAllSelections } from './RightMenu/ReportMenu/ReportTypes/TestAllSelections/TestAllSelectionsComponent';
import { TextSelection } from './RightMenu/ReportMenu/Selections/TextSelection/TextSelectionComponent';
import { TimeAccumulatorSelection } from './RightMenu/ReportMenu/Selections/TimeAccumulatorSelection/TimeAccumulatorSelectionComponent';
import { TimeIntervalSelection } from './RightMenu/ReportMenu/Selections/TimeIntervalSelection/TimeIntervalSelectionComponent';
import { TimePeriodSelection } from './RightMenu/ReportMenu/Selections/TimePeriodSelection/TimePeriodSelectionComponent';
import { UsersSelection } from './RightMenu/ReportMenu/Selections/UsersSelection/UsersSelectionComponent';
import { VacationGroupSelection } from './RightMenu/ReportMenu/Selections/VacationGroupSelection/VacationGroupSelectionComponent';
import { WeekSelection } from './RightMenu/ReportMenu/Selections/WeekSelection/WeekSelectionComponent';
import { BarInsightController } from './RightMenu/ReportMenu/Insights/Common/BarInsightController';
import { LineInsightController } from './RightMenu/ReportMenu/Insights/Common/LineInsightController';
import { PieInsightController } from './RightMenu/ReportMenu/Insights/Common/PieInsightController';
import { AreaInsightController } from './RightMenu/ReportMenu/Insights/Common/AreaInsightController';
import { TreemapInsightController } from './RightMenu/ReportMenu/Insights/Common/TreemapInsightController';
import { AggregatedTimeStatisticsReport } from './RightMenu/ReportMenu/ReportTypes/AggregatedTimeStatisticsReport/AggregatedTimeStatisticsReportComponent';
import { StaffingneedsStatisticsReport } from './RightMenu/ReportMenu/ReportTypes/StaffingneedsStatisticsReport/StaffingneedsStatisticsReportComponent';
import { GridFilterSelection } from './RightMenu/ReportMenu/Selections/GridFilterSelection/GridFilterSelectionComponent';
import { ScheduledTimeSummaryReport } from './RightMenu/ReportMenu/ReportTypes/ScheduledTimeSummaryReport/ScheduledTimeSummaryReportComponent';
import { InsightController } from './RightMenu/ReportMenu/Insights/Common/InsightController';
import { GTPReport } from './RightMenu/ReportMenu/ReportTypes/GTPReport/GTPReportComponent';
import { SkandiaPensionReport } from './RightMenu/ReportMenu/ReportTypes/SkandiaPensionReport/SkandiaPensionReportComponent';
import { SoftOneStatusEventReport } from './RightMenu/ReportMenu/ReportTypes/SoftOneStatusEventReport/SoftOneStatusEventReportComponent';
import { SoftOneStatusResultReport } from './RightMenu/ReportMenu/ReportTypes/SoftOneStatusResultReport/SoftOneStatusResultReportComponent';
import { SoftOneStatusUpTimeReport } from './RightMenu/ReportMenu/ReportTypes/SoftOneStatusUpTimeReport/SoftOneStatusUpTimeReportComponent';
import { EmploymentHistoryReport } from './RightMenu/ReportMenu/ReportTypes/Time/EmploymentHistoryReport/EmploymentHistoryReportComponent';
import { VacationBalanceReport } from './RightMenu/ReportMenu/ReportTypes/Time/VacationBalanceReport/VacationBalanceReportComponent';
import { EmploymentDaysReport } from './RightMenu/ReportMenu/ReportTypes/EmploymentDaysReport/EmploymentDaysReportComponent';
import { OrderAnalysisReport } from './RightMenu/ReportMenu/ReportTypes/Billing/OrderAnalysisReport/OrderAnalysisReportComponent';
import { InvoiceAnalysisReport } from './RightMenu/ReportMenu/ReportTypes/Billing/InvoiceAnalysisReport/InvoiceAnalysisReportComponent';
import { AccountDimsMultiSelectionDirectiveFactory } from "../Common/Directives/AccountDimsMultiSelection/AccountDimsMultiSelectionDirective";
import { AccountHierarchyReport } from './RightMenu/ReportMenu/ReportTypes/AccountHierarchyReport/AccountHierarchyReportComponent';
import { AccountingService } from "../Shared/Economy/Accounting/AccountingService";
import { IFMetallReport } from './RightMenu/ReportMenu/ReportTypes/IFMetallReport/IFMetallReportComponent';
import { SEFReport } from './RightMenu/ReportMenu/ReportTypes/SEFReport/SEFReportComponent';
import { AnnualProgressReport } from './RightMenu/ReportMenu/ReportTypes/AnnualProgressReport/AnnualProgressReportComponent';
import { LongtermAbsenceReport } from './RightMenu/ReportMenu/ReportTypes/LongtermAbsenceReport/LongtermAbsenceReportComponent';
import { VoucherSeriesSelection } from './RightMenu/ReportMenu/Selections/VoucherSeriesSelection/VoucherSeriesSelectionComponent';
import { ProductGroupSelection } from './RightMenu/ReportMenu/Selections/ProductGroupSelection/ProductGroupSelectionComponent';
import { ShiftQueueReport } from './RightMenu/ReportMenu/ReportTypes/ShiftQueueReport/ShiftQueueReportComponent';
import { ShiftHistoryReport } from './RightMenu/ReportMenu/ReportTypes/ShiftHistoryReport/ShiftHistoryReportComponent';
import { ShiftRequestReport } from './RightMenu/ReportMenu/ReportTypes/ShiftRequestReport/ShiftRequestReportComponent';
import { SwapShiftReport } from './RightMenu/ReportMenu/ReportTypes/SwapShiftReport/SwapShiftReportComponent';
import { AbsenceRequestReport } from './RightMenu/ReportMenu/ReportTypes/AbsenceRequestReport/AbsenceRequestReportComponent';
import { ForaMonthlyReport } from './RightMenu/ReportMenu/ReportTypes/ForaMonthlyReport/ForaMonthlyReportComponent';
import { VismaPayrollChangesReport } from './RightMenu/ReportMenu/ReportTypes/VismaPayrollChangesReport/VismaPayrollChangesReportComponent';
import { DemandLetterReport } from './RightMenu/ReportMenu/ReportTypes/Economy/DemandLetterReport/DemandLetterReportComponent';
import { SupplierInvoiceEdiReport } from './RightMenu/ReportMenu/ReportTypes/Economy/SupplierInvoiceEdiReport/SupplierInvoiceEdiReportComponent';
import { FinvoiceSupplierInvoiceReport } from './RightMenu/ReportMenu/ReportTypes/Economy/FinvoiceSupplierInvoiceReport/FinvoiceSupplierInvoiceReportComponent';
import { ImportCustomerInvoiceReport } from './RightMenu/ReportMenu/ReportTypes/Economy/ImportCustomerInvoiceReport/ImportCustomerInvoiceReportComponent';
import { ImportVoucherReport } from './RightMenu/ReportMenu/ReportTypes/Economy/ImportVoucherReport/ImportVoucherReportComponent';
import { InterestEstimateReport } from './RightMenu/ReportMenu/ReportTypes/Economy/InterestEstimateReport/InterestEstimateReportComponent';
import { OrderChecklistReport } from './RightMenu/ReportMenu/ReportTypes/Billing/OrderChecklistReport/OrderChecklistReportComponent';
import { VerticalTimeTrackerReport } from './RightMenu/ReportMenu/ReportTypes/Time/VerticalTimeTrackerReport/VerticalTimeTrackerReportComponent';
import { HorizontalTimeTrackerReport } from './RightMenu/ReportMenu/ReportTypes/Time/HorizontalTimeTrackerReport/HorizontalTimeTrackerReportComponent';
import { LicenseInformationReport } from './RightMenu/ReportMenu/ReportTypes/LicenseInformationReport/LicenseInformationReportComponent';
import { OrderAccountsByStartsWithFirst } from './Pipes/OrderAccountsByStartsWithFirst';
import { PayrollWarningsFactory } from '../Common/Directives/PayrollWarnings/PayrollWarningsDirective';
import { InventoryAnalysisReport } from './RightMenu/ReportMenu/ReportTypes/Economy/InventoryAnalysisReport/InventoryAnalysisReportComponent';
import { DepreciationAnalysisReport } from './RightMenu/ReportMenu/ReportTypes/Economy/DepreciationAnalysisReport/DepreciationAnalysisReportComponent';
import { EmployeeChildReport } from './RightMenu/ReportMenu/ReportTypes/EmployeeChildReport/EmployeeChildReportComponent';
import { EmployeePayrollAdditionsReport } from './RightMenu/ReportMenu/ReportTypes/EmployeePayrollAdditionsReport/EmployeePayrollAdditionsReportComponent';
import { AnnualLeaveTransactionReport } from './RightMenu/ReportMenu/ReportTypes/AnnualLeaveTransactionReport/AnnualLeaveTransactionReportComponent';
import { FixedContentFactory } from './Directives/FixedContent';
import { RequestReportService } from '../Shared/Reports/RequestReportService';
import { RequestReportApiService } from '../Shared/Reports/RequestReportApiService';
import { InvoiceStatisticsReport } from './RightMenu/ReportMenu/ReportTypes/Billing/InvoiceStatisticsReport/InvoiceStatisticsReportComponent';
import { ReportFieldSettingFactory } from '../Common/Directives/ReportFieldSetting/ReportFieldSettingDirective';
import { ReportMetaFieldsFactory } from '../Common/Directives/ReportMetaFields/ReportMetaFieldsDirective';
import { InventoryCategorySelection } from './RightMenu/ReportMenu/Selections/InventorySelections/InventoryCategorySelection/InventoryCategorySelectionComponent';
import { InventoryStatusSelection } from './RightMenu/ReportMenu/Selections/InventorySelections/InventoryStatusSelection/InventoryStatusSelectionComponent';
import { PrognoseTypeSelection } from './RightMenu/ReportMenu/Selections/PrognoseTypeSelection/PrognoseTypeSelectionComponent';
import { TimeScheduleCopyReport } from './RightMenu/ReportMenu/ReportTypes/TimeScheduleCopyReport/TimeScheduleCopyReportComponent';
import { TaxReductionBalanceListReport } from './RightMenu/ReportMenu/ReportTypes/Billing/TaxReductionBalanceListReport/TaxReductionBalanceListReportComponent';

declare var agGrid;

agGrid.LicenseManager.setLicenseKey("CompanyName=Softone Applications AB,LicensedApplication=GO,LicenseType=SingleApplication,LicensedConcurrentDeveloperCount=2,LicensedProductionInstancesCount=3,AssetReference=AG-028405,ExpiryDate=1_July_2023_[v2]_MTY4ODE2NjAwMDAwMA==ad75f682a0646302913cc1e62225e9ca");
agGrid.initialiseAgGridWithAngular1(angular);

angular.module("Soe.Core", [
    'pascalprecht.translate', 'ngSanitize', 'ui.bootstrap', 'ui.router', 'ui.indeterminate',
    'ui.grid', 'ui.grid.autoResize', 'ui.grid.resizeColumns', 'ui.grid.moveColumns', 'ui.grid.pinning', 'ui.grid.selection',
    'ui.grid.saveState', 'ui.grid.exporter', 'ui.grid.edit', 'ui.grid.cellNav', 'ui.grid.expandable', 'ui.grid.grouping',
    'ui.select', 'angularjs-dropdown-multiselect', 'angularFileUpload', 'angularMoment', 'ngLocale',
    'agGrid', 'minicolors', 'cfp.hotkeys', 'oc.lazyLoad', 'ui.bootstrap.contextMenu'
])
    .provider("httpService", HttpServiceProvider)
    .provider("urlHelperService", UrlHelperServiceProvider)
    .provider("termPartsLoader", TermPartsLoaderProvider)
    .provider("fileUploaderFactory", FileUploaderFactoryProvider)
    .provider("authenticationService", AuthenticationServiceProvider)
    .service("lazyLoadService", LazyLoadService)
    .service("translationService", TranslationService)
    .service("messagingService", MessagingService)
    .service("notificationService", NotificationService)
    .service("reportService", ReportService)
    .service("coreService", CoreService)
    .service("accountingService", AccountingService)
    .service("storageService", StorageService)
    .service("focusService", FocusService)
    .service("dateHelperService", DateHelperService)
    .service("angularFeatureCheckService", AngularFeatureCheckService)
    .service("progressHandlerFactory", ProgressHandlerFactory)
    .service("reportJobDefinitionFactory", ReportJobDefinitionFactory)
    .service("controllerFlowHandlerFactory", ControllerFlowHandlerFactory)
    .service("toolbarFactory", ToolbarFactory)
    .service("validationSummaryHandlerFactory", ValidationSummaryHandlerFactory)
    .service("messagingHandlerFactory", MessagingHandlerFactory)
    .service("tabHandlerFactory", TabHandlerFactory)
    .service("dirtyHandlerFactory", DirtyHandlerFactory)
    .service("gridHandlerFactory", GridHandlerFactory)
    .service("shortCutService", ShortCutService)
    .service("selectedItemsService", SelectedItemsService)
    .service("contextMenuHandlerFactory", ContextMenuHandlerFactory)
    .service("scopeTreeTraverserService", ScopeTreeTraverserService)
    .service("scopeWatcherService", ScopeWatcherService)
    .service("reportDataService", ReportDataService)
    .service("commonCustomerService", CommonCustomerService)
    .service("requestReportService", RequestReportService)
    .service("requestReportApiService", RequestReportApiService)

    .directive("parseDate", ParseDateDirectiveFactory.create)
    .directive("parseTime", ParseTimeDirectiveFactory.create)
    .directive("parseTimebox", ParseTimeboxDirectiveFactory.create)
    .directive("debug", DebugDirectiveFactory.create)
    .directive("decimalPercentage", DecimalPercentageDirectiveFactory.create)
    .directive("decimal", DecimalDirectiveFactory.create)
    .directive("confirmOnExit", ConfirmOnExitDirectiveFactory.create)
    .directive("createdModified", CreatedModifiedDirectiveFactory.create)
    .directive("formState", FormStateDirectiveFactory.create)
    .directive("l10nBind", L10NBindDirectiveFactory.create)
    .directive("enter", EnterDirectiveFactory.create)
    .directive("preventEnter", PreventEnterDirectiveFactory.create)
    .directive("enterFocus", EnterFocusDirectiveFactory.create)
    .directive("eventFocus", EventFocusDirectiveFactory.create)
    .directive("focusOnShow", FocusOnShowDirectiveFactory.create)
    .directive("setFocus", SetFocusDirectiveFactory.create)
    .directive("tabWithEnter", TabWithEnterDirectiveFactory.create)
    .directive("heightChecker", HeightCheckerFactory.create)
    .directive("fixedContent", FixedContentFactory.create)
    .directive("soeAccordion", SoeAccordionDirectiveFactory.create)
    .directive("soePanel", SoePanelDirectiveFactory.create)
    .directive("recordNavigator", RecordNavigatorDirectiveFactory.create)
    .directive("soeToolbar", SoeToolbarDirectiveFactory.create)
    .directive("soeLabel", SoeLabelDirectiveFactory.create)
    .directive("soeText", SoeTextDirectiveFactory.create)
    .directive("soeInstruction", SoeInstructionDirectiveFactory.create)
    .directive("soeTextbox", SoeTextboxDirectiveFactory.create)
    .directive("soeTextarea", SoeTextareaDirectiveFactory.create)
    .directive("textareaAutoSize", TextareaAutoSizeDirectiveFactory.create)
    .directive("soeCheckbox", SoeCheckboxDirectiveFactory.create)
    .directive("soeRadiobutton", SoeRadiobuttonDirectiveFactory.create)
    .directive("soeColorpicker", SoeColorpickerDirectiveFactory.create)
    .directive("soeDatepicker", SoeDatepickerDirectiveFactory.create)
    .directive("datespicker", DatespickerDirectiveFactory.create)
    .directive("soeTimebox", SoeTimeboxDirectiveFactory.create)
    .directive("soeTimepicker", SoeTimepickerDirectiveFactory.create)
    .directive("soeSelect", SoeSelectDirectiveFactory.create)
    .directive("soeTypeahead", SoeTypeaheadDirectiveFactory.create)
    .directive("soeMultiselect", SoeMultiselectDirectiveFactory.create)
    .directive("soeButton", SoeButtonDirectiveFactory.create)
    .directive("soeProgressbar", SoeProgressbarDirectiveFactory.create)
    .directive("soeCategoryAccounts", SoeCategoryAccountsDirectiveFactory.create)
    .directive("soeMenubutton", SoeMenubuttonDirectiveFactory.create)
    .directive("soeNavigationMenu", SoeNavigationMenuDirectiveFactory.create)
    .directive("soeSplitbutton", SoeSplitbuttonDirectiveFactory.create)
    .directive("dateFilter", DateFilterFactory.create)
    .directive("shapeFilter", ShapeFactory.create)
    .directive("validateAlphaNumeric", ValidateAlphaNumericFactory.create)
    .directive("validateEmail", ValidateEmailFactory.create)
    .directive("validateNumeric", ValidateNumeric.create)
    .directive("validateNumericNotZero", ValidateNumericNotZero.create)
    .directive("validateSocialSecurityNumber", ValidateSocialSecurityNumberFactory.create)
    .directive("gridKeypress", GridKeypressDirectiveFactory.create)
    .directive("tabController", TabControllerDirectiveFactory.create)
    .directive("uiGridEditDropdownWithFocusDelay", uiGridEditDropdownWithFocusDelay.create)
    .directive("uiGridTypeaheadEditor", uiGridTypeaheadEditor.create)
    .directive("uiGridUiSelect", uiGridUiSelect.create)
    .directive("uiGridEditDatepicker", UIGridEditDatepickerFactory.create)
    .directive("informationMenu", InformationMenuDirectiveFactory.create)
    .directive("helpMenu", HelpMenuDirectiveFactory.create)
    .directive("helpMenuContent", HelpMenuContentFactory.create)
    .directive("academyMenu", AcademyMenuDirectiveFactory.create)
    .directive("documentMenu", DocumentMenuDirectiveFactory.create)
    .directive("messageMenu", MessageMenuDirectiveFactory.create)
    .directive("reportMenu", ReportMenuDirectiveFactory.create)
    .directive("conditionalFocus", ConditionalFocusDirective.create)
    .directive("ignoreDirty", IgnoreDirtyDirectiveFactory.create)
    .directive("uibModalWindow", DraggableDialogDirectiveFactory.create)
    .directive("whenRendered", WhenRenderedDirectiveFactory.create)
    .directive("windowBlur", WindowBlurDirectiveFactory.create)
    .directive("windowFocus", WindowFocusDirectiveFactory.create)
    .directive("infoBar", SoeInfobarDirectiveFactory.create)
    .directive("pdfViewer", PdfViewerDirective)
    .directive("soeFileUpload", FileUploadDirectiveFactory.create)
    .directive("soeFileDisplay", FileDisplayDirectiveFactory.create)
    .directive("soeFileItemDisplay", FileItemDisplayDirectiveFactory.create)
    .directive("accountDimsMultiSelection", AccountDimsMultiSelectionDirectiveFactory.create)
    .directive("soePayrollWarnings", PayrollWarningsFactory.create)
    .directive("soeReportMetaFields", ReportMetaFieldsFactory.create)
    .directive("reportFieldSetting", ReportFieldSettingFactory.create)

    // Selection components
    .component(AccountingYearAndPeriodRangeSelection.componentKey, AccountingYearAndPeriodRangeSelection.component())
    .component(AccountRangeSelection.componentKey, AccountRangeSelection.component())
    .component(BoolSelection.componentKey, BoolSelection.component())
    .component(CategorySelection.componentKey, CategorySelection.component())
    .component(DateSelection.componentKey, DateSelection.component())
    .component(DateTimeIntervalSelection.componentKey, DateTimeIntervalSelection.component())
    .component(EmployeeRangeSelection.componentKey, EmployeeRangeSelection.component())
    .component(EmployeesSelection.componentKey, EmployeesSelection.component())
    .component(IdListSelection.componentKey, IdListSelection.component())
    .component(IdSelection.componentKey, IdSelection.component())
    .component(InventoryAndCategoryRangeSelection.componentKey, InventoryAndCategoryRangeSelection.component())
    .component(JournalRangeSelection.componentKey, JournalRangeSelection.component())
    .component(MatrixColumnSelection.componentKey, MatrixColumnSelection.component())
    .component(MonthSelection.componentKey, MonthSelection.component())
    .component(PayrollMonthYearSelection.componentKey, PayrollMonthYearSelection.component())
    .component(PayrollPeriodSelection.componentKey, PayrollPeriodSelection.component())
    .component(PayrollPriceTypeSelection.componentKey, PayrollPriceTypeSelection.component())
    .component(PayrollProductRowSelection.componentKey, PayrollProductRowSelection.component())
    .component(PayrollProductSelection.componentKey, PayrollProductSelection.component())
    .component(PlanningPeriodSelection.componentKey, PlanningPeriodSelection.component())
    .component(ProjectSelection.componentKey, ProjectSelection.component())
    .component(ShiftTypeSelection.componentKey, ShiftTypeSelection.component())
    .component(TestAllSelections.componentKey, TestAllSelections.component())
    .component(TextSelection.componentKey, TextSelection.component())
    .component(TimeAccumulatorSelection.componentKey, TimeAccumulatorSelection.component())
    .component(TimeIntervalSelection.componentKey, TimeIntervalSelection.component())
    .component(TimePeriodSelection.componentKey, TimePeriodSelection.component())
    .component(UserSelection.componentKey, UserSelection.component())
    .component(UserSelections.componentKey, UserSelections.component())
    .component(UsersSelection.componentKey, UsersSelection.component())
    .component(VacationGroupSelection.componentKey, VacationGroupSelection.component())
    .component(WeekSelection.componentKey, WeekSelection.component())
    .component(InventoryCategorySelection.componentKey, InventoryCategorySelection.component())
    .component(InventoryStatusSelection.componentKey, InventoryStatusSelection.component())
    .component(PrognoseTypeSelection.componentKey, PrognoseTypeSelection.component())

    // Reports
    .component(GenericReport.componentKey, GenericReport.component())
    .component(AgdEmployeeReport.componentKey, AgdEmployeeReport.component())
    .component(AgdEmployeeAbsenceReport.componentKey, AgdEmployeeAbsenceReport.component())
    .component(CertificateOfEmploymentReport.componentKey, CertificateOfEmploymentReport.component())
    .component(CollectumReport.componentKey, CollectumReport.component())
    .component(EmployeeListReport.componentKey, EmployeeListReport.component())
    .component(BalanceReport.componentKey, BalanceReport.component())
    .component(OrderReport.componentKey, OrderReport.component())
    .component(ExpenseReport.componentKey, ExpenseReport.component())
    .component(ContractReport.componentKey, ContractReport.component())
    .component(StockListReport.componentKey, StockListReport.component())
    .component(ProductListReport.componentKey, ProductListReport.component())
    .component(PurchaseOrderReport.componentKey, PurchaseOrderReport.component())
    .component(OfferReport.componentKey, OfferReport.component())
    .component(InvoiceReport.componentKey, InvoiceReport.component())
    .component(ProjektstatistikReport.componentKey, ProjektstatistikReport.component())
    .component(ProjekttidReport.componentKey, ProjekttidReport.component())
    .component(StockInventoryReport.componentKey, StockInventoryReport.component())
    .component(InvoiceJournalCustomerReport.componentKey, InvoiceJournalCustomerReport.component())
    .component(CustomerPaymentJournalReport.componentKey, CustomerPaymentJournalReport.component())
    .component(PaymentJournalSupplierReport.componentKey, PaymentJournalSupplierReport.component())
    .component(GeneralLedgerReport.componentKey, GeneralLedgerReport.component())
    .component(VoucherListReport.componentKey, VoucherListReport.component())
    .component(PeriodAccountingForecastReport.componentKey, PeriodAccountingForecastReport.component())
    .component(StockHistoryReport.componentKey, StockHistoryReport.component())
    .component(PeriodAccountingRegulationsReport.componentKey, PeriodAccountingRegulationsReport.component())
    .component(EmployeeTimePeriodReport.componentKey, EmployeeTimePeriodReport.component())
    .component(EmployeeDateReport.componentKey, EmployeeDateReport.component())
    .component(UserReport.componentKey, UserReport.component())
    .component(EmployeeSalaryReport.componentKey, EmployeeSalaryReport.component())
    .component(EmployeeMeetingReport.componentKey, EmployeeMeetingReport.component())
    .component(EmployeeDocumentReport.componentKey, EmployeeDocumentReport.component())
    .component(EmployeeAccountReport.componentKey, EmployeeAccountReport.component())
    .component(ReportStatisticsReport.componentKey, ReportStatisticsReport.component())
    .component(SupplierReport.componentKey, SupplierReport.component())
    .component(EmployeeExperienceReport.componentKey, EmployeeExperienceReport.component())
    .component(ShiftTypeSkillReport.componentKey, ShiftTypeSkillReport.component())
    .component(CustomerReport.componentKey, CustomerReport.component())
    .component(InventoryAnalysisReport.componentKey, InventoryAnalysisReport.component())
    .component(DepreciationAnalysisReport.componentKey, DepreciationAnalysisReport.component())
    .component(EmployeeSkillReport.componentKey, EmployeeSkillReport.component())
    .component(EmployeeEndReasonsReport.componentKey, EmployeeEndReasonsReport.component())
    .component(EmployeeFixedPayLinesReport.componentKey, EmployeeFixedPayLinesReport.component())
    .component(PayrollProductsReport.componentKey, PayrollProductsReport.component())
    .component(EmployeeSalaryDistressReport.componentKey, EmployeeSalaryDistressReport.component())
    .component(EmployeeSalaryUnionFeesReport.componentKey, EmployeeSalaryUnionFeesReport.component())
    .component(EmployeeVacationDebtReport.componentKey, EmployeeVacationDebtReport.component())
    .component(EmployeeVacationInformationReport.componentKey, EmployeeVacationInformationReport.component())
    .component(EmployeeChildReport.componentKey, EmployeeChildReport.component())
    .component(EmployeePayrollAdditionsReport.componentKey, EmployeePayrollAdditionsReport.component())
    .component(ForaReport.componentKey, ForaReport.component())
    .component(ForaMonthlyReport.componentKey, ForaMonthlyReport.component())
    .component(ResultReport.componentKey, ResultReport.component())
    .component(SruReport.componentKey, SruReport.component())
    .component(CustomerBalanceListReport.componentKey, CustomerBalanceListReport.component())
    .component(TaxReductionReport.componentKey, TaxReductionReport.component())
    .component(SupplierBalanceListReport.componentKey, SupplierBalanceListReport.component())
    .component(InvoiceJournalSupplierReport.componentKey, InvoiceJournalSupplierReport.component())
    .component(TaxReturnReport.componentKey, TaxReturnReport.component())
    .component(TaxReturnFnReport.componentKey, TaxReturnFnReport.component())
    .component(FixedAssetReport.componentKey, FixedAssetReport.component())
    .component(TimeReport.componentKey, TimeReport.component())
    .component(KPAReport.componentKey, KPAReport.component())
    .component(KPADirektReport.componentKey, KPADirektReport.component())
    .component(GTPReport.componentKey, GTPReport.component())
    .component(SkandiaPensionReport.componentKey, SkandiaPensionReport.component())
    .component(BygglosenReport.componentKey, BygglosenReport.component())
    .component(KronofogdenReport.componentKey, KronofogdenReport.component())
    .component(IFMetallReport.componentKey, IFMetallReport.component())
    .component(SEFReport.componentKey, SEFReport.component())
    .component(Ku10Report.componentKey, Ku10Report.component())
    .component(PayrollAccountingReport.componentKey, PayrollAccountingReport.component())
    .component(PayrollVacationAccountingReport.componentKey, PayrollVacationAccountingReport.component())
    .component(PayrollPeriodWarningCheckReport.componentKey, PayrollPeriodWarningCheckReport.component())
    .component(PayrollProductReport.componentKey, PayrollProductReport.component())
    .component(PayrollSlipReport.componentKey, PayrollSlipReport.component())
    .component(PayrollTransactionStatisticsReport.componentKey, PayrollTransactionStatisticsReport.component())
    .component(RoleReport.componentKey, RoleReport.component())
    .component(SCBKLPReport.componentKey, SCBKLPReport.component())
    .component(SCBKSJUReport.componentKey, SCBKSJUReport.component())
    .component(SCBKSPReport.componentKey, SCBKSPReport.component())
    .component(SCBSLPReport.componentKey, SCBSLPReport.component())
    .component(SKDReport.componentKey, SKDReport.component())
    .component(SNReport.componentKey, SNReport.component())
    .component(TimeAccumulatorDetailedReport.componentKey, TimeAccumulatorDetailedReport.component())
    .component(TimeAccumulatorReport.componentKey, TimeAccumulatorReport.component())
    .component(TimeCategoryScheduleReport.componentKey, TimeCategoryScheduleReport.component())
    .component(TimeCategoryStatisticsReport.componentKey, TimeCategoryStatisticsReport.component())
    .component(TimeEmploymentContractReport.componentKey, TimeEmploymentContractReport.component())
    .component(TimeEmployeeLineScheduleReport.componentKey, TimeEmployeeLineScheduleReport.component())
    .component(TimeEmployeeScheduleReport.componentKey, TimeEmployeeScheduleReport.component())
    .component(TimeMonthlyReport.componentKey, TimeMonthlyReport.component())
    .component(TimePayrollTransactionReport.componentKey, TimePayrollTransactionReport.component())
    .component(TimeSalaryControlInfoReport.componentKey, TimeSalaryControlInfoReport.component())
    .component(TimeSalarySpecificationReport.componentKey, TimeSalarySpecificationReport.component())
    .component(TimeScheduleBlockHistoryReport.componentKey, TimeScheduleBlockHistoryReport.component())
    .component(TimeScheduleTasksAndDeliverysReport.componentKey, TimeScheduleTasksAndDeliverysReport.component())
    .component(TimeStampEntryReport.componentKey, TimeStampEntryReport.component())
    .component(InvoiceProductReport.componentKey, InvoiceProductReport.component())
    .component(InvoiceProductUnitConvertReport.componentKey, InvoiceProductUnitConvertReport.component())
    .component(StaffingneedsFrequencyReport.componentKey, StaffingneedsFrequencyReport.component())
    .component(OrganisationHrReport.componentKey, OrganisationHrReport.component())
    .component(ResultReportV2.componentKey, ResultReportV2.component())
    .component(InsightController.componentKey, InsightController.component())
    .component(SalesStatistikReport.componentKey, SalesStatistikReport.component())
    .component(BarInsightController.componentKey, BarInsightController.component())
    .component(LineInsightController.componentKey, LineInsightController.component())
    .component(PieInsightController.componentKey, PieInsightController.component())
    .component(AreaInsightController.componentKey, AreaInsightController.component())
    .component(TreemapInsightController.componentKey, TreemapInsightController.component())
    .component(AggregatedTimeStatisticsReport.componentKey, AggregatedTimeStatisticsReport.component())
    .component(StaffingneedsStatisticsReport.componentKey, StaffingneedsStatisticsReport.component())
    .component(ProjectTransactionsReport.componentKey, ProjectTransactionsReport.component())
    .component(InterestInvoiceReport.componentKey, InterestInvoiceReport.component())
    .component(GridFilterSelection.componentKey, GridFilterSelection.component())
    .component(ScheduledTimeSummaryReport.componentKey, ScheduledTimeSummaryReport.component())
    .component(SoftOneStatusEventReport.componentKey, SoftOneStatusEventReport.component())
    .component(SoftOneStatusResultReport.componentKey, SoftOneStatusResultReport.component())
    .component(SoftOneStatusUpTimeReport.componentKey, SoftOneStatusUpTimeReport.component())
    .component(LicenseInformationReport.componentKey, LicenseInformationReport.component())
    .component(EmploymentHistoryReport.componentKey, EmploymentHistoryReport.component())
    .component(VerticalTimeTrackerReport.componentKey, VerticalTimeTrackerReport.component())
    .component(HorizontalTimeTrackerReport.componentKey, HorizontalTimeTrackerReport.component())
    .component(VacationBalanceReport.componentKey, VacationBalanceReport.component())
    .component(ShiftQueueReport.componentKey, ShiftQueueReport.component())
    .component(ShiftHistoryReport.componentKey, ShiftHistoryReport.component())
    .component(ShiftRequestReport.componentKey, ShiftRequestReport.component())
    .component(SwapShiftReport.componentKey, SwapShiftReport.component())
    .component(AbsenceRequestReport.componentKey, AbsenceRequestReport.component())
    .component(EmploymentDaysReport.componentKey, EmploymentDaysReport.component())
    .component(OrderAnalysisReport.componentKey, OrderAnalysisReport.component())
    .component(InvoiceAnalysisReport.componentKey, InvoiceAnalysisReport.component())
    .component(AccountHierarchyReport.componentKey, AccountHierarchyReport.component())
    .component(AnnualProgressReport.componentKey, AnnualProgressReport.component())
    .component(LongtermAbsenceReport.componentKey, LongtermAbsenceReport.component())
    .component(VoucherSeriesSelection.componentKey, VoucherSeriesSelection.component())
    .component(ProductGroupSelection.componentKey, ProductGroupSelection.component())
    .component(VismaPayrollChangesReport.componentKey, VismaPayrollChangesReport.component())
    .component(TimeStampHistoryReport.componentKey, TimeStampHistoryReport.component())
    .component(AgiAbsenceReport.componentKey, AgiAbsenceReport.component())
    .component(DemandLetterReport.componentKey, DemandLetterReport.component())
    .component(SupplierInvoiceEdiReport.componentKey, SupplierInvoiceEdiReport.component())
    .component(FinvoiceSupplierInvoiceReport.componentKey, FinvoiceSupplierInvoiceReport.component())
    .component(ImportCustomerInvoiceReport.componentKey, ImportCustomerInvoiceReport.component())
    .component(ImportVoucherReport.componentKey, ImportVoucherReport.component())
    .component(InterestEstimateReport.componentKey, InterestEstimateReport.component())
    .component(OrderChecklistReport.componentKey, OrderChecklistReport.component())
    .component(AnnualLeaveTransactionReport.componentKey, AnnualLeaveTransactionReport.component())
    .component(InvoiceStatisticsReport.componentKey, InvoiceStatisticsReport.component())
    .component(TimeScheduleCopyReport.componentKey, TimeScheduleCopyReport.component())
    .component(TaxReductionBalanceListReport.componentKey, TaxReductionBalanceListReport.component())
    .filter('abs', AbsFilter.create)
    .filter('amount', AmountFilter.create)
    .filter('formatHtml', FormatHtmlFilter.create)
    .filter('getDayName', GetDayNameFilter.create)
    .filter('isSameDate', IsSameDateFilter.create)
    .filter('isMonthDate', IsSameMonthFilter.create)
    .filter('minutesToTimeSpan', MinutesToTimeSpanFilter.create)
    .filter('orderObjectBy', OrderObjectByFilter.create)
    .filter('percent', PercentFilter.create)
    .filter('substring', SubstringFilter.create)
    .filter('toUpperCaseFirstLetter', ToUpperCaseFirstLetterFilter.create)
    .filter('orderAccountsByStartsWithFirst', OrderAccountsByStartsWithFirst.create)
    .config(/*@ngInject*/(httpServiceProvider: IHttpServiceProvider,
        $translateProvider,
        $provide,
        $qProvider,
        $httpProvider: ng.IHttpProvider,
        termPartsLoaderProvider: TermPartsLoaderProvider,
        fileUploaderFactoryProvider: IFileUploaderFactoryProvider,
        uiGridConstants: uiGrid.IUiGridConstants,
        uiSelectConfig,
        urlHelperServiceProvider: IUrlHelperServiceProvider,
        authenticationServiceProvider: IAuthenticationServiceProvider) => {
        urlHelperServiceProvider.setPath(soeConfig.baseUrl);

        httpServiceProvider.setPrefix(CoreUtility.apiPrefix);
        httpServiceProvider.setLanguage(CoreUtility.language);
        httpServiceProvider.setSoeParameters(CoreUtility.soeParameters);

        // Combines processing of multiple http responses when received at the same time:
        // https://docs.angularjs.org/api/ng/provider/$httpProvider#useApplyAsync
        $httpProvider.useApplyAsync(true);

        // Translations
        $translateProvider.useLoader('termPartsLoader', { urlTemplate: 'translation/{lang}/{part}' });
        $translateProvider.preferredLanguage(CoreUtility.language);
        termPartsLoaderProvider.setVersion(CoreUtility.termVersionNr);
        termPartsLoaderProvider.addPart('core');
        termPartsLoaderProvider.addPart('error');
        termPartsLoaderProvider.addPart('common');
        //$translateProvider.useSanitizeValueStrategy('sanitize');
        //$translateProvider.useSanitizeValueStrategy('escape');

        // File uploader
        fileUploaderFactoryProvider.setSoeParameters(CoreUtility.soeParameters);
        fileUploaderFactoryProvider.setLanguage(CoreUtility.language);

        // UI-Select
        uiSelectConfig.theme = 'bootstrap';

        //this makes the enter key in the grid go to the right instead of down.
        $provide.decorator('uiGridCellNavService', function ($delegate, uiGridCellNavConstants) {
            var getDirectionOriginal = $delegate.getDirection;

            $delegate.getDirection = function (evt) {

                if (evt.keyCode === 13)//enter
                    return uiGridCellNavConstants.direction.RIGHT;

                return getDirectionOriginal(evt);
            }

            return $delegate;
        });

        $provide.decorator('uiGridExporterService', function ($delegate, $filter, translationService: TranslationService, dateHelperService: DateHelperService) {
            var yesTerm: string;
            var noTerm: string;
            var keys: string[] = ["core.yes", "core.no"];
            translationService.translateMany(keys).then(terms => {
                yesTerm = terms["core.yes"];
                noTerm = terms["core.no"];
            });

            $delegate.formatFieldAsCsv = function (field) {
                if (field.value == null) { // We want to catch anything null-ish, hence just == not ===
                    return '';
                }
                if (typeof (field.value) === 'string') {
                    return '"' + field.value + '"';
                }
                if (typeof (field.value) === 'number') {
                    return field.value;
                }
                if (typeof (field.value) === 'boolean') {
                    return (field.value ? yesTerm : noTerm);
                }
                if (field.value instanceof Date) {
                    return $filter('date')(field.value, dateHelperService.getShortDateFormat());
                }

                return JSON.stringify(field.value);
            };

            return $delegate
        });

        // Set default options on ui-grid
        $provide.decorator('GridOptions', function ($delegate) {
            var gridOptions;
            gridOptions = angular.copy($delegate);
            gridOptions.initialize = function (options) {
                var defaultOptions: any = {};

                defaultOptions.rowHeight = 22;
                defaultOptions.enableColumnResizing = true;
                defaultOptions.enableGridMenu = true;
                defaultOptions.enableSelectAll = true;
                defaultOptions.enableFiltering = true;
                defaultOptions.showGridFooter = true;
                defaultOptions.enableHorizontalScrollbar = uiGridConstants.scrollbars.ALWAYS;
                defaultOptions.enableVerticalScrollbar = uiGridConstants.scrollbars.ALWAYS;

                // Optimizes performance when databinding, but also causes problems if the bound value contains åäö or spaces etc
                //defaultOptions.flatEntityAccess = true; // Performance optimization, if all columns map a property in the bound object

                // CSV export
                defaultOptions.exporterCsvColumnSeparator = ";";
                defaultOptions.exporterCsvLinkElement = angular.element(document.querySelectorAll(".custom-csv-link-location"));

                // PDF export
                defaultOptions.exporterPdfDefaultStyle = { fontSize: 8 };
                defaultOptions.exporterPdfTableStyle = { margin: [0, 0, 0, 0] };
                defaultOptions.exporterPdfTableHeaderStyle = { fontSize: 8, bold: true, italics: true, color: 'black' };
                defaultOptions.exporterPdfCustomFormatter = function (docDefinition) {
                    docDefinition.styles.headerStyle = { fontSize: 12, bold: true };
                    docDefinition.styles.footerStyle = { fontSize: 8, bold: true };
                    return docDefinition;

                };
                defaultOptions.exporterPdfOrientation = 'portrait';
                defaultOptions.exporterPdfPageSize = 'A4';
                defaultOptions.exporterPdfMaxGridWidth = 500;

                // Save state                  
                defaultOptions.saveFilter = false;
                defaultOptions.saveFocus = false;
                defaultOptions.saveGrouping = true;
                defaultOptions.saveGroupingExpandedStates = false;
                defaultOptions.saveOrder = true;
                defaultOptions.savePinning = true;
                defaultOptions.saveRowIdentity = undefined;
                defaultOptions.saveScroll = false;
                defaultOptions.saveSelection = false;
                defaultOptions.saveSort = true;
                defaultOptions.saveTreeView = false;
                defaultOptions.saveVisible = true;
                defaultOptions.saveWidths = true;

                var uiGridOptions = $delegate.initialize({});
                var diff = {};
                for (var x in options) {
                    if (options[x] !== uiGridOptions[x]) {
                        diff[x] = options[x];
                    } else if (options[x] !== defaultOptions[x]) {
                        diff[x] = options[x];
                    }
                }
                return _.extend(uiGridOptions, defaultOptions, diff);
            };

            return gridOptions;
        });

        //authenticationServiceProvider.setRefreshTimeout(30 * 60);

        // TODO: Needed when upgraded to Angular 1.6, ask Chris about this!
        //$qProvider.errorOnUnhandledRejections(false);
    })

    /*.run(['$templateCache', function ($templateCache) {
        $templateCache.put('ui-grid/expandableRowHeader',
            "<div class=\"ui-grid-row-header-cell ui-grid-expandable-buttons-cell\"><div class=\"ui-grid-cell-contents\"><i ng-class=\"{ 'fal fa-plus' : !row.isExpanded, 'fal fa-minus' : row.isExpanded }\" ng-click=\"grid.api.expandable.toggleRowExpansion(row.entity)\"></i></div></div>"
        );
    }])*/
    .run(/*@ngInject*/(
        i18nService,
        translationService: ITranslationService,
        dateHelperService: DateHelperService,
        termPartsLoader,
        uibDatepickerConfig: angular.ui.bootstrap.IDatepickerConfig,
        uibDatepickerPopupConfig: angular.ui.bootstrap.IDatepickerPopupConfig,
        $http: ng.IHttpService,
        $templateCache: ng.ITemplateCacheService,
        urlHelperService: IUrlHelperService) => {

        // Set ui-grid language (use built in translation)
        let lang: string = CoreUtility.language.substr(0, 2);
        if (lang.toLowerCase() === "nb")
            lang = "no";
        i18nService.setCurrentLang(lang);

        termPartsLoader.clearCachedTerms();

        // Set locale based on language
        moment.locale(lang);
        // Set timezone to same as server location
        //moment.tz.setDefault(Constants.DEFAULT_TIMEZONE);
        //moment.tz.setDefault("Europe/Helsinki");

        if (lang === 'en') {
            // ISO-8601, Europe
            moment.updateLocale("en", {
                week: {
                    dow: 1, // First day of week is Monday
                    doy: 4  // First week of year must contain 4 January (7 + 1 - 4)
                }
            });
        }

        const format: string = dateHelperService.getShortDateFormat();
        uibDatepickerConfig['format'] = format;
        uibDatepickerConfig.startingDay = moment.localeData().firstDayOfWeek();
        uibDatepickerPopupConfig.datepickerPopup = format;

        $http.get(urlHelperService.getCoreTemplateUrl("uiGrid/checkBoxHeaderCellTemplate.html")).then(function (response) {
            $templateCache.put('uiGrid/checkBoxHeaderCellTemplate.html', response.data);
        });

        $http.get(urlHelperService.getCoreDirectiveUrl("SoeTypeahead", "soeTypeaheadPopup.html")).then(function (response) {
            $templateCache.put('soeTypeahead/soeTypeaheadPopup.html', response.data);
        });
    });