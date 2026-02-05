import { IFixedPayrollRowDTO } from "../../Scripts/TypeLite.Net4";
import { SoeEntityState } from "../../Util/CommonEnumerations";
import { CalendarUtility } from "../../Util/CalendarUtility";

export class FixedPayrollRowDTO implements IFixedPayrollRowDTO {
    actorCompanyId: number;
    amount: number;
    created: Date;
    createdBy: string;
    distribute: boolean;
    employeeId: number;
    fixedPayrollRowId: number;
    fromDate: Date;
    isReadOnly: boolean;
    isSpecifiedUnitPrice: boolean;
    modified: Date;
    modifiedBy: string;
    payrollProductNrAndName: string;
    productId: number;
    quantity: number;
    state: SoeEntityState;
    toDate: Date;
    unitPrice: number;
    vatAmount: number;

    public fixDates() {
        this.fromDate = CalendarUtility.convertToDate(this.fromDate);
        this.toDate = CalendarUtility.convertToDate(this.toDate);
    }
}
