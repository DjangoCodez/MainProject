import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { AttestEmployeeDayDTO, AttestEmployeeDayTimeStampDTO, AttestEmployeeDayTimeBlockDTO } from "../../../Common/Models/TimeEmployeeTreeDTO";
import { SOEMessageBoxImage } from "../../../Util/Enumerations";
import { ShiftDTO } from "../../../Common/Models/TimeSchedulePlanningDTOs";
import { ScheduleHandler } from "./ScheduleHandler";
import { TranslationService } from "../../../Core/Services/TranslationService";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { TimeAttestTimeStampDTO } from "../../../Common/Models/TimeAttestDTOs";
import { TimeStampEntryType, Feature, CompanySettingType, SoeValidateDeviationChangeResultCode, SoeTimeBlockClientChange, TermGroup_TimeReportType } from "../../../Util/CommonEnumerations";
import { EditShiftHelper } from "../../../Shared/Time/Schedule/Planning/Dialogs/EditShift/EditShiftHelper";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IScheduleService as ISharedScheduleService } from "../../../Shared/Time/Schedule/ScheduleService";
import { IScheduleService } from "../../Schedule/ScheduleService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { Constants } from "../../../Util/Constants";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { DragDropHelper } from "./DragDropHelper";
import { ITimeService } from "../../Time/TimeService";
import { ITimeDeviationCauseGridDTO } from "../../../Scripts/TypeLite.Net4";
import { SelectTimeDeviationCauseController } from "./Dialogs/SelectTimeDeviationCause/SelectTimeDeviationCauseController";
import { TimeBlockDialogController } from "./Dialogs/TimeBlockDialog/TimeBlockDialogController";
import { DeleteTimeBlockDialogController } from "./Dialogs/DeleteTimeBlockDialog/DeleteTimeBlockDialogController";
import { ProjectTimeBlockDTO } from "../../../Common/Models/ProjectDTO";
import { NumberUtility } from "../../../Util/NumberUtility";
import { TimeDeviationCauseGridDTO } from "../../../Common/Models/TimeDeviationCauseDTOs";
import { IProgressHandler } from "../../../Core/Handlers/ProgressHandler";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { PayrollImportEmployeeTransactionDTO } from "../../../Common/Models/PayrollImport";
import { AccountingSettingsRowDTO } from "../../../Common/Models/AccountingSettingsRowDTO";

export class ScheduleDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Directives/AttestEmployee/Views/Schedule.html'),
            scope: {
                isModal: '=?',
                showMultipleDays: '=?',
                model: '=',
                showTimeStamp: '=?',
                showTransaction: '=?',
                showProjectTimeBlock: '=?',
                timeStampIsModified: '=?',
                onTimeStampSelected: '&',
                reloadingTimeBlocks: '=?',
                validatingTimeBlocks: '=?',
                timeBlocksIsModified: '=?',
                onResizeNeeded: '&',
            },
            restrict: 'E',
            replace: true,
            controller: ScheduleController,
            controllerAs: 'ctrl',
            bindToController: true,
        };
    }
}

export class ScheduleController {

    // Init parameters
    private isModal: boolean;
    private showMultipleDays: boolean;
    public model: AttestEmployeeDayDTO;

    private _showTimeStamp: boolean;
    private get showTimeStamp(): boolean {
        return this._showTimeStamp;
    }
    private set showTimeStamp(value: boolean) {
        this._showTimeStamp = value;
        this.renderScheduleOnResize();
    }

    private _showTransaction: boolean;
    private get showTransaction(): boolean {
        return this._showTransaction;
    }
    private set showTransaction(value: boolean) {
        this._showTransaction = value;
        this.renderScheduleOnResize();
    }

    private _showProjectTimeBlock: boolean;
    private get showProjectTimeBlock(): boolean {
        return this._showProjectTimeBlock;
    }
    private set showProjectTimeBlock(value: boolean) {
        this._showProjectTimeBlock = value;
        this.renderScheduleOnResize();
    }

    private onTimeStampSelected: Function;
    public timeStampIsModified: boolean;
    private reloadingTimeBlocks: boolean;
    private validatingTimeBlocks: boolean;
    public timeBlocksIsModified: boolean;
    private onResizeNeeded: Function;

    // Handlers
    private scheduleHandler: ScheduleHandler;
    private dragDropHelper: DragDropHelper;
    private editShiftHelper: EditShiftHelper;

    // Data
    public terms: { [index: string]: string; };
    public dates: DateDay[];
    public timeStamps: TimeAttestTimeStampDTO[];
    public projectTimeBlocks: ProjectTimeBlockDTO[];
    private deviationCauses: TimeDeviationCauseGridDTO[] = [];
    private validAccountIds: number[] = [];

    // Permissions
    private isEmployeeCurrentUser: boolean = false;
    public editShiftPermission: boolean = false;

    // Settings
    private showTimeHeader: boolean = true;
    public dayViewStartTime: number = 0;   // Minutes from midnight
    public dayViewEndTime: number = 0;     // Minutes from midnight
    public dateFrom: Date;
    public dateTo: Date;

    // Company settings
    public defaultTimeCodeId: number = 0;
    private useAccountHierarchy: boolean = false;
    private useMultipleScheduleTypes: boolean = false;

    // Flags
    private openingEditShift: boolean = false;
    public renderTimeStamps: boolean = false;
    public renderProjectTimeblocks: boolean = false;
    public hasTimeStampWarnings: boolean = false;
    public hasTimeStampErrors: boolean = false;
    private showAccountDim2 = false;

    // Properties
    public get tableId(): string {
        if (!this.model.date)
            return '';

        let id: string;
        if (!this.isModal)
            id = "table{0}".format(this.model.date.toFormattedDate("YYYYMMDD"));
        else if (!this.showMultipleDays)
            id = "dialog_table";
        else
            id = "dialog_table{0}".format(this.model.date.toFormattedDate("YYYYMMDD"));

        return id;
    }

    private get startHour(): number {
        return this.dayViewStartTime / 60;
    }
    private get endHour(): number {
        return this.dayViewEndTime / 60;
    }
    private get nbrOfVisibleHours(): number {
        let hours: number = this.endHour - this.startHour;
        if (hours <= 0)
            hours += 24;

        return hours;
    }

    private progress: IProgressHandler;

    // Modal
    private editShiftModal: any;

