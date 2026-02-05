import {
    SoeFormGroup,
    SoeNumberFormControl,
  } from '@shared/extensions';
  import { ValidationHandler } from '@shared/handlers';
  import { TimeWorkAccountWorkTimeWeekDTO } from '../../models/timeworkaccount.model';
  
  interface ITimeWorkAccountWorkTimeWeekForm {
    validationHandler: ValidationHandler;
    element: TimeWorkAccountWorkTimeWeekDTO | undefined;
  }
  export class TimeWorkAccountWorkTimeWeekForm extends SoeFormGroup {
    thisValidationHandler: ValidationHandler;
    constructor({
      validationHandler,
      element,
    }: ITimeWorkAccountWorkTimeWeekForm) {
      super(validationHandler, {
        timeWorkAccountWorkTimeWeekId: new SoeNumberFormControl(
          element?.timeWorkAccountWorkTimeWeekId || 0,
          {}
        ),
        timeWorkAccountYearId: new SoeNumberFormControl(
          element?.timeWorkAccountYearId || 0,
          {}
        ),
        workTimeWeekFrom: new SoeNumberFormControl(
          element?.workTimeWeekFrom != undefined 
          ? Math.round((element?.workTimeWeekFrom / 60) * 100) / 100
          : 0,
          { minDecimals: 0, maxDecimals: 0 },
          'common.from'
        ),
        workTimeWeekTo: new SoeNumberFormControl(
          element?.workTimeWeekTo != undefined 
          ? Math.round((element?.workTimeWeekTo / 60) * 100) / 100
          : 0,
          { minDecimals: 0, maxDecimals: 0 },
          'common.to'
        ),
        paidLeaveTime: new SoeNumberFormControl(
          element?.paidLeaveTime != undefined 
          ? Math.round((element?.paidLeaveTime / 60) * 100) / 100
          : 0.0,
          { minDecimals: 0, maxDecimals: 2 },
          'time.payroll.worktimeaccount.employee.worktimeweek.maxhours'
        ),
      });

      this.thisValidationHandler = validationHandler;
    }

    get workTimeWeekFrom(): SoeNumberFormControl {
      return <SoeNumberFormControl>this.controls.workTimeWeekFrom;
    }

    get workTimeWeekTo(): SoeNumberFormControl {
      return <SoeNumberFormControl>this.controls.workTimeWeekTo;
    }

    get paidLeaveTime(): SoeNumberFormControl {
      return <SoeNumberFormControl>this.controls.paidLeaveTime;
    }
  }
  