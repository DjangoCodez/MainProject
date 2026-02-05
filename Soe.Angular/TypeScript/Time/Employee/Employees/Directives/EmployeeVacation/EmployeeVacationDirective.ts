import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { EmployeeVacationSEDTO } from "../../../../../Common/Models/EmployeeVacationDTOs";
import { EmploymentVacationGroupDTO, EmploymentDTO } from "../../../../../Common/Models/EmployeeUserDTO";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../../../Util/Enumerations";
import { TranslationService } from "../../../../../Core/Services/TranslationService";
import { TermGroup_VacationGroupVacationHandleRule, TermGroup_VacationGroupType, TermGroup_VacationGroupRemainingDaysRule } from "../../../../../Util/CommonEnumerations";
import { VacationGroupDTO } from "../../../../../Common/Models/VacationGroupDTO";
import { IEmployeeService as ISharedEmployeeService } from "../../../../../Shared/Time/Employee/EmployeeService";
import { IEmployeeService } from "../../../EmployeeService";
import { IPayrollService } from "../../../../Payroll/PayrollService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";

export class EmployeeVacationDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Employee/Employees/Directives/EmployeeVacation/Views/EmployeeVacation.html'),
            scope: {
                employeeVacation: '=',
                employment: '=',
                selectedEmploymentDate: '=',
                modifyPermission: '=',
                isValid: '=',
                validationErrors: '='
            },
            restrict: 'E',
            replace: true,
            controller: EmployeeVacationController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class EmployeeVacationController {

    // Terms
    private terms: { [index: string]: string; };
    private currentVacationYear: string;
    private remainingHoursLabel: string;
    private amountLabel: string;
    private dueDateLabel: string;
    private savedDaysYear1Label: string;
    private savedDaysYear2Label: string;
    private savedDaysYear3Label: string;
    private savedDaysYear4Label: string;
    private savedDaysYear5Label: string;

    // Data
    private employeeVacation: EmployeeVacationSEDTO;
    private employment: EmploymentDTO;
    private vacationGroupId: number;
    private vacationGroup: VacationGroupDTO;
    private selectedEmploymentDate: Date;

    private prelPayedDaysYear1: number;
    private vacationDaysPaidByLaw: number = 0;

    private usedDaysPrel: number = 0;
    private usedDaysPrelDetails: string;
    private usedDaysPrelSum: number = 0;
    private remainingDaysSum: number = 0;
    private earnedDaysRemainingHoursSum: number = 0;

    // Properties
    private get employeeId(): number {
        if (this.employeeVacation && this.employeeVacation.employeeId)
            return this.employeeVacation.employeeId;

        if (this.employment && this.employment.employeeId)
            return this.employment.employeeId;

        return 0;
    }

    // Flags
    private modifyPermission: boolean;
    private isValid: boolean;
    private validationErrors: string;
    private readOnly: boolean = true;
    private showHours: boolean = false;
    private showYear2To5: boolean = false;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private translationService: TranslationService,
        private notificationService: INotificationService,
        private employeeService: IEmployeeService,
        private sharedEmployeeService: ISharedEmployeeService,
        private payrollService: IPayrollService) {
    }

    public $onInit() {        
        this.calculateRemainingDaysSum();
        this.calculateRemainingDaysPrelSum();
        this.calculateEarnedDaysRemainingHoursSum();
        this.$q.all([this.loadTerms()]).then(() => {
            this.setupWatchers();
        });
    }

    private watchPrelSum: Function;
    private watchSum: Function;
    private watchHoursSum: Function;
    private watchValidate: Function;
    private setupWatchers() {
        this.$scope.$watch(() => this.employeeVacation, (newVal, oldVal) => {
            if (newVal != oldVal) {

                if (this.watchPrelSum)
                    this.watchPrelSum();
                if (this.watchSum)
                    this.watchSum();
                if (this.watchHoursSum)
                    this.watchHoursSum();
                if (this.watchValidate)
                    this.watchValidate();
                if (this.employeeVacation) {
                    this.watchPrelSum = this.$scope.$watchGroup([() => this.employeeVacation.usedDaysPaid, () => this.employeeVacation.usedDaysUnpaid, () => this.employeeVacation.usedDaysAdvance, () => this.employeeVacation.usedDaysYear1, () => this.employeeVacation.usedDaysYear2, () => this.employeeVacation.usedDaysYear3, () => this.employeeVacation.usedDaysYear4, () => this.employeeVacation.usedDaysYear5, () => this.employeeVacation.usedDaysOverdue], (newValue, oldValue, scope) => {
                        this.calculateRemainingDaysPrelSum();
                    });
                    this.watchSum = this.$scope.$watchGroup([() => this.employeeVacation.remainingDaysPaid, () => this.employeeVacation.remainingDaysUnpaid, () => this.employeeVacation.remainingDaysAdvance, () => this.employeeVacation.remainingDaysYear1, () => this.employeeVacation.remainingDaysYear2, () => this.employeeVacation.remainingDaysYear3, () => this.employeeVacation.remainingDaysYear4, () => this.employeeVacation.remainingDaysYear5, () => this.employeeVacation.remainingDaysOverdue, () => this.usedDaysPrel], (newValue, oldValue, scope) => {
                        this.calculateRemainingDaysSum();
                    });
                    this.watchHoursSum = this.$scope.$watchGroup([() => this.employeeVacation.earnedDaysRemainingHoursPaid, () => this.employeeVacation.earnedDaysRemainingHoursUnpaid, () => this.employeeVacation.earnedDaysRemainingHoursAdvance, () => this.employeeVacation.earnedDaysRemainingHoursYear1, () => this.employeeVacation.earnedDaysRemainingHoursYear2, () => this.employeeVacation.earnedDaysRemainingHoursYear3, () => this.employeeVacation.earnedDaysRemainingHoursYear4, () => this.employeeVacation.earnedDaysRemainingHoursYear5, () => this.employeeVacation.earnedDaysRemainingHoursOverdue], (newValue, oldValue, scope) => {
                        this.calculateEarnedDaysRemainingHoursSum();
                    });
                    this.watchValidate = this.$scope.$watchGroup([() => this.vacationDaysPaidByLaw, () => this.employeeVacation.earnedDaysPaid, () => this.employeeVacation.earnedDaysUnpaid, () => this.employeeVacation.earnedDaysAdvance], (newValue, oldValue, scope) => {
                        this.validate();
                    });
                } else {
                    this.calculateRemainingDaysPrelSum();
                    this.calculateRemainingDaysSum();
                    this.calculateEarnedDaysRemainingHoursSum();
                    this.validate();
                }
            }

            this.$q.all([
                this.loadPrelUsedDays(),
                this.loadPrelPayedDaysYear1()
            ]).then(() => { });
        });
        this.$scope.$watch(() => this.employment, (newVal, oldVal) => {
            this.setCurrentVacationYear();
            this.loadVacationGroup();
        });
        this.$scope.$watch(() => this.readOnly, (newVal, oldVal) => {
            if (newVal !== oldVal && newVal === false && this.vacationDaysPaidByLaw === 0)
                this.loadVacationDaysPaidByLaw();
        });

    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "common.amount",
            "time.employee.employee.vacation.currentvacationyear",
            "time.employee.employee.vacation.remaininghours",
            "time.employee.employee.vacation.remainingshifts",
            "time.employee.employee.vacation.debtinadvanceduedate",
            "time.employee.employee.vacation.saved",
            "time.employee.employee.vacation.savedyear",
            "time.employee.employee.vacation.invalidearneddayssum",
            "time.employee.employee.vacation.preliminaryuseddays"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;

            this.amountLabel = this.terms["common.amount"];
            this.dueDateLabel = this.terms["time.employee.employee.vacation.debtinadvanceduedate"];

            this.savedDaysYear2Label = '{0} 2'.format(this.terms["time.employee.employee.vacation.savedyear"]);
            this.savedDaysYear3Label = '{0} 3'.format(this.terms["time.employee.employee.vacation.savedyear"]);
            this.savedDaysYear4Label = '{0} 4'.format(this.terms["time.employee.employee.vacation.savedyear"]);
            this.savedDaysYear5Label = '{0} 5'.format(this.terms["time.employee.employee.vacation.savedyear"]);
        });
    }

    private loadPrelUsedDays(): ng.IPromise<any> {
        var deferral = this.$q.defer();
        var empId = this.employeeId;
        if (!empId)
            deferral.resolve();
        else {
            return this.sharedEmployeeService.getPrelUsedVacationDays(empId, CalendarUtility.getDateToday()).then(result => {
                this.usedDaysPrel = -result.sum;
                this.usedDaysPrelDetails = result.details;
                this.calculateRemainingDaysSum();
                this.calculateRemainingDaysPrelSum();
                this.calculateEarnedDaysRemainingHoursSum();
            });
        }

        return deferral.promise;
    }

    private loadPrelPayedDaysYear1(): ng.IPromise<any> {
        var deferral = this.$q.defer();

        var empId = this.employeeId;
        if (!empId)
            deferral.resolve();
        else {
            return this.employeeService.getPrelPaidDaysYear1(empId).then(days => {
                this.prelPayedDaysYear1 = days;
            });
        }

        return deferral.promise;
    }

    private loadVacationGroup() {
        if (!this.vacationGroupId) {
            this.setFieldVisibility();
            return;
        }

        this.employeeService.getVacationGroup(this.vacationGroupId).then(x => {
            this.vacationGroup = x;
            this.loadEarningYearIsVacationYearVacationDays();
            this.setFieldVisibility();
        });
    }

    private loadEarningYearIsVacationYearVacationDays() {
        if (this.employeeVacation && this.vacationGroup && this.vacationGroup.type === TermGroup_VacationGroupType.EarningYearIsVacationYear && !this.employeeVacation.earnedDaysPaid && this.employeeId) {
            this.payrollService.getEarningYearIsVacationYearVacationDays(this.vacationGroup.vacationGroupId, this.employeeId, this.selectedEmploymentDate, this.employment.dateFrom, this.employment.dateTo).then(days => {
                this.employeeVacation.earnedDaysPaid = days;
            });
        }
    }

    private loadVacationDaysPaidByLaw() {
        if (this.employeeId) {
            this.payrollService.getVacationDaysPaidByLaw(this.employeeId, CalendarUtility.getDateToday()).then(days => {
                this.vacationDaysPaidByLaw = days;
            });
        }
    }

    // EVENTS

    private edit() {
        this.readOnly = false;
    }

    private showPreliminaryDetails() {
        if (!this.hasPreliminaryDetails())
            return;

        this.notificationService.showDialog(this.terms["time.employee.employee.vacation.preliminaryuseddays"], this.usedDaysPrelDetails, SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
    }

    // HELP-METHODS

    private hasPreliminaryDetails() {
        return this.usedDaysPrelDetails && this.usedDaysPrelDetails.length > 0;
    }

    private setCurrentVacationYear() {
        var str: string = "{0}: ".format(this.terms["time.employee.employee.vacation.currentvacationyear"]);

        this.vacationGroupId = 0;
        var currentVacationGroup = this.getCurrentEmploymentVacationGroup();
        if (currentVacationGroup) {
            this.vacationGroupId = currentVacationGroup.vacationGroupId;
            str += currentVacationGroup.name;
            if (currentVacationGroup.fromDate)
                str += " ({0})".format(currentVacationGroup.fromDate.toFormattedDate());
        }

        this.currentVacationYear = str;
    }

    private getCurrentEmploymentVacationGroup(): EmploymentVacationGroupDTO {
        if (!this.employment || !this.employment.employmentVacationGroup || this.employment.employmentVacationGroup.length === 0)
            return null;

        return _.orderBy(_.filter(this.employment.employmentVacationGroup, v => (!v.fromDate || v.fromDate.isSameOrBeforeOnDay(this.selectedEmploymentDate ?? CalendarUtility.getDateToday()))), 'sortableDate', 'desc')[0];
    }

    private setFieldVisibility() {
        // Show hours if vacation group use it
        this.showHours = (this.vacationGroup && this.vacationGroup.vacationGroupSE && (this.vacationGroup.vacationGroupSE.vacationHandleRule === TermGroup_VacationGroupVacationHandleRule.Hours || this.vacationGroup.vacationGroupSE.vacationHandleRule === TermGroup_VacationGroupVacationHandleRule.Shifts));
        this.remainingHoursLabel = (this.vacationGroup && this.vacationGroup.vacationGroupSE && this.vacationGroup.vacationGroupSE.vacationHandleRule === TermGroup_VacationGroupVacationHandleRule.Shifts ? this.terms["time.employee.employee.vacation.remainingshifts"] : this.terms["time.employee.employee.vacation.remaininghours"]);
        this.showYear2To5 = (this.vacationGroup && this.vacationGroup.vacationGroupSE && this.vacationGroup.vacationGroupSE.remainingDaysRule !== TermGroup_VacationGroupRemainingDaysRule.AllRemainingDaysSavedToYear1);
        this.savedDaysYear1Label = (this.showYear2To5 ? "{0} 1".format(this.terms["time.employee.employee.vacation.savedyear"]) : this.terms["time.employee.employee.vacation.saved"]);
    }

    private calculateRemainingDaysPrelSum() {
        this.usedDaysPrelSum = this.employeeVacation ?
            (this.employeeVacation.usedDaysPaid || 0) +
            (this.employeeVacation.usedDaysUnpaid || 0) +
            (this.employeeVacation.usedDaysAdvance || 0) +
            (this.employeeVacation.usedDaysYear1 || 0) +
            (this.employeeVacation.usedDaysYear2 || 0) +
            (this.employeeVacation.usedDaysYear3 || 0) +
            (this.employeeVacation.usedDaysYear4 || 0) +
            (this.employeeVacation.usedDaysYear5 || 0) +
            (this.employeeVacation.usedDaysOverdue || 0) : 0;
    }

    private calculateRemainingDaysSum() {
        this.remainingDaysSum = this.employeeVacation ?
            (this.employeeVacation.remainingDaysPaid || 0) +
            (this.employeeVacation.remainingDaysUnpaid || 0) +
            (this.employeeVacation.remainingDaysAdvance || 0) +
            (this.employeeVacation.remainingDaysYear1 || 0) +
            (this.employeeVacation.remainingDaysYear2 || 0) +
            (this.employeeVacation.remainingDaysYear3 || 0) +
            (this.employeeVacation.remainingDaysYear4 || 0) +
            (this.employeeVacation.remainingDaysYear5 || 0) +
            (this.employeeVacation.remainingDaysOverdue || 0) +
            (this.usedDaysPrel || 0) : 0;
    }

    private calculateEarnedDaysRemainingHoursSum() {
        this.earnedDaysRemainingHoursSum = this.employeeVacation ?
            (this.employeeVacation.earnedDaysRemainingHoursPaid || 0) +
            (this.employeeVacation.earnedDaysRemainingHoursUnpaid || 0) +
            (this.employeeVacation.earnedDaysRemainingHoursAdvance || 0) +
            (this.employeeVacation.earnedDaysRemainingHoursYear1 || 0) +
            (this.employeeVacation.earnedDaysRemainingHoursYear2 || 0) +
            (this.employeeVacation.earnedDaysRemainingHoursYear3 || 0) +
            (this.employeeVacation.earnedDaysRemainingHoursYear4 || 0) +
            (this.employeeVacation.earnedDaysRemainingHoursYear5 || 0) +
            (this.employeeVacation.earnedDaysRemainingHoursOverdue || 0) : 0;
    }

    private validate() {
        this.isValid = true;
        this.validationErrors = '';

        //Removed since item 50474
        //if (this.vacationDaysPaidByLaw !== 0) {
        //    var paid: number = (this.employeeVacation.earnedDaysPaid || 0);
        //    var unpaid: number = (this.employeeVacation.earnedDaysUnpaid || 0);
        //    var advance: number = (this.employeeVacation.earnedDaysAdvance || 0);
        //    if (paid + unpaid + advance !== this.vacationDaysPaidByLaw) {
        //        this.validationErrors += this.terms["time.employee.employee.vacation.invalidearneddayssum"].format(paid.toString(), unpaid.toString(), advance.toString(), this.vacationDaysPaidByLaw.toString());
        //        this.isValid = false;
        //    }
        //}
    }
}
