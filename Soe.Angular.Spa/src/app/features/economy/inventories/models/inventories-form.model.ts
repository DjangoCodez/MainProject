import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { InventoryDTO } from './inventories.model';
import { FormArray, FormControl } from '@angular/forms';
import { IInventoryDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { arrayToFormArray } from '@shared/util/form-util';
import { AccountingSettingsFormArray } from '@shared/components/accounting-settings/accounting-settings/accounting-settings-form.model';
import { TermGroup_InventoryStatus } from '@shared/models/generated-interfaces/Enumerations';

interface IInventoriesForm {
  validationHandler: ValidationHandler;
  element: InventoryDTO | undefined;
}
export class InventoriesForm extends SoeFormGroup {
  inventoryValidator: ValidationHandler;
  constructor({ validationHandler, element }: IInventoriesForm) {
    super(validationHandler, {
      inventoryId: new SoeTextFormControl(element?.inventoryId || 0, {
        isIdField: true,
      }),
      inventoryNr: new SoeTextFormControl(
        element?.inventoryNr || '',
        { required: true },
        'economy.inventory.inventories.inventorynr'
      ),
      name: new SoeTextFormControl(
        element?.name || '',
        {
          isNameField: true,
          required: true,
        },
        'common.name'
      ),
      inventoryStatus: new SoeNumberFormControl(
        element?.status || TermGroup_InventoryStatus.Draft
      ),
      statusName: new SoeTextFormControl(element?.statusName || '', { disabled: true}),
      parentId: new SoeSelectFormControl(element?.parentId),
      writeOffAmount: new SoeNumberFormControl(element?.writeOffAmount || 0.0, {
        minDecimals: 0,
        maxDecimals: 2,
        disabled: true,
      }),
      writeOffRemainingAmount: new SoeNumberFormControl(
        element?.writeOffRemainingAmount || 0.0,
        { minDecimals: 0, maxDecimals: 2, disabled: true }
      ),
      accWriteOffAmount: new SoeNumberFormControl(
        element?.accWriteOffAmount || 0.0,
        { minDecimals: 0, maxDecimals: 2, disabled: true }
      ),
      purchaseDate: new SoeDateFormControl(
        element?.purchaseDate || null,
        {
          required: true,
        },
        'economy.inventory.inventories.purchasedate'
      ),
      writeOffDate: new SoeDateFormControl(
        element?.writeOffDate || null,
        {
          required: true,
        },
        'economy.inventory.inventories.writeoffdate'
      ),
      purchaseAmount: new SoeNumberFormControl(element?.purchaseAmount || 0.0, {
        minDecimals: 0,
        maxDecimals: 2,
      }),
      writeOffSum: new SoeNumberFormControl(element?.writeOffSum || 0.0, {
        minDecimals: 0,
        maxDecimals: 2,
      }),
      writeOffPeriods: new SoeTextFormControl(element?.writeOffPeriods || ''),
      endAmount: new SoeNumberFormControl(element?.endAmount || 0.0, {
        minDecimals: 0,
        maxDecimals: 2,
      }),
      description: new SoeTextFormControl(element?.description || ''),
      accountingSettings: new AccountingSettingsFormArray(validationHandler),
      categoryIds: arrayToFormArray(element?.categoryIds || []),
      showPreliminary: new SoeCheckboxFormControl(
        element?.showPreliminary || true
      ),

      //Writeoff
      notes: new SoeTextFormControl(element?.notes || ''),
      writeOffTemplateId: new SoeSelectFormControl(0),
      voucherSeriesTypeId: new SoeSelectFormControl(
        element?.voucherSeriesTypeId,
        {
          required: true,
        },
        'economy.inventory.inventories.voucherseriestype'
      ),
      inventoryWriteOffMethodId: new SoeSelectFormControl(
        element?.inventoryWriteOffMethodId,
        {
          required: true,
        },
        'economy.inventory.inventories.writeoffmethod'
      ),
      periodType: new SoeSelectFormControl(element?.periodType, { disabled: true }),
      periodValue: new SoeNumberFormControl(element?.periodValue || 0, { disabled: true }),
      info: new SoeTextFormControl(element?.info || '', { disabled: true }),

      supplierInvoiceId: new SoeSelectFormControl(
        element?.supplierInvoiceId || undefined
      ),
      customerInvoiceId: new SoeSelectFormControl(
        element?.customerInvoiceId || undefined
      ),
    });
    this.inventoryValidator = validationHandler;
    this.accountingSettings.rawPatch(element?.accountingSettings ?? []);
  }

  get showPreliminary(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.showPreliminary;
  }

  get parentId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.parentId;
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }

  get inventoryId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.inventoryId;
  }

  get inventoryNr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.inventoryNr;
  }

  get inventoryStatus(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.inventoryStatus;
  }

  get statusName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.statusName;
  }

  get accWriteOffAmount(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.accWriteOffAmount;
  }

  get writeOffRemainingAmount(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.writeOffRemainingAmount;
  }

  get writeOffPeriods(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.writeOffPeriods;
  }

  get purchaseAmount(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.purchaseAmount;
  }

  get writeOffSum(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.writeOffSum;
  }

  get writeOffAmount(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.writeOffAmount;
  }

  get purchaseDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.purchaseDate;
  }

  get writeOffDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.writeOffDate;
  }

  get endAmount(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.endAmount;
  }

  get description(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.description;
  }

  get accountingSettings() {
    return <AccountingSettingsFormArray>this.controls.accountingSettings;
  }

  get writeOffTemplateId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.writeOffTemplateId;
  }

  get voucherSeriesTypeId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.voucherSeriesTypeId;
  }

  get inventoryWriteOffMethodId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.inventoryWriteOffMethodId;
  }

  get periodType(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.periodType;
  }

  get periodValue(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.periodValue;
  }

  get info(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.info;
  }

  get supplierInvoiceId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.supplierInvoiceId;
  }

  get customerInvoiceId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.customerInvoiceId;
  }

  get categoryIds(): FormArray<FormControl<number>> {
    return <FormArray>this.controls.categoryIds;
  }

  customPatchValue(element: IInventoryDTO) {
    this.patchValue(element);
    this.patchCategories(element.categoryIds ?? []);
    this.accountingSettings.rawPatch(element.accountingSettings);
  }

  public patchCategories(categoryIds: number[]): void {
    this.categoryIds.clear({ emitEvent: false });
    arrayToFormArray(categoryIds).controls.forEach(f => {
      this.categoryIds.push(<FormControl<number>>f, { emitEvent: false });
    });
    this.categoryIds.updateValueAndValidity();
  }
}
