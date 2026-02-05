import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IInvoiceService } from "../../../Shared/Billing/Invoices/InvoiceService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { Feature, SoeOriginType } from "../../../Util/CommonEnumerations";
import { CustomerInvoiceTemplateGridFunctions } from "../../../Util/Enumerations";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { Constants } from "../../../Util/Constants";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    private terms: { [index: string]: string; };
    private readonlyOfferPermission: boolean;
    private modifyOfferPermission: boolean;
    private readonlyOrderPermission: boolean;
    private modifyOrderPermission: boolean;
    private readonlyInvoicePermission: boolean;
    private modifyInvoicePermission: boolean;
    private types: any[];
    private invoiceTemplates: any[];

    // Functions
    buttonFunctions: any = [];

    //Footer
    gridFooterComponentUrl: any;

    //modal
    private modalInstance: any;

    //@ngInject
    constructor(private invoiceService: IInvoiceService,
        private translationService: ITranslationService,
        private coreService: ICoreService,
        private notificationService: INotificationService,
        private messagingService: IMessagingService,
        private $timeout: ng.ITimeoutService,
        private $filter: ng.IFilterService,
        $uibModal,
        private $q: ng.IQService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private urlHelperService: IUrlHelperService,
    ) {
        super(gridHandlerFactory, "Billing.Invoices.Templates", progressHandlerFactory, messagingHandlerFactory);

        this.modalInstance = $uibModal;

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))    
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData());
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        this.flowHandler.start([
            { feature: Feature.Billing_Preferences_InvoiceSettings_Templates_Edit, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Billing_Offer_Offers_Edit, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Billing_Order_Orders_Edit, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Billing_Invoice_Invoices_Edit, loadReadPermissions: true, loadModifyPermissions: true }
        ]);

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.reloadData(); });
        }

        this.gridFooterComponentUrl = this.urlHelperService.getViewUrl("gridFooter.html");
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Billing_Preferences_InvoiceSettings_Templates_Edit].readPermission;
        this.modifyPermission = response[Feature.Billing_Preferences_InvoiceSettings_Templates_Edit].modifyPermission;
        this.readonlyOfferPermission = response[Feature.Billing_Offer_Offers_Edit].readPermission;
        this.modifyOfferPermission = response[Feature.Billing_Offer_Offers_Edit].modifyPermission;
        this.readonlyOrderPermission = response[Feature.Billing_Order_Orders_Edit].readPermission;
        this.modifyOrderPermission = response[Feature.Billing_Order_Orders_Edit].modifyPermission;
        this.readonlyInvoicePermission = response[Feature.Billing_Invoice_Invoices_Edit].readPermission;
        this.modifyInvoicePermission = response[Feature.Billing_Invoice_Invoices_Edit].modifyPermission;
    }

    edit(row) {
        if(row)
            this.openEdit(row.originType, row);
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.reloadData());
    }

    public setupGrid() {
        var keys: string[] = [
            "common.name",
            "common.customer",
            "common.type",
            "common.offer",
            "common.order",
            "common.customerinvoice",
            "billing.invoices.templates.createoffer",
            "billing.invoices.templates.createorder",
            "billing.invoices.templates.createinvoice",
            "core.aggrid.totals.total",
            "core.aggrid.totals.filtered"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;

            this.types = [];
            this.types.push({ id: this.terms["common.offer"], name: this.terms["common.offer"] });
            this.types.push({ id: this.terms["common.order"], name: this.terms["common.order"] });
            this.types.push({ id: this.terms["common.customerinvoice"], name: this.terms["common.customerinvoice"] });

            this.gridAg.options.enableRowSelection = false;

            this.gridAg.addColumnText("name", this.terms["common.name"], null);
            this.gridAg.addColumnText("actorName", this.terms["common.customer"], null);
            this.gridAg.addColumnSelect("typeName", this.terms["common.type"], null, { displayField: "typeName", selectOptions: this.types, dropdownValueLabel: "name" });
            this.gridAg.addColumnIcon(null, "", 40, { icon: "fal fa-pencil iconEdit", onClick: this.edit.bind(this), showIcon: (row) => row.hasEditPermission })

             this.gridAg.finalizeInitGrid("common.customer.customer.templates", true);

            //Functions
            if (this.modifyOfferPermission)
                this.buttonFunctions.push({ id: CustomerInvoiceTemplateGridFunctions.CreateOffer, name: terms["billing.invoices.templates.createoffer"] });
            if (this.modifyOrderPermission)
                this.buttonFunctions.push({ id: CustomerInvoiceTemplateGridFunctions.CreateOrder, name: terms["billing.invoices.templates.createorder"] });
            if (this.modifyInvoicePermission)
                this.buttonFunctions.push({ id: CustomerInvoiceTemplateGridFunctions.CreateInvoice, name: terms["billing.invoices.templates.createinvoice"] });
        });
    }

    private executeButtonFunction(option) {
        switch (option.id) {
            case CustomerInvoiceTemplateGridFunctions.CreateOffer:
                this.openEdit(SoeOriginType.Offer, null);
                break;
            case CustomerInvoiceTemplateGridFunctions.CreateOrder:
                this.openEdit(SoeOriginType.Order, null);
                break;
            case CustomerInvoiceTemplateGridFunctions.CreateInvoice:
                this.openEdit(SoeOriginType.CustomerInvoice, null);
                break;
        }
    }

    public openEdit(originType: SoeOriginType, row: any) {
        this.messagingService.publish(Constants.EVENT_OPEN_CUSTOMERINVOICE, { originType: originType, row: row});
    }

    public loadGridData() {
        this.progress.startLoadingProgress([() => {
            return this.invoiceService.getInvoiceTemplates().then(x => {
                this.invoiceTemplates = x;
                _.forEach(this.invoiceTemplates, (dto) => {
                    switch (dto.originType) {
                        case SoeOriginType.Offer:
                            dto.typeName = this.terms["common.offer"];
                            dto['hasEditPermission'] = this.modifyOfferPermission;
                            break;
                        case SoeOriginType.Order:
                            dto.typeName = this.terms["common.order"];
                            dto['hasEditPermission'] = this.modifyOrderPermission;
                            break;
                        case SoeOriginType.CustomerInvoice:
                            dto.typeName = this.terms["common.customerinvoice"];
                            dto['hasEditPermission'] = this.modifyInvoicePermission;
                            break;
                        default:
                            break;
                    }
                });
                return this.invoiceTemplates;
            }).then(data => {
                this.setData(data);
            });
        }]);
    }

    private reloadData() {        
        this.loadGridData();
    }
}