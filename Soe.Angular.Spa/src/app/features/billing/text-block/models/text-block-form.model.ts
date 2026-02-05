import { ValidationHandler } from '@shared/handlers';
import { TextblockDTO } from './text-block.model';
import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ICompTermDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { LanguageTranslationForm } from '@shared/features/language-translations/models/language-translations-form.model';
import { FormArray } from '@angular/forms';
import { CompTermDTO } from '@shared/features/language-translations/models/language-translations.model';

interface ITextBlockForm {
  validationHandler: ValidationHandler;
  element: TextblockDTO | undefined;
}

export class TextBlockForm extends SoeFormGroup {
  translateValidationHandler: ValidationHandler;

  constructor({ validationHandler, element }: ITextBlockForm) {
    super(validationHandler, {
      textblockId: new SoeSelectFormControl(element?.textblockId || 0, {
        isIdField: true,
      }),
      headline: new SoeTextFormControl(
        element?.headline || undefined,
        { isNameField: true, required: true, maxLength: 100 },
        'common.name'
      ),
      text: new SoeTextFormControl(
        element?.text || undefined,
        {},
        'common.text'
      ),
      type: new SoeTextFormControl(
        element?.type || undefined,
        {
          required: true,
        },
        'common.type'
      ),
      showInContract: new SoeCheckboxFormControl(
        element?.showInContract || false,
        {},
        'common.contract'
      ),
      showInOffer: new SoeCheckboxFormControl(
        element?.showInOffer || false,
        {},
        'common.offer'
      ),
      showInOrder: new SoeCheckboxFormControl(
        element?.showInOrder || false,
        {},
        'common.order'
      ),
      showInInvoice: new SoeCheckboxFormControl(
        element?.showInInvoice || false,
        {},
        'common.customerinvoice'
      ),
      showInPurchase: new SoeCheckboxFormControl(
        element?.showInPurchase || false,
        {},
        'billing.purchase.list.purchase'
      ),
      translations: new FormArray<LanguageTranslationForm>([]),
      created: new SoeDateFormControl(element?.created || undefined),
      createdBy: new SoeTextFormControl(element?.createdBy || undefined),
      modified: new SoeDateFormControl(element?.modified || undefined),
      modifiedBy: new SoeTextFormControl(element?.modifiedBy || undefined),
      actorCompanyId: new SoeNumberFormControl(element?.actorCompanyId || 0),
      isModified: new SoeTextFormControl(element?.isModified || false),
    });

    this.translateValidationHandler = validationHandler;
    this.patchCompTerms(element?.translations ?? []);
  }

  get textblockId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.textblockId;
  }
  get headline(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.headline;
  }
  get text(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.text;
  }
  get type(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.type;
  }
  get showInContract(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.showInContract;
  }
  get showInOffer(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.showInOffer;
  }
  get showInOrder(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.showInOrder;
  }
  get showInInvoice(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.showInInvoice;
  }
  get showInPurchase(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.showInPurchase;
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
