import { IChecklistHeadDTO, IChecklistRowDTO } from "../../Scripts/TypeLite.Net4";
import { SoeEntityState, TermGroup_ChecklistHeadType } from "../../Util/CommonEnumerations";


export class ChecklistHeadDTO implements IChecklistHeadDTO {
    actorCompanyId: number;
    addAttachementsToEInvoice: boolean;
    checklistHeadId: number;
    checklistHeadRecordId: number;
    checklistRows: IChecklistRowDTO[];
    created: Date;
    createdBy: string;
    description: string;
    modified: Date;
    modifiedBy: string;
    name: string;
    reportId: number;
    defaultInOrder: boolean;
    state: SoeEntityState;
    type: TermGroup_ChecklistHeadType;
    typeName: string;
}

