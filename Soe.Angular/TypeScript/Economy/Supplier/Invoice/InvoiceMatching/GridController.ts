import { GridControllerBase } from "../../../../Core/Controllers/GridControllerBase";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { ToolBarUtility, ToolBarButton } from "../../../../Util/ToolBarUtility";
import { IconLibrary, SoeGridOptionsEvent, InsecureDebtsButtonFunctions, SupplierInvoiceAttestFlowButtonFunctions, SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../Util/Enumerations";
import { GridEvent } from "../../../../Util/SoeGridOptions";
import { ISupplierService } from "../../../../Shared/Economy/Supplier/SupplierService";
import { Feature, SoeInvoiceType } from "../../../../Util/CommonEnumerations";
import { Constants } from "../../../../Util/Constants";

export class GridController extends GridControllerBase {
    private supplierInvoicePermission: boolean;
    private terms: { [index: string]: string; };
    //@ngInject
    constructor($http,
        $templateCache,
        $timeout: ng.ITimeoutService,
        $uibModal,
        private $filter: ng.IFilterService,
        coreService: ICoreService,
        private supplierService: ISupplierService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        uiGridConstants: uiGrid.IUiGridConstants,
        private $q: ng.IQService) {

        super("Soe.Economy.Supplier.Invoice.Matches", "economy.supplier.invoice.matches.supplier", Feature.Economy_Supplier_Invoice_Matching, $http, $templateCache, $timeout, $uibModal, coreService, translationService, urlHelperService, messagingService, notificationService, uiGridConstants);
        this.$q.all([this.loadModifyPermissions()]).then(x => this.setupMatchesGrid());
    }

    public setupMatchesGrid() {
        this.soeGridOptions.showColumnFooter = true;
        // Columns
        var keys: string[] = [
            "economy.supplier.supplier.suppliernr",
            "economy.supplier.invoice.matches.suppliername",
            "economy.supplier.invoice.matches.openposts",
            "economy.supplier.invoice.currencycode",
            "common.amount",
            "core.edit"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
            super.addColumnNumber("number", terms["economy.supplier.supplier.suppliernr"], null, null, null, "");
            super.addColumnText("name", terms["economy.supplier.invoice.matches.suppliername"], "50%");
            super.addColumnNumber("count", terms["economy.supplier.invoice.matches.openposts"], null, null, null, "");
            super.addColumnNumber("sum", terms["common.amount"], null, null, 2, "");
            this.soeGridOptions.addColumnEdit(terms["core.edit"]);
        });
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        var featureIds: number[] = [];
        featureIds.push(Feature.Economy_Supplier_Invoice);

        return this.coreService.hasReadOnlyPermissions(featureIds)
            .then((x) => {
                if (x[Feature.Economy_Supplier_Invoice]) {
                    this.supplierInvoicePermission = true;
                }
            });
    }

    private addSumFooter(column: uiGrid.IColumnDefOf<any>) {
        column.aggregationType = this.uiGridConstants.aggregationTypes.sum;
        column.aggregationHideLabel = true;
        column.width = "100";
        this.addSumAggregationFooterToColumns(column);
    }

    public loadGridData() {
        this.search();
    }

    public openSupplierInvoice(row: any) {
        this.messagingService.publish(Constants.EVENT_OPEN_EDITSUPPLIERINVOICE, {
            id: row.invoiceId,
            name: this.terms["economy.supplier.invoice.invoice"] + " " + row.seqNr
        });
    }

    public search() {
        this.supplierService.getMatchingCustomerSupplier(SoeInvoiceType.SupplierInvoice).then((x: any[]) => {
            super.gridDataLoaded(x);
        });
    }
}
