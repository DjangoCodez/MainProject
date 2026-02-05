import { ICheckListMultipleChoiceAnswerRowDTO, IChecklistRowDTO } from "../../Scripts/TypeLite.Net4";
import { SoeEntityState } from "../../Util/CommonEnumerations";


export class CheckListMultipleChoiceAnswerRowDTO implements ICheckListMultipleChoiceAnswerRowDTO {
    checkListMultipleChoiceAnswerHeadId: number;
    checkListMultipleChoiceAnswerRowId: number;
    checklistRows: IChecklistRowDTO[];
    created: Date;
    createdBy: string;
    modified: Date;
    modifiedBy: string;
    question: string;
    state: SoeEntityState;
    typeName: string;
}

