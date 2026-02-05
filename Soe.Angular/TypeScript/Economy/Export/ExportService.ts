import { IHttpService } from "../../Core/Services/HttpService";
import { IActionResult } from "../../Scripts/TypeLite.Net4";
import { Constants } from "../../Util/Constants";

export interface IExportService {

    getSAFTTransactions(dateFrom: Date, dateTo: Date): ng.IPromise<any[]>;
    getSAFTExportFile(dateFrom: Date, dateTo: Date): ng.IPromise<IActionResult>;
    // POST

    // DELETE

}

export class ExportService implements IExportService {

    //@ngInject
    constructor(private httpService: IHttpService) { }

    getSAFTTransactions(dateFrom: Date, dateTo: Date) {
        const dateFromString: string = dateFrom.toDateTimeString();
        const dateToString = dateTo.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_ECONOMY_EXPORT_SAFT_TRANSACTIONS + dateFromString + "/" + dateToString, false);
    }

    getSAFTExportFile(dateFrom: Date, dateTo: Date) {
        const dateFromString: string = dateFrom.toDateTimeString();
        const dateToString = dateTo.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_ECONOMY_EXPORT_SAFT_EXPORT + dateFromString + "/" + dateToString, false);
    }

    // POST


    // DELETE

}
