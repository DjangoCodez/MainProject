import { IContextMenuHandler } from "../../../../Core/Handlers/ContextMenuHandler";
import { IContextMenuHandlerFactory } from "../../../../Core/Handlers/ContextMenuHandlerFactory";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { ITimeCodeBreakSmallDTO } from "../../../../Scripts/TypeLite.Net4";
import { ScheduleService } from "../../../../Shared/Time/Schedule/ScheduleService";
import { CalendarUtility } from "../../../../Util/CalendarUtility";
import { Feature, TermGroup_OrderPlanningShiftInfo } from "../../../../Util/CommonEnumerations";
import { EditEmployeeAvailabilityDialogController } from "../../../Dialogs/EditEmployeeAvailability/EditEmployeeAvailabilityDialogController";
import { DateRangeDTO } from "../../../Models/DateRangeDTO";
import { EmployeeListDTO } from "../../../Models/EmployeeListDTO";
import { ShiftDTO } from "../../../Models/TimeSchedulePlanningDTOs";
import { ScheduleHandler } from "./ScheduleHandler";

export class ScheduleDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Common/Dashboard/Widgets/MySchedule/Schedule.html'),
            scope: {
                isSchedulePlanningMode: '=',
                isOrderPlanningMode: '=',
                employee: '=',
                dateFrom: '=',
                dateTo: '=',
                showOpenShifts: '=',
                showColleaguesShifts: '=',
                useMultipleScheduleTypes: '=',
                myShifts: '=',
                openShifts: '=',
                colleaguesShifts: '=',
                onEditShift: '&',
                onDateExpanded: '&',
                onReloadEmployee: '&',
                onReloadShifts: '&'
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
    public isSchedulePlanningMode: boolean;
    public isOrderPlanningMode: boolean;
    public employee: EmployeeListDTO;
    public dateFrom: Date;
    public dateTo: Date;
    public showOpenShifts: boolean;
    public showColleaguesShifts: boolean;
    public useMultipleScheduleTypes: boolean;
    public myShifts: ShiftDTO[];
    public openShifts: ShiftDTO[];
    public colleaguesShifts: ShiftDTO[];
    private onEditShift: Function;
    private onDateExpanded: Function;
    private onReloadEmployee: Function;
    private onReloadShifts: Function;

    // Handlers
    private scheduleHandler: ScheduleHandler;

    // Terms
    public terms: { [index: string]: string; };
    private shiftDefined: string;
    private shiftUndefined: string;
    private shiftsDefined: string;
    private shiftsUndefined: string;

    // Data
    public dates: MyScheduleDate[] = [];
    public timeSlots: number[] = [];
    private breakTimeCodes: ITimeCodeBreakSmallDTO[];

    // Settings
    public dayViewStartTime: number = 0;   // Minutes from midnight
    public dayViewEndTime: number = 0;     // Minutes from midnight

    // Permissions
    public editShiftPermission: boolean = false;
    public showQueuePermission: boolean = false;
    private availablePermission: boolean = false;
    private notAvailablePermission: boolean = false;
    public enableAvailibility: boolean = false;

    // Properties
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

    private hasMultipleEmployeeAccounts: boolean = false;

    // Context menus
    private shiftContextMenuHandler: IContextMenuHandler;
    private dateContextMenuHandler: IContextMenuHandler;

    //@ngInject
    constructor(
        private $uibModal,
        private urlHelperService: IUrlHelperService,
        private coreService: ICoreService,
        private sharedScheduleService: ScheduleService,
        private translationService: ITranslationService,
        private contextMenuHandlerFactory: IContextMenuHandlerFactory,
        private $filter: ng.IFilterService,
        private $timeout: ng.ITimeoutService,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $compile: ng.ICompileService
    ) { }

    public $onInit() {
        this.$q.all([
            this.loadTerms(),
            this.loadReadPermissions(),
            this.loadModifyPermissions(),
            this.loadBreakTimeCodes()
        ]).then(() => {
            this.scheduleHandler = new ScheduleHandler(this, this.$filter, this.$timeout, this.$q, this.$scope, this.$compile);
            this.setupWatchers();
            this.setupContextMenus();
        });
    }

    private setupWatchers() {
        this.$scope.$watchGroup([() => this.employee, () => this.dateFrom, () => this.dateTo], (newValue, oldValue, scope) => {
            this.setEnableAvailibility();
            this.getHasMultipleEmployeeAccounts();
        });

        this.$scope.$on("renderSchedule", (event, params) => {
            let keepDatesExpanded: boolean = !!params.keepDatesExpanded;
            this.renderAll(keepDatesExpanded);
        });
    }

    private renderAll(keepDatesExpanded: boolean) {
        if (!this.myShifts)
            return;

        this.setTimes();
        this.setDateRange(keepDatesExpanded);

        // Schedule
        this.setShiftToolTips();

        // Render GUI
        this.scheduleHandler.renderSchedule();
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "common.absence",
            "common.weekshort",
            "common.dashboard.myschedule.editmyshift",
            "common.dashboard.myschedule.editopenshift",
            "common.dashboard.myschedule.availability.edit",
            "time.schedule.planning.wholedaylabel",
            "time.schedule.planning.todaysschedule",
            "time.schedule.planning.todaysorders",
            "time.schedule.planning.todaysbookings",
            "time.schedule.planning.scheduletime",
            "time.schedule.planning.scheduletypefactortime",
            "time.schedule.planning.thisshift",
            "time.schedule.planning.thisorder",
            "time.schedule.planning.thisbooking",
            "time.schedule.planning.orderstatus",
            "time.schedule.planning.breakprefix",
            "time.schedule.planning.breaklabel",
            "time.schedule.planning.availability",
            "time.schedule.planning.available",
            "time.schedule.planning.unavailable",
            "time.schedule.planning.handleshift.title",
            "time.schedule.planning.handleshift.action.wanted"
        ];

        if (this.isSchedulePlanningMode) {
            keys.push("time.schedule.planning.shiftdefined");
            keys.push("time.schedule.planning.shiftundefined");
            keys.push("time.schedule.planning.shiftsdefined");
            keys.push("time.schedule.planning.shiftsundefined");
        } else if (this.isOrderPlanningMode) {
            keys.push("time.schedule.planning.assignmentdefined");
            keys.push("time.schedule.planning.assignmentundefined");
            keys.push("time.schedule.planning.assignmentsdefined");
            keys.push("time.schedule.planning.assignmentsundefined");
        }

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;

            if (this.isSchedulePlanningMode) {
                this.shiftDefined = this.terms["time.schedule.planning.shiftdefined"];
                this.shiftUndefined = this.terms["time.schedule.planning.shiftundefined"];
                this.shiftsDefined = this.terms["time.schedule.planning.shiftsdefined"];
                this.shiftsUndefined = this.terms["time.schedule.planning.shiftsundefined"];
            } else if (this.isOrderPlanningMode) {
                this.shiftDefined = this.terms["time.schedule.planning.assignmentdefined"];
                this.shiftUndefined = this.terms["time.schedule.planning.assignmentundefined"];
                this.shiftsDefined = this.terms["time.schedule.planning.assignmentsdefined"];
                this.shiftsUndefined = this.terms["time.schedule.planning.assignmentsundefined"];
            }
        });
    }

    private loadReadPermissions(): ng.IPromise<any> {
        var featureIds: number[] = [];
        featureIds.push(Feature.Time_Schedule_SchedulePlanningUser_HandleShiftShowQueue);
        featureIds.push(Feature.Time_Schedule_AvailabilityUser_Available);
        featureIds.push(Feature.Time_Schedule_AvailabilityUser_NotAvailable);

        return this.coreService.hasReadOnlyPermissions(featureIds).then((x) => {
            this.showQueuePermission = x[Feature.Time_Schedule_SchedulePlanningUser_HandleShiftShowQueue];
            this.availablePermission = x[Feature.Time_Schedule_AvailabilityUser_Available];
            this.notAvailablePermission = x[Feature.Time_Schedule_AvailabilityUser_NotAvailable];
            this.setEnableAvailibility();
        });
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        var featureIds: number[] = [];
        featureIds.push(Feature.Time_Time_Attest_EditSchedule);

        return this.coreService.hasModifyPermissions(featureIds).then((x) => {
            this.editShiftPermission = x[Feature.Time_Time_Attest_EditSchedule];
        });
    }

    private getHasMultipleEmployeeAccounts() {
        if (!this.employee) {
            this.hasMultipleEmployeeAccounts = false;
            return;
        }

        this.sharedScheduleService.hasMultipleEmployeeAccounts(this.employee.employeeId, this.dateFrom, this.dateTo).then(x => {
            this.hasMultipleEmployeeAccounts = x;
        });
    }

    private loadBreakTimeCodes(): ng.IPromise<any> {
        return this.sharedScheduleService.getTimeCodeBreaks(false).then((x) => {
            this.breakTimeCodes = x;
        });
    }

    // CONTEXT MENUS

    private setupContextMenus() {
        this.dateContextMenuHandler = this.contextMenuHandlerFactory.create();
        this.shiftContextMenuHandler = this.contextMenuHandlerFactory.create();
    }

    private getDateContextMenuOptions(formattedDate: string): any[] {
        // Context menu for date

        // Get clicked date
        var date = formattedDate.parsePipedDate();

        return this.createDateContextMenuOptions(date);
    }

    private createDateContextMenuOptions(date: Date): any[] {
        this.dateContextMenuHandler.clearContextMenuItems();

        // Availability
        if (this.enableAvailibility)
            this.dateContextMenuHandler.addContextMenuItem(this.terms["common.dashboard.myschedule.availability.edit"], 'fa-calendar-check textColor', ($itemScope, $event, modelValue) => { this.editAvailability(date.toPipedDate()); }, () => { return true });

        return this.dateContextMenuHandler.getContextMenuOptions();
    }

    private getShiftContextMenuOptions(shiftId: number): any[] {
        // Context menu for shift

        // Get clicked shift
        var shift: ShiftDTO = this.getShiftById(shiftId);

        return this.createShiftContextMenuOptions(shift);
    }

    private createShiftContextMenuOptions(shift: ShiftDTO): any[] {
        if (this.isOrderPlanningMode && shift && !shift.isOrder && !shift.isBooking)
            return [];

        // If right clicking on a shift that is not selected, unselect all other
        if (!shift || !shift.selected)
            this.scheduleHandler.clearSelectedShifts();

        // Make sure linked shifts are selected
        if (shift)
            this.scheduleHandler.selectShift(shift);
        var selectedShifts = this.scheduleHandler.getSelectedShifts();

        // If right clicking a shift that does not belong to same employee as already selected shift,
        // it has not yet been unselected. Clear all selected shifts and reselect the current one.
        if (selectedShifts.length > 1 && shift) {
            _.forEach(selectedShifts, s => {
                if (s.employeeId !== shift.employeeId) {
                    this.scheduleHandler.clearSelectedShifts();
                    this.scheduleHandler.selectShift(shift);
                    selectedShifts = this.scheduleHandler.getSelectedShifts();
                    return false;
                }
            });
        }

        // If clicked shift does not exist in collection of selected shifts, unselect all selected shifts
        if (shift && !_.includes(selectedShifts.map(s => s.timeScheduleTemplateBlockId), shift.timeScheduleTemplateBlockId)) {
            this.scheduleHandler.clearSelectedShifts();
            selectedShifts = [];
        }

        this.shiftContextMenuHandler.clearContextMenuItems();

        // Edit shift
        if (shift && shift.isSchedule) {
            let title: string = (this.isMyShift(shift.timeScheduleTemplateBlockId) ? this.terms["time.schedule.planning.handleshift.title"] : this.terms["time.schedule.planning.handleshift.action.wanted"]).toString().format(this.shiftUndefined);
            this.shiftContextMenuHandler.addContextMenuItem(title, 'fa-pencil', ($itemScope, $event, modelValue) => { this.editShift(shift); }, () => { return true });
        }

        return this.shiftContextMenuHandler.getContextMenuOptions();
    }

    // PUBLIC METHODS (called from ScheduleHandler)

    public getShiftById(shiftId: number): ShiftDTO {
        var shift = _.find(this.myShifts, t => t.timeScheduleTemplateBlockId === shiftId);
        if (!shift)
            shift = _.find(this.openShifts, t => t.timeScheduleTemplateBlockId === shiftId);
        if (!shift)
            shift = _.find(this.colleaguesShifts, t => t.timeScheduleTemplateBlockId === shiftId);

        return shift;
    }

    public isMyShift(shiftId: number): boolean {
        return _.filter(this.myShifts, t => t.timeScheduleTemplateBlockId === shiftId).length > 0;
    }

    public isOpenShift(shiftId: number): boolean {
        return _.filter(this.openShifts, t => t.timeScheduleTemplateBlockId === shiftId).length > 0;
    }

    public isColleagueShift(shiftId: number): boolean {
        return _.filter(this.colleaguesShifts, t => t.timeScheduleTemplateBlockId === shiftId).length > 0;
    }

    public getShiftsOfSameType(shiftId: number): ShiftDTO[] {
        if (this.isMyShift(shiftId))
            return this.myShifts;
        else if (this.isOpenShift(shiftId))
            return this.openShifts;
        else if (this.isColleagueShift(shiftId))
            return this.colleaguesShifts;

        return [];
    }

    public getShifts(date: Date): ShiftDTO[] {
        if (!this.myShifts || !this.myShifts.length || !date)
            return [];

        var shfts = [];
        for (let i = 0, j = this.myShifts.length; i < j; i++) {
            if (this.myShifts[i].actualStartTime.isSameDayAs(date) || this.myShifts[i].actualStopTime.isSameDayAs(date)) {
                shfts.push(this.myShifts[i]);
            }
        }

        return shfts.sort(ShiftDTO.wholeDayStartTimeSort);
    }

    public getOpenShifts(date: Date): ShiftDTO[] {
        if (!this.openShifts || !this.openShifts.length || !date)
            return [];

        var shfts = [];
        for (let i = 0, j = this.openShifts.length; i < j; i++) {
            if (this.openShifts[i].actualStartTime.isSameDayAs(date) || this.openShifts[i].actualStopTime.isSameDayAs(date)) {
                shfts.push(this.openShifts[i]);
            }
        }

        return shfts.sort(ShiftDTO.wholeDayStartTimeSort);
    }

    public getColleaguesShifts(date: Date): ShiftDTO[] {
        if (!this.colleaguesShifts || !this.colleaguesShifts.length || !date)
            return [];

        var shfts = [];
        for (let i = 0, j = this.colleaguesShifts.length; i < j; i++) {
            if (this.colleaguesShifts[i].actualStartTime.isSameDayAs(date) || this.colleaguesShifts[i].actualStopTime.isSameDayAs(date)) {
                shfts.push(this.colleaguesShifts[i]);
            }
        }

        return shfts.sort(ShiftDTO.wholeDayStartTimeSort);
    }

    public isSaturday(date: Date): boolean {
        // Saturday
        if (date.getDay() === 6)
            return true;

        // Holiday not red day
        //var holiday = _.find(this.holidays, h => h.date.isSameDayAs(date) && !h.isRedDay);
        //if (holiday)
        //    return true;

        return false;
    }

    public isSunday(date: Date): boolean {
        // Sunday
        if (date.getDay() === 0)
            return true;

        // Holiday red day
        //var holiday = _.find(this.holidays, h => h.date.isSameDayAs(date) && h.isRedDay);
        //if (holiday)
        //    return true;

        return false;
    }

    // EVENTS

    public shiftSelected() {
    }

    public editAvailability(formattedDate: string) {
        var date: Date = null;
        if (formattedDate)
            date = formattedDate.parsePipedDate();

        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/EditEmployeeAvailability/EditEmployeeAvailabilityDialog.html"),
            controller: EditEmployeeAvailabilityDialogController,
            controllerAs: "ctrl",
            size: 'lg',
            resolve: {
                readOnly: () => { return false },
                employeeId: () => { return this.employee.employeeId },
                dateFrom: () => { return date ? date : this.dateFrom },
                dateTo: () => { return date ? date : this.dateTo },
                date: () => { return date },
                employeeInfo: () => { return null },
                commentMandatory: () => { return false }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.succeess) {
                if (this.onReloadEmployee)
                    this.onReloadEmployee();
            }
        });
    }

    public editShift(shift: ShiftDTO) {
        if (this.onEditShift)
            this.onEditShift({ shift: shift });
    }

    private toggleDate(formattedDate: string) {
        var date: Date = formattedDate.parsePipedDate();
        var msd: MyScheduleDate = _.find(this.dates, d => d.date.isSameDayAs(date));
        if (msd) {
            msd.expanded = !msd.expanded;
            if (msd.expanded) {
                if (this.onDateExpanded)
                    this.onDateExpanded({ date: date });
            } else {
                this.renderAll(true);
            }
        }
    }

    public isDateExpanded(date: Date): boolean {
        var msd: MyScheduleDate = _.find(this.dates, d => d.date.isSameDayAs(date));
        return msd && msd.expanded;
    }

    public isDateExpandedFormatted(formattedDate: string): boolean {
        var date: Date = formattedDate.parsePipedDate();
        return this.isDateExpanded(date);
    }

    // HELP-METHODS

    private setTimes() {
        // Get first start time and last end time overall for all shifts
        this.dayViewStartTime = null;
        this.dayViewEndTime = null;

        // Shifts
        for (let date: Date = this.dateFrom; date.isSameOrBeforeOnDay(this.dateTo); date = date.addDays(1)) {
            let dateShifts = this.getShifts(date);
            if (this.isDateExpanded(date)) {
                dateShifts = dateShifts.concat(this.getOpenShifts(date));
                dateShifts = dateShifts.concat(this.getColleaguesShifts(date));
            }
            dateShifts = _.orderBy(dateShifts, ['actualStartTime', 'actualStopTime']);
            if (dateShifts.length > 0)
                this.setTimesByDates(dateShifts[0].actualStartTime, _.last(dateShifts).actualStopTime);

            // Availability
            let dateAvailability: DateRangeDTO[] = this.employee ? _.filter(this.employee.available, a => a.start.isSameDayAs(date) && !(a.start.isBeginningOfDay() && a.stop.isEndOfDay())) : [];
            if (dateAvailability.length > 0)
                this.setTimesByDates(dateAvailability[0].start, _.last(dateAvailability).stop);
            let dateUnavailability: DateRangeDTO[] = this.employee ? _.filter(this.employee.unavailable, a => a.start.isSameDayAs(date) && !(a.start.isBeginningOfDay() && a.stop.isEndOfDay())) : [];
            if (dateUnavailability.length > 0)
                this.setTimesByDates(dateUnavailability[0].start, _.last(dateUnavailability).stop);
        }

        if (!this.dayViewStartTime)
            this.dayViewStartTime = 0;
        if (!this.dayViewEndTime)
            this.dayViewEndTime = 24 * 60;
        if (this.dayViewEndTime > this.dayViewStartTime + (24 * 60))
            this.dayViewEndTime = this.dayViewStartTime + (24 * 60);
    }

    private setTimesByDates(startTime: Date, endTime: Date) {
        let dayStartTime = (startTime.getHours() > 0 ? startTime.addMinutes(-30).getHours() : startTime.getHours()) * 60;
        let dayEndTime = (endTime.getHours() < 23 ? (endTime.addMinutes(30).getHours() + 1) : endTime.getHours() + 1) * 60;

        // Handle over midnight
        let deltaDays = endTime.beginningOfDay().diffDays(startTime.beginningOfDay());
        dayEndTime += (60 * 24 * deltaDays);

        if (!this.dayViewStartTime || this.dayViewStartTime > dayStartTime)
            this.dayViewStartTime = dayStartTime;
        if (!this.dayViewEndTime || this.dayViewEndTime < dayEndTime)
            this.dayViewEndTime = dayEndTime;
    }

    private setDateRange(keepDatesExpanded: boolean) {
        if (this.nbrOfVisibleHours === 0)
            return;

        if (!keepDatesExpanded || this.dates.length === 0)
            this.dates = MyScheduleDate.createDates(this.dateFrom, this.dateTo);

        this.timeSlots = [];
        for (var i: number = 0; i < this.nbrOfVisibleHours; i++) {
            this.timeSlots.push(this.dayViewStartTime + (i * 60));
        }
    }

    private setEnableAvailibility() {
        this.enableAvailibility = (this.employee && (this.availablePermission || this.notAvailablePermission));
    }

    // Shifts

    private setShiftToolTips() {
        _.forEach(this.myShifts, shift => {
            this.setShiftLabel(shift, false);
            this.setShiftToolTip(this.myShifts, shift);
        });
        _.forEach(this.openShifts, shift => {
            this.setShiftLabel(shift, false);
            this.setShiftToolTip(_.filter(this.openShifts, s => s.link === shift.link), shift);
        });
        _.forEach(this.colleaguesShifts, shift => {
            // Don't show the specific cause on colleagues
            this.setShiftLabel(shift, true);
            this.setShiftToolTip(this.colleaguesShifts, shift);
        });
    }

    private setShiftLabel(shift: ShiftDTO, hideTimeDevistionCauseName: boolean) {
        shift.setLabel(this.terms["time.schedule.planning.breaklabel"], this.terms["time.schedule.planning.wholedaylabel"], this.terms["common.absence"], false, this.hasMultipleEmployeeAccounts, true, TermGroup_OrderPlanningShiftInfo.NoInfo, TermGroup_OrderPlanningShiftInfo.NoInfo, TermGroup_OrderPlanningShiftInfo.NoInfo, this.useMultipleScheduleTypes, hideTimeDevistionCauseName, false);
    }

    private setShiftToolTip(allShifts: ShiftDTO[], shift: ShiftDTO) {
        if (!shift)
            return;

        var toolTip: string = '';
        var wholeDayToolTip: string = '';
        var breakPrefix: string = this.terms["time.schedule.planning.breakprefix"];
        var isHiddenEmployee: boolean = false; //(shift.employeeId === this.hiddenEmployeeId);

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

            // Order number, customer and delivery address
            if (shift.isOrder) {
                toolTip += "\n";
                if (shift.order.orderNr)
                    toolTip += "{0}, ".format(shift.order.orderNr.toString());
                toolTip += shift.order.customerName;
                if (shift.order.deliveryAddress && shift.order.deliveryAddress.length > 0)
                    toolTip += ", {0}".format(shift.order.deliveryAddress);
            }
        }

        // Schedule type
        if (!shift.isOrder && (shift.getTimeScheduleTypeNames(this.useMultipleScheduleTypes)))
            toolTip += " - {0}".format(shift.getTimeScheduleTypeNames(this.useMultipleScheduleTypes));

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

        // Order planning
        if (shift.isOrder) {
            toolTip += "\n{0}: {1}".format(this.terms["time.schedule.planning.orderstatus"], shift.order.attestStateName);
            if (shift.order.workingDescription && shift.order.workingDescription.length > 0)
                toolTip += "\n\n{0}".format(shift.order.workingDescription);
            if (shift.order.internalDescription && shift.order.internalDescription.length > 0)
                toolTip += "\n\n{0}".format(shift.order.internalDescription);
        }

        // Whole day

        var dayShifts: ShiftDTO[] = [];

        // If whole day absence, skip this part
        dayShifts = _.filter(allShifts, s => s.employeeId === shift.employeeId &&
            ((s.actualStartTime.isSameDayAs(shift.actualStartTime) && !s.belongsToPreviousDay && !s.belongsToNextDay) || (s.actualStartTime.isSameDayAs(shift.actualStartTime.addDays(1)) && s.belongsToPreviousDay) || (s.actualStartTime.isSameDayAs(shift.actualStartTime.addDays(-1)) && s.belongsToNextDay)) &&
            (s.link === shift.link || !isHiddenEmployee) &&
            !((s.isAbsence || s.isAbsenceRequest) && CalendarUtility.toFormattedTime(s.actualStartTime, true) === '00:00:00' && CalendarUtility.toFormattedTime(s.actualStopTime, true) === '23:59:59'));

        var minutes: number = _.sumBy(_.filter(dayShifts, s => !s.isStandby && !s.isOnDuty), s => s.getShiftLength());
        var factorMinutes: number = 0;

        if (dayShifts.length > 0) {
            // Get all breaks

            var timeCode = _.find(this.breakTimeCodes, b => b.timeCodeId === shift.break1TimeCodeId);
            var break1TimeCode: string = timeCode ? timeCode.name : '';
            if (!break1TimeCode.startsWithCaseInsensitive(breakPrefix))
                break1TimeCode = "{0} {1}".format(breakPrefix, break1TimeCode);

            timeCode = _.find(this.breakTimeCodes, b => b.timeCodeId === shift.break2TimeCodeId);
            var break2TimeCode: string = timeCode ? timeCode.name : '';
            if (!break2TimeCode.startsWithCaseInsensitive(breakPrefix))
                break2TimeCode = "{0} {1}".format(breakPrefix, break2TimeCode);

            timeCode = _.find(this.breakTimeCodes, b => b.timeCodeId === shift.break3TimeCodeId);
            var break3TimeCode: string = timeCode ? timeCode.name : '';
            if (!break3TimeCode.startsWithCaseInsensitive(breakPrefix))
                break3TimeCode = "{0} {1}".format(breakPrefix, break3TimeCode);

            timeCode = _.find(this.breakTimeCodes, b => b.timeCodeId === shift.break4TimeCodeId);
            var break4TimeCode: string = timeCode ? timeCode.name : '';
            if (!break4TimeCode.startsWithCaseInsensitive(breakPrefix))
                break4TimeCode = "{0} {1}".format(breakPrefix, break4TimeCode);

            var break1: string = shift.break1TimeCodeId !== 0 && (shift.break1Link === shift.link || !isHiddenEmployee) ? "\n{0}-{1}  {2}".format(shift.break1StartTime.toFormattedTime(), shift.break1StartTime.addMinutes(shift.break1Minutes).toFormattedTime(), break1TimeCode) : '';
            var break2: string = shift.break2TimeCodeId !== 0 && (shift.break2Link === shift.link || !isHiddenEmployee) ? "\n{0}-{1}  {2}".format(shift.break2StartTime.toFormattedTime(), shift.break2StartTime.addMinutes(shift.break2Minutes).toFormattedTime(), break2TimeCode) : '';
            var break3: string = shift.break3TimeCodeId !== 0 && (shift.break3Link === shift.link || !isHiddenEmployee) ? "\n{0}-{1}  {2}".format(shift.break3StartTime.toFormattedTime(), shift.break3StartTime.addMinutes(shift.break3Minutes).toFormattedTime(), break3TimeCode) : '';
            var break4: string = shift.break4TimeCodeId !== 0 && (shift.break4Link === shift.link || !isHiddenEmployee) ? "\n{0}-{1}  {2}".format(shift.break4StartTime.toFormattedTime(), shift.break4StartTime.addMinutes(shift.break4Minutes).toFormattedTime(), break4TimeCode) : '';

            if (shift.isSchedule || shift.isStandby)
                wholeDayToolTip += "{0}:".format(this.terms["time.schedule.planning.todaysschedule"]);
            else if (shift.isOrder)
                wholeDayToolTip += "{0}:".format(this.terms["time.schedule.planning.todaysorders"]);
            else if (shift.isBooking)
                wholeDayToolTip += "{0}:".format(this.terms["time.schedule.planning.todaysbookings"]);

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

                // Order number, customer and delivery address
                if (dayShift.isOrder) {
                    wholeDayToolTip += "\n";
                    if (dayShift.order.orderNr)
                        wholeDayToolTip += "{0}, ".format(dayShift.order.orderNr.toString());
                    wholeDayToolTip += dayShift.order.customerName;
                    if (dayShift.order.deliveryAddress && dayShift.order.deliveryAddress.length > 0)
                        wholeDayToolTip += ", {0}".format(dayShift.order.deliveryAddress);
                }

                // TimeScheduleType factor multiplyer
                factorMinutes += dayShift.getTimeScheduleTypeFactorsWithinShift();
            });

            if (shift.isSchedule || shift.isStandby) {
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
        else {
            var shiftTypeName: string = '';
            if (shift.isSchedule || shift.isStandby)
                shiftTypeName = this.terms["time.schedule.planning.thisshift"];
            else if (shift.isOrder)
                shiftTypeName = this.terms["time.schedule.planning.thisorder"];
            else if (shift.isBooking)
                shiftTypeName = this.terms["time.schedule.planning.thisbooking"];

            shift.toolTip = (toolTip.length > 0 ? "{0}:\n{1}\n\n".format(shiftTypeName, toolTip) : '') + wholeDayToolTip;
        }

        if (shift.availabilityToolTip)
            shift.toolTip += "\n\n{0}".format(shift.availabilityToolTip);
    }

    public getBreakToolTip(shift: ShiftDTO, breakNo: number): string {
        var timeCode = _.find(this.breakTimeCodes, t => t.timeCodeId === shift[`break${breakNo}TimeCodeId`]);
        var toolTip: string = timeCode ? timeCode.name : '';

        var breakPrefix: string = this.terms["time.schedule.planning.breakprefix"];
        if (!toolTip || !toolTip.startsWithCaseInsensitive(breakPrefix))
            toolTip = "{0} {1}".format(breakPrefix, toolTip);

        return toolTip;
    }
}

class MyScheduleDate {
    constructor(date: Date) {
        this.date = date;
        this.expanded = false;
    }

    public date: Date;
    public expanded: boolean;

    public static createDates(dateFrom: Date, dateTo: Date): MyScheduleDate[] {
        let range: Date[] = CalendarUtility.getDates(dateFrom, dateTo);
        let dates: MyScheduleDate[] = [];

        _.forEach(range, d => {
            dates.push(new MyScheduleDate(d));
        });

        return dates;
    }
}
