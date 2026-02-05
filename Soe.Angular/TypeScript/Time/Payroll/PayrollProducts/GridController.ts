import { BatchUpdateController } from "../../../Common/Dialogs/BatchUpdate/BatchUpdateDirective";
import { PayrollProductGridDTO } from "../../../Common/Models/ProductDTOs";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { ReportJobDefinitionFactory } from "../../../Core/Handlers/ReportJobDefinitionFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IReportDataService } from "../../../Core/RightMenu/ReportMenu/ReportDataService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ISelectedItemsService } from "../../../Core/Services/SelectedItemsService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { CompanySettingType, Feature, SoeEntityType, SoeReportTemplateType } from "../../../Util/CommonEnumerations";
import { IconLibrary, SoeGridOptionsEvent, SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../Util/Enumerations";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { ToolBarButton, ToolBarUtility } from "../../../Util/ToolBarUtility";
import { IPayrollService } from "../PayrollService";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Terms
    private terms: { [index: string]: string; };

    //Permissions
    private hasMassUpdatePermission: boolean;

    // Company settings
    private defaultReportId: number = 0;

    // Data
    private rows: PayrollProductGridDTO[];

    // Flags
    private gridHasSelectedRows: boolean = false;

    //@ngInject
    constructor(
        private $scope: ng.IScope,
        private $uibModal,
        private urlHelperService: IUrlHelperService,
        private $q: ng.IQService,
        private coreService: ICoreService,
        private payrollService: IPayrollService,
        private reportDataService: IReportDataService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private selectedItemsService: ISelectedItemsService,
        gridHandlerFactory: IGridHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory) {
        super(gridHandlerFactory, "Time.Payroll.PayrollProducts.PayrollProducts", progressHandlerFactory, messagingHandlerFactory);

        this.useRecordNavigatorInEdit('productId', 'description');

        this.selectedItemsService.setup($scope, "productId", (items: number[]) => this.save(items));

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onBeforeSetUpGrid(() => this.loadLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData(false));

        super.onTabActivetedAndModified(() => {
            this.loadGridData(false);
        });
    }

    // SETUP

    onInit(parameters: any) {
        this.guid = parameters.guid;
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        this.flowHandler.start([
            { feature: Feature.Time_Preferences_SalarySettings_PayrollProduct, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Time_Preferences_SalarySettings_PayrollProduct_MassUpdate, loadReadPermissions: true, loadModifyPermissions: true }
        ]);
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Time_Preferences_SalarySettings_PayrollProduct].readPermission;
        this.modifyPermission = response[Feature.Time_Preferences_SalarySettings_PayrollProduct].modifyPermission;
        this.hasMassUpdatePermission = response[Feature.Time_Preferences_SalarySettings_PayrollProduct_MassUpdate].modifyPermission;
        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();
    }

    private setupGrid() {
        this.gridAg.options.subscribe([new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (row: uiGrid.IGridRow) => { this.gridSelectionChanged(); })]);
        this.gridAg.options.useGrouping(false, false, { keepColumnsAfterGroup: true, selectChildren: false });
        this.gridAg.options.groupHideOpenParents = true;

        this.gridAg.addColumnActive("isActive", this.terms["common.active"], 50, (params) => this.selectedItemsService.CellChanged(params));
        this.gridAg.addColumnText("number", this.terms["common.number"], 100);
        this.gridAg.addColumnText("shortName", this.terms["common.shortname"], 100, true);
        this.gridAg.addColumnText("name", this.terms["common.name"], null);
        this.gridAg.addColumnText("externalNumber", this.terms["time.payroll.payrollproduct.externalnumber"], 100, true);
        this.gridAg.addColumnText("sysPayrollTypeLevel1Name", this.terms["time.payroll.payrollproduct.syspayrolltypelevel1"], 100, true, { enableRowGrouping: true });
        this.gridAg.addColumnText("sysPayrollTypeLevel2Name", this.terms["time.payroll.payrollproduct.syspayrolltypelevel2"], 100, true, { enableRowGrouping: true });
        this.gridAg.addColumnText("sysPayrollTypeLevel3Name", this.terms["time.payroll.payrollproduct.syspayrolltypelevel3"], 100, true, { enableRowGrouping: true });
        this.gridAg.addColumnText("sysPayrollTypeLevel4Name", this.terms["time.payroll.payrollproduct.syspayrolltypelevel4"], 100, true, { enableRowGrouping: true });
        this.gridAg.addColumnNumber("factor", this.terms["time.payroll.payrollproduct.factor"], 50, { decimals: 2, enableHiding: true, enableRowGrouping: true });
        this.gridAg.addColumnText("resultTypeText", this.terms["time.payroll.payrollproduct.resulttype"], 100, true, { enableRowGrouping: true });
        this.gridAg.addColumnText("payedText", this.terms["time.payroll.payrollproduct.payed"], 80, true, { enableRowGrouping: true });
        this.gridAg.addColumnText("excludeInWorkTimeSummaryText", this.terms["time.payroll.payrollproduct.excludeinworktimesummary"], 80, true, { enableRowGrouping: true });
        this.gridAg.addColumnText("averageCalculatedText", this.terms["time.payroll.payrollproduct.averagecalculated"], 80, true, { enableRowGrouping: true });
        this.gridAg.addColumnText("useInPayrollText", this.terms["time.payroll.payrollproduct.useinpayroll"], 80, true, { enableRowGrouping: true });
        this.gridAg.addColumnText("exportText", this.terms["time.payroll.payrollproduct.export"], 80, true, { enableRowGrouping: true });
        this.gridAg.addColumnText("includeAmountInExportText", this.terms["time.payroll.payrollproduct.includeamountinexport"], 80, true, { enableRowGrouping: true });
        this.gridAg.addColumnEdit(this.terms["core.edit"], this.edit.bind(this), false);

        this.gridAg.finalizeInitGrid("time.payroll.payrollproducts.payrollproducts", true, undefined, true);
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.reloadData(), true, () => this.selectedItemsService.Save(), () => { return this.saveButtonIsDisabled() });

        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("", "core.print", IconLibrary.FontAwesome, "fa-print", () => {
            this.printPayrollproducts();
        }, () => {
            return !this.gridHasSelectedRows;
        })));
        
        if (this.hasMassUpdatePermission) {
            this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("common.batchupdate.title", "common.batchupdate.title", IconLibrary.FontAwesome, "fa-pencil",
                () => { this.openBatchUpdate(); }, () => { return !this.gridHasSelectedRows; }, () => { return false }
            )));
        }
    }

    private saveButtonIsDisabled(): boolean {
        return !this.selectedItemsService.SelectedItemsExist();
    }

    // SERVICE CALLS   

    private loadLookups(): ng.IPromise<any> {
        return this.$q.all([this.loadTerms(), this.loadCompanySettings()]);
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.edit",
            "core.yes",
            "core.no",
            "core.warning",
            "common.active",
            "common.number",
            "common.name",
            "common.shortname",
            "common.reportsettingmissing",
            "time.payroll.payrollproduct.averagecalculated",
            "time.payroll.payrollproduct.excludeinworktimesummary",
            "time.payroll.payrollproduct.export",
            "time.payroll.payrollproduct.externalnumber",
            "time.payroll.payrollproduct.factor",
            "time.payroll.payrollproduct.includeamountinexport",
            "time.payroll.payrollproduct.payrolltype",
            "time.payroll.payrollproduct.payed",
            "time.payroll.payrollproduct.resulttype",
            "time.payroll.payrollproduct.syspayrolltypelevel1",
            "time.payroll.payrollproduct.syspayrolltypelevel2",
            "time.payroll.payrollproduct.syspayrolltypelevel3",
            "time.payroll.payrollproduct.syspayrolltypelevel4",
            "time.payroll.payrollproduct.useinpayroll",
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.PayrollSettingsDefaultReport);
        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.defaultReportId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.PayrollSettingsDefaultReport, 0);
        });
    }

    public loadGridData(useCache: boolean) {
        var yesText = this.terms["core.yes"];
        var noText = this.terms["core.no"];

        this.progress.startLoadingProgress([() => {
            return this.payrollService.getPayrollProductsGrid(false).then(x => {
                this.rows = x;
                _.forEach(this.rows, row => {
                    row['payedText'] = row.payed ? yesText : noText;
                    row['excludeInWorkTimeSummaryText'] = row.excludeInWorkTimeSummary ? yesText : noText;
                    row['averageCalculatedText'] = row.averageCalculated ? yesText : noText;
                    row['useInPayrollText'] = row.useInPayroll ? yesText : noText;
                    row['exportText'] = row.export ? yesText : noText;
                    row['includeAmountInExportText'] = row.includeAmountInExport ? yesText : noText;
                });

                return this.rows;
            }).then(data => {
                this.setData(data);
            });
        }]);
    }

    private reloadData() {
        this.loadGridData(false);
    }

    private save(items: number[]) {
        var dict: any = {};

        _.forEach(items, (id: number) => {
            // Find entity
            var entity: any = this.gridAg.options.findInData((ent: any) => ent["productId"] === id);

            // Push id and active flag to array
            if (entity !== undefined) {
                dict[id] = entity.isActive;
            }
        });

        if ((dict !== undefined) && (Object.keys(dict).length > 0)) {
            this.payrollService.updatePayrollProductsState(dict).then(result => {
                if (result.success && result.booleanValue && result.keys.length > 0) {
                    let keys: string[] = [];
                    keys.push("time.payroll.payrollproduct.unabletodisable.title");
                    keys.push("time.payroll.payrollproduct.unabletodisable.message");
                    this.translationService.translateMany(keys).then(terms => {
                        this.notificationService.showDialogEx(terms["time.payroll.payrollproduct.unabletodisable.title"], terms["time.payroll.payrollproduct.unabletodisable.message"].format(result.keys.length.toString()), SOEMessageBoxImage.Forbidden);
                    });
                }
                this.reloadData();
            });
        }
    }

    private openBatchUpdate() {
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/BatchUpdate/Views/BatchUpdate.html"),
            controller: BatchUpdateController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'lg',
            resolve: {
                entityType: () => { return SoeEntityType.PayrollProduct },
                selectedIds: () => { return _.map(this.gridAg.options.getSelectedRows(), 'productId') }
            }
        }
        this.$uibModal.open(options).result.then(result => {
            // Reset cache
            this.loadGridData(false);
            this.gridHasSelectedRows = false;
        }, (reason) => {
            // Cancelled
        });
        this.$scope.$applyAsync();
    }

    // EVENTS   

    private gridSelectionChanged() {
        this.$scope.$applyAsync(() => {
            this.gridHasSelectedRows = (this.gridAg.options.getSelectedCount() > 0);
        });
    }

    private printPayrollproducts() {
        if (this.defaultReportId) {
            var ids = [];
            _.forEach(this.gridAg.options.getSelectedRows(), (row: PayrollProductGridDTO) => {
                ids.push(row.productId);
            });

            this.reportDataService.createReportJob(ReportJobDefinitionFactory.createPayrollProductReportDefinition(this.defaultReportId, SoeReportTemplateType.PayrollProductReport, ids), true);
        } else {
            this.notificationService.showDialog(this.terms["core.warning"], this.terms["common.reportsettingmissing"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
        }
    }
}