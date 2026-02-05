import { FormArray } from '@angular/forms';
import {
  SoeCheckboxFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import {
  ExportDefinitionLevelColumnDTO,
  ExportDefinitionLevelDTO,
} from '../../../models/export.model';
import { ExportStandardDefinitionLevelColumnForm } from './export-standard-definition-level-column-form.model';

interface IExportStandardDefinitionLevelForm {
  validationHandler: ValidationHandler;
  element: ExportDefinitionLevelDTO | undefined;
}
export class ExportStandardDefinitionLevelForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;

  constructor({
    validationHandler,
    element,
  }: IExportStandardDefinitionLevelForm) {
    super(validationHandler, {
      exportDefinitionLevelId: new SoeTextFormControl(
        element?.exportDefinitionLevelId || 0,
        { isIdField: true }
      ),
      level: new SoeNumberFormControl(element?.level || 0),
      xml: new SoeTextFormControl(element?.xml || ''),
      useColumnHeaders: new SoeCheckboxFormControl(
        element?.useColumnHeaders || false
      ),
      exportDefinitionLevelColumns: new FormArray([]),
    });
    this.thisValidationHandler = validationHandler;
  }

  get exportDefinitionLevelId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.exportDefinitionLevelId;
  }

  get level(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.level;
  }

  get xml(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.xml;
  }

  get useColumnHeaders(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.useColumnHeaders;
  }

  get exportDefinitionLevelColumns(): FormArray {
    return <FormArray>this.controls.exportDefinitionLevelColumns;
  }

  addColumnForm(column: ExportDefinitionLevelColumnDTO) {
    this.exportDefinitionLevelColumns.push(
      new ExportStandardDefinitionLevelColumnForm({
        validationHandler: this.thisValidationHandler,
        element: column,
      })
    );
  }

  removeColumnForm(index: number) {
    this.exportDefinitionLevelColumns.removeAt(index);
    this.exportDefinitionLevelColumns.markAsDirty();
    this.exportDefinitionLevelColumns.markAsTouched();
    this.markAsDirty();
    this.markAsTouched();
  }
}
