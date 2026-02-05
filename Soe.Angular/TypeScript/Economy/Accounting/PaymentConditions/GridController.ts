import { ICoreService } from "../../../Core/Services/CoreService";
import { IReportService } from "../../../Core/Services/ReportService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { SoeGridOptionsEvent } from "../../../Util/Enumerations";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { Feature } from "../../../Util/CommonEnumerations";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    //@ngInject
    constructor(
        private accountingService: IAccountingService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "Economy.Accounting.PaymentConditions", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify

                if (this.modifyPermission) {
                    // Send messages to TabsController
                    this.messagingHandler.publishActivateAddTab();
                }
            })
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.loadGridData(false); });
        }

        this.flowHandler.start({ feature: Feature.Economy_Preferences_PayCondition, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg, () => this.loadGridData());
    }

    public setupGrid() {
        // Columns
        const keys: string[] = [
            "common.code",
            "common.name",
            "economy.accounting.paymentcondition.days",
            "core.edit"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnText("code", terms["common.code"], null, true);
            this.gridAg.addColumnText("name", terms["common.name"], null);
            this.gridAg.addColumnText("days", terms["economy.accounting.paymentcondition.days"], null, true);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.finalizeInitGrid("economy.accounting.paymentcondition.paymentconditions", true)
        });
    }

    public loadGridData(useCache: boolean = true) {
        this.gridAg.clearData();
        this.progress.startLoadingProgress([() => {
            return this.accountingService.getPaymentConditions(useCache).then(data => {
                this.setData(data);
            });
        }]);
    }

    edit(row) {
        // Send message to TabsController
        if (this.doubleClickToEdit && (this.readPermission || this.modifyPermission)) {
            this.messagingHandler.publishEditRow(row);
        }
    }
}