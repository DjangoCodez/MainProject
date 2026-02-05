import { IEmployeeService } from "../EmployeeService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../Util/Enumerations";
import { Feature } from "../../../Util/CommonEnumerations";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    gridFooterComponentUrl: any;

    //@ngInject
    constructor(
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private employeeService: IEmployeeService,
        private notificationService: INotificationService,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory) {

        super(gridHandlerFactory, "time.employee.cardnumber.cardnumbers", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onSetUpGrid(() => this.setUpGrid())
            .onLoadGridData(() => this.loadGridData())

        // Setup footer information
        this.gridFooterComponentUrl = this.urlHelperService.getViewUrl("info.html");
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.guid = parameters.guid;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.reloadData(); });
        }

        this.flowHandler.start({ feature: Feature.Time_Employee_CardNumbers, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Time_Employee_CardNumbers].readPermission;
        this.modifyPermission = response[Feature.Time_Employee_CardNumbers].modifyPermission;

        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();
    }

    public setUpGrid() {
        // Columns
        const keys: string[] = [
            "time.employee.cardnumber.number",
            "time.employee.employeenumber",
            "time.employee.name"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnText("cardNumber", terms["time.employee.cardnumber.number"], 125);
            this.gridAg.addColumnText("employeeNumber", terms["time.employee.employeenumber"], 125);
            this.gridAg.addColumnText("employeeName", terms["time.employee.name"], null);
            if (this.modifyPermission)
                this.gridAg.addColumnDelete(terms["core.delete"], this.initDeleteRow.bind(this));

            this.gridAg.options.enableRowSelection = false;
            this.gridAg.finalizeInitGrid("time.employee.cardnumber.cardnumbers", true);

        });
    }

    public loadGridData() {
        this.progress.startLoadingProgress([() => {
            return this.employeeService.getCardNumbers()
                .then(data => {
                    this.setData(data);
                });
        }]);
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.reloadData());
    }

    private reloadData() {
        this.loadGridData();
    }

    protected initDeleteRow(row) {
        const id: number = row['employeeId'];

        // Show verification dialog
        const keys: string[] = [
            "core.warning",
            "time.employee.cardnumber.deletewarning"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            const message: string = terms["time.employee.cardnumber.deletewarning"].format(row['cardNumber'], row['employeeName']);
            this.notificationService.showDialog(terms["core.warning"], message, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.YesNo).result.then(val => {
                if (val) {
                    this.progress.startDeleteProgress((completion) => {
                        this.employeeService.deleteCardNumber(id).then((result) => {
                            if (result.success) {
                                completion.completed(null, true);
                                this.reloadData();
                            } else {
                                completion.failed(result.errorMessage);
                            }
                        }, error => {
                            completion.failed(error.message);
                        });
                    });
                }
            });
        });
    }
}
