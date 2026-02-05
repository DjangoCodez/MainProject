import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { IHolidayDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { DateUtil } from '@shared/util/date-util';

interface IHolidayForm {
  validationHandler: ValidationHandler;
  element: IHolidayDTO | undefined;
}

export class HolidayForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IHolidayForm) {
    super(validationHandler, {
      holidayId: new SoeTextFormControl(element?.holidayId || 0, {
        isIdField: true,
      }),
      name: new SoeTextFormControl(
        element?.name || '',
        { isNameField: true, required: true, maxLength: 100, minLength: 1 },
        'common.name'
      ),
      description: new SoeTextFormControl(
        element?.description || '',
        { maxLength: 512 },
        'common.description'
      ),
      dayTypeId: new SoeSelectFormControl(
        element?.dayTypeId || undefined,
        { 
          required: true 
        },
        'time.schedule.daytype.daytype'
      ),
      sysHolidayTypeId: new SoeSelectFormControl(
        element?.sysHolidayTypeId || undefined
      ),
      date: new SoeDateFormControl(
        element?.date || DateUtil.getToday(),
        { required: true },
        'common.date'
      ),
      isRedDay: new SoeCheckboxFormControl(
        element?.isRedDay || false,
        {},
        'time.time.timeearnedholiday.holiday'
      ),
    });
  }

  get holidayId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.holidayId;
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }

  get description(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.description;
  }

  get dayTypeId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.dayTypeId;
  }

  get sysHolidayTypeId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.sysHolidayTypeId;
  }

  get date(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.date;
  }

  get isRedDay(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.isRedDay;
  }
}
