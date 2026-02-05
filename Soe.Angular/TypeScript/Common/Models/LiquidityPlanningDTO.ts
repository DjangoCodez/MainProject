import { ILiquidityPlanningDTO } from "../../Scripts/TypeLite.Net4";
import { SoeOriginType, LiquidityPlanningTransactionType } from "../../Util/CommonEnumerations";


export class LiquidityPlanningDTO implements ILiquidityPlanningDTO {
    created: Date;
    createdBy: string;
    date: Date;
    invoiceId: number;
    invoiceNr: string;
    liquidityPlanningTransactionId: number;
    modified: Date;
    modifiedBy: string;
    originType: SoeOriginType;
    specification: string;
    total: number;
    transactionType: LiquidityPlanningTransactionType;
    transactionTypeName: string;
    valueIn: number;
    valueOut: number;
}
