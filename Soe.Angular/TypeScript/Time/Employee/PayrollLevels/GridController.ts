import { ITranslationService } from "../../../Core/Services/TranslationService";
import { Feature } from "../../../Util/CommonEnumerations";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IPayrollService } from "../../Payroll/PayrollService";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {


    // Toolbar
    private toolbarInclude: any;

    //@ngInject
    constructor(
        private translationService: ITranslationService,
        private urlHelperService: IUrlHelperService,
        private $timeout: ng.ITimeoutService,
        private payrollService: IPayrollService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {

        super(gridHandlerFactory, "Time.Employee.PayrollLevels", progressHandlerFactory, messagingHandlerFactory);
        this.useRecordNavigatorInEdit('payrollLevelId', 'name');
        this.toolbarInclude = urlHelperService.getViewUrl("gridHeader.html");

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onSetUpGrid(() => this.setUpGrid())
            .onLoadGridData(() => this.loadGridData());

    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.guid = parameters.guid;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.reloadData(); });
        }

        this.flowHandler.start({
            feature: Feature.Time_Employee_PayrollLevels, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Time_Employee_PayrollLevels].readPermission;
        this.modifyPermission = response[Feature.Time_Employee_PayrollLevels].modifyPermission;

        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.reloadData());
        this.toolbar.addInclude(this.toolbarInclude);
    }

    public setUpGrid() {
        // Columns
        var keys: string[] = [
            "common.code",
            "common.name",
            "common.active",
            "common.description",
            "common.externalcode",
            "core.edit"
        ];

        this.translationService.translateMany(keys).then((terms) => {
           this.gridAg.addColumnBool("isActive", terms["common.active"], 40, false, null, null, true);
           this.gridAg.addColumnText("name", terms["common.name"], null);
           this.gridAg.addColumnText("description", terms["common.description"], null);
           this.gridAg.addColumnText("code", terms["common.code"], null);
           this.gridAg.addColumnText("externalCode", terms["common.externalcode"], null);
           this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

           this.gridAg.options.enableRowSelection = false;
            this.gridAg.finalizeInitGrid("time.employee.payrolllevels", true, undefined, true);
        });
    }

    private reloadData() {
        this.loadGridData();
    }
    
    public loadGridData() {
        this.progress.startLoadingProgress([() => {
            return this.payrollService.getPayrollLevels().then(data => {
                this.setData(data);

            });
        }]);
       
    }
}
