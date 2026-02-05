import { IGrossProfitCodeDTO } from "../../Scripts/TypeLite.Net4";

export class GrossProfitCodeDTO implements IGrossProfitCodeDTO {
    grossProfitCodeId: number;
    actorCompanyId: number;
    accountYear: string;
    accountYearId: number;
    accountDateFrom: Date;
    accountDateTo: Date;
    accountDimId: number;
    accountId: number;
    code: number;
    name: string;
    description: string;
    openingBalance: number;
    period1: number;
    period2: number;
    period3: number;
    period4: number;
    period5: number;
    period6: number;
    period7: number;
    period8: number;
    period9: number;
    period10: number;
    period11: number;
    period12: number;
    created: Date;
    createdBy: string;
    modified: Date;
    modifiedBy: string;
}
