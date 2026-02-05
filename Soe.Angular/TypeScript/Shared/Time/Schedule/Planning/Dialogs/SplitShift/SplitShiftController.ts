import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../../../Util/Enumerations";
import { CalendarUtility } from "../../../../../../Util/CalendarUtility";
import { EmployeeListDTO } from "../../../../../../Common/Models/EmployeeListDTO";
import { ITranslationService } from "../../../../../../Core/Services/TranslationService";
import { INotificationService } from "../../../../../../Core/Services/NotificationService";
import { ICoreService } from "../../../../../../Core/Services/CoreService";
import { IScheduleService as ISharedScheduleService } from "../../../ScheduleService";
import { ShiftDTO } from "../../../../../../Common/Models/TimeSchedulePlanningDTOs";
import { TermGroup_ShiftHistoryType } from "../../../../../../Util/CommonEnumerations";
import { TimeScheduleTemplateHeadSmallDTO } from "../../../../../../Common/Models/TimeScheduleTemplateDTOs";
import { TemplateHelper } from "../../../../../../Time/Schedule/Planning/TemplateHelper";
import { DateRangeDTO } from "../../../../../../Common/Models/DateRangeDTO";

export class SplitShiftController {

    // Terms
    private terms: any = [];
    private title: string;
    private instruction: string;
    private shiftDefined: string;
    private shiftUndefined: string;
    private shiftsDefined: string;
    private shiftsUndefined: string;

    // Data
    private employees: EmployeeListDTO[];
    private placement1: DateRangeDTO;
    private placement2: DateRangeDTO;

    // Properties
    private get shiftIsOrder(): boolean {
        return this.shift.isOrder;
    }

    private get shiftIsBooking(): boolean {
        return this.shift.isBooking;
    }

    private formattedTime: string;
    private sameDate: boolean = true;
    private splitDate: Date;
    private splitTime: Date;

    private _selectedEmployee1: EmployeeListDTO;
    private set selectedEmployee1(item: EmployeeListDTO) {
        this._selectedEmployee1 = item;

        if (this.isTemplate)
            this.placement1 = this.selectedEmployee1 ? this.selectedEmployee1.getEmployeeSchedule(this.shift.date) : null;
    }
    private get selectedEmployee1(): EmployeeListDTO {
        return this._selectedEmployee1;
    }

    private _selectedEmployee2: EmployeeListDTO;
    private set selectedEmployee2(item: EmployeeListDTO) {
        this._selectedEmployee2 = item;

        if (this.isTemplate)
            this.placement2 = this.selectedEmployee2 ? this.selectedEmployee2.getEmployeeSchedule(this.shift.date) : null;
    }
    private get selectedEmployee2(): EmployeeListDTO {
        return this._selectedEmployee2;
    }

    // Flags
    private executing: boolean = false;

    // Filters
    private showEmployeeFilters: boolean = false;
    private filterEmployeesOnSkill: boolean = false;
    private filterEmployeesOnAvailability: boolean = true;

    // Skills
    private shiftTypeIds: number[] = [];
    private _invalidSkills1: boolean = false;
    get invalidSkills1(): boolean {
        return this._invalidSkills1;
    }
    set invalidSkills1(value: boolean) {
        this._invalidSkills1 = value;
        if (value) {
            this.skillsOpen1 = true;
        }
    }
    private _invalidSkills2: boolean = false;
    get invalidSkills2(): boolean {
        return this._invalidSkills2;
    }
    set invalidSkills2(value: boolean) {
        this._invalidSkills2 = value;
        if (value) {
            this.skillsOpen2 = true;
        }
    }
    private skillsOpen1: boolean = false;
    private skillsOpen2: boolean = false;
    private skillsOpened1() {
        this.skillsOpen1 = true;
    }
    private skillsOpened2() {
        this.skillsOpen2 = true;
    }
    private ignoreSkillEmployeeIds: number[] = [];
    private validateSkills: boolean = true;

