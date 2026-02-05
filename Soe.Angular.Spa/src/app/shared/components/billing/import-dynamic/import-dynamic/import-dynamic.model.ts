import { FormArray, FormRecord } from '@angular/forms';
import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { SettingDataType } from '@shared/models/generated-interfaces/Enumerations';
import {
  IImportDynamicDTO,
  IImportDynamicLogDTO,
  IImportDynamicResultDTO,
  IImportFieldDTO,
  IImportOptionsDTO,
  ISupplierProductImportRawDTO,
} from '@shared/models/generated-interfaces/ImportDynamicDTO';
import { DateUtil } from '@shared/util/date-util';
import { DialogData, DialogSize } from '@ui/dialog/models/dialog';
import { Observable } from 'rxjs';

interface IImportDyanmicDialogData extends DialogData {
  importDTO: ImportDynamicDTO;
  callback: (
    data: ISupplierProductImportRawDTO[],
    options: IImportOptionsDTO
  ) => Observable<IImportDynamicResultDTO>;
}

export type ParseRowsResult = { [id: string]: object };
export type ValueMapping = Record<string, SmallGenericType>;

export class ImportOptionsDTO implements IImportOptionsDTO {
  skipFirstRow: boolean;
  importNew: boolean;
  updateExisting: boolean;
  constructor() {
    this.skipFirstRow = true;
    this.importNew = true;
    this.updateExisting = true;
  }
}

export class ImportFieldDTO implements IImportFieldDTO {
  field!: string;
  label!: string;
  index!: number;
  dataType!: SettingDataType;
  isRequired!: boolean;
  isConfigured!: boolean;
  defaultStringValue!: string;
  defaultBoolValue?: boolean | undefined;
  defaultIntValue?: number;
  defaultDecimalValue?: number;
  defaultDateTimeValue!: Date;
  enableValueMapping!: boolean;
  availableValues: SmallGenericType[] = [];
  defaultGenericTypeValue!: SmallGenericType;
  valueMapping!: Record<string, SmallGenericType>;
}

export class ImportDynamicDTO implements IImportDynamicDTO {
  options!: ImportOptionsDTO;
  fields: ImportFieldDTO[] = [];
}

export class ImportDynamicResultDTO implements IImportDynamicResultDTO {
  success: boolean = false;
  message: string = '';
  logs: IImportDynamicLogDTO[] = [];
  totalCount: number = 0;
  skippedCount: number = 0;
  newCount: number = 0;
  updateCount: number = 0;
}

export class ImportDyanmicDialogData implements IImportDyanmicDialogData {
  importDTO!: ImportDynamicDTO;
  callback!: (
    data: ISupplierProductImportRawDTO[],
    options: IImportOptionsDTO
  ) => Observable<IImportDynamicResultDTO>;
  size?: DialogSize | undefined;
  title!: string;
  content?: string | undefined;
  primaryText?: string | undefined;
  secondaryText?: string | undefined;
  disableClose?: boolean | undefined;
  disableContentScroll?: boolean | undefined;
  noToolbar?: boolean | undefined;
  hideFooter?: boolean | undefined;
  callbackAction?: (() => unknown) | undefined;
}

export class ImportDynamicDialogDTO {
  fileUploadTab: FileUploadTab;

  constructor() {
    this.fileUploadTab = new FileUploadTab();
  }
}

export class ParseRowsModel {
  fields: ImportFieldDTO[] = [];
  options!: ImportOptionsDTO;
  data: string[][] = [];
}

export class FileUploadTab {
  fileType!: number;
  fileName!: string;
  fileContent!: string;
}

interface IImportDyanmicDialogform {
  validationHandler: ValidationHandler;
  element: ImportDynamicDialogDTO;
}

interface IFileUploadTabForm {
  validationHandler: ValidationHandler;
  element: FileUploadTab;
}

interface IImportOptionsForm {
  validationHandler: ValidationHandler;
  element: ImportOptionsDTO;
}

interface IImportFieldForm {
  validationHandler: ValidationHandler;
  element: ImportFieldDTO;
}

interface IImportDynamicForm {
  validationHandler: ValidationHandler;
  element: ImportDynamicDTO;
}

interface ISmallGenericForm {
  validationHandler: ValidationHandler;
  element?: SmallGenericType;
}

interface IImportDynamicResultForm {
  validationHandler: ValidationHandler;
  element?: ImportDynamicResultDTO;
}

