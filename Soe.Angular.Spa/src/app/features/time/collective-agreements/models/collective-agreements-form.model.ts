import {
  SoeCheckboxFormControl,
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { IEmployeeCollectiveAgreementDTO } from '@shared/models/generated-interfaces/EmployeeCollectiveAgreementDTO';

interface ICollectiveAgreementsForm {
  validationHandler: ValidationHandler;
  element: IEmployeeCollectiveAgreementDTO | undefined;
}
export class CollectiveAgreementsForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ICollectiveAgreementsForm) {
    super(validationHandler, {
      employeeCollectiveAgreementId: new SoeTextFormControl(
        element?.employeeCollectiveAgreementId || 0,
        {
          isIdField: true,
        }
      ),
      code: new SoeTextFormControl(
        element?.code || '',
        { maxLength: 50 },
        'common.code'
      ),
      name: new SoeTextFormControl(
        element?.name || '',
        { isNameField: true, required: true, maxLength: 100, minLength: 1 },
        'common.name'
      ),
      externalCode: new SoeTextFormControl(
        element?.externalCode || '',
        { maxLength: 50 },
        'common.externalcode'
      ),
      description: new SoeTextFormControl(
        element?.description || '',
        { maxLength: 512 },
        'common.description'
      ),
      employeeGroupId: new SoeSelectFormControl(
        element?.employeeGroupId || undefined,
        { required: true },
        'time.employee.employeegroup.employeegroup'
      ),
      payrollGroupId: new SoeSelectFormControl(
        element?.payrollGroupId || undefined,
        { required: true },
        'time.employee.payrollgroup.payrollgroup'
      ),
      vacationGroupId: new SoeSelectFormControl(
        element?.vacationGroupId || undefined,
        { required: true },
        'time.employee.vacationgroup.vacationgroup'
      ),
      annualLeaveGroupId: new SoeSelectFormControl(
        element?.annualLeaveGroupId || undefined,
        { required: false },
        'time.employee.annualleavegroup'
      ),
    });
  }

  get employeeCollectiveAgreementId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.employeeCollectiveAgreementId;
  }

  get code(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.code;
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }

  get externalCode(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.externalCode;
  }

  get description(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.description;
  }

  get employeeGroupId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.employeeGroupId;
  }

  get payrollGroupId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.payrollGroupId;
  }

  get vacationGroupId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.vacationGroupId;
  }

  get isActive(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.isActive;
  }
}
