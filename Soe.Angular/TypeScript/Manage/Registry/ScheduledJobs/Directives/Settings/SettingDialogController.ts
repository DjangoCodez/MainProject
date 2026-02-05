import { ScheduledJobSettingDTO } from "../../../../../Common/Models/ScheduledJobDTOs";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { SettingDataType } from "../../../../../Util/CommonEnumerations";
import { IRegistryService } from "../../../RegistryService";

export class SettingDialogController {

    private setting: ScheduledJobSettingDTO;
    private isNew = true;

    //@ngInject
    constructor(
        private $timeout: ng.ITimeoutService,
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private registryService: IRegistryService,
        private types: ISmallGenericType[],
        setting: ScheduledJobSettingDTO) {

        this.setting = new ScheduledJobSettingDTO();
        if (!setting) {
            this.setting.dataType = SettingDataType.String;
        } else {
            angular.extend(this.setting, setting);
            this.isNew = false;
        }
    }

    // EVENTS

    private typeChanged() {
        this.$timeout(() => {
            this.setting.setDataType();
            this.setting.name = this.types.find(t => t.id === this.setting.type).name;

            this.setting.boolData = undefined;
            this.setting.dateData = undefined;
            this.setting.decimalData = undefined;
            this.setting.intData = undefined;
            this.setting.strData = undefined;
            this.setting.timeData = undefined;
            this.setting.options = undefined;

            if (this.setting.dataType === SettingDataType.Integer)
                this.loadOptions();
        });
    }

    private loadOptions(): ng.IPromise<any> {
        return this.registryService.getScheduledJobSettingOptions(this.setting.type).then(x => {
            if (x && x.length)
                this.setting.options = x;
        });
    }

    private cancel() {
        this.$uibModalInstance.close();
    }

    private ok() {
        this.$uibModalInstance.close({ setting: this.setting });
    }
}
