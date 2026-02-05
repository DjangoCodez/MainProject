import { IContractGroupExtendedGridDTO, IContractGroupGridDTO, IContractGroupDTO, ICommodityCodeDTO, IIntrastatTransactionDTO } from "../../Scripts/TypeLite.Net4";
import { AccountDTO } from "./AccountDTO";
import { SoeEntityState, TermGroup_ContractGroupPeriod, TermGroup_ContractGroupPriceManagement } from "../../Util/CommonEnumerations";

export class CommodityCodeDTO implements ICommodityCodeDTO  {
	code: string;
	endDate: Date;
	intrastatCodeId: number;
	isActive: boolean;
	startDate: Date;
	sysIntrastatCodeId: number;
	text: string;
	useOtherQuantity: boolean;
}

export class IntrastatTransactionDTO implements IIntrastatTransactionDTO {
	amount: number;
	customerInvoiceRowId: number;
	intrastatCodeId: number;
	intrastatTransactionId: number;
	intrastatTransactionType: number;
	netWeight: number;
	notIntrastat: boolean;
	originId: number;
	otherQuantity: string;
	productName: string;
	productNr: string;
	productUnitCode: string;
	productUnitId: number;
	quantity: number;
	rowNr: number;
	state: SoeEntityState;
	sysCountryId: number;

	// Extensions
	instrastatCodeName: string;
	transactionTypeName: string;
	sysCountryName: string;
	isModified: boolean;
}
