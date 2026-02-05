import { TermGroup_SignatoryContractPermissionType } from '@shared/models/generated-interfaces/Enumerations';
import { DialogData, DialogSize } from '@ui/dialog/models/dialog';

export class SignatoryContractAuthDialogData implements DialogData {
  title!: string;
  content?: string;
  primaryText?: string;
  secondaryText?: string;
  size?: DialogSize;

  permissionType: TermGroup_SignatoryContractPermissionType =
    TermGroup_SignatoryContractPermissionType.Unknown;
    signatoryContractId?: number;
}
