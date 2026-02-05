import { VoucherRowIODTO } from "./VoucherRowIODTO";
import { SoeEntityState, TermGroup_IOType, TermGroup_IOStatus, TermGroup_IOSource, TermGroup_IOImportHeadType } from "../../Util/CommonEnumerations";

    export class VoucherHeadIODTO  {

        voucherHeadIOId: number;
        actorCompanyId: number;
        import: boolean;
        type: TermGroup_IOType;
        status: TermGroup_IOStatus;
        source: TermGroup_IOSource;
        importHeadType: TermGroup_IOImportHeadType;
        batchId: string;
        errorMessage: string;
        voucherHeadId: number;
        date: Date;
        voucherNr: string;
        text: string;
        note: string;
        created: Date;
        createdBy: string;
        modified: Date;
        modifiedBy: string;
        isVatVoucher: boolean;
        transferType: string;
        voucherSeriesTypeNr: number;
        statusName: string;
        accountYearId: number;
        accountPeriodId: number;
        voucherSeriesId: number;
        isSelected: boolean;
        isModified: boolean;
        voucherSeriesName: string;
        isVatVoucherText: string;
        rows: VoucherRowIODTO[];

    }
