import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { EmployeeService } from "../EmployeeService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { CoreUtility } from "../../../Util/CoreUtility";
import { Feature } from "../../../Util/CommonEnumerations";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { SoeGridOptionsEvent, SOEMessageBoxImage } from "../../../Util/Enumerations";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { GridEvent } from "../../../Util/SoeGridOptions";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Terms
    private terms: { [index: string]: string; };

    private gridFooterComponentUrl: any;

    // Functions
    private hasSelectedRows: boolean = false;
    private buttonFunctions: any = [];

    private get disableFunctions(): boolean {
        return this.parameters.type === "Sys" && !this.hasSelectedRows;
    }

    //@ngInject
    constructor(
        private $scope,
        private employeeService: EmployeeService,
        private translationService: ITranslationService,
        private coreService: ICoreService,
        private urlHelperService: IUrlHelperService,
        private notificationService: INotificationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "time.employee.position.positions", progressHandlerFactory, messagingHandlerFactory);
        this.useRecordNavigatorInEdit('positionId', 'name');

        this.gridFooterComponentUrl = urlHelperService.getViewUrl("gridFooter.html");

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onBeforeSetUpGrid(() => this.loadTerms())
            .onSetUpGrid(() => this.setUpGrid())
            .onLoadGridData(() => this.loadGridData(true))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
    }

    // SETUP

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Time_Employee_Positions].readPermission;
        this.modifyPermission = response[Feature.Time_Employee_Positions].modifyPermission;

        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.guid = parameters.guid;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.reloadData(); });
        }

        this.flowHandler.start({ feature: Feature.Time_Employee_Positions, loadReadPermissions: true, loadModifyPermissions: true });
        this.$scope.$on('onTabActivated', (e, a) => {
            this.reloadData();
        });
    }

    private setUpGrid() {
        if (this.parameters.type === "Sys") {
            this.gridAg.options.enableRowSelection = true;

            this.gridAg.addColumnText("sysCountryCode", this.terms["time.employee.position.countrycode"], 50, false);
            this.gridAg.addColumnText("sysLanguageCode", this.terms["time.employee.position.langcode"], 50, false);
            this.gridAg.addColumnText("code", this.terms["time.employee.position.ssyk"], null, false);
            this.gridAg.addColumnText("name", this.terms["common.name"], 100, false);
            this.gridAg.addColumnText("description", this.terms["common.description"], null, true);
            this.gridAg.addColumnIcon("isLinked", null, null, { icon: "fal fa-link iconEdit", showIcon: this.showLinkedIcon.bind(this), toolTip: this.terms["time.employee.position.link"] });

            this.doubleClickToEdit = false;

            var events: GridEvent[] = [];
            events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row: uiGrid.IGridRow) => { this.gridSelectionChanged(); }));
            events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (row: uiGrid.IGridRow) => { this.gridSelectionChanged(); }));
            this.gridAg.options.subscribe(events);
        } else {
            this.gridAg.options.enableRowSelection = false;

            this.gridAg.addColumnText("code", this.terms["common.code"], 50, false);
            this.gridAg.addColumnText("name", this.terms["common.name"], 100, false);
            this.gridAg.addColumnText("description", this.terms["common.description"], null, true);
            this.gridAg.addColumnIcon("isLinked", null, null, { icon: "fal fa-link iconEdit", showIcon: this.showLinkedIcon.bind(this), toolTip: this.terms["time.employee.position.link"] });
            this.gridAg.addColumnEdit(this.terms["core.edit"], this.edit.bind(this));
        }

        this.setupFunctions();
        this.gridAg.finalizeInitGrid("time.employee.position.positions", true);
    }

    protected showLinkedIcon(row: any): boolean {
        return row.isLinked;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.reloadData());
    }

    private setupFunctions() {
        if (this.parameters.type === "Sys") {
            this.buttonFunctions.push({ id: 2, name: this.terms["time.employee.position.updatesyspositions"] });
            this.buttonFunctions.push({ id: 3, name: this.terms["time.employee.position.updateandlinksyspositions"] });
        } else {
            this.buttonFunctions.push({ id: 1, name: this.terms["time.employee.position.updatepositions"] });
        }
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "common.name",
            "common.description",
            "core.edit",
            "common.code",
            "time.employee.position.link",
            "time.employee.position.ssyk",
            "time.employee.position.countrycode",
            "time.employee.position.langcode",
            "time.employee.position.updatepositions",
            "time.employee.position.updatepositions.error",
            "time.employee.position.updatesyspositions",
            "time.employee.position.updateandlinksyspositions",
            "time.employee.position.updateandlinksyspositions.error"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadGridData(useCache: boolean) {
        this.gridAg.clearData();
        if (this.parameters.type === "Sys") {
            this.progress.startLoadingProgress([() => {
                return this.coreService.getSysPositions(CoreUtility.sysCountryId, CoreUtility.languageId, useCache).then(data => {
                    this.setData(data);
                });
            }]);
        } else {
            this.progress.startLoadingProgress([() => {
                return this.employeeService.getPositionsGrid(false, useCache).then(data => {
                    _.forEach(data, (item: any) => {
                        item.isLinked = (item.sysPositionId > 0);
                    });
                    this.setData(data);
                });
            }]);
        }
    }

    private reloadData() {
        this.loadGridData(false);
    }

    // EVENTS

    private gridSelectionChanged() {
        this.$scope.$applyAsync(() => {
            this.hasSelectedRows = this.gridAg.options.getSelectedCount() > 0;
        });
    }

    private executeButtonFunction(option) {
        switch (option.id) {
            case 1:
                this.employeeService.updateLinkedPositions().then(result => {
                    if (result.success) {
                        this.reloadData();
                    } else {
                        this.notificationService.showDialogEx(this.terms["time.employee.position.updatepositions.error"], result.errorMessage, SOEMessageBoxImage.Error);
                    }
                });
                break;
            case 2:
            case 3:
                this.employeeService.updateSysPosionsGrid(this.gridAg.options.getSelectedRows(), option.id === 3).then(result => {
                    if (result.success) {
                        this.hasSelectedRows = false;
                        this.reloadData();
                    } else {
                        this.notificationService.showDialogEx(this.terms["time.employee.position.updateandlinksyspositions.error"], result.errorMessage, SOEMessageBoxImage.Error);
                    }
                });
                break;
        }
    }
}