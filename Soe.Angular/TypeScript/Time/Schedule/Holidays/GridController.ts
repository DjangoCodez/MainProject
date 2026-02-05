import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IScheduleService } from "../ScheduleService";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { Feature } from "../../../Util/CommonEnumerations";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    dayTypes: any[];
    sysHolidayTypes: any[];

    //@ngInject
    constructor(
        coreService: ICoreService,
        private scheduleService: IScheduleService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "Time.Schedule.Holidays", progressHandlerFactory, messagingHandlerFactory);
        this.useRecordNavigatorInEdit('holidayId', 'name');
        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            //.onDoLookUp(() => this.loadDayTypesDict())
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

        this.flowHandler.start({ feature: Feature.Time_Preferences_ScheduleSettings_Holidays, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Time_Preferences_ScheduleSettings_Holidays].readPermission;
        this.modifyPermission = response[Feature.Time_Preferences_ScheduleSettings_Holidays].modifyPermission;

        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();
    }

    private setUpGrid() {
        var translationKeys: string[] = [
            "common.name",
            "time.schedule.daytype.date",
            "time.schedule.daytype.daytype",
            "common.description",
            "time.schedule.daytype.sysholidaytype",
            "core.edit"
        ];

        this.translationService.translateMany(translationKeys).then((terms) => {
            this.gridAg.addColumnText("name", terms["common.name"], 150);
            this.gridAg.addColumnDate("date", terms["time.schedule.daytype.date"], 75);
            this.gridAg.addColumnSelect("dayTypeName", terms["time.schedule.daytype.daytype"], 100, { displayField: "dayTypeName", selectOptions: null, populateFilterFromGrid: true });
            this.gridAg.addColumnText("description", terms["common.description"], 100);
            this.gridAg.addColumnSelect("sysHolidayTypeName", terms["time.schedule.daytype.sysholidaytype"], 100, { displayField: "sysHolidayTypeName", selectOptions: null, populateFilterFromGrid: true });
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.options.enableRowSelection = false;
            this.gridAg.finalizeInitGrid("billing.invoices.deliverycondition.deliverycondition", true);
        });

    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.reloadData());
    }

    private loadGridData(useCache: boolean = true) {

        this.progress.startLoadingProgress([() => {
            return this.scheduleService.getHolidays().then((data) => {
                this.setData(data);
            });
        }]);
    }

    //private loadDayTypesDict(): ng.IPromise<any> {
    //    this.dayTypes = [];
    //    return this.scheduleService.getDayTypesDict(false).then((x) => {
    //        _.forEach(x, (y: any) => {
    //            this.dayTypes.push({ value: y.name, label: y.name })
    //        });
    //    });
    //}

    //private loadSysDayHolidayTypes(): ng.IPromise<any> {
    //    this.sysHolidayTypes.push({ id: 0, name: "" });
    //    return this.scheduleService.getSysHolidayTypes().then((x) => {
    //        _.forEach(x, (y: any) => {
    //            this.sysHolidayTypes.push({ value: y.sysHolidayTypeId, label: y.name })
    //        });
    //    }); 
    //}

    private reloadData() {
        this.loadGridData(false);
    }
}
