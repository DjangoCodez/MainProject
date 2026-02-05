import { RightMenuControllerBase, RightMenuType } from "../RightMenuControllerBase";
import { IUrlHelperService } from "../../Services/UrlHelperService";
import { Feature, TermGroup_ReportPrintoutStatus, SoeModule, TermGroup_ReportExportType, SoeReportTemplateType, MatrixDataType, SoeReportType, AnalysisMode, TermGroup_MatrixGroupAggOption, SoeEntityType, UserSettingType, CompanySettingType } from "../../../Util/CommonEnumerations";
import { ReportJobStatusDTO, ReportMenuDTO, ReportMenuModuleDTO, ReportMenuPageDTO, ReportUserSelectionDTO } from "../../../Common/Models/ReportDTOs";
import { IMessagingService } from "../../Services/MessagingService";
import { IReportJobDefinitionDTO } from "../../../Scripts/TypeLite.Net4";
import { SelectionCollection } from "./SelectionCollection";
import { IContextMenuHandler } from "../../Handlers/ContextMenuHandler";
import { IContextMenuHandlerFactory } from "../../Handlers/ContextMenuHandlerFactory";
import { EmbeddedGridController } from "../../Controllers/EmbeddedGridController";
import { ITranslationService } from "../../Services/TranslationService";
import { INotificationService } from "../../Services/NotificationService";
import { ICoreService } from "../../Services/CoreService";
import { IReportService } from "../../Services/reportservice";
import { IReportDataService } from "./ReportDataService";
import { IGridHandlerFactory } from "../../Handlers/gridhandlerfactory";
import { Constants } from "../../../Util/Constants";
import { CoreUtility } from "../../../Util/CoreUtility";
import { GeneralReportSelectionDTO, MatrixColumnsSelectionDTO, TextSelectionDTO } from "../../../Common/Models/ReportDataSelectionDTO";
import { SOEMessageBoxImage, SOEMessageBoxButtons, ReportMenuPages, SoeGridOptionsEvent, SortReports } from "../../../Util/Enumerations";
import { EditController as ReportEditController } from "../../../Common/Reports/ReportGrid/EditController";
import { ExportUtility } from "../../../Util/ExportUtility";
import { MatrixDefinition } from "../../../Common/Models/MatrixResultDTOs";
import { IProgressHandler } from "../../Handlers/ProgressHandler";
import { IProgressHandlerFactory } from "../../Handlers/progresshandlerfactory";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { BatchUpdateController } from "../../../Common/Dialogs/BatchUpdate/BatchUpdateDirective";
import { SettingsUtility } from "../../../Util/SettingsUtility";

export class ReportMenuDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('/Core/RightMenu/ReportMenu/ReportMenu.html'),
            scope: {
                positionIndex: "@",
                feature: "@",
                soeModule: "@",
            },
            restrict: 'E',
            replace: true,
            controller: ReportMenuController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

class ReportMenuController extends RightMenuControllerBase {

    // Init parameters
    private feature: Feature;
    private soeModule: SoeModule;
    private get currentModule(): ReportMenuModuleDTO {
        if (this.selectedPageIsReports) {
            return _.find(this.reportModules, m => m.module == this.soeModule);
        } else if (this.selectedPageIsAnalysis) {
            return _.find(this.analysisModules, m => m.module == this.soeModule);
        } else {
            return null;
        }
    }

    // Terms
    private terms: { [index: string]: string; };
    private title: string;
    private favoriteAddToolTip: string;
    private favoriteRemoveToolTip: string;

    // Permissions
    private billingReportEditPermission = false;
    private economyReportEditPermission = false;
    private timeReportEditPermission = false;

    private billingInsightsPermission = false;
    private economyInsightsPermission = false;
    private timeInsightsPermission = false;

    private get hasInsightsPermission(): boolean {
        return this.billingInsightsPermission || this.economyInsightsPermission || this.timeInsightsPermission;
    }

    private employeeBatchUpdatePermission: boolean;
    private payrollProductBatchUpdatePermission: boolean;

    // Company settings
    private useAccountHierarchy: boolean = false;
    private useAnnualLeave: boolean = false;

    // User settings
    private accountHierarchyId: string;

    // Properties
    private pages: ReportMenuPageDTO[] = [];
    private selectedPage: ReportMenuPages = ReportMenuPages.Favorites;
    private reportModules: ReportMenuModuleDTO[] = [];
    private analysisModules: ReportMenuModuleDTO[] = [];
    private favorites: ReportMenuDTO[] = [];
    private reportJobsStatus: ReportJobStatusDTO[] = [];
    private selectedReport: ReportMenuDTO;
    private selectedReportModule: ReportMenuModuleDTO;
    private selectedReportUserSelection: ReportUserSelectionDTO;
    private selectedExportType: TermGroup_ReportExportType;
    private selections: SelectionCollection;
    private freeTextFilter: string;
    private currentReportPrintoutId: number;
    private analysisMode: AnalysisMode = AnalysisMode.Analysis;
    private hasAnalysisColumns: boolean = false;
    private matrixDefinition: MatrixDefinition;
    private matrixSelection: MatrixColumnsSelectionDTO;
    private analysisJsonRows: any;
    private insightReadOnly: boolean = false;
    private matrixGridHasSelectedRows: boolean = false;
    private matrixGridIsEmployeeBatchUpdatable: boolean = false;
    private matrixGridIsPayrollProductBatchUpdatable: boolean = false;
    private exportFileType: number;
    private invalid: boolean = false;
    private excludedReports: number[] = [];

    private get selectedPageIsFavorites(): boolean {
        return this.selectedPage === ReportMenuPages.Favorites;
    }
    private get selectedPageIsReports(): boolean {
        return this.selectedPage === ReportMenuPages.Reports;
    }
    private get selectedPageIsAnalysis(): boolean {
        return this.selectedPage === ReportMenuPages.Analysis;
    }
    private get selectedPageIsPrinted(): boolean {
        return this.selectedPage === ReportMenuPages.Printed;
    }

    private get printIcon(): string {
        return this.selectedReport && this.selectedReport.isAnalysis ? 'fa-table' : 'fa-print';
    }

    private get showExportTypeSelector(): boolean {
        return !this.showInsightExecuter;
    }

    private get showInsightExecuter(): boolean {
        return this.analysisMode == AnalysisMode.Insights;
    }

    // Flags
    private allModulesExpanded = false;
    private showInactive = false;
    private loadingReport = false;
    private loadingReports = false;
    private loadingQueue = false;
    private openQueue = false;
    private isOverview = false;
    private overviewInitialized = false;
    private isMatrixGrid = false;
    private matrixGridInitialized = false;
    private isInsight = false;

    private createInsightFunctions: any = [];

    // Polling
    private pollRequestedReportsInterval;
    private pollTimeout = 5000;

    // Handlers
    private queueContextMenuHandler: IContextMenuHandler;
    private gridHandler: EmbeddedGridController;
    private matrixGridHandler: EmbeddedGridController;
    private progress: IProgressHandler;

    private sortingReports: any = [];
    private reportsForSorting: ReportMenuModuleDTO;
    private expandedModules: ReportMenuModuleDTO[] = [];
    private allExpandedModulesElementArr: any[] = [];

    private modalInstance: any;

    //@ngInject
    constructor(
        $timeout: ng.ITimeoutService,
        messagingService: IMessagingService,
        private $scope: ng.IScope,
        private $interval: ng.IIntervalService,
        private $q: ng.IQService,
        private $uibModal: ng.ui.bootstrap.IModalService,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private coreService: ICoreService,
        private reportService: IReportService,
        private reportDataService: IReportDataService,
        progressHandlerFactory: IProgressHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private contextMenuHandlerFactory: IContextMenuHandlerFactory) {
        super($timeout, messagingService, RightMenuType.Report);

        this.modalInstance = $uibModal;

        if (progressHandlerFactory)
            this.progress = progressHandlerFactory.create();

        this.gridHandler = new EmbeddedGridController(gridHandlerFactory, "ReportMenu.ReportOverview");
        this.matrixGridHandler = new EmbeddedGridController(gridHandlerFactory, "ReportMenu.MatrixGrid");
    }

