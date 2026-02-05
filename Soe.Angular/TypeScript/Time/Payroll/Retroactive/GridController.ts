import { ITranslationService } from "../../../Core/Services/TranslationService"; import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IPayrollService } from "../PayrollService";
import { Feature } from "../../../Util/CommonEnumerations";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    private terms: any;

    //@ngInject
    constructor(
        private payrollService: IPayrollService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "Time.Payroll.Retroactive", progressHandlerFactory, messagingHandlerFactory);
        this.onTabActivetedAndModified(() => this.reloadData());

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onBeforeSetUpGrid(() => this.loadTerms())
            .onSetUpGrid(() => this.setUpGrid())
            .onLoadGridData(() => this.loadGridData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;
        this.guid = this.parameters.guid;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.reloadData(); });
        }

        this.flowHandler.start({ feature: Feature.Time_Payroll_Retroactive, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Time_Payroll_Retroactive].readPermission;
        this.modifyPermission = response[Feature.Time_Payroll_Retroactive].modifyPermission;

        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.edit",
            "common.name",
            "common.startdate",
            "common.stopdate",
            "common.status",
            "time.time.timeperiod.timeperiodhead",
            "common.period",
            "time.time.timeperiod.paymentdate",
            "time.time.attest.nrofemployees",
            "common.note",
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private setUpGrid() {
        this.gridAg.addColumnText("name", this.terms["common.name"], 140, false);
        this.gridAg.addColumnDate("dateFrom", this.terms["common.startdate"], 75);
        this.gridAg.addColumnDate("dateTo", this.terms["common.stopdate"], 75);
        this.gridAg.addColumnText("statusName", this.terms["common.status"], 50, false);
        this.gridAg.addColumnDate("timePeriodPaymentDate", this.terms["time.time.timeperiod.paymentdate"], 75, false);
        this.gridAg.addColumnText("timePeriodName", this.terms["common.period"], 80, false);
        this.gridAg.addColumnText("timePeriodHeadName", this.terms["time.time.timeperiod.timeperiodhead"], 50, false);
        this.gridAg.addColumnNumber("nrOfEmployees", this.terms["time.time.attest.nrofemployees"], 60);
        this.gridAg.addColumnText("note", this.terms["common.note"], null, false);
        this.gridAg.addColumnEdit(this.terms["core.edit"], this.edit.bind(this));

        this.gridAg.options.enableRowSelection = false;
        this.gridAg.finalizeInitGrid("time.payroll.retroactive", true);

    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.reloadData());
    }

    private loadGridData(useCache: boolean = true) {

        this.progress.startLoadingProgress([() => {
            return this.payrollService.getRetroactivePayrolls(useCache).then(data => {
                this.setData(data);
            });
        }]);
    }

    private reloadData() {
        this.loadGridData(false);
    }
}
