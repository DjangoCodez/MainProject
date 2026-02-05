import { FormArray } from '@angular/forms';
import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { ITimeBreakTemplateDTONew } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { arrayToFormArray } from '@shared/util/form-util';

interface ITimeBreakTemplatesFormParams {
  validationHandler: ValidationHandler;
  element: ITimeBreakTemplateDTONew | undefined;
}

export class TimeBreakTemplatesForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ITimeBreakTemplatesFormParams) {
    super(validationHandler, {
      timeBreakTemplateId: new SoeTextFormControl(
        element?.timeBreakTemplateId || 0,
        {
          isIdField: true,
        }
      ),
      name: new SoeTextFormControl(null, { isNameField: true }),
      actorCompanyId: new SoeTextFormControl(element?.actorCompanyId || 0),

      shiftLength: new SoeNumberFormControl(
        element?.shiftLength || null,
        {},
        'time.schedule.timebreaktemplate.lengthincludingbreak'
      ),
      shiftStartFromTime: new SoeTextFormControl(
        element?.shiftStartFromTime || null,
        {},
        'time.schedule.timebreaktemplate.shiftstartfromtime'
      ),
      minTimeBetweenBreaks: new SoeNumberFormControl(
        element?.minTimeBetweenBreaks || 0,
        {},
        'time.schedule.timebreaktemplate.mintimebetweenbreaks'
      ),
      useMaxWorkTimeBetweenBreaks: new SoeCheckboxFormControl(
        element?.useMaxWorkTimeBetweenBreaks || false,
        {},
        'time.schedule.timebreaktemplate.usemaxworktimebetweenbreaks'
      ),

      startDate: new SoeDateFormControl(
        element?.startDate || null,
        {},
        'common.startdate'
      ),
      stopDate: new SoeDateFormControl(
        element?.stopDate || null,
        {},
        'common.stopdate'
      ),

      shiftTypeIds: (() => {
        const ids = element?.shiftTypeIds || [];
        const transformed = ids.map((item: any) =>
          typeof item === 'object' && item !== null && 'id' in item
            ? item
            : { id: item }
        );
        return arrayToFormArray(transformed);
      })(),
      dayTypeIds: (() => {
        const ids = element?.dayTypeIds || [];
        const transformed = ids.map((item: any) =>
          typeof item === 'object' && item !== null && 'id' in item
            ? item
            : { id: item }
        );
        return arrayToFormArray(transformed);
      })(),
      dayOfWeeks: (() => {
        const ids = element?.dayOfWeeks || [];
        const transformed = ids.map((item: any) =>
          typeof item === 'object' && item !== null && 'id' in item
            ? item
            : { id: item }
        );
        return arrayToFormArray(transformed);
      })(),

      majorNbrOfBreaks: new SoeNumberFormControl(
        element?.majorNbrOfBreaks ?? 0,
        {},
        'time.schedule.timebreaktemplate.numberofmealbreaks'
      ),
      majorTimeCodeBreakGroupId: new SoeNumberFormControl(
        element?.majorTimeCodeBreakGroupId ?? null,
        {},
        'time.schedule.timebreaktemplate.breakgroupmeal'
      ),
      majorMinTimeAfterStart: new SoeNumberFormControl(
        element?.majorMinTimeAfterStart ?? 0,
        {},
        'time.schedule.timebreaktemplate.minutesafterstart'
      ),
      majorMinTimeBeforeEnd: new SoeNumberFormControl(
        element?.majorMinTimeBeforeEnd ?? 0,
        {},
        'time.schedule.timebreaktemplate.minutesbeforeend'
      ),

      minorNbrOfBreaks: new SoeNumberFormControl(
        element?.minorNbrOfBreaks ?? 0,
        {},
        'time.schedule.timebreaktemplate.numberofbreaks'
      ),
      minorTimeCodeBreakGroupId: new SoeNumberFormControl(
        element?.minorTimeCodeBreakGroupId ?? null,
        {},
        'time.schedule.timebreaktemplate.breakgroup'
      ),
      minorMinTimeAfterStart: new SoeNumberFormControl(
        element?.minorMinTimeAfterStart ?? 0,
        {},
        'time.schedule.timebreaktemplate.minutesafterstart'
      ),
      minorMinTimeBeforeEnd: new SoeNumberFormControl(
        element?.minorMinTimeBeforeEnd ?? 0,
        {},
        'time.schedule.timebreaktemplate.minutesbeforeend'
      ),
    });
  }

  get shiftTypeIds(): FormArray {
    return this.controls.shiftTypeIds as FormArray;
  }

  get dayTypeIds(): FormArray {
    return this.controls.dayTypeIds as FormArray;
  }

  get dayOfWeeks(): FormArray {
    return this.controls.dayOfWeeks as FormArray;
  }
}
