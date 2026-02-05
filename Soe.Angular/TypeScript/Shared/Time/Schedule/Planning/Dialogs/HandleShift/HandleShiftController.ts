import { ICoreService } from "../../../../../../Core/Services/CoreService";
import { ITimeService as ISharedTimeService } from "../../../../Time/TimeService";
import { IScheduleService as ISharedScheduleService } from "../../../ScheduleService";
import { ITranslationService } from "../../../../../../Core/Services/TranslationService";
import { ShiftDTO } from "../../../../../../Common/Models/TimeSchedulePlanningDTOs";
import { SOEMessageBoxImage, SOEMessageBoxButtons, SOEMessageBoxSize } from "../../../../../../Util/Enumerations";
import { Feature, TermGroup_TimeScheduleTemplateBlockShiftUserStatus, HandleShiftAction } from "../../../../../../Util/CommonEnumerations";
import { CalendarUtility } from "../../../../../../Util/CalendarUtility";
import { ITimeScheduleShiftQueueDTO, ITimeDeviationCauseDTO, ISmallGenericType, ITimeCodeBreakSmallDTO } from "../../../../../../Scripts/TypeLite.Net4";
import { StringUtility } from "../../../../../../Util/StringUtility";
import { INotificationService } from "../../../../../../Core/Services/NotificationService";

export class HandleShiftController {

    // Terms
    private terms: any = [];
    private shiftDefined: string;
    private shiftUndefined: string;
    private shiftsDefined: string;
    private shiftsUndefined: string;
    private title: string;
    private headerRow1: string;
    private headerRow2: string;
    private dateRangeText: string;
    private employeeText: string;
    private queueTitle: string;

    private buttonWantedLabel: string;
    private buttonUndoWantedLabel: string;
    private buttonUnwantedLabel: string;
    private buttonUndoUnwantedLabel: string;
    private buttonAbsenceLabel: string;
    private buttonUndoAbsenceLabel: string;

    private informationWanted: string;
    private informationUndoWanted: string;
    private informationUnwanted: string;
    private informationUndoUnwanted: string;
    private informationAbsence: string;
    private informationWholeDayAbsence: string;
    private informationUndoAbsence: string;
    private informationDeviationCause: string;
    private informationChangeEmployee: string;
    private informationSwapEmployee: string;
    private informationAbsenceAnnouncement: string;
    private noActionAvailableInfo: string;

    // Permissions
    private hasActionPermission: boolean = false;
    private wantedActionPermission: boolean = false;
    private unwantedActionPermission: boolean = false;
    private absenceActionPermission: boolean = false;
    private changeEmployeeActionPermission: boolean = false;
    private swapEmployeeActionPermission: boolean = false;
    private absenceAnnouncementActionPermission: boolean = false;
    private showQueuePermission: boolean = false;

    // Lookups
    private deviationCauses: ITimeDeviationCauseDTO[];
    private deviationCausesAbsenceAnnouncement: ISmallGenericType[];
    private breakTimeCodes: ITimeCodeBreakSmallDTO[];
    private employeeChilds: ISmallGenericType[];

    // Flags
    private multipleDates: boolean = false;
    private hasMultipleEmployeeAccounts: boolean = false;
    private isEmployeeInQueue: boolean;

    private showWanted: boolean = false;
    private showUndoWanted: boolean = false;
    private showUnwanted: boolean = false;
    private showUndoUnwanted: boolean = false;
    private showAbsence: boolean = false;
    private showUndoAbsence: boolean = false;
    private showChangeEmployee: boolean = false;
    private showSwapEmployee: boolean = false;
    private showAbsenceAnnouncement: boolean = false;

    private executing: boolean = false;

    // Skills
    private shiftTypeIds: number[] = [];
    private _invalidSkills: boolean = false;
    get invalidSkills(): boolean {
        return this._invalidSkills;
    }
    set invalidSkills(value: boolean) {
        this._invalidSkills = value;
        if (value) {
            this.skillsOpen = true;
        }
    }
    private skillsOpen: boolean = false;
    private skillsOpened() {
        this.skillsOpen = true;
    }

    // Properties
    private get multipleShifts(): boolean {
        return this.shifts.length > 1;
    }

    private get customerInfo(): string {
        return this.selectedShift && this.selectedShift.order ? "({0}) {1}".format(this.selectedShift.order.customerNr, this.selectedShift.order.customerName) : '';
    }

