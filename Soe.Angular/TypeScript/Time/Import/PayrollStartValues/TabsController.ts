import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { GridController } from "./GridController";
import { EditController } from "./EditController";
import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../Core/Handlers/tabhandlerfactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { Feature } from "../../../Util/CommonEnumerations";
import { ICoreService } from "../../../Core/Services/CoreService";

export class TabsController implements ICompositionTabsController {

    private importPermission: boolean = false;

    //@ngInject
    constructor(private coreService: ICoreService, private urlHelperService: IUrlHelperService, tabHandlerFactory: ITabHandlerFactory) {
        var part: string = "time.import.payrollstartvalue.";

        this.loadModifyPermissions().then(() => {
            this.tabs = tabHandlerFactory.create()
                .onGetRowIdentifier(row => row.payrollStartValueHeadId)
                .onGetRowEditName(row => row.importedFrom)
                .onSetupTabs((tabHandler) => {
                    tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
                    if (this.importPermission)
                        tabHandler.enableAddTab(() => this.add());
                    tabHandler.enableRemoveAll();
                })
                .onEdit((row) => this.edit(row))
                .initialize(part + "payrollstartvalue", part + "payrollstartvalues", part + "new");
        });
    }

    private edit(row: any) {
        // Open edit page
        this.tabs.addEditTab(row, EditController);
    }

    private add() {
        this.tabs.addCreateNewTab(EditController, this.urlHelperService.getViewUrl("edit.html"));
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        let features: number[] = [];
        features.push(Feature.Time_Import_PayrollStartValuesImport);

        return this.coreService.hasModifyPermissions(features).then((x) => {
            this.importPermission = x[Feature.Time_Import_PayrollStartValuesImport];
        });
    }

    public tabs: ITabHandler;
}