export class SmallGenericForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ISmallGenericForm) {
    super(validationHandler, {
      id: new SoeNumberFormControl(element?.id || undefined),
      name: new SoeTextFormControl(element?.name || undefined),
    });
  }

  get id(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.id;
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }
}

export class FileUploadTabForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IFileUploadTabForm) {
    super(validationHandler, {
      fileType: new SoeSelectFormControl(element?.fileType || 0, {
        required: true,
      }),
      fileName: new SoeTextFormControl(element?.fileName || undefined),
      fileContent: new SoeTextFormControl(element?.fileContent || undefined),
    });

    this.controls.fileName.disable({ onlySelf: true });
  }

  get fileType(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.fileType;
  }

  get fileName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.fileName;
  }

  get fileContent(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.fileContent;
  }
}

export class ImportDynamicResultForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IImportDynamicResultForm) {
    super(validationHandler, {
      newCount: new SoeTextFormControl(element?.newCount || 0),
      updateCount: new SoeTextFormControl(element?.updateCount || 0),
    });
  }

  get newCount(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.newCount;
  }

  get updateCount(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.updateCount;
  }
}

export class ImportDyanmicDialogform extends SoeFormGroup {
  constructor({ validationHandler, element }: IImportDyanmicDialogform) {
    super(validationHandler, {
      fileUploadTab: new FileUploadTabForm({
        validationHandler,
        element: element.fileUploadTab,
      }),
    });
  }

  get fileUploadTab(): FileUploadTabForm {
    return <FileUploadTabForm>this.controls.fileUploadTab;
  }

  public patchFileUploadTabValues(fileName: string): void {
    this.fileUploadTab.patchValue({
      fileName,
    });
  }
}

export class ImportOptionsForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IImportOptionsForm) {
    super(validationHandler, {
      skipFirstRow: new SoeCheckboxFormControl(element?.skipFirstRow || false),
      importNew: new SoeCheckboxFormControl(element?.importNew || false),
      updateExisting: new SoeCheckboxFormControl(
        element?.updateExisting || false
      ),
    });
  }

  get skipFirstRow(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.skipFirstRow;
  }

  get importNew(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.importNew;
  }

  get updateExisting(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.updateExisting;
  }
}

