import { INotificationService } from "../../../../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../../../../Core/Services/TranslationService";
import { ISystemService } from "../../../../SystemService";
import { InformationDTO, SysInformationSysCompDbDTO } from "../../../../../../Common/Models/InformationDTOs";
import { ModalUtility } from "../../../../../../Util/ModalUtility";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../../../Util/Enumerations";

export class NotificationDetailsController {

    private deleted: boolean = false;

    //@ngInject
    constructor(private $uibModalInstance,
        $uibModal,
        private $q: ng.IQService,
        private notificationService: INotificationService,
        private translationService: ITranslationService,
        private systemService: ISystemService,
        private information: InformationDTO,
        private modifyPermission: boolean) {
    }

    // ACTIONS

    private deleteNotificationSent(compDb: SysInformationSysCompDbDTO) {
        let keys: string[] = [
            "core.informationmenu.companyinformation.notificationsent.delete",
            "core.informationmenu.companyinformation.notificationsent.delete.message"
        ];

        this.translationService.translateMany(keys).then(terms => {
            var modal = this.notificationService.showDialogEx(terms["core.informationmenu.companyinformation.notificationsent.delete"], terms["core.informationmenu.companyinformation.notificationsent.delete.message"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                if (val) {
                    compDb.notificationSent = null;
                    this.systemService.deleteSysInformationNotificationSent(this.information.informationId, compDb.sysCompDbId).then(result => {
                        if (result.success)
                            this.deleted = true;
                    })
                }
            });
        });
    }

    private cancel() {
        if (this.deleted)
            this.$uibModalInstance.close({ deleted: true });
        else
            this.$uibModalInstance.dismiss(ModalUtility.MODAL_CANCEL);
    }
}