    //@ngInject
    constructor(
        private $uibModal,
        private coreService: ICoreService,
        private sharedScheduleService: ISharedScheduleService,
        private scheduleService: IScheduleService,
        private timeService: ITimeService,
        private urlHelperService: IUrlHelperService,
        private translationService: TranslationService,
        private messagingService: IMessagingService,
        private notificationService: INotificationService,
        private progressHandlerFactory: IProgressHandlerFactory,
        private $filter: ng.IFilterService,
        private $timeout: ng.ITimeoutService,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $compile: ng.ICompileService) {

        if (progressHandlerFactory)
            this.progress = progressHandlerFactory.create();
    }

    public $onInit() {
        this.$q.all([
            this.loadTerms(),
            this.getIsEmployeeCurrentUser()
        ]).then(() => {
            this.$q.all([
                this.loadModifyPermissions(),
                this.loadCompanySettings(),
                this.loadIsUsingAccountDims(),
                this.loadValidAccounts()
            ]).then(() => {
                this.renderTimeStamps = (!this.model.autogenTimeblocks) && (this.model.timeReportType !== TermGroup_TimeReportType.ERP);
                this.renderProjectTimeblocks = (this.model.timeReportType == TermGroup_TimeReportType.ERP);
                this.scheduleHandler = new ScheduleHandler(this, this.$filter, this.$timeout, this.$q, this.$scope, this.$compile);
                this.dragDropHelper = new DragDropHelper(this, this.scheduleHandler, this.$filter);

                this.renderAll();
                this.setupWatchers();

                if (this.isModal) {
                    if (this.showMultipleDays) {
                        if (this.showTimeStamp)
                            this.toggleTimeStamp(false);
                        if (this.showTransaction)
                            this.toggleTimeBlock(false);
                    } else {
                        if (!this.showTimeStamp && this.renderTimeStamps)
                            this.toggleTimeStamp(false);
                        if (!this.showTransaction)
                            this.toggleTimeBlock(false);
                    }
                }

                this.resizeContainer();
            });
        });
    }

    private renderAll(useDelay: boolean = true) {
        this.getTimes();
        this.setDateRange();

        // Schedule
        this.setShiftToolTips();

        // Time stamps
        if (this.renderTimeStamps)
            this.createTimeStamps();

        //Project
        if (this.renderProjectTimeblocks)
            this.projectTimeBlocks = this.model.projectTimeBlocks;

        // Time blocks
        this.setTimeBlockToolTips();

        // Render GUI
        this.scheduleHandler.renderSchedule(useDelay);
    }

