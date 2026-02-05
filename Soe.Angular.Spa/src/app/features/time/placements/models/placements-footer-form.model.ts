import { ValidationErrors, ValidatorFn } from '@angular/forms';
import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeSelectFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { DateUtil } from '@shared/util/date-util';
import { IActivateScheduleGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

export interface IPlacementsFooter {
  functionId: number;
  templateHeadId: number;
  templatePeriodId: number;
  startDate: Date | null;
  stopDate: Date | null;
  isPreliminary: boolean;
}

export interface IPlacementsFooterForm {
  validationHandler: ValidationHandler;
  element: IPlacementsFooter | undefined;
}
export class PlacementsFooterForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IPlacementsFooterForm) {
    super(validationHandler, {
      functionId: new SoeSelectFormControl(
        element?.functionId || undefined,
        { required: true },
        'core.function'
      ),
      templateHeadId: new SoeSelectFormControl(
        element?.templateHeadId || 0,
        { required: false },
        'time.schedule.activate.templatename'
      ),
      templatePeriodId: new SoeSelectFormControl(
        element?.templatePeriodId || 0,
        { required: false },
        'time.schedule.activate.startday'
      ),
      startDate: new SoeDateFormControl(
        element?.startDate,
        { required: true },
        'common.startdate'
      ),
      stopDate: new SoeDateFormControl(
        element?.stopDate,
        { required: true },
        'common.stopdate'
      ),
      isPreliminary: new SoeCheckboxFormControl(
        element?.isPreliminary || false,
        { required: false },
        'time.schedule.planning.templateschedule.activate.preliminary'
      ),
    });
  }

  get functionId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.functionId;
  }

  get templateHeadId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.templateHeadId;
  }

  get templatePeriodId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.templatePeriodId;
  }

  get startDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.startDate;
  }

  get stopDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.stopDate;
  }

  get isPreliminary(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.isPreliminary;
  }
}
export function createSelectedRowValidator(
  errormessage: string,
  getSelectedRows: () => IActivateScheduleGridDTO[]
): ValidatorFn {
  return (form): ValidationErrors | null => {
    const selectedRows = getSelectedRows();
    let anySelectedRow = selectedRows && selectedRows.length > 0;
    return !anySelectedRow ? { [errormessage]: true } : null;
  };
}

export function createStopdateAfterStartdateValidator(
  errormessage: string
): ValidatorFn {
  return (form): ValidationErrors | null => {
    const startDate = form.get('startDate')?.value;
    const stopDate = form.get('stopDate')?.value;
    const stopdateBeforeStartdate: boolean = startDate && startDate >= stopDate;
    return stopdateBeforeStartdate ? { [errormessage]: true } : null;
  };
}

export function createStopdateMaxTwoYearsValidator(
  errormessage: string
): ValidatorFn {
  return (form): ValidationErrors | null => {
    const todaysDate = DateUtil.getToday();
    const stopDate = form.get('stopDate')?.value;
    const stopdateTwoYearsInFuture: boolean =
      stopDate >= todaysDate.addYears(2);
    return stopdateTwoYearsInFuture ? { [errormessage]: true } : null;
  };
}

export function createHasInitialAttestStateValidator(
  hasInitialAttestState: () => boolean,
  errormessage: string
): ValidatorFn {
  return (form): ValidationErrors | null => {
    const noInitialAttestState = !hasInitialAttestState();
    return noInitialAttestState ? { [errormessage]: true } : null;
  };
}
