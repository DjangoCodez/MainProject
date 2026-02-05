import { ICoreService } from "../../../../Core/Services/CoreService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { EditAssignmentHelper } from "../../../../Shared/Time/Schedule/Planning/Dialogs/EditAssignment/EditAssignmentHelper";
import { EditShiftHelper } from "../../../../Shared/Time/Schedule/Planning/Dialogs/EditShift/EditShiftHelper";
import { HandleShiftHelper } from "../../../../Shared/Time/Schedule/Planning/Dialogs/HandleShift/HandleShiftHelper";
import { IScheduleService as ISharedScheduleService } from "../../../../Shared/Time/Schedule/ScheduleService";
import { Feature, SettingDataType, SoeModule } from "../../../../Util/CommonEnumerations";
import { ISoeGridOptions } from "../../../../Util/SoeGridOptions";
import { MyShiftsGaugeDTO, UserGaugeSettingDTO } from "../../../Models/DashboardDTOs";
import { WidgetControllerBase } from "../../Base/WidgetBase";

export class MyShiftsGaugeDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return WidgetControllerBase.getWidgetDirective(urlHelperService.getWidgetUrl('MyShifts', 'MyShiftsGauge.html'), MyShiftsGaugeController);
    }
}

class MyShiftsGaugeController extends WidgetControllerBase {
    private editShiftHelper: EditShiftHelper;
    private editAssignmentHelper: EditAssignmentHelper;
    private handleShiftHelper: HandleShiftHelper;

    private soeGridOptions: ISoeGridOptions;
    private myShifts: MyShiftsGaugeDTO[] = [];

    private nbrOfDaysAhead: number = 7; // Default 7 if not set
    private nbrOfDaysBack: number = 0;  // Default 0 if not set
    private employeeId: number = 0;
    private employeeGroupId: number = 0;

    // Terms
    private terms: { [index: string]: string; };
    private shiftTypeLabel: string;

    // Flags
    private isSchedulePlanningMode: boolean = false;
    private isOrderPlanningMode: boolean = false;

    // Permissions
    editShiftPermission: boolean = false;
    showQueuePermission: boolean = false;

    //@ngInject
    constructor(
        $timeout: ng.ITimeoutService,
        $q: ng.IQService,
        private $uibModal,
        private coreService: ICoreService,
        private sharedScheduleService: ISharedScheduleService,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        uiGridConstants: uiGrid.IUiGridConstants) {
        super($timeout, $q, uiGridConstants);

        this.soeGridOptions = this.createGrid();
    }

    protected setup(): ng.IPromise<any> {
        var deferral = this.$q.defer();
        this.widgetHasSettings = true;

        if (this.widgetUserGauge.module === SoeModule.Time)
            this.isSchedulePlanningMode = true;
        else if (this.widgetUserGauge.module === SoeModule.Billing)
            this.isOrderPlanningMode = true;

        this.$q.all([
            this.loadModifyPermissions(),
            this.loadEmployee()
        ]).then(() => {
            this.loadSettings();
            this.setupGrid();
            deferral.resolve();
        });
        return deferral.promise;
    }

