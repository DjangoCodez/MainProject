import { SoeEntityState, TermGroup_IOType, TermGroup_IOStatus, TermGroup_IOSource, TermGroup_IOImportHeadType } from "../../Util/CommonEnumerations";

export class ProjectIODTO {

    isSelected: boolean;
    statusName: string;
    batchId: string;
    errorMessage: string;
    projectId: number;
    projectNr: string;
    parentProjectNr: string;
    name: string;
    startDate: Date;
    stopDate: Date;
    accountNr: string;
    accountDim2Nr: string;
    accountDim3Nr: string;
    accountDim4Nr: string;
    accountDim5Nr: string;
    accountDim6Nr: string;
    actorCompanyId: number;
    allocationType: number;
    bookAccordingToThisProject: boolean;
    categoryCode1: string;
    categoryCode2: string;
    categoryCode3: string;
    categoryCode4: string;
    categoryCode5: string;
    categoryCode6: string;
    categoryCode7: string;
    categoryCode8: string;
    categoryCode9: string;
    categoryIds: number[];
    customerNr: string;
    description: string;
    import: boolean;
    note: string;
    participantEmployeeNr1: string;
    participantEmployeeNr2: string;
    participantEmployeeNr3: string;
    participantEmployeeNr4: string;
    participantEmployeeNr5: string;
    participantEmployeeNr6: string;
    projectIOId: number;
    type: TermGroup_IOType;
    status: TermGroup_IOStatus;
    source: TermGroup_IOSource;
    state: SoeEntityState;
    created: Date;
    createdBy: string;
    modified: Date;
    modifiedBy: string;
    isModified: boolean;
   
}