import { EditController } from "./EditController";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IScheduleService } from "../ScheduleService";
import { IScheduleService as ISharedScheduleService } from "../../../Shared/Time/Schedule/ScheduleService";
import { ShiftDTO } from "../../../Common/Models/TimeSchedulePlanningDTOs";
import { TimeScheduleTemplateHeadSmallDTO } from "../../../Common/Models/timescheduletemplatedtos";
import { SOEMessageBoxImage } from "../../../Util/Enumerations";
import { EmployeeListDTO } from "../../../Common/Models/EmployeeListDTO";
import { SoeScheduleWorkRules, TermGroup_ShiftHistoryType } from "../../../Util/CommonEnumerations";
import { DateRangeDTO } from "../../../Common/Models/DateRangeDTO";

export class TemplateHelper {
    private tempBlockIdCounter: number = 0;
    private templatesToSave: any[] = [];

    constructor(private controller: EditController,
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private scheduleService: IScheduleService,
        private sharedScheduleService: ISharedScheduleService) {
    }

    public setDayNumberFromTemplate(shift: ShiftDTO, template: TimeScheduleTemplateHeadSmallDTO) {
        if (!template)
            return;

        // Set DayNumber
        if (!template.firstMondayOfCycle)
            template.firstMondayOfCycle = template.startDate;

        if (template.noOfDays === 0)
            template.noOfDays = 7;

        let currentDate: Date = template.firstMondayOfCycle;
        if (!template.stopDate || template.stopDate.isSameOrAfterOnDay(shift.startTime)) {
            while (currentDate.addDays(template.noOfDays).isSameOrBeforeOnDay(shift.actualStartDate)) {
                currentDate = currentDate.addDays(template.noOfDays);
            }
        }
        shift.setDayNumber(currentDate);

        // Set number of weeks
        shift.nbrOfWeeks = Math.floor(template.noOfDays / 7);
        if (shift.nbrOfWeeks < 1)
            shift.nbrOfWeeks = 1;
    }

    public getTemplateSchedules(employeeIdentifier: number): TimeScheduleTemplateHeadSmallDTO[] {
        let employee: EmployeeListDTO = this.controller.isEmployeePostView ? this.controller.getEmployeePostById(employeeIdentifier) : this.controller.getEmployeeById(employeeIdentifier);
        if (employee?.hasTemplateSchedules)
            return _.orderBy(employee.templateSchedules, 'startDate', 'desc');

        return [];
    }

    public getTemplateSchedule(employeeIdentifier: number, date: Date, notifyIfNotFound: boolean = true): TimeScheduleTemplateHeadSmallDTO {
        let template = this.selectTemplateSchedule(employeeIdentifier, date, date);
        if (!template && notifyIfNotFound) {
            let employee = this.controller.isEmployeePostView ? this.controller.getEmployeePostById(employeeIdentifier) : this.controller.getEmployeeById(employeeIdentifier);
            let keys: string[] = [
                "time.schedule.planning.noschedule.title",
                "time.schedule.planning.noschedule.message"];
            this.translationService.translateMany(keys).then(terms => {
                this.notificationService.showDialogEx(terms["time.schedule.planning.noschedule.title"], terms["time.schedule.planning.noschedule.message"].format(employee.name.trim()), SOEMessageBoxImage.Error);
            });
        }

        return template;
    }

    private selectTemplateSchedule(employeeIdentifier: number, dateLimitFrom: Date, dateLimitTo: Date): TimeScheduleTemplateHeadSmallDTO {
        dateLimitFrom = dateLimitFrom.beginningOfDay();
        dateLimitTo = dateLimitTo.endOfDay();

        // Get all loaded schedules for employee
        let empSchedules = _.orderBy(this.getTemplateSchedules(employeeIdentifier), t => t.startDate, 'desc');

        // Try to get a template that starts within current interval
        let template: TimeScheduleTemplateHeadSmallDTO;
        let templates = empSchedules.filter(t => t.startDate.isSameOrAfterOnDay(dateLimitFrom) && t.startDate.isSameOrBeforeOnDay(dateLimitTo));
        if (templates.length === 1) {
            template = templates[0];
        } else if (templates.length === 0) {
            // No template found
            // Try to get the closest template that starts before current interval
            template = empSchedules.find(t => t.startDate.isBeforeOnDay(dateLimitFrom));
        } else if (templates.length > 1) {
            // TODO: More than one template found in specified interval, show select dialog?
            // For now, we just select the first
            template = templates[0];
        }

        return template;
    }

