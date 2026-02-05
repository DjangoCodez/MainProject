import { SoeEntityState } from '@shared/models/generated-interfaces/Enumerations';
import { IUserSmallDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { DialogData, DialogSize } from '@ui/dialog/models/dialog';

export class UserSmallDTO implements IUserSmallDTO {
  licenseId!: number;
  licenseNr!: string;
  defaultActorCompanyId?: number;
  userId!: number;
  loginName!: string;
  name!: string;
  email!: string;
  langId?: number;
  blockedFromDate?: Date;
  idLoginActive!: boolean;
  state: SoeEntityState = 0;
  hideEditButton!: boolean;
  allowSupportLogin!: boolean;
  changePassword!: boolean;
  defaultRoleName!: string;
  isSelected!: boolean;
  isSelectedForEmail!: boolean;
  main!: boolean;
  attestFlowIsRequired!: boolean;
  attestFlowHasAnswered!: boolean;
  attestFlowRowId!: number;
  attestRoleId!: number;
  categories!: string;
}

export class SelectUsersDialogData implements DialogData {
  title!: string;
  content?: string;
  primaryText?: string;
  secondaryText?: string;
  size?: DialogSize;
  showParticipant!: boolean;
  showMain!: boolean;
  selectedUsers!: IUserSmallDTO[];
}
