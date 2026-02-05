import { SoeFormGroup, SoeSelectFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { ISearchExtraFieldsGridModel } from '@shared/features/extra-fields/components/extra-fields-grid/extra-fields-grid-filter/extra-fields-grid-filter.component';

interface IExtraFieldsFilterForm {
  validationHandler: ValidationHandler;
  element: ISearchExtraFieldsGridModel | undefined;
}
export class ExtraFieldsFilterForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IExtraFieldsFilterForm) {
    super(validationHandler, {
      entity: new SoeSelectFormControl(element?.entity || 0),
    });
  }
  get entity(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.entity;
  }
}
