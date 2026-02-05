import { IContactPersonDTO } from "../../Scripts/TypeLite.Net4";
import { TermGroup_Sex, SoeEntityState } from "../../Util/CommonEnumerations";

export class ContactPersonDTO implements IContactPersonDTO {
    actorContactPersonId: number;
    consentDate: Date;
    consentModified: Date;
    consentModifiedBy: string;
    created: Date;
    createdBy: string;
    description: string;
    categoryIds: number[];
    categoryRecords: any[];
    email: string;
    firstAndLastName: string;
    firstName: string;
    hasConsent: boolean;
    lastName: string;
    modified: Date;
    modifiedBy: string;
    phoneNumber: string;
    position: number;
    positionName: string;
    sex: TermGroup_Sex;
    socialSec: string;
    state: SoeEntityState;

}
