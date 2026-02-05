import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ISupportService } from "../SupportService";
import { ToolBarButton, ToolBarUtility } from "../../../Util/ToolBarUtility";
import { IconLibrary } from "../../../Util/Enumerations";
import { SearchSysLogsDTO } from "../../../Common/Models/SearchSysLogsDTO";
import { Feature, SoeLogType } from "../../../Util/CommonEnumerations";
import { HtmlUtility } from "../../../Util/HtmlUtility";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    //Parameters
    logType: SoeLogType;
    nrOfLoadsRpdWeb: number;
    nrOfDisposeRpdWeb: number;
    diffRpdWeb: number;
    nrOfLoadsRpdWs: number;
    nrOfDisposeRpdWs: number;
    diffRpdWs: number;
    gridHeaderComponentUrl: any;
    search: SearchSysLogsDTO;

    //Filter
    excludeMessageInput: string;
    excludeMessage: string;

    //Lookups
    sysLogs: any[];
    levelFilterOptions = [];

    //@ngInject
    constructor(
        private supportService: ISupportService,
        private translationService: ITranslationService,
        private urlHelperService: IUrlHelperService,
        private $window,
        private $scope: ng.IScope,
        gridHandlerFactory: IGridHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory) {
        super(gridHandlerFactory, "Manage.Support.Logs", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify;
            })
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData());
    }

    // SETUP

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        this.logType = soeConfig.logType;

        if (this.logType == SoeLogType.System_All_Today || this.logType == SoeLogType.System_Error_Today || this.logType == SoeLogType.System_Warning_Today || this.logType == SoeLogType.System_Information_Today) {
            this.gridHeaderComponentUrl = this.urlHelperService.getViewUrl("Components/filterLogs.html");
            this.nrOfLoadsRpdWeb = soeConfig.nrOfLoadsRpdWeb;
            this.nrOfDisposeRpdWeb = soeConfig.nrOfDisposeRpdWeb;
            this.diffRpdWeb = soeConfig.diffRpdWeb;
            this.nrOfLoadsRpdWs = soeConfig.nrOfLoadsRpdWs;
            this.nrOfDisposeRpdWs = soeConfig.nrOfDisposeRpdWs;
            this.diffRpdWs = soeConfig.diffRpdWs;
        }
        else if (this.logType == SoeLogType.System_Search) {
            this.gridHeaderComponentUrl = this.urlHelperService.getViewUrl("Components/searchLogs.html");
            this.setupLevels();
            this.search = new SearchSysLogsDTO();
            this.search.fixDates();
        }

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(() => { this.loadGridData(); });
        }

        this.flowHandler.start({ feature: Feature.Manage_Support_Logs, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private setupGrid() {
        var keys: string[] = [
            "manage.support.logs.level",
            "common.date",
            "common.company",
            "common.message",
            "common.quantity",
            "common.stacktrace",
            "core.edit",
            "core.search",
            "manage.support.logs.searchunique",
            "manage.support.logs.showunique",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
            "core.aggrid.totals.selected"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.gridAg.addColumnText("level", terms["manage.support.logs.level"], 20);
            this.gridAg.addColumnDateTime("date", terms["common.date"], 40);
            this.gridAg.addColumnText("companyName", terms["common.company"], 30);
            this.gridAg.addColumnText("message", terms["common.message"], null, true, { usePlainText: true });
            this.gridAg.addColumnText("stackTrace", terms["common.stacktrace"], 100, true, { usePlainText: true });
            this.gridAg.addColumnNumber("uniqueCounter", terms["common.quantity"], 15);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this), false);
            this.gridAg.addColumnIcon(null, null, null, { icon: "fal fa-download", onClick: this.downloadFile.bind(this), showIcon: this.showDownload.bind(this), toolTip: terms["common.download"] });

            this.gridAg.options.addTotalRow("#totals-grid", {
                filtered: terms["core.aggrid.totals.filtered"],
                total: terms["core.aggrid.totals.total"],
                selected: terms["core.aggrid.totals.selected"]
            });

            this.gridAg.addStandardMenuItems();
            this.gridAg.options.finalizeInitGrid();
        });
    }

    public setupLevels() {
        this.levelFilterOptions.push({ id: "NONE", name: "Alla" })
        this.levelFilterOptions.push({ id: "INFO", name: "Information" })
        this.levelFilterOptions.push({ id: "WARN", name: "Vaning" })
        this.levelFilterOptions.push({ id: "ERROR", name: "Fel" })
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg, () => this.loadGridData(), false);

        var showUniqueTerm = this.isSearch() ? "manage.support.logs.searchunique" : "manage.support.logs.showunique";
        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton(showUniqueTerm, showUniqueTerm, IconLibrary.FontAwesome, "fa-sync",
            () => { this.loadUnique(); }
        )));
        if (this.search) {
            this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("core.search", "core.search", IconLibrary.FontAwesome, "fa-search",
                () => { this.searchSysLogs(); }
            )));
        }
    }

    // SERVICE CALLS

    public loadGridData() {
        this.loadSysLogs();
    }

    public loadUnique() {
        if (this.isSearch())
            this.searchSysLogs(true);
        else
            this.loadSysLogs(true);        
    }
    
    public loadSysLogs(showUnique: boolean = false) {
        if (this.isSearch())
            return;

        this.progress.startLoadingProgress([() => {
            return this.supportService.getSysLogs(this.logType, showUnique).then(x => {
                _.forEach(x, (log: any) => {
                    log.date = new Date(log.date);
                });
                this.sysLogs = x;
                return this.sysLogs;
            }).then(data => {
                this.setData(data);
            });
        }]);
    }

    public searchSysLogs(showUnique: boolean = false) {
        if (!this.search)
            return;

        this.search.showUnique = showUnique;
        this.progress.startLoadingProgress([() => {
            return this.supportService.searchSysLogs(this.search).then(x => {
                _.forEach(x, (log: any) => {
                    log.date = new Date(log.date);
                });
                this.sysLogs = x;
                return this.sysLogs;
            }).then(data => {
                this.setData(data);
            });
        }]);
    }

    // EVENTS   

    private excludeFilterChanged = _.debounce(() => {
        this.excludeMessage = this.excludeMessageInput;
        this.applyExcludeFilter();
        this.$scope.$apply();
    }, 500, { leading: false, trailing: true });

    edit(row) {
        // Send message to TabsController
        //if (this.readPermission || this.modifyPermission)
        this.messagingHandler.publishEditRow(row);
    }

    private showDownload(row: any) {
        if (row)
            return true;
        return false;
    }

    private downloadFile(row: any) {
        HtmlUtility.openInSameTab(this.$window, "/soe/manage/support/logs/edit/download/?sysLogId=" + row.sysLogId);
    }

    // HELP-METHODS

    protected isSearch(): boolean {
        return this.logType == SoeLogType.System_Search
    }

    protected hasFilter(): boolean {
        return this.excludeMessage && this.excludeMessage.length > 0;
    }

    private applyExcludeFilter() {
        if (!this.sysLogs)
            return;

        var filteredSysLogs = this.sysLogs;

        if (this.hasFilter()) {
            var excludes: string[] = this.excludeMessage.split(';')            
            _.forEach(excludes, (exclude: string) => {
                if (exclude && exclude.length > 0)
                    filteredSysLogs = filteredSysLogs.filter(sysLog => !sysLog.message.includes(exclude));
            });
        }

        this.setData(filteredSysLogs);
    }
}

