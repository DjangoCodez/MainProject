import { IContractGroupDTO } from "../../models/webapi/generated-interfaces/SOECompModelDTOs";

export const defaultContractGroupDTO: IContractGroupDTO = {
    contractGroupId: 0,
    name: "Test",
    description: "Test Description",
    period: 2,
    priceManagement: 2,
    interval: 1,
    dayInMonth: 5,
    invoiceText: "",
    invoiceTextRow: "",
    created: undefined,
    createdBy: "",
    modified: undefined,
    modifiedBy: "",
    state: 0,
    actorCompanyId: 0
};