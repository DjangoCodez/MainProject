import { IHttpService } from "../../Core/Services/HttpService";
import { Constants } from "../../Util/Constants";
import { TermGroup } from "../../Util/CommonEnumerations";

export interface IImportService {

    // GET
    getBatches(selectedIOImportType: TermGroup.IOImportHeadType, allItemsSelection: number): ng.IPromise<any>;

    // POST   

    // DELETE

}

export class ImportService implements IImportService {

    //@ngInject
    constructor(private httpService: IHttpService, private $q: ng.IQService) { }

    // GET
    getBatches(selectedIOImportType: TermGroup.IOImportHeadType, allItemsSelection: number) {
        return this.httpService.get(Constants.WEBAPI_CORE_CONNECT_BATCHES + "/" + selectedIOImportType + "/" + allItemsSelection, false);
    }

    // POST
    
    // DELETE

}