export class ImportFieldForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;
  constructor({ validationHandler, element }: IImportFieldForm) {
    super(validationHandler, {
      field: new SoeTextFormControl(element?.field || undefined),
      label: new SoeTextFormControl(element?.label || undefined),
      index: new SoeNumberFormControl(element?.index || undefined),
      fieldDataType: new SoeTextFormControl(element?.dataType || undefined),
      isRequired: new SoeCheckboxFormControl(element?.isRequired || false),
      isConfigured: new SoeCheckboxFormControl(element?.isConfigured || false),
      defaultStringValue: new SoeTextFormControl(
        element?.defaultStringValue || undefined
      ),
      defaultBoolValue: new SoeCheckboxFormControl(
        element?.defaultBoolValue || false
      ),
      defaultIntValue: new SoeNumberFormControl(
        element?.defaultIntValue || undefined,
        {
          decimals: 0,
        }
      ),
      defaultDecimalValue: new SoeNumberFormControl(
        element?.defaultDecimalValue || undefined,
        {
          decimals: 0,
          zeroNotAllowed: true,
        }
      ),
      defaultDateTimeValue: new SoeDateFormControl(
        element?.defaultDateTimeValue || DateUtil.defaultDateTime()
      ),
      enableValueMapping: new SoeCheckboxFormControl(
        element?.enableValueMapping || false
      ),
      defaultGenericTypeValue: new SmallGenericForm({
        validationHandler: validationHandler,
        element: element?.defaultGenericTypeValue,
      }),
      defaultGenericTypeValueInt: new SoeSelectFormControl(
        element?.defaultGenericTypeValue?.id || 0
      ),
      valueMapping: new FormRecord<SmallGenericForm>({}),
      valueMappingControls: new FormArray<ValueMapFieldForm>([]),
    });
    this.thisValidationHandler = validationHandler;
  }

  get field(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.field;
  }

  get label(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.label;
  }

  get index(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.index;
  }

  get fieldDataType(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.fieldDataType;
  }

  get isRequired(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.isRequired;
  }

  get isConfigured(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.isConfigured;
  }

  get defaultStringValue(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.defaultStringValue;
  }

  get defaultBoolValue(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.defaultBoolValue;
  }

  get defaultIntValue(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.defaultIntValue;
  }

  get defaultDecimalValue(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.defaultDecimalValue;
  }

  get defaultDateTimeValue(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.defaultDateTimeValue;
  }

  get enableValueMapping(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.enableValueMapping;
  }

  get defaultGenericTypeValue(): SmallGenericForm {
    return <SmallGenericForm>this.controls.defaultGenericTypeValue;
  }

  get defaultGenericTypeValueInt(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.defaultGenericTypeValueInt;
  }

  get valueMapping(): FormRecord<SmallGenericForm> {
    return <FormRecord<SmallGenericForm>>this.controls.valueMapping;
  }

  get valueMappingControls(): FormArray<ValueMapFieldForm> {
    return <FormArray<ValueMapFieldForm>>this.controls.valueMappingControls;
  }

  customPatch(element: ImportFieldDTO, uniqueValues?: string[]): void {
    this.reset(element, { emitEvent: false });
    this.defaultGenericTypeValueInt.reset(
      element.defaultGenericTypeValue?.id ?? undefined,
      {
        emitEvent: false,
      }
    );
    this.defaultGenericTypeValue.reset(
      {
        id: element.defaultGenericTypeValue?.id ?? 0,
        name: element.defaultGenericTypeValue?.name ?? '',
      },
      { emitEvent: false }
    );

    this.patchValueMapping(element, uniqueValues);
    this.updateValueAndValidity();
  }

  private patchValueMapping(element: ImportFieldDTO, uniqueValues?: string[]) {
    this.valueMapping.reset({}, { emitEvent: false });
    this.valueMappingControls.clear({ emitEvent: false });
    if (uniqueValues) {
      uniqueValues.forEach(uv => {
        const valMapF = new ValueMapField();
        const existingMap = element.valueMapping
          ? element.valueMapping[uv]
          : undefined;

        valMapF.fieldName = uv;
        valMapF.fieldValue = existingMap?.id ?? undefined;

        this.valueMappingControls.push(
          new ValueMapFieldForm({
            validationHandler: this.thisValidationHandler,
            element: valMapF,
          }),
          { emitEvent: false }
        );

        this.valueMapping.addControl(
          uv,
          new SmallGenericForm({
            validationHandler: this.thisValidationHandler,
            element: existingMap,
          }),
          { emitEvent: false }
        );
      });
    }
    this.valueMapping.updateValueAndValidity();
    this.valueMappingControls.updateValueAndValidity();
  }

  disableUnwanted(): void {
    this.controls['created'].disable();
    this.controls['createdBy'].disable();
    this.controls['isActive'].disable();
    this.controls['modified'].disable();
    this.controls['modifiedBy'].disable();
    this.controls['state'].disable();
    this.controls.defaultGenericTypeValueInt.disable();
    this.controls.fieldDataType.disable();
    this.controls.valueMappingControls.disable();

    //Disable empty valueMappings
    Object.keys(this.valueMapping.controls).forEach(x => {
      if (
        !(
          this.valueMapping.controls[x].id?.value &&
          this.valueMapping.controls[x].id.value >= 0
        )
      ) {
        this.valueMapping.controls[x].disable();
      }
    });
  }
}

export class ImportDynamicForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;
  constructor({ validationHandler, element }: IImportDynamicForm) {
    super(validationHandler, {
      options: new ImportOptionsForm({
        validationHandler,
        element: element?.options,
      }),
      fields: new FormArray<ImportFieldForm>([]),
    });
    this.thisValidationHandler = validationHandler;
  }

  get options(): ImportOptionsForm {
    return <ImportOptionsForm>this.controls.options;
  }

  get fields(): FormArray<ImportFieldForm> {
    return <FormArray<ImportFieldForm>>this.controls.fields;
  }
}

class ValueMapField {
  fieldName: string;
  fieldValue?: number;

  constructor() {
    this.fieldName = '';
  }
}
interface IValueMapFieldForm {
  validationHandler: ValidationHandler;
  element?: ValueMapField;
}

export class ValueMapFieldForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IValueMapFieldForm) {
    super(validationHandler, {
      fieldName: new SoeTextFormControl(element?.fieldName || undefined),
      fieldValue: new SoeSelectFormControl(element?.fieldValue || undefined),
    });
  }

  get fieldName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.fieldName;
  }

  get fieldValue(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.fieldValue;
  }
}
