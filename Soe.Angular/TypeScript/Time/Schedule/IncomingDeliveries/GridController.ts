import { ITranslationService } from "../../../Core/Services/TranslationService";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IScheduleService } from "../ScheduleService";
import { Feature, CompanySettingType } from "../../../Util/CommonEnumerations";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { ICoreService } from "../../../Core/Services/CoreService";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    private terms: any;
    private useAccountsHierarchy: boolean;

    //@ngInject
    constructor(
        private $timeout: ng.ITimeoutService,
        private $q: ng.IQService,
        private scheduleService: IScheduleService,
        private translationService: ITranslationService,
        private coreService: ICoreService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private uiGridConstants: uiGrid.IUiGridConstants) {
        super(gridHandlerFactory, "Time.Schedule.IncomingDeliveries", progressHandlerFactory, messagingHandlerFactory);
        this.useRecordNavigatorInEdit('incomingDeliveryHeadId', 'name');
        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onBeforeSetUpGrid(() => this.loadLookups())
            .onSetUpGrid(() => this.setUpGrid())
            .onLoadGridData(() => this.loadGridData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.reloadData(); });
        }

        this.guid = this.parameters.guid;

        this.flowHandler.start({ feature: Feature.Time_Schedule_StaffingNeeds_IncomingDeliveries, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Time_Schedule_StaffingNeeds_IncomingDeliveries].readPermission;
        this.modifyPermission = response[Feature.Time_Schedule_StaffingNeeds_IncomingDeliveries].modifyPermission;

        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();
    }

    private loadLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTerms(),
            this.loadCompanySettings()]);
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];

        settingTypes.push(CompanySettingType.UseAccountHierarchy);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useAccountsHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);

        });
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.notspecified",
            "core.edit",
            "common.name",
            "common.description",
            "common.dailyrecurrencepattern.startdate",
            "common.dailyrecurrencepattern.rangetype",
            "common.dailyrecurrencepattern",
            "time.schedule.incomingdelivery.startdate",
            "time.schedule.incomingdelivery.stopdate",
            "time.schedule.incomingdelivery.nbrofoccurrences",
            "time.schedule.incomingdeliverytype.incomingdeliverytype",
            "time.schedule.shifttype.shifttype",
            "time.schedule.incomingdelivery.row.nbrofpackages",
            "time.schedule.incomingdelivery.row.offsetdays",
            "time.schedule.incomingdelivery.row.starttime",
            "time.schedule.incomingdelivery.row.stoptime",
            "time.schedule.incomingdelivery.row.length",
            "common.user.attestrole.accounthierarchy"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private setUpGrid() {
        this.gridAg.enableMasterDetail(true, null, null, true);
        this.gridAg.options.setDetailCellDataCallback((params) => {
            this.$timeout(() => {
                this.gridAg.detailOptions.enableRowSelection = false;
                this.gridAg.detailOptions.sizeColumnToFit();
                this.getIncomingDeliveryRows(params);
            });
        });

        // Master grid
        this.gridAg.addColumnText("name", this.terms["common.name"], 150, false);
        this.gridAg.addColumnText("description", this.terms["common.description"], null, true);
        this.gridAg.addColumnText("recurrenceStartsOnDescription", this.terms["common.dailyrecurrencepattern.startdate"], 75, true);
        this.gridAg.addColumnText("recurrenceEndsOnDescription", this.terms["common.dailyrecurrencepattern.rangetype"], 75, true);
        if (this.useAccountsHierarchy)
            this.gridAg.addColumnText("accountName", this.terms["common.user.attestrole.accounthierarchy"], 75, true);

        this.gridAg.addColumnText("recurrencePatternDescription", this.terms["common.dailyrecurrencepattern"], null, true);
        this.gridAg.addColumnEdit(this.terms["core.edit"], this.edit.bind(this));

        // Detals grid
        this.gridAg.detailOptions.addColumnText("name", this.terms["common.name"], 100);
        this.gridAg.detailOptions.addColumnText("description", this.terms["common.description"], null);
        this.gridAg.detailOptions.addColumnText("shiftTypeName", this.terms["time.schedule.shifttype.shifttype"], null);
        this.gridAg.detailOptions.addColumnText("typeName", this.terms["time.schedule.incomingdeliverytype.incomingdeliverytype"], null);
        this.gridAg.detailOptions.addColumnNumber("nbrOfPackages", this.terms["time.schedule.incomingdelivery.row.nbrofpackages"], 100);
        this.gridAg.detailOptions.addColumnNumber("offsetDays", this.terms["time.schedule.incomingdelivery.row.offsetdays"], 100);
        this.gridAg.detailOptions.addColumnTime("startTime", this.terms["time.schedule.incomingdelivery.row.starttime"], 100);
        this.gridAg.detailOptions.addColumnTime("stopTime", this.terms["time.schedule.incomingdelivery.row.stoptime"], 100);
        this.gridAg.detailOptions.addColumnNumber("length", this.terms["time.schedule.incomingdelivery.row.length"], 100);
        this.gridAg.detailOptions.finalizeInitGrid();

        this.gridAg.options.enableRowSelection = false;
        this.gridAg.options.noDetailPadding = true;

        this.gridAg.finalizeInitGrid("time.schedule.incomingdelivery.incomingdeliveries", true);
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.reloadData());
    }

    private getIncomingDeliveryRows(params) {
        this.scheduleService.getIncomingDeliveryRows(params.data.incomingDeliveryHeadId, false).then((rows) => {
            params.data['rows'] = rows;
            params.data['rowsLoaded'] = true;
            params.successCallback(params.data['rows']);
        });
    }
    private loadGridData(useCache: boolean = true) {
        this.progress.startLoadingProgress([() => {
            return this.scheduleService.getIncomingDeliveriesGrid(useCache).then(data => {

                for (var i = 0; i < data.length; i++) {
                    var row = data[i];
                    row['expander'] = "";
                }
                this.setData(data);
            });
        }]);
    }

    public reloadData() {
        this.loadGridData(false);
    }
}


