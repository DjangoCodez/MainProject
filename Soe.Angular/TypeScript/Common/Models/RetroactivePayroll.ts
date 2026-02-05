import { IRetroactivePayrollDTO, IRetroactivePayrollAccountDTO, IRetroactivePayrollEmployeeDTO, IAccountDimDTO, IRetroactivePayrollOutcomeDTO, IAttestPayrollTransactionDTO } from "../../Scripts/TypeLite.Net4";
import { Guid } from "../../Util/StringUtility";
import { SoeEntityState, TermGroup_SoeRetroactivePayrollStatus, TermGroup_RetroactivePayrollAccountType, TermGroup_SoeRetroactivePayrollEmployeeStatus, TermGroup_SoeRetroactivePayrollOutcomeErrorCode, TermGroup_PayrollResultType } from "../../Util/CommonEnumerations";
import { AccountDTO } from "./AccountDTO";
import { AccountDimDTO } from "./AccountDimDTO";
import { CalendarUtility } from "../../Util/CalendarUtility";

export class RetroactivePayrollDTO implements IRetroactivePayrollDTO {
    actorCompanyId: number;
    created: Date;
    createdBy: string;
    dateFrom: Date;
    dateTo: Date;
    modified: Date;
    modifiedBy: string;
    name: string;
    note: string;
    nrOfEmployees: number;
    retroactivePayrollAccounts: RetroactivePayrollAccountDTO[];
    retroactivePayrollEmployees: RetroactivePayrollEmployeeDTO[];
    retroactivePayrollId: number;
    state: SoeEntityState;
    status: TermGroup_SoeRetroactivePayrollStatus;
    statusName: string;
    timePeriodHeadId: number;
    timePeriodHeadName: string;
    timePeriodId: number;
    timePeriodName: string;
    timePeriodPaymentDate: Date;
    totalAmount: number;

    public fixDates() {
        this.dateFrom = CalendarUtility.convertToDate(this.dateFrom);
        this.dateTo = CalendarUtility.convertToDate(this.dateTo);
        this.timePeriodPaymentDate = CalendarUtility.convertToDate(this.timePeriodPaymentDate);
    }
}

export class RetroactivePayrollAccountDTO implements IRetroactivePayrollAccountDTO {
    accountDim: AccountDimDTO;
    accountDimId: number;
    accountId: number;
    created: Date;
    createdBy: string;
    modified: Date;
    modifiedBy: string;
    retroactivePayrollAccountId: number;
    retroactivePayrollId: number;
    state: SoeEntityState;
    type: TermGroup_RetroactivePayrollAccountType;

    //extensions
    guid: Guid;
    selectedAccount: AccountDTO;

    constructor() {
    }
}

export class RetroactivePayrollEmployeeDTO implements IRetroactivePayrollEmployeeDTO {
    actorCompanyId: number;
    categoryIds: number[];
    created: Date;
    createdBy: string;
    employeeId: number;
    employeeName: string;
    employeeNr: string;
    hasOutcomes: boolean;
    hasTransactions: boolean;
    modified: Date;
    modifiedBy: string;
    note: string;
    payrollGroupId: number;
    retroactivePayrollEmployeeId: number;
    retroactivePayrollId: number;
    retroactivePayrollOutcomes: IRetroactivePayrollOutcomeDTO[];
    state: SoeEntityState;
    status: TermGroup_SoeRetroactivePayrollEmployeeStatus;
    statusName: string;
    totalAmount: number;

    //extensions
    selected: boolean;
    moved: boolean;
    guid: Guid;

    constructor() {
    }
}

export class RetroactivePayrollOutcomeDTO implements IRetroactivePayrollOutcomeDTO {
    actorCompanyId: number;
    amount: number;
    created: Date;
    createdBy: string;
    employeeId: number;
    errorCode: TermGroup_SoeRetroactivePayrollOutcomeErrorCode;
    errorCodeText: string;
    isQuantity: boolean;
    isReadOnly: boolean;
    isRetroCalculated: boolean;
    isReversed: boolean;
    isSpecifiedUnitPrice: boolean;
    modified: Date;
    modifiedBy: string;
    productId: number;
    payrollProductName: string;
    payrollProductNumber: string;
    payrollProductNumberSort: string;
    payrollProductString: string;
    quantity: number;
    quantityString: string;
    resultType: TermGroup_PayrollResultType;
    retroactivePayrolIEmployeeId: number;
    retroactivePayrollOutcomeId: number;
    retroUnitPrice: number;
    specifiedUnitPrice: number;
    state: SoeEntityState;
    transactionUnitPrice: number;

    //extensions        
    retroDiffFormatted: string;
    amountFormatted: string;
    hasTransactions: boolean;
    transactions: IAttestPayrollTransactionDTO[];
    constructor() {
    }
}