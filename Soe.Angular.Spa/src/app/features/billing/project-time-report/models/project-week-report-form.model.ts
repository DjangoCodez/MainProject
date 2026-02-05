import { ValidationHandler } from '@shared/handlers';
import { SoeFormGroup, SoeTextFormControl } from '@shared/extensions';
import { WeekReportFooterDTO } from './project-time-report.model';

interface IWeekReportForm {
  validationHandler: ValidationHandler;
  element: WeekReportFooterDTO | undefined;
}

export class WeekReportForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IWeekReportForm) {
    super(validationHandler, {
      workedTime: new SoeTextFormControl(element?.workedTime || 0, {
        disabled: true,
      }),
      invoicedTime: new SoeTextFormControl(element?.invoicedTime || 0, {
        disabled: true,
      }),
    });
  }

  get workedTime(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.workedTime;
  }

  get invoicedTime(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.invoicedTime;
  }
}
