import { ICoreService } from "../../../../Core/Services/CoreService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { EditAssignmentHelper } from "../../../../Shared/Time/Schedule/Planning/Dialogs/EditAssignment/EditAssignmentHelper";
import { EditShiftHelper } from "../../../../Shared/Time/Schedule/Planning/Dialogs/EditShift/EditShiftHelper";
import { HandleShiftHelper } from "../../../../Shared/Time/Schedule/Planning/Dialogs/HandleShift/HandleShiftHelper";
import { IScheduleService as ISharedScheduleService } from "../../../../Shared/Time/Schedule/ScheduleService";
import { CalendarUtility } from "../../../../Util/CalendarUtility";
import { CompanySettingType, Feature, SettingDataType, SoeModule, TimeSchedulePlanningDisplayMode } from "../../../../Util/CommonEnumerations";
import { SettingsUtility } from "../../../../Util/SettingsUtility";
import { UserGaugeSettingDTO } from "../../../Models/DashboardDTOs";
import { EmployeeListDTO } from "../../../Models/EmployeeListDTO";
import { ShiftDTO } from "../../../Models/TimeSchedulePlanningDTOs";
import { WidgetControllerBase } from "../../Base/WidgetBase";

export class MyScheduleGaugeDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return WidgetControllerBase.getWidgetDirective(urlHelperService.getWidgetUrl('MySchedule', 'MyScheduleGauge.html'), MyScheduleGaugeController);
    }
}

class MyScheduleGaugeController extends WidgetControllerBase {
    private editShiftHelper: EditShiftHelper;
    private editAssignmentHelper: EditAssignmentHelper;
    private handleShiftHelper: HandleShiftHelper;

    private dateFrom: Date;
    private dateTo: Date;
    private myShifts: ShiftDTO[];
    private openShifts: ShiftDTO[];
    private colleaguesShifts: ShiftDTO[];

    // Settings
    private nbrOfDaysAhead: number = 7; // Default 7 if not set
    private nbrOfDaysBack: number = 0;  // Default 0 if not set
    private showOpenShifts: boolean = false;
    private showColleaguesShifts: boolean = false;

    private employeeId: number = 0;
    private employeeGroupId: number = 0;
    private employee: EmployeeListDTO;

    // Terms
    private terms: { [index: string]: string; };

    // Flags
    private isSchedulePlanningMode: boolean = false;
    private isOrderPlanningMode: boolean = false;

    // Permissions
    private seeOtherEmployeesShiftsPermission: boolean = false;
    private editShiftPermission: boolean = false;
    private showQueuePermission: boolean = false;

    // Company settings
    private useMultipleScheduleTypes: boolean = false;

    //@ngInject
    constructor(
        $timeout: ng.ITimeoutService,
        $q: ng.IQService,
        private $scope: ng.IScope,
        private $uibModal,
        private coreService: ICoreService,
        private sharedScheduleService: ISharedScheduleService,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        uiGridConstants: uiGrid.IUiGridConstants) {
        super($timeout, $q, uiGridConstants);
    }

    protected setup(): ng.IPromise<any> {
        var deferral = this.$q.defer();
        this.widgetHasSettings = true;
        this.widgetCss = 'col-sm-8';

        if (this.widgetUserGauge.module === SoeModule.Time)
            this.isSchedulePlanningMode = true;
        else if (this.widgetUserGauge.module === SoeModule.Billing)
            this.isOrderPlanningMode = true;

        this.$q.all([
            this.loadTerms(),
            this.loadReadPermissions(),
            this.loadModifyPermissions(),
            this.loadCompanySettings(),
            this.loadEmployeeId()
        ]).then(() => {
            this.loadSettings();

            this.$q.all([
                this.loadEmployee(false)
            ]).then(() => {
                deferral.resolve();
            });
        });
        return deferral.promise;
    }

    private setDateRange() {
        this.dateFrom = CalendarUtility.getDateToday().addDays(-this.nbrOfDaysBack);
        this.dateTo = CalendarUtility.getDateToday().addDays(this.nbrOfDaysAhead);
    }

    // SERVICE CALLS