    // SAVE

    public updateTemplateSchedule(employeeIdentifier: number, timeScheduleTemplateHeadId: number, activateDayNumber: number, activateDates: Date[]): ng.IPromise<boolean> {
        return this.initSaveForOneTemplate(employeeIdentifier, timeScheduleTemplateHeadId, activateDayNumber, activateDates);
    }

    private initSaveForOneTemplate(employeeIdentifier: number, timeScheduleTemplateHeadId: number, activateDayNumber: number, activateDates: Date[]): ng.IPromise<boolean> {
        let deferral = this.$q.defer<boolean>();

        if (!employeeIdentifier || !timeScheduleTemplateHeadId)
            deferral.resolve(false);

        let template = this.getTemplateSchedules(employeeIdentifier).find(t => t.timeScheduleTemplateHeadId === timeScheduleTemplateHeadId);
        if (template) {
            if (this.validateSaveForOneTemplate(employeeIdentifier, template)) {
                let shifts: ShiftDTO[] = this.getTemplateShiftsToSave(employeeIdentifier, template);
                this.validateWorkRulesForOneTemplate(shifts).then(valid => {
                    if (valid) {
                        this.saveTemplateSchedule(employeeIdentifier, template, shifts, activateDayNumber, activateDates).then(success => {
                            deferral.resolve(success);
                        });
                    } else {
                        deferral.resolve(false);
                    }
                });
            } else {
                deferral.resolve(false);
            }
        } else {
            deferral.resolve(false);
        }

        return deferral.promise;
    }

    private saveTemplateSchedule(employeeId: number, template: TimeScheduleTemplateHeadSmallDTO, shifts: ShiftDTO[], activateDayNumber: number, activateDates: Date[]): ng.IPromise<boolean> {
        let deferral = this.$q.defer<boolean>();

        shifts.forEach(shift => {
            shift.setTimesForSave();
        });

        // Set date range to current visible range (in day numbers)
        let dayNumberFrom: number = this.getFirstDayNumberInVisibleRange(template);
        let visibleDays = (this.controller.nbrOfVisibleDays > 0 ? this.controller.nbrOfVisibleDays : 1);
        if (template.startDate.isAfterOnDay(this.controller.dateFrom))
            visibleDays -= template.startDate.diffDays(this.controller.dateFrom);
        let dayNumberTo: number = dayNumberFrom + visibleDays - 1;

        if (template.noOfDays === 0)
            template.noOfDays = 7;

        if (dayNumberTo > template.noOfDays) {
            this.doSaveTemplateSchedule(employeeId, template.timeScheduleTemplateHeadId, 1, dayNumberTo - template.noOfDays, shifts.filter(s => s.dayNumber >= 1 && s.dayNumber <= dayNumberTo - template.noOfDays), activateDayNumber, activateDates).then(success => {
                if (success) {
                    if (dayNumberFrom <= dayNumberTo - template.noOfDays)
                        dayNumberFrom = dayNumberTo - template.noOfDays + 1;
                    if (dayNumberFrom <= template.noOfDays) {
                        this.doSaveTemplateSchedule(employeeId, template.timeScheduleTemplateHeadId, dayNumberFrom, template.noOfDays, shifts.filter(s => s.dayNumber >= dayNumberFrom && s.dayNumber <= template.noOfDays), activateDayNumber, activateDates).then(success2 => {
                            deferral.resolve(success2);
                        });
                    } else {
                        deferral.resolve(true);
                    }
                } else {
                    deferral.resolve(false);
                }
            });
        } else {
            this.doSaveTemplateSchedule(employeeId, template.timeScheduleTemplateHeadId, dayNumberFrom, dayNumberTo, shifts, activateDayNumber, activateDates).then(success => {
                deferral.resolve(success);
            });
        }

        return deferral.promise;
    }

