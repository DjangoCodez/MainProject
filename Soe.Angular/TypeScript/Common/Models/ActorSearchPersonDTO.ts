import { SoeEntityType } from "../../Util/CommonEnumerations";
import { IActorSearchPersonDTO } from "../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../Util/CalendarUtility";


export class ActorSearchPersonDTO implements IActorSearchPersonDTO {
    consentDate: Date;
    entityType: SoeEntityType;
    entityTypeName: string;
    hasConsent: boolean;
    hasConsentString: string;
    isPrivatePerson: boolean;
    name: string;
    number: string;
    orgNr: string;
    recordId: number;

    public fixDates() {
        this.consentDate = CalendarUtility.convertToDate(this.consentDate);
    }
}

