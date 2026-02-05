import { IHouseholdTaxDeductionApplicantDTO } from "../../Scripts/TypeLite.Net4";
import { SoeEntityState } from "../../Util/CommonEnumerations";

export class HouseholdTaxDeductionApplicantDTO implements IHouseholdTaxDeductionApplicantDTO {
    hidden: boolean;
    showButton: boolean;
    householdTaxDeductionApplicantId: number;
    socialSecNr: string;
    apartmentNr: string;
    name: string;
    property: string;
    cooperativeOrgNr: string;
    identifierString: string;
    share: number;
    state: SoeEntityState;
    customerInvoiceRowId: number;
    comment: string;
}