    private doSaveTemplateSchedule(employeeId: number, timeScheduleTemplateHeadId: number, dayNumberFrom: number, dayNumberTo: number, shifts: ShiftDTO[], activateDayNumber: number, activateDates: Date[]): ng.IPromise<boolean> {
        let deferral = this.$q.defer<boolean>();

        this.scheduleService.saveTimeScheduleTemplateHead(employeeId, shifts, timeScheduleTemplateHeadId, dayNumberFrom, dayNumberTo, this.controller.dateFrom, activateDayNumber, activateDates, this.controller.selectableInformationSettings.skipXEMailOnChanges).then(result => {
            if (result.success) {
                deferral.resolve(true);
            } else {
                this.translationService.translate("time.schedule.planning.templateschedule.save.error").then(term => {
                    this.notificationService.showDialogEx(term, result.errorMessage, SOEMessageBoxImage.Error);
                });
                deferral.resolve(false);
            }
        });

        return deferral.promise;
    }

    // VALIDATION

    private validateSaveForMultipleTemplates(): boolean {
        let allValid = true;

        this.templatesToSave.forEach(templateToSave => {
            let valid = false;

            let employeeIdentifier: number = this.getEmployeeIdentifierFromTemplate(templateToSave);
            if (employeeIdentifier)
                valid = this.validateSaveForOneTemplate(employeeIdentifier, templateToSave.templateHead);

            if (!valid)
                allValid = false;
        });

        return allValid;
    }

    private validateSaveForOneTemplate(employeeIdentifier: number, templateHead: TimeScheduleTemplateHeadSmallDTO): boolean {
        let valid = true;

        if (!templateHead || templateHead.locked) {
            let employee = this.controller.isEmployeePostView ? this.controller.getEmployeePostById(employeeIdentifier) : this.controller.getEmployeeById(employeeIdentifier);

            let keys: string[] = [
                "time.schedule.planning.templateschedule.save.error",
            ];

            if (!templateHead)
                keys.push("time.schedule.planning.templateschedule.save.error.notemplate");
            else
                keys.push("time.schedule.planning.templateschedule.save.error.locked");

            this.translationService.translateMany(keys).then(terms => {
                let message: string;
                if (!templateHead)
                    message = terms["time.schedule.planning.templateschedule.save.error.notemplate"];
                else
                    message = terms["time.schedule.planning.templateschedule.save.error.locked"];

                this.notificationService.showDialogEx(terms["time.schedule.planning.templateschedule.save.error"], message.format(employee.name), SOEMessageBoxImage.Warning);
            });
            valid = false;
        }
        return valid;
    }

    private validateWorkRulesForMultipleTemplates(): ng.IPromise<boolean> {
        let deferral = this.$q.defer<boolean>();

        let allValid = true;

        let counter: number = this.templatesToSave.length;
        this.templatesToSave.forEach(templateToSave => {
            counter--;
            // Do not evaluate work rules for hidden employee
            if (templateToSave.employeeId === this.controller.hiddenEmployeeId) {
                if (counter === 0)
                    deferral.resolve(allValid);
            } else {
                this.validateWorkRulesForOneTemplate(templateToSave.shifts).then(valid => {
                    if (!valid)
                        allValid = false;

                    if (counter === 0)
                        deferral.resolve(allValid);
                });
            }
        });

        return deferral.promise;
    }

    private validateWorkRulesForOneTemplate(shifts: ShiftDTO[]): ng.IPromise<boolean> {
        let deferral = this.$q.defer<boolean>();

        let rules: SoeScheduleWorkRules[] = null;
        if (this.controller.selectableInformationSettings.skipWorkRules) {
            // The following rules should always be evaluated
            rules = [];
            rules.push(SoeScheduleWorkRules.OverlappingShifts);
        }

        if (!shifts || shifts.length === 0)
            deferral.resolve(true);
        else {
            this.$timeout(() => {
                shifts.forEach(shift => {
                    shift.setTimesForSave();                    
                });

                let identifier: number = this.getEmployeeIdentifierFromShift(shifts[0]);
                this.sharedScheduleService.evaluatePlannedShiftsAgainstWorkRules(shifts, rules, identifier, true, null).then(result => {                    
                    this.notificationService.showValidateWorkRulesResult(TermGroup_ShiftHistoryType.TemplateScheduleSave, result, shifts[0].employeeId).then(passed => {
                        deferral.resolve(passed);
                    });
                });
            });
        }

        return deferral.promise;
    }

