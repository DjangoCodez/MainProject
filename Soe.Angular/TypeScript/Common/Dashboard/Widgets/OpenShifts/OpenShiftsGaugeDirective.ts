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
import { OpenShiftsGaugeDTO, UserGaugeSettingDTO } from "../../../Models/DashboardDTOs";
import { WidgetControllerBase } from "../../Base/WidgetBase";

export class OpenShiftsGaugeDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return WidgetControllerBase.getWidgetDirective(urlHelperService.getWidgetUrl('OpenShifts', 'OpenShiftsGauge.html'), OpenShiftsGaugeController);
    }
}

class OpenShiftsGaugeController extends WidgetControllerBase {
    private editShiftHelper: EditShiftHelper;
    private editAssignmentHelper: EditAssignmentHelper;
    private handleShiftHelper: HandleShiftHelper;

    private soeGridOptions: ISoeGridOptions;
    private openShifts: OpenShiftsGaugeDTO[] = [];
    private selectedShift: OpenShiftsGaugeDTO;

    private nbrOfDaysAhead: number = 28;    // Default 28 if not set
    private employeeId: number = 0;
    private employeeGroupId: number = 0;

    // Terms
    private terms: { [index: string]: string; };
    private shiftTypeLabel: string;

    // Flags
    private isSchedulePlanningMode: boolean = false;
    private isOrderPlanningMode: boolean = false;

    // Permissions
    private editShiftPermission: boolean = false;
    private showQueuePermission: boolean = false;

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
            this.setupGrid().then(() => {
                deferral.resolve();
            });
        });
        return deferral.promise;
    }

    protected setupGrid(): ng.IPromise<any> {
        var keys: string[] = [
            "core.edit",
            "common.day",
            "common.date",
            "common.time",
            "common.type",
            "common.dashboard.openshifts.inqueue",
            "common.dashboard.openshifts.iaminqueue",
            "common.dashboard.openshifts.open",
            "common.dashboard.openshifts.unwanted",
            "common.dashboard.openshifts.linked"
        ];

        if (this.isSchedulePlanningMode) {
            keys.push("common.dashboard.openshifts.title");
            keys.push("common.shifttype");
        } else if (this.isOrderPlanningMode) {
            keys.push("common.dashboard.openassignments.title");
            keys.push("common.ordershifttype");
        }

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;

            if (this.isSchedulePlanningMode) {
                this.widgetTitle = this.terms["common.dashboard.openshifts.title"];
                this.shiftTypeLabel = this.terms["common.shifttype"];
            } else if (this.isOrderPlanningMode) {
                this.widgetTitle = this.terms["common.dashboard.openassignments.title"];
                this.shiftTypeLabel = this.terms["common.ordershifttype"];
            }

            this.soeGridOptions.addColumnIcon("", "fal fa-link", this.terms["common.dashboard.openshifts.linked"], "selectShift", null, "showLinkedIcon");
            this.soeGridOptions.addColumnText("dayName", this.terms["common.day"], "15%", false, null, null, "linked-shifts", "isLinked");
            this.soeGridOptions.addColumnText("dateString", this.terms["common.date"], "20%", false, null, null, "linked-shifts", "isLinked");  // class on date column doesn't work
            this.soeGridOptions.addColumnText("time", this.terms["common.time"], "20%", false, null, null, "linked-shifts", "isLinked");
            this.soeGridOptions.addColumnText("shiftTypeName", this.shiftTypeLabel, null, false, null, null, "linked-shifts", "isLinked");
            this.soeGridOptions.addColumnText("openTypeName", terms["common.type"], null, false, null, null, "linked-shifts", "isLinked");
            if (this.showQueuePermission)
                this.soeGridOptions.addColumnNumber("nbrInQueue", terms["common.dashboard.openshifts.inqueue"], "10%", true, null, null, null, null, null, false, null, null, "linked-shifts", "isLinked");
            this.soeGridOptions.addColumnIcon(null, "fal fa-users", terms["common.dashboard.openshifts.iaminqueue"], null, "iamInQueue");
            this.soeGridOptions.addColumnEdit(terms["core.edit"], "edit");
        });
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        let featureIds: number[] = [];
        featureIds.push(Feature.Time_Schedule_SchedulePlanning);
        featureIds.push(Feature.Time_Schedule_SchedulePlanningUser_HandleShiftShowQueue);
        featureIds.push(Feature.Billing_Order_Planning);

        return this.coreService.hasModifyPermissions(featureIds).then((x) => {
            //this.editShiftPermission = x[Feature.Time_Schedule_SchedulePlanning] || x[Feature.Billing_Order_Planning];
            if (this.isOrderPlanningMode)
                this.editShiftPermission = x[Feature.Billing_Order_Planning];
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
        this.coreService.getOpenShiftsWidgetData(this.employeeId, new Date(), new Date().addDays(this.nbrOfDaysAhead)).then((x) => {
            this.openShifts = x;
            _.forEach(this.openShifts, (row: OpenShiftsGaugeDTO) => {
                switch (row.openType) {
                    case 1:
                        row.openTypeName = this.terms["common.dashboard.openshifts.open"];
                        break;
                    case 2:
                        row.openTypeName = this.terms["common.dashboard.openshifts.unwanted"];
                        break;
                }
            });
            this.soeGridOptions.setData(this.openShifts);
            super.loadComplete(this.openShifts.length);
        });
    }

    public loadSettings() {
        var settingNbrOfDaysAhead: UserGaugeSettingDTO = this.getUserGaugeSetting('NbrOfDaysAhead');
        this.nbrOfDaysAhead = (settingNbrOfDaysAhead ? settingNbrOfDaysAhead.intData : 28);
    }

    public saveSettings() {
        var settings: UserGaugeSettingDTO[] = [];
        var settingNbrOfDaysAhead = new UserGaugeSettingDTO('NbrOfDaysAhead', SettingDataType.Integer);
        settingNbrOfDaysAhead.intData = this.nbrOfDaysAhead;
        settings.push(settingNbrOfDaysAhead);

        this.coreService.saveUserGaugeSettings(this.widgetUserGauge.userGaugeId, settings).then(result => {
            if (result.success)
                this.widgetUserGauge.userGaugeSettings = settings;
        });
    }

    private edit(row: OpenShiftsGaugeDTO) {
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
            this.editShiftHelper.openEditShiftDialog(shift, null, 0, true, false, (result) => {
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

    private showLinkedIcon(row: OpenShiftsGaugeDTO): boolean {
        if (row.link) {
            return _.filter(this.openShifts, s => s.link === row.link).length > 1;
        }

        return false;
    }

    private selectShift(row: OpenShiftsGaugeDTO) {
        _.forEach(this.openShifts, s => {
            s['linked'] = s.link === row.link;
        });
    }

    private isLinked(row: OpenShiftsGaugeDTO): boolean {
        return row['linked'];
    }
}