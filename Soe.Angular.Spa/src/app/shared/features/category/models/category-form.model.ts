import { ValidationHandler } from '@shared/handlers';
import {
  SoeCheckboxFormControl,
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { SoeCategoryType } from '@shared/models/generated-interfaces/Enumerations';
import { ICategoryDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface ICategoryForm {
  validationHandler: ValidationHandler;
  element?: ICategoryDTO;
}

export class CategoryForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ICategoryForm) {
    super(validationHandler, {
      categoryId: new SoeTextFormControl(element?.categoryId || 0, {
        isIdField: true,
      }),
      code: new SoeTextFormControl(
        element?.code || '',
        {
          required: true,
          maxLength: 50,
        },
        'common.code'
      ),
      name: new SoeTextFormControl(
        element?.name || '',
        {
          isNameField: true,
          required: true,
          maxLength: 100,
        },
        'common.name'
      ),
      type: new SoeTextFormControl(element?.type || SoeCategoryType.Unknown),
      parentId: new SoeSelectFormControl(element?.parentId || undefined),
      isSelected: new SoeCheckboxFormControl(element?.isSelected || false),
      isVisible: new SoeCheckboxFormControl(element?.isVisible || false),
      childrenNamesString: new SoeTextFormControl(
        element?.childrenNamesString || undefined
      ),
    });

    this.parentId.valueChanges.subscribe(v => {
      if (v === 0) {
        this.parentId.patchValue(undefined, {
          onlySelf: true,
          emitEvent: false,
          emitModelToViewChange: false,
          emitViewToModelChange: false,
        });
      }
    });
  }

  get categoryId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.categoryId;
  }

  get code(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.code;
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }

  get type(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.type;
  }

  get parentId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.parentId;
  }

  get isSelected(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.isSelected;
  }

  get isVisible(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.isVisible;
  }

  get childrenNamesString(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.childrenNamesString;
  }
}
