import {
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { InventoryWriteOffTemplatesDTO } from './inventory-write-off-templates.model';
import { AccountingSettingsFormArray } from '@shared/components/accounting-settings/accounting-settings/accounting-settings-form.model';

interface IInventoryWriteOffTemplateForm {
  validationHandler: ValidationHandler;
  element: InventoryWriteOffTemplatesDTO | undefined;
}

export class InventoryWriteOffTemplateForm extends SoeFormGroup {
  writeOffTemplateValidationHandler: ValidationHandler;

  constructor({ validationHandler, element }: IInventoryWriteOffTemplateForm) {
    super(validationHandler, {
      inventoryWriteOffTemplateId: new SoeTextFormControl(
        element?.inventoryWriteOffTemplateId || 0,
        {
          isIdField: true,
        }
      ),
      name: new SoeTextFormControl(
        element?.name || '',
        { isNameField: true, required: true, maxLength: 100, minLength: 1 },
        'common.name'
      ),
      description: new SoeTextFormControl(
        element?.description || '',
        { maxLength: 255 },
        'common.description'
      ),
      inventoryWriteOffMethodId: new SoeSelectFormControl(
        element?.inventoryWriteOffMethodId || undefined,
        { required: true },
        'economy.inventory.inventorywriteofftemplate.writeoffmethod'
      ),
      voucherSeriesTypeId: new SoeSelectFormControl(
        element?.voucherSeriesTypeId || undefined,
        { required: true },
        'economy.inventory.inventorywriteofftemplate.voucherserie'
      ),
      accountingSettings: new AccountingSettingsFormArray(validationHandler),
    });
    this.writeOffTemplateValidationHandler = validationHandler;
    this.accountingSettings.patch(element?.accountingSettings ?? []);
  }

  get inventoryWriteOffTemplateId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.inventoryWriteOffTemplateId;
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }

  get description(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.description;
  }

  get inventoryWriteOffMethodId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.inventoryWriteOffMethodId;
  }

  get voucherSeriesTypeId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.voucherSeriesTypeId;
  }

  get accountingSettings() {
    return <AccountingSettingsFormArray>this.controls.accountingSettings;
  }

  customPatch(template: InventoryWriteOffTemplatesDTO): void {
    this.reset(template);
    this.accountingSettings.patch(template.accountingSettings);
  }
}