    // SETUP

    public $onInit() {
        this.setTopPosition();
        this.messagingService.subscribe(Constants.EVENT_RELOAD_REPORT_SETTINGS, () => {
            this.reloadSettings();
            this.loadReports(this.currentModule);
        });

        this.messagingService.subscribe(Constants.EVENT_TOGGLE_REPORT_MENU, (data: any) => {
            this.toggleShowMenu();
        });

        this.messagingService.subscribe(Constants.EVENT_SHOW_REPORT_MENU, (data: any) => {
            if (!this.showMenu)
                this.toggleShowMenu();

            this.openQueue = !data?.showFavorites;

            this.loadModules().then(() => {
                this.loadReports(null, false, true).then(() => {
                    if (data?.reportPrintoutId)
                        this.downloadPrintedReport(data.reportPrintoutId);
                });
            });
        });

        this.messagingService.subscribe(Constants.EVENT_SHOW_REPORT_MENU_ONLY_DOWNLOAD, (data: any) => {
            if (data?.reportPrintoutId)
                this.downloadPrintedReport(data.reportPrintoutId);
        });

        this.messagingService.subscribe('matrixModeChanged', (data: any) => {
            this.analysisMode = data.mode;
            this.hasAnalysisColumns = data.hasColumns;
            this.insightReadOnly = data.insightReadOnly;
        });

        this.messagingService.subscribe('insightChanged', (data: any) => {
            this.analysisJsonRows = undefined;
        });
        this.messagingService.subscribe(Constants.EVENT_REPORT_VALIDATION_CHANGED, (data: any) => {
            this.invalid = false;
            if (data.invalid) {
                this.invalid = true;
            }            
        });
    }

    private init() {
        this.$q.all([
            this.loadTerms(),
            this.loadModifyPermissions(),
            this.loadCompanySettings()
        ]).then(() => {
            if (this.useAccountHierarchy) {
                this.getAccountHierarchyId().then(x => {
                    this.accountHierarchyId = x;
                });
            }

            if (!this.useAnnualLeave) {
                this.excludedReports.push(SoeReportTemplateType.AnnualLeaveTransactionAnalysis);
            }


            this.setupContextMenu();
            this.setupInsightFunction();
            this.loadModules();
        });
    }

    private shouldExclude(report: ReportMenuDTO) {
        return this.excludedReports.includes(report.sysReportTemplateTypeId)
    }

    private setupContextMenu() {
        this.queueContextMenuHandler = this.contextMenuHandlerFactory.create();
    }

    private getQueueContextMenuOptions(report: ReportJobStatusDTO): any[] {
        return this.createQueueContextMenuOptions(report);
    }

    private createQueueContextMenuOptions(report: ReportJobStatusDTO): any[] {
        let isGeneric = report.sysReportTemplateTypeId === SoeReportTemplateType.Generic;

        this.queueContextMenuHandler.clearContextMenuItems();
        if (report.isAnalysis)
            this.queueContextMenuHandler.addContextMenuItem(this.terms["core.reportmenu.queue.contextmenu.openanalysis"], 'fa-table', ($itemScope, $event, modelValue) => { this.showPrintedReport(report); }, () => { return report.printoutStatus == TermGroup_ReportPrintoutStatus.Delivered; });
        else
            this.queueContextMenuHandler.addContextMenuItem(this.terms["core.reportmenu.queue.contextmenu.openreport"], 'fa-file-alt', ($itemScope, $event, modelValue) => { this.showPrintedReport(report); }, () => { return report.printoutStatus == TermGroup_ReportPrintoutStatus.Delivered; });

        if (!isGeneric) {
            this.queueContextMenuHandler.addContextMenuItem(this.terms["core.reportmenu.queue.contextmenu.openselection"], 'fa-ballot-check iconEdit', ($itemScope, $event, modelValue) => { this.queueContextMenuOpenSelection(report); }, () => { return true; });
            if (CoreUtility.isSupportAdmin && !report.isAnalysis)
                this.queueContextMenuHandler.addContextMenuItem(this.terms["core.reportmenu.queue.contextmenu.openxml"], 'fa-code', ($itemScope, $event, modelValue) => { this.queueContextMenuOpenXML(report); }, () => { return true; });
            this.queueContextMenuHandler.addContextMenuSeparator();
            this.queueContextMenuHandler.addContextMenuItem(this.terms["core.reportmenu.queue.contextmenu.print"], 'fa-print', ($itemScope, $event, modelValue) => { this.queueContextMenuPrint(report); }, () => { return true; });
        }
        this.queueContextMenuHandler.addContextMenuSeparator();
        this.queueContextMenuHandler.addContextMenuItem(this.terms["core.reportmenu.queue.contextmenu.delete"], 'fa-times iconDelete', ($itemScope, $event, modelValue) => { this.deletePrintedReport(report); }, () => { return true; });

        return this.queueContextMenuHandler.getContextMenuOptions();
    }

    private setupInsightFunction() {
        this.createInsightFunctions.push({ id: 1, name: this.terms["core.reportmenu.insights.create"] });
        this.createInsightFunctions.push({ id: 2, name: this.terms["core.reportmenu.insights.create.keepdata"] });
    }

