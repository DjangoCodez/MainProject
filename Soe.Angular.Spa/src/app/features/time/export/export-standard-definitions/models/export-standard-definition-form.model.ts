import { FormArray } from '@angular/forms';
import {
  SoeCheckboxFormControl,
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import {
  ExportDefinitionDTO,
  ExportDefinitionLevelDTO,
} from '../../../models/export.model';
import { ExportStandardDefinitionLevelColumnForm } from './export-standard-definition-level-column-form.model';
import { ExportStandardDefinitionLevelForm } from './export-standard-definition-level-form.model';

interface IExportStandardDefinitionForm {
  validationHandler: ValidationHandler;
  element: ExportDefinitionDTO | undefined;
}
export class ExportStandardDefinitionForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;
  selectedLevelForm: ExportStandardDefinitionLevelForm | undefined;

  constructor({ validationHandler, element }: IExportStandardDefinitionForm) {
    super(validationHandler, {
      exportDefinitionId: new SoeTextFormControl(
        element?.exportDefinitionId || 0,
        { isIdField: true }
      ),
      name: new SoeTextFormControl(
        element?.name || '',
        { isNameField: true, required: true },
        'common.name'
      ),
      type: new SoeSelectFormControl(
        element?.type || 0,
        { required: true },
        'common.type'
      ),
      specialFunctionality: new SoeTextFormControl(
        element?.specialFunctionality || ''
      ),
      xmlTagHead: new SoeTextFormControl(element?.xmlTagHead || ''),
      separator: new SoeTextFormControl(element?.separator || ''),
      exportDefinitionLevels: new FormArray([]),
      isActive: new SoeCheckboxFormControl(true),
    });
    this.thisValidationHandler = validationHandler;
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }

  get type(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.type;
  }

  get specialFunctionality(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.specialFunctionality;
  }

  get xmlTagHead(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.xmlTagHead;
  }

  get separator(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.separator;
  }

  get exportDefinitionLevels(): FormArray {
    return <FormArray>this.controls.exportDefinitionLevels;
  }

  get isActive(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.isActive;
  }

  customPatchValue(element: ExportDefinitionDTO) {
    (this.controls.exportDefinitionLevels as FormArray).clear();
    for (const exportDefinitionLevel of element.exportDefinitionLevels) {
      const level = new ExportStandardDefinitionLevelForm({
        validationHandler: this.thisValidationHandler,
        element: exportDefinitionLevel,
      });
      (level.controls.exportDefinitionLevelColumns as FormArray).clear();
      for (const exportDefinitionLevelColumn of exportDefinitionLevel.exportDefinitionLevelColumns) {
        level.addColumnForm(exportDefinitionLevelColumn);
      }
      this.exportDefinitionLevels.push(level);
    }
    this.patchValue(element);
  }

  getSelectedLevelColumns(): ExportStandardDefinitionLevelColumnForm[] {
    //TODO: Proper typing instead of 'any'
    return (
      this.selectedLevelForm!.controls.exportDefinitionLevelColumns as any
    ).controls;
  }

  addLevelForm(levelForm: ExportStandardDefinitionLevelForm | undefined) {
    this.exportDefinitionLevels.push(
      levelForm ??
        new ExportStandardDefinitionLevelForm({
          validationHandler: this.thisValidationHandler,
          element: new ExportDefinitionLevelDTO(),
        })
    );
  }

  removeLevelForm(index: number) {
    this.exportDefinitionLevels.removeAt(index);
    this.exportDefinitionLevels.markAsDirty();
    this.exportDefinitionLevels.markAsTouched();
    this.markAsDirty();
    this.markAsTouched();
  }
}
