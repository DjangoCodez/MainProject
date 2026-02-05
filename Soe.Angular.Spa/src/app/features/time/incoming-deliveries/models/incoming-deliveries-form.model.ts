import { FormArray, FormControl } from '@angular/forms';
import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import {
  IIncomingDeliveryHeadDTO,
  IIncomingDeliveryRowDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { arrayToFormArray, clearAndSetFormArray } from '@shared/util/form-util';
import { IncomingDeliveriesRowsForm } from './incoming-deliveries-rows-form.model';

interface IIncomingDeliveriesForm {
  validationHandler: ValidationHandler;
  element: IIncomingDeliveryHeadDTO | undefined;
}
export class IncomingDeliveriesForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;

  constructor({ validationHandler, element }: IIncomingDeliveriesForm) {
    super(validationHandler, {
      incomingDeliveryHeadId: new SoeTextFormControl(
        element?.incomingDeliveryHeadId || 0,
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
        { maxLength: 512 },
        'common.description'
      ),
      accountId: new SoeSelectFormControl(element?.accountId),
      rows: arrayToFormArray(element?.rows || []),

      // Recurrence
      startDate: new SoeDateFormControl(element?.startDate),
      stopDate: new SoeDateFormControl(element?.stopDate),
      nbrOfOccurrences: new SoeNumberFormControl(element?.nbrOfOccurrences),
      recurrencePattern: new SoeTextFormControl(element?.recurrencePattern),
      recurrencePatternDescription: new SoeTextFormControl(
        element?.recurrencePatternDescription
      ),
      recurrenceStartsOnDescription: new SoeTextFormControl(
        element?.recurrenceStartsOnDescription
      ),
      recurrenceEndsOnDescription: new SoeTextFormControl(
        element?.recurrenceEndsOnDescription
      ),
      excludedDates: arrayToFormArray(element?.excludedDates || []),
      excludedDatesDescription: new SoeTextFormControl(''),

      // Extentions
      isCopy: new SoeCheckboxFormControl(false),
    });

    this.thisValidationHandler = validationHandler;
  }

  get rows(): FormArray<IncomingDeliveriesRowsForm> {
    return <FormArray>this.controls.rows;
  }

  get recurrencePatternDescription(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.recurrencePatternDescription;
  }

  get excludedDates(): FormArray<FormControl<Date>> {
    return <FormArray>this.controls.excludedDates;
  }

  get excludedDatesDescription(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.excludedDatesDescription;
  }

  get hasRecurrencePattern(): boolean {
    return (
      this.controls.recurrencePattern.value &&
      this.controls.recurrencePattern.value !== '______'
    );
  }

  customPatchValue(element: IIncomingDeliveryHeadDTO) {
    this.patchValue(element);
    this.customExcludedDatesPatchValue(element.excludedDates);
    this.patchRows(element.rows);
  }

  customExcludedDatesPatchValue(excludedDates: Date[]) {
    clearAndSetFormArray(excludedDates, this.excludedDates);
  }

  patchRows(rows: IIncomingDeliveryRowDTO[]) {
    this.rows.clear({ emitEvent: false });
    rows.forEach(r => {
      const rowsForm = new IncomingDeliveriesRowsForm({
        validationHandler: this.thisValidationHandler,
        element: r,
      });
      rowsForm.customPatchValue(r, false);

      this.rows.push(rowsForm, { emitEvent: false });
    });
    this.rows.markAsUntouched({ onlySelf: true });
    this.rows.markAsPristine({ onlySelf: true });
    this.rows.updateValueAndValidity();
  }

  onDoCopy() {
    this.controls.isCopy.patchValue(true);

    // Remember the rows
    const rows = this.rows.value;

    // Clear the form, we need to create new rows with correct form
    this.rows.clear({ emitEvent: false });

    rows.forEach(row => {
      row.incomingDeliveryRowId = 0;

      // Create new form for each row
      const rowsForm = new IncomingDeliveriesRowsForm({
        validationHandler: this.thisValidationHandler,
        element: undefined,
      });
      rowsForm.patchValue(row);

      this.rows.push(rowsForm, { emitEvent: false });
    });
  }
}
