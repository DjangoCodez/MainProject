import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { ITabHandlerFactory } from "../../../Core/Handlers/TabHandlerFactory";
import { GridController } from "./GridController";
import { EditController } from "./EditController";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { Guid } from "../../../Util/StringUtility";

export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService, private translationService: ITranslationService, tabHandlerFactory: ITabHandlerFactory) {
        let part: string = "time.employee.position.";
        this.translationService.translate("time.employee.position.syspositions").then(term => {
            this.tabs = tabHandlerFactory.create()
                .onGetRowIdentifier(row => row.positionId)
                .onGetRowEditName(row => row.name)
                .onSetupTabs((tabHandler) => {
                    tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"), soeConfig.tabHeader);
                    tabHandler.addNewTab(term, new Guid(), GridController, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"), { type: "Sys" }, false);
                    tabHandler.enableAddTab(() => this.add());
                    tabHandler.enableRemoveAll();
                })
                .onEdit((row, data) => this.edit(row, data))
                .initialize(part + "position", part + "positions", part + "new_position");

        });
    }

    private edit(row: any, data: ISmallGenericType[] = null) {
        this.tabs.addEditTab(row, EditController, { navigatorRecords: data });
    }

    private add() {
        this.tabs.addCreateNewTab(EditController, this.urlHelperService.getViewUrl("edit.html"));
    }

    public tabs: ITabHandler;
}