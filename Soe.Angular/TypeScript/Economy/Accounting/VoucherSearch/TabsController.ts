import { TabsControllerBase1 } from "../../../Core/Controllers/TabsControllerBase1";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IReportService } from "../../../Core/Services/ReportService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { GridController } from "../../../Shared/Economy/Accounting/VoucherSearch/GridController";
import { EditController as VouchersEditController } from "../../../Shared/Economy/Accounting/Vouchers/EditController";
import { ITabHandlerFactory } from "../../../Core/Handlers/tabhandlerfactory";
import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService, tabHandlerFactory: ITabHandlerFactory) {
        // Setup base class
        const part: string = "economy.accounting.vouchersearch";
        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier((row) => { return row.voucherHeadId; })
            .onGetRowEditName(row => row.voucherNr)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
                tabHandler.enableAddTab(() => this.add());
                tabHandler.enableRemoveAll();
            })
            .onEdit(row => this.edit(row))

            this.tabs.initialize("economy.accounting.voucher.voucher", part, "economy.accounting.voucher.new");
    }

    protected add() {
        this.tabs.addCreateNewTab(VouchersEditController, this.urlHelperService.getGlobalUrl('Economy/Accounting/Vouchers/Views/edit.html'));
    }

    protected edit(row: any) {
        this.tabs.addEditTab(row, VouchersEditController, { id: this.getEditIdentifier(row) }, this.urlHelperService.getGlobalUrl('Economy/Accounting/Vouchers/Views/edit.html'));
    }

    protected getEditIdentifier(row: any): any {
        return row.voucherHeadId;
    }

    protected getEditName(data: any): string {
        return data.voucherNr;
    }

    public tabs: ITabHandler;
}