    private setupWatchers() {
        this.messagingService.subscribe(Constants.EVENT_SAVE_SHIFTS, (data: any) => {
            let shifts: ShiftDTO[] = data.shifts;
            if (shifts && shifts.length > 0) {
                let shiftDate = shifts[0].actualDateOnLoad ? shifts[0].actualDateOnLoad : shifts[0].startTime;
                if (shiftDate && shiftDate.isSameDayAs(this.model.date)) {
                    this.saveShifts(data.guid, shifts);
                }
            }
        }, this.$scope);

        // TimeStamp entry was selected in time stamp list
        // Mark selected block in GUI
        this.$scope.$on('SelectedTimeStampChanged', (event, data) => {
            var timeStampEntry: AttestEmployeeDayTimeStampDTO = data.timeStampEntry;
            if (timeStampEntry) {
                var timeStamp = _.find(this.timeStamps, t => t.timeStampEntryId === timeStampEntry.identifier);
                if (!timeStamp)
                    timeStamp = _.find(this.timeStamps, t => t.stampOutId === timeStampEntry.identifier);
                if (timeStamp)
                    this.scheduleHandler.selectTimeStamp(timeStamp, false);
            }
        });

        // TimeStamp entry collection was modified, repaint GUI
        this.$scope.$on('TimeStampChanged', (event, data) => {
            if (this.renderTimeStamps)
                this.recreateTimeStamps();
        });

        this.$scope.$on('CreateTimeBlockFromPayrollImport', (event, data) => {
            let trans: PayrollImportEmployeeTransactionDTO = data.trans;
            if (trans)
                this.editTimeBlock(null, null, false, false, trans);
        });

        // Time blocks updated (after save), need to repaint
        this.$scope.$on(Constants.EVENT_ATTESTEMPLOYEE_CONTENT_DAY_CHANGED, (event, data) => {
            // Will be triggered inside grid
            if (this.model.employeeId === data.employeeId && this.model.date.isSameDayAs(data.date))
                this.employeeContentChanged();
        });
        const subscription = this.messagingService.subscribe(Constants.EVENT_ATTESTEMPLOYEE_CONTENT_DAY_CHANGED, (data: { employeeId: number, date: Date }) => {
            // Will be triggered in dialog
            if (data.employeeId === this.model.employeeId && data.date.isSameDayAs(this.model.date))
                this.employeeContentChanged();
        });

        // Move between days in dialog
        this.$scope.$on('DateChanged', (event, data) => {
            if (this.isModal) {
                this.model = data;
                this.renderAll(false);
            }
        });

        this.$scope.$on('ExpandAllDays', (event, data) => {
            if (!this.showTransaction)
                this.toggleTimeBlock();
        });

        this.$scope.$on('CollapseAllDays', (event, data) => {
            if (this.showTransaction)
                this.toggleTimeBlock();
        });

        this.$scope.$on('$destroy', () => {
            subscription.unsubscribe();
        });
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.savefailed",
            "common.week",
            "common.weekshort",
            "error.default_error",
            "time.time.attest.schedule",
            "time.time.attest.schedule.edit",
            "time.time.attest.timestamps",
            "time.time.attest.timeblocks",
            "time.time.attest.timeblocks.add",
            "time.time.attest.timeblocks.savetocalculate",
            "time.schedule.planning.breakprefix",
            "time.schedule.planning.breaklabel",
            "time.schedule.planning.wholedaylabel",
            "time.schedule.planning.todaysschedule",
            "time.schedule.planning.scheduletime",
            "time.schedule.planning.scheduletypefactortime",
            "time.schedule.planning.thisshift",
            "time.time.attest.registeredtime"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private getIsEmployeeCurrentUser(): ng.IPromise<any> {
        return this.timeService.isEmployeeCurrentUser(this.model.employeeId).then(x => {
            this.isEmployeeCurrentUser = x;
        });
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        var featureIds: number[] = [];

        if (this.isEmployeeCurrentUser)
            featureIds.push(Feature.Time_Time_AttestUser_EditSchedule);
        else
            featureIds.push(Feature.Time_Time_Attest_EditSchedule);

        return this.coreService.hasModifyPermissions(featureIds).then((x) => {
            if (this.isEmployeeCurrentUser)
                this.editShiftPermission = x[Feature.Time_Time_AttestUser_EditSchedule];
            else
                this.editShiftPermission = x[Feature.Time_Time_Attest_EditSchedule];
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];

        settingTypes.push(CompanySettingType.TimeDefaultTimeCode);
        settingTypes.push(CompanySettingType.UseAccountHierarchy);
        settingTypes.push(CompanySettingType.UseMultipleScheduleTypes);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.defaultTimeCodeId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeDefaultTimeCode);
            this.useAccountHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);
            this.useMultipleScheduleTypes = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseMultipleScheduleTypes);
        });
    }

    private loadIsUsingAccountDims(): ng.IPromise<any> {
        return this.timeService.getTimeTerminalAccountDim(0, 2).then(x => {
            if (x?.accountDimId) {
                this.showAccountDim2 = true;
            }
        });
    }

    private loadValidAccounts(): ng.IPromise<any> {
        return this.coreService.getAccountIdsFromHierarchyByUser(this.model.date, this.model.date, false, false, false, true, false, false, this.model.employeeId).then(x => {
            this.validAccountIds = x;
        });
    }

    private loadDeviationCauses(): ng.IPromise<any> {
        return this.timeService.getTimeDeviationCausesByEmployeeGroupGrid(this.model.employeeGroupId, true, true).then(x => {
            this.deviationCauses = x;
        });
    }

    private saveShifts = _.debounce((source: string, shifts: ShiftDTO[]) => {
        this.messagingService.publish(Constants.EVENT_SAVE_START, null);

        this.prepareShiftsForSave(shifts);

        this.scheduleService.saveShifts(source, shifts, true, false, false, 0, null).then(result => {
            if (result.success) {
                this.messagingService.publish(Constants.EVENT_ATTESTEMPLOYEE_ROWS_RELOAD, { date: this.model.date, fromModal: this.isModal });
                this.messagingService.publish(Constants.EVENT_SAVE_COMPLETE, null);

                if (this.editShiftModal)
                    this.editShiftModal.close();
            } else {
                if (!result.booleanValue)
                    this.messagingService.publish(Constants.EVENT_SAVE_FAILED, result.errorMessage);
            }
        }, error => {
            if (error.error && error.error === Constants.SERVICE_ERROR_DUPLICATE_CALLS)
                this.coreService.addSysLogMessage("Time.Directives.AttestEmployee.ScheduleDirective.saveShifts", error.message, source + "\n\n" + JSON.stringify(shifts), true);
        });
    }, 200, { leading: true, trailing: false });

    // PUBLIC METHODS (called from ScheduleHandler)

    public get isAutogenTimeblocks(): boolean {
        return this.model.autogenTimeblocks;
    }

    public getShiftById(shiftId: number): ShiftDTO {
        let shift = _.find(this.model.shifts, t => t.timeScheduleTemplateBlockId === shiftId);
        if (!shift)
            shift = _.find(this.model.standbyShifts, t => t.timeScheduleTemplateBlockId === shiftId);
        return shift;
    }

    public getShifts(date: Date): ShiftDTO[] {
        let hasShifts: boolean = this.model.shifts && this.model.shifts.length > 0;
        let hasStandbyShifts: boolean = this.model.standbyShifts && this.model.standbyShifts.length > 0;

        if (!date || (!hasShifts && !hasStandbyShifts))
            return undefined;

        let shifts = [];
        let allShifts = this.model.shifts.concat(this.model.standbyShifts);
        for (let shift of allShifts) {
            shift.setBelongsToBasedOnStartTime(date);
            if (!shift.actualStartTime.isSameMinuteAs(shift.actualStopTime) && shift.actualStartDate.isSameDayAs(date))
                shifts.push(shift);
        }

        return shifts.sort(ShiftDTO.wholeDayStartTimeSort);
    }

    public getScheduleStart(): Date {
        if (!this.model.shifts || !this.model.shifts.length)
            return this.dateFrom;

        var actualShifts = this.getActualShifts();
        return actualShifts.length ? actualShifts[0].actualStartTime : this.dateFrom;
    }

    public getScheduleStop(): Date {
        if (!this.model.shifts || !this.model.shifts.length)
            return this.dateTo;

        var actualShifts = this.getActualShifts();
        return actualShifts.length ? _.last(actualShifts).actualStopTime : this.dateTo;
    }

    public getTimeStampById(timeStampEntryId: number): TimeAttestTimeStampDTO {
        return _.find(this.timeStamps, t => t.timeStampEntryId === timeStampEntryId);
    }

    public getTimeStamps(date: Date): TimeAttestTimeStampDTO[] {
        if (!this.timeStamps || !this.timeStamps.length || !date)
            return undefined;

        var timeStamps: TimeAttestTimeStampDTO[] = [];
        for (let i = 0, j = this.timeStamps.length; i < j; i++) {
            if (this.timeStamps[i].stampIn < date.addDays(1) && this.timeStamps[i].stampOut >= date) {
                timeStamps.push(this.timeStamps[i]);
            }
        }

        return _.sortBy(timeStamps, t => t.stampIn);
    }

    public getProjectTimeBlockById(projectTimeBlockId: number): ProjectTimeBlockDTO {
        return _.find(this.projectTimeBlocks, t => t.projectTimeBlockId === projectTimeBlockId);
    }

    public getProjectTimeBlocks(date: Date): ProjectTimeBlockDTO[] {
        if (!this.projectTimeBlocks || !this.projectTimeBlocks.length || !date)
            return undefined;

        return _.sortBy(this.projectTimeBlocks, t => t.startTime);
    }

    public getTimeBlockByGuidId(guidId: string): AttestEmployeeDayTimeBlockDTO {
        return _.find(this.model.timeBlocks, t => t.guidId === guidId);
    }

    public getTimeBlocks(fromDate: Date, toDate: Date): AttestEmployeeDayTimeBlockDTO[] {
        if (!this.model.timeBlocks || !this.model.timeBlocks.length || !fromDate || !toDate)
            return [];

        var timeBlocks: AttestEmployeeDayTimeBlockDTO[] = [];
        for (let i = 0, j = this.model.timeBlocks.length; i < j; i++) {
            //if (this.model.timeBlocks[i].startTime < toDate && this.model.timeBlocks[i].stopTime >= fromDate)
            timeBlocks.push(this.model.timeBlocks[i]);
        }

        return _.sortBy(timeBlocks, t => t.startTime);
    }

    // EVENTS

    private employeeContentChanged() {
        let dateShifts: ShiftDTO[] = this.getShifts(this.model.date);
        _.forEach(dateShifts, shift => {
            this.setShiftToolTip(shift);
        });

        if (this.renderTimeStamps) {
            this.timeStampIsModified = false;
            this.setTimeBlockToolTips();
            this.recreateTimeStamps();
        } else {
            // Called from reload in transactions directive
            this.renderAll();
            this.reloadingTimeBlocks = false;
        }
        this.resizeContainer();
    }

    private resizeContainer() {
        if (this.isModal)
            return;

        // Trigger event that will resize the grid row and the height of the whole grid.

        // Kinda ugly workaround for slow browsers:
        // First wait 500 ms so the page will be rendered before measuring needed height.
        // Sometimes in slow browsers, the render is not finished within 500 ms,
        // therefor we make a new call after 500 more ms.
        // The grid will not change height again if it was correctly measured the first time.
        if (this.onResizeNeeded) {
            this.$timeout(() => {
                this.onResizeNeeded();
                this.$timeout(() => {
                    this.onResizeNeeded();
                }, 500);
            }, 500);
        }
    }

    private renderScheduleOnResize() {
        if (this.scheduleHandler)
            this.scheduleHandler.renderSchedule(true);
    }

    public shiftSelected() {
    }

    public editShift(shift: ShiftDTO) {
        if (!this.editShiftPermission || this.openingEditShift)
            return;

        this.openingEditShift = true;

        let hasDeviations: boolean = false;
        if (!shift) {
            let shifts = this.getShifts(this.model.date);
            if (shifts && shifts.length > 0)
                shift = shifts[0];
            if (shift)
                hasDeviations = this.model.hasDeviations;
        }

        this.progress.startLoadingProgress([() => this.initOpenEditShiftDialog(shift ? shift.timeScheduleTemplateBlockId : 0, shift ? shift.actualStartDate : this.model.date, hasDeviations)]);
    }

    private initOpenEditShiftDialog(timeScheduleTemplateBlockId: number, date: Date, dayHasDeviations: boolean): ng.IPromise<any> {
        let deferral = this.$q.defer<any>();

        if (!this.editShiftHelper) {
            this.editShiftHelper = new EditShiftHelper(this.$uibModal, this.$q, this.coreService, this.sharedScheduleService, this.urlHelperService, this.translationService, this.editShiftPermission, date, date, this.model.employeeId, !this.editShiftPermission, () => {
                this.openEditShiftDialog(timeScheduleTemplateBlockId, date, dayHasDeviations).then(() => {
                    deferral.resolve();
                });
            });
        } else {
            this.openEditShiftDialog(timeScheduleTemplateBlockId, date, dayHasDeviations).then(() => {
                deferral.resolve();
            });
        }

        return deferral.promise;
    }

    private openEditShiftDialog(timeScheduleTemplateBlockId: number, date: Date, dayHasDeviations: boolean): ng.IPromise<any> {
        return this.editShiftHelper.loadShift(timeScheduleTemplateBlockId).then(shift => {
            return this.editShiftHelper.openEditShiftDialog(shift, date, shift ? shift.employeeId : this.model.employeeId, true, dayHasDeviations).then(modal => {
                this.editShiftModal = modal;
                this.openingEditShift = false;
            })
        });
    }

    public editTimeBlock(timeBlock: AttestEmployeeDayTimeBlockDTO, row, changingStartTime: boolean = false, changingStopTime: boolean = false, trans: PayrollImportEmployeeTransactionDTO = null) {
        if (this.deviationCauses.length === 0) {
            this.loadDeviationCauses().then(() => {
                this.editTimeBlock(timeBlock, row, changingStartTime, changingStopTime, trans);
            });
            return;
        }
        const accountSetting = null;
        let options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Directives/AttestEmployee/Dialogs/TimeBlockDialog/TimeBlockDialog.html"),
            controller: TimeBlockDialogController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
                deviationCauses: () => { return this.deviationCauses },
                timeBlock: () => { return timeBlock },
                accountSetting: () => { return accountSetting },
                date: () => { return this.model.date },
                changingStartTime: () => { return changingStartTime },
                changingStopTime: () => { return changingStopTime },
                trans: () => { return trans },

            }
        }     
        this.$uibModal.open(options).result.then(result => {
            if (result && result.reload) {
                this.messagingService.publish(Constants.EVENT_ATTESTEMPLOYEE_ROWSANDWARNINGS_RELOAD, { date: this.model.date, fromModal: this.isModal });
            } else if (result && result.timeBlock) {
                // Add new
                if (!timeBlock)
                    timeBlock = new AttestEmployeeDayTimeBlockDTO();

                // Update fields                
                timeBlock.comment = result.timeBlock.comment;
                if (!timeBlock.timeBlockId) {
                    timeBlock.startTime = result.timeBlock.startTime;
                    timeBlock.stopTime = result.timeBlock.stopTime;
                    timeBlock.timeDeviationCauseStartId = result.timeBlock.timeDeviationCauseStartId;
                    this.timeBlockAdded(timeBlock, timeBlock.timeDeviationCauseStartId, timeBlock.employeeChildId, timeBlock.comment, result.trans, result.accountSetting);
                } else {
                    var clientChange: SoeTimeBlockClientChange = SoeTimeBlockClientChange.None;
                    if (timeBlock.startTimeDuringMove) {
                        timeBlock.startTimeDuringMove = result.timeBlock.startTime;
                        clientChange = SoeTimeBlockClientChange.Left;
                    }
                    if (timeBlock.stopTimeDuringMove) {
                        timeBlock.stopTimeDuringMove = result.timeBlock.stopTime;
                        clientChange = SoeTimeBlockClientChange.Right;
                    }
                    this.timeBlockResized(timeBlock, clientChange, result.timeBlock.timeDeviationCauseStartId, timeBlock.employeeChildId, timeBlock.comment, null, result.accountSetting)
                }
            } else if (changingStartTime || changingStopTime) {
                this.restoreCancelledTimeBlockEdit(timeBlock, row, changingStartTime, changingStopTime);
            }
        }).catch(() => {
            // User pressed escape in dialog
            this.restoreCancelledTimeBlockEdit(timeBlock, row, changingStartTime, changingStopTime);
        });
    }

    private restoreCancelledTimeBlockEdit(timeBlock: AttestEmployeeDayTimeBlockDTO, row, changingStartTime: boolean, changingStopTime: boolean) {
        // Restore saved data
        if (changingStartTime) {
            timeBlock.startTimeDuringMove = timeBlock.startTime;
        } else if (changingStopTime) {
            timeBlock.stopTimeDuringMove = timeBlock.stopTime;
        }
        this.scheduleHandler.updateTimeBlockRow(row);
    }

    public removeTimeBlock(timeBlock: AttestEmployeeDayTimeBlockDTO, row) {
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Directives/AttestEmployee/Dialogs/DeleteTimeBlockDialog/DeleteTimeBlockDialog.html"),
            controller: DeleteTimeBlockDialogController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
                timeBlock: () => { return timeBlock }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.timeBlock) {
                var clientChange: SoeTimeBlockClientChange = SoeTimeBlockClientChange.None;
                if (timeBlock.startTimeDuringMove) {
                    clientChange = SoeTimeBlockClientChange.Left
                    timeBlock.startTimeDuringMove = result.timeBlock.startTime;
                } else if (timeBlock.stopTimeDuringMove) {
                    clientChange = SoeTimeBlockClientChange.Right
                    timeBlock.stopTimeDuringMove = result.timeBlock.stopTime;
                }
                this.timeBlockResized(timeBlock, clientChange);
            } else {
                // Restore saved data
                timeBlock.startTimeDuringMove = null;
                timeBlock.stopTimeDuringMove = null;
                this.$timeout(() => {
                    this.scheduleHandler.updateTimeBlockRow(row);
                });
            }
        });
    }

    private toggleTimeStamp(resize: boolean = true) {
        this.showTimeStamp = !this.showTimeStamp;
        if (resize)
            this.resizeContainer();
    }

    public timeStampSelected(timeStamp: TimeAttestTimeStampDTO) {
        if (this.onTimeStampSelected) {
            // Get time stamp entry
            var entry = _.find(this.model.timeStampEntrys, t => t.identifier === timeStamp.timeStampEntryId);
            if (entry)
                this.onTimeStampSelected({ id: entry.identifier });
        }
    }

    private toggleProjectTimeBlock(resize: boolean = true) {
        this.showProjectTimeBlock = !this.showProjectTimeBlock;
        if (resize)
            this.resizeContainer();
    }

    private toggleTimeBlock(resize: boolean = true) {
        this.showTransaction = !this.showTransaction;
        if (resize)
            this.resizeContainer();
    }

    private timeBlockAdded(timeBlock: AttestEmployeeDayTimeBlockDTO, timeDeviationCauseId?: number, employeeChildId?: number, comment?: string, trans?: PayrollImportEmployeeTransactionDTO, accountSetting?: AccountingSettingsRowDTO) {
        this.timeBlockResized(timeBlock, SoeTimeBlockClientChange.None, timeDeviationCauseId, employeeChildId, comment, trans, accountSetting);
    }

    public timeBlockResized(timeBlock: AttestEmployeeDayTimeBlockDTO, clientChange: SoeTimeBlockClientChange, timeDeviationCauseId?: number, employeeChildId?: number, comment?: string, trans?: PayrollImportEmployeeTransactionDTO, accountSetting?: AccountingSettingsRowDTO) {
        // Keep old data
        this.model['savedTimeBlocks'] = this.model.timeBlocks;
        this.model['savedAttestPayrollTransactions'] = this.model.attestPayrollTransactions;
        this.model['savedTimeCodeTransactions'] = this.model.timeCodeTransactions;

        // Clear data
        this.model.timeBlocks = [];
        this.model.attestPayrollTransactions = [];
        this.model.timeCodeTransactions = [];

        this.validateDeviationChange(timeBlock, clientChange, timeDeviationCauseId, employeeChildId, comment, trans, accountSetting).then(passed => {
            if (passed) {
                // Set transactions as modified
                this.timeBlocksIsModified = true;
            } else {
                // Restore saved data
                this.model.timeBlocks = this.model['savedTimeBlocks'];
                _.forEach(this.model.timeBlocks, b => {
                    b.startTimeDuringMove = undefined;
                    b.stopTimeDuringMove = undefined;
                });
                this.model.attestPayrollTransactions = this.model['savedAttestPayrollTransactions'];
                this.model.timeCodeTransactions = this.model['savedTimeCodeTransactions'];
            }

            this.getTimes();
            this.setDateRange();
            this.scheduleHandler.renderSchedule(false);
            this.resizeContainer();
        });
    }

    private validateDeviationChange(timeBlock: AttestEmployeeDayTimeBlockDTO, clientChange: SoeTimeBlockClientChange, timeDeviationCauseId?: number, employeeChildId?: number, comment?: string, trans?: PayrollImportEmployeeTransactionDTO, accountSetting?: AccountingSettingsRowDTO): ng.IPromise<boolean> {
        var deferral = this.$q.defer<boolean>();

        this.validatingTimeBlocks = true;

        // Expand transactions
        if (!this.showTransaction)
            this.toggleTimeBlock();

        this.timeService.validateDeviationChange(this.model.employeeId, timeBlock.timeBlockId, timeBlock.guidId, this.model['savedTimeBlocks'], this.model.timeScheduleTemplatePeriodId, this.model.date, timeBlock.getStartTimeIfMoved(), timeBlock.getStopTimeIfMoved(), clientChange, this.isEmployeeCurrentUser, timeDeviationCauseId, employeeChildId, comment, accountSetting).then(result => {
            this.validatingTimeBlocks = false;
            if (result.success) {
                if (result.resultCode === SoeValidateDeviationChangeResultCode.ChooseDeviationCause) {
                    this.openSelectTimeDeviationCause(timeBlock, clientChange, result.timeDeviationCauses).then(res => {
                        deferral.resolve(res);
                    });
                } else {
                    this.model.timeBlocks = result.generatedTimeBlocks;

                    this.model.attestPayrollTransactions = result.generatedTimePayrollTransactions;
                    this.model.timeCodeTransactions = result.generatedTimeCodeTransactions;
                    this.model['applyAbsenceItems'] = result.applyAbsenceItems;

                    if (!this.model['payrollImportEmployeeTransactionIds'])
                        this.model['payrollImportEmployeeTransactionIds'] = [];
                    if (trans)
                        this.model['payrollImportEmployeeTransactionIds'].push(trans.payrollImportEmployeeTransactionId);

                    this.setTimeBlockToolTips();

                    deferral.resolve(true);
                }
            } else {
                this.notificationService.showDialogEx(this.terms["error.default_error"], result.message, SOEMessageBoxImage.Error);
                deferral.resolve(false);
            }
        }).catch(reason => {
            this.notificationService.showServiceError(reason);
            deferral.resolve(false);
        });

        return deferral.promise;
    }

    // HELP-METHODS

    private getTimes() {
        // Get first start time and last end time overall for Shifts, TimeStamps and TimeBlocks
        this.dayViewStartTime = 0;
        this.dayViewEndTime = 24 * 60;
        let startTime: Date;
        let endTime: Date;

        // Shifts
        let actualShifts = this.getActualShifts();
        let standbyShifts = this.getStandbyShifts();
        if (actualShifts.length > 0 || standbyShifts.length > 0) {
            startTime = _.orderBy(actualShifts.concat(standbyShifts), s => s.actualStartTime)[0].actualStartTime;
            endTime = _.orderBy(actualShifts.concat(standbyShifts), s => s.actualStopTime, 'desc')[0].actualStopTime;
        }

        // TimeStamps
        if (this.renderTimeStamps && this.model.timeStampEntrys.length > 0) {
            if (!startTime || startTime.isAfterOnMinute(this.model.timeStampEntrys[0].time))
                startTime = this.model.timeStampEntrys[0].time;
            if (!endTime || endTime.isBeforeOnMinute(_.last(this.model.timeStampEntrys).time))
                endTime = _.last(this.model.timeStampEntrys).time;
        }

        // ProjectTimeBlocks
        if (this.renderProjectTimeblocks && this.model.projectTimeBlocks.length > 0) {

            if (!startTime || startTime.isAfterOnMinute(this.model.projectTimeBlocks[0].actualStartTime))
                startTime = this.model.projectTimeBlocks[0].actualStartTime;
            if (!endTime || endTime.isBeforeOnMinute(_.last(this.model.projectTimeBlocks).actualStopTime))
                endTime = _.last(this.model.projectTimeBlocks).actualStopTime;
        }

        // TimeBlocks
        if (this.model.timeBlocks.length > 0) {
            if (!startTime || startTime.isAfterOnMinute(this.model.timeBlocks[0].startTime))
                startTime = this.model.timeBlocks[0].startTime;
            if (!endTime || endTime.isBeforeOnMinute(_.last(this.model.timeBlocks).stopTime))
                endTime = _.last(this.model.timeBlocks).stopTime;
        }

        if (startTime)
            this.dayViewStartTime = (startTime.getHours() > 0 ? startTime.addMinutes(-30).getHours() : startTime.getHours()) * 60;
        if (endTime)
            this.dayViewEndTime = (endTime.getHours() < 23 ? (endTime.addMinutes(30).getHours() + 1) : endTime.getHours() + 1) * 60;

        // Handle start before midnight
        if (startTime && startTime.isBeforeOnDay(this.model.date))
            this.dayViewStartTime -= (60 * 24);

        // Handle end after midnight
        if (endTime && endTime.isAfterOnDay(this.model.date))
            this.dayViewEndTime += (60 * 24);
    }

    private getActualShifts(): ShiftDTO[] {
        return this.model.shifts ? _.filter(this.model.shifts, s => !s.actualStartTime.isSameMinuteAs(s.actualStopTime)) : [];
    }

    private getStandbyShifts(): ShiftDTO[] {
        return this.model.standbyShifts ? _.filter(this.model.standbyShifts, s => !s.actualStartTime.isSameMinuteAs(s.actualStopTime)) : [];
    }

    private setDateRange() {
        if (this.nbrOfVisibleHours === 0)
            return;

        this.dateFrom = this.model.date.beginningOfDay().addMinutes(this.dayViewStartTime);
        this.dateTo = (this.dateFrom.addHours(this.nbrOfVisibleHours - 1)).endOfHour();

        var tmpDates: DateDay[] = [];
        var cols = this.nbrOfVisibleHours;

        for (var i: number = 0; i < cols; i++) {
            tmpDates.push(new DateDay(this.dateFrom.addMinutes(i * 60)));
        }

        this.dates = tmpDates;
    }

    // Shifts

    private setShiftToolTips() {
        _.forEach(this.model.shifts, shift => {
            this.setShiftToolTip(shift);
        });
        _.forEach(this.model.standbyShifts, shift => {
            this.setShiftToolTip(shift);
        });
    }

    private setShiftToolTip(shift: ShiftDTO) {
        var toolTip: string = '';
        var wholeDayToolTip: string = '';

        // Current shift

        // Time
        if (!shift.isAbsenceRequest) {
            if (shift.isWholeDay)
                toolTip += "{0}  ".format(this.terms["time.schedule.planning.wholedaylabel"]);
            else
                toolTip += "{0}-{1}  ".format(shift.actualStartTime.toFormattedTime(), shift.actualStopTime.toFormattedTime());
        }

        if (shift.timeDeviationCauseId && shift.timeDeviationCauseId !== 0) {
            // Absence
            toolTip += shift.timeDeviationCauseName;
        } else {
            // Shift type
            if (shift.shiftTypeName)
                toolTip += shift.shiftTypeName;
        }

        // Schedule type
        let scheduleTypeNames = shift.getTimeScheduleTypeNames(this.useMultipleScheduleTypes);
        if (scheduleTypeNames)
            toolTip += " - {0}".format(scheduleTypeNames);

        // Week number/Number of weeks
        if (shift.nbrOfWeeks > 0) {
            if (toolTip && toolTip.length > 0)
                toolTip += ", ";
            toolTip += "{0}/{1}{2}".format(CalendarUtility.getWeekNr(shift.dayNumber).toString(), shift.nbrOfWeeks.toString(), this.terms["common.weekshort"]);
        }

        // Description
        if (shift.description) {
            if (toolTip && toolTip.length > 0)
                toolTip += "\n";
            toolTip += shift.description;
        }

        // Whole day

        var dayShifts: ShiftDTO[] = [];

        // If whole day absence, skip this part
        dayShifts = _.filter(this.model.shifts, s => s.employeeId === shift.employeeId &&
            ((s.actualStartTime.isSameDayAs(shift.actualStartTime) && !s.belongsToPreviousDay && !s.belongsToNextDay) || (s.actualStartTime.isSameDayAs(shift.actualStartTime.addDays(1)) && s.belongsToPreviousDay) || (s.actualStartTime.isSameDayAs(shift.actualStartTime.addDays(-1)) && s.belongsToNextDay)) &&
            !((s.isAbsence || s.isAbsenceRequest) && CalendarUtility.toFormattedTime(s.actualStartTime, true) === '00:00:00' && CalendarUtility.toFormattedTime(s.actualStopTime, true) === '23:59:59'));

        dayShifts.push(..._.filter(this.model.standbyShifts, s => s.employeeId === shift.employeeId &&
            ((s.actualStartTime.isSameDayAs(shift.actualStartTime) && !s.belongsToPreviousDay && !s.belongsToNextDay) || (s.actualStartTime.isSameDayAs(shift.actualStartTime.addDays(1)) && s.belongsToPreviousDay) || (s.actualStartTime.isSameDayAs(shift.actualStartTime.addDays(-1)) && s.belongsToNextDay)) &&
            !((s.isAbsence || s.isAbsenceRequest) && CalendarUtility.toFormattedTime(s.actualStartTime, true) === '00:00:00' && CalendarUtility.toFormattedTime(s.actualStopTime, true) === '23:59:59')));

        var minutes: number = _.sumBy(dayShifts, s => s.getShiftLength());
        var factorMinutes: number = 0;

        if (dayShifts.length > 0) {
            // Get all breaks
            var break1: string = shift.break1TimeCodeId !== 0 ? "\n{0}-{1}  {2}".format(shift.break1StartTime.toFormattedTime(), shift.break1StartTime.addMinutes(shift.break1Minutes).toFormattedTime(), shift['break1TimeCode']) : '';
            var break2: string = shift.break2TimeCodeId !== 0 ? "\n{0}-{1}  {2}".format(shift.break2StartTime.toFormattedTime(), shift.break2StartTime.addMinutes(shift.break2Minutes).toFormattedTime(), shift['break2TimeCode']) : '';
            var break3: string = shift.break3TimeCodeId !== 0 ? "\n{0}-{1}  {2}".format(shift.break3StartTime.toFormattedTime(), shift.break3StartTime.addMinutes(shift.break3Minutes).toFormattedTime(), shift['break3TimeCode']) : '';
            var break4: string = shift.break4TimeCodeId !== 0 ? "\n{0}-{1}  {2}".format(shift.break4StartTime.toFormattedTime(), shift.break4StartTime.addMinutes(shift.break4Minutes).toFormattedTime(), shift['break4TimeCode']) : '';

            wholeDayToolTip += "{0}:".format(this.terms["time.schedule.planning.todaysschedule"]);

            _.forEach(_.orderBy(dayShifts, 'actualStartTime'), dayShift => {
                // Breaks within day

                minutes -= dayShift.getBreakTimeWithinShift();

                if (shift.isSchedule) {
                    var breakEndTime: Date;
                    if (break1) {
                        breakEndTime = shift.break1StartTime.addMinutes(shift.break1Minutes);
                        if (breakEndTime.isSameOrBeforeOnMinute(dayShift.actualStartTime)) {
                            wholeDayToolTip += break1;
                            break1 = '';
                        }
                    }
                    if (break2) {
                        breakEndTime = shift.break2StartTime.addMinutes(shift.break2Minutes);
                        if (breakEndTime.isSameOrBeforeOnMinute(dayShift.actualStartTime)) {
                            wholeDayToolTip += break2;
                            break2 = '';
                        }
                    }
                    if (break3) {
                        breakEndTime = shift.break3StartTime.addMinutes(shift.break3Minutes);
                        if (breakEndTime.isSameOrBeforeOnMinute(dayShift.actualStartTime)) {
                            wholeDayToolTip += break3;
                            break3 = '';
                        }
                    }
                    if (break4) {
                        breakEndTime = shift.break4StartTime.addMinutes(shift.break4Minutes);
                        if (breakEndTime.isSameOrBeforeOnMinute(dayShift.actualStartTime)) {
                            wholeDayToolTip += break4;
                            break4 = '';
                        }
                    }
                }

                // Time
                wholeDayToolTip += "\n{0}-{1}  ".format(dayShift.actualStartTime.toFormattedTime(), dayShift.actualStopTime.toFormattedTime());

                // Shift type
                if (dayShift.shiftTypeName)
                    wholeDayToolTip += dayShift.shiftTypeName;

                // TimeScheduleType factor multiplyer
                factorMinutes += dayShift.getTimeScheduleTypeFactorsWithinShift();
            });

            if (shift.isSchedule) {
                // The rest of the breaks
                if (break1)
                    wholeDayToolTip += break1;
                if (break2)
                    wholeDayToolTip += break2;
                if (break3)
                    wholeDayToolTip += break3;
                if (break4)
                    wholeDayToolTip += break4;

                // Summary

                var breakMinutes: number = 0;
                if (shift.break1TimeCodeId !== 0)
                    breakMinutes += shift.break1Minutes;
                if (shift.break2TimeCodeId !== 0)
                    breakMinutes += shift.break2Minutes;
                if (shift.break3TimeCodeId !== 0)
                    breakMinutes += shift.break3Minutes;
                if (shift.break4TimeCodeId !== 0)
                    breakMinutes += shift.break4Minutes;

                wholeDayToolTip += "\n\n{0}: {1}".format(this.terms["time.schedule.planning.scheduletime"], CalendarUtility.minutesToTimeSpan(minutes));
                if (breakMinutes > 0)
                    wholeDayToolTip += " ({0})".format(breakMinutes.toString());
            }

            if (factorMinutes !== 0)
                wholeDayToolTip += "\n{0}: {1}".format(this.terms["time.schedule.planning.scheduletypefactortime"], CalendarUtility.minutesToTimeSpan(factorMinutes));
        }

        if (wholeDayToolTip.length === 0)
            shift.toolTip = toolTip;
        else
            shift.toolTip = (toolTip.length > 0 ? "{0}:\n{1}\n\n".format(this.terms["time.schedule.planning.thisshift"], toolTip) : '') + wholeDayToolTip;

        if (shift.availabilityToolTip)
            shift.toolTip += "\n\n{0}".format(shift.availabilityToolTip);
    }

    private prepareShiftsForSave(shifts: ShiftDTO[]) {
        _.forEach(shifts, shift => {
            shift.prepareShiftsForSave(this.defaultTimeCodeId);
        });

        // If all shifts are marked as deleted, unmark one of the them,
        // otherwise the whole day will be deleted and not visible in the attest view
        if (shifts.length > 0 && shifts.length === _.filter(shifts, s => s.isDeleted).length)
            _.last(shifts).isDeleted = false;
    }

    // Time stamps

    private createTimeStamps() {
        this.timeStamps = [];
        this.hasTimeStampWarnings = (this.model.timeStampEntrys.length % 2 > 0);
        this.hasTimeStampErrors = false;

        var prevEntry: AttestEmployeeDayTimeStampDTO;
        _.forEach(_.orderBy(this.model.timeStampEntrys, ['time', 'timeStampEntryId', 'identifier']), entry => {
            if (prevEntry && prevEntry.type === TimeStampEntryType.In && entry.type === TimeStampEntryType.Out) {
                // Two complete entries (in + out) create a visual block
                let timeStamp = new TimeAttestTimeStampDTO();
                timeStamp.timeStampEntryId = prevEntry.identifier;
                timeStamp.stampOutId = entry.identifier;
                timeStamp.stampIn = prevEntry.time;
                timeStamp.stampOut = entry.time;
                if (timeStamp.stampIn < timeStamp.stampOut) {
                    this.timeStamps.push(timeStamp);

                    if (this.useAccountHierarchy) {
                        let shiftAccountIds = _.map(_.filter(this.getIntersectingShifts(timeStamp.stampIn, timeStamp.stampOut), s => s.accountId), s => s.accountId);
                        if (shiftAccountIds.length > 0) {
                            // Check if time stamp is within a schedule that current user is not allowed to edit
                            if (NumberUtility.intersect(this.validAccountIds, shiftAccountIds).length === 0) {
                                if (prevEntry.timeStampEntryId)
                                    prevEntry.isReadonly = true;
                                if (entry.timeStampEntryId)
                                    entry.isReadonly = true;
                            }
                        } else if(!this.showAccountDim2) {
                            // No shifts, check accounting on terminal
                            if (prevEntry.timeTerminalAccountId && !_.includes(this.validAccountIds, prevEntry.timeTerminalAccountId)) {
                                if (prevEntry.timeStampEntryId)
                                    prevEntry.isReadonly = true;
                                if (entry.timeStampEntryId)
                                    entry.isReadonly = true;
                            }
                        }
                    }
                }
            } else if (prevEntry && prevEntry.type === entry.type) {
                // Two entries with the same type after each other
                // Skip and mark as error
                this.hasTimeStampErrors = true;
            }

            prevEntry = entry;
        });

        this.setTimeStampToolTips();
    }

    private getIntersectingShifts(timeFrom: Date, timeTo: Date): ShiftDTO[] {
        let shifts: ShiftDTO[] = [];

        let dayShifts = this.getActualShifts();
        _.forEach(dayShifts, shift => {
            if (CalendarUtility.isRangesOverlapping(shift.actualStartTime, shift.actualStopTime, timeFrom, timeTo)) {
                shifts.push(shift);
            }
        });

        return shifts;
    }

    private recreateTimeStamps() {
        if (this.renderTimeStamps) {
            this.getTimes();
            this.setDateRange();
            this.createTimeStamps();
            this.scheduleHandler.renderSchedule(false);
        }
    }

    private setTimeStampToolTips() {
        _.forEach(this.timeStamps, timeStamp => {
            this.setTimeStampToolTip(timeStamp);
        });
    }

    private setTimeStampToolTip(timeStamp: TimeAttestTimeStampDTO) {
        var toolTip: string = '';

        // Time
        toolTip += "{0}-{1}".format(timeStamp.stampIn.toFormattedTime(), timeStamp.stampOut.toFormattedTime());

        timeStamp.toolTip = toolTip;
    }

    // Time blocks

    private setTimeBlockToolTips() {
        _.forEach(this.model.timeBlocks, timeBlock => {
            this.setTimeBlockToolTip(timeBlock);
        });
    }

    private setTimeBlockToolTip(timeBlock: AttestEmployeeDayTimeBlockDTO) {
        var toolTip: string = '';

        // Time
        toolTip += "{0}-{1}".format(timeBlock.startTime.toFormattedTime(), timeBlock.stopTime.toFormattedTime());

        // Deviation cause
        if (timeBlock.timeDeviationCauseName)
            toolTip += " {0}".format(timeBlock.timeDeviationCauseName);

        // Break time codes
        if (timeBlock.isBreak && timeBlock.timeCodes && timeBlock.timeCodes.length > 0) {
            toolTip += "\n" + _.map(timeBlock.timeCodes, c => c.name).join(', ');
        }

        // Transactions
        var transactions = _.filter(this.model.attestPayrollTransactions, t => t.guidIdTimeBlock === timeBlock.guidId);
        _.forEach(transactions, trans => {
            let transText: string = "{0} {1}, {2}, {3}-{4} = {5}, {6}".format(
                trans.payrollProductNumber,
                trans.payrollProductName,
                trans.attestStateName,
                trans.startTimeString,
                trans.stopTimeString,
                trans.quantityString,
                trans.accountingShortString);

            toolTip += "\n" + transText;
        });

        timeBlock.toolTip = toolTip;
    }

    private openSelectTimeDeviationCause(timeBlock: AttestEmployeeDayTimeBlockDTO, clientChange: SoeTimeBlockClientChange, timeDeviationCauses: ITimeDeviationCauseGridDTO[]): ng.IPromise<boolean> {
        var deferral = this.$q.defer<boolean>();
        const accountSetting = null;
        // Show select time deviation casuse dialog
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Directives/AttestEmployee/Dialogs/SelectTimeDeviationCause/SelectTimeDeviationCause.html"),
            controller: SelectTimeDeviationCauseController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'lg',
            resolve: {
                employeeId: () => { return this.model.employeeId; },
                timeDeviationCauses: () => { return timeDeviationCauses; },
                accountSetting: () => { return accountSetting },
                deviationAccounts: () => { return timeBlock.deviationAccounts; },
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result.success) {
                if (result.timeDeviationCauseId) {
                    // Validate again
                    this.validateDeviationChange(timeBlock, clientChange, result.timeDeviationCauseId, result.employeeChildId, result.comment, null, result.accountSetting).then(passed => {
                        deferral.resolve(passed);
                    });
                }
            } else {
                deferral.resolve(false);
            }
        }, (reason) => {
            // Cancelled
            deferral.resolve(false);
        });

        return deferral.promise;
    }
}

class DateDay {
    constructor(date: Date) {
        this.date = date;
    }

    public date: Date;
}

