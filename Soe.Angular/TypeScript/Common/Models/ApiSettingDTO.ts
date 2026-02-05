import { IApiSettingDTO } from "../../Scripts/TypeLite.Net4";
import { SettingDataType, SoeEntityState, TermGroup_ApiSettingType } from "../../Util/CommonEnumerations";
import { Guid } from "../../Util/StringUtility";

export class ApiSettingDTO implements IApiSettingDTO {
	apiSettingId: number;
	booleanValue: boolean;
	created: Date;
	createdBy: string;
	dataType: SettingDataType;
	description: string;
	integerValue: number;
	isModified: boolean;
	modified: Date;
	modifiedBy: string;
	name: string;
	startDate: Date;
	state: SoeEntityState;
	stopDate: Date;
	stringValue: string;
	type: TermGroup_ApiSettingType;

	// Extensions
	guid: Guid;
	isEditing: boolean;
}
