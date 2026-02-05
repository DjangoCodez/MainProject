import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IEmployeeService } from "../EmployeeService";
import { Feature } from "../../../Util/CommonEnumerations";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    //@ngInject
    constructor(
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private employeeService: IEmployeeService,
        gridHandlerFactory: IGridHandlerFactory) {

        super(gridHandlerFactory, "Time.Employee.Vehicles", progressHandlerFactory, messagingHandlerFactory);
        this.useRecordNavigatorInEdit('employeeVehicleId', 'description');

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData());
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.guid = parameters.guid;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.reloadData(); });
        }

        this.flowHandler.start({ feature: Feature.Time_Employee_Vehicles, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Time_Employee_Vehicles].readPermission;
        this.modifyPermission = response[Feature.Time_Employee_Vehicles].modifyPermission;

        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();
    }

    public setupGrid() {
        // Columns
        var keys: string[] = [
            "time.employee.employeenumbershort",
            "time.employee.name",
            "time.employee.vehicle.licenseplatenumbershort",
            "time.employee.vehicle.makeandmodel",
            "common.fromdate",
            "common.todate",
            "time.employee.vehicle.price",
            "time.employee.vehicle.equipmentsum",
            "time.employee.vehicle.netsalarydeduction",
            "time.employee.vehicle.currenttaxablevalue",
            "core.edit"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnText("employeeNr", terms["time.employee.employeenumbershort"], null, true);
            this.gridAg.addColumnText("employeeName", terms["time.employee.name"], null, true);
            this.gridAg.addColumnText("licensePlateNumber", terms["time.employee.vehicle.licenseplatenumbershort"], null);
            this.gridAg.addColumnText("vehicleMakeAndModel", terms["time.employee.vehicle.makeandmodel"], null, true);
            this.gridAg.addColumnDate("fromDate", terms["common.fromdate"], null, true);
            this.gridAg.addColumnDate("toDate", terms["common.todate"], null, true);
            this.gridAg.addColumnNumber("price", terms["time.employee.vehicle.price"], null, { enableHiding: true });
            this.gridAg.addColumnNumber("equipmentSum", terms["time.employee.vehicle.equipmentsum"], null, { enableHiding: true });
            this.gridAg.addColumnNumber("netSalaryDeduction", terms["time.employee.vehicle.netsalarydeduction"], null, { enableHiding: true });
            this.gridAg.addColumnNumber("taxableValue", terms["time.employee.vehicle.currenttaxablevalue"], null, { enableHiding: true });

            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.options.enableRowSelection = false;
            this.gridAg.finalizeInitGrid("time.employee.vehicle.employeevehicles", true);
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.reloadData());
    }

    // Load data
    public loadGridData(useCache: boolean = false) {
        this.progress.startLoadingProgress([() => {
            return this.employeeService.getEmployeeVehicles(true, true, true, true).then(data => {
                this.setData(data);
            });
        }]);
    }

    private reloadData() {
        this.loadGridData();
    }
}