import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { GridController } from "./GridController";
import { EditController as VouchersEditController } from "../../../Shared/Economy/Accounting/Vouchers/EditController";
import { EditController as SupplierInvoiceEditController } from "../../../Shared/Economy/Supplier/Invoices/EditController";
import { EditController as CustomerInvoiceEditController } from "../../../Shared/Billing/Invoices/EditController";
import { EditController as CustomerInvoiceLedgerEditController } from "../../../Common/Customer/Invoices/EditController";
import { SoeOriginType, OrderInvoiceRegistrationType } from "../../../Util/CommonEnumerations";

export class TabsController implements ICompositionTabsController {
    //@ngInject
    constructor(private urlHelperService: IUrlHelperService, tabHandlerFactory: ITabHandlerFactory, private translationService: ITranslationService) {

        var part: string = "common.report.report.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(item => this.getRowIdentifier(item))
            .onGetRowEditName(item => this.getRowEditName(item))
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
                tabHandler.enableRemoveAll();
            })
            .onEdit(item => this.edit(item))
            .initialize(part + "report", part + "reports", part + "newreport");
    }

    private edit(item: any) {
        var row = item.row;
        if (item.openInvoice) {
            switch (row.invoiceOriginType) {
                case SoeOriginType.CustomerInvoice:
                    this.openCustomerInvoice(item);
                    break;
                case SoeOriginType.SupplierInvoice:
                    this.openSupplierInvoice(item);
                    break;
            }
        }
        else {
            this.openVoucher(item);
        }
    }

    private openVoucher(item: any) {
        return this.translationService.translate("economy.accounting.voucher.voucher").then((term) => {
            this.tabs.addEditTab(item, VouchersEditController, { id: item.row.voucherHeadId }, this.urlHelperService.getGlobalUrl('Economy/Accounting/Vouchers/Views/edit.html'), term);
        });
    }

    private openSupplierInvoice(item: any) {
        return this.translationService.translate("common.supplierinvoice").then((term) => {
            this.tabs.addEditTab(item, SupplierInvoiceEditController, { id: item.row.invoiceId }, this.urlHelperService.getGlobalUrl('Shared/Economy/Supplier/Invoices/Views/edit.html'), term);
        });
    }

    private openCustomerInvoice(item: any) {
        return this.translationService.translate("common.customerinvoice").then((term) => {
            if (item.row.invoiceRegistrationType === OrderInvoiceRegistrationType.Ledger)
                this.tabs.addEditTab(item, CustomerInvoiceLedgerEditController, { id: item.row.invoiceId }, this.urlHelperService.getGlobalUrl('Common/Customer/Invoices/Views/edit.html'), term);
            else
                this.tabs.addEditTab(item, CustomerInvoiceEditController, { id: item.row.invoiceId }, this.urlHelperService.getGlobalUrl('Shared/Billing/Invoices/Views/edit.html'), term);
        });
    }

    private getRowIdentifier(item: any): string {
        var identifier = undefined;
        switch (item.originType) {
            case SoeOriginType.CustomerInvoice:
                identifier = "ci_" + item.row.invoiceId;
                break
            case SoeOriginType.SupplierInvoice:
                identifier = "si_" + item.row.invoiceId;
                break
            default:
                identifier = "v_" + item.row.voucherHeadId;
                break
        }
        return identifier;
    }

    private getRowEditName(item: any): string {
        return item.editName;
    }

    private add() {
        this.tabs.addCreateNewTab(VouchersEditController, this.urlHelperService.getGlobalUrl('Economy/Accounting/Vouchers/Views/edit.html'));
    }

    public tabs: ITabHandler;
}