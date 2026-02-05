import { IAttestStateDTO, IAttestStateSmallDTO } from "../../Scripts/TypeLite.Net4";
import { TermGroup_AttestEntity, SoeModule, SoeEntityState } from "../../Util/CommonEnumerations";

export class AttestStateDTO implements IAttestStateDTO {
    attestStateId: number;
    actorCompanyId: number;
    entity: TermGroup_AttestEntity;
    module: SoeModule;
    name: string;
    description: string;
    color: string;
    imageSource: string;
    sort: number;
    initial: boolean;
    closed: boolean;
    hidden: boolean;
    locked: boolean;
    created: Date;
    createdBy: string;
    modified: Date;
    modifiedBy: string;
    state: SoeEntityState;
    langId: number;
    entityName: string;
    constructor() {
    }
}

export class AttestStateSmallDTO implements IAttestStateSmallDTO {
    attestStateId: number;
    closed: boolean;
    color: string;
    description: string;
    imageSource: string;
    initial: boolean;
    name: string;
    sort: number;
}
