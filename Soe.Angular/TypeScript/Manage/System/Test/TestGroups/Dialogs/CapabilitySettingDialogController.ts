import { CalendarUtility } from "../../../../../Util/CalendarUtility";

export class CapabilitySettingDialogController {

    private capabilities;
    private originalKey: string = undefined;
    private key: string;
    private value: string

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        capabilities: any,
        key: string) {
        this.capabilities = capabilities;
        this.originalKey = key;
        if (!key) {
            this.key = "";
            this.value = "";
        }
        else {
            this.key = key;
            this.value = capabilities[key];
        }
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public ok() {
        if (this.originalKey) {
            delete this.capabilities[this.originalKey]
        }
        this.capabilities[this.key] = this.value; 
        this.$uibModalInstance.close({ capabilities: this.capabilities });
    }
}
