import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { GridController } from "./GridController";
import { PriceBasedGridController } from "./PriceBasedGridController";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService, tabHandlerFactory: ITabHandlerFactory) {
        const isCustomerDiscount = soeConfig.isCustomerDiscount;
        const isPriceBased = soeConfig.isPriceBased;
        var label = this.getLabel(isCustomerDiscount, isPriceBased);
        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.sysPriceListHeadId)
            .onGetRowEditName(row => row.sysWholesellerName)
            .onSetupTabs((tabHandler) => {
                if(isPriceBased)
                    tabHandler.addHomeTab(PriceBasedGridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
                else
                    tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
            })            
            .initialize(label, label, label);
    }

    public getLabel(isCustomerDiscount: boolean, isPriceBased: boolean): string {
        if (isCustomerDiscount)
            return "billing.invoices.markup.customerdiscount";

        return isPriceBased ? "billing.invoices.pricebasedmarkup.pricebasedmarkup" : "billing.invoices.markup.markup";
    }

    public tabs: ITabHandler;
}