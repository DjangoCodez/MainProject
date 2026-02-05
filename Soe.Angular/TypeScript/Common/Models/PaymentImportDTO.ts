import { IPaymentImportDTO } from "../../Scripts/TypeLite.Net4";
import { ImportPaymentType, SoeEntityState, TermGroup_SysPaymentType } from "../../Util/CommonEnumerations";

export class PaymentImportDTO implements IPaymentImportDTO {
    actorCompanyId: number;
    batchId: number;
    created: Date;
    createdBy: string;
    filename: string;
    importDate: Date;
    importType: ImportPaymentType;
    modified: Date;
    modifiedBy: string;
    numberOfPayments: number;
    paymentImportId: number;
    paymentLabel: string;
    paymentMethodName: string;
    state: SoeEntityState;
    statusName: string;
    sysPaymentTypeId: TermGroup_SysPaymentType;
    totalAmount: number;
    transferStatus: number;
    type: number;
    typeName: string;
}
