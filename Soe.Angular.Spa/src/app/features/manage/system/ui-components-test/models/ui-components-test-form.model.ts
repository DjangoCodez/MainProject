import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeDateRangeFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeNumberRangeFormControl,
  SoeRadioFormControl,
  SoeSelectFormControl,
  SoeSwitchFormControl,
  SoeTextFormControl,
  SoeTimeFormControl,
  SoeTimeRangeFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { UiComponentsTestDTO } from './ui-components-test.model';
import { arrayToFormArray } from '@shared/util/form-util';
import { FormArray } from '@angular/forms';
import { EditableGridTestDataDTO } from '../components/grid-test-components/editable-grid.component';
import { EditableGridTestDataForm } from './editable-grid-test-data-form.model';

interface IUiComponentsTestForm {
  validationHandler: ValidationHandler;
  element: UiComponentsTestDTO | undefined;
}
export class UiComponentsTestForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;

  constructor({ validationHandler, element }: IUiComponentsTestForm) {
    super(validationHandler, {
      id: new SoeTextFormControl(element?.id || 0, {
        isIdField: true,
      }),
      color: new SoeTextFormControl(element?.color || '', {}, 'ColorPicker'),
      check: new SoeCheckboxFormControl(element?.check || false),
      date: new SoeDateFormControl(
        element?.date || new Date(),
        { required: true },
        'DatePicker'
      ),
      daterange: new SoeDateRangeFormControl(
        element?.daterange || [undefined, undefined],
        {},
        'DateRangePicker'
      ),
      daterange2: new SoeDateRangeFormControl(
        element?.daterange2 || [undefined, undefined],
        {},
        'DateRangePicker'
      ),
      daterange3: new SoeDateRangeFormControl(
        element?.daterange3 || [undefined, undefined],
        {},
        'DateRangePicker'
      ),
      daterange4: new SoeDateRangeFormControl(
        element?.daterange4 || [undefined, undefined],
        {},
        'DateRangePicker'
      ),
      onlyCsvFiles: new SoeCheckboxFormControl(
        element?.onlyCsvFiles || false,
        {},
        'Only .csv & .txt files'
      ),
      filename: new SoeTextFormControl(
        element?.filename || '',
        {},
        'FileUpload'
      ),
      menuSelectId: new SoeSelectFormControl(
        element?.menuSelectId || undefined
      ),
      menu2SelectId: new SoeSelectFormControl(
        element?.menu2SelectId || undefined
      ),
      sliderShowActiveTrack: new SoeCheckboxFormControl(
        element?.sliderShowActiveTrack || false
      ),
      sliderShowTicks: new SoeCheckboxFormControl(
        element?.sliderShowTicks || true
      ),
      sliderShowThumbLabel: new SoeCheckboxFormControl(
        element?.sliderShowThumbLabel || true
      ),
      sliderShowMinMax: new SoeCheckboxFormControl(
        element?.sliderShowMinMax || false
      ),
      sliderDisabled: new SoeCheckboxFormControl(
        element?.sliderDisabled || false
      ),
      splitSelectId: new SoeSelectFormControl(
        element?.splitSelectId || undefined
      ),
      num: new SoeNumberFormControl(
        element?.num || 0,
        { maxDecimals: 2 },
        'NumberBox'
      ),
      num2: new SoeNumberFormControl(
        element?.num2 || 0,
        { maxDecimals: 0 },
        'NumberBox'
      ),
      numberrange: new SoeNumberRangeFormControl(
        element?.numberrange || [undefined, undefined],
        {},
        'NumberRange'
      ),
      radio: new SoeRadioFormControl<string>(
        element?.radio || '',
        {},
        'RadioButton'
      ),
      radio2: new SoeRadioFormControl<string>(
        element?.radio2 || '',
        {},
        'RadioButton 2'
      ),
      radio3: new SoeRadioFormControl<string>(
        element?.radio3 || '',
        {},
        'RadioButton 3'
      ),
      radio4: new SoeRadioFormControl<string>(
        element?.radio4 || '',
        {},
        'RadioButton 4'
      ),
      selectId: new SoeSelectFormControl(element?.selectId || undefined),
      multiSelectIds: new SoeSelectFormControl(element?.multiSelectIds || []),
      multiSelectIds2: new SoeSelectFormControl(element?.multiSelectIds2 || []),
      multiSelectIds3: new SoeSelectFormControl(element?.multiSelectIds3 || []),
      multiSelectIds4: new SoeSelectFormControl(element?.multiSelectIds4 || []),
      multiSelectIds5: new SoeSelectFormControl(element?.multiSelectIds5 || []),
      multiSelectIds6: new SoeSelectFormControl(element?.multiSelectIds6 || []),
      multiSelectIds7: new SoeSelectFormControl(element?.multiSelectIds7 || []),
      swtch: new SoeSwitchFormControl(element?.swtch || false),
      text: new SoeTextFormControl(
        element?.text || '',
        { isNameField: true, required: true },
        'TextBox'
      ),
      text2: new SoeTextFormControl(
        element?.text2 || '',
        {},
        'TextBox with button'
      ),
      text3: new SoeTextFormControl(
        element?.text3 || '',
        {},
        'TextBox with password'
      ),
      textarea: new SoeTextFormControl(
        element?.textarea || '',
        { maxLength: 1000 },
        'TextArea'
      ),
      textedit: new SoeTextFormControl(
        element?.textedit || '',
        {},
        'TextEditor'
      ),
      time: new SoeDateFormControl(element?.time || new Date(), {}, 'TimeBox'),
      timerange: new SoeTimeRangeFormControl(
        element?.timerange || [undefined, undefined],
        {},
        'TimeRange'
      ),
      timerange2: new SoeTimeRangeFormControl(
        element?.timerange2 || [undefined, undefined],
        {},
        'TimeRange 2'
      ),
      duration: new SoeTextFormControl(element?.duration || 0, {}, 'TimeBox'),
      autocompleteId: new SoeSelectFormControl(
        element?.autocompleteId || undefined,
        {},
        'Autocomplete'
      ),
      editableGridRows: arrayToFormArray(element?.rows || []),
    });
    this.thisValidationHandler = validationHandler;
  }

  get daterange(): SoeDateRangeFormControl {
    return <SoeDateRangeFormControl>this.controls.daterange;
  }

  get numberrange(): SoeNumberRangeFormControl {
    return <SoeNumberRangeFormControl>this.controls.numberrange;
  }

  get editableGridRows(): FormArray {
    return <FormArray>this.controls.editableGridRows;
  }

  get timerange(): SoeTimeFormControl {
    return <SoeTimeFormControl>this.controls.timerange;
  }

  get timerange2(): SoeTimeFormControl {
    return <SoeTimeFormControl>this.controls.timerange2;
  }

  customPatchValue(element: UiComponentsTestDTO) {
    this.patchValue(element);
    this.patchRows(element.rows || []);
  }

  patchRows(rows: EditableGridTestDataDTO[]) {
    this.editableGridRows.clear({ emitEvent: false });
    rows.forEach(r => {
      const rowsForm = new EditableGridTestDataForm({
        validationHandler: this.thisValidationHandler,
        element: r,
      });
      rowsForm.patchValue(r);

      this.editableGridRows.push(rowsForm, { emitEvent: false });
    });
    this.editableGridRows.markAsUntouched({ onlySelf: true });
    this.editableGridRows.markAsPristine({ onlySelf: true });
    this.editableGridRows.updateValueAndValidity();
  }

  addNewRow(row: EditableGridTestDataDTO) {
    const rowsForm = new EditableGridTestDataForm({
      validationHandler: this.thisValidationHandler,
      element: row,
    });
    rowsForm.patchValue(row);
    this.editableGridRows.push(rowsForm);
  }
}
