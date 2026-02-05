import {
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { IShiftTypeEmployeeStatisticsTargetDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface IShiftTypeEmployeeStatisticsTargetForm {
  validationHandler: ValidationHandler;
  element: IShiftTypeEmployeeStatisticsTargetDTO | undefined;
}
export class ShiftTypeEmployeeStatisticsTargetForm extends SoeFormGroup {
  constructor({
    validationHandler,
    element,
  }: IShiftTypeEmployeeStatisticsTargetForm) {
    super(validationHandler, {
      shiftTypeEmployeeStatisticsTargetId: new SoeTextFormControl(
        element?.shiftTypeEmployeeStatisticsTargetId || 0,
        {
          isIdField: true,
        }
      ),
      shiftTypeId: new SoeNumberFormControl(element?.shiftTypeId || 0),
      employeeStatisticsType: new SoeNumberFormControl(
        element?.employeeStatisticsType || 0
      ),
      targetValue: new SoeNumberFormControl(element?.targetValue || 0),
      fromDate: new SoeDateFormControl(element?.fromDate, {}),
      employeeStatisticsTypeName: new SoeTextFormControl(
        element?.employeeStatisticsTypeName || ''
      ),
    });
  }
}
