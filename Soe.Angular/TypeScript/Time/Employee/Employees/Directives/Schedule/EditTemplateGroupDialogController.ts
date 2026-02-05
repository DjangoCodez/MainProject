import { TimeScheduleTemplateGroupEmployeeDTO } from "../../../../../Common/Models/TimeScheduleTemplateDTOs";

export class EditTemplateGroupDialogController {

    private templateGroup: TimeScheduleTemplateGroupEmployeeDTO;
    private isNew: boolean;

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        templateGroup: TimeScheduleTemplateGroupEmployeeDTO,
        initialFromDate: Date) {

        this.isNew = !templateGroup;

        this.templateGroup = new TimeScheduleTemplateGroupEmployeeDTO();
        angular.extend(this.templateGroup, templateGroup);

        if (this.isNew) {
            this.templateGroup.fromDate = initialFromDate;
        }
    }

    // EVENTS

    private cancel() {
        this.$uibModalInstance.close();
    }

    private ok() {
        this.$uibModalInstance.close({ templateGroup: this.templateGroup });
    }

    // HELP-METHODS

    private get enableOk(): boolean {
        if (!this.templateGroup.fromDate)
            return false;

        if (this.templateGroup.toDate && this.templateGroup.toDate.isBeforeOnDay(this.templateGroup.fromDate))
            return false;

        return true;
    }
}
