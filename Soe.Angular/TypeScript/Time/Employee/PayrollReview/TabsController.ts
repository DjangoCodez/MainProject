import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../Core/Handlers/tabhandlerfactory";
import { GridController } from "./GridController";
import { EditController } from "./EditController";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";


export class TabsController implements ICompositionTabsController {

    //@ngInject
    constructor(private urlHelperService: IUrlHelperService, tabHandlerFactory: ITabHandlerFactory) {

        var part: string = "time.employee.payrollreview.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.payrollReviewHeadId)
            .onGetRowEditName(row => row.name ? row.name : "")
            .onSetupTabs((tabHandler) => {
                tabHandler.addHomeTab(GridController, { isHomeTab: true }, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"));
                tabHandler.enableAddTab(() => this.add());
                tabHandler.enableRemoveAll();
            })
            .onEdit((row, data) => this.edit(row, data))
            .initialize(part + "payrollreview", part + "payrollreview", part + "payrollreview.new");
    }

    private edit(row: any, data: ISmallGenericType[] = null) {
       this.tabs.addEditTab(row, EditController, { navigatorRecords: data });
    }

    private add() {
        this.tabs.addCreateNewTab(EditController, this.urlHelperService.getViewUrl("edit.html"));
    }

    public tabs: ITabHandler;
    //@ngInject
    /*constructor($state: angular.ui.IStateService,
        $stateParams: angular.ui.IStateParamsService,
        $window: ng.IWindowService,
        $timeout: ng.ITimeoutService,
        private $http,
        private $templateCache,
        private $uibModal,
        private $filter,
        translationService: ITranslationService,
        urlHelperService: IUrlHelperService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        private coreService: ICoreService,
        private uiGridConstants: uiGrid.IUiGridConstants,
        private $q: ng.IQService,
        $scope: ng.IScope) {
        super($state, $stateParams, $window, $timeout, translationService, urlHelperService, messagingService, notificationService, $scope);

        // Setup base class
        const part = "time.employee.payrollreview.";
        super.initialize(part + "payrollreview", part + "payrollreview", part + "new");
    }
    protected setupTabs() {
        this.enableRemoveAll();
        this.enableAddTab();
        this.addHomeTab(GridController);
    }

    protected add() {
        this.addCreateNewTab(EditController);
    }

    protected edit(row: any) {
        this.addEditTab(this.getEditName(row), this.getEditIdentifier(row), EditController, { id: this.getEditIdentifier(row) });
    }

    protected getEditIdentifier(row: any): any {
        return row.payrollReviewHeadId;
    }

    protected getEditName(row: any): string {
        return row.name;
    }*/
}
