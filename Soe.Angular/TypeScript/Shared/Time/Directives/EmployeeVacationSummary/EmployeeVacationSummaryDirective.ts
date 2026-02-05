import { ICoreService } from "../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { VacationGroupDTO, VacationGroupSEDTO } from "../../../../Common/Models/VacationGroupDTO";
import { EmployeeVacationSEDTO } from "../../../../Common/Models/EmployeeVacationDTOs";
import { IEmployeeService as ISharedEmployeeService } from "../../Employee/EmployeeService";
import { TermGroup_VacationGroupRemainingDaysRule, TermGroup_VacationGroupVacationHandleRule } from "../../../../Util/CommonEnumerations";
import { SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../../Util/Enumerations";
import { EmployeeVacationPrelUsedDaysDTO } from "../../../../Common/Models/EmployeeVacationPrelUsedDaysDTO";

export class EmployeeVacationSummaryDirectiveFactory {

    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Shared/Time/Directives/EmployeeVacationSummary/Views/EmployeeVacationSummary.html'),
            scope: {
                employeeId: '=',
                date: '='
            },
            restrict: 'E',
            replace: true,
            controller: EmployeeVacationSummaryController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class EmployeeVacationSummaryController {

    // Init parameters
    private employeeId: number;
    private date: Date;

    // Collections
    private termsArray: any;

    // Data
    private vacationGroup: VacationGroupDTO;
    private summary: EmployeeVacationSEDTO;
    private remainingDaysSum: number = 0;
    private remainingHoursSum: number = 0;
    private preliminaryHoursSum: number = 0;
    private employeeVacationPrelUsedDays: EmployeeVacationPrelUsedDaysDTO;

    // Flags
    private showHours: boolean = false;
    private showYear2To5: boolean = false;

    //@ngInject
    constructor(
        private sharedEmployeeService: ISharedEmployeeService,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private messagingService: IMessagingService,
        private $q: ng.IQService,
        private $scope: ng.IScope) {

        this.loadTerms();
        this.setupWatchers();
    }

    // SETUP

    private setupWatchers() {
        this.$scope.$watch(() => this.employeeId, (newVal, oldVal) => {
            if (newVal !== oldVal || !this.vacationGroup) {
                if (!this.employeeId)
                    this.clear(true);
                this.loadData();
            }
        });
        this.$scope.$watch(() => this.date, (newVal, oldVal) => {
            if (newVal !== oldVal || !this.vacationGroup)
                this.loadData();
        });
    }

    // SERVICE CALLS

    private loadTerms() {
        var keys: string[] = [
            "time.employeevacationsummary.preliminarydayssum",
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.termsArray = terms;
        });
    }

    private loadData() {
        this.loadVacationGroupForEmployee();
        this.loadSummary().then(() => {
            this.getPrelUsedVacationDaysForEmployee();
        });
    }

    private loadVacationGroupForEmployee() {
        if (!this.employeeId) {
            this.vacationGroup = null;
            return;
        }

        this.sharedEmployeeService.getVacationGroupForEmployee(this.employeeId, this.date).then(x => {
            this.vacationGroup = x;
            
            let vacationGroupSE: VacationGroupSEDTO = this.vacationGroup ? this.vacationGroup.vacationGroupSE : null;

            // Show hours if vacation group use it
            this.showHours = vacationGroupSE && (vacationGroupSE.vacationHandleRule === TermGroup_VacationGroupVacationHandleRule.Hours || vacationGroupSE.vacationHandleRule === TermGroup_VacationGroupVacationHandleRule.Shifts);
            
            // Show year 2 to 5
            this.showYear2To5 = !vacationGroupSE || vacationGroupSE.remainingDaysRule !== TermGroup_VacationGroupRemainingDaysRule.AllRemainingDaysSavedToYear1;
        });
    }

    private getPrelUsedVacationDaysForEmployee() {
        if (!this.employeeId || !this.date) {
            this.employeeVacationPrelUsedDays = null;
            this.preliminaryHoursSum = 0;
            this.calculateRemainingDaysSum();
            return;
        }

        this.sharedEmployeeService.getPrelUsedVacationDays(this.employeeId, this.date).then(result => {
            this.employeeVacationPrelUsedDays = result;
            this.calculateRemainingDaysSum();
        });
    }

    private loadSummary(): ng.IPromise<any> {
        var deferral = this.$q.defer<any>();

        if (!this.employeeId) {
            this.clear(true);
            deferral.resolve();
        } else {
            this.sharedEmployeeService.getEmployeeVacation(this.employeeId).then(x => {
                // Convert to typed DTO
                this.clear(false);
                if (x)
                    angular.extend(this.summary, x);
                deferral.resolve();
            });
        }

        return deferral.promise;
    }

    // EVENTS

    private showPreliminaryDetails() {
        if (!this.hasPreliminaryDetails())
            return;

        this.notificationService.showDialog(this.termsArray["time.employeevacationsummary.preliminarydayssum"], this.employeeVacationPrelUsedDays.details, SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
    }

    // HELP-METHODS

    private hasPreliminaryDetails() {
        return this.employeeVacationPrelUsedDays && this.employeeVacationPrelUsedDays.details && this.employeeVacationPrelUsedDays.details.length > 0;
    }

    private clear(clearEmployee: boolean) {
        if (clearEmployee)
            this.employeeId = 0;

        this.summary = new EmployeeVacationSEDTO();
        this.employeeVacationPrelUsedDays = null;
        this.preliminaryHoursSum = 0;
        this.remainingDaysSum = 0;
        this.calculateEarnedDaysRemainingHoursSum();
    }

    private calculateRemainingDaysSum() {
        if (!this.summary)
            this.summary = new EmployeeVacationSEDTO();

        this.remainingDaysSum = this.summary.remainingDaysPaid +
            this.summary.remainingDaysUnpaid +
            this.summary.remainingDaysAdvance +
            this.summary.remainingDaysYear1 +
            this.summary.remainingDaysYear2 +
            this.summary.remainingDaysYear3 +
            this.summary.remainingDaysYear4 +
            this.summary.remainingDaysYear5 +
            this.summary.remainingDaysOverdue -
            this.getPreliminaryDaysSum();
    }

    private getPreliminaryDaysSum(): number {
        return this.employeeVacationPrelUsedDays ? this.employeeVacationPrelUsedDays.sum : 0;
    }

    private calculateEarnedDaysRemainingHoursSum() {
        this.remainingHoursSum = this.summary.earnedDaysRemainingHoursPaid +
            this.summary.earnedDaysRemainingHoursUnpaid +
            this.summary.earnedDaysRemainingHoursAdvance +
            this.summary.earnedDaysRemainingHoursYear1 +
            this.summary.earnedDaysRemainingHoursYear2 +
            this.summary.earnedDaysRemainingHoursYear3 +
            this.summary.earnedDaysRemainingHoursYear4 +
            this.summary.earnedDaysRemainingHoursYear5 +
            this.summary.earnedDaysRemainingHoursOverdue -
            (this.preliminaryHoursSum || 0);
    }
}
