import { IEmployeeDatesDTO } from "../../Scripts/TypeLite.Net4";

export class EmployeeDatesDTO implements IEmployeeDatesDTO {
	dateRangeText: string;
	dates: Date[];
	employeeId: number;
	startDate: Date;
	stopDate: Date;
}