import {
  SoeFormGroup,
  SoeNumberFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { CompTermDTO } from './language-translations.model';

interface ILanguageTranslationForm {
  validationHandler: ValidationHandler;
  element: CompTermDTO | undefined;
}

export class LanguageTranslationForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ILanguageTranslationForm) {
    super(validationHandler, {
      lang: new SoeTextFormControl(element?.lang || 0, {
        isIdField: true,
        required: true,
      }),
      name: new SoeTextFormControl(
        element?.name || '',
        { isNameField: true },
        ''
      ),
      compTermId: new SoeTextFormControl(element?.compTermId || undefined),
      recordId: new SoeTextFormControl(element?.recordId || undefined),
      recordType: new SoeTextFormControl(element?.recordType || undefined),
      langName: new SoeTextFormControl(element?.langName || undefined),
    });
  }

  get lang(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.lang;
  }
  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }
  get compTermId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.compTermId;
  }
  get recordId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.recordId;
  }
  get recordType(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.recordType;
  }
  get langName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.langName;
  }
  get state(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.state;
  }
}
