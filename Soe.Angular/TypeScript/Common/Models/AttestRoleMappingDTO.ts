import { IAttestRoleMappingDTO } from "../../Scripts/TypeLite.Net4";
import { TermGroup_AttestEntity, SoeEntityState } from "../../Util/CommonEnumerations";

export class AttestRoleMappingDTO implements IAttestRoleMappingDTO {
	attestRoleMappingId: number;
	childtAttestRoleId: number;
	childtAttestRoleName: string;
	created: Date;
	createdBy: string;
	dateFrom: Date;
	dateTo: Date;
	entity: TermGroup_AttestEntity;
	modified: Date;
	modifiedBy: string;
	parentAttestRoleId: number;
	parentAttestRoleName: string;
	state: SoeEntityState;
	selected: boolean;
    constructor() {
    }
}
