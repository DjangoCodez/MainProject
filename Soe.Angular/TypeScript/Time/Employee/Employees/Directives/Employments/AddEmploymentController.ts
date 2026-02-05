import { SoeEmploymentFinalSalaryStatus } from "../../../../../Util/CommonEnumerations";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { EmploymentDTO } from "../../../../../Common/Models/EmployeeUserDTO";
import { TimeDeviationCauseDTO } from "../../../../../Common/Models/TimeDeviationCauseDTOs";
import { EmployeeService } from "../../../EmployeeService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../../../Util/Enumerations";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { IFocusService } from "../../../../../Core/Services/focusservice";

export class AddEmploymentController {

    // Terms
    private terms: { [index: string]: string; };

    // Properties
    private dateFrom: Date;
    private dateTo: Date;
    private isSecondaryEmployment: boolean = false;
    private isTemporaryPrimaryEmployment: boolean = false;
    private hibernatingTimeDeviationCauseId: number;
    private copyLatest: boolean = false;
    private closePrevious: boolean = false;
    private finalSalary: boolean = false;
    private closePreviousEnabled: boolean = false;
    private finalSalaryEnabled: boolean = false;
    private comment: string;
    public okClicked: boolean = false;

    // Lookups
    private hibernatingTimeDeviationCauses: TimeDeviationCauseDTO[];

    private get isAnyOtherEmploymentMarkedApplyFinalSalary(): boolean {
        return _.filter(this.employments, i => i.finalSalaryStatus === SoeEmploymentFinalSalaryStatus.ApplyFinalSalary).length > 0;
    }

    //@ngInject
    constructor(
        private $uibModalInstance,
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService,
        private focusService: IFocusService,
        private notificationService: INotificationService,
        private employeeService: EmployeeService,
        private isNew: boolean,
        private showTemporaryPrimary: boolean,
        private showSecondary: boolean,
        private employments: EmploymentDTO[],
        private latestEmployment: EmploymentDTO) {

        if (this.showTemporaryPrimary)
            this.loadHibernatingTimeDeviationCauses();
        this.closePrevious = this.latestEmployment && !this.latestEmployment.dateTo && !this.isTemporaryPrimaryEmployment && !this.isSecondaryEmployment;
        if (this.latestEmployment && this.latestEmployment.dateTo)
            this.dateFrom = this.latestEmployment.dateTo.addDays(1);

        this.focusService.focusById("ctrl_dateFrom");
    }

    // EVENTS

    private isTemporaryPrimaryEmploymentChanged() {
        this.$timeout(() => {
            if (this.isTemporaryPrimaryEmployment) {
                this.isSecondaryEmployment = false;
                this.copyLatest = false;
                this.closePrevious = false;
                this.finalSalary = false;
            }
        });
    }

    private isSecondaryEmploymentChanged() {
        this.$timeout(() => {
            if (this.isSecondaryEmployment) {
                this.isTemporaryPrimaryEmployment = false;
                this.closePrevious = false;
                this.finalSalary = false;
            }
        });
    }

    private doShowCopyLatest() {
        return !this.isNew;
    }

    private doDisableCopyLatest() {
        return this.isTemporaryPrimaryEmployment || (this.latestEmployment && this.latestEmployment.isSecondaryEmployment);
    }

    private doShowClosePrevious() {
        return !this.isNew;
    }

    private doDisableClosePrevious() {
        return this.isTemporaryPrimaryEmployment || this.isSecondaryEmployment || !this.latestEmployment || this.latestEmployment.dateTo;
    }

    private doShowApplyFinalSalary() {
        return this.closePrevious;
    }

    private doDisableApplyFinalSalary() {
        return this.isTemporaryPrimaryEmployment || this.isSecondaryEmployment || !this.closePrevious || this.isAnyOtherEmploymentMarkedApplyFinalSalary;
    }

    private doDisableOk() {
        if (this.okClicked)
            return true;
        if (this.isTemporaryPrimaryEmployment && (!this.dateFrom || !this.dateTo))
            return true;
        return false;
    }

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }

    private loadHibernatingTimeDeviationCauses(): ng.IPromise<any> {
        return this.employeeService.getHibernatingTimeDeviationCauses().then((x) => {
            this.hibernatingTimeDeviationCauses = x;
        });
    }

    private trySave() {
        if (this.isTemporaryPrimaryEmployment) {
            var keys: string[] = [
                "core.warning",
                "time.employee.addemployment.hibernate",
            ];

            return this.translationService.translateMany(keys).then((terms) => {
                var modal = this.notificationService.showDialog(terms["core.warning"], terms["time.employee.addemployment.hibernate"].format(CalendarUtility.toFormattedDate(this.dateFrom), CalendarUtility.toFormattedDate(this.dateTo)), SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
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
            dateFrom: this.dateFrom,
            dateTo: this.dateTo,
            isTemporaryPrimaryEmployment: this.isTemporaryPrimaryEmployment,
            isSecondaryEmployment: this.isSecondaryEmployment,
            copyLatest: this.copyLatest,
            closePrevious: this.closePrevious,
            finalSalary: this.closePrevious && this.finalSalary,
            hibernatingTimeDeviationCauseId: this.hibernatingTimeDeviationCauseId > 0 ? this.hibernatingTimeDeviationCauseId : null,
            comment: this.comment,
        });
    }
}
