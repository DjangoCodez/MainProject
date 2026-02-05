import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ILazyLoadService } from "../../../Core/Services/LazyLoadService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { GridController } from "./GridController";
import { EditController } from "./Editcontroller";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";

export class TabsController implements ICompositionTabsController {

    private htmlEditorLoaderPromise: Promise<any>;

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService, tabHandlerFactory: ITabHandlerFactory, private lazyLoadService: ILazyLoadService) {

        // Setup base class        

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.emailTemplateId)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
                tabHandler.enableAddTab(() => this.add());
                tabHandler.enableRemoveAll();
            })
            .onEdit(row => this.edit(row))
            .initialize("billing.invoices.emailtemplate", "billing.invoices.emailtemplates", "billing.invoices.emailtemplate.new");
    }

    $onInit() {
        this.htmlEditorLoaderPromise = this.lazyLoadService.loadBundle("Soe.Common.HtmlEditor.Bundle");
    }

    private edit(row: any) {
        this.htmlEditorLoaderPromise.then(() => {
            // Open edit page
            this.tabs.addEditTab(row, EditController);
        });       
    }

    private add() {
        this.tabs.addCreateNewTab(EditController, this.urlHelperService.getViewUrl("edit.html"));
    }

    public tabs: ITabHandler;
}