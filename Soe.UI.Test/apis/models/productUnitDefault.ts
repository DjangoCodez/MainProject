import { IProductUnitDTO, IProductUnitSmallDTO } from "../../models/webapi/generated-interfaces/SOECompModelDTOs";
import { IProductUnitModel } from "../../models/webapi/generated-interfaces/BillingModels";

export const defaultProductUnitDTO: IProductUnitDTO = {
    productUnitId: 0,
    code: "Test",
    name: "Test Product Unit",
    actorCompanyId: 0,
    created: new Date(),
    createdBy: "system",
    modified: new Date(),
    modifiedBy: "system",
};

export const defaultProductUnitModel: IProductUnitModel = {
    productUnit: defaultProductUnitDTO,
    translations: [],
};