    protected loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.edit",
            "common.dashboard.myschedule.title",
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;

            this.widgetTitle = this.terms["common.dashboard.myschedule.title"];
        });
    }

    private loadReadPermissions(): ng.IPromise<any> {
        var featureIds: number[] = [];

        featureIds.push(Feature.Time_Schedule_SchedulePlanningUser_SeeOtherEmployeesShifts);
        featureIds.push(Feature.Time_Schedule_SchedulePlanningUser_HandleShiftShowQueue);

        return this.coreService.hasReadOnlyPermissions(featureIds).then((x) => {
            this.seeOtherEmployeesShiftsPermission = x[Feature.Time_Schedule_SchedulePlanningUser_SeeOtherEmployeesShifts];
            this.showQueuePermission = x[Feature.Time_Schedule_SchedulePlanningUser_HandleShiftShowQueue];
        });
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        var featureIds: number[] = [];

        featureIds.push(Feature.Time_Schedule_SchedulePlanning);
        featureIds.push(Feature.Billing_Order_Planning);

        return this.coreService.hasModifyPermissions(featureIds).then((x) => {
            //this.editShiftPermission = x[Feature.Time_Schedule_SchedulePlanning] || x[Feature.Billing_Order_Planning];
            if (this.isOrderPlanningMode)
                this.editShiftPermission = x[Feature.Billing_Order_Planning];
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];

        settingTypes.push(CompanySettingType.UseMultipleScheduleTypes);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useMultipleScheduleTypes = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseMultipleScheduleTypes);
        });
    }

    private loadEmployeeId(): ng.IPromise<any> {
        return this.coreService.getIdsForEmployeeAndGroup().then((x) => {
            this.employeeId = x.item1;
            this.employeeGroupId = x.item2;
        });
    }

    private loadEmployee(render: boolean): ng.IPromise<any> {
        return this.sharedScheduleService.getEmployeesForPlanning([this.employeeId], null, false, false, false, true, false, this.dateFrom, this.dateTo, false, TimeSchedulePlanningDisplayMode.User).then(x => {
            this.employee = (x.length > 0) ? x[0] : null;
            if (!this.employee)
                console.warn("Employee with ID {0} not found".format(this.employeeId.toString()));

            if (render && this.employee)
                this.broadcastRender(false, null);
        });
    }

    protected load() {
        super.load();
        this.loadMySchedule(false);
    }

    private loadMySchedule(keepDatesExpanded: boolean, date: Date = null) {
        this.coreService.getMyScheduleMyShifts(this.employeeId, this.dateFrom, this.dateTo).then(x => {
            this.myShifts = x;

            _.forEach(this.myShifts, shift => {
                if (shift.isAbsenceRequest || shift.isAbsence)
                    shift.shiftTypeColor = "#ef545e";   // @shiftAbsenceBackgroundColor
            });

            if (date && (this.showOpenShifts || this.showColleaguesShifts)) {
                this.loadAdditionalShifts(date, true);
            } else {
                super.loadComplete(this.myShifts.length);
                this.broadcastRender(keepDatesExpanded, null);
            }
        });
    }

    private loadAdditionalShifts(date: Date, forceReload: boolean) {
        var queue = [];
        if (this.showOpenShifts)
            queue.push(this.loadOpenShifts(date, forceReload));
        if (this.showColleaguesShifts)
            queue.push(this.loadMyColleaguesSchedule(date, forceReload));

        this.$q.all(queue).then(() => {
            this.broadcastRender(true, date);
        });
    }

    private loadOpenShifts(date: Date, forceReload: boolean): ng.IPromise<any> {
        var deferral = this.$q.defer();

        if (this.showOpenShifts) {
            // Check if already loaded for specified date
            if (!forceReload && _.filter(this.openShifts, s => s.date.isSameDayAs(date)).length > 0) {
                deferral.resolve();
            } else {
                this.coreService.getMyScheduleOpenShifts(this.employeeId, date, date).then(shifts => {
                    if (!this.openShifts)
                        this.openShifts = [];

                    if (this.openShifts.length > 0) {
                        let blockIds: number[] = _.map(shifts, s => s.timeScheduleTemplateBlockId);
                        _.pullAll(this.openShifts, _.filter(this.openShifts, s => _.includes(blockIds, s.timeScheduleTemplateBlockId)));
                    }
                    this.openShifts.push(...shifts);
                    deferral.resolve();
                });
            }
        } else {
            deferral.resolve();
        }

        return deferral.promise;
    }

    private loadMyColleaguesSchedule(date: Date, forceReload: boolean): ng.IPromise<any> {
        var deferral = this.$q.defer();

        if (this.showColleaguesShifts) {
            // Check if already loaded for specified date
            if (!forceReload && _.filter(this.colleaguesShifts, s => s.date.isSameDayAs(date)).length > 0) {
                deferral.resolve();
            } else {
                this.coreService.getMyScheduleColleaguesShifts(this.employeeId, date, date).then(shifts => {
                    _.forEach(shifts, shift => {
                        if (shift.isAbsenceRequest || shift.isAbsence)
                            shift.shiftTypeColor = "#ef545e";   // @shiftAbsenceBackgroundColor
                    });

                    if (!this.colleaguesShifts)
                        this.colleaguesShifts = [];

                    if (this.colleaguesShifts.length > 0) {
                        let blockIds: number[] = _.map(shifts, s => s.timeScheduleTemplateBlockId);
                        _.pullAll(this.colleaguesShifts, _.filter(this.colleaguesShifts, s => _.includes(blockIds, s.timeScheduleTemplateBlockId)));
                    }
                    this.colleaguesShifts.push(...shifts);
                    deferral.resolve();
                });
            }
        } else {
            deferral.resolve();
        }

        return deferral.promise;
    }

    // SETTINGS

    public loadSettings() {
        var settingNbrOfDaysAhead: UserGaugeSettingDTO = this.getUserGaugeSetting('NbrOfDaysAhead');
        this.nbrOfDaysAhead = (settingNbrOfDaysAhead ? settingNbrOfDaysAhead.intData : 7);
        var settingNbrOfDaysBack: UserGaugeSettingDTO = this.getUserGaugeSetting('NbrOfDaysBack');
        this.nbrOfDaysBack = (settingNbrOfDaysBack ? settingNbrOfDaysBack.intData : 0);

        var settingShowOpenShifts: UserGaugeSettingDTO = this.getUserGaugeSetting('ShowOpenShifts');
        this.showOpenShifts = (settingShowOpenShifts ? settingShowOpenShifts.boolData : false);
        if (this.seeOtherEmployeesShiftsPermission) {
            var settingShowColleaguesShifts: UserGaugeSettingDTO = this.getUserGaugeSetting('ShowColleaguesShifts');
            this.showColleaguesShifts = (settingShowColleaguesShifts ? settingShowColleaguesShifts.boolData : false);
        }

        this.setDateRange();
    }

    public saveSettings() {
        var settings: UserGaugeSettingDTO[] = [];
        var settingNbrOfDaysAhead = new UserGaugeSettingDTO('NbrOfDaysAhead', SettingDataType.Integer);
        settingNbrOfDaysAhead.intData = this.nbrOfDaysAhead;
        settings.push(settingNbrOfDaysAhead);
        var settingNbrOfDaysBack = new UserGaugeSettingDTO('NbrOfDaysBack', SettingDataType.Integer);
        settingNbrOfDaysBack.intData = this.nbrOfDaysBack;
        settings.push(settingNbrOfDaysBack);

        var settingShowOpenShifts = new UserGaugeSettingDTO('ShowOpenShifts', SettingDataType.Boolean);
        settingShowOpenShifts.boolData = this.showOpenShifts;
        settings.push(settingShowOpenShifts);
        var settingShowColleaguesShifts = new UserGaugeSettingDTO('ShowColleaguesShifts', SettingDataType.Boolean);
        settingShowColleaguesShifts.boolData = this.showColleaguesShifts;
        settings.push(settingShowColleaguesShifts);

        this.coreService.saveUserGaugeSettings(this.widgetUserGauge.userGaugeId, settings).then(result => {
            if (result.success)
                this.widgetUserGauge.userGaugeSettings = settings;

            this.setDateRange();
            this.loadMySchedule(false);
        });
    }

    // DIALOGS

    private editShift(shift: ShiftDTO) {
        if (!shift)
            return;

        if (this.isSchedulePlanningMode) {
            // If permitted, open EditShiftDialog, otherwise open HandleShiftDialog
            // If shift belongs to current employee, always open HandleShiftDialog
            if (this.editShiftPermission && this.employeeId !== shift.employeeId) {
                if (!this.editShiftHelper)
                    this.editShiftHelper = new EditShiftHelper(this.$uibModal, this.$q, this.coreService, this.sharedScheduleService, this.urlHelperService, this.translationService, this.editShiftPermission, shift.actualStartTime.date(), shift.actualStopTime.date(), this.employeeId, !this.editShiftPermission, () => this.openEditShiftDialog(shift.timeScheduleTemplateBlockId));
                else
                    this.openEditShiftDialog(shift.timeScheduleTemplateBlockId);
            } else if (this.employeeId && this.employeeGroupId) {
                if (!this.handleShiftHelper)
                    this.handleShiftHelper = new HandleShiftHelper(this.$uibModal, this.$q, this.coreService, this.sharedScheduleService, this.urlHelperService, this.isSchedulePlanningMode, this.isOrderPlanningMode, this.employeeId, this.employeeGroupId, () => { this.openHandleShiftDialog(shift.timeScheduleTemplateBlockId); });
                else
                    this.openHandleShiftDialog(shift.timeScheduleTemplateBlockId);
            }
        } else if (this.isOrderPlanningMode) {
            // If permitted, open EditAssignmentDialog, otherwise open HandleShiftDialog
            if (this.editShiftPermission) {
                if (!this.editAssignmentHelper)
                    this.editAssignmentHelper = new EditAssignmentHelper(this.$uibModal, this.$q, this.coreService, this.sharedScheduleService, this.urlHelperService, this.translationService, this.editShiftPermission, this.employeeId, !this.editShiftPermission, () => this.openEditAssignmentDialog(shift.timeScheduleTemplateBlockId));
                else
                    this.openEditAssignmentDialog(shift.timeScheduleTemplateBlockId);
            } else if (this.employeeId && this.employeeGroupId) {
                if (!this.handleShiftHelper)
                    this.handleShiftHelper = new HandleShiftHelper(this.$uibModal, this.$q, this.coreService, this.sharedScheduleService, this.urlHelperService, this.isSchedulePlanningMode, this.isOrderPlanningMode, this.employeeId, this.employeeGroupId, () => { this.openHandleShiftDialog(shift.timeScheduleTemplateBlockId); });
                else
                    this.openHandleShiftDialog(shift.timeScheduleTemplateBlockId);
            }
        }
    }

    private openEditShiftDialog(timeScheduleTemplateBlockId: number) {
        this.editShiftHelper.loadShift(timeScheduleTemplateBlockId).then(shift => {
            this.editShiftHelper.openEditShiftDialog(shift, null, 0, false, false, (result) => {
                if (result.reload && result.reload === true) {
                    this.loadMySchedule(true, shift ? shift.actualStartTime.date() : null);
                }
            });
        });
    }

    private openHandleShiftDialog(timeScheduleTemplateBlockId: number) {
        this.handleShiftHelper.loadShift(timeScheduleTemplateBlockId).then(shift => {
            this.handleShiftHelper.loadLinkedShifts(shift).then(shifts => {
                this.handleShiftHelper.openHandleShiftDialog(shifts, (result) => {
                    if (result.reload && result.reload === true) {
                        this.loadMySchedule(true, shift ? shift.actualStartTime.date() : null);
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
                    this.loadMySchedule(true, shift ? shift.actualStartTime.date() : null);
                }
            });
        });
    }

    // HELP-METHODS

    private broadcastRender(keepDatesExpanded: boolean, date: Date) {
        this.$timeout(() => {
            this.$scope.$broadcast('renderSchedule', { keepDatesExpanded: keepDatesExpanded, date: date });
        })
    }

    private validateSettings() {
        this.$timeout(() => {
            this.widgetSettingsValid = this['edit'].$valid;
        });
    }
}