import { IChecklistHeadDTO, IChecklistRowDTO, System } from "../../Scripts/TypeLite.Net4";
import { SoeEntityState, TermGroup_ChecklistRowType } from "../../Util/CommonEnumerations";


export class ChecklistRowDTO implements IChecklistRowDTO {
    checklistHead: IChecklistHeadDTO;
    checklistHeadId: number;
    checkListMultipleChoiceAnswerHeadId: number;
    checklistRowId: number;
    created: Date;
    createdBy: string;
    guid: System.IGuid;
    isModified: boolean;
    mandatory: boolean;
    mandatoryName: string;
    modified: Date;
    modifiedBy: string;
    rowNr: number;
    state: SoeEntityState;
    text: string;
    type: TermGroup_ChecklistRowType;
    typeName: string;
}

