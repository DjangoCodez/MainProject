import { IPurchaseDTO, IPurchaseGridDTO, IOriginUserSmallDTO, IPurchaseRowDTO, IPurchaseSmallDTO, IPurchaseRowSmallDTO } from "../../Scripts/TypeLite.Net4";
import { PurchaseDeliveryStatus, PurchaseRowType, SoeEntityState, SoeOriginStatus } from "../../Util/CommonEnumerations";
import { SmallGenericType } from "./SmallGenericType";

export class PurchaseDTO implements IPurchaseDTO {
	confirmedDeliveryDate: Date;
	contactEComId: number;
	created: Date;
	createdBy: string;
	currencyId: number;
	currencyDate: Date;
	currencyRate: number;
	defaultDim1AccountId: number;
	defaultDim2AccountId: number;
	defaultDim3AccountId: number;
	defaultDim4AccountId: number;
	defaultDim5AccountId: number;
	defaultDim6AccountId: number;
	deliveryAddress: string;
	deliveryAddressId: number;
	deliveryConditionId: number;
	deliveryTypeId: number;
	internalDescription: string;
	modified: Date;
	modifiedBy: string;
	origindescription: string;
	originUsers: IOriginUserSmallDTO[];
	originStatus: number;
	paymentConditionId: number;
	projectId: number;
	projectNr: string;
	orderId: number;
	orderNr: string;
	purchaseDate: Date;
	purchaseId: number;
	purchaseLabel: string;
    purchaseRows: IPurchaseRowDTO[];
	purchaseNr: string;
	referenceOur: string;
	referenceYour: string;
	statusName: string;
	supplierCustomerNr: string;
	supplierId: number;
	supplierEmail: string;
	totalAmountCurrency: number;
	totalAmount: number;
	totalAmountExVatCurrency: number;
	totalAmountExVat: number;
	vatAmountCurrency: number;
	vatAmount: number;
	vatType: number;
	wantedDeliveryDate: Date;

	public static getPropertiesToSkipOnSave(): string[] {
		return ['created', 'createdBy', 'purchaseRows', 'modified', 'modifiedBy', 'originStatusName', 'projectNr'];
	}
}

export class PurchaseSmallDTO implements IPurchaseSmallDTO {
	purchaseId: number;
	purchaseNr: string;
    supplierNr: string
	supplierId: number;
	supplierName: string;
	name: string;
	originDescription: string;
	status: number;
	displayName: string;
	purchaseRows: IPurchaseRowSmallDTO[];
}

export class PurchaseGridDTO implements IPurchaseGridDTO {
	confirmedDate: Date;
	currencyCode: string;
	deliveryStatus: PurchaseDeliveryStatus;
	deliveryDate: Date;
	origindescription: string;
	originStatus: SoeOriginStatus;
	projectNr: string;
	statusName: string;
	purchaseDate: Date;
	purchaseId: number;
	purchaseNr: string;
	purchaseStatus: string;
	statusIcon: number;
	supplierName: string;
	supplierNr: string;
	sysCurrencyId: number;
	totalAmount: number;
	totalAmountExVat: number;
	totalAmountExVatCurrency: number;

	//extension 
	deliveryStatusIcon: string;
	statusIconValue: string;
	statusIconMessage: string;

	public get NumberName(): string {
		return this.purchaseNr + " " + this.supplierName;
	}
}

export class PurchaseRowSmallDTO implements IPurchaseRowSmallDTO {
	purchaseRowId: number;
	purchaseRowNr: number;

	supplierProductId: number;
	supplierProductName: string;
	supplierProductNr: string;

	productId: number;
	productName: string;
	productNumber: string;

	price: number;
	deliveredQuantity: number;
	text: string;

	displayName: string;
}

export class PurchaseRowDTO implements IPurchaseRowDTO {
	accDeliveryDate: Date;
	customerInvoiceRowIds: number[];
	deliveredQuantity: number;
	deliveryDate: Date;
	discountAmount: number;
	discountAmountCurrency: number;
	discountPercent: number;
	discountType: number;
	modified: Date;
	modifiedBy: string;
	parentRowId: number;
	productId: number;
	productName: string;
	productNr: string;
	purchaseName: string;
    purchaseNr: string;
	purchaseId: number;
	purchaseRowId: number;
	purchasePrice: number;
	purchasePriceCurrency: number;
	purchaseUnitId: number;
	quantity: number;
	rowNr: number;
	state: SoeEntityState;
	stockCode: string;
	stockId: number;
	sumAmount: number;
	sumAmountCurrency: number;
	supplierProductId: number;
	supplierProductNr: string;
	intrastatCodeId: number;
	intrastatTransactionId: number;
	sysCountryId: number;
	tempRowId: number;
	text: string;
	type: PurchaseRowType;

	vatAmount: number;
	vatAmountCurrency: number;
	vatCodeCode: string;
	vatCodeId: number;
	vatCodeName: string;
	vatRate: number;
	wantedDeliveryDate: Date;
	orderId: number;
	orderNr: string;

	status: SoeOriginStatus;
	statusName: string;
	isLocked: boolean;

	// Extensions
	purchaseProductUnitCode: string;
	isModified: boolean;
	discountTypeText: string;
	discountValue: number;
	stocksForProduct: SmallGenericType[];
	statusIcon: string;
	
	public static getNextRowNr(rows: IPurchaseRowDTO[]) {
		let rowNr = 0;
		const maxRow = _.maxBy(rows, 'rowNr');
		if (maxRow)
			rowNr = maxRow.rowNr;

		return rowNr + 1;
	}

	public get isTextRow(): boolean {
		return this.type === PurchaseRowType.TextRow;
	}

	public static getPropertiesToSkipOnSave(): string[] {
		return ['created', 'createdBy', 'modified', 'modifiedBy', 'purchaseProductUnitCode', 'productNr','isModified', 'stocksForProduct', 'statusName'];
	}
}