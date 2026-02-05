import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { GridController as AccountYearGridController } from "../AccountYear/GridController";
import { GridController as BalancesGridController } from "../Balance/GridController";
import { GridController as VoucherSeriesGridController } from "../VoucherSeries/GridController";
import { EditController as VoucherSeriesEditController } from "../VoucherSeries/EditController";
import { ITranslationService } from "../../../Core/Services/TranslationService";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService,
        tabHandlerFactory: ITabHandlerFactory,
        private translationService: ITranslationService) {
        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.voucherSeriesTypeId)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                this.setupTabs();
            })
            .onEdit(row => this.edit(row))
            .initialize("dummy", "dummy", "dummy");
    }

    private setupTabs() {
        const keys: string[] = [
            "economy.accounting.voucherseriestypes",
            "economy.accounting.balance.balance",
            "economy.accounting.accountyear.accountyears"
        ];
        this.translationService.translateMany(keys).then((terms) => {
            this.tabs.enableRemoveAll();
            this.tabs.addNewTab(terms["economy.accounting.accountyear.accountyears"], null, AccountYearGridController, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"), { isHomeTab: true, setup: true }, false, true);
            this.tabs.addNewTab(terms["economy.accounting.balance.balance"], null, BalancesGridController, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"), { isHomeTab: true, setup: true }, false, false);
            this.tabs.addNewTab(terms["economy.accounting.voucherseriestypes"], null, VoucherSeriesGridController, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"), { isHomeTab: true, setup: true }, false, false);
        });
    }

    private edit(row: any) {
        // Open edit page - only voucher series
        if (row.row)
            this.tabs.addEditTab(row.row, VoucherSeriesEditController, { ids: row.ids });
    }

    public tabs: ITabHandler;
}