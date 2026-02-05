import { ISchoolHolidayDTO } from "../../Scripts/TypeLite.Net4";
import { SoeEntityState } from "../../Util/CommonEnumerations";

export class SchoolHolidayDTO implements ISchoolHolidayDTO {
	accountId: number;
	accountName: string;
	actorCompanyId: number;
	created: Date;
	createdBy: string;
	dateFrom: Date;
	dateTo: Date;
	isSummerHoliday: boolean;
	modified: Date;
	modifiedBy: string;
	name: string;
	schoolHolidayId: number;
	state: SoeEntityState;
}