import { ICoreService } from "../../Core/Services/CoreService";
import { IHttpService } from "../../Core/Services/HttpService";
import { Constants } from "../../Util/Constants";
import { SoeModule } from "../../Util/CommonEnumerations";
import { IActionResult } from "../../Scripts/TypeLite.Net4";
import { ImportDTO } from "../../Common/Models/ImportDTO";
import { CustomerIODTO } from "../../Common/Models/CustomerIODTO";
import { CustomerInvoiceIODTO } from "../../Common/Models/CustomerInvoiceIODTO";
import { CustomerInvoiceRowIODTO } from "../../Common/Models/CustomerInvoiceRowIODTO";
import { SupplierIODTO } from "../../Common/Models/SupplierIODTO";
import { SupplierInvoiceHeadIODTO } from "../../Common/Models/SupplierInvoiceHeadIODTO";
import { VoucherHeadIODTO } from "../../Common/Models/VoucherHeadIODTO";
import { ProjectIODTO } from "../../Common/Models/ProjectIODTO";
import { TermGroup_IOImportHeadType } from "../../Util/CommonEnumerations";
import { FilesLookupDTO } from "../Models/FilesLookupDTO";


export interface IConnectService {

    // GET
    getImport(importId: number): ng.IPromise<any>;
    getImports(module: SoeModule): ng.IPromise<any>;
    getSysImportDefinitions(module: SoeModule): ng.IPromise<any>;
    getSysImportHeads(): ng.IPromise<any>;
    getImportGridColumns(importHeadType: number): ng.IPromise<any>;
    getImportIOResult(type: number, batchId: string): ng.IPromise<any>;        

    // POST
    getImportSelectionGrid(files: FilesLookupDTO): ng.IPromise<any>;
    saveImport(importDTO: ImportDTO): ng.IPromise<any>;
    importFiles(importId: number, dataStorageIds: any[], accountYearId: number, voucherSeriesId: number, importDefinitionId: number): ng.IPromise<any>
    SaveCustomerIODTO(customerIODTOs: CustomerIODTO[]): ng.IPromise<any>;
    SaveCustomerInvoiceHeadIODTO(customerInvoiceHeadIODTOs: CustomerInvoiceIODTO[]): ng.IPromise<any>;
    SaveCustomerInvoiceRowIODTO(customerInvoiceRowIODTOs: CustomerInvoiceRowIODTO[]): ng.IPromise<any>;
    SaveSupplierIODTO(supplierIODTOs: SupplierIODTO[]): ng.IPromise<any>;
    SaveSupplierInvoiceHeadIODTO(supplierInvoiceHeadIODTOs: SupplierInvoiceHeadIODTO[]): ng.IPromise<any>;
    SaveVoucherHeadIODTO(voucherHeadIODTOs: VoucherHeadIODTO[]): ng.IPromise<any>;
    SaveProjectIODTO(projectIODTOs: ProjectIODTO[]): ng.IPromise<any>;
    ImportIO(importHeadType: TermGroup_IOImportHeadType, ioIds: any[], useAccountDistribution?: boolean, useAccountDims?: boolean, defaultDim1AccountId?: number, defaultDim2AccountId?: number, defaultDim3AccountId?: number, defaultDim4AccountId?: number, defaultDim5AccountId?: number, defaultDim6AccountId?: number): ng.IPromise<any>;

    // DELETE
    deleteImport(importId: number): ng.IPromise<any>;

}

export class ConnectService implements IConnectService {

    //@ngInject
    constructor(private httpService: IHttpService, private $q: ng.IQService) { }

    // GET
    getImports(module: SoeModule) {
        return this.httpService.get(Constants.WEBAPI_CORE_CONNECT_IMPORTS + module, false);
    }

    getImport(importId: number) {
        return this.httpService.get(Constants.WEBAPI_CORE_CONNECT_IMPORT_EDIT + importId, false);
    }

    getSysImportDefinitions(module: SoeModule) {
        return this.httpService.get(Constants.WEBAPI_CORE_CONNECT_SYSIMPORTDEFINITIONS + module, false);
    }

