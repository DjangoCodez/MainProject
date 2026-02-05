import { ICompositionGridController } from "../../Core/ICompositionGridController";
import { IGridHandlerFactory } from "../../Core/Handlers/GridHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../Core/Handlers/MessagingHandlerFactory";
import { IUrlHelperService } from "../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../Core/Services/TranslationService";
import { INotificationService } from "../../Core/Services/NotificationService";
import { IconLibrary, SoeGridOptionsEvent, SOEMessageBoxImage } from "../../Util/Enumerations";
import { Feature, TermGroup_ApiMessageSourceType, TermGroup_ApiMessageType } from "../../Util/CommonEnumerations";
import { IPermissionRetrievalResponse } from "../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../Core/Handlers/ToolbarFactory";
import { IGridHandler } from "../../Core/Handlers/GridHandler";
import { IActionResult } from "../../Scripts/TypeLite.Net4";
import { IApiService } from "./ApiService";
import { GridControllerBase2Ag } from "../../Core/Controllers/GridControllerBase2Ag";
import { ToolBarUtility, ToolBarButton } from "../../Util/ToolBarUtility";
import { CoreUtility } from "../../Util/CoreUtility";
import { Constants } from "../../Util/Constants";
import { HtmlUtility } from "../../Util/HtmlUtility";
import { GridEvent } from "../../Util/SoeGridOptions";
import { ApiMessageGridDTO } from "../Models/ApiMessageDTO";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Init parameters
    private feature: Feature;

    // Terms:
    private terms: any;

    // Data
    private messages: ApiMessageGridDTO[];
    private selectedCount: number = 0;

    // Toolbar
    private toolbarInclude: any;

    // Flags
    private disableAutoLoad: boolean = true;
    private firstLoadHasOccurred: boolean = false;
    private filterShowVerified: boolean = false;
    private filterShowOnlyErrors: boolean = false;
    private _filterFromDate: Date;
    private get filterFromDate(): Date {
        return this._filterFromDate;
    }
    private set filterFromDate(date: Date) {
        if (!date)
            date = new Date().date();
        this._filterFromDate = date;
    }
    private _filterToDate: Date;
    private get filterToDate(): Date {
        return this._filterToDate;
    }
    private set filterToDate(date: Date) {
        if (!date)
            date = new Date().date();
        this._filterToDate = date;
    }
    

    //@ngInject
    constructor(
        private apiService: IApiService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private $timeout: ng.ITimeoutService,
        private $window,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        urlHelperService: IUrlHelperService) {
        super(gridHandlerFactory, "Common.Api", progressHandlerFactory, messagingHandlerFactory);

        this.filterFromDate = new Date().date().addMonths(-1);
        this.filterToDate = new Date().date();
        this.toolbarInclude = urlHelperService.getViewUrl("gridHeader.html");
        
        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onBeforeSetUpGrid(() => this.loadTerms())
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData(false));
    }

    // SETUP

    public onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;
        this.feature = soeConfig.feature;

        this.flowHandler.start([
            { feature: this.feature, loadReadPermissions: true, loadModifyPermissions: true },
        ]);
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[this.feature].readPermission;
        this.modifyPermission = response[this.feature].modifyPermission;
    }

    public setupGrid() {

        this.doubleClickToEdit = false;
        this.gridAg.options.enableRowSelection = true;
        let colDef = this.gridAg.addColumnText("typeName", this.terms["common.api.type"], 40, true);
        colDef.cellRenderer = 'agGroupCellRenderer';
        this.gridAg.addColumnText("identifiers", this.terms["common.api.identifier"], 47, true);
        this.gridAg.addColumnText("recordCount", this.terms["common.api.recordcount"], 25, true);
        this.gridAg.addColumnText("sourceTypeName", this.terms["common.api.sourcetype"], 35, true);
        this.gridAg.addColumnText("statusName", this.terms["common.api.status"], 45, true);
        this.gridAg.addColumnDateTime("created", this.terms["common.api.created"], 60, true, null, { showSeconds: true });
        this.gridAg.addColumnDateTime("modified", this.terms["common.api.modified"], 60, true, null, { showSeconds: true });
        this.gridAg.addColumnText("comment", this.terms["common.api.comment"], 85, true);
        var colValidationMessage = this.gridAg.addColumnText("validationMessage", this.terms["common.api.validationmessage"], null, true);
        if (colValidationMessage) {
            colValidationMessage.cellClassRules = {
                "errorRow": (params) => params.data.hasError === true,
            };
        }
        this.gridAg.addColumnIcon(null, null, null, { icon: "fal fa-download", onClick: this.downloadFile.bind(this), showIcon: this.showDownload.bind(this), toolTip: this.terms["common.download"] });

        var events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row: any) => {
            this.$timeout(() => {
                this.selectedCount = this.gridAg.options.getSelectedCount();
            });
        }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (row: any) => {
            this.$timeout(() => {
                this.selectedCount = this.gridAg.options.getSelectedCount();
            });
        }));
        this.gridAg.options.subscribe(events);

        // Details
        this.gridAg.enableMasterDetail(true);
        this.gridAg.detailOptions.enableFiltering = false;
        this.gridAg.options.setDetailCellDataCallback((params) => {
            // Hide selection row, since enableRowSelection does not seem to work for detail grids
            let gridName = (params.node && params.node.detailGridInfo ? params.node.detailGridInfo.id : null);
            if (gridName)
                this.gridAg.options.setChildGridColumnVisibility(gridName, 'soe-row-selection', false);

            // Return data
            params.successCallback(params.data['changes']);
        });
        this.gridAg.detailOptions.addColumnText("typeName", this.terms["common.api.changetype"], 47);
        this.gridAg.detailOptions.addColumnText("identifier", this.terms["common.api.identifier"], 45);
        this.gridAg.detailOptions.addColumnText("recordName", this.terms["common.api.recordname"], 57);
        this.gridAg.detailOptions.addColumnText("fieldTypeName", this.terms["common.api.changefieldtype"], 60);
        this.gridAg.detailOptions.addColumnText("fromValue", this.terms["common.api.fromvalue"], 80);
        this.gridAg.detailOptions.addColumnText("toValue", this.terms["common.api.tovalue"], 80);
        this.gridAg.detailOptions.addColumnDate("fromDate", this.terms["common.from"], 35);
        this.gridAg.detailOptions.addColumnDate("toDate", this.terms["common.to"], 35);
        var colError = this.gridAg.detailOptions.addColumnText("error", this.terms["core.error"], null);
        if (colError) {
            colError.cellClassRules = {
                "errorRow": (params) => params.data.hasError === true,
            };
        }

        this.gridAg.finalizeInitGrid("common.api", true);
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.reloadData());
        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("common.api.importfile.onlylogging", "common.api.importfile.onlylogging", IconLibrary.FontAwesome, "fa-file-export",
            () => { this.importFile(true); }
        )));
        if (CoreUtility.isSupportAdmin) {
            this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("common.api.importfile", "common.api.importfile", IconLibrary.FontAwesome, "fa-file-export",
                () => { this.importFile(false); }
            )));
        }

        var group = ToolBarUtility.createGroup(new ToolBarButton("common.api.saveverified", "common.api.saveverified", IconLibrary.FontAwesome, "fa-file-export", () => {
            this.saveAsVerified();
        }, () => {
            return this.selectedCount === 0;
        }, () => {
            return !this.modifyPermission;
        }));
        this.toolbar.addButtonGroup(group);

        this.toolbar.addInclude(this.toolbarInclude);
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
            "common.api.created",
            "common.api.modified",
            "common.api.type",
            "common.api.sourcetype",
            "common.api.status",
            "common.api.comment",
            "common.api.validationmessage",
            "common.api.recordcount",
            "common.api.recordname",
            "common.api.type",
            "common.api.changetype",
            "common.api.identifier",
            "common.api.changefieldtype",
            "common.api.fromvalue",
            "common.api.tovalue",
            "common.api.importfile",
            "common.api.importfile.complete",
            "common.api.importfile.error",
            "common.api.importfile.onlylogging",
            "common.from",
            "common.to",
            "common.download",
            "core.error",
            "core.workfailed",
            "core.worked",
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    public loadGridData(force: boolean) {
        if (!force && this.isAutoLoadDisabled())
            return;

        this.progress.startLoadingProgress([() => {
            return this.apiService.getApiMessages(TermGroup_ApiMessageType.Employee, TermGroup_ApiMessageSourceType.AllAPI, this.filterFromDate, this.filterToDate, this.filterShowVerified, this.filterShowOnlyErrors).then(x => {
                this.messages = x;
                this.setData(this.messages);
                this.firstLoadHasOccurred = true;
            });
        }]);
    }

    private reloadData() {
        this.loadGridData(true);
    }

    private filterChanged() {
        if (this.isAutoLoadDisabled())
            return;
        this.$timeout(() => {
            this.reloadData();
        });
    }

    private saveAsVerified() {
        var ids: number[] = this.gridAg.options.getSelectedIds("apiMessageId");
        if (ids.length > 0) {
            this.progress.startWorkProgress((completion) => {
                this.apiService.setApiMessagesAsVerified(ids).then(result => {
                    if (result.success) {
                        completion.completed(null, true);
                        this.reloadData();
                        this.gridAg.options.clearSelectedRows();
                    } else {
                        completion.failed(this.terms["core.workfailed"]);
                    }
                }, error => {
                    completion.failed(this.terms["core.workfailed"]);
                });
            });
        }
    }

    // EVENTS

    private importFile(onlyLogging: boolean) {
        var url = CoreUtility.apiPrefix + Constants.WEBAPI_CORE_API_MESSAGES_IMPORT_EMPLOYEES + onlyLogging;
        var modal = this.notificationService.showFileUpload(url, this.terms["common.api.importfile"], true, true, false);
        modal.result.then(res => {
            let result: IActionResult = res.result;
            if (result.success) {
                this.notificationService.showDialogEx(this.terms["common.api.importfile"], this.terms["common.api.importfile.complete"], SOEMessageBoxImage.OK);
                this.loadGridData(true);
            } else {
                this.notificationService.showDialogEx(this.terms["common.api.importfile"], this.terms["common.api.importfile.error"], SOEMessageBoxImage.Error);
            }
        });
    }

    private showDownload(row: ApiMessageGridDTO) {
        if (row && row.hasFile)
            return true;
        return false;
    }

    private downloadFile(row: ApiMessageGridDTO) {
        HtmlUtility.openInSameTab(this.$window, "/soe/time/import/api/download/?apiMessageId=" + row.apiMessageId);
    }

    // HELP-METHODS

    private isAutoLoadDisabled() {
        return this.disableAutoLoad && !this.firstLoadHasOccurred;
    }
}