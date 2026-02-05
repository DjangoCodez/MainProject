import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { GridController } from "./GridController";
import { SoeOriginStatusClassification, Feature } from "../../../Util/CommonEnumerations";
import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../Core/Handlers/tabhandlerfactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";

export class TabsController implements ICompositionTabsController {

    protected terms: any;
    protected hasOverViewPermission: boolean;
    protected hasAttestedInvoicesPermission: boolean;

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService,
        tabHandlerFactory: ITabHandlerFactory,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        private $q: ng.IQService) {

        // Setup base class
        var part: string = "economy.supplier.attestflowoverview.";
        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.vatCodeId)
            .onGetRowEditName(row => row.name)
            .onSetupTabs(() => { this.setupTabs(); })
            .initialize(part + "overview", part + "overview", part + "overview");
    }

    private setupTabs() {
        this.$q.all([
            this.loadTerms(),
            this.loadModifyPermissions(),

        ]).then(() => {

            var gridUrl = this.urlHelperService.getViewUrl("grid.html");
            var activateTab = true;
            if (this.hasOverViewPermission) {
                this.tabs.addNewTab(this.terms["economy.supplier.attestflowoverview.overview"], null, GridController, gridUrl, { classification: SoeOriginStatusClassification.SupplierInvoicesAttestFlowMyActive }, false, activateTab);
                activateTab = false;
            }
            if (this.hasAttestedInvoicesPermission) {
                this.tabs.addNewTab(this.terms["economy.supplier.attestflowoverview.myattested"], null, GridController, gridUrl, { classification: SoeOriginStatusClassification.SupplierInvoicesAttestFlowMyClosed }, false, activateTab);
            }
        });
    }

    private loadTerms(): ng.IPromise<any> {
        var keys = [
            "economy.supplier.attestflowoverview.overview",
            "economy.supplier.attestflowoverview.myattested",
            // "economy.supplier.invoice.suggestion",
            //"economy.supplier.invoice.paid",
            //"economy.supplier.invoice.paidvoucher"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private loadModifyPermissions() {
        return this.coreService.hasModifyPermissions([Feature.Economy_Supplier_Invoice_AttestFlow_MyItems, Feature.Economy_Supplier_Invoice_AttestFlow_Overview]).then((x) => {
            if (x[Feature.Economy_Supplier_Invoice_AttestFlow_Overview])
                this.hasOverViewPermission = true;
            if (x[Feature.Economy_Supplier_Invoice_AttestFlow_MyItems])
                this.hasAttestedInvoicesPermission = true;
        });
    }


    public tabs: ITabHandler;
}