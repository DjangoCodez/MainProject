import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { GridController } from "./GridController";
import { EditController } from "../../../Shared/Economy/Accounting/Vouchers/EditController";
import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../Core/Handlers/tabhandlerfactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { ITranslationService } from "../../../Core/Services/TranslationService";

export class TabsController implements ICompositionTabsController {

    // Config
    isTemplates = false;

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService, tabHandlerFactory: ITabHandlerFactory, private translationService: ITranslationService) {
        
        // Config parameters
        if (soeConfig.isTemplates)
            this.isTemplates = true;

        // Setup base class
        const part = "economy.accounting.voucher.";
        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier((row) => { if (row.row) return row.row.voucherHeadId; else return row.voucherHeadId; })
            .onGetRowEditName(row => row.voucherNr)
            .onSetupTabs((tabHandler) => {
                this.setupTabs(tabHandler);
            })
            .onEdit(row => this.edit(row))

        if (this.isTemplates)
            this.tabs.initialize(part + "template", part + "templates", part + "newtemplate");
        else
            this.tabs.initialize(part + "voucher", part + "vouchers", part + "new");
    }

    protected setupTabs(tabHandler: ITabHandler) {
        tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
        tabHandler.enableAddTab(() => this.add());
        tabHandler.enableRemoveAll();
        if (soeConfig.voucherHeadId > 0) {
            this.translationService.translateMany(['economy.accounting.voucher.voucher']).then((terms) => {
                const params = { id: soeConfig.voucherHeadId }
                const templateUrl = this.urlHelperService.getViewUrl("edit.html");

                const title = `${terms['economy.accounting.voucher.voucher']} ${soeConfig.voucherNr}`;
                this.tabs.addEditTab({ voucherHeadId: soeConfig.voucherHeadId }, EditController, params, templateUrl, title, true);
            });
        } else if (soeConfig.openNewVoucher) {
            this.add();
        }
    }

    protected getEditIdentifier(row: any): any {
        return row.voucherHeadId;
    }

    private edit(rowAndIdsObj: any) {
        var rowAndIds = { row : null,ids: null};
        if (rowAndIdsObj.row) {
            rowAndIds = rowAndIdsObj;
        } else {
            rowAndIds.row = rowAndIdsObj;
        }
        if (rowAndIdsObj.ids) {
            rowAndIds.ids = rowAndIdsObj.ids;
        }
        const activeTab = this.tabs.getTabByIdentifier(this.getEditIdentifier(rowAndIds.row));
        if (activeTab) {
            this.tabs.setActiveTabIndex(this.tabs.getIndexOf(activeTab));
        } else {
            // Open edit page
            this.tabs.addEditTab(rowAndIds.row, EditController, { id: this.getEditIdentifier(rowAndIds.row), selectedYear: rowAndIds.row['selectedYear'], ids: rowAndIds.ids });
        }
    }
    private add() {
        this.tabs.addCreateNewTab(EditController, this.urlHelperService.getViewUrl("edit.html"), {});
    }

    public tabs: ITabHandler;
}
