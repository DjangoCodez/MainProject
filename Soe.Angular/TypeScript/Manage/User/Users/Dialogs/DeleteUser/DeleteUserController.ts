import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { DeleteUserAction } from "../../../../../Util/CommonEnumerations";
import { DeleteUserDTO } from "../../../../../Common/Models/EmployeeUserDTO";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { SOEMessageBoxImage } from "../../../../../Util/Enumerations";
import { UserService } from "../../../UserService";

export class DeleteUserController {

    // Terms
    private terms: { [index: string]: string; };
    private countDownText: string;

    // Flags
    private enableInactivate: boolean = false;
    private enableRemoveInfo: boolean = false;
    private enableUnidentify: boolean = false;
    private enableDelete: boolean = false;

    private hasEmployee: boolean = false;

    private validating: boolean = false;
    private validationInactivateMessages: string[] = [];
    private validationDeleteMessages: string[] = [];
    private validationImmediateDeleteMessages: string[] = [];
    private onlyWarnings: boolean = false;
    private countDownInProgress: boolean = false;

    // Properties
    private selectedAction: DeleteUserAction = DeleteUserAction.Cancel;
    private get selectedActionValue(): string {
        return this.selectedAction.toString();
    }
    private set selectedActionValue(value: string) {
        this.selectedAction = parseInt(value);
    }

    private removeInfoAddress: boolean = true;
    private removeInfoPhone: boolean = true;
    private removeInfoEmail: boolean = true;
    private removeInfoClosestRelative: boolean = true;
    private removeInfoOtherContactInfo: boolean = true;

    private disconnectEmployee: boolean = false;

    private validationCode: number;
    private get isValidCode(): boolean {
        return this.validationCode === this.userId;
    }

    //@ngInject
    constructor(
        private $uibModalInstance,
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private $interval: ng.IIntervalService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private userService: UserService,
        private userId: number,
        private userText: string,
        employeeId: number,
        private employeeNr: string) {

        this.hasEmployee = !!employeeId;
        this.disconnectEmployee = this.hasEmployee;
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
                    this.selectedAction = DeleteUserAction.Inactivate;
            });
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "manage.user.user.delete.countdowntext",
            "manage.user.user.delete.result.title"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private validateInactivate(): ng.IPromise<any> {
        return this.userService.validateInactivateUser(this.userId).then(result => {
            this.validationInactivateMessages = result.strings;
            if (result.success) {
                this.enableInactivate = true;
                this.enableRemoveInfo = true;
            }
        });
    }

    private validateDelete(): ng.IPromise<any> {
        return this.userService.validateDeleteUser(this.userId).then(result => {
            this.validationDeleteMessages = result.strings;
            if (result.success) {
                this.enableUnidentify = true;
                this.onlyWarnings = (this.validationDeleteMessages.length > 0);
            }
        });
    }

    private validateImmediateDelete(): ng.IPromise<any> {
        return this.userService.validateImmediateDeleteUser(this.userId).then(result => {
            this.validationImmediateDeleteMessages = result.strings;
            if (result.success) {
                this.enableDelete = true;
            }
        });
    }

    private countDownInterval;
    private countDown() {
        this.countDownInProgress = true;
        this.countDownText = this.terms["manage.user.user.delete.countdowntext"].format('7');

        var counter: number = 7;
        this.countDownInterval = this.$interval(() => {
            counter--;
            if (counter === 0 && this.countDownInProgress)
                this.delete();
            else
                this.countDownText = this.terms["manage.user.user.delete.countdowntext"].format(counter.toString());
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
        var input = new DeleteUserDTO(this.userId, this.selectedAction);

        if (this.selectedAction == DeleteUserAction.RemoveInfo) {
            input.removeInfoAddress = this.removeInfoAddress;
            input.removeInfoPhone = this.removeInfoPhone;
            input.removeInfoEmail = this.removeInfoEmail;
            input.removeInfoClosestRelative = this.removeInfoClosestRelative;
            input.removeInfoOtherContactInfo = this.removeInfoOtherContactInfo;
        }

        input.disconnectEmployee = this.disconnectEmployee;

        this.userService.deleteUser(input).then(result => {
            if (result.success) {
                let key: string;
                switch (this.selectedAction) {
                    case DeleteUserAction.Inactivate:
                        key = "manage.user.user.delete.result.inactivated";
                        break;
                    case DeleteUserAction.RemoveInfo:
                        key = "manage.user.user.delete.result.inforemoved";
                        break;
                    case DeleteUserAction.Unidentify:
                        key = "manage.user.user.delete.result.unidentified";
                        break;
                    case DeleteUserAction.Delete:
                        key = "manage.user.user.delete.result.deleted";
                        break;
                }
                this.translationService.translate(key).then(term => {
                    this.notificationService.showDialogEx(this.terms["manage.user.user.delete.result.title"], term.format(this.userText), SOEMessageBoxImage.OK);

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