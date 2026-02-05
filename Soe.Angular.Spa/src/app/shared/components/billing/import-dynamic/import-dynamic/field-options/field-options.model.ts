import { IImportFieldDTO } from '@shared/models/generated-interfaces/ImportDynamicDTO';
import { DialogData, DialogSize } from '@ui/dialog/models/dialog';

interface IFieldOptionsData extends DialogData {
  field?: IImportFieldDTO;
  uniqueValues?: string[];
}

export class FieldOptionsData implements IFieldOptionsData {
  field?: IImportFieldDTO | undefined;
  uniqueValues?: string[] = [];
  size?: DialogSize | undefined;
  title: string = '';
  content?: string | undefined;
  primaryText?: string | undefined;
  secondaryText?: string | undefined;
  disableClose?: boolean | undefined;
  disableContentScroll?: boolean | undefined;
  noToolbar?: boolean | undefined;
  hideFooter?: boolean | undefined;
  callbackAction?: (() => unknown) | undefined;
}
