import { EditControllerBase } from "../../../Core/Controllers/EditControllerBase";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ToolBarButtonGroup } from "../../../Util/ToolBarUtility";
import { IScheduleService } from "../ScheduleService";
import { Feature } from "../../../Util/CommonEnumerations";

export class EditController extends EditControllerBase {

    // Data
    employeeGroup: any;

    // ToolBar
    protected gridButtonGroups = new Array<ToolBarButtonGroup>();

    // Lookups

    //@ngInject
    constructor(
        private employeeGroupId: number,
        $uibModal,
        coreService: ICoreService,
        private scheduleService: IScheduleService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        private uiGridConstants: uiGrid.IUiGridConstants) {

        super("Time.Schedule.HalfDays", Feature.Time_Preferences_ScheduleSettings_Halfdays_Edit, $uibModal, translationService, messagingService, coreService, notificationService, urlHelperService);
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