    private get projectInfo(): string {
        return this.selectedShift && this.selectedShift.order ? "({0}) {1}".format(this.selectedShift.order.projectNr, this.selectedShift.order.projectName) : '';
    }

    private _selectedAction: HandleShiftAction = HandleShiftAction.Cancel;
    private get selectedAction(): HandleShiftAction {
        return this._selectedAction;
    }
    private set selectedAction(value: HandleShiftAction) {
        this._selectedAction = value;

        if (this._selectedAction == HandleShiftAction.Absence)
            this.loadDeviationCauses();
        else if (this._selectedAction == HandleShiftAction.AbsenceAnnouncement)
            this.loadDeviationCausesAbsenceAnnouncement();
    }

    private get selectedActionValue(): string {
        return this.selectedAction.toString();
    }
    private set selectedActionValue(value: string) {
        this.selectedAction = parseInt(value);
    }

    private _selectedDeviationCause: ITimeDeviationCauseDTO;
    private get selectedDeviationCause(): ITimeDeviationCauseDTO {
        return this._selectedDeviationCause;
    }
    private set selectedDeviationCause(value: ITimeDeviationCauseDTO) {
        this._selectedDeviationCause = value;

        if (this._selectedDeviationCause && this._selectedDeviationCause.specifyChild)
            this.loadEmployeeChilds();
    }

    private selectedDeviationCauseAbsenceAnnouncement: ISmallGenericType;

    private selectedShift: ShiftDTO;
    private selectedQueue: ITimeScheduleShiftQueueDTO;
    private selectedEmployeeId: number;
    private selectedEmployeeChildId: number;
    private isWholeDayAbsence: boolean = false;

    private get noActionAvailable(): boolean {
        return !this.showWanted && !this.showUndoWanted && !this.showUnwanted && !this.showUndoUnwanted && !this.showAbsence && !this.showUndoAbsence && !this.showChangeEmployee && !this.showSwapEmployee && !this.showAbsenceAnnouncement;
    }

    private breaks: ShiftDTO[];

    //@ngInject
    constructor(
        private $uibModalInstance,
        private $q: ng.IQService,
        private coreService: ICoreService,
        private sharedScheduleService: ISharedScheduleService,
        private sharedTimeService: ISharedTimeService,
        private notificationService: INotificationService,
        private translationService: ITranslationService,
        private isSchedulePlanningMode: boolean,
        private isOrderPlanningMode: boolean,
        private employeeId: number,
        private employeeGroupId: number,
        private shifts: ShiftDTO[],
        private showSkills: boolean,
        private useAccountHierarchy: boolean) {

        $q.all([
            this.loadModifyPermissions(),
            this.loadTerms(),
            this.loadBreakTimeCodes(),
            this.getHasMultipleEmployeeAccounts()
        ]).then(() => {
            this.setup();
            this.setShiftTypeIds();
            this.showButtons();
        })
    }

    // SETUP

    private setup() {
        let firstShift: ShiftDTO = _.orderBy(_.filter(this.shifts, s => !s.belongsToPreviousDay && !s.belongsToNextDay), s => s.actualStartTime)[0];
        let lastShift: ShiftDTO = _.orderBy(_.filter(this.shifts, s => !s.belongsToPreviousDay && !s.belongsToNextDay), s => s.actualStopTime, 'desc')[0];
        this.selectedShift = firstShift;

        this.multipleDates = !firstShift.startTime.isSameDayAs(lastShift.stopTime);
        this.dateRangeText = this.multipleDates ? "{0} - {1}".format(firstShift.actualStartTime.toFormattedDate(), lastShift.actualStopTime.toFormattedDate()) : firstShift.actualStartTime.toFormattedDate();
        this.employeeText = firstShift.employeeName;

        // Setup GUI based on permissions
        if (this.hasActionPermission) {
            // Header
            this.headerRow1 = this.terms["time.schedule.planning.handleshift.headerrow1"].format(this.multipleShifts ? this.shiftsDefined : this.shiftDefined);
            this.headerRow2 = this.terms["time.schedule.planning.handleshift.headerrow2"];
        } else {
            // Header
            this.headerRow1 = this.terms["time.schedule.planning.handleshift.headerrow1.nopermission"].format(this.multipleShifts ? this.shiftsDefined : this.shiftDefined);
            this.headerRow2 = this.terms["time.schedule.planning.handleshift.headerrow2.nopermission"].format(this.multipleShifts ? this.shiftsDefined : this.shiftDefined);
        }

        this.createBreaksFromShift();

        if (this.isOrderPlanningMode && firstShift.isOrder)
            this.setOrderInfo();
    }

