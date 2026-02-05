import { IAttestEmployeeListDTO, IEmployeeListEmploymentDTO, ICompanyCategoryRecordDTO, IValidateDeviationChangeResult, IApplyAbsenceDTO } from "../../Scripts/TypeLite.Net4";
import { AttestEmployeeDayTimeBlockDTO, AttestEmployeeDayTimeCodeTransactionDTO } from "./TimeEmployeeTreeDTO";
import { AttestPayrollTransactionDTO } from "./AttestPayrollTransactionDTO";
import { TimeDeviationCauseGridDTO } from "./TimeDeviationCauseDTOs";
import { SoeValidateDeviationChangeResultCode } from "../../Util/CommonEnumerations";

export class AttestEmployeeListDTO implements IAttestEmployeeListDTO {
    employeeId: number;
    employeeNr: string;
    employeeNrSort: string;
    name: string;
    employments: IEmployeeListEmploymentDTO[];
    categoryRecords: ICompanyCategoryRecordDTO[];

    constructor() {
    }
}

export class ValidateDeviationChangeResult implements IValidateDeviationChangeResult {
    applyAbsenceItems: IApplyAbsenceDTO[];
    generatedTimeBlocks: AttestEmployeeDayTimeBlockDTO[];
    generatedTimeCodeTransactions: AttestEmployeeDayTimeCodeTransactionDTO[];
    generatedTimePayrollTransactions: AttestPayrollTransactionDTO[];
    message: string;
    resultCode: SoeValidateDeviationChangeResultCode;
    success: boolean;
    timeDeviationCauses: TimeDeviationCauseGridDTO[];
}