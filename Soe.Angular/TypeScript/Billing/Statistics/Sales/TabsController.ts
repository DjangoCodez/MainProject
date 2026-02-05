import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { GridController } from "./GridController";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService,
        tabHandlerFactory: ITabHandlerFactory) {

        const part: string = "billing.statistics.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.accountDistributionHeadId)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
            })
            .onEdit(row => this.edit(row))
            .initialize(part + "sales", part + "sales", part + "new");
    }

    private edit(row: any) {
        // Open edit page
        //this.tabs.addEditTab(row, EditController, null, this.urlHelperService.getGlobalUrl("Shared/Economy/Accounting/AccountDistribution/Views/edit.html"));
    }

    public tabs: ITabHandler;
}