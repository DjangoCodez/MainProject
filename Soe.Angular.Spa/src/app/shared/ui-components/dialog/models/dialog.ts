import { Observable } from 'rxjs';

export type DialogSize = 'sm' | 'md' | 'lg' | 'xl' | 'fullscreen';

export interface DialogData<T = unknown> {
  size?: DialogSize;
  title: string;
  content?: string;
  primaryText?: string;
  secondaryText?: string;
  disableClose?: boolean;
  hideCloseButton?: boolean;
  disableContentScroll?: boolean;
  noToolbar?: boolean;
  hideFooter?: boolean;
  hasBackdrop?: boolean;
  maxHeight?: string;
  parameters?: T;
  callbackAction?: () => Observable<unknown> | unknown | Promise<unknown>;
}
