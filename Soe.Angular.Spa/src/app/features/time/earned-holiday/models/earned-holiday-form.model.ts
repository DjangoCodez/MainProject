import { SoeCheckboxFormControl, SoeFormGroup, SoeSelectFormControl, SoeTextFormControl } from "@shared/extensions";
import { ValidationHandler } from "@shared/handlers";

interface IEarnedHolidayForm {
    validationHandler: ValidationHandler;
    element: EarnedHolidayForm | undefined;
  }
  export class EarnedHolidayForm extends SoeFormGroup {
    yearId: number;
    holidayId: number;
    loadsuggestions: boolean;

    constructor({ validationHandler, element }: IEarnedHolidayForm) {
      super(validationHandler, {
     
        yearId: new SoeSelectFormControl(
            element?.yearId || 0,
            {
              required: true,
            },
            'common.year'
          ),
          holidayId: new SoeSelectFormControl(
            element?.holidayId || 0,
            {
              required: true,
            },
            'time.time.timeearnedholiday.holiday'
          ),
          loadsuggestions: new SoeCheckboxFormControl(
            element?.loadsuggestions || false,
            {
              
            },
          ),
      }
      );
      this.loadsuggestions = element?.loadsuggestions || false;
      this.yearId = element?.yearId || 0;
      this.holidayId = element?.holidayId || 0;

    }
  }
 