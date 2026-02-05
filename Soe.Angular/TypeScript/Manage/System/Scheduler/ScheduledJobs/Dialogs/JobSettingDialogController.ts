import { SysJobSettingDTO } from "../../../../../Common/Models/SysJobDTO";
import { SettingDataType } from "../../../../../Util/CommonEnumerations";

export class JobSettingDialogController {

    private setting: SysJobSettingDTO;

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private settingTypes: any[],
        setting: SysJobSettingDTO) {

        this.setting = new SysJobSettingDTO();
        if (!setting) {
            this.setting.dataType = SettingDataType.String;
        } else {
            angular.extend(this.setting, setting);
        }
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public ok() {
        this.$uibModalInstance.close({ setting: this.setting });
    }
}
