import { IEmployeeVacationPrelUsedDaysDTO } from "../../Scripts/TypeLite.Net4";

export class EmployeeVacationPrelUsedDaysDTO implements IEmployeeVacationPrelUsedDaysDTO {
	details: string;
	employeeId: number;
	isHours: boolean;
	sum: number;
}
