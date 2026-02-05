import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { GridController } from "./GridController";
import { SoeAccountDistributionType } from "../../../Util/CommonEnumerations";
import { GridController as DistributionGridController } from "../AccountDistribution/GridController"
import { EditController as DistributionEditController } from "../../../Shared/Economy/Accounting/AccountDistribution/EditController"
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService,
        tabHandlerFactory: ITabHandlerFactory,
        private translationService: ITranslationService,) {

        if (soeConfig.accountDistributionType == SoeAccountDistributionType.Period) {
            var part: string = "economy.accounting.accountdistributionentry.";

            this.tabs = tabHandlerFactory.create()
                .onGetRowIdentifier(row => row.accountDistributionHeadId)
                .onGetRowEditName(row => row.accountDistributionHeadName ? row.accountDistributionHeadName : row.name)
                .onSetupTabs((tabHandler) => {
                    return this.translationService.translate("economy.accounting.accountdistribution.accountdistributions").then((term) => {
                        tabHandler.addHomeTab(GridController, { isHomeTab: true, activeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAgTotals.html"));
                        tabHandler.addHomeTab(DistributionGridController, { isHomeTab: true, activeTab: false, accountDistributionType: SoeAccountDistributionType.Period }, this.urlHelperService.getCoreViewUrl("gridCompositionAgTotals.html"), term);
                        tabHandler.enableAddTab(() => this.add());
                        tabHandler.enableRemoveAll();
                    });
                })
                .onEdit((row, data) => this.edit(row, data))
                .initialize(part + "entry", part + "entries", part + "new");        
        }
        else {
            var part: string = "economy.inventory.accountdistributionentry.";

            this.tabs = tabHandlerFactory.create()
                .onGetRowIdentifier(row => row.accountDistributionEntryId)
                .onGetRowEditName(row => row.name)
                .onSetupTabs((tabHandler) => {
                    tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAgTotals.html"));
                })
                .initialize(part + "entry", part + "entries", part + "new");      
        }   
    }

    private edit(row: any, data: ISmallGenericType[] = null) {
        // Check
        if (row.row) 
            row = row.row;
        
        // Open edit page
        return this.translationService.translate("economy.accounting.accountdistribution.accountdistribution").then((term) => {
            this.tabs.addEditTab(row, DistributionEditController, { id: row.accountDistributionHeadId, accountDistributionType: SoeAccountDistributionType.Period, navigatorRecords: data }, this.urlHelperService.getGlobalUrl("Shared/Economy/Accounting/AccountDistribution/Views/edit.html"), term);
        });
    }
    private add() {
        return this.translationService.translate("economy.accounting.accountdistribution.new").then((term) => {
            this.tabs.addCreateNewTab(DistributionEditController, this.urlHelperService.getGlobalUrl("Shared/Economy/Accounting/AccountDistribution/Views/edit.html"), {}, term);
        });
    }

    public tabs: ITabHandler;
}