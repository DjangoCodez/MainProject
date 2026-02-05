import { ISysPayrollPriceDTO, ISysPayrollPriceIntervalDTO } from "../../Scripts/TypeLite.Net4";
import { SoeEntityState, TermGroup_SysPayrollPriceAmountType, TermGroup_SysPayrollPrice } from "../../Util/CommonEnumerations";
import { CalendarUtility } from "../../Util/CalendarUtility";

export class SysPayrollPriceDTO implements ISysPayrollPriceDTO {
    amount: number;
    amountType: any;
    amountTypeName: string;
    code: string;
    created: Date;
    createdBy: string;
    fromDate: Date;
    intervals: SysPayrollPriceIntervalDTO[];
    isModified: boolean;
    modified: Date;
    modifiedBy: string;
    name: string;
    state: SoeEntityState;
    sysCountryId: number;
    sysPayrollPriceId: number;
    sysTermId: number;
    type: any;
    typeName: string;

    public fixDates() {
        this.fromDate = CalendarUtility.convertToDate(this.fromDate);
    }

    public get showIntervals(): boolean {
        let visible = false;

        switch (this.sysTermId) {
            case TermGroup_SysPayrollPrice.SE_EmploymentTax:
            case TermGroup_SysPayrollPrice.SE_PayrollTax:
                visible = true;
                break;
        }

        return visible;
    }
}

export class SysPayrollPriceIntervalDTO implements ISysPayrollPriceIntervalDTO {
    amount: number;
    amountType: TermGroup_SysPayrollPriceAmountType;
    amountTypeName: string;
    created: Date;
    createdBy: string;
    fromInterval: number;
    modified: Date;
    modifiedBy: string;
    state: SoeEntityState;
    sysPayrollPrice: any;
    sysPayrollPriceId: number;
    sysPayrollPriceIntervalId: number;
    toInterval: number;
}
