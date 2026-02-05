import {
  SoeFormGroup,
  SoeTextFormControl,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeCheckboxFormControl,
  SoeFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { InventoryWriteOffMethodDTO } from './inventory-write-off-method.model';

interface IInventoryWriteOffMethodForm {
  validationHandler: ValidationHandler;
  element: InventoryWriteOffMethodDTO | undefined;
}

export class InventoryWriteOffMethodForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IInventoryWriteOffMethodForm) {
    super(validationHandler, {
      inventoryWriteOffMethodId: new SoeTextFormControl(
        element?.inventoryWriteOffMethodId || 0,
        {
          isIdField: true,
        }
      ),
      name: new SoeTextFormControl(
        element?.name || '',
        { isNameField: true, required: true, maxLength: 100 },
        'common.name'
      ),
      description: new SoeTextFormControl(
        element?.description || '',
        { maxLength: 255 },
        'common.description'
      ),
      type: new SoeSelectFormControl(
        element?.type || 1,
        { required: true },
        'economy.inventory.inventorywriteoffmethod.type'
      ),
      periodType: new SoeSelectFormControl(
        element?.periodType || undefined,
        { required: true },
        'economy.inventory.inventorywriteoffmethod.periodtype'
      ),
      periodValue: new SoeNumberFormControl(
        element?.periodValue || 0,
        { minValue: 0 },
        'economy.inventory.inventorywriteoffmethod.periodvalue'
      ),
      yearPercent: new SoeNumberFormControl(
        element?.periodValue || 30,
        { minValue: 0 },
        'economy.inventory.inventorywriteoffmethod.yearpercent'
      ),
      hasAcitveWirteOffs: new SoeCheckboxFormControl(
        element?.hasAcitveWirteOffs || false
      ),
    });
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }

  get type(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.type;
  }

  get periodType(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.periodType;
  }

  get periodValue(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.periodValue;
  }

  get yearPercent(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.yearPercent;
  }

  get hasAcitveWirteOffs(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.hasAcitveWirteOffs;
  }

  enableFormControls(controls: Array<SoeFormControl>): void {
    controls.forEach(control => {
      control.enable();
    });
  }

  disableFormControls(controls: Array<SoeFormControl>): void {
    controls.forEach(control => {
      control.disable();
    });
  }
}
