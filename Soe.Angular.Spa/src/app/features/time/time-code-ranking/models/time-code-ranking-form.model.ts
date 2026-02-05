import {
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';

import { TimeCodeRankingRowForm } from './time-code-ranking-row-form.model';
import { FormArray } from '@angular/forms';
import {
  ITimeCodeRankingDTO,
  ITimeCodeRankingGroupDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface ITimeCodeRankingForm {
  validationHandler: ValidationHandler;
  element?: ITimeCodeRankingGroupDTO;
}

export class TimeCodeRankingForm extends SoeFormGroup {
  timeCodeRankingHandler: ValidationHandler;
  constructor({ validationHandler, element }: ITimeCodeRankingForm) {
    super(validationHandler, {
      timeCodeRankingGroupId: new SoeNumberFormControl(
        element?.timeCodeRankingGroupId || '',
        { isIdField: true },
        ''
      ),
      startDate: new SoeDateFormControl(
        element?.startDate || '',
        { required: true },
        'common.startdate'
      ),
      stopDate: new SoeDateFormControl(
        element?.stopDate || '',
        {},
        'common.stopdate'
      ),
      description: new SoeTextFormControl(
        element?.description || '',
        { maxLength: 512 },
        'common.description'
      ),
      name: new SoeTextFormControl('', { isNameField: true }, 'common.name'),
      timeCodeRankings: new FormArray([]),
    });
    this.timeCodeRankingHandler = validationHandler;
    if (element?.timeCodeRankings) {
      this.patchRows(element.timeCodeRankings);
    }
  }

  get timeCodeRankings(): FormArray<TimeCodeRankingRowForm> {
    return <FormArray>this.controls.timeCodeRankings;
  }

  onAddTimeCode() {
    this.timeCodeRankings.push(
      new TimeCodeRankingRowForm({
        validationHandler: this.timeCodeRankingHandler,
        element: <ITimeCodeRankingDTO>{},
      })
    );
    this.patchRows(this.timeCodeRankings.value);
  }

  customPatchValue(element: ITimeCodeRankingGroupDTO) {
    this.patchValue(element);
    this.timeCodeRankings.clear();
    if (element.timeCodeRankings) {
      setTimeout(() => this.patchRows(element.timeCodeRankings), 50);
    }
  }

  patchRows(timeCodeRankings: ITimeCodeRankingDTO[]) {
    this.timeCodeRankings.clear();
    timeCodeRankings.forEach(timeCodeRanking => {
      const rowForm = new TimeCodeRankingRowForm({
        validationHandler: this.formValidationHandler,
        element: timeCodeRanking,
      });
      rowForm.customPatchValue(timeCodeRanking);

      rowForm.updateValueAndValidity({ emitEvent: false });
      this.timeCodeRankings.push(rowForm);
    });

    this.updateValueAndValidity({ emitEvent: false });
  }
}
