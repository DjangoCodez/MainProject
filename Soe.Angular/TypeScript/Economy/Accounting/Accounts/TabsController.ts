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
import { EditController } from "./EditController";
import { EditController as AccountDimsEditController} from "../../../Economy/Accounting/AccountDims/EditController";
import { Constants } from "../../../Util/Constants";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";

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

        var part: string = "economy.accounting.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.accountId)
            .onGetRowEditName(row => row.accountNr)
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
                tabHandler.enableAddTab(() => this.add());
                tabHandler.enableRemoveAll();
            })
            .onEdit((row, data) => this.edit(row, data))

        if (!soeConfig.isStdAccount && soeConfig.accountDimId > 0) {
            this.translationService.translate("common.new").then((term) => {
                this.term_new = term;

                this.accountingService.getAccountDim(soeConfig.accountDimId, false, false, false).then((x) => {
                    this.accountDim = x;

                    this.tabs.initialize(this.accountDim.name, this.accountDim.name, this.term_new + " " + this.accountDim.name);
                });
            });
        } else {
            this.tabs.initialize(part + "account", part + "accounts", part + "newaccount");
        }

        this.messagingService.subscribe(Constants.EVENT_OPEN_ACCOUNTDIM, (x) => {
            if (x.accountDimId)
                this.openAccountDimEdit(x.accountDimId, x.accountDimName);
        });
    }

    private edit(row: any, data: ISmallGenericType[] = null) {
        this.tabs.addEditTab(row, EditController, { accountDimId: soeConfig.accountDimId, navigatorRecords: data  });
    }

    private add() {
        this.tabs.addCreateNewTab(EditController, this.urlHelperService.getViewUrl("edit.html"), { accountDimId: soeConfig.accountDimId });
    }

    private openAccountDimEdit(accountDimId: number, accountDimName: string) {
        var parameters = { id: accountDimId, isStdAccountDim: true, guid: this.accountDimTabGuid };
        this.tabs.addNewTab(accountDimName, null, AccountDimsEditController, this.urlHelperService.getGlobalUrl('Economy/Accounting/AccountDims/Views/edit.html'), parameters, true, true);
    }
}