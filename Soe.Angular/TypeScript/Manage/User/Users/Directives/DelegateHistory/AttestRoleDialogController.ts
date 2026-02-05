import { UserAttestRoleDTO } from "../../../../../Common/Models/EmployeeUserDTO";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { SOEMessageBoxImage } from "../../../../../Util/Enumerations";

export class AttestRoleDialogController {

    private attestRole: UserAttestRoleDTO;
    private sourceAttestRole: UserAttestRoleDTO;

    //@ngInject
    constructor(
        private $timeout: ng.ITimeoutService,
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private attestRoles: UserAttestRoleDTO[],
        attestRole: UserAttestRoleDTO) {

        this.attestRole = new UserAttestRoleDTO();
        angular.extend(this.attestRole, attestRole);

        this.setSourceAttestRole();
    }

    // HELP-METHODS

    private setSourceAttestRole() {
        this.sourceAttestRole = this.attestRole.attestRoleUserId ? _.find(this.attestRoles, r => r.attestRoleUserId === this.attestRole.attestRoleUserId) : null;
    }

    // EVENTS

    private attestRoleChanged() {
        this.$timeout(() => {
            this.setSourceAttestRole();
            // Set attestRoleUserId to be used when saving
            this.attestRole.attestRoleUserId = this.sourceAttestRole ? this.sourceAttestRole.attestRoleUserId : undefined;
            this.attestRole.attestRoleId = this.sourceAttestRole ? this.sourceAttestRole.attestRoleId : undefined;
            this.attestRole.name = this.sourceAttestRole ? this.sourceAttestRole.name : '';
            this.attestRole.moduleName = this.sourceAttestRole ? this.sourceAttestRole.moduleName : '';
            this.attestRole.accountDimName = this.sourceAttestRole ? this.sourceAttestRole.accountDimName : '';
            this.attestRole.accountName = this.sourceAttestRole ? this.sourceAttestRole.accountName : '';
            this.attestRole.accountPermissionTypeName = this.sourceAttestRole ? this.sourceAttestRole.accountPermissionTypeName : '';

            // Children
            this.attestRole.children = [];
            if (this.sourceAttestRole) {
                _.forEach(this.sourceAttestRole.children, sourceChild => {
                    let child = new UserAttestRoleDTO();
                    angular.extend(child, sourceChild);
                    child.fixDates();
                    this.attestRole.children.push(child);

                    child.children = [];
                    _.forEach(sourceChild.children, sourceSubChild => {
                        let subChild = new UserAttestRoleDTO();
                        angular.extend(subChild, sourceSubChild);
                        subChild.fixDates();
                        child.children.push(subChild);
                    });
                });
            }
        });
    }

    private cancel() {
        this.$uibModalInstance.close();
    }

    private ok() {
        let dateFrom = CalendarUtility.convertToDate(this.attestRole.dateFrom);
        let dateTo = CalendarUtility.convertToDate(this.attestRole.dateTo);

        let invalidDates: boolean = dateFrom && dateTo && dateFrom > dateTo;
        if (!invalidDates && this.sourceAttestRole.dateFrom && dateFrom && dateFrom < this.sourceAttestRole.dateFrom)
            invalidDates = true;
        if (!invalidDates && this.sourceAttestRole.dateTo && dateTo && dateTo > this.sourceAttestRole.dateTo)
            invalidDates = true;

        if (invalidDates) {
            this.translationService.translate("error.invaliddaterange").then(term => {
                this.notificationService.showDialogEx("", term, SOEMessageBoxImage.Forbidden);
            });
        } else {
            _.forEach(this.attestRole.children, child => {
                child.dateFrom = dateFrom;
                child.dateTo = dateTo;
            });
            this.$uibModalInstance.close({ attestRole: this.attestRole });
        }
    }
}
