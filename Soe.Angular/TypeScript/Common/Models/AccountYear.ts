import { IAccountYearBalanceFlatDTO, IAccountYearDTO, IAccountPeriodDTO } from "../../Scripts/TypeLite.Net4";
import { AccountInternalDTO } from "./AccountInternalDTO";
import { SoeEntityState, TermGroup_AccountStatus } from "../../Util/CommonEnumerations";
import { CalendarUtility } from "../../Util/CalendarUtility";

export class AccountYearDTO implements IAccountYearDTO {
    accountYearId: number;
    actorCompanyId: number;
    created: Date;
    createdBy: string;
    from: Date;
    modified: Date;
    modifiedBy: string;
    noOfPeriods: number;
    periods: AccountPeriodDTO[];
    status: TermGroup_AccountStatus;
    statusText: string;
    to: Date;
    yearFromTo: string;

    public fixDates() {
        this.from = CalendarUtility.convertToDate(this.from);
        this.to = CalendarUtility.convertToDate(this.to);
    }
}
export class AccountPeriodDTO implements IAccountPeriodDTO {
    accountPeriodId: number;
    accountYearId: number;
    created: Date;
    createdBy: string;
    from: Date;
    hasExistingVouchers: boolean;
    isDeleted: boolean;
    modified: Date;
    modifiedBy: string;
    periodNr: number;
    startValue: string;
    status: TermGroup_AccountStatus;
    to: Date;

    // Extensions
    isModified: boolean;
    monthName: string;
}

export class AccountYearLightDTO {
    id: number;
    name: string;
}


