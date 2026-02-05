import { FilesLookupDTO } from '@shared/models/file.model';
import { DialogData } from '@ui/dialog/models/dialog';

export class ImportSelectionModel implements DialogData {
  size?: 'sm' | 'md' | 'lg' | 'xl' | 'fullscreen';
  title: string;
  content?: string;
  primaryText?: string;
  secondaryText?: string;
  disableClose?: boolean;
  disableContentScroll?: boolean;
  noToolbar?: boolean;
  hideFooter?: boolean;
  uploadedFiles?: FilesLookupDTO;
  callbackAction?:
    | (() => Promise<unknown>)
    | (() => unknown)
    | (() => Promise<unknown> | unknown);
  constructor() {
    this.title = 'common.connect.new_import';
  }
}
