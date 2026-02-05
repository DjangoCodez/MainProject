import { UserCompanyRoleDTO } from "../../../../../Common/Models/EmployeeUserDTO";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { SOEMessageBoxImage } from "../../../../../Util/Enumerations";

export class RoleDialogController {

    private role: UserCompanyRoleDTO;
    private sourceRole: UserCompanyRoleDTO;

    //@ngInject
    constructor(
        private $timeout: ng.ITimeoutService,
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private roles: UserCompanyRoleDTO[],
        role: UserCompanyRoleDTO) {

        this.role = new UserCompanyRoleDTO();
        angular.extend(this.role, role);

        this.setSourceRole();
    }

    // HELP-METHODS

    private setSourceRole() {
        this.sourceRole = this.role.roleId ? _.find(this.roles, r => r.roleId === this.role.roleId) : null;
    }

    // EVENTS

    private roleChanged() {
        this.$timeout(() => {
            this.setSourceRole();
            this.role.name = this.sourceRole ? this.sourceRole.name : '';
        });
    }

    private cancel() {
        this.$uibModalInstance.close();
    }

    private ok() {
        let dateFrom = CalendarUtility.convertToDate(this.role.dateFrom);
        let dateTo = CalendarUtility.convertToDate(this.role.dateTo);

        let invalidDates: boolean = dateFrom && dateTo && dateFrom > dateTo;
        if (!invalidDates && this.sourceRole.dateFrom && dateFrom && dateFrom < this.sourceRole.dateFrom)
            invalidDates = true;
        if (!invalidDates && this.sourceRole.dateTo && dateTo && dateTo > this.sourceRole.dateTo)
            invalidDates = true;

        if (invalidDates) {
            this.translationService.translate("error.invaliddaterange").then(term => {
                this.notificationService.showDialogEx("", term, SOEMessageBoxImage.Forbidden);
            });
        } else {
            this.$uibModalInstance.close({ role: this.role });
        }
    }
}
