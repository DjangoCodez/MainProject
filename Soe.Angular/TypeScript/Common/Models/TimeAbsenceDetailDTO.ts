import { ITimeAbsenceDetailDTO } from "../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../Util/CalendarUtility";

export class TimeAbsenceDetailDTO implements ITimeAbsenceDetailDTO {
	created: Date;
	createdBy: string;
	date: Date;
	dayName: string;
	dayOfWeekNr: number;
	dayTypeId: number;
	dayTypeName: string;
	employeeId: number;
	employeeNrAndName: string;
	holidayAndDayTypeName: string;
	holidayId: number;
	holidayName: string;
	isHoliday: boolean;
	manuallyAdjusted: boolean;
	modified: Date;
	modifiedBy: string;
	ratio: number;
	ratioText: string;
	sysPayrollTypeLevel3: number;
	sysPayrollTypeLevel3Name: string;
	timeBlockDateDetailId: number;
	timeBlockDateId: number;
	timeDeviationCauseId: number;
	timeDeviationCauseName: string;
	weekInfo: string;
	weekNr: number;

	public fixDates() {
		this.date = CalendarUtility.convertToDate(this.date);
		this.created = CalendarUtility.convertToDate(this.created);
		this.modified = CalendarUtility.convertToDate(this.modified);
	}
}