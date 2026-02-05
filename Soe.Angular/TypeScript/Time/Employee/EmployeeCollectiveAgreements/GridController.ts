import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IEmployeeService } from "../EmployeeService";
import { Feature } from "../../../Util/CommonEnumerations";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { ISelectedItemsService } from "../../../Core/Services/SelectedItemsService";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    //@ngInject
    constructor(
        $scope: ng.IScope,
        private employeeService: IEmployeeService,
        private selectedItemsService: ISelectedItemsService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {

        super(gridHandlerFactory, "Time.Employee.EmployeeCollectiveAgreement", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onSetUpGrid(() => this.setUpGrid())
            .onLoadGridData(() => this.loadGridData())
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.guid = parameters.guid;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.reloadData(); });
        }

        this.flowHandler.start({ feature: Feature.Time_Employee_EmployeeCollectiveAgreements, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Time_Employee_EmployeeCollectiveAgreements].readPermission;
        this.modifyPermission = response[Feature.Time_Employee_EmployeeCollectiveAgreements].modifyPermission;

        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.reloadData());
    }

    public setUpGrid() {
        // Columns
        const keys: string[] = [
            "common.active",
            "common.name",
            "common.description",
            "common.code",
            "common.externalcode",
            "time.employee.employeegroup.employeegroup",
            "time.employee.payrollgroup.payrollgroup",
            "time.employee.vacationgroup.vacationgroup",
            "time.employee.employeetemplate.employeetemplates",
            "core.edit"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnActive("isActive", terms["common.active"], 70);
            this.gridAg.addColumnText("code", terms["common.code"], 60);
            this.gridAg.addColumnText("name", terms["common.name"], null);
            this.gridAg.addColumnText("description", terms["common.description"], null);
            this.gridAg.addColumnText("externalCode", terms["common.externalcode"], 60);
            this.gridAg.addColumnText("employeeGroupName", terms["time.employee.employeegroup.employeegroup"], null);
            this.gridAg.addColumnText("payrollGroupName", terms["time.employee.payrollgroup.payrollgroup"], null);
            this.gridAg.addColumnText("vacationGroupName", terms["time.employee.vacationgroup.vacationgroup"], null);
            this.gridAg.addColumnText("employeeTemplateNames", terms["time.employee.employeetemplate.employeetemplates"], null);
            if (this.modifyPermission)
                this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.options.enableRowSelection = false;
            this.gridAg.finalizeInitGrid("time.employee.employeecollectiveagreement.employeecollectiveagreement", false, undefined, true);
        });
    }

    private reloadData() {
        this.loadGridData();
    }

    public loadGridData() {
        this.progress.startLoadingProgress([() => {
            return this.employeeService.getEmployeeCollectiveAgreementsGrid().then(data => {
                this.setData(data);
            });
        }]);
    }
}