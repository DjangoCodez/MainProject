import { ValidationHandler } from '@shared/handlers';
import {
  SoeDateFormControl,
  SoeDateRangeFormControl,
  SoeFormGroup,
  SoeSelectFormControl,
} from '@shared/extensions';
import { DateUtil } from '@shared/util/date-util';
import { ProjectTimeReportGridHeaderDTO } from './project-time-report.model';
import { IFilterExpensesModel } from '@shared/models/generated-interfaces/BillingModels';

interface IProjectTimeReportGridHeaderForm {
  validationHandler: ValidationHandler;
  element: ProjectTimeReportGridHeaderDTO | undefined;
}

export class ProjectTimeReportGridHeaderForm extends SoeFormGroup {
  constructor({
    validationHandler,
    element,
  }: IProjectTimeReportGridHeaderForm) {
    super(validationHandler, {
      from: new SoeDateFormControl(
        element?.from ||
          DateUtil.format(new Date().dayOfWeek(), `yyyyMMdd'T'HHmmss`)
      ),
      to: new SoeDateFormControl(
        element?.to || DateUtil.format(new Date(), `yyyyMMdd'T'HHmmss`)
      ),
      employeeIds: new SoeSelectFormControl(element?.employeeIds || []),
      categoriesIds: new SoeSelectFormControl(element?.categoriesIds || []),
      projectIds: new SoeSelectFormControl(element?.projectIds || []),
      orderIds: new SoeSelectFormControl(element?.orderIds || []),
      timeDeviationCauseIds: new SoeSelectFormControl(
        element?.timeDeviationCauseIds || []
      ),
      dateRange: new SoeDateRangeFormControl([
        DateUtil.getDateFirstInWeek(new Date()),
        DateUtil.getDateLastInWeek(new Date()),
      ]),
    });
  }

  get from(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.from;
  }

  get to(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.to;
  }

  get dateRange(): SoeDateRangeFormControl {
    return <SoeDateRangeFormControl>this.controls.dateRange;
  }

  get categoriesIds(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.categoriesIds;
  }

  get employeeIds(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.employeeIds;
  }

  get projectIds(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.projectIds;
  }

  get orderIds(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.orderIds;
  }

  get timeDeviationCauseIds(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.timeDeviationCauseIds;
  }

  getFilterExpenseModel(): IFilterExpensesModel {
    const filterModel: IFilterExpensesModel = {
      employeeId: 0,
      from: new Date(this.dateRange.value[0]),
      to: new Date(this.dateRange.value[1]),
      employees: this.employeeIds.value,
      projects: this.projectIds.value,
      orders: this.orderIds.value,
      employeeCategories: this.categoriesIds.value,
    };

    return filterModel;
  }
}