    //@ngInject
    constructor(
        private $uibModalInstance,
        $uibModal,
        private $timeout: ng.ITimeoutService,
        private $q: ng.IQService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private coreService: ICoreService,
        private sharedScheduleService: ISharedScheduleService,
        private currentEmployeeId: number,
        private templateHelper: TemplateHelper,
        private isTemplate: boolean,
        private isEmployeePost: boolean,
        private showSkills: boolean,
        private showExtraShift: boolean,
        private showSubstitute: boolean,
        private clockRounding: number,
        private keepShiftsTogether: boolean,
        private skillCantBeOverridden: boolean,
        private hiddenEmployeeId: number,
        private vacantEmployeeIds: number[],
        private allEmployees: EmployeeListDTO[],
        private shift: ShiftDTO,
        private timeScheduleScenarioHeadId?: number,
        private planningPeriodStartDate?: Date,
        private planningPeriodStopDate?: Date) {
        this.setup();
    }

    // SETUP

    private setup() {
        // Setup employees to ignore skill matching on
        this.ignoreSkillEmployeeIds.push(this.hiddenEmployeeId);
        _.forEach(this.vacantEmployeeIds, vacantEmployeeId => {
            this.ignoreSkillEmployeeIds.push(vacantEmployeeId);
        });

        this.$q.all([
            this.loadTerms(),
        ]).then(() => {
            this.populate();
        });
    }

    private populate() {
        this.sameDate = this.shift.actualStartTime.isSameDayAs(this.shift.actualStopTime);

        this.setFormattedTime();
        this.setSplitTime();
        this.filterEmployees();

        // Set current employee as default on both sides
        this.selectedEmployee1 = this.selectedEmployee2 = _.find(this.employees, e => (this.isEmployeePost ? e.employeePostId : e.employeeId) === this.getShiftEmployeeIdentifier());

        this.shiftTypeIds = [this.shift.shiftTypeId];
    }

    // LOOKUPS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "common.employees",
            "time.schedule.employeepost.employeeposts",
            "time.schedule.planning.splitshift.title",
            "time.schedule.planning.splitshift.instruction",
            "time.schedule.planning.shiftdefined",
            "time.schedule.planning.shiftundefined",
            "time.schedule.planning.shiftsdefined",
            "time.schedule.planning.shiftsundefined",
            "time.schedule.planning.bookingdefined",
            "time.schedule.planning.bookingundefined",
            "time.schedule.planning.bookingsdefined",
            "time.schedule.planning.bookingsundefined",
            "time.schedule.planning.assignmentdefined",
            "time.schedule.planning.assignmentundefined",
            "time.schedule.planning.assignmentsdefined",
            "time.schedule.planning.assignmentsundefined",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;

            if (this.shiftIsOrder) {
                this.shiftDefined = this.terms["time.schedule.planning.assignmentdefined"];
                this.shiftUndefined = this.terms["time.schedule.planning.assignmentundefined"];
                this.shiftsDefined = this.terms["time.schedule.planning.assignmentsdefined"];
                this.shiftsUndefined = this.terms["time.schedule.planning.assignmentsundefined"];
            } else if (this.shiftIsBooking) {
                this.shiftDefined = this.terms["time.schedule.planning.bookingdefined"];
                this.shiftUndefined = this.terms["time.schedule.planning.bookingundefined"];
                this.shiftsDefined = this.terms["time.schedule.planning.bookingsdefined"];
                this.shiftsUndefined = this.terms["time.schedule.planning.bookingsundefined"];
            } else {
                this.shiftDefined = this.terms["time.schedule.planning.shiftdefined"];
                this.shiftUndefined = this.terms["time.schedule.planning.shiftundefined"];
                this.shiftsDefined = this.terms["time.schedule.planning.shiftsdefined"];
                this.shiftsUndefined = this.terms["time.schedule.planning.shiftsundefined"];
            }

