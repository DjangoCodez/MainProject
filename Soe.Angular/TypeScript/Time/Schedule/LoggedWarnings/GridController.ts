import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IScheduleService } from "../ScheduleService";
import { Feature, TermGroup } from "../../../Util/CommonEnumerations";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Grid header and footer
    toolbarInclude: any;
    progressBusy: boolean = true;

    allItemsSelectionDict: any[];

    private _allItemsSelection: any;
    get allItemsSelection() {
        return this._allItemsSelection;
    }
    set allItemsSelection(item: any) {
        this._allItemsSelection = item;
        if (!this.progressBusy)
            this.updateItemsSelection();
    }

    //@ngInject
    constructor(
        private scheduleService: IScheduleService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        urlHelperService: IUrlHelperService,
        private coreService: ICoreService) {
        super(gridHandlerFactory, "Time.Schedule.LoggedWarnings", progressHandlerFactory, messagingHandlerFactory)
        this.toolbarInclude = urlHelperService.getViewUrl("gridHeader.html");

        this.progress = progressHandlerFactory.create();

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onBeforeSetUpGrid(() => this.loadSelectionTypes())
            .onSetUpGrid(() => this.setUpGrid())
            .onLoadGridData(() => this.loadGridData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;
        //No edit page in logged warnings
        this.doubleClickToEdit = false;
        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.reloadData(); });
        }

        this.flowHandler.start({ feature: Feature.Time_Schedule_SchedulePlanning_LoggedWarnings, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Time_Schedule_SchedulePlanning_LoggedWarnings].readPermission;
        this.modifyPermission = response[Feature.Time_Schedule_SchedulePlanning_LoggedWarnings].modifyPermission;

        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();
    }

    private setUpGrid() {
        var translationKeys: string[] = [
            "common.date",
            "common.employee",
            "time.schedule.workrulebypass.messagetext",
            "time.schedule.workrulebypass.actiontext",
            "time.schedule.workrulebypass.createdby"
        ];

        this.translationService.translateMany(translationKeys).then(terms => {
            this.gridAg.addColumnDate("date", terms["common.date"], 50, true);
            this.gridAg.addColumnText("employeeNrAndName", terms["common.employee"], 75, true);
            this.gridAg.addColumnText("message", terms["time.schedule.workrulebypass.messagetext"], null, false);
            this.gridAg.addColumnText("actionText", terms["time.schedule.workrulebypass.actiontext"], 50, true);
            this.gridAg.addColumnText("createdBy", terms["time.schedule.workrulebypass.createdby"], 50, true);

            this.gridAg.options.enableRowSelection = false;
            this.gridAg.finalizeInitGrid("time.schedule.workrulebypass.warnings", true);
        });

    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.reloadData());
        this.toolbar.addInclude(this.toolbarInclude);
    }

    private loadGridData() {
        this.progressBusy = true;
        if (!this.allItemsSelection)
            this.allItemsSelection = 1;
        this.progress.startLoadingProgress([() => {
            return this.scheduleService.getWorkRuleBypassLog(this.allItemsSelection).then(data => {
                this.setData(data);
                this.progressBusy = false;
            });
        }]);
    }

    private loadSelectionTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.ChangeStatusGridAllItemsSelection, false, true, true).then((x) => {
            this.allItemsSelectionDict = x;
        });
    }

    public updateItemsSelection() {
        this.loadGridData();
    }

    private reloadData() {
        this.loadGridData();
    }
}
