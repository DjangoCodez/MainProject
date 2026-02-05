import { PayrollGroupPriceTypeDTO } from "../../../../../Common/Models/PayrollGroupDTOs";
import { PayrollLevelDTO } from "../../../../../Common/Models/PayrollLevelDTO";
import { PayrollPriceTypeDTO } from "../../../../../Common/Models/PayrollPriceTypeDTOs";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";

export class PayrollGroupPriceTypeDialogController {

    private priceType: PayrollGroupPriceTypeDTO;
    private isNew: boolean;

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private payrollPriceTypes: PayrollPriceTypeDTO[],
        private payrollLevels: PayrollLevelDTO[],        
        private payrollLevelVisible: boolean,
        priceType: PayrollGroupPriceTypeDTO) {

        this.isNew = !priceType;        
        this.priceType = new PayrollGroupPriceTypeDTO();
        angular.extend(this.priceType, priceType);        
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public ok() {        
        if (this.priceType.payrollLevelId && this.priceType.payrollLevelId != 0) {            
            this.priceType.readOnlyOnEmployee = false;            
        }
        this.$uibModalInstance.close({ priceType: this.priceType });
    }
}
