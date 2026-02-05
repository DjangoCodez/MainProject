import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { GridController } from "./GridController";
import { EditController } from "../../../Shared/Economy/Supplier/Suppliers/EditController";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { HtmlUtility } from "../../../Util/HtmlUtility";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { ITranslationService } from "../../../Core/Services/TranslationService";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(tabHandlerFactory: ITabHandlerFactory, protected urlHelperService: IUrlHelperService, private translationService: ITranslationService, private $timeout: ng.ITimeoutService, private $window) {

        var part: string = "economy.supplier.supplier.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.actorSupplierId)
            .onGetRowEditName(row => row.supplierNr)
            .onSetupTabs((tabHandler) => {
                this.setupTabs(tabHandler);
            })
            .onEdit((row, data) => this.edit(row, data))
            .initialize(part + "supplier", part + "suppliers", part + "new");
    }

    private $onInit() {
        const actorsupplierid = HtmlUtility.getQueryParameterByName(this.$window.location, "actorsupplierid")
        if (actorsupplierid) {
            var supplierNr = HtmlUtility.getQueryParameterByName(this.$window.location, "suppliernr");
            var row = { actorSupplierId: actorsupplierid, supplierNr: supplierNr };
            this.$timeout(() => { this.edit(row) });
        }
    }

    protected setupTabs(tabHandler: ITabHandler) {
        tabHandler.addHomeTab(GridController, null, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
        tabHandler.enableAddTab(() => this.add());
        tabHandler.enableRemoveAll();
        if (soeConfig.invoiceId > 0) {
            this.translationService.translateMany(['economy.supplier.supplier']).then((terms) => {
                const params = { id: soeConfig.invoiceId }
                const templateUrl = this.urlHelperService.getViewUrl("edit.html");
                const title = `${terms['economy.supplier.supplier']} ${soeConfig.invoiceNr}`;
                this.tabs.addEditTab({ invoiceId: soeConfig.invoiceId, invoiceNr: soeConfig.invoiceNr }, EditController, params, templateUrl, title, true);
            });
        } else if (soeConfig.supplierId > 0) {
            this.tabs.addEditTab({ row: { id: soeConfig.supplierId, type: "supplier" } }, EditController, { id: soeConfig.supplierId, type: "supplier" }, this.urlHelperService.getGlobalUrl("Shared/Economy/Supplier/Suppliers/Views/edit.html"));
        }
    }

    protected add() {
        this.tabs.addCreateNewTab(EditController, this.urlHelperService.getGlobalUrl("Shared/Economy/Supplier/Suppliers/Views/edit.html"));
    }

    protected edit(row: any, data: ISmallGenericType[] = null) {
        this.tabs.addEditTab(row, EditController, { navigatorRecords: data }, this.urlHelperService.getGlobalUrl("Shared/Economy/Supplier/Suppliers/Views/edit.html"));
    }

    public tabs: ITabHandler;
}