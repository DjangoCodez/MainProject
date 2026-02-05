import { ICompositionTabsController } from "../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../Core/Handlers/TabHandlerFactory";
import { ITabHandler } from "../../Core/Handlers/TabHandler";
import { IUrlHelperService } from "../../Core/Services/UrlHelperService";
import { EditController } from "./EditController";

export class TabsController implements ICompositionTabsController {
    //@ngInject
    constructor(private urlHelperService: IUrlHelperService, tabHandlerFactory: ITabHandlerFactory) {

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.contractGroupId)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(EditController, { isHomeTab: true }, this.urlHelperService.getGlobalUrl("/Common/CustomerSpecificExports/Views/edit.html"));      
            })
            .onEdit(row => this.edit(row))
            .initialize("common.customerspecificexports.exports", "common.customerspecificexports.exports", "common.customerspecificexports.exports");
    }

    private edit(row: any) {
    }

    private add() {
    }

    public tabs: ITabHandler;
}