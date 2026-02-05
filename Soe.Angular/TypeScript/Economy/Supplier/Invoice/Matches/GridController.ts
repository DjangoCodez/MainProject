import { ICoreService } from "../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { ToolBarUtility, ToolBarButton } from "../../../../Util/ToolBarUtility";
import { IconLibrary, SoeGridOptionsEvent, InsecureDebtsButtonFunctions, SupplierInvoiceAttestFlowButtonFunctions, SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../Util/Enumerations";
import { GridEvent } from "../../../../Util/SoeGridOptions";
import { ISupplierService } from "../../../../Shared/Economy/Supplier/SupplierService";
import { ISmallGenericType } from "../../../../Scripts/TypeLite.Net4";
import { TabMessage } from "../../../../Core/Controllers/TabsControllerBase1";
import { Feature, SoeOriginType, TermGroup_BillingType } from "../../../../Util/CommonEnumerations";
import { Constants } from "../../../../Util/Constants";
import { EditController as InvoiceEditController } from "../../../../Shared/Economy/Supplier/Invoices/EditController";
import { EditController as PaymentEditController } from "../../../../Shared/Economy/Supplier/Payments/EditController";
import { GridControllerBase2Ag } from "../../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../../Core/ICompositionGridController";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../../Core/Handlers/gridhandlerfactory";
import { IToolbarFactory } from "../../../../Core/Handlers/ToolbarFactory";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    public gridHeaderComponentUrl;
    public gridFooterComponentUrl;
    public actors: ISmallGenericType[];
    public types: any[];

    public searchModel: any;
    public selectedActor: any;
    private terms: any;
    public selectedRow: any;
    //@ngInject
    constructor($http,
        $templateCache,
        $timeout: ng.ITimeoutService,
        $uibModal,
        private $filter: ng.IFilterService,
        coreService: ICoreService,
        private supplierService: ISupplierService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private urlHelperService: IUrlHelperService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private $q: ng.IQService) {

        super(gridHandlerFactory, "economy.supplier.invoice.matches.matches", progressHandlerFactory, messagingHandlerFactory);

        this.gridHeaderComponentUrl = this.urlHelperService.getViewUrl("searchHeader.html");

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify

                if (this.modifyPermission) {
                    // Send messages to TabsController
                    this.messagingHandler.publishActivateAddTab();
                }
            })
            .onDoLookUp(() => this.doLookups())
            .onSetUpGrid(() => this.setupGrid())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onLoadGridData(() => this.setData(""));


        /*super("Soe.Economy.Supplier.Invoice.Matches", "economy.supplier.invoice.matches.matches", Feature.Economy_Supplier_Invoice_Matches, $http, $templateCache, $timeout, $uibModal, coreService, translationService, urlHelperService, messagingService, notificationService, uiGridConstants);

        this.doubleClickToEdit = false;

        this.gridHeaderComponentUrl = this.urlHelperService.getViewUrl("searchHeader.html");

        this.soeGridOptions.multiSelect = false;
        this.soeGridOptions.enableRowHeaderSelection = false;
        this.soeGridOptions.enableRowSelection = true;
        this.soeGridOptions.subscribe([
            new GridEvent(SoeGridOptionsEvent.RowSelectionChanged,
                row => {
                    this.selectedRow = row ? row.entity : null;
                })
        ]);

        this.resetSearchModel();

        // load stuff
        this.$q.all([this.loadActors(), this.loadTypes()]).then(() => this.setupGrid2());*/
        this.setData("");
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.setData(""); });
        }

        this.flowHandler.start({ feature: Feature.Economy_Supplier_Invoice_Matches, loadReadPermissions: true, loadModifyPermissions: true });

        this.resetSearchModel();
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg, () => this.setData(""));
    }

    private doLookups(): ng.IPromise<any> {
        return this.$q.all([this.loadActors(), this.loadTypes()]);
    }

    private setupGrid() {
        //this.gridAg.showColumnFooter = true;
        this.gridAg.addColumnText("actorName", this.terms["economy.supplier.supplier.supplier"], null);
        this.gridAg.addColumnText("invoiceNr", this.terms["economy.supplier.invoice.matches.invoicenr"], null);
        this.gridAg.addColumnText("paymentNr", this.terms["economy.supplier.invoice.matches.paymentnr"], null);
        this.gridAg.addColumnNumber("invoiceTotalAmount", this.terms["economy.supplier.invoice.matches.amount"], null, { decimals: 2 });
        //this.gridAg.addColumnText("typeName", this.terms["common.type"], null);
        this.gridAg.addColumnSelect("typeName", this.terms["common.type"], null, { displayField: "typeName", selectOptions: null, populateFilterFromGrid: true });
        this.gridAg.addColumnDate("date", this.terms["economy.supplier.invoice.invoicedate"], null);
        this.gridAg.addColumnIcon(null, " ", null, { icon: "fal fa-file-search", toolTip: this.terms["economy.supplier.invoice.matches.showinvoice"], onClick: this.openSupplierInvoice.bind(this) });

        this.gridAg.finalizeInitGrid("economy.supplier.invoice.matches.matches", true)
    }

    public openSupplierInvoice(row: any) {

        if (row.type === SoeOriginType.SupplierInvoice) {
            var message = new TabMessage(
                `${this.terms["economy.supplier.invoice.invoice"]} ${row.invoiceNr}`,
                row.invoiceId,
                InvoiceEditController,
                { id: row.invoiceId },
                this.urlHelperService.getGlobalUrl("Shared/Economy/Supplier/Invoices/Views/edit.html")
            );
            this.messagingService.publish(Constants.EVENT_OPEN_TAB, message);
        } else {
            var message = new TabMessage(
                `${this.terms["economy.supplier.invoice.matches.payment"]} ${row.invoiceNr}`,
                row.paymentRowId,
                PaymentEditController,
                { paymentId: row.paymentRowId },
                this.urlHelperService.getGlobalUrl("Shared/Economy/Supplier/Payments/Views/edit.html")
            );
            this.messagingService.publish(Constants.EVENT_OPEN_TAB, message);
        }
    }

    private loadTypes() {
        var keys: string[] = [
            "common.type",
            "economy.supplier.supplier.supplier",
            "economy.supplier.invoice.invoice",
            "economy.supplier.invoice.invoicedate",
            "economy.supplier.invoice.matches.invoicenr",
            "economy.supplier.invoice.matches.paymentnr",
            "economy.supplier.invoice.matches.amount",
            "economy.supplier.invoice.matches.showpayment",
            "economy.supplier.invoice.matches.showinvoice",
            "economy.supplier.invoice.matches.debitinvoice",
            "economy.supplier.invoice.matches.creditinvoice",
            "economy.supplier.invoice.matches.interestinvoice",
            "economy.supplier.invoice.matches.demandinvoice",
            "economy.supplier.invoice.matches.payment",
            "economy.supplier.invoice.matches.paymentsuggestion"];

        this.types = [];
        return this.translationService.translateMany(keys)
            .then(terms => {
                var i = 0;
                this.types = [
                    { id: i++, name: "" },
                    { id: i++, name: terms["economy.supplier.invoice.matches.debitinvoice"] },
                    { id: i++, name: terms["economy.supplier.invoice.matches.creditinvoice"] },
                    { id: i++, name: terms["economy.supplier.invoice.matches.interestinvoice"] },
                    { id: i++, name: terms["economy.supplier.invoice.matches.demandinvoice"] },
                    { id: i++, name: terms["economy.supplier.invoice.matches.payment"] }
                ];
                this.terms = terms;
                this.searchModel.type = 0;
            });
    }

    private loadActors() {
        return this.supplierService.getSuppliersDict(false, false, true)
            .then((actors: ISmallGenericType[]) => this.actors = actors);
    }

    public loadGridData() {
        this.search();
    }

    private setTypeName(row: any) {
        if (row.type === SoeOriginType.CustomerInvoice || row.type === SoeOriginType.SupplierInvoice) {
            switch (row.billingType) {
                case TermGroup_BillingType.Credit: row.typeName = this.terms["economy.supplier.invoice.matches.creditinvoice"]; break;
                case TermGroup_BillingType.Debit: row.typeName = this.terms["economy.supplier.invoice.matches.debitinvoice"]; break;
                case TermGroup_BillingType.Interest: row.typeName = this.terms["economy.supplier.invoice.matches.interestinvoice"]; break;
                case TermGroup_BillingType.Reminder: row.typeName = this.terms["economy.supplier.invoice.matches.demandinvoice"]; break;
            }
            if (row.typeName && !row.isEditable) {
                row.typeName += ` (${this.terms["economy.supplier.invoice.matches.paymentsuggestion"]})`;
            }
            return;
        }
        row.typeName = this.terms["economy.supplier.invoice.matches.payment"];
    }

    public search() {
        this.gridAg.clearData();
        this.searchModel.actorId = this.selectedActor ? this.selectedActor.id : 0;

        this.progress.startLoadingProgress([() => {
            return this.supplierService.getInvoicesPaymentsAndMatches(this.searchModel).then((x) => {
                x.forEach(row => this.setTypeName(row));

                this.setData(x);
            });
        }]);
    }

    public resetSearchModel() {
        this.searchModel = {
            originType: SoeOriginType.SupplierInvoice
        };
    }
}
