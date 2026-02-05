import { GridControllerBase } from "../../../Core/Controllers/GridControllerBase";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { IconLibrary, SoeGridOptionsEvent, InsecureDebtsButtonFunctions } from "../../../Util/Enumerations";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { IImportService } from "../ImportService";
import { Feature } from "../../../Util/CommonEnumerations";

export class GridController extends GridControllerBase {

    //@ngInject
    constructor($http,
        $templateCache,
        $timeout: ng.ITimeoutService,
        $uibModal,
        private $filter: ng.IFilterService,
        coreService: ICoreService,
        private importService: IImportService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        uiGridConstants: uiGrid.IUiGridConstants) {

        super("Time.Employee.Csr.Import", "time.employee.csr.imports", Feature.Time_Employee_Csr_Import, $http, $templateCache, $timeout, $uibModal, coreService, translationService, urlHelperService, messagingService, notificationService, uiGridConstants);
    }

    protected loadLookups() {
    }

    public setupGrid() {
        // Columns

    }

    public loadGridData() {
        // Load data

    }

    public createFile() {


    }

    public updateItemsSelection() {

    }
}
