import { ICoreService } from "../../../Core/Services/CoreService";
import { IEmployeeService } from "../EmployeeService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { EditControllerBase } from "../../../Core/Controllers/EditControllerBase";
import { ToolBarButtonGroup } from "../../../Util/ToolBarUtility";
import { Feature } from "../../../Util/CommonEnumerations";

export class EditController extends EditControllerBase {

    // Data
    employeeGroup: any;

    // ToolBar
    protected gridButtonGroups = new Array<ToolBarButtonGroup>();

    // Lookups

    constructor(
        private employeeGroupId: number,
        $uibModal,
        coreService: ICoreService,
        private employeeService: IEmployeeService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        private uiGridConstants: uiGrid.IUiGridConstants) {

        super("Time.Employee.EmployeeGroups.Edit", Feature.Time_Employee_Groups_Edit, $uibModal, translationService, messagingService, coreService, notificationService, urlHelperService);
    }

    // SETUP

    protected setupLookups() {
        this.lookups = 1;
        this.load();
    }

    private setupToolBar() {
        this.setupDefaultToolBar();
    }

    // LOOKUPS

    private load() {
        this.lookupLoaded();
    }

    // EVENTS

    protected lookupLoaded() {
        super.lookupLoaded();
        if (this.lookups <= 0) {
            this.setupToolBar();
        }
    }

    // ACTIONS

    // HELP-METHODS

    private new() {
        this.isNew = true;
        this.employeeGroupId = 0;
        this.employeeGroup = {};
    }

    // VALIDATION
}
