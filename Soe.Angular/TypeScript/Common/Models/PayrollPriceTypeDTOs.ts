import { IPayrollPriceTypeDTO, IPayrollPriceTypePeriodDTO, IPayrollPriceTypeGridDTO } from "../../Scripts/TypeLite.Net4";
import { SoeEntityState } from "../../Util/CommonEnumerations";
import { CalendarUtility } from "../../Util/CalendarUtility";

export class PayrollPriceTypeDTO implements IPayrollPriceTypeDTO {
    actorCompanyId: number;
    code: string;
    conditionAgeYears: number;
    conditionEmployeedMonths: number;
    conditionExperienceMonths: number;
    created: Date;
    createdBy: string;
    description: string;
    modified: Date;
    modifiedBy: string;
    name: string;
    payrollPriceTypeId: number;
    periods: PayrollPriceTypePeriodDTO[];
    state: SoeEntityState;
    type: number;
    typeName: string;

    public fixDates() {
        this.created = CalendarUtility.convertToDate(this.created);
        this.modified = CalendarUtility.convertToDate(this.modified);
    }
}

export class PayrollPriceTypePeriodDTO implements IPayrollPriceTypePeriodDTO {
    amount: number;
    fromDate: Date;
    payrollPriceTypeId: number;
    payrollPriceTypePeriodId: number;

    public fixDates() {
        this.fromDate = CalendarUtility.convertToDate(this.fromDate);
    }
}

export class PayrollPriceTypeGridDTO implements IPayrollPriceTypeGridDTO {
    code: string;
    description: string;
    name: string;
    payrollPriceTypeId: number;
    typeName: string;
}