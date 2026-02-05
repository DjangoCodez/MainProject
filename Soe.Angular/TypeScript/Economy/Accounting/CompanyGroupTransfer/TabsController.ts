import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { EditController } from "./EditController";
import { EditController as VouchersEditController } from "../../../Shared/Economy/Accounting/Vouchers/EditController";
import { EditController as BudgetEditController } from "../../../Economy/Accounting/Budget/EditController";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { Constants } from "../../../Util/Constants";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService,
        tabHandlerFactory: ITabHandlerFactory,
        private messagingService: IMessagingService,
        private translationService: ITranslationService) {
        var part: string = "economy.accounting.companygroup.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.companyGroupTransferHeadId )
            .onGetRowEditName(row => row.statusName)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(EditController, { isHomeTab: true }, this.urlHelperService.getViewUrl("edit.html"));
            })
            //.onEdit(row => this.edit(row))
            .initialize(part + "transfer", part + "transfer", part + "transfer");

        this.messagingService.subscribe(Constants.EVENT_OPEN_OFFER, (x) => {
            if (x.voucherHeadId && x.voucherHeadId > 0)
                this.openVoucher(x);
            else if (x.budgetHeadId && x.budgetHeadId > 0)
                this.openBudget(x);
        });
    }

    private edit(row: any) {
    }

    private openVoucher(row: any) {
        return this.translationService.translate("economy.accounting.voucher.voucher").then((term) => {
            this.tabs.addEditTab(row, VouchersEditController, { id: row.voucherHeadId, updateTab: true }, this.urlHelperService.getGlobalUrl('Economy/Accounting/Vouchers/Views/edit.html'), term);
        });
    }

    private openBudget(row: any) {
        return this.translationService.translate("economy.accounting.budget.budget").then((term) => {
            this.tabs.addEditTab(row, BudgetEditController, { id: row.budgetHeadId, updateTab: true }, this.urlHelperService.getGlobalUrl('Economy/Accounting/Budget/Views/edit.html'), term + " " + row.budgetName);
        });
    }

    public tabs: ITabHandler;
}