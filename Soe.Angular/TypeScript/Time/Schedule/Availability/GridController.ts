import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { Feature, TermGroup_EmployeeRequestType } from "../../../Util/CommonEnumerations";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { EmployeeRequestDTO } from "../../../Common/Models/EmployeeRequestDTO";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Terms:
    private terms: any;

    // Data
    private requests: EmployeeRequestDTO[];

    //@ngInject
    constructor(
        private coreService: ICoreService,
        private translationService: ITranslationService,
        private $timeout: ng.ITimeoutService,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        urlHelperService: IUrlHelperService) {
        super(gridHandlerFactory, "Time.Schedule.Availability", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onBeforeSetUpGrid(() => this.loadTerms())
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData());
    }

    // SETUP

    public onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        this.flowHandler.start([
            { feature: Feature.Time_Schedule_Availability, loadReadPermissions: true, loadModifyPermissions: true },
        ]);
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Time_Schedule_Availability].readPermission;
        this.modifyPermission = response[Feature.Time_Schedule_Availability].modifyPermission;
    }

    public setupGrid() {
        this.doubleClickToEdit = false;

        this.gridAg.addColumnText("typeName", this.terms["common.type"], 50);
        this.gridAg.addColumnText("employeeName", this.terms["common.name"], null);
        this.gridAg.addColumnDateTime("start", this.terms["common.from"], 50);
        this.gridAg.addColumnDateTime("stop", this.terms["common.to"], 50);
        this.gridAg.addColumnDateTime("created", this.terms["common.created"], 50, true);
        this.gridAg.addColumnDateTime("modified", this.terms["common.modified"], 50, true);

        this.gridAg.finalizeInitGrid("time.schedule.availability", true);
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.reloadData());
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
            "common.type",
            "common.name",
            "common.from",
            "common.to",
            "common.created",
            "common.modified",
            "time.schedule.planning.available",
            "time.schedule.planning.unavailable"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    public loadGridData() {
        this.progress.startLoadingProgress([() => {
            return this.coreService.getEmployeeRequests(0, null, null).then(x => {
                this.requests = x;

                _.forEach(this.requests, r => {
                    r.typeName = (r.type === TermGroup_EmployeeRequestType.InterestRequest ? this.terms["time.schedule.planning.available"] : this.terms["time.schedule.planning.unavailable"]);
                });
                this.setData(this.requests);
            });
        }]);
    }

    private reloadData() {
        this.loadGridData();
    }
}