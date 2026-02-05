export class TimeAttestTimeStampDTO {
    timeStampEntryId: number;
    stampIn: Date;
    stampOut: Date;
    toolTip: string;

    // Extensions
    stampOutId: number;
    selected: boolean;
    isModified: boolean;
}