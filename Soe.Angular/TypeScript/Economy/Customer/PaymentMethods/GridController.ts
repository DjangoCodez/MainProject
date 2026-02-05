import { ICoreService } from "../../../Core/Services/CoreService";
import { IReportService } from "../../../Core/Services/ReportService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { ICommonCustomerService } from "../../../Common/Customer/CommonCustomerService";
import { Feature, SoeOriginType } from "../../../Util/CommonEnumerations";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    // Filters

    accountFilterOptions: Array<any> = [];

    //@ngInject
    constructor(
        private commonCustomerService: ICommonCustomerService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "Economy.Customer.PaymentMethods", progressHandlerFactory, messagingHandlerFactory);

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
            .onDoLookUp(() => this.loadLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired( () => { this.loadGridData(); });
        }

        this.flowHandler.start({ feature: Feature.Economy_Preferences_CustInvoiceSettings_PaymentMethods, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg, () => this.loadGridData());
    }

    public setupGrid() {
        // Columns
        const keys: string[] = [
            "common.name",
            "economy.common.paymentmethods.importtype",
            "economy.common.paymentmethods.paymentnr",
            "economy.common.paymentmethods.accountnr",
            "economy.common.paymentmethods.useincashsales",
            "core.edit",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnText("name", terms["common.name"], null, false);
            this.gridAg.addColumnText("sysPaymentMethodName", terms["economy.common.paymentmethods.importtype"], null, false);
            this.gridAg.addColumnText("paymentNr", terms["economy.common.paymentmethods.paymentnr"], null, false);
            this.gridAg.addColumnText("accountNr", terms["economy.common.paymentmethods.accountnr"], null, true);
            this.gridAg.addColumnBoolEx("useInCashSales", terms["economy.common.paymentmethods.useincashsales"], null, {maxWidth:150});
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.finalizeInitGrid("economy.customer.paymentmethod.paymentmethods", true);
        });
    }

    public loadGridData() {
        this.gridAg.clearData();
        this.progress.startLoadingProgress([() => {
            return this.commonCustomerService.getPaymentMethods(SoeOriginType.CustomerPayment, false, false, true, false, true).then((data) => {
                this.setData(data);
            });
        }]);
    }

    private loadLookups() {
        return this.commonCustomerService.getAccountStdsDict(true).then((x) => {
            _.forEach(x, (y: any) => {
                this.accountFilterOptions.push({ value: y.name, label: y.name })
            });
        });
    }
}