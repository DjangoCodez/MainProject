import { IApiMessageGridDTO, IApiMessageChangeGridDTO } from "../../Scripts/TypeLite.Net4";

export class ApiMessageGridDTO implements IApiMessageGridDTO {
	apiMessageId: number;
	changes: IApiMessageChangeGridDTO[];
	comment: string;
	created: Date;
	hasError: boolean;
	hasFile: boolean;
	modified: Date;
	recordCount: number;
	identifiers: string;
	sourceTypeName: string;
	statusName: string;
	typeName: string;
	validationMessage: string;
}

export class ApiMessageChangeGridDTO implements IApiMessageChangeGridDTO {
	error: string;
	fieldTypeName: string;
	fromDate: Date;
	fromValue: string;
	hasError: boolean;
	identifier: string;
	recordName: string;
	toDate: Date;
	toValue: string;
	typeName: string;
}

