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

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    //@ngInject
    constructor(
        private employeeService: IEmployeeService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {

        super(gridHandlerFactory, "Time.Employee.EmployeeTemplate", progressHandlerFactory, messagingHandlerFactory);

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

        this.flowHandler.start({ feature: Feature.Time_Employee_EmployeeTemplates, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Time_Employee_EmployeeTemplates].readPermission;
        this.modifyPermission = response[Feature.Time_Employee_EmployeeTemplates].modifyPermission;

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
            "core.edit",
            "time.employee.employeecollectiveagreement.employeecollectiveagreement"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnActive("isActive", terms["common.active"], 75);
            this.gridAg.addColumnText("employeeCollectiveAgreementName", terms["time.employee.employeecollectiveagreement.employeecollectiveagreement"], null);
            this.gridAg.addColumnText("code", terms["common.code"], 50);
            this.gridAg.addColumnText("name", terms["common.name"], null);
            this.gridAg.addColumnText("description", terms["common.description"], null);
            this.gridAg.addColumnText("externalCode", terms["common.externalcode"], 50);
            if (this.modifyPermission)
                this.gridAg.addColumnIcon(null, "", 40, { icon: "fal fa-pencil iconEdit", onClick: this.edit.bind(this) })

            this.gridAg.options.enableRowSelection = false;
            this.gridAg.finalizeInitGrid("time.employee.employeetemplate.employeetemplate", true, undefined, true);
        });
    }

    private reloadData() {
        this.loadGridData();
    }

    public loadGridData() {
        this.progress.startLoadingProgress([() => {
            return this.employeeService.getEmployeeTemplatesGrid().then(data => {
                this.setData(data);
            });
        }]);
    }
}