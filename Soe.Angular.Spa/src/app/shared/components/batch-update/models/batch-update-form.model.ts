import { ValidationHandler } from '@shared/handlers';
import { BatchUpdateDTO, PerformBatchUpdateModel } from './batch-update.model';
import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { FormArray, ValidationErrors, ValidatorFn } from '@angular/forms';
import { BatchUpdateFieldType } from '@shared/models/generated-interfaces/Enumerations';
import { SmallGenericForm } from '@shared/components/billing/import-dynamic/import-dynamic/import-dynamic.model';
import { SmallGenericType } from '@shared/models/generic-type.model';

interface IBatchUpdateCollection {
  validationHandler: ValidationHandler;
  element?: PerformBatchUpdateModel;
}

interface IBatchUpdateForm {
  validationHandler: ValidationHandler;
  element?: BatchUpdateDTO;
}

export class BatchUpdateForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;
  constructor({ validationHandler, element }: IBatchUpdateForm) {
    super(validationHandler, {
      field: new SoeTextFormControl(element?.field || 0),
      label: new SoeTextFormControl(element?.label || ''),
      fieldType: new SoeTextFormControl(
        element?.dataType || BatchUpdateFieldType.Unknown
      ),
      doShowFilter: new SoeCheckboxFormControl(element?.doShowFilter || false),
      doShowFromDate: new SoeCheckboxFormControl(
        element?.doShowFromDate || false
      ),
      doShowToDate: new SoeCheckboxFormControl(element?.doShowToDate || false),
      stringValue: new SoeTextFormControl(element?.stringValue || ''),
      boolValue: new SoeCheckboxFormControl(element?.boolValue || false),
      intValue: new SoeNumberFormControl(element?.intValue || undefined),
      decimalValue: new SoeNumberFormControl(
        element?.decimalValue || undefined,
        { decimals: 2 }
      ),
      dateValue: new SoeDateFormControl(element?.dateValue || undefined),
      fromDate: new SoeDateFormControl(element?.fromDate || undefined),
      toDate: new SoeDateFormControl(element?.toDate || undefined),
      options: new FormArray<SmallGenericForm>([]),
      children: new FormArray<BatchUpdateForm>([]),
      timeValue: new SoeTextFormControl(element?.timeValue || ''),
    });
    this.thisValidationHandler = validationHandler;
    this.patchOptions(element?.options ?? []);
    this.patchChildren(element?.children ?? []);
  }

  get field(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.field;
  }

  get label(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.label;
  }

  get fieldType(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.fieldType;
  }

  get doShowFilter(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.doShowFilter;
  }

  get doShowFromDate(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.doShowFromDate;
  }

  get doShowToDate(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.doShowToDate;
  }

  get stringValue(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.stringValue;
  }

  get boolValue(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.boolValue;
  }

  get intValue(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.intValue;
  }

  get decimalValue(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.decimalValue;
  }

  get dateValue(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.dateValue;
  }

  get fromDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.fromDate;
  }

  get toDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.toDate;
  }

  get options(): FormArray<SmallGenericForm> {
    return <FormArray<SmallGenericForm>>this.controls.options;
  }

  get children(): FormArray<BatchUpdateForm> {
    return <FormArray<BatchUpdateForm>>this.controls.children;
  }

  get timeValue(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.timeValue;
  }

  customPatchValue(model: BatchUpdateDTO): void {
    this.reset(model);
    this.patchOptions(model.options);
    this.patchChildren(model.children);
  }

  private patchOptions(options: SmallGenericType[]): void {
    this.options.clear({ emitEvent: false });
    options.forEach(o => {
      this.options.push(
        new SmallGenericForm({
          validationHandler: this.thisValidationHandler,
          element: o,
        }),
        { emitEvent: false }
      );
    });
    this.options.updateValueAndValidity();
  }

  private patchChildren(bChildren: BatchUpdateDTO[]): void {
    this.children.clear({ emitEvent: false });
    bChildren.forEach(c => {
      this.children.push(
        new BatchUpdateForm({
          validationHandler: this.thisValidationHandler,
          element: c,
        }),
        { emitEvent: false }
      );
    });
    this.children.updateValueAndValidity();
  }
}

export class BatchUpdateCollectionForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;
  constructor({ validationHandler, element }: IBatchUpdateCollection) {
    super(validationHandler, {
      selectedFieldId: new SoeSelectFormControl(element?.selectedFieldId || 0),
      entityType: new SoeTextFormControl(element?.entityType || undefined),
      ids: new SoeSelectFormControl(element?.ids || []),
      filterIds: new SoeSelectFormControl(element?.filterIds || []),
      batchUpdates: new FormArray<BatchUpdateForm>([]),
    });
    this.thisValidationHandler = validationHandler;
  }

  get selectedFieldId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.selectedFieldId;
  }

  get entityType(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.entityType;
  }

  get ids(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.ids;
  }

  get filterIds(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.filterIds;
  }

  get batchUpdates(): FormArray<BatchUpdateForm> {
    return <FormArray<BatchUpdateForm>>this.controls.batchUpdates;
  }

  clearBatchUpdates(): void {
    this.batchUpdates.clear();
  }

  addBatchUpdate(batchUpdate: BatchUpdateDTO): void {
    this.batchUpdates.push(
      new BatchUpdateForm({
        validationHandler: this.thisValidationHandler,
        element: batchUpdate,
      })
    );
    this.batchUpdates.at(this.batchUpdates.length - 1).fromDate.disable();
  }

  removeBatchUpdate(idx: number): void {
    this.batchUpdates.removeAt(idx);
  }
}

export function employeeFromDateValidator(errorTerm: string): ValidatorFn {
  return (_form): ValidationErrors | null => {
    if (_form) {
      const batchUpdates = <FormArray<BatchUpdateForm>>_form;
      for (const batchUpdate of batchUpdates.controls) {
        if (
          batchUpdate.doShowFromDate &&
          !(batchUpdate.fromDate.value || batchUpdate.fromDate.value !== '')
        ) {
          const error: ValidationErrors = {
            custom: { value: errorTerm },
          };
          return error;
        }
      }
    }
    return null;
  };
}
