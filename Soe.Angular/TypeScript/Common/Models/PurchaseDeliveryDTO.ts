import { IPurchaseDeliverySaveDTO, IPurchaseDeliverySaveRowDTO, IPurchaseDeliveryDTO, IPurchaseDeliveryRowDTO, IPurchaseDeliveryInvoiceDTO } from "../../Scripts/TypeLite.Net4";
import { SoeEntityState } from "../../Util/CommonEnumerations";

export class PurchaseDeliveryDTO implements IPurchaseDeliveryDTO {
	created: Date;
	createdBy: string;
	deliveryDate: Date;
	deliveryNr: number;
	modified: Date;
	modifiedBy: string;
	purchaseDeliveryId: number;
	supplierId: number;
	supplierName: string;
	supplierNr: string;
	supplierDisplay: string;
}

export class PurchaseDeliveryInvoiceDTO implements IPurchaseDeliveryInvoiceDTO {
	askedPrice: number;
	deliveredQuantity: number;
	linkToInvoice: boolean;
	price: number;
	productId: number;
	productName: string;
	productNumber: string;
	purchaseDeliveryInvoiceId: number;
	purchaseId: number;
	purchaseNr: string;
	purchaseQuantity: number;
	purchaseRowDisplayName: string;
	purchaseRowId: number;
	purchaseRowNr: number;
	quantity: number;
	supplierinvoiceId: number;
	supplierInvoiceSeqNr: number;
	supplierProductId: number;
	supplierProductName: string;
	supplierProductNr: string;
	text: string;
	isModified: boolean;
	isDeleted: boolean;

	//Extensions
	invoiceRowSum: number;
}


export class PurchaseDeliveryRowDTO implements IPurchaseDeliveryRowDTO {
    deliveredQuantity: number;
    deliveryDate: Date;
    isLocked: boolean;
    modified: Date;
    modifiedBy: string;
    productName: string;
    productNr: string;
    purchaseDeliveryId: number;
    purchaseDeliveryRowId: number;
    purchaseNr: string;
    purchasePrice: number;
    purchasePriceCurrency: number;
    purchaseQuantity: number;
    purchaseRowId: number;
    remainingQuantity: number;
    state: SoeEntityState;
    stockCode: string;
    tempRowId: number;

	//extensions 
	isModified: boolean;

	createSaveDTO(): PurchaseDeliverySaveRowDTO {
		const dto = new PurchaseDeliverySaveRowDTO();
		dto.deliveredQuantity = this.deliveredQuantity;
		dto.deliveryDate = this.deliveryDate;
		dto.purchaseDeliveryRowId = this.purchaseDeliveryRowId;
		dto.purchasePrice = this.purchasePrice;
		dto.purchaseRowId = this.purchaseRowId;
		return dto;
    }
}

export class PurchaseDeliverySaveDTO implements IPurchaseDeliverySaveDTO {
	deliveryDate: Date;
	purchaseDeliveryId: number;
	rows: IPurchaseDeliverySaveRowDTO[];
	supplierId: number;
}

export class PurchaseDeliverySaveRowDTO implements IPurchaseDeliverySaveRowDTO {
	deliveredQuantity: number;
	deliveryDate: Date;
	purchaseDeliveryRowId: number;
	purchaseNr: string;
	purchasePrice: number;
	purchasePriceCurrency: number;
	purchaseRowId: number;
	isModified: boolean;
	setRowAsDelivered: boolean;
}