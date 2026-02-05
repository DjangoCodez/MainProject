import { ICoreService } from "../../Core/Services/CoreService";
import { ITranslationService } from "../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../Core/Services/UrlHelperService";
import { GridControllerBase2Ag } from "../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../Core/ICompositionGridController";
import { IControllerFlowHandlerFactory } from "../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../Core/Handlers/gridhandlerfactory";
import { IToolbarFactory } from "../../Core/Handlers/ToolbarFactory";
import { CalendarUtility } from "../../Util/CalendarUtility";
import { SoeEntityType } from "../../Util/CommonEnumerations";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    // Filters

    fromDate: Date;
    toDate: Date;
    users = [];
    selectedUsers = [];

    private entityType: number;
    private feature: number;

    // Grid header
    toolbarInclude: any;

    get searchDisabled() {
        return !this.fromDate || !this.toDate;
    }

    //@ngInject
    constructor(
        private $timeout: ng.ITimeoutService,
        private $scope: ng.IScope,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        private urlHelperService: IUrlHelperService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {

        super(gridHandlerFactory, "common.trackchanges.trackchanges", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify

                if (this.modifyPermission) {
                    // Send messages to TabsController
                    this.messagingHandler.publishActivateAddTab();
                }
            })
            .onBeforeSetUpGrid(() => this.loadUsers())
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.setData([]))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;
        this.entityType = soeConfig.entityType;

        this.toolbarInclude = this.urlHelperService.getViewUrl("filterHeader.html");

        this.flowHandler.start({ feature: soeConfig.feature, loadReadPermissions: true, loadModifyPermissions: true });

        // Set dates
        const today: Date = CalendarUtility.getDateToday();
        this.fromDate = new Date(today.getFullYear(), today.getMonth(), 1);
        this.toDate = new Date(today.getFullYear(), today.getMonth() + 1, 0);

        this.setupWatches();
    }

    private setupWatches() {
        this.$scope.$watch(() => this.fromDate, (newValue: Date, oldValue: Date) => {
            if (newValue && oldValue && (newValue.getTime() > this.toDate.getTime()))
                this.toDate = this.fromDate.addDays(6);
        });
        this.$scope.$watch(() => this.toDate, (newValue: Date, oldValue: Date) => {
            if (newValue && oldValue && (newValue.getTime() < this.fromDate.getTime()))
                this.fromDate = this.toDate;
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg, () => this.search());
        this.toolbar.addInclude(this.toolbarInclude);
    }

    public setupGrid() {
        // Columns
        const keys: string[] = [
            "common.dashboard.attestflow.supplier",
            "common.date",
            "common.entitylogviewer.changelog.toprecordname",
            "common.field",
            "common.from",
            "common.modified",
            "common.modifiedby",
            "common.time",
            "common.to",
            "common.type",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            let topRecordLabel: string = terms["common.entitylogviewer.changelog.toprecordname"];
            if (this.entityType === SoeEntityType.Supplier)
                topRecordLabel = terms["common.dashboard.attestflow.supplier"];

            this.gridAg.addColumnText("topRecordName", topRecordLabel, null, false, { enableRowGrouping: true, enableHiding: false });
            this.gridAg.addColumnText("columnText", terms["common.field"], null, false, { enableRowGrouping: true, enableHiding: false });
            this.gridAg.addColumnText("actionText", terms["common.type"], null, false, { enableRowGrouping: true, enableHiding: false });
            this.gridAg.addColumnText("fromValueText", terms["common.from"], null, false, { enableRowGrouping: true, enableHiding: false });
            this.gridAg.addColumnText("toValueText", terms["common.to"], null, false, { enableRowGrouping: true, enableHiding: false });
            this.gridAg.addColumnDate("created", terms["common.date"], null);
            this.gridAg.addColumnTime("created", terms['common.time'], null);
            this.gridAg.addColumnText("createdBy", terms["common.modifiedby"], null, false, { enableRowGrouping: true, enableHiding: false });
            this.gridAg.addColumnText("batch", "Batch", null, false, { enableRowGrouping: true, enableHiding: false });
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.options.useGrouping(false, false);
            this.gridAg.finalizeInitGrid("common.changehistory", true);
        });
    }

    public edit(row) {
        if (this.doubleClickToEdit && (this.readPermission || this.modifyPermission))
            this.messagingHandler.publishEditRow({ id: row.recordId, entityType: this.entityType });
    }

    private search() {
        this.progress.startLoadingProgress([() => {
            return this.coreService.getTrackChangesLogForEntity(this.entityType, this.fromDate, this.toDate, _.map(this.selectedUsers, u => u.id)).then(data => {
                this.setData(data);
            });
        }]);
    }

    protected setData(data: any) {
        this.gridAg.setData(data);
        this.messagingHandler.publishResizeWindow();
    }

    private loadUsers(): ng.IPromise<any> {
        this.users = [];
        return this.coreService.getUsers(false, true).then(x => {
            _.forEach(x, (y) => {
                this.users.push({ id: y.userId, label: y.loginName });
            });
        });
    }

    private decreaseDate() {
        const diffDays = this.fromDate.diffDays(this.toDate) - 1;
        this.fromDate = this.fromDate.addDays(diffDays);
        this.toDate = this.toDate.addDays(diffDays);
    }

    private increaseDate() {
        const diffDays = this.toDate.diffDays(this.fromDate) + 1;
        this.fromDate = this.fromDate.addDays(diffDays);
        this.toDate = this.toDate.addDays(diffDays);
    }
}