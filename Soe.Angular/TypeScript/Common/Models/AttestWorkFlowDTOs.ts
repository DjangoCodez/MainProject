import { IAttestWorkFlowHeadDTO, IAttestWorkFlowRowDTO, IAttestWorkFlowTemplateHeadDTO, IAttestWorkFlowTemplateHeadGridDTO, IAttestWorkFlowTemplateRowDTO } from "../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../Util/CalendarUtility";
import { SoeEntityType, SoeEntityState, TermGroup_AttestWorkFlowType, TermGroup_AttestFlowRowState, TermGroup_AttestWorkFlowRowProcessType, TermGroup_AttestEntity } from "../../Util/CommonEnumerations";

export class AttestWorkFlowTemplateHeadDTO implements IAttestWorkFlowTemplateHeadDTO {
    actorCompanyId: number;
    attestEntity: TermGroup_AttestEntity;
    attestWorkFlowTemplateHeadId: number;
    created: Date;
    createdBy: string;
    description: string;
    modified: Date;
    modifiedBy: string;
    name: string;
    state: SoeEntityState;
    type: TermGroup_AttestWorkFlowType;
}

export class AttestWorkFlowTemplateHeadGridDTO implements IAttestWorkFlowTemplateHeadGridDTO {
    attestWorkFlowTemplateHeadId: number;
    description: string;
    name: string;
    type: TermGroup_AttestWorkFlowType;
}

export class AttestWorkFlowTemplateRowDTO implements IAttestWorkFlowTemplateRowDTO {
    attestStateFromName: string;
    attestStateToColor: string;
    attestStateToName: string;
    attestTransitionId: number;
    attestTransitionName: string;
    attestWorkFlowTemplateHeadId: number;
    attestWorkFlowTemplateRowId: number;
    closed: boolean;
    initial: boolean;
    sort: number;
    type: number;
    typeName: string;

    // Extensions
    checked: boolean;

    public get sortAndTransitionName(): string {
        return "{0}. {1}".format(this.sort.toString(), this.attestTransitionName);
    }

    public get sortAndAttestStateToName(): string {
        return "{0}. {1}".format(this.sort.toString(), this.attestStateToName);
    }
}

export class AttestWorkFlowHeadDTO implements IAttestWorkFlowHeadDTO {
    actorCompanyId: number;
    adminInformation: string;
    attestWorkFlowGroupId: number;
    attestWorkFlowHeadId: number;
    attestWorkFlowTemplateHeadId: number;
    created: Date;
    createdBy: string;
    entity: SoeEntityType;
    isAttestGroup: boolean;
    isDeleted: boolean;
    modified: Date;
    modifiedBy: string;
    name: string;
    recordId: number;
    rows: AttestWorkFlowRowDTO[];
    sendMessage: boolean;
    signInitial: boolean;
    state: SoeEntityState;
    templateName: string;
    type: TermGroup_AttestWorkFlowType;
    typeName: string;

    public setTypes() {
        if (this.rows) {
            this.rows = this.rows.map(x => {
                let obj = new AttestWorkFlowRowDTO();
                angular.extend(obj, x);
                obj.fixDates();
                return obj;
            });
        } else {
            this.rows = [];
        }
    }
}

export class AttestWorkFlowRowDTO implements IAttestWorkFlowRowDTO {
    answer: boolean;
    answerDate: Date;
    answerText: string;
    attestRoleId: number;
    attestRoleName: string;
    attestStateFromId: number;
    attestStateFromName: string;
    attestStateSort: number;
    attestStateToName: string;
    attestTransitionId: number;
    attestTransitionName: string;
    attestWorkFlowHeadId: number;
    attestWorkFlowRowId: number;
    comment: string;
    commentDate: Date;
    commentUser: string;
    created: Date;
    createdBy: string;
    isCurrentUser: boolean;
    isDeleted: boolean;
    loginName: string;
    modified: Date;
    modifiedBy: string;
    name: string;
    originateFromRowId: number;
    processType: TermGroup_AttestWorkFlowRowProcessType;
    processTypeName: string;
    processTypeSort: number;
    state: TermGroup_AttestFlowRowState;
    type: TermGroup_AttestWorkFlowType;
    typeName: string;
    userId: number;
    workFlowRowIdToReplace: number;

    public fixDates() {
        this.answerDate = CalendarUtility.convertToDate(this.answerDate);
        this.commentDate = CalendarUtility.convertToDate(this.commentDate);
        this.created = CalendarUtility.convertToDate(this.created);
        this.modified = CalendarUtility.convertToDate(this.modified);
    }

    public get isRegistered(): boolean {
        return this.processType === TermGroup_AttestWorkFlowRowProcessType.Registered;
    }

    public get isOpened(): boolean {
        return this.answerDate && this.answer === undefined;
    }

    public get isSigned(): boolean {
        return this.answerDate && this.answer === true;
    }

    public get isRejected(): boolean {
        return this.answerDate && this.answer === false;
    }
}