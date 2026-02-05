import { IVoucherHeadDTO } from "../../Scripts/TypeLite.Net4";
import { AccountingRowDTO } from "./AccountingRowDTO";
import { TermGroup_AccountStatus, TermGroup_VoucherHeadSourceType } from "../../Util/CommonEnumerations";
import { VoucherRowDTO } from "./VoucherRowDTO";

export class VoucherHeadDTO implements IVoucherHeadDTO {
    accountIds: number[];
    accountIdsHandled: boolean;
    accountPeriodId: number;
    accountYearId: number;
    actorCompanyId: number;
    budgetAccountId: number;
    companyGroupVoucher: boolean;
    created: Date;
    createdBy: string;
    date: Date;
    isSelected: boolean;
    modified: Date;
    modifiedBy: string;
    note: string;
    rows: VoucherRowDTO[];
    sourceType: TermGroup_VoucherHeadSourceType;
    sourceTypeName: string;
    status: TermGroup_AccountStatus;
    template: boolean;
    text: string;
    typeBalance: boolean;
    vatVoucher: boolean;
    voucherHeadId: number;
    voucherNr: number;
    voucherSeriesId: number;
    voucherSeriesTypeId: number;
    voucherSeriesTypeName: string;
    voucherSeriesTypeNr: number;

    // Extensions
    accountingRows: AccountingRowDTO[];
}
