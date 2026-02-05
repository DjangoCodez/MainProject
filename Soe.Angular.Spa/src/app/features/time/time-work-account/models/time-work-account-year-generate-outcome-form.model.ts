import {
    SoeCheckboxFormControl,
    SoeDateFormControl,
    SoeFormGroup,
    SoeSelectFormControl,
    SoeTextFormControl,
  } from '@shared/extensions';
  import { ValidationHandler } from '@shared/handlers';
import { ITimeWorkAccountGenerateOutcomeModel } from '@shared/models/generated-interfaces/SOECompModelDTOs';

  
  interface TimeWorkAcoountYearGenerateOutcome {
    validationHandler: ValidationHandler;
    element: ITimeWorkAccountGenerateOutcomeModel | undefined;
  }
  export class TimeWorkAccountGenerateOutcomeForm extends SoeFormGroup {
    thisValidationHandler: ValidationHandler;
    constructor({ validationHandler, element }: TimeWorkAcoountYearGenerateOutcome) {
      super(validationHandler, {
        timeWorkAccountYearId: new SoeTextFormControl(
          element?.timeWorkAccountYearId || 0,
          { isIdField: true }
        ),
        timeWorkAccountId: new SoeTextFormControl(
          element?.timeWorkAccountId || 0,
          { isIdField: true }
        ),
        paymentDateId: new SoeSelectFormControl(
            element?.paymentDateId || 0,
          {},
          'time.time.timeperiod.paymentdate'
        ),
        paymentDate: new SoeDateFormControl(
            element?.paymentDate || new Date(),
            {},
            'time.time.timeperiod.paymentdate'
        ),
        overrideChoosen: new SoeCheckboxFormControl(
            element?.overrideChoosen || false,
            {},
            'time.payroll.worktimeaccount.overridechoosen'
        ),
        timeWorkAccountYearEmployeeIds: new SoeSelectFormControl(
            element?.timeWorkAccountYearEmployeeIds || null
        )
       
      });
      this.thisValidationHandler = validationHandler;
    }
  
    get timeWorkAccountId(): SoeTextFormControl {
      return <SoeTextFormControl>this.controls.timeWorkAccountId;
    }
    get PaymentDateId(): SoeSelectFormControl {
      return <SoeSelectFormControl>this.controls.paymentDateId;
    }
    get PaymentDate(): SoeDateFormControl {
        return <SoeDateFormControl>this.controls.paymentDate;
      }
    get overrideChoosen(): SoeCheckboxFormControl {
        return <SoeCheckboxFormControl>this.controls.overrideChoosen;
    }
   
  }
  