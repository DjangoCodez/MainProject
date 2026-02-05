import { IPayrollReviewHeadDTO, IPayrollReviewRowDTO, IPayrollReviewEmployeeDTO } from "../../Scripts/TypeLite.Net4";
import { SoeEntityState, TermGroup_PayrollReviewStatus } from "../../Util/CommonEnumerations";
import { CalendarUtility } from "../../Util/CalendarUtility";

export class PayrollReviewHeadDTO implements IPayrollReviewHeadDTO {
    actorCompanyId: number;
    created: Date;
    createdBy: string;
    dateFrom: Date;
    modified: Date;
    modifiedBy: string;
    name: string;
    payrollGroupIds: number[];
    payrollGroupNames: string;
    payrollLevelIds: number[];
    payrollLevelNames: string;
    payrollPriceTypeIds: number[];
    payrollPriceTypeNames: string;
    payrollReviewHeadId: number;
    rows: PayrollReviewRowDTO[];
    state: SoeEntityState;
    status: TermGroup_PayrollReviewStatus;
    statusName: string;

    public fixDates() {
        this.dateFrom = CalendarUtility.convertToDate(this.dateFrom);
    }

    public setTypes() {
        if (this.rows) {
            this.rows = this.rows.map(r => {
                let rObj = new PayrollReviewRowDTO();
                angular.extend(rObj, r);
                return rObj;
            });
        } else {
            this.rows = [];
        }
    }
}

export class PayrollReviewRowDTO implements IPayrollReviewRowDTO {
    adjustment: number;
    amount: number;
    employeeId: number;
    employeeName: string;
    employeeNr: string;
    employmentAmount: number;
    errorMessage: string;
    isModified: boolean;
    payrollGroupAmount: number;
    payrollGroupId: number;
    payrollGroupName: string;
    payrollLevelId: number;
    payrollLevelName: string;
    payrollPriceTypeId: number;
    payrollPriceTypeName: string;
    payrollReviewHeadId: number;
    payrollReviewRowId: number;
    readOnly: boolean;
    warningMessage: string;

    //extensions
    selectableLevels: PayrollReviewSelectableLevelDTO[];

    public get hasWarning(): boolean {
        return !!this.warningMessage;
    }

    public get hasError(): boolean {
        return !!this.errorMessage;
    }
}

export class PayrollReviewEmployeeDTO implements IPayrollReviewEmployeeDTO {
    employeeId: number;
    employeeNr: string;
    employmentAmount: number;
    name: string;
    payrollGroupAmount: number;
    payrollGroupId: number;
    payrollLevelId: number;
    payrollPriceTypeId: number;
    readOnly: boolean;
    selectableLevels: PayrollReviewSelectableLevelDTO[];
}

export class PayrollReviewSelectableLevelDTO implements PayrollReviewSelectableLevelDTO {
    amount: number;
    fromDate: Date;
    name: string;
    payrollLevelId: number;
}