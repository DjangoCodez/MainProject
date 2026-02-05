import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { Feature, TermGroup_TimePeriodType } from "../../../Util/CommonEnumerations";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { ITimeService } from "../../Time/TimeService";
import { IPayrollService } from "../../Payroll/PayrollService";
import { TimePeriodHeadGridDTO } from "../../../Common/Models/TimePeriodHeadDTO";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Terms:
    private terms: { [index: string]: string; };

    // Data
    private timePeriods: TimePeriodHeadGridDTO[];

    //@ngInject
    constructor(
        private coreService: ICoreService,
        private timeService: ITimeService,
        private payrollService: IPayrollService,
        private translationService: ITranslationService,
        private $timeout: ng.ITimeoutService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "Time.Employee.PayrollGroups", progressHandlerFactory, messagingHandlerFactory);
        this.useRecordNavigatorInEdit('payrollGroupId', 'name');
        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onBeforeSetUpGrid(() => this.loadTimePeriods())
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
            { feature: Feature.Time_Employee_PayrollGroups_Edit, loadReadPermissions: true, loadModifyPermissions: true },
        ]);
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Time_Employee_PayrollGroups_Edit].readPermission;
        this.modifyPermission = response[Feature.Time_Employee_PayrollGroups_Edit].modifyPermission;

        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();
    }

    private loadTimePeriods(): ng.IPromise<any> {
        this.timePeriods = [];
        return this.timeService.getTimePeriodHeadsForGrid(TermGroup_TimePeriodType.Payroll, true, false, false).then(x => {
            this.timePeriods = x;
        });
    }

    public setupGrid() {
        // Columns
        var keys: string[] = [
            "core.edit",
            "common.active",
            "common.name",
            "time.employee.payrollgroup.timeperiod"
        ];

        this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;

            this.gridAg.addColumnBool("isActive", terms["common.active"], 40, false, null, null, true);
            this.gridAg.addColumnText("name", terms["common.name"], null);
            this.gridAg.addColumnSelect("timePeriodHeadName", this.terms["time.employee.payrollgroup.timeperiod"], null, { displayField: "timePeriodHeadName", selectOptions: this.timePeriods, dropdownValueLabel: "name", enableHiding: true });
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.options.enableRowSelection = false;
            this.gridAg.finalizeInitGrid("time.employee.payrollgroups", false, undefined, true);
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.reloadData());
    }

    public loadGridData(useCache: boolean = true) {
        this.progress.startLoadingProgress([() => {
            return this.payrollService.getPayrollGroupsGrid(useCache).then(x => {
                return x;
            }).then(data => {
                this.setData(data);
            });
        }]);
    }

    private reloadData() {
        this.loadGridData(false);
    }

}