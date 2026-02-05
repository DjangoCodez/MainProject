import {
  SoeEntityState,
  SoeEntityType,
  TermGroup_AttestWorkFlowType,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IAttestGroupDTO,
  IAttestGroupGridDTO,
  IAttestWorkFlowHeadDTO,
  IAttestWorkFlowRowDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class AttestGroupGridDTO implements IAttestGroupGridDTO {
  attestWorkFlowHeadId!: number;
  attestWorkFlowTemplateHeadId!: number;
  code!: string;
  name!: string;
  attestGroupName!: string;
}

export class AttestWorkFlowHeadDTO
  implements IAttestWorkFlowHeadDTO, IAttestGroupDTO
{
  attestWorkFlowHeadId!: number;
  attestWorkFlowTemplateHeadId!: number;
  attestGroupCode!: string;
  attestGroupName!: string;
  isAttestGroup!: boolean;
  actorCompanyId: number = 0;
  type!: TermGroup_AttestWorkFlowType;
  entity!: SoeEntityType;
  recordId!: number;
  name!: string;
  sendMessage!: boolean;
  adminInformation!: string;
  created!: Date;
  createdBy!: string;
  modified!: Date;
  modifiedBy!: string;
  state!: SoeEntityState;
  isDeleted!: boolean;
  typeName!: string;
  templateName!: string;
  rows!: IAttestWorkFlowRowDTO[];
  attestWorkFlowGroupId!: number;
  signInitial!: boolean;
}
