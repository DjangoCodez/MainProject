import { DialogData, DialogSize } from '@ui/dialog/models/dialog';

export class LoadResultDialogData implements DialogData {
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

  dim2Name!: string;
  dim3Name!: string;
  useDim2!: boolean;
  useDim3!: boolean;
}

export class EditLoadResultDTO {
  useDim2: boolean;
  useDim3: boolean;

  constructor() {
    this.useDim2 = false;
    this.useDim3 = false;
  }
}
