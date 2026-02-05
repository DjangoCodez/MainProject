import { GridController } from "./GridController";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { SoeOriginStatusClassification, CompanySettingType, Feature } from "../../../Util/CommonEnumerations";
import { EditController } from "../../../Shared/Economy/Supplier/Payments/EditController";
import { ITabHandlerFactory } from "../../../Core/Handlers/tabhandlerfactory";
import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { HtmlUtility } from "../../../Util/HtmlUtility";

export class TabsController implements ICompositionTabsController {

    protected terms: any;
    protected usePaymentSuggestion: boolean;
    protected hasUnpaidPermission: boolean;
    protected hasSuggestionPermission: boolean;
    protected hasPaidPermission: boolean;
    protected hasVoucherPermission: boolean;

    //@ngInject
    constructor(tabHandlerFactory: ITabHandlerFactory,
        private coreService: ICoreService,
        private $q: ng.IQService,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private $window) {

        var part: string = "economy.supplier.payment.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.paymentRowId)
            .onGetRowEditName(row => row.paymentSeqNr ? row.paymentSeqNr : "" )
            .onSetupTabs(() => { this.setupTabs(); })
            .onEdit(row => this.edit(row))
            .initialize(part + "payment", part + "payments", part + "newpayment");
    }

    protected setupTabs() {
        this.$q.all([
            this.loadTerms(),
            this.loadModifyPermissions(),
            this.loadCompanySettings()
        ]).then(() => {
            var gridUrl = this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"); //this.urlHelperService.getViewUrl("grid.html");
            var activateTab = true;
            if (this.hasUnpaidPermission) {
                this.tabs.addNewTab(this.terms["economy.supplier.invoice.unpaid"], null, GridController, gridUrl, { isHomeTab: true, classification: SoeOriginStatusClassification.SupplierPaymentsUnpayed, usePaymentSuggestion: this.usePaymentSuggestion }, false, activateTab);
                activateTab = false;
            }
            if (this.hasSuggestionPermission && this.usePaymentSuggestion) {
                const isProposal = HtmlUtility.getQueryParameterByName(this.$window.location, "proposal");
                if (isProposal)
                    activateTab = true;
                this.tabs.addNewTab(this.terms["economy.supplier.invoice.suggestion"], null, GridController, gridUrl, { isHomeTab: true, classification: SoeOriginStatusClassification.SupplierPaymentSuggestions, usePaymentSuggestion: this.usePaymentSuggestion }, false, activateTab);
                activateTab = false;
            }
            if (this.hasPaidPermission) {
                this.tabs.addNewTab(this.terms["economy.supplier.invoice.paid"], null, GridController, gridUrl, { isHomeTab: true, classification: SoeOriginStatusClassification.SupplierPaymentsPayed, usePaymentSuggestion: this.usePaymentSuggestion }, false, activateTab);
                activateTab = false;
            }
            if (this.hasVoucherPermission) {
                this.tabs.addNewTab(this.terms["economy.supplier.invoice.paidvoucher"], null, GridController, gridUrl, { isHomeTab: true, classification: SoeOriginStatusClassification.SupplierPaymentsVoucher, usePaymentSuggestion: this.usePaymentSuggestion }, false, activateTab);
                activateTab = false;
            }

            this.tabs.enableRemoveAll();
            this.tabs.enableAddTab(() => this.add());

            // Navigate to invoice from new Angular
            const paymentIdQP = HtmlUtility.getQueryParameterByName(this.$window.location, "paymentId")
            if (soeConfig.paymentId > 0) {
                this.edit({ paymentRowId: soeConfig.paymentId });
            } else if (soeConfig.invoiceId === 0 && paymentIdQP) {
                this.add();
            }
        })
    }
    protected add() {
        this.tabs.addCreateNewTab(EditController, this.urlHelperService.getGlobalUrl("Shared/Economy/Supplier/Payments/Views/edit.html"));
    }

    protected edit(row: any) {
        if(row.paymentRowId === 0 && row.supplierInvoiceId > 0)
            this.tabs.addEditTab(row, EditController, { invoiceId: row.supplierInvoiceId }, this.urlHelperService.getGlobalUrl("Shared/Economy/Supplier/Payments/Views/edit.html"));
        else
            this.tabs.addEditTab(row, EditController, { paymentId: this.getEditIdentifier(row) }, this.urlHelperService.getGlobalUrl("Shared/Economy/Supplier/Payments/Views/edit.html"));
    }
    protected getEditIdentifier(row: any): any {
        return row.paymentRowId;
    }

    private loadTerms(): ng.IPromise<any> {
        var keys = ["economy.supplier.invoice.unpaid", "economy.supplier.invoice.suggestion", "economy.supplier.invoice.paid", "economy.supplier.invoice.paidvoucher"];
        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        return this.coreService.getCompanySettings([CompanySettingType.SupplierUsePaymentSuggestions]).then(x => {
            this.usePaymentSuggestion = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.SupplierUsePaymentSuggestions, false);
        });
    }

    private loadModifyPermissions() {
        return this.coreService.hasModifyPermissions([Feature.Economy_Supplier_Invoice_Status_OriginToPayment, Feature.Economy_Supplier_Invoice_Status_OriginToPaymentSuggestion, Feature.Economy_Supplier_Invoice_Status_PayedToVoucher, Feature.Economy_Supplier_Invoice_Status_PaymentVoucher]).then((x) => {
            if (x[Feature.Economy_Supplier_Invoice_Status_OriginToPayment])
                this.hasUnpaidPermission = true;
            if (x[Feature.Economy_Supplier_Invoice_Status_OriginToPaymentSuggestion])
                this.hasSuggestionPermission = true;
            if (x[Feature.Economy_Supplier_Invoice_Status_PayedToVoucher])
                this.hasPaidPermission = true;
            if (x[Feature.Economy_Supplier_Invoice_Status_PaymentVoucher])
                this.hasVoucherPermission = true;
        });
    }

    public tabs: ITabHandler;
}