import { SoeCheckboxFormControl, SoeFormGroup } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { DialogData, DialogSize } from '@ui/dialog/models/dialog';

export class CopyProductDialogData implements DialogData {
  size?: DialogSize = 'md';
  title: string = '';
  content?: string;
  primaryText?: string;
  secondaryText?: string;
  disableClose?: boolean;
  disableContentScroll?: boolean;
  noToolbar?: boolean;
  hideFooter?: boolean;
  callbackAction?: () => unknown;

  //Extensions
  copyProductSetting?: CopyProductSettingsDTO;
}

export class CopyProductSettingsDTO {
  copyPrice: boolean;
  copyAccounts: boolean;
  copyStock: boolean;

  constructor() {
    this.copyAccounts = this.copyPrice = this.copyStock = true;
  }
}

interface ICopyProductSettingsForm {
  validationHandler: ValidationHandler;
  element?: CopyProductSettingsDTO;
}

export class CopyProductSettingsForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ICopyProductSettingsForm) {
    super(validationHandler, {
      copyPrice: new SoeCheckboxFormControl(element?.copyPrice || true),
      copyAccounts: new SoeCheckboxFormControl(element?.copyAccounts || true),
      copyStock: new SoeCheckboxFormControl(element?.copyStock || true),
    });
  }
}
