import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { GridController } from "./GridController";
import { TabMessage } from "../../../Core/Controllers/TabsControllerBase1";
import { Guid } from "../../../Util/StringUtility";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { Constants } from "../../../Util/Constants";
import { SoeHouseholdClassificationGroup, SoeEntityType } from "../../../Util/CommonEnumerations";
import { EditController as BillingInvoicesEditController } from "../../../Shared/Billing/Invoices/EditController";
import { EditController as VouchersEditController } from "../../../Shared/Economy/Accounting/Vouchers/EditController";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private urlHelperService: IUrlHelperService,
        tabHandlerFactory: ITabHandlerFactory,
        private $timeout: ng.ITimeoutService) {

        // Setup base class
        const part = "billing.invoices.householddeduction.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.customerInvoiceRowId)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                const keys: string[] = [
                    "billing.invoices.householddeduction.applyrut",
                    "billing.invoices.householddeduction.applyrot",
                    "billing.invoices.householddeduction.appliedmany",
                    "billing.invoices.householddeduction.approvedmulti",
                    "billing.invoices.householddeduction.deniedmulti",
                    "common.all"
                ];
                const gridUrl = this.urlHelperService.getCoreViewUrl("gridCompositionAg.html");

                this.translationService.translateMany(keys).then((terms) => {
                    tabHandler.addNewTab(terms["billing.invoices.householddeduction.applyrot"], null, GridController, gridUrl, { classification: SoeHouseholdClassificationGroup.Apply }, false, true);
                    tabHandler.addNewTab(terms["billing.invoices.householddeduction.appliedmany"], null, GridController, gridUrl, { classification: SoeHouseholdClassificationGroup.Applied }, false);
                    tabHandler.addNewTab(terms["billing.invoices.householddeduction.approvedmulti"], null, GridController, gridUrl, { classification: SoeHouseholdClassificationGroup.Received }, false);
                    tabHandler.addNewTab(terms["billing.invoices.householddeduction.deniedmulti"], null, GridController, gridUrl, { classification: SoeHouseholdClassificationGroup.Denied }, false);
                    tabHandler.addNewTab(terms["common.all"], null, GridController, gridUrl, { classification: SoeHouseholdClassificationGroup.All }, false);
                    tabHandler.enableRemoveAll();
                });
            })
            .onEdit(row => this.edit(row))
            .initialize(part + "householddeduction", part + "householddeductions", part + "new");

        this.messagingService.subscribe(Constants.EVENT_OPEN_HOUSEHOLD, (x) => {
            this.$timeout(() => {
                this.openEdit(x);
            });
        });

        this.messagingService.subscribe(Constants.EVENT_OPEN_INVOICE, (x) => {
            this.$timeout(() => {
                this.edit(x);
            });
        });

        this.messagingService.subscribe(Constants.EVENT_OPEN_VOUCHER, (x) => {
            this.$timeout(() => {
                this.editVoucher(x);
            });
        });
    }

    private edit(row: any) {
        const message = new TabMessage(
            row.name,
            row.id,
            BillingInvoicesEditController,
            { id: row.id },
            this.urlHelperService.getGlobalUrl('Shared/Billing/Invoices/Views/edit.html')
        );
        this.messagingService.publish(Constants.EVENT_OPEN_TAB, message);
    }

    private editVoucher(row: any) {
        const message = new TabMessage(
            row.name,
            row.invoiceId,
            VouchersEditController,
            { id: row.id },
            this.urlHelperService.getGlobalUrl('Economy/Accounting/Vouchers/Views/edit.html')
        );
        this.messagingService.publish(Constants.EVENT_OPEN_TAB, message);
    }

    private openEdit(message: any) {
        if (message.entityType === SoeEntityType.CustomerInvoice) {
            this.translationService.translate("common.customer.invoices.newcustomerinvoice").then((term) => {
                const tabMessage = new TabMessage(
                    term,
                    Guid.newGuid(),
                    BillingInvoicesEditController,
                    { createHousehold: true, id: message.id, rowId: message.rowId, taxDeductionType: message.taxDeductionType, percent: message.percent, amount: message.amount },
                    this.urlHelperService.getGlobalUrl('Shared/Billing/Invoices/Views/edit.html')
                );
                this.messagingService.publish(Constants.EVENT_OPEN_TAB, tabMessage);
            });
        }
        else if (message.entityType === SoeEntityType.Voucher) {
            this.translationService.translate("billing.invoices.householddeduction.newvoucher").then((term) => {
                const tabMessage = new TabMessage(
                    term,
                    Guid.newGuid(),
                    VouchersEditController,
                    { createHousehold: true, date: message.date, amount: message.amount, ids: message.ids, nbrs: message.nbrs, taxDeductionType: message.taxDeductionType, productId: message.productId },
                    this.urlHelperService.getGlobalUrl("Economy/Accounting/Vouchers/Views/edit.html")
                );
                this.messagingService.publish(Constants.EVENT_OPEN_TAB, tabMessage);
            });
        }
    }

    public tabs: ITabHandler;
}