import {
  SoeFormGroup,
  SoeTextFormControl,
  SoeSelectFormControl,
  SoeNumberFormControl,
  SoeCheckboxFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { FormArray } from '@angular/forms';
import { LanguageTranslationForm } from '@shared/features/language-translations/models/language-translations-form.model';
import { CompTermDTO } from '@shared/features/language-translations/models/language-translations.model';
import {
  IExtraFieldDTO,
  IExtraFieldValueDTO,
} from '@shared/models/generated-interfaces/ExtraFieldDTO';
import {
  TermGroup_ExtraFieldType,
  TermGroup_ExtraFieldValueType,
} from '@shared/models/generated-interfaces/Enumerations';

interface IExtraFieldForm {
  validationHandler: ValidationHandler;
  element: IExtraFieldDTO | undefined;
}

interface IExtraFieldValueForm {
  validationHandler: ValidationHandler;
  element: IExtraFieldValueDTO | undefined;
}

export class ExtraFieldForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;

  constructor({ validationHandler, element }: IExtraFieldForm) {
    super(validationHandler, {
      extraFieldId: new SoeTextFormControl(element?.extraFieldId || 0, {
        isIdField: true,
      }),
      sysExtraFieldId: new SoeSelectFormControl(
        element?.sysExtraFieldId || undefined
      ),
      entity: new SoeTextFormControl(element?.entity || 0),
      text: new SoeTextFormControl(
        element?.text || '',
        { required: true, isNameField: true },
        'common.appellation'
      ),
      type: new SoeSelectFormControl(
        element?.type || TermGroup_ExtraFieldType.FreeText
      ),
      connectedEntity: new SoeTextFormControl(
        element?.connectedEntity || undefined
      ),
      connectedRecordId: new SoeSelectFormControl(
        element?.connectedRecordId || 0,
        { required: true, zeroNotAllowed: true },
        'common.accountdim'
      ),
      translations: new FormArray<LanguageTranslationForm>([]),
      extraFieldValues: new FormArray<ExtraFieldValueForm>([]),

      // Extentions
      isCopy: new SoeCheckboxFormControl(false),
    });

    this.thisValidationHandler = validationHandler;
    this.patchCompTerms(<CompTermDTO[]>element?.translations ?? []);
    this.patchExtraFieldValues(
      <IExtraFieldValueDTO[]>element?.extraFieldValues ?? []
    );
  }

  get text(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.text;
  }

  get type(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.type;
  }

  get translations(): FormArray<LanguageTranslationForm> {
    return <FormArray>this.controls.translations;
  }

  get extraFieldValues(): FormArray<ExtraFieldValueForm> {
    return <FormArray>this.controls.extraFieldValues;
  }

  get isTypeSingleChoice(): boolean {
    return this.controls.type.value === TermGroup_ExtraFieldType.SingleChoice;
  }

  get sysExtraFieldId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.sysExtraFieldId;
  }

  onDoCopy() {
    this.controls.isCopy.patchValue(true);

    this.translations.controls.forEach(row => {
      row.patchValue({
        recordId: 0,
        compTermId: 0,
      });
    });

    this.extraFieldValues.controls.forEach(row => {
      row.patchValue({
        extraFieldValueId: 0,
      });
    });
  }

  patchCompTerms(rows: CompTermDTO[]) {
    this.translations?.clear();

    for (const row of rows) {
      const formRow = new LanguageTranslationForm({
        validationHandler: this.thisValidationHandler,
        element: row,
      });
      this.translations.push(formRow, { emitEvent: false });
    }
    this.translations.updateValueAndValidity();
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

export class ExtraFieldValueForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;

  constructor({ validationHandler, element }: IExtraFieldValueForm) {
    super(validationHandler, {
      extraFieldValueId: new SoeTextFormControl(
        element?.extraFieldValueId || 0,
        {
          isIdField: true,
        }
      ),
      extraFieldId: new SoeTextFormControl(element?.extraFieldId || 0),
      type: new SoeSelectFormControl(
        element?.type || TermGroup_ExtraFieldValueType.String
      ),
      value: new SoeTextFormControl(element?.value || '', {
        isNameField: true,
      }),
      sort: new SoeNumberFormControl(element?.sort || 0),
    });
    this.thisValidationHandler = validationHandler;
  }
}
