import {
  SoeSelectFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeDateFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { IDrillDownReportDTO } from './drill-down-reports.model';
interface IDrillDownReportForm {
  validationHandler: ValidationHandler;
  element: IDrillDownReportDTO | undefined;
}

export class DrillDownReportForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IDrillDownReportForm) {
    super(validationHandler, {
      reportId: new SoeSelectFormControl(element?.reportId || undefined),
      sysReportTemplateTypeId: new SoeNumberFormControl(
        element?.sysReportTemplateTypeId || undefined
      ),
      budgetId: new SoeSelectFormControl(element?.budgetId || undefined),
      accountYearFromId: new SoeSelectFormControl(
        element?.accountYearFromId || undefined
      ),
      accountYearToId: new SoeSelectFormControl(
        element?.accountYearToId || undefined
      ),
      accountPeriodFromId: new SoeSelectFormControl(
        element?.accountPeriodFromId || undefined
      ),
      accountPeriodToId: new SoeSelectFormControl(
        element?.accountPeriodToId || undefined
      ),
      accountPeriodFrom: new SoeDateFormControl(
        element?.accountPeriodFrom || undefined
      ),
      accountPeriodTo: new SoeDateFormControl(
        element?.accountPeriodTo || undefined
      ),
    });
  }
  get reportId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.reportId;
  }
  get sysReportTemplateTypeId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.sysReportTemplateTypeId;
  }
  get budgetId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.budgetId;
  }
  get accountYearFromId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.accountYearFromId;
  }
  get accountYearToId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.accountYearToId;
  }
  get accountPeriodFromId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.accountPeriodFromId;
  }
  get accountPeriodToId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.accountPeriodToId;
  }
  get accountPeriodFrom(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.accountPeriodFrom;
  }
  get accountPeriodTo(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.accountPeriodTo;
  }
}
