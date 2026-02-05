import { PayrollImportEmployeeScheduleDTO } from "../../../../../Common/Models/PayrollImport";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { TermGroup_PayrollImportEmployeeScheduleStatus } from "../../../../../Util/CommonEnumerations";
import { IPayrollService } from "../../../PayrollService";

export class EditScheduleDialogController {

    private schedule: PayrollImportEmployeeScheduleDTO;
    private isNew: boolean;

    //@ngInject
    constructor(
        private $timeout: ng.ITimeoutService,
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private payrollService: IPayrollService,
        private scheduleStatuses: ISmallGenericType[],
        payrollImportEmployeeId: number,
        schedule: PayrollImportEmployeeScheduleDTO) {

        this.isNew = !schedule;

        this.schedule = new PayrollImportEmployeeScheduleDTO();
        angular.extend(this.schedule, schedule);
        if (this.isNew) {
            this.schedule.payrollImportEmployeeId = payrollImportEmployeeId;
            this.schedule.status = TermGroup_PayrollImportEmployeeScheduleStatus.Unprocessed;
            this.schedule.date = CalendarUtility.getDateToday();
            this.schedule.startTime = CalendarUtility.DefaultDateTime();
            this.schedule.stopTime = CalendarUtility.DefaultDateTime();
        }
    }

    // EVENTS

    private startTimeChanged() {
        let val = this.schedule.startTime;
        this.$timeout(() => {
            // Check that value is actually changed and not just tabbing through the field
            if (this.schedule.startTime !== val)
                this.schedule.quantity = this.schedule.duration;
        });
    }

    private stopTimeChanged() {
        let val = this.schedule.stopTime;
        this.$timeout(() => {
            // Check that value is actually changed and not just tabbing through the field
            if (this.schedule.stopTime !== val)
                this.schedule.quantity = this.schedule.duration;
        });
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public save() {
        this.payrollService.savePayrollImportEmployeeSchedule(this.schedule).then(result => {
            if (result.success)
                this.$uibModalInstance.close({ schedule: this.schedule });
            else {
                this.translationService.translate("error.default_error").then(term => {
                    this.notificationService.showErrorDialog(term, result.errorMessage, result.stackTrace);
                });
            }
        });
    }
}
