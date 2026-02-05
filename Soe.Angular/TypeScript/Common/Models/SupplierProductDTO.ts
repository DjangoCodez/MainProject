import { ISupplierProductDTO, ISupplierProductGridDTO, ISupplierProductPriceDTO, ISupplierProductSearchDTO, ISupplierProductSaveDTO, ISupplierProductPricelistDTO, ISupplierProductPriceComparisonDTO } from "../../Scripts/TypeLite.Net4";
import { SoeEntityState } from "../../Util/CommonEnumerations";

export class SupplierProductDTO implements ISupplierProductDTO {
	created: Date;
	createdBy: string;
	deliveryLeadTimeDays: number;
	intrastatCodeId: number;
	modified: Date;
	modifiedBy: string;
	packSize: number;
	productId: number;
	supplierId: number;
	supplierProductCode: string;
	supplierProductId: number;
	supplierProductName: string;
	supplierProductNr: string;
	supplierProductUnitId: number;
	sysCountryId: number;
}

export class SupplierProductGridDTO implements ISupplierProductGridDTO {
	productId: number;
	productName: string;
	productNr: string;
	supplierId: number;
	supplierName: string;
	supplierNr: string;
	supplierProductCode: string;
	supplierProductId: number;
	supplierProductName: string;
	supplierProductNr: string;
	supplierProductUnitName: string;
}

export class SupplierProductSearchDTO implements ISupplierProductSearchDTO {
	invoiceProductId: number;
	product: string;
	productName: string;
	supplierIds: number[];
	supplierProduct: string;
	supplierProductName: string;
}

export class SupplierProductPriceDTO implements ISupplierProductPriceDTO {
	supplierProductPriceListId: number;
	currencyCode: string;
	currencyId: number;
	endDate: Date;
	price: number;
	quantity: number;
	startDate: Date;
	state: SoeEntityState;
	supplierProductId: number;
	supplierProductPriceId: number;
	sysCurrencyId: number;
	// Extensions
	isModified: boolean;
}

export class SupplierProductSaveDTO implements ISupplierProductSaveDTO {
	priceRows: ISupplierProductPriceDTO[];
	product: ISupplierProductDTO;
}

export class SupplierProductPricelistDTO implements ISupplierProductPricelistDTO {
	startDate: Date;
	endDate: Date;

	supplierId: number;
	supplierName: string;
	supplierNr: string;
	supplierProductPriceListId: number;

	sysWholeSellerId: number;
	sysWholeSellerName: string;
	sysWholeSellerType: number;
	sysWholeSellerTypeName: string;

	created: Date;
	createdBy: string;
	modified: Date;
	modifiedBy: string;
	state: SoeEntityState;
	currencyId: number;

	constructor(id) {
		this.supplierProductPriceListId = id;
	}
    currencyCode: string;
    sysCurrencyId: number;
}

export class SupplierProductPriceComparisonDTO extends SupplierProductPriceDTO implements ISupplierProductPriceComparisonDTO {

	compareSupplierProductPriceId: number;
	comparePrice: number;
	compareQuantity: number;
	compareEndDate: Date;
	compareStartDate: Date;
	ourProductName: string;
	productName: string;
	productNr: string;
}