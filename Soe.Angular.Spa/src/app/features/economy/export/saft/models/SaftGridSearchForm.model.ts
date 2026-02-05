import { SoeDateFormControl, SoeFormGroup } from '@shared/extensions';
import { SaftGridSearchFormDTO } from './SaftGridSearchDTO.model';
import { ValidationHandler } from '@shared/handlers';

export interface ISafrGridSearchForm {
  validationHandler: ValidationHandler;
  element: SaftGridSearchFormDTO;
}

export class SaftGridSearchForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ISafrGridSearchForm) {
    super(validationHandler, {
      fromDate: new SoeDateFormControl(element?.fromDate || new Date()),
      toDate: new SoeDateFormControl(element?.toDate || new Date()),
    });
  }
  get fromDate() {
    return <SoeDateFormControl>this.controls.fromDate;
  }
  get toDate() {
    return <SoeDateFormControl>this.controls.toDate;
  }
}
