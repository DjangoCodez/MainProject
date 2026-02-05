import { FormArray } from '@angular/forms';
import {
  SoeFormGroup,
  SoeNumberFormControl,
  SoeTextFormControl,
  SoeSelectFormControl,
  SoeCheckboxFormControl,
  SoeDateFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import {
  IExtraFieldRecordDTO,
  IExtraFieldValueDTO,
} from '@shared/models/generated-interfaces/ExtraFieldDTO';
import { ExtraFieldValueForm } from './extra-fields-form.model';

interface IExtraFieldsInputForm {
  validationHandler: ValidationHandler;
  elements: IExtraFieldRecordDTO[];
  readOnly: boolean;
}

export class ExtraFieldsInputForm extends SoeFormGroup {
  get rows() {
    return <FormArray<ExtraFieldsInputRowForm>>this.controls.rows;
  }

  constructor({
    validationHandler,
    elements,
    readOnly,
  }: IExtraFieldsInputForm) {
    super(validationHandler, {
      rows: new FormArray<ExtraFieldsInputRowForm>(
        elements.map(
          element => new ExtraFieldsInputRowForm({ validationHandler, element })
        )
      ),
    });

    if (readOnly) {
      this.disable();
    }
  }

  public disableControlsByReadOnly(readOnly: boolean) {
    if (readOnly) {
      this.disable();
    } else {
      this.enable();
    }
  }
}

interface IExtraFieldsInputRowForm {
  validationHandler: ValidationHandler;
  element: IExtraFieldRecordDTO;
}

export class ExtraFieldsInputRowForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;

  constructor({ validationHandler, element }: IExtraFieldsInputRowForm) {
    super(validationHandler, {
      extraFieldRecordId: new SoeNumberFormControl(element.extraFieldRecordId),
      extraFieldId: new SoeNumberFormControl(element.extraFieldId),
      extraFieldText: new SoeTextFormControl(element.extraFieldText),
      extraFieldType: new SoeSelectFormControl(element.extraFieldType),
      comment: new SoeTextFormControl(element.comment),
      dataTypeId: new SoeNumberFormControl(element.dataTypeId),
      strData: new SoeTextFormControl(element.strData),
      intData: new SoeNumberFormControl(element.intData, {
        decimals: 0,
        maxDecimals: 0,
      }),
      boolData: new SoeCheckboxFormControl(element.boolData),
      decimalData: new SoeNumberFormControl(element.decimalData, {
        decimals: 4,
        maxDecimals: 4,
      }),
      dateData: new SoeDateFormControl(element.dateData),
      recordId: new SoeNumberFormControl(element.recordId),
      value: new SoeTextFormControl(element.value),

      extraFieldValues: new FormArray<ExtraFieldValueForm>([]),
    });

    this.thisValidationHandler = validationHandler;
    this.patchExtraFieldValues(element?.extraFieldValues ?? []);
  }

  get extraFieldValues(): FormArray<ExtraFieldValueForm> {
    return <FormArray>this.controls.extraFieldValues;
  }

  patchExtraFieldValues(rows: IExtraFieldValueDTO[]) {
    this.extraFieldValues?.clear();

    for (const row of rows) {
      const formRow = new ExtraFieldValueForm({
        validationHandler: this.thisValidationHandler,
        element: row,
      });
      this.extraFieldValues.push(formRow, { emitEvent: false });
    }
    this.extraFieldValues.updateValueAndValidity();
  }
}
