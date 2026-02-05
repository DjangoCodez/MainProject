import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { IScheduleService } from "../../../ScheduleService";
import { EmployeeListDTO } from "../../../../../Common/Models/EmployeeListDTO";
import { StringUtility } from "../../../../../Util/StringUtility";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../../Util/Enumerations";
import { IProgressHandler } from "../../../../../Core/Handlers/ProgressHandler";
import { IProgressHandlerFactory } from "../../../../../Core/Handlers/progresshandlerfactory";

export class CopyScheduleController {

    private progress: IProgressHandler;
    private executing = false;

    // Terms
    private terms: any = [];
    private infoStr: string;
    private validationInfoStr: string;

    // Properties
    private sourceEmployee: EmployeeListDTO;
    private targetEmployee: EmployeeListDTO;

    private sourceDateEnd: Date;
    private targetDateStart: Date;
    private targetDateEnd: Date;
    private useAccountingFromSourceSchedule: boolean = true;

    // Validation
    private mandatoryFieldKeys: string[] = [];
    private validationErrorKeys: string[] = [];

    private edit: ng.IFormController;

    //@ngInject
    constructor(
        private $uibModalInstance,
        progressHandlerFactory: IProgressHandlerFactory,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private scheduleService: IScheduleService,
        private useAccountHierarchy: boolean,
        private employees: EmployeeListDTO[],
        private employeeId: number) {

        this.progress = progressHandlerFactory.create();

        this.loadTerms();
        this.init();
    }

    // INIT

    private init() {
        if (this.employeeId)
            this.sourceEmployee = _.find(this.employees, e => e.employeeId === this.employeeId);
    }

    // SERVICE CALLS

    private loadTerms() {
        var keys: string[] = [
            "time.schedule.planning.copyschedule.info",
            "time.schedule.planning.copyschedule.validation.info",
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;

            this.infoStr = StringUtility.ToBr(this.terms["time.schedule.planning.copyschedule.info"]);
            this.validationInfoStr = StringUtility.ToBr(this.terms["time.schedule.planning.copyschedule.validation.info"]);

        });
    }

    private copySchedule() {
        this.executing = true;
        this.progress.startWorkProgress((completion) => {
            if (this.sourceEmployee.employeeId && this.targetEmployee.employeeId && this.targetDateStart) {
                this.scheduleService.copySchedule(this.sourceEmployee.employeeId, this.sourceDateEnd, this.targetEmployee.employeeId, this.targetDateStart, this.targetDateEnd, this.useAccountingFromSourceSchedule).then(copyResult => {
                    this.executing = false;
                    if (copyResult.success) {
                        completion.completed(null, true);

                        this.$uibModalInstance.close({
                            success: true,
                            sourceEmployeeId: this.sourceEmployee.employeeId,
                            targetEmployeeId: this.targetEmployee.employeeId
                        });
                    } else {
                        this.translationService.translate("time.schedule.planning.copyschedule.servererror.title").then(term => {
                            completion.failed(null, true);
                            this.notificationService.showErrorDialog(term, copyResult.errorMessage, copyResult.stackTrace);
                        })
                    }
                });
            }
        });
    }

    // VALIDATION

    private validate() {
        this.mandatoryFieldKeys = [];
        this.validationErrorKeys = [];

        var errors = this['edit'].$error;

        if (!this.sourceEmployee)
            this.mandatoryFieldKeys.push("time.schedule.planning.copyschedule.sourceemployee");

        if (!this.targetEmployee)
            this.mandatoryFieldKeys.push("time.schedule.planning.copyschedule.targetemployee");

        if (errors["sameEmployee"])
            this.validationErrorKeys.push("time.schedule.planning.copyschedule.error.sameemployee");

        if (!this.targetDateStart)
            this.mandatoryFieldKeys.push("time.schedule.planning.copyschedule.targetdatestart");

        if (errors["targetDates"])
            this.validationErrorKeys.push("time.schedule.planning.copyschedule.error.targetdates");
    }

    private showValidationError() {
        this.mandatoryFieldKeys = [];
        this.validationErrorKeys = [];
        this.validate();

        var keys: string[] = [];

        if (this.mandatoryFieldKeys.length > 0 || this.validationErrorKeys.length > 0) {
            keys.push("time.schedule.planning.copyschedule.error.title");

            // Mandatory fields
            if (this.mandatoryFieldKeys.length > 0) {
                keys.push("core.missingmandatoryfield");
                _.forEach(this.mandatoryFieldKeys, (key) => {
                    keys.push(key);
                });
            }

            // Other messages
            if (this.validationErrorKeys.length > 0) {
                _.forEach(this.validationErrorKeys, (key) => {
                    keys.push(key);
                });
            }
        }

        if (keys.length > 0) {
            this.translationService.translateMany(keys).then((terms) => {
                var message: string = "";

                // Mandatory fields
                if (this.mandatoryFieldKeys.length > 0) {
                    _.forEach(this.mandatoryFieldKeys, (key) => {
                        message = message + terms["core.missingmandatoryfield"] + " " + terms[key].toLocaleLowerCase() + ".\\n";
                    });
                }

                // Other messages
                if (this.validationErrorKeys.length > 0) {
                    _.forEach(this.validationErrorKeys, (key) => {
                        message = message + terms[key] + ".\\n";
                    });
                }

                this.notificationService.showDialog(terms["time.schedule.planning.copyschedule.error.title"], message, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
            });
        }
    }

    // EVENTS

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }

    private ok() {
        this.copySchedule();
    }
}
