import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { Guid } from "../../../Util/StringUtility";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { GridController } from "./GridController";
import { Constants } from "../../../Util/Constants";

export class TabsController implements ICompositionTabsController {

    public tabs: ITabHandler;

    private accountDimTabGuid: Guid;
    private accountDim: any;
    private term_new: string;

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService,
        tabHandlerFactory: ITabHandlerFactory,
        private accountingService: IAccountingService,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService) {

        var part: string = "economy.accounting.balance.balance";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.accountId)
            .onGetRowEditName(row => row.accountNr)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
                tabHandler.enableRemoveAll();
            })
            .onEdit(row => this.edit(row));

            this.tabs.initialize(part, part, part);

        this.messagingService.subscribe(Constants.EVENT_OPEN_ACCOUNTDIM, (x) => {
            /*if (x.accountDimId)
                this.openAccountDimEdit(x.accountDimId, x.accountDimName);*/
        });
    }

    private edit(row: any) {
    }

    private add() {
    }
}