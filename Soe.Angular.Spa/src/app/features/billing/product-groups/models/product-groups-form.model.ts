import { SoeFormGroup, SoeTextFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { ProductGroupDTO } from './product-groups.model';

interface IProductGroupsForm {
  validationHandler: ValidationHandler;
  element: ProductGroupDTO | undefined;
}
export class ProductGroupsForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IProductGroupsForm) {
    super(validationHandler, {
      productGroupId: new SoeTextFormControl(element?.productGroupId || 0, {
        isIdField: true,
      }),
      code: new SoeTextFormControl(
        element?.code || '',
        { required: true, isNameField: true, maxLength: 20 },
        'common.code'
      ),
      name: new SoeTextFormControl(
        element?.name || '',
        { required: true, maxLength: 100, minLength: 1 },
        'common.name'
      ),
    });
  }

  get productGroupId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.productGroupId;
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }

  get code(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.code;
  }
}
