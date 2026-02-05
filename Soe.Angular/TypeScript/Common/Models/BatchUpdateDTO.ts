import { IBatchUpdateDTO } from "../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../Util/CalendarUtility";
import { BatchUpdateFieldType, SoeEntityType } from "../../Util/CommonEnumerations";
import { NameAndIdDTO } from "./NameAndIdDTO";

export class BatchUpdateDTO implements IBatchUpdateDTO {
	boolValue: boolean;
	children: BatchUpdateDTO[];
	dataType: BatchUpdateFieldType;
	dateValue: Date;
	decimalValue: number;
	doShowFilter: boolean;
	doShowFromDate: boolean;
	doShowToDate: boolean;
	field: number;
	fromDate: Date;
	intValue: number;
	label: string;
	options: NameAndIdDTO[];
	stringValue: string;
	timeValue: number;
	toDate: Date;

	//Extensions
	added: boolean;

	//Properties
	public get timeValueFormatted(): string {
		return CalendarUtility.minutesToTimeSpan(this.intValue);
	}
	public set timeValueFormatted(time: string) {
		var span = CalendarUtility.parseTimeSpan(time);
		this.intValue = CalendarUtility.timeSpanToMinutes(span);
	}
}

export class BatchUpdateModel {
    entityType: SoeEntityType;
	batchUpdates: BatchUpdateDTO[];
	ids: number[];
	filterIds: number[];
}