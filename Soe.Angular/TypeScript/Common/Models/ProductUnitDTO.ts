import { IProductUnitDTO } from "../../Scripts/TypeLite.Net4";

export class ProductUnitDTO implements IProductUnitDTO {
	actorCompanyId: number;
	code: string;
	created: Date;
	createdBy: string;
	modified: Date;
	modifiedBy: string;
	name: string;
	productUnitId: number;
}

