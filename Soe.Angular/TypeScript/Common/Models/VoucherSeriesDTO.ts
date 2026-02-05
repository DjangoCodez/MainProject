import { AccountingRowDTO } from "./AccountingRowDTO";
import { IVoucherSeriesDTO } from "../../Scripts/TypeLite.Net4";
import { AccountInternalDTO } from "./AccountInternalDTO";
import { SoeEntityState, TermGroup_AccountStatus } from "../../Util/CommonEnumerations";

export class VoucherSeriesDTO implements IVoucherSeriesDTO {
    accountYearId: number;
    created: Date;
    createdBy: string;
    isDeleted: boolean;
    isModified: boolean;
    modified: Date;
    modifiedBy: string;
    status: TermGroup_AccountStatus;
    voucherDateLatest: Date;
    voucherNrLatest: number;
    voucherSeriesId: number;
    voucherSeriesTypeId: number;
    voucherSeriesTypeIsTemplate: boolean;
    voucherSeriesTypeName: string;
    voucherSeriesTypeNr: number;
}