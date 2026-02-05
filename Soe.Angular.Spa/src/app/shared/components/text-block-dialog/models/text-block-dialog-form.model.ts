import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { TextblockDTO } from './text-block-dialog.model';

interface ITextBlockDialogForm {
  validationHandler: ValidationHandler;
  element: TextblockDTO | undefined;
}

export class TextBlockDialogForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ITextBlockDialogForm) {
    super(validationHandler, {
      textBlockId: new SoeSelectFormControl(
        element?.textblockId || 0,
        {
          isIdField: true,
        },
        'economy.accounting.voucher.template'
      ),
      headline: new SoeTextFormControl(
        element?.headline || undefined,
        {},
        'common.name'
      ),
      text: new SoeTextFormControl(
        element?.text || undefined,
        {},
        'common.text'
      ),
      type: new SoeTextFormControl(element?.type || undefined),
      created: new SoeDateFormControl(element?.created || undefined),
      createdBy: new SoeTextFormControl(element?.createdBy || undefined),
      modified: new SoeDateFormControl(element?.modified || undefined),
      modifiedBy: new SoeTextFormControl(element?.modifiedBy || undefined),
      actorCompanyId: new SoeNumberFormControl(element?.actorCompanyId || 0),
      isModified: new SoeTextFormControl(element?.isModified || false),
      showInContract: new SoeCheckboxFormControl(
        element?.showInContract || false
      ),
      showInOffer: new SoeCheckboxFormControl(element?.showInOffer || false),
      showInOrder: new SoeCheckboxFormControl(element?.showInOrder || false),
      showInInvoice: new SoeCheckboxFormControl(
        element?.showInInvoice || false
      ),
      showInPurchase: new SoeCheckboxFormControl(
        element?.showInPurchase || false
      ),
    });
  }

  get textBlockId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.textBlockId;
  }
  get headline(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.headline;
  }

  get text(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.text;
  }
  get type(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.type;
  }
  get created(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.created;
  }
  get createdBy(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.createdBy;
  }
  get modified(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.modified;
  }
  get modifiedBy(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.modifiedBy;
  }
  get actorCompanyId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.actorCompanyId;
  }
  get isModified(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.isModified;
  }
  get showInContract(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.showInContract;
  }
  get showInOffer(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.showInOffer;
  }
  get showInOrder(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.showInOrder;
  }
  get showInInvoice(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.showInInvoice;
  }
  get showInPurchase(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.showInPurchase;
  }
}
