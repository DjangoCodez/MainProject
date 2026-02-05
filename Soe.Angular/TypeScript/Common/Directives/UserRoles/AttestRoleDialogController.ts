import { UserCompanyAttestRoleDTO } from "../../../Common/Models/EmployeeUserDTO";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { SOEMessageBoxImage } from "../../../Util/Enumerations";
import { CalendarUtility } from "../../../Util/CalendarUtility";

export class AttestRoleDialogController {

    private userCompanyAttestRole: UserCompanyAttestRoleDTO;
    private isNew: boolean;
    // Filters
    private amountFilter: any;

    //@ngInject
    constructor(
        private $filter: ng.IFilterService,
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        attestRole: UserCompanyAttestRoleDTO) {

        this.amountFilter = this.$filter("amount");
        this.userCompanyAttestRole = new UserCompanyAttestRoleDTO();
        angular.extend(this.userCompanyAttestRole, attestRole);
        this.isNew = this.userCompanyAttestRole.attestRoleUserId === 0;
    }

    private setDirty() {
        this.userCompanyAttestRole.isModified = true;
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public ok() {

        var dateFrom = CalendarUtility.convertToDate(this.userCompanyAttestRole.dateFrom);
        var dateTo = CalendarUtility.convertToDate(this.userCompanyAttestRole.dateTo);

        if (this.userCompanyAttestRole.maxAmount > this.userCompanyAttestRole.defaultMaxAmount) {
            let keys: string[] = [
                "common.user.invalidmaxamount.title",
                "common.user.invalidmaxamount.message"
            ];
            this.translationService.translateMany(keys).then(terms => {
                this.notificationService.showDialogEx(terms["common.user.invalidmaxamount.title"], terms["common.user.invalidmaxamount.message"].format(this.amountFilter(this.userCompanyAttestRole.defaultMaxAmount)), SOEMessageBoxImage.Forbidden);
            });
        } else if (dateFrom && dateTo && dateFrom > dateTo) {
            this.translationService.translate("error.invaliddaterange").then(term => {
                this.notificationService.showDialogEx("", term, SOEMessageBoxImage.Forbidden);
            });
        } else {
            this.$uibModalInstance.close({ attestRole: this.userCompanyAttestRole });
        }
    }
}
