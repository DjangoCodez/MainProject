import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { EmployeeFactorDTO } from "../../../../../Common/Models/EmployeeUserDTO";
import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";
import { TermGroup_EmployeeFactorType } from "../../../../../Util/CommonEnumerations";
import { NotificationService } from "../../../../../Core/Services/NotificationService";
import { TranslationService } from "../../../../../Core/Services/TranslationService";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../../Util/Enumerations";

export class EmployeeFactorDialogController {

    private factor: EmployeeFactorDTO;
    private isNew: boolean;

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private translationService: TranslationService,
        private notificationService: NotificationService,
        private types: SmallGenericType[],
        factor: EmployeeFactorDTO) {

        this.isNew = !factor;

        this.factor = new EmployeeFactorDTO();
        angular.extend(this.factor, factor);
        if (this.isNew) {
            this.factor.fromDate = CalendarUtility.getDateToday();
        }
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public ok() {
        if (this.validate())
            this.$uibModalInstance.close({ factor: this.factor });
    }

    private validate(): boolean {
        if (this.factor.type === TermGroup_EmployeeFactorType.Net && this.factor.factor > 5) {
            var keys: string[] = [
                "common.errormessage",
                "time.employee.employee.factor.validate.net"
            ];

            this.translationService.translateMany(keys).then(terms => {
                this.notificationService.showDialogEx(terms["common.errormessage"], terms["time.employee.employee.factor.validate.net"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
            });

            return false;
        }

        return true;
    }
}
