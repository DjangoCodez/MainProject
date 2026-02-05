import { ICheckListMultipleChoiceAnswerHeadDTO, IChecklistRowDTO } from "../../Scripts/TypeLite.Net4";
import { SoeEntityState } from "../../Util/CommonEnumerations";


export class CheckListMultipleChoiceAnswerHeadDTO implements ICheckListMultipleChoiceAnswerHeadDTO {
    actorCompanyId: number;
    checkListMultipleChoiceAnswerHeadId: number;
    checklistRows: IChecklistRowDTO[];
    created: Date;
    createdBy: string;
    modified: Date;
    modifiedBy: string;
    state: SoeEntityState;
    title: string;
    typeName: string;
}

