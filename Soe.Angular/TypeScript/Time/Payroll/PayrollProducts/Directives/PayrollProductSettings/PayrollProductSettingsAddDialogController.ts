import { PayrollGroupSmallDTO } from "../../../../../Common/Models/PayrollGroupDTOs";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";

export class PayrollProductSettingsAddDialogController {
    
    private isNew: boolean;
    private createForPayrollGroupId: number = 0;
    private createFromPayrollGroupId: number = 0;

    //@ngInject
    constructor(private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,        
        private usedPayrollGroups: ISmallGenericType[],        
        private availablePayrollGroups: PayrollGroupSmallDTO[]) {
    }


    $onInit() {

        this.setup();
    }


    public setup() {
     
    }

    //Events
    public cancel() {
        this.$uibModalInstance.close();
    }

    public ok() {
        this.$uibModalInstance.close({ createForPayrollGroupId: this.createForPayrollGroupId, createFromPayrollGroupId: this.createFromPayrollGroupId});
    }

    //Help Methods
    
}
