import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeTextFormControl,
  SoeTimeFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { ITimeStampEntryDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface IAdjustTimeStampsForm {
  validationHandler: ValidationHandler;
  element: ITimeStampEntryDTO | undefined;
}
export class AdjustTimeStampsForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler | undefined;

  constructor({ validationHandler, element }: IAdjustTimeStampsForm) {
    super(validationHandler, {
      isModified: new SoeCheckboxFormControl(element?.isModified || false),
      employeeNr: new SoeTextFormControl(
        element?.employeeNr || '',
        {},
        'time.employee.employee.employeenr'
      ),
      employeeName: new SoeTextFormControl(
        element?.employeeName || '',
        {},
        'common.name'
      ),
      timeTerminalId: new SoeTextFormControl(
        element?.timeTerminalId || '',
        {},
        'time.time.timeterminal.timeterminal'
      ),
      typeName: new SoeTextFormControl(
        element?.typeName || '',
        {},
        'common.type'
      ),
      manuallyAdjusted: new SoeCheckboxFormControl(
        element?.manuallyAdjusted || false,
        {},
        'time.time.adjusttimestamps.adjusted'
      ),
      date: new SoeDateFormControl(element?.date || null, {}, 'common.date'),
      adjustedTime: new SoeTimeFormControl(element?.adjustedTime || '', {}, 'common.time'),
      adjustedTimeBlockDateDate: new SoeDateFormControl(
        element?.adjustedTimeBlockDateDate || null,
        {},
        'time.time.adjusttimestamps.belongstodate'
      ),
      timeDeviationCauseName: new SoeTextFormControl(
        element?.timeDeviationCauseName || '',
        {},
        'common.time.timedeviationcause'
      ),
      accountName: new SoeTextFormControl(
        element?.accountName || '',
        {},
        'common.accounting'
      ),

      note: new SoeTextFormControl(element?.note || '', {}, 'core.comment'),
    });

    this.thisValidationHandler = validationHandler;
  }
}
