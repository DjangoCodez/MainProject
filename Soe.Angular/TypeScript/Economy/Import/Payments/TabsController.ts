import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { GridController } from "./GridController";
import { EditController } from "../../../Shared/Economy/Import/Payments/EditController";
import { ImportPaymentType } from "../../../Util/CommonEnumerations";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(
        private urlHelperService: IUrlHelperService,
        tabHandlerFactory: ITabHandlerFactory) {

        let textkeyMultiple: string = "economy.import.payments";
        let textkeySingle: string = "economy.import.payment";
        
        if (soeConfig.importType === ImportPaymentType.CustomerPayment) {
            textkeyMultiple += ".customer";
            textkeySingle += ".customer"
        }
        else if (soeConfig.importType === ImportPaymentType.SupplierPayment) {
            textkeyMultiple += ".supplier";
            textkeySingle += ".supplier"
        }
        else {
            textkeyMultiple = "economy.import.payment.payments";
            textkeySingle = "economy.import.payment.payment"
        }

        const parameters = { importType: soeConfig.importType };

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.paymentImportId)
            .onGetRowEditName(row => row.batchId)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, parameters, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
                tabHandler.enableAddTab(() => this.add());
                tabHandler.enableRemoveAll();
            })
            .onEdit(row => this.edit(row))
            .initialize(textkeySingle, textkeyMultiple, "economy.import.payment.new_payment");
    }

    private edit(row: any) {
        const parameters = { importType: soeConfig.importType };
        this.tabs.addEditTab(row, EditController, parameters, this.urlHelperService.getGlobalUrl('Shared/Economy/Import/Payments/Views/edit.html'));
    }

    private add() {
        const parameters = { importType: soeConfig.importType };

        this.tabs.addCreateNewTab(EditController, this.urlHelperService.getGlobalUrl('Shared/Economy/Import/Payments/Views/edit.html'), parameters);
    }

    public tabs: ITabHandler;
}