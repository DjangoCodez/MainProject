import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { ITimeService } from "../TimeService";
import { Feature } from "../../../Util/CommonEnumerations";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    //@ngInject
    constructor(
        private $timeout: ng.ITimeoutService,
        private $scope: ng.IScope,
        private timeService: ITimeService,
        private translationService: ITranslationService,
        gridHandlerFactory: IGridHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory) {
        super(gridHandlerFactory, "Time.Time.TimeAbsenceRules", progressHandlerFactory, messagingHandlerFactory);
        this.useRecordNavigatorInEdit('timeAbsenceRuleHeadId', 'name');


        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify;
                if (this.modifyPermission) {
                    // Send messages to TabsController
                    this.messagingHandler.publishActivateAddTab();
                }
            })
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

        this.flowHandler.start({ feature: Feature.Time_Preferences_TimeSettings_TimeAbsenceRule, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private setupGrid() {
        let keys: string[] = [
            "core.edit",
            "common.name",
            "common.description",
            "common.type",
            "common.timecode",
            "common.employeegroups",
        ];
        
        return this.translationService.translateMany(keys).then(terms => {
            this.gridAg.addColumnText("name", terms["common.name"], 100);
            this.gridAg.addColumnText("description", terms["common.description"], null, true);
            this.gridAg.addColumnText("typeName", terms["common.type"], null, true);
            this.gridAg.addColumnText("timeCodeName", terms["common.timecode"], null, true);
            this.gridAg.addColumnText("employeeGroupNames", terms["common.employeegroups"], null, true);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this), false);
            this.gridAg.finalizeInitGrid("time.time.timeabsencerules.timeabsencerules", true);
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.reloadData());
    }

    // SERVICE CALLS   
    
    public loadGridData(useCache: boolean) {
        this.gridAg.clearData();
        this.progress.startLoadingProgress([() => {
            return this.timeService.getTimeAbsenceRules().then(x => {
                this.setData(x);
            });
        }]);
    }

    private reloadData() { 
        this.loadGridData(false);
    }

    // EVENTS   

}