import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { EmployeeService } from "../../../EmployeeService";
import { DeleteEmployeeAction } from "../../../../../Util/CommonEnumerations";
import { DeleteEmployeeDTO } from "../../../../../Common/Models/EmployeeUserDTO";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { SOEMessageBoxImage } from "../../../../../Util/Enumerations";

export class DeleteEmployeeController {

    // Terms
    private terms: { [index: string]: string; };
    private countDownText: string;

    // Flags
    private enableInactivate: boolean = false;
    private enableRemoveInfo: boolean = false;
    private enableUnidentify: boolean = false;
    private enableDelete: boolean = false;

    private validating: boolean = false;
    private validationInactivateMessages: string[] = [];
    private validationDeleteMessages: string[] = [];
    private validationImmediateDeleteMessages: string[] = [];
    private onlyWarnings: boolean = false;
    private countDownInProgress: boolean = false;

    // Properties
    private selectedAction: DeleteEmployeeAction = DeleteEmployeeAction.Cancel;
    private get selectedActionValue(): string {
        return this.selectedAction.toString();
    }
    private set selectedActionValue(value: string) {
        this.selectedAction = parseInt(value);
    }

    private removeInfoSalaryDistress: boolean = true;
    private removeInfoUnionFee: boolean = true;
    private removeInfoAbsenceSick: boolean = true;
    private removeInfoAbsenceParentalLeave: boolean = true;
    private removeInfoMeeting: boolean = true;
    private removeInfoNote: boolean = true;

    private removeInfoAddress: boolean = true;
    private removeInfoPhone: boolean = true;
    private removeInfoEmail: boolean = true;
    private removeInfoClosestRelative: boolean = true;
    private removeInfoOtherContactInfo: boolean = true;
    private removeInfoImage: boolean = true;
    private removeInfoBankAccount: boolean = true;
    private removeInfoSkill: boolean = true;

    private validationCode: number;
    private get isValidCode(): boolean {
        return this.validationCode === this.employeeId;
    }

    //@ngInject
    constructor(
        private $uibModalInstance,
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private $interval: ng.IIntervalService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private employeeService: EmployeeService,
        private employeeId: number,
        private employeeText: string) {
    }

    public $onInit() {
        this.validating = true;
        this.$q.all([
            this.loadTerms(),
            this.validateInactivate(),
            this.validateDelete(),
            this.validateImmediateDelete()]).then(() => {
                this.validating = false;

                if (this.enableInactivate)
                    this.selectedAction = DeleteEmployeeAction.Inactivate;
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
        return this.employeeService.validateInactivateEmployee(this.employeeId).then(result => {
            this.validationInactivateMessages = result.strings;
            if (result.success) {
                this.enableInactivate = true;
                this.enableRemoveInfo = true;
            }
        });
    }

    private validateDelete(): ng.IPromise<any> {
        return this.employeeService.validateDeleteEmployee(this.employeeId).then(result => {
            this.validationDeleteMessages = result.strings;
            if (result.success) {
                this.enableUnidentify = true;
                this.onlyWarnings = (this.validationDeleteMessages.length > 0);
            }
        });
    }

    private validateImmediateDelete(): ng.IPromise<any> {
        return this.employeeService.validateImmediateDeleteEmployee(this.employeeId).then(result => {
            this.validationImmediateDeleteMessages = result.strings;
            if (result.success) {
                this.enableDelete = true;
            }
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
                this.delete();
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

    private delete() {
        var input = new DeleteEmployeeDTO(this.employeeId, this.selectedAction);

        if (this.selectedAction == DeleteEmployeeAction.RemoveInfo) {
            input.removeInfoSalaryDistress = this.removeInfoSalaryDistress;
            input.removeInfoUnionFee = this.removeInfoUnionFee;
            input.removeInfoAbsenceSick = this.removeInfoAbsenceSick;
            input.removeInfoAbsenceParentalLeave = this.removeInfoAbsenceParentalLeave;
            input.removeInfoMeeting = this.removeInfoMeeting;
            input.removeInfoNote = this.removeInfoNote;
            input.removeInfoAddress = this.removeInfoAddress;
            input.removeInfoPhone = this.removeInfoPhone;
            input.removeInfoEmail = this.removeInfoEmail;
            input.removeInfoClosestRelative = this.removeInfoClosestRelative;
            input.removeInfoOtherContactInfo = this.removeInfoOtherContactInfo;
            input.removeInfoImage = this.removeInfoImage;
            input.removeInfoBankAccount = this.removeInfoBankAccount;
            input.removeInfoSkill = this.removeInfoSkill;
        }

        this.employeeService.deleteEmployee(input).then(result => {
            if (result.success) {
                let key: string;
                switch (this.selectedAction) {
                    case DeleteEmployeeAction.Inactivate:
                        key = "time.employee.employee.delete.result.inactivated";
                        break;
                    case DeleteEmployeeAction.RemoveInfo:
                        key = "time.employee.employee.delete.result.inforemoved";
                        break;
                    case DeleteEmployeeAction.Unidentify:
                        key = "time.employee.employee.delete.result.unidentified";
                        break;
                    case DeleteEmployeeAction.Delete:
                        key = "time.employee.employee.delete.result.deleted";
                        break;
                }
                this.translationService.translate(key).then(term => {
                    this.notificationService.showDialogEx(this.terms["time.employee.employee.delete.result.title"], term.format(this.employeeText), SOEMessageBoxImage.OK);

                    this.$uibModalInstance.close({ success: true, action: this.selectedAction });
                });
            }
            else {
                this.translationService.translate("error.default_error").then(term => {
                    this.notificationService.showDialogEx(term, result.errorMessage, SOEMessageBoxImage.Error);
                });
            }
        });
    }
}