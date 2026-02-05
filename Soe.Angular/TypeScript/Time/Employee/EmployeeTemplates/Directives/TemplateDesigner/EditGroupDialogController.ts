import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../../Util/Enumerations";
import { EmployeeTemplateGroupDTO } from "../../../../../Common/Models/EmployeeTemplateDTOs";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { TermGroup_EmployeeTemplateGroupType } from "../../../../../Util/CommonEnumerations";

export class EditGroupDialogController {

    private group: EmployeeTemplateGroupDTO;
    private isNew: boolean;    

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private types: ISmallGenericType[],
        group: EmployeeTemplateGroupDTO) {

        this.isNew = !group;

        this.group = new EmployeeTemplateGroupDTO();
        angular.extend(this.group, group);

        if (this.isNew) {
            this.group.type = TermGroup_EmployeeTemplateGroupType.Normal;
            this.group.newPageBefore = false;
        }
    }

    private initDelete() {
        let keys: string[] = [
            "time.employee.employeetemplate.designer.tools.deletegroup",
            "time.employee.employeetemplate.designer.tools.deletegroup.message"
        ];

        this.translationService.translateMany(keys).then(terms => {
            this.notificationService.showDialogEx(terms["time.employee.employeetemplate.designer.tools.deletegroup"], terms["time.employee.employeetemplate.designer.tools.deletegroup.message"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel).result.then(val => {
                if (val)
                    this.delete();
            }, () => {
                // User cancelled
            });
        });
    }

    private delete() {
        this.$uibModalInstance.close({ delete: true });
    }

    private cancel() {
        this.$uibModalInstance.close();
    }

    private ok() {
        this.$uibModalInstance.close({ group: this.group });
    }
}
