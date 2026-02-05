import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { IScheduleService } from "../ScheduleService";
import { CompanySettingType, Feature } from "../../../Util/CommonEnumerations";
import { ISelectedItemsService } from "../../../Core/Services/SelectedItemsService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { ITimeService } from "../../Time/TimeService";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    hideShowInTerminal: boolean = true;
   
    //@ngInject
    constructor(
        private $timeout: ng.ITimeoutService,
        $scope: ng.IScope,
        private scheduleService: IScheduleService,
        private translationService: ITranslationService,
        private selectedItemsService: ISelectedItemsService,
        private coreService: ICoreService,
        private timeService: ITimeService,
        gridHandlerFactory: IGridHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory) {
        super(gridHandlerFactory, "Time.Schedule.ScheduleTypes", progressHandlerFactory, messagingHandlerFactory);
        this.useRecordNavigatorInEdit('timeScheduleTypeId', 'name');
        this.selectedItemsService.setup($scope, "timeScheduleTypeId", (items: number[]) => this.save(items));

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
            .onBeforeSetUpGrid(() => this.loadCompanySettings())
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData(false));
    }

    // SETUP
    loadCompanySettings() {
        var settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.PossibilityToRegisterAdditionsInTerminal);
       
        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.hideShowInTerminal = !SettingsUtility.getBoolCompanySetting(x, CompanySettingType.PossibilityToRegisterAdditionsInTerminal);
        });
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.reloadData(); });
        }

        this.flowHandler.start({ feature: Feature.Time_Preferences_TimeSettings_TimeScheduleType, loadReadPermissions: true, loadModifyPermissions: true });
        this.loadCompanySettings();
    }

    private setupGrid() {
        var keys: string[] = [
            "core.edit",
            "common.active",
            "common.code",
            "common.name",
            "common.description",
            "time.payrollproduct.payrollproducts",
            "time.schedule.scheduletype.bilagaj",
            "common.all",
            "time.schedule.scheduletype.showinterminal",
            "time.schedule.scheduletype.replacewithdeviationcause"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.gridAg.addColumnActive("isActive", terms["common.active"], 50, (params) => this.selectedItemsService.CellChanged(params));
            this.gridAg.addColumnText("code", terms["common.code"], 100);
            this.gridAg.addColumnText("name", terms["common.name"], null);
            this.gridAg.addColumnText("description", terms["common.description"], null, true);
            this.gridAg.addColumnBool("isAll", terms["common.all"], null);
            this.gridAg.addColumnBool("isBilagaJ", terms["time.schedule.scheduletype.bilagaj"], null);
            if (!this.hideShowInTerminal)
                this.gridAg.addColumnBool("showInTerminal", terms["time.schedule.scheduletype.showinterminal"], null);
            this.gridAg.addColumnText("timeDeviationCauseName", terms["time.schedule.scheduletype.replacewithdeviationcause"], null, true);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this), false);

            this.gridAg.finalizeInitGrid("time.schedule.scheduletype.scheduletypes", true, undefined, true);
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
            return this.scheduleService.getScheduleTypes(true, false, false,true).then(x => {
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
            var entity: any = this.gridAg.options.findInData((ent: any) => ent["timeScheduleTypeId"] === id);

            // Push id and active flag to array
            if (entity !== undefined) {
                dict[id] = entity.isActive;
            }
        });

        if ((dict !== undefined) && (Object.keys(dict).length > 0)) {
            this.scheduleService.updateScheduleTypesState(dict).then(result => {
                this.reloadData();
            });
        }
    }
}