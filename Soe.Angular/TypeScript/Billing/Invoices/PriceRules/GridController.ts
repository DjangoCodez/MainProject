import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IInvoiceService } from "../../../Shared/Billing/Invoices/InvoiceService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { Feature } from "../../../Util/CommonEnumerations";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    //modal
    private modalInstance: any;

    pricelists: any[];
    wholesellers: any[];

    //@ngInject
    constructor(private invoiceService: IInvoiceService,
        private translationService: ITranslationService,
        $uibModal,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory
    ) {
        super(gridHandlerFactory, "Billing.Invoices.PriceRules", progressHandlerFactory, messagingHandlerFactory);

        this.modalInstance = $uibModal;

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify;
                if (this.modifyPermission) {
                    // Send messages to TabsController
                    this.messagingHandler.publishActivateAddTab();
                }
            })
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onBeforeSetUpGrid(() => this.loadCompanyPricelists())
            .onBeforeSetUpGrid(() => this.loadWholesellers())
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData());
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;
        this.guid = this.parameters.guid;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(() => { this.reloadData(); });
        }

        this.flowHandler.start({ feature: Feature.Billing_Preferences_InvoiceSettings_PriceRules, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private loadCompanyPricelists(): ng.IPromise<any> {
        return this.invoiceService.getPriceListsGrid().then((x) => {
            this.pricelists = [];

            _.forEach(x, (row) => {
                this.pricelists.push({ id: row.name, value: row.name });
            });
        });
    }

    private loadWholesellers(): ng.IPromise<any> {
        return this.invoiceService.getCompanyPriceRules().then((x) => {
            this.wholesellers = [];

            _.forEach(x, (row) => {
                if (!_.find(this.wholesellers, { value: row.sysWholesellerName }))
                    this.wholesellers.push({ id: row.sysWholesellerName, value: row.sysWholesellerName });
            });
        });
    }

    edit(row) {
        // Send message to TabsController        
        if (this.readPermission || this.modifyPermission)
            this.messagingHandler.publishEditRow(row);
    }

    public setupGrid() {
        // Columns
        const keys: string[] = [
            "billing.invoices.pricerules.pricelisttypename",
            "billing.invoices.pricerules.syswholesellername",
            "common.date",
            "core.edit"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnSelect("priceListTypeName", terms["billing.invoices.pricerules.pricelisttypename"], 40, { displayField: "priceListTypeName", selectOptions: this.pricelists });
            this.gridAg.addColumnSelect("sysWholesellerName", terms["billing.invoices.pricerules.syswholesellername"], 40, { displayField: "sysWholesellerName", selectOptions: this.wholesellers });
            this.gridAg.addColumnDate("date", terms["common.date"], 15);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this), false);

            this.gridAg.finalizeInitGrid("billing.invoices.pricerules.pricerules", true);
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.reloadData());
    }

    public loadGridData() {
        this.progress.startLoadingProgress([() => {
            return this.invoiceService.getCompanyPriceRules().then((x) => {
                this.setData(x);
            });
        }]);
    }

    private reloadData() {
        this.loadGridData();
    }


}