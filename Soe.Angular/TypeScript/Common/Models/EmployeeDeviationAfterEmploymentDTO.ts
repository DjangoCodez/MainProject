import { IEmployeeDeviationAfterEmploymentDTO } from "../../Scripts/TypeLite.Net4";
import { EmployeeDatesDTO } from "./EmployeeDatesDTO";

export class EmployeeDeviationAfterEmploymentDTO implements IEmployeeDeviationAfterEmploymentDTO {
	employeeDates: EmployeeDatesDTO;
	employeeId: number;
	employeeNr: string;
	employmentStopDate: Date;
	name: string;
	timePayrollTransactionIds: number[];
	timeSchedulePayrollTransactionIds: number[];
}