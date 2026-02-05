import { SmallGenericType } from "../../../../Common/Models/SmallGenericType";
import { IHttpService } from "../../../../Core/Services/httpservice";
import { IActionResult, ISupplierProductDTO, ISupplierProductGridDTO, ISupplierProductImportDTO, ISupplierProductPriceComparisonDTO, ISupplierProductPriceDTO, ISupplierProductPricelistDTO, ISupplierProductPriceSearchDTO, ISupplierProductSaveDTO, ISupplierProductSearchDTO, ISupplierProductSmallDTO } from "../../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../../Util/Constants";

export interface ISupplierProductService {
    // GET
    getSupplierProducts(searchModel: ISupplierProductSearchDTO): ng.IPromise<ISupplierProductGridDTO[]>;
    getSupplierProductsSmall(supplierId: number): ng.IPromise<ISupplierProductSmallDTO[]>;
    getSupplierProduct(supplierProductId: number): ng.IPromise<any>;
    getSupplierProductByInvoiceProduct(invoiceProductId: number, supplierId: number): ng.IPromise<ISupplierProductDTO>;
    getSuppliersByInvoiceProduct(invoiceProductId: number): ng.IPromise<SmallGenericType[]>
    getSupplierProductPrices(supplierProductId: number): ng.IPromise<any>;
    getSupplierProductPrice(supplierProductId: number, currentDate: Date, quantity: number, currencyId: number): ng.IPromise<ISupplierProductPriceDTO>;
    getSupplierProductPriceByProduct(productId: number, supplierId: number, currentDate: Date, quantity: number, currencyId: number): ng.IPromise<ISupplierProductPriceDTO>;
    getSupplierPricelists(supplierId: number): ng.IPromise<ISupplierProductPricelistDTO[]>
    getSupplierPricelist(pricelistId: number): ng.IPromise<ISupplierProductPricelistDTO>
    getSupplierPricelistCompare(searchModel: ISupplierProductPriceSearchDTO): ng.IPromise<any>;
    getSupplierPricelistPrices(pricelistId: number, includeComparison: boolean): ng.IPromise<ISupplierProductPriceComparisonDTO[]>;
    getSupplierPricelistImport(importToPriceList: boolean, importPrices: boolean, multipleSuppliers: boolean): ng.IPromise<any>;

    // POST
    saveSupplierProduct(saveDto: ISupplierProductSaveDTO): ng.IPromise<IActionResult>;
    saveSupplierPricelist(saveDto: any): ng.IPromise<IActionResult>;
    performSupplierProductImport(dto: ISupplierProductImportDTO);

    // DELETE
    deleteSupplierProduct(supplierProductId: number): ng.IPromise<IActionResult>;
    deleteSupplierPricelist(pricelistId: number): ng.IPromise<IActionResult>;
}

export class SupplierProductService implements ISupplierProductService {
    //@ngInject
    constructor(private httpService: IHttpService) { }

    getSupplierProducts = (searchModel: ISupplierProductSearchDTO) => {
        return this.httpService.post(Constants.WEBAPI_BILLING_SUPPLIER_PRODUCTS, searchModel);
    }
    getSupplierProductsSmall = (supplierId: number) => {
        return this.httpService.get(Constants.WEBAPI_BILLING_SUPPLIER_PRODUCT_SMALL + supplierId, false);
    }

    getSupplierProduct = (supplierProductId: number) => {
        return this.httpService.get(Constants.WEBAPI_BILLING_SUPPLIER_PRODUCT + supplierProductId, false);
    }

    getSupplierProductByInvoiceProduct = (invoiceProductId: number, supplierId: number) => {
        return this.httpService.get(Constants.WEBAPI_BILLING_SUPPLIER_PRODUCT_BY_INVOICEPRODUCT + invoiceProductId + "/" + supplierId, false);
    }

    getSuppliersByInvoiceProduct = (invoiceProductId: number) => {
        return this.httpService.get(Constants.WEBAPI_BILLING_SUPPLIER_PRODUCT_SUPPLIERS + invoiceProductId, false);
    }

    getSupplierProductPrices = (supplierProductId: number) => {
        return this.httpService.get(Constants.WEBAPI_BILLING_SUPPLIER_PRODUCT_PRICE_LIST + supplierProductId, false);
    }

    getSupplierProductPrice = (supplierProductId: number, currentDate: Date, quantity: number, currencyId: number) => {
        var currentDateString: string = currentDate ? currentDate.toDateTimeString(): null;
        return this.httpService.get(Constants.WEBAPI_BILLING_SUPPLIER_PRODUCT_PRICE + supplierProductId + "/" + currentDateString + "/" + quantity + "/" + currencyId, false);
    }

    getSupplierProductPriceByProduct = (productId: number, supplierId: number, currentDate: Date, quantity: number, currencyId: number) => {
        var currentDateString: string = currentDate ? currentDate.toDateTimeString() : null;
        return this.httpService.get(Constants.WEBAPI_BILLING_SUPPLIER_PRODUCT_PRICE + productId + "/" + supplierId + "/" + currentDateString + "/" + quantity + "/" + currencyId, false);
    }

    getSupplierPricelists = (supplierId: number) => {
        return this.httpService.get(Constants.WEBAPI_BILLING_SUPPLIER_PRODUCT_PRICELIST_LIST + supplierId, false);
    }

    getSupplierPricelist = (pricelistId: number) => {
        return this.httpService.get(Constants.WEBAPI_BILLING_SUPPLIER_PRODUCT_PRICELIST + pricelistId, false);
    }

    getSupplierPricelistCompare = (searchModel: any) => {
        return this.httpService.post(Constants.WEBAPI_BILLING_SUPPLIER_PRODUCT_PRICELIST_COMPARE, searchModel);
    }

    getSupplierPricelistPrices = (pricelistId: number, includeComparison: boolean) => {
        return this.httpService.get(Constants.WEBAPI_BILLING_SUPPLIER_PRODUCT_PRICELIST_PRICES + pricelistId + "/" + includeComparison, false);
    }

    getSupplierPricelistImport = (importToPriceList: boolean, importPrices: boolean, multipleSuppliers: boolean) => {
        return this.httpService.get(Constants.WEBAPI_BILLING_SUPPLIER_PRODUCT_PRICELIST_IMPORT + importToPriceList + "/" + importPrices + "/" + multipleSuppliers, false);
    }

    //POST
    saveSupplierProduct = (saveDto: ISupplierProductSaveDTO) => {
        return this.httpService.post(Constants.WEBAPI_BILLING_SUPPLIER_PRODUCT, saveDto);
    }
    saveSupplierPricelist = (data) => {
        return this.httpService.post(Constants.WEBAPI_BILLING_SUPPLIER_PRODUCT_PRICELIST, data);
    }
    performSupplierProductImport = (data) => {
        return this.httpService.post(Constants.WEBAPI_BILLING_SUPPLIER_PRODUCT_PRICELIST_IMPORT_PERFORM, data);
    }


    // DELETE
    deleteSupplierProduct = (supplierProductId: number) => {
        return this.httpService.delete(Constants.WEBAPI_BILLING_SUPPLIER_PRODUCT + supplierProductId);
    }
    deleteSupplierPricelist = (pricelistId: number) => {
        return this.httpService.delete(Constants.WEBAPI_BILLING_SUPPLIER_PRODUCT_PRICELIST + pricelistId);
    }
}