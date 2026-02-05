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
import { SoeTimeCodeType, Feature } from "../../../Util/CommonEnumerations";
import { ISelectedItemsService } from "../../../Core/Services/SelectedItemsService";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    //@ngInject
    constructor(
        $scope: ng.IScope,
        private timeService: ITimeService,
        private translationService: ITranslationService,
        private selectedItemsService: ISelectedItemsService,
        gridHandlerFactory: IGridHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory) {
        super(gridHandlerFactory, "Time.Time.TimeCodeBreaks", progressHandlerFactory, messagingHandlerFactory);
        this.useRecordNavigatorInEdit('timeCodeId', 'name');
        this.selectedItemsService.setup($scope, "timeCodeId", (items: number[]) => this.save(items));

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
    }

    // SETUP

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.reloadData(); });
        }

        this.flowHandler.start({ feature: Feature.Time_Preferences_TimeSettings_TimeCodeBreak, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private setupGrid() {
        const keys: string[] = [
            "core.edit",
            "common.active",
            "common.code",
            "common.name",
            "common.description",
            "time.time.timecodebreaks.template",
            "time.time.timecodebreaks.breakgroup",
            "time.time.timecodebreaks.employeegroup"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.gridAg.addColumnActive("isActive", terms["common.active"], 50, (params) => this.selectedItemsService.CellChanged(params));
            this.gridAg.addColumnText("code", terms["common.code"], 100);
            this.gridAg.addColumnText("name", terms["common.name"], null);
            this.gridAg.addColumnText("description", terms["common.description"], null, true);
            this.gridAg.addColumnText("templateText", terms["time.time.timecodebreaks.template"], 50, true);
            this.gridAg.addColumnText("timeCodeBreakGroupName", terms["time.time.timecodebreaks.breakgroup"], 100, true);
            this.gridAg.addColumnText("timeCodeBreakEmployeeGroupNames", terms["time.time.timecodebreaks.employeegroup"], null, true);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this), false);

            this.gridAg.finalizeInitGrid("time.time.timecodebreaks.timecodebreaks", true, undefined, true);
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.reloadData(), true, () => this.selectedItemsService.Save(), () => { return this.saveButtonIsDisabled() });
    }

    private saveButtonIsDisabled(): boolean {
        return !this.selectedItemsService.SelectedItemsExist();
    }

    // SERVICE CALLS   

    public loadGridData(useCache: boolean) {
        this.progress.startLoadingProgress([() => {
            return this.timeService.getTimeCodesGrid(SoeTimeCodeType.Break, false, true).then(x => {
                return x;
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
            var entity: any = this.gridAg.options.findInData((ent: any) => ent["timeCodeId"] === id);

            // Push id and active flag to array
            if (entity !== undefined) {
                dict[id] = entity.isActive;
            }
        });

        if ((dict !== undefined) && (Object.keys(dict).length > 0)) {
            this.timeService.updateTimeCodesState(dict).then(result => {
                this.reloadData();
            });
        }
    }
}