    protected setupGrid(): ng.IPromise<any> {
        var keys: string[] = [
            "core.edit",
            "common.day",
            "common.date",
            "common.time",
            "common.status",
            "common.dashboard.myshifts.open",
            "common.dashboard.myshifts.unwanted"
        ];

        if (this.isSchedulePlanningMode) {
            keys.push("common.dashboard.myshifts.title");
            keys.push("common.shifttype");
        } else if (this.isOrderPlanningMode) {
            keys.push("common.dashboard.myassignments.title");
            keys.push("common.ordershifttype");
        }

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;

            if (this.isSchedulePlanningMode) {
                this.widgetTitle = this.terms["common.dashboard.myshifts.title"];
                this.shiftTypeLabel = this.terms["common.shifttype"];
            } else if (this.isOrderPlanningMode) {
                this.widgetTitle = this.terms["common.dashboard.myassignments.title"];
                this.shiftTypeLabel = this.terms["common.ordershifttype"];
            }

            this.soeGridOptions.addColumnText("dayName", this.terms["common.day"], "15%");
            this.soeGridOptions.addColumnDate("date", this.terms["common.date"], "20%");
            this.soeGridOptions.addColumnText("time", this.terms["common.time"], "20%");
            this.soeGridOptions.addColumnText("shiftTypeName", this.shiftTypeLabel, null);
            this.soeGridOptions.addColumnText("shiftUserStatusName", terms["common.status"], null);
            this.soeGridOptions.addColumnEdit(terms["core.edit"], "edit");
        });
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        var featureIds: number[] = [];

        featureIds.push(Feature.Time_Schedule_SchedulePlanning);
        featureIds.push(Feature.Time_Schedule_SchedulePlanningUser_HandleShiftShowQueue);
        featureIds.push(Feature.Billing_Order_Planning);

        return this.coreService.hasModifyPermissions(featureIds).then((x) => {
            this.editShiftPermission = x[Feature.Time_Schedule_SchedulePlanning] || x[Feature.Billing_Order_Planning];
            this.showQueuePermission = x[Feature.Time_Schedule_SchedulePlanningUser_HandleShiftShowQueue];
        });
    }

    private loadEmployee(): ng.IPromise<any> {
        return this.coreService.getIdsForEmployeeAndGroup().then((x) => {
            this.employeeId = x.item1;
            this.employeeGroupId = x.item2;
        });
    }

    protected load() {
        super.load();
        this.coreService.getMyShiftsWidgetData(this.employeeId, new Date().addDays(this.nbrOfDaysBack * -1), new Date().addDays(this.nbrOfDaysAhead)).then((x) => {
            this.myShifts = x;
            this.soeGridOptions.setData(this.myShifts);
            super.loadComplete(this.myShifts.length);
        });
    }

    public loadSettings() {
        var settingNbrOfDaysAhead: UserGaugeSettingDTO = this.getUserGaugeSetting('NbrOfDaysAhead');
        this.nbrOfDaysAhead = (settingNbrOfDaysAhead ? settingNbrOfDaysAhead.intData : 7);

        var settingNbrOfDaysBack: UserGaugeSettingDTO = this.getUserGaugeSetting('NbrOfDaysBack');
        this.nbrOfDaysBack = (settingNbrOfDaysBack ? settingNbrOfDaysBack.intData : 0);
    }

    public saveSettings() {
        var settings: UserGaugeSettingDTO[] = [];
        var settingNbrOfDaysAhead = new UserGaugeSettingDTO('NbrOfDaysAhead', SettingDataType.Integer);
        settingNbrOfDaysAhead.intData = this.nbrOfDaysAhead;
        settings.push(settingNbrOfDaysAhead);
        var settingNbrOfDaysBack = new UserGaugeSettingDTO('NbrOfDaysBack', SettingDataType.Integer);
        settingNbrOfDaysBack.intData = this.nbrOfDaysBack;
        settings.push(settingNbrOfDaysBack);

        this.coreService.saveUserGaugeSettings(this.widgetUserGauge.userGaugeId, settings).then(result => {
            if (result.success)
                this.widgetUserGauge.userGaugeSettings = settings;
        });
    }

    private edit(row: MyShiftsGaugeDTO) {
        if (!row)
            return;

        if (this.isSchedulePlanningMode) {
            // If permitted, open EditShiftDialog, otherwise open HandleShiftDialog
            if (this.editShiftPermission) {
                if (!this.editShiftHelper)
                    this.editShiftHelper = new EditShiftHelper(this.$uibModal, this.$q, this.coreService, this.sharedScheduleService, this.urlHelperService, this.translationService, this.editShiftPermission, row.date, row.date, this.employeeId, !this.editShiftPermission, () => this.openEditShiftDialog(row.timeScheduleTemplateBlockId));
                else
                    this.openEditShiftDialog(row.timeScheduleTemplateBlockId);
            } else if (this.employeeId && this.employeeGroupId) {
                if (!this.handleShiftHelper)
                    this.handleShiftHelper = new HandleShiftHelper(this.$uibModal, this.$q, this.coreService, this.sharedScheduleService, this.urlHelperService, this.isSchedulePlanningMode, this.isOrderPlanningMode, this.employeeId, this.employeeGroupId, () => { this.openHandleShiftDialog(row.timeScheduleTemplateBlockId); });
                else
                    this.openHandleShiftDialog(row.timeScheduleTemplateBlockId);
            }
        } else if (this.isOrderPlanningMode) {
            // If permitted, open EditAssignmentDialog, otherwise open HandleShiftDialog
            if (this.editShiftPermission) {
                if (!this.editAssignmentHelper)
                    this.editAssignmentHelper = new EditAssignmentHelper(this.$uibModal, this.$q, this.coreService, this.sharedScheduleService, this.urlHelperService, this.translationService, this.editShiftPermission, this.employeeId, !this.editShiftPermission, () => this.openEditAssignmentDialog(row.timeScheduleTemplateBlockId));
                else
                    this.openEditAssignmentDialog(row.timeScheduleTemplateBlockId);
            } else if (this.employeeId && this.employeeGroupId) {
                if (!this.handleShiftHelper)
                    this.handleShiftHelper = new HandleShiftHelper(this.$uibModal, this.$q, this.coreService, this.sharedScheduleService, this.urlHelperService, this.isSchedulePlanningMode, this.isOrderPlanningMode, this.employeeId, this.employeeGroupId, () => { this.openHandleShiftDialog(row.timeScheduleTemplateBlockId); });
                else
                    this.openHandleShiftDialog(row.timeScheduleTemplateBlockId);
            }
        }
    }

    private openEditShiftDialog(timeScheduleTemplateBlockId: number) {
        this.editShiftHelper.loadShift(timeScheduleTemplateBlockId).then(shift => {
            this.editShiftHelper.openEditShiftDialog(shift, null, 0, false, false, (result) => {
                if (result.reload && result.reload === true) {
                    this.reload();
                }
            });
        });
    }

    private openHandleShiftDialog(timeScheduleTemplateBlockId: number) {
        this.handleShiftHelper.loadShift(timeScheduleTemplateBlockId).then(shift => {
            this.handleShiftHelper.loadLinkedShifts(shift).then(shifts => {
                this.handleShiftHelper.openHandleShiftDialog(shifts, (result) => {
                    if (result.reload && result.reload === true) {
                        this.reload();
                    }
                });
            });
        });
    }

    private openEditAssignmentDialog(timeScheduleTemplateBlockId: number) {
        // If permitted, open EditAssignmentDialog, otherwise open HandleAssignmentDialog
        this.editAssignmentHelper.loadShift(timeScheduleTemplateBlockId).then(shift => {
            this.editAssignmentHelper.openEditAssignmentDialog(shift, (result) => {
                if (result.reload && result.reload === true) {
                    this.reload();
                }
            });
        });
    }
}