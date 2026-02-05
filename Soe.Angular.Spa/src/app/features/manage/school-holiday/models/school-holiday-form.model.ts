import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { ISchoolHolidayDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { DateUtil } from '@shared/util/date-util';

interface ISchoolHolidayForm {
  validationHandler: ValidationHandler;
  element: ISchoolHolidayDTO | undefined;
}
export class SchoolHolidayForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ISchoolHolidayForm) {
    super(validationHandler, {
      schoolHolidayId: new SoeTextFormControl(element?.schoolHolidayId || 0, {
        isIdField: true,
      }),
      name: new SoeTextFormControl(
        element?.name || '',
        { isNameField: true, required: true, maxLength: 100 },
        'common.name'
      ),
      dateFrom: new SoeDateFormControl(
        element?.dateFrom || DateUtil.getToday(),
        { required: true },
        'common.fromdate'
      ),
      dateTo: new SoeDateFormControl(
        element?.dateTo || DateUtil.getToday(),
        { required: true },
        'common.todate'
      ),
      isSummerHoliday: new SoeCheckboxFormControl(
        element?.isSummerHoliday || false
      ),
    });
  }
}