            this.title = "{0} {1}".format(this.terms["time.schedule.planning.splitshift.title"], this.shiftUndefined);
            this.instruction = this.terms["time.schedule.planning.splitshift.instruction"].format(this.shiftDefined, (this.isEmployeePost ? this.terms["time.schedule.employeepost.employeeposts"].toString().toLocaleLowerCase() : this.terms["common.employees"].toString().toLocaleLowerCase()));
        });
    }

    // ACTIONS       

    private matchEmployeesByShiftTypeSkills() {
        if (!this.shift.shiftTypeId)
            return;

        this.sharedScheduleService.matchEmployeesByShiftTypeSkills(this.shift.shiftTypeId).then(employeeIds => {
            this.filterEmployeesByIds(employeeIds);

            // If availability filter is checked, do that match also
            if (this.filterEmployeesOnAvailability)
                this.matchEmployeesByAvailability();
        });
    }

    private matchEmployeesByAvailability() {
        this.sharedScheduleService.getAvailableEmployeeIds(this.shift.actualStartTime, this.shift.actualStopTime, this.isTemplate, false).then(employeeIds => {
            // Returns all available employees, so keep them in the list and remove the other
            employeeIds = _.intersection(this.employees.map(e => (this.isEmployeePost ? e.employeePostId : e.employeeId)), employeeIds);
            this.filterEmployeesByIds(employeeIds);
        });
    }

    // EVENTS

    private toggleEmployeeFilters() {
        this.showEmployeeFilters = !this.showEmployeeFilters;
    }

    private onFilterEmployees() {
        this.$timeout(() => {
            this.filterEmployees();
        });
    }

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }

    private ok() {
        this.executing = true;

        this.validate().then(result => {
            if (result) {
                this.evaluateWorkRules().then(rulesResult => {
                    if (rulesResult) {
                        this.$uibModalInstance.close({
                            splitTime: this.splitTime,
                            employeeId1: this.isEmployeePost ? this.selectedEmployee1.employeePostId : this.selectedEmployee1.employeeId,
                            employeeId2: this.isEmployeePost ? this.selectedEmployee2.employeePostId : this.selectedEmployee2.employeeId
                        });
                    } else {
                        this.shift.fixDates();
                        this.executing = false;
                    }
                });
            } else {
                this.executing = false;
            }
        });
    }

    // HELP-METHODS

    private getShiftEmployeeIdentifier(): number {
        return this.isEmployeePost ? this.shift.employeePostId : this.shift.employeeId;
    }

    private setFormattedTime() {
        this.formattedTime = this.shift.actualStartTime.toFormattedDate();
        if (!this.sameDate)
            this.formattedTime += " - " + this.shift.actualStopTime.toFormattedDate();
        this.formattedTime += "   {0}-{1}".format(this.shift.actualStartTime.toFormattedTime(), this.shift.actualStopTime.toFormattedTime());
    }

    private setSplitTime() {
        // Set split time to middle of shift
        this.splitTime = this.shift.actualStartTime.addMinutes(this.shift.getShiftLength() / 2);
        this.splitDate = this.splitTime.date();
    }

    private filterEmployees() {
        if (!this.filterEmployeesOnSkill || !this.shift.shiftTypeId) {
            // Skill filter is not selected, copy all employees to list
            this.copyAllEmployees(!this.filterEmployeesOnAvailability);
            // If availability filter is checked, do that match
            if (this.filterEmployeesOnAvailability)
                this.matchEmployeesByAvailability();
        } else {
            // Skill filter is checked, do that match
            // After match is completed a check on availability filter is performed
            this.matchEmployeesByShiftTypeSkills();
        }
    }

    private filterEmployeesByIds(employeeIds: number[]) {
        // If current employee already assigned to the shift is not matched, add it anyway
        // Otherwise it will disappear from the list
        if (!_.includes(employeeIds, this.getShiftEmployeeIdentifier()))
            employeeIds.push(this.getShiftEmployeeIdentifier());

        // Same for the hidden employee
        if (!_.includes(employeeIds, this.hiddenEmployeeId))
            employeeIds.push(this.hiddenEmployeeId);

        this.employees = [];
        _.forEach(employeeIds, employeeId => {
            var employee = _.find(this.allEmployees, e => (this.isEmployeePost ? e.employeePostId : e.employeeId) === employeeId);
            if (employee)
                this.copyEmployee(employee);
        });

        if (this.isEmployeePost) {
            if (this.selectedEmployee1 && !_.includes(this.employees.map(e => e.employeePostId), this.selectedEmployee1.employeePostId))
                this.selectedEmployee1 = undefined;
            if (this.selectedEmployee2 && !_.includes(this.employees.map(e => e.employeePostId), this.selectedEmployee2.employeePostId))
                this.selectedEmployee2 = undefined;
        } else {
            if (this.selectedEmployee1 && !_.includes(this.employees.map(e => e.employeeId), this.selectedEmployee1.employeeId))
                this.selectedEmployee1 = undefined;
            if (this.selectedEmployee2 && !_.includes(this.employees.map(e => e.employeeId), this.selectedEmployee2.employeeId))
                this.selectedEmployee2 = undefined;
        }

        this.sortEmployees();
    }

    private copyAllEmployees(sort: boolean) {
        this.employees = [];
        _.forEach(_.orderBy(this.allEmployees, 'name'), (employee) => {
            this.copyEmployee(employee);
        });

        if (sort)
            this.sortEmployees();
    }

    private copyEmployee(employee: EmployeeListDTO) {
        if (employee.isGroupHeader)
            return;

        if (this.isEmployeePost && !employee.employeePostId)
            return;

        if (!this.isEmployeePost && !employee.employeeId)
            return;

        this.employees.push(employee);
    }

    private sortEmployees() {
        this.employees = _.orderBy(this.employees, ['hidden', 'vacant', 'name'], ['desc', 'desc', 'asc']);
    }

    // VALIDATION

    private validate(): ng.IPromise<boolean> {
        var deferral = this.$q.defer<boolean>();

        var invalidSplitTime: boolean = false;
        var invalidEmployees: boolean = false;
        var invalidSkills: boolean = false;

        // Check that date and time is selected and that it's between shift start and end
        if (CalendarUtility.isValidDate(this.splitDate) && CalendarUtility.isValidDate(this.splitTime)) {
            this.splitTime = this.splitDate.mergeTime(this.splitTime);

            if (this.splitTime.isSameOrBeforeOnMinute(this.shift.actualStartTime) || this.splitTime.isSameOrAfterOnMinute(this.shift.actualStopTime))
                invalidSplitTime = true;
        } else {
            invalidSplitTime = true;
        }

        // Check that both employees are selected (it's OK to select same on both)
        if (!this.selectedEmployee1 || !this.selectedEmployee2)
            invalidEmployees = true;

        if (this.validateSkills && (this.invalidSkills1 || this.invalidSkills2))
            invalidSkills = true;

        if (invalidSplitTime || invalidEmployees || invalidSkills) {
            var keys: string[] = [
                "time.schedule.planning.splitshift.invalid"
            ];

            if (invalidSplitTime)
                keys.push("time.schedule.planning.splitshift.invalidsplittime");
            if (invalidEmployees)
                keys.push("time.schedule.planning.splitshift.invalidemployees");
            if (invalidSkills) {
                keys.push("time.schedule.planning.splitshift.invalidskills");
                keys.push("time.schedule.planning.splitshift.invalidskills.override");
            }

            this.translationService.translateMany(keys).then(terms => {
                var msg: string = '';
                if (invalidSplitTime)
                    msg += terms["time.schedule.planning.splitshift.invalidsplittime"] + "\n";
                if (invalidEmployees)
                    msg += terms["time.schedule.planning.splitshift.invalidemployees"] + "\n";
                if (invalidSkills)
                    msg += terms["time.schedule.planning.splitshift.invalidskills"].format(this.shiftDefined) + "\n";

                // If only skills are invalid and skills can be overridden show ask save dialog, otherwise just show errors
                if (!invalidSplitTime && !invalidEmployees && invalidSkills && !this.skillCantBeOverridden) {
                    msg += terms["time.schedule.planning.splitshift.invalidskills.override"];
                    var modal = this.notificationService.showDialogEx(terms["time.schedule.planning.splitshift.invalid"], msg, SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.YesNo);
                    modal.result.then(val => {
                        if (val) {
                            this.validateSkills = false;
                            this.ok();
                        }
                    });
                } else {
                    this.notificationService.showDialogEx(terms["time.schedule.planning.splitshift.invalid"], msg, SOEMessageBoxImage.Error);
                }
            });
        }

        deferral.resolve(!invalidSplitTime && !invalidEmployees && !invalidSkills);

        return deferral.promise;
    }

    private evaluateWorkRules(): ng.IPromise<boolean> {
        var deferral = this.$q.defer<boolean>();

        this.shift.setTimesForSave();

        if (this.isTemplate || this.isEmployeePost) {
            let employeeId1 = this.isEmployeePost ? 0 : this.selectedEmployee1.employeeId;
            let employeePostId1 = this.isEmployeePost ? this.selectedEmployee1.employeePostId : 0;
            let employeeId2 = this.isEmployeePost ? 0 : this.selectedEmployee2.employeeId;
            let employeePostId2 = this.isEmployeePost ? this.selectedEmployee2.employeePostId : 0;

            let sourceTemplate: TimeScheduleTemplateHeadSmallDTO;
            let template1: TimeScheduleTemplateHeadSmallDTO;
            let template2: TimeScheduleTemplateHeadSmallDTO;
            if (this.templateHelper) {
                sourceTemplate = this.templateHelper.getTemplateSchedule(this.getShiftEmployeeIdentifier(), this.shift.startTime);
                template1 = this.templateHelper.getTemplateSchedule(this.isEmployeePost ? employeePostId1 : employeeId1, this.shift.startTime);
                template2 = this.templateHelper.getTemplateSchedule(this.isEmployeePost ? employeePostId2 : employeeId2, this.shift.startTime);
            }
            let sourceTemplateHeadId: number = sourceTemplate ? sourceTemplate.timeScheduleTemplateHeadId : 0;
            let template1HeadId: number = template1 ? template1.timeScheduleTemplateHeadId : 0;
            let template2HeadId: number = template2 ? template2.timeScheduleTemplateHeadId : 0;

            this.sharedScheduleService.evaluateSplitTemplateShiftAgainstWorkRules(this.shift, sourceTemplateHeadId, this.splitTime, employeeId1, employeePostId1, template1HeadId, employeeId2, employeePostId2, template2HeadId, this.keepShiftsTogether).then(result => {
                this.notificationService.showValidateWorkRulesResult(TermGroup_ShiftHistoryType.TaskSplitTimeScheduleShift, result, employeeId1).then(passed => {
                    deferral.resolve(passed);
                });
            });
        } else {
            this.sharedScheduleService.evaluateSplitShiftAgainstWorkRules(this.shift, this.splitTime, this.selectedEmployee1.employeeId, this.selectedEmployee2.employeeId, this.keepShiftsTogether, this.isTemplate, this.timeScheduleScenarioHeadId, this.planningPeriodStartDate, this.planningPeriodStopDate).then(result => {
                this.notificationService.showValidateWorkRulesResult(TermGroup_ShiftHistoryType.TaskSplitTimeScheduleShift, result, this.selectedEmployee1.employeeId).then(passed => {
                    deferral.resolve(passed);
                });
            });
        }

        return deferral.promise;
    }
}
