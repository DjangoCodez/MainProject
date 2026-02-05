import { SupplierAgreementDTO } from "../../../../../Common/Models/SupplierAgreementDTO";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { SoeEntityState, SoeSupplierAgreemntCodeType } from "../../../../../Util/CommonEnumerations";

export class SetGeneralDiscountController {

    //@ngInject
    constructor(private $uibModalInstance,
        private wholesellers: ISmallGenericType[],
        private pricelistTypes: any[],
        private codeTypes: ISmallGenericType[],
        private row: SupplierAgreementDTO
    ) {


    }

    private isSaveDisabled(): boolean {
        return (!this.row.sysWholesellerId) || (!this.row.discountPercent);
    }

    private isNew(): boolean {
        return (!this.row.rebateListId);
    }

    private allowChangeCodeType(): boolean {
        return (this.row.sysWholesellerId === 62 || this.row.sysWholesellerId === 63);
    }

    private allowChangeCode(): boolean {
        return (this.row.codeType === SoeSupplierAgreemntCodeType.MaterialCode || this.row.codeType === SoeSupplierAgreemntCodeType.Product);
    }

    buttonOkClick() {
        this.$uibModalInstance.close(
            {
                row: this.row,
            });
    }

    buttonCancelClick() {
        this.$uibModalInstance.dismiss('cancel');
    }

    buttonDeleteClick() {
        this.row.state = SoeEntityState.Deleted;
        this.$uibModalInstance.close(
            {
                row: this.row,
            });
    }
}