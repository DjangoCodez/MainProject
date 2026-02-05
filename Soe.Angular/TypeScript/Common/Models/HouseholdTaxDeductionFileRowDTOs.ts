import { IHouseholdTaxDeductionApplicantDTO, IHouseholdTaxDeductionFileRowDTO, IHouseholdTaxDeductionFileRowTypeDTO } from "../../Scripts/TypeLite.Net4";
import { SoeEntityState, TermGroup_HouseHoldTaxDeductionType } from "../../Util/CommonEnumerations";

export class HouseholdTaxDeductionFileRowDTO implements IHouseholdTaxDeductionFileRowDTO {
	customerInvoiceRowId: number;
	invoiceNr: string;
	name: string;
	socialSecNr: string;
	property: string;
	apartmentNr: string;
	cooperativeOrgNr: string;
	invoiceTotalAmount: number;
	workAmount: number;
	paidAmount: number;
	appliedAmount: number;
	nonValidAmount: number;
	comment: string;
	paidDate?: Date;
	houseHoldTaxDeductionType: TermGroup_HouseHoldTaxDeductionType;
	types: HouseholdTaxDeductionFileRowTypeDTO[];
}
export class HouseholdTaxDeductionFileRowTypeDTO implements IHouseholdTaxDeductionFileRowTypeDTO {
	sysHouseholdTypeId: number;
	text: string;
	hours: number;
	amount: number;
}
