import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { EmployeeService } from "../../../EmployeeService";
import { InactivateEmployeeDTO } from "../../../../../Common/Models/EmployeeUserDTO";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { SOEMessageBoxImage } from "../../../../../Util/Enumerations";

export class InactivateEmployeesController {

    // Terms
    private terms: { [index: string]: string; };
    private countDownText: string;

    // Data
    private employees: InactivateEmployeeDTO[];

    // Flags
    private validating: boolean = false;
    private countDownInProgress: boolean = false;

    // Properties
    private get showWarning(): boolean {
        return _.some(this.employees, e => !e.success);
    }

    private get nbrOfSelected(): number {
        return _.filter(this.employees, e => e.selected).length;
    }

    private validationCode: number;
    private get isValidCode(): boolean {
        return this.validationCode === this.employeeIds[0];
    }

    //@ngInject
    constructor(
        private $uibModalInstance,
        private $q: ng.IQService,
        private $interval: ng.IIntervalService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private employeeService: EmployeeService,
        private employeeIds: number[]) {
    }

    public $onInit() {
        this.validating = true;
        this.$q.all([
            this.loadTerms(),
            this.validateInactivate()]).then(() => {
                this.validating = false;
            });
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "time.employee.employee.delete.countdowntext",
            "time.employee.employee.delete.result.title"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private validateInactivate(): ng.IPromise<any> {
        return this.employeeService.validateInactivateEmployees(this.employeeIds).then(x => {
            this.employees = x;
            _.filter(this.employees, emp => emp.success).forEach(e => e.selected = true);
        });
    }

    private countDownInterval;
    private countDown() {
        this.countDownInProgress = true;
        this.countDownText = this.terms["time.employee.employee.delete.countdowntext"].format('7');

        var counter: number = 7;
        this.countDownInterval = this.$interval(() => {
            counter--;
            if (counter === 0 && this.countDownInProgress)
                this.inactivate();
            else
                this.countDownText = this.terms["time.employee.employee.delete.countdowntext"].format(counter.toString());
        }, 1000, 7)
    }

    private ok() {
        this.countDown();
    }

    private cancel() {
        if (this.countDownInProgress) {
            this.$interval.cancel(this.countDownInterval);
            this.countDownInProgress = false;
        } else
            this.$uibModalInstance.dismiss('cancel');
    }

    private inactivate() {
        this.employeeService.inactivateEmployees(_.filter(this.employees, e => e.selected).map(e => e.employeeId)).then(result => {
            let invalid = _.filter(result, r => !r.success);
            if (invalid.length > 0) {
                let message: string = '';
                _.forEach(invalid, emp => {
                    message += '({0}) {1}: {2}\n'.format(emp.employeeNr, emp.employeeName, emp.message);
                });

                let key: string = "time.employee.employee.inactivate.unabletoinactivate.title";
                this.translationService.translate(key).then(term => {
                    var modal = this.notificationService.showDialogEx(term, message, SOEMessageBoxImage.Error);
                    modal.result.then(val => {
                        this.$uibModalInstance.close({ success: false });
                    }, (cancel) => { });
                });
            } else {
                this.$uibModalInstance.close({ success: true });
            }
        });
    }
}