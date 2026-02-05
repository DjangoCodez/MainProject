import { Component, OnInit, inject } from '@angular/core';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { FieldOptionsData } from './field-options.model';
import { ImportFieldDTO, ImportFieldForm } from '../import-dynamic.model';
import { ValidationHandler } from '@shared/handlers';
import { SettingDataType } from '@shared/models/generated-interfaces/Enumerations';

@Component({
    selector: 'soe-field-options',
    templateUrl: './field-options.component.html',
    styleUrl: './field-options.component.scss',
    standalone: false
})
export class FieldOptionsComponent
  extends DialogComponent<FieldOptionsData>
  implements OnInit
{
  readonly validationHandler = inject(ValidationHandler);
  protected form = new ImportFieldForm({
    validationHandler: this.validationHandler,
    element: new ImportFieldDTO(),
  });

  ngOnInit(): void {
    this.form?.customPatch(
      this.data.field as ImportFieldDTO,
      this.data.uniqueValues
    );
  }

  cancel(): void {
    this.dialogRef.close(false);
  }

  configureField(): void {
    let tempField = this.data.field;
    this.form?.disableUnwanted();
    tempField = this.form?.value as ImportFieldDTO;
    tempField.availableValues = this.data.field?.availableValues ?? [];
    tempField.dataType = this.data.field?.dataType || SettingDataType.Undefined;
    tempField.isConfigured = true;
    this.dialogRef.close(<ImportFieldDTO>tempField);
  }

  defaultGenericTypeChanged(value: number): void {
    const item = this.data.field?.availableValues.find(x => x.id === value);
    if (item) {
      this.form.defaultGenericTypeValue.patchValue({
        id: item.id,
        name: item.name,
      });
    }
  }

  valueMapFieldValueChange(value: number, fieldName: string): void {
    const item = this.data.field?.availableValues.find(x => x.id === value);
    if (item) {
      this.form?.valueMapping.controls[fieldName]?.patchValue(item);
    }
  }
}
