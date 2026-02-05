import { SoeFormGroup, SoeTextFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { ProductUnitDTO, ProductUnitModel } from './product-units.model';
import { LanguageTranslationForm } from '@shared/features/language-translations/models/language-translations-form.model';
import { FormArray } from '@angular/forms';
import { CompTermDTO } from '@shared/features/language-translations/models/language-translations.model';
import { ICompTermDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface IProductUnitsForm {
  validationHandler: ValidationHandler;
  element: ProductUnitDTO | undefined;
}
export class ProductUnitsForm extends SoeFormGroup {
  translateValidationHandler: ValidationHandler;

  constructor({ validationHandler, element }: IProductUnitsForm) {
    super(validationHandler, {
      productUnitId: new SoeTextFormControl(element?.productUnitId || 0, {
        isIdField: true,
      }),
      code: new SoeTextFormControl(
        element?.code || '',
        { isNameField: true, required: true, maxLength: 20 },
        'common.code'
      ),
      name: new SoeTextFormControl(
        element?.name || '',
        { required: true, maxLength: 100, minLength: 1 },
        'common.name'
      ),
      translations: new FormArray<LanguageTranslationForm>([]),
    });

    this.translateValidationHandler = validationHandler;
    this.patchCompTerms(element?.translations ?? []);
  }

  get productUnitId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.productUnitId;
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }

  get code(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.code;
  }

  get translations(): FormArray<LanguageTranslationForm> {
    return <FormArray>this.controls.translations;
  }

  onDoCopy(): void {
    const formArray = this.translations;

    if (formArray) {
      const elementArray: ICompTermDTO[] = [];
      for (const trans of formArray.controls) {
        elementArray.push(
          new CompTermDTO(
            0,
            0,
            trans.value.state,
            trans.value.recordType,
            trans.value.name,
            trans.value.lang,
            trans.value.langName
          )
        );
      }

      this.patchCompTerms(elementArray);
    }
  }

  customPatch(element: ProductUnitModel) {
    this.reset(element);

    this.translations.clear({ emitEvent: false });
    element.translations.forEach(s => {
      this.translations.push(
        new LanguageTranslationForm({
          validationHandler: this.translateValidationHandler,
          element: <CompTermDTO>s,
        }),
        { emitEvent: false }
      );
    });
    this.translations.updateValueAndValidity();
  }

  patchCompTerms(compTermRows: ICompTermDTO[]) {
    this.translations?.clear();

    for (const compTerm of compTermRows) {
      const languageRow = new LanguageTranslationForm({
        validationHandler: this.translateValidationHandler,
        element: compTerm as CompTermDTO,
      });
      this.translations.push(languageRow, { emitEvent: false });
    }
    this.translations.updateValueAndValidity();
  }
}
