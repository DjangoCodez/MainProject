import { SoeEmploymentFinalSalaryStatus, SoeEntityState } from "../../../../../Util/CommonEnumerations";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { EmployeeService } from "../../../EmployeeService";
import { EmploymentDTO } from "../../../../../Common/Models/EmployeeUserDTO";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";
import { CoreUtility } from "../../../../../Util/CoreUtility";
import { EditUserEmploymentFunctions, SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../../../Util/Enumerations";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { IFocusService } from "../../../../../Core/Services/focusservice";

export class EditEmploymentController {

    // Terms
    private terms: { [index: string]: string; };

    // Properties
    private function: EditUserEmploymentFunctions = EditUserEmploymentFunctions.ChangeEmployment;
    private dateFrom: Date;
    public okClicked: boolean = false;
    private _dateTo: Date;
    private get dateTo(): Date {
        return this._dateTo;
    }
    private set dateTo(date: Date) {
        this._dateTo = date;

        if (!date)
            this.finalSalary = false;
    }
    private selectedEndReason: SmallGenericType;
    private finalSalary: boolean = false;
    private appliedFinalSalaryManually: boolean = false;
    private comment: string;

    // Lookups
    private employmentEndReasons: ISmallGenericType[] = [];

    private get finalSalaryEnabled(): boolean {
        return !!this.dateTo && !this.isAnyOtherEmploymentMarkedApplyFinalSalary && !this.employment.isSecondaryEmployment;
    }
    private get appliedFinalSalaryManuallyEnabled(): boolean {
        return !!this.dateTo && this.employment.finalSalaryStatus !== SoeEmploymentFinalSalaryStatus.AppliedFinalSalary && !this.employment.isSecondaryEmployment;
    }
    private get isAnyOtherEmploymentMarkedApplyFinalSalary(): boolean {
        return _.filter(this.employments, i => i.state === SoeEntityState.Active && i.finalSalaryStatus === SoeEmploymentFinalSalaryStatus.ApplyFinalSalary && this.employment.employmentId !== i.employmentId).length > 0;
    }

    //@ngInject
    constructor(
        private $uibModalInstance,
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService,
        private focusService: IFocusService,
        private notificationService: INotificationService,
        private employeeService: EmployeeService,
        private employment: EmploymentDTO,
        private employments: EmploymentDTO[],
        private usePayroll: boolean,
        private forceModifyEmploymentDates: boolean) {
    }

    public $onInit() {
        this.$q.all([
            this.loadTerms(),
            this.loadEmploymentEndReasons()]).then(() => {
                this.setup();
            });
    }

    private setup() {
        this.dateFrom = this.employment.dateFrom;
        this.dateTo = this.employment.dateTo;
        this.finalSalary = this.employment.finalSalaryStatus == SoeEmploymentFinalSalaryStatus.ApplyFinalSalary;
        this.appliedFinalSalaryManually = this.employment.finalSalaryStatus == SoeEmploymentFinalSalaryStatus.AppliedFinalSalaryManually;
        this.selectedEndReason = _.find(this.employmentEndReasons, r => r.id === this.employment.employmentEndReason);
        if (this.forceModifyEmploymentDates) {
            this.function = EditUserEmploymentFunctions.ChangeEmploymentDates;
        }
        this.focusService.focusById("ctrl_dateFrom");
    }

    private loadTerms() {
        var keys: string[] = [
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private loadEmploymentEndReasons(): ng.IPromise<any> {
        return this.employeeService.getEmploymentEndReasons(CoreUtility.languageId).then(x => {
            this.employmentEndReasons = x;
        });
    }

    // EVENTS

    private doDisableOk() {
        if (this.okClicked)
            return true;
        if (!this.dateFrom)
            return true;
        if (this.dateFrom && this.dateTo && this.dateFrom > this.dateTo)
            return true;
        if (this.employment.isTemporaryPrimary && !this.dateTo && this.function !== EditUserEmploymentFunctions.ChangeToNotTemporary)
            return true;
        return false;
    }

    private finalSalaryChanged() {
        this.$timeout(() => {
            if (this.finalSalary)
                this.appliedFinalSalaryManually = false;
        });
    }

    private appliedFinalSalaryManuallyChanged() {
        this.$timeout(() => {
            if (this.appliedFinalSalaryManually)
                this.finalSalary = false;
        });
    }

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }

    private trySave() {
        if (this.function === EditUserEmploymentFunctions.ChangeEmploymentDates && this.employment.isTemporaryPrimary) {
            var keys: string[] = [
                "core.warning",
                "time.employee.editemployment.hibernate",
            ];

            return this.translationService.translateMany(keys).then((terms) => {
                var modal = this.notificationService.showDialog(terms["core.warning"], terms["time.employee.editemployment.hibernate"].format(CalendarUtility.toFormattedDate(this.dateTo)), SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(result => {
                    this.save();
                });
            });
        }
        else {
            this.save();
        }
    }

    private save() {
        this.okClicked = true;
        this.$uibModalInstance.close({
            success: true,
            changeEmploymentFunction: this.function,
            dateFrom: this.dateFrom,
            dateTo: this.dateTo,
            finalSalary: this.finalSalary,
            appliedFinalSalaryManually: this.appliedFinalSalaryManually,
            employmentEndReason: this.selectedEndReason,
            comment: this.comment
        });
    }
}