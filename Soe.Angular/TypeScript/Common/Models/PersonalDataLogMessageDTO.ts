import { IPersonalDataLogMessageDTO, System } from "../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../Util/CalendarUtility";

export class PersonalDataLogMessageDTO implements IPersonalDataLogMessageDTO {
    actionTypeText: string;
    batch: System.IGuid;
    batchNbr: number;
    informationTypeText: string;
    message: string;
    timeStamp: Date;
    url: string;
    userName: string;

    public fixDates() {
        this.timeStamp = CalendarUtility.convertToDate(this.timeStamp);
    }
}