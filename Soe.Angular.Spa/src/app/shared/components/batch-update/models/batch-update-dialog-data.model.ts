import { SoeEntityType } from '@shared/models/generated-interfaces/Enumerations';
import {
  DialogData,
  DialogSize,
} from '../../../ui-components/dialog/models/dialog';
export class BatchUpdateDialogData implements DialogData {
  size?: DialogSize = 'lg';
  title!: string;
  content?: string;
  primaryText?: string;
  secondaryText?: string;
  disableClose?: boolean;
  disableContentScroll?: boolean;
  noToolbar?: boolean;
  hideFooter?: boolean;
  callbackAction?: () => unknown;

  //Extensions
  entityType!: SoeEntityType;
  selectedIds: number[] = [];
}
