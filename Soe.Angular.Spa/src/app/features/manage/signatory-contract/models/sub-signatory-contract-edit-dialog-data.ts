import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { ISignatoryContractDTO } from '@shared/models/generated-interfaces/SignatoryContractDTO';
import { DialogData } from '@ui/dialog/models/dialog';
export interface ISubSignatoryContractEditDialogData extends DialogData {
  rowToUpdate: ISignatoryContractDTO;
  users: ISmallGenericType[];
  permissionTerms: ISmallGenericType[];
}