    private setupGrid(): ng.IPromise<any> {
        const keys: string[] = [
            "common.number",
            "common.type",
            "common.name",
            "common.description",
            "common.report.report.selectionname",
            "common.report.report.standard",
            "common.report.report.roles",
            "common.report.report.print",
            "common.report.selection.exporttype",
            "core.delete",
            "core.edit",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.gridHandler.gridAg.addColumnText("reportNr", terms["common.number"], 100);
            this.gridHandler.gridAg.addColumnText("sysReportTypeName", terms["common.type"], null, true);
            this.gridHandler.gridAg.addColumnText("reportName", terms["common.name"], null);
            this.gridHandler.gridAg.addColumnText("reportDescription", terms["common.description"], null, true);
            this.gridHandler.gridAg.addColumnText("reportSelectionText", terms["common.report.report.selectionname"], null, true);
            this.gridHandler.gridAg.addColumnText("standardText", terms["common.report.report.standard"], 60, true, { buttonConfiguration: { iconClass: "fal fa-lock", show: (row) => !!row.isSystemReport, callback: () => undefined } });
            this.gridHandler.gridAg.addColumnText("roleNames", terms["common.report.report.roles"], null, true);
            this.gridHandler.gridAg.addColumnText("exportTypeName", terms["common.report.selection.exporttype"], 100, true);

            let module = this.currentModule;
            if (module && this.hasModuleSettingsPermission(module.module))
                this.gridHandler.gridAg.addColumnDelete(terms["core.delete"], this.deleteReport.bind(this));
        });
    }

    private setupMatrixGrid(def: MatrixDefinition): ng.IPromise<any> {
        this.matrixGridHasSelectedRows = false;

        this.matrixGridHandler.gridAg.options.resetColumnDefs(true);

        this.matrixGridHandler.gridAg.options.addGroupCountAggFunction();
        this.matrixGridHandler.gridAg.options.addGroupAverageAggFunction();
        this.matrixGridHandler.gridAg.options.addGroupMedianAggFunction();
        this.matrixGridHandler.gridAg.options.addGroupTimeSpanSumAggFunction(false);
        this.matrixGridHandler.gridAg.options.addGroupTimeSpanMinAggFunction();
        this.matrixGridHandler.gridAg.options.addGroupTimeSpanMaxAggFunction();
        this.matrixGridHandler.gridAg.options.addGroupTimeSpanAverageAggFunction();
        this.matrixGridHandler.gridAg.options.addGroupTimeSpanMedianAggFunction();

        // TODO: Experimental
        if (CoreUtility.isSupportAdmin) {
            this.matrixGridHandler.gridAg.options.enableContextMenu = true;
            this.matrixGridHandler.gridAg.options.enableCharts = true;
        }

        this.matrixGridIsEmployeeBatchUpdatable = def.matrixDefinitionColumns.filter(c => c.field === 'employeeId').length > 0;
        this.matrixGridIsPayrollProductBatchUpdatable = def.matrixDefinitionColumns.filter(c => c.field === 'payrollProductId').length > 0;

        let groupColList: any[] = [];
        _.forEach(def.matrixDefinitionColumns, colDef => {
            let alignLeft: boolean = colDef.hasOptions && colDef.options.alignLeft;
            let alignRight: boolean = colDef.hasOptions && colDef.options.alignRight;
            let clearZero = colDef.hasOptions && !!colDef.options.clearZero;
            let groupBy: boolean = colDef.hasOptions && !!colDef.options.groupBy;

            let useTimeColumn = colDef.matrixDataType === MatrixDataType.Time && (colDef.options.minutesToTimeSpan || colDef.options.minutesToDecimal);
            let useTimeSpanAgg = colDef.matrixDataType === MatrixDataType.Time && colDef.options?.minutesToTimeSpan;

            let groupByOption: TermGroup_MatrixGroupAggOption = (colDef.hasOptions && colDef.options.groupOption ? colDef.options.groupOption : TermGroup_MatrixGroupAggOption.Sum);
            let aggFunction: string;
            switch (groupByOption) {
                case TermGroup_MatrixGroupAggOption.Sum:
                    aggFunction = useTimeSpanAgg ? 'sumTimeSpan' : 'sum';
                    break;
                case TermGroup_MatrixGroupAggOption.Min:
                    aggFunction = useTimeSpanAgg ? 'minTimeSpan' : 'min';
                    break;
                case TermGroup_MatrixGroupAggOption.Max:
                    aggFunction = useTimeSpanAgg ? 'maxTimeSpan' : 'max';
                    break;
                case TermGroup_MatrixGroupAggOption.Count:
                    aggFunction = 'soeCount';
                    break;
                case TermGroup_MatrixGroupAggOption.Average:
                    aggFunction = useTimeSpanAgg ? 'avgTimeSpan' : 'soeAvg';
                    break;
                case TermGroup_MatrixGroupAggOption.Median:
                    aggFunction = useTimeSpanAgg ? 'medianTimeSpan' : 'median';
                    break;
                case TermGroup_MatrixGroupAggOption.None:
                    break;
            }

            switch (colDef.matrixDataType) {
                case (MatrixDataType.Boolean):
                    let colBool = this.matrixGridHandler.gridAg.addColumnBoolEx(colDef.field, colDef.title, null, { enableRowGrouping: true });
                    if (groupBy)
                        groupColList.push(colBool);
                    break;
                case (MatrixDataType.Date):
                    let dateFormat: string = colDef.hasOptions ? CalendarUtility.getDateFormatForMatrix(colDef.options.dateFormatOption) : '';
                    let colDate = this.matrixGridHandler.gridAg.addColumnDate(colDef.field, colDef.title, null, true, null, { alignRight: alignRight, dateFormat: dateFormat, enableRowGrouping: true });
                    if (groupBy)
                        groupColList.push(colDate);
                    break;
                case (MatrixDataType.DateAndTime):
                    let colDateTime = this.matrixGridHandler.gridAg.addColumnDateTime(colDef.field, colDef.title, null, true, null, { alignRight: alignRight, enableRowGrouping: true });
                    if (groupBy)
                        groupColList.push(colDateTime);
                    break;
                case (MatrixDataType.Decimal):
                    let colDec = this.matrixGridHandler.gridAg.addColumnNumber(colDef.field, colDef.title, null, { alignLeft: alignLeft, decimals: colDef.hasOptions ? colDef.options.decimals : 2, clearZero: clearZero, aggFuncOnGrouping: aggFunction, enableRowGrouping: true });
                    if (groupBy)
                        groupColList.push(colDec);
                    break;
                case (MatrixDataType.Integer):
                    let colInt = this.matrixGridHandler.gridAg.addColumnNumber(colDef.field, colDef.title, null, { hide: colDef.options ? colDef.options.hidden : false, alignLeft: alignLeft, decimals: 0, clearZero: clearZero, aggFuncOnGrouping: aggFunction, enableRowGrouping: true });
                    if (groupBy)
                        groupColList.push(colInt);
                    break;
                case (MatrixDataType.String):
                    let colStr = this.matrixGridHandler.gridAg.addColumnText(colDef.field, colDef.title, null, true, { alignRight: alignRight, enableRowGrouping: true });
                    if (groupBy)
                        groupColList.push(colStr);
                    break;
                case (MatrixDataType.Time):
                    let colTime;
                    if (useTimeColumn)
                        colTime = this.matrixGridHandler.gridAg.addColumnTime(colDef.field, colDef.title, null, { alignLeft: alignLeft, minutesToTimeSpan: colDef.options?.minutesToTimeSpan, minutesToDecimal: colDef.options?.minutesToDecimal, clearZero: clearZero, aggFuncOnGrouping: (colDef.options?.minutesToDecimal || colDef.options?.minutesToTimeSpan) ? aggFunction : null, showGroupedAsNumber: groupByOption === TermGroup_MatrixGroupAggOption.Count, enableRowGrouping: true, formatTimeWithSeconds: colDef.options?.formatTimeWithSeconds, formatTimeWithDays: colDef.options?.formatTimeWithDays });
                    else
                        colTime = this.matrixGridHandler.gridAg.addColumnNumber(colDef.field, colDef.title, null, { alignLeft: alignLeft, decimals: 0, clearZero: clearZero, aggFuncOnGrouping: aggFunction, enableRowGrouping: true });
                    if (groupBy)
                        groupColList.push(colTime);
                    break;
            }
        });

        if (groupColList.length > 0) {
            _.forEach(groupColList, col => {
                this.matrixGridHandler.gridAg.options.groupRowsByColumn(col, true);
            });
        }

        return this.$timeout(() => {
            if (!this.matrixGridInitialized) {
                this.matrixGridHandler.gridAg.options.ignoreResizeToFit = true;
                this.matrixGridHandler.gridAg.options.enableGridMenu = true;

                this.matrixGridHandler.gridAg.options.useGrouping(true, true, { keepColumnsAfterGroup: false, selectChildren: false });
                this.matrixGridHandler.gridAg.options.groupHideOpenParents = false;

                let events: GridEvent[] = [];
                events.push(new GridEvent(SoeGridOptionsEvent.ColumnRowGroupChanged, (params) => { this.columnRowGroupChanged(params); }));
                events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (params) => { this.matrixGridSelectionChanged(params); }));
                events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (params) => { this.matrixGridSelectionChanged(params); }));
                this.matrixGridHandler.gridAg.options.subscribe(events);

                this.matrixGridHandler.gridAg.finalizeInitGrid("core.export", true, "matrix-totals-grid");
                this.matrixGridInitialized = true;
            } else {
                // resetColumnDefs will also remove the grid menu, need to add it again here
                this.matrixGridHandler.gridAg.options.finalizeInitGrid(false);
            }
        });
    }

    private columnRowGroupChanged(params) {
        let groupedFields: string[] = [];
        _.forEach(params.columns, groupedCol => {
            groupedFields.push(groupedCol.colDef.field);
        });
        _.forEach(this.matrixGridHandler.gridAg.options.getColumnDefs(), colDef => {
            if (!_.includes(groupedFields, colDef.field)) {
                colDef.showRowGroup = false;
                colDef.rowGroup = false;
                colDef.hide = false;
                colDef.cellRenderer = (prms: { valueFormatted: any, eGridCell: HTMLElement, data: any }) => {
                    return prms.valueFormatted;
                }
            }
        });
    }

    private matrixGridSelectionChanged(params) {
        this.$scope.$applyAsync(() => {
            this.matrixGridHasSelectedRows = this.matrixGridHandler.gridAg.options.getSelectedCount() > 0;
        });
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "core.reportmenu.analysis",
            "core.reportmenu.favorites",
            "core.reportmenu.favorites.add",
            "core.reportmenu.favorites.remove",
            "core.reportmenu.favorites.rename",
            "core.reportmenu.insights",
            "core.reportmenu.insights.create",
            "core.reportmenu.insights.create.keepdata",
            "core.reportmenu.insights.creating",
            "core.reportmenu.printed",
            "core.reportmenu.printed.error",
            "core.reportmenu.queue.contextmenu.delete",
            "core.reportmenu.queue.contextmenu.openanalysis",
            "core.reportmenu.queue.contextmenu.openreport",
            "core.reportmenu.queue.contextmenu.openselection",
            "core.reportmenu.queue.contextmenu.openxml",
            "core.reportmenu.queue.contextmenu.print",
            "core.reportmenu.reports",
            "core.reportmenu.selection.insight.readonly",
            "core.reportmenu.selection.insight.readonly.message",
            "core.reportmenu.selection.new",
            "core.reportmenu.selection.save",
            "core.reportmenu.selection.save.error",
            "core.reportmenu.title",
            "core.warning",
            "common.name",
            "common.start.module.billing",
            "common.start.module.economy",
            "common.start.module.time",
            "common.number",
            "common.standard",
            "common.notstandard"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
            this.setupSortings(this.terms);
            this.title = this.terms["core.reportmenu.title"];
            this.favoriteAddToolTip = this.terms["core.reportmenu.favorites.add"];
            this.favoriteRemoveToolTip = this.terms["core.reportmenu.favorites.remove"];
        });
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        const features: number[] = [
            Feature.Billing_Distribution_Reports_Edit,
            Feature.Economy_Distribution_Reports_Edit,
            Feature.Time_Distribution_Reports_Edit,
            Feature.Time_Employee_MassUpdateEmployeeFields,
            Feature.Time_Preferences_SalarySettings_PayrollProduct_MassUpdate
        ];

        return this.coreService.hasModifyPermissions(features).then((x) => {
            this.billingReportEditPermission = x[Feature.Billing_Distribution_Reports_Edit];
            this.economyReportEditPermission = x[Feature.Economy_Distribution_Reports_Edit];
            this.timeReportEditPermission = x[Feature.Time_Distribution_Reports_Edit];
            this.employeeBatchUpdatePermission = x[Feature.Time_Employee_MassUpdateEmployeeFields];
            this.payrollProductBatchUpdatePermission = x[Feature.Time_Preferences_SalarySettings_PayrollProduct_MassUpdate];
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        let settingTypes: number[] = [
            CompanySettingType.UseAccountHierarchy,
            CompanySettingType.UseAnnualLeave
        ];

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useAccountHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);
            this.useAnnualLeave = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAnnualLeave);
        });
    }

    private getAccountHierarchyId(): ng.IPromise<string> {
        let deferral = this.$q.defer<string>();

        this.coreService.getUserAndCompanySettings([UserSettingType.AccountHierarchyId]).then(x => {
            deferral.resolve(SettingsUtility.getStringUserSetting(x, UserSettingType.AccountHierarchyId, '0'));
        });

        return deferral.promise;
    }

    private loadModules(): ng.IPromise<any> {
        const featureIds: number[] = [
            Feature.Billing_Distribution_Reports,
            Feature.Billing_Analysis,
            Feature.Billing_Insights,
            Feature.Economy_Distribution_Reports,
            Feature.Economy_Analysis,
            Feature.Economy_Insights,
            Feature.Time_Distribution_Reports,
            Feature.Time_Analysis,
            Feature.Time_Insights
        ];

        return this.coreService.hasReadOnlyPermissions(featureIds).then(x => {
            this.billingInsightsPermission = x[Feature.Billing_Insights];
            this.economyInsightsPermission = x[Feature.Economy_Insights];
            this.timeInsightsPermission = x[Feature.Time_Insights];

            this.pages = [];

            // Favorites
            this.pages.push(new ReportMenuPageDTO(ReportMenuPages.Favorites, this.terms["core.reportmenu.favorites"], this.terms["core.reportmenu.favorites"], "mediumFont fal fa-star", true));
            // Reports
            if (x[Feature.Billing_Distribution_Reports] || x[Feature.Economy_Distribution_Reports] || x[Feature.Time_Distribution_Reports])
                this.pages.push(new ReportMenuPageDTO(ReportMenuPages.Reports, this.terms["core.reportmenu.reports"], this.terms["core.reportmenu.reports"], "mediumFont fal fa-print"));
            // Analysis/Insights
            if (x[Feature.Billing_Analysis] || x[Feature.Economy_Analysis] || x[Feature.Time_Analysis]) {
                let analysisTooltip = this.terms["core.reportmenu.analysis"];
                let analysisIcon = "mediumFont fal ";
                if (this.hasInsightsPermission) {
                    analysisTooltip += " + " + this.terms["core.reportmenu.insights"].toLocaleLowerCase();
                    analysisIcon += "fa-analytics";
                } else {
                    analysisIcon += "fa-table";
                }
                this.pages.push(new ReportMenuPageDTO(ReportMenuPages.Analysis, this.terms["core.reportmenu.analysis"], analysisTooltip, analysisIcon));
            }
            // Queue
            this.pages.push(new ReportMenuPageDTO(ReportMenuPages.Printed, this.terms["core.reportmenu.printed"], this.terms["core.reportmenu.printed"], "mediumFont fal fa-layer-group"));

            this.reportModules = [];

            if (x[Feature.Billing_Distribution_Reports])
                this.reportModules.push(new ReportMenuModuleDTO(SoeModule.Billing, this.terms["common.start.module.billing"], '/img/modules/billing-white.png'));

            if (x[Feature.Economy_Distribution_Reports])
                this.reportModules.push(new ReportMenuModuleDTO(SoeModule.Economy, this.terms["common.start.module.economy"], '/img/modules/economy-white.png'));

            if (x[Feature.Time_Distribution_Reports])
                this.reportModules.push(new ReportMenuModuleDTO(SoeModule.Time, this.terms["common.start.module.time"], '/img/modules/time-white.png'));

            this.analysisModules = [];
            if (x[Feature.Billing_Analysis]) {
                let mod = new ReportMenuModuleDTO(SoeModule.Billing, this.terms["common.start.module.billing"], '/img/modules/billing-white.png');
                mod.hasInsightsPermission = this.billingInsightsPermission;
                this.analysisModules.push(mod);
            }
            if (x[Feature.Economy_Analysis]) {
                let mod = new ReportMenuModuleDTO(SoeModule.Economy, this.terms["common.start.module.economy"], '/img/modules/economy-white.png');
                mod.hasInsightsPermission = this.economyInsightsPermission;
                this.analysisModules.push(mod);
            }
            if (x[Feature.Time_Analysis]) {
                let mod = new ReportMenuModuleDTO(SoeModule.Time, this.terms["common.start.module.time"], '/img/modules/time-white.png');
                mod.hasInsightsPermission = this.timeInsightsPermission;
                this.analysisModules.push(mod);
            }

            if (this.openQueue)
                this.selectPage(this.getPage(ReportMenuPages.Printed), false);
            else
                this.selectPage(this.getPage(ReportMenuPages.Favorites), true);
        });
    }

    private loadReports(module: ReportMenuModuleDTO, showProgress = true, forceLoad = false): ng.IPromise<any> {
        const deferral = this.$q.defer();
        if (this.selectedPageIsFavorites) {
            this.cancelPollRequestedReports();

            if (!forceLoad && this.favorites.length > 0) {
                deferral.resolve();
                return;
            }

            this.loadFavorites(showProgress);
            deferral.resolve();
        } else if (this.selectedPageIsPrinted) {
            if (showProgress)
                this.loadingQueue = true;

            let reportPrintoutIds: number[] = this.requestedReportIds;

            if (forceLoad)
                reportPrintoutIds = [];

            this.reportService.getReportJobsStatus(reportPrintoutIds, CoreUtility.isSupportAdmin || CoreUtility.isSupportSuperAdmin).then(x => {
                // If polling requested printout, just replace the ones being polled, otherwise replace whole collection
                if (reportPrintoutIds.length > 0) {
                    _.forEach(x, job => {
                        const printout = _.find(this.reportJobsStatus, s => s.reportPrintoutId === job.reportPrintoutId);
                        if (printout)
                            angular.extend(printout, job);
                    });
                } else {
                    this.reportJobsStatus = x;
                }

                // Check if there are any reports processing and start polling
                if (this.hasRequestedReport)
                    this.pollRequestedReports();

                this.loadingQueue = false;
                deferral.resolve();
            });
        } else if (module) {
            this.cancelPollRequestedReports();

            if (!forceLoad && module.reportsLoaded) {
                deferral.resolve();
                return;
            }

            module.reports = [];

            if (showProgress) {
                this.loadingReports = true;
                module.loadingReports = true;
            }

            this.reportsForSorting = module;

            this.reportService.getReportsForMenu(module.module, this.selectedPageIsAnalysis ? SoeReportType.Analysis : SoeReportType.CrystalReport).then(x => {
                let prevGroup: string = null;
                _.forEach(x, report => {
                    // Don't show reports in list that user shouldn't see
                    if (this.shouldExclude(report)) {
                        return;
                    }
                    report.standardText = report.isStandard ?
                        this.terms["common.standard"] :
                        this.terms["common.notstandard"];

                    if (!prevGroup || prevGroup !== report.groupName) {
                        this.addGroup(module.reports, report.groupName);
                        prevGroup = report.groupName;
                    }

                    module.reports.push(report);
                });

                module.hasNoRolesSpecified = _.filter(module.reports, r => r.noRolesSpecified).length > 0;

                this.loadingReports = false;
                module.loadingReports = false;
                module.reportsLoaded = true;
                module.expanded = true;
                deferral.resolve();
            });
        } else {
            deferral.resolve();
            return;
        }

        return deferral.promise;
    }

    private loadFavorites = _.debounce((showProgress: boolean) => {
        this.favorites = [];

        if (showProgress)
            this.loadingReports = true;

        this.reportService.getReportsForMenu(0, 0).then((x: ReportMenuDTO[]) => {
            let prevGroup: string = null;
            _.forEach(x, report => {
                if (!prevGroup || prevGroup !== report.groupName) {
                    this.addGroup(this.favorites, report.groupName);
                    prevGroup = report.groupName;
                }
                report.standardText = report.isStandard ?
                    this.terms["common.standard"] :
                    this.terms["common.notstandard"];

                this.favorites.push(report);
            });

            this.loadingReports = false;
        });
    }, 200, { leading: true, trailing: false });

    private loadReportsOverview(module: ReportMenuModuleDTO): ng.IPromise<any> {
        this.loadingReports = true;

        return this.reportService.getReportViewsForModule(module.module, true, false).then((data => {
            this.loadingReports = false;
            data.forEach(r => {
                r.standardText = r.standard ?
                    this.terms["common.standard"] :
                    this.terms["common.notstandard"];
            });

            // Grid not in DOM until reports are loaded.
            // Therefore we need to initialize it here.
            this.$timeout(() => {
                if (!this.overviewInitialized) {
                    this.gridHandler.gridAg.options.enableRowSelection = false;
                    this.gridHandler.gridAg.finalizeInitGrid("common.report.report.reports", true, "report-totals-grid");
                    this.overviewInitialized = true;
                }
                this.gridHandler.gridAg.setData(data);
            });
        }));
    }

    private loadReportUserSelectionFromReportPrintout(reportPrintoutId: number): ng.IPromise<any> {
        return this.reportService.getReportSelectionFromReportPrintout(reportPrintoutId).then(x => {
            this.selectedReportUserSelection = x;
        });
    }

    // EVENTS

    protected toggleShowMenu() {
        super.toggleShowMenu();

        if (this.showMenu && !this.terms)
            this.init();
        else {
            this.cancelPollRequestedReports();

            if (this.isOverview)
                this.toggleShowOverview(null);
            else if (this.isMatrixGrid)
                this.toggleShowMatrixGrid();
            else if (this.isInsight)
                this.toggleShowInsight();
        }
    }

    protected toggleFullscreen(): ng.IPromise<any> {
        return super.toggleFullscreen().then(() => {
            this.setReportSelectionHeight();

            if (!this.fullscreen) {
                this.isOverview = false;
                this.isMatrixGrid = false;
                this.isInsight = false;
            }
        });
    }

    private toggleShowOverview(module: ReportMenuModuleDTO) {
        this.toggleFullscreen();

        this.isOverview = this.fullscreen;
        if (this.isOverview) {
            if (this.gridHandler.gridAg.options.getColumnDefs().length === 0) {
                this.setupGrid().then(() => {
                    this.loadReportsOverview(module);
                });
            } else {
                this.loadReportsOverview(module);
            }
        }
    }

    private toggleShowMatrixGrid() {
        this.toggleFullscreen();
        this.isMatrixGrid = this.fullscreen;
    }

    private setupSortings(terms: { [index: string]: string }) {

        this.sortingReports.push({ id: SortReports.Name, name: terms["common.name"], icon: "fal fa-sort-alpha-down" });
        this.sortingReports.push({ id: SortReports.Number, name: terms["common.number"], icon: "fal fa-sort-numeric-down" });
    }

    private executeSorting(option) {
        _.forEach(this.expandedModules, expandedModule => {
            this.loadSortedReports(expandedModule, expandedModule.reports, option.id, false, true);
        });
    }

    private loadSortedReports(module: ReportMenuModuleDTO, allExpandedModulesElementArrObj: any[], sortingOption: number, showProgress = true, forceLoad = false): ng.IPromise<any> {

        const deferral = this.$q.defer();
        if (module) {
            this.cancelPollRequestedReports();

            if (!forceLoad && module.reportsLoaded) {
                deferral.resolve();
                return;
            }

            let prevGroup: string = null;
            var isGroupList = _.filter(allExpandedModulesElementArrObj, r => r.isGroup);

            module.reports = [];

            if (showProgress) {
                this.loadingReports = true;
                module.loadingReports = true;
            }

            _.forEach(isGroupList, reportGroup => {

                var tempReports: ReportMenuDTO[] = [];
                var tempChildReports: ReportMenuDTO[] = [];
                _.forEach(allExpandedModulesElementArrObj, report => {

                    if (report.groupName == reportGroup.name && !report.isGroup) {
                        tempChildReports.push(report);
                    }
                });

                switch (sortingOption) {
                    case SortReports.Name:
                        tempReports = _.sortBy(tempChildReports, (c) => { return c.name; });
                        break;
                    case SortReports.Number:
                        tempReports = _.sortBy(tempChildReports, (c) => { return c.reportNr; });
                        break;
                }

                module.reports.push(reportGroup);
                _.forEach(tempReports, report => {
                    module.reports.push(report);
                });

            });

            module.hasNoRolesSpecified = _.filter(module.reports, r => r.noRolesSpecified).length > 0;
            this.loadingReports = false;
            module.loadingReports = false;
            module.reportsLoaded = true;
            module.expanded = true;
            deferral.resolve();
        } else {
            deferral.resolve();
            return;
        }

        return deferral.promise;
    }

    private toggleShowInsight() {
        this.toggleFullscreen();
        this.isInsight = this.fullscreen;
    }

    private toggleExpandModule(module: ReportMenuModuleDTO) {
        module.expanded = !module.expanded;

        if (module.expanded) {
            this.loadReports(module);
            this.expandedModules.push(module);
        }
        else {
            var tempArr: ReportMenuModuleDTO[] = [];
            _.forEach(this.expandedModules, p => {
                if (p.module != module.module)
                    tempArr.push(p);
            });

            this.expandedModules = tempArr;
        }
    }

    private selectPage(page: ReportMenuPageDTO, loadReports: boolean = true) {
        _.forEach(this.pages, p => {
            p.selected = false;
        });
        page.selected = true;
        this.selectedPage = page.page;

        if (this.selectedReport)
            this.deselectReport();

        if (this.selectedPage === ReportMenuPages.Favorites || this.selectedPage === ReportMenuPages.Printed) {
            if (this.isOverview)
                this.toggleShowOverview(null);
            else if (this.isMatrixGrid)
                this.toggleShowMatrixGrid();
            else if (this.isInsight)
                this.toggleShowInsight();

            if (loadReports)
                this.loadReports(this.currentModule);
        } else {
            let module = this.currentModule;
            if (module && !module.expanded)
                this.toggleExpandModule(module);

            if (this.isOverview && loadReports)
                this.loadReportsOverview(this.currentModule);
        }
    }

    private reloadSettings() {
        if (this.selectedReport) {
            this.reportService.getReportItem(this.selectedReport.reportId, this.selectedReport.isAnalysis ? SoeReportType.Analysis : SoeReportType.CrystalReport).then((reportItem) => {
                this.selectedReport.reportItem = reportItem;
            });
        }
    }

    private selectReport(report: ReportMenuDTO): ng.IPromise<boolean> {
        const deferral = this.$q.defer<boolean>();

        if (report.isGroup || report.noPrintPermission) {
            deferral.resolve(false);
            return;
        }

        if (!report.reportId) {
            this.openReportSettings(report);
            deferral.resolve(false);
            return;
        }

        // Clear selected report and selections
        this.deselectReport();

        this.loadingReport = true;

        if (this.isOverview)
            this.isOverview = false;
        else if (this.isMatrixGrid)
            this.isMatrixGrid = false;
        else if (this.isInsight)
            this.isInsight = false;

        this.analysisMode = AnalysisMode.Analysis;

        this.reportService.getReportItem(report.reportId, report.isAnalysis ? SoeReportType.Analysis : SoeReportType.CrystalReport).then((reportItem) => {
            if (!this.fullscreen)
                this.toggleFullscreen();
            this.selectedReport = report;
            this.selectedReport.reportItem = reportItem;
            this.selectedReportUserSelection = null;
            this.exportFileType = reportItem.exportFileType;

            this.setDefaultExportType();

            this.loadingReport = false;
            deferral.resolve(true);
        });

        return deferral.promise;
    }

    private deselectReport() {
        if (this.fullscreen)
            this.toggleFullscreen();

        this.selectedReport = null;
        this.selectedReportModule = this.currentModule;
        this.selections = new SelectionCollection();
    }

    private reportUserSelectionChanged(reportUserSelection: ReportUserSelectionDTO) {
        this.selectedReportUserSelection = reportUserSelection;
        this.setGeneralReportSelections();
    }

    private queueContextMenuOpenSelection(report: ReportJobStatusDTO) {
        if (report.sysReportTemplateTypeId === SoeReportTemplateType.Generic)
            return;

        // Remember export type from menu, needs to be re-set after selectReport below.
        const exportType = report.exportType;
        this.reportService.getPrintedReportForMenu(report.reportPrintoutId).then(x => {
            this.selectReport(x).then(() => {
                this.selectedExportType = exportType;
                this.loadReportUserSelectionFromReportPrintout(report.reportPrintoutId);
            });
        });
    }

    private openAnalysisSelection() {
        if (this.currentReportPrintoutId) {
            this.reportService.getPrintedReportForMenu(this.currentReportPrintoutId).then(x => {
                this.selectReport(x).then(() => {
                    this.loadReportUserSelectionFromReportPrintout(this.currentReportPrintoutId);
                });
            });
        } else {
            // Opened from insight
            this.isMatrixGrid = false;
        }
    }

    private openInsightsSelection() {
        this.isInsight = false;
        this.selectedReportUserSelection = new ReportUserSelectionDTO();
        this.selectedReportUserSelection.selections = [];
        this.selectedReportUserSelection.selections.push(this.matrixSelection);
    }

    private openEmployeeBatchUpdate() {
        if (!this.employeeBatchUpdatePermission || !this.matrixGridHasSelectedRows)
            return;

        this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/BatchUpdate/Views/BatchUpdate.html"),
            controller: BatchUpdateController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'lg',
            resolve: {
                entityType: () => { return SoeEntityType.Employee },
                selectedIds: () => { return _.map(this.matrixGridHandler.gridAg.options.getSelectedRows(), 'employeeId') }
            }
        });
    }

    private openPayrollProductBatchUpdate() {
        if (!this.payrollProductBatchUpdatePermission || !this.matrixGridHasSelectedRows)
            return;

        this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/BatchUpdate/Views/BatchUpdate.html"),
            controller: BatchUpdateController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'lg',
            resolve: {
                entityType: () => { return SoeEntityType.PayrollProduct },
                selectedIds: () => { return _.map(this.matrixGridHandler.gridAg.options.getSelectedRows(), 'payrollProductId') }
            }
        });
    }
    private queueContextMenuOpenXML(report: ReportJobStatusDTO) {
        this.reportService.getPrintedXmlForMenu(report.reportPrintoutId).then(x => {
            ExportUtility.Export(x, report.name + '.xml');
        });
    }

    private queueContextMenuPrint(report: ReportJobStatusDTO) {
        this.reCreatePrintJob(report.reportPrintoutId);
    }

    private initCreatePrintJob(forceValidation: boolean = false) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_REPORT_SELECTION_TEXT, new TextSelectionDTO(this.selectedReportUserSelection ? this.selectedReportUserSelection.name : ""))

        if (this.useAccountHierarchy) {
            this.getAccountHierarchyId().then(x => {
                this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_ACCOUNT_HIERARCHY_ID, new TextSelectionDTO(x));

                // Check if user selected account hierarchy has been changed since page open
                if (this.accountHierarchyId !== x) {
                    let keys: string[] = [
                        "core.reportmenu.accounthierarchymismatch.title",
                        "core.reportmenu.accounthierarchymismatch.message"
                    ];
                    this.translationService.translateMany(keys).then(terms => {
                        this.notificationService.showDialogEx(terms["core.reportmenu.accounthierarchymismatch.title"], terms["core.reportmenu.accounthierarchymismatch.message"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel).result.then(val => {
                            if (val)
                                this.createPrintJob(forceValidation);
                        });
                    });
                } else {
                    this.createPrintJob(forceValidation);
                }
            });
        } else {
            this.createPrintJob(forceValidation);
        }
    }

    private createPrintJob(forceValidation: boolean = false) {

        const printJobDefinition = <IReportJobDefinitionDTO>{
            selections: this.selections.materialize(),
            reportId: this.selectedReport.reportId,
            sysReportTemplateTypeId: this.selectedReport.sysReportTemplateTypeId,
            exportType: this.selectedExportType,
            forceValidation: forceValidation,
        };

        this.reportDataService.createReportJob(printJobDefinition).then(result => {
            if (result) {
                if (result.resultMessage === TermGroup_ReportPrintoutStatus.Error) {
                    this.notificationService.showDialogEx(this.terms["core.error"], result.resultMessageDetails, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                } else if (result.resultMessage === TermGroup_ReportPrintoutStatus.Warning && !forceValidation) {
                    const modal = this.notificationService.showDialogEx(this.terms["core.warning"], result.resultMessageDetails, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                    modal.result.then(modalResult => {
                        if (modalResult)
                            this.createPrintJob(true);
                    });
                } else {
                    this.createPrintJobComplete();
                }
            }
        });
    }

    private createPrintJobComplete() {
        // Hide selection and show queue
        this.closeSelection();
        this.selectPage(this.getPage(ReportMenuPages.Printed), false);
        this.loadReports(null, false, true);
        this.$timeout(function () {
            // Trigger resize event (needed in for example in schedule planning to repaint)
            window.dispatchEvent(new Event('resize'));
        })
    }

    private reCreatePrintJob(reportPrintoutId: number, forceValidation: boolean = false) {
        if (!this.selectedPageIsPrinted)
            return;

        this.reportDataService.reCreateReportJob(reportPrintoutId, false, forceValidation).then(result => {
            if (result) {
                if (!result.success && result.canUserOverride && !forceValidation) {
                    const modal = this.notificationService.showDialogEx(this.terms["core.warning"], result.errorMessage, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                    modal.result.then(modalResult => {
                        if (modalResult)
                            this.reCreatePrintJob(reportPrintoutId, true);
                    });
                }
                else if (result.success) {
                    this.createPrintJobComplete();
                }
            }
        });
    }

    private createInsight(option) {
        switch (option.id) {
            case 1:
                this.createInsightLoadData();
                break;
            case 2:
                this.createInsightKeepData();
                break;
        }
    }

    private createInsightLoadData() {
        if (this.insightReadOnly) {
            this.notificationService.showDialogEx(this.terms["core.reportmenu.selection.insight.readonly"], this.terms["core.reportmenu.selection.insight.readonly.message"], SOEMessageBoxImage.Forbidden);
            return;
        }

        this.progress.startWorkProgress((completion) => {
            const printJobDefinition = <IReportJobDefinitionDTO>{
                selections: this.selections.materialize(),
                reportId: this.selectedReport.reportId,
                sysReportTemplateTypeId: this.selectedReport.sysReportTemplateTypeId,
                exportType: TermGroup_ReportExportType.Insight
            };

            this.matrixSelection = <MatrixColumnsSelectionDTO>printJobDefinition.selections.find(s => s.typeName === 'MatrixColumnsSelectionDTO');

            this.reportDataService.createInsight(printJobDefinition).then(x => {
                if (x.json) {
                    let result = JSON.parse(x.json);
                    this.analysisJsonRows = result.jsonRows;

                    this.matrixDefinition = new MatrixDefinition();
                    angular.extend(this.matrixDefinition, result.matrixDefinition);
                    this.matrixDefinition.setTypes();

                    this.isInsight = true;
                    if (!this.fullscreen)
                        this.toggleFullscreen();
                }
                completion.completed(null, true);
            }, error => {
                completion.failed(error.message);
            });
        }, null, this.terms["core.reportmenu.insights.creating"]);
    }

    private createInsightKeepData() {
        if (!this.analysisJsonRows) {
            this.createInsightLoadData();
            return;
        }

        this.isInsight = true;
        if (!this.fullscreen)
            this.toggleFullscreen();

        this.matrixSelection = <MatrixColumnsSelectionDTO>this.selections.materialize().find(s => s.typeName === 'MatrixColumnsSelectionDTO');

        this.$timeout(() => {
            this.$scope.$broadcast('refreshChart');
        });
    }

    private toggleFavorite(report: ReportMenuDTO) {
        report.isFavorite = !report.isFavorite;

        if (report.isFavorite) {
            this.reportDataService.saveFavorite(report.reportId).then(result => {
                this.loadFavorites(true);
            });
        }
        else {
            this.reportDataService.deleteFavorite(report.reportId).then(result => {
                this.loadFavorites(true);
            });
        }
    }

    private renameFavorite(report: ReportMenuDTO) {
        const modal = this.notificationService.showDialogEx(this.terms["core.reportmenu.favorites.rename"], "", SOEMessageBoxImage.None, SOEMessageBoxButtons.OKCancel, { showTextBox: true, textBoxLabel: this.terms["common.name"], textBoxValue: report.name });
        modal.result.then(val => {
            modal.result.then(result => {
                if (result.result && result.textBoxValue) {
                    this.reportDataService.renameFavorite(report.reportId, result.textBoxValue).then(res => {
                        if (res && res.success)
                            report.name = result.textBoxValue;
                    });
                }
            });
        });
    }

    private setReportSelectionHeight() {
        this.$timeout(() => {
            let minHeight = document.documentElement.clientHeight - 85;
            let newHeight = minHeight;

            let panelBody = $('.report-selection');
            if (panelBody)
                newHeight = _.max([panelBody.height() + 200, minHeight]);

            let reportMenu = $('#reportMenu');
            if (reportMenu)
                reportMenu.css('height', newHeight);

            let reportMenuPanel = $('#reportMenuPanel');
            if (reportMenuPanel)
                reportMenuPanel.css('height', newHeight);

            let reportSelectionPanel = $('#reportSelectionPanel');
            if (reportSelectionPanel)
                reportSelectionPanel.css('height', newHeight - 100);
        }, 800);
    }

    private setReportGridHeight() {
        this.$timeout(() => {
            var reportMenu = document.getElementById('report-menu-overview-container');
            var reportGrid = document.getElementById('report-grid');
            if (reportMenu && reportGrid) {
                let newHeight = $(reportMenu).height() + 3 - 25;
                if (newHeight < 500)
                    newHeight = 500;
                $(reportGrid).css('height', newHeight);
            }
        }, 200);
    }

    private setMatrixGridHeight() {
        this.$timeout(() => {
            var reportMenu = document.getElementById('report-menu-overview-container');
            var reportGrid = document.getElementById('matrix-grid');
            if (reportMenu && reportGrid) {
                let newHeight = $(reportMenu).height() + 3 - 25;
                if (newHeight < 500)
                    newHeight = 500;
                let winHeight = $(window).height() - 138;
                if (newHeight > winHeight)
                    newHeight = winHeight;
                $(reportGrid).css('height', newHeight);
            }
        }, 200);
    }

    private showPrintedReport(report: ReportJobStatusDTO) {
        this.currentReportPrintoutId = report.reportPrintoutId;
        if (report.exportType == TermGroup_ReportExportType.MatrixGrid) {
            this.viewCreatedMatrixLoadData(report.reportPrintoutId);
        } else {
            this.downloadPrintedReport(report.reportPrintoutId);
        }
    }

    private viewCreatedMatrixLoadData(reportPrintoutId) {
        this.reportDataService.getMatrixGridResult(reportPrintoutId).then(x => {
            if (x.json && x.selection) {
                let result = JSON.parse(x.json);
                this.analysisJsonRows = result.jsonRows;

                this.matrixDefinition = new MatrixDefinition();
                angular.extend(this.matrixDefinition, result.matrixDefinition);
                this.matrixDefinition.setTypes();

                this.viewCreatedMatrixKeepData();
            }
        });
    }

    private viewCreatedMatrixKeepData() {
        if (!this.analysisJsonRows) {
            return;
        }

        if (!this.isMatrixGrid) {
            if (!this.fullscreen)
                this.toggleShowMatrixGrid();
            else
                this.isMatrixGrid = true;
        }

        this.$timeout(() => {
            this.setupMatrixGrid(this.matrixDefinition).then(() => {
                this.matrixGridHandler.gridAg.setData(this.analysisJsonRows);
            });
        }, 200);
    }

    private downloadPrintedReport(reportPrintoutId) {
        const reportUrl: string = `/ajax/downloadReport.aspx?templatetype=${SoeReportTemplateType.Unknown}&reportprintoutid=${reportPrintoutId}&reportuserid=${CoreUtility.userId}`;
        window.location.href = reportUrl;
        //window.open(reportUrl, '_blank');
    }

    private deletePrintedReport(report: ReportJobStatusDTO) {
        this.reportService.deletePrintedReport(report.reportPrintoutId).then(result => {
            if (result.success)
                this.loadReports(null, false, true);
        });
    }

    private deleteReport(report: ReportMenuDTO) {
        return this.progress.startDeleteProgress(completion => {
            this.reportService.deleteReport(report.reportId).then(result => {
                if (result.success) {
                    completion.completed(null, true);
                    this.loadReportsOverview(this.currentModule);
                    this.loadReports(this.currentModule, true, true);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, (error) => {
                completion.failed(error.message);
            });
        });
    }

    private showHasNoRolesSpecifiedInfo() {
        let keys: string[] = [
            "core.reportmenu.hasnorolesspecified",
            "core.reportmenu.hasnorolesspecified.info"
        ];

        this.translationService.translateMany(keys).then(terms => {
            this.notificationService.showDialogEx(terms["core.reportmenu.hasnorolesspecified"], terms["core.reportmenu.hasnorolesspecified.info"], SOEMessageBoxImage.Information);
        });
    }

    private showError(report: ReportJobStatusDTO) {
        this.notificationService.showDialogEx(this.terms["core.reportmenu.printed.error"], report.printoutErrorMessage, SOEMessageBoxImage.Error);
    }

    private showSettings(report: ReportMenuDTO) {
        this.openReportSettings(report);
    }

    private closeSelection() {
        this.selectedReport = null;
        this.selectedReportModule = this.currentModule;
        this.fullscreen = false;
    }

    // HELP-METHODS

    private setDefaultExportType() {
        if (this.selectedReport && this.selectedReport.reportItem) {
            if (this.selectedReport.reportItem.defaultExportType)
                this.selectedExportType = this.selectedReport.reportItem.defaultExportType.id;
            else if (this.selectedReport.reportItem.supportedExportTypes && this.selectedReport.reportItem.supportedExportTypes.length > 0)
                this.selectedExportType = this.selectedReport.reportItem.supportedExportTypes[0].id;
        }
    }

    private getPage(page: ReportMenuPages) {
        return _.find(this.pages, p => p.page === page);
    }

    private filteredReports(reports: ReportMenuDTO[]): ReportMenuDTO[] {
        const filtered = this.matchFreeTextFilter(reports);

        return _.filter(reports, r => ((r['active'] && !r.noPrintPermission) || this.showInactive) && (r.name.contains(this.freeTextFilter) || (r.reportNr && r.reportNr.toString().contains(this.freeTextFilter)) || (r['isGroup'] && this.groupHasReports(r.groupName, filtered))));
    }

    private matchFreeTextFilter(reports: ReportMenuDTO[]): ReportMenuDTO[] {
        return _.filter(reports, r => (r['active'] || this.showInactive) && (!this.freeTextFilter || r.name.contains(this.freeTextFilter) || (r.reportNr && r.reportNr.toString().contains(this.freeTextFilter))));
    }

    private filteredReportJobsStatus(): ReportJobStatusDTO[] {
        return _.filter(this.reportJobsStatus, r => (!this.freeTextFilter || r.name.contains(this.freeTextFilter)));
    }

    private hasModuleSettingsPermission(module: SoeModule): boolean {
        // Check permission based on module
        switch (module) {
            case SoeModule.Billing:
                return this.billingReportEditPermission;
            case SoeModule.Economy:
                return this.economyReportEditPermission;
            case SoeModule.Time:
                return this.timeReportEditPermission;
        }

        return false;
    }

    private hasReportSettingsPermission(report: ReportMenuDTO): boolean {
        return report ? this.hasModuleSettingsPermission(report.module) : false;
    }

    private groupHasReports(groupName: string, reports: ReportMenuDTO[]): boolean {
        return _.filter(reports, r => r.groupName === groupName).length > 0;
    }

    private get hasRequestedReport(): boolean {
        return this.requestedReportIds.length > 0;
    }

    private get requestedReportIds(): number[] {
        return _.map(_.filter(this.reportJobsStatus, s => (s.printoutStatus === TermGroup_ReportPrintoutStatus.Ordered || s.printoutStatus === TermGroup_ReportPrintoutStatus.Queued) && s.printoutRequested.isToday()), s => s.reportPrintoutId);
    }

    private pollRequestedReports() {
        // Cancel any active polling
        this.cancelPollRequestedReports();

        this.pollRequestedReportsInterval = this.$interval(() => {
            if (this.hasRequestedReport)
                this.loadReports(null, false);
            else
                this.cancelPollRequestedReports();
        }, this.pollTimeout);
    }

    private cancelPollRequestedReports() {
        if (this.pollRequestedReportsInterval)
            this.$interval.cancel(this.pollRequestedReportsInterval);
    }

    private addGroup(reports: ReportMenuDTO[], name: string) {
        const report = new ReportMenuDTO();
        report.isGroup = true;
        report.groupName = report.name = name;
        report.active = true;
        reports.push(report);
    }

    private determineFeature(module: SoeModule): Feature {
        switch (module) {
            case SoeModule.Billing:
                return Feature.Billing_Distribution_Reports_Selection;
            case SoeModule.Economy:
                return Feature.Economy_Distribution_Reports_Selection;
            case SoeModule.Time:
                return Feature.Time_Distribution_Reports_Selection;
            default:
                return Feature.None;
        }
    }

    private setGeneralReportSelections() {
        const general: GeneralReportSelectionDTO = this.selectedReportUserSelection.getGeneralReportSelection();
        if (general) {
            this.selectedExportType = general.exportType;
        } else {
            this.setDefaultExportType();
        }
    }

    private openReportSettings(report: ReportMenuDTO) {
        if (!this.hasReportSettingsPermission(report))
            return;

        const modal = this.$uibModal.open({
            templateUrl: this.urlHelperService.getCommonViewUrl("Reports/ReportGrid", "edit.html"),
            controller: ReportEditController,
            controllerAs: 'ctrl',
            bindToController: true,
            backdrop: 'static',
            size: 'md',
            windowClass: 'fullsize-modal',
            scope: this.$scope
        });

        modal.rendered.then(() => {
            this.$scope.$broadcast(Constants.EVENT_ON_INIT_MODAL, {
                sysReportType: report.isAnalysis ? SoeReportType.Analysis : SoeReportType.CrystalReport,
                module: report.module,
                feature: this.determineFeature(report.module),
                modal: modal,
                id: report.reportId,
                reportTemplateId: report.reportTemplateId,
                isCompanyTemplate: report.isCompanyTemplate
            });
        });

        modal.result.then(result => {
            if (result.modified) {
                if (!this.selectedReportModule)
                    this.selectedReportModule = this.currentModule;
                this.loadReports(this.selectedReportModule, true, true).then(() => {
                    const modifiedReport = this.selectedReportModule.reports.find(r => r.reportId === result.id);
                    if (modifiedReport) {
                        this.selectReport(modifiedReport);
                    } else {
                        this.deselectReport();
                    }
                });
            }
        });
    }
}