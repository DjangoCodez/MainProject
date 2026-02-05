import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { Feature, CompanySettingType, UserSettingType, TermGroup, TimeTerminalType, TimeTerminalSettingType } from "../../../Util/CommonEnumerations";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { Constants } from "../../../Util/Constants";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";
import { ITimeService } from "../TimeService";
import { CalendarUtility } from "../../../Util/CalendarUtility";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Terms:
    private terms: any;

    // Data
    private timeTerminalTypes: SmallGenericType[];

    //@ngInject
    constructor(
        private coreService: ICoreService,
        private timeService: ITimeService,
        private translationService: ITranslationService,
        private $filter: ng.IFilterService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "Time.Time.TimeTerminals", progressHandlerFactory, messagingHandlerFactory);
        this.useRecordNavigatorInEdit('timeTerminalId', 'name');
        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onBeforeSetUpGrid(() => this.loadTimeTerminalTypes())
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData());
    }

    public onInit(parameters: any) {
        this.parameters = parameters;
        this.guid = parameters.guid;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.onTabActivetedAndModified(() => {
                this.reloadData();
            });
        }

        this.flowHandler.start([
            { feature: Feature.Time_Preferences_TimeSettings_TimeTerminals_Edit, loadReadPermissions: true, loadModifyPermissions: true },
        ]);
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Time_Preferences_TimeSettings_TimeTerminals_Edit].readPermission;
        this.modifyPermission = response[Feature.Time_Preferences_TimeSettings_TimeTerminals_Edit].modifyPermission;

        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();
    }

    private loadTimeTerminalTypes(): ng.IPromise<any> {
        this.timeTerminalTypes = [];
        return this.coreService.getTermGroupContent(TermGroup.TimeTerminalType, false, true).then(x => {
            this.timeTerminalTypes = x;
        });
    }

    public setupGrid() {
        // Columns
        const keys: string[] = [
            "core.edit",
            "common.name",
            "common.dashboard.timeterminal.neversynced",
            "time.time.timeterminal.active",
            "time.time.timeterminal.registeredshort",
            "time.time.timeterminal.typename",
            "time.time.timeterminal.terminalversion",
            "time.time.timeterminal.terminaldbschemaversion",
            "time.time.timeterminal.lastsync",
            "time.time.timeterminal.neversynced",
            "time.time.timeterminal.timesincelastsync"
        ];

        this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;

            this.gridAg.addColumnBool("isActive", terms["time.time.timeterminal.active"], 40, false, null, null, true);
            this.gridAg.addColumnBool("registered", terms["time.time.timeterminal.registeredshort"], 40);
            this.gridAg.addColumnText("name", terms["common.name"], null);
            this.gridAg.addColumnSelect("typeName", this.terms["time.time.timeterminal.typename"], 150, { displayField: "typeName", selectOptions: this.timeTerminalTypes, dropdownValueLabel: "name", enableHiding: true });
            this.gridAg.addColumnText("terminalVersion", terms["time.time.timeterminal.terminalversion"], 100);
            this.gridAg.addColumnText("terminalDbSchemaVersion", terms["time.time.timeterminal.terminaldbschemaversion"], 100);
            this.gridAg.addColumnDateTime("lastSync", terms["time.time.timeterminal.lastsync"], 100);
            this.gridAg.addColumnShape("lastSyncStateColor", null, 50, { shape: Constants.SHAPE_CIRCLE, toolTipField: "syncStateTooltip", showIconField: "lastSyncStateColor" });
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.finalizeInitGrid("time.time.timeterminals", true, undefined, true);
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.reloadData());
    }

    public loadGridData() {
        this.progress.startLoadingProgress([() => {
            return this.timeService.getTimeTerminals(TimeTerminalType.Unknown, false, false, false, true, false, true, true).then(x => {
                _.forEach(x, row => {
                    if (row.type === TimeTerminalType.WebTimeStamp || row.type === TimeTerminalType.GoTimeStamp) {
                        row.lastSyncStateColor = ""; // None
                    } else if (!row.lastSync) {
                        row.syncStateTooltip = this.terms["common.dashboard.timeterminal.neversynced"];
                        row.lastSyncStateColor = "#D3D3D3"; // Gray
                    } else {
                        let now: Date = new Date();
                        let diffMinutes = now.diffMinutes(row.lastSync);
                        let diffSec = diffMinutes * 60;

                        // Get sync interval for current terminal
                        let syncInterval: number = 900; // Default 15 minutes
                        let setting = _.find(row.timeTerminalSettings, s => s.type === TimeTerminalSettingType.SyncInterval);
                        if (setting && setting.intData)
                            syncInterval = setting.intData;

                        row.syncStateTooltip = this.terms["time.time.timeterminal.timesincelastsync"].format(CalendarUtility.minutesToTimeSpan(diffMinutes));
                        if (diffSec < syncInterval) {
                            row.lastSyncStateColor = "#00FF00"; // Green
                        } else if (diffSec < (syncInterval * 3)) {
                            row.lastSyncStateColor = "#FFFF00"; // Yellow
                        } else {
                            row.lastSyncStateColor = "#FF0000"; // Red
                        }
                    }
                });

                return x;
            }).then(data => {
                this.setData(data);
            });
        }]);
    }

    private reloadData() {
        this.loadGridData();
    }
}