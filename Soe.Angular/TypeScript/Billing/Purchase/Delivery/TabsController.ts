import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { GridController } from "./GridController";
import { EditController } from "../../../Shared/Billing/Purchase/Delivery/EditController";
import { EditController as EditPurchaseController } from "../../../Shared/Billing/Purchase/Purchase/EditController";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../Core/Handlers/tabhandlerfactory";
import { ITranslationService } from "../../../Core/Services/TranslationService";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService, tabHandlerFactory: ITabHandlerFactory, private translationService: ITranslationService) {

        const part = "billing.purchase.delivery.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => this.getEditIdentifier(row))
            .onGetRowEditName(row => this.getRowEditName(row))
            .onSetupTabs((tabHandler) => {
                const keys: string[] = [
                    "billing.purchase.delivery.awaitingdelivery",
                    "billing.purchase.delivery.deliveries",
                ];
                const gridUrl = this.urlHelperService.getCoreViewUrl("gridCompositionAg.html");

                this.translationService.translateMany(keys).then((terms) => {
                    tabHandler.addNewTab(terms["billing.purchase.delivery.awaitingdelivery"], null, GridController, gridUrl, { awaitingDelivery: true }, false, true);
                    tabHandler.addNewTab(terms["billing.purchase.delivery.deliveries"], null, GridController, gridUrl, { awaitingDelivery: false }, false);
                    tabHandler.enableAddTab(() => this.add());
                    tabHandler.enableRemoveAll();
                });
            })
            .onEdit(row => this.edit(row))
            .initialize(part + "delivery", part + "deliveries", part + "new_delivery");
    }

    protected getRowEditName(row: any)
    {
        return row.deliveryNr ?? row.row.deliveryNr;
    }

    protected getEditIdentifier(row: any): any {
        if (row && (row.createDelivery || row.isDelivery)) {
            return row.purchaseDeliveryId ?? row.row?.purchaseDeliveryId
        }
        else {
            return row.purchaseId ?? row.row?.purchaseId
        }
    }

    private edit(params) {
        if (params.row) {
            const keys: string[] = [
                "billing.purchase.delivery.delivery",
                "billing.purchase.list.purchase",
            ];

            this.translationService.translateMany(keys).then((terms) => {
                if (params.isDelivery) {
                    this.tabs.addEditTab(params, EditController, { id: this.getEditIdentifier(params.row), type: "delivery" }, this.urlHelperService.getViewUrl("edit.html"), terms["billing.purchase.delivery.delivery"]);
                }
                else if (params.createDelivery)
                    this.tabs.addCreateNewTab(EditController, this.urlHelperService.getViewUrl("edit.html"), {id:0, purchaseId: params.row.purchaseId, type: "delivery" });
                else
                    this.tabs.addEditTab(params, EditPurchaseController, { id: params.row.purchaseId, type: "purchase" }, this.urlHelperService.getGlobalUrl('Billing/Purchase/Purchase/Views/edit.html'), terms["billing.purchase.list.purchase"] + " " + params.row.purchaseNr);
            });
        }
    }

    private add() {
        this.tabs.addCreateNewTab(EditController, this.urlHelperService.getViewUrl("edit.html"));
    }

    public tabs: ITabHandler;
}