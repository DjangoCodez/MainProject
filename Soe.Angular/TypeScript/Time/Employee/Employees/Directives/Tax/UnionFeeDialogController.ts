import { EmployeeUnionFeeDTO } from "../../../../../Common/Models/EmployeeUserDTO";
import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";

export class UnionFeeDialogController {

    private unionFee: EmployeeUnionFeeDTO;

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private unionFees: SmallGenericType[],
        unionFee: EmployeeUnionFeeDTO) {

        this.unionFee = new EmployeeUnionFeeDTO();
        angular.extend(this.unionFee, unionFee);
        this.unionFee.fixDates();        
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public ok() {
        this.$uibModalInstance.close({ unionFee: this.unionFee });
    }
}
