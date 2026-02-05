import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { GridController } from "./GridController";
import { EditController } from "./EditController";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService,
        tabHandlerFactory: ITabHandlerFactory) {

        // Setup base class
        var part: string = "economy.import.batches.";
        
        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.recordId)
            .onGetRowEditName(row => row.batchId)
            .onSetupTabs((tabHandler) => {                
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));                
                tabHandler.enableRemoveAll();
            })
            .onEdit(row => this.edit(row))
            .initialize(part + "batch", part + "batches", part + "new_batch");
    }

    private edit(row: any) {
        // Open edit page
        this.tabs.addEditTab(row, EditController);
    }
    
    public tabs: ITabHandler;
}