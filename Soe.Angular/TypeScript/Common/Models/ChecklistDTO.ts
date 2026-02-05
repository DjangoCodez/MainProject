import { IChecklistHeadRecordCompactDTO, IChecklistExtendedRowDTO, System, IImagesDTO, IFileUploadDTO } from "../../Scripts/TypeLite.Net4";
import { SoeEntityState, TermGroup_ChecklistRowType } from "../../Util/CommonEnumerations";


export class ChecklistHeadRecordCompactDTO implements IChecklistHeadRecordCompactDTO {
    checklistHeadId: number;
    checklistHeadName: string;
    checklistHeadRecordId: number;
    checklistRowRecords: ChecklistExtendedRowDTO[];
    created: Date;
    recordId: number;
    rowNr: number;
    signatures: IImagesDTO[];
    state: SoeEntityState;
    tempHeadId: System.IGuid;
    addAttachementsToEInvoice: boolean;
    // Extensions
    isRendered: boolean;
    hideSignatureIcon: boolean;
}

export class ChecklistExtendedRowDTO implements IChecklistExtendedRowDTO {
    boolData: boolean;
    checkListMultipleChoiceAnswerHeadId: number;
    comment: string;
    created: Date;
    createdBy: string;
    dataTypeId: number;
    date: Date;
    dateString: string;
    dateData: Date;
    decimalData: number;
    description: string;
    guid: System.IGuid;
    headId: number;
    headRecordId: number;
    fileUploads: IFileUploadDTO[];
    intData: number;
    isHeadline: boolean;
    mandatory: boolean;
    modified: Date;
    modifiedBy: string;
    name: string;
    rowId: number;
    rowNr: number;
    rowRecordId: number;
    strData: string;
    text: string;
    type: TermGroup_ChecklistRowType;
    value: string;

    // Extensions
    isModified: boolean;
    isRendered: boolean;
    selectOption: any[];
    showEditIcon: boolean = true;
}
