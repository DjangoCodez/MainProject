import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { IProjectUserDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { DialogData, DialogSize } from '@ui/dialog/models/dialog';
import { Observable } from 'rxjs';

export class ProjectPersonsDialogData implements DialogData {
  size?: DialogSize | undefined;
  title: string = '';
  content?: string | undefined;
  rowToUpdate?: IProjectUserDTO;
  primaryText?: string | undefined;
  secondaryText?: string | undefined;
  disableClose?: boolean | undefined;
  disableContentScroll?: boolean | undefined;
  noToolbar?: boolean | undefined;
  hideFooter?: boolean | undefined;
  userTypes!: ISmallGenericType[];
  users!: ISmallGenericType[];
  timeCodes!: ISmallGenericType[];
  calculatedCostPermission!: boolean;
  callbackAction?:
    | (() => Observable<unknown> | unknown | Promise<unknown>)
    | undefined;
}