    // SERVICE CALLS

    private loadModifyPermissions(): ng.IPromise<any> {
        var features: number[] = [];

        features.push(Feature.Time_Schedule_SchedulePlanningUser_HandleShiftWanted);
        features.push(Feature.Time_Schedule_SchedulePlanningUser_HandleShiftUnwanted);
        features.push(Feature.Time_Schedule_SchedulePlanningUser_HandleShiftAbsence);
        features.push(Feature.Time_Schedule_SchedulePlanningUser_HandleShiftChangeEmployee);
        features.push(Feature.Time_Schedule_SchedulePlanningUser_HandleShiftSwapEmployee);
        features.push(Feature.Time_Schedule_SchedulePlanningUser_HandleShiftAbsenceAnnouncement);
        features.push(Feature.Time_Schedule_SchedulePlanningUser_HandleShiftShowQueue);

        return this.coreService.hasReadOnlyPermissions(features).then((x) => {
            this.wantedActionPermission = x[Feature.Time_Schedule_SchedulePlanningUser_HandleShiftWanted];
            this.unwantedActionPermission = x[Feature.Time_Schedule_SchedulePlanningUser_HandleShiftUnwanted];
            this.absenceActionPermission = x[Feature.Time_Schedule_SchedulePlanningUser_HandleShiftAbsence];
            this.changeEmployeeActionPermission = x[Feature.Time_Schedule_SchedulePlanningUser_HandleShiftChangeEmployee];
            this.swapEmployeeActionPermission = x[Feature.Time_Schedule_SchedulePlanningUser_HandleShiftSwapEmployee];
            this.absenceAnnouncementActionPermission = x[Feature.Time_Schedule_SchedulePlanningUser_HandleShiftAbsenceAnnouncement];
            this.showQueuePermission = x[Feature.Time_Schedule_SchedulePlanningUser_HandleShiftShowQueue];

            this.hasActionPermission = (
                this.wantedActionPermission ||
                this.unwantedActionPermission ||
                this.absenceActionPermission ||
                this.changeEmployeeActionPermission ||
                this.swapEmployeeActionPermission ||
                this.absenceAnnouncementActionPermission);
        });
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.unabletosave",
            "time.schedule.planning.shiftqueue.title",
            "time.schedule.planning.handleshift.title",
            "time.schedule.planning.handleshift.headerrow1",
            "time.schedule.planning.handleshift.headerrow1.nopermission",
            "time.schedule.planning.handleshift.headerrow2",
            "time.schedule.planning.handleshift.headerrow2.nopermission",
            "time.schedule.planning.handleshift.actions",
            "time.schedule.planning.handleshift.action.wanted",
            "time.schedule.planning.handleshift.action.undowanted",
            "time.schedule.planning.handleshift.action.unwanted",
            "time.schedule.planning.handleshift.action.undounwanted",
            "time.schedule.planning.handleshift.action.absence",
            "time.schedule.planning.handleshift.action.undoabsence",
            "time.schedule.planning.handleshift.action.changeemployee",
            "time.schedule.planning.handleshift.action.swapemployee",
            "time.schedule.planning.handleshift.action.absenceannouncement",
            "time.schedule.planning.handleshift.information.wanted",
            "time.schedule.planning.handleshift.information.undowanted",
            "time.schedule.planning.handleshift.information.unwanted",
            "time.schedule.planning.handleshift.information.undounwanted",
            "time.schedule.planning.handleshift.information.absence",
            "time.schedule.planning.handleshift.information.mustbeapproved",
            "time.schedule.planning.handleshift.information.absence.deviationcause",
            "time.schedule.planning.handleshift.information.undoabsence",
            "time.schedule.planning.handleshift.information.changeemployee",
            "time.schedule.planning.handleshift.information.swapemployee",
            "time.schedule.planning.handleshift.information.absenceannouncement",
            "time.schedule.planning.handleshift.noactionavailable",
            "time.schedule.planning.handleshift.missingdeviationcause",
            "time.schedule.planning.handleshift.missingchild"
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

            this.title = this.terms["time.schedule.planning.handleshift.title"].format(this.shiftsUndefined);
            this.queueTitle = this.terms["time.schedule.planning.shiftqueue.title"].format(this.shiftUndefined);

            this.buttonWantedLabel = this.terms["time.schedule.planning.handleshift.action.wanted"].format(this.multipleShifts ? this.shiftsDefined : this.shiftDefined);
            this.informationWanted = this.terms["time.schedule.planning.handleshift.information.wanted"].format(this.multipleShifts ? this.shiftsDefined : this.shiftDefined);
            this.buttonUndoWantedLabel = this.terms["time.schedule.planning.handleshift.action.undowanted"].format(this.multipleShifts ? this.shiftsDefined : this.shiftDefined);
            this.informationUndoWanted = this.terms["time.schedule.planning.handleshift.information.undowanted"].format(this.multipleShifts ? this.shiftsDefined : this.shiftDefined);

            this.informationUnwanted = this.terms["time.schedule.planning.handleshift.information.unwanted"].format((this.multipleShifts ? this.shiftsDefined : this.shiftDefined).toUpperCaseFirstLetter(), this.multipleShifts ? this.shiftsDefined : this.shiftDefined);
            this.buttonUnwantedLabel = this.terms["time.schedule.planning.handleshift.action.unwanted"].format(this.multipleShifts ? this.shiftsDefined : this.shiftDefined);
            this.informationUndoUnwanted = this.terms["time.schedule.planning.handleshift.information.undounwanted"].format(this.multipleShifts ? this.shiftsDefined : this.shiftDefined, this.multipleShifts ? this.shiftsDefined : this.shiftDefined);
            this.buttonUndoUnwantedLabel = this.terms["time.schedule.planning.handleshift.action.undounwanted"].format(this.multipleShifts ? this.shiftsDefined : this.shiftDefined);

            this.informationAbsence = this.terms["time.schedule.planning.handleshift.information.absence"].format((this.multipleShifts ? this.shiftsDefined : this.shiftDefined).toUpperCaseFirstLetter());
            this.informationAbsence += " {0}".format(this.terms["time.schedule.planning.handleshift.information.mustbeapproved"]);
            this.buttonAbsenceLabel = this.terms["time.schedule.planning.handleshift.action.absence"].format(this.multipleShifts ? this.shiftsDefined : this.shiftDefined);
            this.informationDeviationCause = this.terms["time.schedule.planning.handleshift.information.absence.deviationcause"].format(this.multipleShifts ? this.shiftsDefined : this.shiftDefined);
            this.informationUndoAbsence = this.terms["time.schedule.planning.handleshift.information.undoabsence"].format(this.multipleShifts ? this.shiftsDefined : this.shiftDefined);
            this.buttonUndoAbsenceLabel = this.terms["time.schedule.planning.handleshift.action.undoabsence"].format(this.multipleShifts ? this.shiftsDefined : this.shiftDefined);

            this.informationChangeEmployee = this.terms["time.schedule.planning.handleshift.information.changeemployee"].format((this.multipleShifts ? this.shiftsDefined : this.shiftDefined).toUpperCaseFirstLetter(), this.multipleShifts ? this.shiftsDefined : this.shiftDefined);
            this.informationChangeEmployee += " {0}".format(this.terms["time.schedule.planning.handleshift.information.mustbeapproved"]);
            this.informationSwapEmployee = this.terms["time.schedule.planning.handleshift.information.swapemployee"].format(this.shiftUndefined, this.shiftUndefined);
            this.informationSwapEmployee += " {0}".format(this.terms["time.schedule.planning.handleshift.information.mustbeapproved"]);

            this.informationAbsenceAnnouncement = this.terms["time.schedule.planning.handleshift.information.absenceannouncement"].format((this.multipleShifts ? this.shiftsDefined : this.shiftDefined).toUpperCaseFirstLetter());

            this.noActionAvailableInfo = StringUtility.ToBr(this.terms["time.schedule.planning.handleshift.noactionavailable"]);
        });
    }

    private loadDeviationCauses() {
        if (this.deviationCauses && this.deviationCauses.length > 0)
            return;

        this.sharedTimeService.getAbsenceTimeDeviationCauses().then(x => {
            this.deviationCauses = x;
        });
    }

    private loadDeviationCausesAbsenceAnnouncement() {
        if (this.deviationCausesAbsenceAnnouncement && this.deviationCausesAbsenceAnnouncement.length > 0)
            return;

        this.sharedTimeService.getAbsenceTimeDeviationCausesAbsenceAnnouncement(this.employeeGroupId, false).then(x => {
            this.deviationCausesAbsenceAnnouncement = x;
        });
    }

    private loadBreakTimeCodes(): ng.IPromise<any> {
        return this.sharedScheduleService.getTimeCodeBreaks(false).then((x) => {
            this.breakTimeCodes = x;
        });
    }

    private getHasMultipleEmployeeAccounts() {
        if (!this.useAccountHierarchy)
            return;

        var firstShift: ShiftDTO = _.orderBy(_.filter(this.shifts, s => !s.belongsToPreviousDay && !s.belongsToNextDay), s => s.actualStartTime)[0];
        var lastShift: ShiftDTO = _.orderBy(_.filter(this.shifts, s => !s.belongsToPreviousDay && !s.belongsToNextDay), s => s.actualStopTime, 'desc')[0];

        this.sharedScheduleService.hasMultipleEmployeeAccounts(this.employeeId, firstShift.actualStartTime, lastShift.actualStopTime).then(x => {
            this.hasMultipleEmployeeAccounts = x;
        });
    }

    private loadEmployeeChilds() {
        if (this.employeeChilds && this.employeeChilds.length > 0)
            return;

        this.sharedTimeService.getEmployeeChildsDict(this.shifts[0].employeeId, false).then(x => {
            this.employeeChilds = x;
            if (this.employeeChilds.length > 0)
                this.selectedEmployeeChildId = this.employeeChilds[0].id;
        });
    }

    // EVENTS

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }

    private close() {
        this.$uibModalInstance.close({ success: true, action: this.selectedAction, reload: true });
    }

    private save() {
        this.executing = true;

        if (this.validate()) {
            let deviationCauseId: number = 0;
            if (this.selectedAction == HandleShiftAction.Absence && this.selectedDeviationCause)
                deviationCauseId = this.selectedDeviationCause.timeDeviationCauseId;
            else if (this.selectedAction == HandleShiftAction.AbsenceAnnouncement && this.selectedDeviationCauseAbsenceAnnouncement)
                deviationCauseId = this.selectedDeviationCauseAbsenceAnnouncement.id;

            this.sharedScheduleService.handleShift(this.selectedAction, this.shifts[0].timeScheduleTemplateBlockId, deviationCauseId, this.employeeId, 0, true).then(result => {
                if (result) {
                    if (result.success)
                        this.close();
                    else if (result.errorNumber || result.errorMessage) {
                        this.translationService.translate("core.workfailed").then(term => {
                            this.notificationService.showDialogEx(term, result.errorMessage ? result.errorMessage : result.errorNumber.toString(), SOEMessageBoxImage.Error);
                            this.executing = false;
                        });
                    }
                }
            });
        } else {
            this.executing = false;
        }
    }

    // HELP-METHODS

    private createBreaksFromShift() {
        if (!this.shifts || this.shifts.length === 0)
            return;

        // Create break shifts from break information on shift DTO
        let dates: Date[] = [];
        _.forEach(this.shifts, shift => {
            let date = _.find(dates, d => d.isSameDayAs(shift.startTime));
            if (!date)
                dates.push(shift.startTime.date());
        });

        this.breaks = [];
        _.forEach(dates, date => {
            let breaksForDate = _.head(_.filter(this.shifts, s => s.startTime.isSameDayAs(date))).createBreaksFromShift(date);
            if (breaksForDate.length > 0) {
                _.forEach(breaksForDate, brk => {
                    this.setBreakTimeCodeFromTimeCodeId(brk);
                    this.breaks.push(brk);
                });
            }
        });
    }

    private setBreakTimeCodeFromTimeCodeId(brk: ShiftDTO) {
        if (brk && brk.isBreak && brk.break1TimeCodeId) {
            var timeCode = _.find(this.breakTimeCodes, t => t.timeCodeId === brk.break1TimeCodeId);
            brk.shiftTypeName = timeCode ? timeCode.name : '';
            brk.break1TimeCodeId = timeCode ? brk.break1TimeCodeId : 0;
        }
    }

    private setShiftTypeIds() {
        this.shiftTypeIds = _.uniq(_.map(_.filter(this.shifts, s => !s.isBreak && s.shiftTypeId !== 0), s => s.shiftTypeId));
    }

    private setOrderInfo() {
        console.log("setOrderInfo");
    }

    private showButtons() {
        // Show buttons depending on permission and shift user status

        var shiftIsMine: boolean = (this.employeeId === this.shifts[0].employeeId);

        this.showWanted = this.wantedActionPermission && !shiftIsMine;
        this.showUndoWanted = false;
        this.showUnwanted = false; // this.unwantedActionPermission && shiftIsMine;
        this.showUndoUnwanted = false;
        this.showAbsence = this.absenceActionPermission && shiftIsMine;
        this.showUndoAbsence = false;
        this.showChangeEmployee = false; // this.changeEmployeeActionPermission && shiftIsMine;
        this.showSwapEmployee = false; // this.swapEmployeeActionPermission && shiftIsMine;
        this.showAbsenceAnnouncement = this.absenceAnnouncementActionPermission && shiftIsMine;

        if (this.showWanted && _.filter(this.shifts, s => s.iamInQueue).length > 0) {
            // Employee already in queue, show undo button instead
            this.showWanted = false;
            this.showUndoWanted = true;
        }

        switch (this.shifts[0].shiftUserStatus) {
            case TermGroup_TimeScheduleTemplateBlockShiftUserStatus.None:
                break;
            case TermGroup_TimeScheduleTemplateBlockShiftUserStatus.Accepted:
                break;
            case TermGroup_TimeScheduleTemplateBlockShiftUserStatus.Unwanted:
                if (this.showUnwanted) {
                    // Shift already marked as unwanted, show undo button instead
                    this.showUnwanted = false;
                    this.showUndoUnwanted = true;
                }
                break;
            case TermGroup_TimeScheduleTemplateBlockShiftUserStatus.AbsenceRequested:
                if (this.showAbsence) {
                    // Already requested for absence, show undo button instead
                    this.showAbsence = false;
                    this.showAbsenceAnnouncement = false;
                    this.showUndoAbsence = true;
                }
                break;
            case TermGroup_TimeScheduleTemplateBlockShiftUserStatus.AbsenceApproved:
                // Absence request not pending anymore, no action can be performed here
                this.showWanted = false;
                this.showUndoWanted = false;
                this.showUnwanted = false;
                this.showUndoUnwanted = false;
                this.showAbsence = false;
                this.showUndoAbsence = false;
                this.showAbsenceAnnouncement = false;
                this.showChangeEmployee = false;
                this.showSwapEmployee = false;
                break;
        }

        // If an action is already performed, only show coresponding undo button
        if (this.showUndoWanted) {
            this.showWanted = false;
            this.showUnwanted = false;
            this.showUndoUnwanted = false;
            this.showAbsence = false;
            this.showUndoAbsence = false;
            this.showAbsenceAnnouncement = false;
            this.showChangeEmployee = false;
            this.showSwapEmployee = false;
        } else if (this.showUndoUnwanted) {
            this.showWanted = false;
            this.showUndoWanted = false;
            this.showUnwanted = false;
            this.showAbsence = false;
            this.showUndoAbsence = false;
            this.showAbsenceAnnouncement = false;
            this.showChangeEmployee = false;
            this.showSwapEmployee = false;
        } else if (this.showUndoAbsence) {
            this.showWanted = false;
            this.showUndoWanted = false;
            this.showUnwanted = false;
            this.showUndoUnwanted = false;
            this.showAbsence = false;
            this.showAbsenceAnnouncement = false;
            this.showChangeEmployee = false;
            this.showSwapEmployee = false;
        }

        if (this.shifts[0].actualStartTime.isBeforeOnDay(CalendarUtility.getDateToday()) ||
            this.shifts[0].actualStartTime.isAfterOnDay(CalendarUtility.getDateToday().addDays(1)))
            this.showAbsenceAnnouncement = false;

        // Set default selected action
        if (this.showWanted)
            this.selectedAction = HandleShiftAction.Wanted;
        else if (this.showUndoWanted)
            this.selectedAction = HandleShiftAction.UndoWanted;
        else if (this.showUnwanted)
            this.selectedAction = HandleShiftAction.Unwanted;
        else if (this.showUndoUnwanted)
            this.selectedAction = HandleShiftAction.UndoUnwanted;
        else if (this.showAbsence)
            this.selectedAction = HandleShiftAction.Absence;
        else if (this.showUndoAbsence)
            this.selectedAction = HandleShiftAction.UndoAbsence;
        else if (this.showChangeEmployee)
            this.selectedAction = HandleShiftAction.ChangeEmployee;
        else if (this.showSwapEmployee)
            this.selectedAction = HandleShiftAction.SwapEmployee;
        else if (this.showAbsenceAnnouncement)
            this.selectedAction = HandleShiftAction.AbsenceAnnouncement;
    }

    private validate(): boolean {
        var validationErrors: string = '';
        var isValid: boolean = true;

        if (this.selectedAction == HandleShiftAction.Wanted || this.selectedAction == HandleShiftAction.UndoWanted || this.selectedAction == HandleShiftAction.Unwanted || this.selectedAction == HandleShiftAction.UndoUnwanted) {
            this.selectedEmployeeId = this.employeeId;
        } else if (this.selectedAction == HandleShiftAction.Absence) {
            if (!this.selectedDeviationCause || this.selectedDeviationCause.timeDeviationCauseId === 0) {
                isValid = false;
                validationErrors += this.terms["time.schedule.planning.handleshift.missingdeviationcause"].format(this.buttonAbsenceLabel) + "\n";
            } else if (this.selectedDeviationCause.specifyChild && !this.selectedEmployeeChildId) {
                isValid = false;
                validationErrors += this.terms["time.schedule.planning.handleshift.missingchild"] + "\n";
            }
        } else if (this.selectedAction == HandleShiftAction.AbsenceAnnouncement) {
            if (!this.selectedDeviationCauseAbsenceAnnouncement || this.selectedDeviationCauseAbsenceAnnouncement.id === 0) {
                isValid = false;
                validationErrors += this.terms["time.schedule.planning.handleshift.missingdeviationcause"].format(this.terms["time.schedule.planning.handleshift.action.absenceannouncement"]) + "\n";
            }
        } else if (this.selectedAction == HandleShiftAction.ChangeEmployee) {
            //selectedEmployeeId = EmployeeSelector.SelectedItem != null ? ((KeyValuePair<int, string>)EmployeeSelector.SelectedItem).Key: employeeId;
            //if (selectedEmployeeId == employeeId) {
            //    GraphicsUtility.ShowDialog(new SOEMessageBox(termUtil.GetTerm(178, "Observera"), String.Format(termUtil.GetTerm(180, "Du har valt '{0}' men inte valt någon annan under '{1}'.\nVälj en annan person och försök igen."), ButtonChangeEmployee.Content, EmployeeSelector.Label), SOEMessageBoxImage.Warning));
            //    return;
            //}
        } else if (this.selectedAction == HandleShiftAction.SwapEmployee) {
            //selectedEmployeeId = EmployeeSelector.SelectedItem != null ? ((KeyValuePair<int, string>)EmployeeSelector.SelectedItem).Key: employeeId;
            //selectedShiftId = ShiftSelector.GetRequiredComboBoxValue();
            //if (selectedEmployeeId == employeeId) {
            //    GraphicsUtility.ShowDialog(new SOEMessageBox(termUtil.GetTerm(178, "Observera"), String.Format(termUtil.GetTerm(180, "Du har valt '{0}' men inte valt någon annan under '{1}'.\nVälj en annan person och försök igen."), ButtonSwapEmployee.Content, EmployeeSelector.Label), SOEMessageBoxImage.Warning));
            //    return;
            //}
            //if (selectedShiftId == 0) {
            //    GraphicsUtility.ShowDialog(new SOEMessageBox(termUtil.GetTerm(178, "Observera"), String.Format(termUtil.GetTerm(306, "Du har valt '{0}' men inte valt något {1} under '{2}'.\nVälj ett {3} och försök igen."), ButtonSwapEmployee.Content, shiftUndefined, ShiftSelector.Label, shiftUndefined), SOEMessageBoxImage.Warning));
            //    return;
            //}
        }

        if (!isValid)
            this.notificationService.showDialog(this.terms["core.unabletosave"], validationErrors, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK, SOEMessageBoxSize.Medium);

        return isValid;
    }
}