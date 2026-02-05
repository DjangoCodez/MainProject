import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { GridController } from "./GridController";
import { EditController } from "./EditController";
import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../Core/Handlers/tabhandlerfactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";

export class TabsController implements ICompositionTabsController {

    private type: number;
    public tabs: ITabHandler;
    //@ngInject
    constructor(
        private urlHelperService: IUrlHelperService,
        tabHandlerFactory: ITabHandlerFactory) {

        // Setup base class
        var part: string = "economy.export.paymentservice.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.invoiceExportId)
            .onGetRowEditName(row => row.batchId)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
                tabHandler.enableAddTab(() => this.add());
                tabHandler.enableRemoveAll();
            })
            .onEdit(row => this.edit(row))
            .initialize(part + "paymentservices", part + "paymentservices", part + "new_paymentservice");
    }

    private edit(row: any) {
        this.tabs.addEditTab(row, EditController, { id: row.invoiceExportId, batchId: row.batchId });
    }

    private add() {
        this.tabs.addCreateNewTab(EditController, this.urlHelperService.getViewUrl("edit.html"));
    }
}