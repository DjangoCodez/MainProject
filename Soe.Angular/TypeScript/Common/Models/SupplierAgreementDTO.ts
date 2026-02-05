import { ISupplierAgreementDTO } from "../../Scripts/TypeLite.Net4";

export class SupplierAgreementDTO implements ISupplierAgreementDTO {   
	categoryId: number;
	code: string;
	codeType: number;
	date: Date;
	discountPercent: number;
	priceListTypeId: number;
	priceListTypeName: string;
	rebateListId: number;
	state: number;
	sysWholesellerId: number;
	wholesellerName: string;
}