    // HELP-METHODS

    public getNextTempBlockId(): number {
        return ++this.tempBlockIdCounter;
    }

    private getTemplateShiftsToSave(employeeIdentifier: number, template: TimeScheduleTemplateHeadSmallDTO): ShiftDTO[] {
        // Get shifts in view
        let shifts: ShiftDTO[] = this.controller.shifts.filter(s => (s.timeScheduleTemplateHeadId === template.timeScheduleTemplateHeadId || !s.timeScheduleTemplateHeadId) && !s['isRecurringWeek'] && !s.actualStartTime.isSameMinuteAs(s.actualStopTime));
        if (this.controller.isTemplateView)
            shifts = shifts.filter(s => s.employeeId === employeeIdentifier);
        else if (this.controller.isEmployeePostView)
            shifts = shifts.filter(s => s.employeePostId === employeeIdentifier);

        shifts.forEach(shift => {
            // Fix times
            shift.setTimesForSave();

            // Day number
            this.setDayNumberFromTemplate(shift, template);

            // Default TimeCode
            if (!shift.timeCodeId)
                shift.timeCodeId = this.controller.defaultTimeCodeId;

            if (!shift.timeScheduleTemplateHeadId && template)
                shift.timeScheduleTemplateHeadId = template.timeScheduleTemplateHeadId;
        });

        return shifts;
    }

    public getTemplateVisibleRange(template: TimeScheduleTemplateHeadSmallDTO): DateRangeDTO {
        let templateStart: Date = template.firstMondayOfCycle ? template.firstMondayOfCycle : template.startDate;
        if (templateStart.isBeforeOnDay(template.startDate))    // Happens if template does not start on a monday, and we look at first week in template
            templateStart = template.startDate;
        let templateDays = template.noOfDays > 0 ? template.noOfDays : 7;

        while (templateStart.addDays(templateDays).isSameOrBeforeOnDay(this.controller.dateFrom)) {
            templateStart = templateStart.addDays(templateDays);
        }

        let templateStartDaysBeforeVisibleStart: number = 0;
        if (templateStart.isBeforeOnDay(this.controller.dateFrom))
            templateStartDaysBeforeVisibleStart = this.controller.dateFrom.diffDays(templateStart);

        let templateStop: Date = templateStart.addDays(templateDays - 1).addDays(templateStartDaysBeforeVisibleStart);

        if (templateStart.isBeforeOnDay(this.controller.dateFrom))
            templateStart = this.controller.dateFrom;
        if (templateStop.isAfterOnDay(this.controller.dateTo))
            templateStop = this.controller.dateTo;

        return new DateRangeDTO(templateStart, templateStop);

    }

    private getFirstDayNumberInVisibleRange(template: TimeScheduleTemplateHeadSmallDTO): number {
        let dayNumber: number = 1;
        let date: Date = this.controller.dateFrom;

        if (template && template.startDate) {
            if (template.noOfDays === 0)
                template.noOfDays = 7;

            let currentDate: Date = template.firstMondayOfCycle ? template.firstMondayOfCycle : template.startDate;
            if (!template.stopDate || template.stopDate.isAfterOnDay(date)) {
                while (currentDate.addDays(template.noOfDays).isSameOrBeforeOnDay(date)) {
                    currentDate = currentDate.addDays(template.noOfDays);
                }
            }
            if (template.startDate.isAfterOnDay(date))
                dayNumber = template.startDate.diffDays(date) + 1;
            else
                dayNumber = date.diffDays(currentDate) + 1;
        }

        return dayNumber;
    }

    private getEmployeeIdentifierFromTemplate(template: any): number {
        let employeeIdentifier: number;
        if (this.controller.isTemplateView)
            employeeIdentifier = template.employeeId;
        else if (this.controller.isEmployeePostView)
            employeeIdentifier = template.employeePostId;

        return employeeIdentifier;
    }

    private getEmployeeIdentifierFromShift(shift: ShiftDTO): number {
        let employeeIdentifier: number;
        if (this.controller.isTemplateView)
            employeeIdentifier = shift.employeeId;
        else if (this.controller.isEmployeePostView)
            employeeIdentifier = shift.employeePostId;

        return employeeIdentifier;
    }
}
