import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { GridController } from "./GridController";
import { EditController } from "../../../Shared/Economy/Accounting/AccountDistribution/EditController";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService,
        tabHandlerFactory: ITabHandlerFactory) {

        const part = "economy.accounting.accountdistribution.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.accountDistributionHeadId)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
                tabHandler.enableAddTab(() => this.add());
                tabHandler.enableRemoveAll();
            })
            .onEdit((row, data) => this.edit(row, data))
            .initialize(part + soeConfig.lastpartEntityNameSingleKey, part + soeConfig.lastpartentityNameMultipleKey, part + soeConfig.lastpartentityNameNewKey);
    }

    private edit(row: any, data: ISmallGenericType[] = null) {
        // Open edit page
        this.tabs.addEditTab(row, EditController, { navigatorRecords: data }, this.urlHelperService.getGlobalUrl("Shared/Economy/Accounting/AccountDistribution/Views/edit.html"));
    }
    private add() {
        this.tabs.addCreateNewTab(EditController, this.urlHelperService.getGlobalUrl("Shared/Economy/Accounting/AccountDistribution/Views/edit.html"));
    }

    public tabs: ITabHandler;
}