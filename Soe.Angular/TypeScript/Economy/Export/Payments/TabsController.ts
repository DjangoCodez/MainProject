import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { GridController } from "./GridController";
import { EditController as PaymentsEditController } from "../../../Shared/Economy/Supplier/Payments/EditController";
import { ITranslationService } from "../../../Core/Services/TranslationService";

export class TabsController implements ICompositionTabsController {
    private type: number;
    protected paymentTerm: any;
    protected homeTabTerm: any;

    //@ngInject
    constructor(private $window: ng.IWindowService,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        tabHandlerFactory: ITabHandlerFactory) {

        // Setup base class
        var part: string = "economy.export.payment.";

        this.type = soeConfig.exportType;
        this.onLoadTerms();
        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.paymentExportId)
            .onGetRowEditName(row => row.name)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true, type: this.type }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"), this.homeTabTerm);
                tabHandler.enableRemoveAll();
            })
            .onEdit(row => this.edit(row))
            .initialize(part + "paymentexport", part + "paymentexports", part + "new_paymentexport");
    }

    private onLoadTerms() {
        this.translationService.translate("economy.export.payments.payment").then((term) => this.paymentTerm = term)
    }

    protected getEditIdentifier(row: any) {
        return row.paymentRowId;
    }

    protected getTitle(row: any) {
        return this.paymentTerm + " " + row.seqNr
    }

    private edit(row: any) {
        if (this.getEditIdentifier(row) > 0) {
            this.tabs.addEditTab(row, PaymentsEditController, { paymentId: this.getEditIdentifier(row) }, this.urlHelperService.getGlobalUrl("Shared/Economy/Supplier/Payments/Views/edit.html"), this.getTitle(row))
        }
    }

    public tabs: ITabHandler;
}