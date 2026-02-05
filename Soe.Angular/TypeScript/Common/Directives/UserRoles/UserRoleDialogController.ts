import { UserCompanyRoleDTO } from "../../../Common/Models/EmployeeUserDTO";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { SOEMessageBoxImage } from "../../../Util/Enumerations";
import { SmallGenericType } from "../../Models/SmallGenericType";

export class UserRoleDialogController {

    private userCompanyRole: UserCompanyRoleDTO;
    private isNew: boolean;

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private companyRoles: SmallGenericType[],
        userCompanyRole: UserCompanyRoleDTO,
        private roleReadOnly: boolean) {

        this.userCompanyRole = new UserCompanyRoleDTO();
        angular.extend(this.userCompanyRole, userCompanyRole);
        this.isNew = this.userCompanyRole.userCompanyRoleId === 0;

        if (!this.userCompanyRole.roleId && this.companyRoles.length > 0) {
            let role = this.companyRoles[0];
            this.userCompanyRole.roleId = role.id;
            this.userCompanyRole.name = role.name;
        }
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public ok() {
        this.userCompanyRole.isModified = true;
        let dateFrom = CalendarUtility.convertToDate(this.userCompanyRole.dateFrom);
        let dateTo = CalendarUtility.convertToDate(this.userCompanyRole.dateTo);

        if (dateFrom && dateTo && dateFrom > dateTo) {
            this.translationService.translate("error.invaliddaterange").then(term => {
                this.notificationService.showDialogEx("", term, SOEMessageBoxImage.Forbidden);
            });
        } else {
            this.$uibModalInstance.close({ userCompanyRole: this.userCompanyRole });
        }
    }
}
