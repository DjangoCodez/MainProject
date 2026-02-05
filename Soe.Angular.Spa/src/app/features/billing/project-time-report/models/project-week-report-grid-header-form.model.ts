import { ValidationHandler } from '@shared/handlers';
import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
} from '@shared/extensions';
import { ProjectWeekReportGridHeaderDTO } from './project-time-report.model';

interface IProjectWeekReportGridHeaderForm {
  validationHandler: ValidationHandler;
  element: ProjectWeekReportGridHeaderDTO | undefined;
}

export class ProjectWeekReportGridHeaderForm extends SoeFormGroup {
  constructor({
    validationHandler,
    element,
  }: IProjectWeekReportGridHeaderForm) {
    super(validationHandler, {
      employeeId: new SoeSelectFormControl(element?.employeeId || 0),
      timeProjectFrom: new SoeDateFormControl(
        element?.timeProjectFrom || new Date().beginningOfWeek()
      ),
      weekNr: new SoeNumberFormControl(element?.weekNr || new Date().weekNbr()),
      showWeekend: new SoeCheckboxFormControl(element?.showWeekend || true),
    });
  }

  get employeeId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.employeeId;
  }
  get timeProjectFrom(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.timeProjectFrom;
  }
  get weekNr(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.weekNr;
  }
  get showWeekend(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.showWeekend;
  }
}
