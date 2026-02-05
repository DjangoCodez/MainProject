import { IReportPrintDTO } from "../../../Scripts/TypeLite.Net4";

export class ReportPrintDTO implements IReportPrintDTO {
    reportId: number;
    ids: number[];
    queue: boolean = false;

    constructor(ids: number[]) {
        this.ids = ids;
    }
}