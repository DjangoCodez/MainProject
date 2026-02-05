import { ISearchSysLogsDTO } from "../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../Util/CalendarUtility";

export class SearchSysLogsDTO implements ISearchSysLogsDTO {
	companySearch: string;
	exExceptionSearch: string;
	exlMessageSearch: string;
	fromDate: Date;
	fromTime: Date;
	incExceptionSearch: string;
	incMessageSearch: string;
	level: string;
	licenseSearch: string;
	noOfRecords: number;
	roleSearch: string;
	toDate: Date;
	toTime: Date;
	userSearch: string;
	showUnique: boolean;

	public fixDates() {
		this.fromDate = CalendarUtility.convertToDate(this.fromDate);
		this.toDate = CalendarUtility.convertToDate(this.toDate);
		this.fromTime = CalendarUtility.convertToDate(this.fromTime);
		this.toTime = CalendarUtility.convertToDate(this.toTime);
	}

}