    getSysImportHeads() {
        return this.httpService.get(Constants.WEBAPI_CORE_CONNECT_SYSIMPORTHEADS, false);
    }

    getImportGridColumns(importHeadType: number) {
        return this.httpService.get(Constants.WEBAPI_CORE_CONNECT_IMPORT_GRIDCOLUMNS + importHeadType, false);
    }

    getImportIOResult(type: number, batchId: string) {
        return this.httpService.get(Constants.WEBAPI_CORE_CONNECT_IMPORT_IO_RESULT + type + "/" + batchId, false);
    }    

    // POST
    getImportSelectionGrid(files: FilesLookupDTO): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_CORE_CONNECT_IMPORT_SELECTION_GRID, files);
    }

    saveImport(importDTO: ImportDTO): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_CORE_CONNECT_IMPORT_EDIT, importDTO);
    }

    importFiles(importId: number, dataStorageIds: any[], accountYearId: number, voucherSeriesId: number, importDefinitionId: number): ng.IPromise<IActionResult> {

        var model = {
            importId: importId,
            dataStorageIds: dataStorageIds,
            accountYearId: accountYearId,
            voucherSeriesId: voucherSeriesId,
            importDefinitionId: importDefinitionId,
        }

        return this.httpService.post(Constants.WEBAPI_CORE_CONNECT_IMPORT_FILE, model);
    }

    SaveCustomerIODTO(customerIODTOs: CustomerIODTO[]): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_CORE_CONNECT_SAVE_CUSTOMER_IO, customerIODTOs);
    }

    SaveCustomerInvoiceHeadIODTO(customerInvoiceHeadIODTOs: CustomerInvoiceIODTO[]): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_CORE_CONNECT_SAVE_CUSTOMER_INVOICE_HEAD_IO, customerInvoiceHeadIODTOs);
    }

    SaveCustomerInvoiceRowIODTO(customerInvoiceRowIODTOs: CustomerInvoiceRowIODTO[]): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_CORE_CONNECT_SAVE_CUSTOMER_INVOICE_ROW_IO, customerInvoiceRowIODTOs);
    }

    SaveSupplierIODTO(supplierIODTOs: SupplierIODTO[]): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_CORE_CONNECT_SAVE_SUPPLIER_IO, supplierIODTOs);
    }

    SaveSupplierInvoiceHeadIODTO(supplierInvoiceHeadIODTOs: SupplierInvoiceHeadIODTO[]): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_CORE_CONNECT_SAVE_SUPPLIER_INVOICE_HEAD_IO, supplierInvoiceHeadIODTOs);
    }

    SaveVoucherHeadIODTO(voucherHeadIODTOs: VoucherHeadIODTO[]): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_CORE_CONNECT_SAVE_VOUCHER_HEAD_IO, voucherHeadIODTOs);
    }

    SaveProjectIODTO(projectIODTOs: ProjectIODTO[]): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_CORE_CONNECT_SAVE_PROJECT_IO, projectIODTOs);
    }

    ImportIO(importHeadType: TermGroup_IOImportHeadType, ioIds: any[], useAccountDistribution = false, useAccountDims = false, defaultDim1AccountId = 0, defaultDim2AccountId = 0, defaultDim3AccountId = 0, defaultDim4AccountId = 0, defaultDim5AccountId = 0, defaultDim6AccountId = 0): ng.IPromise<IActionResult> {

        var model = {
            importHeadType: importHeadType,
            ioIds: ioIds,
            useAccountDistribution: useAccountDistribution,
            useAccoungDims: useAccountDims,
            defaultDim1AccountId: defaultDim1AccountId,
            defaultDim2AccountId: defaultDim2AccountId,
            defaultDim3AccountId: defaultDim3AccountId,
            defaultDim4AccountId: defaultDim4AccountId,
            defaultDim5AccountId: defaultDim5AccountId,
            defaultDim6AccountId: defaultDim6AccountId,
        }

        return this.httpService.post(Constants.WEBAPI_CORE_CONNECT_IMPORT_IO, model);
    }

    // DELETE
    deleteImport(importId: number) {
        return this.httpService.delete(Constants.WEBAPI_CORE_CONNECT_IMPORT_EDIT + importId);
    }
}
