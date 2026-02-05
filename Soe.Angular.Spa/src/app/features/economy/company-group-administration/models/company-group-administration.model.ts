import { SoeEntityState } from '@shared/models/generated-interfaces/Enumerations';
import { ICompanyGroupAdministrationDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class CompanyGroupAdministrationDTO
  implements ICompanyGroupAdministrationDTO
{
  companyGroupAdministrationId!: number;
  groupCompanyActorCompanyId!: number;
  childActorCompanyId!: number;
  childActorCompanyName!: string;
  childCompanyName!: string;
  childActorCompanyNr!: number;
  companyGroupMappingHeadId!: number;
  accountId!: number;
  conversionfactor?: number;
  note!: string;
  matchInternalAccountOnNr!: boolean;
  created?: Date;
  createdBy!: string;
  modified?: Date;
  modifiedBy!: string;
  state: SoeEntityState = 0;
}
